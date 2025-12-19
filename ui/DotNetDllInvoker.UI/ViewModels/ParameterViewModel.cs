// File: ui/DotNetDllInvoker.UI/ViewModels/ParameterViewModel.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Abstract base class for parameter input ViewModels.
// Provides common properties (Name, TypeName) and abstract GetValue() method.
//
// Depends on:
// - System.Reflection
//
// Used by:
// - StringParameterViewModel, IntParameterViewModel, BoolParameterViewModel,
//   EnumParameterViewModel, JsonParameterViewModel, CollectionParameterViewModel
// - MethodViewModel (creates parameter VMs)
// - MainWindow.xaml (ParameterTemplateSelector)
//
// Execution Risk:
// None. Pure data container.

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
