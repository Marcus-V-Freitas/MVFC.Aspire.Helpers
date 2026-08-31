namespace MVFC.Aspire.Helpers.GcpBigQuery.Models;

/// <summary>
/// Represents the BigQuery configuration for a GCP project.
/// </summary>
public sealed record class BigQueryConfig
{
    /// <summary>
    /// Initializes a new instance of <see cref="BigQueryConfig"/>.
    /// </summary>
    /// <param name="projectId">GCP project ID used by BigQuery.</param>
    /// <param name="dataset">Optional default dataset name.</param>
    public BigQueryConfig(string projectId, string? dataset = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectId);
        ProjectId = projectId;
        Dataset = dataset;
    }

    /// <summary>
    /// GCP project ID used by BigQuery.
    /// </summary>
    public string ProjectId { get; init; }

    /// <summary>
    /// Optional default dataset name.
    /// </summary>
    public string? Dataset { get; init; }
}
