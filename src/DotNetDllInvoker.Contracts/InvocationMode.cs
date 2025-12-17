// File: src/DotNetDllInvoker.Contracts/InvocationMode.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Defines the operation modes for the invocation engine.
//
// Depends on:
// - None
//
// Execution Risk:
// None. Enum.

namespace DotNetDllInvoker.Contracts;

public enum InvocationMode
{
    SingleMethod,
    InvokeAll
}
