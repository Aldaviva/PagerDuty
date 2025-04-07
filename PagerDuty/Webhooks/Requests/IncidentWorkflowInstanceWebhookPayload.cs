namespace Pager.Duty.Webhooks.Requests;

public class IncidentWorkflowInstanceWebhookPayload: AbstractWebhookPayload<IncidentWorkflowInstanceEventType> {

    public const string ResourceType = "incident_workflow_instance";

    public string Id { get; set; } = null!;
    public string Summary { get; set; } = null!;
    public PagerDutyReference Incident { get; set; } = null!;
    public PagerDutyReference IncidentWorkflow { get; set; } = null!;
    public PagerDutyReference WorkflowTrigger { get; set; } = null!;
    public PagerDutyReference Service { get; set; } = null!;

    public override IncidentWorkflowInstanceEventType EventType => EventTypeSuffix switch {
        "workflow.started"   => IncidentWorkflowInstanceEventType.Started,
        "workflow.completed" => IncidentWorkflowInstanceEventType.Completed
    };

}

public enum IncidentWorkflowInstanceEventType {

    Started,
    Completed

}