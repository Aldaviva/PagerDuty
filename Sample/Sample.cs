using Pager.Duty;

using IPagerDuty pagerDuty = new PagerDuty(integrationKey: "77b876d00b6c4e0dc0fadc3062f8c1a6");

try {
    await pagerDuty.Send(new Change("Starting a load of laundry") {
        Source    = Environment.MachineName,
        Timestamp = DateTimeOffset.Now,
        CustomDetails = new {
            A = 1,
            B = "2"
        },
        Links = { new Link("https://aldaviva.com/", "Aldaviva") }
    });

    TriggerAlert triggerAlert = new(Severity.Info, "Washing machine finished a load of laundry") {
        Class     = "laundry",
        Component = "washing-machine-00",
        Group     = "garage-00",
        Source    = Environment.MachineName,
        Timestamp = DateTimeOffset.Now,
        CustomDetails = new {
            A = 1,
            B = "2"
        },
        Links    = { new Link("https://aldaviva.com/", "Aldaviva") },
        Images   = { new Image("https://aldaviva.com/avatars/ben.jpg", "https://aldaviva.com/", "Alt text") },
        DedupKey = null
    };
    AlertResponse alertResponse = await pagerDuty.Send(
        triggerAlert);

    await pagerDuty.Send(new AcknowledgeAlert(alertResponse.DedupKey));
    await pagerDuty.Send(new ResolveAlert(alertResponse.DedupKey));

} catch (PagerDutyException e) when (e.RetryAllowedAfterDelay) {
    //wait and try again
} catch (BadRequest e) {
    Console.WriteLine($"{e.StatusCode} Bad Request from PagerDuty: {e.Response}");
}