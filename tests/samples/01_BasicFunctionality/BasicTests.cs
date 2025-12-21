// Basic Functionality Test DLL
// Tests core features that should work correctly

namespace Sample01_Basic;

/// <summary>
/// Tests basic method invocation capabilities.
/// </summary>
public class Calculator
{
    // Basic arithmetic - these should all work
    public int Add(int a, int b) => a + b;
    public int Subtract(int a, int b) => a - b;
    public double Divide(double a, double b) => a / b;
    
    // Return types
    public string GetMessage() => "Hello from Calculator!";
    public bool IsPositive(int number) => number > 0;
    
    // Void method
    public void DoNothing() { }
}

/// <summary>
/// Tests static method invocation.
/// </summary>
public static class StaticHelper
{
    public static string GetVersion() => "1.0.0";
    public static DateTime GetCurrentTime() => DateTime.Now;
    public static int Max(int a, int b) => Math.Max(a, b);
}

/// <summary>
/// Tests parameter types.
/// </summary>
public class ParameterTests
{
    public string JoinStrings(string a, string b) => a + b;
    public double Average(double[] numbers) => numbers.Length > 0 ? numbers.Average() : 0;
    public string FormatDate(DateTime date) => date.ToString("yyyy-MM-dd");
}
