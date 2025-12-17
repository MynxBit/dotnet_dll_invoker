using System.Reflection;

namespace DotNetDllInvoker.UI.ViewModels;

public class StringParameterViewModel : ParameterViewModel
{
    private string _value = string.Empty;

    public StringParameterViewModel(ParameterInfo info) : base(info)
    {
    }

    public string Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public override object? GetValue() => Value;
}
