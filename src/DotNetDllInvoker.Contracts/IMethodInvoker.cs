// File: src/DotNetDllInvoker.Contracts/IMethodInvoker.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Defines the contract for the execution boundary.
// Implementations are responsible for the physical execution of code.
//
// Depends on:
// - System.Reflection.MethodInfo
// - DotNetDllInvoker.Results.InvocationResult
//
// Used by:
// - DotNetDllInvoker.Execution.InvocationEngine (implements)
// - DotNetDllInvoker.Core.InvocationCoordinator (depends on)
//
// Execution Risk:
// Implementations WILL execute arbitrary code. ⚠

using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DotNetDllInvoker.Results;

namespace DotNetDllInvoker.Contracts;

public interface IMethodInvoker
{
    Task<InvocationResult> InvokeAsync(
        MethodInfo method, 
        object? instance, 
        object?[] parameters, 
        CancellationToken cancellationToken);
}
