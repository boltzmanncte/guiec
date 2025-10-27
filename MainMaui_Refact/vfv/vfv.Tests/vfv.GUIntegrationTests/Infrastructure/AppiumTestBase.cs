using OpenQA.Selenium.Appium;
using System.Diagnostics;
using System.Reflection;

namespace vfv.GUIntegrationTests.Infrastructure;

/// <summary>
/// Base class for GUI integration tests using WinAppDriver and Appium
/// </summary>
public abstract class AppiumTestBase : IDisposable
{
    protected WindowsDriver<WindowsElement>? Session { get; private set; }
    protected const string WindowsApplicationDriverUrl = "http://127.0.0.1:4723";
    protected const int ImplicitWaitSeconds = 10;
    private ExecutableRunner? _appRunner;

	public static string AssemblyDirectory
	{
		get
		{
			string codeBase = Assembly.GetExecutingAssembly().Location;
			UriBuilder uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);
			return Path.GetDirectoryName(path);
		}
	}

	/// <summary>
	/// Initialize the WinAppDriver session
	/// </summary>
	/// <param name="appPath">Path to the application executable or Package Family Name for UWP apps</param>
	protected void InitializeSession(string appPath)
    {
		// Launch the application first
		_appRunner = new ExecutableRunner(appPath);
		_appRunner.Start();
		Console.WriteLine($"Launched application with PID: {_appRunner.ProcessId}, waiting for window to appear...");

		// Wait for the application window to stabilize
		Thread.Sleep(5000);

		// Check if app is still running
		if (!_appRunner.IsRunning)
		{
			throw new InvalidOperationException("Application exited immediately after launch. Check application logs for errors.");
		}

		Console.WriteLine($"Application still running (PID: {_appRunner.ProcessId}), attempting to connect...");

		// Retry logic for session initialization
		int maxRetries = 5;
		int retryCount = 0;
		Exception? lastException = null;

		while (retryCount < maxRetries)
		{
			try
			{
				// Get fresh window handle for each attempt
				var windowHandle = GetWindowHandleByProcessId(_appRunner.ProcessId!.Value);
				Console.WriteLine($"Found window handle: {windowHandle}");

				var appiumOptions = new AppiumOptions();
				appiumOptions.AddAdditionalCapability("appTopLevelWindow", windowHandle);
				appiumOptions.AddAdditionalCapability("platformName", "Windows");
				appiumOptions.AddAdditionalCapability("deviceName", "WindowsPC");
				appiumOptions.AddAdditionalCapability("ms:experimental-webdriver", true);

				Session = new WindowsDriver<WindowsElement>(
					new Uri(WindowsApplicationDriverUrl),
					appiumOptions,
					TimeSpan.FromSeconds(60)
				);

				Session.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(ImplicitWaitSeconds);

				// Successfully created session
				Console.WriteLine($"Successfully connected to application window (attempt {retryCount + 1})");
				return;
			}
			catch (Exception ex)
			{
				lastException = ex;
				retryCount++;

				if (retryCount < maxRetries)
				{
					Console.WriteLine($"Failed to connect to window (attempt {retryCount}): {ex.Message}, retrying in 3 seconds...");
					Thread.Sleep(3000);
				}
			}
		}

		// If we get here, all retries failed
		throw new InvalidOperationException(
			$"Failed to connect to application window after {maxRetries} attempts. " +
			"The application may take longer to start, or the window may not be appearing. " +
			"Try running the application manually to verify it starts correctly.",
			lastException);
    }

    /// <summary>
    /// Get window handle by process ID
    /// </summary>
    private static string GetWindowHandleByProcessId(int processId)
    {
        var process = Process.GetProcessById(processId);
        if (process == null || process.HasExited)
            throw new InvalidOperationException($"Process with PID {processId} not found or has exited");

        // Return the main window handle in hexadecimal format
        var mainHandle = process.MainWindowHandle;
        if (mainHandle == IntPtr.Zero)
            throw new InvalidOperationException("Process found but main window handle is not available (window may not be visible yet)");

        return mainHandle.ToString("X");
    }

    /// <summary>
    /// Wait for element to be available
    /// </summary>
    protected IWebElement? WaitForElement(By locator, int timeoutSeconds = 10)
    {
        if (Session == null)
            throw new InvalidOperationException("Session not initialized. Call InitializeSession first.");

        var endTime = DateTime.Now.AddSeconds(timeoutSeconds);
        while (DateTime.Now < endTime)
        {
            try
            {
                var element = Session.FindElement(locator);
                if (element != null && element.Displayed)
                {
                    return element;
                }
            }
            catch (NoSuchElementException)
            {
                // Element not found yet, continue waiting
            }
            catch (WebDriverException)
            {
                // Other WebDriver exceptions, continue waiting
            }

            Thread.Sleep(500);
        }

        return null;
    }

    /// <summary>
    /// Wait for multiple elements to be available
    /// </summary>
    protected IReadOnlyCollection<IWebElement>? WaitForElements(By locator, int timeoutSeconds = 10)
    {
        if (Session == null)
            throw new InvalidOperationException("Session not initialized. Call InitializeSession first.");

        var endTime = DateTime.Now.AddSeconds(timeoutSeconds);
        while (DateTime.Now < endTime)
        {
            try
            {
                var elements = Session.FindElements(locator);
                if (elements != null && elements.Count > 0)
                {
                    return elements;
                }
            }
            catch (NoSuchElementException)
            {
                // Elements not found yet, continue waiting
            }

            Thread.Sleep(500);
        }

        return null;
    }

    /// <summary>
    /// Click element with retry logic
    /// </summary>
    protected void ClickElement(IWebElement element, int maxRetries = 3)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                element.Click();
                return;
            }
            catch (WebDriverException ex)
            {
                if (i == maxRetries - 1)
                    throw new InvalidOperationException($"Failed to click element after {maxRetries} attempts", ex);

                Thread.Sleep(500);
            }
        }
    }

    /// <summary>
    /// Type text into element
    /// </summary>
    protected void SendKeysToElement(IWebElement element, string text)
    {
        element.Clear();
        element.SendKeys(text);
    }

    /// <summary>
    /// Take screenshot for debugging
    /// </summary>
    protected void TakeScreenshot(string fileName)
    {
        if (Session == null)
            return;

        var screenshot = Session.GetScreenshot();
        var screenshotDir = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
        Directory.CreateDirectory(screenshotDir);

        var filePath = Path.Combine(screenshotDir, $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
        screenshot.SaveAsFile(filePath);
    }

    public virtual void Dispose()
    {
        try
        {
            Session?.Quit();
            Session?.Dispose();
        }
        catch
        {
            // Ignore disposal errors
        }

        try
        {
            _appRunner?.Stop();
            _appRunner?.Dispose();
        }
        catch
        {
            // Ignore disposal errors
        }

        GC.SuppressFinalize(this);
    }
}