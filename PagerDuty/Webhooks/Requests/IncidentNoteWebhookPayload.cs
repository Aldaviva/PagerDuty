namespace Pager.Duty.Webhooks.Requests;

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

public enum IncidentNoteEventType {

    Annotated

}