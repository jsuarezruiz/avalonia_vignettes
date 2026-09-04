#!/usr/bin/env python3
"""Generate desktop PNG, Windows ICO, and macOS ICNS vignette icons."""

from pathlib import Path

try:
    from PIL import Image, ImageDraw, ImageFilter
except ImportError as error:
    raise SystemExit(
        "Pillow is required. Install it with: python3 -m pip install Pillow"
    ) from error


REPOSITORY_ROOT = Path(__file__).resolve().parent.parent
VIGNETTES_ROOT = REPOSITORY_ROOT / "vignettes"
SOURCE_GLOB = "*/*.iOS/Assets.xcassets/AppIcon.appiconset/Icon-1024.png"
ICON_SIZE = 1024
ICON_INSET = 72
CORNER_RADIUS = 192
ICO_SIZES = [(16, 16), (24, 24), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)]
ICNS_SIZES = [(16, 16), (32, 32), (64, 64), (128, 128), (256, 256), (512, 512), (1024, 1024)]


def make_desktop_icon(source: Path) -> Image.Image:
    source_image = Image.open(source).convert("RGBA")
    icon_extent = ICON_SIZE - ICON_INSET * 2
    source_image = source_image.resize((icon_extent, icon_extent), Image.Resampling.LANCZOS)

    icon_mask = Image.new("L", (icon_extent, icon_extent), 0)
    draw = ImageDraw.Draw(icon_mask)
    draw.rounded_rectangle(
        (0, 0, icon_extent - 1, icon_extent - 1),
        radius=CORNER_RADIUS,
        fill=255,
    )

    shadow_source = Image.new("L", (ICON_SIZE, ICON_SIZE), 0)
    shadow_source.paste(icon_mask, (ICON_INSET, ICON_INSET))
    shadow_mask = shadow_source.filter(ImageFilter.GaussianBlur(24))
    shadow = Image.new("RGBA", (ICON_SIZE, ICON_SIZE), (0, 0, 0, 0))
    shadow.putalpha(shadow_mask.point(lambda alpha: alpha * 42 // 255))

    canvas = Image.new("RGBA", (ICON_SIZE, ICON_SIZE), (0, 0, 0, 0))
    canvas.alpha_composite(shadow, (0, 14))
    canvas.paste(source_image, (ICON_INSET, ICON_INSET), icon_mask)
    return canvas


def main() -> None:
    sources = sorted(VIGNETTES_ROOT.glob(SOURCE_GLOB))
    if not sources:
        raise SystemExit("No iOS AppIcon sources found.")

    for source in sources:
        ios_project = source.parents[2]
        assembly_name = ios_project.name.removesuffix(".iOS")
        desktop_project = ios_project.parent / f"{assembly_name}.Desktop"
        if not desktop_project.is_dir():
            raise SystemExit(f"Desktop project not found for {assembly_name}")

        destination = desktop_project / "Assets"
        destination.mkdir(exist_ok=True)
        icon = make_desktop_icon(source)
        icon.save(destination / "AppIcon.png", optimize=True)
        icon.save(destination / "AppIcon.ico", sizes=ICO_SIZES)
        icon.save(destination / "AppIcon.icns", sizes=ICNS_SIZES)
        print(f"Generated {desktop_project.relative_to(REPOSITORY_ROOT)}")

    print(f"Generated desktop icons for {len(sources)} vignettes.")


if __name__ == "__main__":
    main()
