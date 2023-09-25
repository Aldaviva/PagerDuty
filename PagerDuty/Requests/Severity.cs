namespace Pager.Duty.Requests;

/// <summary>
/// The perceived severity of the status the event is describing with respect to the affected system.
/// </summary>
public enum Severity {

    /// <summary>
    /// The highest severity
    /// </summary>
    Critical,

    /// <summary>
    /// The second-highest severity
    /// </summary>
    Error,

    /// <summary>
    /// The second-lowest severity
    /// </summary>
    Warning,

    /// <summary>
    /// The lowest severity
    /// </summary>
    Info

}