using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).

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

public class IncidentWebhookPayload: AbstractWebhookPayload<IncidentEventType> {

    public const string ResourceType = "incident";

    public string Id { get; set; } = null!;
    public Uri Self { get; set; } = null!;
    public Uri HtmlUrl { get; set; } = null!;
    [JsonProperty("number")] public long IncidentNumber { get; set; }
    public IncidentStatus Status { get; set; }
    public string IncidentKey { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public string Title { get; set; } = null!;
    [JsonProperty("incident_type")] internal IncidentTypeWrapper IncidentTypeWrapper { get; set; } = null!;
    [JsonIgnore] public string IncidentType {
        get => IncidentTypeWrapper.Name;
        set => IncidentTypeWrapper.Name = value;
    }
    public PagerDutyReference Service { get; set; } = null!;
    public ICollection<PagerDutyReference> Assignees { get; } = new List<PagerDutyReference>();
    public PagerDutyReference EscalationPolicy { get; set; } = null!;
    public ICollection<PagerDutyReference> Teams { get; } = new List<PagerDutyReference>();
    public PagerDutyReference Priority { get; set; } = null!;
    [JsonProperty("urgency")] internal string Urgency { get; set; } = null!;
    [JsonIgnore] public bool HighUrgency {
        get => Urgency == "high";
        set => Urgency = value ? "high" : "low";
    }
    public ConferenceBridge? ConferenceBridge { get; set; }
    public string? ResolveReason { get; set; }

    public override IncidentEventType EventType => EventTypeSuffix switch {
        "acknowledged"          => IncidentEventType.Acknowledged,
        "delegated"             => IncidentEventType.Delegated,
        "escalated"             => IncidentEventType.Escalated,
        "incident_type.changed" => IncidentEventType.IncidentTypeChanged,
        "priority_updated"      => IncidentEventType.PriorityUpdated,
        "reassigned"            => IncidentEventType.Reassigned,
        "reopened"              => IncidentEventType.Reopened,
        "resolved"              => IncidentEventType.Resolved,
        "service_updated"       => IncidentEventType.ServiceUpdated,
        "triggered"             => IncidentEventType.Triggered,
        "unacknowledged"        => IncidentEventType.Unacknowledged
    };

}

public class IncidentNoteWebhookPayload: AbstractWebhookPayload<IncidentNoteEventType> {

    public const string ResourceType = "incident_note";

    public PagerDutyReference Incident { get; set; } = null!;
    public string Id { get; set; } = null!;
    public string Content { get; set; } = null!;
    public bool Trimmed { get; set; }

    public override IncidentNoteEventType EventType => EventTypeSuffix switch {
        "annotated" => IncidentNoteEventType.Annotated
    };

}

public class IncidentConferenceBridgeWebhookPayload: AbstractWebhookPayload<IncidentConferenceBridgeEventType> {

    public const string ResourceType = "incident_conference_bridge";

    public PagerDutyReference Incident { get; set; } = null!;
    public ICollection<ConferenceNumber> ConferenceNumbers { get; } = new List<ConferenceNumber>();
    public Uri ConferenceUrl { get; set; } = null!;

    public override IncidentConferenceBridgeEventType EventType => EventTypeSuffix switch {
        "conference_bridge.updated" => IncidentConferenceBridgeEventType.Updated
    };

}

public class IncidentFieldValuesWebhookPayload: AbstractWebhookPayload<IncidentCustomFieldValuesEventType> {

    public const string ResourceType = "incident_field_values";

    public PagerDutyReference Incident { get; set; } = null!;
    public ICollection<CustomField> CustomFields { get; } = new List<CustomField>();
    public ICollection<CustomField> ChangedCustomFields { get; } = new List<CustomField>();

    public override IncidentCustomFieldValuesEventType EventType => EventTypeSuffix switch {
        "custom_field_values.updated" => IncidentCustomFieldValuesEventType.Updated
    };

}

public class IncidentStatusUpdateWebhookPayload: AbstractWebhookPayload<IncidentStatusUpdateEventType> {

    public const string ResourceType = "incident_status_update";

    public PagerDutyReference Incident { get; set; } = null!;
    public string Id { get; set; } = null!;
    public string Message { get; set; } = null!;
    public bool Trimmed { get; set; }

    public override IncidentStatusUpdateEventType EventType => EventTypeSuffix switch {
        "status_update_published" => IncidentStatusUpdateEventType.Published
    };

}

public class IncidentResponderWebhookPayload: AbstractWebhookPayload<IncidentResponderEventType> {

    public const string ResourceType = "incident_responder";

    public PagerDutyReference Incident { get; set; } = null!;
    public PagerDutyReference User { get; set; } = null!;
    public PagerDutyReference EscalationPolicy { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string State { get; set; } = null!;

    public override IncidentResponderEventType EventType => EventTypeSuffix switch {
        "added"   => IncidentResponderEventType.Added,
        "replied" => IncidentResponderEventType.Replied
    };

}

public class IncidentWorkflowInstanceWebhookPayload: AbstractWebhookPayload<IncidentWorkflowInstanceEventType> {

    public const string ResourceType = "incident_workflow_instance";

    public string Id { get; set; } = null!;
    public string Summary { get; set; } = null!;
    public PagerDutyReference Incident { get; set; } = null!;
    public PagerDutyReference IncidentWorkflow { get; set; } = null!;
    public PagerDutyReference WorkflowTrigger { get; set; } = null!;
    public PagerDutyReference Service { get; set; } = null!;

    public override IncidentWorkflowInstanceEventType EventType => EventTypeSuffix switch {
        "workflow.started"   => IncidentWorkflowInstanceEventType.Started,
        "workflow.completed" => IncidentWorkflowInstanceEventType.Completed
    };

}

public class ServiceWebhookPayload: AbstractWebhookPayload<ServiceEventType> {

    public const string ResourceType = "service";

    public string Id { get; set; } = null!;
    public Uri HtmlUrl { get; set; } = null!;
    public Uri Self { get; set; } = null!;
    public string Summary { get; set; } = null!;
    public string AlertCreation { get; set; } = null!;
    public ICollection<PagerDutyReference> Teams { get; } = new List<PagerDutyReference>();

    public override ServiceEventType EventType => EventTypeSuffix switch {
        "created" => ServiceEventType.Created,
        "deleted" => ServiceEventType.Deleted,
        "updated" => ServiceEventType.Updated
    };

}

internal class IncidentTypeWrapper {

    public string Name { get; set; } = null!;

}

public record PagerDutyReference(string Id, ReferenceType Type, Uri? HtmlUrl, Uri? Self, string? Summary);

public record ConferenceBridge(string? ConferenceNumber, Uri? ConferenceUrl);

public class PingWebhookPayload: AbstractWebhookPayload<PingEventType> {

    public const string ResourceType = "pagey";

    public string Message { get; set; } = null!;

    public override PingEventType EventType => EventTypeSuffix switch {
        "ping" => PingEventType.Ping
    };

}

public record ConferenceNumber(string Label, string Number);

public record CustomField(string Id, string Name, string Namespace, CustomFieldDataType DataType, CustomFieldType FieldType, /*string Type, (this is always "field") */ object Value);

public enum ReferenceType {

    UserReference,
    EscalationPolicyReference,
    ServiceReference,
    TeamReference,
    PriorityReference,
    IncidentWorkflowReference,
    WorkflowTriggerReference,
    InboundIntegrationReference

}

public enum IncidentStatus {

    Triggered,
    Acknowledged,
    Resolved

}

public enum CustomFieldType {

    SingleValue,
    SingleValueFixed,
    MultiValue,
    MultiValueFixed

}

public enum CustomFieldDataType {

    Boolean,
    Integer,
    Float,
    String,
    Datetime,
    Url

}

public enum IncidentEventType {

    Acknowledged,
    Delegated,
    Escalated,
    IncidentTypeChanged,
    PriorityUpdated,
    Reassigned,
    Reopened,
    Resolved,
    ServiceUpdated,
    Triggered,
    Unacknowledged

}

public enum PingEventType {

    Ping

}

public enum IncidentNoteEventType {

    Annotated

}

public enum IncidentConferenceBridgeEventType {

    Updated

}

public enum IncidentCustomFieldValuesEventType {

    Updated

}

public enum IncidentResponderEventType {

    Added,
    Replied

}

public enum IncidentStatusUpdateEventType {

    Published

}

public enum IncidentWorkflowInstanceEventType {

    Started,
    Completed

}

public enum ServiceEventType {

    Created,
    Deleted,
    Updated

}
