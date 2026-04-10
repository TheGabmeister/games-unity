# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Unity 6 (`6000.3.12f1`) project for recreating Super Mario World. Uses URP 2D and the `Universal Render Pipeline`. The project is in an early/scaffolding state — most game systems do not exist yet, so when adding code, follow the conventions below rather than copying from a non-existent example.

## How to build / run

There is no command-line build script. Open the project in Unity Hub (`6000.3.12f1` exact) and run from the editor. The single playable scene is `Assets/_Project/Scenes/Main.unity`. There is no test framework set up yet (no `.asmdef` files exist), even though `com.unity.test-framework` is in the manifest — adding tests will require creating assembly definitions first.

The Visual Studio solution `SuperMarioWorld.slnx` only references `Assembly-CSharp-Editor.csproj` because all current code lives in an `Editor/` folder. Runtime `.cs` files placed under `Assets/` will automatically appear in `Assembly-CSharp.csproj` once Unity regenerates project files.

## Repository layout convention

All game-specific assets and scripts go under [Assets/_Project/](Assets/_Project/) (the leading underscore keeps it sorted to the top of the Project window, separate from third-party packages). The current subfolders are:

- [Assets/_Project/Scripts/](Assets/_Project/Scripts/) — gameplay code. Currently only contains `Editor/`.
- [Assets/_Project/Scenes/](Assets/_Project/Scenes/) — scenes (`Main.unity`).
- [Assets/_Project/Settings/](Assets/_Project/Settings/) — URP render pipeline + 2D renderer + global volume profile assets. Modify these rather than creating parallel copies.
- [Assets/_Project/Input/](Assets/_Project/Input/) — `InputSystem_Actions.inputactions` (uses the new Input System package, not the legacy `Input` API).

When adding new content, mirror this structure (e.g. `Assets/_Project/Prefabs/`, `Assets/_Project/Art/`) — do not put files at the `Assets/` root.

## Key packages already wired up

- **`com.unity.inputsystem` 1.19.0** — new Input System. Use `InputAction`/`PlayerInput` flows; do not call `Input.GetKey`.
- **`com.unity.render-pipelines.universal` 17.3.0** with the **2D Renderer** — sprite lit/unlit shaders, 2D lights. Don't add 3D-only URP features.
- **`com.unity.feature.2d`** — 2D feature set (Tilemap, Sprite Shape, 2D Animation, PSD Importer, Aseprite Importer, Pixel Perfect).
- **`com.kyrylokuzyk.primetween` 1.3.8** — tween library. Prefer this over coroutines/`DOTween` for animations.
- **`com.eflatun.scenereference`** (git package) — type-safe scene references in serialized fields. Use `SceneReference` instead of raw scene name strings.

## Existing code

- [Assets/_Project/Scripts/Editor/FileExtensions.cs](Assets/_Project/Scripts/Editor/FileExtensions.cs) — `[InitializeOnLoad]` editor utility that draws file extensions next to asset names in the Project window. Pure editor tooling; not part of any game system. Sourced from a public gist (link in the file header).
