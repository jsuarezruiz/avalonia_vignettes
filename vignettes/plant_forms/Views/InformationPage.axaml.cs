using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using PlantForms.Controls;
using PlantForms.Models;

namespace PlantForms.Views;

/// <summary>
/// The second page: who is buying and where it is going. Port of
/// <c>plant_form_information.dart</c>.
/// </summary>
/// <remarks>
/// The country decides which fields follow it and in what order, so they are built rather than
/// declared. Changing it forgets every field's validity, as the original does, because the fields
/// themselves have changed.
/// </remarks>
public partial class InformationPage : FormPage
{
    private readonly FormProgress _progress = new();

    public InformationPage()
    {
        InitializeComponent();

        Country.Options = CountryData.GetCountries();

        AddHandler(FormField.ValidatedEvent, OnFieldValidated);

        Continue.Bind(SubmitButton.CompletionProperty, Bound(nameof(FormProgress.Completion)));
        Continue.Bind(SubmitButton.IsErrorVisibleProperty, Bound(nameof(FormProgress.IsErrorVisible)));
    }

    /// <summary>
    /// Control themes resolve by exact type, so the page borrows its base's.
    /// </summary>
    protected override Type StyleKeyOverride => typeof(FormPage);

    private OrderForm Order => DataContext as OrderForm ?? new OrderForm();

    private Binding Bound(string property) => new(property) { Source = _progress };

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        // The third country is the one the original opens on.
        if (!Order.Contains(FormKeys.Country))
        {
            Order[FormKeys.Country] = CountryData.GetCountries()[2];
        }

        Country.Value = Order[FormKeys.Country];

        BuildCountryFields();
    }

    private static string TitleFor(string key) => string.Join(
        ' ',
        key.Split('_').Select(word => string.Concat(word[..1].ToUpper(CultureInfo.InvariantCulture), word[1..])));

    private void OnFieldValidated(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not FormField field || field.FieldKey.Length == 0)
        {
            return;
        }

        var hasChanged = Order[field.FieldKey] != field.Value;

        Order[field.FieldKey] = field.Value;
        _progress.Set(field.FieldKey, field.IsValid);

        if (field == Country && hasChanged)
        {
            _progress.Clear();

            BuildCountryFields();
        }
    }

    private void BuildCountryFields()
    {
        var country = Order[FormKeys.Country];
        var postalTitle = country == "United States" ? "Zip Code" : "Postal Code";

        CountryFields.Children.Clear();

        switch (country)
        {
            case "United States":
            case "Canada":
                AddText(FormKeys.FirstName);
                AddText(FormKeys.LastName, required: true);
                AddText(FormKeys.Address, required: true);
                AddText(FormKeys.Apt, "Apartment, suite, etc.");
                AddText(FormKeys.City, required: true);
                AddSubdivision(country);
                AddText(FormKeys.Postal, postalTitle, required: true);
                break;

            case "Japan":
                AddText(FormKeys.Company);
                AddText(FormKeys.LastName, required: true);
                AddText(FormKeys.FirstName);
                AddText(FormKeys.Postal, postalTitle, required: true);
                AddSubdivision(country);
                AddText(FormKeys.City, required: true);
                AddText(FormKeys.Address, required: true);
                AddText(FormKeys.Apt, "Apartment, suite, etc.");
                break;

            case "France":
                AddText(FormKeys.FirstName);
                AddText(FormKeys.LastName, required: true);
                AddText(FormKeys.Company);
                AddText(FormKeys.Address, required: true);
                AddText(FormKeys.Apt, "Apartment, suite, etc.");
                AddText(FormKeys.Postal, postalTitle, required: true);
                AddText(FormKeys.City, required: true);
                break;
        }
    }

    private void AddText(string key, string? title = null, bool required = false)
    {
        _progress.Register(key, required);

        CountryFields.Children.Add(new FormField
        {
            FieldKey = key,
            Helper = title ?? TitleFor(key),
            IsRequired = required,
            Value = Order[key],
        });
    }

    private void AddSubdivision(string country)
    {
        var title = CountryData.GetSubdivisionTitle(country);

        if (title.Length == 0)
        {
            return;
        }

        var options = CountryData.GetSubdivisions(title);

        if (!Order.Contains(title))
        {
            Order[title] = options[0];
        }

        _progress.Register(title, isRequired: false);

        CountryFields.Children.Add(new DropDownField
        {
            FieldKey = title,
            Label = title,
            Options = options,
            Value = Order[title],
        });
    }

    private void OnContinueClick(object? sender, RoutedEventArgs e)
    {
        if (_progress.IsComplete)
        {
            RequestNext();
        }
        else
        {
            _progress.IsErrorVisible = true;
        }
    }
}
