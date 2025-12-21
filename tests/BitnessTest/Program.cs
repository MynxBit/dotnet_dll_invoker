// Bitness Loading Test
// Tests what happens when loading x86 DLL from x64 process and vice versa

using System;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.InteropServices;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine($"Current Process Architecture: {RuntimeInformation.ProcessArchitecture}");
        Console.WriteLine($"OS Architecture: {RuntimeInformation.OSArchitecture}");
        Console.WriteLine();

        string x86DllPath = @"C:\Users\mayan\.gemini\antigravity\scratch\dotnet_dll_invoker\tests\TestManagedDll_x86\bin\Release\net8.0\TestManagedDll_x86.dll";
        string x64DllPath = @"C:\Users\mayan\.gemini\antigravity\scratch\dotnet_dll_invoker\tests\TestManagedDll_x64\bin\Release\net8.0\TestManagedDll_x64.dll";

        Console.WriteLine("=== Test 1: Loading x86 DLL ===");
        TryLoadDll(x86DllPath);

        Console.WriteLine();
        Console.WriteLine("=== Test 2: Loading x64 DLL ===");
        TryLoadDll(x64DllPath);
    }

    static void TryLoadDll(string path)
    {
        try
        {
            string fullPath = Path.GetFullPath(path);
            Console.WriteLine($"Attempting to load: {fullPath}");
            
            var context = new AssemblyLoadContext("Test", isCollectible: true);
            var assembly = context.LoadFromAssemblyPath(fullPath);
            
            Console.WriteLine($"✅ SUCCESS! Loaded: {assembly.GetName().Name}");
            
            // Try to get types
            foreach (var type in assembly.GetExportedTypes())
            {
                Console.WriteLine($"   Found type: {type.FullName}");
            }
            
            context.Unload();
        }
        catch (BadImageFormatException ex)
        {
            Console.WriteLine($"❌ FAILED: BadImageFormatException");
            Console.WriteLine($"   Message: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FAILED: {ex.GetType().Name}");
            Console.WriteLine($"   Message: {ex.Message}");
        }
    }
}
