using System.Reflection;

namespace DotNetDllInvoker.UI.ViewModels;

public class IntParameterViewModel : ParameterViewModel
{
    private int _value;

    public IntParameterViewModel(ParameterInfo info) : base(info)
    {
    }

    public int Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public override object? GetValue() => Value;
}
