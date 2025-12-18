# Security Policy & Threat Model

## ⚠️ Critical Warning
**DotNet DLL Invoker executes arbitrary code.**

By design, this tool uses `System.Reflection` to load assemblies and invoke methods. It is **NOT** a sandbox.

### 1. Static Constructor Risk
When an assembly is loaded (even for inspection), the .NET Runtime **MAY** execute:
*   Module Initializers
*   Static Constructors (`.cctor`) of accessed types

**Mitigation:**
*   This tool cannot prevent the CLR from running these initializers.
*   **ALWAYS** analyze unknown DLLs in an isolated Virtual Machine.

### 2. No Sandbox
This tool runs with the full privileges of the current user.
*   **Do not** run this tool as Administrator.
*   **Do not** load malware on your host machine.

---

## Threat Model

**Project:** .NET DLL Invoker
**Date:** 2025-12-17

### 1. Asset Definition
*   **The Analyst's Machine:** Primary asset to protect.
*   **The OS Process:** The boundary of current execution.
*   **User Input:** Parameters supplied for invocation.

### 2. Trust Boundaries
*   **Untrusted:** The Target DLL.
*   **Untrusted:** Dependencies of the Target DLL.
*   **Trusted:** The DotNetDllInvoker Core Logic.
*   **Trusted:** The Analyst (User).

### 3. Threat Scenarios

#### A. Malicious Static Constructor (RCE on Load)
*   **Attack:** DLL contains a `.cctor` that spawns a shell or drops a file.
*   **Impact:** Code executes immediately upon assembly load.
*   **Mitigation:** Only run in VM.

#### B. Invocation Escape
*   **Attack:** Invoked method acts as a gadget to promote privileges.
*   **Impact:** Tool crash or defined behavior abuse.
*   **Mitigation:** `try/catch` blocks around `MethodInfo.Invoke`.

#### C. Serialization Attack (Parameter Injection)
*   **Attack:** User provides crafted object triggering deserialization.
*   **Impact:** RCE during parameter resolution.
*   **Mitigation:** Parameters constructed via `Convert.ChangeType`. No `BinaryFormatter`.

#### D. Denial of Service (Infinite Loop)
*   **Attack:** Invoked method contains `while(true)`.
*   **Impact:** Thread hangs.
*   **Mitigation:** Invocation runs on background thread to prevent UI freeze.

### 4. Accepted Risks
1.  **Loader Execution:** We accept that `Assembly.Load` carries inherent risk.
2.  **No Sandboxing:** We accept that we are not writing a managed sandbox.
3.  **Thread Leaks:** We accept that malicious deadlocks may force a tool restart.

---

## Intended Use
*   Malware Analysis (in VM)
*   Debugging
*   Security Research

**Rule of Thumb:** If you wouldn't double-click the DLL, don't load it in this tool on your host.
