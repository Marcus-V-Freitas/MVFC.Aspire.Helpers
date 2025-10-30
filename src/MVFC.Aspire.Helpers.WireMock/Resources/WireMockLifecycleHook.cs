namespace MVFC.Aspire.Helpers.WireMock.Resources;

/// <summary>
/// Hook de ciclo de vida para recursos WireMock no Aspire.
/// Responsável por publicar eventos e atualizar o estado dos recursos WireMock durante a alocação de endpoints.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class WireMockLifecycleHook(
    ResourceNotificationService resourceNotificationService,
    IDistributedApplicationEventing eventing,
    ResourceLoggerService resourceLoggerService,
    IServiceProvider serviceProvider) : IDistributedApplicationLifecycleHook {
    private readonly ResourceNotificationService _resourceNotificationService = resourceNotificationService;
    private readonly IDistributedApplicationEventing _eventing = eventing;
    private readonly ResourceLoggerService _resourceLoggerService = resourceLoggerService;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <summary>
    /// Executa ações após a alocação dos endpoints dos recursos WireMock,
    /// publicando eventos de início e pronto, além de atualizar o estado conforme o resultado da inicialização.
    /// </summary>
    /// <param name="appModel">Modelo da aplicação distribuída contendo os recursos.</param>
    /// <param name="cancellationToken">Token para cancelamento da operação assíncrona.</param>
    /// <returns>Uma tarefa que representa a operação assíncrona.</returns>
    public async Task BeforeStartAsync(
        DistributedApplicationModel appModel,
        CancellationToken cancellationToken = default) {
        foreach (var wireMockResource in appModel.Resources.OfType<WireMockResource>()) {
            var logger = _resourceLoggerService.GetLogger(wireMockResource);

            await PublicarEventoInicioAsync(wireMockResource, logger, cancellationToken);
            await PublicarEventoProntoAsync(wireMockResource, cancellationToken);

            if (wireMockResource.Server.IsStarted) {
                await NotificarWireMockIniciadoAsync(wireMockResource, logger);
            }
            else {
                await NotificarWireMockErroAsync(wireMockResource, logger);
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
    private async Task PublicarEventoInicioAsync(
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
    private async Task PublicarEventoProntoAsync(
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
    private async Task NotificarWireMockIniciadoAsync(
        WireMockResource resource,
        ILogger logger) {

        logger.logReadyAspireWireMock(resource.Name, resource.Port);

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
    private async Task NotificarWireMockErroAsync(
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