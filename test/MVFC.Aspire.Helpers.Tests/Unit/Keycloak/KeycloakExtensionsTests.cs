namespace MVFC.Aspire.Helpers.Tests.Unit.Keycloak;

public sealed class KeycloakExtensionsTests
{
    [Fact]
    public void AddKeycloak_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IDistributedApplicationBuilder? builder = null;

        // Act
        var act = () => builder!.AddKeycloak("keycloak");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddKeycloak_ShouldThrow_WhenNameIsNullOrWhitespace(string? name)
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var act = () => builder.AddKeycloak(name!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddKeycloak_ShouldConfigureResource_WithDefaultCredentials()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var kcBuilder = builder.AddKeycloak("keycloak");

        // Assert
        kcBuilder.Should().NotBeNull();
        kcBuilder.Resource.Name.Should().Be("keycloak");
        kcBuilder.Resource.AdminUsername.Should().NotBeNullOrWhiteSpace();
        kcBuilder.Resource.AdminPassword.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void WithDockerImage_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<KeycloakResource>? builder = null;

        // Act
        var act = () => builder!.WithDockerImage("keycloak", "25");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null, "25")]
    [InlineData("", "25")]
    [InlineData(" ", "25")]
    [InlineData("keycloak/keycloak", null)]
    [InlineData("keycloak/keycloak", "")]
    [InlineData("keycloak/keycloak", " ")]
    public void WithDockerImage_ShouldThrow_WhenImageOrTagInvalid(string? image, string? tag)
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var kcBuilder = appBuilder.AddKeycloak("keycloak");

        // Act
        var act = () => kcBuilder.WithDockerImage(image!, tag!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithDockerImage_ShouldNotThrow_WithValidParameters()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var kcBuilder = appBuilder.AddKeycloak("keycloak");

        // Act
        var act = () => kcBuilder.WithDockerImage("quay.io/keycloak/keycloak", "25.0");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void WithAdminCredentials_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<KeycloakResource>? builder = null;

        // Act
        var act = () => builder!.WithAdminCredentials("admin", "pass");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null, "pass")]
    [InlineData("", "pass")]
    [InlineData(" ", "pass")]
    [InlineData("admin", null)]
    [InlineData("admin", "")]
    [InlineData("admin", " ")]
    public void WithAdminCredentials_ShouldThrow_WhenUsernameOrPasswordInvalid(string? user, string? pass)
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var kcBuilder = appBuilder.AddKeycloak("keycloak");

        // Act
        var act = () => kcBuilder.WithAdminCredentials(user!, pass!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithAdminCredentials_ShouldUpdateResource()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var kcBuilder = appBuilder.AddKeycloak("keycloak");

        // Act
        kcBuilder.WithAdminCredentials("superadmin", "Sup3r$ecret!");

        // Assert
        kcBuilder.Resource.AdminUsername.Should().Be("superadmin");
        kcBuilder.Resource.AdminPassword.Should().Be("Sup3r$ecret!");
    }

    [Fact]
    public void WithSeeds_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<KeycloakResource>? builder = null;

        // Act
        var act = () => builder!.WithSeeds([]);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithSeeds_ShouldThrow_WhenSeedsIsNull()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var kcBuilder = appBuilder.AddKeycloak("keycloak");

        // Act
        var act = () => kcBuilder.WithSeeds(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithSeeds_ShouldNotThrow_WithEmptySeeds()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var kcBuilder = appBuilder.AddKeycloak("keycloak");

        // Act
        var act = () => kcBuilder.WithSeeds([]);

        // Assert
        act.Should().NotThrow();
        kcBuilder.Resource.Seeds.Should().NotBeNull();
        kcBuilder.Resource.Seeds.Should().BeEmpty();
    }

    [Fact]
    public void WithSeeds_ShouldRegisterSeedsOnResource()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var kcBuilder = appBuilder.AddKeycloak("keycloak");

        IReadOnlyCollection<IKeycloakRealmSeed> seeds =
        [
            new StubRealmSeed("my-app"),
            new StubRealmSeed("another-app")
        ];

        // Act
        kcBuilder.WithSeeds(seeds);

        // Assert
        kcBuilder.Resource.Seeds.Should().HaveCount(2);
        kcBuilder.Resource.Seeds!.Select(s => s.RealmName)
            .Should().BeEquivalentTo(["my-app", "another-app"]);
    }

    [Fact]
    public void WithImportStrategy_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<KeycloakResource>? builder = null;

        // Act
        var act = () => builder!.WithImportStrategy();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(KeycloakImportStrategy.IgnoreExisting)]
    [InlineData(KeycloakImportStrategy.OverwriteExisting)]
    public void WithImportStrategy_ShouldNotThrow_ForAnyValidStrategy(KeycloakImportStrategy strategy)
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var kcBuilder = appBuilder.AddKeycloak("keycloak");

        // Act
        var act = () => kcBuilder.WithImportStrategy(strategy);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void WithDataVolume_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<KeycloakResource>? builder = null;

        // Act
        var act = () => builder!.WithDataVolume("kc-data");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void WithDataVolume_ShouldThrow_WhenVolumeNameInvalid(string? volumeName)
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var kcBuilder = appBuilder.AddKeycloak("keycloak");

        // Act
        var act = () => kcBuilder.WithDataVolume(volumeName!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithDataVolume_ShouldNotThrow_WhenVolumeNameIsValid()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var kcBuilder = appBuilder.AddKeycloak("keycloak");

        // Act
        var act = () => kcBuilder.WithDataVolume("key-cloak-data");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void WithReference_ShouldThrow_WhenProjectIsNull()
    {
        // Arrange
        IResourceBuilder<ProjectResource>? project = null;
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var kcBuilder = appBuilder.AddKeycloak("keycloak");

        // Act
        var act = () => project!.WithReference(kcBuilder, "my-realm", "my-client");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithReference_ShouldThrow_WhenKeycloakBuilderIsNull()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var project = appBuilder.AddProject<Projects.MVFC_Aspire_Helpers_Playground_Api>("api");
        IResourceBuilder<KeycloakResource>? kcBuilder = null;

        // Act
        var act = () => project.WithReference(kcBuilder!, "my-realm", "my-client");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null, "client-id")]
    [InlineData("", "client-id")]
    [InlineData(" ", "client-id")]
    [InlineData("my-realm", null)]
    [InlineData("my-realm", "")]
    [InlineData("my-realm", " ")]
    public void WithReference_ShouldThrow_WhenRealmOrClientIdInvalid(string? realm, string? clientId)
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var project = appBuilder.AddProject<Projects.MVFC_Aspire_Helpers_Playground_Api>("api");
        var kcBuilder = appBuilder.AddKeycloak("keycloak");

        // Act
        var act = () => project.WithReference(kcBuilder, realm!, clientId!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithReference_ShouldNotThrow_WithValidParameters()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var project = appBuilder.AddProject<Projects.MVFC_Aspire_Helpers_Playground_Api>("api");
        var kcBuilder = appBuilder.AddKeycloak("keycloak");

        // Act
        var act = () => project.WithReference(kcBuilder, "my-realm", "my-client", "secret-123");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void WithReference_ShouldNotThrow_WithoutClientSecret()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var project = appBuilder.AddProject<Projects.MVFC_Aspire_Helpers_Playground_Api>("api");
        var kcBuilder = appBuilder.AddKeycloak("keycloak");

        // Act
        var act = () => project.WithReference(kcBuilder, "my-realm", "my-client");

        // Assert
        act.Should().NotThrow();
    }
}
