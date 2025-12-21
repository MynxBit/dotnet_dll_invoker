// File: src/DotNetDllInvoker.Parameters/AutoParameterGenerator.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Generates a type-compatible value for a given parameter type.
// Handles Primitives, Enums, Arrays (empty), and Reference types (null or ctor).
//
// Depends on:
// - System.Reflection
// - DotNetDllInvoker.Shared
//
// Execution Risk:
// Low. May trigger constructors of value types or arrays.

using System;
using System.Reflection;
using DotNetDllInvoker.Shared;

namespace DotNetDllInvoker.Parameters;

public class AutoParameterGenerator
{
    public object? Generate(Type type)
    {
        Guard.NotNull(type, nameof(type));

        // 1. Check Pre-defined defaults
        if (TypeDefaultMap.TryGetDefault(type, out var safeDefault))
        {
            return safeDefault;
        }

        // 2. Handle Enums (First defined value, or 0)
        if (type.IsEnum)
        {
            var values = Enum.GetValues(type);
            return values.Length > 0 ? values.GetValue(0) : Activator.CreateInstance(type);
        }

        // 3. Handle Arrays (Empty array)
        if (type.IsArray)
        {
            return Array.CreateInstance(type.GetElementType() ?? typeof(object), 0);
        }

        // 4. Handle Nullable Reference Types or Interfaces
        if (!type.IsValueType)
        {
            return null; // Safe default for classes/interfaces is null
        }

        // 5. Structure / ValueType without specific mapping (Structs)
        try 
        {
            return Activator.CreateInstance(type);
        }
        catch (Exception ex)
        {
            // Struct creation failed - this is rare but possible for:
            // - Structs with no parameterless ctor (IL-generated)
            // - Types requiring special construction
            System.Diagnostics.Debug.WriteLine(
                $"[AutoParameterGenerator] Failed to create instance of {type.Name}: {ex.GetType().Name}");
            return null; // Fallback, though for Structs null is invalid if unboxed.
        }
    }
}
