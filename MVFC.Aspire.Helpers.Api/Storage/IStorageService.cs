namespace MVFC.Aspire.Helpers.Api.Storage;

public interface IStorageService {
    Task<IEnumerable<string>> ListFilesAsync(string bucketName, string? prefix = null);

    Task<Stream> DownloadFileAsync(string bucketName, string fileName);

    Task DeleteFileAsync(string bucketName, string fileName);
}