namespace MVFC.Aspire.Helpers.Mongo.Annotations;

/// <summary>
/// Shares a single dump execution and its outcome across all consumers of a MongoDB resource.
/// </summary>
internal sealed class MongoDumpsExecutedAnnotation : IResourceAnnotation
{
    /// <summary>
    /// Protects creation of the shared execution task.
    /// </summary>
    private readonly Lock _sync = new();

    /// <summary>
    /// Retains success, failure or cancellation without implicitly replaying append dumps.
    /// </summary>
    private Task? _execution;

    /// <summary>
    /// Starts the configured dumps once using the lifetime of the application.
    /// </summary>
    internal Task ExecuteAsync(MongoReplicaSetResource resource, CancellationToken cancellationToken)
    {
        lock (_sync)
            return _execution ??= ExecuteDumpsAsync(resource, cancellationToken);
    }

    /// <summary>
    /// Gates consumer startup on primary readiness and shared dumps, without letting a consumer cancel other consumers.
    /// </summary>
    internal async Task WaitAsync(
        MongoReplicaSetResource resource,
        Task primaryReady,
        CancellationToken applicationStopping,
        CancellationToken consumerCancellation)
    {
        consumerCancellation.ThrowIfCancellationRequested();
        await primaryReady.WaitAsync(consumerCancellation).ConfigureAwait(false);
        await ExecuteAsync(resource, applicationStopping).WaitAsync(consumerCancellation).ConfigureAwait(false);
    }

    /// <summary>
    /// Resolves the connection only when dumps are configured and preserves cancellation.
    /// </summary>
    private static async Task ExecuteDumpsAsync(MongoReplicaSetResource resource, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var dumps = resource.Dumps;
        if (dumps?.Count is not > 0)
            return;

        var connectionString = await resource.ConnectionStringExpression.GetValueAsync(cancellationToken).ConfigureAwait(false);
        await MongoClientFactory.ExecuteDumpsAsync(connectionString!, dumps, cancellationToken).ConfigureAwait(false);
    }
}
