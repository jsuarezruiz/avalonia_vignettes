using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using ParticleSwipe.Models;

namespace ParticleSwipe.Controls;

/// <summary>
/// The contents of one inbox row: sender, time, subject and a two line preview. Port of
/// <c>message_card.dart</c>.
/// </summary>
public sealed class EmailCard : TemplatedControl
{
    public static readonly StyledProperty<Email?> EmailProperty =
        AvaloniaProperty.Register<EmailCard, Email?>(nameof(Email));

    public static readonly StyledProperty<bool> IsAlternateProperty =
        AvaloniaProperty.Register<EmailCard, bool>(nameof(IsAlternate));

    static EmailCard()
    {
        EmailProperty.Changed.AddClassHandler<EmailCard>((x, e) => x.OnEmailChanged(e));
        IsAlternateProperty.Changed.AddClassHandler<EmailCard>((x, e) =>
            x.PseudoClasses.Set(":alternate", e.GetNewValue<bool>()));
    }

    /// <summary>
    /// Gets or sets the message being shown.
    /// </summary>
    public Email? Email
    {
        get => GetValue(EmailProperty);
        set => SetValue(EmailProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether this row takes the darker of the two fills.
    /// </summary>
    public bool IsAlternate
    {
        get => GetValue(IsAlternateProperty);
        set => SetValue(IsAlternateProperty, value);
    }

    private void OnEmailChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is Email old)
        {
            old.PropertyChanged -= OnEmailPropertyChanged;
        }

        if (e.NewValue is Email added)
        {
            added.PropertyChanged += OnEmailPropertyChanged;
        }

        UpdateStateClasses();
    }

    private void OnEmailPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) =>
        UpdateStateClasses();

    private void UpdateStateClasses()
    {
        PseudoClasses.Set(":read", Email?.IsRead == true);
        PseudoClasses.Set(":starred", Email?.IsFavorite == true);
    }
}
