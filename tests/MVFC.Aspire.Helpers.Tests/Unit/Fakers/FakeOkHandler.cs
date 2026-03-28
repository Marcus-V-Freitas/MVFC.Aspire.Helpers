namespace MVFC.Aspire.Helpers.Tests.Unit.Fakers;

internal sealed class FakeOkHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage _, CancellationToken __)
        => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });
}
