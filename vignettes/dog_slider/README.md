# Dog Toy Slider

An Avalonia port of [dog_slider](https://github.com/gskinnerTeam/flutter_vignettes/tree/master/vignettes/dog_slider).

<p align="center"><img src="images/dog_slider.gif" width="320"/></p>

Drag the ball along the track and the dog gives chase, then folds into a sit once it arrives.

## Running

```
dotnet run --project DogSlider.Desktop/DogSlider.Desktop.csproj
```

The original plays the dog as a Flare document. Avalonia has no runtime for that format, so the character is rebuilt from vector shapes and posed from code.
