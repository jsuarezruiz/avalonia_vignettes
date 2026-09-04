# Dog Toy Slider

An Avalonia port of [dog_slider](https://github.com/gskinnerTeam/flutter_vignettes/tree/master/vignettes/dog_slider).

<p align="center"><img src="images/dog_slider.gif" width="320"/></p>

Drag the ball along the track and the dog gives chase, then folds into a sit once it arrives.

## Running

```
dotnet run --project DogSlider.Desktop/DogSlider.Desktop.csproj
```

The control derives from Avalonia's `Slider`: its native `Track` and `Thumb` own mouse, touch, keyboard, capture, and value changes. The artwork adds the ball hop and dog chase on top.

The original Flare dog's paths and poses are drawn as native Avalonia vector geometry, including its walk and front-facing sit. See [the reproducible exporter](tools/README.md) for source provenance and timing. No Flutter or Flare runtime is required by the Avalonia application.
