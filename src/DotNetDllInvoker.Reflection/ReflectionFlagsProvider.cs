// File: src/DotNetDllInvoker.Reflection/ReflectionFlagsProvider.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Centralizes the BindingFlags used across the application.
// Ensures that NO methods are accidentally hidden by inconsistent flags.
//
// Depends on:
// - System.Reflection
//
// Execution Risk:
// None. Constants only.

using System.Reflection;

namespace DotNetDllInvoker.Reflection;

public static class ReflectionFlagsProvider
{
    public const BindingFlags AllMethods =
        BindingFlags.Public |
        BindingFlags.NonPublic |
        BindingFlags.Static |
        BindingFlags.Instance |
        BindingFlags.DeclaredOnly; // We only want methods declared in the specific type
}
