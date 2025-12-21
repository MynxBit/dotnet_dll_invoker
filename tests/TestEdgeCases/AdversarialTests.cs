// File: tests/TestEdgeCases/AdversarialTests.cs
// Purpose: Stress test the DLL Invoker with edge cases and error scenarios

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace TestEdgeCases;

/// <summary>
/// Test methods designed to expose edge cases and error handling.
/// </summary>
public class AdversarialTests
{
    // ====== BASIC TESTS ======
    
    /// <summary>Simple method that works - baseline test.</summary>
    public string HelloWorld() => "Hello from TestEdgeCases!";
    
    /// <summary>Static method test.</summary>
    public static int Add(int a, int b) => a + b;
    
    /// <summary>Void method test.</summary>
    public void DoNothing() { }
    
    /// <summary>Method with many parameters.</summary>
    public string ManyParams(int a, string b, bool c, double d, DateTime e)
        => $"a={a}, b={b}, c={c}, d={d}, e={e}";

    // ====== EXCEPTION TESTS ======
    
    /// <summary>Throws immediately - tests TargetInvocationException handling.</summary>
    public void ThrowsException()
    {
        throw new InvalidOperationException("This method always throws!");
    }
    
    /// <summary>Throws ArgumentException.</summary>
    public void ThrowsArgumentException(string input)
    {
        throw new ArgumentException("Bad argument!", nameof(input));
    }
    
    /// <summary>Throws custom exception.</summary>
    public void ThrowsCustomException()
    {
        throw new CustomTestException("Custom exception with inner", 
            new InvalidOperationException("Inner exception"));
    }

    // ====== TIMEOUT/HANG TESTS ======
    
    /// <summary>Hangs for 5 seconds - tests timeout handling.</summary>
    public void SlowMethod()
    {
        Thread.Sleep(5000);
    }
    
    /// <summary>Infinite loop - DANGER: will hang without cancellation.</summary>
    public void InfiniteLoop()
    {
        while (true) { Thread.Sleep(100); }
    }

    // ====== RETURN TYPE TESTS ======
    
    /// <summary>Returns null.</summary>
    public object? ReturnsNull() => null;
    
    /// <summary>Returns complex object.</summary>
    public ComplexResult ReturnsComplexObject()
    {
        return new ComplexResult 
        { 
            Id = 42, 
            Name = "Test", 
            Data = new byte[] { 1, 2, 3 } 
        };
    }
    
    /// <summary>Returns list.</summary>
    public List<string> ReturnsList() => new() { "one", "two", "three" };
    
    /// <summary>Returns dictionary.</summary>
    public Dictionary<string, int> ReturnsDictionary() 
        => new() { ["a"] = 1, ["b"] = 2 };

    // ====== PARAMETER EDGE CASES ======
    
    /// <summary>Nullable parameter.</summary>
    public string NullableParam(string? maybeNull) 
        => maybeNull ?? "(was null)";
    
    /// <summary>Array parameter.</summary>
    public int SumArray(int[] numbers) 
        => numbers?.Sum() ?? 0;
    
    /// <summary>Enum parameter.</summary>
    public string EnumParam(TestEnum value) 
        => $"Enum value: {value}";

    // ====== CONSOLE OUTPUT TESTS ======
    
    /// <summary>Writes to Console - tests output capture.</summary>
    public void WritesToConsole()
    {
        Console.WriteLine("This is stdout");
        Console.Error.WriteLine("This is stderr");
    }
    
    /// <summary>Writes many lines - tests output limits.</summary>
    public void WritesLotsOfOutput()
    {
        for (int i = 0; i < 1000; i++)
        {
            Console.WriteLine($"Line {i}: Lorem ipsum dolor sit amet...");
        }
    }
}

/// <summary>Static class with static methods only.</summary>
public static class StaticOnlyClass
{
    public static string GetVersion() => "1.0.0";
    public static int Multiply(int a, int b) => a * b;
}

/// <summary>Abstract class - should not be instantiable.</summary>
public abstract class AbstractClass
{
    public abstract void AbstractMethod();
    public virtual string VirtualMethod() => "base";
}

/// <summary>Class with no parameterless constructor.</summary>
public class NoDefaultConstructor
{
    private readonly string _name;
    
    public NoDefaultConstructor(string name)
    {
        _name = name;
    }
    
    public string GetName() => _name;
}

/// <summary>Class with static constructor - tests .cctor execution.</summary>
public class HasStaticConstructor
{
    public static string InitLog = "";
    
    static HasStaticConstructor()
    {
        InitLog = $"Static ctor ran at {DateTime.Now}";
        Console.WriteLine("[STATIC CTOR] HasStaticConstructor initialized!");
    }
    
    public string GetInitLog() => InitLog;
}

/// <summary>Class with P/Invoke - tests native dependency detection.</summary>
public class NativeCalls
{
    [DllImport("kernel32.dll")]
    public static extern uint GetCurrentThreadId();
    
    [DllImport("user32.dll")]
    public static extern bool MessageBeep(uint uType);
    
    public uint GetThreadId() => GetCurrentThreadId();
}

/// <summary>Generic class - tests generic handling.</summary>
public class GenericClass<T>
{
    private T? _value;
    
    public void SetValue(T value) => _value = value;
    public T? GetValue() => _value;
}

// ====== SUPPORTING TYPES ======

public class ComplexResult
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public byte[] Data { get; set; } = Array.Empty<byte>();
    
    public override string ToString() => $"ComplexResult(Id={Id}, Name={Name}, DataLen={Data.Length})";
}

public class CustomTestException : Exception
{
    public CustomTestException(string message, Exception? inner = null) 
        : base(message, inner) { }
}

public enum TestEnum
{
    None = 0,
    First = 1,
    Second = 2,
    Third = 3
}
