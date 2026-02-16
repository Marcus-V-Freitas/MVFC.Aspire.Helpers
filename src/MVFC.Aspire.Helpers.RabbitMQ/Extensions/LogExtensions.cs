namespace MVFC.Aspire.Helpers.RabbitMQ.Extensions;

internal static partial class LogExtensions
{

    [LoggerMessage(Level = LogLevel.Information, Message = "RabbitMQ '{Name}' iniciado")]
    public static partial void LogRabbitMQStarted(this ILogger logger, string name);

    [LoggerMessage(Level = LogLevel.Information, Message = "Exchange '{ExchangeName}' criado no RabbitMQ '{RabbitName}'")]
    public static partial void LogExchangeCreated(this ILogger logger, string rabbitName, string exchangeName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Queue '{QueueName}' criada no RabbitMQ '{RabbitName}'")]
    public static partial void LogQueueCreated(this ILogger logger, string rabbitName, string queueName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Falha ao configurar RabbitMQ '{Name}'")]
    public static partial void LogRabbitMQConfigFailed(this ILogger logger, string name, Exception exception);
}
