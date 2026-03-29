namespace MVFC.Aspire.Helpers.Tests.Unit.Fakers;

internal sealed class FakeHostApplicationLifetime : IHostApplicationLifetime
{
    public CancellationToken ApplicationStarted { get; } = new();
    public CancellationToken ApplicationStopping { get; } = new();
    public CancellationToken ApplicationStopped { get; } = new();

    public void StopApplication()
    {
    }
}
