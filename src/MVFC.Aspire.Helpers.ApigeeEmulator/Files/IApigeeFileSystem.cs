namespace MVFC.Aspire.Helpers.ApigeeEmulator.Files;

/// <summary>
/// Abstraction for file system operations to allow unit testing.
/// </summary>
internal interface IApigeeFileSystem
{
    public bool FileExists(string? path);
    public void FileDelete(string path);
    public string FileReadAllText(string path);
    public Task FileWriteAllTextAsync(string path, string contents);
    public Stream FileOpenRead(string path);
    public void FileCopy(string source, string dest, bool overwrite);
    public bool DirectoryExists(string path);
    public void DirectoryDelete(string path, bool recursive);
    public void DirectoryCreateDirectory(string path);
    public string[] DirectoryGetFiles(string path, string searchPattern, SearchOption searchOption);
    public Task ZipCreateFromDirectoryAsync(string source, string destination);
}
