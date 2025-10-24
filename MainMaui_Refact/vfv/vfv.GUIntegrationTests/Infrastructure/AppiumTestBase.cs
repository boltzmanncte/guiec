using OpenQA.Selenium.Appium;

namespace vfv.GUIntegrationTests.Infrastructure;

/// <summary>
/// Base class for GUI integration tests using WinAppDriver and Appium
/// </summary>
public abstract class AppiumTestBase : IDisposable
{
    protected WindowsDriver? Session { get; private set; }
    protected const string WindowsApplicationDriverUrl = "http://127.0.0.1:4723";
    protected const int ImplicitWaitSeconds = 10;

    /// <summary>
    /// Initialize the WinAppDriver session
    /// </summary>
    /// <param name="appPath">Path to the application executable or Package Family Name for UWP apps</param>
    protected void InitializeSession(string appPath)
    {
        var appiumOptions = new AppiumOptions();
        appiumOptions.AddAdditionalOption("app", appPath);
        appiumOptions.AddAdditionalOption("deviceName", "WindowsPC");
        appiumOptions.AddAdditionalOption("platformName", "Windows");

        Session = new WindowsDriver(
            new Uri(WindowsApplicationDriverUrl),
            appiumOptions,
            TimeSpan.FromSeconds(60)
        );

        Session.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(ImplicitWaitSeconds);
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

        GC.SuppressFinalize(this);
    }
}