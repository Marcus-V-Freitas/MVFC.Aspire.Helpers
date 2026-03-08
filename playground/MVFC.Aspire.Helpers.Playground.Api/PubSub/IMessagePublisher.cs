namespace MVFC.Aspire.Helpers.Playground.Api.PubSub;

internal interface IMessagePublisher 
{
    internal Task PublishAsync<T>(string topicName, T message);
}
