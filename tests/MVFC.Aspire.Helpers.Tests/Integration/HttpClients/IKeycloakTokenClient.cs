namespace MVFC.Aspire.Helpers.Tests.Integration.HttpClients; 

internal interface IKeycloakTokenClient
{
    [Post("/realms/{realm}/protocol/openid-connect/token")]
    internal Task<ApiResponse<JsonNode>> GetTokenAsync(string realm, [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> requestData);
}
