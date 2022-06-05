namespace Tests.Helpers;

public abstract class FakeHttpMessageHandler: HttpMessageHandler {

    protected sealed override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
        return SendAsync(request);
    }

    public abstract Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);

}