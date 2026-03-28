namespace MVFC.Aspire.Helpers.ApigeeEmulator.Notifications;

internal sealed class DefaultApigeeNotificationService(ResourceNotificationService inner) : IApigeeNotificationService
{
    public Task WaitForResourceAsync(string resourceName, string? targetState, CancellationToken ct = default)
        => inner.WaitForResourceAsync(resourceName, targetState, ct);
}
