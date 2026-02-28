namespace MVFC.Aspire.Helpers.Tests.TestUtils; 

internal static class WireMockTestExtensions {
    
    public static async ValueTask<HttpClient> CreateWireMockHttpClient(this ProjectAppHost app, string resourceName) 
    {
        var uri = await app.GetConnectionString(resourceName);

        return new HttpClient() {
            BaseAddress = new(uri!),
        };
    }
}
