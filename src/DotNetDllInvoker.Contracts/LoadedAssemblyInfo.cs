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
}
