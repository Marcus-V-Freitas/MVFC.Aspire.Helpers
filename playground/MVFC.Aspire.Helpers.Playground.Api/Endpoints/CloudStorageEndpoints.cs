namespace MVFC.Aspire.Helpers.Playground.Api.Endpoints;

public static class CloudStorageEndpoints {
    public static void MapCloudStorageEndpoints(this IEndpointRouteBuilder apiGroup) =>
        apiGroup.MapGet("/bucket/{bucketName}", async (string bucketName, IStorageService storageClient) => {
            try {
                var files = await storageClient.ListFilesAsync(bucketName);

                foreach (var file in files) {
                    await using var fileStream = await storageClient.DownloadFileAsync(bucketName, file);

                    using (var reader = new StreamReader(fileStream)) {
                        var content = await reader.ReadToEndAsync();
                    }

                    await storageClient.DeleteFileAsync(bucketName, file);
                }

                return Results.Ok();
            }
            catch (Exception ex) {
                return Results.Problem($"Erro ao listar arquivos: {ex.Message}");
            }
        });
}