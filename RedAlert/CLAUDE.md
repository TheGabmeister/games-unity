# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

RedAlert is a Unity 6 (6000.3.12f1) 2D game project using Universal Render Pipeline with the 2D Renderer. It uses the new Input System and PrimeTween for animation. The project is in early stage — infrastructure is in place but no gameplay systems exist yet.

## Build & Run

This is a Unity project — there is no CLI build. Open in Unity 6 (6000.3.12f1+) and use the Editor to build/run. Test framework (`com.unity.test-framework`) is installed but no tests exist yet.

## Architecture

### Bootstrapper Pattern
`Assets/_Project/Scripts/Editor/Bootstrapper.cs` uses `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` to instantiate a `Resources/Systems` prefab before any scene loads. All global managers/systems should live as components on this prefab rather than as scene-dependent singletons.

### Scene Structure
Two scenes are configured in build settings:
- **Init** (index 0) — `Assets/_Project/Scenes/Init.unity`
- **Gameplay** (index 1) — `Assets/_Project/Scenes/Gameplay.unity`

### Input
`Assets/_Project/Input/InputSystem_Actions.inputactions` defines a Player action map with: Move, Look, Attack, Interact (hold), Crouch, Jump, Previous, Next, Sprint. Bindings exist for keyboard+mouse, gamepad, touch, and XR.

### Project Layout
All game content goes under `Assets/_Project/`. Editor-only scripts are in `Assets/_Project/Scripts/Editor/`. URP and rendering settings are in `Assets/_Project/Settings/`.

## Conventions

- Scripts use the global namespace (no C# namespaces).
- Private fields use underscore prefix (`_health`, `_rb`). Locals stay plain. Constants and static readonly use PascalCase.
- No custom assembly definitions — everything compiles into the default assemblies.
- **KISS** — simplest thing that works
- **YAGNI** — don't build for hypothetical needs
- **DRY** — remove real duplication, not shape-similar code
- **Locality of change** — adding a new entity or feature should touch as few files as possible

## Reference Source Code

The original C&C: Red Alert source code (EA GPL release) is at `D:\CnC_Red_Alert\CODE\`. Key files for gameplay data:
- `BDATA.CPP` — building definitions, footprint sizes, occupy/overlap lists
- `UDATA.CPP` — vehicle unit definitions, locomotion types
- `IDATA.CPP` — infantry unit definitions
- `VDATA.CPP` — naval vessel definitions
- `AADATA.CPP` — aircraft definitions
- `BULLET.CPP` — projectile behavior, homing, scatter, arcs
- `COMBAT.CPP` — splash damage, damage calculation, friendly fire
- `TECHNO.CPP` — targeting, Can_Fire logic, turret rotation
- `UNIT.CPP` — fire-while-moving, harvester AI, unit state machine
- `RULES.H` / `RULES.CPP` — all configurable game rules (ore values, growth rates, build speed, etc.)
- `DEFINES.H` — enums: BSizeType, SpeedType, ArmorType, WarheadType

## Asset Pipeline

- **Sprites**: SVG source files exported to PNG via Inkscape. Sprite size is 64×64 px. Sprite sheets are horizontal strips (e.g., 512×64 for 8 frames).
  - Inkscape path: `"/c/Program Files/Inkscape/bin/inkscape.exe"`
  - Batch export: `bash Tools/export_sprites.sh` (re-exports all SVGs)
  - Python: C:/Users/Admin/AppData/Local/Python/pythoncore-3.14-64/python.exe
- **Sounds**: generate with rfxgen (`"D:/rfxgen_v5.0_win_x64/rfxgen.exe" -g coin -o sound.wav`)
- **Music**: Python scripts in `Tools/music/` use `midiutil` to generate MIDI → FluidSynth renders with a soundfont to WAV → ffmpeg converts to OGG. Tool paths: `D:/fluidsynth-v2.5.4-win10-x64-cpp11/bin/fluidsynth.exe`, `D:/ffmpeg-8.1-essentials_build/bin/ffmpeg.exe`, soundfont `D:/GeneralUser-GS/GeneralUser-GS.sf2`
