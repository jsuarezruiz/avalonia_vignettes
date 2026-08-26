using Avalonia;

namespace AvaloniaVignettes.Tests;

public sealed class DogSliderTests
{
    [Fact]
    public void ArcRadiusReportsAccurateComputedPropertyChanges()
    {
        var slider = new DogSlider.Controls.DogSlider();
        var oldSize = slider.BallSize;
        var oldLeft = slider.BallLeft;
        AvaloniaPropertyChangedEventArgs? sizeChange = null;
        AvaloniaPropertyChangedEventArgs? leftChange = null;

        slider.PropertyChanged += (_, change) =>
        {
            if (change.Property == DogSlider.Controls.DogSlider.BallSizeProperty)
            {
                sizeChange = change;
            }
            else if (change.Property == DogSlider.Controls.DogSlider.BallLeftProperty)
            {
                leftChange = change;
            }
        };

        slider.ArcRadius = 20d;

        Assert.NotNull(sizeChange);
        Assert.Equal(oldSize, sizeChange.GetOldValue<double>());
        Assert.Equal(slider.BallSize, sizeChange.GetNewValue<double>());

        Assert.NotNull(leftChange);
        Assert.Equal(oldLeft, leftChange.GetOldValue<double>());
        Assert.Equal(slider.BallLeft, leftChange.GetNewValue<double>());
    }

    [Fact]
    public void ValueChangeReportsAccurateBallPositionChange()
    {
        var slider = new DogSlider.Controls.DogSlider();
        slider.Measure(new Size(300d, 100d));
        slider.Arrange(new Rect(0d, 0d, 300d, 100d));

        var oldLeft = slider.BallLeft;
        AvaloniaPropertyChangedEventArgs? leftChange = null;

        slider.PropertyChanged += (_, change) =>
        {
            if (change.Property == DogSlider.Controls.DogSlider.BallLeftProperty)
            {
                leftChange = change;
            }
        };

        slider.Value = 0.5d;

        Assert.NotNull(leftChange);
        Assert.Equal(oldLeft, leftChange.GetOldValue<double>());
        Assert.Equal(slider.BallLeft, leftChange.GetNewValue<double>());
    }
}
