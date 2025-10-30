namespace MVFC.Aspire.Helpers.Playground.Api.PubSub;

public interface IMessagePublisher {
    Task PublishAsync<T>(string topicName, T message);
}