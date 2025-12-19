// File: src/DotNetDllInvoker.Shared/Guard.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Provides standardized argument validation to prevent runtime errors.
// This is a "dumb" utility with no business logic.
//
// Depends on:
// - System
//
// Used by:
// - All modules for parameter validation
// - AssemblyLoader, InvocationEngine, ParameterResolver, etc.
//
// Execution Risk:
// None. Pure logic.

using System;
using System.Runtime.CompilerServices;

namespace DotNetDllInvoker.Shared;

public static class Guard
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotNull(object argument, string argumentName)
    {
        if (argument is null)
        {
            throw new ArgumentNullException(argumentName);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotNullOrEmpty(string argument, string argumentName)
    {
        if (string.IsNullOrEmpty(argument))
        {
            throw new ArgumentException($"'{argumentName}' cannot be null or empty.", argumentName);
        }
    }
}
