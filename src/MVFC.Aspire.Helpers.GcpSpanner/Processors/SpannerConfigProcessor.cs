namespace MVFC.Aspire.Helpers.GcpSpanner.Processors;

/// <summary>
/// Responsible for provisioning Spanner instances and databases in the emulator at runtime.
/// Comunicação exclusivamente via gRPC (porta 9010) — nunca HTTP direto na 9020.
/// </summary>
internal static class SpannerConfigProcessor
{
    internal static async Task ConfigureAsync(
        IReadOnlyList<SpannerConfig> configs,
        CancellationToken ct)
    {
        var instanceAdminClient = await new InstanceAdminClientBuilder
        {
            EmulatorDetection = EmulatorDetection.EmulatorOnly,
        }.BuildAsync(ct).ConfigureAwait(false);

        var dbAdminClient = await new DatabaseAdminClientBuilder
        {
            EmulatorDetection = EmulatorDetection.EmulatorOnly,
        }.BuildAsync(ct).ConfigureAwait(false);

        foreach (var config in configs)
            await ProvisionAsync(instanceAdminClient, dbAdminClient, config, ct).ConfigureAwait(false);
    }

    private static async Task ProvisionAsync(
        InstanceAdminClient instanceAdminClient,
        DatabaseAdminClient dbAdminClient,
        SpannerConfig config,
        CancellationToken ct)
    {
        await EnsureInstanceAsync(instanceAdminClient, config, ct).ConfigureAwait(false);
        await EnsureDatabaseAsync(dbAdminClient, config, ct).ConfigureAwait(false);
    }

    private static async Task EnsureInstanceAsync(InstanceAdminClient client, SpannerConfig config, CancellationToken ct)
    {
        var instanceName = InstanceName.FromProjectInstance(config.ProjectId, config.InstanceId);

        try
        {
            await client.GetInstanceAsync(instanceName, ct).ConfigureAwait(false);
            return;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            // não existe — cria abaixo
        }

        var projectName = ProjectName.FromProject(config.ProjectId);
        var instanceConfigName = InstanceConfigName.FromProjectInstanceConfig(config.ProjectId, SpannerDefaults.EMULATOR_INSTANCE_CONFIG);

        var operation = await client.CreateInstanceAsync(
            new CreateInstanceRequest
            {
                ParentAsProjectName = projectName,
                InstanceId = config.InstanceId,
                Instance = new Instance
                {
                    ConfigAsInstanceConfigName = instanceConfigName,
                    DisplayName = config.InstanceId,
                    NodeCount = 1,
                },
            }, ct).ConfigureAwait(false);

        await operation.PollUntilCompletedAsync().ConfigureAwait(false);
    }

    private static async Task EnsureDatabaseAsync(DatabaseAdminClient client, SpannerConfig config, CancellationToken ct)
    {
        var databaseName = DatabaseName.FromProjectInstanceDatabase(config.ProjectId, config.InstanceId, config.DatabaseId);

        try
        {
            await client.GetDatabaseAsync(databaseName, ct).ConfigureAwait(false);
            return;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            // não existe — cria abaixo
        }

        var instanceName = InstanceName.FromProjectInstance(config.ProjectId, config.InstanceId);

        var operation = await client.CreateDatabaseAsync(
            new CreateDatabaseRequest
            {
                ParentAsInstanceName = instanceName,
                CreateStatement = $"CREATE DATABASE `{config.DatabaseId}`",
                ExtraStatements = { config.DdlStatements },
            }, ct).ConfigureAwait(false);

        await operation.PollUntilCompletedAsync().ConfigureAwait(false);
    }
}
