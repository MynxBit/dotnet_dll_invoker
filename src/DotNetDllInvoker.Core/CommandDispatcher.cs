// File: src/DotNetDllInvoker.Core/CommandDispatcher.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// The high-level API for the application.
// Connects UI commands to the underlying engines (Loader, Scanner, Invoker).
// Maintains ProjectState.
//
// Depends on:
// - All sub-systems.
//
// Execution Risk:
// Orchestrates execution.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DotNetDllInvoker.Contracts;
using DotNetDllInvoker.Results;
using DotNetDllInvoker.Shared;
using DotNetDllInvoker.Reflection;
using DotNetDllInvoker.Dependency;
using DotNetDllInvoker.Parameters;
using DotNetDllInvoker.Execution;

namespace DotNetDllInvoker.Core;

public class CommandDispatcher
{
    public ProjectState State { get; } = new();

    private readonly IAssemblyLoader _loader;
    private readonly IMethodEnumerator _scanner;
    private readonly IDependencyResolver _dependencyResolver;
    private readonly InvocationCoordinator _coordinator;

    public CommandDispatcher()
    {
        // Composition Root (Simple for now, could use DI container later)
        _loader = new AssemblyLoader();
        _scanner = new MethodScanner();
        _dependencyResolver = new DependencyResolver();
        
        var parameterResolver = new ParameterResolver();
        var invoker = new InvocationEngine();
        _coordinator = new InvocationCoordinator(invoker, parameterResolver);
    }

    public void LoadAssembly(string path)
    {
        Guard.NotNullOrEmpty(path, nameof(path));
        
        // Multi-DLL: Always add, don't clear.
        var loadedInfo = _loader.LoadIsolated(path);
        
        State.AddAssembly(loadedInfo);
        
        // Auto-Activate and Analyze
        ActivateAssembly(loadedInfo);
    }

    public void ActivateAssembly(LoadedAssemblyInfo info)
    {
        State.SetActiveAssembly(info);
        
        // Re-run analysis for the active assembly
        var methods = _scanner.EnumerateMethods(info.Assembly);
        State.AddMethods(methods);

        var deps = _dependencyResolver.ResolveDependencies(info.Assembly);
        State.SetDependencies(deps);
    }

    public void UnloadAssembly(LoadedAssemblyInfo info)
    {
        State.RemoveAssembly(info);
    }

    public async Task<InvocationResult> InvokeMethod(string methodName, string[]? args, CancellationToken token = default)
    {
        if (State.ActiveAssembly == null)
             throw new InvalidOperationException("No active assembly.");
             
        // Simple name matching (First match for CLI simplicity)
        var methods = State.DiscoveredMethods
            .Where(m => m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase))
            .ToList();
            
        if (!methods.Any())
             throw new ArgumentException($"Method '{methodName}' not found.");

        // Smart Resolution: Prefer method with matching parameter count
        var argCount = args?.Length ?? 0;
        var method = methods.FirstOrDefault(m => m.GetParameters().Length == argCount);
        
        // Fallback: Default to first usage if no exact parameter count match (e.g. maybe params array? or just closest match)
        if (method == null)
            method = methods.First();

        return await _coordinator.InvokeMethodAsync(method, args, token);
    }

    public async Task<InvocationResult> InvokeMethod(MethodBase method, object[]? args, CancellationToken token = default)
    {
        return await _coordinator.InvokeMethodAsync(method, args, token);
    }

    public async Task<List<InvocationResult>> InvokeAllMethods(CancellationToken token = default)
    {
        if (State.ActiveAssembly == null)
             throw new InvalidOperationException("No active assembly.");

        var results = new List<InvocationResult>();

        foreach (var method in State.DiscoveredMethods)
        {
            if (token.IsCancellationRequested) break;
            
            // We pass NULL args to force Auto-Generation
            var result = await _coordinator.InvokeMethodAsync(method, null, token);
            results.Add(result);
        }

        return results;
    }

    public object? GenerateAutoParameter(Type type)
    {
        return new ParameterResolver().GenerateDefault(type);
    }
    
    public void UnloadAll()
    {
        // _loader.UnloadAll(); // Removed from interface
        State.ClearAll();
    }
}
