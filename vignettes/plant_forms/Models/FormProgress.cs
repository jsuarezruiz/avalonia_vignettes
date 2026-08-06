using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PlantForms.Models;

/// <summary>
/// How much of one page's form is filled in, which its submit button fills up with. Port of
/// <c>FormMixin</c>.
/// </summary>
public sealed class FormProgress : INotifyPropertyChanged
{
    private readonly Dictionary<string, bool> _valid = [];

    private double _completion;
    private bool _isErrorVisible;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets the share of the page's fields that are valid, from 0 to 1.
    /// </summary>
    public double Completion
    {
        get => _completion;
        private set
        {
            if (_completion.Equals(value))
            {
                return;
            }

            _completion = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets whether to say that the form is unfinished.
    /// </summary>
    public bool IsErrorVisible
    {
        get => _isErrorVisible;
        set
        {
            if (_isErrorVisible == value)
            {
                return;
            }

            _isErrorVisible = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets whether every field on the page is valid.
    /// </summary>
    public bool IsComplete => Completion >= 1d;

    /// <summary>
    /// Records a field, which starts out valid only if it is optional.
    /// </summary>
    public void Register(string key, bool isRequired)
    {
        if (!_valid.ContainsKey(key))
        {
            Set(key, !isRequired);
        }
    }

    /// <summary>
    /// Records whether a field is valid, and works the page's completion out again.
    /// </summary>
    public void Set(string key, bool isValid)
    {
        _valid[key] = isValid;

        Completion = _valid.Count == 0 ? 0d : (double)_valid.Count(x => x.Value) / _valid.Count;

        if (IsComplete)
        {
            IsErrorVisible = false;
        }
    }

    /// <summary>
    /// Forgets every field, as the original does when the country changes.
    /// </summary>
    public void Clear()
    {
        _valid.Clear();

        Completion = 0d;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        if (propertyName == nameof(Completion))
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsComplete)));
        }
    }
}
