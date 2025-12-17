// File: src/DotNetDllInvoker.Core/InvocationCoordinator.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Orchestrates the specific steps to invoke a method.
// Bridges the gap between ParameterResolver and Invoker.
//
// Depends on:
// - DotNetDllInvoker.Contracts
// - DotNetDllInvoker.Results
// - DotNetDllInvoker.Shared
//
// Execution Risk:
// Triggers the execution chain.

using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DotNetDllInvoker.Contracts;
using DotNetDllInvoker.Results;
using DotNetDllInvoker.Shared;

namespace DotNetDllInvoker.Core;

public class InvocationCoordinator
{
    private readonly IMethodInvoker _invoker;
    private readonly IParameterResolver _parameterResolver;

    public InvocationCoordinator(IMethodInvoker invoker, IParameterResolver parameterResolver)
    {
        _invoker = invoker;
        _parameterResolver = parameterResolver;
    }

    public async Task<InvocationResult> InvokeMethodAsync(
        MethodBase methodBase, 
        object?[]? userProvidedParameters, // Can be null if we want auto-generation
        CancellationToken cancellationToken)
    {
        Guard.NotNull(methodBase, nameof(methodBase));

        if (methodBase is not MethodInfo methodInfo)
        {
             // TODO: Handle Constructor invocation if we support it later.
             // For now, fail if not MethodInfo.
             return InvocationResult.Failure(
                 new InvocationError { Code = ErrorCodes.InvocationFailed, Message = "Only Methods are supported, not Constructors (yet)." }, 
                 System.TimeSpan.Zero);
        }

        // 1. Resolve Parameters
        var parameters = methodInfo.GetParameters();
        var invocationArgs = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var paramInfo = parameters[i];
            
            // Check if user provided ANY args, and if this index has one
            // Simple mapping for now: Positional.
            string? userInputString = null;
            if (userProvidedParameters != null && i < userProvidedParameters.Length)
            {
                userInputString = userProvidedParameters[i]?.ToString();
            }

            // Resolve (Manual or Auto)
            // Note: Our Interface says Resolve(Param, string?). 
            // If we have object input from UI, we might need a different overload or cast.
            // Assuming CLI passes strings.
            
            invocationArgs[i] = _parameterResolver.Resolve(paramInfo, userInputString);
        }

        // 2. Resolve Instance (if needed)
        object? instance = null;
        if (!methodInfo.IsStatic)
        {
            // We need an instance.
            // InstanceFactory is in Execution. Coordinator shouldn't instantiate directly if following strict layers?
            // Actually, Core creates instance via InstanceFactory? 
            // Or Execution handles instantiation?
            // InvocationEngine.Invoke takes `object? instance`.
            // So Core must provide it.
            // Core needs InstanceFactory.
            
            // For this design, we'll instantiate a fresh instance for every call (Stateless analysis).
            try 
            {
                var factory = new DotNetDllInvoker.Execution.InstanceFactory();
                instance = factory.CreateInstance(methodInfo.DeclaringType!);
            }
            catch (System.Exception ex)
            {
                 return InvocationResult.Failure(
                     InvocationError.FromException(ex, ErrorCodes.InvocationFailed), 
                     System.TimeSpan.Zero);
            }
        }

        // 3. Invoke
        return await _invoker.InvokeAsync(methodInfo, instance, invocationArgs, cancellationToken);
    }
}
