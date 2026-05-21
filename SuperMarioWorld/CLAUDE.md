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
- **PlayerInputBinding** — lives in the Systems scene (not on the player prefab). Reads the `InputActionAsset` directly (no Unity `PlayerInput` component). Exposes intent properties (Move, JumpHeld, ActionPressedThisFrame, etc.) that gameplay code polls. `SwitchActionMap()` toggles between Player/Overworld/UI maps. **Input latching**: button presses are latched in `Update()` and persist until `ConsumeFixedUpdate()` is called at the end of `FixedUpdate()`, ensuring presses are never missed across the Update/FixedUpdate boundary.
- **AudioBus** — audio playback across 5 channels (music, SFX, jingles, ambient, UI) via AudioCatalog. Music channel supports push/pop stack for temporary music (e.g. star invincibility).
- **SceneLoader** — async scene transitions with ScreenFader; tracks previous/current scene for unload
- **ScreenFader** — fade in/out transitions via PrimeTween

### Level Loading

**LevelManager** is the per-level-scene entry point. On Start it finds the spawn point via `GameObject.FindGameObjectWithTag("PlayerStart")`, instantiates the player prefab and camera prefab, wires `LevelCamera.Target` to the player, and passes `LevelData.VerticalCameraMode` to the camera.

### Player System

**Coordinate system**: 1 Unity unit = 64 pixels = 1024 subpixels. All SNES speed values from the spec are in subpixels/frame and are used directly as integers — no floating-point conversion in the core loop.

- **PlayerController** (`[RequireComponent: Rigidbody2D, BoxCollider2D, GroundProbe]`): Kinematic Rigidbody2D with manual integration at 60 Hz. Maintains integer subpixel accumulators (`long _subX`, `long _subY`) and velocity (`int _velX`, `int _velY`). Converts to Unity units only at the `MovePosition` boundary. Movement mode enum (Ground, Air, Crouch, Slide) with modifier flags — not a state-pattern. SNES Y-axis is inverted: positive `_velY` = falling, applied as `_subY -= _velY`.
- **GroundProbe**: OverlapBox collision sampling for ground/ceiling/wall detection on Solid (layer 13) and OneWay (layer 14) layers; infers ground normal via raycast for slope angle. Must call `Sample()` each physics tick. Ceiling checks exclude OneWay.
- **PlayerCarry**: stub — `IsCarrying` always false, all methods are no-ops. Full carry/throw wired in a later phase.

### Camera

**LevelCamera** runs in LateUpdate with three systems:
- **Horizontal**: off-center lead (35% of half-screen in travel direction), dead zone (10%), reversal at 65% threshold, smooth recentering
- **Vertical**: two modes via `VerticalCameraMode` enum on LevelData — `ScrollAtWill` (smooth follow) or `NoScrollUnlessTriggered` (only scrolls when landing at different elevation)
- **Nudge**: L/R input offsets camera horizontally (max ±1.5 units)
- **Bounds**: clamps to LevelBounds if present in scene

### Data

- **LevelData** (ScriptableObject): level metadata, scene reference (Eflatun.SceneReference), entry points, sub-areas, music, unlock chains, vertical camera mode
- **AudioCatalog** (ScriptableObject): maps SfxId/MusicId/JingleId/UiSfxId enums to AudioClips
- **HudViewModel**: event-driven viewmodel for HUD updates (lives, coins, score, timer, dragon coins, power state)

## Physics Constants

FixedUpdate runs at **60 Hz** (0.01666667s timestep, matching SNES NTSC). Key values from SPEC.md used directly in PlayerController:

- Walk max: 21, Run max: 37, Sprint max: 49 (subpx/frame)
- Gravity held: +3, Gravity released: +6 (subpx/frame²)
- Terminal velocity: 64 (subpx/frame)
- P-Meter max: 112
- Jump Y-speed lookup table: 7 entries indexed by `abs(xSpeed) / 8`

## Layers

| Layer | Name | Purpose |
|-------|------|---------|
| 8 | Player | Player character |
| 9 | PlayerInvulnerable | Post-damage i-frames |
| 10 | PlayerProjectile | Fireballs |
| 11 | Enemy | Enemy characters |
| 12 | EnemyProjectile | Enemy fireballs, etc. |
| 13 | Solid | Ground, walls, ceilings |
| 14 | OneWay | One-way platforms (solid on top, pass-through from below) |
| 15 | Hazard | Lava, spikes |
| 16 | Pickup | Coins, power-ups |
| 17 | Interactive | Pipes, doors, switches |
| 18 | LevelBounds | Camera clamp region |

## Code Conventions

- **No namespace** — all scripts live in the global namespace
- **No assembly definitions** — Unity's default compilation pipeline
- **Private fields**: `_camelCase` with underscore prefix (`_rb`, `_probe`, `_input`)
- **Single-player only** — no PlayerInputManager, no multi-player join logic

## Key Packages

- **Eflatun.SceneReference**: type-safe scene references by GUID (used in LevelData, GameStateMachine)
- **PrimeTween**: tweening/animation (used for screen fades, UI transitions)
- **Unity.InputSystem**: New Input System with InputSystem_Actions.inputactions asset (Player, Overworld, UI maps). Used directly via `InputActionAsset` — no `PlayerInput` component.
- **URP** (Universal Render Pipeline): active render pipeline
