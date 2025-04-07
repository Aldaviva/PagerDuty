namespace Pager.Duty.Webhooks.Requests;

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

public enum IncidentResponderEventType {

    Added,
    Replied

}