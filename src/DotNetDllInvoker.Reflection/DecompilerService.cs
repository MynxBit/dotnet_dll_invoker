using System;
using System.IO;
using System.Reflection;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.Metadata;

namespace DotNetDllInvoker.Reflection;

public class DecompilerService
{
    public static string Decompile(MethodBase method)
    {
        if (method == null) return "// Error: Method is null";
        if (method.DeclaringType == null) return "// Error: DeclaringType is null";
        
        var assembly = method.DeclaringType.Assembly;
        if (string.IsNullOrEmpty(assembly.Location) || !File.Exists(assembly.Location))
        {
            return "// Error: Cannot decompile in-memory or dynamic assembly.\n// Assembly file not found on disk.";
        }

        try
        {
            var decompiler = new CSharpDecompiler(assembly.Location, new DecompilerSettings()
            {
                ThrowOnAssemblyResolveErrors = false
            });

            // Create Handle from Token
            // Requires System.Reflection.Metadata
            var handle = System.Reflection.Metadata.Ecma335.MetadataTokens.MethodDefinitionHandle(method.MetadataToken);
            
            return decompiler.DecompileAsString(handle);
        }
        catch (Exception ex)
        {
            return $"// Decompilation Error: {ex.Message}\n\n// Full Stack:\n// {ex.StackTrace}";
        }
    }
}
