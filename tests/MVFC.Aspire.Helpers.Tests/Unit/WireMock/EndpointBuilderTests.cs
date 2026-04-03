namespace MVFC.Aspire.Helpers.Tests.Unit.WireMock;

public sealed class EndpointBuilderTests : IDisposable
{
    private readonly WireMockServer _server = WireMockServer.Start();

    public void Dispose() => _server.Stop();

    [Fact]
    public async Task OnGet_WithJsonHandler_ShouldReturnJsonResponse()
    {
        // Arrange
        var builder = new EndpointBuilder(_server, "/api/test");
        builder.OnGet<object>(() => (new { Id = 1, Name = "Test" }, HttpStatusCode.OK, (BodyType?)null));

        using var client = new HttpClient { BaseAddress = new Uri(_server.Url!) };

        // Act
        var response = await client.GetAsync("/api/test", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.Should().Contain("Id");
    }

    [Fact]
    public async Task OnPost_WithJsonHandler_ShouldDeserializeAndRespond()
    {
        // Arrange
        var builder = new EndpointBuilder(_server, "/api/post");
        builder.OnPost<Dictionary<string, string>, object>(req =>
            (new { Echo = req["key"] }, HttpStatusCode.Created, (BodyType?)BodyType.Json));

        using var client = new HttpClient { BaseAddress = new Uri(_server.Url!) };
        var content = new StringContent("""{"key":"value"}""", Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/post", content, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task OnPost_WithEmptyBody_ShouldReturnBadRequest()
    {
        // Arrange
        var builder = new EndpointBuilder(_server, "/api/empty");
        builder.OnPost<Dictionary<string, string>, object>(req =>
            (new { Echo = "should-not-get-here" }, HttpStatusCode.OK, (BodyType?)null));

        using var client = new HttpClient { BaseAddress = new Uri(_server.Url!) };
        var content = new StringContent("", Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/empty", content, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task OnPut_WithHandler_ShouldReturnResponse()
    {
        // Arrange
        var builder = new EndpointBuilder(_server, "/api/put");
        builder.OnPut<Dictionary<string, string>, object>(req =>
            (new { Updated = true }, HttpStatusCode.OK, (BodyType?)null));

        using var client = new HttpClient { BaseAddress = new Uri(_server.Url!) };
        var content = new StringContent("""{"field":"value"}""", Encoding.UTF8, "application/json");

        // Act
        var response = await client.PutAsync("/api/put", content, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OnPatch_WithHandler_ShouldReturnResponse()
    {
        // Arrange
        var builder = new EndpointBuilder(_server, "/api/patch");
        builder.OnPatch<Dictionary<string, string>, object>(req =>
            (new { Patched = true }, HttpStatusCode.OK, (BodyType?)null));

        using var client = new HttpClient { BaseAddress = new Uri(_server.Url!) };
        var content = new StringContent("""{"field":"value"}""", Encoding.UTF8, "application/json");

        // Act
        var response = await client.PatchAsync("/api/patch", content, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OnDelete_WithHandler_ShouldReturnResponse()
    {
        // Arrange
        var builder = new EndpointBuilder(_server, "/api/delete");
        builder.OnDelete<object>(() => (new { Deleted = true }, HttpStatusCode.OK, (BodyType?)null));

        using var client = new HttpClient { BaseAddress = new Uri(_server.Url!) };

        // Act
        var response = await client.DeleteAsync("/api/delete", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RequireBearer_ValidToken_ShouldAllowRequest()
    {
        // Arrange
        var builder = new EndpointBuilder(_server, "/api/secure-valid");
        builder
            .RequireBearer("my-secret-token")
            .OnGet<object>(() => (new { Data = "safe" }, HttpStatusCode.OK, (BodyType?)null));

        using var client = new HttpClient { BaseAddress = new Uri(_server.Url!) };
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "my-secret-token");

        // Act
        var response = await client.GetAsync("/api/secure-valid", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RequireBearer_InvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var builder = new EndpointBuilder(_server, "/api/secure-invalid");
        builder
            .RequireBearer("correct-token", new { Error = "Unauthorized" })
            .OnGet<object>(() => (new { Data = "safe" }, HttpStatusCode.OK, (BodyType?)null));

        using var client = new HttpClient { BaseAddress = new Uri(_server.Url!) };
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "wrong-token");

        // Act
        var response = await client.GetAsync("/api/secure-invalid", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RequireBearer_NoAuthHeader_ShouldReturnUnauthorized()
    {
        // Arrange
        var builder = new EndpointBuilder(_server, "/api/secure-noheader");
        builder
            .RequireBearer("token123")
            .OnGet<object>(() => (new { Data = "safe" }, HttpStatusCode.OK, (BodyType?)null));

        using var client = new HttpClient { BaseAddress = new Uri(_server.Url!) };

        // Act
        var response = await client.GetAsync("/api/secure-noheader", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RequireCustomAuth_ShouldApplyCustomValidator()
    {
        // Arrange
        var builder = new EndpointBuilder(_server, "/api/custom-auth");
        builder.RequireCustomAuth(request =>
        {
            var hasKey = request.Headers != null &&
                         request.Headers.TryGetValue("X-Api-Key", out var keys) &&
                         keys.FirstOrDefault() == "valid-key";
            return hasKey
                ? (true, null, BodyType.Json)
                : (false, new { Error = "Forbidden" }, BodyType.Json);
        });
        builder
            .WithDefaultErrorStatusCode(HttpStatusCode.Forbidden)
            .OnGet<object>(() => (new { Ok = true }, HttpStatusCode.OK, (BodyType?)null));

        using var client = new HttpClient { BaseAddress = new Uri(_server.Url!) };
        client.DefaultRequestHeaders.Add("X-Api-Key", "invalid-key");

        // Act
        var response = await client.GetAsync("/api/custom-auth", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task WithResponseHeader_ShouldIncludeCustomHeader()
    {
        // Arrange
        var builder = new EndpointBuilder(_server, "/api/headers");
        builder
            .WithResponseHeader("X-Custom", "value1")
            .OnGet<object>(() => (new { Ok = true }, HttpStatusCode.OK, (BodyType?)null));

        using var client = new HttpClient { BaseAddress = new Uri(_server.Url!) };

        // Act
        var response = await client.GetAsync("/api/headers", TestContext.Current.CancellationToken);

        // Assert
        response.Headers.GetValues("X-Custom").Should().Contain("value1");
    }

    [Fact]
    public async Task WithResponseHeader_DuplicateKey_ShouldAppendValues()
    {
        // Arrange
        var builder = new EndpointBuilder(_server, "/api/multi-headers");
        builder
            .WithResponseHeader("X-Multi", "val1")
            .WithResponseHeader("X-Multi", "val2")
            .OnGet<object>(() => (new { Ok = true }, HttpStatusCode.OK, (BodyType?)null));

        using var client = new HttpClient { BaseAddress = new Uri(_server.Url!) };

        // Act
        var response = await client.GetAsync("/api/multi-headers", TestContext.Current.CancellationToken);

        // Assert
        response.Headers.GetValues("X-Multi").Should().Contain("val1");
        response.Headers.GetValues("X-Multi").Should().Contain("val2");
    }

    [Fact]
    public async Task WithResponseHeaders_ShouldIncludeAllHeaders()
    {
        // Arrange
        var builder = new EndpointBuilder(_server, "/api/batch-headers");
        builder
            .WithResponseHeaders(new Dictionary<string, IEnumerable<string>>
            {
                ["X-Header-A"] = ["a1"],
                ["X-Header-B"] = ["b1", "b2"]
            })
            .OnGet<object>(() => (new { Ok = true }, HttpStatusCode.OK, (BodyType?)null));

        using var client = new HttpClient { BaseAddress = new Uri(_server.Url!) };

        // Act
        var response = await client.GetAsync("/api/batch-headers", TestContext.Current.CancellationToken);

        // Assert
        response.Headers.GetValues("X-Header-A").Should().Contain("a1");
        response.Headers.GetValues("X-Header-B").Should().Contain("b1");
    }

    [Fact]
    public void WithResponseHeaders_NullDictionary_ShouldNotThrow()
    {
        // Arrange
        var builder = new EndpointBuilder(_server, "/api/null-headers");

        // Act
        var act = () => builder.WithResponseHeaders(null!);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task WithResponseBodyType_ShouldApplyToResponse()
    {
        // Arrange
        var builder = new EndpointBuilder(_server, "/api/string-body");
        builder
            .WithResponseBodyType(BodyType.String)
            .OnGet<string>(() => ("plain text", HttpStatusCode.OK, (BodyType?)BodyType.String));

        using var client = new HttpClient { BaseAddress = new Uri(_server.Url!) };

        // Act
        var response = await client.GetAsync("/api/string-body", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task WithDefaultBodyType_ShouldSetBothRequestAndResponse()
    {
        // Arrange
        var builder = new EndpointBuilder(_server, "/api/default-body");

        // Act
        builder.WithDefaultBodyType(BodyType.String);
        builder.OnPost<string, string>(req => (req.ToUpper(CultureInfo.InvariantCulture), HttpStatusCode.OK, (BodyType?)null));

        using var client = new HttpClient { BaseAddress = new Uri(_server.Url!) };
        var content = new StringContent("hello", Encoding.UTF8, "text/plain");

        // Act
        var response = await client.PostAsync("/api/default-body", content, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task WithRequestBodyType_Bytes_ShouldDeserializeAsBytes()
    {
        // Arrange
        var builder = new EndpointBuilder(_server, "/api/bytes-req");
        builder
            .WithRequestBodyType(BodyType.Bytes)
            .WithResponseBodyType(BodyType.Bytes)
            .OnPost<byte[], byte[]>(req => (req, HttpStatusCode.OK, (BodyType?)BodyType.Bytes));

        using var client = new HttpClient { BaseAddress = new Uri(_server.Url!) };
        var content = new StringContent("binary-data", Encoding.UTF8, "application/octet-stream");

        // Act
        var response = await client.PostAsync("/api/bytes-req", content, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OnGet_WithNullResponse_ShouldReturnEmptyBody()
    {
        // Arrange
        var builder = new EndpointBuilder(_server, "/api/null-resp");
        builder.OnGet<object?>(() => ((object?)null, HttpStatusCode.NoContent, (BodyType?)null));

        using var client = new HttpClient { BaseAddress = new Uri(_server.Url!) };

        // Act
        var response = await client.GetAsync("/api/null-resp", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public void SetEncoding_ShouldChangeEncoding()
    {
        // Arrange
        var builder = new EndpointBuilder(_server, "/api/encoding");

        // Act
        var result = builder.SetEncoding(Encoding.ASCII);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public async Task OnPost_FormUrlEncoded_ShouldDeserializeFormBody()
    {
        // Arrange
        var builder = new EndpointBuilder(_server, "/api/form");
        builder
            .WithRequestBodyType(BodyType.FormUrlEncoded)
            .WithResponseBodyType(BodyType.FormUrlEncoded)
            .OnPost<Dictionary<string, string>, Dictionary<string, string>>(req =>
                (req, HttpStatusCode.OK, (BodyType?)BodyType.FormUrlEncoded));

        using var client = new HttpClient { BaseAddress = new Uri(_server.Url!) };
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["key1"] = "value1",
            ["key2"] = "value2"
        });

        // Act
        var response = await client.PostAsync("/api/form", content, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task WithResponseBodyType_Bytes_NonByteObject_ShouldConvertToString()
    {
        // Arrange
        var builder = new EndpointBuilder(_server, "/api/bytes-string");
        builder.OnGet<string>(() => ("text-as-bytes", HttpStatusCode.OK, (BodyType?)BodyType.Bytes));

        using var client = new HttpClient { BaseAddress = new Uri(_server.Url!) };

        // Act
        var response = await client.GetAsync("/api/bytes-string", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task EndpointBuilder_WithCustomSettings_ShouldUseCustomSerializer()
    {
        // Arrange
        var settings = new EndpointSettings
        {
            Serialize = obj => System.Text.Json.JsonSerializer.Serialize(obj),
            Deserialize = (json, type) => System.Text.Json.JsonSerializer.Deserialize(json, type)
        };
        var builder = new EndpointBuilder(_server, "/api/custom-settings", settings);
        builder.OnGet<object>(() => (new { Custom = true }, HttpStatusCode.OK, (BodyType?)null));

        using var client = new HttpClient { BaseAddress = new Uri(_server.Url!) };

        // Act
        var response = await client.GetAsync("/api/custom-settings", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RequireBearer_NoBearerPrefix_ShouldReturnUnauthorized()
    {
        // Arrange
        var builder = new EndpointBuilder(_server, "/api/no-bearer-prefix");
        builder
            .RequireBearer("token123")
            .OnGet<object>(() => (new { Data = "safe" }, HttpStatusCode.OK, (BodyType?)null));

        using var client = new HttpClient { BaseAddress = new Uri(_server.Url!) };
        client.DefaultRequestHeaders.Add("Authorization", "Basic token123");

        // Act
        var response = await client.GetAsync("/api/no-bearer-prefix", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RequireBearer_WithNullError_ShouldReturnEmptyBody()
    {
        // Arrange
        var builder = new EndpointBuilder(_server, "/api/bearer-null-error");
        builder
            .RequireBearer("token", error: null)
            .OnGet<object>(() => (new { Data = "safe" }, HttpStatusCode.OK, (BodyType?)null));

        using var client = new HttpClient { BaseAddress = new Uri(_server.Url!) };
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "wrong");

        // Act
        var response = await client.GetAsync("/api/bearer-null-error", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
