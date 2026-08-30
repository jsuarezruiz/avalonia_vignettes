using System.ComponentModel;
using Avalonia.Threading;
using Avalonia.Interactivity;
using PlantForms.Controls;
using PlantForms.Models;

namespace PlantForms.Views;

/// <summary>
/// The last page: how it is being paid for. Port of <c>plant_form_payment.dart</c>.
/// </summary>
public partial class PaymentPage : FormPage
{
    private static readonly TimeSpan FillDelay = TimeSpan.FromMilliseconds(500);

    private readonly FormProgress _progress = new();
    private readonly DispatcherTimer _fill;

    public PaymentPage()
    {
        InitializeComponent();

        AddHandler(FormField.ValidatedEvent, OnFieldValidated);

        _fill = new DispatcherTimer { Interval = FillDelay };
        _fill.Tick += OnFillTick;

        Purchase.IsErrorVisible = _progress.IsErrorVisible;
        _progress.PropertyChanged += OnProgressPropertyChanged;
    }

    /// <summary>
    /// Control themes resolve by exact type, so the page borrows its base's.
    /// </summary>
    protected override Type StyleKeyOverride => typeof(FormPage);

    private OrderForm Order => DataContext as OrderForm ?? new OrderForm();

    private void OnProgressPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is null or nameof(FormProgress.IsErrorVisible))
        {
            Purchase.IsErrorVisible = _progress.IsErrorVisible;
        }
    }

    private void OnFieldValidated(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not FormField field || field.FieldKey.Length == 0)
        {
            return;
        }

        Order[field.FieldKey] = field.Value;
        _progress.Set(field.FieldKey, field.IsValid);

        // The original lets the button catch up half a second later.
        _fill.Stop();
        _fill.Start();
    }

    private void OnFillTick(object? sender, EventArgs e)
    {
        _fill.Stop();

        Purchase.Completion = _progress.Completion;
    }

    private void OnPurchaseClick(object? sender, RoutedEventArgs e)
    {
        if (!_progress.IsComplete)
        {
            _progress.IsErrorVisible = true;
        }
    }
}
