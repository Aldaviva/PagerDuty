using System;
using System.Collections.Generic;

namespace Pager.Duty.Webhooks.Requests;

public class ServiceWebhookPayload: AbstractWebhookPayload<ServiceEventType> {

    public const string ResourceType = "service";

    public string Id { get; set; } = null!;
    public Uri HtmlUrl { get; set; } = null!;
    public Uri Self { get; set; } = null!;
    public string Summary { get; set; } = null!;
    public string AlertCreation { get; set; } = null!;
    public ICollection<PagerDutyReference> Teams { get; } = [];

    public override ServiceEventType EventType => EventTypeSuffix switch {
        "created" => ServiceEventType.Created,
        "deleted" => ServiceEventType.Deleted,
        "updated" => ServiceEventType.Updated
    };

}

public enum ServiceEventType {

    Created,
    Deleted,
    Updated

}