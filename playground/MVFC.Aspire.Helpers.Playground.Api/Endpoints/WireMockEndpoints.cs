namespace MVFC.Aspire.Helpers.Playground.Api.Endpoints;

public static class WireMockEndpoints 
{
    public static void MapWireMockEndpoints(this IEndpointRouteBuilder apiGroup) =>
      apiGroup.MapPost("/payment-callback", async (HttpRequest request) => {
          using var reader = new StreamReader(request.Body);
          var body = await reader.ReadToEndAsync();
          return Results.Ok(body);
      });
}
