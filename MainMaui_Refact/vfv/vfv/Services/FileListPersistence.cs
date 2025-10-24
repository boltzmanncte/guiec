using System.Text.Json;
using vfv.Models;

namespace vfv.Services;

public class FileListPersistence
{
    private readonly string _storageFilePath;
    private const string StorageFileName = "fileList.json";

    /// <summary>
    /// Default constructor for production use - uses MAUI FileSystem
    /// </summary>
    public FileListPersistence()
        : this(Path.Combine(FileSystem.AppDataDirectory, StorageFileName))
    {
    }

    /// <summary>
    /// Constructor with custom storage path - primarily for testing
    /// </summary>
    /// <param name="storageFilePath">Full path to the storage file</param>
    public FileListPersistence(string storageFilePath)
    {
        _storageFilePath = storageFilePath;
    }

    public async Task SaveFileListAsync(IEnumerable<FileItem> files)
    {
        try
        {
            var fileData = files.Select(f => new PersistedFileItem
            {
                Id = f.Id,
                Name = f.Name,
                Extension = f.Extension,
                Description = f.Description,
                Size = f.Size,
                Modified = f.Modified,
                FilePath = f.FilePath,
                IsSelected = f.IsSelected
            }).ToList();

            var json = JsonSerializer.Serialize(fileData, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(_storageFilePath, json);
        }
        catch (Exception ex)
        {
            // Log error but don't throw - persistence failure shouldn't crash the app
            System.Diagnostics.Debug.WriteLine($"Error saving file list: {ex.Message}");
        }
    }

    public async Task<List<FileItem>> LoadFileListAsync()
    {
        try
        {
            if (!File.Exists(_storageFilePath))
            {
                return new List<FileItem>();
            }

            var json = await File.ReadAllTextAsync(_storageFilePath);
            var fileData = JsonSerializer.Deserialize<List<PersistedFileItem>>(json);

            if (fileData == null)
            {
                return new List<FileItem>();
            }

            var fileItems = new List<FileItem>();

            foreach (var data in fileData)
            {
                // Verify the file still exists
                if (File.Exists(data.FilePath))
                {
                    // Update file info in case it changed
                    var fileInfo = new FileInfo(data.FilePath);

                    var fileItem = new FileItem
                    {
                        Id = data.Id,
                        Name = data.Name,
                        Extension = data.Extension,
                        Description = data.Description,
                        Size = FormatFileSize(fileInfo.Length),
                        Modified = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm"),
                        FilePath = data.FilePath,
                        IsSelected = false // Reset selection state
                    };

                    fileItems.Add(fileItem);
                }
            }

            return fileItems;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading file list: {ex.Message}");
            return new List<FileItem>();
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

public class PersistedFileItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Modified { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}