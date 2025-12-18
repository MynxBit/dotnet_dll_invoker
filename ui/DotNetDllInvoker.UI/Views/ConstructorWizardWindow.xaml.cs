using System.Windows;
using DotNetDllInvoker.UI.ViewModels;

namespace DotNetDllInvoker.UI.Views;

/// <summary>
/// Constructor Wizard dialog for creating instances with parameters.
/// </summary>
public partial class ConstructorWizardWindow : Window
{
    public ConstructorWizardWindow()
    {
        InitializeComponent();
    }

    public ConstructorWizardWindow(ConstructorViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    /// <summary>
    /// The created instance (set when user clicks Create).
    /// </summary>
    public object? CreatedInstance { get; private set; }

    private void OnCreate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is ConstructorViewModel vm)
            {
                CreatedInstance = vm.CreateInstance();
                DialogResult = true;
                Close();
            }
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Failed to create instance:\n{ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
