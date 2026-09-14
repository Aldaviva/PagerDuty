using System;
using System.Collections.Generic;

namespace Pager.Duty.Webhooks.Requests;

/// <summary>An incident's custom field values were changed</summary>
public class IncidentFieldValuesWebhookPayload: AbstractWebhookPayload<IncidentCustomFieldValuesEventType> {

    /// <summary>The type of the data object sent by PagerDuty</summary>
    public const string DataType = "incident_field_values";

    /// <summary>The incident these field values belong to</summary>
    public PagerDutyReference Incident { get; set; } = null!;

    /// <summary>All custom field values for this incident, including ones that were not recently modified</summary>
    public ICollection<CustomField> CustomFields { get; } = [];

    /// <summary>Custom field values for this incident which were just modified</summary>
    public ICollection<CustomField> ChangedCustomFields { get; } = [];

    /// <inheritdoc />
    public override IncidentCustomFieldValuesEventType EventType => EventTypeSuffix switch {
        "custom_field_values.updated" => IncidentCustomFieldValuesEventType.Updated
    };

}

/// <summary>User-defined key-value pairs attached to an incident</summary>
/// <param name="Id">Unique identifier of the field</param>
/// <param name="Name">Label or key for the field</param>
/// <param name="Namespace">Scopes the <paramref name="Name"/> to avoid collisions</param>
/// <param name="DataType">The type of data that can be stored in the field's <paramref name="Value"/></param>
/// <param name="FieldType">Constraints on the <paramref name="Value"/>'s array length and enum values</param>
/// <param name="Value">The data stored in the field for a specific incident</param>
public record CustomField(string Id, string Name, string Namespace, CustomFieldDataType DataType, CustomFieldType FieldType, object Value);

/// <summary>Constraints on the length and possible values of the field</summary>
public enum CustomFieldType {

    /// <summary>Singleton value, can be any value in the field data type's domain</summary>
    SingleValue,

    /// <summary>Singleton value, restricted to only certain predefined values, like an enum</summary>
    SingleValueFixed,

    /// <summary>Multiple values like an array, can be any values in the field data type's domain</summary>
    MultiValue,

    /// <summary>Multiple values like an array, restricted to only certain predefined values, like an enum</summary>
    MultiValueFixed

}

/// <summary>The type of data that can be stored in the field's value</summary>
public enum CustomFieldDataType {

    /// <summary><see cref="bool"/></summary>
    Boolean,

    /// <summary>64-bit signed <see cref="long"/></summary>
    Integer,

    /// <summary>64-bit <see cref="double"/></summary>
    Float,

    /// <summary>UTF-8 <see cref="string"/></summary>
    String,

    /// <summary>Zoned date time in the format <c>yyyy-MM-dd hh:mm:ss zzzz</c>, where <c>zzzz</c> is an IANA/Olsen time zone ID</summary>
    Datetime,

    /// <summary><see cref="Uri"/></summary>
    Url

}

/// <summary>Reasons the event was sent</summary>
public enum IncidentCustomFieldValuesEventType {

    /// <inheritdoc cref="IncidentFieldValuesWebhookPayload" path="/summary" />
    Updated

}