namespace MVFC.Aspire.Helpers.Tests.Unit.Extensions;

internal static class AppHelper
{
    internal static async Task<Dictionary<string, object>> GetConfiguredEnvironmentAsync(
        this IDistributedApplicationBuilder builder, IResource resource, CancellationToken ct)
    {
        var environment = new Dictionary<string, object>();
        var context = new EnvironmentCallbackContext(builder.ExecutionContext, resource, environment, ct);

        foreach (var annotation in resource.Annotations.OfType<EnvironmentCallbackAnnotation>())
            await annotation.Callback(context).ConfigureAwait(false);

        return environment;
    }

    internal static async Task<Dictionary<string, string>> GetEnvs(this IDistributedApplicationBuilder builder, IResource resource)
    {
        using var app = builder.Build();

        var executionContext = app.Services.GetRequiredService<DistributedApplicationExecutionContext>();
        var executionConfig = await ExecutionConfigurationBuilder
                                        .Create(resource)
                                        .WithEnvironmentVariablesConfig()
                                        .BuildAsync(executionContext)
                                        .ConfigureAwait(true);

        return executionConfig.EnvironmentVariables.ToDictionary();
    }

    internal static ResourceNotificationService GetResourceNotificationService()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddSingleton<ResourceLoggerService>();

        var provider = services.BuildServiceProvider();
        var lifetime = new FakeHostApplicationLifetime();

        var resourceLogger = provider.GetRequiredService<ResourceLoggerService>();

        return new ResourceNotificationService(
            NullLogger<ResourceNotificationService>.Instance,
            lifetime,
            provider,
            resourceLogger);
    }
}
