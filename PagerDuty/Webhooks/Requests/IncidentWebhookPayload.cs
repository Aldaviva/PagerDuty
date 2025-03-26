using Newtonsoft.Json;
using System;
using System.Collections.Generic;

#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).

namespace Pager.Duty.Webhooks.Requests;

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
    public ICollection<PagerDutyReference> Assignees { get; } = [];
    public PagerDutyReference EscalationPolicy { get; set; } = null!;
    public ICollection<PagerDutyReference> Teams { get; } = [];
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

public record ConferenceBridge(string? ConferenceNumber, Uri? ConferenceUrl);

internal class IncidentTypeWrapper {

    public string Name { get; set; } = null!;

}

public enum IncidentStatus {

    Triggered,
    Acknowledged,
    Resolved

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