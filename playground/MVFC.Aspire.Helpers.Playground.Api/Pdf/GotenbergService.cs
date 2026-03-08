namespace MVFC.Aspire.Helpers.Playground.Api.Pdf;

internal sealed class GotenbergService(IGotenbergApi api) : IGotenbergService 
{
    public async Task<byte[]> HtmlToPdfAsync(string html, CancellationToken ct) 
    {
        var htmlPart = new ByteArrayPart(System.Text.Encoding.UTF8.GetBytes(html), "index.html", "text/html");
        var response = await api.HtmlToPdfAsync(htmlPart, ct).ConfigureAwait(false);

        return await ConvertStreamToArray(response, ct).ConfigureAwait(false);
    }

    private static async Task<byte[]> ConvertStreamToArray(ApiResponse<Stream> response, CancellationToken ct) 
    {
        using var ms = new MemoryStream();
        await response.Content!.CopyToAsync(ms, ct).ConfigureAwait(false);
        return ms.ToArray();
    }
}
