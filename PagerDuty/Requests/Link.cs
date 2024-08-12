using System;

namespace Pager.Duty.Requests;

/// <summary>
/// Link to be shown on the alert and/or corresponding incident.
/// </summary>
public class Link {

    /*
     * Using string instead of Uri because Json.NET unfortunately erroneously fails the DeepEquals check between a JValue<Uri> and a JValue<string> with the same content, and JToken.Parse doesn't create JToken<Uri> automatically.
     */
    /// <summary>
    /// The link being attached to an incident or alert.
    /// </summary>
    public string Href { get; }

    /// <summary>
    /// Optional information pertaining to this context link.
    /// </summary>
    public string? Text { get; }

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

    /// <inheritdoc cref="Equals(object?)" />
    protected bool Equals(Link other) {
        return Href == other.Href && Text == other.Text;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Link) obj);
    }

    /// <inheritdoc />
    public override int GetHashCode() {
        unchecked {
            return (Href.GetHashCode() * 397) ^ (Text != null ? Text.GetHashCode() : 0);
        }
    }

}