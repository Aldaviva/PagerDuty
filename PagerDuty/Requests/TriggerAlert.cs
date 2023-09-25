using Newtonsoft.Json;
using Pager.Duty.Responses;

// ReSharper disable ReturnTypeCanBeEnumerable.Global - would prevent consumers from initializing the collection
// ReSharper disable CollectionNeverQueried.Global - queried by Json.NET during serialization
// ReSharper disable UnusedAutoPropertyAccessor.Global - queried by Json.NET during serialization

namespace Pager.Duty.Requests;

/// <summary>
/// <para>A new alert is opened or a trigger log entry is created on an existing alert if one already exists with the same <see cref="AlertResponse.DedupKey"/>.</para>
/// <para>Use this event action when a new problem has been detected. Additional triggers may be sent when a previously detected problem has occurred again.</para>
/// </summary>
public class TriggerAlert: Alert {

    /// <summary>
    /// <para>The key used to correlate triggers, acknowledges, and resolves for the same alert.</para>
    /// <para>Every alert event has a <see cref="DedupKey"/>: a string which identifies the alert triggered for the given event. The <see cref="DedupKey"/> can be specified when submitting the first trigger event that creates an alert. If omitted, it will be generated automatically by PagerDuty and returned in the Events API v2 response.</para>
    /// <para>Submitting subsequent events with the same <see cref="DedupKey"/> will result in those events being applied to an open alert matching that <see cref="DedupKey"/>. Once the alert is resolved, any further events with the same <see cref="DedupKey"/> will create a new alert (for trigger events) or be dropped (for acknowledge and resolve events). Alerts will only be created for events with an event_action of trigger. Acknowledge or resolve events without a currently open alert will not create a new one.</para>
    /// <para>Subsequent events for the same <see cref="DedupKey"/> will only apply to the open alert if the events are sent via the same <c>integrationKey</c> as the original trigger event. Subsequent acknowledge or resolve events sent via a different <c>integrationKey</c> from the original will be dropped.</para>
    /// <para>A trigger event sent without a <see cref="DedupKey"/> will always generate a new alert because the automatically generated <see cref="DedupKey"/> will be a unique UUID.</para>
    /// </summary>
    public string? DedupKey { get; set; }

    /// <summary>
    /// A brief text summary of the event, used to generate the summaries/titles of any associated alerts. The maximum permitted length of this property is 1024 characters.
    /// </summary>
    [JsonIgnore] public string Summary { get; }

    /// <summary>
    /// <para>The unique name of the location where the Change Event occurred.</para>
    /// <para>Required, defaults to your computer's NETBIOS name.</para>
    /// </summary>
    [JsonIgnore] public string Source { get; set; } = Environment.MachineName;

    /// <summary>
    /// The perceived severity of the status the event is describing with respect to the affected system.
    /// </summary>
    [JsonIgnore] public Severity Severity { get; }

    /// <summary>
    /// <para>The time at which the emitting tool detected or generated the event.</para>
    /// <para>Optional. When omitted, PagerDuty will use the current time when it receives the Event.</para>
    /// </summary>
    [JsonIgnore] public DateTimeOffset? Timestamp { get; set; }

    /// <summary>
    /// <para>Component of the source machine that is responsible for the event, for example <code>mysql</code> or <code>eth0</code></para>
    /// <para>Optional</para>
    /// </summary>
    [JsonIgnore] public string? Component { get; set; }

    /// <summary>
    /// <para>Logical grouping of components of a service, for example <code>app-stack</code></para>
    /// <para>Optional</para>
    /// </summary>
    [JsonIgnore] public string? Group { get; set; }

    /// <summary>
    /// <para>The class/type of the event, for example <code>ping failure</code> or <code>cpu load</code></para>
    /// <para>Optional</para>
    /// </summary>
    [JsonIgnore] public string? Class { get; set; }

    /// <summary>
    /// <para>Additional details about the event and affected system.</para>
    /// <para>You can pass a <c>new Dictionary&lt;string, object&gt;{{"key", "value"}}</c>, an anonymous object: <c>new { key = "value" }</c>, or any other object which can be serialized to JSON.</para>
    /// <para>Optional. When omitted, no custom details are sent.</para>
    /// </summary>
    [JsonIgnore] public object? CustomDetails { get; set; }

    /// <summary>
    /// <para>Links to be shown on the alert and/or corresponding incident.</para>
    /// <para>Optional. When omitted, an empty list is sent.</para>
    /// </summary>
    public ICollection<Link> Links { get; } = new List<Link>();

    /// <summary>
    /// <para>Images to be displayed on the alert and/or corresponding incident.</para>
    /// <para>Optional. When omitted, an empty list is sent.</para>
    /// </summary>
    public ICollection<Image> Images { get; } = new List<Image>();

    /// <summary>
    /// <para>The name of the monitoring client that is triggering this event.</para>
    /// <para>This will have the prefix "View in " and appear as a hyperlink to <see cref="ClientUrl"/> under the Client heading of the Alert's Details in the PagerDuty web application.</para>
    /// <para><see cref="Client"/> and <see cref="ClientUrl"/> should both be specified or omitted together.</para>
    /// <para>Optional. If you don't specify <see cref="Client"/>, the Alert details will not contain a Client heading or link at all, regardless of whether or not you specify <see cref="ClientUrl"/>. If you specify <see cref="Client"/> but not <see cref="ClientUrl"/>, the link will appear but clicking on it will have no effect.</para>
    /// </summary>
    public string? Client { get; set; }

    /// <summary>
    /// <para>The name of the monitoring client that is triggering this event.</para>
    /// <para>This will as the hyperlink destination "View in <c>Client</c>" link found under the Client heading of the Alert's details in the PagerDuty web application.</para>
    /// <para><see cref="Client"/> and <see cref="ClientUrl"/> should both be specified or omitted together.</para>
    /// <para>Optional. If you specify neither <see cref="Client"/> nor <see cref="ClientUrl"/>, the Alert details will not contain a Client heading or link at all. If you specify <see cref="Client"/> but not <see cref="ClientUrl"/>, the link will appear but clicking on it will have no effect.</para>
    /// </summary>
    public string? ClientUrl { get; set; }

    [JsonProperty] internal object Payload => new { Summary, Source, Severity, Timestamp, Component, Group, Class, CustomDetails };

    /// <summary>
    /// <para>A new alert is opened or a trigger log entry is created on an existing alert if one already exists with the same <see cref="AlertResponse.DedupKey"/>.</para>
    /// <para>Use this event action when a new problem has been detected. Additional triggers may be sent when a previously detected problem has occurred again.</para>
    /// </summary>
    /// <param name="severity">The perceived severity of the status the event is describing with respect to the affected system.</param>
    /// <param name="summary">A brief text summary of the event, used to generate the summaries/titles of any associated alerts. The maximum permitted length of this property is 1024 characters.</param>
    public TriggerAlert(Severity severity, string summary): base(EventAction.Trigger) {
        Summary  = summary;
        Severity = severity;
    }

}