// File: ui/DotNetDllInvoker.UI/ViewModels/ResultViewModel.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Wraps InvocationResult for UI display. Formats output, duration, and errors.
//
// Depends on:
// - DotNetDllInvoker.Results
//
// Used by:
// - MainViewModel (LastResult property)
// - MainWindow.xaml (Result panel bindings)
//
// Execution Risk:
// None. Pure data formattin.

using System;
using DotNetDllInvoker.Results;

namespace DotNetDllInvoker.UI.ViewModels;

public class ResultViewModel : ViewModelBase
{
    private readonly InvocationResult _result;

    public ResultViewModel(InvocationResult result)
    {
        _result = result ?? throw new ArgumentNullException(nameof(result));
    }

    public bool IsSuccess => _result.IsSuccess;
    public string Duration => $"{_result.Duration.TotalMilliseconds:F2} ms";
    
    public string OutputText
    {
        get
        {
            var text = "";
            if (_result.CapturedStdOut.Length > 0)
                text += $"[STDOUT]\n{_result.CapturedStdOut}\n";
            if (_result.CapturedStdErr.Length > 0)
                text += $"[STDERR]\n{_result.CapturedStdErr}\n";
            return text;
        }
    }

    public string ResultValue
    {
        get
        {
            if (!_result.IsSuccess)
            {
                return $"[ERROR] {_result.Error?.Message}\n{_result.Error?.StackTrace}";
            }
            return _result.ReturnValue?.ToString() ?? "<null>";
        }
    }
}
