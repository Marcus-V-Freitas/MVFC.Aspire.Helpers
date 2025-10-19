namespace MVFC.Aspire.Helpers.PubSub.Models;

/// <summary>
/// Representa a configuração do Pub/Sub para um projeto, incluindo o ID do projeto, as configurações de mensagens (tópicos e assinaturas)
/// e o tempo de espera (delay) para inicialização dos recursos.
/// </summary>
public sealed record class PubSubConfig
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="PubSubConfig"/> com um único <see cref="MessageConfig"/>.
    /// </summary>
    /// <param name="projectId">ID do projeto GCP utilizado pelo Pub/Sub.</param>
    /// <param name="messageConfig">Configuração de mensagem (tópico e assinatura) a ser utilizada.</param>
    /// <param name="secondsDelay">(Opcional) Tempo de espera em segundos para inicialização dos recursos. Padrão: 5.</param>
    public PubSubConfig(string projectId, MessageConfig messageConfig, int secondsDelay = 5) : this(projectId, secondsDelay, [messageConfig])
    {

    }

    /// <summary>
    /// Inicializa uma nova instância de <see cref="PubSubConfig"/> com múltiplas configurações de mensagens.
    /// </summary>
    /// <param name="projectId">ID do projeto GCP utilizado pelo Pub/Sub.</param>
    /// <param name="secondsDelay">(Opcional) Tempo de espera em segundos para inicialização dos recursos. Padrão: 5.</param>
    /// <param name="messageConfigs">(Opcional) Lista de configurações de mensagens (tópicos e assinaturas) a serem utilizadas.</param>
    public PubSubConfig(string projectId, int secondsDelay = 5, IList<MessageConfig>? messageConfigs = null)
    {
        ProjectId = projectId;
        MessageConfigs = messageConfigs ?? [];
        UpDelay = TimeSpan.FromSeconds(secondsDelay);
    }

    /// <summary>
    /// ID do projeto GCP utilizado pelo Pub/Sub.
    /// </summary>
    public string ProjectId { get; init; }

    /// <summary>
    /// Lista de configurações de mensagens, cada uma contendo informações de tópico, assinatura e endpoint de push (opcional).
    /// </summary>
    public IList<MessageConfig> MessageConfigs { get; init; }

    /// <summary>
    /// Tempo de espera para inicialização dos recursos do Pub/Sub.
    /// </summary>
    public TimeSpan UpDelay { get; init; }
}