namespace MVFC.Aspire.Helpers.Tests.Unit.GcpFirestore;

public sealed class FirestoreDefaultsTests
{
    [Fact]
    public void Constants_ShouldHaveExpectedValues()
    {
        FirestoreDefaults.EMULATOR_PORT.Should().Be(8080);
        FirestoreDefaults.EMULATOR_IMAGE.Should().Be("mtlynch/firestore-emulator");
        FirestoreDefaults.EMULATOR_IMAGE_TAG.Should().Be("latest");
        FirestoreDefaults.EMULATOR_HOST_ENV_VAR.Should().Be("FIRESTORE_EMULATOR_HOST");
        FirestoreDefaults.GCP_PROJECT_IDS_ENV_VAR.Should().Be("Firestore__ProjectIds");
        FirestoreDefaults.WAIT_TIMEOUT_SECONDS_DEFAULT.Should().Be(15);
        FirestoreDefaults.CREATION_DELIMITER.Should().Be(',');
    }

    [Fact]
    public void DockerInternalHost_ShouldReturnCorrectValue_BasedOnOS()
    {
        var expected = OperatingSystem.IsLinux() ? "172.17.0.1" : "host.docker.internal";
        FirestoreDefaults.DockerInternalHost.Should().Be(expected);
    }
}
