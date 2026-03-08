namespace MVFC.Aspire.Helpers.Playground.Api.Contracts;

[Headers("Accept: application/pdf")]
internal interface IGotenbergApi
{
    [Multipart]
    [Post("/forms/chromium/convert/html")]
    internal Task<ApiResponse<Stream>> HtmlToPdfAsync([AliasAs("files")] ByteArrayPart indexHtml, CancellationToken ct);
}
