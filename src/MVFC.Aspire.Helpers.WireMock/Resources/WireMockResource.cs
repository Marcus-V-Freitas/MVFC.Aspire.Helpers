namespace MVFC.Aspire.Helpers.WireMock.Resources;

/// <summary>
/// Representa um recurso WireMock gerenciado pelo Aspire, com inicialização explícita e tratamento de erro.
/// </summary>
public sealed class WireMockResource : Resource, IResourceWithEndpoints, IResourceWithConnectionString
{
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
        ReferenceExpression.Create($"http://localhost:{Port.ToString(CultureInfo.InvariantCulture)}");
}
