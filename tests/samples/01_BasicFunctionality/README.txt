# Sample 01: Basic Functionality Tests

## Purpose
Tests the core "happy path" functionality that should always work.

## What This Tests

### ✅ Expected to Pass

| Method | Tests |
|--------|-------|
| `Calculator.Add(int, int)` | Basic arithmetic with primitives |
| `Calculator.GetMessage()` | String return type |
| `Calculator.DoNothing()` | Void methods |
| `StaticHelper.GetVersion()` | Static method invocation |
| `ParameterTests.JoinStrings(string, string)` | String parameters |

## How to Use

```
load Sample01_Basic.dll
list
invoke Add 5 3
# Expected: 8
invoke GetMessage
# Expected: "Hello from Calculator!"
```

## Limitations Tested

| Limitation | Status |
|------------|--------|
| Basic invocation | ✅ Works |
| Auto parameter generation | ✅ Works for primitives |
| Return value display | ✅ Works |
| Static methods | ✅ Works |

## Known Issues
None - this DLL should work perfectly.
