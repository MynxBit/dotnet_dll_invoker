// File: ui/DotNetDllInvoker.UI/ViewModels/CollectionParameterViewModel.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// ViewModel for collection parameters (List<T>, T[]) with dynamic item editing.
//
// Depends on:
// - System.Reflection
// - System.Collections.Generic
//
// Used by:
// - MethodViewModel (creates for array/list parameters)
// - MainWindow.xaml (CollectionTemplate)
//
// Execution Risk:
// None. UI logic only.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Input;

namespace DotNetDllInvoker.UI.ViewModels;

public class CollectionParameterViewModel : ParameterViewModel
{
    public CollectionParameterViewModel(ParameterInfo info) : base(info)
    {
        ElementType = GetElementType(info.ParameterType);
        Items = new ObservableCollection<CollectionItemViewModel>();
        
        AddItemCommand = new RelayCommand(ExecuteAddItem);
        RemoveItemCommand = new RelayCommand(ExecuteRemoveItem);
        
        // Add one empty item by default
        ExecuteAddItem(null);
    }

    public Type ElementType { get; }
    public ObservableCollection<CollectionItemViewModel> Items { get; }
    
    public ICommand AddItemCommand { get; }
    public ICommand RemoveItemCommand { get; }

    public override object? GetValue()
    {
        var paramType = Info.ParameterType;
        
        if (paramType.IsArray)
        {
            var array = Array.CreateInstance(ElementType, Items.Count);
            for (int i = 0; i < Items.Count; i++)
            {
                array.SetValue(Items[i].GetConvertedValue(ElementType), i);
            }
            return array;
        }
        
        // Assume List<T>
        var listType = typeof(List<>).MakeGenericType(ElementType);
        var list = (IList)Activator.CreateInstance(listType)!;
        
        foreach (var item in Items)
        {
            list.Add(item.GetConvertedValue(ElementType));
        }
        
        return list;
    }

    private void ExecuteAddItem(object? obj)
    {
        Items.Add(new CollectionItemViewModel(ElementType, Items.Count));
    }

    private void ExecuteRemoveItem(object? obj)
    {
        if (obj is CollectionItemViewModel item && Items.Contains(item))
        {
            Items.Remove(item);
            // Renumber items
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].Index = i;
            }
        }
        else if (Items.Count > 0)
        {
            Items.RemoveAt(Items.Count - 1);
        }
    }

    private static Type GetElementType(Type collectionType)
    {
        if (collectionType.IsArray)
            return collectionType.GetElementType() ?? typeof(object);
            
        if (collectionType.IsGenericType)
            return collectionType.GetGenericArguments()[0];
            
        return typeof(object);
    }
}

public class CollectionItemViewModel : ViewModelBase
{
    private string _value = string.Empty;
    private int _index;

    public CollectionItemViewModel(Type elementType, int index)
    {
        ElementType = elementType;
        _index = index;
    }

    public Type ElementType { get; }

    public int Index
    {
        get => _index;
        set => SetProperty(ref _index, value);
    }

    public string Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public object? GetConvertedValue(Type targetType)
    {
        if (string.IsNullOrEmpty(_value))
        {
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }

        try
        {
            if (targetType == typeof(string)) return _value;
            return Convert.ChangeType(_value, targetType);
        }
        catch
        {
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }
    }
}
