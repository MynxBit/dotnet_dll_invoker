# Sample 02: Edge Cases and Limitations

## Purpose
Tests the specific limitations of the DLL Invoker tool.

## LIMITATION SUMMARY

| Feature | Support | Notes |
|---------|---------|-------|
| Abstract classes | ❌ FAIL | Cannot instantiate |
| Interfaces | ❌ FAIL | Cannot instantiate |
| Generic classes | ⚠️ PARTIAL | Type inference issues |
| No default constructor | ❌ FAIL | Cannot create instance |
| P/Invoke (native calls) | ⚠️ WORKS | But can crash app |
| ref/out parameters | ❌ FAIL | Not supported |
| Nested classes | ⚠️ PARTIAL | May not be discoverable |

## Detailed Tests

### ❌ AbstractClass.AbstractMethod()
```
Error: Cannot instantiate abstract type or interface
```
WHY: We cannot create an instance of abstract class.
SOLUTION: None - by design.

### ❌ ITestInterface.DoSomething()
```
Error: Cannot instantiate abstract type or interface
```
WHY: Interfaces cannot be instantiated.
SOLUTION: None - by design.

### ⚠️ GenericContainer<T>.SetValue(T)
```
May fail or use Object as fallback
```
WHY: We cannot infer the generic type at runtime.
SOLUTION: Limited - works with common types.

### ❌ NoDefaultCtor.GetRequired()
```
Error: Cannot find parameterless constructor
```
WHY: Instance creation fails without constructor args.
SOLUTION: Would need constructor parameter UI.

### ⚠️ NativeMethod.GetThreadIdWrapper()
```
Works - returns thread ID
```
WHY: P/Invoke is allowed in .NET.
RISK: If native code crashes, ENTIRE APP DIES.

### ❌ RefOutMethods.IncrementRef(ref int)
```
Error: ref parameters not supported
```
WHY: We pass parameters by value only.
SOLUTION: Would require special ref handling.

## Known Limitations

1. **No instance management** - Each invocation creates new instance
2. **No constructor parameters** - Only parameterless constructors
3. **No generic type specification** - Types inferred as Object
4. **No ref/out parameters** - By value only
5. **P/Invoke crash risk** - Native code can kill process
