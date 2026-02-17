namespace MVFC.Aspire.Helpers.WireMock.Resources;

/// <summary>
/// Representa um recurso WireMock gerenciado pelo Aspire, com inicialização explícita e tratamento de erro.
/// </summary>
public sealed class WireMockResource : Resource, IResourceWithEndpoints, IDisposable {
    /// <summary>
    /// Instância do servidor WireMock associado ao recurso.
    /// </summary>
    private readonly WireMockServer _server;

    /// <summary>
    /// Indica se os recursos do servidor WireMock foram liberados, para evitar múltiplas chamadas de Dispose.
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// Porta TCP utilizada pelo servidor WireMock.
    /// </summary>
    public int Port { get; }

    /// <summary>
    /// Inicializa uma nova instância de <see cref="WireMockResource"/>.
    /// </summary>
    /// <param name="name">Nome do recurso.</param>
    /// <param name="port">Porta TCP para o servidor WireMock.</param>
    /// <param name="configure">Ação opcional para configuração adicional do servidor.</param>
    public WireMockResource(string name, int port, Action<WireMockServer>? configure = null)
        : base(name) {
        Port = port;
        _server = WireMockServer.Start(port);

        configure?.Invoke(_server);
    }

    /// <summary>
    /// Obtém a instância do servidor WireMock.
    /// </summary>
    public WireMockServer Server => _server;

    /// <summary>
    /// Libera os recursos do servidor WireMock, parando o servidor se estiver em execução.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            if (_server?.IsStarted == true)
            {
                _server.Stop();
                Thread.Sleep(100);
            }

            (_server as IDisposable)?.Dispose();
        }
        finally
        {
            _disposed = true;
        }
    }
}
