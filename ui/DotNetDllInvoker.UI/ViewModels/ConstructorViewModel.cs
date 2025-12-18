// File: ui/DotNetDllInvoker.UI/ViewModels/ConstructorViewModel.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// ViewModel for selecting and invoking constructors with parameters.
//
// Depends on:
// - System.Reflection
//
// Execution Risk:
// None. UI logic only.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using DotNetDllInvoker.Execution;

namespace DotNetDllInvoker.UI.ViewModels;

public class ConstructorViewModel : ViewModelBase
{
    private readonly Type _targetType;
    private ConstructorInfo? _selectedConstructor;

    public ConstructorViewModel(Type targetType)
    {
        _targetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
        
        Constructors = new ObservableCollection<ConstructorInfo>(
            InstanceFactory.GetConstructors(targetType));
        
        Parameters = new ObservableCollection<ParameterViewModel>();
        
        // Select first constructor by default
        if (Constructors.Count > 0)
        {
            SelectedConstructor = Constructors[0];
        }
    }

    public Type TargetType => _targetType;
    public string TypeName => _targetType.Name;
    public string FullTypeName => _targetType.FullName ?? _targetType.Name;

    public ObservableCollection<ConstructorInfo> Constructors { get; }
    public ObservableCollection<ParameterViewModel> Parameters { get; }

    public ConstructorInfo? SelectedConstructor
    {
        get => _selectedConstructor;
        set
        {
            if (SetProperty(ref _selectedConstructor, value))
            {
                RebuildParameters();
            }
        }
    }

    public string SelectedConstructorSignature => _selectedConstructor != null
        ? BuildSignature(_selectedConstructor)
        : "No constructor selected";

    /// <summary>
    /// Creates an instance using the selected constructor and parameter values.
    /// </summary>
    public object? CreateInstance()
    {
        if (_selectedConstructor == null)
            throw new InvalidOperationException("No constructor selected");

        var args = Parameters.Select(p => p.GetValue()).ToArray();
        var factory = new InstanceFactory();
        return factory.CreateInstance(_targetType, _selectedConstructor, args);
    }

    private void RebuildParameters()
    {
        Parameters.Clear();
        
        if (_selectedConstructor == null)
            return;

        foreach (var param in _selectedConstructor.GetParameters())
        {
            Parameters.Add(CreateParameterVM(param));
        }
        
        OnPropertyChanged(nameof(SelectedConstructorSignature));
    }

    private ParameterViewModel CreateParameterVM(ParameterInfo p)
    {
        var paramType = p.ParameterType;
        
        if (paramType == typeof(int)) return new IntParameterViewModel(p);
        if (paramType == typeof(bool)) return new BoolParameterViewModel(p);
        if (paramType.IsEnum) return new EnumParameterViewModel(p);
        
        return new StringParameterViewModel(p);
    }

    private static string BuildSignature(ConstructorInfo ctor)
    {
        var paramStrings = ctor.GetParameters()
            .Select(p => $"{p.ParameterType.Name} {p.Name}");
        return $"new {ctor.DeclaringType?.Name}({string.Join(", ", paramStrings)})";
    }
}
