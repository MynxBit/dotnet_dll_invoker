// File: src/DotNetDllInvoker.Shared/ErrorCodes.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Defines immutable error codes for structured result reporting.
// Ensures consistent error identification across UI and Core.
//
// Depends on:
// - None
//
// Execution Risk:
// None. Constants only.

namespace DotNetDllInvoker.Shared;

public static class ErrorCodes
{
    public const string AssemblyNotFound = "ASM_NOT_FOUND";
    public const string AssemblyLoadFailed = "ASM_LOAD_FAIL";
    public const string MethodNotFound = "MTH_NOT_FOUND";
    public const string InvocationFailed = "INVOKE_FAIL";
    public const string ParameterMismatch = "PARAM_MISMATCH";
    public const string DependencyUnresolved = "DEP_UNRESOLVED";
    public const string StaticConstructorFailure = "CCTOR_FAIL";
    public const string ExecutionBlocked = "EXEC_BLOCKED";
}
