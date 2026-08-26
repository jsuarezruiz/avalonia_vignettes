using PlantForms.Models;

namespace AvaloniaVignettes.Tests;

public sealed class FormProgressTests
{
    [Fact]
    public void RequiredFieldsDriveCompletionAndErrorState()
    {
        var progress = new FormProgress();

        progress.Register("name", isRequired: true);
        progress.Register("notes", isRequired: false);

        Assert.Equal(0.5d, progress.Completion);
        Assert.False(progress.IsComplete);

        progress.IsErrorVisible = true;
        progress.Set("name", isValid: true);

        Assert.Equal(1d, progress.Completion);
        Assert.True(progress.IsComplete);
        Assert.False(progress.IsErrorVisible);
    }

    [Fact]
    public void ClearForgetsRegisteredValidity()
    {
        var progress = new FormProgress();
        progress.Register("name", isRequired: true);
        progress.Set("name", isValid: true);

        progress.Clear();

        Assert.Equal(0d, progress.Completion);
        Assert.False(progress.IsComplete);
    }
}
