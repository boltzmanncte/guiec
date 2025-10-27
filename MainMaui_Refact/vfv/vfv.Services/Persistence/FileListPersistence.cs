using System.Text.Json;
using vfv.Services.Models;

namespace vfv.Services.Persistence;

public class FileListPersistence
{
    private readonly string _storageFilePath;
    private const string StorageFileName = "fileList.json";

    /// <summary>
    /// Default constructor for production use - requires storage path to be provided
    /// </summary>
    /// <param name="storageDirectory">Directory where the file list should be persisted</param>
    public FileListPersistence(string storageDirectory)
    {
        _storageFilePath = Path.Combine(storageDirectory, StorageFileName);
    }

    public async Task SaveFileListAsync(IEnumerable<FileItemDto> files)
    {
        try
        {
            var json = JsonSerializer.Serialize(files, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var directory = Path.GetDirectoryName(_storageFilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(_storageFilePath, json);
        }
        catch (Exception ex)
        {
            // Log error but don't throw - persistence failure shouldn't crash the app
            System.Diagnostics.Debug.WriteLine($"Error saving file list: {ex.Message}");
        }
    }

    public async Task<List<FileItemDto>> LoadFileListAsync()
    {
        try
        {
            if (!File.Exists(_storageFilePath))
            {
                return new List<FileItemDto>();
            }

            var json = await File.ReadAllTextAsync(_storageFilePath);
            var fileData = JsonSerializer.Deserialize<List<FileItemDto>>(json);

            if (fileData == null)
            {
                return new List<FileItemDto>();
            }

            var fileItems = new List<FileItemDto>();

            foreach (var data in fileData)
            {
                // Verify the file still exists
                if (File.Exists(data.FilePath))
                {
                    // Update file info in case it changed
                    var fileInfo = new FileInfo(data.FilePath);

                    data.Size = FormatFileSize(fileInfo.Length);
                    data.Modified = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm");
                    data.IsSelected = false; // Reset selection state

                    fileItems.Add(data);
                }
            }

            return fileItems;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading file list: {ex.Message}");
            return new List<FileItemDto>();
        }
    }

    public async Task ClearStorageAsync()
    {
        try
        {
            if (File.Exists(_storageFilePath))
            {
                File.Delete(_storageFilePath);
            }
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error clearing storage: {ex.Message}");
        }
    }

    public bool HasPersistedData()
    {
        return File.Exists(_storageFilePath);
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.#} {sizes[order]}";
    }
}