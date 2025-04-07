using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Pager.Duty.Webhooks.Requests;

public interface IWebhookPayload {

    public WebhookPayloadMetadata Metadata { get; set; }

}

public interface IWebhookPayload<out T>: IWebhookPayload where T: Enum {

    public T EventType { get; }

}

public abstract class AbstractWebhookPayload<T>: IWebhookPayload<T> where T: Enum {

    public WebhookPayloadMetadata Metadata { get; set; } = null!;
    public abstract T EventType { get; }
    protected ReadOnlySpan<char> EventTypeSuffix => Metadata.EventType.AsSpan(Metadata.EventType.IndexOf('.') + 1);

}

internal class WebhookPayloadEnvelope {

    public WebhookPayloadMetadata Event { get; set; } = null!;

}

public class WebhookPayloadMetadata {

    public string Id { get; set; } = null!;
    [JsonProperty] internal string EventType { get; set; } = null!;
    public string ResourceType { get; set; } = null!;
    public DateTimeOffset OccurredAt { get; set; }
    public PagerDutyReference? Agent { get; set; }
    public IDictionary<string, string>? Client { get; set; }
    [JsonProperty] internal JToken Data { get; set; } = null!;

}

public record PagerDutyReference(string Id, ReferenceType Type, Uri? HtmlUrl, Uri? Self, string? Summary);

public enum ReferenceType {

    UserReference,
    EscalationPolicyReference,
    ServiceReference,
    TeamReference,
    PriorityReference,
    IncidentWorkflowReference,
    WorkflowTriggerReference,
    InboundIntegrationReference,
    IncidentReference

}