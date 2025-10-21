namespace MVFC.Aspire.Helpers.PubSub.Models;

/// <summary>
/// Representa a configuração de uma mensagem para o Pub/Sub, incluindo o nome do tópico, o nome da assinatura,
/// o endpoint de push para entrega de mensagens e o tempo limite para confirmação (acknowledgment) da mensagem.
/// </summary>
/// <param name="TopicName">
/// Nome do tópico Pub/Sub ao qual a mensagem será publicada.
/// </param>
/// <param name="SubscriptionName">
/// Nome da assinatura associada ao tópico.
/// </param>
/// <param name="PushEndpoint">
/// (Opcional) Endpoint HTTP para entrega de mensagens via push. Se não informado, a assinatura será do tipo pull.
/// </param>
/// <param name="AckDeadlineSeconds">
/// (Opcional) Tempo limite para confirmação (acknowledgment) da mensagem após o recebimento.
/// Se não informado, será utilizado o valor de 5 minutos.
/// </param>
public sealed record class MessageConfig(
    string TopicName,
    string SubscriptionName,
    string? PushEndpoint = null,
    TimeSpan? AckDeadlineSeconds = null);