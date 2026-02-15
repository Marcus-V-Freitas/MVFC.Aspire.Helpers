namespace MVFC.Aspire.Helpers.WireMock.Resources;

/// <summary>
/// Subscriber de eventos do ciclo de vida para recursos WireMock no Aspire.
/// Responsável por publicar eventos e atualizar o estado dos recursos WireMock durante a inicialização.
/// </summary>
internal sealed class WireMockLifecycleHook(
    ResourceNotificationService resourceNotificationService,
    IDistributedApplicationEventing eventing,
    ResourceLoggerService resourceLoggerService,
    IServiceProvider serviceProvider) : IDistributedApplicationEventingSubscriber {
    private readonly ResourceNotificationService _resourceNotificationService = resourceNotificationService;
    private readonly IDistributedApplicationEventing _eventing = eventing;
    private readonly ResourceLoggerService _resourceLoggerService = resourceLoggerService;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <summary>
    /// Registra a assinatura do evento <see cref="BeforeStartEvent"/> no pipeline de eventos do Aspire.
    /// </summary>
    /// <param name="eventing">Instância do sistema de eventos da aplicação distribuída.</param>
    /// <param name="executionContext">Contexto de execução da aplicação distribuída.</param>
    /// <param name="cancellationToken">Token para cancelamento da operação assíncrona.</param>
    /// <returns>Uma tarefa que representa a operação assíncrona.</returns>
    public Task SubscribeAsync(
        IDistributedApplicationEventing eventing,
        DistributedApplicationExecutionContext executionContext,
        CancellationToken cancellationToken = default) {

        eventing.Subscribe<BeforeStartEvent>(OnBeforeStartAsync);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Manipula o evento BeforeStartEvent, executando ações de inicialização dos recursos WireMock,
    /// publicando eventos de início e pronto, além de atualizar o estado conforme o resultado da inicialização.
    /// </summary>
    /// <param name="event">Evento disparado antes do início da aplicação distribuída.</param>
    /// <param name="cancellationToken">Token para cancelamento da operação assíncrona.</param>
    /// <returns>Uma tarefa que representa a operação assíncrona.</returns>
    private async Task OnBeforeStartAsync(
        BeforeStartEvent @event,
        CancellationToken cancellationToken = default) {
        var appModel = @event.Model;
        foreach (var wireMockResource in appModel.Resources.OfType<WireMockResource>()) {
            var logger = _resourceLoggerService.GetLogger(wireMockResource);

            await PublishStartEventAsync(wireMockResource, logger, cancellationToken);
            await PublishReadyEventAsync(wireMockResource, cancellationToken);

            if (wireMockResource.Server.IsStarted) {
                await NotifyWireMockStartedAsync(wireMockResource, logger);
            }
            else {
                await NotifyWireMockErrorAsync(wireMockResource, logger);
            }
        }
    }

    /// <summary>
    /// Publica o evento de início do recurso WireMock e registra no log.
    /// </summary>
    /// <param name="resource">Recurso WireMock.</param>
    /// <param name="logger">Logger associado ao recurso.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Uma tarefa que representa a operação assíncrona.</returns>
    private async Task PublishStartEventAsync(
        WireMockResource resource,
        ILogger logger,
        CancellationToken cancellationToken) {

        var startEvent = new BeforeResourceStartedEvent(resource, _serviceProvider);
        await _eventing.PublishAsync(startEvent, cancellationToken);
        logger.LogStartingAspireWireMock();
    }

    /// <summary>
    /// Publica o evento indicando que o recurso WireMock está pronto.
    /// </summary>
    /// <param name="resource">Recurso WireMock.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Uma tarefa que representa a operação assíncrona.</returns>
    private async Task PublishReadyEventAsync(
        WireMockResource resource,
        CancellationToken cancellationToken) {

        var readyEvent = new ResourceReadyEvent(resource, _serviceProvider);
        await _eventing.PublishAsync(readyEvent, cancellationToken);
    }

    /// <summary>
    /// Atualiza o estado do recurso WireMock para "Running" e registra no log.
    /// </summary>
    /// <param name="resource">Recurso WireMock.</param>
    /// <param name="logger">Logger associado ao recurso.</param>
    /// <returns>Uma tarefa que representa a operação assíncrona.</returns>
    private async Task NotifyWireMockStartedAsync(
        WireMockResource resource,
        ILogger logger) {

        logger.LogReadyAspireWireMock(resource.Name, resource.Port);

        await _resourceNotificationService.PublishUpdateAsync(
            resource,
            s => s with {
                StartTimeStamp = DateTime.UtcNow,
                Urls = [new UrlSnapshot("http", resource.Server.Url!, false)],
                State = new ResourceStateSnapshot("Running", KnownResourceStateStyles.Success)
            });
    }

    /// <summary>
    /// Atualiza o estado do recurso WireMock para "Error" e registra no log.
    /// </summary>
    /// <param name="resource">Recurso WireMock.</param>
    /// <param name="logger">Logger associado ao recurso.</param>
    /// <returns>Uma tarefa que representa a operação assíncrona.</returns>
    private async Task NotifyWireMockErrorAsync(
        WireMockResource resource,
        ILogger logger) {

        logger.LogErrorAspireWireMock(resource.Name, resource.Port);

        await _resourceNotificationService.PublishUpdateAsync(
            resource,
            s => s with {
                StopTimeStamp = DateTime.UtcNow,
                State = new ResourceStateSnapshot("Error", KnownResourceStateStyles.Error)
            });
    }
}
