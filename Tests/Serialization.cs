using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pager.Duty;

namespace Tests;

public class Serialization {

    private readonly JsonSerializerSettings _jsonSettings = PagerDuty.JsonSerializerSettings;
    private readonly JsonSerializer         _jsonSerializer;

    public Serialization() {
        _jsonSerializer = JsonSerializer.Create(_jsonSettings);
    }

    [Fact]
    public void SerializeAcknowledgeRequest() {
        AcknowledgeAlert @event = new("dedupkeyhere") { RoutingKey = "routingkeyhere" };

        AssertEquivalentJson(@event, @"{
  ""routing_key"": ""routingkeyhere"",
  ""dedup_key"": ""dedupkeyhere"",
  ""event_action"": ""acknowledge""
}");
    }

    [Fact]
    public void SerializeResolveRequest() {
        ResolveAlert @event = new("dedupkeyhere") { RoutingKey = "routingkeyhere" };

        AssertEquivalentJson(@event, @"{
  ""routing_key"": ""routingkeyhere"",
  ""dedup_key"": ""dedupkeyhere"",
  ""event_action"": ""resolve""
}");
    }

    [Fact]
    public void SerializeTriggerRequest() {
        TriggerAlert @event = new(Severity.Info, "Example alert on host1.example.com") {
            RoutingKey = "samplekeyhere",
            Class      = "deploy",
            Component  = "postgres",
            CustomDetails = new Dictionary<string, object> {
                { "ping time", "1500ms" },
                { "load avg", 0.75 }
            },
            Images    = { new Image("https://www.pagerduty.com/wp-content/uploads/2016/05/pagerduty-logo-green.png", "https://example.com/", "Example text") },
            Timestamp = new DateTimeOffset(2015, 7, 17, 8, 42, 58, 315, TimeSpan.Zero),
            Group     = "prod-datapipe",
            Links     = { new Link("https://example.com/", "Link text") },
            Source    = "monitoringtool:cloudvendor:central-region-dc-01:852559987:cluster/api-stats-prod-003",
            DedupKey  = "samplekeyhere"
        };

        const string expected = @"{
  ""payload"": {
    ""summary"": ""Example alert on host1.example.com"",
    ""timestamp"": ""2015-07-17T08:42:58.315+00:00"",
    ""source"": ""monitoringtool:cloudvendor:central-region-dc-01:852559987:cluster/api-stats-prod-003"",
    ""severity"": ""info"",
    ""component"": ""postgres"",
    ""group"": ""prod-datapipe"",
    ""class"": ""deploy"",
    ""custom_details"": {
      ""ping time"": ""1500ms"",
      ""load avg"": 0.75
    }
  },
  ""routing_key"": ""samplekeyhere"",
  ""dedup_key"": ""samplekeyhere"",
  ""images"": [
    {
      ""src"": ""https://www.pagerduty.com/wp-content/uploads/2016/05/pagerduty-logo-green.png"",
      ""href"": ""https://example.com/"",
      ""alt"": ""Example text""
    }
  ],
  ""links"": [
    {
      ""href"": ""https://example.com/"",
      ""text"": ""Link text""
    }
  ],
  ""event_action"": ""trigger"",
  ""client"": ""Aldaviva/PagerDuty"",
  ""client_url"": ""https://github.com/Aldaviva/PagerDuty""
}";

        AssertEquivalentJson(@event, expected);
    }

    [Fact]
    public void SerializeChangeRequest() {
        Change @event = new("Build Success: Increase snapshot create timeout to 30 seconds") {
            Timestamp = new DateTimeOffset(2020, 7, 17, 8, 42, 58, 315, TimeSpan.Zero),
            CustomDetails = new {
                build_state  = "passed",
                build_number = "2",
                run_time     = "1236s"
            },
            Links      = { new Link("https://acme.pagerduty.dev/build/2", "View more details in Acme!") },
            Images     = { new Image("https://www.pagerduty.com/wp-content/uploads/2016/05/pagerduty-logo-green.png", "https://example.com/", "Example text") },
            RoutingKey = "samplekeyhere",
            Source     = "acme-build-pipeline-tool-default-i-9999"
        };

        /*
         * Documentation shows example timestamp as 2020-07-17T08:42:58.315+0000, but Json.NET serializes 2020-07-17T08:42:58.315+00:00 by default (colon in time zone offset).
         * Other docs shows "Z" UTC suffix.
         * From testing, the API does successfully parse the +00:00 format, and mostly likely handles all ISO-8601 date+time formats correctly, so I'm not going to try to match the +0000 style.
         */
        const string expected = @"{
  ""links"": [
    {
      ""href"": ""https://acme.pagerduty.dev/build/2"",
      ""text"": ""View more details in Acme!""
    }
  ],
  ""images"": [
    {
      ""src"": ""https://www.pagerduty.com/wp-content/uploads/2016/05/pagerduty-logo-green.png"",
      ""href"": ""https://example.com/"",
      ""alt"": ""Example text""
    }
  ],
  ""payload"": {
    ""summary"": ""Build Success: Increase snapshot create timeout to 30 seconds"",
    ""source"": ""acme-build-pipeline-tool-default-i-9999"",
    ""timestamp"": ""2020-07-17T08:42:58.315+00:00"",
    ""custom_details"": {
      ""build_state"": ""passed"",
      ""build_number"": ""2"",
      ""run_time"": ""1236s""
    }
  },
  ""routing_key"": ""samplekeyhere""
}";

        AssertEquivalentJson(@event, expected);
    }

    private void AssertEquivalentJson(object actual, string expected) {
        JObject actualJson   = JObject.FromObject(actual, _jsonSerializer);
        JToken  expectedJson = JToken.Parse(expected);
        JToken.DeepEquals(actualJson, expectedJson)
            .Should().BeTrue("actual {0} should be equivalent to expected {1}", actualJson, expectedJson);
    }

}