using Newtonsoft.Json;
using System;

namespace Pager.Duty.Requests;

/// <summary>
/// <para>A problem in a machine monitored system.</para>
/// <para>Alert events create incidents on a service in PagerDuty. The incident will be assigned to the person on-call. This will generate a notification (phone call, SMS, email, or mobile push notification depending on the on-call responder's preferences).</para>
/// </summary>
public abstract class Alert: Event {

    private static readonly Uri AlertUri = new("enqueue", UriKind.Relative);
    internal override Uri ApiUriPath => AlertUri;

    [JsonProperty]
    internal EventAction EventAction { get; }

    internal Alert(EventAction eventAction) {
        EventAction = eventAction;
    }

}