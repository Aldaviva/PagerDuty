using System;

// ReSharper disable UnusedAutoPropertyAccessor.Global - read by library consumers

namespace Pager.Duty;

/// <summary>
/// The top-level exception class for the PagerDuty library.
/// </summary>
public abstract class PagerDutyException: Exception {

    /// <summary>
    /// <c>true</c> if you may retry the API call because it may succeed in the future, or <c>false</c> if you should not retry because it will never succeed when sent again.
    /// </summary>
    public abstract bool RetryAllowedAfterDelay { get; }

    internal PagerDutyException(string message, Exception? cause): base(message, cause) { }

}

/// <summary>
/// The HTTP request to PagerDuty's Events API v2 failed (unreachable, DNS, TLS, timeout, etc.)
/// </summary>
public class NetworkException: PagerDutyException {

    /// <inheritdoc />
    public override bool RetryAllowedAfterDelay => true;

    /// <summary>
    /// Error while trying to communicate with PagerDuty servers.
    /// </summary>
    public NetworkException(string message, Exception? cause): base(message, cause) { }

}

/// <summary>
/// <para>Any unsuccessful HTTP response from PagerDuty's API.</para>
/// <para>There are also specialized subclasses for 400 (<see cref="BadRequest"/>), 429 (<see cref="RateLimited"/>), and 5xx (<see cref="InternalServerError"/>) responses.</para>
/// </summary>
public class WebApplicationException: PagerDutyException {

    // ReSharper disable once MemberCanBePrivate.Global - read by library consumers
    /// <summary>
    /// The HTTP status code of the API response, such as <c>400</c>.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// The plaintext content of the response body, if one was supplied.
    /// </summary>
    public string? Response { get; set; }

    /// <inheritdoc />
    public override bool RetryAllowedAfterDelay => true;

    internal WebApplicationException(int statusCode, string message): base(message, null) {
        StatusCode = statusCode;
    }

}

/// <summary>
/// <para>Check that the JSON is valid.</para>
/// </summary>
public class BadRequest: WebApplicationException {

    /// <inheritdoc />
    public override bool RetryAllowedAfterDelay => false;

    /// <summary>
    /// <para>Check that the JSON is valid.</para>
    /// </summary>
    public BadRequest(string message): base(400, message) { }

}

/// <summary>
/// <para>Too many API calls at a time.</para>
/// <para>There is a limit on the number of events that a service can accept at any given time. Depending on the behaviour of the incoming traffic and how many incidents are being created at once, we dynamically adjust our throttle limits.</para>
/// <para>If each of the events your monitoring system is sending is important, be sure to retry on a 429 response code, preferably with a backoff of a few minutes.</para>
/// </summary>
public class RateLimited: WebApplicationException {

    /// <inheritdoc />
    public override bool RetryAllowedAfterDelay => true;

    /// <summary>
    /// <para>Too many API calls at a time.</para>
    /// <para>There is a limit on the number of events that a service can accept at any given time. Depending on the behaviour of the incoming traffic and how many incidents are being created at once, we dynamically adjust our throttle limits.</para>
    /// <para>If each of the events your monitoring system is sending is important, be sure to retry on a 429 response code, preferably with a backoff of a few minutes.</para>
    /// </summary>
    public RateLimited(string message): base(429, message) { }

}

/// <summary>
/// The PagerDuty server experienced an error while processing the event.
/// </summary>
public class InternalServerError: WebApplicationException {

    /// <inheritdoc />
    public override bool RetryAllowedAfterDelay => true;

    /// <summary>
    /// The PagerDuty server experienced an error while processing the event.
    /// </summary>
    public InternalServerError(int statusCode, string message): base(statusCode, message) { }

}