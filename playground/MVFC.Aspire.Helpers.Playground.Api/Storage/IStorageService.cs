namespace MVFC.Aspire.Helpers.Playground.Api.Storage;

internal interface IStorageService 
{
    internal Task<IEnumerable<string>> ListFilesAsync(string bucketName, string? prefix = null);

    internal Task<Stream> DownloadFileAsync(string bucketName, string fileName);

    internal Task DeleteFileAsync(string bucketName, string fileName);
}
