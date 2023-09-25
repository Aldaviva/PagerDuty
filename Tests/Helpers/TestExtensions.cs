using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tests.Helpers;

public static class TestExtensions {

    public static HttpRequestMessage Matches(this IArgumentConstraintManager<HttpRequestMessage> manager, HttpMethod expectedVerb, string expectedUri, string expectedJsonBody) {
        return manager.Matches(IsMatch(expectedVerb, expectedUri, expectedJsonBody), writer => {
            writer.Write(expectedVerb);
            writer.Write(" ");
            writer.Write(expectedUri);
            writer.Write("\n");
            writer.Write(expectedJsonBody);
        });
    }

    public static Func<HttpRequestMessage, bool> IsMatch(HttpMethod expectedMethod, string expectedUri, string expectedJsonBody) {
        return actual => actual.Method == expectedMethod &&
            actual.RequestUri!.ToString() == expectedUri &&
            JToken.DeepEquals(JToken.ReadFrom(new JsonTextReader(new StringReader(actual.Content!.ReadAsStringAsync().GetAwaiter().GetResult()))), JObject.Parse(expectedJsonBody));
    }

}