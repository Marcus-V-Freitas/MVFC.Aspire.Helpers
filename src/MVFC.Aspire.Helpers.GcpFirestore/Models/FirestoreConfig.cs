namespace MVFC.Aspire.Helpers.GcpFirestore.Models;

/// <summary>
/// Represents the Firestore configuration for a GCP project.
/// </summary>
public sealed record class FirestoreConfig
{
    /// <summary>
    /// Initializes a new instance of <see cref="FirestoreConfig"/>.
    /// </summary>
    /// <param name="projectId">GCP project ID used by Firestore.</param>
    public FirestoreConfig(string projectId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectId);
        ProjectId = projectId;
    }

    /// <summary>
    /// GCP project ID used by Firestore.
    /// </summary>
    public string ProjectId { get; init; }
}