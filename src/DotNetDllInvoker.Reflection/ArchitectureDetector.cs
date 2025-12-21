// ═══════════════════════════════════════════════════════════════════════════
// FILE: ArchitectureDetector.cs
// PATH: src/DotNetDllInvoker.Reflection/ArchitectureDetector.cs
// LAYER: Business (Reflection)
// ═══════════════════════════════════════════════════════════════════════════
//
// PRIMARY RESPONSIBILITY:
//   Determines the target architecture (x86, x64, AnyCPU) of a DLL file by parsing its PE headers.
//
// SECONDARY RESPONSIBILITIES:
//   - Detecting if a DLL is Native or Managed.
//   - Checking compatibility between the target DLL and the current running process.
//
// NON-RESPONSIBILITIES:
//   - Loading the assembly (AssemblyLoadContext does that).
//   - Executing any code from the DLL.
//
// ───────────────────────────────────────────────────────────────────────────
// DEPENDENCIES:
//   - System.Reflection.PortableExecutable.PEReader -> For reading headers.
//   - System.Runtime.InteropServices.RuntimeInformation -> For process architecture.
//
// DEPENDENTS:
//   - MainViewModel -> Uses this to validate DLLs before loading.
//
// ───────────────────────────────────────────────────────────────────────────
// CHANGE LOG:
//   2025-12-21 - Antigravity - Created for V14 bitness compatibility checks.
// ═══════════════════════════════════════════════════════════════════════════

using System;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;

namespace DotNetDllInvoker.Reflection;

public enum DllArchitecture
{
    Unknown,
    AnyCPU,
    AnyCPU_Prefer32Bit,
    x86,
    x64,
    Native_x86,
    Native_x64
}

public static class ArchitectureDetector
{
    /// <summary>
    /// Detects the target architecture of a DLL without loading it.
    /// </summary>
    public static DllArchitecture Detect(string dllPath)
    {
        if (!File.Exists(dllPath))
            return DllArchitecture.Unknown;

        try
        {
            using var stream = File.OpenRead(dllPath);
            using var pe = new PEReader(stream);

            var coffHeader = pe.PEHeaders.CoffHeader;
            var corHeader = pe.PEHeaders.CorHeader;

            // Native DLL (no CLR header)
            if (corHeader == null)
            {
                return coffHeader.Machine switch
                {
                    Machine.I386 => DllArchitecture.Native_x86,
                    Machine.Amd64 => DllArchitecture.Native_x64,
                    _ => DllArchitecture.Unknown
                };
            }

            // Managed .NET Assembly
            var corFlags = corHeader.Flags;
            bool isILOnly = (corFlags & CorFlags.ILOnly) != 0;
            bool is32BitRequired = (corFlags & CorFlags.Requires32Bit) != 0;
            // CorFlags.Prefer32Bit = 0x20000 (may not be in all SDK versions)
            bool is32BitPreferred = ((int)corFlags & 0x20000) != 0;

            // Decision matrix based on PE Machine and CorFlags
            if (coffHeader.Machine == Machine.Amd64)
                return DllArchitecture.x64;

            if (coffHeader.Machine == Machine.I386)
            {
                if (is32BitRequired)
                    return DllArchitecture.x86;
                if (is32BitPreferred)
                    return DllArchitecture.AnyCPU_Prefer32Bit;
                return DllArchitecture.AnyCPU;
            }

            return DllArchitecture.Unknown;
        }
        catch
        {
            return DllArchitecture.Unknown;
        }
    }

    /// <summary>
    /// Gets the architecture of the current running process.
    /// </summary>
    public static DllArchitecture GetCurrentProcessArchitecture()
    {
        return RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X86 => DllArchitecture.x86,
            Architecture.X64 => DllArchitecture.x64,
            Architecture.Arm64 => DllArchitecture.x64, // Treat ARM64 as 64-bit for compatibility
            _ => DllArchitecture.Unknown
        };
    }

    /// <summary>
    /// Checks if the DLL can be loaded by the current process.
    /// </summary>
    public static (bool IsCompatible, string Message) CheckCompatibility(string dllPath)
    {
        var dllArch = Detect(dllPath);
        var processArch = GetCurrentProcessArchitecture();

        // AnyCPU is always compatible
        if (dllArch == DllArchitecture.AnyCPU || dllArch == DllArchitecture.AnyCPU_Prefer32Bit)
            return (true, $"DLL is {dllArch} - Compatible with any process.");

        // Exact match
        if ((dllArch == DllArchitecture.x64 || dllArch == DllArchitecture.Native_x64) && processArch == DllArchitecture.x64)
            return (true, "DLL is x64 - Compatible.");

        if ((dllArch == DllArchitecture.x86 || dllArch == DllArchitecture.Native_x86) && processArch == DllArchitecture.x86)
            return (true, "DLL is x86 - Compatible.");

        // Mismatch
        string suggestion = processArch == DllArchitecture.x64
            ? "Use the x86 version of this tool (DotNetDllInvoker.UI.x86.exe)."
            : "Use the x64 version of this tool (DotNetDllInvoker.UI.x64.exe).";

        return (false, $"Architecture Mismatch! DLL is {dllArch}, but this tool is {processArch}. {suggestion}");
    }

    /// <summary>
    /// Returns a short display string for UI.
    /// </summary>
    public static string GetDisplayName(DllArchitecture arch) => arch switch
    {
        DllArchitecture.AnyCPU => "AnyCPU",
        DllArchitecture.AnyCPU_Prefer32Bit => "AnyCPU (32-bit preferred)",
        DllArchitecture.x86 => "x86",
        DllArchitecture.x64 => "x64",
        DllArchitecture.Native_x86 => "Native x86",
        DllArchitecture.Native_x64 => "Native x64",
        _ => "Unknown"
    };
}
