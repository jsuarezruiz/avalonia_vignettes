using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace DarkInkTransition.Views;

/// <summary>
/// The article, in one of the two colour schemes. Port of <c>dark_ink_content.dart</c>.
/// </summary>
/// <remarks>
/// A whole copy of the page exists per scheme rather than the colours being animated: the reveal
/// paints one over the other through the ink, so both have to be on screen at once and each has to
/// be complete.
/// </remarks>
public partial class ArticlePage : UserControl
{
    /// <summary>
    /// Defines the <see cref="IsDark"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDarkProperty =
        AvaloniaProperty.Register<ArticlePage, bool>(nameof(IsDark));

    private static readonly IBrush LightPage = new ImmutableSolidColorBrush(Colors.White);
    private static readonly IBrush DarkPage = new ImmutableSolidColorBrush(Color.FromRgb(0x31, 0x34, 0x66));
    private static readonly IBrush LightText = new ImmutableSolidColorBrush(Color.FromRgb(0x21, 0x0A, 0x3B));
    private static readonly IBrush DarkText = new ImmutableSolidColorBrush(Color.FromRgb(0xBC, 0xFE, 0xEA));
    private static readonly IBrush LightSubHeader = new ImmutableSolidColorBrush(Color.FromRgb(0x00, 0x8F, 0x9C));
    private static readonly IBrush DarkSubHeader = new ImmutableSolidColorBrush(Color.FromRgb(0x00, 0xEB, 0xAC));
    private static readonly IBrush LightRule = new ImmutableSolidColorBrush(Color.FromArgb(0x51, 0x2B, 0x77, 0x7E));
    private static readonly IBrush DarkRule = new ImmutableSolidColorBrush(Color.FromArgb(0x51, 0x00, 0x98, 0xA3));

    /// <summary>
    /// Defines the <see cref="PageBrush"/> property.
    /// </summary>
    public static readonly DirectProperty<ArticlePage, IBrush> PageBrushProperty =
        AvaloniaProperty.RegisterDirect<ArticlePage, IBrush>(nameof(PageBrush), o => o.PageBrush);

    /// <summary>
    /// Defines the <see cref="TextBrush"/> property.
    /// </summary>
    public static readonly DirectProperty<ArticlePage, IBrush> TextBrushProperty =
        AvaloniaProperty.RegisterDirect<ArticlePage, IBrush>(nameof(TextBrush), o => o.TextBrush);

    /// <summary>
    /// Defines the <see cref="SubHeaderBrush"/> property.
    /// </summary>
    public static readonly DirectProperty<ArticlePage, IBrush> SubHeaderBrushProperty =
        AvaloniaProperty.RegisterDirect<ArticlePage, IBrush>(nameof(SubHeaderBrush), o => o.SubHeaderBrush);

    /// <summary>
    /// Defines the <see cref="RuleBrush"/> property.
    /// </summary>
    public static readonly DirectProperty<ArticlePage, IBrush> RuleBrushProperty =
        AvaloniaProperty.RegisterDirect<ArticlePage, IBrush>(nameof(RuleBrush), o => o.RuleBrush);

    private IBrush _pageBrush = LightPage;
    private IBrush _textBrush = LightText;
    private IBrush _subHeaderBrush = LightSubHeader;
    private IBrush _ruleBrush = LightRule;

    static ArticlePage() =>
        IsDarkProperty.Changed.AddClassHandler<ArticlePage>((x, _) => x.Refresh());

    /// <summary>
    /// Initializes a new instance of the <see cref="ArticlePage"/> class.
    /// </summary>
    public ArticlePage()
    {
        InitializeComponent();
        DataContext = this;
    }

    /// <summary>
    /// Repaints the page for the current scheme. The brushes are raised rather than computed on
    /// read, because the scheme is set after construction, by which time the bindings have already
    /// been evaluated once and will not look again unless told to.
    /// </summary>
    private void Refresh()
    {
        PageBrush = IsDark ? DarkPage : LightPage;
        TextBrush = IsDark ? DarkText : LightText;
        SubHeaderBrush = IsDark ? DarkSubHeader : LightSubHeader;
        RuleBrush = IsDark ? DarkRule : LightRule;
    }

    /// <summary>
    /// Gets or sets a value indicating whether this copy is the dark one.
    /// </summary>
    public bool IsDark
    {
        get => GetValue(IsDarkProperty);
        set => SetValue(IsDarkProperty, value);
    }

    /// <summary>
    /// Gets the headline.
    /// </summary>
    public string Header => "The Private History of a Campaign That Failed";

    /// <summary>
    /// Gets the article text.
    /// </summary>
    public string Body => ArticleText.Body;

    /// <summary>
    /// Gets the page's background.
    /// </summary>
    public IBrush PageBrush
    {
        get => _pageBrush;
        private set => SetAndRaise(PageBrushProperty, ref _pageBrush, value);
    }

    /// <summary>
    /// Gets the brush the headline and body are set in.
    /// </summary>
    public IBrush TextBrush
    {
        get => _textBrush;
        private set => SetAndRaise(TextBrushProperty, ref _textBrush, value);
    }

    /// <summary>
    /// Gets the brush the reading time is set in.
    /// </summary>
    public IBrush SubHeaderBrush
    {
        get => _subHeaderBrush;
        private set => SetAndRaise(SubHeaderBrushProperty, ref _subHeaderBrush, value);
    }

    /// <summary>
    /// Gets the brush of the rule under the heading.
    /// </summary>
    public IBrush RuleBrush
    {
        get => _ruleBrush;
        private set => SetAndRaise(RuleBrushProperty, ref _ruleBrush, value);
    }

    /// <summary>
    /// Gets the scroller, so both copies can be kept at the same offset.
    /// </summary>
    public ScrollViewer Scroll => Scroller;
}
