# Sample 03: Error Handling Tests

## Purpose
Tests how the DLL Invoker handles various error conditions.

## ERROR HANDLING SUMMARY

| Error Type | Support | Notes |
|------------|---------|-------|
| Standard exceptions | ✅ HANDLED | Shown with stack trace |
| Custom exceptions | ✅ HANDLED | Message and type shown |
| Inner exceptions | ✅ HANDLED | Full chain shown |
| Slow methods | ⚠️ WORKS | UI may appear frozen |
| Infinite loops | ⚠️ HANGS | Only cancellation helps |
| StackOverflow | ❌ CRASH | CANNOT be caught |
| OutOfMemory | ❌ CRASH | CANNOT be caught |

## Test Scenarios

### ✅ ExceptionThrower.ThrowInvalidOperation()
```
Error Code: INVOKE_FAIL
Message: This operation is not valid!
Type: System.InvalidOperationException
Stack Trace: [full trace shown]
```
RESULT: Exception properly caught and displayed.

### ✅ ExceptionThrower.ThrowWithInner()
```
Outer: Failed to process data
Inner: Data is corrupt
```
RESULT: Inner exception chain preserved.

### ⚠️ TimeoutTests.SlowMethod()
```
[Invokes successfully after 5 seconds]
```
RESULT: Works but UI freezes. No timeout abort.

### ⚠️ TimeoutTests.InfiniteLoop()
```
[Never returns - app hangs]
```
RESULT: Must manually terminate app or use Cancel.
LIMITATION: No automatic timeout enforcement.

## Critical Limitations

### UNCATCHABLE EXCEPTIONS
These will CRASH the entire application:

1. **StackOverflowException**
   - Cause: Infinite recursion
   - Effect: Process terminates immediately
   - Mitigation: NONE

2. **OutOfMemoryException**  
   - Cause: Excessive allocation
   - Effect: Process terminates
   - Mitigation: NONE

3. **AccessViolationException**
   - Cause: Native code crash
   - Effect: Process terminates
   - Mitigation: NONE

### NO TIMEOUT ENFORCEMENT
- Slow/hanging methods block the thread
- Only CancellationToken can abort (if supported)
- UI thread remains responsive (async)

## Recommendations

1. Always run in a VM for untrusted DLLs
2. Be prepared to force-quit the application
3. Watch for methods that might loop forever
