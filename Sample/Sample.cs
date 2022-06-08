using Pager.Duty;

using IPagerDuty pagerDuty = new PagerDuty(integrationKey: "77b876d00b6c4e0dc0fadc3062f8c1a6");

try {

    await pagerDuty.Send(new TriggerAlert(Severity.Info, "Neither client nor url"));

    Console.WriteLine("Done");

} catch (PagerDutyException e) when (e.RetryAllowedAfterDelay) {
    Console.WriteLine($"Error while sending event to PagerDuty: {e.Message}");
} catch (BadRequest e) {
    Console.WriteLine($"{e.StatusCode} Bad Request from PagerDuty: {e.Response}");
}