// File: src/DotNetDllInvoker.Core/InstanceRegistry.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Stores live instances for reuse across method invocations.
// Enables "Object Workbench" functionality similar to BlueJ/LINQPad.
//
// Depends on:
// - System.Collections.Generic
//
// Execution Risk:
// Low. State management only.

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetDllInvoker.Core;

/// <summary>
/// Represents a registered instance in the workbench.
/// </summary>
public record InstanceEntry(
    string Id,
    object Instance,
    Type Type,
    DateTime CreatedAt,
    string? Label = null);

/// <summary>
/// Registry for persisting instances across method invocations.
/// </summary>
public class InstanceRegistry
{
    private readonly Dictionary<string, InstanceEntry> _instances = new();
    private int _counter = 0;

    /// <summary>
    /// Registers an instance and returns its unique ID.
    /// </summary>
    public string Register(object instance, string? label = null)
    {
        ArgumentNullException.ThrowIfNull(instance);
        
        var id = $"obj_{++_counter}";
        var entry = new InstanceEntry(
            id, 
            instance, 
            instance.GetType(), 
            DateTime.Now, 
            label ?? instance.GetType().Name);
        
        _instances[id] = entry;
        return id;
    }

    /// <summary>
    /// Gets an instance by ID.
    /// </summary>
    public object? Get(string id)
    {
        return _instances.TryGetValue(id, out var entry) ? entry.Instance : null;
    }

    /// <summary>
    /// Gets an instance entry by ID (includes metadata).
    /// </summary>
    public InstanceEntry? GetEntry(string id)
    {
        return _instances.TryGetValue(id, out var entry) ? entry : null;
    }

    /// <summary>
    /// Removes an instance from the registry.
    /// </summary>
    public bool Remove(string id)
    {
        return _instances.Remove(id);
    }

    /// <summary>
    /// Clears all registered instances.
    /// </summary>
    public void Clear()
    {
        _instances.Clear();
        _counter = 0;
    }

    /// <summary>
    /// Gets all registered instances.
    /// </summary>
    public IEnumerable<InstanceEntry> GetAll()
    {
        return _instances.Values.OrderByDescending(e => e.CreatedAt);
    }

    /// <summary>
    /// Gets instances of a specific type.
    /// </summary>
    public IEnumerable<InstanceEntry> GetByType(Type type)
    {
        return _instances.Values.Where(e => type.IsAssignableFrom(e.Type));
    }

    /// <summary>
    /// Gets the count of registered instances.
    /// </summary>
    public int Count => _instances.Count;
}
