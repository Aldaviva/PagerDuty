using Pager.Duty;
using Pager.Duty.Exceptions;
using Pager.Duty.Requests;
using Pager.Duty.Responses;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Tests.Helpers;

namespace Tests;

public class PagerDutyTest {

    private PagerDuty _pagerDuty = new("123");

    private readonly FakeHttpMessageHandler _httpMessageHandler = A.Fake<FakeHttpMessageHandler>();
    private readonly HttpClient             _httpClient;

    public PagerDutyTest() {
        _httpClient           = new HttpClient(_httpMessageHandler);
        _pagerDuty.HttpClient = _httpClient;
    }

    [Fact]
    public async Task SendAlert() {
        Alert alert = new ResolveAlert("abc");

        A.CallTo(() => _httpMessageHandler.SendAsync(An<HttpRequestMessage>._)).Returns(
            new HttpResponseMessage(HttpStatusCode.Accepted) {
                Content = new StringContent(
                    """
                    {
                      "status": "success",
                      "message": "Event processed",
                      "dedup_key": "abc"
                    }
                    """)
            });

        AlertResponse alertResponse = await _pagerDuty.Send(alert);

        alertResponse.DedupKey.Should().Be("abc");
        alertResponse.Message.Should().Be("Event processed");
        alertResponse.Status.Should().Be("success");
        alertResponse.IsSuccessful.Should().BeTrue();

        const string expectedJsonBody =
            """
            {
              "routing_key": "123",
              "dedup_key": "abc",
              "event_action": "resolve"
            }
            """;

        A.CallTo(() => _httpMessageHandler.SendAsync(An<HttpRequestMessage>.That.Matches(
            HttpMethod.Post, "https://events.pagerduty.com/v2/enqueue", expectedJsonBody)
        )).MustHaveHappened();
    }

    [Fact]
    public async Task SendChange() {
        Change change = new("my change") { Source = "my source" };

        A.CallTo(() => _httpMessageHandler.SendAsync(An<HttpRequestMessage>._)).Returns(
            new HttpResponseMessage(HttpStatusCode.Accepted) {
                Content = new StringContent(
                    """
                    {
                      "status": "success",
                      "message": "Change event processed"
                    }
                    """)
            });

        ChangeResponse changeResponse = await _pagerDuty.Send(change);

        changeResponse.Message.Should().Be("Change event processed");
        changeResponse.Status.Should().Be("success");

        const string expectedJsonBody =
            """
            {
              "routing_key": "123",
              "payload": {
                "summary": "my change",
                "source": "my source"
              },
              "links": [],
              "images": []
            }
            """;

        A.CallTo(() => _httpMessageHandler.SendAsync(An<HttpRequestMessage>.That.Matches(
            HttpMethod.Post, "https://events.pagerduty.com/v2/change/enqueue", expectedJsonBody)
        )).MustHaveHappened();
    }

    [Fact]
    public async Task NetworkException() {
        AcknowledgeAlert acknowledge = new("abc");

        A.CallTo(() => _httpMessageHandler.SendAsync(An<HttpRequestMessage>._)).Throws<HttpRequestException>();

        Func<Task> thrower = async () => await _pagerDuty.Send(acknowledge);
        (await thrower.Should().ThrowAsync<NetworkException>()).Which.RetryAllowedAfterDelay.Should().BeTrue();
    }

    [Fact]
    public async Task BadRequest() {
        AcknowledgeAlert acknowledge = new("abc");

        A.CallTo(() => _httpMessageHandler.SendAsync(An<HttpRequestMessage>._)).Returns(new HttpResponseMessage(HttpStatusCode.BadRequest) {
            Content = new StringContent("Invalid JSON string")
        });

        Func<Task> thrower = async () => await _pagerDuty.Send(acknowledge);
        BadRequest actual  = (await thrower.Should().ThrowAsync<BadRequest>()).Which;
        actual.RetryAllowedAfterDelay.Should().BeFalse();
        actual.Response.Should().Be("Invalid JSON string");
    }

    [Fact]
    public async Task RateLimited() {
        AcknowledgeAlert acknowledge = new("abc");

        A.CallTo(() => _httpMessageHandler.SendAsync(An<HttpRequestMessage>._)).Returns(new HttpResponseMessage((HttpStatusCode) 429) {
            Content = new StringContent("") // don't know what this response body actually looks like, the documentation is broken and I'm not really willing to trigger it on myself
        });

        Func<Task> thrower = async () => await _pagerDuty.Send(acknowledge);
        (await thrower.Should().ThrowAsync<RateLimited>()).Which.RetryAllowedAfterDelay.Should().BeTrue();
    }

    [Fact]
    public async Task InternalServerError() {
        AcknowledgeAlert acknowledge = new("abc");

        A.CallTo(() => _httpMessageHandler.SendAsync(An<HttpRequestMessage>._)).Returns(new HttpResponseMessage(HttpStatusCode.InternalServerError) {
            Content = new StringContent("") // don't know what this response body actually looks like, the documentation is broken, and I don't know how I could trigger it
        });

        Func<Task>          thrower = async () => await _pagerDuty.Send(acknowledge);
        InternalServerError actual  = (await thrower.Should().ThrowAsync<InternalServerError>()).Which;
        actual.RetryAllowedAfterDelay.Should().BeTrue();
        actual.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task UnknownError() {
        AcknowledgeAlert acknowledge = new("abc");

        A.CallTo(() => _httpMessageHandler.SendAsync(An<HttpRequestMessage>._)).Returns(new HttpResponseMessage(HttpStatusCode.PaymentRequired) {
            Content = new StringContent("") // made-up
        });

        Func<Task>              thrower = async () => await _pagerDuty.Send(acknowledge);
        WebApplicationException actual  = (await thrower.Should().ThrowAsync<WebApplicationException>()).Which;
        actual.RetryAllowedAfterDelay.Should().BeTrue();
        actual.StatusCode.Should().Be(402);
    }

    private static void AssertDisposed(HttpClient httpClient) {
        ((Action) (() => httpClient.Timeout = TimeSpan.FromSeconds(1))).Should().Throw<ObjectDisposedException>();
    }

    private static void AssertNotDisposed(HttpClient httpClient) {
        httpClient.Timeout = TimeSpan.FromSeconds(1);
    }

    [Fact]
    public void DisposeOwnedHttpClient() {
        _pagerDuty = new PagerDuty("test using owned HttpClient");
        AssertNotDisposed(_pagerDuty.HttpClient);
        _pagerDuty.Dispose();

        AssertDisposed(_pagerDuty.HttpClient);
    }

    [Fact]
    public void DisposeOwnedHttpClientWhenSwitchingToNonOwnedHttpClient() {
        _pagerDuty = new PagerDuty("ownedToNonOwned");
        HttpClient ownedHttpClient = _pagerDuty.HttpClient;
        AssertNotDisposed(ownedHttpClient);

        HttpClient customHttpClient = new();
        _pagerDuty.HttpClient = customHttpClient;
        _pagerDuty.HttpClient.Should().BeSameAs(customHttpClient);
        AssertDisposed(ownedHttpClient);
        AssertNotDisposed(_pagerDuty.HttpClient);
    }

    [Fact]
    public void DontDisposeNonOwnedHttpClient() {
        _pagerDuty.HttpClient.Should().BeSameAs(_httpClient);
        _pagerDuty.Dispose();
        AssertNotDisposed(_pagerDuty.HttpClient);
    }

    [Fact]
    public void DontDisposeOwnedHttpClientWhenSetIdempotently() {
        PagerDuty  pagerDuty       = new("abc");
        HttpClient ownedHttpClient = pagerDuty.HttpClient;
        AssertNotDisposed(ownedHttpClient);

        pagerDuty.HttpClient = ownedHttpClient;
        pagerDuty.HttpClient.Should().BeSameAs(ownedHttpClient);
        AssertNotDisposed(ownedHttpClient);

        pagerDuty.Dispose();
        AssertDisposed(pagerDuty.HttpClient);
    }

    [Fact]
    public void DisposeIsIdempotent() {
        _pagerDuty = new PagerDuty("test using owned HttpClient");
        _pagerDuty.Dispose();
        _pagerDuty.Dispose();
    }

    [Theory]
    [InlineData("https://events.eu.pagerduty.com/v2")]
    [InlineData("https://events.eu.pagerduty.com/v2/")]
    public async Task AllowAlternateBaseUrl(string baseUrl) {
        _pagerDuty = new PagerDuty("456") {
            BaseUrl    = new Uri(baseUrl),
            HttpClient = _httpClient
        };

        A.CallTo(() => _httpMessageHandler.SendAsync(An<HttpRequestMessage>._))
            .ReturnsLazily(() => new HttpResponseMessage(HttpStatusCode.Accepted) { Content = new StringContent(string.Empty) });

        await _pagerDuty.Send(new Change("change"));
        await _pagerDuty.Send(new ResolveAlert("abc"));

        A.CallTo(() => _httpMessageHandler.SendAsync(An<HttpRequestMessage>.That.Matches(
            HttpMethod.Post, "https://events.eu.pagerduty.com/v2/change/enqueue")
        )).MustHaveHappened();

        A.CallTo(() => _httpMessageHandler.SendAsync(An<HttpRequestMessage>.That.Matches(
            HttpMethod.Post, "https://events.eu.pagerduty.com/v2/enqueue")
        )).MustHaveHappened();
    }

    [Fact]
    public void BaseUrlMustBeAbsolute() {
        Action thrower = () => _pagerDuty.BaseUrl = new Uri("/a/b/c", UriKind.Relative);
        thrower.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData("https://a.b")]
    [InlineData("http://c.d/e")]
    public void BaseUrlAllowsHttpSchemes(string url) {
        _pagerDuty.BaseUrl = new Uri(url);
    }

    [Theory]
    [InlineData("ftp://events.pagerduty.com/")]
    [InlineData("mailto:events.pagerduty.com")]
    [InlineData("file:///c:/windows/system32/calc.exe")]
    public void BaseUrlDeniesNonHttpSchemes(string url) {
        Uri    uri     = new(url);
        Action thrower = () => { _pagerDuty.BaseUrl = uri; };
        thrower.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void BaseUrlDeniesInvalidUris() {
        string maxLengthPath = new(Enumerable.Repeat('p', 65489).ToArray());
        Uri    uri           = new($"https://events.pagerduty.com/{maxLengthPath}/");
        Action thrower       = () => { _pagerDuty.BaseUrl = uri; };
        thrower.Should().Throw<ArgumentOutOfRangeException>();
    }

}