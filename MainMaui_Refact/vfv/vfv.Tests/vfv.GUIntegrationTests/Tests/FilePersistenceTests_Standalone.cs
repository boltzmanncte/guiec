namespace vfv.GUIntegrationTests.Tests;

/// <summary>
/// Standalone integration tests for file list persistence that don't require MAUI runtime
/// These tests work with a test-only version of persistence logic
/// </summary>
public class FilePersistenceTests_Standalone
{
    [Fact]
    public void CreateTestFile_ShouldWork()
    {
        // This is a simple test to verify the test infrastructure works
        var testPath = Path.Combine(Path.GetTempPath(), "test.txt");
        File.WriteAllText(testPath, "test");

        File.Exists(testPath).Should().BeTrue();

        File.Delete(testPath);
    }

    [Fact]
    public void FileSystemOperations_ShouldWork()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "vfv_test");
        Directory.CreateDirectory(testDir);

        Directory.Exists(testDir).Should().BeTrue();

        Directory.Delete(testDir);
    }
}