namespace MVFC.Aspire.Helpers.Api.Endpoints;

public static class PubSubEndpoints
{
    public static void MapPubSubEndpoints(this IEndpointRouteBuilder apiGroup)
    {
        apiGroup.MapGet("/pub-sub-enter", (IMessagePublisher messagePublisher) =>
        {
            messagePublisher.PublishAsync("test-topic", "ola mundo");

            return Results.Ok();
        });

        apiGroup.MapPost("/pub-sub-exit", async (HttpRequest request) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();
            return Results.Ok(body);
        });
    }
}