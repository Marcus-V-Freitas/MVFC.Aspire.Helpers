namespace MVFC.Aspire.Helpers.Playground.Api.Endpoints;

public static class BigQueryEndpoints
{
    public static void MapBigQueryEndpoints(this IEndpointRouteBuilder apiGroup)
    {
        apiGroup.MapGet("/bigquery/datasets", async (BigQueryClient client) =>
        {
            var datasets = await client.ListDatasetsAsync().ReadPageAsync(10).ConfigureAwait(false);
            return Results.Ok(datasets.Select(d => d.Reference.DatasetId));
        });

        apiGroup.MapGet("/bigquery/users", async (BigQueryClient client) =>
        {
            const string sql = "SELECT id, name FROM `test-project.test_dataset.users`";
            var results = await client.ExecuteQueryAsync(sql, parameters: null).ConfigureAwait(false);
            var users = results.Select(row => new { Id = (string)row["id"], Name = (string)row["name"] });
            return Results.Ok(users);
        });
    }
}
