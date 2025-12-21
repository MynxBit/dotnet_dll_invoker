// ═══════════════════════════════════════════════════════════════════════════
// FILE: StealthInvoker.cs
// PATH: ui/DotNetDllInvoker.UI/Services/StealthInvoker.cs
// LAYER: Presentation (Service)
// ═══════════════════════════════════════════════════════════════════════════
//
// PRIMARY RESPONSIBILITY:
//   Manages the lifecycle and communication of the pre-warmed CLI worker process for Stealth Mode.
//
// SECONDARY RESPONSIBILITIES:
//   - JSON-based IPC serialization/deserialization.
//   - Process start/stop/kill management.
//
// NON-RESPONSIBILITIES:
//   - Execution logic (that happens in the CLI process).
//   - UI presentation (that happens in ViewModels).
//
// ───────────────────────────────────────────────────────────────────────────
// DEPENDENCIES:
//   - System.Diagnostics.Process -> For spawning the CLI worker.
//   - DotNetDllInvoker.Results.InvocationResult -> For returning structured data.
//
// DEPENDENTS:
//   - MainViewModel -> Uses this to execute methods when Stealth Mode is on.
//
// ───────────────────────────────────────────────────────────────────────────
// CHANGE LOG:
//   2025-12-21 - Antigravity - Created service for V14 Stealth Mode.
// ═══════════════════════════════════════════════════════════════════════════

using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using DotNetDllInvoker.Results;

namespace DotNetDllInvoker.UI.Services;

public class StealthInvoker : IDisposable
{
    private Process? _workerProcess;
    private StreamWriter? _stdin;
    private StreamReader? _stdout;
    private bool _isReady;

    /// <summary>
    /// Gets the path to the CLI executable (same directory, matching bitness).
    /// </summary>
    private static string GetCliPath()
    {
        var exeDir = AppContext.BaseDirectory;
        var bitness = Environment.Is64BitProcess ? "x64" : "x86";
        
        // Try versioned name first, then fallback
        var candidates = new[]
        {
            Path.Combine(exeDir, $"DotNetDllInvoker.CLI.{bitness}.v14.exe"),
            Path.Combine(exeDir, $"DotNetDllInvoker.CLI.{bitness}.exe"),
            Path.Combine(exeDir, "DotNetDllInvoker.CLI.exe")
        };

        foreach (var path in candidates)
        {
            if (File.Exists(path)) return path;
        }

        throw new FileNotFoundException($"CLI executable not found. Searched in: {exeDir}");
    }

    /// <summary>
    /// Starts the worker process if not already running.
    /// </summary>
    public async Task EnsureStartedAsync()
    {
        if (_isReady && _workerProcess != null && !_workerProcess.HasExited)
            return;

        var cliPath = GetCliPath();

        _workerProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = cliPath,
                Arguments = "--server",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        _workerProcess.Start();
        _stdin = _workerProcess.StandardInput;
        _stdout = _workerProcess.StandardOutput;

        // Wait for READY signal
        var ready = await _stdout.ReadLineAsync();
        if (ready != "READY")
        {
            throw new InvalidOperationException($"Worker did not signal READY. Got: {ready}");
        }

        _isReady = true;
    }

    /// <summary>
    /// Invokes a method via the worker process.
    /// </summary>
    public async Task<InvocationResult> InvokeAsync(string dllPath, string methodName, string[] args)
    {
        await EnsureStartedAsync();

        var command = new
        {
            action = "invoke",
            path = dllPath,
            method = methodName,
            args = args
        };

        var json = JsonSerializer.Serialize(command);
        await _stdin!.WriteLineAsync(json);
        await _stdin.FlushAsync();

        var responseLine = await _stdout!.ReadLineAsync();
        if (string.IsNullOrEmpty(responseLine))
        {
            return InvocationResult.Failure(
                new InvocationError { Code = "WORKER_ERROR", Message = "No response from worker" },
                TimeSpan.Zero);
        }

        try
        {
            var response = JsonSerializer.Deserialize<WorkerResponse>(responseLine);
            if (response == null)
            {
                return InvocationResult.Failure(
                    new InvocationError { Code = "PARSE_ERROR", Message = "Failed to parse worker response" },
                    TimeSpan.Zero);
            }

            if (response.Success)
            {
                return InvocationResult.Success(
                    response.ReturnValue,
                    TimeSpan.FromMilliseconds(response.DurationMs),
                    response.Stdout ?? "",
                    response.Stderr ?? "");
            }
            else
            {
                return InvocationResult.Failure(
                    new InvocationError { Code = "INVOKE_ERROR", Message = response.Error ?? "Unknown error" },
                    TimeSpan.FromMilliseconds(response.DurationMs));
            }
        }
        catch (JsonException ex)
        {
            return InvocationResult.Failure(
                new InvocationError { Code = "JSON_ERROR", Message = ex.Message },
                TimeSpan.Zero);
        }
    }

    /// <summary>
    /// Shuts down the worker process gracefully.
    /// </summary>
    public void Shutdown()
    {
        if (_stdin != null && _workerProcess != null && !_workerProcess.HasExited)
        {
            try
            {
                _stdin.WriteLine("{\"action\":\"exit\"}");
                _stdin.Flush();
                _workerProcess.WaitForExit(1000);
            }
            catch { }
        }

        _workerProcess?.Kill();
        _workerProcess?.Dispose();
        _workerProcess = null;
        _stdin = null;
        _stdout = null;
        _isReady = false;
    }

    public void Dispose()
    {
        Shutdown();
    }

    private class WorkerResponse
    {
        public bool Success { get; set; }
        public string? ReturnValue { get; set; }
        public string? Stdout { get; set; }
        public string? Stderr { get; set; }
        public string? Error { get; set; }
        public long DurationMs { get; set; }
    }
}
