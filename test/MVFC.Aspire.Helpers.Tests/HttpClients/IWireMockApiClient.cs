namespace MVFC.Aspire.Helpers.Tests.HttpClients;

internal interface IWireMockApiClient
{
    [Get("/api/test")]
    internal Task<ApiResponse<string>> GetTestAsync();

    [Post("/api/echo")]
    [Headers("Content-Type: text/plain")]
    internal Task<ApiResponse<string>> PostEchoAsync([Body] string body);

    [Post("/api/echo")]
    internal Task<ApiResponse<string>> PostEchoEmptyAsync();

    [Get("/api/secure")]
    internal Task<ApiResponse<string>> GetSecureAsync();

    [Get("/api/secure")]
    internal Task<ApiResponse<string>> GetSecureWithTokenAsync([Authorize("Bearer")] string token);

    [Put("/api/put")]
    [Headers("Content-Type: text/plain")]
    internal Task<ApiResponse<string>> PutEchoAsync([Body] string body);

    [Get("/api/customauth")]
    internal Task<ApiResponse<string>> GetCustomAuthAsync();

    [Get("/api/customauth")]
    [Headers("X-Test: ok")]
    internal Task<ApiResponse<string>> GetCustomAuthWithHeaderAsync();

    [Get("/api/headers")]
    internal Task<ApiResponse<string>> GetHeadersAsync();

    [Get("/api/error")]
    internal Task<ApiResponse<string>> GetErrorAsync();

    [Delete("/api/delete")]
    internal Task<ApiResponse<string>> DeleteAsync();

    [Post("/api/form")]
    internal Task<ApiResponse<string>> PostFormAsync([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> data);

    [Post("/api/form-wrong")]
    [Headers("Content-Type: application/x-www-form-urlencoded")]
    internal Task<ApiResponse<string>> PostFormWrongAsync([Body] string data);

    [Patch("/api/patch")]
    [Headers("Content-Type: text/plain")]
    internal Task<ApiResponse<string>> PatchAsync([Body] string data);

    [Post("/api/bytes")]
    [Headers("Content-Type: application/octet-stream")]
    internal Task<ApiResponse<Stream>> PostBytesAsync([Body] HttpContent data);

    [Post("/api/unsupported")]
    [Headers("Content-Type: application/unsupported")]
    internal Task<ApiResponse<string>> PostUnsupportedAsync([Body] string data);

    [Post("/api/json")]
    [Headers("Content-Type: application/json")]
    internal Task<ApiResponse<JsonModel>> PostJsonAsync([Body] JsonNode data);

    [Get("/webhook/payment")]
    internal Task<ApiResponse<string>> GetWebhookPaymentAsync();
}
