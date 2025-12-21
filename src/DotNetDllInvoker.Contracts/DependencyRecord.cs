// File: src/DotNetDllInvoker.Contracts/DependencyRecord.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Data structure for reporting dependency analysis results.
//
// Depends on:
// - System.Reflection.AssemblyName
//
// Used by:
// - DotNetDllInvoker.Dependency.DependencyResolver (creates)
// - DotNetDllInvoker.Core.ProjectState (stores)
// - DotNetDllInvoker.UI.MainViewModel (displays)
//
// Execution Risk:
// None. Data container.

using System.Reflection;

namespace DotNetDllInvoker.Contracts;

public record DependencyRecord(
    AssemblyName AssemblyName,
    DependencyStatus Status,
    DependencyType Type = DependencyType.Managed,
    string? ResolvedPath = null,
    string? ErrorMessage = null
);
