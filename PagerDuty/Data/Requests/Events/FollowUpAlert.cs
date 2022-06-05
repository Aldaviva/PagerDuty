using Newtonsoft.Json;

namespace Pager.Duty;

/// <summary>
/// <para>Follow up events can be sent to acknowledge or resolve an existing alert.</para>
/// <para>If an alert already exists for a problem, it can be grouped into a single incident by setting <see cref="DedupKey"/> to the corresponding value returned by the <see cref="TriggerAlert"/>.</para>
/// <para>See <see cref="AcknowledgeAlert"/> and <see cref="ResolveAlert"/>.</para>
/// </summary>
public abstract class FollowUpAlert: Alert {

    [JsonProperty]
    internal string DedupKey { get; }

    internal FollowUpAlert(EventAction eventAction, string dedupKey): base(eventAction) {
        DedupKey = dedupKey;
    }

}