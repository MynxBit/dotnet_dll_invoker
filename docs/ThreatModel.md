# Threat Model

**Project:** .NET DLL Invoker
**Date:** 2025-12-17

## 1. Asset Definition
*   **The Analyst's Machine:** Primary asset to protect.
*   **The OS Process:** The boundary of current execution.
*   **User Input:** Parameters supplied for invocation.

## 2. Trust Boundaries
*   **Untrusted:** The Target DLL.
*   **Untrusted:** Dependencies of the Target DLL.
*   **Trusted:** The DotNetDllInvoker Core Logic.
*   **Trusted:** The Analyst (User).

## 3. Threat Scenarios

### A. Malicious Static Constructor (RCE on Load)
*   **Attack:** DLL contains a `.cctor` that spawns a shell or drops a file.
*   **Impact:** Code executes immediately upon assembly load or type inspection.
*   **Mitigation:**
    *   Documentation (SECURITY.md).
    *   Analyst education (Teach.md).
    *   **Recommendation:** Use strict VM isolation.

### B. Invocation Escape
*   **Attack:** Invoked method acts as a gadget to promote privileges or corrupt tool state.
*   **Impact:** Tool crash or defined behavior abuse.
*   **Mitigation:** `try/catch` blocks around `MethodInfo.Invoke`.

### C. Serialization Attack (Parameter Injection)
*   **Attack:** User provides a crafted object as a parameter that triggers a deserialization chain.
*   **Impact:** RCE during parameter resolution.
*   **Mitigation:** Parameters are constructed via `Convert.ChangeType` or specific primitive activators. No dangerous deserializers (BinaryFormatter) are used.

### D. UI Spoofing
*   **Attack:** DLL uses UI threads to paint over the specific tool inspector.
*   **Impact:** Analyst is misled about method contents.
*   **Mitigation:** WPF/Console separation. Tool UI runs largely independent of loaded assembly logic (unless UI thread is hijacked).

### E. Denial of Service (Infinite Loop)
*   **Attack:** Invoked method contains `while(true)` or equivalent hang.
*   **Impact:** The invocation thread hangs indefinitely. Modern .NET cannot safely abort threads.
*   **Mitigation:**
    *   User Warning: "Invoking unknown code may hang the application."
    *   Architecture: Invocation runs on a background thread to prevent UI freeze, but the thread itself may leak.

## 4. Accepted Risks
1.  **Loader Execution:** We accept that `Assembly.Load` carries inherent risk.
2.  **No Sandboxing:** We accept that we are not writing a managed sandbox.
3.  **Thread Leiaks:** We accept that malicious deadlocks may force a tool restart.

## 5. Security Requirements
*   All `Invoke()` calls MUST be wrapped in exception handlers.
*   No auto-loading of dependencies from internet usage.
*   Clear visual distinction between Active and Passive states.
