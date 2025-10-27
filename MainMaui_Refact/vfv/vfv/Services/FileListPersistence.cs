using vfv.Models;
using vfv.Services.Persistence;
using vfv.Services.Models;

namespace vfv.Services;

/// <summary>
/// MAUI-specific wrapper for FileListPersistence that handles FileItem to FileItemDto conversion
/// </summary>
public class FileListPersistence
{
    private readonly vfv.Services.Persistence.FileListPersistence _persistence;

    /// <summary>
    /// Default constructor for production use - uses MAUI FileSystem
    /// </summary>
    public FileListPersistence()
    {
        _persistence = new vfv.Services.Persistence.FileListPersistence(FileSystem.AppDataDirectory);
    }

    /// <summary>
    /// Constructor with custom storage directory - primarily for testing
    /// </summary>
    /// <param name="storageDirectory">Directory where files should be persisted</param>
    public FileListPersistence(string storageDirectory)
    {
        _persistence = new vfv.Services.Persistence.FileListPersistence(storageDirectory);
    }

    public async Task SaveFileListAsync(IEnumerable<FileItem> files)
    {
        var fileDtos = files.Select(f => new FileItemDto
        {
            Id = f.Id,
            Name = f.Name,
            Extension = f.Extension,
            Description = f.Description,
            Size = f.Size,
            Modified = f.Modified,
            FilePath = f.FilePath,
            IsSelected = f.IsSelected
        });

        await _persistence.SaveFileListAsync(fileDtos);
    }

    public async Task<List<FileItem>> LoadFileListAsync()
    {
        var fileDtos = await _persistence.LoadFileListAsync();

        return fileDtos.Select(dto => new FileItem
        {
            Id = dto.Id,
            Name = dto.Name,
            Extension = dto.Extension,
            Description = dto.Description,
            Size = dto.Size,
            Modified = dto.Modified,
            FilePath = dto.FilePath,
            IsSelected = dto.IsSelected
        }).ToList();
    }

    public async Task ClearStorageAsync()
    {
        await _persistence.ClearStorageAsync();
    }

    public bool HasPersistedData()
    {
        return _persistence.HasPersistedData();
    }
}