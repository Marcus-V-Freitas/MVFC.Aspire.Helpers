namespace MVFC.Aspire.Helpers.ApigeeEmulator.Files;

/// <summary>
/// Default implementation of <see cref="IApigeeFileSystem"/> using <see cref="System.IO"/>.
/// </summary>
internal sealed class DefaultApigeeFileSystem : IApigeeFileSystem
{
    public bool FileExists(string? path) => File.Exists(path);
    public void FileDelete(string path) => File.Delete(path);
    public string FileReadAllText(string path) => File.ReadAllText(path);
    public void FileCopy(string source, string dest, bool overwrite) => File.Copy(source, dest, overwrite);
    public Stream FileOpenRead(string path) => File.OpenRead(path);
    public async Task FileWriteAllTextAsync(string path, string contents) => await File.WriteAllTextAsync(path, contents).ConfigureAwait(false);
    public bool DirectoryExists(string path) => Directory.Exists(path);
    public void DirectoryDelete(string path, bool recursive) => Directory.Delete(path, recursive);
    public void DirectoryCreateDirectory(string path) => Directory.CreateDirectory(path);
    public string[] DirectoryGetFiles(string path, string searchPattern, SearchOption searchOption) => Directory.GetFiles(path, searchPattern, searchOption);
    public async Task ZipCreateFromDirectoryAsync(string source, string destination) =>
        await Task.Run(() => ZipFile.CreateFromDirectory(source, destination, CompressionLevel.Optimal, includeBaseDirectory: false)).ConfigureAwait(false);
}
