using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using BasketballView = BasketballPullToRefresh.Views.MainView;
using BubbleTabView = BubbleTabBar.Views.MainView;
using ConstellationsView = ConstellationsList.Views.MainView;
using DarkInkView = DarkInkTransition.Views.MainView;
using DogSliderView = DogSlider.Views.MainView;
using DrinkRewardsView = DrinkRewardsList.Views.MainView;
using FluidNavView = FluidNavBar.Views.MainView;
using GooeyEdgeView = GooeyEdge.Views.MainView;
using Indie3DView = Indie3D.Views.MainView;
using ParticleSwipeView = ParticleSwipe.Views.MainView;
using PlantFormsView = PlantForms.Views.MainView;
using ProductZoomView = ProductDetailZoom.Views.MainView;
using SparklePartyView = SparkleParty.Views.MainView;
using SpendingTrackerView = SpendingTracker.Views.MainView;
using TicketFoldView = TicketFold.Views.MainView;
using TravelHeroView = ParallaxTravelCardsHero.Views.MainView;
using TravelListView = ParallaxTravelCardsList.Views.MainView;

namespace AvaloniaVignettes.Showcase.Models;

public static class VignetteCatalog
{
    public static IReadOnlyList<VignetteSample> All { get; } =
    [
        Sample<TravelListView>(
            "travel-cards", "01", "Travel Cards", "PARALLAX",
            "Layered destination cards with depth, drag and three-dimensional tilt.",
            "Drag the cards vertically and watch the artwork move at different speeds.",
            "parallax_travel_cards_list", "ParallaxTravelCardsList", ThemeVariant.Light,
            ["Styles/Resources.axaml", "Themes/TravelCard.axaml", "Themes/HotelList.axaml"],
            ["Styles/TextStyles.axaml"]),

        Sample<GooeyEdgeView>(
            "gooey-edge", "02", "Mindfulness", "GESTURE",
            "A calm carousel revealed through a liquid edge that follows your pointer.",
            "Swipe horizontally across the screen to pull the next page into view.",
            "gooey_edge", "GooeyEdge", ThemeVariant.Light,
            ["Styles/Resources.axaml", "Themes/ContentCard.axaml"]),

        Sample<TicketFoldView>(
            "ticket-fold", "03", "Boarding Passes", "3D MOTION",
            "Boarding passes unfold one panel at a time like a paper concertina.",
            "Tap a boarding pass to unfold it; tap again to close it.",
            "ticket_fold", "TicketFold", ThemeVariant.Light,
            ["Styles/Resources.axaml", "Themes/FlightSummary.axaml", "Themes/FlightDetails.axaml", "Themes/FlightBarcode.axaml", "Themes/Ticket.axaml"]),

        Sample<ParticleSwipeView>(
            "particle-swipe", "04", "Inbox Swipe", "PARTICLES",
            "Swipe actions turn an ordinary inbox into expressive particle bursts.",
            "Swipe a row right to star it or left to delete it.",
            "particle_swipe", "ParticleSwipe", ThemeVariant.Dark,
            ["Styles/Resources.axaml", "Themes/EmailCard.axaml", "Themes/SwipeItem.axaml"]),

        Sample<BubbleTabView>(
            "bubble-tab-bar", "05", "Icon Flip Bar", "NAVIGATION",
            "A playful navigation bar whose buttons reshape as they are selected.",
            "Pick different icons in the bottom bar and scroll the live pages.",
            "bubble_tab_bar", "BubbleTabBar", ThemeVariant.Light,
            ["Styles/Resources.axaml", "Themes/NavBar.axaml"]),

        Sample<DrinkRewardsView>(
            "drink-rewards", "06", "Liquid Rewards", "PHYSICS",
            "Reward cards spring open and fill with liquid that sloshes as it settles.",
            "Tap a drink card to open it and collect its reward.",
            "drink_rewards_list", "DrinkRewardsList", ThemeVariant.Dark,
            ["Styles/Resources.axaml", "Themes/DrinkCard.axaml"]),

        Sample<DogSliderView>(
            "dog-slider", "07", "Dog Toy Slider", "CHARACTER",
            "A characterful slider where the dog chases a ball and folds into a sit.",
            "Drag the tennis ball along the track and let the dog catch up.",
            "dog_slider", "DogSlider", ThemeVariant.Light,
            ["Styles/Resources.axaml", "Themes/DogSlider.axaml"]),

        Sample<ConstellationsView>(
            "constellations", "08", "Guide to the Stars", "PARTICLES",
            "A reactive starfield accelerates behind a guide to the constellations.",
            "Scroll the list, then select a constellation to open its story.",
            "constellations_list", "ConstellationsList", ThemeVariant.Dark,
            ["Styles/Resources.axaml", "Themes/ConstellationTitleCard.axaml", "Themes/DetailPage.axaml"]),

        Sample<TravelHeroView>(
            "travel-hero", "09", "Paris Travel Hero", "HERO",
            "A destination card expands into a layered Paris scene and hotel guide.",
            "Tap the Paris card to launch the shared-element transition.",
            "parallax_travel_cards_hero", "ParallaxTravelCardsHero", ThemeVariant.Light,
            ["Styles/Resources.axaml", "Themes/CityScenery.axaml"],
            ["Styles/TextStyles.axaml"]),

        Sample<FluidNavView>(
            "fluid-nav", "10", "Fluid Button Bar", "FLUID",
            "The navigation surface dips and sloshes beneath the selected action.",
            "Choose any bottom navigation item to move the fluid notch.",
            "fluid_nav_bar", "FluidNavBar", ThemeVariant.Light,
            ["Styles/Resources.axaml", "Themes/FluidNavBar.axaml"]),

        Sample<ProductZoomView>(
            "product-zoom", "11", "Product Zoom", "TRANSITION",
            "A product spins into a cinematic detail view with animated callouts.",
            "Tap the zoom control on the speaker to reveal its details.",
            "product_detail_zoom", "ProductDetailZoom", ThemeVariant.Dark,
            ["Styles/Resources.axaml", "Themes/Controls.axaml"],
            ["Styles/TextStyles.axaml"]),

        Sample<DarkInkView>(
            "dark-ink", "12", "Article Dark Mode", "MASKING",
            "Ink spreads across an article to carry it between light and dark modes.",
            "Tap the mode control and watch the ink consume the page.",
            "dark_ink_transition", "DarkInkTransition", ThemeVariant.Light,
            ["Styles/Resources.axaml", "Themes/Controls.axaml"],
            ["Styles/TextStyles.axaml"]),

        Sample<BasketballView>(
            "basketball-refresh", "13", "Pull to Refresh", "GESTURE",
            "A basketball circles the hoop while the sports scores reload.",
            "Pull the scoreboard down far enough to trigger a refresh.",
            "basketball_ptr", "BasketballPullToRefresh", ThemeVariant.Light,
            ["Styles/Resources.axaml", "Themes/GameScoreBoard.axaml"],
            ["Styles/TextStyles.axaml"]),

        Sample<PlantFormsView>(
            "plant-forms", "14", "Plant Checkout", "FORMS",
            "A polished checkout flow with stacked pages, validation and progress.",
            "Complete each form to advance through the live checkout.",
            "plant_forms", "PlantForms", ThemeVariant.Light,
            ["Styles/Resources.axaml", "Themes/Controls.axaml"],
            ["Styles/TextStyles.axaml"]),

        Sample<Indie3DView>(
            "indie-3d", "15", "Feature Artists", "3D MOTION",
            "A bold artist carousel surrounded by drifting three-dimensional shapes.",
            "Swipe horizontally between artists to move through the scene.",
            "indie_3d", "Indie3D", ThemeVariant.Light,
            ["Styles/Resources.axaml"],
            ["Styles/TextStyles.axaml"]),

        Sample<SparklePartyView>(
            "sparkle-party", "16", "Sparkle Party", "20K PARTICLES",
            "A high-performance particle playground rendered in four presets.",
            "Move the pointer over the scene and switch effects at the bottom.",
            "sparkle_party", "SparkleParty", ThemeVariant.Light,
            ["Styles/Resources.axaml"],
            ["Styles/TextStyles.axaml"]),

        Sample<SpendingTrackerView>(
            "spending-tracker", "17", "Budget Tracker", "DATA VIZ",
            "A tactile spending tracker with draggable charts and rolling totals.",
            "Drag the chart horizontally and release to snap to another month.",
            "spending_tracker", "SpendingTracker", ThemeVariant.Light,
            ["Styles/Resources.axaml", "Themes/Controls.axaml"],
            ["Styles/TextStyles.axaml"]),
    ];

    private static VignetteSample Sample<TView>(
        string id,
        string number,
        string title,
        string category,
        string description,
        string instruction,
        string preview,
        string assembly,
        ThemeVariant theme,
        IReadOnlyList<string> resources,
        IReadOnlyList<string>? styles = null)
        where TView : Control, new() =>
        new(
            id,
            number,
            title,
            category,
            description,
            instruction,
            LoadPreview(preview),
            new Uri($"https://github.com/jsuarezruiz/avalonia_vignettes/tree/main/vignettes/{preview}"),
            theme,
            static () => new TView(),
            resources.Select(path => Uri(assembly, path)).ToArray(),
            (styles ?? []).Select(path => Uri(assembly, path)).ToArray());

    private static Uri Uri(string assembly, string path) =>
        new($"avares://{assembly}/{path}");

    private static Bitmap LoadPreview(string name) =>
        new(AssetLoader.Open(new Uri($"avares://AvaloniaVignettes.Showcase/Assets/Previews/{name}.jpg")));
}
