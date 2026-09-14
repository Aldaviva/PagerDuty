using System;
using System.Collections.Generic;

namespace Pager.Duty.Webhooks.Requests;

/// <summary>A service was created, updated, or deleted</summary>
public class ServiceWebhookPayload: AbstractWebhookPayload<ServiceEventType>, IBelongsToAccount {

    /// <summary>The type of the data object sent by PagerDuty</summary>
    public const string DataType = "service";

    /// <summary>Unique identifier</summary>
    public string Id { get; set; } = null!;

    /// <summary>Web page URL of the service</summary>
    public Uri HtmlUrl { get; set; } = null!;

    /// <summary>API URL of the service</summary>
    public Uri Self { get; set; } = null!;

    /// <summary>Title of the service</summary>
    public string Summary { get; set; } = null!;

    /// <summary>The teams that are assigned to this service</summary>
    public ICollection<PagerDutyReference> Teams { get; } = [];

    /// <inheritdoc />
    public override ServiceEventType EventType => EventTypeSuffix switch {
        "created" => ServiceEventType.Created,
        "deleted" => ServiceEventType.Deleted,
        "updated" => ServiceEventType.Updated
    };

    /// <inheritdoc />
    public string? AccountSubdomain => BelongsToAccountHelper.GetAccountSubdomain(HtmlUrl);

}

/// <summary>Reasons the event was sent</summary>
public enum ServiceEventType {

    /// <summary>A new service was provisioned</summary>
    Created,

    /// <summary>A service was removed</summary>
    Deleted,

    /// <summary>A service was changed</summary>
    Updated

}