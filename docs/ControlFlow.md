# ControlFlow.md

## Project: .NET DLL Invoker

### 1. Entry Point
*   The application starts in idle state.
*   No DLLs are loaded.
*   No code is executed.
*   **Intentional:** Safe by default.

### 2. DLL Load Flow
**Step-by-step:**
1.  User selects **Add DLL**.
2.  Tool validates file existence and format.
3.  DLL is loaded via **Reflection** (`Assembly.Load*`).
4.  Metadata extracted (Name, Version, References).
5.  Added to "Loaded DLLs" list.

**Guarantees:**
*   **No method executed.**
*   **No constructor invoked intentionally.** (Static constructors may run implicitly).
*   Only metadata accessed.

### 3. Dependency Check Flow
**Runtime Hook:** `AssemblyLoadContext.Resolving` event is hooked to probe adjacent files.
**Trigger:** 
1.  **Diagnostic:** Right-click DLL → **Check Dependencies**.
2.  **Runtime:** When executed code requests a missing DLL.
**Flow:**
1.  Enumerate `GetReferencedAssemblies()` (Diagnostic).
2.  Probe file system in loaded assembly's directory.
3.  Load into the **Same Context** if found.
**Outcome:** Seamless execution for side-by-side dependencies.

### 4. Method Enumeration Flow
**Trigger:** DLL Selection.
**Flow:**
1.  Enumerate all Types.
2.  Enumerate all Methods (`Public | NonPublic | Static | Instance`).
3.  Categories: Constructors, Generic, Async, Overloaded.
**Outcome:** Full visibility. No filtering. No execution.

### 5. Method Selection Flow
**Trigger:** Click specific method.
**Flow:**
1.  Extract Metadata (Signature, Return, Attributes).
2.  Reconstruct/Decompile body (best-effort view).
3.  Prepare Parameter UI.
**Outcome:** Analyst inspects before invoking.

### 6. Parameter Resolution Flow
**Decision:**
*   **User Inputs?** ✅ Use them.
*   **Missing?** ❌ Trigger **Auto Parameter Generator**.

**Auto Generator:**
*   Creates type-compatible defaults (0, false, null).
*   Non-semantic (just enables execution).
**Outcome:** Ready to invoke.

### 7. Single Method Invocation Flow
**Trigger:** **Invoke Method**.
**Flow:**
1.  Check Dependency Status (Info).
2.  **Instance Creation:** (if non-static).
3.  **Invocation:** `MethodInfo.Invoke`.
4.  **Async Handling:** If return is `Task<T>`, await and unwrap result.
5.  **Capture:** Result / Exception.
**Outcome:** Surgical execution of ONE method.

### 8. Invoke All Flow (Bulk)
**Trigger:** **Invoke All**.
**Flow:**
1.  Iterate all methods.
2.  Resolve params (Auto).
3.  Invoke.
4.  Capture result.
5.  **Continue** on failure.
**Characteristics:** Sequential, Brute-force covers.

### 9. Error Handling Flow
*   Exceptions caught at every stage.
*   Surfaced to UI.
*   **Non-fatal:** App does not crash.

### 10. Exit Flow
*   Release resources.
*   No persistence.

## Mental Model
```text
Load DLL
   ↓
Inspect Metadata
   ↓
Check Dependencies
   ↓
Enumerate Methods
   ↓
Inspect Method
   ↓
Resolve Parameters
   ↓
Invoke (Single | All)
```
**Top-down flow with explicit user control at every boundary.**
