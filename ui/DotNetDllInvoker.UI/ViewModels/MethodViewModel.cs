using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using DotNetDllInvoker.Reflection;

namespace DotNetDllInvoker.UI.ViewModels;

public class MethodViewModel : ViewModelBase
{
    private readonly MethodBase _method;
    private readonly Func<MethodViewModel, Task> _invokeCallback;

    public MethodViewModel(MethodBase method, Func<MethodViewModel, Task> invokeCallback)
    {
        _method = method ?? throw new ArgumentNullException(nameof(method));
        _invokeCallback = invokeCallback;

        // Initialize Parameters
        Parameters = new ObservableCollection<ParameterViewModel>();
        foreach (var p in _method.GetParameters())
        {
            Parameters.Add(CreateParameterVM(p));
        }

        InvokeCommand = new RelayCommand(ExecuteInvoke, CanExecuteInvoke);
    }

    public string Name => _method.Name;
    public string Signature => SignatureBuilder.BuildSignature(_method);
    public string ReturnType => (_method as MethodInfo)?.ReturnType.Name ?? "void";
    
    public ObservableCollection<ParameterViewModel> Parameters { get; }

    public ICommand InvokeCommand { get; }

    public string ILCode => BuildIL(_method);
    public string CSCode => BuildCSharpDeclaration(_method);
    
    private string BuildIL(MethodBase method)
    {
        var sb = new System.Text.StringBuilder();
        try
        {
            var instructions = ILReader.Read(method);
            if (instructions.Count == 0) sb.AppendLine("// No IL body");
            else foreach (var instr in instructions) sb.AppendLine(instr.ToString());
        }
        catch (Exception ex) { sb.AppendLine($"// Error reading IL: {ex.Message}"); }
        return sb.ToString();
    }

    private string BuildCSharpDeclaration(MethodBase method)
    {
        // Use external decompiler for full body
        return DecompilerService.Decompile(method);
    }
    
    // Used by MainViewModel to know which method to invoke
    public MethodBase MethodBase => _method;

    private void ExecuteInvoke(object? obj)
    {
        // Fire and forget via async void wrapper for ICommand?
        // Better: Delegate to MainViewModel to handle lifecycle.
        _ = _invokeCallback(this);
    }

    private bool CanExecuteInvoke(object? obj) => true; // Could lock if busy

    private ParameterViewModel CreateParameterVM(ParameterInfo p)
    {
        if (p.ParameterType == typeof(int)) return new IntParameterViewModel(p);
        if (p.ParameterType == typeof(bool)) return new BoolParameterViewModel(p);
        
        // Default to String for everything else (for V1 simplicity)
        // Complex objects would use a specialized 'JsonObjectParameterViewModel' later
        return new StringParameterViewModel(p);
    }
}
