namespace MVFC.Aspire.Helpers.PubSub.Models;

/// <summary>
/// Representa a configuração de uma mensagem para o Pub/Sub, incluindo o nome do tópico, o nome da assinatura
/// e, opcionalmente, o endpoint de push para entrega de mensagens.
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
public sealed record class MessageConfig(
    string TopicName,
    string SubscriptionName,
    string? PushEndpoint = null);