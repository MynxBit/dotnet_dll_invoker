using System.Reflection;
using System.Runtime.Loader;

namespace DotNetDllInvoker.Contracts;

public class LoadedAssemblyInfo
{
    public Assembly Assembly { get; }
    public AssemblyLoadContext Context { get; }
    public string FilePath { get; }
    public string Name { get; }

    public LoadedAssemblyInfo(Assembly assembly, AssemblyLoadContext context, string filePath)
    {
        Assembly = assembly;
        Context = context;
        FilePath = filePath;
        Name = assembly.GetName().Name ?? "Unknown";
    }

    /// <summary>
    /// Unloads the assembly from memory by unloading its AssemblyLoadContext.
    /// Note: Actual memory release happens when GC collects and there are no remaining references.
    /// </summary>
    public void Unload()
    {
        try
        {
            Context.Unload();
        }
        catch
        {
            // Context may already be unloaded or not collectible
        }
    }
}

