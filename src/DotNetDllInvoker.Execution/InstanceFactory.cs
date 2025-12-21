// File: src/DotNetDllInvoker.Execution/InstanceFactory.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Creates instances of types when invoking non-static methods.
// Supports default constructors and parameterized constructor injection.
//
// Depends on:
// - System.Reflection
// - DotNetDllInvoker.Shared
//
// Execution Risk:
// Triggers constructors of arbitrary types. ⚠ DANGER.

using System;
using System.Linq;
using System.Reflection;
using DotNetDllInvoker.Shared;

namespace DotNetDllInvoker.Execution;

public class InstanceFactory
{
    // ⚠ EXECUTION BOUNDARY ⚠
    // This method executes constructors of unknown types.
    public object? CreateInstance(Type type)
    {
        return CreateInstanceRecursive(type, 0);
    }

    private object? CreateInstanceRecursive(Type type, int depth)
    {
        Guard.NotNull(type, nameof(type));

        // 1. Stack Guard
        if (depth > 10) return null; // Give up

        // 2. Primitives & Value Types (Enum, Struct, Int, Bool)
        if (type == typeof(string)) return string.Empty;
        if (type.IsValueType) return Activator.CreateInstance(type); // Returns 0/False/DefaultStruct
        if (type.IsArray) return Array.CreateInstance(type.GetElementType() ?? typeof(object), 0);

        if (type.IsAbstract || type.IsInterface)
        {
            throw new InvalidOperationException($"Cannot instantiate abstract type or interface: {type.Name}");
        }

        // 3. Try Default Parameterless Constructor first
        if (HasParameterlessConstructor(type))
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException ?? ex;
            }
        }

        // 4. Try Greedy Constructors (Constructor Injection)
        var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .OrderBy(c => c.GetParameters().Length) // Start with simplest
            .ToArray();

        foreach (var ctor in constructors)
        {
            try
            {
                var paramsInfo = ctor.GetParameters();
                var args = new object?[paramsInfo.Length];

                for (int i = 0; i < paramsInfo.Length; i++)
                {
                    // Recursively create dependencies (Mocking)
                    args[i] = CreateInstanceRecursive(paramsInfo[i].ParameterType, depth + 1);
                }

                return ctor.Invoke(args);
            }
            catch
            {
                // If this constructor fails, try the next one
                continue;
            }
        }

        // 5. Fail
        throw new InvalidOperationException($"Could not instantiate {type.Name}. No default constructor, and failed to auto-inject dependencies.");
    }

    /// <summary>
    /// Creates an instance using a specific constructor with provided arguments.
    /// ⚠ EXECUTION BOUNDARY ⚠
    /// </summary>
    public object? CreateInstance(Type type, ConstructorInfo constructor, object?[] args)
    {
        Guard.NotNull(type, nameof(type));
        Guard.NotNull(constructor, nameof(constructor));

        if (type.IsAbstract || type.IsInterface)
        {
            throw new InvalidOperationException($"Cannot instantiate abstract type or interface: {type.Name}");
        }

        try
        {
            return constructor.Invoke(args);
        }
        catch (TargetInvocationException ex)
        {
            throw ex.InnerException ?? ex;
        }
    }

    /// <summary>
    /// Gets all public constructors for a type.
    /// </summary>
    public static ConstructorInfo[] GetConstructors(Type type)
    {
        Guard.NotNull(type, nameof(type));
        return type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
    }

    /// <summary>
    /// Checks if a type has a parameterless constructor.
    /// </summary>
    public static bool HasParameterlessConstructor(Type type)
    {
        Guard.NotNull(type, nameof(type));
        return type.GetConstructor(Type.EmptyTypes) != null;
    }
}
