namespace Pager.Duty.Webhooks.Requests;

/// <summary>A status update was added to an incident</summary>
public class IncidentStatusUpdateWebhookPayload: AbstractWebhookPayload<IncidentStatusUpdateEventType> {

    /// <summary>The type of the data object sent by PagerDuty</summary>
    public const string DataType = "incident_status_update";

    /// <summary>The incident this status was added to</summary>
    public PagerDutyReference Incident { get; set; } = null!;

    /// <summary>Unique identifier</summary>
    public string Id { get; set; } = null!;

    /// <summary>Text body of the status update</summary>
    public string Message { get; set; } = null!;

    /// <inheritdoc />
    public override IncidentStatusUpdateEventType EventType => EventTypeSuffix switch {
        "status_update_published" => IncidentStatusUpdateEventType.Published
    };

}

/// <summary>Reasons the event was sent</summary>
public enum IncidentStatusUpdateEventType {

    /// <inheritdoc cref="IncidentStatusUpdateWebhookPayload" path="/summary" />
    Published

}