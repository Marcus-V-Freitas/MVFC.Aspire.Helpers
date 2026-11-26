namespace MVFC.Aspire.Helpers.Tests.Unit.Extensions;

/// <summary>
/// Supplies controlled HTTP outcomes without opening a network connection.
/// </summary>
internal sealed class TestHttpMessageHandler(Func<CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
{
    /// <summary>
    /// Executes the response configured by the test.
    /// </summary>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        send(cancellationToken);
}
