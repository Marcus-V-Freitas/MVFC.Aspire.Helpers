namespace MVFC.Aspire.Helpers.Keycloak;

internal static class KeycloakDefaults
{    
    internal const string DEFAULT_IMAGE = "quay.io/keycloak/keycloak";
    internal const string DEFAULT_TAG = "26.1";
    internal const int HOST_PORT = 8080;
    internal const string ADMIN_USERNAME_ENV = "KEYCLOAK_ADMIN";
    internal const string ADMIN_PASSWORD_ENV = "KEYCLOAK_ADMIN_PASSWORD";
    internal const string DEFAULT_ADMIN_USER = "admin";
    internal const string DEFAULT_ADMIN_PASS = "admin";
    internal const string IMPORT_PATH = "/opt/keycloak/data/import";
    internal const string DATA_VOLUME_PATH = "/opt/keycloak/data";
    internal const string START_DEV_ARG = "start-dev";
    internal const string IMPORT_REALM_ARG = "--import-realm";
    internal const string BASE_URL_ENV = "Keycloak__BaseUrl";
    internal const string REALM_ENV = "Keycloak__Realm";
    internal const string CLIENT_ID_ENV = "Keycloak__ClientId";
    internal const string CLIENT_SECRET_ENV = "Keycloak__ClientSecret";
    internal const string HEALTH_PATH = "/health/ready";
    internal const int MANAGEMENT_PORT = 9000;
    internal const string MANAGEMENT_ENV = "KC_HEALTH_ENABLED";
    internal const string MANAGEMENT_ENDPOINT = "management";
}
