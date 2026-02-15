namespace MVFC.Aspire.Helpers.WireMock.Extensions;

internal static partial class LogExtensions {

    [LoggerMessage(Level = LogLevel.Information, Message = "Iniciando serviço Aspire WireMock")]
    public static partial void LogStartingAspireWireMock(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Aspire WireMock '{Name}' iniciado na porta {Port}")]
    public static partial void LogReadyAspireWireMock(this ILogger logger, string name, int port);

    [LoggerMessage(Level = LogLevel.Error, Message = "Aspire WireMock '{Name}' falhou ao iniciar na porta {Port}")]
    public static partial void LogErrorAspireWireMock(this ILogger logger, string name, int port);
}
