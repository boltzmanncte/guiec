# vfv Unit Tests

This project contains unit tests for the vfv.Services library.

## Running Tests

To run all unit tests:

```bash
dotnet test vfv.UnitTests.csproj
```

To run a specific test:

```bash
dotnet test vfv.UnitTests.csproj --filter "FullyQualifiedName~FilePersistenceTests.SaveFileListAsync_WhenCalled_ShouldCreateStorageFile"
```

## Test Coverage

### Prerequisites

Install the ReportGenerator global tool (only needed once):

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

### Generate Coverage Report

**Step 1: Run tests with coverage collection**

```bash
dotnet test vfv.UnitTests.csproj --collect:"XPlat Code Coverage"
```

This will generate a `coverage.cobertura.xml` file in the `TestResults` directory.

**Step 2: Generate HTML coverage report**

```bash
reportgenerator -reports:"TestResults/*/coverage.cobertura.xml" -targetdir:"TestResults/CoverageReport" -reporttypes:"Html;TextSummary"
```

**Step 3: View the coverage report**

Open the HTML report in your browser:

```bash
start TestResults/CoverageReport/index.html
```

Or view the text summary in the console:

```bash
cat TestResults/CoverageReport/Summary.txt
```

### One-Line Command

To run tests and generate coverage in one go:

```bash
dotnet test vfv.UnitTests.csproj --collect:"XPlat Code Coverage" && reportgenerator -reports:"TestResults/*/coverage.cobertura.xml" -targetdir:"TestResults/CoverageReport" -reporttypes:"Html;TextSummary"
```

### Alternative: Visual Studio

If you're using Visual Studio:

1. Open **Test Explorer** (Test > Test Explorer)
2. Click the **Analyze Code Coverage** button in the toolbar
3. Run **All Tests with Code Coverage**
4. View coverage results in the **Code Coverage Results** window

## Current Coverage

As of the last run:

- **Line Coverage**: 78.8% (67/85 lines)
- **Branch Coverage**: 81.2% (13/16 branches)
- **Method Coverage**: 100% (14/14 methods)

### Coverage by Class

- `FileItemDto`: 100%
- `FileListPersistence`: 76.6%

## Test Structure

```
vfv.UnitTests/
├── Tests/
│   └── FilePersistenceTests.cs   # Tests for FileListPersistence service
├── TestResults/                   # Generated coverage reports (git-ignored)
├── vfv.UnitTests.csproj
└── README.md
```

## Dependencies

- **xUnit**: Test framework
- **FluentAssertions**: Assertion library for readable tests
- **Moq**: Mocking library for unit tests
- **coverlet.collector**: Code coverage data collector

## Writing New Tests

When adding new tests, follow these conventions:

1. Use xUnit `[Fact]` attribute for tests without parameters
2. Use FluentAssertions for assertions (`.Should().Be()`, `.Should().NotBeNull()`, etc.)
3. Follow the naming pattern: `MethodName_WhenCondition_ShouldExpectedBehavior`
4. Test both success paths and error conditions

Example:

```csharp
[Fact]
public async Task SaveFileListAsync_WhenCalled_ShouldCreateStorageFile()
{
    // Arrange
    var persistence = new FileListPersistence();
    var files = new List<FileItemDto>
    {
        new() { FileName = "test.xml", FilePath = @"C:\test.xml", FileSize = 1024 }
    };

    // Act
    await persistence.SaveFileListAsync(files);

    // Assert
    var loaded = await persistence.LoadFileListAsync();
    loaded.Should().HaveCount(1);
    loaded[0].FileName.Should().Be("test.xml");
}
```

## Continuous Integration

For CI/CD pipelines, use the following command to fail the build if coverage falls below a threshold:

```bash
dotnet test vfv.UnitTests.csproj --collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Threshold=80
```

This will fail the build if coverage drops below 80%.