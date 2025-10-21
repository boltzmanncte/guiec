using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using vfv.Models;

namespace vfv.ViewModels;

public partial class MainViewModel : ObservableObject
{
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

    public MainViewModel()
    {
        // Initialize with mock data
        Files = new ObservableCollection<FileItem>
        {
            new FileItem
            {
                Id = "1",
                Name = "config.xml",
                Extension = "xml",
                Description = "Configuration file containing application settings and parameters.",
                Size = "2.4 KB",
                Modified = "2025-10-08 14:30"
            },
            new FileItem
            {
                Id = "2",
                Name = "data.json",
                Extension = "json",
                Description = "JSON data file with user preferences and application state.",
                Size = "5.1 KB",
                Modified = "2025-10-08 12:15"
            },
            new FileItem
            {
                Id = "3",
                Name = "schema.xml",
                Extension = "xml",
                Description = "XML schema definition for data validation and structure.",
                Size = "3.8 KB",
                Modified = "2025-10-07 16:45"
            },
            new FileItem
            {
                Id = "4",
                Name = "settings.json",
                Extension = "json",
                Description = "Application settings stored in JSON format.",
                Size = "1.2 KB",
                Modified = "2025-10-07 09:20"
            },
            new FileItem
            {
                Id = "5",
                Name = "declaration.xml",
                Extension = "xml",
                Description = "Declaration file defining application components and dependencies.",
                Size = "4.5 KB",
                Modified = "2025-10-06 11:00"
            },
            new FileItem
            {
                Id = "6",
                Name = "manifest.json",
                Extension = "json",
                Description = "Manifest file containing metadata and configuration.",
                Size = "2.9 KB",
                Modified = "2025-10-05 15:30"
            }
        };
    }

    [RelayCommand]
    private void SelectFile(FileItem file)
    {
        if (ActiveFile != null)
            ActiveFile.IsActive = false;

        file.IsActive = true;
        ActiveFile = file;
    }

    [RelayCommand]
    private void ToggleFileSelection(FileItem file)
    {
        file.IsSelected = !file.IsSelected;
    }

    [RelayCommand]
    private async Task OpenFile()
    {
        // TODO: Implement file opening logic
        await Task.CompletedTask;
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
}
