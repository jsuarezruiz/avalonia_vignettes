# Reproducing the dog artwork

`DogView` draws native Avalonia vector geometry exported from gskinner's original
`DogAnimation.flr`. It retains the original paths, deformed bones, fills, strokes,
clipping, draw order, and artboard padding, sampled at 60 Hz. The walk is 65 poses
(1.08333 seconds), and the non-looping front sit is 19 poses (0.3 seconds).
The Slider and chase physics still run natively; Flutter is not a runtime dependency.

Source: [flutter_vignettes](https://github.com/gskinnerTeam/flutter_vignettes),
commit `055406a71912d7a7d89420359988963e0b39ed98`.
Original file: `vignettes/dog_slider/lib/assets/DogAnimation.flr`.
SHA-256: `c6c48c4c1bb6cd48473bc2f735edfc124cb4420d44eb1260d468c041f8b93fd2`.
Artwork license: `../DogSlider/Assets/Animation/LICENSE.gskinner`.
The rounded-path conversion derives from Flare's MIT-licensed implementation;
its notice is retained in `LICENSE.flare`.

From the original Flutter `vignettes/dog_slider` project:

```sh
flutter pub get
flutter test /absolute/path/to/avalonia_vignettes/vignettes/dog_slider/tools/export_dog_test.dart \
  --dart-define=DOG_OUTPUT=/absolute/output/dog-poses.json.gz
```

Copy the generated `.json.gz` to `DogSlider/Assets/Animation/`. The two sibling
PNGs are reference poses rendered by Flare itself, for comparison only.
The exporter deliberately fails on unsupported paints, clipping operations,
blend modes, or trimmed strokes instead of silently approximating an updated asset.

Validated using Flutter 3.44.8 / Dart 3.12.2 and `flare_flutter` 3.0.2. That legacy
package needs one compatibility change on current Flutter: replace
`hashValues(bundle, name)` with `Object.hash(bundle, name)` in
`lib/provider/asset_flare.dart`. Apply that only in an isolated local copy and
reference it through `dependency_overrides`; it does not change animation or drawing.
