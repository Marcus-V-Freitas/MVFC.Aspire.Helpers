namespace MVFC.Aspire.Helpers.Playground.AppHost.Helpers;

internal interface IApiServiceHelper 
{
    [Post("/api/payment-callback")]
    public Task<ApiResponse<string>> PaymentCallbackAsync([Body] PaymentPayload request);
}