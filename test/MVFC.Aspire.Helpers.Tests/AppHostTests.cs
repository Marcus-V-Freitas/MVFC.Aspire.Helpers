namespace MVFC.Aspire.Helpers.Tests;

public sealed class AppHostTests(AppHostFixture fixture) : IClassFixture<AppHostFixture> {
    private readonly AppHostFixture _fixture = fixture;

    #region Mongo

    [Fact]
    public async Task MongoOkStatusCode() {
        // Arrange & Act
        var response = await _fixture.AppHttpClient.GetAsync("/api/mongo", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region CloudStorage

    [Fact]
    public async Task CloudStorageOkStatusCode() {

        // Arrange & Act
        var response = await _fixture.AppHttpClient.GetAsync("/api/bucket/bucket-teste", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region PubSub

    [Fact]
    public async Task PubSubOkStatusCode() {

        // Arrange & Act
        var response = await _fixture.AppHttpClient.GetAsync("/api/pub-sub-enter", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region MailPit

    [Fact]
    public async Task MailPitOkStatusCode() {

        // Act
        var body = JsonContent.Create(new {
            From = "noreply@teste.com",
            To = "teste@exemplo.com",
            Subject = "Teste",
            Body = "Mensagem de teste"
        });

        var response = await _fixture.AppHttpClient.PostAsync("/api/send-email", body, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region WireMock

    [Fact]
    public async Task WireMockEndpoint_Get_ShouldReturnMockedResponse() {
        var response = await _fixture.HttpClient.GetAsync("/api/test", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Be("Aspire GET OK");
    }

    [Fact]
    public async Task WireMockEndpoint_Post_ShouldReturnPostedData() {
        var httpContent = new StringContent("Aspire", Encoding.UTF8, "text/plain");
        var response = await _fixture.HttpClient.PostAsync("/api/echo", httpContent, TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        content.Should().Be("Echo: Aspire");
    }

    [Fact]
    public async Task WireMockEndpoint_Post_ShouldReturnBadRequest() {
        var response = await _fixture.HttpClient.PostAsync("/api/echo", null, TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        content.Should().BeEmpty();
    }

    [Fact]
    public async Task WireMockEndpoint_Auth_ShouldReturnUnauthorized_WhenTokenMissing() {
        var response = await _fixture.HttpClient.GetAsync("/api/secure", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        content.Should().Be("Unauthorized");
    }

    [Fact]
    public async Task WireMockEndpoint_Auth_ShouldReturnData_WhenTokenPresent() {
        var message = new HttpRequestMessage(HttpMethod.Get, "/api/secure") {
            Headers = { { "Authorization", "Bearer mytoken" } }
        };

        var response = await _fixture.HttpClient.SendAsync(message, TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Be("Secret Data");
    }

    [Fact]
    public async Task PutEndpoint_ShouldReturnAccepted() {
        var response = await _fixture.HttpClient.PutAsync("/api/put", new StringContent("Aspire", Encoding.UTF8, "text/plain"), TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        content.Should().Be("Echo: Aspire");
    }

    [Fact]
    public async Task CustomAuth_ShouldReturnForbidden_WhenInvalid() {
        var response = await _fixture.HttpClient.GetAsync("/api/customauth", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        content.Should().Be("Forbidden");
    }

    [Theory]
    [InlineData(true, HttpStatusCode.OK, "Authorized")]
    [InlineData(false, HttpStatusCode.Forbidden, "Forbidden")]
    public async Task CustomAuth_ShouldReturnAuthorized_WhenValid(bool useHeader, HttpStatusCode expectedStatusCode, string expectedContent) {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/customauth");

        if (useHeader)
            request.Headers.Add("X-Test", "ok");

        var response = await _fixture.HttpClient.SendAsync(request, TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(expectedStatusCode);
        content.Should().Be(expectedContent);
    }

    [Fact]
    public async Task ResponseHeaders_ShouldReturnAllHeaders() {
        var response = await _fixture.HttpClient.GetAsync("/api/headers", TestContext.Current.CancellationToken);
        string.Join(',', response.Headers.GetValues("X-Test")).Should().Be("v1,v2");
        string.Join(',', response.Headers.GetValues("X-Other")).Should().Be("v3");
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.Should().Be("Headers OK");
    }

    [Fact]
    public async Task ErrorStatusCode_ShouldReturnCustomStatus() {
        var response = await _fixture.HttpClient.GetAsync("/api/error", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be((HttpStatusCode)418);
        content.Should().Be("I am a teapot");
    }

    [Fact]
    public async Task WireMockEndpoint_Delete_ShouldReturnNoContent() {
        var response = await _fixture.HttpClient.DeleteAsync("/api/delete", TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task WireMockEndpoint_FormUrlEncoded_ShouldParseFormData() {
        var formContent = new FormUrlEncodedContent([new KeyValuePair<string, string>("key", "value")]);
        var response = await _fixture.HttpClient.PostAsync("/api/form", formContent, TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Be("key=value");
    }

    [Fact]
    public async Task WireMockEndpoint_Patch_ShouldReturnPatched() {
        var patchContent = new StringContent("patch-data", Encoding.UTF8, "text/plain");
        var request = new HttpRequestMessage(HttpMethod.Patch, "/api/patch") { Content = patchContent };
        var response = await _fixture.HttpClient.SendAsync(request, TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Be("Patched: patch-data");
    }

    [Fact]
    public async Task WireMockEndpoint_Bytes_ShouldReturnBytes() {
        var bytes = Encoding.UTF8.GetBytes("AspireBytes");
        var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
        var response = await _fixture.HttpClient.PostAsync("/api/bytes", content, TestContext.Current.CancellationToken);
        var responseBytes = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseBytes.Should().BeEquivalentTo(bytes);
    }

    [Fact]
    public async Task WireMockEndpoint_FormUrlEncoded_ShouldReturnBadRequest_WhenNotDictionary() {
        var content = new StringContent("not-a-dictionary", Encoding.UTF8, "application/x-www-form-urlencoded");
        var response = await _fixture.HttpClient.PostAsync("/api/form-wrong", content, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task WireMockEndpoint_Headers_ShouldOverwriteAndReturnAll() {
        var response = await _fixture.HttpClient.DeleteAsync("/api/delete", TestContext.Current.CancellationToken);
        var headers = response.Headers.GetValues("v1").ToArray();
        headers.Should().Contain("v2");
        headers.Should().Contain("v3");
        headers.Should().Contain("v4");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task WireMockEndpoint_UnsupportedBodyType_ShouldReturnError() {
        var content = new StringContent("unsupported", Encoding.UTF8, "application/unsupported");
        var response = await _fixture.HttpClient.PostAsync("/api/unsupported", content, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task WireMockEndpoint_Json_ShouldEchoJsonObject() {
        var model = new JsonModel("Aspire JSON");
        var json = JsonSerializer.Serialize(model);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _fixture.HttpClient.PostAsync("/api/json", content, TestContext.Current.CancellationToken);
        var responseJson = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var responseModel = JsonSerializer.Deserialize<JsonModel>(responseJson);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseModel.Should().NotBeNull();
        responseModel!.Message.Should().Be("Aspire JSON");
    }

    [Fact]
    public async Task WireMockEndpoint_Webhook_ShouldAccept() {

        var response = await _fixture.HttpClient.GetAsync("/webhook/payment", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }

    #endregion

    #region Redis

    [Fact]
    public async Task Redis_Get_ShouldReturnValue() {
        // Arrange
        await _fixture.AppHttpClient.GetAsync("/api/redis/set/integration-key/integration-value", TestContext.Current.CancellationToken);

        // Act
        var response = await _fixture.AppHttpClient.GetAsync("/api/redis/get/integration-key", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Be("integration-value");
    }

    [Fact]
    public async Task Redis_GetNonExistent_ShouldReturnNotFound() {
        // Arrange & Act
        var response = await _fixture.AppHttpClient.GetAsync("/api/redis/get/non-existent-key", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region RabbitMQ

    [Fact]
    public async Task RabbitMQ_PublishAndConsume_ShouldReturnMessage() {
        // Arrange
        await Task.Delay(2000, TestContext.Current.CancellationToken);
        var message = "integration-test-message";

        // Act - Publish
        await _fixture.AppHttpClient.PostAsync($"/api/rabbitmq/publish/test-exchange/test.key/{message}", null, TestContext.Current.CancellationToken);

        // Wait a bit for message to be processed
        await Task.Delay(2000, TestContext.Current.CancellationToken);

        // Act - Consume
        var response = await _fixture.AppHttpClient.GetAsync("/api/rabbitmq/consume/test-queue", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain(message);
    }

    [Fact]
    public async Task RabbitMQ_ConsumeEmpty_ShouldReturnNoContent() {
        // Arrange & Act
        await Task.Delay(2000, TestContext.Current.CancellationToken);
        var response = await _fixture.AppHttpClient.GetAsync("/api/rabbitmq/consume/empty-queue", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    #endregion
}