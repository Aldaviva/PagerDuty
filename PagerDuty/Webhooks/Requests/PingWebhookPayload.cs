namespace Pager.Duty.Webhooks.Requests;

/// <summary>A test request was sent from the webhook management web page</summary>
public class PingWebhookPayload: AbstractWebhookPayload<PingEventType> {

    /// <summary>The type of the data object sent by PagerDuty</summary>
    public const string DataType = "ping";

    /// <summary>Text sent with the ping</summary>
    public string Message { get; set; } = null!;

    /// <inheritdoc />
    public override PingEventType EventType => EventTypeSuffix switch {
        "ping" => PingEventType.Ping
    };

}

/// <summary>Reasons the event was sent</summary>
public enum PingEventType {

    /// <summary>PagerDuty sent a ping to test the webhook server</summary>
    Ping

}