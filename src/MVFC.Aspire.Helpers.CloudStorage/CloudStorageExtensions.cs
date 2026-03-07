namespace MVFC.Aspire.Helpers.CloudStorage;

/// <summary>
/// Provides extension methods to simplify the configuration and integration of a Cloud Storage resource (GCS emulator)
/// in distributed applications.
/// </summary>
public static class CloudStorageExtensions
{
    /// <summary>
    /// Adds a Cloud Storage resource (GCS emulator) to the distributed application with default settings.
    /// Use fluent methods such as WithBucketFolder to customize.
    /// </summary>
    public static IResourceBuilder<CloudStorageResource> AddCloudStorage(
        this IDistributedApplicationBuilder builder,
        string name,
        int port = CloudStorageDefaults.HOST_PORT)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(builder);

        var resource = new CloudStorageResource(name);

        return builder.AddResource(resource)
            .WithDockerImage(
                image: CloudStorageDefaults.DEFAULT_IMAGE,
                tag: CloudStorageDefaults.DEFAULT_IMAGE_TAG)
            .WithCloudStorageEndpoint(port)
            .WithArgs("--scheme", CloudStorageResource.HTTP_ENDPOINT_NAME);
    }

    /// <summary>
    /// Replaces the Docker image used by the Cloud Storage resource.
    /// </summary>
    public static IResourceBuilder<CloudStorageResource> WithDockerImage(
        this IResourceBuilder<CloudStorageResource> builder,
        string image,
        string tag)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(image);
        ArgumentNullException.ThrowIfNullOrEmpty(tag);
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithImage(image).WithImageTag(tag);
    }

    /// <summary>
    /// Configures a local folder for bucket persistence (bind mount in the container).
    /// </summary>
    public static IResourceBuilder<CloudStorageResource> WithBucketFolder(
        this IResourceBuilder<CloudStorageResource> builder,
        string localBucketFolder)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(localBucketFolder);
        ArgumentNullException.ThrowIfNull(builder);

        builder.WithBindMount(localBucketFolder, "/data");
        return builder;
    }

    /// <summary>
    /// Adds a reference to the Cloud Storage resource in the project.
    /// </summary>
    public static IResourceBuilder<ProjectResource> WithReference(
        this IResourceBuilder<ProjectResource> project,
        IResourceBuilder<CloudStorageResource> cloudStorage)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(cloudStorage);

        return project.WithReference(source: cloudStorage)
                      .WithEnvironment(CloudStorageDefaults.STORAGE_EMULATOR_VARIABLE_NAME, cloudStorage.Resource.ConnectionStringExpression);
    }

    /// <summary>
    /// Configures the HTTP endpoint for the Cloud Storage emulator.
    /// </summary>
    private static IResourceBuilder<CloudStorageResource> WithCloudStorageEndpoint(
        this IResourceBuilder<CloudStorageResource> resource, int port)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(port, IPEndPoint.MinPort);

        return resource.WithHttpEndpoint(
            port: port,
            targetPort: CloudStorageDefaults.HOST_PORT,
            name: CloudStorageResource.HTTP_ENDPOINT_NAME,
            isProxied: false);
    }
}
