// File: ui/DotNetDllInvoker.UI/ViewModels/StringParameterViewModel.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// ParameterViewModel for string input. Binds to TextBox.
//
// Depends on:
// - ParameterViewModel (base)
// - System.Reflection
//
// Used by:
// - MethodViewModel (creates for string parameters)
// - MainWindow.xaml (StringTemplate)
//
// Execution Risk:
// None. Pure data binding.

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
