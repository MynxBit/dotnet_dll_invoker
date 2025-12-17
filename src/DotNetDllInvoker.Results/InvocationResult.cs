// File: src/DotNetDllInvoker.Results/InvocationResult.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// encapsules the outcome of a single method invocation.
// Guaranteed to be serializable-friendly (no raw Exceptions leaking logic).
//
// Depends on:
// - System
//
// Execution Risk:
// None. Data container.

using System;
using System.Collections.Generic;

namespace DotNetDllInvoker.Results;

public record InvocationResult
{
    public bool IsSuccess { get; init; }
    public object? ReturnValue { get; init; }
    public InvocationError? Error { get; init; }
    public TimeSpan Duration { get; init; }
    public string CapturedStdOut { get; init; } = string.Empty;
    public string CapturedStdErr { get; init; } = string.Empty;

    public static InvocationResult Success(object? value, TimeSpan duration, string stdout = "", string stderr = "")
    {
        return new InvocationResult
        {
            IsSuccess = true,
            ReturnValue = value,
            Error = null,
            Duration = duration,
            CapturedStdOut = stdout,
            CapturedStdErr = stderr
        };
    }

    public static InvocationResult Failure(InvocationError error, TimeSpan duration, string stdout = "", string stderr = "")
    {
        return new InvocationResult
        {
            IsSuccess = false,
            ReturnValue = null,
            Error = error,
            Duration = duration,
            CapturedStdOut = stdout,
            CapturedStdErr = stderr
        };
    }
}
