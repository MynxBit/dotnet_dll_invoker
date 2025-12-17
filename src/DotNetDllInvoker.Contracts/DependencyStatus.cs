// File: src/DotNetDllInvoker.Contracts/DependencyStatus.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Defines the resolution state of a discovered dependency.
//
// Depends on:
// - None
//
// Execution Risk:
// None. Enum.

namespace DotNetDllInvoker.Contracts;

public enum DependencyStatus
{
    Resolved,
    Unresolved,
    LoadError
}
