using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pager.Duty.Webhooks.Requests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Pager.Duty.Webhooks;

public class WebhookResource {

    private static readonly IReadOnlyDictionary<string, Type> PayloadTypes = new Dictionary<string, Type> {
        [PingWebhookPayload.ResourceType]                     = typeof(PingWebhookPayload),
        [IncidentWebhookPayload.ResourceType]                 = typeof(IncidentWebhookPayload),
        [IncidentNoteWebhookPayload.ResourceType]             = typeof(IncidentNoteWebhookPayload),
        [IncidentConferenceBridgeWebhookPayload.ResourceType] = typeof(IncidentConferenceBridgeWebhookPayload),
        [IncidentFieldValuesWebhookPayload.ResourceType]      = typeof(IncidentFieldValuesWebhookPayload),
        [IncidentStatusUpdateWebhookPayload.ResourceType]     = typeof(IncidentStatusUpdateWebhookPayload),
        [IncidentResponderWebhookPayload.ResourceType]        = typeof(IncidentResponderWebhookPayload),
        [IncidentWorkflowInstanceWebhookPayload.ResourceType] = typeof(IncidentWorkflowInstanceWebhookPayload),
        [ServiceWebhookPayload.ResourceType]                  = typeof(ServiceWebhookPayload)
    };

    private readonly ICollection<byte[]> _pagerDutySecrets;

    private ILogger<WebhookResource>? _logger;

    public event EventHandler<PingWebhookPayload>? PingReceived;
    public event EventHandler<IncidentWebhookPayload>? IncidentReceived;
    public event EventHandler<IncidentNoteWebhookPayload>? IncidentNoteReceived;
    public event EventHandler<IncidentConferenceBridgeWebhookPayload>? IncidentConferenceBridgeReceived;
    public event EventHandler<IncidentFieldValuesWebhookPayload>? IncidentFieldValuesReceived;
    public event EventHandler<IncidentStatusUpdateWebhookPayload>? IncidentStatusUpdateReceived;
    public event EventHandler<IncidentResponderWebhookPayload>? IncidentResponderReceived;
    public event EventHandler<IncidentWorkflowInstanceWebhookPayload>? IncidentWorkflowInstanceReceived;
    public event EventHandler<ServiceWebhookPayload>? ServiceReceived;

    public WebhookResource(params IEnumerable<string> pagerDutySecrets) {
        _pagerDutySecrets = pagerDutySecrets.Select(PagerDuty.Utf8.GetBytes).ToList();
        if (!_pagerDutySecrets.Any()) {
            throw new ArgumentOutOfRangeException(nameof(pagerDutySecrets), pagerDutySecrets, "At least one PagerDuty webhook secret must be supplied");
        }
    }

    public async Task HandlePostRequest(HttpContext httpContext) {
        HttpRequest  req = httpContext.Request;
        HttpResponse res = httpContext.Response;
        _logger ??= httpContext.RequestServices.GetRequiredService<ILogger<WebhookResource>>();

        if (!HttpMethods.IsPost(req.Method)) {
            res.StatusCode = StatusCodes.Status405MethodNotAllowed;
            return;
        } else if (!req.HasJsonContentType()) {
            res.StatusCode = StatusCodes.Status415UnsupportedMediaType;
            return;
        } else {
            res.StatusCode = StatusCodes.Status204NoContent;
        }

        MemoryStream bodyBuffer = new((int) (req.ContentLength ?? 0));
        await httpContext.Request.Body.CopyToAsync(bodyBuffer).ConfigureAwait(false);
        bodyBuffer.Position = 0;
        byte[] body = bodyBuffer.ToArray();

        if (!ValidateSignature(httpContext, body)) {
            _logger.LogWarning("Invalid signature, ignoring webhook that was spoofing PagerDuty");
            res.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        using TextReader       streamReader = new StreamReader(bodyBuffer, PagerDuty.Utf8);
        await using JsonReader jsonReader   = new JsonTextReader(streamReader);
        if (_logger.IsEnabled(LogLevel.Trace)) {
            // ReSharper disable once MethodHasAsyncOverload - it's not reading from an async stream, it's reading from a buffered in-memory byte array
            _logger.LogTrace("Received request with body {data}", streamReader.ReadToEnd());
            bodyBuffer.Position = 0;
        }

        if (PagerDuty.JsonSerializer.Deserialize<WebhookPayloadEnvelope>(jsonReader) is not { } payloadEnvelope) {
            _logger.LogError("Failed to convert 'data' property to {type}", nameof(WebhookPayloadEnvelope));
            return;
        }

        if (!PayloadTypes.TryGetValue(payloadEnvelope.Event.ResourceType, out Type? dataType)) {
            _logger.LogWarning("Unrecognized resource type {type} received in PagerDuty webhook, ignoring", payloadEnvelope.Event.ResourceType);
            return;
        }

        IWebhookPayload payload = (IWebhookPayload) payloadEnvelope.Event.Data.ToObject(dataType, PagerDuty.JsonSerializer)!;
        payload.Metadata           = payloadEnvelope.Event;
        payloadEnvelope.Event.Data = JValue.CreateNull();
        _logger.LogDebug("Received {eventType} webhook", payload.Metadata.EventType);

        switch (payload) {
            case PingWebhookPayload p:
                PingReceived?.Invoke(this, p);
                break;
            case IncidentWebhookPayload p:
                IncidentReceived?.Invoke(this, p);
                break;
            case IncidentNoteWebhookPayload p:
                IncidentNoteReceived?.Invoke(this, p);
                break;
            case IncidentConferenceBridgeWebhookPayload p:
                IncidentConferenceBridgeReceived?.Invoke(this, p);
                break;
            case IncidentFieldValuesWebhookPayload p:
                IncidentFieldValuesReceived?.Invoke(this, p);
                break;
            case IncidentStatusUpdateWebhookPayload p:
                IncidentStatusUpdateReceived?.Invoke(this, p);
                break;
            case IncidentResponderWebhookPayload p:
                IncidentResponderReceived?.Invoke(this, p);
                break;
            case IncidentWorkflowInstanceWebhookPayload p:
                IncidentWorkflowInstanceReceived?.Invoke(this, p);
                break;
            case ServiceWebhookPayload p:
                ServiceReceived?.Invoke(this, p);
                break;
        }
    }

    /// <returns><c>true</c> if the signature is valid, or <c>false</c> if someone is spoofing PagerDuty requests to our server</returns>
    private bool ValidateSignature(HttpContext context, byte[] requestBody) {
        IEnumerable<byte[]>? offeredSignatures = context.Request.Headers["X-PagerDuty-Signature"].FirstOrDefault()?.Split(',').Select(s => s.Split('=', 2)).Where(kv => kv[0] == "v1")
            .Select(kv => Convert.FromHexString(kv[1]));
        ICollection<byte[]> desiredSignatures = _pagerDutySecrets.Select(secret => HMACSHA256.HashData(secret, requestBody)).ToList();
        // Can't use Intersect with an IEqualityComparer because that uses the hashcode, not pair-wise comparisons, so it can't use FixedTimeEquals
        return offeredSignatures?.Any(offeredSignature => desiredSignatures.Any(desiredSignature => CryptographicOperations.FixedTimeEquals(offeredSignature, desiredSignature))) ?? false;
    }

}