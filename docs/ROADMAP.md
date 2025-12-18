# Project Roadmap

This document tracks features identified during Gap Analysis that are slated for future versions (V11+).

## 🚀 Future Features

### 1. Generic Method Support (`<T>`)
*   **Current Limit**: Cannot invoke methods with generic type parameters (e.g., `Verify<T>(T item)`).
*   **Proposed Solution**: Add a "Type Selector" UI in the method cockpit to allow users to specify concrete types (int, string, existing types) for `T` before invocation.

### 2. Complex Parameter Sandbox
*   **Current Limit**: Only `int`, `bool`, `string` are supported.
*   **Proposed Solution**:
    *   **JSON Input**: Allow passing complex objects via JSON deserialization.
    *   **Collection Editor**: specific UI for `List<T>` and Arrays.

### 3. Object Workbench (State Persistence)
*   **Current Limit**: Every "Invoke" creates a fresh instance. No state is shared between calls.
*   **Proposed Solution**:
    *   **Instance Registry**: "Keep Alive" checkbox.
    *   **Object Explorer**: View and manipulate live instances in memory (similar to BlueJ or LINQPad).

    - [ ] **Constructor Injection**: Wizard for instantiating classes

### 5. Dependency Call Graph
*   **Feature**: Visual Graph of Method Calls (Who calls Who?).
*   **UI Design**:
    *   **Entry Point**: Right-click on Assembly in Sidebar -> "Show Call Graph".
    *   **Separate Window**: Graph opens in a dedicated maximized window.
    *   **Interactive Nodes**: Click a node to view its IL/Pseudocode in a side panel.
    *   **Scope**: Show dependencies for the entire DLL or subgraph for a single method.

---

## 📅 Immediate Priorities (V10/V11)
*   ✅ **x64 Support** (Complete)
*   ✅ **x86 Support** (Complete) - 32-bit builds available in `dist/`.
