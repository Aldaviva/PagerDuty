using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Pager.Duty.Webhooks.Requests;

/// <summary>An incident was either triggered, acknowledged, unacknowledged, delegated, escalated, prioritized, assigned, reopened, or resolved, or had its type or services changed</summary>
public class IncidentWebhookPayload: AbstractWebhookPayload<IncidentEventType>, IBelongsToAccount {

    /// <summary>The type of the data object sent by PagerDuty</summary>
    public const string DataType = "incident";

    /// <summary>Unique identifier, like <c>PGR0VU2</c></summary>
    public string Id { get; set; } = null!;

    /// <summary>API URL of the incident</summary>
    public Uri Self { get; set; } = null!;

    /// <summary>Web page URL of the incident</summary>
    public Uri HtmlUrl { get; set; } = null!;

    /// <summary>Numeric identifier for the incident, like <c>2</c></summary>
    [JsonProperty("number")]
    public long IncidentNumber { get; set; }

    /// <summary>Whether the incident has been acknowledged or resolved</summary>
    public IncidentStatus Status { get; set; }

    /// <summary>Deduplication key for the incident</summary>
    public string IncidentKey { get; set; } = null!;

    /// <summary>When the incident was first triggered</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Summary of the incident</summary>
    public string Title { get; set; } = null!;

    [JsonProperty("incident_type")]
    internal IncidentTypeWrapper IncidentTypeWrapper { get; set; } = new();

    /// <summary>Name of the type of incident, such as <c>incident_default</c> for Base Incident</summary>
    [JsonIgnore]
    public string IncidentType {
        get => IncidentTypeWrapper.Name;
        set => IncidentTypeWrapper.Name = value;
    }

    /// <summary>The main service on which this incident was triggered</summary>
    public PagerDutyReference Service { get; set; } = null!;

    /// <summary>Zero or more responders who have been assigned to this incident</summary>
    public ICollection<PagerDutyReference> Assignees { get; } = [];

    /// <summary>Escalation policy of the service on which this incident was triggered</summary>
    public PagerDutyReference EscalationPolicy { get; set; } = null!;

    /// <summary>Zero or more teams that have been assigned to the incident</summary>
    public ICollection<PagerDutyReference> Teams { get; } = [];

    /// <summary>The custom urgency of the incident</summary>
    public PagerDutyReference? Priority { get; set; } = null!;

    [JsonProperty("urgency")]
    internal string Urgency { get; set; } = null!;

    /// <summary>Whether the incident urgency is high or low</summary>
    [JsonIgnore]
    public bool HighUrgency {
        get => Urgency == "high";
        set => Urgency = value ? "high" : "low";
    }

    /// <summary>Meeting about the incident</summary>
    public ConferenceBridge? ConferenceBridge { get; set; }

    /// <summary>If the incident was resolved by being merged into another incident</summary>
    public string? ResolveReason { get; set; }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public string? AccountSubdomain => BelongsToAccountHelper.GetAccountSubdomain(HtmlUrl);

}

/// <summary>Meeting about the incident</summary>
/// <param name="ConferenceNumber">Phone number to dial into the meeting</param>
/// <param name="ConferenceUrl">URL to join the meeting</param>
public record ConferenceBridge(string? ConferenceNumber, Uri? ConferenceUrl);

internal sealed class IncidentTypeWrapper {

    public string Name { get; set; } = null!;

}

/// <summary>Whether an incident has been acknowledged or resolved</summary>
public enum IncidentStatus {

    /// <summary>The incident is open and no one has acknowledged it</summary>
    Triggered,

    /// <summary>The incident is open and has been acknowledged by someone</summary>
    Acknowledged,

    /// <summary>The incident is closed</summary>
    Resolved

}

/// <summary>Reasons the event was sent</summary>
public enum IncidentEventType {

    /// <summary>Someone acknowledged the incident</summary>
    Acknowledged,

    /// <summary>The responder assigned the incident to someone else</summary>
    Delegated,

    /// <summary>The assignee did not respond, so the escalation policy reassigned it to someone higher up</summary>
    Escalated,

    /// <summary>The type of the incident was modified, for example from Base to Major or Security</summary>
    IncidentTypeChanged,

    /// <summary>The incident was reprioritized</summary>
    PriorityUpdated,

    /// <summary>The incident was assigned to a different user</summary>
    Reassigned,

    /// <summary>The incident state was changed from <see cref="IncidentStatus.Resolved"/> to <see cref="IncidentStatus.Triggered"/></summary>
    Reopened,

    /// <summary>The incident was closed</summary>
    Resolved,

    /// <summary>The main service of the incident was changed</summary>
    ServiceUpdated,

    /// <summary>The incident was created and opened</summary>
    Triggered,

    /// <summary>The user who previously acknowledged the incident undid that action to put the incident back in the <see cref="IncidentStatus.Triggered"/> state</summary>
    Unacknowledged

}