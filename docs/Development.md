# Developer Guide & Standards

## 1. Coding Standards & Commitments

### 1.1 Header Requirements
Every source file MUST start with the standard header to track responsibility and risk.

```csharp
// File: src/DotNetDllInvoker.Execution/InvocationEngine.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Executes reflected methods inside a strict execution boundary.
//
// Depends on:
// - System.Reflection
//
// Execution Risk:
// ⚠ This file executes untrusted code. Handle with care.

namespace DotNetDllInvoker.Execution;
```

### 1.2 Design Principles
1.  **Explicit Namespace**: Must match directory structure.
2.  **Explicit Dependencies**: List key dependencies in comments.
3.  **Execution Boundary Markers**: Use `// ⚠ EXECUTION BOUNDARY ⚠` for any code that calls `Invoke()`.

---

## 2. UI Design Architecture (WPF)

**Philosophy:** The UI is a "Cockpit". Active, Responsive, and Honest.

### Structure
*   **Views**: `src/DotNetDllInvoker.UI/Views`
*   **ViewModels**: `src/DotNetDllInvoker.UI/ViewModels` (MVVM)
*   **Zero Dependencies**: We use our own lightweight `RelayCommand` and `ViewModelBase`.

### Dynamic Input Generation
Since parameters are unknown at compile time, we use `ItemTemplateSelector` to map `ParameterViewModel` types to controls:
*   `StringParameterVM` -> `TextBox`
*   `BoolParameterVM` -> `CheckBox`
*   `EnumParameterVM` -> `ComboBox`

### Styling
*   **Theme**: Premium Dark Mode (`#1E1E1E`, `#252526`).
*   **Typography**: Segoe UI / Consolas (for code).

---

## 3. Build Instructions

### Prerequisites
*   .NET 10 SDK

### A. Standalone Release (Recommended)
Bundles the .NET Runtime (~130MB). Runs anywhere on Windows x64.
```powershell
dotnet publish ui/DotNetDllInvoker.UI/DotNetDllInvoker.UI.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o bin/Publish_Standalone
```

### B. Framework-Dependent Release ("Light")
Requires .NET Runtime installed. Tiny size (~3MB).
```powershell
dotnet publish ui/DotNetDllInvoker.UI/DotNetDllInvoker.UI.csproj -c Release -r win-x64 -p:SelfContained=false -p:PublishSingleFile=true -o bin/Publish_Light
```

### C. CLI Release (Silent Mode)
Headless build for automation.
```powershell
dotnet publish cli/DotNetDllInvoker.CLI/DotNetDllInvoker.CLI.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o bin/Publish_CLI
```

---

## 4. Contributing
*   **Branching**: Stick to `main` for releases. Feature branches for work.
*   **Testing**: Run `DotNetDllInvoker.Tests` before PR.
