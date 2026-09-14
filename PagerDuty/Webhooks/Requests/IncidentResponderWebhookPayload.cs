namespace Pager.Duty.Webhooks.Requests;

/// <summary>A responder was added to an incident, or they replied to a request</summary>
public class IncidentResponderWebhookPayload: AbstractWebhookPayload<IncidentResponderEventType> {

    /// <summary>The type of the data object sent by PagerDuty</summary>
    public const string DataType = "incident_responder";

    /// <summary>The incident this user is responding to</summary>
    public PagerDutyReference Incident { get; set; } = null!;

    /// <summary>The user that has been added as a responder to the <see cref="Incident"/></summary>
    public PagerDutyReference User { get; set; } = null!;

    /// <summary>The escalation policy that has been added as a responder to the <see cref="Incident"/></summary>
    public PagerDutyReference EscalationPolicy { get; set; } = null!;

    /// <summary>Text that is sent to responders describing the request</summary>
    public string Message { get; set; } = null!;

    /// <summary>Whether the responder has accepted the invitation</summary>
    public ResponderStatus State { get; set; }

    /// <inheritdoc />
    public override IncidentResponderEventType EventType => EventTypeSuffix switch {
        "added"   => IncidentResponderEventType.Added,
        "replied" => IncidentResponderEventType.Replied
    };

}

/// <inheritdoc cref="IncidentResponderWebhookPayload.State" path="/summary" />
public enum ResponderStatus {

    /// <summary>The user has not responded yet</summary>
    Pending,

    /// <summary>The user has accepted the request</summary>
    Joined,

    /// <summary>The user has rejected the request</summary>
    Declined

}

/// <summary>Reasons the event was sent</summary>
public enum IncidentResponderEventType {

    /// <summary>A responder was invited to join the incident</summary>
    Added,

    /// <summary>A responder accepted or rejected the request to join the incident</summary>
    Replied

}