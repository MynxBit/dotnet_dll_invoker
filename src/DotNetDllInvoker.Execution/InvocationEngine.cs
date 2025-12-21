// ═══════════════════════════════════════════════════════════════════════════
// FILE: InvocationEngine.cs
// PATH: src/DotNetDllInvoker.Execution/InvocationEngine.cs
// LAYER: Business (Execution)
// ═══════════════════════════════════════════════════════════════════════════
//
// PRIMARY RESPONSIBILITY:
//   Executes a single reflected method inside a controlled execution boundary.
//   This is the ONLY location in the entire codebase where MethodInfo.Invoke() permitted.
//
// SECONDARY RESPONSIBILITIES:
//   - Capturing StdOut and StdErr during execution.
//   - Safely handling exceptions (unwrapping TargetInvocationException).
//   - Handling Task and Task<T> return types (async unwrapping).
//   - Enforcing dynamic method blocks (No Reflection.Emit).
//
// NON-RESPONSIBILITIES:
//   - Parameter resolution (delegated to ParameterResolver).
//   - Instance creation (delegated to InstanceFactory).
//
// ───────────────────────────────────────────────────────────────────────────
// DEPENDENCIES:
//   - System.Reflection.MethodInfo -> For invocation.
//   - DotNetDllInvoker.Contracts.IMethodInvoker -> Implementation contract.
//   - DotNetDllInvoker.Results.InvocationResult -> Structured return type.
//
// DEPENDENTS:
//   - InvocationCoordinator -> The only authorized consumer.
//
// ───────────────────────────────────────────────────────────────────────────
// CHANGE LOG:
//   2025-12-19 - Antigravity - Enhanced with async/await logic.
//   2025-12-21 - Antigravity - Added checks for native/extern methods.
// ═══════════════════════════════════════════════════════════════════════════

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DotNetDllInvoker.Contracts;
using DotNetDllInvoker.Results;
using DotNetDllInvoker.Shared;

namespace DotNetDllInvoker.Execution;

public class InvocationEngine : IMethodInvoker
{
    public async Task<InvocationResult> InvokeAsync(
        MethodInfo method, 
        object? instance, 
        object?[] parameters, 
        CancellationToken cancellationToken)
    {
        Guard.NotNull(method, nameof(method));
        
        // 0. Dynamic Method Safety Check
        // Dynamic methods (Reflection.Emit) may contain hostile IL that crashes the process.
        // They have no file-backed body and cannot be properly inspected.
        if (method.GetType().Name == "DynamicMethod" || method.GetType().Name == "RTDynamicMethod")
        {
            return InvocationResult.Failure(
                new InvocationError 
                { 
                    Code = ErrorCodes.ExecutionBlocked, 
                    Message = "Dynamic methods (Reflection.Emit) are not supported due to crash risk. These methods are generated at runtime and may contain unsafe IL." 
                }, 
                TimeSpan.Zero);
        }
        
        // Also check for methods with no body (abstract, interface, extern)
        try
        {
            var body = method.GetMethodBody();
            if (body == null && !method.IsAbstract && method.DeclaringType?.IsInterface != true)
            {
                // This is likely an extern or native method
                return InvocationResult.Failure(
                    new InvocationError 
                    { 
                        Code = ErrorCodes.ExecutionBlocked, 
                        Message = $"Cannot invoke method '{method.Name}' - no managed method body found. This may be a native/extern method." 
                    }, 
                    TimeSpan.Zero);
            }
        }
        catch (Exception ex)
        {
            // GetMethodBody() can throw for dynamic methods or certain special methods
            // Log but continue - the method may still be invokable
            System.Diagnostics.Debug.WriteLine(
                $"[InvocationEngine] GetMethodBody failed for {method.Name}: {ex.GetType().Name}");
        }
        
        // 1. Validation
        InvocationGuard.EnsureInvokable(method);

        // 2. Prepare Output Capture
        var originalOut = Console.Out;
        var originalErr = Console.Error;
        using var stringWriterOut = new StringWriter();
        using var stringWriterErr = new StringWriter();

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // 3. Set Capture Context (WARNING: This is process-global. Not safe for parallel invocation)
            // Implementation Decision: Architecture dictates Sequential execution.
            Console.SetOut(stringWriterOut);
            Console.SetError(stringWriterErr);

            // 4. Invocation Wrapper
            // We run on a thread pool to allow async cancellation (sort of) and UI responsiveness
            // BUT: MethodInfo.Invoke is blocking. 
            // CancellationToken is checked before start, but cannot abort Invoke easily (Thread.Abort is unsafe/obsolete).
            
            if (cancellationToken.IsCancellationRequested)
            {
                 return InvocationResult.Failure(
                     new InvocationError { Code = ErrorCodes.ExecutionBlocked, Message = "Invocation Cancelled" }, 
                     TimeSpan.Zero);
            }

            // ⚠ EXECUTION BOUNDARY ⚠
            // Code below MAY execute untrusted DLL logic.
            object? result = null;
            
            // Run on a separate thread to keep calling thread responsive? 
            // Yes, but we await it.
            
            await Task.Run(async () => 
            {
                 var rawResult = method.Invoke(instance, parameters);

                 // Async Handling: If it returns a Task, we must await it to get the real result/exception
                 if (rawResult is Task task)
                 {
                     await task.ConfigureAwait(false);
                     
                     // Check if it is Task<T> to get the value
                     var taskType = task.GetType();
                     if (taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof(Task<>))
                     {
                         result = taskType.GetProperty("Result")?.GetValue(task);
                     }
                     else
                     {
                         // Plain Task (void)
                         result = null;
                     }
                 }
                 else
                 {
                     result = rawResult;
                 }
            }, cancellationToken);

            stopwatch.Stop();

            return InvocationResult.Success(
                result, 
                stopwatch.Elapsed, 
                stringWriterOut.ToString(), 
                stringWriterErr.ToString());
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            // Unwrap TargetInvocationException to get the REAL exception from the DLL
            var actualEx = (ex is TargetInvocationException tie) ? tie.InnerException ?? tie : ex;

            // Handle Cancellation explicitly?
            if (ex is OperationCanceledException)
            {
                 return InvocationResult.Failure(
                     new InvocationError { Code = ErrorCodes.ExecutionBlocked, Message = "Invocation Cancelled" }, 
                     stopwatch.Elapsed);
            }

            return InvocationResult.Failure(
                InvocationError.FromException(actualEx, ErrorCodes.InvocationFailed), 
                stopwatch.Elapsed,
                stringWriterOut.ToString(),
                stringWriterErr.ToString());
        }
        finally
        {
            // 5. Restore Output
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
        }
    }
}
