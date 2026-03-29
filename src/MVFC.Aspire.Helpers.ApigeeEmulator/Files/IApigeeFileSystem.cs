namespace MVFC.Aspire.Helpers.ApigeeEmulator.Files;

/// <summary>
/// Abstraction for file system operations to allow unit testing and extensibility.
/// Provides methods for file and directory manipulation, as well as zip archive creation.
/// </summary>
internal interface IApigeeFileSystem
{
    /// <summary>
    /// Determines whether the specified file exists.
    /// </summary>
    /// <param name="path">The file path to check.</param>
    /// <returns><c>true</c> if the file exists; otherwise, <c>false</c>.</returns>
    internal bool FileExists(string? path);

    /// <summary>
    /// Deletes the specified file.
    /// </summary>
    /// <param name="path">The path of the file to delete.</param>
    internal void FileDelete(string path);

    /// <summary>
    /// Reads all text from the specified file.
    /// </summary>
    /// <param name="path">The path of the file to read.</param>
    /// <returns>The contents of the file as a string.</returns>
    internal string FileReadAllText(string path);

    /// <summary>
    /// Asynchronously writes text to a file, overwriting if it exists.
    /// </summary>
    /// <param name="path">The file path to write to.</param>
    /// <param name="contents">The text to write to the file.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    internal Task FileWriteAllTextAsync(string path, string contents);

    /// <summary>
    /// Opens an existing file for reading.
    /// </summary>
    /// <param name="path">The path of the file to open.</param>
    /// <returns>A read-only file stream.</returns>
    internal Stream FileOpenRead(string path);

    /// <summary>
    /// Copies an existing file to a new location, optionally overwriting the destination file.
    /// </summary>
    /// <param name="source">The file to copy.</param>
    /// <param name="dest">The destination file path.</param>
    /// <param name="overwrite">Whether to overwrite the destination file if it exists.</param>
    internal void FileCopy(string source, string dest, bool overwrite);

    /// <summary>
    /// Determines whether the specified directory exists.
    /// </summary>
    /// <param name="path">The directory path to check.</param>
    /// <returns><c>true</c> if the directory exists; otherwise, <c>false</c>.</returns>
    internal bool DirectoryExists(string path);

    /// <summary>
    /// Deletes the specified directory and, if indicated, any subdirectories and files.
    /// </summary>
    /// <param name="path">The directory to delete.</param>
    /// <param name="recursive">Whether to delete subdirectories and files.</param>
    internal void DirectoryDelete(string path, bool recursive);

    /// <summary>
    /// Creates all directories and subdirectories in the specified path.
    /// </summary>
    /// <param name="path">The directory path to create.</param>
    internal void DirectoryCreateDirectory(string path);

    /// <summary>
    /// Returns the names of files in a specified directory that match a search pattern and search option.
    /// </summary>
    /// <param name="path">The directory to search.</param>
    /// <param name="searchPattern">The search string to match against the names of files.</param>
    /// <param name="searchOption">Specifies whether to search only the current directory or all subdirectories.</param>
    /// <returns>An array of file names that match the search pattern.</returns>
    internal string[] DirectoryGetFiles(string path, string searchPattern, SearchOption searchOption);

    /// <summary>
    /// Asynchronously creates a zip archive from the specified directory.
    /// </summary>
    /// <param name="source">The directory to archive.</param>
    /// <param name="destination">The path of the resulting zip file.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    internal Task ZipCreateFromDirectoryAsync(string source, string destination);
}
