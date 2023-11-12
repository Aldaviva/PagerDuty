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
using System.Threading.Tasks;

namespace Pager.Duty;

/// <inheritdoc />
public class PagerDuty: IPagerDuty {

    private static readonly NamingStrategy JsonNamingStrategy = new SnakeCaseNamingStrategy(false, false);
    private static readonly Encoding       Utf8               = new UTF8Encoding(false, true);

    internal static readonly JsonSerializerSettings JsonSerializerSettings = new() {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver  = new DefaultContractResolver { NamingStrategy = JsonNamingStrategy },
        Converters        = { new StringEnumConverter(JsonNamingStrategy, false) }
    };

    private HttpClient _httpClient     = new();
    private bool       _ownsHttpClient = true;

    /// <inheritdoc />
    public HttpClient HttpClient {
        get => _httpClient;
        set {
            if (_ownsHttpClient && _httpClient != value) {
                _ownsHttpClient = false;
                _httpClient.Dispose();
            }

            _httpClient = value;
        }
    }

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
    private async Task<TResponse> Send<TResponse>(Event pagerDutyEvent) {
        pagerDutyEvent.RoutingKey = _routingKey;

        Uri uri = pagerDutyEvent.ApiUri;

        using HttpContent requestBody = new JsonContent(pagerDutyEvent) { JsonSerializer = _jsonSerializer, Encoding = Utf8 };

        HttpResponseMessage response;
        try {
            response = await HttpClient.PostAsync(uri, requestBody).ConfigureAwait(false);
        } catch (HttpRequestException e) {
            throw new NetworkException($"Failed to send {typeof(Event)} to {uri}: {e.Message}", e);
        }

        using (response) {
            int statusCode = (int) response.StatusCode;
            if (statusCode == (int) HttpStatusCode.Accepted) {
                using Stream     responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                using TextReader textReader     = new StreamReader(responseStream, Utf8);
                using JsonReader jsonReader     = new JsonTextReader(textReader);
                return _jsonSerializer.Deserialize<TResponse>(jsonReader)!;
            } else {
                string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                string message      = $"Failed to send {typeof(Event)} to {uri}";
                throw statusCode switch {
                    (int) HttpStatusCode.BadRequest                       => new BadRequest(message) { Response                          = responseBody },
                    429                                                   => new RateLimited(message) { Response                         = responseBody },
                    >= (int) HttpStatusCode.InternalServerError and < 600 => new InternalServerError(statusCode, message) { Response     = responseBody },
                    _                                                     => new WebApplicationException(statusCode, message) { Response = responseBody }
                };
            }
        }
    }

    /// <summary>
    /// Clean up this instance to ensure memory can be freed by the garbage collector. It will not be able to send requests after calling this method.
    /// </summary>
    public void Dispose() {
        if (_ownsHttpClient) {
            HttpClient.Dispose();
        }
    }

}