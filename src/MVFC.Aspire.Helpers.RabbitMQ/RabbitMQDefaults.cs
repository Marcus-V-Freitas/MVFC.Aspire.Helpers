namespace MVFC.Aspire.Helpers.RabbitMQ;

/// <summary>
/// Valores padrão para configuração do RabbitMQ.
/// </summary>
public static class RabbitMQDefaults
{
    /// <summary>
    /// Imagem Docker padrão do RabbitMQ.
    /// </summary>
    public const string DefaultRabbitMQImage = "rabbitmq";

    /// <summary>
    /// Tag padrão da imagem Docker do RabbitMQ (com Management UI).
    /// </summary>
    public const string DefaultRabbitMQTag = "3-management";

    /// <summary>
    /// Porta padrão AMQP do RabbitMQ.
    /// </summary>
    public const int DefaultAmqpPort = 5672;

    /// <summary>
    /// Porta padrão do Management UI do RabbitMQ.
    /// </summary>
    public const int DefaultManagementPort = 15672;

    /// <summary>
    /// Usuário padrão do RabbitMQ.
    /// </summary>
    public const string DefaultUsername = "guest";

    /// <summary>
    /// Senha padrão do RabbitMQ.
    /// </summary>
    public const string DefaultPassword = "guest";

    /// <summary>
    /// Seção padrão da connection string do RabbitMQ na configuração da aplicação.
    /// </summary>
    public const string DefaultConnectionStringSection = "ConnectionStrings:rabbitmq";
}
