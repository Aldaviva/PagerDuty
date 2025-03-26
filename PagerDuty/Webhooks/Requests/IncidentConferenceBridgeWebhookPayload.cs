using System;
using System.Collections.Generic;

#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).

namespace Pager.Duty.Webhooks.Requests;

public class IncidentConferenceBridgeWebhookPayload: AbstractWebhookPayload<IncidentConferenceBridgeEventType> {

    public const string ResourceType = "incident_conference_bridge";

    public PagerDutyReference Incident { get; set; } = null!;
    public ICollection<ConferenceNumber> ConferenceNumbers { get; } = [];
    public Uri ConferenceUrl { get; set; } = null!;

    public override IncidentConferenceBridgeEventType EventType => EventTypeSuffix switch {
        "conference_bridge.updated" => IncidentConferenceBridgeEventType.Updated
    };

}

public record ConferenceNumber(string Label, string Number);

public enum IncidentConferenceBridgeEventType {

    Updated

}