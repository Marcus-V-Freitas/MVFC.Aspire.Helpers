namespace MVFC.Aspire.Helpers.Api.Endpoints;

public static class MongoEndpoints {
    public static void MapMongoEndpoints(this IEndpointRouteBuilder apiGroup) =>
        apiGroup.MapGet("/mongo", async (IConfiguration configuration) => {
            var mongoClient = new MongoClient(configuration.GetConnectionString("mongo"));
            var database = mongoClient.GetDatabase("admin");

            try {
                var pingCommand = new BsonDocument("ping", 1);
                var result = await database.RunCommandAsync<BsonDocument>(pingCommand);

                return Results.Ok("Funcionou");
            }
            catch {

                return Results.InternalServerError("Erro");
            }
        });
}