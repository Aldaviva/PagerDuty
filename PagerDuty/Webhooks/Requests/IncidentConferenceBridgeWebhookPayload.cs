using System;
using System.Collections.Generic;

namespace Pager.Duty.Webhooks.Requests;

/// <summary>An incident's conference bridge phone number or meeting URL were changed</summary>
public class IncidentConferenceBridgeWebhookPayload: AbstractWebhookPayload<IncidentConferenceBridgeEventType> {

    /// <summary>The type of the data object sent by PagerDuty</summary>
    public const string DataType = "incident_conference_bridge";

    /// <summary>The incident this conference bridge is about</summary>
    public PagerDutyReference Incident { get; set; } = null!;

    /// <summary>Phone numbers to join the meeting</summary>
    public ICollection<ConferenceNumber> ConferenceNumbers { get; } = [];

    /// <summary>URL to join the meeting</summary>
    public Uri ConferenceUrl { get; set; } = null!;

    /// <inheritdoc />
    public override IncidentConferenceBridgeEventType EventType => EventTypeSuffix switch {
        "conference_bridge.updated" => IncidentConferenceBridgeEventType.Updated
    };

}

/// <summary>Phone number to join a meeting</summary>
/// <param name="Label">Name of the phone number</param>
/// <param name="Number">Dial-in phone number</param>
public record ConferenceNumber(string Label, string Number);

/// <summary>Reasons the event was sent</summary>
public enum IncidentConferenceBridgeEventType {

    /// <inheritdoc cref="IncidentConferenceBridgeWebhookPayload" path="/summary" />
    Updated

}