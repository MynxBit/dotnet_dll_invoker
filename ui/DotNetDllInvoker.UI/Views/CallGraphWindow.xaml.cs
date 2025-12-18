using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DotNetDllInvoker.UI.ViewModels;

namespace DotNetDllInvoker.UI.Views;

/// <summary>
/// Call Graph visualization window.
/// </summary>
public partial class CallGraphWindow : Window
{
    public CallGraphWindow()
    {
        InitializeComponent();
    }

    public CallGraphWindow(CallGraphViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    /// <summary>
    /// Creates and shows a call graph for an assembly.
    /// </summary>
    public static void ShowForAssembly(Assembly assembly)
    {
        var vm = new CallGraphViewModel();
        var window = new CallGraphWindow(vm);
        
        window.Show();
        vm.LoadFromAssembly(assembly);
    }

    /// <summary>
    /// Creates and shows a call graph for a specific method.
    /// </summary>
    public static void ShowForMethod(MethodBase method, int depth = 3)
    {
        var vm = new CallGraphViewModel();
        var window = new CallGraphWindow(vm);
        
        window.Show();
        vm.LoadFromMethod(method, depth);
    }

    private void OnNodeClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && 
            element.DataContext is CallGraphNodeViewModel nodeVm &&
            DataContext is CallGraphViewModel graphVm)
        {
            // Deselect previous
            if (graphVm.SelectedNode != null)
            {
                graphVm.SelectedNode.IsSelected = false;
            }
            
            // Select new
            nodeVm.IsSelected = true;
            graphVm.SelectedNode = nodeVm;
        }
    }

    private async void OnNodeInvokeClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && 
            button.Tag is CallGraphNodeViewModel nodeVm &&
            DataContext is CallGraphViewModel graphVm)
        {
            try
            {
                // Get the method from the node
                var method = nodeVm.GetMethod();
                if (method == null)
                {
                    MessageBox.Show("Cannot invoke this method - no method info available", "Invoke Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Build parameter info for confirmation
                var parameters = method.GetParameters();
                var paramInfo = parameters.Length == 0 
                    ? "No parameters required" 
                    : string.Join("\n", parameters.Select(p => $"  - {p.Name}: {p.ParameterType.Name} (auto-generated)"));

                // Show confirmation dialog
                var result = MessageBox.Show(
                    $"Invoke method: {method.DeclaringType?.Name}.{method.Name}\n\n" +
                    $"Parameters:\n{paramInfo}\n\n" +
                    "Auto-generated values will be used.\nProceed with invocation?",
                    "Confirm Invocation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return;

                // Invoke using dispatcher
                var dispatcher = new DotNetDllInvoker.Core.CommandDispatcher();
                
                // Auto-generate parameters
                var args = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    args[i] = dispatcher.GenerateAutoParameter(parameters[i].ParameterType);
                }

                // Invoke
                var invokeResult = await dispatcher.InvokeMethod(method, args);
                
                // Show result
                if (invokeResult.IsSuccess)
                {
                    MessageBox.Show($"✅ Invocation Successful!\n\nResult: {invokeResult.ReturnValue ?? "(void)"}\n\nTime: {invokeResult.Duration.TotalMilliseconds:F2}ms",
                        "Invocation Result", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"❌ Invocation Failed!\n\nError: {invokeResult.Error?.Message ?? "Unknown error"}",
                        "Invocation Result", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Invoke Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        // Stop event from bubbling to node click
        e.Handled = true;
    }
}


