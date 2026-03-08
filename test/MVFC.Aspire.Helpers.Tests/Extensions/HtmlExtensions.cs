namespace MVFC.Aspire.Helpers.Tests.Extensions;

internal static class HtmlExtensions 
{
    internal static async Task<string> ExtractHtmlTemplateAsync() 
    {
        var assembly = typeof(AppHostTests).Assembly;
        var resourceName = "MVFC.Aspire.Helpers.Tests.Templates.Index.html";

        await using var stream = assembly.GetManifestResourceStream(resourceName)!;

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);
    }
}
