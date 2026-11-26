namespace MVFC.Aspire.Helpers.Tests.Integration.Fixture;

internal sealed class ProjectAppHost() : DistributedApplicationFactory(typeof(MVFC_Aspire_Helpers_Playground_AppHost))
{
    /// <summary>
    /// Application captured before startup readiness is awaited.
    /// </summary>
    private DistributedApplication _application = default!;

    /// <summary>
    /// Captures the application so functional tests can wait for real HTTP readiness.
    /// </summary>
    protected override void OnBuilt(DistributedApplication application) => _application = application;

    /// <summary>
    /// Waits for the resource checks under the caller's startup budget.
    /// </summary>
    internal Task WaitForHealthyAsync(string resourceName, CancellationToken cancellationToken) =>
        _application.ResourceNotifications.WaitForResourceHealthyAsync(resourceName, cancellationToken);

    protected override void OnBuilderCreating(DistributedApplicationOptions applicationOptions, HostApplicationBuilderSettings hostOptions)
    {
        applicationOptions.AllowUnsecuredTransport = true;
        hostOptions.Args = ["--testmode=true"];
    }

    protected override void OnBuilderCreated(DistributedApplicationBuilder builder)
    {
        // Aspire's test clients use the default name; health probes use dedicated names.
        FunctionalHttpResilience.AddClient(builder.Services);
    }
}
