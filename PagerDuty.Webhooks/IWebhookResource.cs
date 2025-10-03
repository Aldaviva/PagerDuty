using Microsoft.AspNetCore.Http;
using Pager.Duty.Webhooks.Requests;
using System;
using System.Threading.Tasks;

namespace Pager.Duty.Webhooks;

public interface IWebhookResource {

    event EventHandler<PingWebhookPayload>? PingReceived;
    event EventHandler<IncidentWebhookPayload>? IncidentReceived;
    event EventHandler<IncidentNoteWebhookPayload>? IncidentNoteReceived;
    event EventHandler<IncidentConferenceBridgeWebhookPayload>? IncidentConferenceBridgeReceived;
    event EventHandler<IncidentFieldValuesWebhookPayload>? IncidentFieldValuesReceived;
    event EventHandler<IncidentStatusUpdateWebhookPayload>? IncidentStatusUpdateReceived;
    event EventHandler<IncidentResponderWebhookPayload>? IncidentResponderReceived;
    event EventHandler<IncidentWorkflowInstanceWebhookPayload>? IncidentWorkflowInstanceReceived;
    event EventHandler<ServiceWebhookPayload>? ServiceReceived;

    Task HandlePostRequest(HttpContext httpContext);

}