using System.Windows;
using DotNetDllInvoker.UI.ViewModels;

namespace DotNetDllInvoker.UI.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnDllDragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            // Only accept .dll files
            if (files.Length > 0 && files[0].EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
                return;
            }
        }
        e.Effects = DragDropEffects.None;
        e.Handled = true;
    }

    private void OnDllDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            if (files.Length > 0 && files[0].EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                // Forward to ViewModel
                if (DataContext is MainViewModel vm)
                {
                    vm.LoadDllFromPath(files[0]);
                }
            }
        }
    }
}
