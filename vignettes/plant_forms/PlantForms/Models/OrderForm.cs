using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PlantForms.Models;

/// <summary>
/// The values every page of the form writes into. Port of <c>SharedFormState</c>, which the original
/// hands down the tree with a provider.
/// </summary>
public sealed class OrderForm : INotifyPropertyChanged
{
    private readonly Dictionary<string, string> _values = [];

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the note left with the order.
    /// </summary>
    public string Instructions
    {
        get => this[FormKeys.Instructions];
        set => this[FormKeys.Instructions] = value;
    }

    /// <summary>
    /// Gets the address the order is confirmed to.
    /// </summary>
    public string Email => this[FormKeys.Email];

    /// <summary>
    /// Gets the shipping address as the payment page prints it.
    /// </summary>
    public string ShippingAddress
    {
        get
        {
            var apt = this[FormKeys.Apt] is { Length: > 0 } value ? $"#{value} " : string.Empty;
            var subdivision = this[CountryData.GetSubdivisionTitle(this[FormKeys.Country])];

            return $"{apt}{this[FormKeys.Address]}\n" +
                $"{this[FormKeys.City]}, {subdivision} {this[FormKeys.Postal].ToUpperInvariant()}\n" +
                $"{this[FormKeys.Country].ToUpperInvariant()}";
        }
    }

    /// <summary>
    /// Gets or sets the value stored under <paramref name="key"/>, empty if unset.
    /// </summary>
    public string this[string key]
    {
        get => _values.TryGetValue(key, out var value) ? value : string.Empty;
        set
        {
            if (this[key] == value)
            {
                return;
            }

            _values[key] = value;

            OnPropertyChanged(nameof(Email));
            OnPropertyChanged(nameof(Instructions));
            OnPropertyChanged(nameof(ShippingAddress));
        }
    }

    /// <summary>
    /// Gets whether a value has been stored under <paramref name="key"/>.
    /// </summary>
    public bool Contains(string key) => _values.ContainsKey(key);

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
