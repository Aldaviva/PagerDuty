namespace Pager.Duty.Webhooks.Requests;

/// <summary>An incident workflow started or finished</summary>
public class IncidentWorkflowInstanceWebhookPayload: AbstractWebhookPayload<IncidentWorkflowInstanceEventType> {

    /// <summary>The type of the data object sent by PagerDuty</summary>
    public const string DataType = "incident_workflow_instance";

    /// <summary>Unique identifier</summary>
    public string Id { get; set; } = null!;

    /// <summary>Name of the workflow instance</summary>
    public string Summary { get; set; } = null!;

    /// <summary>The incident this workflow ran on</summary>
    public PagerDutyReference Incident { get; set; } = null!;

    /// <summary>Workflow that started or finished</summary>
    public PagerDutyReference IncidentWorkflow { get; set; } = null!;

    /// <summary>Trigger that started the workflow</summary>
    public PagerDutyReference WorkflowTrigger { get; set; } = null!;

    /// <summary>Service affected by the incident</summary>
    public PagerDutyReference Service { get; set; } = null!;

    /// <inheritdoc />
    public override IncidentWorkflowInstanceEventType EventType => EventTypeSuffix switch {
        "workflow.started"   => IncidentWorkflowInstanceEventType.Started,
        "workflow.completed" => IncidentWorkflowInstanceEventType.Completed
    };

}

/// <summary>Reasons the event was sent</summary>
public enum IncidentWorkflowInstanceEventType {

    /// <summary>Workflow began running</summary>
    Started,

    /// <summary>Workflow finished running</summary>
    Completed

}