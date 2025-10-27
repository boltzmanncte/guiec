using System.Diagnostics;

namespace vfv.GUIntegrationTests.Infrastructure;

/// <summary>
/// Fixture that ensures WinAppDriver is running before tests execute
/// </summary>
public class WinAppDriverFixture : IDisposable
{
    private Process? _winAppDriverProcess;
    private const string WinAppDriverPath = @"C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe";
    private const string WinAppDriverUrl = "http://127.0.0.1:4723";

    public WinAppDriverFixture()
    {
        StartWinAppDriver();
    }

    private void StartWinAppDriver()
    {
        // Check if WinAppDriver is already running
        var existingProcess = Process.GetProcessesByName("WinAppDriver").FirstOrDefault();
        if (existingProcess != null)
        {
            Console.WriteLine("WinAppDriver is already running (PID: {0})", existingProcess.Id);
            return;
        }

        // Check if WinAppDriver executable exists
        if (!File.Exists(WinAppDriverPath))
        {
            throw new FileNotFoundException(
                $"WinAppDriver not found at {WinAppDriverPath}. " +
                "Please install Windows Application Driver from: " +
                "https://github.com/microsoft/WinAppDriver/releases");
        }

        try
        {
            // Start WinAppDriver process
            var startInfo = new ProcessStartInfo
            {
                FileName = WinAppDriverPath,
                Arguments = WinAppDriverUrl,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            _winAppDriverProcess = Process.Start(startInfo);

            if (_winAppDriverProcess == null)
            {
                throw new InvalidOperationException("Failed to start WinAppDriver process");
            }

            Console.WriteLine("Started WinAppDriver (PID: {0})", _winAppDriverProcess.Id);

            // Wait a moment for WinAppDriver to start listening
            Thread.Sleep(2000);

            // Verify it's running
            if (_winAppDriverProcess.HasExited)
            {
                throw new InvalidOperationException(
                    $"WinAppDriver exited immediately with code {_winAppDriverProcess.ExitCode}. " +
                    "It may require administrator privileges.");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Failed to start WinAppDriver. Ensure you have administrator privileges and WinAppDriver is installed.",
                ex);
        }
    }

    public void Dispose()
    {
        // Only kill the process if we started it
        if (_winAppDriverProcess != null && !_winAppDriverProcess.HasExited)
        {
            try
            {
                Console.WriteLine("Stopping WinAppDriver (PID: {0})", _winAppDriverProcess.Id);
                _winAppDriverProcess.Kill();
                _winAppDriverProcess.WaitForExit(5000);
                _winAppDriverProcess.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error stopping WinAppDriver: {0}", ex.Message);
            }
        }

        GC.SuppressFinalize(this);
    }
}
