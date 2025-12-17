// File: src/DotNetDllInvoker.Core/ProjectState.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Maintains the mutable state of the current session.
// Tracks loaded assembly, discovered methods, and analysis results.
//
// Depends on:
// - System.Reflection
// - System.Collections.Generic
// - DotNetDllInvoker.Contracts
//
// Execution Risk:
// None. Data holder.

using System.Collections.Generic;
using System.Reflection;
using DotNetDllInvoker.Contracts;

namespace DotNetDllInvoker.Core;

public class ProjectState
{
    // The currently active assembly for the UI
    public LoadedAssemblyInfo? ActiveAssembly { get; private set; }
    
    // All loaded assemblies
    public List<LoadedAssemblyInfo> LoadedAssemblies { get; } = new();

    // Data for the ACTIVE assembly
    public List<MethodBase> DiscoveredMethods { get; } = new();
    public List<DependencyRecord> Dependencies { get; } = new();

    public void AddAssembly(LoadedAssemblyInfo info)
    {
        LoadedAssemblies.Add(info);
        // By default, activate the newly loaded one?
        SetActiveAssembly(info);
    }

    public void RemoveAssembly(LoadedAssemblyInfo info)
    {
        if (LoadedAssemblies.Contains(info))
        {
            info.Context.Unload();
            LoadedAssemblies.Remove(info);

            if (ActiveAssembly == info)
            {
                ActiveAssembly = null;
                DiscoveredMethods.Clear();
                Dependencies.Clear();
            }
        }
    }

    public void SetActiveAssembly(LoadedAssemblyInfo info)
    {
        if (!LoadedAssemblies.Contains(info)) return; // Should likely throw

        ActiveAssembly = info;
        DiscoveredMethods.Clear();
        Dependencies.Clear();
        // NOTE: Caller (Dispatcher) must re-populate methods/deps after setting active
    }
    
    public void AddMethods(IEnumerable<MethodBase> methods)
    {
        DiscoveredMethods.AddRange(methods);
    }

    public void SetDependencies(IEnumerable<DependencyRecord> dependencies)
    {
        Dependencies.Clear();
        Dependencies.AddRange(dependencies);
    }
    
    public void ClearAll()
    {
        foreach (var asm in LoadedAssemblies)
        {
            try { asm.Context.Unload(); } catch { }
        }
        LoadedAssemblies.Clear();
        ActiveAssembly = null;
        DiscoveredMethods.Clear();
        Dependencies.Clear();
    }
}
