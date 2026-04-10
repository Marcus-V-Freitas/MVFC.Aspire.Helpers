namespace MVFC.Aspire.Helpers.Playground.Api.Endpoints;

public static class CloudStorageEndpoints
{
    public static void MapCloudStorageEndpoints(this IEndpointRouteBuilder apiGroup) =>
        apiGroup.MapGet("/bucket/{bucketName}", async (string bucketName, IStorageService storageClient) =>
        {
            var files = await storageClient.ListFilesAsync(bucketName).ConfigureAwait(false);

            foreach (var file in files)
            {
                var fileStream = await storageClient.DownloadFileAsync(bucketName, file).ConfigureAwait(false);

                await using(fileStream.ConfigureAwait(false))
                {
                    using var reader = new StreamReader(fileStream);
                    var content = await reader.ReadToEndAsync().ConfigureAwait(false);
                    Console.WriteLine(content);
                }

                await storageClient.DeleteFileAsync(bucketName, file).ConfigureAwait(false);
            }

            return Results.Ok();
        });
}
