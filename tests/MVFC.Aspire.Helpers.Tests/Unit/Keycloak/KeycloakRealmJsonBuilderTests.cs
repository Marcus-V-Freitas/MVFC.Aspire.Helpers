namespace MVFC.Aspire.Helpers.Tests.Unit.Keycloak;

public sealed class KeycloakRealmJsonBuilderTests
{
    [Fact]
    public void Build_WithEmptySeed_ShouldProduceMinimalJson()
    {
        // Arrange
        var seed = new MinimalRealmSeed("my-realm");

        // Act
        var json = KeycloakRealmJsonBuilder.Build(seed);

        // Assert
        json.Should().Contain("\"realm\":\"my-realm\"");
        json.Should().Contain("\"enabled\":true");
        json.Should().NotContain("\"roles\"");
        json.Should().NotContain("\"clients\"");
        json.Should().NotContain("\"groups\"");
        json.Should().NotContain("\"users\"");
    }

    [Fact]
    public void Build_WithRoles_ShouldIncludeRolesSection()
    {
        // Arrange
        var seed = new SeedWithRoles("role-realm");

        // Act
        var json = KeycloakRealmJsonBuilder.Build(seed);

        // Assert
        json.Should().Contain("\"roles\"");
        json.Should().Contain("admin");
        json.Should().Contain("user");
    }

    [Fact]
    public void Build_WithClients_ShouldIncludeClientsSection()
    {
        // Arrange
        var seed = new SeedWithClients("client-realm");

        // Act
        var json = KeycloakRealmJsonBuilder.Build(seed);

        // Assert
        json.Should().Contain("\"clients\"");
        json.Should().Contain("my-api");
        json.Should().Contain("audience-mapper");
    }

    [Fact]
    public void Build_WithClients_NoAudienceMapper_ShouldOmitProtocolMappers()
    {
        // Arrange
        var seed = new SeedWithClientsNoAudience("no-audience-realm");

        // Act
        var json = KeycloakRealmJsonBuilder.Build(seed);

        // Assert
        json.Should().Contain("\"clients\"");
        json.Should().Contain("my-api");
        json.Should().NotContain("audience-mapper");
    }

    [Fact]
    public void Build_WithUsers_WithPassword_ShouldIncludeCredentials()
    {
        // Arrange
        var seed = new SeedWithUsers("user-realm", "temp-pass");

        // Act
        var json = KeycloakRealmJsonBuilder.Build(seed);

        // Assert
        json.Should().Contain("\"users\"");
        json.Should().Contain("testuser");
        json.Should().Contain("temp-pass");
        json.Should().Contain("\"temporary\":true");
    }

    [Fact]
    public void Build_WithUsers_WithoutPassword_ShouldNotIncludeCredentials()
    {
        // Arrange
        var seed = new SeedWithUsers("user-realm-no-pass", null);

        // Act
        var json = KeycloakRealmJsonBuilder.Build(seed);

        // Assert
        json.Should().Contain("\"users\"");
        json.Should().Contain("testuser");
        json.Should().NotContain("credentials");
    }

    [Fact]
    public void Build_WithGroups_ShouldIncludeGroupsSection()
    {
        // Arrange
        var seed = new SeedWithGroups("group-realm");

        // Act
        var json = KeycloakRealmJsonBuilder.Build(seed);

        // Assert
        json.Should().Contain("\"groups\"");
        json.Should().Contain("dev-team");
    }

    [Fact]
    public void Build_DisabledRealm_ShouldIncludeEnabledFalse()
    {
        // Arrange
        var seed = new DisabledRealmSeed("disabled-realm");

        // Act
        var json = KeycloakRealmJsonBuilder.Build(seed);

        // Assert
        json.Should().Contain("\"enabled\":false");
    }

    // ─── Test Helper Seeds ─────────────────────────────────────

    private sealed class MinimalRealmSeed(string realmName) : IKeycloakRealmSeed
    {
        public string RealmName => realmName;
    }

    private sealed class SeedWithRoles(string realmName) : IKeycloakRealmSeed
    {
        public string RealmName => realmName;
        public IReadOnlyList<KeycloakRoleConfig> Roles =>
        [
            new("admin", "Administrator role"),
            new("user", "Standard user role")
        ];
    }

    private sealed class SeedWithClients(string realmName) : IKeycloakRealmSeed
    {
        public string RealmName => realmName;
        public IReadOnlyList<KeycloakClientConfig> Clients =>
        [
            new("my-api", ClientSecret: "secret", AddAudienceMapper: true)
        ];
    }

    private sealed class SeedWithClientsNoAudience(string realmName) : IKeycloakRealmSeed
    {
        public string RealmName => realmName;
        public IReadOnlyList<KeycloakClientConfig> Clients =>
        [
            new("my-api", ClientSecret: "secret", AddAudienceMapper: false)
        ];
    }

    private sealed class SeedWithUsers(string realmName, string? password) : IKeycloakRealmSeed
    {
        public string RealmName => realmName;
        public IReadOnlyList<KeycloakUserConfig> Users =>
        [
            new("testuser", "test@test.com", TemporaryPassword: password)
        ];
    }

    private sealed class SeedWithGroups(string realmName) : IKeycloakRealmSeed
    {
        public string RealmName => realmName;
        public IReadOnlyList<KeycloakGroupConfig> Groups =>
        [
            new("dev-team", ["admin"])
        ];
    }

    private sealed class DisabledRealmSeed(string realmName) : IKeycloakRealmSeed
    {
        public string RealmName => realmName;
        public bool Enabled => false;
    }
}
