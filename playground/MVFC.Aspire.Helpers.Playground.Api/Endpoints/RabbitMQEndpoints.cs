namespace MVFC.Aspire.Helpers.Playground.Api.Endpoints;

public static class RabbitMQEndpoints {

    public static void MapRabbitMQEndpoints(this IEndpointRouteBuilder apiGroup) {

        apiGroup.MapPost("/rabbitmq/publish/{exchange}/{routingKey}/{message}", (IConnection connection, string exchange, string routingKey, string message) => {
            using var channel = connection.CreateModel();
            var body = System.Text.Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: exchange, routingKey: routingKey, basicProperties: null, body: body);
            return Results.Ok($"Mensagem publicada em '{exchange}' com routing key '{routingKey}'");
        });

        apiGroup.MapGet("/rabbitmq/consume/{queue}", (IConnection connection, string queue) => {
            using var channel = connection.CreateModel();
            var result = channel.BasicGet(queue, autoAck: true);

            if (result == null)
                return Results.NoContent();

            var message = System.Text.Encoding.UTF8.GetString(result.Body.ToArray());
            return Results.Ok(message);
        });
    }
}
