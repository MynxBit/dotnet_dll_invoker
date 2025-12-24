# Architecture & Core Concepts

## 1. Core Philosophy
**Does it Execute?** Yes.
**Is it Safe?** Controlled, but inherently risky.

## 🧱 Top-Level Structure
```text
DotNetDllInvoker/
├── DotNetDllInvoker.sln
├── README.md
├── LICENSE
├── SECURITY.md
├── src/
│   ├── DotNetDllInvoker.Contracts/   [Interfaces & Immutable Records]
│   ├── DotNetDllInvoker.Shared/      [Utilities & Constants]
│   ├── DotNetDllInvoker.Results/     [Structured Outcomes]
│   ├── DotNetDllInvoker.Reflection/  [Metadata Inspection ONLY]
│   ├── DotNetDllInvoker.Dependency/  [Dependency Analysis]
│   ├── DotNetDllInvoker.Parameters/  [Input Synthesis]
│   ├── DotNetDllInvoker.Execution/   [The Execution Boundary]
│   ├── DotNetDllInvoker.Core/        [Orchestration "Brain"]
├── ui/
│   ├── DotNetDllInvoker.UI/          [WPF/Avalonia UI]
├── cli/
│   ├── DotNetDllInvoker.CLI/         [Command Line Interface]
├── docs/
│   ├── Architecture.md
│   ├── LowLevelDiagram.md
│   ├── ControlFlow.md
│   ├── ThreatModel.md
└── tests/
    ├── DotNetDllInvoker.Tests/
```

## 🧠 Core Principles (The "Legend" Perspective)
1.  **Core** never executes code.
2.  **Execution** never touches UI.
3.  **Reflection** never makes decisions.
4.  **Parameter** logic is deterministic.
5.  **Results** are structured, not strings.
6.  **Contracts** are immutable.
7.  **Shared** contains zero business logic.

## 🔩 Layer Details

### 1. src/DotNetDllInvoker.Shared
*Dumb utilities only.*
*   `Guard.cs`
*   `Logger.cs`
*   `ErrorCodes.cs`
*   `PathHelpers.cs`
*   **Rule:** If it starts to "think", it doesn't belong here.

### 2. src/DotNetDllInvoker.Contracts
*Pure interfaces and records. Zero logic.*
*   `IAssemblyLoader.cs`
*   `IMethodInvoker.cs`
*   `IParameterResolver.cs`
*   `InvocationMode.cs`
*   **Why:** Defines *what* happens, never *how*. Enables future sandboxing.

### 3. src/DotNetDllInvoker.Results
*Structured outcome handling.*
*   `InvocationResult.cs` (Includes `CapturedStdOut/StdErr`)
*   `InvocationError.cs`
*   `ResultFormatter.cs`
*   **Why:** Never leak raw exceptions to UI. Capture everything the DLL writes.

### 4. src/DotNetDllInvoker.Reflection
*Everything reflection-related, nothing executes.*
*   `AssemblyLoader.cs` (Uses `AssemblyLoadContext` with `Resolving` hook)
*   `MethodScanner.cs`
*   `SignatureBuilder.cs` (Reconstructs C# signatures from metadata)
*   `ReflectionFlagsProvider.cs` (Centralized flags)
*   **Rule:** ❌ No Invoke() ❌ No instance creation.

### 5. src/DotNetDllInvoker.Dependency
*Read-only dependency visibility.*
*   `DependencyResolver.cs`
*   `DependencyStatus.cs`
*   **Rule:** Predictive analysis only. No auto-loading from internet.

### 6. src/DotNetDllInvoker.Parameters
*Controlled parameter synthesis.*
*   `ParameterResolver.cs`
*   `AutoParameterGenerator.cs` (Type-correct defaults)
*   `TypeDefaultMap.cs`
*   **Why:** Keeps parameter generation type-safe but logic-blind.

### 7. src/DotNetDllInvoker.Execution (⚠ DANGER ZONE)
*The only place code runs.*
*   `InvocationEngine.cs` (The **ONLY** place `MethodInfo.Invoke` exists)
*   `InstanceFactory.cs` (Handles Smart Recursive Instantiation)
*   `InvocationGuard.cs`
*   **Rule:** This file should be easy to grep and audit.

### 8. src/DotNetDllInvoker.Core
*Application brain. No UI, no execution.*
*   `CommandDispatcher.cs`
*   `InvocationCoordinator.cs`
*   `ProjectState.cs`
*   **Why:** Orchestrates flows defined in ControlFlow.md. Turns architecture into behavior.

## Presentation Layers

### cli/DotNetDllInvoker.CLI
*Thin wrapper + Stealth Server.*
*   `CommandParser.cs`
*   `CliRenderer.cs`
*   **Server Mode (v16):** `--server` flag enters a JSON-based IPC loop for low-noise invocation.

### ui/DotNetDllInvoker.UI
*Presentation only.*
*   **Stealth Mode (v16):** Toggle to route invocations through pre-warmed CLI worker.
*   **Rule:** UI must never call Invoke() directly. Only talks to `Core`.
