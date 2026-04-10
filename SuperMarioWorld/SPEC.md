# Super Mario World Recreation — SPEC

## 1. Project Overview

Recreate the **gameplay** of *Super Mario World* (SNES, 1990) in Unity 6 (URP 2D). The goal is mechanical fidelity, not visual fidelity. All sprites, textures, and audio are replaced with placeholders so the project can be built end-to-end without external assets, while remaining structured to drop those assets in later.

### Goals
- Faithful recreation of SMW's core platformer feel: physics, jumping, power-ups, enemies, blocks, level flow.
- A complete vertical slice: overworld map → enter level → play level → exit at goal → return to map.
- Architecture clean enough that adding the missing presentation layer (sprites, animations, audio) is purely additive — no rewrites.

### Non-Goals
- Pixel-perfect visuals or 1:1 SNES-accurate frame counts. We approximate physics constants and tune for feel.
- Full level content. We ship a small, hand-authored set of test levels that exercise every system; we are not recreating all 96 SMW levels.
- Multiplayer / Luigi co-op.
- Save battery / SRAM accuracy. We use Unity's `JsonUtility` save files.

## 2. Tech Stack & Constraints

| Area | Choice |
|---|---|
| Engine | Unity `6000.3.12f1` |
| Render pipeline | URP 17.3 with the **2D Renderer** |
| Physics | Built-in **Physics2D**. Player and enemies use dynamic Rigidbody2D with `gravityScale = 0` and a manual velocity controller (see §4.2). Tilemap solids use `CompositeCollider2D`. |
| Input | `com.unity.inputsystem` 1.19 (no legacy `Input.*`) |
| Tweens | **PrimeTween** (`com.kyrylokuzyk.primetween`) — preferred over coroutines for animation |
| Scene refs | **Eflatun.SceneReference** — type-safe scene fields |
| Tilemaps | Unity `Tilemap` + `TilemapCollider2D` + `CompositeCollider2D` |
| Tests | Unity Test Framework (PlayMode + EditMode), once `.asmdef` files exist |

### Visual placeholder rules
- All in-game visuals are **primitive shapes** drawn either as `SpriteRenderer`s with Unity's built-in `Square`/`Circle`/`Triangle` sprites, or as `Shapes`-style procedural meshes / `LineRenderer`s.
- A central [Assets/_Project/Art/Procedural/](Assets/_Project/) palette ScriptableObject defines colors per entity type (Mario, Goomba, brick, coin, etc.) so the look is consistent and re-skinnable.
- No texture imports, no `.png`, no `.aseprite`. The Aseprite/PSD importers stay installed but unused for now.
- No audio clips. The audio system (§4.16) is a fully-wired stub that plays nothing.

### Coding conventions
- C# 9+ features OK (Unity 6 supports them). Nullable reference types **off** (Unity default).
- Namespaces rooted at `SMW.<Subsystem>` (e.g. `SMW.Player`, `SMW.Enemies`, `SMW.Audio`).
- One `MonoBehaviour` per file. Data-only classes (`GoombaData`, `LevelData`) live as **ScriptableObjects**, not hardcoded.
- Prefer composition over inheritance. Enemy variants are different ScriptableObject configs of the same `Enemy` MonoBehaviour where possible.
- Public fields in inspector-driven components use `[SerializeField] private` + property accessor — no bare `public` fields.
- No singletons-by-instance-field. Use a single `GameServices` locator or `[DefaultExecutionOrder]` bootstrapper that wires references at scene load.

## 3. Architecture Overview

```
                 ┌────────────────┐
                 │  GameServices  │  (locator: persists across scenes)
                 └───────┬────────┘
                         │
       ┌─────────────────┼──────────────────────────────┐
       │                 │                              │
 ┌─────▼─────┐    ┌──────▼──────┐               ┌───────▼───────┐
 │ GameState │    │  AudioBus   │               │  SaveManager  │
 │  (FSM)    │    │  (stub)     │               │   (Json)      │
 └─────┬─────┘    └─────────────┘               └───────────────┘
       │
   Title → Overworld → Level → Pause → GameOver
                          │
              ┌───────────┴────────────┐
              │      LevelContext      │  (per-level DI container)
              └─┬──────┬────────┬──────┘
                │      │        │
          ┌─────▼┐ ┌───▼───┐ ┌──▼─────┐
          │Player│ │Enemies│ │Tilemap │
          └──────┘ └───────┘ └────────┘
```

- **Scene per top-level state**: `Boot`, `Title`, `Overworld`, `Level`. Levels are loaded **additively** on top of a persistent `Systems` scene that hosts `GameServices`.
- **Bootstrap**: a `Boot.unity` scene with a single `Bootstrapper` GameObject loads the persistent `Systems.unity` scene additively, then transitions to `Title`.
- **No `FindAnyObjectByType` / `FindFirstObjectByType` in hot paths** (and never the deprecated `FindObjectOfType`). Cross-system references are resolved once at scene load via `GameServices` or per-level `LevelContext` and cached.

## 4. Core Systems

### 4.1 Input
- `InputSystem_Actions.inputactions` already exists. Define action maps:
  - `Player`: `Move (Vector2)`, `Jump (Button)`, `SpinJump (Button)`, `Run (Button, hold)`, `Crouch (Button)`, `Pause (Button)`.
  - `Overworld`: `Move (Vector2)`, `Confirm (Button)`, `Cancel (Button)`.
  - `UI`: standard navigation set.
- Switch maps via a single `InputRouter` component so subsystems never enable/disable maps directly.

### 4.2 Player Controller (Mario)
SMW physics are deliberately *non-Newtonian*. Use a **dynamic Rigidbody2D with `gravityScale = 0`**, then write velocity directly each `FixedUpdate` from the controller. We let Physics2D handle solid-vs-collider penetration but never let it apply gravity, friction, or impulses to the player. (Pure kinematic was considered but loses `OnCollision*` callbacks and forces us to reimplement depenetration; the dynamic-zero-gravity pattern is the standard Unity 6 approach for tight platformer feel.)

Components on the `Player` prefab:
- `PlayerController` — owns velocity, applies gravity, drives the rigidbody via `rb.linearVelocity = …` (note: `velocity` is renamed to `linearVelocity` in Unity 6).
- `PlayerStateMachine` — see §4.3.
- `PlayerInputBinding` — translates `InputAction` callbacks into intent fields the controller reads each `FixedUpdate`.
- `GroundProbe` — separate component that does `Physics2D.BoxCast` / `RaycastNonAlloc` to detect ground, ceiling, walls, and slope angle. Uses a dedicated `Solid` layer mask, never `Physics2D.OverlapCircleAll` against everything.
- `PlayerCarry` — manages a held object (shell, P-switch, springboard) and its release/throw vector.
- `PlayerVisuals` — owns the procedural shape renderer; subscribes to state changes via events.

Required mechanics (these are the things that *make it feel like SMW*; do not skip any):
- **Variable jump height** based on jump button hold duration (≈ 0–18 frames @ 60Hz cuts gravity). Hold-time is measured in `FixedUpdate` ticks, not real seconds, so behavior stays stable across frame rates.
- **Coyote time** (~6 frames after walking off a ledge) and **jump buffering** (~6 frames before landing).
- **Run speed gate**: holding Run gradually accelerates Mario past the walk cap; only at full sprint can he perform a *long jump* (extra horizontal velocity). Sprint also fills the "P-meter" used for cape flight.
- **Spin jump** (separate input): slightly lower jump arc, can break rotating/used blocks from above, **kills most enemies on contact like a stomp** (with no rebound height), and lets Mario safely land on spiked enemies (Buzzy Beetle, spike-shelled Koopa) that would otherwise damage him.
- **Slope handling**: walking down slopes preserves grounded state (no airborne flicker); running down slopes accelerates.
- **Skidding**: turning while at high velocity produces a deceleration state with its own visual.
- **Crouch / duck slide** (Super+ only): Small Mario cannot crouch. Ducking while sliding on a slope produces a damaging slide.
- **Ceiling hit cancels upward velocity** without rebounding.
- **Pickup & throw**: pressing Run while next to a stunned shell, P-switch, or springboard picks it up; releasing Run drops it; pressing Run while moving throws it horizontally. Held objects move with the carry transform and disable their own collisions until release.
- **Pipe entry** (down/right pipes): triggered by holding the directional input over a flagged pipe segment; smoothly tweens position into the pipe via PrimeTween, then triggers a sub-area transition (§4.5).
- **Star invincibility** (overlay state): timer-based, kills any enemy on touch including normally-invulnerable ones (except Boo and bosses), grants no flight or block-break privileges, and replaces music via a music stack push (§4.16). Implemented as a *modifier* on top of the power-up FSM, not a state in it.

### 4.3 Player Power-Up State
A finite state machine over the **size/power** axis. Star invincibility is *not* a state here — it's a separate overlay timer (see §4.2) that any of these states can have active.

```
                  ┌─── Fire Flower ──▶ Fire ─┐
                  │                          │ (any feather→cape, any flower→fire)
Small ─Mushroom─▶ Super                      │
  ▲               │  └─ Cape Feather ─▶ Cape ┘
  │               │
  │               ▼  damage
  │             Super  (Fire/Cape → Super on damage, never directly to Small)
  │               │
  │               ▼  damage
  └─── Small ──── ┘
        │
        ▼  damage
      Death
```

- All "grow" pickups (mushroom while Small) move you up exactly one level.
- All "ability" pickups (flower / feather) take you to that ability's state regardless of which ability you currently hold — picking up a feather as Fire Mario becomes Cape Mario, with no intermediate Super stage.
- Damage flow is always Fire/Cape → Super → Small → Death. There is no path from Fire/Cape directly to Small.
- States are ScriptableObject configs (`PowerStateData`) holding: collider size, base color, can-shoot-fireball, can-fly, hit-from-above-breaks-bricks, max-fireballs-on-screen, etc.
- Transitions emit a `PlayerStateChanged(prev, next)` event so visuals/audio/UI/collider-resize all react without polling.
- After damage, brief invulnerability (~2s) implemented as a flashing tween via PrimeTween *and* a temporary layer swap to `PlayerInvulnerable` so enemy contact is ignored.

### 4.4 Camera
- `CinemachineCamera2D`-style follow is overkill for V1 — implement a simple `LevelCamera` with:
  - **Forward bias** (camera leads Mario when running).
  - **Vertical lock** unless Mario is on the ground above/below the lock window (SMW's "the camera doesn't follow you mid-air" rule).
  - **Hard bounds** from a `LevelBounds` component (BoxCollider2D used as a marker).
- Pixel-perfect package is installed but only enable it later if/when sprite assets land.

### 4.5 Level / World System
- A **`LevelData` ScriptableObject** is the canonical reference for a level. Fields: `levelId` (stable string), `displayName`, `sceneRef` (`SceneReference`), `timeLimitSeconds`, `musicId`, `themePalette` (`Palette` SO), `entryPoints` (named spawn markers for normal/secret/sub-area returns), `subAreas` (list of named offset regions within the scene), `unlocksOnNormalExit` / `unlocksOnSecretExit` (other `LevelData` references for overworld branching). Levels are added to the game by creating one of these — never by hardcoding a scene name anywhere else.
- One `Level.unity` scene template containing: `Grid` + tilemaps (`Background`, `Solid`, `OneWay`, `Hazard`, `Decoration`, `Interactive`), `LevelBounds`, `LevelRoot` (holds a `LevelData` reference), spawn markers, `MidwayGate`s, `GoalGate`.
- Solid geometry uses `TilemapCollider2D` + `CompositeCollider2D` (set to *Polygons*) for performance.
- **Tile types** are custom `TileBase` subclasses for purely-static layout: `SolidTile`, `OneWayTile`, `SlopeTile`, `PipeTile` (visual only — actual pipe interaction is a GameObject trigger, see below).
- **Interactive tiles** (brick, `?`, note block, used, rotating, P-switch) are *not* TileBase types. They're marker tiles painted on the `Interactive` tilemap layer that the `InteractiveBlockSpawner` (§4.6) replaces with prefab GameObjects on level load. Per-tile mutable state belongs to those GameObjects, not the tilemap.
- **Slopes**: Unity's `TilemapCollider2D` does not natively produce slope shapes — every tile becomes a box. To get real slope physics, `SlopeTile.GetTileData` must set `colliderType = Tile.ColliderType.Sprite` and supply a sprite whose **Custom Physics Shape** is a triangle. The `CompositeCollider2D` will then merge adjacent slope sprites into a continuous polygon edge. Document this in the slope tile's tooltip — it's the most likely thing to forget when authoring new slope variants.
- **Sub-areas** (pipe / door destinations): for V1, sub-areas live as **separate regions of the same Level scene** at a large coordinate offset (e.g. `+10000` units on Y). Pipe entry tweens Mario into the pipe, then snaps the camera + Mario to the destination region's `SpawnMarker` and tweens out. This avoids additive scene loads on every pipe and keeps level state in one place. Each sub-area has its own `LevelBounds` so the camera respects the right rectangle. Switch to additive scenes only if a level grows large enough to hurt scene-load time, which won't happen in V1.
- **Checkpoints / midway**: a `MidwayGate` GameObject placed in the level. Crossing it stores its `CheckpointId` on the player. On death, respawn happens at the most recent checkpoint instead of level start. Crossing a midway when Small also auto-grants a Mushroom (SMW behavior).

### 4.6 Block & Object Interactions
Blocks that need GameObject behavior (animated bumps, contained items, P-switches, switch-palace blocks) are spawned as **runtime GameObjects** by an `InteractiveBlockSpawner` that scans the `Interactive` tilemap layer on level load, instantiates the matching prefab at the tile's world position, and **clears the marker tile** so it doesn't double up. This keeps the tilemap as the source of truth for layout while letting blocks have their own scripts.

Required block behaviors:
- **`?` block** — releases coin / power-up / 1-up based on its `BlockContents` SO. Becomes a "used" block (inert sprite + collider) after triggering. Power-up contents are context-aware: if Mario is Small, give Mushroom; if Super or higher, give whatever the SO declares (Flower/Feather).
- **Brick** — breaks into shards if hit by Super+ Mario from below or stomped by spin jump; bumps (small upward tween) if hit by Small Mario; can also contain coins (`MultiCoinBrick` releases up to 10 coins on a timer).
- **Note block / springboard** — bounces Mario upward; Note variant gives extra height if jump is held on contact.
- **Rotating block** — spins on hit; breakable by spin-jump from above only.
- **P-switch** — when stomped, starts a global timer (~10s) that swaps every coin tile/object → brick and every brick → coin in the active region. Music pitches up as a stack-pushed track. State reverts when the timer expires.
- **Switch Palace blocks** — colored "dotted" outline blocks (Yellow/Green/Red/Blue). Pass-through until the corresponding switch palace has been completed; then become solid blocks globally. State lives in the save file (§4.15) and is read by every block on `Awake`. No per-block listener — the global flag is queried once.
- **Used block** — inert sprite + collider; spawned as the "after" state of `?` blocks and `?`-with-coin bricks.

### 4.7 Enemy System
- Base `Enemy` MonoBehaviour + `EnemyData` ScriptableObject (HP, speed, stomp behavior, fire-vulnerable, spin-jump-vulnerable, can-be-eaten-by-yoshi, points awarded, color, drops).
- Enemies move via the same dynamic-zero-gravity Rigidbody2D approach as the player so they share slope/ground-probe code (factor that into a shared `KinematicBody2D` helper, not a base class).
- Common stomp logic lives in `Enemy.OnStompedFromAbove(player)` and is overridden / parametrized by data. Resolution is centralized in a `CombatResolver` static so the Player ↔ Enemy contact logic isn't duplicated on both sides.
- **Stomp combos**: a `StompCombo` component on the player tracks consecutive enemy kills without touching the ground. Each subsequent kill in a chain awards more points (200 → 400 → 800 → 1000 → ... → 1-up at 8). Resets on landing.
- Enemies are **pooled** (§4.18) and despawn when far off-camera. Off-camera spawners (e.g. `BulletBillLauncher`) re-emit when their position re-enters the camera frustum + a hysteresis margin.

V1 enemy roster (smallest set that exercises every behavior shape):
- **Goomba/Galoomba** — walks, stomp kills.
- **Koopa Troopa** — walks, stomp turns into a sliding shell that can hit other enemies and ricochet off walls.
- **Piranha Plant** — emerges from pipe on a timer; cannot be stomped, killed by fireballs.
- **Bullet Bill** — spawned by an off-screen `BulletBillLauncher`; flies in a straight line.
- **Chargin' Chuck** — runs, takes 3 stomps, charges at the player.
- **Boo** — moves toward player only when player isn't looking; intangible to fireballs.

These six cover: stomping, shells, projectile spawners, multi-hit enemies, line-of-sight AI, fireball interaction.

### 4.8 Yoshi
- Yoshi is a `RideableMount` component. When active, the player rigidbody is parented to Yoshi's saddle transform and the `PlayerController`'s physics step is suspended; a `YoshiController` drives Yoshi's body, which forwards `Jump` input to a Yoshi-specific jump (with the well-known "extended jump" — Mario can jump again from Yoshi's apex). The player power-up state machine and HUD remain unchanged.
- **Tongue**: a short box-cast attack on Fire input. The first eligible target (enemy, edible berry, certain pickups) is parented to a `Mouth` transform and tweened in. Pressing Fire again swallows it; the swallow effect depends on the target.
- **Yoshi color** (`YoshiData` SO: Green/Red/Blue/Yellow) determines passive abilities (e.g. Blue gives flight when holding any shell). Yoshi color is intrinsic — it does NOT come from the held shell. *In SMW the held shell can grant a temporary borrowed ability to Green Yoshi*; that's a separate `BorrowedShellAbility` flag set when Green Yoshi has a colored shell in mouth.
- **V1 scope**: Green Yoshi only. The `YoshiData` plumbing exists so other colors can be added without code changes, but Phase 5 ships only Green and the swallow → spit-fireball / wings-from-shell logic for Green-with-colored-shell. Other colors are a stretch goal.
- **Dismount on damage**: Yoshi panics, runs offscreen for ~3 seconds while flashing, and becomes uncatchable. If the player can recover Yoshi within the window, they remount with no damage taken.

### 4.9 Pickups / Collectibles
- Single `Pickup` MonoBehaviour + `PickupData` SO (coin, dragon coin, 1-up mushroom, super mushroom, fire flower, cape feather, star).
- On player overlap → applies effect via a small `PickupEffect` strategy object (avoid one giant switch statement).
- Dragon coins increment a per-level counter; collecting all 5 awards a 1-up.

### 4.10 Power-up Spawning
- Power-ups emerging from `?` blocks need a small "rise out of block" animation, then act as walking pickups (mushrooms walk and bounce off walls; fire flower / feather are stationary).

### 4.11 Goal / Level Completion
- `GoalGate` component at the end of a level with a rotating bar collider on a fixed-period rotation. Touching the bar freezes player input, scores points based on the bar's height at contact (high → 8000, mid → 4000, low → 2000, plus the remaining-timer bonus × 50), then transitions back to the overworld with a `LevelClearPayload { levelId, secretExit: false, dragonCoinsCollected, scoreEarned }`.
- Optional secret-exit `KeyHole` component triggered by carrying a `Key` pickup into it (uses `PlayerCarry` from §4.2) — emits the same payload with `secretExit: true` for branching map paths.
- The level scene receives the payload from `LevelContext.Begin(LevelData, entryPoint)` and sends the clear payload back via `GameStateMachine.Transition(OverworldState, payload)`.

### 4.12 Overworld Map
- Tilemap-based map with `MapNode` GameObjects for each level. Mario walks node-to-node along `MapPath` connections (BFS over allowed directions per node).
- Selecting a node and pressing Confirm loads the level scene. On clear, the map updates: completed nodes mark with a flag, branching paths unlock.
- Map state persists in the save file (§4.15).

### 4.13 Game State Manager
A top-level FSM with states: `Boot`, `Title`, `Overworld`, `Level`, `Paused`, `GameOver`. Each state knows what scene(s) should be loaded and which input map is active. Transitions go through a single `GameStateMachine.Transition(IGameState next)` method so save/load and analytics hooks are centralized.

### 4.14 Scene Management
- `SceneLoader` service wraps `SceneManager.LoadSceneAsync` with: fade-out, additive load, swap, fade-in.
- All scene references are `SceneReference` (Eflatun) — never raw strings.

### 4.15 Save System
- `SaveManager` writes one JSON file per slot at `Application.persistentDataPath/save_{slot}.json` (slots 1–3).
- A small `SaveIndex.json` tracks which slot is "current" + last-played metadata for the file select screen.
- Persisted in `SaveData`: lives, score, current overworld node id, completed-level map (normal/secret exit flags per level id), switch palace states (4 bools), collected-dragon-coins bitmask per level, midway-checkpoint flags per level, audio volume preferences.
- Serialization uses `JsonUtility` for V1. It can't round-trip polymorphic class hierarchies or `Dictionary<,>`, so all persisted collections are `List<T>` of plain structs with explicit id fields. If that becomes painful, swap to `Newtonsoft.Json` (already pulled in transitively via Eflatun.SceneReference).
- Saves happen at: level clear, overworld node move, switch palace activation, file-select. Never mid-level.

### 4.16 Audio System (stub)
**Critical:** this must be designed *as if* clips and music exist, even though none do.

```
AudioBus  ──▶  SfxChannel    (one-shot 2D sounds)
          ──▶  MusicChannel  (looped, with crossfade)
          ──▶  AmbientChannel
```

- All gameplay code calls `AudioBus.PlaySfx(SfxId.JUMP)` / `AudioBus.PushMusic(MusicId.STAR)` / `AudioBus.PlayJingle(JingleId.ONE_UP)`. **No code references `AudioClip` directly.**
- `SfxId`, `MusicId`, `JingleId` are enums. An `AudioCatalog` ScriptableObject maps each id → `AudioClip` reference. While the catalog is empty, calls log at `Debug.Log` level (gated by a `verbose` flag) and return early.
- `MusicChannel` supports stack-based push/pop with crossfade. Star power and P-switch push; level boss music replaces (clears the stack). On pop, the previous track resumes from where it left off (track current playback position before pushing).
- `JingleChannel` is separate: one-shot non-loopable cues like 1-up jingle, course clear fanfare, game-over. Jingles pause the music stack temporarily and resume it when the jingle ends — they do NOT push onto the stack.
- All channels use **unscaled time** (`AudioSource.ignoreListenerPause` / explicit `unscaledTime` driven crossfades) so menu music continues during `Time.timeScale = 0` pauses, while gameplay SFX honor `AudioListener.pause = true` set by the pause flow (§4.23).
- Master/SFX/Music volume sliders feed an `AudioMixer` asset that already has the routing wired up. Volume settings persist via the save system.

When real assets land, the only work is filling out `AudioCatalog` entries — no gameplay code changes.

### 4.17 UI / HUD
- Single `HUD.uxml` (UI Toolkit) bound via a `UIDocument` component on a `HUDRoot` GameObject in the persistent `Systems` scene. The HUD is *not* a separate scene — UI Toolkit doesn't need one; a single `PanelSettings` asset handles screen scaling.
- Shows: lives, coin count, score, timer (countdown), current power state indicator, dragon coin count, P-switch / star timers when active.
- Pause menu, title screen, file select are additional `UIDocument` components on the same root, each with their own `.uxml` and a `visible` toggle.
- HUD reads from a `HudViewModel` that subscribes to game events (`OnLivesChanged`, `OnCoinsChanged`, `OnPowerStateChanged`, `OnTimerTick`, etc.) — no `Update()` polling of player fields.
- All HUD-driving events are also routed through the `AudioBus` for cue sounds (coin collect, 1-up jingle, low-time warning), so audio and UI stay in lockstep.

### 4.18 Object Pooling
- Use Unity's built-in `ObjectPool<T>` for: enemies, projectiles (fireballs, shells when thrown), pickups, particle bursts.
- Prefab spawning that does *not* go through a pool is a code smell — flag in review.

### 4.19 Procedural Visuals
- `PrimitiveShapeRenderer` component: holds a `ShapeKind` enum (`Square`, `Circle`, `Triangle`, `Capsule`) and a color, draws via a `SpriteRenderer` referencing one of a few generated-at-edit-time primitive sprites in [Assets/_Project/Art/Procedural/](Assets/_Project/). For shapes that need crisp outlines (UI cues, debug visualizations) use `LineRenderer` or a procedural mesh.
- Animated states (walking, jumping, hurt) are conveyed by **color flashes**, **squash/stretch tweens** (PrimeTween), and **rotation** — never by sprite swapping.
- A `Palette` ScriptableObject holds all entity colors keyed by an enum (`PaletteRole.PlayerSmall`, `PaletteRole.Goomba`, `PaletteRole.Brick`, ...). Prefabs reference the palette + a role, never a hardcoded `Color`. Re-theming = swapping the palette asset.

### 4.20 Layers & Physics Matrix
A 2D physics matrix is non-optional for a platformer of this complexity. Set up before Phase 1.

| Layer | Used by |
|---|---|
| `Player` | Mario's collider |
| `PlayerInvulnerable` | temp swap during damage iframes |
| `PlayerProjectile` | fireballs, thrown shells while owned by player |
| `Enemy` | enemies |
| `EnemyProjectile` | bullet bills, hammer-bro hammers |
| `Solid` | tilemap solids, ground |
| `OneWay` | jump-through platforms |
| `Hazard` | spikes, lava, falling-zone trigger |
| `Pickup` | coins, power-ups |
| `Interactive` | `?` blocks, bricks, P-switches, springboards |
| `LevelBounds` | camera bound markers (no collision) |
| `MapNode` | overworld map nodes |

The matrix disables: `Player ↔ PlayerProjectile`, `Enemy ↔ Enemy` (enemies pass through each other except via shells), `Pickup ↔ Pickup`, `Pickup ↔ Solid` collisions for floating pickups, `LevelBounds ↔ everything`, `PlayerInvulnerable ↔ Enemy`. The full table lives in `ProjectSettings/DynamicsManager.asset` and must be checked into git.

### 4.21 Score & Combo
A single `ScoreService` registered on `GameServices`. All point awards funnel through `ScoreService.Award(ScoreReason reason, int basePoints, Vector3 worldPos)` so we can:
- show floating-point popups at the world position via the particle system (§4.24),
- apply combo multipliers (stomp combo, see §4.7),
- centralize 1-up triggers (every 100 coins, every 8-stomp combo, every 5 dragon coins, scored 1-up pickups),
- log to a debug overlay during development.

`ScoreReason` is an enum so we never pass magic numbers around.

### 4.22 Level Timer
- `LevelTimer` component on the level scene root, configured by `LevelMetadata` (default 300 in-game seconds).
- In-game seconds tick faster than real seconds (~0.4s real per tick, matching SMW). Use `Time.deltaTime` accumulation, not real time.
- Fires `OnLowTime` (~100s remaining) which pushes a "hurry up" jingle and pitches the music up via the `AudioBus`.
- At zero: kill the player via the same path as enemy contact while Small (no special "time over" state needed).
- Pauses with `Time.timeScale = 0` automatically.

### 4.23 Pause & Time
- Pause sets `Time.timeScale = 0` AND `AudioListener.pause = true`. UI animations use PrimeTween's `useUnscaledTime: true` flag so they keep running. Background music is on the `AudioBus` which is configured to ignore listener pause for menu music only.
- Unpausing restores both. The pause flow lives on `GameStateMachine.Push(PausedState)`.
- Avoid coroutines in gameplay code that need to keep running while paused — use PrimeTween with unscaled time instead.

### 4.24 Particles & Feedback
- A `FeedbackService` on `GameServices` exposes `Spawn(FeedbackId id, Vector3 pos)` for one-shot visuals: brick shards, coin sparkle, stomp puff, score popup, dust kick, splash.
- Each effect is a small pooled prefab using `ParticleSystem` with shape-only rendering (no textures — built-in default particle is fine, tinted by palette role).
- Score popups (`+200`, `+1UP`) use UI Toolkit world-space rendering or a tiny TextMeshPro instance from a pool — but TMP is not in the manifest yet, so V1 uses a simple sprite-based digit row generated procedurally.

## 5. Game Content (V1 scope)

- **1 overworld** with ~5 connected nodes (`Yoshi's Island 1`-style learning curve).
- **5 hand-authored test levels**, each focused on exercising one subsystem:
  1. Movement & jumps only — no enemies.
  2. Enemies (Goomba, Koopa) + shells + brick blocks.
  3. Power-ups + fire flower combat + Piranha Plants.
  4. Yoshi mount + tongue + colored Yoshi behavior.
  5. Goal post + secret exit + sub-area pipe.
- **1 boss room** (placeholder Bowser-shaped primitive that takes 3 stomps).

Levels are stored as `.unity` scenes referenced by `LevelData` ScriptableObjects. Adding a level = creating the SO + scene, no code changes.

## 6. Implementation Phases

Each phase ends with a runnable build that demos the new system. **Do not skip phases** — each one validates the architecture against real gameplay.

### Phase 0 — Project foundation
- Create assembly definitions: `SMW.Runtime`, `SMW.Editor`, `SMW.Tests` (the Tests asmdef references `UnityEngine.TestRunner` + `UnityEditor.TestRunner`).
- Define the **physics layers + 2D collision matrix** (§4.20). Commit `ProjectSettings/DynamicsManager.asset`.
- Define the **input action maps** in the existing `InputSystem_Actions.inputactions` (§4.1) and write `InputRouter`.
- Bootstrap scene + persistent `Systems` scene + `GameServices` locator.
- `GameStateMachine` skeleton with empty Title/Overworld/Level/Paused states.
- `SceneLoader` with fade transition (using `SceneReference`).
- `AudioBus` stub (logs only) + empty `AudioCatalog` SO + `AudioMixer` asset wired up.
- `Palette` SO with placeholder colors for every `PaletteRole` we know we'll need.
- `PrimitiveShapeRenderer` + procedural shape sprites in `Assets/_Project/Art/Procedural/`.
- `UIDocument`-based HUD root (empty) + pause overlay + pause flow with `Time.timeScale` toggle.
- `SaveManager` round-trips an empty `SaveData` to `save_1.json`.
- `ScoreService` and `FeedbackService` skeletons registered on `GameServices`.

### Phase 1 — Player & physics
- `PlayerController` with dynamic Rigidbody2D (`gravityScale = 0`), `GroundProbe`, manual gravity.
- Walk, run, jump (variable height + coyote + buffer), spin jump, skid, crouch, ceiling cancel.
- `PlayerCarry` placeholder (no actual carryables yet).
- `LevelCamera` with forward bias, vertical lock, and `LevelBounds` clamping.
- One test level: a flat tilemap with a few platforms and a slope. **Tune until it feels right** before moving on. This is the most important tuning gate in the project — do not advance until jump arcs / coyote / buffering are dialed in.

### Phase 2 — Tiles, blocks, pickups, timer
- Custom `TileBase` types (`SolidTile`, `OneWayTile`, `SlopeTile`, `PipeTile`) with proper sprite-collider physics shapes for slopes.
- `InteractiveBlockSpawner` and the full block roster (`?`, brick, multi-coin brick, note, used, P-switch, rotating, switch palace).
- Coin, dragon coin, 1-up mushroom pickups + HUD counters via `HudViewModel`.
- `LevelTimer` countdown + low-time warning.
- `MidwayGate` checkpoints.

### Phase 3 — Power-ups & combat
- `PlayerStateMachine` (Small/Super/Fire/Cape) with full damage flow per §4.3.
- Mushroom, Fire Flower, Cape Feather, Star pickups (Star as `PlayerController` overlay timer, not a state).
- Fireball projectile (pooled, bounces along ground, dies on enemy/wall).
- Cape spin attack (sweep collider that knocks enemies / shells back).
- Death (fall pit / hit while small / time over), respawn at last checkpoint, life counter, game over.
- `PlayerCarry` wired up so future shells/P-switches/springs can be carried.

### Phase 4 — Enemies & combat resolution
- Base `Enemy` + `KinematicBody2D` shared helper + pooling.
- Six V1 enemies (§4.7).
- `CombatResolver` with stomp / fireball / cape / shell-collision matrix.
- `StompCombo` chain scoring.
- Off-screen culling and re-spawn hysteresis.

### Phase 5 — Yoshi
- `RideableMount` + `YoshiController` + saddle parenting.
- Tongue, swallow, dismount-on-damage, recovery window.
- Green Yoshi only; `YoshiData` plumbing in place for future colors.

### Phase 6 — Goal, sub-areas, overworld
- `GoalGate` (with timing-based bonus points) + secret `KeyHole` + carryable `Key` pickup.
- Pipe entry → offset-region sub-area transition → return.
- `Overworld.unity` scene with `MapNode` graph, BFS path traversal, level entry/exit, save persistence (slot system + file select UI).

### Phase 7 — Content & polish
- Author the 5 test levels + 1 boss room.
- Title screen, file select, pause menu UI passes.
- Tune palettes, camera curves, jump constants, audio mixer routing with playtesting.

### Phase 8 — Hardening
- PlayMode tests for: jump arc reproducibility, power-up transition matrix, damage flow, stomp resolution, save/load round-trip, level state restoration after midway respawn, P-switch swap correctness.
- EditMode tests for SO data validation (every `EnemyData`, `PowerStateData`, `LevelData`, `PaletteRole` reference is checked at editor time via an asset postprocessor or one-shot validator).
- Profile with the Unity Profiler; pool any remaining `Instantiate`/`Destroy` hot spots; verify no per-frame allocations in `FixedUpdate`.

## 7. Open Questions

These need a decision before their phase begins; flag them when reached.

- **Slopes:** match SMW's 22.5°/45° slope set exactly, or allow arbitrary angles? (Affects physics tuning.)
- **Cape flight:** full SMW glide/dive physics, or simplified hover? (Cape physics are notoriously tricky.)
- **Save format:** stick with `JsonUtility` or jump straight to Newtonsoft for cleaner polymorphism? (Newtonsoft is already in the project.)
- **UI Toolkit vs uGUI:** uGUI is more familiar to most Unity devs but UI Toolkit is the modern path. Defaulting to UI Toolkit; revisit if it slows us down.
