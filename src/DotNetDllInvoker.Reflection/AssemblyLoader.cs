// File: src/DotNetDllInvoker.Reflection/AssemblyLoader.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Loads assemblies into a collectible AssemblyLoadContext.
// proper handling of file locks (via using LoadFromStream logic if possible, or LoadFrom).
//
// Depends on:
// - System.Reflection
// - System.Runtime.Loader
// - DotNetDllInvoker.Contracts
// - DotNetDllInvoker.Shared
//
// Execution Risk:
// Triggers assembly loading, which may execute static constructors (.cctor).

using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using DotNetDllInvoker.Contracts;
using DotNetDllInvoker.Shared;

namespace DotNetDllInvoker.Reflection;

public class AssemblyLoader : IAssemblyLoader
{
    // No shared state. Each load is isolated.

    public LoadedAssemblyInfo LoadIsolated(string path)
    {
        Guard.NotNullOrEmpty(path, nameof(path));

        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Assembly file not found.", path);
        }

        string name = Path.GetFileNameWithoutExtension(path);
        // Create a unique, collectible context for this assembly
        var context = new AssemblyLoadContext($"Isolated_{name}_{Guid.NewGuid()}", isCollectible: true);
        
        // Setup resolution for this specific context
        var resolver = new ContextDependencyResolver(Path.GetDirectoryName(path));
        context.Resolving += resolver.OnResolving;

        try 
        {
            var assembly = context.LoadFromAssemblyPath(path);
            return new LoadedAssemblyInfo(assembly, context, path);
        }
        catch
        {
            context.Unload();
            throw;
        }
    }

    // Helper class to hold state for the event handler
    private class ContextDependencyResolver
    {
        private readonly string? _searchDirectory;

        public ContextDependencyResolver(string? searchDirectory)
        {
            _searchDirectory = searchDirectory;
        }

        public Assembly? OnResolving(AssemblyLoadContext context, AssemblyName assemblyName)
        {
            if (string.IsNullOrEmpty(_searchDirectory)) return null;

            string simpleName = assemblyName.Name + ".dll";
            string candidatePath = Path.Combine(_searchDirectory, simpleName);

            if (File.Exists(candidatePath))
            {
                return context.LoadFromAssemblyPath(candidatePath);
            }
            return null;
        }
    }
}
