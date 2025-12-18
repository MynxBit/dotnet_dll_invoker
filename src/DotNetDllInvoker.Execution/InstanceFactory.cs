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
        Guard.NotNull(type, nameof(type));

        if (type.IsAbstract || type.IsInterface)
        {
            throw new InvalidOperationException($"Cannot instantiate abstract type or interface: {type.Name}");
        }

        try
        {
            // Best effort instantiation
            return Activator.CreateInstance(type);
        }
        catch (TargetInvocationException ex)
        {
            // Unwrap and rethrow to generic exception or handle caller side
             throw ex.InnerException ?? ex;
        }
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
