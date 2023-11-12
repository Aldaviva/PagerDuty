// ReSharper disable UnusedMemberInSuper.Global - meant to be used by library consumers

using Pager.Duty.Exceptions;
using Pager.Duty.Requests;
using Pager.Duty.Responses;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pager.Duty;

/// <summary>
/// <para>The entry point into the PagerDuty library is <see cref="PagerDuty"/>, which implements the <see cref="IPagerDuty"/> interface.</para>
/// <para> </para>
/// <para>To get started:</para>
/// <list type="number">
/// <item><description>Sign in to PagerDuty, or create a new free account.</description></item>
/// <item><description>Create a new Service in PagerDuty.</description></item>
/// <item><description>Add a new Integration of type "Events API V2" to your Service.</description></item>
/// <item><description>Copy the automatically-generated Integration Key.</description></item>
/// <item><description>Construct a new <see cref="PagerDuty"/> instance, passing it the Integration Key.</description></item>
/// <item><description>Call the <see cref="Send(Alert)"/> method, passing a new <see cref="Alert"/> parameter.</description></item>
/// <item><description>To get the result of the request, you can await the <see cref="Task"/> and catch <see cref="PagerDutyException"/>.</description></item>
/// </list>
/// <para> </para>
/// <para>Example:</para>
/// <code> using IPagerDuty pagerDuty = new PagerDuty("dfca74ebb7450b3e6da3ba6083a323f4");
/// try {
///     AlertResponse response = await pagerDuty.Send(new TriggerAlert(Severity.Error, "Health check failing") {
///         Component = "db-prod-00",
///         Group = "db-prod"
///     });
///
///     await Task.Delay(1000);
///     await pagerDuty.Send(new ResolveAlert(response.DedupKey));
/// } catch (PagerDutyException e) {
///     Console.WriteLine(e.Message);
/// }</code>
/// </summary>
public interface IPagerDuty: IDisposable {

    /// <summary>
    /// <para>Optional <see cref="System.Net.Http.HttpClient"/> to use for sending API requests to PagerDuty. Useful to set if you need custom settings for a proxy or TLS, for example.</para>
    /// <para>If you don't set this property, a default instance will automatically be created and used instead.</para>
    /// <para>The default instance will be automatically disposed of when you call <see cref="IDisposable.Dispose"/>, but custom instances you set here will not, so that you can reuse them.</para>
    /// </summary>
    HttpClient HttpClient { get; set; }

    /// <summary>
    /// <para>Upload an Alert event to PagerDuty.</para>
    /// <para>To send follow-up events later which relate to the same incident, make sure to read the <see cref="AlertResponse.DedupKey"/> from the response. By setting the same <c>DedupKey</c> in an <see cref="AcknowledgeAlert"/> or <see cref="ResolveAlert"/> that you received from the response to a prior <see cref="TriggerAlert"/>, you can acknowledge or resolve the incident that you triggered.</para>
    /// </summary>
    /// <param name="alert">Construct a new <see cref="TriggerAlert"/>, <see cref="AcknowledgeAlert"/>, or <see cref="ResolveAlert"/> instance to send.</param>
    /// <returns>The response from PagerDuty, most notably including the <see cref="AlertResponse.DedupKey"/>, which should be passed in future events for the same incident in order to correlate them.</returns>
    /// <exception cref="NetworkException">the request to PagerDuty's Events API v2 failed without returning an HTTP response (unreachable, DNS, TLS, timeout, etc.)</exception>
    /// <exception cref="WebApplicationException">the request to PagerDuty's Events API v2 returned a non-2xx HTTP response, such as <see cref="BadRequest"/>, <see cref="RateLimited"/>, or <see cref="InternalServerError"/>, which you can inspect for the <see cref="WebApplicationException.StatusCode"/>, <see cref="WebApplicationException.Response"/>, and <see cref="WebApplicationException.RetryAllowedAfterDelay"/></exception>
    Task<AlertResponse> Send(Alert alert);

    /// <summary>
    /// <para>Upload a Change event to PagerDuty.</para>
    /// </summary>
    /// <param name="alert">Construct a new <see cref="Change"/> instance to send.</param>
    /// <returns>The response from PagerDuty, which just tells you that the event was processed successfully.</returns>
    /// <exception cref="NetworkException">the HTTP request to PagerDuty's Events API v2 failed (unreachable, DNS, TLS, timeout, etc.)</exception>
    /// <exception cref="WebApplicationException">the HTTP request to PagerDuty's Events API v2 returned a non-2xx response, such as <see cref="BadRequest"/>, <see cref="RateLimited"/>, or <see cref="InternalServerError"/></exception>
    Task<ChangeResponse> Send(Change alert);

}