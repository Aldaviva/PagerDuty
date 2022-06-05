using System;
using Newtonsoft.Json;

namespace Pager.Duty;

/// <summary>
/// <para>The Events API v2 can ingest multiple types of events.</para>
/// <para>Concrete implementations are <see cref="TriggerAlert"/>, <see cref="AcknowledgeAlert"/>, <see cref="ResolveAlert"/>, and <see cref="Change"/>.</para>
/// </summary>
public abstract class PagerDutyEvent {

    [JsonProperty] internal string? RoutingKey { get; set; }

    internal abstract Uri ApiUri { get; }

}