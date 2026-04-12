namespace MVFC.Aspire.Helpers.GcpFirestore;

/// <summary>
/// Default configuration constants for Google Firestore.
/// </summary>
internal static class FirestoreDefaults
{
    /// <summary>
    /// Default internal port used by the Firestore emulator container.
    /// </summary>
    internal const int EMULATOR_PORT = 8080;

    /// <summary>
    /// Default external port mapped to the Firestore emulator.
    /// </summary>
    internal const int DEFAULT_EXTERNAL_PORT = 8084;

    /// <summary>
    /// Official Docker image for the Firestore emulator (via firebase-tools).
    /// </summary>
    internal const string EMULATOR_IMAGE = "mtlynch/firestore-emulator";

    /// <summary>
    /// Docker image tag for the Firestore emulator.
    /// </summary>
    internal const string EMULATOR_IMAGE_TAG = "latest";

    /// <summary>
    /// Environment variable name for the Firestore emulator host.
    /// </summary>
    public const string EMULATOR_HOST_ENV_VAR = "FIRESTORE_EMULATOR_HOST";

    /// <summary>
    /// Environment variable name for the delimited list of GCP Project IDs.
    /// </summary>
    public const string GCP_PROJECT_IDS_ENV_VAR = "Firestore__ProjectIds";

    /// <summary>
    /// Delimiter used for separating project IDs in environment variables.
    /// </summary>
    internal const char CREATION_DELIMITER = ',';
}