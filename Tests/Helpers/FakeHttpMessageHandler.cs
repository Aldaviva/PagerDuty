using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.Helpers;

public abstract class FakeHttpMessageHandler: HttpMessageHandler {

    public const string RequestBodyStream = "RequestBodyStream";

// ReSharper disable once MethodSupportsCancellation - overload does not exist in .NET Framework 4.5.2 build
#pragma warning disable CS0618, CA2016
    protected sealed override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
        if (request.Content != null) {
            // the request body is disposed after sending the request, before we can assert any of its properties in the test, so make a copy of the body
            MemoryStream requestBodyCopy = new();
            request.Content.CopyToAsync(requestBodyCopy);
            request.Properties[RequestBodyStream] = requestBodyCopy;
            requestBodyCopy.Seek(0, SeekOrigin.Begin);
        }

        return SendAsync(request);
    }
#pragma warning restore CA2016, CS0618

    public abstract Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);

}