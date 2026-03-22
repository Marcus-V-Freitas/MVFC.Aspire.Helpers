namespace MVFC.Aspire.Helpers.GcpSpanner.Models;

/// <summary>
/// Configuration for a Spanner instance + database to be provisioned in the emulator.
/// </summary>
public sealed record SpannerConfig(
    string ProjectId,
    string InstanceId,
    string DatabaseId,
    IReadOnlyList<string>? DdlStatements = null)
{
    /// <summary>
    /// DDL statements to execute after database creation (CREATE TABLE, etc).
    /// </summary>
    public IReadOnlyList<string> DdlStatements { get; init; } =
        DdlStatements ?? [];
}
