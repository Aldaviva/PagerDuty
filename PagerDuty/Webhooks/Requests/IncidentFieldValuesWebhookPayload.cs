using System.Collections.Generic;

#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).

namespace Pager.Duty.Webhooks.Requests;

public class IncidentFieldValuesWebhookPayload: AbstractWebhookPayload<IncidentCustomFieldValuesEventType> {

    public const string ResourceType = "incident_field_values";

    public PagerDutyReference Incident { get; set; } = null!;
    public ICollection<CustomField> CustomFields { get; } = [];
    public ICollection<CustomField> ChangedCustomFields { get; } = [];

    public override IncidentCustomFieldValuesEventType EventType => EventTypeSuffix switch {
        "custom_field_values.updated" => IncidentCustomFieldValuesEventType.Updated
    };

}

public record CustomField(string Id, string Name, string Namespace, CustomFieldDataType DataType, CustomFieldType FieldType, /*string Type, (this is always "field") */ object Value);

public enum CustomFieldType {

    SingleValue,
    SingleValueFixed,
    MultiValue,
    MultiValueFixed

}

public enum CustomFieldDataType {

    Boolean,
    Integer,
    Float,
    String,
    Datetime,
    Url

}

public enum IncidentCustomFieldValuesEventType {

    Updated

}