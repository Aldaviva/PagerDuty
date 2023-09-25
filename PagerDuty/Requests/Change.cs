using Newtonsoft.Json;

// ReSharper disable ReturnTypeCanBeEnumerable.Global - would prevent consumers from initializing the collection
// ReSharper disable CollectionNeverQueried.Global - queried by Json.NET during serialization

namespace Pager.Duty.Requests;

/// <summary>
/// <para>A change in a system that does not represent a problem.</para>
/// <para>Change events provide context to responders when triaging an incident.</para>
/// <para>Change Events API enables you to send informational events about recent changes such as code deploys and system config changes from any system that can make an outbound HTTP connection. These events do not create incidents and do not send notifications; they are shown in context with incidents on the same PagerDuty service.</para>
/// <para>PagerDuty will only keep the last 90 days of Change Events, as per our data retention guidelines.</para>
/// <para>Examples: High error rate, CPU usage exceeded limit, Deployment failed</para>
/// </summary>
public class Change: Event {

    // ExceptionAdjustment: M:System.Uri.#ctor(System.String) -T:System.UriFormatException
    private static readonly Uri ChangeUri = new("https://events.pagerduty.com/v2/change/enqueue");
    internal override Uri ApiUri => ChangeUri;

    /// <summary>
    /// A brief text summary of the event, used to generate the summaries/titles of any associated alerts.
    /// </summary>
    [JsonIgnore] public string Summary { get; set; }

    /// <summary>
    /// <para>The unique name of the location where the Change Event occurred.</para>
    /// <para>Optional. When omitted, this is set to your computer's NETBIOS name.</para>
    /// </summary>
    [JsonIgnore] public string? Source { get; set; } = Environment.MachineName;

    /// <summary>
    /// <para>The time at which the emitting tool detected or generated the event.</para>
    /// <para>Optional. When omitted, PagerDuty will use the current time when it receives the Event.</para>
    /// </summary>
    [JsonIgnore] public DateTimeOffset? Timestamp { get; set; }

    /// <summary>
    /// <para>Additional details about the event.</para>
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

    [JsonProperty]
    internal object Payload => new { Summary, Source, Timestamp, CustomDetails };

    /// <summary>
    /// A change in a system that does not represent a problem.
    /// </summary>
    /// <param name="summary">A brief text summary of the event, used to generate the summaries/titles of any associated alerts.</param>
    public Change(string summary) {
        Summary = summary;
    }

}