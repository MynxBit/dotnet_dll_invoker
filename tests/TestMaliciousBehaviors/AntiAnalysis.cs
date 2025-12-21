// File: tests/TestMaliciousBehaviors/AntiAnalysis.cs
// Purpose: Simulate malware anti-analysis techniques to test tool resilience

using System;
using System.Diagnostics;
using System.Threading;

namespace TestMaliciousBehaviors;

/// <summary>
/// Simulates malware anti-analysis techniques that try to evade/crash the tool.
/// </summary>
public static class AntiAnalysis
{
    // ====== PROCESS NAME CHECKS ======
    
    /// <summary>
    /// Checks if running under expected process name. 
    /// Real malware would crash if not running under target.
    /// </summary>
    public static string CheckProcessName()
    {
        var processName = Process.GetCurrentProcess().ProcessName;
        var expected = "malware_target"; // What real malware would expect
        
        if (processName != expected)
        {
            // Real malware might do: Environment.Exit(0);
            // or: Environment.FailFast("Detected analysis!");
            return $"DETECTED! Running under '{processName}' not '{expected}'";
        }
        return "Running under expected target";
    }
    
    /// <summary>
    /// SILENT EXIT - Calls Environment.Exit which terminates process immediately.
    /// This CANNOT be caught by try/catch!
    /// </summary>
    public static void SilentExit()
    {
        Console.WriteLine("[AntiAnalysis] About to call Environment.Exit(0)...");
        Environment.Exit(0);
        // Never reaches here
    }
    
    /// <summary>
    /// FAILFAST - Even more aggressive, writes to Windows Error Reporting.
    /// This CANNOT be caught!
    /// </summary>
    public static void FailFast()
    {
        Console.WriteLine("[AntiAnalysis] About to call Environment.FailFast...");
        Environment.FailFast("Analysis detected - terminating!");
        // Never reaches here
    }
    
    /// <summary>
    /// Checks for debugger attachment.
    /// </summary>
    public static string CheckDebugger()
    {
        if (Debugger.IsAttached)
        {
            return "DEBUGGER DETECTED!";
        }
        return "No debugger detected";
    }
    
    /// <summary>
    /// Kills the parent process (the invoker tool itself!).
    /// </summary>
    public static void KillParent()
    {
        var current = Process.GetCurrentProcess();
        // In reality malware would find parent and kill it
        // For testing, just log what it would do
        Console.WriteLine($"[AntiAnalysis] Would kill parent process of PID {current.Id}");
        // Process.GetProcessById(parentId).Kill(); // Dangerous!
    }
    
    // ====== TIMING CHECKS ======
    
    /// <summary>
    /// Sleeps for unusual time to detect sandbox fast-forwarding.
    /// </summary>
    public static string TimingCheck()
    {
        var start = DateTime.UtcNow;
        Thread.Sleep(100); // Sleep 100ms
        var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
        
        if (elapsed < 90 || elapsed > 200)
        {
            return $"TIMING ANOMALY! Expected ~100ms, got {elapsed:F0}ms - sandbox detected?";
        }
        return $"Timing OK: {elapsed:F0}ms";
    }
    
    // ====== STACKTRACE CHECKS ======
    
    /// <summary>
    /// Checks call stack for analysis tool signatures.
    /// </summary>
    public static string CheckCallStack()
    {
        var stack = new StackTrace().ToString();
        
        var suspiciousTerms = new[] { 
            "DotNetDllInvoker", 
            "Invoke", 
            "Reflection",
            "dnSpy",
            "ILSpy"
        };
        
        foreach (var term in suspiciousTerms)
        {
            if (stack.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                return $"DETECTED! Call stack contains '{term}'";
            }
        }
        return "Call stack looks clean";
    }
    
    // ====== VM / SANDBOX DETECTION ======
    
    /// <summary>
    /// Checks for VM indicators (simplified).
    /// </summary>
    public static string CheckVMIndicators()
    {
        var computerName = Environment.MachineName;
        var userName = Environment.UserName;
        
        // Common sandbox indicators
        var suspiciousNames = new[] { "SANDBOX", "VIRUS", "MALWARE", "TEST", "ANALYSIS" };
        
        foreach (var name in suspiciousNames)
        {
            if (computerName.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                userName.Contains(name, StringComparison.OrdinalIgnoreCase))
            {
                return $"VM/SANDBOX DETECTED: {computerName}/{userName}";
            }
        }
        return $"Environment looks real: {computerName}/{userName}";
    }
}
