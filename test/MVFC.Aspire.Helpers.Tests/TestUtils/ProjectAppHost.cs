namespace MVFC.Aspire.Helpers.Tests.TestUtils; 

internal class ProjectAppHost() : DistributedApplicationFactory(typeof(MVFC_Aspire_Helpers_Playground_AppHost)) {

    protected override void OnBuilderCreating(DistributedApplicationOptions applicationOptions, HostApplicationBuilderSettings hostOptions) {
        applicationOptions.AllowUnsecuredTransport = true;
        hostOptions.Args = ["--testmode=true"];
    }

    protected override void OnBuilderCreated(DistributedApplicationBuilder builder) =>
        builder.Services.ConfigureHttpClientDefaults(x => x.AddStandardResilienceHandler(c => {
            var timeSpan = TimeSpan.FromMinutes(2);
            c.AttemptTimeout.Timeout = timeSpan;
            c.CircuitBreaker.SamplingDuration = timeSpan * 2;
            c.TotalRequestTimeout.Timeout = timeSpan * 3;
        }));
}