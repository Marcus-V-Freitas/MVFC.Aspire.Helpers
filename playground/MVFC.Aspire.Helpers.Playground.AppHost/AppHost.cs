var builder = DistributedApplication.CreateBuilder(args);

var messageConfig = new MessageConfig(
                            TopicName: "test-topic",
                            SubscriptionName: "test-subscription",
                            PushEndpoint: "/api/pub-sub-exit") {
    DeadLetterTopic = "test-dead-letter-topic",
    MaxDeliveryAttempts = 5,
    AckDeadlineSeconds = 300,
};

var rabbitConfig = new RabbitMQConfig(
    Exchanges: [
        new ExchangeConfig("test-exchange", "topic"), 
        new ExchangeConfig("dead-letter", "fanout")], 
    Queues: [
        new QueueConfig(Name: "test-queue", ExchangeName: "test-exchange", RoutingKey: "test.*", DeadLetterExchange: "dead-letter"), 
        new QueueConfig(Name: "empty-queue", ExchangeName: "test-exchange", RoutingKey: "empty.*"), 
        new QueueConfig(Name: "dlq", ExchangeName: "dead-letter")],
    VolumeName: "rabbit-mq");

var pubSubConfig = new PubSubConfig(
                            projectId: "test-project",
                            messageConfig: messageConfig);

var redisConfig = new RedisConfig(
    WithCommander: true, 
    VolumeName: "redis-data");

IReadOnlyCollection<IMongoClassDump> dumps = [
    new MongoClassDump<TestDatabase>("TestDatabase", "TestCollection", 100,
        new Faker<TestDatabase>()
              .CustomInstantiator(f => new TestDatabase(f.Person.FirstName, f.Person.Cpf())))
];

var api = builder.AddProject<Projects.MVFC_Aspire_Helpers_Playground_Api>("api-exemplo")
                 .WithCloudStorage(builder, name: "cloud-storage", localBucketFolder: "./bucket-data")
                 .WithMongoReplicaSet(builder, name: "mongo", dumps: dumps, port: 8282)
                 .WithGcpPubSub(builder, name: "gcp-pubsub", pubSubConfig: pubSubConfig)
                 .WithMailPit(builder, name: "mailpit")
                 .WithRabbitMQ(builder, name: "rabbitmq", rabbitMQConfig: rabbitConfig)
                 .WithRedis(builder, name: "redis", redisConfig: redisConfig);

var wireMock = builder.AddWireMock("wireMock", port: 8080, configure: (server) => {
    server.Endpoint("/api/echo")
          .WithDefaultBodyType(BodyType.String)
          .OnPost<string, string>(body => ($"Echo: {body}", HttpStatusCode.Created, null));

    server.Endpoint("/api/test")
          .WithDefaultBodyType(BodyType.String)
          .OnGet<string>(() => ("Aspire GET OK", HttpStatusCode.OK, null));

    server.Endpoint("/api/secure")
           .RequireBearer("mytoken", "Unauthorized", BodyType.String)
           .OnGet(() => ("Secret Data", HttpStatusCode.OK, BodyType.String));

    server.Endpoint("/api/secure")
          .RequireBearer("mytoken", "Unauthorized", BodyType.String)
          .OnGet(() => ("Secret Data", HttpStatusCode.OK, BodyType.String));

    server.Endpoint("/api/put")
           .WithDefaultBodyType(BodyType.String)
           .OnPut<string, string>(req => ($"Echo: {req}", HttpStatusCode.Accepted, BodyType.String));

    server.Endpoint("/api/customauth")
        .WithDefaultErrorStatusCode(HttpStatusCode.Forbidden)
        .RequireCustomAuth(req => (req.Headers!.ContainsKey("X-Test"), "Forbidden", BodyType.String))
        .OnGet(() => ("Authorized", HttpStatusCode.OK, BodyType.String));

    server.Endpoint("/api/headers")
        .WithResponseHeaders(new() { { "X-Test", ["v1", "v2"] } })
        .WithResponseHeader("X-Other", "v3")
        .OnGet(() => ("Headers OK", HttpStatusCode.OK, BodyType.String));

    server.Endpoint("/api/error")
        .WithRequestBodyType(BodyType.String)
        .WithDefaultErrorStatusCode((HttpStatusCode)418)
        .OnGet(() => ("I am a teapot", (HttpStatusCode)418, BodyType.String));

    server.Endpoint("/api/delete")
       .WithResponseBodyType(BodyType.String)
       .WithResponseHeader("v1", "v1")
       .WithResponseHeaders(new() { { "v1", ["v2", "v3"] } })
       .WithResponseHeader("v1", "v4")
       .OnDelete<string>(() => (null!, HttpStatusCode.NoContent, null));

    server.Endpoint("/api/form")
        .WithDefaultBodyType(BodyType.FormUrlEncoded)
        .OnPost<Dictionary<string, string>, IDictionary<string, string>>(body => (body, HttpStatusCode.OK, BodyType.FormUrlEncoded));

    server.Endpoint("/api/form-wrong")
       .WithDefaultBodyType(BodyType.FormUrlEncoded)
       .OnPost<string, string>(body => (body, HttpStatusCode.OK, BodyType.FormUrlEncoded));

    server.Endpoint("/api/patch")
        .WithDefaultBodyType(BodyType.String)
        .OnPatch<string, string>(body => ($"Patched: {body}", HttpStatusCode.OK, BodyType.String));

    server.Endpoint("/api/bytes")
        .WithDefaultBodyType(BodyType.Bytes)
        .OnPost<byte[], byte[]>(body => (body, HttpStatusCode.OK, BodyType.Bytes));

    server.Endpoint("/api/unsupported")
        .WithDefaultBodyType((BodyType)999) // BodyType n�o suportado
        .OnPost<string, string>(_ => ("Not Supported", HttpStatusCode.NotImplemented, null));

    server.Endpoint("/api/json")
        .WithDefaultBodyType(BodyType.Json)
        .OnPost<JsonModel, JsonModel>(body => (body, HttpStatusCode.OK, BodyType.Json));

    server.Endpoint("/webhook/payment")
          .WithDefaultBodyType(BodyType.Json)
          .OnGet<string>(() => {
              var payload = new PaymentPayload(2000, "R$");

              _ = Task.Run(async () => {
                  ApiHelper helper = new(api.GetEndpoint("http").Port);

                  await helper.SendPayloadAsync(payload);
              });

              return (null!, HttpStatusCode.Accepted, BodyType.Json);
          });
});

await builder.Build().RunAsync();
