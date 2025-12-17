// File: src/DotNetDllInvoker.Execution/InstanceFactory.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Creates instances of types when invoking non-static methods.
// Uses Activator.CreateInstance or default constructors.
//
// Depends on:
// - System.Reflection
// - DotNetDllInvoker.Shared
//
// Execution Risk:
// Triggers constructors of arbitrary types. ⚠ DANGER.

using System;
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
}
