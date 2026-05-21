# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Super Mario World recreation in Unity 6 (6000.3.15f1). Mechanics-first approach — procedural/placeholder visuals, gameplay accuracy over polish. See SPEC.md for authoritative gameplay data (movement tables, jump physics, P-meter values) and PHASES.md for the implementation roadmap. Phase 1 (Player Movement & Camera) is complete.

## Architecture

### Bootstrap & Scene Flow

The game uses additive scene loading with a persistent Systems scene:

1. **Bootstrapper** (`[RuntimeInitializeOnLoadMethod]`) loads Systems.unity additively before any scene runs
2. **Systems.unity** holds singleton services as scene components (no DontDestroyOnLoad — lifetimes managed manually)
3. Scene-specific controllers (TitleScene, OverworldController, LevelRoot) check `GameStateMachine.Instance` on Awake/Start
4. Never press Play on Systems.unity directly — use Title, Overworld, or a Level scene

### Singletons

Services use a simple singleton pattern: `Instance` set in Awake, cleared in OnDestroy, no DontDestroyOnLoad. Access via `X.Instance`:

- **GameStateMachine** — owns game-wide state (`GameState` enum: None, Title, Overworld, Level) and scene loading. Transition methods (`TransitionToTitle`, `TransitionToOverworld`, `TransitionToLevel`) set state, switch input maps via `InputMapNames`, and load the target scene via SceneLoader.
- **AudioBus** — audio playback across 5 channels (music, SFX, jingles, ambient, UI) via AudioCatalog. Music channel supports push/pop stack for temporary music (e.g. star invincibility).
- **SceneLoader** — async scene transitions with ScreenFader; tracks previous/current scene for unload
- **ScreenFader** — fade in/out transitions via PrimeTween

### Player System

- **PlayerController**: Dynamic Rigidbody2D with `gravityScale = 0` — all physics applied manually in FixedUpdate. Implements SNES-accurate movement: walk/run/sprint speed tiers, P-meter (0–112, fills while Y/X + direction + grounded), speed oscillation patterns at run/sprint thresholds, variable-height jump with velocity that scales by X-speed, slope sliding, and no coyote time.
- **GroundProbe**: OverlapBox collision sampling for ground/ceiling/wall detection; infers ground normal via raycast for slope angle
- **PlayerInputBinding**: Adapts New Input System actions into intent fields (Move, Jump, SpinJump, Action, Pause, CameraNudgeLeft/Right); supports test override mode via `DebugOverride()`; hosts `SwitchMapOnAllPlayers` static helper
- **PlayerCarry**: Phase 1 stub — `IsCarrying` always false, all methods are no-ops. Full carry/throw wired in Phase 2+.

Unit conversion: 1 SNES subpixel/frame ≈ `6/21` Unity velocity (units/sec). Walk 21 subpx/f → 6, Run 37 → 10.57, Sprint 49 → 14.

### Level Loading

GameStateMachine.TransitionToLevel() loads level scenes via SceneLoader. LevelRoot spawns the player via LevelContext, resolving spawn position from SpawnMarker components or LevelData coordinates. LevelCamera is a separate GameObject in the level scene, wired to the player via `LevelContext.Begin()`.

### Camera

**LevelCamera** follows the player with:
- Forward bias in travel direction with smooth easing
- Direction reversal hold — camera freezes until player reaches ~65% screen width, then fast linear transition
- Two vertical modes (`VerticalScrollMode` enum): `ScrollAtWill` (tracks Y smoothly) or `LockUnlessTriggered` (only updates Y when grounded outside lock window)
- L/R camera nudge (Q/E or triggers), disableable via `SetNudgeEnabled()` for auto-scroll/boss rooms
- Hard clamp to LevelBounds rect

### Data

- **LevelData** (ScriptableObject): level metadata, scene reference (Eflatun.SceneReference), entry points, unlock chains
- **AudioCatalog** (ScriptableObject): maps SfxId/MusicId/JingleId/UiSfxId enums to AudioClips
- **HudViewModel**: event-driven viewmodel for HUD updates (lives, coins, score, timer, dragon coins, power state)

### Environment

- **GroundPlatform**: configurable `length` field auto-scales transform and BoxCollider2D
- **Slope**: 4 orientations via `SlopeKind` (SteepL/R 45°, ShallowL/R ~26.5°), auto-generates PolygonCollider2D vertices

## Code Conventions

- **No namespace** — all scripts live in the global namespace
- **No assembly definitions** — Unity's default compilation pipeline
- **Sealed classes** by default — no inheritance hierarchies, prefer composition
- **Private fields**: `_camelCase` with underscore prefix (`_rb`, `_probe`, `_input`)
- **Scenes are hand-authored** in the Unity Editor — do not write scripts that compose or save scenes to disk. Prefab and ScriptableObject generation is fine.

## Key Packages

- **Eflatun.SceneReference**: type-safe scene references by GUID (used in LevelData, GameStateMachine)
- **PrimeTween**: tweening/animation (used for screen fades, UI transitions)
- **Unity.InputSystem**: New Input System with InputSystem_Actions.inputactions asset (Player, Overworld, UI maps)
- **Unity.VectorGraphics**: SVG/vector rendering for procedural sprites
- **URP** (Universal Render Pipeline): active render pipeline

## Project Layout

```
Assets/_Project/
  Scripts/
    Runtime/           — all gameplay code
      Audio/           — AudioBus, AudioCatalog, AudioIds
      Environment/     — GroundPlatform, Slope, SlopeKind
      Player/          — PlayerController, GroundProbe, PlayerInputBinding, PlayerCarry
      Scene/           — LevelRoot, LevelContext, LevelCamera, LevelBounds, SpawnMarker,
                         SceneLoader, ScreenFader, TitleScene, OverworldController
      (root)           — Bootstrapper, GameStateMachine, InputMapNames, LevelData, HudViewModel
    Editor/            — FileExtensions (project panel helper)
  _Scenes/             — Systems, Title, Overworld, GameSession, Debug
  _Prefabs/            — Player/, Environment/ (Ground_Platform, Slope variants)
  Data/                — AudioCatalog.asset, Levels/LevelData_Debug.asset
  Audio/               — SFX/ (Jump, SpinJump, Skid, Land .wav), Music/ (Overworld .ogg + .mid)
  Art/Sprites/         — placeholder sprite textures
  Player/              — SmallMario sprite sheet (.svg, .png, generator .py)
  Input/               — InputSystem_Actions.inputactions
```
