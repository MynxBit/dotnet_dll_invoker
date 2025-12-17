// File: src/DotNetDllInvoker.Parameters/TypeDefaultMap.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Maintains a mapping of common .NET types to safe "execution-enabling" defaults.
// e.g. int -> 0, string -> ""
//
// Depends on:
// - System
// - System.Collections.Generic
//
// Execution Risk:
// None. Data container.

using System;
using System.Collections.Generic;

namespace DotNetDllInvoker.Parameters;

public static class TypeDefaultMap
{
    private static readonly Dictionary<Type, object?> _defaults = new()
    {
        { typeof(int), 0 },
        { typeof(string), "" },
        { typeof(bool), false },
        { typeof(double), 0.0 },
        { typeof(float), 0.0f },
        { typeof(decimal), 0m },
        { typeof(long), 0L },
        { typeof(short), (short)0 },
        { typeof(byte), (byte)0 },
        { typeof(char), '\0' },
        { typeof(Guid), Guid.Empty },
        { typeof(DateTime), DateTime.MinValue },
        { typeof(TimeSpan), TimeSpan.Zero },
        { typeof(object), null }
    };

    public static bool TryGetDefault(Type type, out object? result)
    {
        return _defaults.TryGetValue(type, out result);
    }
}
