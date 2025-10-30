namespace MVFC.Aspire.Helpers.Playground.Api.Endpoints;

public static class DefaultEndpoints {
    public static void MapAllEndpoints(this IEndpointRouteBuilder app) {
        var apiGroup = app.MapGroup("/api");

        apiGroup.MapCloudStorageEndpoints();
        apiGroup.MapMongoEndpoints();
        apiGroup.MapPubSubEndpoints();
    }
}