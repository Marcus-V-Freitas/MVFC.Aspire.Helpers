namespace MVFC.Aspire.Helpers.Tests.Integration.HttpClients;

internal interface IApigeeApiClient
{
    // Spike Arrest — GET /demo/spike-arrest (rate limited via SpikeArrest policy)
    [Get("/demo/spike-arrest")]
    internal Task<HttpResponseMessage> GetSpikeArrestAsync();

    // Root — GET /demo/
    [Get("/demo/")]
    internal Task<ApiResponse<string>> GetRootAsync();

    // Health — GET /demo/health
    [Get("/demo/health")]
    internal Task<ApiResponse<string>> GetHealthAsync();

    // Echo — GET /demo/echo
    [Get("/demo/echo")]
    internal Task<ApiResponse<string>> GetEchoAsync();

    // Transform — GET /demo/transform (JS envelope)
    [Get("/demo/transform")]
    internal Task<ApiResponse<string>> GetTransformAsync();

    // Quota Test — GET /demo/quota-test (rate limited: 5/min)
    [Get("/demo/quota-test")]
    internal Task<ApiResponse<string>> GetQuotaTestAsync();

    // Info — GET /demo/info
    [Get("/demo/info")]
    internal Task<ApiResponse<string>> GetInfoAsync();

    // Cached — GET /demo/cached (response cached 30s)
    [Get("/demo/cached")]
    internal Task<ApiResponse<string>> GetCachedAsync();

    // Admin — GET /demo/admin (IP restricted)
    [Get("/demo/admin")]
    internal Task<ApiResponse<string>> GetAdminAsync();

    // Secure — GET /demo/secure (Basic Auth required)
    [Get("/demo/secure")]
    internal Task<ApiResponse<string>> GetSecureAsync();

    // Secure with Basic Auth header
    [Get("/demo/secure")]
    internal Task<ApiResponse<string>> GetSecureWithAuthAsync([Header("Authorization")] string authorization);

    // XML to JSON — GET /demo/xml
    [Get("/demo/xml")]
    internal Task<ApiResponse<string>> GetXmlAsync();

    // Not Found — GET /demo/notfound
    [Get("/demo/notfound")]
    internal Task<ApiResponse<string>> GetNotFoundAsync();

    // Health Check Aggregation — GET /demo/health-check
    [Get("/demo/health-check")]
    internal Task<ApiResponse<string>> GetHealthCheckAsync();

    // DELETE (blocked method) — DELETE /demo/
    [Delete("/demo/")]
    internal Task<ApiResponse<string>> DeleteRootAsync();

    // PUT (blocked method) — PUT /demo/
    [Put("/demo/")]
    [Headers("Content-Type: text/plain")]
    internal Task<ApiResponse<string>> PutRootAsync([Body] string body);

    // PATCH (blocked method) — PATCH /demo/
    [Patch("/demo/")]
    [Headers("Content-Type: text/plain")]
    internal Task<ApiResponse<string>> PatchRootAsync([Body] string body);
}
