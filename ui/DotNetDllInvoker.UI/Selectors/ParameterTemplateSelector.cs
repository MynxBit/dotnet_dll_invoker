using System.Windows;
using System.Windows.Controls;
using DotNetDllInvoker.UI.ViewModels;

namespace DotNetDllInvoker.UI.Selectors;

public class ParameterTemplateSelector : DataTemplateSelector
{
    public DataTemplate? StringTemplate { get; set; }
    public DataTemplate? IntTemplate { get; set; }
    public DataTemplate? BoolTemplate { get; set; }
    public DataTemplate? EnumTemplate { get; set; }
    public DataTemplate? JsonTemplate { get; set; }
    public DataTemplate? CollectionTemplate { get; set; }
    public DataTemplate? GenericTypeTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        return item switch
        {
            StringParameterViewModel => StringTemplate,
            IntParameterViewModel => IntTemplate,
            BoolParameterViewModel => BoolTemplate,
            EnumParameterViewModel => EnumTemplate,
            JsonParameterViewModel => JsonTemplate,
            CollectionParameterViewModel => CollectionTemplate,
            GenericTypeArgumentViewModel => GenericTypeTemplate,
            _ => base.SelectTemplate(item, container)
        };
    }
}
