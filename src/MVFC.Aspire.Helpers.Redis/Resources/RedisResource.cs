namespace MVFC.Aspire.Helpers.Redis.Resources;

/// <summary>
/// Representa o recurso Redis como um container Aspire, fornecendo endpoint Redis
/// e uma expressão de string de conexão para integração em aplicações distribuídas.
/// </summary>
/// <remarks>
/// Esta classe encapsula a configuração do endpoint necessário para o funcionamento do Redis,
/// permitindo fácil referência e integração com outros recursos Aspire.
/// </remarks>
public sealed class RedisResource(string name) : ContainerResource(name), IResourceWithConnectionString {
    /// <summary>
    /// Nome do endpoint Redis utilizado pelo container.
    /// </summary>
    internal const string RedisEndpointName = "redis";

    private EndpointReference? _redisReference;

    /// <summary>
    /// Referência ao endpoint Redis do recurso.
    /// </summary>
    public EndpointReference RedisEndpoint =>
        _redisReference ??= new(this, RedisEndpointName);

    /// <summary>
    /// Expressão que representa a string de conexão Redis.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"{RedisEndpoint.Property(EndpointProperty.Host)}:{RedisEndpoint.Property(EndpointProperty.Port)}"
        );
}
