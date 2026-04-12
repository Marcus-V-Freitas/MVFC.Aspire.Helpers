namespace MVFC.Aspire.Helpers.GcpFirestore;

/// <summary>
/// Responsible for validating Firestore emulator readiness and applying project configurations at runtime.
/// </summary>
internal static class FirestoreConfigurator
{
    /// <summary>
    /// Waits for the emulator to be ready by probing the REST API health endpoint,
    /// then applies any startup delay defined per project configuration.
    /// </summary>
    /// <param name="firestoreConfigs">A list of Firestore project configurations to apply.</param>
    /// <param name="portEndpoint">The port where the Firestore emulator is running.</param>
    /// <param name="ct">A cancellation token to observe while waiting for readiness or delays.</param>
    internal static async Task ConfigureAsync(
        IReadOnlyList<FirestoreConfig> firestoreConfigs,
        int portEndpoint,
        CancellationToken ct)
    {
        using var httpClient = new HttpClient
        {
            BaseAddress = new Uri($"http://localhost:{portEndpoint.ToString(CultureInfo.InvariantCulture)}"),
        };

        await WaitForEmulatorAsync(httpClient, ct).ConfigureAwait(false);

        var maxDelay = firestoreConfigs
            .Select(c => c.UpDelay)
            .DefaultIfEmpty(TimeSpan.Zero)
            .Max();

        if (maxDelay > TimeSpan.Zero)
            await Task.Delay(maxDelay, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Polls the emulator root endpoint until it responds with a successful status code.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to send requests to the emulator.</param>
    /// <param name="ct">A cancellation token to observe while polling for readiness.</param>
    private static async Task WaitForEmulatorAsync(HttpClient httpClient, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var response = await httpClient.GetAsync("/", ct).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                    return;
            }
            catch (HttpRequestException)
            {
                // Emulator is not up yet
            }

            await Task.Delay(500, ct).ConfigureAwait(false);
        }

        ct.ThrowIfCancellationRequested();
    }
}