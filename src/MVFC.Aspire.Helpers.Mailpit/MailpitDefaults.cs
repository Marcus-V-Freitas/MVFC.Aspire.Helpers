namespace MVFC.Aspire.Helpers.Mailpit;

/// <summary>
/// Default values for Mailpit configuration.
/// </summary>
public static class MailpitDefaults
{
    /// <summary>Default Mailpit Docker image.</summary>
    public const string DEFAULT_IMAGE = "axllent/mailpit";

    /// <summary>Default Mailpit Docker image tag.</summary>
    public const string DEFAULT_IMAGE_TAG = "latest";

    /// <summary>Default Mailpit web interface HTTP port.</summary>
    public const int DEFAULT_HTTP_PORT = 8025;

    /// <summary>Default Mailpit SMTP port.</summary>
    public const int DEFAULT_SMTP_PORT = 1025;

    /// <summary>Default maximum number of stored messages.</summary>
    public const int DEFAULT_MAX_MESSAGES = 500;

    /// <summary>Environment variable for the maximum number of messages.</summary>
    public const string MAX_MESSAGES_ENV_VAR = "MP_MAX_MESSAGES";

    /// <summary>Environment variable for the maximum message size (in bytes).</summary>
    public const string MAX_MESSAGE_SIZE_ENV_VAR = "MP_MAX_MESSAGE_SIZE";

    /// <summary>Environment variable to accept any SMTP credential.</summary>
    public const string SMTP_AUTH_ACCEPT_ANY_ENV_VAR = "MP_SMTP_AUTH_ACCEPT_ANY";

    /// <summary>Environment variable to allow insecure SMTP authentication.</summary>
    public const string SMTP_AUTH_ALLOW_INSECURE_ENV_VAR = "MP_SMTP_AUTH_ALLOW_INSECURE";

    /// <summary>Environment variable for the SMTP server hostname.</summary>
    public const string SMTP_HOSTNAME_ENV_VAR = "MP_SMTP_HOSTNAME";

    /// <summary>Environment variable for the persistent data file path.</summary>
    public const string DATA_FILE_ENV_VAR = "MP_DATA_FILE";

    /// <summary>Internal path of the Mailpit data file in the container.</summary>
    public const string DATA_FILE_PATH = "/data/mailpit.db";

    /// <summary>Volume mount path for data in the container.</summary>
    public const string VOLUME_MOUNT_PATH = "/data";

    /// <summary>Environment variable for the web interface username.</summary>
    public const string UI_AUTH_USERNAME_ENV_VAR = "MP_UI_AUTH_USERNAME";

    /// <summary>Environment variable for the web interface password.</summary>
    public const string UI_AUTH_PASSWORD_ENV_VAR = "MP_UI_AUTH_PASSWORD";

    /// <summary>Environment variable to enable verbose logging.</summary>
    public const string VERBOSE_ENV_VAR = "MP_VERBOSE";

    /// <summary>Boolean true value for Mailpit environment variables.</summary>
    public const string TRUE_VALUE = "1";

    /// <summary>Boolean false value for Mailpit environment variables.</summary>
    public const string FALSE_VALUE = "0";
}
