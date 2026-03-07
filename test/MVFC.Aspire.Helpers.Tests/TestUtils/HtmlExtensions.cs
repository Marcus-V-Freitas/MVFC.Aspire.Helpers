namespace MVFC.Aspire.Helpers.Tests.TestUtils; 

public static class HtmlExtensions 
{
    public static async Task<string> ExtractHtmlTemplateAsync() 
    {
        var assembly = typeof(AppHostTests).Assembly;
        var resourceName = "MVFC.Aspire.Helpers.Tests.Templates.Relatorio.html";

        await using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
    }
}
