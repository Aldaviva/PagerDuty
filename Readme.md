📟 PagerDuty
===

[![Nuget](https://img.shields.io/nuget/v/PagerDuty?logo=nuget&color=blue)](https://www.nuget.org/packages/PagerDuty/) [![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/Aldaviva/PagerDuty/dotnetpackage.yml?branch=master&logo=github)](https://github.com/Aldaviva/PagerDuty/actions/workflows/dotnetpackage.yml) [![Testspace](https://img.shields.io/testspace/tests/Aldaviva/Aldaviva:PagerDuty/master?passed_label=passing&failed_label=failing&logo=data%3Aimage%2Fsvg%2Bxml%3Bbase64%2CPHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA4NTkgODYxIj48cGF0aCBkPSJtNTk4IDUxMy05NCA5NCAyOCAyNyA5NC05NC0yOC0yN3pNMzA2IDIyNmwtOTQgOTQgMjggMjggOTQtOTQtMjgtMjh6bS00NiAyODctMjcgMjcgOTQgOTQgMjctMjctOTQtOTR6bTI5My0yODctMjcgMjggOTQgOTQgMjctMjgtOTQtOTR6TTQzMiA4NjFjNDEuMzMgMCA3Ni44My0xNC42NyAxMDYuNS00NFM1ODMgNzUyIDU4MyA3MTBjMC00MS4zMy0xNC44My03Ni44My00NC41LTEwNi41UzQ3My4zMyA1NTkgNDMyIDU1OWMtNDIgMC03Ny42NyAxNC44My0xMDcgNDQuNXMtNDQgNjUuMTctNDQgMTA2LjVjMCA0MiAxNC42NyA3Ny42NyA0NCAxMDdzNjUgNDQgMTA3IDQ0em0wLTU1OWM0MS4zMyAwIDc2LjgzLTE0LjgzIDEwNi41LTQ0LjVTNTgzIDE5Mi4zMyA1ODMgMTUxYzAtNDItMTQuODMtNzcuNjctNDQuNS0xMDdTNDczLjMzIDAgNDMyIDBjLTQyIDAtNzcuNjcgMTQuNjctMTA3IDQ0cy00NCA2NS00NCAxMDdjMCA0MS4zMyAxNC42NyA3Ni44MyA0NCAxMDYuNVMzOTAgMzAyIDQzMiAzMDJ6bTI3NiAyODJjNDIgMCA3Ny42Ny0xNC44MyAxMDctNDQuNXM0NC02NS4xNyA0NC0xMDYuNWMwLTQyLTE0LjY3LTc3LjY3LTQ0LTEwN3MtNjUtNDQtMTA3LTQ0Yy00MS4zMyAwLTc2LjY3IDE0LjY3LTEwNiA0NHMtNDQgNjUtNDQgMTA3YzAgNDEuMzMgMTQuNjcgNzYuODMgNDQgMTA2LjVTNjY2LjY3IDU4NCA3MDggNTg0em0tNTU3IDBjNDIgMCA3Ny42Ny0xNC44MyAxMDctNDQuNXM0NC02NS4xNyA0NC0xMDYuNWMwLTQyLTE0LjY3LTc3LjY3LTQ0LTEwN3MtNjUtNDQtMTA3LTQ0Yy00MS4zMyAwLTc2LjgzIDE0LjY3LTEwNi41IDQ0UzAgMzkxIDAgNDMzYzAgNDEuMzMgMTQuODMgNzYuODMgNDQuNSAxMDYuNVMxMDkuNjcgNTg0IDE1MSA1ODR6IiBmaWxsPSIjZmZmIi8%2BPC9zdmc%2B)](https://aldaviva.testspace.com/spaces/247338) [![Coveralls](https://img.shields.io/coveralls/github/Aldaviva/PagerDuty?logo=coveralls)](https://coveralls.io/github/Aldaviva/PagerDuty?branch=master)

*Trigger, acknowledge, and resolve [Alerts](https://support.pagerduty.com/docs/alerts) and create [Changes](https://support.pagerduty.com/docs/change-events) using the [PagerDuty Events API V2](https://developer.pagerduty.com/docs/events-api-v2/overview/).*

<!-- MarkdownTOC autolink="true" bracket="round" levels="1,2,3" bullets="1.,-" -->

1. [Quick Start](#quick-start)
1. [Prerequisites](#prerequisites)
1. [Installation](#installation)
1. [Configuration](#configuration)
    - [HTTP settings](#http-settings)
    - [Base URL](#base-url)
1. [Usage](#usage)
    - [Triggering an Alert](#triggering-an-alert)
    - [Acknowledging an Alert](#acknowledging-an-alert)
    - [Resolving an Alert](#resolving-an-alert)
    - [Creating a Change](#creating-a-change)
    - [Handling exceptions](#handling-exceptions)
    - [Cleaning up](#cleaning-up)
1. [Examples](#examples)
1. [References](#references)
1. [Presentation](#presentation)

<!-- /MarkdownTOC -->

## Quick Start
```cmd
dotnet add package PagerDuty
```
```cs
using Pager.Duty;

using var pagerDuty = new PagerDuty("my service's integration key");
AlertResponse alertResponse = await pagerDuty.Send(new TriggerAlert(Severity.Error, "My Alert"));
Console.WriteLine("Triggered alert, waiting 30 seconds before resolving...");

await Task.Delay(30 * 1000);
await pagerDuty.Send(new ResolveAlert(alertResponse.DedupKey));
Console.WriteLine("Resolved alert.");
```

## Prerequisites

- [PagerDuty account](https://www.pagerduty.com/sign-up/) (the [free plan](https://www.pagerduty.com/sign-up-free/?type=free) is sufficient)
- .NET runtime
    - [.NET 5.0 or later](https://dotnet.microsoft.com/en-us/download/dotnet)
    - [.NET Core 2.0 or later](https://dotnet.microsoft.com/en-us/download/dotnet)
    - [.NET Framework 4.5.2 or later](https://dotnet.microsoft.com/en-us/download/dotnet-framework)
    - Any other runtime that supports [.NET Standard 2.0 or later](https://docs.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0#net-standard-versions)

## Installation

You can install this library into your project from [NuGet Gallery](https://www.nuget.org/packages/PagerDuty):
- `dotnet add package PagerDuty`
- `Install-Package PagerDuty`
- Go to Project › Manage NuGet Packages in Visual Studio and search for `PagerDuty`

## Configuration

1. Create an **Integration** in PagerDuty and get its **Integration Key**.
    1. Sign into your [PagerDuty account](https://app.pagerduty.com/).
    1. Go to **Services › Service Directory**.
    1. Select an existing Service for which you want to publish events, or create a new Service.
    1. In the **Integrations** tab of the Service, add a new **Integration**.
    1. Under **Most popular integrations**, select **Events API V2**, then click **Add**.
    1. Expand the newly-created Integration and copy its **Integration Key**, which will be used to authorize this library to send Events to the correct Service.
1. Construct a new **`PagerDuty`** client instance in your project, passing your **Integration Key** as a constructor parameter.
    ```cs
    using Pager.Duty;

    IPagerDuty pagerDuty = new PagerDuty(integrationKey: "dfca74ebb7450b3e6da3ba6083a323f4");
    ```

`PagerDuty` instances can be reused to send multiple events to the same service over the lifetime of your application. You can add one to your dependency injection context and retain it for as long as you like. If you need to send events to multiple services, construct multiple `PagerDuty` instance objects.

### HTTP settings
If you need to customize any of the settings for the HTTP connection to PagerDuty's API servers, you may optionally provide a custom [`HttpClient`](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient) instance to the `IPagerDuty` object. This allows you to set a proxy server, TLS settings, concurrent connection count, [DNS TTL](https://docs.microsoft.com/en-us/dotnet/fundamentals/networking/httpclient-guidelines#dns-behavior), and other properties.

If you don't set this property, a default `HttpClient` instance is used instead, and will be automatically disposed of when the `PagerDuty` instance is disposed of. If you do set this property to a custom `HttpClient` instance, `PagerDuty` won't dispose of it, so that you can reuse it in multiple places.

```cs
pagerDuty.HttpClient = new HttpClient(new SocketsHttpHandler {
    UseProxy = true,
    Proxy = new WebProxy("10.0.0.2", 8443),
    MaxConnectionsPerServer = 10,
    PooledConnectionLifetime = TimeSpan.FromMinutes(2),
    SslOptions = new SslClientAuthenticationOptions {
        EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
    }
}) {
    Timeout = TimeSpan.FromSeconds(5)
};
```

### Base URL
By default, this library sends event requests to the global PagerDuty cluster, `https://events.pagerduty.com/v2/`.

You may change this by setting the `IPagerDuty.BaseUrl` property. For example, if your tenant is hosted in the European Union, you must change the base URL to the EU cluster:
```cs
pagerDuty.BaseUrl = new Uri("https://events.eu.pagerduty.com/v2/");
```

## Usage

### Triggering an Alert

This creates a new, unresolved alert for your PagerDuty service, showing that a new event has just occurred.

Construct a new **`TriggerAlert`** instance with the required `severity` and `summary` parameters, and pass it to the `IPagerDuty.Send(Alert)` method. This returns an `AlertResponse` once it's been successfully uploaded to PagerDuty.

```cs
AlertResponse alertResponse = await pagerDuty.Send(new TriggerAlert(Severity.Error, "Summary"));
```

In addition to the two required parameters, `TriggerAlert` also has several optional parameters, all of which you can specify using an object initializer or property assignments.

```cs
TriggerAlert trigger = new(Severity.Warning, "Summary of warning") {
    Class = "my class",
    Component = "my component",
    Group = "my group",
    Links = { new Link("https://aldaviva.com", "Aldaviva") },
    Images = { new Image("https://aldaviva.com/avatars/ben.jpg", "https://aldaviva.com", "Ben") },
    CustomDetails = new {
        A = 1,
        B = "2"
    }
};

trigger.Source = "my source";
trigger.Timestamp = DateTimeOffset.Now;
trigger.Client = "My Client";
trigger.ClientUrl = "https://myclient.mycompany.com";

AlertResponse alertResponse = await pagerDuty.Send(trigger);
```

If a key in your `CustomDetails` object isn't a valid identifier in C#, for example if it contains spaces, you can also use an `IDictionary<string, object>`, or any other type that can be serialized into JSON.

```cs
trigger.CustomDetails = new Dictionary<string, object> {
    { "key 1", "value 1" },
    { "key 2", "value 2" },
};
```

### Acknowledging an Alert

This moves an existing unresolved alert for your service into the acknowledged state, showing that someone is aware of it.

The value of the required `DedupKey` constructor parameter comes from an `AlertResponse`, which is returned when you send a `TriggerAlert`.

```cs
await pagerDuty.Send(new AcknowledgeAlert(alertResponse.DedupKey));
```

### Resolving an Alert

This marks an existing alert for your service as resolved, showing that the original conditions that triggered the alert are no longer present.

The value of the required `DedupKey` constructor parameter comes from an `AlertResponse`, which is returned when you send a `TriggerAlert`.

```cs
await pagerDuty.Send(new ResolveAlert(alertResponse.DedupKey));
```

### Creating a Change

This is not an alert, it's a different type of event showing that something expected changed in your service, which may be useful to know about if an alert occurs later.

```cs
await pagerDuty.Send(new Change("Summary of Change"));
```

### Handling exceptions

All of the exceptions thrown by `IPagerDuty.Send` inherit from `PagerDutyException`, so you can catch that superclass, or the more specialized subclasses: `NetworkException`, `BadRequest`, `RateLimited`, and `InternalServerError`.

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

### Cleaning up

When you're done with a `PagerDuty` instance, call `PagerDuty.Dispose()` to clean it up and allow the default `HttpClient` to be garbage collected. A custom `HttpClient` instance, [if set](#http-settings), won't be disposed, so that you can reuse it in multiple places.

```cs
pagerDuty.Dispose();
```

## Examples
- [Sample program](https://github.com/Aldaviva/PagerDuty/blob/master/Sample/Sample.cs)
- [LaundryDuty](https://github.com/Aldaviva/LaundryDuty)
- [DryerDuty](https://github.com/Aldaviva/DryerDuty)

## References

- [PagerDuty Knowledge Base](https://support.pagerduty.com/docs/alerts)
- [PagerDuty Events API V2 Documentation](https://developer.pagerduty.com/docs/events-api-v2/overview/)
- [PagerDuty Events API V2 API Reference](https://developer.pagerduty.com/api-reference/YXBpOjI3NDgyNjU-pager-duty-v2-events-api)

## Presentation

I gave a talk about this project during PagerDuty's 2024-02-09 How-To Happy Hour on their [Twitch channel](https://twitch.tv/pagerduty).

- [🎞 Video recording](https://www.youtube.com/watch?v=0Gui4cGQ2ds)
- [💡 Community spotlight](https://www.pagerduty.com/blog/community-spotlight-ben-hutchison/)

[![Video](https://i.imgur.com/acOTH8I.jpeg)](https://www.youtube.com/watch?v=0Gui4cGQ2ds)