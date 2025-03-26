#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).

namespace Pager.Duty.Webhooks.Requests;

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

public enum IncidentStatusUpdateEventType {

    Published

}