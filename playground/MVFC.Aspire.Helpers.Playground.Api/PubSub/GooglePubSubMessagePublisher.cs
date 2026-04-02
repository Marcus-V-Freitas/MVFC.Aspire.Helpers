namespace MVFC.Aspire.Helpers.Playground.Api.PubSub;

internal sealed class GooglePubSubMessagePublisher(PublisherServiceApiClient publisherClient) : IMessagePublisher
{
    private readonly PublisherServiceApiClient _publisherClient = publisherClient;

    public async Task PublishAsync<T>(string topicName, T message)
    {
        var topic = TopicName.FromProjectTopic("test-project", topicName);
        var json = JsonSerializer.Serialize(message);
        var pubsubMessage = new PubsubMessage
        {
            Data = ByteString.CopyFromUtf8(json)
        };

        await _publisherClient.PublishAsync(topic, [pubsubMessage]).ConfigureAwait(false);
    }
}
