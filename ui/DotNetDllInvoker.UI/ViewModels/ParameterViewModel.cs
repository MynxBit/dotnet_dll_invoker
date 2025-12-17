using System;
using System.Reflection;

namespace DotNetDllInvoker.UI.ViewModels;

public abstract class ParameterViewModel : ViewModelBase
{
    public ParameterInfo Info { get; }

    protected ParameterViewModel(ParameterInfo info)
    {
        Info = info ?? throw new ArgumentNullException(nameof(info));
    }

    public string Name => Info.Name ?? "Param";
    public string TypeName => Info.ParameterType.Name;

    // The value that the UI binds to
    public abstract object? GetValue();
}
