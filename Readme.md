<img src="https://raw.githubusercontent.com/Aldaviva/PagerDuty/master/PagerDuty/icon.png" height="23" alt="PagerDuty logo" /> PagerDuty
===

[![Nuget](https://img.shields.io/nuget/v/PagerDuty?logo=nuget)](https://www.nuget.org/packages/PagerDuty/) [![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/Aldaviva/PagerDuty/dotnetpackage.yml?branch=master&logo=github)](https://github.com/Aldaviva/PagerDuty/actions/workflows/dotnetpackage.yml) [![Coveralls](https://img.shields.io/coveralls/github/Aldaviva/PagerDuty?logo=coveralls)](https://coveralls.io/github/Aldaviva/PagerDuty?branch=master)

*Trigger, acknowledge, and resolve [Alerts](https://support.pagerduty.com/docs/alerts) and create [Changes](https://support.pagerduty.com/docs/change-events) using the [PagerDuty Events API V2](https://developer.pagerduty.com/docs/events-api-v2/overview/).*

## Prerequisites

- [PagerDuty account](https://www.pagerduty.com/sign-up/) (the [free plan](https://www.pagerduty.com/sign-up-free/?type=free) is sufficient)
- Any Microsoft .NET runtime that supports [.NET Standard 2.0 or later](https://docs.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0#net-standard-versions)
    - [.NET 5.0 or later](https://dotnet.microsoft.com/en-us/download/dotnet)
    - [.NET Core 2.0 or later](https://dotnet.microsoft.com/en-us/download/dotnet)
    - [.NET Framework 4.6.1 or later](https://dotnet.microsoft.com/en-us/download/dotnet-framework)

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
1. Construct a new **`PagerDuty`** API client instance in your project, passing your **Integration Key** as a constructor parameter.
    ```cs
    IPagerDuty pagerDuty = new PagerDuty(integrationKey: "dfca74ebb7450b3e6da3ba6083a323f4");
    ```

`IPagerDuty` instances can be reused to send multiple events over the lifetime of your application. You can add one to your dependency injection context and retain it for as long as you like.

### HTTP settings
If you need to customize any of the settings for the HTTP connection to PagerDuty's API servers, you may optionally provide a custom [`HttpClient`](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient) instance to the `IPagerDuty` object. This allows you to set a proxy server, TLS settings, concurrent connection count, [DNS TTL](https://docs.microsoft.com/en-us/dotnet/fundamentals/networking/httpclient-guidelines#dns-behavior), and other properties. If you don't set this property, a default `HttpClient` instance is used instead.

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

## Usage

### Triggering an Alert

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

The value of the required `DedupKey` constructor parameter comes from an `AlertResponse`, which is returned when you send a `TriggerAlert`.

```cs
await pagerDuty.Send(new AcknowledgeAlert(alertResponse.DedupKey));
```

### Resolving an Alert

The value of the required `DedupKey` constructor parameter comes from an `AlertResponse`, which is returned when you send a `TriggerAlert`.

```cs
await pagerDuty.Send(new ResolveAlert(alertResponse.DedupKey));
```

### Creating a Change

You can also create Changes.

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

`PagerDuty` contains an `HttpClient` instance, so when you're done with the instance, call `Dispose()` to clean it up and allow the `HttpClient` to be garbage collected.

```cs
pagerDuty.Dispose();
```

## References

- [PagerDuty Knowledge Base](https://support.pagerduty.com/docs/alerts)
- [PagerDuty Events API V2 Documentation](https://developer.pagerduty.com/docs/events-api-v2/overview/)
- [PagerDuty Events API V2 API Reference](https://developer.pagerduty.com/api-reference/YXBpOjI3NDgyNjU-pager-duty-v2-events-api)
