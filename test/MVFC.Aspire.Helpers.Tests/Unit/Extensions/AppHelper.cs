namespace MVFC.Aspire.Helpers.Tests.Unit.Extensions;

internal static class AppHelper
{
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
}
