// File: ui/DotNetDllInvoker.UI/ViewModels/BoolParameterViewModel.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// ParameterViewModel for boolean input. Binds to CheckBox.
//
// Depends on:
// - ParameterViewModel (base)
// - System.Reflection
//
// Used by:
// - MethodViewModel (creates for bool parameters)
// - MainWindow.xaml (BoolTemplate)
//
// Execution Risk:
// None. Pure data binding.

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
