// File: ui/DotNetDllInvoker.UI/ViewModels/ViewModelBase.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Base class for all ViewModels. Provides INotifyPropertyChanged pattern.
// Enables WPF data binding to react to property changes.
//
// Depends on:
// - System.ComponentModel
// - System.Runtime.CompilerServices
//
// Used by:
// - All ViewModel classes (MainViewModel, MethodViewModel, ParameterViewModel, etc.)
//
// Execution Risk:
// None. Pure infrastructure.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DotNetDllInvoker.UI.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(storage, value)) return false;
        
        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
