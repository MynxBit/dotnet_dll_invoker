# Project Roadmap

This document tracks features identified during Gap Analysis that are slated for future versions.

## Completed Features ✅

### v16.0: Stealth Mode
*   **Status**: ✅ **Completed**
*   **Solution**: Added Pre-Warmed Worker Process architecture. UI spawns CLI with `--server` flag, communicates via stdin/stdout JSON. After first invocation, subsequent calls generate ZERO runtime noise.

### V13.3: Architecture Detection
*   **Status**: ✅ **Completed**
*   **Solution**: PE header parsing via `PEReader` to detect x86/x64/AnyCPU. Shows warning on mismatch.

### V13.2: Generic Method Support
*   **Status**: ✅ **Completed**
*   **Solution**: `ResolvedMethod` logic automatically closes open generic methods using `object` (or inferred types).

### V13.0: Constructor Injection (Smart Instantiation)
*   **Status**: ✅ **Completed**
*   **Solution**: `InstanceFactory.CreateInstanceRecursive` handles classes without default constructors by creating dependencies recursively.

### V13.1: Decompilation Restored
*   **Status**: ✅ **Completed**
*   **Solution**: Re-enabled Decompiler and IL Reader with background loading and robust error handling.

### CLI Mode (Silent Execution)
*   **Status**: ✅ **Completed**
*   **Solution**: Published separate CLI executable for headless/silent operation.

---

## 🔮 Future Features

### 1. Complex Parameter Sandbox
*   **Current Limit**: Only primitives and JSON-serializable types are supported.
*   **Proposed Solution**: Collection Editor UI for `List<T>` and Arrays.

### 2. Object Workbench (State Persistence)
*   **Current Limit**: Every "Invoke" creates a fresh instance.
*   **Proposed Solution**: "Keep Alive" checkbox to reuse instances across calls.

### 3. Native CLR Host (v16?)
*   **Feature**: C++ native loader for absolute zero noise from first invocation.
*   **Status**: 🔮 Planned (if user demand exists)

---

## 🚀 Version History
*   **v16.0**: Stealth Mode (Pre-Warmed Worker Process).
*   **V13.3**: Architecture Detection.
*   **V13.2**: Generics Fix, Stego Support, CLI Silent Mode.
*   **V13.1**: Decompilation Restored (Safe Mode).
*   **V13.0**: Smart Recursive Instantiation.
*   **V12.0**: Official x64/x86 Split Release.
