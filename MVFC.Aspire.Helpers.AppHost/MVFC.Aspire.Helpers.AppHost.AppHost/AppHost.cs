var builder = DistributedApplication.CreateBuilder(args);

var messageConfig = new MessageConfig(
                            TopicName: "test-topic",
                            SubscriptionName: "test-subscription",
                            PushEndpoint: "/api/pub-sub-exit");

var pubSubConfig = new PubSubConfig(
                            projectId: "test-project",
                            messageConfig: messageConfig);

builder.AddProject<Projects.MVFC_Aspire_Helpers_Api>("api-exemplo")
       .WithCloudStorage(builder, "cloud-storage", "./bucket-data")
       .WithMongoReplicaSet(builder, "mongo")
       .WithGcpPubSub(builder, "gcp-pubsub", pubSubConfig);

await builder.Build().RunAsync();