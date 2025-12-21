# Coding Standards for Modular, Debuggable, AI-Assisted Software Projects

**Version**: v1.2 – Foundational Standard  
**Last Updated**: 2024-12-20 *(Date reflects last structural revision, not content validity)*

---

## Table of Contents

1. [Purpose](#1-purpose)
2. [Core Design Philosophy](#2-core-design-philosophy)
3. [File-Level Mandatory Contract](#3-file-level-mandatory-contract)
4. [Minimalism & File Size Discipline](#4-minimalism--file-size-discipline)
5. [Functional-First Programming Model](#5-functional-first-programming-model)
6. [Wrapper-Driven Architecture](#6-wrapper-driven-architecture)
7. [Dependency Direction](#7-dependency-direction-strict)
8. [Error Handling Standards](#8-error-handling-standards-non-negotiable)
9. [Invariant Rules](#9-invariant-rules-mandatory)
10. [Logging & Observability Standards](#10-logging--observability-standards)
11. [Layer-Aware Error Responsibilities](#11-layer-aware-error-responsibilities)
12. [Naming Conventions](#12-naming-conventions)
13. [Testing Standards](#13-testing-standards)
14. [Configuration Management](#14-configuration-management)
15. [Security Standards](#15-security-standards)
16. [Performance Guidelines](#16-performance-guidelines)
17. [Version Control Practices](#17-version-control-practices)
18. [Documentation Standards](#18-documentation-standards)
19. [Architectural Decision Records](#19-architectural-decision-records-adr)
20. [Plugin-Oriented Architecture](#20-plugin-oriented-architecture-poa)
21. [AI-Assisted Development Rules](#21-ai-assisted-development-rules)
22. [Code Review Standards](#22-code-review-standards)
23. [Manual Debugging Guarantee](#23-manual-debugging-guarantee)
24. [Enforcement Policy](#24-enforcement-policy)
25. [Final Principle](#25-final-principle)

---

## 1. Purpose

This document defines **mandatory** coding and architectural standards for all projects developed using AI assistance and/or manual implementation.

### Primary Objectives

| Objective | Description |
|-----------|-------------|
| **Absolute Clarity** | Every failure must be immediately understandable |
| **Minimal Cognitive Load** | Debugging should require zero guessing |
| **Maximum Modularity** | Changes should be isolated and safe |
| **Predictable Structure** | Both humans and AI can reason about the system |
| **Zero Hidden Behavior** | No magic, no implicit side effects |
| **Self-Locating Errors** | Failures answer: Where? What? Why? Impact? |

> [!IMPORTANT]
> This document is **not optional guidance**. It is the baseline for all design and code decisions.

> [!NOTE]
> Examples use JavaScript for illustration; all principles apply equally to any language or runtime.

---

## 2. Core Design Philosophy

### 2.1 Design for Failure First

Failures are **expected**. Silent failures are **unacceptable**.

Every failure must clearly answer:

```
┌─────────────────────────────────────────┐
│  WHERE  │  What file/function/line?    │
│  WHAT   │  What operation failed?      │
│  WHY    │  What caused the failure?    │
│  IMPACT │  What is affected?           │
│  NEXT   │  What should happen now?     │
└─────────────────────────────────────────┘
```

If any of these are missing, the implementation is **incomplete**.

### 2.2 Debuggability Over Cleverness

Readable, traceable, debuggable code **always wins** over:

- ❌ Shortcuts
- ❌ Clever abstractions
- ❌ Over-optimized logic
- ❌ Hidden magic
- ❌ One-liners that sacrifice clarity

**Litmus Test**: If debugging requires:
- Guessing
- Adding logs after the fact
- Attaching a debugger to understand flow
- Reading unrelated files to understand context

...then the system has already **failed its design goal**.

### 2.3 Change Is the Default State

The system is designed assuming:

- Requirements **will** change
- Tools **will** change
- AI models **will** change
- Environments **will** change
- Team members **will** change

**Architecture must absorb change locally, not propagate it globally.**

### 2.4 Explicit Over Implicit

- Prefer explicit configuration over convention
- Prefer explicit dependencies over auto-injection
- Prefer explicit error paths over exception swallowing
- Prefer explicit state over hidden mutations

---

## 3. File-Level Mandatory Contract

Every source file must start with a structured header comment.

### 3.1 Header Template

#### JavaScript / TypeScript
```javascript
/**
 * ═══════════════════════════════════════════════════════════════════════════
 * FILE: [filename.ext]
 * PATH: [/full/path/to/file.ext]
 * LAYER: [UI | Core | Wrapper | Adapter | Infrastructure | Domain | Plugin]
 * ═══════════════════════════════════════════════════════════════════════════
 * ...
 */
```

#### C# / .NET
```csharp
// ═══════════════════════════════════════════════════════════════════════════
// FILE: [FileName.cs]
// PATH: [src/Module/FileName.cs]
// LAYER: [UI | Core | Business | Foundation | Presentation]
// ═══════════════════════════════════════════════════════════════════════════
//
// PRIMARY RESPONSIBILITY:
//   [Exactly ONE sentence describing what this file does]
//
// SECONDARY RESPONSIBILITIES:
//   - [Optional list]
//
// NON-RESPONSIBILITIES:
//   - [What this file must NEVER do]
//
// ───────────────────────────────────────────────────────────────────────────
// DEPENDENCIES:
//   - [Class/Interface] -> [Reason]
//
// DEPENDENTS:
//   - [Consumer Class]
//
// ───────────────────────────────────────────────────────────────────────────
// CHANGE LOG:
//   [YYYY-MM-DD] - [Author] - [Description]
// ═══════════════════════════════════════════════════════════════════════════
```

### 3.2 Why This Matters

### 3.2 Why This Matters

- Makes navigation **deterministic**
- Enables AI-assisted analysis without full context
- Forces single-responsibility thinking
- Documents architectural intent

---

## 4. Minimalism & File Size Discipline

### Hard Limits

| Metric | Limit | Action on Violation |
|--------|-------|---------------------|
| Lines of code (excluding comments) | ≤ 300 | Split file |
| Functions per file | ≤ 10 | Extract to new file |
| Function length | ≤ 50 lines | Decompose |
| Nesting depth | ≤ 3 levels | Flatten or extract |
| Parameters per function | ≤ 5 | Use options object |

### File Growth Protocol

```
File grows → Ask: "Does this file have multiple reasons to change?"
                    │
         ┌──────────┴──────────┐
         │ YES                 │ NO
         ▼                     ▼
    Split immediately     Document why single
                          responsibility still
                          applies
```

**One file = One reason to change. No exceptions.**

---

## 5. Functional-First Programming Model

### Preferred Patterns

```javascript
// ✅ GOOD: Pure function, explicit I/O
function calculateUsage(currentBytes, previousBytes, timeElapsed) {
  return {
    bytesUsed: currentBytes - previousBytes,
    rate: (currentBytes - previousBytes) / timeElapsed
  };
}

// ❌ BAD: Hidden state, implicit behavior
let lastBytes = 0;
function calculateUsage(currentBytes) {
  const used = currentBytes - lastBytes;
  lastBytes = currentBytes; // Side effect!
  return used;
}
```

### State Management Rules

| Requirement | Implementation |
|-------------|----------------|
| State is required | Inject it explicitly |
| State must persist | Wrap in observable container |
| State changes | Log the transition |
| Global state | **Forbidden** (use dependency injection) |

**Side effects must live only at the edges of the system.**

---

## 6. Wrapper-Driven Architecture

### Core Rule

> **Core logic must NEVER know about the outside world.**

### Mandatory Wrappers

All interactions with external systems require wrappers:

```
┌──────────────────────────────────────────────────────────────┐
│                      EXTERNAL WORLD                          │
│  File System │ Network │ OS │ UI │ Environment │ Time │ DB  │
└──────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌──────────────────────────────────────────────────────────────┐
│                    WRAPPER / ADAPTER LAYER                   │
│  FileWrapper │ HttpAdapter │ SystemAdapter │ TimeProvider    │
└──────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌──────────────────────────────────────────────────────────────┐
│                      CORE LOGIC (Pure)                       │
│  Business Rules │ Calculations │ Transformations │ Validation│
└──────────────────────────────────────────────────────────────┘
```

### Benefits

- ✅ Core logic becomes **testable** without mocks
- ✅ Implementations become **replaceable**
- ✅ Failures become **isolated**
- ✅ Debugging becomes **surgical**

---

## 7. Dependency Direction (Strict)

### The Dependency Rule

Dependencies must flow in **one direction only**:

```
┌─────────────────────┐
│     UI / CLI        │  ← User-facing layer
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│  Wrappers/Adapters  │  ← Infrastructure layer
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│  Core Logic (Pure)  │  ← Business layer
└─────────────────────┘
```

### Violation Detection

```javascript
// ❌ HARD VIOLATION: Core importing infrastructure
// File: core/calculator.js
import { FileSystem } from '../infrastructure/file-system.js'; // FORBIDDEN

// ✅ CORRECT: Core receives abstraction via injection
// File: core/calculator.js
function processData(dataProvider) {  // Abstraction injected
  const data = dataProvider.getData();
  return transform(data);
}
```

**If core logic imports infrastructure code → Immediate refactor required.**

---

## 8. Error Handling Standards (Non-Negotiable)

### 8.1 Typed Error Categories

```javascript
/**
 * Error type hierarchy - ALL errors must extend from these
 */
class AppError extends Error {
  constructor(message, code, context = {}) {
    super(message);
    this.code = code;           // Machine-readable code
    this.context = context;     // Debugging context
    this.timestamp = new Date().toISOString();
    this.layer = 'UNKNOWN';     // Must be overridden
  }
}

class ValidationError extends AppError {
  constructor(message, field, value) {
    super(message, 'VALIDATION_ERROR', { field, value });
    this.layer = 'CORE';
  }
}

class NetworkError extends AppError {
  constructor(message, endpoint, statusCode) {
    super(message, 'NETWORK_ERROR', { endpoint, statusCode });
    this.layer = 'ADAPTER';
  }
}

class PluginError extends AppError {
  constructor(message, pluginId, operation) {
    super(message, 'PLUGIN_ERROR', { pluginId, operation });
    this.layer = 'PLUGIN';
  }
}
```

### 8.2 Forbidden Patterns

```javascript
// ❌ FORBIDDEN: Generic errors
throw new Error('Something went wrong');

// ❌ FORBIDDEN: Empty catch
try { ... } catch (e) { }

// ❌ FORBIDDEN: Swallowing exceptions
try { ... } catch (e) { console.log(e); }

// ❌ FORBIDDEN: Blanket catch without context
try { ... } catch (e) { throw e; }
```

### 8.3 Required Pattern

```javascript
// ✅ REQUIRED: Contextual error handling
try {
  await networkAdapter.fetchData(endpoint);
} catch (error) {
  throw new NetworkError(
    `Failed to fetch data from ${endpoint}: ${error.message}`,
    endpoint,
    error.statusCode
  ).withCause(error);  // Preserve original
}
```

### 8.4 Fail Fast Principle

```javascript
// ✅ CORRECT: Validate early, fail immediately
function processWidget(config) {
  // Guard clauses at the TOP
  if (!config) {
    throw new ValidationError('Config is required', 'config', config);
  }
  if (!config.id) {
    throw new ValidationError('Widget ID is required', 'config.id', undefined);
  }
  if (config.refreshRate < 100) {
    throw new ValidationError('Refresh rate must be >= 100ms', 'config.refreshRate', config.refreshRate);
  }
  
  // Happy path after all validations
  return doActualWork(config);
}
```

---

## 9. Invariant Rules (Mandatory)

An **invariant** is a condition that must **always** be true for the system to be correct.

### 9.1 Invariant vs Error Distinction

| Type | Description | Example |
|------|-------------|---------|
| **Validation Error** | Bad external input | User entered negative age |
| **Environmental Error** | External system failed | Network timeout, disk full |
| **Invariant Violation** | Internal logic bug | Usage bytes became negative |

> [!CAUTION]
> **Invariant violations are bugs, not errors.** They indicate the system has entered an impossible state.

### 9.2 Invariant Rules

1. Invariants must be **explicit** (documented in code)
2. Invariants must be **checked** (asserted at boundaries)
3. Invariants must be **logged** when violated
4. Invariant violations are **fatal by default**

### 9.3 Invariant Error Type

```javascript
/**
 * Represents an impossible state - always a bug, never user error
 */
class InvariantViolationError extends AppError {
  constructor(invariant, context = {}) {
    super(
      `INVARIANT VIOLATED: ${invariant}`,
      'INVARIANT_VIOLATION',
      context
    );
    this.layer = 'CORE';
    this.isBug = true;  // Flag for special handling
  }
}
```

### 9.4 Invariant Enforcement Pattern

```javascript
// ✅ REQUIRED: Explicit invariant checks
function calculateUsage(usageBytes, previousBytes) {
  // Invariant: usage bytes can never be negative
  if (usageBytes < 0) {
    throw new InvariantViolationError(
      'Usage bytes must never be negative',
      { usageBytes, previousBytes, source: 'calculateUsage' }
    );
  }
  
  // Invariant: current must be >= previous (monotonic counter)
  if (usageBytes < previousBytes) {
    throw new InvariantViolationError(
      'Usage counter cannot decrease (monotonic invariant)',
      { usageBytes, previousBytes, source: 'calculateUsage' }
    );
  }
  
  return usageBytes - previousBytes;
}
```

### 9.5 Why Invariants Matter

- Removes ambiguity between "error" and "bug"
- Helps humans debug: "This is a code defect, not a user mistake"
- Helps AI diagnose: distinguishes input/environment/logic failures
- Prevents silent corruption from propagating

---

## 10. Logging & Observability Standards

### 10.1 Mandatory Log Structure

```javascript
{
  "timestamp": "2024-12-20T12:30:45.123Z",  // UTC always
  "level": "ERROR",                          // DEBUG|INFO|WARN|ERROR|FATAL
  "correlationId": "req-abc-123",            // Trace across operations
  "file": "adapters/network-adapter.js",     // Exact location
  "function": "fetchData",                   // Exact operation
  "message": "HTTP request failed",          // Human-readable
  "code": "NETWORK_ERROR",                   // Machine-readable
  "context": {                               // Debugging data
    "endpoint": "/api/usage",
    "statusCode": 503,
    "retryCount": 3
  },
  "stack": "..."                             // Full stack trace
}
```

### 10.2 Log Level Guidelines

| Level | When to Use | Example |
|-------|-------------|---------|
| `DEBUG` | Development-only detail | Variable values, loop iterations |
| `INFO` | Significant business events | Widget loaded, user action |
| `WARN` | Recoverable issues | Retry succeeded, fallback used |
| `ERROR` | Operation failed | API call failed, validation error |
| `FATAL` | System cannot continue | Missing critical config, corrupt state |

### 10.3 Forbidden Log Messages

```javascript
// ❌ FORBIDDEN: Vague messages
logger.error('Something went wrong');
logger.error('Unexpected error');
logger.error('Unknown failure');
logger.info('Done');

// ✅ REQUIRED: Specific, actionable messages
logger.error({
  message: 'WiFi usage API returned invalid response',
  code: 'INVALID_API_RESPONSE', 
  context: { expected: 'object', received: typeof response }
});
```

---

## 11. Layer-Aware Error Responsibilities

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ LAYER          │ ERROR RESPONSIBILITY                                       │
├─────────────────────────────────────────────────────────────────────────────┤
│ Core Logic     │ • Produce typed errors                                     │
│                │ • No logging (pure functions)                              │
│                │ • No environment awareness                                 │
│                │ • Return Result types or throw typed errors                │
├─────────────────────────────────────────────────────────────────────────────┤
│ Wrappers/      │ • Translate low-level errors to domain errors             │
│ Adapters       │ • Attach environmental context                             │
│                │ • Perform logging                                          │
│                │ • Handle retries and circuit breaking                      │
├─────────────────────────────────────────────────────────────────────────────┤
│ UI / CLI       │ • Decide error presentation                                │
│                │ • Never generate or reinterpret error meaning              │
│                │ • Map error codes to user-friendly messages                │
│                │ • Trigger user notifications                               │
├─────────────────────────────────────────────────────────────────────────────┤
│ Plugins        │ • Contain failures within plugin boundary                  │
│                │ • Report failures via plugin error channel                 │
│                │ • Never crash the host system                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 12. Naming Conventions

### 12.1 Files & Directories

| Type | Convention | Example |
|------|------------|---------|
| Directories | `kebab-case` | `network-adapters/` |
| Source files | `kebab-case` | `wifi-widget.js` |
| Test files | `*.test.js` or `*.spec.js` | `wifi-widget.test.js` |
| Config files | `kebab-case` | `widget-config.json` |
| Constants files | `SCREAMING_SNAKE_CASE` | `ERROR_CODES.js` |

### 12.2 Code Elements

| Type | Convention | Example |
|------|------------|---------|
| Classes | `PascalCase` | `NetworkAdapter` |
| Functions | `camelCase` | `calculateUsage` |
| Variables | `camelCase` | `currentSpeed` |
| Constants | `SCREAMING_SNAKE_CASE` | `MAX_RETRY_COUNT` |
| Private members | `_prefixUnderscore` | `_internalState` |
| Boolean variables | `is/has/can/should` prefix | `isConnected`, `hasError` |
| Event handlers | `on` prefix | `onDataReceived` |
| Factory functions | `create` prefix | `createWidget` |

### 12.3 Naming Anti-Patterns

```javascript
// ❌ BAD: Vague names
const data = getData();
const info = process(data);
const result = handle(info);

// ✅ GOOD: Intention-revealing names
const networkUsageBytes = fetchNetworkUsage();
const formattedUsage = formatBytesToHumanReadable(networkUsageBytes);
const displayModel = createUsageDisplayModel(formattedUsage);
```

---

## 13. Testing Standards

### 13.1 Test Categories

| Category | Purpose | Location | Naming |
|----------|---------|----------|--------|
| Unit | Test pure functions in isolation | `__tests__/unit/` | `*.unit.test.js` |
| Integration | Test adapter interactions | `__tests__/integration/` | `*.integration.test.js` |
| Plugin | Test plugin contract compliance | `__tests__/plugins/` | `*.plugin.test.js` |
| E2E | Test full user flows | `__tests__/e2e/` | `*.e2e.test.js` |

### 13.2 Test Structure (AAA Pattern)

```javascript
describe('WiFiUsageCalculator', () => {
  describe('calculateDailyUsage', () => {
    it('should return zero when no data received', () => {
      // Arrange
      const startBytes = 1000;
      const endBytes = 1000;
      
      // Act
      const result = calculateDailyUsage(startBytes, endBytes);
      
      // Assert
      expect(result).toEqual({
        bytesUsed: 0,
        formatted: '0 B'
      });
    });
  });
});
```

### 13.3 Test Coverage Requirements

| Component Type | Minimum Coverage |
|----------------|------------------|
| Core logic | 90% |
| Adapters | 80% |
| Plugins | 85% |
| UI components | 70% |

### 13.4 What Must Be Tested

- ✅ All pure functions (unit tests)
- ✅ All error paths
- ✅ All plugin contracts
- ✅ All adapter boundaries
- ✅ All state transitions

---

## 14. Configuration Management

### 14.1 Configuration Hierarchy

```
┌─────────────────────────────────────────────────┐
│ Priority (highest to lowest):                   │
├─────────────────────────────────────────────────┤
│ 1. Command-line arguments                       │
│ 2. Environment variables                        │
│ 3. User config file (~/.app/config.json)        │
│ 4. Project config file (./config.json)          │
│ 5. Default values in code                       │
└─────────────────────────────────────────────────┘
```

### 14.2 Configuration Rules

```javascript
// ✅ REQUIRED: All config must have defaults and validation
const CONFIG_SCHEMA = {
  refreshInterval: {
    default: 5000,
    validate: (v) => v >= 1000 && v <= 60000,
    description: 'Widget refresh interval in milliseconds'
  },
  timezone: {
    default: 'UTC',
    validate: (v) => isValidTimezone(v),
    description: 'Timezone for usage calculations'
  }
};

// ❌ FORBIDDEN: Magic numbers in code
const interval = 5000;  // What is this? Why 5000?

// ✅ REQUIRED: Named configuration
const interval = config.get('refreshInterval');
```

### 14.3 Secrets Management

- ❌ Never commit secrets to version control
- ✅ Use environment variables for secrets
- ✅ Use `.env.example` with placeholder values
- ✅ Document all required environment variables

---

## 15. Security Standards

### 15.1 Input Validation

```javascript
// ✅ REQUIRED: Validate ALL external input
function processUserInput(input) {
  // Type check
  if (typeof input !== 'string') {
    throw new ValidationError('Input must be string', 'input', typeof input);
  }
  
  // Length check
  if (input.length > MAX_INPUT_LENGTH) {
    throw new ValidationError('Input too long', 'input.length', input.length);
  }
  
  // Content sanitization
  const sanitized = sanitize(input);
  
  return sanitized;
}
```

### 15.2 Security Checklist

- [ ] All user input is validated and sanitized
- [ ] No secrets in source code or logs
- [ ] Dependencies are audited for vulnerabilities
- [ ] Principle of least privilege applied
- [ ] Sensitive operations are logged (but not sensitive data)

---

## 16. Performance Guidelines

### 16.1 Performance Budgets

| Metric | Budget |
|--------|--------|
| Widget initial render | < 100ms |
| Data refresh cycle | < 500ms |
| Memory per widget | < 50MB |
| CPU idle usage | < 1% |

### 16.2 Performance Rules

```javascript
// ❌ FORBIDDEN: Synchronous operations in hot paths
const data = fs.readFileSync(path);  // Blocks event loop

// ✅ REQUIRED: Async operations
const data = await fs.promises.readFile(path);

// ❌ FORBIDDEN: Unnecessary work in loops
items.forEach(item => {
  const config = loadConfig();  // Loaded every iteration!
  process(item, config);
});

// ✅ REQUIRED: Hoist invariants
const config = loadConfig();  // Load once
items.forEach(item => process(item, config));
```

### 16.3 Caching Strategy

- Cache expensive computations
- Cache external API responses (with TTL)
- Invalidate cache explicitly on data changes
- Log cache hit/miss ratios

---

## 17. Version Control Practices

### 17.1 Commit Message Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types**: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

**Examples**:
```
feat(wifi-widget): add monthly usage display

- Implemented usage tracking with timezone support
- Added formatted display for bytes/MB/GB
- Integrated with network adapter

Closes #123
```

### 17.2 Branch Strategy

| Branch | Purpose |
|--------|---------|
| `main` | Production-ready code |
| `develop` | Integration branch |
| `feature/*` | New features |
| `fix/*` | Bug fixes |
| `plugin/*` | New plugin development |

### 17.3 PR Requirements

- [ ] All tests pass
- [ ] No linting errors
- [ ] File headers updated
- [ ] CHANGELOG updated (for features/fixes)
- [ ] Documentation updated
- [ ] Reviewed by at least one team member

---

## 18. Documentation Standards

### 18.1 Required Documentation

| Document | Location | Purpose |
|----------|----------|---------|
| `README.md` | Project root | Setup, usage, overview |
| `ARCHITECTURE.md` | `/docs` | System design, diagrams |
| `CHANGELOG.md` | Project root | Version history |
| `CONTRIBUTING.md` | Project root | How to contribute |
| `API.md` | `/docs` | Public API reference |
| `PLUGINS.md` | `/docs` | Plugin development guide |

### 18.2 Code Documentation

```javascript
/**
 * Calculates network usage for a given time period.
 * 
 * @description
 * This function computes the difference between start and end byte counts,
 * applying timezone adjustments for accurate daily/monthly boundaries.
 * 
 * @param {number} startBytes - Byte count at period start
 * @param {number} endBytes - Byte count at period end  
 * @param {string} timezone - IANA timezone identifier
 * @returns {UsageResult} Calculated usage with formatted display
 * 
 * @throws {ValidationError} If bytes are negative
 * @throws {ValidationError} If timezone is invalid
 * 
 * @example
 * const usage = calculateUsage(1000, 5000, 'Asia/Kolkata');
 * // Returns: { bytes: 4000, formatted: '4 KB' }
 */
function calculateUsage(startBytes, endBytes, timezone) {
  // Implementation
}
```

---

## 19. Architectural Decision Records (ADR)

For non-obvious decisions that future developers (or AI) might question, document them explicitly.

### 19.1 When to Create an ADR

Create an ADR when:
- A decision would confuse "future you"
- Multiple valid approaches exist and you chose one
- The choice has long-term consequences
- External constraints forced a specific approach

### 19.2 ADR Format

Create a short markdown file in `docs/adr/`:

```markdown
# ADR-001: [Decision Title]

## Status
Accepted | Deprecated | Superseded by ADR-XXX

## Context
[What problem or question prompted this decision?]

## Decision
[What was decided and why?]

## Consequences
[What are the positive and negative results of this decision?]

## Alternatives Considered
[What other options were evaluated?]
```

### 19.3 Example

```
docs/adr/001-plugin-architecture.md
docs/adr/002-electron-over-wpf.md
docs/adr/003-json-config-format.md
```

### 19.4 ADR Rules

- **Lightweight**: An ADR should be 1 page or less
- **Immutable**: Once accepted, don't edit (create a new one if superseded)
- **Numbered**: Sequential numbering for easy reference
- **Linked**: Reference ADRs in code comments when relevant

> [!TIP]
> If a decision seems "obvious" now, ask: "Would this be obvious to someone joining 6 months from now?"

---

## 20. Plugin-Oriented Architecture (POA)

### 20.1 Core Principle

> **No enhancement should require modifying existing core code unless absolutely unavoidable.**

The system must grow by **addition**, not **mutation**.

> **The core must not contain logic that checks for specific plugin identities, plugin names, or plugin capabilities.**

```javascript
// ❌ FORBIDDEN: Core knows about specific plugins
if (plugin.id === 'wifi-widget') { ... }
switch (plugin.type) { case 'network': ... }

// ✅ REQUIRED: Polymorphism and capability-based contracts only
plugin.render();  // All plugins implement render()
if (plugin.capabilities.includes('refresh')) { plugin.refresh(); }
```

### 20.2 Change Classification

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ CHANGE TYPE         │ ACCEPTABLE ACTIONS                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│ New Feature         │ • Add new plugin                                      │
│                     │ • Add new adapter                                     │
│                     │ • Update configuration                                │
├─────────────────────────────────────────────────────────────────────────────┤
│ Bug Fix             │ • Fix within existing file                            │
│                     │ • Add missing error handling                          │
├─────────────────────────────────────────────────────────────────────────────┤
│ Enhancement         │ • Extend via new plugin                               │
│                     │ • Add new adapter implementation                      │
├─────────────────────────────────────────────────────────────────────────────┤
│ ❌ VIOLATION        │ • Adding if/else for new feature in core              │
│                     │ • Modifying existing plugin for unrelated feature     │
│                     │ • Changing core interfaces without versioning         │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 20.3 Plugin Contract

Every plugin must implement:

```javascript
/**
 * Plugin Contract Interface
 * All plugins MUST implement this interface
 */
interface WidgetPlugin {
  // Identity
  readonly id: string;           // Unique identifier
  readonly name: string;         // Display name
  readonly version: string;      // Semantic version
  readonly coreVersion: string;  // Compatible core version
  
  // Lifecycle
  initialize(context: PluginContext): Promise<void>;
  start(): Promise<void>;
  stop(): Promise<void>;
  destroy(): Promise<void>;
  
  // Rendering
  render(): HTMLElement;
  
  // Configuration
  getDefaultConfig(): PluginConfig;
  validateConfig(config: PluginConfig): ValidationResult;
  
  // Health
  getHealthStatus(): HealthStatus;
}
```

### 20.4 Plugin Registration

```javascript
// plugins/wifi-widget/index.js
export default {
  id: 'wifi-widget',
  name: 'WiFi Usage Monitor',
  version: '1.0.0',
  coreVersion: '^1.0.0',
  
  // Manifest for discovery
  manifest: {
    description: 'Displays current WiFi speed and usage statistics',
    author: 'Your Name',
    icon: 'wifi-icon.svg',
    category: 'network',
    permissions: ['network-stats', 'system-time']
  }
};
```

### 20.5 Plugin Isolation

```
┌──────────────────────────────────────────────────────────────────┐
│                         HOST APPLICATION                          │
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐     │
│  │  WiFi Plugin   │  │  CPU Plugin    │  │ Memory Plugin  │     │
│  │  ┌──────────┐  │  │  ┌──────────┐  │  │  ┌──────────┐  │     │
│  │  │ Sandbox  │  │  │  │ Sandbox  │  │  │  │ Sandbox  │  │     │
│  │  └──────────┘  │  │  └──────────┘  │  │  └──────────┘  │     │
│  └───────┬────────┘  └───────┬────────┘  └───────┬────────┘     │
│          │                   │                   │               │
│          └───────────────────┼───────────────────┘               │
│                              │                                    │
│                    ┌─────────▼─────────┐                         │
│                    │   Plugin Bridge    │                         │
│                    │   (Controlled API) │                         │
│                    └───────────────────┘                         │
└──────────────────────────────────────────────────────────────────┘
```

**Plugin failures are isolated. A crashing plugin cannot crash the host.**

### 20.6 Minimal Touch Rule

For any enhancement, explicitly answer:

> **Which existing files are being modified, and why is it impossible to avoid modifying them?**

**Ideal Enhancement Score Card**:

| Action | Score |
|--------|-------|
| Only new files added | ✅ Perfect |
| Only config updated | ✅ Perfect |
| 1 existing file touched | ⚠️ Justify |
| 2+ existing files touched | ❌ Redesign |

---

## 21. AI-Assisted Development Rules

This standard enables efficient AI collaboration.

### 21.1 AI-Friendly Structure

| Requirement | Benefit |
|-------------|---------|
| Predictable file structure | AI can locate code quickly |
| Explicit responsibilities | AI reasons locally |
| Self-locating errors | AI diagnoses without full context |
| Small, focused files | Fits in context window |
| Typed errors | AI can match patterns |

### 21.2 Context Efficiency

```
┌─────────────────────────────────────────────────────────────────┐
│ GOOD: AI can work on isolated plugin                            │
│                                                                  │
│  "Fix the bug in wifi-widget/usage-calculator.js"               │
│  → AI reads ONE file                                            │
│  → AI understands dependencies from header                      │
│  → AI makes isolated fix                                        │
│  → No side effects elsewhere                                    │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ BAD: AI needs full codebase understanding                       │
│                                                                  │
│  "Fix the usage calculation"                                    │
│  → Which file? (search needed)                                  │
│  → What dependencies? (exploration needed)                       │
│  → What might break? (unknown)                                  │
│  → High token usage, risky changes                              │
└─────────────────────────────────────────────────────────────────┘
```

### 21.3 Prompt-Friendly Patterns

- One responsibility per file
- Explicit interfaces over conventions
- Error messages include file paths
- Standardized file headers
- Consistent naming patterns

### 21.4 AI Error Consumption Rule

> **All error messages must be written so that they can be understood without running the code.**

This forces:
- No vague phrasing ("something failed")
- No context-dependent wording ("this operation failed")
- No pronouns without antecedents ("it returned null")

```javascript
// ❌ BAD: Requires running context to understand
"Failed to process the item"
"This returned an unexpected value"
"It timed out"

// ✅ GOOD: Self-contained, AI-parseable
"NetworkAdapter.fetchUsage() failed: endpoint '/api/v1/usage' returned HTTP 503"
"WiFiWidget.calculateDailyUsage() received negative bytes (-500) from NetworkMonitor"
"ConfigLoader.load() timed out after 5000ms reading '/etc/widgets/config.json'"
```

---

## 22. Code Review Standards

### 22.1 Review Checklist

**Architecture**:
- [ ] Dependencies flow in correct direction
- [ ] No core logic pollution
- [ ] Plugin boundaries respected

**Code Quality**:
- [ ] File header present and accurate
- [ ] Functions are pure where possible
- [ ] No magic numbers or strings
- [ ] Naming is intention-revealing

**Error Handling**:
- [ ] All errors are typed
- [ ] No empty catch blocks
- [ ] Errors have sufficient context

**Testing**:
- [ ] Tests cover happy path
- [ ] Tests cover error paths
- [ ] Tests are readable (AAA pattern)

**Documentation**:
- [ ] Complex logic is documented
- [ ] Public interfaces have JSDoc
- [ ] CHANGELOG updated if needed

### 22.2 Review Response Times

| Review Type | SLA |
|-------------|-----|
| Critical fix | 4 hours |
| Bug fix | 24 hours |
| Feature | 48 hours |
| Refactor | 72 hours |

---

## 23. Manual Debugging Guarantee

A human encountering an error must be able to:

1. ✅ **Identify the exact file** (from error message)
2. ✅ **Identify the exact responsibility** (from file header)
3. ✅ **Identify the dependency chain** (from header + error context)
4. ✅ **Fix the issue without guessing** (from explicit error type)

**If this is not possible, the design has failed.**

---

## 24. Enforcement Policy

| Violation | Severity | Action |
|-----------|----------|--------|
| Missing file headers | MEDIUM | PR blocked |
| Missing logs at failure points | HIGH | Defect logged |
| Ambiguous/generic errors | HIGH | PR rejected |
| Dependency inversion | CRITICAL | Immediate refactor |
| File > 300 lines | MEDIUM | Redesign required |
| Empty catch blocks | CRITICAL | PR rejected |
| Hardcoded secrets | CRITICAL | Immediate removal |
| Core importing infrastructure | CRITICAL | Immediate refactor |
| Invariant violation not fatal | HIGH | Must crash or log FATAL |
| Core checks specific plugin ID | CRITICAL | Immediate refactor |

**No exceptions for speed or convenience.**

---

## 25. Final Principle

This standard optimizes for:

| Priority | Over |
|----------|------|
| **Longevity** | Speed |
| **Clarity** | Cleverness |
| **Precision** | Convenience |
| **Debuggability** | Abstraction |
| **Stability** | Features |
| **Locality** | Global optimization |

---

## Summary Philosophy

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                                                                              │
│   ✓ Modular                    ✓ Additive growth                            │
│   ✓ Plugin-first               ✓ Minimal change surface                     │
│   ✓ Maximum debuggability      ✓ Stable core, flexible edges                │
│   ✓ AI-friendly by design      ✓ Human-debuggable by default                │
│                                                                              │
│   Good systems grow by EXTENSION.                                            │
│   Bad systems grow by MODIFICATION.                                          │
│                                                                              │
│   We are building the former.                                                │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

**This is not just "coding standards". This is a system design doctrine.**
