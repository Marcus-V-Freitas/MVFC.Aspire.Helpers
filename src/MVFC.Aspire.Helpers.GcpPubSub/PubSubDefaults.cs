namespace MVFC.Aspire.Helpers.GcpPubSub;

/// <summary>
/// Constantes de configuração para Google Pub/Sub
/// </summary>
internal static class PubSubDefaults {
    public const int HostPort = 8681;
    public const int UIPort = 8680;
    public const int AckDeadlineSecondsDefault = 300;
    public const int MaxDeliveryAttemptsDefault = 5;
    public const int WaitTimeoutSecondsDefault = 15;
    public const char CreationDelimiter = ',';
    public const char SubscriptionDelimiter = ':';
    public const char DockerImageDelimiter = ':';
}
