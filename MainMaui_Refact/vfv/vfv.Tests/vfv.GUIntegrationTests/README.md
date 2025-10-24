# vfv GUI Integration Tests

This project contains end-to-end (E2E) GUI integration tests for the vfv MAUI application.

## Prerequisites

### 1. WinAppDriver (for UI Automation Tests)

WinAppDriver is required for running UI automation tests on Windows.

**Installation:**
1. Download WinAppDriver from: https://github.com/Microsoft/WinAppDriver/releases
2. Install the MSI package (recommended location: `C:\Program Files\Windows Application Driver\`)
3. Run WinAppDriver.exe before executing UI tests

**Starting WinAppDriver:**
```powershell
# Run as Administrator
"C:\Program Files\Windows Application Driver\WinAppDriver.exe"
```

WinAppDriver will start listening on `http://127.0.0.1:4723`

### 2. .NET 8 SDK

Ensure you have .NET 8 SDK installed:
```powershell
dotnet --version
```

## Project Structure

```
vfv.GUIntegrationTests/
├── Infrastructure/
│   └── AppiumTestBase.cs          # Base class for UI automation tests
├── Tests/
│   ├── FileListTests.cs           # Tests for file list UI functionality
│   └── FilePersistenceTests.cs    # Tests for file persistence
└── README.md                       # This file
```

## NuGet Packages

The project uses the following packages:

- **Microsoft.NET.Test.Sdk** - Test SDK for running tests
- **xUnit** - Test framework
- **FluentAssertions** - Fluent assertion library
- **Moq** - Mocking framework
- **Appium.WebDriver** - WebDriver for UI automation via WinAppDriver
- **Microsoft.Maui.Controls** - MAUI controls for testing

## Running Tests

### Run All Tests
```powershell
cd MainMaui_Refact/vfv/vfv.GUIntegrationTests
dotnet test
```

### Run Specific Test Class
```powershell
dotnet test --filter "FullyQualifiedName~FilePersistenceTests"
```

### Run Tests with Verbose Output
```powershell
dotnet test --logger "console;verbosity=detailed"
```

### Run Tests in Visual Studio
1. Open Test Explorer (Test > Test Explorer)
2. Build the solution
3. Tests will appear in Test Explorer
4. Click "Run All" or run individual tests

## Test Categories

### 1. File Persistence Tests (`FilePersistenceTests.cs`)
These tests verify the file list persistence functionality:
- ✅ Can run without WinAppDriver
- ✅ Tests saving/loading file lists
- ✅ Tests data integrity
- ✅ Tests error handling

### 2. UI Automation Tests (`FileListTests.cs`)
These tests verify UI interactions:
- ⚠️ Requires WinAppDriver to be running
- Currently marked with `Skip` attribute
- Tests file list operations (add, delete, move, drag & drop)

## Configuration

### Update Application Path

Before running UI automation tests, update the `AppPath` constant in test files:

```csharp
// Option 1: Use executable path
private const string AppPath = @"C:\Path\To\Your\vfv.exe";

// Option 2: Use Package Family Name (for packaged apps)
private const string AppPath = "YourCompanyName.vfv_8wekyb3d8bbwe!App";
```

To find your Package Family Name:
```powershell
Get-AppxPackage | Where-Object {$_.Name -like "*vfv*"} | Select-Object PackageFamilyName
```

## Writing New Tests

### Persistence Tests

Extend `FilePersistenceTests.cs` for testing data persistence:

```csharp
[Fact]
public async Task YourTest_WhenCondition_ShouldExpectedResult()
{
    // Arrange
    var persistence = new FileListPersistence();

    // Act
    // ... your test code

    // Assert
    result.Should().BeTrue();
}
```

### UI Automation Tests

Extend `AppiumTestBase` for UI tests:

```csharp
public class MyUITests : AppiumTestBase
{
    [Fact]
    public void TestButtonClick()
    {
        // Arrange
        InitializeSession(AppPath);

        // Act
        var button = WaitForElement(By.Name("ButtonName"));
        ClickElement(button);

        // Assert
        var result = WaitForElement(By.Name("ResultLabel"));
        result.Text.Should().Be("Expected Text");
    }
}
```

## Finding UI Elements

WinAppDriver supports various locator strategies:

```csharp
// By AutomationId
By.Id("myAutomationId")

// By Name (button text, label text)
By.Name("Open File")

// By ClassName
By.ClassName("Button")

// By XPath
By.XPath("//Button[@Name='Open File']")

// By AccessibilityId
By.AccessibilityId("openFileButton")
```

## Debugging Tips

### 1. Use Inspect.exe
Windows SDK includes Inspect.exe to find element properties:
- Location: `C:\Program Files (x86)\Windows Kits\10\bin\<version>\x64\inspect.exe`
- Use it to find AutomationIds, Names, and other properties

### 2. Take Screenshots
The base class includes screenshot capability:

```csharp
TakeScreenshot("test-failed");
```

Screenshots are saved to: `bin/Debug/net8.0-windows10.0.19041.0/Screenshots/`

### 3. Check WinAppDriver Logs
WinAppDriver shows real-time logs when running. Monitor them for debugging connection or element location issues.

### 4. Increase Timeouts
If tests are flaky, increase wait times:

```csharp
var element = WaitForElement(By.Name("SlowButton"), timeoutSeconds: 30);
```

## Troubleshooting

### "Connection refused" Error
- Ensure WinAppDriver.exe is running
- Check it's listening on `http://127.0.0.1:4723`
- Run WinAppDriver as Administrator

### "Element not found" Error
- Use Inspect.exe to verify element properties
- Check if element is visible and enabled
- Increase timeout values
- Verify AutomationId or Name is correct

### Tests Fail in CI/CD
- UI automation tests require an active Windows session
- Consider running persistence tests only in CI
- Use Azure DevOps hosted agents with UI capabilities

## Best Practices

1. **Keep Tests Independent**: Each test should be able to run independently
2. **Use Descriptive Names**: Follow pattern `MethodName_WhenCondition_ShouldExpectedResult`
3. **Clean Up Resources**: Implement `IDisposable` to clean up test data
4. **Use Page Object Pattern**: For complex UIs, create page object classes
5. **Avoid Thread.Sleep**: Use `WaitForElement` instead of fixed waits
6. **Test Data Isolation**: Use unique test data to avoid conflicts

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Integration Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: windows-latest

    steps:
    - uses: actions/checkout@v3

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --no-restore

    - name: Run Tests
      run: dotnet test --no-build --verbosity normal
```

## Additional Resources

- [WinAppDriver Documentation](https://github.com/Microsoft/WinAppDriver)
- [Appium Documentation](http://appium.io/docs/en/about-appium/intro/)
- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [MAUI Testing Guide](https://docs.microsoft.com/en-us/dotnet/maui/testing/)