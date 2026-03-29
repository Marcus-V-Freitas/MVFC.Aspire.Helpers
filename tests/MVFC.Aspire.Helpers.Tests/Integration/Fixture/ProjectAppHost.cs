namespace MVFC.Aspire.Helpers.Tests.Integration.Fixture;

internal sealed class ProjectAppHost() : DistributedApplicationFactory(typeof(MVFC_Aspire_Helpers_Playground_AppHost))
{
    protected override void OnBuilderCreating(DistributedApplicationOptions applicationOptions, HostApplicationBuilderSettings hostOptions)
    {
        applicationOptions.AllowUnsecuredTransport = true;
        hostOptions.Args = ["--testmode=true"];
    }

    protected override void OnBuilderCreated(DistributedApplicationBuilder builder)
    {
        builder.Services.ConfigureHttpClientDefaults(http =>
            http.AddStandardResilienceHandler(ConfigureResilience));
    }

    /// <summary>
    /// Configures default timeouts and Circuit Breaker for integration tests.
    /// </summary>
    private static void ConfigureResilience(HttpStandardResilienceOptions options)
    {
        var baseTimeout = TimeSpan.FromMinutes(2);

        options.AttemptTimeout.Timeout = baseTimeout;
        options.CircuitBreaker.SamplingDuration = baseTimeout * 2;
        options.TotalRequestTimeout.Timeout = baseTimeout * 3;

        // Local function to share the 429 rule between Retry and Circuit Breaker
        static ValueTask<bool> ShouldHandleOutcome(Polly.Outcome<HttpResponseMessage> outcome)
        {
            // If it's our 429 from Spike Arrest, DO NOT retry and DO NOT open the circuit.
            // Let it pass directly for the test to evaluate.
            if (outcome.Result?.StatusCode == HttpStatusCode.TooManyRequests)
            {
                return ValueTask.FromResult(false);
            }

            // For other errors (500+, RequestTimeout or network exceptions),
            // keep Aspire's transient failure behavior.
            var isTransientError = outcome.Exception is not null ||
                                   (outcome.Result is not null &&
                                   ((int)outcome.Result.StatusCode >= 500 ||
                                     outcome.Result.StatusCode == HttpStatusCode.RequestTimeout));

            return ValueTask.FromResult(isTransientError);
        }

        // Now extract the Outcome from the specific arguments of each strategy
        options.Retry.ShouldHandle = args => ShouldHandleOutcome(args.Outcome);
        options.CircuitBreaker.ShouldHandle = args => ShouldHandleOutcome(args.Outcome);
    }
}
