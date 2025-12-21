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

## Unrecoverable Exceptions

The following exception types **CANNOT BE CAUGHT** and will crash the tool. This is by .NET design.

| Exception | Cause | Impact |
|-----------|-------|--------|
| `StackOverflowException` | Infinite recursion in target DLL | **Process terminates immediately** |
| `OutOfMemoryException` | Target allocates excessive memory | **Process terminates** |
| `AccessViolationException` | Native code crash in P/Invoke | **Process terminates** |
| `ExecutionEngineException` | CLR internal failure | **Process terminates** |

### Why They Can't Be Caught

These exceptions indicate fatal process state corruption. The CLR terminates the process to prevent further damage. There is no mitigation except:

1. **Run in a VM** - The VM process can crash safely
2. **Use external watchdog** - A separate process monitors and restarts if needed
3. **Accept the crash** - For analyst tools, sometimes crashing is acceptable

### Detection Before Crash

We detect and block some scenarios that commonly cause crashes:

- ✅ `DynamicMethod` / `RTDynamicMethod` - Blocked (crash risk)
- ✅ Methods with no managed body - Blocked (extern/native)
- ⚠️ P/Invoke methods - Warning shown (native crash possible)

---

## Full Access Warning

When a target DLL is invoked, it has **FULL ACCESS** to everything the current user can access:

- ✅ File system (read, write, delete)
- ✅ Network (make connections, host servers)
- ✅ Registry (Windows settings)
- ✅ Process spawning (execute other programs)
- ✅ Environment variables
- ✅ Clipboard contents

**This tool is NOT a sandbox.** If you load a malicious DLL, it can:
- Encrypt your files (ransomware)
- Steal credentials
- Install backdoors
- Damage system configuration

---

## Anti-Analysis Evasion (CRITICAL)

Malware can detect it's running under this tool and silently terminate:

### Detection Methods

| Method | What Malware Checks | Our Tool Shows |
|--------|---------------------|----------------|
| **Process Name** | `Process.GetCurrentProcess().ProcessName` | `DotNetDllInvoker.CLI` or `DotNetDllInvoker.UI` |
| **Call Stack** | `new StackTrace().ToString()` contains "DotNetDllInvoker" | ✅ Detectable |
| **Debugger** | `Debugger.IsAttached` | False (unless debugging) |
| **Parent Process** | Check parent PID | Any parent process |

### Uncatchable Kill Methods

| Method | Effect | Can We Catch? |
|--------|--------|---------------|
| `Environment.Exit(0)` | **Silent process termination** | ❌ NO |
| `Environment.FailFast()` | Crash with Watson dump | ❌ NO |
| Native `TerminateProcess()` | Immediate kill | ❌ NO |

### Why We Can't Prevent This

In .NET Core/.NET 5+:
- **AppDomain sandboxing is deprecated**
- **Code Access Security (CAS) doesn't exist**
- **AssemblyLoadContext is NOT a security boundary**

There is **NO WAY to prevent** a loaded assembly from calling `Environment.Exit()`.

### The Solution: Stealth Mode (V14)

Run untrusted DLLs via the **Pre-Warmed Worker Process**:
```
┌──────────────────┐     ┌─────────────────────┐
│ Main Invoker     │ ==> │ CLI Worker Process  │
│ (stays alive)    │     │ (can be killed)     │
└──────────────────┘     └─────────────────────┘
```

**How to Enable:**
1. Check `🕵️ Stealth Mode` in the UI header.
2. First invocation warms up the CLI worker.
3. Subsequent invocations generate **zero runtime noise** in Process Monitor.

If the target DLL calls `Environment.Exit()`, only the CLI worker dies - the UI survives!

### Accepted Risk

We accept that malware can:
- ✅ Detect it's running under our tool
- ✅ Silently exit without error
- ✅ Leave no trace in logs

**Mitigation:** Always run in a VM when analyzing potentially hostile code.

---

## Intended Use
*   Malware Analysis (in VM)
*   Debugging
*   Security Research

**Rule of Thumb:** If you wouldn't double-click the DLL, don't load it in this tool on your host.
