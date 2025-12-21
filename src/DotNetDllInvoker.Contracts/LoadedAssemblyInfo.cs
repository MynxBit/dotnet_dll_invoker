// File: src/DotNetDllInvoker.Contracts/LoadedAssemblyInfo.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Immutable record holding a loaded assembly reference, its context, and metadata.
// Enables proper unloading via AssemblyLoadContext.
//
// Depends on:
// - System.Reflection
// - System.Runtime.Loader
//
// Used by:
// - DotNetDllInvoker.Reflection.AssemblyLoader (creates instances)
// - DotNetDllInvoker.Core.ProjectState (stores instances)
// - DotNetDllInvoker.UI.ViewModels.MainViewModel (displays in UI)
//
// Execution Risk:
// Low. Unload() triggers AssemblyLoadContext.Unload() which can have side effects.

using System.Reflection;
using System.Runtime.Loader;

namespace DotNetDllInvoker.Contracts;

public class LoadedAssemblyInfo
{
    public Assembly Assembly { get; }
    public AssemblyLoadContext Context { get; }
    public string FilePath { get; }
    public string Name { get; }

    public LoadedAssemblyInfo(Assembly assembly, AssemblyLoadContext context, string filePath)
    {
        Assembly = assembly;
        Context = context;
        FilePath = filePath;
        Name = assembly.GetName().Name ?? "Unknown";
    }

    /// <summary>
    /// Unloads the assembly from memory by unloading its AssemblyLoadContext.
    /// Note: Actual memory release happens when GC collects and there are no remaining references.
    /// </summary>
    public void Unload()
    {
        try
        {
            Context.Unload();
        }
        catch (InvalidOperationException)
        {
            // Context may not be collectible - this is expected for default context
            System.Diagnostics.Debug.WriteLine(
                $"[LoadedAssemblyInfo.Unload] Context not collectible for {Name}");
        }
        catch (Exception ex)
        {
            // Context may already be unloaded
            System.Diagnostics.Debug.WriteLine(
                $"[LoadedAssemblyInfo.Unload] Unload failed for {Name}: {ex.GetType().Name}");
        }
    }
}

