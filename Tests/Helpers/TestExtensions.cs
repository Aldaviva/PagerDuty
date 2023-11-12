using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Tests.Helpers;

public static class TestExtensions {

    public static HttpRequestMessage Matches(this IArgumentConstraintManager<HttpRequestMessage> manager,
                                             HttpMethod                                          expectedVerb,
                                             string                                              expectedUri,
                                             [LanguageInjection(InjectedLanguage.JSON)]
                                             string? expectedJsonBody) {
        return manager.Matches(IsMatch(expectedVerb, expectedUri, expectedJsonBody), writer => {
            writer.Write(expectedVerb);
            writer.Write(" ");
            writer.Write(expectedUri);
            if (expectedJsonBody != null) {
                writer.Write("\n");
                writer.Write(expectedJsonBody);
            }
        });
    }

#pragma warning disable CS0618 // Type or member is obsolete - Options does not exist in .NET Framework 4.5.2
    public static Func<HttpRequestMessage, bool> IsMatch(HttpMethod expectedMethod, string expectedUri, [LanguageInjection(InjectedLanguage.JSON)] string? expectedJsonBody) {
        return actual => actual.Method == expectedMethod &&
            actual.RequestUri!.ToString() == expectedUri &&
            (actual.Properties.TryGetValue(FakeHttpMessageHandler.RequestBodyStream, out object? b) && b is Stream actualBody
                ? expectedJsonBody != null && JToken.DeepEquals(JToken.ReadFrom(new JsonTextReader(new StreamReader(actualBody, Encoding.UTF8))), JObject.Parse(expectedJsonBody))
                : expectedJsonBody == null);
    }
#pragma warning restore CS0618 // Type or member is obsolete

}