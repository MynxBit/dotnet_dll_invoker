// File: src/DotNetDllInvoker.Results/InvocationError.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Provides structured, safe error details.
// Separates the "What happened" (Code) from the "Why" (Message/StackTrace).
//
// Depends on:
// - System
//
// Execution Risk:
// None. Data container.

namespace DotNetDllInvoker.Results;

public record InvocationError
{
    public string Code { get; init; } = "UNKNOWN";
    public string Message { get; init; } = string.Empty;
    public string ExceptionType { get; init; } = string.Empty;
    public string? StackTrace { get; init; }

    public static InvocationError FromException(System.Exception ex, string code)
    {
        return new InvocationError
        {
            Code = code,
            Message = ex.Message,
            ExceptionType = ex.GetType().FullName ?? "UnknownException",
            StackTrace = ex.StackTrace
        };
    }
}
