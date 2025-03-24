using Newtonsoft.Json;

namespace Pager.Duty.Responses;

/// <summary>
/// An alert has been created or updated.
/// </summary>
public class AlertResponse: EventResponse {

    /// <summary>
    /// <para>The key used to correlate triggers, acknowledges, and resolves for the same alert.</para>
    /// <para>Every alert event has a <see cref="DedupKey"/>: a string which identifies the alert triggered for the given event. The <see cref="DedupKey"/> can be specified when submitting the first trigger event that creates an alert. If omitted, it will be generated automatically by PagerDuty and returned in the Events API v2 response.</para>
    /// <para>Submitting subsequent events with the same <see cref="DedupKey"/> will result in those events being applied to an open alert matching that <see cref="DedupKey"/>. Once the alert is resolved, any further events with the same <see cref="DedupKey"/> will create a new alert (for trigger events) or be dropped (for acknowledge and resolve events). Alerts will only be created for events with an event_action of trigger. Acknowledge or resolve events without a currently open alert will not create a new one.</para>
    /// <para>Subsequent events for the same <see cref="DedupKey"/> will only apply to the open alert if the events are sent via the same <c>integrationKey</c> as the original trigger event. Subsequent acknowledge or resolve events sent via a different <c>integrationKey</c> from the original will be dropped.</para>
    /// <para>A trigger event sent without a <see cref="DedupKey"/> will always generate a new alert because the automatically generated <see cref="DedupKey"/> will be a unique UUID.</para>
    /// </summary>
    [JsonProperty] public string DedupKey { get; internal set; } = null!;

}