#!/bin/bash
# Export all SVGs to PNGs for Unity
# Usage: bash Tools/export_sprites.sh

INKSCAPE="/c/Program Files/Inkscape/bin/inkscape.exe"
TOOLS_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_DIR="$(dirname "$TOOLS_DIR")"
SPRITE_DIR="$PROJECT_DIR/Assets/_Project/Sprites"

mkdir -p "$SPRITE_DIR/Terrain"
mkdir -p "$SPRITE_DIR/Units"
mkdir -p "$SPRITE_DIR/UI"

echo "Exporting terrain tiles..."
for svg in "$TOOLS_DIR/sprites/terrain/"*.svg; do
    name=$(basename "$svg" .svg)
    "$INKSCAPE" "$svg" --export-type=png --export-filename="$SPRITE_DIR/Terrain/$name.png" --export-width=64 --export-height=64 2>/dev/null
    echo "  $name"
done

echo "Exporting unit sprites..."
for svg in "$TOOLS_DIR/sprites/units/"*.svg; do
    name=$(basename "$svg" .svg)
    "$INKSCAPE" "$svg" --export-type=png --export-filename="$SPRITE_DIR/Units/$name.png" --export-width=64 --export-height=64 2>/dev/null
    echo "  $name"
done

echo "Exporting UI sprites..."
for svg in "$TOOLS_DIR/sprites/ui/"*.svg; do
    name=$(basename "$svg" .svg)
    "$INKSCAPE" "$svg" --export-type=png --export-filename="$SPRITE_DIR/UI/$name.png" --export-width=64 --export-height=64 2>/dev/null
    echo "  $name"
done

echo "Done!"
