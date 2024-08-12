using Newtonsoft.Json;
using System;

namespace Pager.Duty.Requests;

/// <summary>
/// Image to be displayed on the alert and/or corresponding incident.
/// </summary>
public class Image {

    /// <summary>
    /// The source of the image being attached to the incident or alert. This image must be served via HTTPS.
    /// </summary>
    [JsonProperty("src")] public string Source { get; }

    /// <summary>
    /// Optional link for the image.
    /// </summary>
    public string? Href { get; }

    /// <summary>
    /// Optional alternative text for the image.
    /// </summary>
    [JsonProperty("alt")] public string? AltText { get; }

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

    /// <inheritdoc cref="Equals(object?)" />
    protected bool Equals(Image other) {
        return Source == other.Source && Href == other.Href && AltText == other.AltText;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Image) obj);
    }

    /// <inheritdoc />
    public override int GetHashCode() {
        unchecked {
            int hashCode = Source.GetHashCode();
            hashCode = (hashCode * 397) ^ (Href != null ? Href.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (AltText != null ? AltText.GetHashCode() : 0);
            return hashCode;
        }
    }

}