using System.Diagnostics;

namespace vfv.GUIntegrationTests.Infrastructure;

/// <summary>
/// Utility class for running executable files
/// </summary>
public class ExecutableRunner : IDisposable
{
    private Process? _process;
    private readonly string _executablePath;
    private readonly string? _arguments;
    private readonly bool _redirectOutput;

    /// <summary>
    /// Creates a new executable runner
    /// </summary>
    /// <param name="executablePath">Full path to the executable file</param>
    /// <param name="arguments">Optional command-line arguments</param>
    /// <param name="redirectOutput">Whether to redirect standard output and error</param>
    public ExecutableRunner(string executablePath, string? arguments = null, bool redirectOutput = false)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
            throw new ArgumentException("Executable path cannot be null or empty", nameof(executablePath));

        if (!File.Exists(executablePath))
            throw new FileNotFoundException($"Executable not found: {executablePath}", executablePath);

        _executablePath = executablePath;
        _arguments = arguments;
        _redirectOutput = redirectOutput;
    }

    /// <summary>
    /// Gets whether the process is currently running
    /// </summary>
    public bool IsRunning => _process != null && !_process.HasExited;

    /// <summary>
    /// Gets the process ID if the process is running
    /// </summary>
    public int? ProcessId => _process?.HasExited == false ? _process.Id : null;

    /// <summary>
    /// Starts the executable
    /// </summary>
    /// <param name="waitForExit">If true, waits for the process to exit</param>
    /// <param name="timeoutMilliseconds">Timeout in milliseconds when waiting for exit</param>
    /// <returns>Exit code if waitForExit is true, otherwise null</returns>
    public int? Start(bool waitForExit = false, int timeoutMilliseconds = -1)
    {
        if (IsRunning)
            throw new InvalidOperationException("Process is already running");

        var startInfo = new ProcessStartInfo
        {
            FileName = _executablePath,
            Arguments = _arguments ?? string.Empty,
            UseShellExecute = !_redirectOutput,
            CreateNoWindow = _redirectOutput,
            RedirectStandardOutput = _redirectOutput,
            RedirectStandardError = _redirectOutput
        };

        _process = Process.Start(startInfo);

        if (_process == null)
            throw new InvalidOperationException($"Failed to start process: {_executablePath}");

        Console.WriteLine($"Started process '{Path.GetFileName(_executablePath)}' (PID: {_process.Id})");

        if (waitForExit)
        {
            bool exited;
            if (timeoutMilliseconds < 0)
            {
                _process.WaitForExit();
                exited = true;
            }
            else
            {
                exited = _process.WaitForExit(timeoutMilliseconds);
            }

            if (!exited)
                throw new TimeoutException($"Process did not exit within {timeoutMilliseconds}ms");

            return _process.ExitCode;
        }

        return null;
    }

    /// <summary>
    /// Stops the process if it's running
    /// </summary>
    /// <param name="forceKill">If true, kills the process immediately. Otherwise, attempts graceful shutdown.</param>
    /// <param name="timeoutMilliseconds">Timeout in milliseconds to wait for graceful shutdown before forcing kill</param>
    public void Stop(bool forceKill = false, int timeoutMilliseconds = 5000)
    {
        if (_process == null || _process.HasExited)
            return;

        try
        {
            if (forceKill)
            {
                _process.Kill();
                Console.WriteLine($"Killed process '{Path.GetFileName(_executablePath)}' (PID: {_process.Id})");
            }
            else
            {
                _process.CloseMainWindow();

                if (!_process.WaitForExit(timeoutMilliseconds))
                {
                    Console.WriteLine($"Process did not exit gracefully, forcing kill (PID: {_process.Id})");
                    _process.Kill();
                }
                else
                {
                    Console.WriteLine($"Stopped process '{Path.GetFileName(_executablePath)}' (PID: {_process.Id})");
                }
            }

            _process.WaitForExit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error stopping process: {ex.Message}");
        }
    }

    /// <summary>
    /// Waits for the process to exit
    /// </summary>
    /// <param name="timeoutMilliseconds">Timeout in milliseconds</param>
    /// <returns>True if the process exited within the timeout</returns>
    public bool WaitForExit(int timeoutMilliseconds = -1)
    {
        if (_process == null)
            throw new InvalidOperationException("Process has not been started");

        if (_process.HasExited)
            return true;

        if (timeoutMilliseconds < 0)
        {
            _process.WaitForExit();
            return true;
        }
        else
        {
            return _process.WaitForExit(timeoutMilliseconds);
        }
    }

    /// <summary>
    /// Gets the standard output if output redirection is enabled
    /// </summary>
    /// <returns>Standard output text or empty string if not redirected</returns>
    public string GetStandardOutput()
    {
        if (_process == null || !_redirectOutput)
            return string.Empty;

        return _process.StandardOutput.ReadToEnd();
    }

    /// <summary>
    /// Gets the standard error if output redirection is enabled
    /// </summary>
    /// <returns>Standard error text or empty string if not redirected</returns>
    public string GetStandardError()
    {
        if (_process == null || !_redirectOutput)
            return string.Empty;

        return _process.StandardError.ReadToEnd();
    }

    /// <summary>
    /// Checks if a process with the given name is already running
    /// </summary>
    /// <param name="processName">Process name (without .exe extension)</param>
    /// <returns>True if the process is running</returns>
    public static bool IsProcessRunning(string processName)
    {
        return Process.GetProcessesByName(processName).Length > 0;
    }

    /// <summary>
    /// Gets all running processes with the given name
    /// </summary>
    /// <param name="processName">Process name (without .exe extension)</param>
    /// <returns>Array of process IDs</returns>
    public static int[] GetRunningProcessIds(string processName)
    {
        return Process.GetProcessesByName(processName).Select(p => p.Id).ToArray();
    }

    public void Dispose()
    {
        Stop(forceKill: false);
        _process?.Dispose();
        GC.SuppressFinalize(this);
    }
}
