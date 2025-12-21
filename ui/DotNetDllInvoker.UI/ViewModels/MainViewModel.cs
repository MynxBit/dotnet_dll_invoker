// ═══════════════════════════════════════════════════════════════════════════
// FILE: MainViewModel.cs
// PATH: ui/DotNetDllInvoker.UI/ViewModels/MainViewModel.cs
// LAYER: Presentation (ViewModel)
// ═══════════════════════════════════════════════════════════════════════════
//
// PRIMARY RESPONSIBILITY:
//   Main orchestration logic for the UI, serving as the connection between the View (MainWindow) and the Core Dispatcher.
//
// SECONDARY RESPONSIBILITIES:
//   - Managing application state (Active Assembly, Selected Method, Invocation Results).
//   - Handling UI commands (Load, Invoke, Unload).
//   - Routing invocations between Direct Mode (process-internal) and Stealth Mode (IPC).
//
// NON-RESPONSIBILITIES:
//   - Direct Reflection (delegated to Core).
//   - Direct Execution (delegated to StealthInvoker or CommandDispatcher).
//
// ───────────────────────────────────────────────────────────────────────────
// DEPENDENCIES:
//   - DotNetDllInvoker.Core.CommandDispatcher -> Backend API.
//   - DotNetDllInvoker.UI.Services.StealthInvoker -> V14 Stealth Service.
//
// DEPENDENTS:
//   - MainWindow.xaml -> DataBinding target.
//
// ───────────────────────────────────────────────────────────────────────────
// CHANGE LOG:
//   2025-12-20 - Antigravity - Added Architecture Detection logic.
//   2025-12-21 - Antigravity - Integrated Stealth Mode routing.
// ═══════════════════════════════════════════════════════════════════════════

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
    private readonly Services.RecentFilesService _recentFilesService;
    private readonly Services.StealthInvoker _stealthInvoker;
    
    private string _statusText = "Ready";
    private bool _isBusy;
    private bool _isStealthModeEnabled;

    private MethodViewModel? _selectedMethod;
    private ResultViewModel? _lastResult;

    private string _windowTitle = "DotNet DLL Invoker";

    public MainViewModel()
    {
        // Load default title with bitness
        string bitness = Environment.Is64BitProcess ? "x64" : "x86";
        WindowTitle = $"DotNet DLL Invoker (v14.0) - Process: {bitness}";

        // Composition Root: Use CommandDispatcher's internal composition for simplicity
        _dispatcher = new CommandDispatcher();
        _recentFilesService = new Services.RecentFilesService();
        _stealthInvoker = new Services.StealthInvoker();

        LoadDllCommand = new RelayCommand(ExecuteLoadDll, _ => !IsBusy);
        InvokeAllCommand = new RelayCommand(ExecuteInvokeAll, _ => !IsBusy && Methods.Count > 0);
        OpenRecentCommand = new RelayCommand(ExecuteOpenRecent);
        
        Methods = new ObservableCollection<MethodViewModel>();
        LoadedAssemblies = new ObservableCollection<DotNetDllInvoker.Contracts.LoadedAssemblyInfo>();
        Dependencies = new ObservableCollection<DotNetDllInvoker.Contracts.DependencyRecord>();
        
        ShowDependenciesCommand = new RelayCommand(ExecuteShowDependencies);
        CloseDependencyPanelCommand = new RelayCommand(ExecuteCloseDependencyPanel);
        ShowCallGraphCommand = new RelayCommand(ExecuteShowCallGraph);
        UnloadDllCommand = new RelayCommand(ExecuteUnloadDll);
    }

    public ObservableCollection<MethodViewModel> Methods { get; }
    
    // Search Filter
    private string _methodSearchText = string.Empty;
    public string MethodSearchText
    {
        get => _methodSearchText;
        set
        {
            if (SetProperty(ref _methodSearchText, value))
            {
                OnPropertyChanged(nameof(FilteredMethods));
            }
        }
    }
    
    public IEnumerable<MethodViewModel> FilteredMethods
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_methodSearchText))
                return Methods;
            return Methods.Where(m => m.Name.Contains(_methodSearchText, StringComparison.OrdinalIgnoreCase));
        }
    }

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
    public ICommand OpenRecentCommand { get; }
    
    public IReadOnlyList<string> RecentFiles => _recentFilesService.RecentFiles;

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

    /// <summary>
    /// V14: When enabled, method invocation is routed through a pre-warmed CLI worker process
    /// for minimal process noise during analysis.
    /// </summary>
    public bool IsStealthModeEnabled
    {
        get => _isStealthModeEnabled;
        set => SetProperty(ref _isStealthModeEnabled, value);
    }

    public MethodViewModel? SelectedMethod
    {
        get => _selectedMethod;
        set 
        {
            if (SetProperty(ref _selectedMethod, value))
            {
                value?.LoadCodesAsync();
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

    public string WindowTitle
    {
        get => _windowTitle;
        set => SetProperty(ref _windowTitle, value);
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

    /// <summary>
    /// Public entry point for loading DLLs from Drag & Drop.
    /// </summary>
    public void LoadDllFromPath(string path)
    {
        LoadAssembly(path);
    }

    private async void LoadAssembly(string path)
    {
        try
        {
            IsBusy = true;
            StatusText = $"Loading {path}...";

            // V13.3: Check Architecture Compatibility
            var dllArch = ArchitectureDetector.Detect(path);
            var (isCompatible, compatMessage) = ArchitectureDetector.CheckCompatibility(path);

            if (!isCompatible)
            {
                var result = MessageBox.Show(
                    $"{compatMessage}\n\nDo you want to continue anyway?",
                    "Architecture Mismatch",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                {
                    StatusText = "Load cancelled due to architecture mismatch.";
                    IsBusy = false;
                    return;
                }
            }

            await Task.Run(() => _dispatcher.LoadAssembly(path));

            // Sync UI list with backend
            LoadedAssemblies.Clear();
            foreach(var asm in _dispatcher.State.LoadedAssemblies)
            {
                LoadedAssemblies.Add(asm);
            }
            
            // Auto Select the latest
            SelectedAssembly = _dispatcher.State.ActiveAssembly;
            
            // V13.3: Show architecture in status
            var archDisplay = ArchitectureDetector.GetDisplayName(dllArch);
            StatusText = $"Loaded {System.IO.Path.GetFileName(path)} [{archDisplay}].";
            
            // Track in recent files
            _recentFilesService.AddRecent(path);
            OnPropertyChanged(nameof(RecentFiles));
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
    
    private void ExecuteOpenRecent(object? obj)
    {
        if (obj is string path && !string.IsNullOrEmpty(path))
        {
            LoadAssembly(path);
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
    public ICommand ShowCallGraphCommand { get; }

    private void ExecuteShowCallGraph(object? obj)
    {
        var targetAssembly = obj as DotNetDllInvoker.Contracts.LoadedAssemblyInfo ?? SelectedAssembly;
        
        if (targetAssembly?.Assembly == null) 
        {
            StatusText = "No assembly selected";
            return;
        }
        
        StatusText = $"Opening Call Graph for {targetAssembly.Name}...";
        Views.CallGraphWindow.ShowForAssembly(targetAssembly.Assembly);
    }

    public ICommand UnloadDllCommand { get; }

    private void ExecuteUnloadDll(object? obj)
    {
        var targetAssembly = obj as DotNetDllInvoker.Contracts.LoadedAssemblyInfo ?? SelectedAssembly;
        
        if (targetAssembly == null) return;
        
        var assemblyName = targetAssembly.Name;
        
        // Clear methods if this was the active one
        if (SelectedAssembly == targetAssembly)
        {
            Methods.Clear();
            Dependencies.Clear();
            SelectedAssembly = null;
        }
        
        // Remove from UI list
        LoadedAssemblies.Remove(targetAssembly);
        
        // CRITICAL: Remove from backend state AND unload from memory
        // This calls Context.Unload() internally
        _dispatcher.State.RemoveAssembly(targetAssembly);
        
        // Force GC to help release the assembly
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        // Select another assembly if available
        if (SelectedAssembly == null && LoadedAssemblies.Count > 0)
        {
            SelectedAssembly = LoadedAssemblies.FirstOrDefault();
        }
        
        StatusText = $"Unloaded {assemblyName} from memory";
    }

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

            DotNetDllInvoker.Results.InvocationResult result;

            if (IsStealthModeEnabled)
            {
                // V14: Route through pre-warmed CLI worker for minimal noise
                StatusText = $"[Stealth] Invoking {methodVM.Name}...";
                
                // Serialize args to strings for CLI
                var stringArgs = new string[methodVM.Parameters.Count];
                for (int i = 0; i < methodVM.Parameters.Count; i++)
                {
                    var input = methodVM.Parameters[i].GetValue();
                    stringArgs[i] = input?.ToString() ?? "";
                }

                var dllPath = _selectedAssembly?.FilePath ?? "";
                result = await _stealthInvoker.InvokeAsync(dllPath, methodVM.Name, stringArgs);

                // ENHANCEMENT: Prepend Stealth Metadata to Output for filtering
                var debugHeader = $"""
                    --------------------------------------------------------------------------------
                    [STEALTH MODE EXECUTION]
                    • Worker Process: DotNetDllInvoker.CLI.exe (PID: {_stealthInvoker.WorkerPid})
                    • Payload: {{ action: "invoke", method: "{methodVM.Name}", args: [{string.Join(", ", stringArgs)}] }}
                    • Tip: Filter Process Monitor by PID {_stealthInvoker.WorkerPid} to isolate behavior.
                    --------------------------------------------------------------------------------
                    
                    """;

                // Reconstruct result with prepended header
                var newStdout = debugHeader + (result.CapturedStdOut ?? "");
                // We have to use reflection or create a new result because properties are immutable?
                // InvocationResult is likely immutable. Let's create a new Success/Failure clone.
                
                if (result.IsSuccess)
                {
                    result = DotNetDllInvoker.Results.InvocationResult.Success(
                        result.ReturnValue, 
                        result.ExecutionDuration, 
                        newStdout, 
                        result.CapturedStdErr);
                }
                else
                {
                     result = DotNetDllInvoker.Results.InvocationResult.Failure(
                        result.Error!, 
                        result.ExecutionDuration, 
                        newStdout, 
                        result.CapturedStdErr);
                }
            }
            else
            {
                // Normal mode: Direct invocation
                // 1. Gather Arguments from UI (Auto-generate if empty)
                var args = new object[methodVM.Parameters.Count];
                for (int i = 0; i < methodVM.Parameters.Count; i++)
                {
                    var input = methodVM.Parameters[i].GetValue();
                    if (IsInputEmpty(input))
                    {
                        // Auto-generate based on type
                        var paramInfo = methodVM.MethodBase.GetParameters()[i];
                        args[i] = _dispatcher.GenerateAutoParameter(paramInfo.ParameterType);
                    }
                    else
                    {
                        args[i] = input;
                    }
                }

                // 2. Invoke (Dispatcher handles async)
                // Use ResolvedMethod to ensure Generics are closed
                result = await _dispatcher.InvokeMethod(methodVM.ResolvedMethod, args);
            }

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
