// File: ui/DotNetDllInvoker.UI/ViewModels/IntParameterViewModel.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// ParameterViewModel for integer input. Binds to TextBox with int binding.
//
// Depends on:
// - ParameterViewModel (base)
// - System.Reflection
//
// Used by:
// - MethodViewModel (creates for int parameters)
// - MainWindow.xaml (IntTemplate)
//
// Execution Risk:
// None. Pure data binding.

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
