namespace Pager.Duty.Webhooks.Requests;

/// <summary>A note was added to an incident</summary>
public class IncidentNoteWebhookPayload: AbstractWebhookPayload<IncidentNoteEventType> {

    /// <summary>The type of the data object sent by PagerDuty</summary>
    public const string DataType = "incident_note";

    /// <summary>The incident this note belongs to</summary>
    public PagerDutyReference Incident { get; set; } = null!;

    /// <summary>Unique identifier</summary>
    public string Id { get; set; } = null!;

    /// <summary>The text body of the note</summary>
    public string Content { get; set; } = null!;

    /// <inheritdoc />
    public override IncidentNoteEventType EventType => EventTypeSuffix switch {
        "annotated" => IncidentNoteEventType.Annotated
    };

}

/// <summary>Reasons the event was sent</summary>
public enum IncidentNoteEventType {

    /// <inheritdoc cref="IncidentNoteWebhookPayload" path="/summary" />
    Annotated

}