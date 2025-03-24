using Pager.Duty.Responses;

namespace Pager.Duty.Requests;

/// <summary>
/// <para>The incident referenced with the <c>dedupKey</c> will enter the resolved state.</para>
/// <para>Once an incident is resolved, it won't generate any additional notifications. New trigger events with the same <c>dedupKey</c> as a resolved incident won't re-open the incident. Instead, a new incident will be created.</para>
/// <para>Use this event action when the problem that caused the initial trigger event has been fixed.</para>
/// </summary>
public class ResolveAlert: FollowUpAlert {

    /// <summary>
    /// <para>The incident referenced with the <c>dedupKey</c> will enter the resolved state.</para>
    /// <para>Once an incident is resolved, it won't generate any additional notifications. New trigger events with the same <c>dedupKey</c> as a resolved incident won't re-open the incident. Instead, a new incident will be created.</para>
    /// <para>Use this event action when the problem that caused the initial trigger event has been fixed.</para>
    /// </summary>
    /// <param name="dedupKey">
    /// <para>Deduplication key for correlating triggers and resolves. This should come from the <see cref="AlertResponse.DedupKey"/> value in the <see cref="AlertResponse"/> returned after sending a <see cref="TriggerAlert"/>. The maximum permitted length of this property is 255 characters.</para>
    /// <para>The key used to correlate triggers, acknowledges, and resolves for the same alert.</para>
    /// <para>Every alert event has a <c>dedupKey</c>: a string which identifies the alert triggered for the given event. The <c>dedupKey</c> can be specified when submitting the first trigger event that creates an alert. If omitted, it will be generated automatically by PagerDuty and returned in the Events API v2 response.</para>
    /// <para>Submitting subsequent events with the same <c>dedupKey</c> will result in those events being applied to an open alert matching that <c>dedupKey</c>. Once the alert is resolved, any further events with the same <c>dedupKey</c> will create a new alert (for trigger events) or be dropped (for acknowledge and resolve events). Alerts will only be created for events with an event_action of trigger. Acknowledge or resolve events without a currently open alert will not create a new one.</para>
    /// <para>Subsequent events for the same <c>dedupKey</c> will only apply to the open alert if the events are sent via the same <c>integrationKey</c> as the original trigger event. Subsequent acknowledge or resolve events sent via a different <c>integrationKey</c> from the original will be dropped.</para>
    /// <para>A trigger event sent without a <c>dedupKey</c> will always generate a new alert because the automatically generated <c>dedupKey</c> will be a unique UUID.</para>
    /// </param>
    public ResolveAlert(string dedupKey): base(EventAction.Resolve, dedupKey) { }

    /// <summary>
    /// <para>The incident triggered by the <paramref name="alert"/> will enter the resolved state.</para>
    /// <para>Once an incident is resolved, it won't generate any additional notifications. New trigger events with the same <c>dedupKey</c> as a resolved incident won't re-open the incident. Instead, a new incident will be created.</para>
    /// <para>Use this event action when the problem that caused the initial trigger event has been fixed.</para>
    /// </summary>
    /// <param name="alert">
    /// <para>Response from PagerDuty after triggering an alert, which will contain a deduplication key for correlating triggers and resolves. This should be returned after sending a <see cref="TriggerAlert"/>. The maximum permitted length of this property is 255 characters.</para>
    /// <para>The key used to correlate triggers, acknowledges, and resolves for the same alert.</para>
    /// <para>Every alert event has a <c>dedupKey</c>: a string which identifies the alert triggered for the given event. The <c>dedupKey</c> can be specified when submitting the first trigger event that creates an alert. If omitted, it will be generated automatically by PagerDuty and returned in the Events API v2 response.</para>
    /// <para>Submitting subsequent events with the same <c>dedupKey</c> will result in those events being applied to an open alert matching that <c>dedupKey</c>. Once the alert is resolved, any further events with the same <c>dedupKey</c> will create a new alert (for trigger events) or be dropped (for acknowledge and resolve events). Alerts will only be created for events with an event_action of trigger. Acknowledge or resolve events without a currently open alert will not create a new one.</para>
    /// <para>Subsequent events for the same <c>dedupKey</c> will only apply to the open alert if the events are sent via the same <c>integrationKey</c> as the original trigger event. Subsequent acknowledge or resolve events sent via a different <c>integrationKey</c> from the original will be dropped.</para>
    /// <para>A trigger event sent without a <c>dedupKey</c> will always generate a new alert because the automatically generated <c>dedupKey</c> will be a unique UUID.</para>
    /// </param>
    public ResolveAlert(AlertResponse alert): this(alert.DedupKey) { }

}