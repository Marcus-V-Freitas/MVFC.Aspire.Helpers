var builder = DistributedApplication.CreateBuilder(args);

var messageConfig = new MessageConfig(
                            TopicName: "test-topic",
                            SubscriptionName: "test-subscription",
                            PushEndpoint: "/api/pub-sub-exit") {
    DeadLetterTopic = "test-dead-letter-topic",
    MaxDeliveryAttempts = 5,
    AckDeadlineSeconds = 300,
};

var pubSubConfig = new PubSubConfig(
                            projectId: "test-project",
                            messageConfig: messageConfig);

IList<IMongoClassDump> dumps = [
    new MongoClassDump<TestDatabase>("TestDatabase", "TestCollection", 100,
        new Faker<TestDatabase>()
              .CustomInstantiator(f => new TestDatabase(f.Person.FirstName, f.Person.Cpf())))
];

builder.AddProject<Projects.MVFC_Aspire_Helpers_Playground_Api>("api-exemplo")
       .WithCloudStorage(builder, "cloud-storage", localBucketFolder: "./bucket-data")
       .WithMongoReplicaSet(builder, "mongo", dumps: dumps)
       .WithGcpPubSub(builder, "gcp-pubsub", pubSubConfig);

await builder.Build().RunAsync();