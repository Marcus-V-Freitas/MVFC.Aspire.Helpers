namespace MVFC.Aspire.Helpers.Tests.Unit.Extensions;

/// <summary>
/// Locates the sample API for unstarted ProjectResource contract tests without generated API metadata.
/// </summary>
internal sealed class TestProjectMetadata : IProjectMetadata
{
    /// <summary>
    /// Gets the API project path relative to the generated AppHost metadata.
    /// </summary>
    public string ProjectPath => Path.GetFullPath(Path.Combine(
        MVFC_Aspire_Helpers_Playground_AppHost.ProjectPath,
        "../MVFC.Aspire.Helpers.Playground.Api/MVFC.Aspire.Helpers.Playground.Api.csproj"));
}
