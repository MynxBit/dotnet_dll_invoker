// File: ui/DotNetDllInvoker.UI/ViewModels/ObjectWorkbenchViewModel.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// ViewModel for the Object Workbench panel.
// Displays registered instances and allows inspection.
//
// Depends on:
// - DotNetDllInvoker.Core (InstanceRegistry)
//
// Execution Risk:
// None. UI logic only.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using DotNetDllInvoker.Core;

namespace DotNetDllInvoker.UI.ViewModels;

public class ObjectWorkbenchViewModel : ViewModelBase
{
    private readonly InstanceRegistry _registry;
    private InstanceEntryViewModel? _selectedInstance;

    public ObjectWorkbenchViewModel(InstanceRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        Instances = new ObservableCollection<InstanceEntryViewModel>();
        
        RefreshCommand = new RelayCommand(ExecuteRefresh);
        RemoveCommand = new RelayCommand(ExecuteRemove, CanExecuteRemove);
        ClearAllCommand = new RelayCommand(ExecuteClearAll, CanExecuteClearAll);
    }

    public ObservableCollection<InstanceEntryViewModel> Instances { get; }
    
    public InstanceEntryViewModel? SelectedInstance
    {
        get => _selectedInstance;
        set
        {
            if (SetProperty(ref _selectedInstance, value))
            {
                value?.LoadProperties();
            }
        }
    }

    public ICommand RefreshCommand { get; }
    public ICommand RemoveCommand { get; }
    public ICommand ClearAllCommand { get; }

    public void Refresh()
    {
        Instances.Clear();
        foreach (var entry in _registry.GetAll())
        {
            Instances.Add(new InstanceEntryViewModel(entry));
        }
    }

    private void ExecuteRefresh(object? obj) => Refresh();

    private void ExecuteRemove(object? obj)
    {
        if (SelectedInstance != null)
        {
            _registry.Remove(SelectedInstance.Id);
            Instances.Remove(SelectedInstance);
            SelectedInstance = null;
        }
    }

    private bool CanExecuteRemove(object? obj) => SelectedInstance != null;

    private void ExecuteClearAll(object? obj)
    {
        _registry.Clear();
        Instances.Clear();
        SelectedInstance = null;
    }

    private bool CanExecuteClearAll(object? obj) => Instances.Count > 0;
}

public class InstanceEntryViewModel : ViewModelBase
{
    private readonly InstanceEntry _entry;

    public InstanceEntryViewModel(InstanceEntry entry)
    {
        _entry = entry;
        Properties = new ObservableCollection<PropertyViewModel>();
    }

    public string Id => _entry.Id;
    public string Label => _entry.Label ?? _entry.Type.Name;
    public string TypeName => _entry.Type.FullName ?? _entry.Type.Name;
    public DateTime CreatedAt => _entry.CreatedAt;
    public object Instance => _entry.Instance;

    public ObservableCollection<PropertyViewModel> Properties { get; }

    public void LoadProperties()
    {
        Properties.Clear();
        
        try
        {
            var props = _entry.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props.Take(50)) // Limit for performance
            {
                try
                {
                    var value = prop.GetValue(_entry.Instance);
                    Properties.Add(new PropertyViewModel(prop.Name, prop.PropertyType.Name, value));
                }
                catch
                {
                    Properties.Add(new PropertyViewModel(prop.Name, prop.PropertyType.Name, "<error>"));
                }
            }
        }
        catch
        {
            // Type reflection failed
        }
    }
}

public class PropertyViewModel
{
    public PropertyViewModel(string name, string typeName, object? value)
    {
        Name = name;
        TypeName = typeName;
        ValueString = value?.ToString() ?? "null";
    }

    public string Name { get; }
    public string TypeName { get; }
    public string ValueString { get; }
}
