// File: src/DotNetDllInvoker.Contracts/DependencyRecord.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Data structure for reporting dependency analysis results.
//
// Depends on:
// - System.Reflection.AssemblyName
//
// Execution Risk:
// None. Data container.

using System.Reflection;

namespace DotNetDllInvoker.Contracts;

public record DependencyRecord(
    AssemblyName AssemblyName,
    DependencyStatus Status,
    string? ResolvedPath = null,
    string? ErrorMessage = null
);
