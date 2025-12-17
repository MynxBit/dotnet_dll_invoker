# Implementation Plan

## Project Overview
**DotNet DLL Invoker:** A precision reflection-driven analysis and execution tool.
**Goal:** Explicit, safe, method-level invocation.

## Documentation Index
*   [Architecture.md](Architecture.md) - **DIRECTORY STRUCTURE & RESPONSIBILITIES** (Source of Truth)
*   [CodingStandards.md](CodingStandards.md) - File Headers & Safety Markers
*   [ControlFlow.md](ControlFlow.md) - Detailed Execution Mechanics
*   [LowLevelDiagram.md](LowLevelDiagram.md) - Execution Pipeline
*   [SECURITY.md](SECURITY.md) - Security Policy
*   [Teach.md](Teach.md) - Conceptual Model

## Runtime Compatibility
| Target | Inspection | Invocation |
| :--- | :--- | :--- |
| **Modern .NET** | ✅ Supported | ✅ Supported |
| **Legacy .NET** | ✅ Supported | ❌ **DISABLED** |

## 🏗️ Build Order (Dependencies First)

### Phase 1: Foundation (Zero Dependencies)
1.  `src/DotNetDllInvoker.Shared` - Utilities, Guards, Logger.
2.  `src/DotNetDllInvoker.Contracts` - Interfaces (IAssemblyLoader, IMethodInvoker).
3.  `src/DotNetDllInvoker.Results` - InvocationResult, Error definitions.

### Phase 2: Core Logic (Implements Contracts)
4.  `src/DotNetDllInvoker.Reflection` - Metadata inspection implementation.
5.  `src/DotNetDllInvoker.Dependency` - Resolver implementation.
6.  `src/DotNetDllInvoker.Parameters` - Generator logic.

### Phase 3: The Danger Zone
7.  `src/DotNetDllInvoker.Execution` - **InvocationEngine.cs**. (Strict Review Required)

### Phase 4: Orchestration
8.  `src/DotNetDllInvoker.Core` - CommandDispatcher, State Management.

### Phase 5: Presentation
9.  `cli/DotNetDllInvoker.CLI` - Console Interface (First runnable target).
10. `ui/DotNetDllInvoker.UI` - Desktop UI (WPF/Avalonia).

### Phase 6: Verification
11. `tests/DotNetDllInvoker.Tests` - Unit & Safety Regressions.

## Requirements Checklist
*   [ ] **Setup Solution:** Create SLN and all Projects.
*   [ ] **Implement Shared & Contracts:** Define the immutable spine.
*   [ ] **Implement Reflection:** Loading & Enumeration (No execution).
*   [ ] **Implement Parameters:** Type-safe defaults.
*   [ ] **Implement Execution:** The Single Boundary.
*   [ ] **Implement Core:** Wiring it all together.
*   [ ] **Implement CLI:** Interactive console loop.
*   [ ] **Verify:** Run "Hello World" invocation.
