namespace MVFC.Aspire.Helpers.Tests.Integration.Fixture;

/// <summary>
/// Applies bounded retries to functional test clients without affecting Aspire probes.
/// </summary>
internal static class FunctionalHttpResilience
{
    /// <summary>
    /// Configures only the default client used by Aspire's functional test requests.
    /// </summary>
    internal static IHttpClientBuilder AddClient(IServiceCollection services)
    {
        var client = services.AddHttpClient(string.Empty);
        client.AddStandardResilienceHandler(Configure);
        return client;
    }

    /// <summary>
    /// Keeps standard HTTP deadlines and retries transient failures of safe methods only.
    /// </summary>
    internal static void Configure(HttpStandardResilienceOptions options)
    {
        options.Retry.ShouldHandle = args => ValueTask.FromResult(
            !args.Context.CancellationToken.IsCancellationRequested &&
            CanRetry(HttpResilienceContextExtensions.GetRequestMessage(args.Context)) &&
            IsTransient(args.Outcome));
        options.Retry.DisableForUnsafeHttpMethods();
        options.CircuitBreaker.ShouldHandle = args => ValueTask.FromResult(
            !args.Context.CancellationToken.IsCancellationRequested && IsTransient(args.Outcome));
    }

    /// <summary>
    /// Excludes legacy playground GET routes that publish, consume, write or delete data.
    /// The HTTP contracts remain unchanged; a lost response must not repeat their effects.
    /// </summary>
    private static bool CanRetry(HttpRequestMessage? request)
    {
        if (request?.RequestUri is not { IsAbsoluteUri: true } uri)
            return false;

        var path = Uri.UnescapeDataString(uri.AbsolutePath).TrimEnd('/');

        return !path.Equals("/api/pub-sub-enter", StringComparison.OrdinalIgnoreCase) &&
               !path.StartsWith("/api/rabbitmq/consume/", StringComparison.OrdinalIgnoreCase) &&
               !path.StartsWith("/api/redis/set/", StringComparison.OrdinalIgnoreCase) &&
               !path.StartsWith("/api/bucket/", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Preserves observable throttling responses and excludes permanent or caller cancellation errors.
    /// </summary>
    private static bool IsTransient(Polly.Outcome<HttpResponseMessage> outcome) =>
        outcome.Exception is HttpRequestException or Polly.Timeout.TimeoutRejectedException ||
        outcome.Result is { StatusCode: HttpStatusCode.RequestTimeout } ||
        outcome.Result is { } response && (int)response.StatusCode >= 500;
}
