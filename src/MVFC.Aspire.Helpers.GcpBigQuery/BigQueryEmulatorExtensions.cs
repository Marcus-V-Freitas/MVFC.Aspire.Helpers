namespace MVFC.Aspire.Helpers.GcpBigQuery;

/// <summary>
/// Extension methods to register the Google Cloud BigQuery emulator in Aspire.
/// </summary>
public static class BigQueryEmulatorExtensions
{
    /// <summary>
    /// Adds the Google Cloud BigQuery emulator to the distributed application.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="name">The resource name.</param>
    /// <param name="restPort">The port to expose the REST emulator endpoint. Default is BigQueryDefaults.DEFAULT_EXTERNAL_REST_PORT.</param>
    /// <param name="grpcPort">The port to expose the gRPC emulator endpoint. Default is BigQueryDefaults.DEFAULT_EXTERNAL_GRPC_PORT.</param>
    /// <returns>A resource builder for the BigQuery emulator resource.</returns>
    public static IResourceBuilder<BigQueryEmulatorResource> AddGcpBigQuery(
        this IDistributedApplicationBuilder builder,
        string name,
        int restPort = BigQueryDefaults.DEFAULT_EXTERNAL_REST_PORT,
        int grpcPort = BigQueryDefaults.DEFAULT_EXTERNAL_GRPC_PORT)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfLessThan(restPort, IPEndPoint.MinPort);
        ArgumentOutOfRangeException.ThrowIfLessThan(grpcPort, IPEndPoint.MinPort);

        var resource = new BigQueryEmulatorResource(name);

        return builder.AddResource(resource)
                      .WithDockerImage(
                          image: BigQueryDefaults.EMULATOR_IMAGE,
                          tag: BigQueryDefaults.EMULATOR_IMAGE_TAG)
                      .WithBigQueryEndpoints(restPort, grpcPort);
    }

    /// <summary>
    /// Adds multiple BigQuery project configurations to the emulator container arguments.
    /// </summary>
    /// <param name="builder">The resource builder for the BigQuery emulator.</param>
    /// <param name="configs">BigQuery project configurations to add.</param>
    /// <returns>The resource builder for chaining.</returns>
    public static IResourceBuilder<BigQueryEmulatorResource> WithBigQueryConfigs(
        this IResourceBuilder<BigQueryEmulatorResource> builder,
        params BigQueryConfig[] configs)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configs);

        builder.Resource.BigQueryConfigs = [.. builder.Resource.BigQueryConfigs, .. configs];

        foreach (var config in configs)
        {
            builder.WithArgs($"--project={config.ProjectId}");

            if (!string.IsNullOrWhiteSpace(config.Dataset))
            {
                builder.WithArgs($"--dataset={config.Dataset}");
            }
        }

        return builder;
    }

    /// <summary>
    /// Seeds the BigQuery emulator with initial data/schemas using a YAML file.
    /// This will bind mount the YAML file into the container and configure the emulator to use it.
    /// </summary>
    /// <param name="builder">The resource builder for the BigQuery emulator.</param>
    /// <param name="yamlFilePath">The path to the local YAML file.</param>
    /// <returns>The resource builder for chaining.</returns>
    public static IResourceBuilder<BigQueryEmulatorResource> WithDataSeed(
        this IResourceBuilder<BigQueryEmulatorResource> builder,
        string yamlFilePath)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(yamlFilePath);

        const string containerPath = "/data/data.yaml";

        builder.WithBindMount(yamlFilePath, containerPath, isReadOnly: true)
               .WithArgs($"--data-from-yaml={containerPath}");

        return builder;
    }

    /// <summary>
    /// Replaces the Docker image used by the BigQuery resource.
    /// </summary>
    /// <param name="builder">The resource builder for the BigQuery emulator.</param>
    /// <param name="image">The Docker image name.</param>
    /// <param name="tag">The Docker image tag.</param>
    /// <returns>The resource builder for chaining.</returns>
    public static IResourceBuilder<BigQueryEmulatorResource> WithDockerImage(
        this IResourceBuilder<BigQueryEmulatorResource> builder,
        string image,
        string tag)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(image);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(tag);

        return builder.WithImage(image)
                      .WithImageTag(tag);
    }

    /// <summary>
    /// Adds a reference to the BigQuery resource in the project, injecting
    /// BIGQUERY_EMULATOR_HOST as an environment variable.
    /// </summary>
    /// <param name="project">The project resource builder.</param>
    /// <param name="bigQuery">The BigQuery emulator resource builder.</param>
    /// <returns>The project resource builder for chaining.</returns>
    public static IResourceBuilder<ProjectResource> WithReference(
        this IResourceBuilder<ProjectResource> project,
        IResourceBuilder<BigQueryEmulatorResource> bigQuery)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(bigQuery);

        project.WithReference(source: bigQuery)
               .WithEnvironment(
                   BigQueryDefaults.EMULATOR_HOST_ENV_VAR,
                   bigQuery.Resource.ConnectionStringExpression)
               .WithEnvironment(
                   BigQueryDefaults.EMULATOR_GRPC_HOST_ENV_VAR,
                   bigQuery.Resource.GrpcConnectionStringExpression);

        return project;
    }

    /// <summary>
    /// Configures the HTTP and gRPC endpoints of the BigQuery emulator.
    /// </summary>
    private static IResourceBuilder<BigQueryEmulatorResource> WithBigQueryEndpoints(
        this IResourceBuilder<BigQueryEmulatorResource> resource,
        int restPort,
        int grpcPort)
    {
        return resource.WithHttpEndpoint(
                           port: restPort,
                           targetPort: BigQueryDefaults.EMULATOR_REST_PORT,
                           name: BigQueryEmulatorResource.REST_ENDPOINT_NAME,
                           isProxied: true)
                       .WithEndpoint(
                           port: grpcPort,
                           targetPort: BigQueryDefaults.EMULATOR_GRPC_PORT,
                           name: BigQueryEmulatorResource.GRPC_ENDPOINT_NAME,
                           isProxied: true);
    }
}
