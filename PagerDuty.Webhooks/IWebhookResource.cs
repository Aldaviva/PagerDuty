using Microsoft.AspNetCore.Http;
using Pager.Duty.Webhooks.Requests;
using System;
using System.Threading.Tasks;

namespace Pager.Duty.Webhooks;

/// <summary>
/// <para>PagerDuty webhook callback HTTP resource</para>
/// <para>You can map a request path to this resource in your ASP.NET Core server by passing <see cref="HandlePostRequest"/> to <c>WebApplication.MapPost</c>, then listen for the events (like <see cref="IncidentReceived"/>) to be notified when a webhook request is received from PagerDuty.</para>
/// </summary>
public interface IWebhookResource {

    /// <summary>A test request was sent from the webhook management web page</summary>
    event EventHandler<PingWebhookPayload>? PingReceived;

    /// <summary>An incident was either triggered, acknowledged, unacknowledged, delegated, escalated, prioritized, assigned, reopened, or resolved, or had its type or services changed</summary>
    event EventHandler<IncidentWebhookPayload>? IncidentReceived;

    /// <summary>A note was added to an incident</summary>
    event EventHandler<IncidentNoteWebhookPayload>? IncidentNoteReceived;

    /// <summary>An incident's conference bridge phone number or meeting URL were changed</summary>
    event EventHandler<IncidentConferenceBridgeWebhookPayload>? IncidentConferenceBridgeReceived;

    /// <summary>An incident's custom field values were changed</summary>
    event EventHandler<IncidentFieldValuesWebhookPayload>? IncidentFieldValuesReceived;

    /// <summary>A status update was added to an incident</summary>
    event EventHandler<IncidentStatusUpdateWebhookPayload>? IncidentStatusUpdateReceived;

    /// <summary>An responder was added to an incident, or they replied to a request</summary>
    event EventHandler<IncidentResponderWebhookPayload>? IncidentResponderReceived;

    /// <summary>An incident workflow started or finished</summary>
    event EventHandler<IncidentWorkflowInstanceWebhookPayload>? IncidentWorkflowInstanceReceived;

    /// <summary>A service was created, updated, or deleted</summary>
    event EventHandler<ServiceWebhookPayload>? ServiceReceived;

    /// <summary>
    /// <para>The request handler delegate method for the webhook callback POST resource.</para>
    /// <para>Usage:<code>
    /// webapp.MapPost("/pagerduty", webhookResource.HandlePostRequest);</code></para>
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    Task HandlePostRequest(HttpContext httpContext);

}