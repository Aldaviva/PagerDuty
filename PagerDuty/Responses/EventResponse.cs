using Newtonsoft.Json;

namespace Pager.Duty.Responses;

/// <summary>
/// An event has been created or updated.
/// </summary>
public abstract class EventResponse {

    /// <summary>
    /// A description of the problem, or <code>Event processed</code> if successful.
    /// </summary>
    [JsonProperty] public string Message { get; internal set; } = null!;

    /// <summary>
    /// Returns <code>success</code> if successful, or a short error message in case of a failure.
    /// </summary>
    [JsonProperty] public string Status { get; internal set; } = null!;

    /// <summary>
    /// <c>true</c> if the event was written successfully, or <c>false</c> if there was a failure.
    /// </summary>
    public bool IsSuccessful => Status == "success";

    internal EventResponse() { }

}