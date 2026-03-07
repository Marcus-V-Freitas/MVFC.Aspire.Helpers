namespace MVFC.Aspire.Helpers.WireMock.Extensions;

internal static partial class LogExtensions
{

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting Aspire WireMock service")]
    public static partial void LogStartingAspireWireMock(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Aspire WireMock '{Name}' started on port {Port}")]
    public static partial void LogReadyAspireWireMock(this ILogger logger, string name, int port);

    [LoggerMessage(Level = LogLevel.Error, Message = "Aspire WireMock '{Name}' failed to start on port {Port}")]
    public static partial void LogErrorAspireWireMock(this ILogger logger, string name, int port);
}
