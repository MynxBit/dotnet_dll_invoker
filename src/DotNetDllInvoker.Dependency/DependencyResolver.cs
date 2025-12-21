// File: src/DotNetDllInvoker.Dependency/DependencyResolver.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Scans GetReferencedAssemblies and P/Invoke [DllImport] attributes.
// Checks if dependencies exist in the same directory or runtime.
// Does NOT load them. Just checks presence.
//
// Depends on:
// - System.Reflection
// - System.IO
// - System.Runtime.InteropServices
// - DotNetDllInvoker.Contracts
// - DotNetDllInvoker.Shared
//
// Execution Risk:
// Low. File system and reflection metadata only.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
        catch (NotSupportedException)
        {
            // Dynamic or in-memory assembly has no location - this is expected
            System.Diagnostics.Debug.WriteLine(
                $"[DependencyResolver] Assembly {assembly.GetName().Name} has no file location (in-memory)");
        }

        string? baseDirectory = !string.IsNullOrEmpty(assemblyLocation) 
            ? Path.GetDirectoryName(assemblyLocation) 
            : AppContext.BaseDirectory;

        // 1. Managed dependencies
        foreach (var refName in references)
        {
            yield return CheckManagedDependency(refName, baseDirectory);
        }
        
        // 2. Native dependencies (P/Invoke)
        foreach (var nativeDep in ScanPInvokeDependencies(assembly, baseDirectory))
        {
            yield return nativeDep;
        }
    }

    private DependencyRecord CheckManagedDependency(AssemblyName refName, string? baseDirectory)
    {
        // Check local file
        if (baseDirectory != null)
        {
            string localPath = Path.Combine(baseDirectory, refName.Name + ".dll");
            if (File.Exists(localPath))
            {
                return new DependencyRecord(refName, DependencyStatus.Resolved, DependencyType.Managed, localPath);
            }
        }

        // Heuristic: If it starts with System. or Microsoft., assume Resolved by runtime (optimistic).
        if (refName.Name != null && (refName.Name.StartsWith("System.") || refName.Name.StartsWith("Microsoft.")))
        {
             return new DependencyRecord(refName, DependencyStatus.Resolved, DependencyType.Managed, "Runtime Provided (Assumed)");
        }

        return new DependencyRecord(refName, DependencyStatus.Unresolved, DependencyType.Managed, null, "Not found in directory.");
    }
    
    /// <summary>
    /// Scans for P/Invoke [DllImport] attributes to find native dependencies.
    /// </summary>
    private IEnumerable<DependencyRecord> ScanPInvokeDependencies(Assembly assembly, string? baseDirectory)
    {
        var nativeLibs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        try
        {
            foreach (var type in assembly.GetTypes())
            {
                try
                {
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | 
                                                            BindingFlags.Static | BindingFlags.Instance | 
                                                            BindingFlags.DeclaredOnly))
                    {
                        // Check for DllImport attribute
                        var dllImport = method.GetCustomAttribute<DllImportAttribute>();
                        if (dllImport != null && !string.IsNullOrWhiteSpace(dllImport.Value))
                        {
                            nativeLibs.Add(dllImport.Value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Some methods may be inaccessible due to security or reflection constraints
                    System.Diagnostics.Debug.WriteLine(
                        $"[DependencyResolver] Skipped methods on {type.Name}: {ex.GetType().Name}");
                }
            }
        }
        catch (ReflectionTypeLoadException ex)
        {
            // Some types failed to load - continue with what we found
            System.Diagnostics.Debug.WriteLine(
                $"[DependencyResolver] Partial P/Invoke scan: {ex.LoaderExceptions?.Length ?? 0} types failed");
        }
        catch (Exception ex)
        {
            // GetTypes() may fail for some assemblies
            System.Diagnostics.Debug.WriteLine(
                $"[DependencyResolver] P/Invoke scan failed: {ex.GetType().Name}: {ex.Message}");
        }
        
        // Create records for each discovered native library
        foreach (var lib in nativeLibs)
        {
            var libName = lib;
            // Normalize: add .dll if not present and no extension
            if (!Path.HasExtension(libName))
            {
                libName = lib + ".dll";
            }
            
            // Create a pseudo AssemblyName for display
            var asmName = new AssemblyName(Path.GetFileNameWithoutExtension(lib));
            
            // Check if native DLL exists
            var status = DependencyStatus.Unresolved;
            string? resolvedPath = null;
            
            if (baseDirectory != null)
            {
                string localPath = Path.Combine(baseDirectory, libName);
                if (File.Exists(localPath))
                {
                    status = DependencyStatus.Resolved;
                    resolvedPath = localPath;
                }
            }
            
            // Check System32 for common Windows DLLs
            if (status == DependencyStatus.Unresolved)
            {
                var system32Path = Path.Combine(Environment.SystemDirectory, libName);
                if (File.Exists(system32Path))
                {
                    status = DependencyStatus.Resolved;
                    resolvedPath = system32Path;
                }
            }
            
            yield return new DependencyRecord(
                asmName, 
                status, 
                DependencyType.Native, 
                resolvedPath, 
                status == DependencyStatus.Unresolved ? $"Native DLL not found: {lib}" : null);
        }
    }
}

