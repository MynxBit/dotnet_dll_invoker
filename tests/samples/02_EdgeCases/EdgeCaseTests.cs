// Edge Cases Test DLL
// Tests limitations and edge cases that may fail or behave unexpectedly

using System.Runtime.InteropServices;

namespace Sample02_EdgeCases;

/// <summary>
/// Tests abstract class handling - CANNOT be instantiated.
/// </summary>
public abstract class AbstractClass
{
    public abstract void AbstractMethod();
    public virtual string VirtualMethod() => "base implementation";
}

/// <summary>
/// Tests interface - CANNOT be instantiated.
/// </summary>
public interface ITestInterface
{
    void DoSomething();
    string GetValue();
}

/// <summary>
/// Tests generic class - Limited support.
/// </summary>
public class GenericContainer<T>
{
    private T? _value;
    public void SetValue(T value) => _value = value;
    public T? GetValue() => _value;
}

/// <summary>
/// Tests class without parameterless constructor.
/// </summary>
public class NoDefaultCtor
{
    private readonly string _required;
    
    // No parameterless constructor!
    public NoDefaultCtor(string required)
    {
        _required = required;
    }
    
    public string GetRequired() => _required;
}

/// <summary>
/// Tests P/Invoke (native) methods.
/// </summary>
public class NativeMethods
{
    [DllImport("kernel32.dll")]
    public static extern uint GetCurrentThreadId();
    
    [DllImport("kernel32.dll")]
    public static extern uint GetTickCount();
    
    // Wrapper that uses P/Invoke internally
    public uint GetThreadIdWrapper() => GetCurrentThreadId();
}

/// <summary>
/// Tests pass-by-reference parameters - NOT SUPPORTED.
/// </summary>
public class RefOutMethods
{
    // ref parameter - NOT SUPPORTED
    public void IncrementRef(ref int value) => value++;
    
    // out parameter - NOT SUPPORTED
    public void GetValue(out int value) => value = 42;
    
    // in parameter - Limited support
    public int DoubleIt(in int value) => value * 2;
}

/// <summary>
/// Tests nullable and complex types.
/// </summary>
public class ComplexTypes
{
    public string? GetNullable() => null;
    public int? GetNullableInt() => 42;
    
    // Nested class
    public class NestedClass
    {
        public string GetFromNested() => "I'm nested!";
    }
}
