namespace MVFC.Aspire.Helpers.Mongo.Extensions;

internal static partial class LogExtensions {

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to process MongoDB dump: {DatabaseName}.{CollectionName}")]
    public static partial void LogMongoDumpFailed(this ILogger logger, Exception exception, string databaseName, string collectionName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unexpected error processing dump: {DatabaseName}.{CollectionName}")]
    public static partial void LogMongoDumpUnexpectedError(this ILogger logger, Exception exception, string databaseName, string collectionName);
}
