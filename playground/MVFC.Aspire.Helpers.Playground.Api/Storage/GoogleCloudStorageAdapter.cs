namespace MVFC.Aspire.Helpers.Playground.Api.Storage;

internal sealed class GoogleCloudStorageAdapter(StorageClient storageClient) : IStorageService 
{
    private readonly StorageClient _storageClient = storageClient;

    public async Task<IEnumerable<string>> ListFilesAsync(string bucketName, string? prefix = null) 
    {
        List<string> items = [];

        await foreach (var item in _storageClient.ListObjectsAsync(bucketName).ConfigureAwait(false))
            items.Add(item.Name);

        return items;
    }

    public async Task<Stream> DownloadFileAsync(string bucketName, string fileName) 
    {
        var stream = new MemoryStream();
        await _storageClient.DownloadObjectAsync(bucketName, fileName, stream).ConfigureAwait(false);
        stream.Position = 0;
        return stream;
    }

    public async Task DeleteFileAsync(string bucketName, string fileName) =>
        await _storageClient.DeleteObjectAsync(bucketName, fileName).ConfigureAwait(false);
}
