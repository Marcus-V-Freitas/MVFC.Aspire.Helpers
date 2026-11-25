namespace MVFC.Aspire.Helpers.Keycloak;

/// <summary>
/// Default container, endpoint, authentication and import settings for Keycloak.
/// </summary>
internal static class KeycloakDefaults
{
    /// <summary>
    /// Default Keycloak container image.
    /// </summary>
    internal const string DEFAULT_IMAGE = "quay.io/keycloak/keycloak";

    /// <summary>
    /// Default Keycloak container image tag.
    /// </summary>
    internal const string DEFAULT_TAG = "26.1";

    /// <summary>
    /// HTTP port used by Keycloak inside the container.
    /// </summary>
    internal const int HOST_PORT = 8081;

    /// <summary>
    /// Environment variable containing the administrator username.
    /// </summary>
    internal const string ADMIN_USERNAME_ENV = "KEYCLOAK_ADMIN";

    /// <summary>
    /// Environment variable containing the administrator password.
    /// </summary>
    internal const string ADMIN_PASSWORD_ENV = "KEYCLOAK_ADMIN_PASSWORD";

    /// <summary>
    /// Default administrator username for local development.
    /// </summary>
    internal const string DEFAULT_ADMIN_USER = "admin";

    /// <summary>
    /// Default administrator password for local development.
    /// </summary>
    internal const string DEFAULT_ADMIN_PASS = "admin";

    /// <summary>
    /// Container directory containing realm import files.
    /// </summary>
    internal const string IMPORT_PATH = "/opt/keycloak/data/import";

    /// <summary>
    /// Container directory used for persistent Keycloak data.
    /// </summary>
    internal const string DATA_VOLUME_PATH = "/opt/keycloak/data";

    /// <summary>
    /// Command-line argument that starts Keycloak in development mode.
    /// </summary>
    internal const string START_DEV_ARG = "start-dev";

    /// <summary>
    /// Command-line argument that imports realm files at startup.
    /// </summary>
    internal const string IMPORT_REALM_ARG = "--import-realm";

    /// <summary>
    /// Consumer environment variable containing the Keycloak base URL.
    /// </summary>
    internal const string BASE_URL_ENV = "Keycloak__BaseUrl";

    /// <summary>
    /// Consumer environment variable containing the realm name.
    /// </summary>
    internal const string REALM_ENV = "Keycloak__Realm";

    /// <summary>
    /// Consumer environment variable containing the client identifier.
    /// </summary>
    internal const string CLIENT_ID_ENV = "Keycloak__ClientId";

    /// <summary>
    /// Consumer environment variable containing the client secret.
    /// </summary>
    internal const string CLIENT_SECRET_ENV = "Keycloak__ClientSecret";

    /// <summary>
    /// Management endpoint path used by the readiness health check.
    /// </summary>
    internal const string HEALTH_PATH = "/health/ready";

    /// <summary>
    /// Port used by the Keycloak management endpoint inside the container.
    /// </summary>
    internal const int MANAGEMENT_PORT = 9000;

    /// <summary>
    /// Environment variable enabling Keycloak health endpoints.
    /// </summary>
    internal const string MANAGEMENT_ENV = "KC_HEALTH_ENABLED";

    /// <summary>
    /// Environment variable selecting the HTTP port inside the Keycloak container.
    /// </summary>
    internal const string HTTP_PORT_ENV = "KC_HTTP_PORT";

    /// <summary>
    /// Command-line option selecting how realm imports handle existing realms.
    /// </summary>
    internal const string IMPORT_STRATEGY_ARG = "--spi-import-importer-file-strategy";

    /// <summary>
    /// Realm import strategy that preserves existing realms.
    /// </summary>
    internal const string IGNORE_EXISTING_IMPORT_STRATEGY = "IGNORE_EXISTING";

    /// <summary>
    /// Realm import strategy that overwrites existing realms.
    /// </summary>
    internal const string OVERWRITE_EXISTING_IMPORT_STRATEGY = "OVERWRITE_EXISTING";

    /// <summary>
    /// Aspire endpoint name for the Keycloak management interface.
    /// </summary>
    internal const string MANAGEMENT_ENDPOINT = "management";
}
