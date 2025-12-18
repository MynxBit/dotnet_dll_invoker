// File: src/DotNetDllInvoker.Reflection/CallGraphAnalyzer.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Analyzes IL instructions to build method call graphs.
// Extracts call/callvirt instructions to determine dependencies.
//
// Depends on:
// - System.Reflection
// - DotNetDllInvoker.Shared
//
// Execution Risk:
// None. Read-only IL analysis.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace DotNetDllInvoker.Reflection;

/// <summary>
/// Represents a node in the call graph.
/// </summary>
public record CallGraphNode(
    string Id,
    string DisplayName,
    MethodBase? Method,
    Type? DeclaringType,
    bool IsExternal = false);

/// <summary>
/// Represents an edge (method call) in the call graph.
/// </summary>
public record CallGraphEdge(
    string FromId,
    string ToId,
    string CallType); // "call", "callvirt", "newobj"

/// <summary>
/// Represents a complete call graph.
/// </summary>
public class CallGraph
{
    public Dictionary<string, CallGraphNode> Nodes { get; } = new();
    public List<CallGraphEdge> Edges { get; } = new();
    
    public void AddNode(CallGraphNode node)
    {
        Nodes.TryAdd(node.Id, node);
    }
    
    public void AddEdge(CallGraphEdge edge)
    {
        Edges.Add(edge);
    }
}

/// <summary>
/// Analyzes assemblies to build method call graphs.
/// </summary>
public class CallGraphAnalyzer
{
    /// <summary>
    /// Builds a call graph for an entire assembly.
    /// </summary>
    public CallGraph BuildGraph(Assembly assembly)
    {
        var graph = new CallGraph();
        
        try
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract && type.IsSealed) continue; // Skip static classes for perf
                
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | 
                                                        BindingFlags.Instance | BindingFlags.Static | 
                                                        BindingFlags.DeclaredOnly))
                {
                    AnalyzeMethod(method, graph);
                }
            }
        }
        catch
        {
            // Assembly may have reflection-blocked types
        }
        
        return graph;
    }

    /// <summary>
    /// Builds a call graph starting from a specific method (subgraph).
    /// </summary>
    public CallGraph BuildGraph(MethodBase method, int maxDepth = 3)
    {
        var graph = new CallGraph();
        var visited = new HashSet<string>();
        
        AnalyzeMethodRecursive(method, graph, visited, 0, maxDepth);
        
        return graph;
    }

    private void AnalyzeMethodRecursive(MethodBase method, CallGraph graph, HashSet<string> visited, int depth, int maxDepth)
    {
        if (depth > maxDepth) return;
        
        var methodId = GetMethodId(method);
        if (!visited.Add(methodId)) return;
        
        AddMethodNode(method, graph);
        
        var calledMethods = GetCalledMethods(method);
        foreach (var (calledMethod, callType) in calledMethods)
        {
            var calledId = GetMethodId(calledMethod);
            AddMethodNode(calledMethod, graph);
            graph.AddEdge(new CallGraphEdge(methodId, calledId, callType));
            
            AnalyzeMethodRecursive(calledMethod, graph, visited, depth + 1, maxDepth);
        }
    }

    private void AnalyzeMethod(MethodBase method, CallGraph graph)
    {
        var methodId = GetMethodId(method);
        AddMethodNode(method, graph);
        
        var calledMethods = GetCalledMethods(method);
        var sourceAssembly = method.DeclaringType?.Assembly;
        
        foreach (var (calledMethod, callType) in calledMethods)
        {
            // FILTER: Only include methods from the same assembly (not .NET framework)
            var calledAssembly = calledMethod.DeclaringType?.Assembly;
            if (calledAssembly == null || sourceAssembly == null) continue;
            if (calledAssembly != sourceAssembly) continue; // Skip .NET/external methods
            
            var calledId = GetMethodId(calledMethod);
            AddMethodNode(calledMethod, graph);
            graph.AddEdge(new CallGraphEdge(methodId, calledId, callType));
        }
    }

    private void AddMethodNode(MethodBase method, CallGraph graph)
    {
        var id = GetMethodId(method);
        var displayName = $"{method.DeclaringType?.Name}.{method.Name}";
        var isExternal = method.DeclaringType?.Assembly != method.Module.Assembly;
        
        graph.AddNode(new CallGraphNode(id, displayName, method, method.DeclaringType, isExternal));
    }

    private List<(MethodBase Method, string CallType)> GetCalledMethods(MethodBase method)
    {
        var result = new List<(MethodBase, string)>();
        
        try
        {
            var instructions = ILReader.Read(method);
            
            foreach (var instr in instructions)
            {
                if (instr.Operand is MethodBase calledMethod)
                {
                    var callType = instr.OpCode.Name ?? "call";
                    
                    if (callType.Contains("call") || callType.Contains("newobj"))
                    {
                        result.Add((calledMethod, callType));
                    }
                }
            }
        }
        catch
        {
            // IL reading may fail for some methods
        }
        
        return result;
    }

    private static string GetMethodId(MethodBase method)
    {
        var typeName = method.DeclaringType?.FullName ?? "?";
        var methodName = method.Name;
        var paramTypes = string.Join(",", method.GetParameters().Select(p => p.ParameterType.Name));
        return $"{typeName}::{methodName}({paramTypes})";
    }
}
