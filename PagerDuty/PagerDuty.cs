using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Pager.Duty.Exceptions;
using Pager.Duty.Requests;
using Pager.Duty.Responses;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pager.Duty;

/// <inheritdoc />
public class PagerDuty: IPagerDuty {

    private static readonly NamingStrategy JsonNamingStrategy = new SnakeCaseNamingStrategy(false, false);
    private static readonly Encoding       Utf8               = new UTF8Encoding(false, true);

    internal static readonly JsonSerializerSettings JsonSerializerSettings = new() {
        MissingMemberHandling = MissingMemberHandling.Ignore,
        NullValueHandling     = NullValueHandling.Ignore,
        ContractResolver      = new DefaultContractResolver { NamingStrategy = JsonNamingStrategy },
        Converters            = { new StringEnumConverter(JsonNamingStrategy, false) }
    };

    internal static readonly JsonSerializer JsonSerializer = JsonSerializer.Create(JsonSerializerSettings);

    private readonly string           _integrationKey;
    private readonly Lazy<HttpClient> _builtInHttpClient = new(LazyThreadSafetyMode.ExecutionAndPublication);

    private Uri         _baseUrl = new("https://events.pagerduty.com/v2/", UriKind.Absolute);
    private HttpClient? _customHttpClient;

    /// <inheritdoc />
    public HttpClient HttpClient {
        get => _customHttpClient ?? _builtInHttpClient.Value;
        set {
            bool builtInExists     = _builtInHttpClient.IsValueCreated;
            bool newValueIsBuiltIn = builtInExists && _builtInHttpClient.Value == value;

            _customHttpClient = builtInExists && newValueIsBuiltIn ? null : value;
            if (builtInExists && !newValueIsBuiltIn) {
                _builtInHttpClient.Value.Dispose();
            }
        }
    }

    /// <inheritdoc />
    public Uri BaseUrl {
        get => _baseUrl;
        set {
            if (value.IsAbsoluteUri && value.Scheme.ToLowerInvariant() is "http" or "https") {
                try {
                    if (!value.AbsolutePath.EndsWith("/")) {
                        UriBuilder builder = new(value);
                        builder.Path += '/';
                        value        =  builder.Uri;
                    }

                    _        = new Uri(value, new TriggerAlert(Severity.Info, string.Empty).ApiUriPath);
                    _baseUrl = value;
                    return;
                } catch (UriFormatException) {
                    // throw ArgumentOutOfRangeException below
                }
            }

            throw new ArgumentOutOfRangeException(nameof(BaseUrl), value, "must be an HTTPS or HTTP URL");
        }
    }

    /// <summary>
    /// <para>Create a new Service-specific instance of a client that sends Events to the PagerDuty Events API V2.</para>
    /// <para>You can retain this instance for the lifetime of your application, or create a new instance for each Event. Be sure to <see cref="Dispose"/> of instances when you're done with them.</para>
    /// <para>To upload Events to multiple Services in PagerDuty, create multiple instances of this class.</para>
    /// </summary>
    /// <param name="integrationKey">The GUID of one of your Events API V2 integrations. This is the "Integration Key" listed on the Events API V2 integration's detail page.</param>
    public PagerDuty(string integrationKey) {
        _integrationKey = integrationKey;
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
    private async Task<TResponse> Send<TResponse>(Event pagerDutyEvent) {
        pagerDutyEvent.RoutingKey = _integrationKey;

        Uri uri = new(BaseUrl, pagerDutyEvent.ApiUriPath);

        using HttpContent requestBody = new JsonContent(pagerDutyEvent) { JsonSerializer = JsonSerializer, Encoding = Utf8 };

        HttpResponseMessage response;
        try {
            response = await HttpClient.PostAsync(uri, requestBody).ConfigureAwait(false);
        } catch (HttpRequestException e) {
            throw new NetworkException($"Failed to send {typeof(Event)} to {uri}: {e.Message}", e);
        }

        using (response) {
            HttpStatusCode statusCode = response.StatusCode;
            if (statusCode == HttpStatusCode.Accepted) {
                using Stream     responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                using TextReader textReader     = new StreamReader(responseStream, Utf8);
                using JsonReader jsonReader     = new JsonTextReader(textReader);
                return JsonSerializer.Deserialize<TResponse>(jsonReader)!;
            } else {
                string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                string message      = $"Failed to send {typeof(Event)} to {uri}";
                WebApplicationException exception = statusCode switch {
                    HttpStatusCode.BadRequest                                         => new BadRequest(message),
                    (HttpStatusCode) 429                                              => new RateLimited(message),
                    >= HttpStatusCode.InternalServerError and <= (HttpStatusCode) 599 => new InternalServerError((int) statusCode, message),
                    _                                                                 => new WebApplicationException((int) statusCode, message)
                };
                exception.Response = responseBody;
                throw exception;
            }
        }
    }

    /// <summary>
    /// Clean up this instance to ensure memory can be freed by the garbage collector. It will not be able to send requests after calling this method.
    /// </summary>
    public void Dispose() {
        if (_builtInHttpClient.IsValueCreated) {
            _builtInHttpClient.Value.Dispose();
        }

        GC.SuppressFinalize(this);
    }

}