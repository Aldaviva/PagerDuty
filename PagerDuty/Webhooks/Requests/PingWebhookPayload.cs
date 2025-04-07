namespace Pager.Duty.Webhooks.Requests;

public class PingWebhookPayload: AbstractWebhookPayload<PingEventType> {

    public const string ResourceType = "ping";

    public string Message { get; set; } = null!;

    public override PingEventType EventType => EventTypeSuffix switch {
        "ping" => PingEventType.Ping
    };

}

public enum PingEventType {

    Ping

}