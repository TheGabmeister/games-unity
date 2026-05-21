# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Super Mario World recreation in Unity 6 (6000.3.15f1). Single-player only. Mechanics-first approach — procedural/placeholder visuals, gameplay accuracy over polish. See SPEC.md for authoritative gameplay data (movement tables, jump physics, P-meter values) and PHASES.md for the implementation roadmap.

## Architecture

### Bootstrap & Scene Flow

The game uses additive scene loading with a persistent Systems scene:

1. **Bootstrapper** (`[RuntimeInitializeOnLoadMethod]`) loads Systems.unity additively before any scene runs
2. **Systems.unity** holds singleton services as scene components (no DontDestroyOnLoad — lifetimes managed manually)
3. Scene-specific controllers (TitleScene, OverworldController, LevelManager) check `GameStateMachine.Instance` on Start
4. Never press Play on Systems.unity directly — use Title, Overworld, or a Level scene

### Singletons

Services use a simple singleton pattern: `Instance` set in Awake, cleared in OnDestroy, no DontDestroyOnLoad. Access via `X.Instance`:

- **GameStateMachine** — owns game-wide state (`GameState` enum: None, Title, Overworld, Level) and scene transitions. Transition methods set state, switch input maps via `PlayerInputBinding.Instance.SwitchActionMap()`, and load the target scene via SceneLoader.
- **PlayerInputBinding** — lives in the Systems scene (not on the player prefab). Reads the `InputActionAsset` directly (no Unity `PlayerInput` component). Exposes intent properties (Move, JumpHeld, ActionPressedThisFrame, etc.) that gameplay code polls. `SwitchActionMap()` toggles between Player/Overworld/UI maps. Supports `DebugOverride()` for test injection.
- **AudioBus** — audio playback across 5 channels (music, SFX, jingles, ambient, UI) via AudioCatalog. Music channel supports push/pop stack for temporary music (e.g. star invincibility).
- **SceneLoader** — async scene transitions with ScreenFader; tracks previous/current scene for unload
- **ScreenFader** — fade in/out transitions via PrimeTween

### Level Loading

**LevelManager** is the per-level-scene entry point. On Start it finds the spawn point via `GameObject.FindGameObjectWithTag("PlayerStart")`, instantiates the player prefab and camera prefab, and wires `LevelCamera.Target` to the player.

### Player System

- **PlayerController** (`[RequireComponent: Rigidbody2D, BoxCollider2D, GroundProbe]`): currently a stub being rebuilt. Reads input from `PlayerInputBinding.Instance`.
- **GroundProbe**: OverlapBox collision sampling for ground/ceiling/wall detection; infers ground normal via raycast for slope angle. Must call `Sample()` each physics tick.
- **PlayerCarry**: stub — `IsCarrying` always false, all methods are no-ops. Full carry/throw wired in a later phase.

### Camera

**LevelCamera** follows its `Target` transform with a configurable Z offset.

### Data

- **LevelData** (ScriptableObject): level metadata, scene reference (Eflatun.SceneReference), entry points, sub-areas, music, unlock chains
- **AudioCatalog** (ScriptableObject): maps SfxId/MusicId/JingleId/UiSfxId enums to AudioClips
- **HudViewModel**: event-driven viewmodel for HUD updates (lives, coins, score, timer, dragon coins, power state)

## Code Conventions

- **No namespace** — all scripts live in the global namespace
- **No assembly definitions** — Unity's default compilation pipeline
- **Private fields**: `_camelCase` with underscore prefix (`_rb`, `_probe`, `_input`)
- **Scenes are hand-authored** in the Unity Editor — do not write scripts that compose or save scenes to disk. Prefab and ScriptableObject generation is fine.
- **Single-player only** — no PlayerInputManager, no multi-player join logic

## Key Packages

- **Eflatun.SceneReference**: type-safe scene references by GUID (used in LevelData, GameStateMachine)
- **PrimeTween**: tweening/animation (used for screen fades, UI transitions)
- **Unity.InputSystem**: New Input System with InputSystem_Actions.inputactions asset (Player, Overworld, UI maps). Used directly via `InputActionAsset` — no `PlayerInput` component.
- **URP** (Universal Render Pipeline): active render pipeline
