namespace MVFC.Aspire.Helpers.Tests;

public sealed class AppHostTests(AppHostFixture fixture) : IClassFixture<AppHostFixture>
{
    private readonly AppHostFixture _fixture = fixture;

    #region Gotenberg

    [Fact]
    public async Task GotenbergOkStatusCode()
    {
        // Arrange
        var body = new JsonObject
        {
            ["Html"] = await HtmlExtensions.ExtractHtmlTemplateAsync()
        };

        // Act
        using var response = await _fixture.PlaygroundApi.ConvertToPdfAsync(body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ContentHeaders!.ContentType!.MediaType.Should().Be("application/pdf");
        response.Content!.Should().NotBeNull();
    }

    #endregion

    #region Mongo

    [Fact]
    public async Task MongoOkStatusCode()
    {
        // Arrange & Act
        using var response = await _fixture.PlaygroundApi.GetMongoStatusAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region CloudStorage

    [Fact]
    public async Task CloudStorageOkStatusCode()
    {
        // Arrange & Act
        using var response = await _fixture.PlaygroundApi.GetCloudStorageBucketAsync("bucket-teste");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region PubSub

    [Fact]
    public async Task PubSubOkStatusCode()
    {
        // Arrange & Act
        using var response = await _fixture.PlaygroundApi.GetPubSubEnterAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region MailPit

    [Fact]
    public async Task MailPitOkStatusCode()
    {
        // Act
        var body = new JsonObject
        {
            ["From"] = "noreply@teste.com",
            ["To"] = "teste@exemplo.com",
            ["Subject"] = "Teste",
            ["Body"] = "Mensagem de teste"
        };

        using var response = await _fixture.PlaygroundApi.SendEmailAsync(body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region WireMock

    [Fact]
    public async Task WireMockEndpoint_Get_ShouldReturnMockedResponse()
    {
        // Arrange & Act
        using var response = await _fixture.WireMockApi.GetTestAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content!.Should().Be("Aspire GET OK");
    }

    [Fact]
    public async Task WireMockEndpoint_Post_ShouldReturnPostedData()
    {
        // Arrange & Act
        var httpContent = "Aspire";
        using var response = await _fixture.WireMockApi.PostEchoAsync(httpContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Content!.Should().Be("Echo: Aspire");
    }

    [Fact]
    public async Task WireMockEndpoint_Post_ShouldReturnBadRequest()
    {
        // Arrange & Act
        using var response = await _fixture.WireMockApi.PostEchoEmptyAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Error!.Content.Should().BeEmpty();
    }

    [Fact]
    public async Task WireMockEndpoint_Auth_ShouldReturnUnauthorized_WhenTokenMissing()
    {
        // Arrange & Act
        using var response = await _fixture.WireMockApi.GetSecureAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Error!.Content.Should().Be("Unauthorized");
    }

    [Fact]
    public async Task WireMockEndpoint_Auth_ShouldReturnData_WhenTokenPresent()
    {
        // Act
        using var response = await _fixture.WireMockApi.GetSecureWithTokenAsync("mytoken");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content!.Should().Be("Secret Data");
    }

    [Fact]
    public async Task PutEndpoint_ShouldReturnAccepted()
    {
        // Arrange & Act
        using var response = await _fixture.WireMockApi.PutEchoAsync("Aspire");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        response.Content!.Should().Be("Echo: Aspire");
    }

    [Fact]
    public async Task CustomAuth_ShouldReturnForbidden_WhenInvalid()
    {
        // Arrange & Act
        using var response = await _fixture.WireMockApi.GetCustomAuthAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        response.Error!.Content.Should().Be("Forbidden");
    }

    [Theory]
    [InlineData(true, HttpStatusCode.OK, "Authorized")]
    [InlineData(false, HttpStatusCode.Forbidden, "Forbidden")]
    public async Task CustomAuth_ShouldReturnAuthorized_WhenValid(bool useHeader, HttpStatusCode expectedStatusCode, string expectedContent)
    {
        // Arrange & Act
        var response = useHeader ? await _fixture.WireMockApi.GetCustomAuthWithHeaderAsync() : await _fixture.WireMockApi.GetCustomAuthAsync();

        // Assert
        response.StatusCode.Should().Be(expectedStatusCode);
        var content = response.IsSuccessful ? response.Content : response.Error!.Content;
        content!.Should().Be(expectedContent);
    }

    [Fact]
    public async Task ResponseHeaders_ShouldReturnAllHeaders()
    {
        // Arrange & Act
        using var response = await _fixture.WireMockApi.GetHeadersAsync();
        string.Join(',', response.Headers.GetValues("X-Test")).Should().Be("v1,v2");
        string.Join(',', response.Headers.GetValues("X-Other")).Should().Be("v3");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content!.Should().Be("Headers OK");
    }

    [Fact]
    public async Task ErrorStatusCode_ShouldReturnCustomStatus()
    {
        // Arrange & Act
        using var response = await _fixture.WireMockApi.GetErrorAsync();

        // Assert
        response.StatusCode.Should().Be((HttpStatusCode)418);
        response.Error!.Content.Should().Be("I am a teapot");
    }

    [Fact]
    public async Task WireMockEndpoint_Delete_ShouldReturnNoContent()
    {
        // Arrange & Act
        using var response = await _fixture.WireMockApi.DeleteAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task WireMockEndpoint_FormUrlEncoded_ShouldParseFormData()
    {
        // Arrange
        var formContent = new Dictionary<string, string> { ["key"] = "value" };

        // Act
        using var response = await _fixture.WireMockApi.PostFormAsync(formContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content!.Should().Be("key=value");
    }

    [Fact]
    public async Task WireMockEndpoint_Patch_ShouldReturnPatched()
    {
        // Arrange & Act
        var patchContent = "patch-data";
        using var response = await _fixture.WireMockApi.PatchAsync(patchContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content!.Should().Be("Patched: patch-data");
    }

    [Fact]
    public async Task WireMockEndpoint_Bytes_ShouldReturnBytes()
    {
        // Arrange
        var bytes = Encoding.UTF8.GetBytes("AspireBytes");
        var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        // Act
        using var response = await _fixture.WireMockApi.PostBytesAsync(content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content!.StreamToByteArray().Should().BeEquivalentTo(bytes);
    }

    [Fact]
    public async Task WireMockEndpoint_FormUrlEncoded_ShouldReturnBadRequest_WhenNotDictionary()
    {
        // Arrange
        var content = "not-a-dictionary";

        // Act
        using var response = await _fixture.WireMockApi.PostFormWrongAsync(content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task WireMockEndpoint_Headers_ShouldOverwriteAndReturnAll()
    {
        // Arrange
        using var responseDelete = await _fixture.WireMockApi.DeleteAsync();

        // Act
        var headers = responseDelete.Headers.GetValues("v1").ToArray();

        // Assert
        headers.Should().Contain("v2");
        headers.Should().Contain("v3");
        headers.Should().Contain("v4");
        responseDelete.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task WireMockEndpoint_UnsupportedBodyType_ShouldReturnError()
    {
        // Arrange
        var content = "unsupported";

        // Act
        using var response = await _fixture.WireMockApi.PostUnsupportedAsync(content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task WireMockEndpoint_Json_ShouldEchoJsonObject()
    {
        // Arrange
        var body = new JsonObject { ["Message"] = "Aspire JSON" };

        // Act
        using var response = await _fixture.WireMockApi.PostJsonAsync(body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Should().NotBeNull();
        response.Content!.Message.Should().Be("Aspire JSON");
    }

    [Fact]
    public async Task WireMockEndpoint_Webhook_ShouldAccept()
    {
        // Arrange & Act
        using var response = await _fixture.WireMockApi.GetWebhookPaymentAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }

    #endregion

    #region Redis

    [Fact]
    public async Task Redis_Get_ShouldReturnValue()
    {
        // Arrange
        using var responseSet = await _fixture.PlaygroundApi.SetRedisValueAsync("integration-key", "integration-value");

        // Act
        using var response = await _fixture.PlaygroundApi.GetRedisValueAsync("integration-key");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content!.Should().Be("integration-value");
    }

    [Fact]
    public async Task Redis_GetNonExistent_ShouldReturnNotFound()
    {
        // Arrange & Act
        using var response = await _fixture.PlaygroundApi.GetRedisValueAsync("non-existent-key");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region RabbitMQ

    [Fact]
    public async Task RabbitMQ_PublishAndConsume_ShouldReturnMessage()
    {
        // Arrange
        var message = "integration-test-message";

        // Act - Publish
        using var responsePublish = await _fixture.PlaygroundApi.PublishRabbitMqMessageAsync("test-exchange", "test.key", message);

        // Act - Consume
        using var response = await _fixture.PlaygroundApi.ConsumeRabbitMqMessageAsync("test-queue");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content!.Should().Contain(message);
    }

    [Fact]
    public async Task RabbitMQ_ConsumeEmpty_ShouldReturnNoContent()
    {
        // Arrange & Act
        using var response = await _fixture.PlaygroundApi.ConsumeRabbitMqMessageAsync("empty-queue");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    #endregion

    #region Keycloak

    [Fact]
    public async Task Keycloak_ApiAuthentication_ShouldReturnUnauthorizedWithoutToken()
    {
        // Arrange & Act
        using var response = await _fixture.PlaygroundApi.GetSecretDataAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Keycloak_ApiAuthentication_ShouldReturnOkWithToken()
    {
        // Arrange
        var tokenRequest = new Dictionary<string, string>
        {
            ["client_id"] = "my-api",
            ["client_secret"] = "api-secret-1234",
            ["grant_type"] = "password",
            ["username"] = "marcus.admin",
            ["password"] = "Admin@123"
        };

        using var tokenResponse = await _fixture.KeycloakApi.GetTokenAsync("my-app", tokenRequest);
        var accessToken = tokenResponse.Content!["access_token"]!.ToString();

        // Act
        using var response = await _fixture.PlaygroundApi.GetSecretDataWithTokenAsync(accessToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}
