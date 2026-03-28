namespace MVFC.Aspire.Helpers.Tests.Unit.Fakers;

internal sealed class FakeStatusHandler(HttpStatusCode status, string body = "{}") : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage _, CancellationToken __)
        => Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(body) });
}
