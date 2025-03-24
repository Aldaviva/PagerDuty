using Pager.Duty.Webhooks;
using Pager.Duty.Webhooks.Requests;

await using WebApplication webapp = WebApplication.Create(args);

var logger = webapp.Services.GetRequiredService<ILogger<Program>>();

const string secretEnvVarName = "PAGERDUTY_WEBHOOK_SECRET";
WebhookResource webhookResource = new(Environment.GetEnvironmentVariable(secretEnvVarName) ??
    throw new ArgumentNullException(secretEnvVarName, "Missing required PagerDuty webhook secret environment variable"));
webapp.MapPost("/", webhookResource.HandlePostRequest);

webhookResource.PingReceived += (_, ping) => logger.LogInformation("Received ping with event {event} and message {msg}", ping.EventType, ping.Message);
webhookResource.IncidentReceived += (_, incident) => {
    switch (incident.EventType) {
        case IncidentEventType.Triggered:
            logger.LogInformation("Incident #{num} {title} triggered on {svc} with {urgency} urgency", incident.IncidentNumber, incident.Title, incident.Service.Summary,
                incident.HighUrgency ? "high" : "low");
            break;
        case IncidentEventType.Acknowledged:
            logger.LogInformation("Incident #{num} {title} acknowledged by {user}", incident.IncidentNumber, incident.Title, incident.Assignees.First().Summary);
            break;
        case IncidentEventType.Unacknowledged:
            logger.LogInformation("Incident #{num} {title} unacknowledged by {user}", incident.IncidentNumber, incident.Title, incident.Assignees.First().Summary);
            break;
        case IncidentEventType.Resolved:
            logger.LogInformation("Incident #{num} {title} resolved by {user}", incident.IncidentNumber, incident.Title, incident.Metadata.Agent?.Summary);
            break;
        default:
            logger.LogInformation("Incident #{num} {title} is now {state}", incident.IncidentNumber, incident.Title, incident.Status);
            break;
    }
};

await webapp.RunAsync();