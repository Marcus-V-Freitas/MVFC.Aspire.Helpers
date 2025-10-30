namespace MVFC.Aspire.Helpers.GcpPubSub.Models;

/// <summary>
/// Representa a configuração de uma mensagem para o Pub/Sub, incluindo o nome do tópico, o nome da assinatura,
/// o endpoint de push para entrega de mensagens, configurações de dead letter e o tempo limite para confirmação (acknowledgment) da mensagem.
/// </summary>
/// <param name="TopicName">
/// Nome do tópico Pub/Sub ao qual a mensagem será publicada.
/// </param>
/// <param name="SubscriptionName">
/// (Opcional) Nome da assinatura associada ao tópico.
/// </param>
/// <param name="PushEndpoint">
/// (Opcional) Endpoint HTTP para entrega de mensagens via push. Se não informado, a assinatura será do tipo pull.
/// </param>
public sealed record class MessageConfig(
    string TopicName,
    string? SubscriptionName = null,
    string? PushEndpoint = null) {

    /// <summary>
    /// Nome do tópico Pub/Sub ao qual a mensagem será publicada.
    /// </summary>
    public string TopicName { get; init; } = TopicName;

    /// <summary>
    /// Nome da assinatura associada ao tópico.
    /// </summary>
    public string? SubscriptionName { get; init; } = SubscriptionName;

    /// <summary>
    /// (Opcional) Endpoint HTTP para entrega de mensagens via push. Se não informado, a assinatura será do tipo pull.
    /// </summary>
    public string? PushEndpoint { get; init; } = PushEndpoint;

    /// <summary>
    /// (Opcional) Nome do tópico de dead letter (DLQ) para onde mensagens não processadas serão encaminhadas após o número máximo de tentativas.
    /// Se não informado, a assinatura não terá suporte a dead letter.
    /// </summary>
    public string? DeadLetterTopic { get; init; }

    /// <summary>
    /// (Opcional) Número máximo de tentativas de entrega antes de encaminhar a mensagem para o tópico de dead letter.
    /// Válido apenas se <see cref="DeadLetterTopic"/> estiver definido. O padrão é 5.
    /// </summary>
    public int? MaxDeliveryAttempts { get; init; }

    /// <summary>
    /// (Opcional) Tempo limite em segundos para confirmação (acknowledgment) da mensagem.
    /// Se não informado, o padrão é 5 minutos.
    /// </summary>
    public int? AckDeadlineSeconds { get; init; }
}