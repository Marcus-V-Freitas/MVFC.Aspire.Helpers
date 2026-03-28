namespace MVFC.Aspire.Helpers.ApigeeEmulator.Notifications;

internal interface IApigeeNotificationService
{
    internal Task WaitForResourceAsync(string resourceName, string? targetState, CancellationToken cancellationToken = default);
}
