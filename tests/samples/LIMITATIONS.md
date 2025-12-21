# DotNet DLL Invoker - Complete Limitations Guide

## External Dependencies Used

| Library | Purpose | Why We Use It |
|---------|---------|---------------|
| **ICSharpCode.Decompiler** | IL disassembly & C# decompilation | Industry standard, used by ILSpy |
| **System.Reflection** | Type/method discovery | Built into .NET |
| **System.Runtime.Loader** | Assembly loading/unloading | Built into .NET |

### NOT Used
- ❌ **Dependency Walker** - Only for native DLLs, not .NET
- ❌ **Mono.Cecil** - ICSharpCode.Decompiler is better for our needs
- ❌ **dnlib** - Similar to Cecil, not needed

---

## COMPLETE LIMITATIONS LIST

### 1. Type Instantiation Limitations

| Limitation | Status | Workaround |
|------------|--------|------------|
| Abstract classes | ❌ Cannot invoke | None |
| Interfaces | ❌ Cannot invoke | None |
| No parameterless constructor | ✅ **SOLVED (V13.0)** | Auto-recursive constructor injection |
| Generic classes | ✅ **SOLVED (V13.2)** | Generic Type Closure implemented |
| Struct types | ⚠️ Limited | Auto-creates default |

### 2. Parameter Limitations

| Limitation | Status | Workaround |
|------------|--------|------------|
| ref parameters | ❌ Not supported | None |
| out parameters | ❌ Not supported | None |
| in parameters | ⚠️ Limited | Works but no optimization |
| Params arrays | ⚠️ Limited | Must provide explicit array |
| Complex objects | ⚠️ Limited | Creates with defaults |
| Nullable types | ⚠️ Works | Defaults to null |

### 3. Method Invocation Limitations

| Limitation | Status | Workaround |
|------------|--------|------------|
| Instance methods | ⚠️ New instance each call | No object persistence (Planned V15) |
| Property getters/setters | ⚠️ Treated as methods | Works but verbose |
| Extension methods | ⚠️ Listed under static type | Invoke with this param |
| Operator overloads | ❌ Not visible | Not discoverable |
| Indexers | ❌ Not visible | Not discoverable |

### 4. Security Limitations (CRITICAL)

| Attack | Prevention | Effect |
|--------|------------|--------|
| Environment.Exit() | ✅ **SOLVED (V14)** | UI survives (Stealth Mode) |
| Environment.FailFast() | ✅ **SOLVED (V14)** | UI survives (Stealth Mode) |
| StackOverflowException | ✅ **SOLVED (V14)** | UI survives (Stealth Mode) |
| OutOfMemoryException | ✅ **SOLVED (V14)** | UI survives (Stealth Mode) |
| Static constructor side effects | ✅ **SOLVED (V14)** | Runs in CLI worker, not UI |
| Process name detection | ⚠️ PARTIAL (V14) | Malware sees `DotNetDllInvoker.CLI` |
| Call stack inspection | ⚠️ PARTIAL (V14) | Malware sees generic host stack |

### 5. Dependency Analysis Limitations

| Feature | Status | Notes |
|---------|--------|-------|
| Managed (.NET) dependencies | ✅ Full | GetReferencedAssemblies() |
| P/Invoke (DllImport) | ✅ Detected | Via attribute scanning |
| Native dependencies of natives | ❌ Not scanned | Would need Dependency Walker |
| Transitive dependencies | ❌ Not scanned | Only direct references |
| Version conflicts | ❌ Not detected | May cause runtime errors |

### 6. Error Handling Limitations

| Error Type | Handled? | Notes |
|------------|----------|-------|
| Standard exceptions | ✅ Yes | Shown with stack trace |
| TargetInvocationException | ✅ Yes | Unwrapped automatically |
| ReflectionTypeLoadException | ✅ Yes | Partial types loaded |
| Infinite loops | ⚠️ Cancellation | Requires user action |
| Memory leaks | ⚠️ GC.Collect | Manual cleanup on unload |

### 7. UI/UX Limitations

| Feature | Status | Notes |
|---------|--------|-------|
| Progress indication | ❌ Limited | Only busy indicator |
| Cancellation | ⚠️ Partial | Some operations cancellable |
| Large method lists | ⚠️ Slow | No virtualization |
| Complex return display | ⚠️ ToString() | Uses object's ToString |

---

## Test Samples Location

```
tests/samples/
├── 01_BasicFunctionality/    # Happy path tests
│   ├── Sample01_Basic.dll
│   └── README.txt
├── 02_EdgeCases/             # Limitation tests
│   ├── Sample02_EdgeCases.dll
│   └── README.txt
├── 03_ErrorHandling/         # Exception tests
│   ├── Sample03_ErrorHandling.dll
│   └── README.txt
└── 04_AntiAnalysis/          # Security tests (DANGEROUS!)
    ├── Sample04_AntiAnalysis.dll
    └── README.txt
```

---

## Why We Don't Use Mono.Cecil or Dependency Walker

### Mono.Cecil
- ICSharpCode.Decompiler is built on Cecil internally
- Decompiler provides higher-level APIs (C# source, IL instructions)
- No benefit to using Cecil directly

### Dependency Walker (depends.exe)
- Only works with native (Win32) DLLs
- Does NOT understand .NET metadata
- Cannot parse IL or .NET references
- We use `Assembly.GetReferencedAssemblies()` instead
- For P/Invoke, we scan `[DllImport]` attributes

---

## Architectural Decision

We chose **simplicity over completeness**:
- Use .NET's built-in reflection (reliable, maintained)
- ICSharpCode.Decompiler for IL/C# (proven, ILSpy uses it)
- No external native tools (cross-platform concerns)
- Accept inherent .NET limitations (no sandbox)
