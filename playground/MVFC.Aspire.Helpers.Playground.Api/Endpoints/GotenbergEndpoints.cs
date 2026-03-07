namespace MVFC.Aspire.Helpers.Playground.Api.Endpoints; 

public static class GotenbergEndpoints 
{
    public static void MapGotenbergEndpoints(this IEndpointRouteBuilder apiGroup) =>
        apiGroup.MapPost("/pdf", async (HtmlRequest request, IGotenbergService gotenberg, CancellationToken ct) =>
        {
            var bytes = await gotenberg.HtmlToPdfAsync(request.Html, ct);

            return Results.File(bytes, "application/pdf", "relatorio.pdf");
        });
}