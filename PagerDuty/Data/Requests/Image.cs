using System;
using Newtonsoft.Json;

namespace Pager.Duty;

/// <summary>
/// Image to be displayed on the alert and/or corresponding incident.
/// </summary>
public class Image {

    [JsonProperty("src")] internal string Source { get; }
    [JsonProperty] internal string? Href { get; }
    [JsonProperty("alt")] internal string? AltText { get; }

    /// <summary>
    /// Image to be displayed on the alert and/or corresponding incident.
    /// </summary>
    /// <param name="source">The source of the image being attached to the incident or alert. This image must be served via HTTPS.</param>
    /// <param name="href">Optional link for the image.</param>
    /// <param name="altText">Optional alternative text for the image.</param>
    public Image(string source, string? href = null, string? altText = null) {
        Source  = source;
        Href    = href;
        AltText = altText;
    }

    /// <summary>
    /// Image to be displayed on the alert and/or corresponding incident.
    /// </summary>
    /// <param name="source">The source of the image being attached to the incident or alert. This image must be served via HTTPS.</param>
    /// <param name="href">Optional link for the image.</param>
    /// <param name="altText">Optional alternative text for the image.</param>
    public Image(Uri source, Uri? href, string? altText): this(source.ToString(), href?.ToString(), altText) { }

}