// File: cli/DotNetDllInvoker.CLI/CliRenderer.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Handles all console output formatting.
// Ensures consistent visual style for errors, success, and tables.
//
// Depends on:
// - System.Console
// - DotNetDllInvoker.Results
//
// Execution Risk:
// None. Output only.

using System;
using DotNetDllInvoker.Results;
using DotNetDllInvoker.Contracts;

namespace DotNetDllInvoker.CLI;

public static class CliRenderer
{
    public static void WriteHeader()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("==================================================");
        Console.WriteLine("           .NET DLL INVOKER - CLI v1.0            ");
        Console.WriteLine("==================================================");
        Console.ResetColor();
        Console.WriteLine("Type 'help' for commands.");
        Console.WriteLine();
    }

    public static void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERROR] {message}");
        Console.ResetColor();
    }

    public static void WriteSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[OK] {message}");
        Console.ResetColor();
    }
    
    public static void WriteInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[INFO] {message}");
        Console.ResetColor();
    }

    public static void RenderResult(InvocationResult result)
    {
        Console.WriteLine();
        Console.WriteLine("--- Invocation Result ---");
        Console.WriteLine($"Success: {result.IsSuccess}");
        Console.WriteLine($"Duration: {result.Duration.TotalMilliseconds}ms");
        
        if (result.IsSuccess)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Return Value: {result.ReturnValue ?? "<null>"}");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error Code: {result.Error?.Code}");
            Console.WriteLine($"Message: {result.Error?.Message}");
            Console.WriteLine($"Type: {result.Error?.ExceptionType}");
            // Stacktrace hidden by default in CLI output? Or show?
            // Let's show it if available
             if (!string.IsNullOrEmpty(result.Error?.StackTrace))
             {
                 Console.WriteLine("Stack Trace:");
                 Console.WriteLine(result.Error.StackTrace);
             }
            Console.ResetColor();
        }

        if (!string.IsNullOrEmpty(result.CapturedStdOut))
        {
            Console.WriteLine("--- StdOut ---");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(result.CapturedStdOut);
            Console.ResetColor();
        }
        
        if (!string.IsNullOrEmpty(result.CapturedStdErr))
        {
            Console.WriteLine("--- StdErr ---");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(result.CapturedStdErr);
            Console.ResetColor();
        }
        Console.WriteLine("-------------------------");
        Console.WriteLine();
    }
}
