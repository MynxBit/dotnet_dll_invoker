# Teach.md

## Project: .NET DLL Invoker
**Audience:** Developers, Reverse Engineers, Malware Analysts
**Purpose:** Teach the conceptual model before touching code or UI.

### 1. What Problem This Project Solves
Modern .NET DLLs often have hidden/unused logic or multiple entry points. Traditional tools either execute the whole binary or only inspect statically. This project bridges that gap.

**Goal:** Allow precise, method-level invocation of .NET DLLs in a controlled, inspectable manner.

### 2. Core Philosophy
1.  **Explicit Control:** Nothing executes automatically. "Invoke All" is a conscious action.
2.  **Maximum Visibility:** All methods (Private/Static/etc.) are visible.
3.  **Analyst-First Design:** Prioritizes understanding over convenience. Assumes DLLs may be hostile.

### 3. Execution Model (Conceptual)
*   **Assembly Load ≠ Code Execution**
*   Tool loads DLL -> Reflects -> Allows selective invocation.
*   Execution happens ONLY when a method is explicitly invoked.

### 4. Dependency Awareness
*   **Status:** ✅ Resolved (Green) or ❌ Unresolved (Red).
*   **Behavior:** Diagnostic only. No auto-download.

### 5. Method Discovery Model
*   Enumerates **ALL** methods: Static, Instance, Public, Private, Constructors, Generic, Async, Overloaded.
*   No safety filters.

### 6. Method Inspection
*   Shows Signature, Parameters, and Best-effort body view.
*   Answers: *"What exactly will happen if I invoke this?"*

### 7. Parameter Handling Strategy
*   **Manual:** User provides explicit values.
*   **Auto-Generation:** Type-aware defaults (0, null, etc.) to enable execution. Zero semantic correctness assumed.

### 8. Invocation Modes
*   **Single Method:** Surgical execution. Primary analysis mode.
*   **Invoke All:** Brute-force coverage. Sequential.

### 9. What This Tool Is NOT
*   ❌ Not a sandbox replacement.
*   ❌ Not a malware detonator.
*   ❌ Not an automatic analyzer.
*   ❌ Not a "safe execution" guarantee.
*   ✅ **It is a precision instrument.**

### 10. Mental Model
> **Think of this tool as: A debugger without a program counter, operating at the method boundary instead of process start.**

if (YouUnderstandThis) { YouUnderstandTheProject(); }
