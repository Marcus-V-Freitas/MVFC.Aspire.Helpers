namespace MVFC.Aspire.Helpers.Tests.Unit.Mailpit;

public sealed class MailpitExtensionsTests
{
    [Fact]
    public void AddMailpit_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IDistributedApplicationBuilder? builder = null;

        // Act
        var act = () => builder!.AddMailpit("mailpit");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddMailpit_ShouldThrow_WhenNameIsNullOrWhitespace(string? name)
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var act = () => builder.AddMailpit(name!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddMailpit_ShouldSetResourceName()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var mp = builder.AddMailpit("mailpit");

        // Assert
        mp.Resource.Name.Should().Be("mailpit");
    }

    [Fact]
    public void WithDockerImage_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<MailpitResource>? builder = null;

        // Act
        var act = () => builder!.WithDockerImage("axllent/mailpit", "latest");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null, "latest")]
    [InlineData("", "latest")]
    [InlineData(" ", "latest")]
    [InlineData("axllent/mailpit", null)]
    [InlineData("axllent/mailpit", "")]
    [InlineData("axllent/mailpit", " ")]
    public void WithDockerImage_ShouldThrow_WhenImageOrTagInvalid(string? image, string? tag)
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var mp = appBuilder.AddMailpit("mailpit");

        // Act
        var act = () => mp.WithDockerImage(image!, tag!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithMaxMessages_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<MailpitResource>? builder = null;

        // Act
        var act = () => builder!.WithMaxMessages(100);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithMaxMessages_ShouldThrow_WhenValueIsNegative()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var mp = appBuilder.AddMailpit("mailpit");

        // Act
        var act = () => mp.WithMaxMessages(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0, "0")]
    [InlineData(100, "100")]
    [InlineData(500, "500")]
    public async Task WithMaxMessages_ShouldSetCorrectEnvVar(int max, string expected)
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var mp = appBuilder.AddMailpit("mailpit").WithMaxMessages(max);

        // Act
        var envVars = await appBuilder.GetEnvs(mp.Resource);

        // Assert
        envVars[MailpitDefaults.MAX_MESSAGES_ENV_VAR].Should().Be(expected);
    }

    [Fact]
    public void WithMaxMessageSize_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<MailpitResource>? builder = null;

        // Act
        var act = () => builder!.WithMaxMessageSize(10);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithMaxMessageSize_ShouldThrow_WhenValueIsNegative()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var mp = appBuilder.AddMailpit("mailpit");

        // Act
        var act = () => mp.WithMaxMessageSize(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(1, "1048576")]
    [InlineData(10, "10485760")]
    [InlineData(25, "26214400")]
    public async Task WithMaxMessageSize_ShouldSetEnvVarInBytes(int sizeMb, string expectedBytes)
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var mp = appBuilder.AddMailpit("mailpit").WithMaxMessageSize(sizeMb);

        // Act
        var envVars = await appBuilder.GetEnvs(mp.Resource);

        // Assert
        envVars[MailpitDefaults.MAX_MESSAGE_SIZE_ENV_VAR].Should().Be(expectedBytes);
    }

    [Fact]
    public void WithSmtpAuth_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<MailpitResource>? builder = null;

        // Act
        var act = () => builder!.WithSmtpAuth();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(true, true, "1", "1")]
    [InlineData(false, true, "0", "1")]
    [InlineData(true, false, "1", "0")]
    [InlineData(false, false, "0", "0")]
    public async Task WithSmtpAuth_ShouldSetCorrectEnvVars(
        bool acceptAny, bool allowInsecure,
        string expectedAcceptAny, string expectedInsecure)
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var mp = appBuilder.AddMailpit("mailpit").WithSmtpAuth(acceptAny, allowInsecure);

        // Act
        var envVars = await appBuilder.GetEnvs(mp.Resource);

        // Assert
        envVars[MailpitDefaults.SMTP_AUTH_ACCEPT_ANY_ENV_VAR].Should().Be(expectedAcceptAny);
        envVars[MailpitDefaults.SMTP_AUTH_ALLOW_INSECURE_ENV_VAR].Should().Be(expectedInsecure);
    }

    [Fact]
    public void WithSmtpHostname_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<MailpitResource>? builder = null;

        // Act
        var act = () => builder!.WithSmtpHostname("mail.local");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void WithSmtpHostname_ShouldThrow_WhenHostnameIsInvalid(string? hostname)
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var mp = appBuilder.AddMailpit("mailpit");

        // Act
        var act = () => mp.WithSmtpHostname(hostname!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task WithSmtpHostname_ShouldSetCorrectEnvVar()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var mp = appBuilder.AddMailpit("mailpit").WithSmtpHostname("smtp.acme.local");

        // Act
        var envVars = await appBuilder.GetEnvs(mp.Resource);

        // Assert
        envVars[MailpitDefaults.SMTP_HOSTNAME_ENV_VAR].Should().Be("smtp.acme.local");
    }

    [Fact]
    public void WithDataFile_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<MailpitResource>? builder = null;

        // Act
        var act = () => builder!.WithDataFile("/data/mailpit.db");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void WithDataFile_ShouldThrow_WhenPathIsInvalid(string? path)
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var mp = appBuilder.AddMailpit("mailpit");

        // Act
        var act = () => mp.WithDataFile(path!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task WithDataFile_ShouldSetDataFileEnvVarToInternalContainerPath()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var mp = appBuilder.AddMailpit("mailpit").WithDataFile("/host/mailpit.db");

        // Act
        var envVars = await appBuilder.GetEnvs(mp.Resource);

        // Assert
        envVars[MailpitDefaults.DATA_FILE_ENV_VAR].Should().Be(MailpitDefaults.DATA_FILE_PATH);
    }

    [Fact]
    public void WithWebAuth_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<MailpitResource>? builder = null;

        // Act
        var act = () => builder!.WithWebAuth("user", "pass");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null, "pass")]
    [InlineData("", "pass")]
    [InlineData(" ", "pass")]
    [InlineData("user", null)]
    [InlineData("user", "")]
    [InlineData("user", " ")]
    public void WithWebAuth_ShouldThrow_WhenUsernameOrPasswordIsInvalid(string? user, string? pass)
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var mp = appBuilder.AddMailpit("mailpit");

        // Act
        var act = () => mp.WithWebAuth(user!, pass!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task WithWebAuth_ShouldSetUsernameAndPasswordEnvVars()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var mp = appBuilder.AddMailpit("mailpit").WithWebAuth("adminuser", "S3cr3t!");

        // Act
        var envVars = await appBuilder.GetEnvs(mp.Resource);

        // Assert
        envVars[MailpitDefaults.UI_AUTH_USERNAME_ENV_VAR].Should().Be("adminuser");
        envVars[MailpitDefaults.UI_AUTH_PASSWORD_ENV_VAR].Should().Be("S3cr3t!");
    }

    [Fact]
    public void WithVerboseLogging_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<MailpitResource>? builder = null;

        // Act
        var act = () => builder!.WithVerboseLogging();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(true, "1")]
    [InlineData(false, "0")]
    public async Task WithVerboseLogging_ShouldSetCorrectEnvVar(bool enable, string expected)
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var mp = appBuilder.AddMailpit("mailpit").WithVerboseLogging(enable);

        // Act
        var envVars = await appBuilder.GetEnvs(mp.Resource);

        // Assert
        envVars[MailpitDefaults.VERBOSE_ENV_VAR].Should().Be(expected);
    }
}
