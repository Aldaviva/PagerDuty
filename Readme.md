PagerDuty
===

*Client that sends Alert and Change events to the [PagerDuty](https://www.pagerduty.com/home/) [Events API V2](https://developer.pagerduty.com/docs/events-api-v2/overview/)*

[![Nuget](https://img.shields.io/nuget/v/PagerDuty?logo=nuget)](https://www.nuget.org/packages/PagerDuty/) [![GitHub Workflow Status](https://img.shields.io/github/workflow/status/Aldaviva/PagerDuty/.NET?logo=github)](https://github.com/Aldaviva/PagerDuty/actions/workflows/dotnetpackage.yml) [![Coveralls](https://img.shields.io/coveralls/github/Aldaviva/PagerDuty?logo=coveralls)](https://coveralls.io/github/Aldaviva/PagerDuty?branch=master)

<!-- MarkdownTOC autolink="true" bracket="round" autoanchor="true" levels="1,2,3" style="ordered" -->

1. [Prerequisites](#prerequisites)
1. [Installation](#installation)
1. [Usage](#usage)
    1. [Configuration](#configuration)
    1. [Triggering an Alert](#triggering-an-alert)
    1. [Acknowledging an Alert](#acknowledging-an-alert)
    1. [Resolving an Alert](#resolving-an-alert)
    1. [Creating a Change](#creating-a-change)
    1. [Handling exceptions](#handling-exceptions)
    1. [Cleaning up](#cleaning-up)
1. [References](#references)

<!-- /MarkdownTOC -->

<a id="prerequisites"></a>
## Prerequisites

- [PagerDuty account](https://www.pagerduty.com/sign-up/) (the [free plan](https://www.pagerduty.com/sign-up-free/?type=free) is sufficient)
- Any Microsoft .NET runtime that supports [.NET Standard 2.0 or later](https://docs.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0#net-standard-versions)
    - [.NET 5.0 or later](https://dotnet.microsoft.com/en-us/download/dotnet)
    - [.NET Core 2.0 or later](https://dotnet.microsoft.com/en-us/download/dotnet)
    - [.NET Framework 4.6.1 or later](https://dotnet.microsoft.com/en-us/download/dotnet-framework)

<a id="installation"></a>
## Installation

You can install this library into your project from [NuGet Gallery](https://www.nuget.org/packages/PagerDuty) using any of the following techniques.
- `dotnet add package PagerDuty`
- `Install-Package PagerDuty`
- Project › Manage NuGet Packages… in Visual Studio

<a id="usage"></a>
## Usage

<a id="configuration"></a>
### Configuration

1. Create an **Integration** in PagerDuty and get its **Integration Key**.
    1. Sign into your [PagerDuty account](https://app.pagerduty.com/).
    1. Go to **Services › Service Directory**.
    1. Select an existing Service for which you want to publish events, or create a new Service.
    1. In the **Integrations** tab of the Service, add a new **Integration**.
    1. Under **Most popular integrations**, select **Events API V2**, then click **Add**.
    1. Expand the newly-created Integration and copy its **Integration Key**, which will be used to authorize this library to send Events to the correct Service.
    1. Optionally, you can also rename the Integration to make it more recognizable.
1. Construct a new **`PagerDuty`** API client instance in your project, passing your **Integration Key** as a constructor parameter.
    ```cs
    IPagerDuty pagerDuty = new PagerDuty(integrationKey: "dfca74ebb7450b3e6da3ba6083a323f4");
    ```

<a id="triggering-an-alert"></a>
### Triggering an Alert

Construct a new **`TriggerAlert`** instance with the required parameters, and pass it to the `IPagerDuty.Send(Alert)` method. This returns an `AlertResponse`.

```cs
AlertResponse alertResponse = await pagerDuty.Send(new TriggerAlert(Severity.Error, "Summary"));
```

You can specify optional parameters with an object initializer or assignments.

```cs
var trigger = new TriggerAlert(Severity.Warning, "Summary of warning") {
    Class = "my class",
    Component = "my component",
    Group = "my group",
    Links = { new Link("https://aldaviva.com/", "Aldaviva") },
    Images = { new Image("https://aldaviva.com/avatars/ben.jpg", "https://aldaviva.com/", "Ben Hutchison") },
    CustomDetails = new {
        A = 1,
        B = "2"
    }
};
trigger.Source = "my source";
trigger.Timestamp = DateTimeOffset.Now;
```

If a key in your `CustomDetails` object isn't a valid identifier in C#, for example if it contains spaces, you can also use an `IDictionary<string, object>`, or any other type that can be serialized into JSON.

```cs
trigger.CustomDetails = new Dictionary<string, object> {
    { "key 1", "value 1" },
    { "key 2", "value 2" },
};
```

<a id="acknowledging-an-alert"></a>
### Acknowledging an Alert

This is the same as triggering an Alert, except you construct an `AcknowledgeAlert` and set its `DedupKey` to the value in the `AlertResponse` from the prior `TriggerAlert`, which identifies the specific Alert to acknowledge.

```cs
await pagerDuty.Send(new AcknowledgeAlert(alertResponse.DedupKey));
```

<a id="resolving-an-alert"></a>
### Resolving an Alert

This is the same as acknowledging an Alert, but with `ResolveAlert`.

```cs
await pagerDuty.Send(new ResolveAlert(alertResponse.DedupKey));
```

<a id="creating-a-change"></a>
### Creating a Change

You can also create Changes in addition to Alerts.

```cs
await pagerDuty.Send(new Change("Summary of Change"));
```

<a id="handling-exceptions"></a>
### Handling exceptions

All of the exceptions thrown by `IPagerDuty.Send` inherit from `PagerDutyAlert`, so you can catch that superclass, or the more specialized subclasses: `NetworkException`, `BadRequest`, `RateLimited`, and `InternalServerError`.

```cs
try {
    await pagerDuty.Send(new Change("Summary of Change"));
} catch (PagerDutyException e) when (e.RetryAllowedAfterDelay) {
    // try again later
} catch (BadRequest e) {
    Console.WriteLine($"{e.Message} {e.StatusCode} {e.Response}");
} catch (WebApplicationException) {
    // catch-all for unexpected status codes
}
```

<a id="cleaning-up"></a>
### Cleaning up

`PagerDuty` contains an `HttpClient`, so when you're done with the instance, call `Dispose()` to clean it up and allow the `HttpClient` to be garbage collected.

```cs
pagerDuty.Dispose();
```

<a id="references"></a>
## References

- [PagerDuty Events API V2 Documentation](https://developer.pagerduty.com/docs/events-api-v2/overview/)
- [PagerDuty Events API V2 API Reference](https://developer.pagerduty.com/api-reference/YXBpOjI3NDgyNjU-pager-duty-v2-events-api)