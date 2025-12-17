using System.Windows;
using System.Windows.Controls;
using DotNetDllInvoker.UI.ViewModels;

namespace DotNetDllInvoker.UI.Selectors;

public class ParameterTemplateSelector : DataTemplateSelector
{
    public DataTemplate? StringTemplate { get; set; }
    public DataTemplate? IntTemplate { get; set; }
    public DataTemplate? BoolTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is StringParameterViewModel) return StringTemplate;
        if (item is IntParameterViewModel) return IntTemplate;
        if (item is BoolParameterViewModel) return BoolTemplate;

        return base.SelectTemplate(item, container);
    }
}
