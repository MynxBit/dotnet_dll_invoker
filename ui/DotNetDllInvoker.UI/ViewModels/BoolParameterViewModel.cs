using System.Reflection;

namespace DotNetDllInvoker.UI.ViewModels;

public class BoolParameterViewModel : ParameterViewModel
{
    private bool _value;

    public BoolParameterViewModel(ParameterInfo info) : base(info)
    {
    }

    public bool Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public override object? GetValue() => Value;
}
