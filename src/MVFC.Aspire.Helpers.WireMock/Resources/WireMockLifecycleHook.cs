namespace MVFC.Aspire.Helpers.WireMock.Resources;

/// <summary>
/// Eventing subscriber for WireMock resource lifecycle in Aspire.
/// Responsible for publishing events and updating WireMock resource state during initialization.
/// </summary>
internal sealed class WireMockLifecycleHook(
    ResourceNotificationService resourceNotificationService,
    IDistributedApplicationEventing eventing,
    ResourceLoggerService resourceLoggerService,
    IServiceProvider serviceProvider) : IDistributedApplicationEventingSubscriber
{
    private readonly ResourceNotificationService _resourceNotificationService = resourceNotificationService;
    private readonly IDistributedApplicationEventing _eventing = eventing;
    private readonly ResourceLoggerService _resourceLoggerService = resourceLoggerService;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <summary>
    /// Registers the <see cref="BeforeStartEvent"/> subscription in the Aspire eventing pipeline.
    /// </summary>
    /// <param name="eventing">Distributed application eventing instance.</param>
    /// <param name="executionContext">Distributed application execution context.</param>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task SubscribeAsync(
        IDistributedApplicationEventing eventing,
        DistributedApplicationExecutionContext executionContext,
        CancellationToken cancellationToken)
    {

        eventing.Subscribe<BeforeStartEvent>(OnBeforeStartAsync);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles the BeforeStartEvent, performing initialization actions for WireMock resources,
    /// publishing start and ready events, and updating state based on the initialization result.
    /// </summary>
    /// <param name="event">Event fired before the distributed application starts.</param>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task OnBeforeStartAsync(
        BeforeStartEvent @event,
        CancellationToken cancellationToken = default)
    {
        var appModel = @event.Model;
        foreach (var wireMockResource in appModel.Resources.OfType<WireMockResource>())
        {
            var logger = _resourceLoggerService.GetLogger(wireMockResource);

            await PublishStartEventAsync(wireMockResource, logger, cancellationToken).ConfigureAwait(false);
            await PublishReadyEventAsync(wireMockResource, cancellationToken).ConfigureAwait(false);
            await NotifyWireMockAsync(wireMockResource, logger).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Publishes the WireMock resource start event and logs it.
    /// </summary>
    /// <param name="resource">WireMock resource.</param>
    /// <param name="logger">Logger associated with the resource.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task PublishStartEventAsync(
        WireMockResource resource,
        ILogger logger,
        CancellationToken cancellationToken)
    {

        var startEvent = new BeforeResourceStartedEvent(resource, _serviceProvider);
        await _eventing.PublishAsync(startEvent, cancellationToken).ConfigureAwait(false);
        logger.LogStartingAspireWireMock();
    }

    /// <summary>
    /// Publishes the event indicating the WireMock resource is ready.
    /// </summary>
    /// <param name="resource">WireMock resource.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task PublishReadyEventAsync(
        WireMockResource resource,
        CancellationToken cancellationToken)
    {

        var readyEvent = new ResourceReadyEvent(resource, _serviceProvider);
        await _eventing.PublishAsync(readyEvent, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates the WireMock resource state to "Running" or "Error" and logs the status.
    /// </summary>
    /// <param name="resource">WireMock resource.</param>
    /// <param name="logger">Logger associated with the resource.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task NotifyWireMockAsync(
        WireMockResource resource,
        ILogger logger)
    {

        var started = resource.Server.IsStarted;

        LogStatusMessage(logger, resource, started);

        await _resourceNotificationService.PublishUpdateAsync(
            resource,
            s => s with
            {
                StartTimeStamp = started ? DateTimeOffset.UtcNow.DateTime : null,
                StopTimeStamp = started ? null : DateTimeOffset.UtcNow.DateTime,
                Urls = BuildUrls(resource, started),
                State = BuildResourceState(started),
            }).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the URL snapshot for the WireMock resource if the server is started; otherwise returns an empty array.
    /// </summary>
    private static ImmutableArray<UrlSnapshot> BuildUrls(WireMockResource resource, bool started) =>
        started ?
        [new UrlSnapshot("http", resource.Server.Url!, false)] :
        [];

    /// <summary>
    /// Returns the resource state snapshot: "Running" if started, or "Error" otherwise.
    /// </summary>
    private static ResourceStateSnapshot BuildResourceState(bool started) =>
        started ?
        new ResourceStateSnapshot("Running", KnownResourceStateStyles.Success) :
        new ResourceStateSnapshot("Error", KnownResourceStateStyles.Error);

    /// <summary>
    /// Logs the WireMock resource status message (ready or error).
    /// </summary>
    private static void LogStatusMessage(ILogger logger, WireMockResource resource, bool started)
    {
        Action<string, int> logMessage = started ? logger.LogReadyAspireWireMock : logger.LogErrorAspireWireMock;

        logMessage(resource.Name, resource.Port);
    }
}
