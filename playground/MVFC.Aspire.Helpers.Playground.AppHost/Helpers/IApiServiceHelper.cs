namespace MVFC.Aspire.Helpers.Playground.AppHost.Helpers; 

internal interface IApiServiceHelper {

    [Post("/api/payment-callback")]
    Task<ApiResponse<string>> PaymentCallbackAsync([Body] PaymentPayload request);
}