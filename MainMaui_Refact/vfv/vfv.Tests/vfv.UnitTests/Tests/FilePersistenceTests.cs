using vfv.Services.Persistence;
using vfv.Services.Models;
using FluentAssertions;
using Xunit;

namespace vfv.UnitTests.Tests;

/// <summary>
/// Unit tests for file list persistence functionality
/// </summary>
public class FilePersistenceTests : IDisposable
{
    private readonly FileListPersistence _persistence;
    private readonly string _testStorageDir;

    public FilePersistenceTests()
    {
        // Use a test-specific storage path that doesn't require MAUI runtime
        _testStorageDir = Path.Combine(Path.GetTempPath(), $"vfv_persistence_tests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testStorageDir);

        _persistence = new FileListPersistence(_testStorageDir);
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
        testFiles.Add(new FileItemDto
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
        var originalFile = new FileItemDto
        {
            Id = Guid.NewGuid().ToString(),
            Name = "test.xml",
            Extension = "xml",
            Description = "Test file",
            Size = "1.5 KB",
            Modified = "2025-10-23 12:00",
            FilePath = CreateTestFile("test.xml")
        };

        var testFiles = new List<FileItemDto> { originalFile };
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

    [Theory]
    [InlineData(500, "500 B")]              // Less than 1 KB - no loop iteration
    [InlineData(1024, "1 KB")]              // Exactly 1 KB - 1 loop iteration
    [InlineData(2048, "2 KB")]              // 2 KB - 1 loop iteration
    [InlineData(1048576, "1 MB")]           // Exactly 1 MB - 2 loop iterations
    [InlineData(1572864, "1.5 MB")]         // 1.5 MB - 2 loop iterations
    [InlineData(1073741824, "1 GB")]        // Exactly 1 GB - 3 loop iterations
    [InlineData(5368709120, "5 GB")]        // 5 GB - 3 loop iterations
    public async Task LoadFileListAsync_WhenCalledWithDifferentFileSizes_ShouldFormatSizeCorrectly(long fileSize, string expectedSize)
    {
        // Arrange
        var testFilePath = CreateTestFileWithSize("sizefile.xml", fileSize);
        var testFile = new FileItemDto
        {
            Id = Guid.NewGuid().ToString(),
            Name = "sizefile.xml",
            Extension = "xml",
            Description = "Test file for size formatting",
            FilePath = testFilePath
        };

        var testFiles = new List<FileItemDto> { testFile };
        await _persistence.SaveFileListAsync(testFiles);

        // Act
        var loadedFiles = await _persistence.LoadFileListAsync();

        // Assert
        var loadedFile = loadedFiles.Should().ContainSingle().Subject;
        loadedFile.Size.Should().Be(expectedSize,
            $"A file of {fileSize} bytes should be formatted as {expectedSize}");
    }

    private List<FileItemDto> CreateTestFileItems()
    {
        var testFile1Path = CreateTestFile("test1.xml");
        var testFile2Path = CreateTestFile("test2.json");

        return new List<FileItemDto>
        {
            new FileItemDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = "test1.xml",
                Extension = "xml",
                Description = "Test XML file",
                Size = "1 KB",
                Modified = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                FilePath = testFile1Path
            },
            new FileItemDto
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

    private string CreateTestFileWithSize(string fileName, long sizeInBytes)
    {
        var testDir = Path.Combine(Path.GetTempPath(), "vfv_test_files");
        Directory.CreateDirectory(testDir);

        var filePath = Path.Combine(testDir, fileName);

        // Create a file with the specified size
        using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            fs.SetLength(sizeInBytes);
        }

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
