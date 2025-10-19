namespace MVFC.Aspire.Helpers.Api.PubSub;

public interface IMessagePublisher
{
    Task PublishAsync<T>(string topicName, T message);
}