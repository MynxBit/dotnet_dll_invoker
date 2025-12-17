# Low-Level Execution Diagram

**Purpose:** Visualizes the `Reflection -> Parameter -> Invocation` pipeline.

```mermaid
graph TD
    UI[UI / CLI Action Trigger] --> Dispatcher[Command/Event Dispatcher]
    Dispatcher --> LoadHandler[Assembly Load Request Handler]
    LoadHandler --> LoadAPI[Assembly.Load / LoadFrom API]
    LoadAPI --> Registry[Assembly Registry In-Memory]
    Registry --> DepRes[Dependency Resolution Engine]
    DepRes --> Enumerator[Method Enumeration Engine]
    Enumerator --> Metadata[Method Metadata Object]
    Metadata --> Inspect[Method Inspection Layer]
    Inspect --> ParamRes[Parameter Resolution Engine]
    ParamRes --> Decision[Invocation Decision Point]
    Decision --> Invoker[Invocation Engine]
    Invoker --> Result[Result / Error Capture Layer]
    Result --> Renderer[UI / CLI Output Renderer]
```

## Key Mechanics
1.  **Assembly Loading:** Uses `Assembly.Load*`. Only metadata access. **Warning:** Static constructors may run.
2.  **Method Enumeration:** `BindingFlags.Public | NonPublic | Static | Instance | DeclaredOnly`.
3.  **Invocation Engine (The Danger Zone):**
    *   **Boundary:** `MethodInfo.Invoke` is the SINGLE execution point.
    *   **Safety:** Wrapped in `try/catch (TargetInvocationException)`.
