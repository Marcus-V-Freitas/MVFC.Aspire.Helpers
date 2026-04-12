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
    /// <param name="secondsDelay">(Optional) Startup delay in seconds. Default: 5.</param>
    public FirestoreConfig(string projectId, int secondsDelay = 5)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectId);
        ProjectId = projectId;
        UpDelay = TimeSpan.FromSeconds(secondsDelay);
    }

    /// <summary>
    /// GCP project ID used by Firestore.
    /// </summary>
    public string ProjectId { get; init; }

    /// <summary>
    /// Startup delay for Firestore resource initialization.
    /// </summary>
    public TimeSpan UpDelay { get; init; }
}