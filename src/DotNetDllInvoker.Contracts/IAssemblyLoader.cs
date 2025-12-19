// File: src/DotNetDllInvoker.Contracts/IAssemblyLoader.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Defines the contract for loading assemblies into the inspection context.
// implementations MUST NOT execute arbitrary code beyond implicit strict construction.
//
// Depends on:
// - System.Reflection.Assembly
//
// Used by:
// - DotNetDllInvoker.Reflection.AssemblyLoader (implements)
// - DotNetDllInvoker.Core.CommandDispatcher (depends on)
//
// Execution Risk:
// Implementation triggers Assembly.Load, which has inherent risks.

using System.Reflection;

namespace DotNetDllInvoker.Contracts;

public interface IAssemblyLoader
{
    LoadedAssemblyInfo LoadIsolated(string path);
    // UnloadAll is removed because we now manage individual contexts

}
