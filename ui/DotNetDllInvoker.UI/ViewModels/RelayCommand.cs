// File: ui/DotNetDllInvoker.UI/ViewModels/RelayCommand.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Implements ICommand for MVVM button/action binding.
// Allows ViewModels to expose commands that Views can bind to.
//
// Depends on:
// - System.Windows.Input
//
// Used by:
// - MainViewModel (LoadDllCommand, InvokeAllCommand, etc.)
// - MethodViewModel (InvokeCommand)
// - All ViewModels with bindable commands
//
// Execution Risk:
// None. Pure infrastructure.

using System;
using System.Windows.Input;

namespace DotNetDllInvoker.UI.ViewModels;

public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;

    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute == null || _canExecute(parameter);
    }

    public void Execute(object? parameter)
    {
        _execute(parameter);
    }
}
