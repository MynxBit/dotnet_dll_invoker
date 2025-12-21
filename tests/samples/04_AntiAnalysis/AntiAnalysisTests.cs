// Anti-Analysis Test DLL
// Tests malware-like evasion techniques that the tool cannot prevent

using System.Diagnostics;

namespace Sample04_AntiAnalysis;

/// <summary>
/// Simulates anti-analysis techniques used by malware.
/// </summary>
public static class DetectionMethods
{
    /// <summary>
    /// Checks process name - malware uses this to detect analysis tools.
    /// </summary>
    public static string CheckProcessName()
    {
        var name = Process.GetCurrentProcess().ProcessName;
        var suspicious = new[] { "DotNetDllInvoker", "dnSpy", "ILSpy", "x64dbg" };
        
        foreach (var s in suspicious)
        {
            if (name.Contains(s, StringComparison.OrdinalIgnoreCase))
                return $"DETECTED: Running under '{name}'";
        }
        return $"Not detected. Process: {name}";
    }
    
    /// <summary>
    /// Checks call stack for analysis tool signatures.
    /// </summary>
    public static string CheckCallStack()
    {
        var stack = new StackTrace().ToString();
        
        if (stack.Contains("DotNetDllInvoker"))
            return "DETECTED: DotNetDllInvoker in call stack!";
        if (stack.Contains("Invoke"))
            return "DETECTED: Reflection invoke detected!";
            
        return "Call stack appears normal";
    }
    
    /// <summary>
    /// Checks for debugger attachment.
    /// </summary>
    public static bool IsDebuggerAttached()
    {
        return Debugger.IsAttached;
    }
}

/// <summary>
/// DANGEROUS: These methods can kill the process!
/// </summary>
public static class ProcessKillers
{
    /// <summary>
    /// DANGER: Silently terminates the ENTIRE PROCESS.
    /// This CANNOT be caught or prevented!
    /// </summary>
    public static void SilentExit()
    {
        Console.WriteLine("Goodbye!");
        Environment.Exit(0);
        // Never reaches here
    }
    
    /// <summary>
    /// DANGER: Terminates with Windows Error Reporting.
    /// This CANNOT be caught!
    /// </summary>
    public static void CrashProcess()
    {
        Environment.FailFast("Analysis detected - crashing!");
        // Never reaches here
    }
    
    /// <summary>
    /// Triggers stack overflow - CANNOT be caught.
    /// </summary>
    public static void StackOverflow()
    {
        StackOverflow(); // Infinite recursion
    }
    
    /// <summary>
    /// Allocates memory until OOM - DANGEROUS.
    /// </summary>
    public static void MemoryBomb()
    {
        var list = new List<byte[]>();
        while (true)
        {
            list.Add(new byte[1024 * 1024 * 100]); // 100MB chunks
        }
    }
}

/// <summary>
/// Static constructor that runs on first type access.
/// </summary>
public class HasStaticConstructor
{
    public static string InitMessage = "";
    
    static HasStaticConstructor()
    {
        InitMessage = $"Static constructor executed at {DateTime.Now}!";
        Console.WriteLine("[STATIC CTOR] I ran when you just loaded me!");
        
        // Malware could do anything here:
        // - Download payload
        // - Establish C2 connection
        // - Encrypt files
        // - Disable security software
    }
    
    public string GetInitMessage() => InitMessage;
}
