namespace MVFC.Aspire.Helpers.Playground.Api.Endpoints;

public static class KeycloakEndpoints
{
    public static void MapKeycloakEndpoints(this IEndpointRouteBuilder apiGroup) =>
        apiGroup.MapGet("/key-cloak", (ClaimsPrincipal user) => new
        {
            Username = user.FindFirstValue("preferred_username"),
            Roles    = user.FindAll(ClaimTypes.Role).Select(c => c.Value),
        }).RequireAuthorization();
}
