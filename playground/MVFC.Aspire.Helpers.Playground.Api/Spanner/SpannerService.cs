namespace MVFC.Aspire.Helpers.Playground.Api.Spanner;

internal sealed class SpannerService(SpannerConnection spannerConnection) : ISpannerService
{
    private readonly SpannerConnection _spannerConnection = spannerConnection;

    public async Task<object?> PingAsync()
    {
        await _spannerConnection.OpenAsync().ConfigureAwait(false);

        await using var cmd = _spannerConnection.CreateSelectCommand("SELECT 1");
        return await cmd.ExecuteScalarAsync().ConfigureAwait(false);
    }

    public async Task CreateUserAsync(Guid id, string name)
    {
        await _spannerConnection.OpenAsync().ConfigureAwait(false);

        await using var cmd = _spannerConnection.CreateInsertCommand("Users",
            new SpannerParameterCollection
            {
                { "UserId", SpannerDbType.String, id.ToString() },
                { "Name", SpannerDbType.String, name },
                { "CreatedAt", SpannerDbType.Timestamp, SpannerParameter.CommitTimestamp },
            });

        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<object>> GetAllUsers()
    {
        await _spannerConnection.OpenAsync().ConfigureAwait(false);

        await using var cmd = _spannerConnection.CreateSelectCommand("SELECT UserId, Name FROM Users LIMIT 50");

        await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

        var users = new List<object>();
        while (await reader.ReadAsync().ConfigureAwait(false))
            users.Add(new { UserId = reader.GetString(0), Name = reader.GetString(1) });

        return users;
    }
}
