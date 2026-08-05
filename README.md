# Avalonia Vignettes

Avalonia ports of [gskinner's Flutter Vignettes](https://github.com/gskinnerTeam/flutter_vignettes), a collection of explorations into what a UI framework can be pushed to do.

Each port is written from the original Dart source, keeping its numbers and its easing curves, and expressing them with Avalonia idioms: custom controls where a control is warranted, resource dictionaries for brushes and metrics, control themes for templates, styles for typography.

## The vignettes

<a href="vignettes/basketball_ptr"><img src="vignettes/basketball_ptr/images/basketball_ptr.gif" width="237"/></a>

### [Sports App Pull To Refresh](vignettes/basketball_ptr)

A custom pull to refresh: a basketball spins around the hoop while the scores reload.

<a href="vignettes/bubble_tab_bar"><img src="vignettes/bubble_tab_bar/images/bubble_tab_bar.gif" width="237"/></a>

### [Icon Flip Button Bar](vignettes/bubble_tab_bar)

A navigation bar whose buttons change size, shape and colour as they are picked.

## Running

Needs the .NET 10 SDK. Each vignette is its own app:

```
dotnet run --project vignettes/basketball_ptr/BasketballPullToRefresh.csproj
```

[`vignettes/_shared`](vignettes/_shared) holds what they have in common: Flutter's animation controller, ticker and curves, a `PageView`, `Hero` flights, perspective transforms.

## Licence

MIT, and so are the originals. See [LICENSE](LICENSE).
