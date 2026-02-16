namespace MVFC.Aspire.Helpers.Redis.Extensions;

internal static partial class LogExtensions {

    [LoggerMessage(Level = LogLevel.Information, Message = "Redis '{Name}' iniciado na porta {Port}")]
    public static partial void LogRedisStarted(this ILogger logger, string name, int port);

    [LoggerMessage(Level = LogLevel.Error, Message = "Falha ao iniciar Redis '{Name}'")]
    public static partial void LogRedisFailed(this ILogger logger, string name, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Redis Commander '{Name}' iniciado na porta {Port}")]
    public static partial void LogRedisCommanderStarted(this ILogger logger, string name, int port);
}
