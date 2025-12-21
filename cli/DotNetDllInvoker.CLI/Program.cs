// ═══════════════════════════════════════════════════════════════════════════
// FILE: Program.cs
// PATH: cli/DotNetDllInvoker.CLI/Program.cs
// LAYER: Presentation (CLI)
// ═══════════════════════════════════════════════════════════════════════════
//
// PRIMARY RESPONSIBILITY:
//   Entry point for the CLI application, handling startup, mode selection, and the main run loop.
//
// SECONDARY RESPONSIBILITIES:
//   - Parsing top-level arguments (e.g., --server).
//   - Implementing the REPL (Read-Eval-Print Loop) for both interactive and server modes.
//   - Graceful shutdown and cleanup.
//
// NON-RESPONSIBILITIES:
//   - Business logic (delegated to Core).
//   - Complex command parsing (delegated to CommandParser, ideally, though some is inline).
//
// ───────────────────────────────────────────────────────────────────────────
// DEPENDENCIES:
//   - DotNetDllInvoker.Core.CommandDispatcher -> The brain of the operation.
//   - System.Console -> Standard I/O.
//
// DEPENDENTS:
//   - None (Entry point).
//
// ───────────────────────────────────────────────────────────────────────────
// CHANGE LOG:
//   2025-12-21 - Antigravity - Added --server mode for V14 Stealth Invocation.
// ═══════════════════════════════════════════════════════════════════════════

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetDllInvoker.Core;

namespace DotNetDllInvoker.CLI;

public class Program
{
    private static CommandDispatcher _dispatcher;

    public static async Task Main(string[] args)
    {
        // Initialize Core
        try 
        {
            _dispatcher = new CommandDispatcher();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CRITICAL ERROR: Failed to initialize Core. {ex.Message}");
            return;
        }

        // V14: Server Mode for Stealth Invocation
        if (args.Length > 0 && args[0] == "--server")
        {
            await RunServerMode();
            return;
        }

        CliRenderer.WriteHeader();

        // Main Loop
        bool running = true;
        while (running)
        {
            Console.Write("> ");
            var input = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(input)) continue;

            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var command = parts[0].ToLowerInvariant();
            var cmdArgs = parts.Skip(1).ToArray();

            try
            {
                await ProcessCommand(command, cmdArgs);
            }
            catch (Exception ex)
            {
                CliRenderer.WriteError(ex.Message);
            }
            
            if (command == "exit" || command == "quit") 
            {
                running = false;
            }
        }
        
        // Cleanup
        _dispatcher.UnloadAll();
    }

    /// <summary>
    /// Server mode: Reads JSON commands from stdin, executes, writes JSON results to stdout.
    /// Used by UI in Stealth Mode for low-noise invocation.
    /// </summary>
    private static async Task RunServerMode()
    {
        // Signal ready
        Console.WriteLine("READY");

        while (true)
        {
            try
            {
                var line = Console.ReadLine();
                if (string.IsNullOrEmpty(line)) continue;

                // Parse JSON command
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var cmd = System.Text.Json.JsonSerializer.Deserialize<ServerCommand>(line, options);
                if (cmd == null) continue;

                if (cmd.Action == "exit")
                {
                    Console.WriteLine("{\"status\":\"exiting\"}");
                    break;
                }

                if (cmd.Action == "invoke")
                {
                    var result = await ExecuteServerInvoke(cmd);
                    var json = System.Text.Json.JsonSerializer.Serialize(result);
                    Console.WriteLine(json);
                }
                else
                {
                    Console.WriteLine("{\"success\":false,\"error\":\"Unknown action\"}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{{\"success\":false,\"error\":\"{EscapeJson(ex.Message)}\"}}");
            }
        }

        _dispatcher.UnloadAll();
    }

    private static async Task<ServerResult> ExecuteServerInvoke(ServerCommand cmd)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Load assembly if not already loaded or different
            if (_dispatcher.State.ActiveAssembly == null || 
                !_dispatcher.State.ActiveAssembly.FilePath.Equals(cmd.Path, StringComparison.OrdinalIgnoreCase))
            {
                _dispatcher.LoadAssembly(cmd.Path!);
            }

            // Invoke
            var result = await _dispatcher.InvokeMethod(cmd.Method!, cmd.Args ?? Array.Empty<string>(), CancellationToken.None);
            sw.Stop();

            return new ServerResult
            {
                Success = result.IsSuccess,
                ReturnValue = result.ReturnValue?.ToString(),
                Stdout = result.CapturedStdOut,
                Stderr = result.CapturedStdErr,
                Error = result.Error?.Message,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new ServerResult
            {
                Success = false,
                Error = ex.Message,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
    }

    private static string EscapeJson(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");

    // JSON Models for Server Mode
    private class ServerCommand
    {
        public string? Action { get; set; }
        public string? Path { get; set; }
        public string? Class { get; set; }
        public string? Method { get; set; }
        public string[]? Args { get; set; }
    }

    private class ServerResult
    {
        public bool Success { get; set; }
        public string? ReturnValue { get; set; }
        public string? Stdout { get; set; }
        public string? Stderr { get; set; }
        public string? Error { get; set; }
        public long DurationMs { get; set; }
    }

    private static async Task ProcessCommand(string command, string[] args)
    {
        switch (command)
        {
            case "help":
                Console.WriteLine("Available commands:");
                Console.WriteLine("  load <path>       Load a DLL from the specified path.");
                Console.WriteLine("  list              List discovered methods in loaded DLL.");
                Console.WriteLine("  deps              Check dependencies.");
                Console.WriteLine("  invoke <name> [p1] [p2] ... Invoke a method by name.");
                Console.WriteLine("  clear             Unload current DLL.");
                Console.WriteLine("  exit              Exit the application.");
                break;

            case "load":
                if (args.Length < 1) throw new ArgumentException("Usage: load <path>");
                string path = string.Join(" ", args); // Allow spaces in path
                _dispatcher.LoadAssembly(path);
                CliRenderer.WriteSuccess($"Loaded assembly: {_dispatcher.State.ActiveAssembly?.Name}");
                break;

            case "list":
                if (_dispatcher.State.ActiveAssembly == null)
                {
                    CliRenderer.WriteInfo("No assembly loaded.");
                    return;
                }
                foreach (var method in _dispatcher.State.DiscoveredMethods)
                {
                    Console.WriteLine($"  {method.Name} ({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))}) -> {(method is System.Reflection.MethodInfo mi ? mi.ReturnType.Name : "void")}");
                }
                break;

            case "deps":
                if (_dispatcher.State.ActiveAssembly == null)
                {
                     CliRenderer.WriteInfo("No assembly loaded.");
                     return;
                }
                foreach (var dep in _dispatcher.State.Dependencies)
                {
                    if (dep.Status == DotNetDllInvoker.Contracts.DependencyStatus.Resolved)
                         Console.ForegroundColor = ConsoleColor.Green;
                    else 
                         Console.ForegroundColor = ConsoleColor.Red;
                    
                    Console.WriteLine($"  [{dep.Status}] {dep.AssemblyName.Name} ({dep.ResolvedPath ?? dep.ErrorMessage})");
                    Console.ResetColor();
                }
                break;

            case "invoke":
                if (args.Length < 1) throw new ArgumentException("Usage: invoke <methodName> [args...]");
                string methodName = args[0];
                string[] invokeArgs = args.Skip(1).ToArray();
                
                CliRenderer.WriteInfo($"Invoking '{methodName}'...");
                var result = await _dispatcher.InvokeMethod(methodName, invokeArgs, CancellationToken.None);
                CliRenderer.RenderResult(result);
                break;

            case "clear":
                _dispatcher.UnloadAll();
                CliRenderer.WriteSuccess("Unloaded.");
                break;
                
            case "exit":
            case "quit":
                break;

            default:
                CliRenderer.WriteError($"Unknown command: {command}");
                break;
        }
    }
}
