namespace MVFC.Aspire.Helpers.WireMock.Resources;

/// <summary>
/// Representa um recurso WireMock gerenciado pelo Aspire, com inicialização explícita e tratamento de erro.
/// </summary>
public sealed class WireMockResource : Resource, IResourceWithEndpoints, IResourceWithConnectionString, IDisposable
{
    /// <summary>
    /// Indica se os recursos do servidor WireMock foram liberados, para evitar múltiplas chamadas de Dispose.
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// Obtém a instância do servidor WireMock.
    /// </summary>
    public WireMockServer Server { get; }

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
    public WireMockResource(string name, int? port, Action<WireMockServer>? configure = null)
        : base(name)
    {

        Server = WireMockServer.Start(port);
        Port = Server.Port;

        configure?.Invoke(Server);
    }

    /// <summary>
    /// Expressão que representa a connection string HTTP local do servidor WireMock.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create($"http://localhost:{Port.ToString()}");

    /// <summary>
    /// Libera os recursos do servidor WireMock, parando o servidor se estiver em execução.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            if (Server?.IsStarted == true)
            {
                Server.Stop();
                Thread.Sleep(100);
            }

            (Server as IDisposable)?.Dispose();
        }
        finally
        {
            _disposed = true;
        }
    }
}
