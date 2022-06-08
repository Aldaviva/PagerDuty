using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Pager.Duty;

internal class JsonContent: HttpContent {

    private const string ContentType = "application/json";

    public JsonSerializer JsonSerializer { get; set; } = JsonSerializer.Create();
    public Encoding Encoding { get; set; } = new UTF8Encoding(false);

    private readonly object? _value;

    public JsonContent(object? value) {
        _value = value;

        Headers.ContentType = new MediaTypeHeaderValue(ContentType) { CharSet = Encoding.WebName };
    }

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) {
        using TextWriter textWriter = new StreamWriter(stream, Encoding, 1024, true);
        JsonSerializer.Serialize(textWriter, _value);
        return Task.CompletedTask;
    }

    protected override bool TryComputeLength(out long length) {
        length = -1;
        return false;
    }

}