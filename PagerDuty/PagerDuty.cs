using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Pager.Duty;

/// <summary>
/// <para>This class is the main entry point into the PagerDuty library.</para>
/// <para> </para>
/// <para>To get started:</para>
/// <list type="number">
/// <item><description>Sign in to PagerDuty, or create a new free account.</description></item>
/// <item><description>Create a new Service in PagerDuty.</description></item>
/// <item><description>Add a new Integration of type "Events API V2" to your Service.</description></item>
/// <item><description>Copy the automatically-generated Integration Key.</description></item>
/// <item><description>Construct a new <see cref="PagerDuty"/> instance, passing it the Integration Key.</description></item>
/// <item><description>Call the <see cref="Send(Alert)"/> method, passing a new <see cref="PagerDutyEvent"/> parameter.</description></item>
/// <item><description>To get the result of the request, you can await the <see cref="Task"/> and catch <see cref="PagerDutyException"/>.</description></item>
/// </list>
/// <para> </para>
/// <para>Example:</para>
/// <code> using IPagerDuty pagerDuty = new PagerDuty("dfca74ebb7450b3e6da3ba6083a323f4");
/// try {
///     AlertResponse response = await pagerDuty.Send(new TriggerAlert(Severity.Error, "Health check failing") {
///         Component = "db-prod-00",
///         Group = "db-prod"
///     });
///
///     await Task.Delay(1000);
///     await pagerDuty.Send(new ResolveAlert(response.DedupKey));
/// } catch (PagerDutyException e) {
///     Console.WriteLine(e.Message);
/// }</code>
/// </summary>
public class PagerDuty: IPagerDuty {

    private static readonly NamingStrategy JsonNamingStrategy = new SnakeCaseNamingStrategy(false, false);

    internal static readonly JsonSerializerSettings JsonSerializerSettings = new() {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver  = new DefaultContractResolver { NamingStrategy = JsonNamingStrategy },
        Converters        = { new StringEnumConverter(JsonNamingStrategy, false) }
    };

    /// <inheritdoc />
    public HttpClient HttpClient { get; set; } = new();

    private readonly JsonSerializer _jsonSerializer;
    private readonly string         _routingKey;

    /// <summary>
    /// <para>Create a new Service-specific instance of a client that sends Events to the PagerDuty Events API V2.</para>
    /// <para>You can retain this instance for the lifetime of your application, or create a new instance for each Event. Be sure to <see cref="Dispose"/> of instances when you're done with them.</para>
    /// <para>To upload Events to multiple Services in PagerDuty, create multiple instances of this class.</para>
    /// </summary>
    /// <param name="integrationKey">The GUID of one of your Events API V2 integrations. This is the "Integration Key" listed on the Events API V2 integration's detail page.</param>
    public PagerDuty(string integrationKey) {
        _routingKey     = integrationKey;
        _jsonSerializer = JsonSerializer.Create(JsonSerializerSettings);
    }

    /// <inheritdoc />
    public Task<AlertResponse> Send(Alert alert) {
        return Send<AlertResponse>(alert);
    }

    /// <inheritdoc />
    public Task<ChangeResponse> Send(Change change) {
        return Send<ChangeResponse>(change);
    }

    /// <exception cref="NetworkException"></exception>
    /// <exception cref="WebApplicationException"></exception>
    private async Task<TResponse> Send<TResponse>(PagerDutyEvent pagerDutyEvent) {
        pagerDutyEvent.RoutingKey = _routingKey;

        Uri uri = pagerDutyEvent.ApiUri;

        HttpContent requestBody = new JsonContent(pagerDutyEvent) { JsonSerializer = _jsonSerializer, Encoding = Encoding.UTF8 };

        HttpResponseMessage response;
        try {
            response = await HttpClient.PostAsync(uri, requestBody).ConfigureAwait(false);
        } catch (HttpRequestException e) {
            throw new NetworkException($"Failed to send {typeof(PagerDutyEvent)} to {uri}: {e.Message}", e);
        }

        int statusCode = (int) response.StatusCode;
        if (statusCode != (int) HttpStatusCode.Accepted) {
            string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            string message      = $"Failed to send {typeof(PagerDutyEvent)} to {uri}";
            throw statusCode switch {
                (int) HttpStatusCode.BadRequest                       => new BadRequest(message) { Response                          = responseBody },
                429                                                   => new RateLimited(message) { Response                         = responseBody },
                >= (int) HttpStatusCode.InternalServerError and < 600 => new InternalServerError(statusCode, message) { Response     = responseBody },
                _                                                     => new WebApplicationException(statusCode, message) { Response = responseBody }
            };
        }

        using Stream     responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        using TextReader textReader     = new StreamReader(responseStream, Encoding.UTF8);
        using JsonReader jsonReader     = new JsonTextReader(textReader);
        return _jsonSerializer.Deserialize<TResponse>(jsonReader)!;
    }

    /// <summary>
    /// Clean up this instance to ensure memory can be freed by the garbage collector. It will not be able to send requests after calling this method.
    /// </summary>
    public void Dispose() {
        HttpClient.Dispose();
    }

}