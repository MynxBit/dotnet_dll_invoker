using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DotNetDllInvoker.Core;
using DotNetDllInvoker.Dependency;
using DotNetDllInvoker.Execution;
using DotNetDllInvoker.Parameters;
using DotNetDllInvoker.Reflection;
using Microsoft.Win32;

namespace DotNetDllInvoker.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly CommandDispatcher _dispatcher;
    
    private string _statusText = "Ready";
    private bool _isBusy;

    private MethodViewModel? _selectedMethod;
    private ResultViewModel? _lastResult;

    public MainViewModel()
    {
        // Composition Root: Use CommandDispatcher's internal composition for simplicity
        _dispatcher = new CommandDispatcher();

        LoadDllCommand = new RelayCommand(ExecuteLoadDll, _ => !IsBusy);
        InvokeAllCommand = new RelayCommand(ExecuteInvokeAll, _ => !IsBusy && Methods.Count > 0);
        
        Methods = new ObservableCollection<MethodViewModel>();
        LoadedAssemblies = new ObservableCollection<DotNetDllInvoker.Contracts.LoadedAssemblyInfo>();
        Dependencies = new ObservableCollection<DotNetDllInvoker.Contracts.DependencyRecord>();
        
        ShowDependenciesCommand = new RelayCommand(ExecuteShowDependencies);
        CloseDependencyPanelCommand = new RelayCommand(ExecuteCloseDependencyPanel);
    }

    public ObservableCollection<MethodViewModel> Methods { get; }
    public ObservableCollection<DotNetDllInvoker.Contracts.LoadedAssemblyInfo> LoadedAssemblies { get; }

    private DotNetDllInvoker.Contracts.LoadedAssemblyInfo? _selectedAssembly;
    public DotNetDllInvoker.Contracts.LoadedAssemblyInfo? SelectedAssembly
    {
        get => _selectedAssembly;
        set 
        {
            if (SetProperty(ref _selectedAssembly, value) && value != null)
            {
                ActivateAssembly(value);
            }
        }
    }

    public ICommand LoadDllCommand { get; }
    public ICommand InvokeAllCommand { get; }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set 
        {
            if (SetProperty(ref _isBusy, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public MethodViewModel? SelectedMethod
    {
        get => _selectedMethod;
        set 
        {
            if (SetProperty(ref _selectedMethod, value))
            {
                OnPropertyChanged(nameof(IsMethodSelected));
            }
        }
    }

    public bool IsMethodSelected => _selectedMethod != null;

    public ResultViewModel? LastResult
    {
        get => _lastResult;
        set => SetProperty(ref _lastResult, value);
    }

    private void ExecuteLoadDll(object? obj)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Net Assemblies (*.dll)|*.dll|All Files (*.*)|*.*",
            Multiselect = true
        };

        if (dialog.ShowDialog() == true)
        {
            foreach(var file in dialog.FileNames)
            {
                LoadAssembly(file);
            }
        }
    }

    private async void LoadAssembly(string path)
    {
        try
        {
            IsBusy = true;
            StatusText = $"Loading {path}...";

            await Task.Run(() => _dispatcher.LoadAssembly(path));

            // Sync UI list with backend
            LoadedAssemblies.Clear();
            foreach(var asm in _dispatcher.State.LoadedAssemblies)
            {
                LoadedAssemblies.Add(asm);
            }
            
            // Auto Select the latest
            SelectedAssembly = _dispatcher.State.ActiveAssembly;
            
            StatusText = $"Loaded {System.IO.Path.GetFileName(path)}.";
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
            MessageBox.Show(ex.Message, "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    // Dependency Sidebar Logic
    public ObservableCollection<DotNetDllInvoker.Contracts.DependencyRecord> Dependencies { get; }

    private bool _isDependencyPanelVisible;
    public bool IsDependencyPanelVisible
    {
        get => _isDependencyPanelVisible;
        set => SetProperty(ref _isDependencyPanelVisible, value);
    }
    
    public ICommand ShowDependenciesCommand { get; }
    public ICommand CloseDependencyPanelCommand { get; }

    private void ExecuteShowDependencies(object? obj)
    {
        var targetAssembly = obj as DotNetDllInvoker.Contracts.LoadedAssemblyInfo ?? SelectedAssembly;
        
        if (targetAssembly == null) return;
        
        // If it's not the active one, we might need to load its dependencies? 
        // For V1, we only analyze Active Assembly fully.
        // But if user asks for dependencies of a loaded but inactive one, we should probably Activate it or at least show warning?
        // Let's Activate it implies selection. 
        // Simpler: Just Activate it.
        
        if (SelectedAssembly != targetAssembly)
        {
            SelectedAssembly = targetAssembly; // This triggers activation via setter? or we need to call Activate?
            // Setter calls SetProperty. Does it trigger Activation? 
            // The setter is:  set => SetProperty(ref _selectedMethod, value); Wait, that is SelectedMethod.
            // SelectedAssembly setter is likely just property set.
        }
        
        // Activate explicitly if needed
        ActivateAssembly(targetAssembly);

        IsDependencyPanelVisible = true;
    }
    
    private void ExecuteCloseDependencyPanel(object? obj)
    {
        IsDependencyPanelVisible = false;
    }

    private void ActivateAssembly(DotNetDllInvoker.Contracts.LoadedAssemblyInfo asm)
    {
        try 
        {
            _dispatcher.ActivateAssembly(asm);
            
            Methods.Clear();
            foreach (var method in _dispatcher.State.DiscoveredMethods)
            {
                 Methods.Add(new MethodViewModel(method, ExecuteInvokeMethod));
            }
            
            Dependencies.Clear();
            foreach (var dep in _dispatcher.State.Dependencies)
            {
                Dependencies.Add(dep);
            }
            
            StatusText = $"Active: {asm.Name} ({Methods.Count} methods)";
        }
        catch(Exception ex)
        {
             StatusText = "Error activating assembly.";
             MessageBox.Show(ex.Message);
        }
    }
    
    // Callback from MethodViewModel
    private async Task ExecuteInvokeMethod(MethodViewModel methodVM)
    {
        try
        {
            IsBusy = true;
            StatusText = $"Invoking {methodVM.Name}...";
            LastResult = null;

            // 1. Gather Arguments from UI (Auto-generate if empty)
            var args = new object[methodVM.Parameters.Count];
            for (int i = 0; i < methodVM.Parameters.Count; i++)
            {
                var input = methodVM.Parameters[i].GetValue();
                if (IsInputEmpty(input))
                {
                    // Auto-generate based on type
                    // methodVM.Parameters[i] doesn't easily expose Type, but we can look up via MethodBase
                    var paramInfo = methodVM.MethodBase.GetParameters()[i];
                    args[i] = _dispatcher.GenerateAutoParameter(paramInfo.ParameterType);
                }
                else
                {
                    args[i] = input;
                }
            }

            // 2. Invoke (Dispatcher handles async)
            var result = await _dispatcher.InvokeMethod(methodVM.MethodBase, args);

            // 3. Show Result
            LastResult = new ResultViewModel(result);
            StatusText = result.IsSuccess ? "Invocation Complete" : "Invocation Failed";
        }
        catch (Exception ex)
        {
            StatusText = "Invocation Error";
            MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    private async void ExecuteInvokeAll(object? obj)
    {
        if (Methods.Count == 0) return;
        
        var confirm = MessageBox.Show(
            "⚠️ DANGER: This will invoke ALL methods sequentially using auto-generated parameters.\n\nAre you sure?", 
            "Execute All Methods", 
            MessageBoxButton.YesNo, 
            MessageBoxImage.Warning);
            
        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            IsBusy = true;
            StatusText = "Invoking All Methods...";
            LastResult = null;

            var results = await _dispatcher.InvokeAllMethods();
            
            // For now, just show the last result or a summary
            StatusText = $"Executed {results.Count} methods.";
            
            if (results.Any())
            {
                // Create a summary result? Or just show the last one?
                // Providing a summary as the result view for now.
                var passed = results.Count(r => r.IsSuccess);
                var failed = results.Count(r => !r.IsSuccess);
                
                var dummyResult = DotNetDllInvoker.Results.InvocationResult.Success(
                    $"Batch Execution Complete.\nTotal: {results.Count}\nPassed: {passed}\nFailed: {failed}", 
                    TimeSpan.Zero, 
                    "See specific method logs (not implemented in V1 UI)", 
                    "");
                    
                LastResult = new ResultViewModel(dummyResult);
            }
        }
        catch (Exception ex)
        {
            StatusText = "Batch Execution Error";
            MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    private bool IsInputEmpty(object? input)
    {
        if (input == null) return true;
        if (input is string s && string.IsNullOrWhiteSpace(s)) return true;
        return false;
    }
}
