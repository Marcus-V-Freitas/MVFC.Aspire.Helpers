namespace MVFC.Aspire.Helpers.Tests.Unit.GcpPubSub;

/// <summary>
/// Isolates process-wide emulator environment changes from all parallel test collections.
/// </summary>
[CollectionDefinition("PubSub emulator environment", DisableParallelization = true)]
public sealed class PubSubEmulatorEnvironmentCollectionDefinition;
