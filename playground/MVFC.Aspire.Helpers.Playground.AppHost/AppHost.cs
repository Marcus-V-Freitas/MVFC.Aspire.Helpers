var builder = DistributedApplication.CreateBuilder(args);

// --- GCP Spanner Configs ---
var spannerConfig = new SpannerConfig(
    ProjectId: "test-project",
    InstanceId: "dev-instance",
    DatabaseId: "dev-db",
    DdlStatements:
    [
        """
        CREATE TABLE Users (
            UserId STRING(36) NOT NULL,
            Name STRING(256) NOT NULL,
            CreatedAt TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true)
        ) PRIMARY KEY (UserId)
        """
    ]);

// --- GCP Pub/Sub Configs ---
var messageConfig = new MessageConfig(
                            TopicName: "test-topic",
                            SubscriptionName: "test-subscription",
                            PushEndpoint: "/api/pub-sub-exit") {
    DeadLetterTopic = "test-dead-letter-topic",
    MaxDeliveryAttempts = 5,
    AckDeadlineSeconds = 300,
};

var emptyMessageConfig = new MessageConfig("empty-topic")
{
    DeadLetterTopic = null,
};

var pubSubConfig1 = new PubSubConfig(projectId: "test-project", messageConfig: messageConfig);
var pubSubConfig2 = new PubSubConfig(projectId: "test-project-2", messageConfig: emptyMessageConfig);

// --- MongoDB Dumps ---
var dumps = new MongoClassDump<TestDatabase>(
    DatabaseName: "TestDatabase",
    CollectionName: "TestCollection",
    Quantity: 100,
    Faker: MongoFaker.GenerateFaker());

// --- Apigee Workspace ---
var apigeeWorkspace = Path.Combine(Directory.GetCurrentDirectory(), "apigee-workspace");

// --- GCP Spanner ---
var spanner = builder.AddGcpSpanner("gcp-spanner")
                     .WithSpannerConfigs(spannerConfig)
                     .WithWaitTimeout(30);

// --- Keycloak ---
var keycloak = builder.AddKeycloak("keycloak")
                      .WithAdminCredentials("admin", "Admin@123")
                      .WithSeeds(new MyAppRealm(), new EmptyRealmSeed())
                      .WithImportStrategy(KeycloakImportStrategy.OverwriteExisting)
                      .WithDataVolume("key-cloak-data");

// --- Cloud Storage ---
var cloudStorage = builder.AddCloudStorage("cloud-storage")
                          .WithBucketFolder("./bucket-data");

// --- MongoDB ---
var mongo = builder.AddMongoReplicaSet("mongo")
                   .WithDumps(dumps)
                   .WithDataVolume("mongo-data");

// --- GCP Pub/Sub ---
var pubSub = builder.AddGcpPubSub("gcp-pubsub")
                    .WithWaitTimeout(15)
                    .WithPubSubConfigs(pubSubConfig1, pubSubConfig2);

var pubSubUI = builder.AddGcpPubSubUI("gcp-pubsub-ui")
                      .WithReference(pubSub);

// --- Mailpit ---
var mailpit = builder.AddMailpit("mailpit")
                     .WithMaxMessageSize(50)
                     .WithSmtpHostname("localhost")
                     .WithDataFile("mailpit-data")
                     .WithWebAuth("teste", "teste")
                     .WithVerboseLogging();

// --- RabbitMQ ---
var rabbitMQ = builder.AddRabbitMQ("rabbitmq")
                      .WithCredentials("teste", "teste")
                      .WithExchanges(
                          new ExchangeConfig("test-exchange", "topic"),
                          new ExchangeConfig("dead-letter", "fanout"))
                      .WithQueues(
                          new QueueConfig(Name: "test-queue", ExchangeName: "test-exchange", RoutingKey: "test.*", DeadLetterExchange: "dead-letter", MessageTTL: 100),
                          new QueueConfig(Name: "empty-queue", ExchangeName: "test-exchange", RoutingKey: "empty.*"),
                          new QueueConfig(Name: "dlq", ExchangeName: "dead-letter"))
                      .WithDataVolume("rabbit-mq");

// --- Redis ---
var redis = builder.AddRedis("redis")
                   .WithPassword("teste")
                   .WithCommander()
                   .WithDataVolume("redis-data");

// --- Gotenberg ---
var gotenberg = builder.AddGotenberg("gotenberg", port: 3000);

// Referenciar recursos no projeto
var api = builder.AddProject<Projects.MVFC_Aspire_Helpers_Playground_Api>("api-exemplo")
                 .WithEndpoint("http", e => e.IsProxied = false)
                 .WithReference(cloudStorage)
                 .WaitFor(cloudStorage)
                 .WithReference(mongo)
                 .WaitFor(mongo)
                 .WithReference(pubSub)
                 .WaitFor(pubSub)
                 .WithReference(mailpit)
                 .WaitFor(mailpit)
                 .WithReference(rabbitMQ)
                 .WaitFor(rabbitMQ)
                 .WithReference(redis)
                 .WaitFor(redis)
                 .WithReference(pubSubUI)
                 .WaitFor(pubSubUI)
                 .WithReference(gotenberg)
                 .WaitFor(gotenberg)
                 .WaitFor(keycloak)
                 .WithReference(keycloak,
                         realmName: "my-app",
                         clientId: "my-api",
                         clientSecret: "api-secret-1234")
                  .WaitFor(spanner)
                  .WithReference(spanner);

var wireMock = builder.AddWireMock("wireMock", port: 7070, configure: (server) => {
    server.Endpoint("/api/echo")
          .SetEncoding(Encoding.UTF8)
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
        .WithDefaultBodyType((BodyType)999)
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

                  await helper.SendPayloadAsync(payload).ConfigureAwait(false);
              });

              return (null!, HttpStatusCode.Accepted, BodyType.Json);
          });
});

// --- Apigee Emulator ---
builder.AddApigeeEmulator("apigee-emulator")
       .WithWorkspace(apigeeWorkspace, "demo/health")
       .WithEnvironment("local")
       .WithBackend(api);

await builder.Build().RunAsync().ConfigureAwait(false);
