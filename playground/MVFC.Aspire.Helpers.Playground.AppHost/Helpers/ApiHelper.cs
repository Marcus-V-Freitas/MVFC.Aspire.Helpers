namespace MVFC.Aspire.Helpers.Playground.AppHost.Helpers;

public sealed class ApiHelper(int port) {
    private readonly string _url = $"http://localhost:{port}";

    public async Task SendPayloadAsync(PaymentPayload payload) =>
        await RestService.For<IApiServiceHelper>(_url).PaymentCallbackAsync(payload);
}