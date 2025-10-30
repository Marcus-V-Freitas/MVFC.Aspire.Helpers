namespace MVFC.Aspire.Helpers.Playground.Api.PubSub;

public sealed class GooglePubSubMessagePublisher(PublisherServiceApiClient publisherClient) : IMessagePublisher {
    private readonly PublisherServiceApiClient _publisherClient = publisherClient;

    public async Task PublishAsync<T>(string topicName, T content) {
        var topic = TopicName.FromProjectTopic("test-project", topicName);
        var json = JsonSerializer.Serialize(content);
        var message = new PubsubMessage {
            Data = ByteString.CopyFromUtf8(json)
        };

        await _publisherClient.PublishAsync(topic, [message]);
    }
}