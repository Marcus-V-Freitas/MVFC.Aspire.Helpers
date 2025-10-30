namespace MVFC.Aspire.Helpers.Tests;

public sealed class AppHostTests(AppHostFixture fixture) : IClassFixture<AppHostFixture> {
    private readonly AppHostFixture _fixture = fixture;

    #region Mongo

    [Fact]
    public async Task MongoOkStatusCode() {
        // Act
        var httpClient = _fixture.DistributedApplication.CreateHttpClient("api-exemplo");
        var response = await httpClient.GetAsync("/api/mongo");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region CloudStorage

    [Fact]
    public async Task CloudStorageOkStatusCode() {

        // Act
        var httpClient = _fixture.DistributedApplication.CreateHttpClient("api-exemplo");
        var response = await httpClient.GetAsync("/api/bucket/bucket-teste");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region PubSub

    [Fact]
    public async Task PubSubOkStatusCode() {

        // Act
        var httpClient = _fixture.DistributedApplication.CreateHttpClient("api-exemplo");
        var response = await httpClient.GetAsync("/api/pub-sub-enter");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region WireMock

    [Fact]
    public async Task WireMockEndpoint_Get_ShouldReturnMockedResponse() {
        var response = await _fixture.HttpClient.GetAsync("/api/test");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Aspire GET OK", content);
    }

    [Fact]
    public async Task WireMockEndpoint_Post_ShouldReturnPostedData() {
        var httpContent = new StringContent("Aspire", Encoding.UTF8, "text/plain");
        var response = await _fixture.HttpClient.PostAsync("/api/echo", httpContent);
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("Echo: Aspire", content);
    }

    [Fact]
    public async Task WireMockEndpoint_Post_ShouldReturnBadRequest() {
        var response = await _fixture.HttpClient.PostAsync("/api/echo", null);
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(content);
    }

    [Fact]
    public async Task WireMockEndpoint_Auth_ShouldReturnUnauthorized_WhenTokenMissing() {
        var response = await _fixture.HttpClient.GetAsync("/api/secure");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("Unauthorized", content);
    }

    [Fact]
    public async Task WireMockEndpoint_Auth_ShouldReturnData_WhenTokenPresent() {
        var message = new HttpRequestMessage(HttpMethod.Get, "/api/secure") {
            Headers = { { "Authorization", "Bearer mytoken" } }
        };

        var response = await _fixture.HttpClient.SendAsync(message);
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Secret Data", content);
    }

    [Fact]
    public async Task PutEndpoint_ShouldReturnAccepted() {
        var response = await _fixture.HttpClient.PutAsync("/api/put", new StringContent("Aspire", Encoding.UTF8, "text/plain"));
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal("Echo: Aspire", content);
    }

    [Fact]
    public async Task CustomAuth_ShouldReturnForbidden_WhenInvalid() {
        var response = await _fixture.HttpClient.GetAsync("/api/customauth");
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("Forbidden", content);
    }

    [Theory]
    [InlineData(true, HttpStatusCode.OK, "Authorized")]
    [InlineData(false, HttpStatusCode.Forbidden, "Forbidden")]
    public async Task CustomAuth_ShouldReturnAuthorized_WhenValid(bool useHeader, HttpStatusCode expectedStatusCode, string expectedContent) {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/customauth");

        if (useHeader)
            request.Headers.Add("X-Test", "ok");

        var response = await _fixture.HttpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Equal(expectedContent, content);
    }

    [Fact]
    public async Task ResponseHeaders_ShouldReturnAllHeaders() {
        var response = await _fixture.HttpClient.GetAsync("/api/headers");
        Assert.Equal("v1,v2", string.Join(',', response.Headers.GetValues("X-Test")));
        Assert.Equal("v3", string.Join(',', response.Headers.GetValues("X-Other")));
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Headers OK", content);
    }

    [Fact]
    public async Task ErrorStatusCode_ShouldReturnCustomStatus() {
        var response = await _fixture.HttpClient.GetAsync("/api/error");
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal((HttpStatusCode)418, response.StatusCode);
        Assert.Equal("I am a teapot", content);
    }

    [Fact]
    public async Task WireMockEndpoint_Delete_ShouldReturnNoContent() {
        var response = await _fixture.HttpClient.DeleteAsync("/api/delete");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task WireMockEndpoint_FormUrlEncoded_ShouldParseFormData() {
        var formContent = new FormUrlEncodedContent([new KeyValuePair<string, string>("key", "value")]);
        var response = await _fixture.HttpClient.PostAsync("/api/form", formContent);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("key=value", content);
    }

    [Fact]
    public async Task WireMockEndpoint_Patch_ShouldReturnPatched() {
        var patchContent = new StringContent("patch-data", Encoding.UTF8, "text/plain");
        var request = new HttpRequestMessage(HttpMethod.Patch, "/api/patch") { Content = patchContent };
        var response = await _fixture.HttpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Patched: patch-data", content);
    }

    [Fact]
    public async Task WireMockEndpoint_Bytes_ShouldReturnBytes() {
        var bytes = Encoding.UTF8.GetBytes("AspireBytes");
        var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
        var response = await _fixture.HttpClient.PostAsync("/api/bytes", content);
        var responseBytes = await response.Content.ReadAsByteArrayAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(bytes, responseBytes);
    }

    [Fact]
    public async Task WireMockEndpoint_FormUrlEncoded_ShouldReturnBadRequest_WhenNotDictionary() {
        var content = new StringContent("not-a-dictionary", Encoding.UTF8, "application/x-www-form-urlencoded");
        var response = await _fixture.HttpClient.PostAsync("/api/form-wrong", content);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task WireMockEndpoint_Headers_ShouldOverwriteAndReturnAll() {
        var response = await _fixture.HttpClient.DeleteAsync("/api/delete");
        var headers = response.Headers.GetValues("v1").ToArray();
        Assert.Contains("v2", headers);
        Assert.Contains("v3", headers);
        Assert.Contains("v4", headers);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task WireMockEndpoint_UnsupportedBodyType_ShouldReturnError() {
        var content = new StringContent("unsupported", Encoding.UTF8, "application/unsupported");
        var response = await _fixture.HttpClient.PostAsync("/api/unsupported", content);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task WireMockEndpoint_Json_ShouldEchoJsonObject() {
        var model = new JsonModel("Aspire JSON");
        var json = JsonSerializer.Serialize(model);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _fixture.HttpClient.PostAsync("/api/json", content);
        var responseJson = await response.Content.ReadAsStringAsync();
        var responseModel = JsonSerializer.Deserialize<JsonModel>(responseJson);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseModel);
        Assert.Equal("Aspire JSON", responseModel!.Message);
    }

    #endregion
}