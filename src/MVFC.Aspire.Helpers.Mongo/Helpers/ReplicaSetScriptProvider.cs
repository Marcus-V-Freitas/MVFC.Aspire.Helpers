namespace MVFC.Aspire.Helpers.Mongo.Helpers;

/// <summary>
/// Provider for Replica Set initialization scripts.
/// </summary>
internal static class ReplicaSetScriptProvider
{
    /// <summary>
    /// Loads and returns the content of the Replica Set initialization script from the assembly's embedded resources.
    /// </summary>
    public static string GetInitScript()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "MVFC.Aspire.Helpers.Mongo.Resources.init-replica-set.js";

        using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException($"Could not load embedded resource '{resourceName}'. Ensure it is configured as EmbeddedResource.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
