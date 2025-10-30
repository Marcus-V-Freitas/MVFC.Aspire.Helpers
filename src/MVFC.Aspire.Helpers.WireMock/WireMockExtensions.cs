namespace MVFC.Aspire.Helpers.WireMock;

/// <summary>
/// Métodos de extensão para facilitar a integração do WireMock com Aspire,
/// incluindo criação de endpoints e registro do recurso WireMock na aplicação distribuída.
/// </summary>
public static class WireMockExtensions {
    /// <summary>
    /// Cria um <see cref="EndpointBuilder"/> para o path informado, permitindo configurar endpoints mockados.
    /// </summary>
    /// <param name="server">Instância do servidor WireMock.</param>
    /// <param name="path">Caminho do endpoint a ser configurado.</param>
    /// <returns>Uma instância de <see cref="EndpointBuilder"/> para configuração do endpoint.</returns>
    public static EndpointBuilder Endpoint(this WireMockServer server, string path)
        => new(server, path);

    /// <summary>
    /// Adiciona um recurso WireMock à aplicação distribuída Aspire, registrando o lifecycle hook e configurando o endpoint.
    /// </summary>
    /// <param name="builder">Builder da aplicação distribuída.</param>
    /// <param name="name">Nome do recurso WireMock.</param>
    /// <param name="port">Porta TCP para o servidor WireMock. Padrão: 8080.</param>
    /// <param name="configure">Ação opcional para configuração adicional do servidor WireMock.</param>
    /// <returns>Um builder de recurso para o <see cref="WireMockResource"/>.</returns>
    public static IResourceBuilder<WireMockResource> AddWireMock(
        this IDistributedApplicationBuilder builder,
        string name,
        int port = 8080,
        Action<WireMockServer>? configure = null) {
        var resource = new WireMockResource(name, port, configure);

        // Registrar lifecycle hook
        builder.Services.TryAddLifecycleHook<WireMockLifecycleHook>();

        return builder.AddResource(resource)
                      .WithInitialState(new CustomResourceSnapshot {
                          Properties =
                          [
                              new(CustomResourceKnownProperties.Source, "WireMock Aspire Resource")
                          ],
                          ResourceType = "WireMockAspire",
                          CreationTimeStamp = DateTime.UtcNow,
                          State = KnownResourceStates.NotStarted,
                      });
    }
}