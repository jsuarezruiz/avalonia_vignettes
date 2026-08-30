using Avalonia.Controls;
using Avalonia.Interactivity;
using PlantForms.Controls;
using PlantForms.Models;

namespace PlantForms.Views;

/// <summary>
/// The store's checkout: a scene at the top with the form's pages stacked over it. Port of
/// <c>demo.dart</c>.
/// </summary>
public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        // Every page writes into the one order, which they pick up from the tree.
        DataContext = new OrderForm();

        AddHandler(FormPage.BackRequestedEvent, (_, _) => Stack.Pop());
        AddHandler(FormPage.NextRequestedEvent, (_, _) => Stack.Push());
        AddHandler(DropDownField.OpenRequestedEvent, OnOpenOptions);
    }

    private void OnOpenOptions(object? sender, RoutedEventArgs e)
    {
        if (e.Source is DropDownField field)
        {
            Options.Show(field);
        }
    }
}
