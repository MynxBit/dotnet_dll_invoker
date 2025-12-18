// File: ui/DotNetDllInvoker.UI/ViewModels/GenericTypeArgumentViewModel.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// ViewModel for selecting type arguments for generic methods.
// Provides common types and allows custom type selection from loaded assemblies.
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

namespace DotNetDllInvoker.UI.ViewModels;

public class GenericTypeArgumentViewModel : ViewModelBase
{
    private Type? _selectedType;
    private string _customTypeName = string.Empty;

    public GenericTypeArgumentViewModel(Type genericParameter, Assembly? loadedAssembly = null)
    {
        GenericParameterName = genericParameter.Name;
        
        // Initialize common types
        AvailableTypes = new ObservableCollection<Type>
        {
            typeof(int),
            typeof(string),
            typeof(bool),
            typeof(double),
            typeof(float),
            typeof(long),
            typeof(DateTime),
            typeof(Guid),
            typeof(object)
        };

        // Add types from loaded assembly if available
        if (loadedAssembly != null)
        {
            try
            {
                foreach (var type in loadedAssembly.GetExportedTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && t.IsPublic)
                    .Take(20)) // Limit to avoid overwhelming the UI
                {
                    AvailableTypes.Add(type);
                }
            }
            catch
            {
                // Ignore errors loading types
            }
        }

        // Default to object
        _selectedType = typeof(object);
    }

    public string GenericParameterName { get; }
    
    public ObservableCollection<Type> AvailableTypes { get; }

    public Type? SelectedType
    {
        get => _selectedType;
        set => SetProperty(ref _selectedType, value);
    }

    public string CustomTypeName
    {
        get => _customTypeName;
        set
        {
            if (SetProperty(ref _customTypeName, value) && !string.IsNullOrWhiteSpace(value))
            {
                // Try to resolve custom type name
                var resolvedType = Type.GetType(value);
                if (resolvedType != null)
                {
                    _selectedType = resolvedType;
                    OnPropertyChanged(nameof(SelectedType));
                }
            }
        }
    }

    public string DisplayName => $"{GenericParameterName}: {SelectedType?.Name ?? "?"}";
}
