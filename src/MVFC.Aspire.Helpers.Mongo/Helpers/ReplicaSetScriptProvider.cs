namespace MVFC.Aspire.Helpers.Mongo.Helpers;

/// <summary>
/// Provedor de scripts para inicialização do Replica Set
/// </summary>
internal static class ReplicaSetScriptProvider
{
    public static string GetInitScript()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "MVFC.Aspire.Helpers.Mongo.Resources.init-replica-set.js";

        using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException($"Could not load embedded resource '{resourceName}'. Ensure it is configured as EmbeddedResource.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
