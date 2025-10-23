using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using vfv.Models;
using vfv.Services;
#if WINDOWS
using Windows.Storage;
#endif

namespace vfv.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly FileCache _fileCache;
    private readonly FileListPersistence _persistence;
    private bool _isInitializing = true;

    [ObservableProperty]
    private ObservableCollection<FileItem> files = new();

    [ObservableProperty]
    private FileItem? activeFile;

    [ObservableProperty]
    private ObservableCollection<string> errors = new();

    [ObservableProperty]
    private bool isExecuting;

    [ObservableProperty]
    private bool isDeclarationMode;

    [ObservableProperty]
    private int progress;

    [ObservableProperty]
    private int selectedFilesCount;

    public MainViewModel()
    {
        _fileCache = new FileCache(maxCacheSize: 50, cacheExpirationMinutes: 30);
        _persistence = new FileListPersistence();

        // Subscribe to collection changes for future additions/removals
        Files.CollectionChanged += (s, e) =>
        {
            if (e.NewItems != null)
            {
                foreach (FileItem item in e.NewItems)
                {
                    item.PropertyChanged += FileItem_PropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (FileItem item in e.OldItems)
                {
                    item.PropertyChanged -= FileItem_PropertyChanged;
                    // Remove from cache when file is removed
                    _fileCache.Remove(item.FilePath);
                }
            }
            UpdateSelectedFilesCount();

            // Save file list when collection changes (but not during initialization)
            if (!_isInitializing)
            {
                _ = SaveFileListAsync();
            }
        };

        // Load persisted file list on startup
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        _isInitializing = true;
        try
        {
            var savedFiles = await _persistence.LoadFileListAsync();
            foreach (var file in savedFiles)
            {
                Files.Add(file);
            }
        }
        finally
        {
            _isInitializing = false;
        }
    }

    private async Task SaveFileListAsync()
    {
        await _persistence.SaveFileListAsync(Files);
    }

    [RelayCommand]
    private async Task SelectFile(FileItem file)
    {
        if (ActiveFile != null)
            ActiveFile.IsActive = false;

        file.IsActive = true;
        ActiveFile = file;

        // Load file content into cache when selected
        await LoadFileContent(file);
    }

    private async Task LoadFileContent(FileItem file)
    {
        try
        {
            // Check if content is already in cache
            if (_fileCache.TryGet(file.FilePath, out var cachedContent))
            {
                file.IsCached = true;
                return;
            }

            // Load file content from disk
            if (File.Exists(file.FilePath))
            {
                var content = await File.ReadAllTextAsync(file.FilePath);
                _fileCache.AddOrUpdate(file.FilePath, content);
                file.IsCached = true;
            }
        }
        catch (Exception ex)
        {
            Errors.Add($"Error loading file content for '{file.Name}': {ex.Message}");
            file.IsCached = false;
        }
    }

    public async Task<string?> GetFileContent(string filePath)
    {
        // Try to get from cache first
        if (_fileCache.TryGet(filePath, out var content))
        {
            return content;
        }

        // Load from disk if not in cache
        try
        {
            if (File.Exists(filePath))
            {
                content = await File.ReadAllTextAsync(filePath);
                _fileCache.AddOrUpdate(filePath, content);
                return content;
            }
        }
        catch (Exception ex)
        {
            Errors.Add($"Error reading file: {ex.Message}");
        }

        return null;
    }

    [RelayCommand]
    private void ToggleFileSelection(FileItem file)
    {
        file.IsSelected = !file.IsSelected;
    }

    [RelayCommand]
    private async Task OpenFile()
    {
        try
        {
			var customFileTypes = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".xml", ".vecto" } },
                    { DevicePlatform.Android, new[] { "text/xml", "application/xml", "application/vecto" } },
                    { DevicePlatform.macOS, new[] { "xml", "vecto" } }
                });

            var options = new PickOptions
            {
                PickerTitle = "Please select files",
                FileTypes = customFileTypes
            };

			var results = await FilePicker.Default.PickMultipleAsync(options);

            if (results != null)
            {
                foreach (var result in results)
                {
                    var fileInfo = new FileInfo(result.FullPath);

                    // Check if file already exists in the list
                    if (Files.Any(f => f.Name == result.FileName))
                    {
                        Errors.Add($"File '{result.FileName}' is already in the list");
                        continue;
                    }

                    var fileItem = new FileItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = result.FileName,
                        Extension = fileInfo.Extension.TrimStart('.'),
                        Description = $"Imported file from {fileInfo.DirectoryName}",
                        Size = FormatFileSize(fileInfo.Length),
                        Modified = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm"),
                        FilePath = result.FullPath
                    };

                    Files.Add(fileItem);
                }
            }
        }
        catch (Exception ex)
        {
            Errors.Add($"Error opening files: {ex.Message}");
        }
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

    [RelayCommand]
    private async Task StartExecution()
    {
        IsExecuting = true;
        Progress = 0;

        // Simulate execution
        for (int i = 0; i <= 100; i++)
        {
            Progress = i;
            await Task.Delay(50);

            if (!IsExecuting)
                break;
        }

        // Simulate an error
        if (IsExecuting)
        {
            Errors.Add($"Error executing {ActiveFile?.Name ?? "file"}: Simulated execution error at {DateTime.Now:HH:mm:ss}");
        }
    }

    [RelayCommand]
    private void StopExecution()
    {
        IsExecuting = false;
        Progress = 0;
    }

    [RelayCommand]
    private void ToggleDeclarationMode()
    {
        IsDeclarationMode = !IsDeclarationMode;
    }

    [RelayCommand]
    private void DeleteSelectedFiles()
    {
        var selectedFiles = Files.Where(f => f.IsSelected).ToList();

        if (selectedFiles.Count == 0)
            return;

        foreach (var file in selectedFiles)
        {
            Files.Remove(file);
        }

        if (ActiveFile != null && selectedFiles.Contains(ActiveFile))
        {
            ActiveFile = null;
        }
    }

    [RelayCommand]
    private void MoveFileUp()
    {
        var selectedFile = Files.FirstOrDefault(f => f.IsSelected);
        if (selectedFile == null)
            return;

        var index = Files.IndexOf(selectedFile);
        if (index > 0)
        {
            Files.Move(index, index - 1);
        }
    }

    [RelayCommand]
    private void MoveFileDown()
    {
        var selectedFile = Files.FirstOrDefault(f => f.IsSelected);
        if (selectedFile == null)
            return;

        var index = Files.IndexOf(selectedFile);
        if (index < Files.Count - 1)
        {
            Files.Move(index, index + 1);
        }
    }

    [RelayCommand]
    private void ClearError(string error)
    {
        Errors.Remove(error);
    }

    [RelayCommand]
    private async Task OpenJobEditor()
    {
        // TODO: Implement Job Editor
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OpenGraph()
    {
        // TODO: Implement Graph
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OpenLog()
    {
        // TODO: Implement Open Log
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OpenSettings()
    {
        // TODO: Implement Settings
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OpenUserManual()
    {
        // TODO: Implement User Manual
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OpenReleaseNotes()
    {
        // TODO: Implement Release Notes
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ReportIssue()
    {
        // TODO: Implement Report Issue
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ShowAbout()
    {
        // TODO: Implement About
        await Task.CompletedTask;
    }

    private void FileItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FileItem.IsSelected))
        {
            UpdateSelectedFilesCount();
        }
    }

    private void UpdateSelectedFilesCount()
    {
        SelectedFilesCount = Files.Count(f => f.IsSelected);
    }

    [RelayCommand]
    private void ClearCache()
    {
        _fileCache.Clear();
        foreach (var file in Files)
        {
            file.IsCached = false;
        }
    }

    public int CachedFilesCount => _fileCache.Count;

    public long TotalCacheSize => _fileCache.TotalCacheSize;

    public IEnumerable<CachedFileInfo> GetCachedFiles() => _fileCache.GetCachedFiles();

#if WINDOWS
    public async Task AddFilesFromDrop(IEnumerable<IStorageFile> storageFiles)
    {
        try
        {
            foreach (var storageFile in storageFiles)
            {
                // Get file extension
                var extension = Path.GetExtension(storageFile.Path).TrimStart('.');

                // Filter only XML and JSON files
                if (extension.ToLower() != "xml" && extension.ToLower() != "json")
                {
                    Errors.Add($"File '{storageFile.Name}' is not a supported file type (xml or json)");
                    continue;
                }

                // Check if file already exists in the list
                if (Files.Any(f => f.Name == storageFile.Name))
                {
                    Errors.Add($"File '{storageFile.Name}' is already in the list");
                    continue;
                }

                var fileInfo = new FileInfo(storageFile.Path);

                var fileItem = new FileItem
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = storageFile.Name,
                    Extension = extension,
                    Description = $"Imported file from {fileInfo.DirectoryName}",
                    Size = FormatFileSize(fileInfo.Length),
                    Modified = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm"),
                    FilePath = storageFile.Path
                };

                Files.Add(fileItem);
            }
        }
        catch (Exception ex)
        {
            Errors.Add($"Error processing dropped files: {ex.Message}");
        }

        await Task.CompletedTask;
    }
#endif
}
