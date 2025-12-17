// File: src/DotNetDllInvoker.Dependency/DependencyResolver.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Scans GetReferencedAssemblies and checks if they exist in the same directory or runtime.
// Does NOT load them. Just checks presence.
//
// Depends on:
// - System.Reflection
// - System.IO
// - DotNetDllInvoker.Contracts
// - DotNetDllInvoker.Shared
//
// Execution Risk:
// Low. File system usage.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DotNetDllInvoker.Contracts;
using DotNetDllInvoker.Shared;

namespace DotNetDllInvoker.Dependency;

public class DependencyResolver : IDependencyResolver
{
    // Implementation note:
    // We want to simulate where the runtime would look (GAC is gone in Core, so mostly app base or adjacent file).
    
    public IEnumerable<DependencyRecord> ResolveDependencies(Assembly assembly)
    {
        Guard.NotNull(assembly, nameof(assembly));

        var references = assembly.GetReferencedAssemblies();
        string assemblyLocation = string.Empty;

        try
        {
            assemblyLocation = assembly.Location;
        }
        catch 
        {
             // Dynamic or in-memory assembly might throw
        }

        string? baseDirectory = !string.IsNullOrEmpty(assemblyLocation) 
            ? Path.GetDirectoryName(assemblyLocation) 
            : AppContext.BaseDirectory;

        foreach (var refName in references)
        {
            yield return CheckDependency(refName, baseDirectory);
        }
    }

    private DependencyRecord CheckDependency(AssemblyName refName, string? baseDirectory)
    {
        // 1. Check if loaded in current domain/context (Best effort)
        // Note: For analysis, we care if the FILE exists mostly, or if runtime provides it.
        
        // Check local file
        if (baseDirectory != null)
        {
            string localPath = Path.Combine(baseDirectory, refName.Name + ".dll");
            if (File.Exists(localPath))
            {
                return new DependencyRecord(refName, DependencyStatus.Resolved, localPath);
            }
        }

        // Check Runtime (Trusted Platform Assemblies) - Hard to check strictly without loading.
        // We can try Assembly.Load(refName) in a try-catch, but that loads it.
        // We promised read-only.
        
        // Heuristic: If it starts with System. or Microsoft., assume Resolved by runtime (optimistic).
        // Real approach: Parse *.deps.json? Too complex.
        
        if (refName.Name != null && (refName.Name.StartsWith("System.") || refName.Name.StartsWith("Microsoft.")))
        {
             return new DependencyRecord(refName, DependencyStatus.Resolved, "Runtime Provided (Assumed)");
        }

        return new DependencyRecord(refName, DependencyStatus.Unresolved, null, "Not found in directory.");
    }
}
