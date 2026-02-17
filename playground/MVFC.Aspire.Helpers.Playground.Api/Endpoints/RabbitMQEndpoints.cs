namespace MVFC.Aspire.Helpers.Playground.Api.Endpoints;

public static class RabbitMQEndpoints {

    public static void MapRabbitMQEndpoints(this IEndpointRouteBuilder apiGroup) {

        apiGroup.MapPost("/rabbitmq/publish/{exchange}/{routingKey}/{message}", async (IConnection connection, string exchange, string routingKey, string message) => {
            using var channel = await connection.CreateChannelAsync();
            var body = System.Text.Encoding.UTF8.GetBytes(message);
            await channel.BasicPublishAsync(exchange, routingKey, mandatory: true, body: body, cancellationToken: CancellationToken.None);
            return Results.Ok($"Mensagem publicada em '{exchange}' com routing key '{routingKey}'");
        });

        apiGroup.MapGet("/rabbitmq/consume/{queue}", async (IConnection connection, string queue) => {
            using var channel = await connection.CreateChannelAsync();
            var result = await channel.BasicGetAsync(queue, autoAck: true, CancellationToken.None);

            if (result == null)
                return Results.NoContent();

            var message = System.Text.Encoding.UTF8.GetString(result.Body.ToArray());
            return Results.Ok(message);
        });
    }
}
