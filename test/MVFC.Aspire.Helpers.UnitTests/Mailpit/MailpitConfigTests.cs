using MVFC.Aspire.Helpers.Mailpit.Models;

namespace MVFC.Aspire.Helpers.UnitTests.Mailpit;

public class MailpitConfigTests {
    [Fact]
    public void DefaultValues_ShouldBeCorrect() {
        var config = new MailpitConfig();

        config.HttpPort.Should().BeNull();
        config.SmtpPort.Should().BeNull();
        config.MaxMessages.Should().Be(500);
        config.DataFilePath.Should().BeNull();
        config.SmtpAuthAcceptAny.Should().BeTrue();
        config.SmtpAuthAllowInsecure.Should().BeTrue();
        config.EnableWebAuth.Should().BeFalse();
        config.WebAuthUsername.Should().BeNull();
        config.WebAuthPassword.Should().BeNull();
        config.ImageName.Should().Be("axllent/mailpit");
        config.ImageTag.Should().Be("latest");
        config.VerboseLogging.Should().BeFalse();
        config.MaxMessageSize.Should().Be(50);
        config.SmtpHostname.Should().BeNull();
    }

    [Fact]
    public void CustomValues_ShouldOverrideDefaults() {
        var config = new MailpitConfig(
            HttpPort: 8080,
            SmtpPort: 1025,
            MaxMessages: 1000,
            DataFilePath: "/data/mailpit.db",
            SmtpAuthAcceptAny: false,
            SmtpAuthAllowInsecure: false,
            EnableWebAuth: true,
            WebAuthUsername: "admin",
            WebAuthPassword: "secret",
            ImageName: "custom/mailpit",
            ImageTag: "v2",
            VerboseLogging: true,
            MaxMessageSize: 100,
            SmtpHostname: "mail.example.com");

        config.HttpPort.Should().Be(8080);
        config.SmtpPort.Should().Be(1025);
        config.MaxMessages.Should().Be(1000);
        config.DataFilePath.Should().Be("/data/mailpit.db");
        config.SmtpAuthAcceptAny.Should().BeFalse();
        config.SmtpAuthAllowInsecure.Should().BeFalse();
        config.EnableWebAuth.Should().BeTrue();
        config.WebAuthUsername.Should().Be("admin");
        config.WebAuthPassword.Should().Be("secret");
        config.ImageName.Should().Be("custom/mailpit");
        config.ImageTag.Should().Be("v2");
        config.VerboseLogging.Should().BeTrue();
        config.MaxMessageSize.Should().Be(100);
        config.SmtpHostname.Should().Be("mail.example.com");
    }

    [Fact]
    public void Equality_ShouldWorkCorrectly() {
        var config1 = new MailpitConfig(MaxMessages: 200);
        var config2 = new MailpitConfig(MaxMessages: 200);

        config1.Should().Be(config2);
    }
}
