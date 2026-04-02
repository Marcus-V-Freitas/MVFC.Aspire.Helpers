namespace MVFC.Aspire.Helpers.Playground.Api.Endpoints;

public static class MongoEndpoints
{
    public static void MapMongoEndpoints(this IEndpointRouteBuilder apiGroup) =>
        apiGroup.MapGet("/mongo", async (IConfiguration configuration) =>
        {
            var mongoClient = new MongoClient(configuration.GetConnectionString("mongo"));
            var database = mongoClient.GetDatabase("admin");

            var pingCommand = new BsonDocument("ping", 1);
            await database.RunCommandAsync<BsonDocument>(pingCommand).ConfigureAwait(false);

            return Results.Ok("Funcionou");
        });
}
