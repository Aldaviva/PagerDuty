using Newtonsoft.Json;
using System;

namespace Pager.Duty.Requests;

/// <summary>
/// <para>The Events API v2 can ingest multiple types of events.</para>
/// <para>Concrete implementations are <see cref="TriggerAlert"/>, <see cref="AcknowledgeAlert"/>, <see cref="ResolveAlert"/>, and <see cref="Change"/>.</para>
/// </summary>
public abstract class Event {

    /// <summary>
    /// Service-specific Integration Key
    /// </summary>
    [JsonProperty] internal string? RoutingKey { get; set; }

    internal abstract Uri ApiUriPath { get; }

}