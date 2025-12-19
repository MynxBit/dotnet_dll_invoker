// File: src/DotNetDllInvoker.Contracts/InvocationMode.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Defines the operation modes for the invocation engine.
//
// Depends on:
// - None
//
// Used by:
// - DotNetDllInvoker.Core.InvocationCoordinator (mode selection)
//
// Execution Risk:
// None. Enum.

namespace DotNetDllInvoker.Contracts;

public enum InvocationMode
{
    SingleMethod,
    InvokeAll
}
