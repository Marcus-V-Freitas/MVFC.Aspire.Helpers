namespace MVFC.Aspire.Helpers.Playground.Api.Spanner;

internal interface ISpannerService
{
    internal Task<object?> PingAsync();

    internal Task CreateUserAsync(Guid id, string name);

    internal Task<IReadOnlyList<object>> GetAllUsers();
}
