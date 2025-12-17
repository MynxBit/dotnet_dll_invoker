// File: src/DotNetDllInvoker.Execution/InvocationGuard.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Validates that a method is safe/ready to be invoked (e.g., not open generic).
//
// Depends on:
// - System.Reflection
// - DotNetDllInvoker.Shared
//
// Execution Risk:
// None. Logic checks.

using System;
using System.Reflection;
using DotNetDllInvoker.Shared;

namespace DotNetDllInvoker.Execution;

public static class InvocationGuard
{
    public static void EnsureInvokable(MethodInfo method)
    {
        Guard.NotNull(method, nameof(method));

        if (method.IsGenericMethodDefinition)
        {
             throw new InvalidOperationException("Cannot invoke Open Generic Method directly. Specify type arguments first.");
        }
        
        if (method.ContainsGenericParameters)
        {
             throw new InvalidOperationException("Cannot invoke method with open generic parameters.");
        }
    }
}
