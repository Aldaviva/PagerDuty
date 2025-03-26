#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).

namespace Pager.Duty.Webhooks.Requests;

public class PingWebhookPayload: AbstractWebhookPayload<PingEventType> {

    public const string ResourceType = "pagey";

    public string Message { get; set; } = null!;

    public override PingEventType EventType => EventTypeSuffix switch {
        "ping" => PingEventType.Ping
    };

}

public enum PingEventType {

    Ping

}