# WPF UI Design Architecture
**Project:** DotNet DLL Invoker

## 1. Core Philosophy
The UI must be **Active**, **Responsive**, and **Honest**.
It serves as a "Cockpit" for the backend engine.

*   **Pattern:** MVVM (Model-View-ViewModel).
*   **Styling:** Modern Dark Theme (Custom ResourceDictionary).
*   **Threading:** STRICT UI/Background thread separation.

## 2. Structure

### A. The Views (`src/DotNetDllInvoker.UI/Views`)
1.  **MainWindow:** The shell. Grid-based layout.
2.  **AssemblyExplorerView:** Left sidebar. TreeView or List of methods.
3.  **MethodDetailView:** Right pane. Details of the selected method.
4.  **ExecutionLogView:** Bottom pane. Real-time(ish) output status.

### B. The ViewModels (`src/DotNetDllInvoker.UI/ViewModels`)
*   **MainViewModel:** Orchestrator. Holds `ProjectState` wrapper.
*   **MethodViewModel:** Wraps `MethodInfo`. Exposes `Name`, `Parameters`, `InvokeCommand`.
*   **ParameterViewModel:** The tricky part. Wraps `ParameterInfo`.
    *   Subtypes: `StringParameterVM`, `IntParameterVM`, `BoolParameterVM`.
*   **ResultViewModel:** Wraps `InvocationResult`.

## 3. The "Hard Part": Dynamic Input Generation
Since we don't know the parameters at compile time, we cannot hardcode TextBoxes.
We will use an `ItemsControl` bound to `ObservableCollection<ParameterViewModel>`.

**DataTemplateSelector Strategy:**
We will create a `ParameterTemplateSelector` that picks the correct UI control based on the ViewModel type:
*   `StringParameterVM` -> `TextBox`
*   `BoolParameterVM` -> `CheckBox` / `ToggleSwitch`
*   `EnumParameterVM` -> `ComboBox`
*   `Int/DoubleVM` -> `TextBox` (with Numeric validation behavior)
*   `Complex/ObjectVM` -> `JsonInput` (Future) or ReadOnly label.

## 4. Execution Flow (The "Hang" Prevention)
1.  User clicks "Invoke".
2.  UI locks the "Invoke" button (`IsBusy = true`).
3.  `MainViewModel` gathers values from `ParameterViewModels`.
4.  **CRITICAL:** Calls `CommandDispatcher.InvokeMethod` inside `Task.Run` or uses the async `InvokeAsync` directly (which is already `Task`-based).
5.  UI shows "Running...".
6.  Upon completion, `IsBusy = false` and `ResultViewModel` is populated.

## 5. Styling (Premium Dark Mode)
We will create `Themes/Generic.xaml`:
*   **Colors:** Deep Grays (`#1E1E1E`, `#252526`), Accent Blue (`#007ACC`).
*   **Controls:** Flat styles for Buttons, TextBoxes.
*   **Typography:** Segoe UI / Consolas for code.

## 6. Zero-Dependency Decision
We will write our own lightweight `RelayCommand` and `ViewModelBase` to avoid adding heavy MVVM libraries, keeping the project "Pure".
