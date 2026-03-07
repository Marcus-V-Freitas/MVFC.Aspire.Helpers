namespace MVFC.Aspire.Helpers.RabbitMQ;

/// <summary>
/// Default values for RabbitMQ configuration.
/// </summary>
public static class RabbitMQDefaults
{
    /// <summary>Default RabbitMQ Docker image.</summary>
    public const string DEFAULT_RABBIT_MQ_IMAGE = "rabbitmq";

    /// <summary>Default RabbitMQ Docker image tag (with Management UI).</summary>
    public const string DEFAULT_RABBIT_MQ_TAG = "3-management";

    /// <summary>Default RabbitMQ AMQP port.</summary>
    public const int DEFAULT_AMQP_PORT = 5672;

    /// <summary>Default RabbitMQ Management UI port.</summary>
    public const int DEFAULT_MANAGEMENT_PORT = 15672;

    /// <summary>Default RabbitMQ username.</summary>
    public const string DEFAULT_USERNAME = "guest";

    /// <summary>Default RabbitMQ password.</summary>
    public const string DEFAULT_PASSWORD = "guest";

    /// <summary>Default RabbitMQ connection string section in application configuration.</summary>
    public const string DEFAULT_CONNECTION_STRING_SECTION = "ConnectionStrings:rabbitmq";

    /// <summary>Environment variable for the RabbitMQ default username.</summary>
    public const string DEFAULT_USER_ENV_VAR = "RABBITMQ_DEFAULT_USER";

    /// <summary>Environment variable for the RabbitMQ default password.</summary>
    public const string DEFAULT_PASS_ENV_VAR = "RABBITMQ_DEFAULT_PASS";

    /// <summary>Environment variable for additional ERL arguments on the RabbitMQ server.</summary>
    public const string ADDITIONAL_ERL_ARGS_ENV_VAR = "RABBITMQ_SERVER_ADDITIONAL_ERL_ARGS";

    /// <summary>ERL arguments value to load the definitions.json file.</summary>
    public const string ADDITIONAL_ERL_ARGS_VALUE = "-rabbitmq_management load_definitions \"/etc/rabbitmq/definitions.json\"";

    /// <summary>Persistent data volume path for RabbitMQ in the container.</summary>
    public const string DATA_VOLUME_PATH = "/var/lib/rabbitmq";

    /// <summary>Directory path where the definitions.json file is mounted in the RabbitMQ container.</summary>
    public const string DEFINITIONS_DIR_PATH = "/etc/rabbitmq";

    /// <summary>Filename of the RabbitMQ definitions file.</summary>
    public const string DEFINITIONS_FILENAME = "definitions.json";
}
