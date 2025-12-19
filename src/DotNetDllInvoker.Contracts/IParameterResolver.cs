// File: src/DotNetDllInvoker.Contracts/IParameterResolver.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Defines the contract for resolving parameter values before invocation.
//
// Depends on:
// - System.Reflection.ParameterInfo
//
// Used by:
// - DotNetDllInvoker.Parameters.ParameterResolver (implements)
// - DotNetDllInvoker.Core.InvocationCoordinator (depends on)
//
// Execution Risk:
// None (Parameter synthesis).

using System.Reflection;

namespace DotNetDllInvoker.Contracts;

public interface IParameterResolver
{
    object? Resolve(ParameterInfo parameter, string? userInput);
    object? GenerateDefault(ParameterInfo parameter);
}
