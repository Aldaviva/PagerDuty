using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Pager.Duty.Webhooks.Requests;

internal sealed class WebhookPayloadEnvelope {

    public WebhookPayloadMetadata Event { get; set; } = null!;

}

/// <summary>A webhook request body received from PagerDuty.</summary>
public interface IWebhookPayload {

    /// <summary>Extra information about the received webhook payload envelope</summary>
    public WebhookPayloadMetadata Metadata { get; set; }

}

/// <inheritdoc />
public interface IWebhookPayload<out T>: IWebhookPayload where T: Enum {

    /// <summary>Verb that describes what happened with the event, such as <see cref="IncidentEventType.Triggered"/>.</summary>
    public T EventType { get; }

}

/// <inheritdoc />
public abstract class AbstractWebhookPayload<T>: IWebhookPayload<T> where T: Enum {

    /// <inheritdoc />
    public WebhookPayloadMetadata Metadata { get; set; } = null!;

    /// <inheritdoc />
    public abstract T EventType { get; }

    /// <summary>The second part of the event type, for example <c>triggered</c> for the event type <c>incident.triggered</c>.</summary>
    protected string EventTypeSuffix => Metadata.EventType.Substring(Metadata.EventType.IndexOf('.') + 1);

}

/// <summary>Extra information about the received webhook payload envelope</summary>
public class WebhookPayloadMetadata {

    /// <summary>The unique identifier of the event</summary>
    public string Id { get; set; } = null!;

    /// <summary>Noun and verb that describe what happened to what, such as <c>incident.triggered</c>.</summary>
    [EditorBrowsable(EditorBrowsableState.Never)] public string EventType { get; set; } = null!;

    /// <summary>The root resource type (leftmost part of the <c>event_type</c>) this event is about (currently <c>incident</c> or <c>service</c>). It can be different from the more specific <c>data.type</c> in the event payload.</summary>
    public string ResourceType { get; set; } = null!;

    /// <summary>When the event occurred</summary>
    public DateTimeOffset OccurredAt { get; set; }

    /// <summary>Indicates who or what initiated the event</summary>
    public PagerDutyReference? Agent { get; set; }

    /// <summary>Information about where the event was triggered</summary>
    public IDictionary<string, string>? Client { get; set; }
    [JsonProperty] internal JToken Data { get; set; } = null!;

}

/// <summary>A pointer to another REST entity</summary>
/// <param name="Id">Unique identifier of the entity that this reference points to</param>
/// <param name="Type">Type of the entity that this reference points to</param>
/// <param name="HtmlUrl">Web page URL of the entity that this reference points to</param>
/// <param name="Self">API URL of the entity that this reference points to</param>
/// <param name="Summary">Title of the entity that this reference points to</param>
public record PagerDutyReference(string Id, ReferenceType Type, Uri? HtmlUrl, Uri? Self, string? Summary): IBelongsToAccount {

    /// <inheritdoc />
    public string? AccountSubdomain => BelongsToAccountHelper.GetAccountSubdomain(HtmlUrl);

}

/// <summary>The type of the entity that a reference points to</summary>
public enum ReferenceType {

    /// <summary>Pointer to a user</summary>
    UserReference,

    /// <summary>Pointer to an escalation policy</summary>
    EscalationPolicyReference,

    /// <summary>Pointer to a service</summary>
    ServiceReference,

    /// <summary>Pointer to a team of users</summary>
    TeamReference,

    /// <summary>Pointer to a custom incident priority</summary>
    PriorityReference,

    /// <summary>Pointer to a workflow</summary>
    IncidentWorkflowReference,

    /// <summary>Pointer to a workflow trigger</summary>
    WorkflowTriggerReference,

    /// <summary>Pointer to an integration that can trigger incidents on a service, like Nagios</summary>
    InboundIntegrationReference,

    /// <summary>Pointer to an incident</summary>
    IncidentReference

}