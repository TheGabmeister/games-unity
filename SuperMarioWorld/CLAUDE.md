# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Super Mario World recreation in Unity 6 (6000.3.15f1). Mechanics-first approach — procedural/placeholder visuals, gameplay accuracy over polish. See SPEC.md for authoritative gameplay data and PHASES.md for the implementation roadmap.

## Architecture

### Bootstrap & Scene Flow

The game uses additive scene loading with a persistent Systems scene:

1. **Bootstrapper** (`[RuntimeInitializeOnLoadMethod]`) loads Systems.unity additively before any scene runs
2. **Systems.unity** holds the GameServices singleton (never unloaded)
3. Scene-specific roots (LevelRoot, TitleRoot, OverworldRoot) bind to GameServices on Start
4. Never press Play on Systems.unity directly — use Boot, Title, Overworld, or a Level scene

### GameServices Singleton

Central hub on Systems.unity. All subsystems are SerializeField references wired in the inspector. Access via static properties: `GameServices.Save`, `GameServices.SceneLoader`, `GameServices.Fader`, `GameServices.GameState`, `GameServices.Session`, `GameServices.Audio`, etc.

### State Machine

`GameStateMachine` manages game-wide state via `IGameState` implementations: TitleState, OverworldState, LevelState, PausedState, GameOverState. States drive scene loading and input map switching.

### Player System

- **PlayerController**: Dynamic Rigidbody2D with `gravityScale = 0` — all physics (gravity, acceleration, jump) applied manually in FixedUpdate. Subpixel-precision movement matching SNES behavior.
- **GroundProbe**: OverlapBox collision sampling for ground/ceiling/wall detection
- **PlayerInputBinding**: Adapts New Input System actions into intent fields; supports test override mode
- **PlayerCarry**: Carry/throw mechanics

### Level Loading

SceneLoader handles async scene transitions with ScreenFader. LevelRoot spawns the player via LevelContext, resolving spawn position from SpawnMarker components or LevelData coordinates. LevelCamera follows the player with configurable horizontal/vertical scroll modes.

### Data

- **LevelData** (ScriptableObject): level metadata, scene reference (Eflatun.SceneReference), entry points, unlock chains
- **SaveData**: serialized via Newtonsoft.Json — 3 save slots with lives, score, coins, level completions, switch palace states
- **AudioCatalog** (ScriptableObject): maps SfxId/MusicId/JingleId enums to AudioClips

## Code Conventions

- **No namespace** — all scripts live in the global namespace
- **No assembly definitions** — Unity's default compilation pipeline
- **Sealed classes** by default — no inheritance hierarchies, prefer composition
- **Private fields**: `_camelCase` with underscore prefix (`_rb`, `_probe`, `_input`)
- **Scenes are hand-authored** in the Unity Editor — do not write scripts that compose or save scenes to disk. Prefab and ScriptableObject generation is fine.

## Key Packages

- **Eflatun.SceneReference**: type-safe scene references by GUID (used in LevelData)
- **PrimeTween**: tweening/animation (used for screen fades, UI transitions)
- **Unity.InputSystem**: New Input System with InputSystem_Actions.inputactions asset
- **Unity.VectorGraphics**: SVG/vector rendering for procedural sprites
- **Newtonsoft.Json**: save serialization (precompiled reference)

## Project Layout

```
Assets/_Project/
  Scripts/Runtime/   — all gameplay code, organized by system folder
  Scripts/Editor/    — editor-only tools
  Scenes/            — Boot, Systems, Title, Overworld, Debug/
  Prefabs/           — Player/, Environment/
  Data/              — ScriptableObject assets (LevelData, AudioCatalog)
  Art/Sprites/       — sprite assets
  Input/             — InputSystem_Actions.inputactions
```
