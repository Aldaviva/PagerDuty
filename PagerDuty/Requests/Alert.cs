using Newtonsoft.Json;

namespace Pager.Duty.Requests;

/// <summary>
/// <para>A problem in a machine monitored system.</para>
/// <para>Alert events create incidents on a service in PagerDuty. The incident will be assigned to the person on-call. This will generate a notification (phone call, SMS, email, or mobile push notification depending on the on-call responder's preferences).</para>
/// </summary>
public abstract class Alert: Event {

    // ExceptionAdjustment: M:System.Uri.#ctor(System.String) -T:System.UriFormatException
    private static readonly Uri AlertUri = new("https://events.pagerduty.com/v2/enqueue");
    internal override Uri ApiUri => AlertUri;

    [JsonProperty]
    internal EventAction EventAction { get; }

    internal Alert(EventAction eventAction) {
        EventAction = eventAction;
    }

}