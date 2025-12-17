// File: cli/DotNetDllInvoker.CLI/Program.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Entry point for the CLI application.
// Implements the REPL loop and handles top-level exceptions.
//
// Depends on:
// - DotNetDllInvoker.Core
// - System.Console
//
// Execution Risk:
// Orchestrates the application.

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
