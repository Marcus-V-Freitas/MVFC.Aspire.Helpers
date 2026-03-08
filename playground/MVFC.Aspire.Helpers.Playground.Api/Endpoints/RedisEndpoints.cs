namespace MVFC.Aspire.Helpers.Playground.Api.Endpoints;

public static class RedisEndpoints 
{
    public static void MapRedisEndpoints(this IEndpointRouteBuilder apiGroup) 
    {
        apiGroup.MapGet("/redis/set/{key}/{value}", async (IConnectionMultiplexer redis, string key, string value) => 
        {
            var db = redis.GetDatabase();
            await db.StringSetAsync(key, value).ConfigureAwait(false);
            return Results.Ok($"Chave '{key}' definida com valor '{value}'");
        });

        apiGroup.MapGet("/redis/get/{key}", async (IConnectionMultiplexer redis, string key) => 
        {
            var db = redis.GetDatabase();
            var value = await db.StringGetAsync(key).ConfigureAwait(false);

            return value.IsNullOrEmpty ?
                Results.NotFound($"Chave '{key}' não encontrada") :
                Results.Text(value.ToString());
        });
    }
}
