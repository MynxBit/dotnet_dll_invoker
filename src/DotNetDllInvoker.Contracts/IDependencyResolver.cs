// File: src/DotNetDllInvoker.Contracts/IDependencyResolver.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Defines the contract for analyzing referenced assemblies.
// Implementation should be diagnostic (predictive), not corrective (no auto-download).
//
// Depends on:
// - System.Reflection.Assembly
// - System.Collections.Generic
//
// Used by:
// - DotNetDllInvoker.Dependency.DependencyResolver (implements)
// - DotNetDllInvoker.Core.CommandDispatcher (depends on)
//
// Execution Risk:
// None. Analysis only.

using System.Collections.Generic;
using System.Reflection;

namespace DotNetDllInvoker.Contracts;

public interface IDependencyResolver
{
    IEnumerable<DependencyRecord> ResolveDependencies(Assembly assembly);
}
