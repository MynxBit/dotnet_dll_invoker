// File: ui/DotNetDllInvoker.UI/ViewModels/EnumParameterViewModel.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// ViewModel for enum parameter types with dropdown selection.
//
// Depends on:
// - System.Reflection
//
// Execution Risk:
// None. UI logic only.

using System;
using System.Reflection;

namespace DotNetDllInvoker.UI.ViewModels;

public class EnumParameterViewModel : ParameterViewModel
{
    private object? _selectedValue;

    public EnumParameterViewModel(ParameterInfo info) : base(info)
    {
        EnumType = info.ParameterType;
        EnumValues = Enum.GetValues(EnumType);
        
        // Default to first value
        if (EnumValues.Length > 0)
        {
            _selectedValue = EnumValues.GetValue(0);
        }
    }

    public Type EnumType { get; }
    public Array EnumValues { get; }

    public object? SelectedValue
    {
        get => _selectedValue;
        set => SetProperty(ref _selectedValue, value);
    }

    public override object? GetValue() => SelectedValue;
}
