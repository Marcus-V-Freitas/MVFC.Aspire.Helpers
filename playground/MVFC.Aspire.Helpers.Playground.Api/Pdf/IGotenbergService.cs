namespace MVFC.Aspire.Helpers.Playground.Api.Pdf; 

public interface IGotenbergService 
{
    Task<byte[]> HtmlToPdfAsync(string html, CancellationToken ct);
}
