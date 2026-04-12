namespace MVFC.Aspire.Helpers.Tests.Unit.GcpFirestore;

public sealed class FirestoreDefaultsTests
{
    [Fact]
    public void Constants_ShouldHaveExpectedValues()
    {
        FirestoreDefaults.EMULATOR_PORT.Should().Be(8080);
        FirestoreDefaults.DEFAULT_EXTERNAL_PORT.Should().Be(8084);
        FirestoreDefaults.EMULATOR_IMAGE.Should().Be("mtlynch/firestore-emulator");
        FirestoreDefaults.EMULATOR_IMAGE_TAG.Should().Be("latest");
        FirestoreDefaults.EMULATOR_HOST_ENV_VAR.Should().Be("FIRESTORE_EMULATOR_HOST");
        FirestoreDefaults.GCP_PROJECT_IDS_ENV_VAR.Should().Be("Firestore__ProjectIds");
        FirestoreDefaults.CREATION_DELIMITER.Should().Be(',');
    }
}
