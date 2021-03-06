using System.Net;
using FakeItEasy;
using FluentAssertions;
using Pager.Duty;
using Tests.Helpers;

namespace Tests;

public class PagerDutyTest {

    private readonly PagerDuty              _pagerDuty          = new("123");
    private readonly FakeHttpMessageHandler _httpMessageHandler = A.Fake<FakeHttpMessageHandler>();

    public PagerDutyTest() {
        _pagerDuty.HttpClient = new HttpClient(_httpMessageHandler);
    }

    [Fact]
    public async Task SendAlert() {
        Alert alert = new ResolveAlert("abc");

        A.CallTo(() => _httpMessageHandler.SendAsync(An<HttpRequestMessage>._)).Returns(
            new HttpResponseMessage(HttpStatusCode.Accepted) {
                Content = new StringContent(@"{
  ""status"": ""success"",
  ""message"": ""Event processed"",
  ""dedup_key"": ""abc""
}")
            });

        AlertResponse alertResponse = await _pagerDuty.Send(alert);

        alertResponse.DedupKey.Should().Be("abc");
        alertResponse.Message.Should().Be("Event processed");
        alertResponse.Status.Should().Be("success");

        const string expectedJsonBody = @"{
  ""routing_key"": ""123"",
  ""dedup_key"": ""abc"",
  ""event_action"": ""resolve""
}";

        A.CallTo(() => _httpMessageHandler.SendAsync(An<HttpRequestMessage>.That.Matches(
            HttpMethod.Post, "https://events.pagerduty.com/v2/enqueue", expectedJsonBody)
        )).MustHaveHappened();
    }

    [Fact]
    public async Task SendChange() {
        Change change = new("my change") { Source = "my source" };

        A.CallTo(() => _httpMessageHandler.SendAsync(An<HttpRequestMessage>._)).Returns(
            new HttpResponseMessage(HttpStatusCode.Accepted) {
                Content = new StringContent(@"{
  ""status"": ""success"",
  ""message"": ""Change event processed""
}")
            });

        ChangeResponse changeResponse = await _pagerDuty.Send(change);

        changeResponse.Message.Should().Be("Change event processed");
        changeResponse.Status.Should().Be("success");

        const string expectedJsonBody = @"{
  ""routing_key"": ""123"",
  ""payload"": {
    ""summary"": ""my change"",
    ""source"": ""my source""
  },
  ""links"": [],
  ""images"": []
}";

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

    [Fact]
    public void Dispose() {
        _pagerDuty.Dispose();

        Action thrower = () => _pagerDuty.HttpClient.Timeout = TimeSpan.FromSeconds(1);
        thrower.Should().Throw<ObjectDisposedException>();
    }

}