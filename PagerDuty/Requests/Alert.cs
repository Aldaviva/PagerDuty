﻿using Newtonsoft.Json;
using System;

namespace Pager.Duty.Requests;

/// <summary>
/// <para>A problem in a machine monitored system.</para>
/// <para>Alert events create incidents on a service in PagerDuty. The incident will be assigned to the person on-call. This will generate a notification (phone call, SMS, email, or mobile push notification depending on the on-call responder's preferences).</para>
/// </summary>
public abstract class Alert: Event {

    // ExceptionAdjustment: M:System.Uri.#ctor(System.String,System.UriKind) -T:System.UriFormatException
    private static readonly Uri AlertUri = new("enqueue", UriKind.Relative);
    internal override Uri ApiUriPath => AlertUri;

    [JsonProperty]
    internal EventAction EventAction { get; }

    internal Alert(EventAction eventAction) {
        EventAction = eventAction;
    }

    /// <inheritdoc cref="Equals(object?)" />
    protected bool Equals(Alert other) {
        return EventAction == other.EventAction;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return ReferenceEquals(this, obj) || (obj is Alert other && Equals(other));
    }

    /// <inheritdoc />
    public override int GetHashCode() {
        return (int) EventAction;
    }

}