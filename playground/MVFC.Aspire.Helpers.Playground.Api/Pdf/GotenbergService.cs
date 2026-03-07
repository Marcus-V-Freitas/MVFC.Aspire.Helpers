namespace MVFC.Aspire.Helpers.Playground.Api.Pdf;

internal sealed class GotenbergService(IGotenbergApi api) : IGotenbergService 
{
    public async Task<byte[]> HtmlToPdfAsync(string html, CancellationToken ct) 
    {
        var htmlPart = new ByteArrayPart(System.Text.Encoding.UTF8.GetBytes(html), "index.html", "text/html");
        var response = await api.HtmlToPdfAsync(htmlPart, ct);

        return await ConvertStreamToArray(response, ct);
    }

    private static async Task<byte[]> ConvertStreamToArray(ApiResponse<Stream> response, CancellationToken ct) 
    {
        using var ms = new MemoryStream();
        await response.Content!.CopyToAsync(ms, ct);
        return ms.ToArray();
    }
}