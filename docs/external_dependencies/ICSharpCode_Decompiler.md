# ICSharpCode.Decompiler

## Package Information

| Property | Value |
|----------|-------|
| **NuGet Package** | [ICSharpCode.Decompiler](https://www.nuget.org/packages/ICSharpCode.Decompiler) |
| **Version Used** | 9.1.0.7988 |
| **License** | MIT |
| **Source Code** | [GitHub: icsharpcode/ILSpy](https://github.com/icsharpcode/ILSpy) |
| **Used In** | `DotNetDllInvoker.Reflection` |

---

## Why We Use It

### The Problem
We need to show analysts **readable C# source code** from compiled DLLs. Raw IL opcodes are not human-friendly:

```
IL_0000: ldarg.0
IL_0001: ldfld       UserQuery.name
IL_0006: ldstr       ", Hello!"
IL_000B: call        System.String.Concat
IL_0010: ret
```

### What We Need
```csharp
public string Greet()
{
    return name + ", Hello!";
}
```

### Why Not Implement Ourselves?

| Aspect | DIY Effort | ICSharpCode Solution |
|--------|------------|---------------------|
| Control flow analysis | 2-3 months | ✅ Built-in |
| Expression tree reconstruction | 3-6 months | ✅ Built-in |
| Async/await state machines | 1-2 months | ✅ Built-in |
| LINQ pattern detection | 1 month | ✅ Built-in |
| Generic type handling | 2 weeks | ✅ Built-in |
| **Total** | **~12+ months** | **0 days** |

The ILSpy/ICSharpCode.Decompiler project represents **15+ years** of development. Reimplementing is not feasible.

---

## How It Works Internally

### Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                    ICSharpCode.Decompiler                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐        │
│  │  Metadata   │ ─→ │  Type       │ ─→ │  Decompile  │        │
│  │  Reader     │    │  System     │    │  Engine     │        │
│  └─────────────┘    └─────────────┘    └─────────────┘        │
│                                                                 │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐        │
│  │  IL AST     │ ─→ │   C# AST    │ ─→ │   Output    │        │
│  │  Builder    │    │  Transform  │    │  Generator  │        │
│  └─────────────┘    └─────────────┘    └─────────────┘        │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Key Components

#### 1. Metadata Reader (System.Reflection.Metadata)
Reads PE file structure:
- Type definitions
- Method signatures
- IL byte arrays
- String literals

#### 2. Type System
Creates semantic model:
- Resolves type references
- Handles generics
- Tracks inheritance

#### 3. IL AST Builder
Converts IL bytes to structured AST:
```
IL_0001: ldarg.0     →    LoadArg(0)
IL_0002: ldfld name  →    LoadField(this, "name")
IL_0003: call Concat →    Call(String.Concat, ...)
```

#### 4. C# AST Transform
Applies pattern recognition:
- Detects `foreach` from enumerator patterns
- Reconstructs `async/await` from state machines
- Identifies LINQ expressions
- Inlines simple expressions

#### 5. Output Generator
Produces C# source text with:
- Proper indentation
- Syntax highlighting hints
- Comment preservation (where available)

---

## How We Use It

### Our Integration
From [DecompilerService.cs](file:///c:/Users/mayan/.gemini/antigravity/scratch/dotnet_dll_invoker/src/DotNetDllInvoker.Reflection/DecompilerService.cs):

```csharp
public static string Decompile(MethodBase method)
{
    // 1. Get assembly file path
    var assembly = method.DeclaringType.Assembly;
    
    // 2. Create decompiler with settings
    var decompiler = new CSharpDecompiler(assembly.Location, new DecompilerSettings()
    {
        ThrowOnAssemblyResolveErrors = false
    });

    // 3. Get method handle from metadata token
    var handle = MetadataTokens.MethodDefinitionHandle(method.MetadataToken);
    
    // 4. Decompile to string
    return decompiler.DecompileAsString(handle);
}
```

### Why These Settings?
- `ThrowOnAssemblyResolveErrors = false`: Allows decompiling even if dependencies are missing

---

## Execution Risk Assessment

| Risk | Level | Mitigation |
|------|-------|------------|
| Code execution | 🟢 None | Decompiler only reads metadata |
| Memory usage | 🟡 Medium | Large assemblies may use significant memory |
| File access | 🟢 Low | Only reads the target DLL file |
| Network access | 🟢 None | No network calls |

**Conclusion**: Safe for our use case. No code execution, only metadata analysis.

---

## Comparison with Alternatives

| Tool | Type | Pros | Cons |
|------|------|------|------|
| **ICSharpCode.Decompiler** | Library | MIT license, active development, full C# 12 support | Large dependency |
| dnlib | Library | Lightweight | Lower-level, no C# output |
| Mono.Cecil | Library | Mature, widely used | No decompilation, only IL |
| dnSpy | Application | Best UI | Not a library, harder to embed |

**Choice Rationale**: ICSharpCode.Decompiler is the only library option that produces readable C# code.

---

## Version History

| Version | Changes |
|---------|---------|
| 9.1.0 | C# 12 support, improved async detection |
| 8.0.0 | C# 11 support, record types |
| 7.0.0 | C# 10 support, file-scoped namespaces |

---

## Updating

To update the package:
```powershell
cd src/DotNetDllInvoker.Reflection
dotnet add package ICSharpCode.Decompiler --version <new-version>
```

After updating, verify:
1. `DecompilerService.Decompile()` still works
2. Complex async methods decompile correctly
3. Generic types display properly
