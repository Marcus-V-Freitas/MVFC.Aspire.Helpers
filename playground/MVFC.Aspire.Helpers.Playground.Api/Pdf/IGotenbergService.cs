namespace MVFC.Aspire.Helpers.Playground.Api.Pdf;

internal interface IGotenbergService 
{
    internal Task<byte[]> HtmlToPdfAsync(string html, CancellationToken ct);
}
