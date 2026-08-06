using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ParticleSwipe.Models;

/// <summary>
/// One message in the inbox. Port of <c>Email</c> in <c>demo_data.dart</c>.
/// </summary>
public sealed class Email : INotifyPropertyChanged
{
    private bool _isFavorite;

    /// <summary>
    /// Initializes a new message.
    /// </summary>
    public Email(string from, string subject, string body, bool isRead = false, bool isFavorite = false)
    {
        From = from;
        Subject = subject;
        Body = body;
        IsRead = isRead;
        _isFavorite = isFavorite;
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets who the message is from.
    /// </summary>
    public string From { get; }

    /// <summary>
    /// Gets the subject line.
    /// </summary>
    public string Subject { get; }

    /// <summary>
    /// Gets the preview text.
    /// </summary>
    public string Body { get; }

    /// <summary>
    /// Gets a value indicating whether the message has been read.
    /// </summary>
    public bool IsRead { get; }

    /// <summary>
    /// Gets a value indicating whether the message is starred.
    /// </summary>
    public bool IsFavorite
    {
        get => _isFavorite;
        private set
        {
            if (_isFavorite == value)
            {
                return;
            }

            _isFavorite = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Stars the message, or removes the star if it already has one.
    /// </summary>
    public void ToggleFavorite() => IsFavorite = !IsFavorite;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
