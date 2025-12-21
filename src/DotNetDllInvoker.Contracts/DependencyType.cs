// File: src/DotNetDllInvoker.Contracts/DependencyType.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Defines the type of dependency (managed vs native).
//
// Depends on:
// - None
//
// Used by:
// - DependencyRecord (type field)
// - DotNetDllInvoker.Dependency.DependencyResolver
//
// Execution Risk:
// None. Enum.

namespace DotNetDllInvoker.Contracts;

/// <summary>
/// Categorizes dependencies by their nature.
/// </summary>
public enum DependencyType
{
    /// <summary>
    /// .NET managed assembly reference (via GetReferencedAssemblies).
    /// </summary>
    Managed,
    
    /// <summary>
    /// Native DLL imported via [DllImport] P/Invoke.
    /// </summary>
    Native,
    
    /// <summary>
    /// COM component imported via [ComImport].
    /// </summary>
    COM
}
