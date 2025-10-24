using vfv.Services;

namespace vfv.GUIntegrationTests.Tests;

/// <summary>
/// Integration tests for file list persistence functionality
/// </summary>
public class FilePersistenceTests : IDisposable
{
    private readonly FileListPersistence _persistence;
    private readonly string _testStorageFile;

    public FilePersistenceTests()
    {
        _persistence = new FileListPersistence();
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _testStorageFile = Path.Combine(appDataPath, "fileList.json");
    }

    [Fact]
    public async Task SaveFileListAsync_WhenCalled_ShouldCreateStorageFile()
    {
        // Arrange
        var testFiles = CreateTestFileItems();

        // Act
        await _persistence.SaveFileListAsync(testFiles);

        // Assert
        _persistence.HasPersistedData().Should().BeTrue("Storage file should exist after saving");
    }

    [Fact]
    public async Task LoadFileListAsync_WhenStorageExists_ShouldReturnSavedFiles()
    {
        // Arrange
        var testFiles = CreateTestFileItems();
        await _persistence.SaveFileListAsync(testFiles);

        // Act
        var loadedFiles = await _persistence.LoadFileListAsync();

        // Assert
        loadedFiles.Should().NotBeNull();
        loadedFiles.Should().HaveCount(testFiles.Count);
    }

    [Fact]
    public async Task LoadFileListAsync_WhenNoStorage_ShouldReturnEmptyList()
    {
        // Arrange
        await _persistence.ClearStorageAsync();

        // Act
        var loadedFiles = await _persistence.LoadFileListAsync();

        // Assert
        loadedFiles.Should().NotBeNull();
        loadedFiles.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadFileListAsync_WhenFileDoesNotExist_ShouldSkipMissingFiles()
    {
        // Arrange
        var testFiles = CreateTestFileItems();
        testFiles.Add(new vfv.Models.FileItem
        {
            Id = Guid.NewGuid().ToString(),
            Name = "nonexistent.xml",
            FilePath = "C:\\NonExistent\\Path\\nonexistent.xml"
        });

        await _persistence.SaveFileListAsync(testFiles);

        // Act
        var loadedFiles = await _persistence.LoadFileListAsync();

        // Assert
        loadedFiles.Should().NotContain(f => f.Name == "nonexistent.xml",
            "Files that no longer exist should not be loaded");
    }

    [Fact]
    public async Task ClearStorageAsync_WhenCalled_ShouldRemoveStorageFile()
    {
        // Arrange
        var testFiles = CreateTestFileItems();
        await _persistence.SaveFileListAsync(testFiles);
        _persistence.HasPersistedData().Should().BeTrue();

        // Act
        await _persistence.ClearStorageAsync();

        // Assert
        _persistence.HasPersistedData().Should().BeFalse("Storage file should be deleted");
    }

    [Fact]
    public async Task SaveAndLoad_ShouldPreserveFileProperties()
    {
        // Arrange
        var originalFile = new vfv.Models.FileItem
        {
            Id = Guid.NewGuid().ToString(),
            Name = "test.xml",
            Extension = "xml",
            Description = "Test file",
            Size = "1.5 KB",
            Modified = "2025-10-23 12:00",
            FilePath = CreateTestFile("test.xml")
        };

        var testFiles = new List<vfv.Models.FileItem> { originalFile };
        await _persistence.SaveFileListAsync(testFiles);

        // Act
        var loadedFiles = await _persistence.LoadFileListAsync();

        // Assert
        var loadedFile = loadedFiles.Should().ContainSingle().Subject;
        loadedFile.Id.Should().Be(originalFile.Id);
        loadedFile.Name.Should().Be(originalFile.Name);
        loadedFile.Extension.Should().Be(originalFile.Extension);
        loadedFile.FilePath.Should().Be(originalFile.FilePath);
    }

    private List<vfv.Models.FileItem> CreateTestFileItems()
    {
        var testFile1Path = CreateTestFile("test1.xml");
        var testFile2Path = CreateTestFile("test2.json");

        return new List<vfv.Models.FileItem>
        {
            new vfv.Models.FileItem
            {
                Id = Guid.NewGuid().ToString(),
                Name = "test1.xml",
                Extension = "xml",
                Description = "Test XML file",
                Size = "1 KB",
                Modified = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                FilePath = testFile1Path
            },
            new vfv.Models.FileItem
            {
                Id = Guid.NewGuid().ToString(),
                Name = "test2.json",
                Extension = "json",
                Description = "Test JSON file",
                Size = "2 KB",
                Modified = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                FilePath = testFile2Path
            }
        };
    }

    private string CreateTestFile(string fileName)
    {
        var testDir = Path.Combine(Path.GetTempPath(), "vfv_test_files");
        Directory.CreateDirectory(testDir);

        var filePath = Path.Combine(testDir, fileName);
        File.WriteAllText(filePath, $"<test>Content for {fileName}</test>");

        return filePath;
    }

    private void CleanupTestFiles()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "vfv_test_files");
        if (Directory.Exists(testDir))
        {
            try
            {
                Directory.Delete(testDir, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    public void Dispose()
    {
        CleanupTestFiles();
        _persistence.ClearStorageAsync().Wait();
        GC.SuppressFinalize(this);
    }
}