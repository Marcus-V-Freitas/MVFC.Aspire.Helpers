namespace MVFC.Aspire.Helpers.Playground.Api.Endpoints;

public static class SpannerEndpoints
{
    public static void MapSpannerEndpoints(this IEndpointRouteBuilder apiGroup)
    {
        apiGroup.MapGet("/spanner", async (ISpannerService service) =>
        {
            var result = await service.PingAsync().ConfigureAwait(false);

            return Results.Ok(new { ping = result, status = "Spanner emulador OK" });
        });

        apiGroup.MapPost("/spanner/users", async (SpannerUserRequest request, ISpannerService service) =>
        {
            await service.CreateUserAsync(Guid.NewGuid(), request.Name).ConfigureAwait(false);

            return Results.Created("/api/spanner/users", new { request.Name });
        });

        apiGroup.MapGet("/spanner/users", async (ISpannerService service) =>
        {
            var users = service.GetAllUsers().ConfigureAwait(false);

            return Results.Ok(users);
        });
    }
}
