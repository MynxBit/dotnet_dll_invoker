// File: src/DotNetDllInvoker.Reflection/MethodScanner.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Enumerates all methods in an assembly using strict visibility flags.
//
// Depends on:
// - System.Reflection
// - DotNetDllInvoker.Contracts
//
// Execution Risk:
// Accesses metadata. Moderate risk of triggering type initializers if types are touched deeply.
// However, GetMethods usually is safe.

using System;
using System.Collections.Generic;
using System.Reflection;
using DotNetDllInvoker.Contracts;
using DotNetDllInvoker.Shared;

namespace DotNetDllInvoker.Reflection;

public class MethodScanner : IMethodEnumerator
{
    public IEnumerable<MethodBase> EnumerateMethods(Assembly assembly)
    {
        Guard.NotNull(assembly, nameof(assembly));

        Type[] types;
        try 
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            // Best effort: return the types that COULD be loaded
            types = ex.Types.Where(t => t != null).ToArray()!;
        }
        
        foreach (var type in types)
        {
             if (type == null) continue;

             // Yield Methods
            var methods = type.GetMethods(ReflectionFlagsProvider.AllMethods);
            foreach (var method in methods)
            {
                yield return method;
            }
            
            // Yield Constructors
            var constructors = type.GetConstructors(ReflectionFlagsProvider.AllMethods);
            foreach (var ctor in constructors)
            {
                yield return ctor;
            }
        }
    }
}
