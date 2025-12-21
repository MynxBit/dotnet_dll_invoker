// File: ui/DotNetDllInvoker.UI/ViewModels/MethodViewModel.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Wraps a MethodBase for UI display. Provides properties for name, signature,
// parameters, IL code, and C# decompilation. Handles generic method resolution.
//
// Depends on:
// - DotNetDllInvoker.Reflection (ILReader, DecompilerService, SignatureBuilder)
// - System.Reflection
//
// Used by:
// - MainViewModel (Methods collection)
// - MainWindow.xaml (Methods ListBox)
//
// Execution Risk:
// Low. Calls decompiler and IL reader (metadata only, no execution).

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using DotNetDllInvoker.Reflection;

namespace DotNetDllInvoker.UI.ViewModels;

public class MethodViewModel : ViewModelBase
{
    private readonly MethodBase _method;
    private readonly Func<MethodViewModel, Task> _invokeCallback;
    private readonly Assembly? _loadedAssembly;

    public MethodViewModel(MethodBase method, Func<MethodViewModel, Task> invokeCallback, Assembly? loadedAssembly = null)
    {
        _method = method ?? throw new ArgumentNullException(nameof(method));
        _invokeCallback = invokeCallback;
        _loadedAssembly = loadedAssembly;

        // Initialize Parameters
        Parameters = new ObservableCollection<ParameterViewModel>();
        
        // Initialize Generic Type Arguments
        GenericTypeArguments = new ObservableCollection<GenericTypeArgumentViewModel>();
        
        if (IsGenericMethod && _method is MethodInfo methodInfo)
        {
            foreach (var typeParam in methodInfo.GetGenericArguments())
            {
                GenericTypeArguments.Add(new GenericTypeArgumentViewModel(typeParam, _loadedAssembly));
            }
        }
        
        // Build parameters (for generic methods, we'll rebuild after type resolution)
        RebuildParameters();

        InvokeCommand = new RelayCommand(ExecuteInvoke, CanExecuteInvoke);
    }

    public string Name => _method.Name;
    public string Signature => SignatureBuilder.BuildSignature(_method);
    public string ReturnType
    {
        get
        {
            try
            {
                return (_method as MethodInfo)?.ReturnType.Name ?? "void";
            }
            catch
            {
                return "Unknown";
            }
        }
    }
    
    public ObservableCollection<ParameterViewModel> Parameters { get; }
    
    /// <summary>
    /// Type arguments for generic methods (e.g., T, TResult)
    /// </summary>
    public ObservableCollection<GenericTypeArgumentViewModel> GenericTypeArguments { get; }
    
    /// <summary>
    /// True if this is a generic method definition (e.g., Method&lt;T&gt;)
    /// </summary>
    public bool IsGenericMethod => (_method as MethodInfo)?.IsGenericMethodDefinition == true;
    
    /// <summary>
    /// True if this is a constructor (.ctor)
    /// </summary>
    public bool IsConstructor => _method is ConstructorInfo;
    
    /// <summary>
    /// Warning message for constructors (invoke through Constructor Wizard instead)
    /// </summary>
    public string? LimitationWarning => IsConstructor 
        ? "⚠ Constructor: Use Object Workbench → Constructor Wizard to create instances" 
        : null;
    
    /// <summary>
    /// Display name with constructor/generic indicators
    /// </summary>
    public string DisplayName
    {
        get
        {
            var prefix = IsConstructor ? "[CTOR] " : "";
            var suffix = IsGenericMethod ? " <T>" : "";
            return $"{prefix}{Name}{suffix}";
        }
    }

    public ICommand InvokeCommand { get; }

    /// <summary>
    /// Returns the resolved MethodInfo with concrete type arguments applied.
    /// For non-generic methods, returns the original method.
    /// </summary>
    public MethodBase ResolvedMethod
    {
        get
        {
            if (!IsGenericMethod || _method is not MethodInfo methodInfo)
                return _method;

            var typeArgs = GenericTypeArguments
                .Select(g => g.SelectedType ?? typeof(object))
                .ToArray();

            try
            {
                return methodInfo.MakeGenericMethod(typeArgs);
            }
            catch
            {
                // If type constraints fail, return original (will fail at invoke time with clear error)
                return _method;
            }
        }
    }
    
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

    private string _ilCode = "";
    public string ILCode
    {
        get => _ilCode; 
        set => SetProperty(ref _ilCode, value);
    }

    private string _csCode = "";
    public string CSCode
    {
        get => _csCode;
        set => SetProperty(ref _csCode, value);
    }
    
    private bool _codesLoaded = false;
    public async void LoadCodesAsync()
    {
        if (_codesLoaded) return;
        _codesLoaded = true;

        CSCode = "// Loading...";
        ILCode = "// Loading...";

        await Task.Run(() => 
        {
            try
            {
                // Decompile Logic
                var cs = DecompilerService.Decompile(_method);
                var il = BuildIL(_method);
                
                // Update UI on correct thread
                System.Windows.Application.Current.Dispatcher.Invoke(() => 
                {
                    CSCode = cs;
                    ILCode = il;
                });
            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => 
                {
                    CSCode = $"// Error loading code: {ex.Message}";
                    ILCode = $"// Error loading IL: {ex.Message}";
                });
            }
        });
    }

    private string BuildCSharpDeclaration(MethodBase method)
    {
        // Legacy: replaced by DecompilerService
        return "Loading...";
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
    
    private void RebuildParameters()
    {
        Parameters.Clear();
        
        try
        {
            // Use ResolvedMethod for generic methods to get concrete parameter types
            // If resolution fails (e.g. invalid metadata), fallback to _method
            var methodToInspect = IsGenericMethod ? ResolvedMethod : _method;
            
            foreach (var p in methodToInspect.GetParameters())
            {
                Parameters.Add(CreateParameterVM(p));
            }
        }
        catch (Exception)
        {
            // Swallow error if GetParameters fails on corrupt method
        }
    }

    private ParameterViewModel CreateParameterVM(ParameterInfo p)
    {
        try
        {
            var paramType = p.ParameterType;
            
            // Wrap checks to avoid crash on bad types
            if (paramType == typeof(int)) return new IntParameterViewModel(p);
            if (paramType == typeof(bool)) return new BoolParameterViewModel(p);
            
            bool isEnum = false;
            try { isEnum = paramType.IsEnum; } catch {}
            if (isEnum) return new EnumParameterViewModel(p);

            if (IsCollectionType(paramType)) return new CollectionParameterViewModel(p);
            
            // IsComplexType can throw on Type Load Error
            bool isComplex = false;
            try { isComplex = IsComplexType(paramType); } catch {}
            
            if (isComplex) return new JsonParameterViewModel(p);
            
            // Default to String for everything else
            return new StringParameterViewModel(p);
        }
        catch (Exception)
        {
            // Fallback for types that fail to load (mismatched metadata)
            return new StringParameterViewModel(p);
        }
    }
    
    private static bool IsCollectionType(Type type)
    {
        try
        {
            if (type.IsArray) return true;
            if (type.IsGenericType)
            {
                var genericDef = type.GetGenericTypeDefinition();
                return genericDef == typeof(System.Collections.Generic.List<>) ||
                       genericDef == typeof(System.Collections.Generic.IList<>) ||
                       genericDef == typeof(System.Collections.Generic.IEnumerable<>);
            }
        }
        catch { }
        return false;
    }
    
    private static bool IsComplexType(Type type)
    {
        try
        {
            // Exclude system types that cause serialization issues
            if (typeof(Delegate).IsAssignableFrom(type)) return false;
            if (typeof(MemberInfo).IsAssignableFrom(type)) return false;
            if (typeof(Type).IsAssignableFrom(type)) return false;
            
            // Complex = class that's not string, and not a collection
            return type.IsClass && type != typeof(string) && !IsCollectionType(type);
        }
        catch { return false; }
    }
}
