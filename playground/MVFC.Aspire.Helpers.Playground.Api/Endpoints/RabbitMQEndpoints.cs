namespace MVFC.Aspire.Helpers.Playground.Api.Endpoints;

internal static class RabbitMQEndpoints 
{
    public static void MapRabbitMQEndpoints(this IEndpointRouteBuilder apiGroup) 
    {
        apiGroup.MapPost("/rabbitmq/publish/{exchange}/{routingKey}/{message}", async (IConnection connection, string exchange, string routingKey, string message) => 
        {
            using var channel = await connection.CreateChannelAsync().ConfigureAwait(false);
            var body = System.Text.Encoding.UTF8.GetBytes(message);
            await channel.BasicPublishAsync(exchange, routingKey, mandatory: true, body: body, cancellationToken: CancellationToken.None).ConfigureAwait(false);
            return Results.Ok($"Mensagem publicada em '{exchange}' com routing key '{routingKey}'");
        });

        apiGroup.MapGet("/rabbitmq/consume/{queue}", async (IConnection connection, string queue) => 
        {
            using var channel = await connection.CreateChannelAsync().ConfigureAwait(false);
            var result = await channel.BasicGetAsync(queue, autoAck: true, CancellationToken.None).ConfigureAwait(false);

            if (result == null)
                return Results.NoContent();

            var message = System.Text.Encoding.UTF8.GetString(result.Body.ToArray());
            return Results.Ok(message);
        });
    }
}
