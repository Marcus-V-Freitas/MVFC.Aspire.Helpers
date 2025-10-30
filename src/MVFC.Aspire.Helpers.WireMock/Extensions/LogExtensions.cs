namespace MVFC.Aspire.Helpers.WireMock.Extensions;

[ExcludeFromCodeCoverage]
internal static partial class LogExtensions {

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Iniciando serviço Aspire WireMock")]
    public static partial void LogStartingAspireWireMock(this ILogger logger);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Aspire WireMock '{Name}' iniciado na porta {Port}")]
    public static partial void logReadyAspireWireMock(this ILogger logger, string name, int port);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Aspire WireMock '{Name}' falhou ao iniciar na porta {Port}")]
    public static partial void LogErrorAspireWireMock(this ILogger logger, string name, int port);
}