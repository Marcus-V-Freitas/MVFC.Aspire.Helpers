namespace MVFC.Aspire.Helpers.CloudStorage;

/// <summary>
/// Default configuration constants for Cloud Storage (GCS emulator).
/// </summary>
internal static class CloudStorageDefaults
{
    /// <summary>Default port for the GCS emulator.</summary>
    public const int HOST_PORT = 4443;

    /// <summary>Default Docker image for the GCS emulator.</summary>
    public const string DEFAULT_IMAGE = "fsouza/fake-gcs-server";

    /// <summary>Default Docker image tag for the GCS emulator.</summary>
    public const string DEFAULT_IMAGE_TAG = "latest";

    /// <summary>
    /// Digest of the default image validated by the integration suite.
    /// </summary>
    internal const string DEFAULT_IMAGE_SHA256 = "797ce226d62f947c009dc40246b30cfb456b8473d8241407f9d6f2c04e4d69ef";

    /// <summary>Environment variable name used to configure the GCS emulator host.</summary>
    public const string STORAGE_EMULATOR_VARIABLE_NAME = "STORAGE_EMULATOR_HOST";
}
