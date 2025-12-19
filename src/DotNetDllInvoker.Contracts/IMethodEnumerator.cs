// File: src/DotNetDllInvoker.Contracts/IMethodEnumerator.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Defines the contract for discovering methods within a loaded assembly.
//
// Depends on:
// - System.Reflection
// - System.Collections.Generic
//
// Used by:
// - DotNetDllInvoker.Reflection.MethodScanner (implements)
// - DotNetDllInvoker.Core.CommandDispatcher (depends on)
//
// Execution Risk:
// None (Inspection only).

using System.Collections.Generic;
using System.Reflection;

namespace DotNetDllInvoker.Contracts;

public interface IMethodEnumerator
{
    IEnumerable<MethodBase> EnumerateMethods(Assembly assembly);
}
