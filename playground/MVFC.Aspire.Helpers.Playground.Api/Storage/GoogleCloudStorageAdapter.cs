namespace MVFC.Aspire.Helpers.Playground.Api.Storage;

public sealed class GoogleCloudStorageAdapter(StorageClient storageClient) : IStorageService {
    private readonly StorageClient _storageClient = storageClient;

    public async Task<IEnumerable<string>> ListFilesAsync(string bucketName, string? prefix = null) {
        List<string> items = [];

        await foreach (var item in _storageClient.ListObjectsAsync(bucketName)) items.Add(item.Name);

        return items;
    }

    public async Task<Stream> DownloadFileAsync(string bucketName, string fileName) {
        var stream = new MemoryStream();
        await _storageClient.DownloadObjectAsync(bucketName, fileName, stream);
        stream.Position = 0;
        return stream;
    }

    public async Task DeleteFileAsync(string bucketName, string fileName) =>
        await _storageClient.DeleteObjectAsync(bucketName, fileName);
}