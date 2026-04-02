namespace MVFC.Aspire.Helpers.Tests.Unit.Fakers;

internal sealed class FakeHostApplicationLifetime : IHostApplicationLifetime
{
    public CancellationToken ApplicationStarted { get; } = CancellationToken.None;
    public CancellationToken ApplicationStopping { get; } = CancellationToken.None;
    public CancellationToken ApplicationStopped { get; } = CancellationToken.None;

    public void StopApplication()
    {
    }
}
