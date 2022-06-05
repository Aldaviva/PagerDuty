using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global - meant to be used by consumers if this class is reused in another project
// ReSharper disable MemberCanBePrivate.Global - meant to be used by consumers if this class is reused in another project

namespace Pager.Duty;

internal class JsonContent: HttpContent {

    private readonly object? _value;

    public JsonSerializer? JsonSerializer { get; set; }
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    public JsonContent(object? value) {
        _value = value;
    }

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) {
        using TextWriter textWriter = new StreamWriter(stream, Encoding, 1024, true);
        (JsonSerializer ?? JsonSerializer.Create()).Serialize(textWriter, _value);
        return Task.CompletedTask;
    }

    protected override bool TryComputeLength(out long length) {
        length = -1;
        return false;
    }

}