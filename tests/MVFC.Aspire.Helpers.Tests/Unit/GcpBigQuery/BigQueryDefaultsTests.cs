namespace MVFC.Aspire.Helpers.Tests.Unit.GcpBigQuery;

public sealed class BigQueryDefaultsTests
{
    [Fact]
    public void Constants_ShouldHaveExpectedValues()
    {
        BigQueryDefaults.EMULATOR_IMAGE.Should().Be("ghcr.io/goccy/bigquery-emulator");
        BigQueryDefaults.EMULATOR_IMAGE_TAG.Should().Be("latest");
        BigQueryDefaults.EMULATOR_REST_PORT.Should().Be(9050);
        BigQueryDefaults.DEFAULT_EXTERNAL_REST_PORT.Should().Be(9050);
        BigQueryDefaults.EMULATOR_GRPC_PORT.Should().Be(9060);
        BigQueryDefaults.DEFAULT_EXTERNAL_GRPC_PORT.Should().Be(9060);
        BigQueryDefaults.EMULATOR_HOST_ENV_VAR.Should().Be("BIGQUERY_EMULATOR_HOST");
        BigQueryDefaults.EMULATOR_GRPC_HOST_ENV_VAR.Should().Be("BIGQUERY_EMULATOR_GRPC_HOST");
    }
}
