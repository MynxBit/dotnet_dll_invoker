// ═══════════════════════════════════════════════════════════════════════════
// FILE: StealthInvoker.cs
// PATH: ui/DotNetDllInvoker.UI/Services/StealthInvoker.cs
// LAYER: Presentation (Service)
// ═══════════════════════════════════════════════════════════════════════════
//
// PRIMARY RESPONSIBILITY:
//   Manages the "Low Noise" (formerly Stealth) invocation strategy.
//   Spawns a FRESH CLI process for *every* invocation to ensure perfect isolation.
//
// SECONDARY RESPONSIBILITIES:
//   - JSON-based Output parsing.
//   - Process argument construction.
//
// NON-RESPONSIBILITIES:
//   - Persistent process management (Removed in v16).
//
// ───────────────────────────────────────────────────────────────────────────
// CHANGE LOG:
//   2025-12-21 - Antigravity - Refactored for v16 Low Noise Mode (One-Shot Isolation).
// ═══════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DotNetDllInvoker.Results;

namespace DotNetDllInvoker.UI.Services;

public class StealthInvoker : IDisposable
{
    // In One-Shot mode, this captures the PID of the MOST RECENT execution.
    // Useful for validation ("See, it ran as PID 1234").
    private int _lastWorkerPid = -1;

    /// <summary>
    /// Gets the Process ID of the last active worker.
    /// </summary>
    public int WorkerPid => _lastWorkerPid;

    /// <summary>
    /// Gets the path to the CLI executable (same directory, matching bitness).
    /// </summary>
    private static string GetCliPath()
    {
        // Fix for Single-File Publish: AppContext.BaseDirectory points to temp extraction folder.
        // We need the location of the *actual* exe file the user clicked.
        var exeDir = Path.GetDirectoryName(Environment.ProcessPath)!;
        var bitness = Environment.Is64BitProcess ? "x64" : "x86";
        
        // Try versioned name first, then fallback
        var candidates = new[]
        {
            Path.Combine(exeDir, $"DotNetDllInvoker.CLI.{bitness}.v16.exe"),
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
    /// Invokes a method by spawning a fresh CLI process (One-Shot).
    /// </summary>
    public async Task<InvocationResult> InvokeAsync(string dllPath, string methodName, string[] args)
    {
        var cliPath = GetCliPath();
        
        // Construct Arguments: --exec "dllPath" "methodName" "arg1" "arg2"
        // Note: We use a list to let .NET handle escaping if possible, but ProcessStartInfo.Arguments is string in older .NET.
        // We will construct the string carefully manually for max campatibility.
        
        var sb = new StringBuilder();
        sb.Append("--exec ");
        sb.Append(Quote(dllPath)).Append(' ');
        sb.Append(Quote(methodName));
        
        if (args != null)
        {
            foreach (var arg in args)
            {
                sb.Append(' ').Append(Quote(arg));
            }
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = cliPath,
            Arguments = sb.ToString(),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        
        var outputBuilder = new StringBuilder();
        
        process.OutputDataReceived += (s, e) => 
        { 
            if (e.Data != null) outputBuilder.AppendLine(e.Data); 
        };

        try
        {
            process.Start();
            _lastWorkerPid = process.Id; // Capture PID for UI feedback
            
            process.BeginOutputReadLine();
            
            // Wait for exit asynchronously
            await process.WaitForExitAsync();
            
            var output = outputBuilder.ToString();
            
            // The last line of output should be the JSON result.
            // Previous lines might be "Loaded assembly...", etc.
            // We need to find the JSON line.
            
            var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var jsonLine = lines.LastOrDefault(l => l.Trim().StartsWith("{") && l.Trim().EndsWith("}"));

            if (jsonLine == null)
            {
                // Fallback: Check Stderr
                var stderr = await process.StandardError.ReadToEndAsync();
                return InvocationResult.Failure(
                    new InvocationError { Code = "CLI_ERROR", Message = $"CLI produced no JSON result.\nStdOut: {output}\nStdErr: {stderr}" },
                    TimeSpan.Zero);
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var response = JsonSerializer.Deserialize<WorkerResponse>(jsonLine, options);

            if (response == null)
            {
                return InvocationResult.Failure(
                   new InvocationError { Code = "PARSE_ERROR", Message = "Failed to parse CLI response" },
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
        catch (Exception ex)
        {
             return InvocationResult.Failure(
                new InvocationError { Code = "EXEC_ERROR", Message = ex.Message },
                TimeSpan.Zero);
        }
    }

    private static string Quote(string text)
    {
        // Simple quoting for CLI args
        if (string.IsNullOrEmpty(text)) return "\"\"";
        return "\"" + text.Replace("\"", "\\\"") + "\"";
    }

    public void Dispose()
    {
        // No persistent resources to dispose in v16
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
