using FluentAssertions.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pager.Duty.Webhooks;
using Pager.Duty.Webhooks.Requests;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Webhooks;

public class WebhookResourceTests: IAsyncDisposable {

    private readonly WebhookResource           _webhookResource = new("HvPtlQNKMPpHSSqJpCdndXfo7HnGXa1pnH6w5axpxLFn9aRBNBGh4e8/HlonK5mz");
    private readonly IHost                     _server;
    private readonly HttpClient                _client;
    private readonly IMonitor<WebhookResource> _webhookEventCaptor;

    public WebhookResourceTests() {
        _server = new HostBuilder().ConfigureWebHost(builder => builder
                .UseTestServer()
                .ConfigureServices(services => services.AddRouting())
                .Configure(app => app
                    .UseRouting()
                    .UseEndpoints(routes =>
                        routes.MapPost("/", _webhookResource.HandlePostRequest))))
            .Start();

        _client             = _server.GetTestClient();
        _webhookEventCaptor = _webhookResource.Monitor();
    }

    private async Task<HttpResponseMessage> ReceiveEvent(string body, string signature) =>
        await _client.PostAsync("/", new StringContent(body, Encoding.UTF8, MediaTypeNames.Application.Json) {
            Headers = { { "X-PagerDuty-Signature", signature } }
        });

    [Fact]
    public async Task InvalidSignature() {
        const string signature = "v1=0e83584cbab86f98c51517cf96a70c90e910532d3d2f067d58c753cc8980a05c";
        const string body =
            """{"event":{"id":"01FKZOXA868SHYEE4DVNWB5P9W","event_type":"pagey.ping","resource_type":"pagey","occurred_at":"2025-04-07T03:00:55.532Z","agent":{"id":"PVE0ZR9","type":"user_reference"},"client":null,"data":{"message":"Hello from your friend Pagey!","type":"ping"}}}""";

        using HttpResponseMessage response = await ReceiveEvent(body, signature);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        _webhookEventCaptor.Should().NotRaise(nameof(IWebhookResource.PingReceived));
    }

    [Fact]
    public async Task ValidSignature() {
        const string signature = "v1=0e83584cbab86f98c51517cf96a70c90e910532d3d2f067d58c753cc8980a05b";
        const string body =
            """{"event":{"id":"01FKZOXA868SHYEE4DVNWB5P9W","event_type":"pagey.ping","resource_type":"pagey","occurred_at":"2025-04-07T03:00:55.532Z","agent":{"id":"PVE0ZR9","type":"user_reference"},"client":null,"data":{"message":"Hello from your friend Pagey!","type":"ping"}}}""";

        using HttpResponseMessage response = await ReceiveEvent(body, signature);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        PingWebhookPayload actual = (PingWebhookPayload) _webhookEventCaptor.Should().Raise(nameof(IWebhookResource.PingReceived)).WithArgs<PingWebhookPayload>().First().Parameters[1];
        actual.EventType.Should().Be(PingEventType.Ping);
        actual.Message.Should().Be("Hello from your friend Pagey!");
    }

    [Fact]
    public async Task TriggerIncident() {
        const string signature = "v1=3e3032b45bbe1801c13f182b0e0a7f4f3f530225cef07e3e90cbc57874d01c49";
        const string body =
            """{"event":{"id":"01FKZSRAPTMF55RE69G8BQHGC6","event_type":"incident.triggered","resource_type":"incident","occurred_at":"2025-04-07T03:47:13.414Z","agent":{"html_url":"https://aldaviva.pagerduty.com/users/PVE0ZR9","id":"PVE0ZR9","self":"https://api.pagerduty.com/users/PVE0ZR9","summary":"Ben","type":"user_reference"},"client":null,"data":{"id":"Q2CKYRLH85UNN4","type":"incident","self":"https://api.pagerduty.com/incidents/Q2CKYRLH85UNN4","html_url":"https://aldaviva.pagerduty.com/incidents/Q2CKYRLH85UNN4","number":37,"status":"triggered","incident_key":"dfead9d88e0649e8b7dee514b1875756","created_at":"2025-04-07T03:47:13Z","title":"Test","service":{"html_url":"https://aldaviva.pagerduty.com/services/PNE3JZM","id":"PNE3JZM","self":"https://api.pagerduty.com/services/PNE3JZM","summary":"Webhook Tester","type":"service_reference"},"assignees":[{"html_url":"https://aldaviva.pagerduty.com/users/PVE0ZR9","id":"PVE0ZR9","self":"https://api.pagerduty.com/users/PVE0ZR9","summary":"Ben","type":"user_reference"}],"escalation_policy":{"html_url":"https://aldaviva.pagerduty.com/escalation_policies/P482651","id":"P482651","self":"https://api.pagerduty.com/escalation_policies/P482651","summary":"Notify Ben and Dad","type":"escalation_policy_reference"},"teams":[],"priority":null,"urgency":"low","conference_bridge":null,"resolve_reason":null,"incident_type":{"name":"incident_default"}}}}""";

        using HttpResponseMessage response = await ReceiveEvent(body, signature);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        IncidentWebhookPayload actual = (IncidentWebhookPayload) _webhookEventCaptor.Should().Raise(nameof(IWebhookResource.IncidentReceived)).WithArgs<IncidentWebhookPayload>().First().Parameters[1];
        actual.EventType.Should().Be(IncidentEventType.Triggered);
        actual.Status.Should().Be(IncidentStatus.Triggered);
        actual.IncidentNumber.Should().Be(37);
        actual.Title.Should().Be("Test");
        actual.Metadata.EventType.Should().Be("incident.triggered");
        actual.Metadata.Agent!.Self.Should().Be(new Uri("https://api.pagerduty.com/users/PVE0ZR9"));
        actual.Metadata.Agent.HtmlUrl.Should().Be(new Uri("https://aldaviva.pagerduty.com/users/PVE0ZR9"));
        actual.Metadata.Agent.Id.Should().Be("PVE0ZR9");
        actual.Metadata.Agent.Summary.Should().Be("Ben");
        actual.Metadata.Agent.Type.Should().Be(ReferenceType.UserReference);
        actual.Metadata.Client.Should().BeNull();
        actual.Metadata.Id.Should().Be("01FKZSRAPTMF55RE69G8BQHGC6");
        actual.Metadata.OccurredAt.Should().Be(new DateTimeOffset(2025, 4, 7, 3, 47, 13, 414, TimeSpan.Zero));
        actual.Metadata.ResourceType.Should().Be("incident");
        actual.Assignees.Single().Self.Should().Be(new Uri("https://api.pagerduty.com/users/PVE0ZR9"));
        actual.Assignees.Single().HtmlUrl.Should().Be(new Uri("https://aldaviva.pagerduty.com/users/PVE0ZR9"));
        actual.Assignees.Single().Id.Should().Be("PVE0ZR9");
        actual.Assignees.Single().Summary.Should().Be("Ben");
        actual.Assignees.Single().Type.Should().Be(ReferenceType.UserReference);
        actual.ConferenceBridge.Should().BeNull();
        actual.CreatedAt.Should().Be(new DateTimeOffset(2025, 4, 7, 3, 47, 13, TimeSpan.Zero));
        actual.EscalationPolicy.Summary.Should().Be("Notify Ben and Dad");
        actual.EscalationPolicy.HtmlUrl.Should().Be(new Uri("https://aldaviva.pagerduty.com/escalation_policies/P482651"));
        actual.EscalationPolicy.Id.Should().Be("P482651");
        actual.EscalationPolicy.Self.Should().Be(new Uri("https://api.pagerduty.com/escalation_policies/P482651"));
        actual.EscalationPolicy.Type.Should().Be(ReferenceType.EscalationPolicyReference);
        actual.HtmlUrl.Should().Be(new Uri("https://aldaviva.pagerduty.com/incidents/Q2CKYRLH85UNN4"));
        actual.Id.Should().Be("Q2CKYRLH85UNN4");
        actual.IncidentKey.Should().Be("dfead9d88e0649e8b7dee514b1875756");
        actual.IncidentType.Should().Be("incident_default");
        actual.Priority.Should().BeNull();
        actual.ResolveReason.Should().BeNull();
        actual.Self.Should().Be(new Uri("https://api.pagerduty.com/incidents/Q2CKYRLH85UNN4"));
        actual.Service.Summary.Should().Be("Webhook Tester");
        actual.Service.HtmlUrl.Should().Be(new Uri("https://aldaviva.pagerduty.com/services/PNE3JZM"));
        actual.Service.Id.Should().Be("PNE3JZM");
        actual.Service.Self.Should().Be(new Uri("https://api.pagerduty.com/services/PNE3JZM"));
        actual.Service.Type.Should().Be(ReferenceType.ServiceReference);
        actual.Teams.Should().BeEmpty();
        actual.HighUrgency.Should().BeFalse();
    }

    [Fact]
    public async Task AcknowledgeIncident() {
        const string signature = "v1=91f533c0411d09f3e68622e2b57fdb561e7113ce7635318a2bbf5cde45d3d876";
        const string body =
            """{"event":{"id":"01FKZSRAQJLP9S3OPM2VALGUPJ","event_type":"incident.acknowledged","resource_type":"incident","occurred_at":"2025-04-07T03:47:13.546Z","agent":{"html_url":"https://aldaviva.pagerduty.com/users/PVE0ZR9","id":"PVE0ZR9","self":"https://api.pagerduty.com/users/PVE0ZR9","summary":"Ben","type":"user_reference"},"client":null,"data":{"id":"Q2CKYRLH85UNN4","type":"incident","self":"https://api.pagerduty.com/incidents/Q2CKYRLH85UNN4","html_url":"https://aldaviva.pagerduty.com/incidents/Q2CKYRLH85UNN4","number":37,"status":"acknowledged","incident_key":"dfead9d88e0649e8b7dee514b1875756","created_at":"2025-04-07T03:47:13Z","title":"Test","service":{"html_url":"https://aldaviva.pagerduty.com/services/PNE3JZM","id":"PNE3JZM","self":"https://api.pagerduty.com/services/PNE3JZM","summary":"Webhook Tester","type":"service_reference"},"assignees":[{"html_url":"https://aldaviva.pagerduty.com/users/PVE0ZR9","id":"PVE0ZR9","self":"https://api.pagerduty.com/users/PVE0ZR9","summary":"Ben","type":"user_reference"}],"escalation_policy":{"html_url":"https://aldaviva.pagerduty.com/escalation_policies/P482651","id":"P482651","self":"https://api.pagerduty.com/escalation_policies/P482651","summary":"Notify Ben and Dad","type":"escalation_policy_reference"},"teams":[],"priority":null,"urgency":"low","conference_bridge":null,"resolve_reason":null,"incident_type":{"name":"incident_default"}}}}""";

        using HttpResponseMessage response = await ReceiveEvent(body, signature);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        IncidentWebhookPayload actual = (IncidentWebhookPayload) _webhookEventCaptor.Should().Raise(nameof(IWebhookResource.IncidentReceived)).WithArgs<IncidentWebhookPayload>().First().Parameters[1];
        actual.EventType.Should().Be(IncidentEventType.Acknowledged);
        actual.Status.Should().Be(IncidentStatus.Acknowledged);
    }

    [Fact]
    public async Task ResolveIncident() {
        const string signature = "v1=60a745384fb05b03acfc592b157b05e105e498da2f72f53c437c9d7c8d9aa879";
        const string body =
            """{"event":{"id":"01FKZSYE5HCMM576S6JUVIGLQT","event_type":"incident.resolved","resource_type":"incident","occurred_at":"2025-04-07T03:49:36.358Z","agent":{"html_url":"https://aldaviva.pagerduty.com/users/PVE0ZR9","id":"PVE0ZR9","self":"https://api.pagerduty.com/users/PVE0ZR9","summary":"Ben","type":"user_reference"},"client":null,"data":{"id":"Q2CKYRLH85UNN4","type":"incident","self":"https://api.pagerduty.com/incidents/Q2CKYRLH85UNN4","html_url":"https://aldaviva.pagerduty.com/incidents/Q2CKYRLH85UNN4","number":37,"status":"resolved","incident_key":"dfead9d88e0649e8b7dee514b1875756","created_at":"2025-04-07T03:47:13Z","title":"Test","service":{"html_url":"https://aldaviva.pagerduty.com/services/PNE3JZM","id":"PNE3JZM","self":"https://api.pagerduty.com/services/PNE3JZM","summary":"Webhook Tester","type":"service_reference"},"assignees":[],"escalation_policy":{"html_url":"https://aldaviva.pagerduty.com/escalation_policies/P482651","id":"P482651","self":"https://api.pagerduty.com/escalation_policies/P482651","summary":"Notify Ben and Dad","type":"escalation_policy_reference"},"teams":[],"priority":null,"urgency":"low","conference_bridge":null,"resolve_reason":null,"incident_type":{"name":"incident_default"}}}}""";

        using HttpResponseMessage response = await ReceiveEvent(body, signature);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        IncidentWebhookPayload actual = (IncidentWebhookPayload) _webhookEventCaptor.Should().Raise(nameof(IWebhookResource.IncidentReceived)).WithArgs<IncidentWebhookPayload>().First().Parameters[1];
        actual.EventType.Should().Be(IncidentEventType.Resolved);
        actual.Assignees.Should().BeEmpty();
        actual.Status.Should().Be(IncidentStatus.Resolved);
        actual.Teams.Should().BeEmpty();
    }

    [Fact]
    public async Task AnnotatedIncident() {
        const string signature = "v1=7b1cc279d644e35511cfca431696498477e97d46f2403ed46d426435e0efef2b";
        const string body =
            """{"event":{"id":"01FKZSYEOK0YQYTT9OZQJDS1P4","event_type":"incident.annotated","resource_type":"incident","occurred_at":"2025-04-07T03:49:36.680Z","agent":{"html_url":"https://aldaviva.pagerduty.com/users/PVE0ZR9","id":"PVE0ZR9","self":"https://api.pagerduty.com/users/PVE0ZR9","summary":"Ben","type":"user_reference"},"client":null,"data":{"incident":{"html_url":"https://aldaviva.pagerduty.com/incidents/Q2CKYRLH85UNN4","id":"Q2CKYRLH85UNN4","self":"https://api.pagerduty.com/incidents/Q2CKYRLH85UNN4","summary":"Test","type":"incident_reference"},"id":"PTSAIFZ","content":"Resolution Note: I'm resolving it","trimmed":false,"type":"incident_note"}}}""";

        using HttpResponseMessage response = await ReceiveEvent(body, signature);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        IncidentNoteWebhookPayload actual =
            (IncidentNoteWebhookPayload) _webhookEventCaptor.Should().Raise(nameof(IWebhookResource.IncidentNoteReceived)).WithArgs<IncidentNoteWebhookPayload>().First().Parameters[1];
        actual.EventType.Should().Be(IncidentNoteEventType.Annotated);
        actual.Id.Should().Be("PTSAIFZ");
        actual.Content.Should().Be("Resolution Note: I'm resolving it");
        actual.Trimmed.Should().BeFalse();
        actual.Incident.Self.Should().Be("https://api.pagerduty.com/incidents/Q2CKYRLH85UNN4");
    }

    public async ValueTask DisposeAsync() {
        _client.Dispose();
        await _server.StopAsync();
        _server.Dispose();
        GC.SuppressFinalize(this);
    }

}