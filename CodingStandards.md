# Coding Standards & Commitments

## 1. Absolute Project File Path
Must be the first line of every file.
```csharp
// File: src/DotNetDllInvoker.Execution/InvocationEngine.cs
```

## 2. Explicit Namespace
Must match directory structure exactly.
```csharp
namespace DotNetDllInvoker.Execution;
```

## 3. Explicit Dependencies
Must list key dependencies and rationale in comments.
```csharp
// Depends on:
// - System.Reflection.MethodInfo (for invocation)
// - DotNetDllInvoker.Contracts.IMethodInvoker (contract boundary)
```

## 4. Responsibility Statement
1-2 lines explaining *why* the file exists.
```csharp
// Responsibility:
// Executes a single reflected method inside a controlled execution boundary.
```

## 5. Execution Boundary Markers
Required for any file capable of code execution.
```csharp
// ⚠ EXECUTION BOUNDARY ⚠
// Code below MAY execute untrusted DLL logic.
```

## 6. Canonical Header Template
```csharp
// File: src/DotNetDllInvoker.Execution/InvocationEngine.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Executes reflected methods inside a strict execution boundary.
// This is the ONLY location where MethodInfo.Invoke() is allowed.
//
// Depends on:
// - System.Reflection
// - DotNetDllInvoker.Contracts.IMethodInvoker
// - DotNetDllInvoker.Results.InvocationResult
//
// Execution Risk:
// ⚠ This file executes untrusted code. Handle with care.

namespace DotNetDllInvoker.Execution;
```
