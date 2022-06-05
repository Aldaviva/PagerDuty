using System;
using Newtonsoft.Json;

namespace Pager.Duty;

/// <summary>
/// Link to be shown on the alert and/or corresponding incident.
/// </summary>
public class Link {

    /*
     * Using string instead of Uri because Json.NET unfortunately erroneously fails the DeepEquals check between a JValue<Uri> and a JValue<string> with the same content, and JToken.Parse doesn't create JToken<Uri> automatically.
     */
    [JsonProperty] internal string Href { get; }
    [JsonProperty] internal string? Text { get; }

    /// <summary>
    /// Link to be shown on the alert and/or corresponding incident.
    /// </summary>
    /// <param name="href">The link being attached to an incident or alert.</param>
    /// <param name="text">Optional information pertaining to this context link.</param>
    public Link(string href, string? text = null) {
        Href = href;
        Text = text;
    }

    /// <summary>
    /// Link to be shown on the alert and/or corresponding incident.
    /// </summary>
    /// <param name="href">The link being attached to an incident or alert.</param>
    /// <param name="text">Optional information pertaining to this context link.</param>
    public Link(Uri href, string? text): this(href.ToString(), text) { }

}