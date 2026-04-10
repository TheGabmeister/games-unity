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
| Physics | Built-in **Physics2D** (Rigidbody2D + custom controller, see §4.2) |
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
- **No `FindObjectOfType` in hot paths.** Cross-system references are resolved at scene load via `GameServices` or per-level `LevelContext`.

## 4. Core Systems

### 4.1 Input
- `InputSystem_Actions.inputactions` already exists. Define action maps:
  - `Player`: `Move (Vector2)`, `Jump (Button)`, `SpinJump (Button)`, `Run (Button, hold)`, `Crouch (Button)`, `Pause (Button)`.
  - `Overworld`: `Move (Vector2)`, `Confirm (Button)`, `Cancel (Button)`.
  - `UI`: standard navigation set.
- Switch maps via a single `InputRouter` component so subsystems never enable/disable maps directly.

### 4.2 Player Controller (Mario)
SMW physics are deliberately *non-Newtonian*. Use a **kinematic Rigidbody2D** with manual velocity integration rather than relying on `Rigidbody2D.gravityScale`. This is the only way to match the game's coyote time, jump-height-by-hold-time, and run-speed-affects-jump-arc behaviors precisely.

Components on the `Player` prefab:
- `PlayerController` — owns velocity, applies gravity, drives the rigidbody.
- `PlayerStateMachine` — see §4.3.
- `PlayerInputBinding` — translates `InputAction` callbacks into intent fields the controller reads each `FixedUpdate`.
- `GroundProbe` — separate component that does box/ray casts to detect ground, ceiling, walls, and slope angle.
- `PlayerVisuals` — owns the procedural shape renderer; subscribes to state changes via events.

Required mechanics (these are the things that *make it feel like SMW*; do not skip any):
- **Variable jump height** based on jump button hold duration (≈ 0–18 frames @ 60Hz cuts gravity).
- **Coyote time** (~6 frames after walking off a ledge) and **jump buffering** (~6 frames before landing).
- **Run speed gate**: holding Run gradually accelerates Mario past the walk cap; only at full sprint can he perform a *long jump* (extra horizontal velocity).
- **Spin jump** (separate input): lower jump arc, can break certain blocks from above, bounces off most enemies harmlessly.
- **Slope handling**: walking down slopes preserves grounded state; running down slopes accelerates.
- **Skidding**: turning while at high velocity produces a deceleration state with its own visual.
- **Crouch / duck slide** (Super Mario only): ducking while sliding on a slope.
- **Ceiling hit cancels upward velocity** without rebounding.
- **Pipe entry** (down/right pipes): triggered by holding the directional input over a flagged pipe segment; smoothly tweens position then triggers a sub-area scene transition.

### 4.3 Player Power-Up State
A finite state machine over the **size/power** axis:

```
Small ──Mushroom──▶ Super ──Fire Flower──▶ Fire
  ▲                  │  └──Cape Feather──▶ Cape
  │                  ▼
  │                Damage drops one level
  │                  │
  └──Damage when Small──▶ Death
```

- States are ScriptableObject configs (`PowerStateData`) holding: collider size, base color, can-shoot-fireball, can-fly, hit-from-above-breaks-blocks, etc.
- Transitions emit a `PlayerStateChanged` event so visuals/audio/UI can react without polling.
- After damage, brief invulnerability (~2s) implemented as a flashing tween via PrimeTween.

### 4.4 Camera
- `CinemachineCamera2D`-style follow is overkill for V1 — implement a simple `LevelCamera` with:
  - **Forward bias** (camera leads Mario when running).
  - **Vertical lock** unless Mario is on the ground above/below the lock window (SMW's "the camera doesn't follow you mid-air" rule).
  - **Hard bounds** from a `LevelBounds` component (BoxCollider2D used as a marker).
- Pixel-perfect package is installed but only enable it later if/when sprite assets land.

### 4.5 Level / World System
- One `Level.unity` scene template containing: `Grid` + tilemaps (`Background`, `Solid`, `OneWay`, `Hazard`, `Decoration`), `LevelBounds`, `LevelMetadata` (ScriptableObject reference: time limit, music id, theme palette, sub-area refs).
- Solid geometry from `TilemapCollider2D` + `CompositeCollider2D` for performance.
- **Tile types** are custom `TileBase` subclasses (`SolidTile`, `OneWayTile`, `SlopeTile`, `BrickTile`, `QuestionTile`, `PipeTile`). The `BrickTile` and `QuestionTile` swap themselves to a "used block" tile when struck — this is the cleanest place to store per-tile mutable state without GameObjects.
- Sub-areas (pipes, doors) load as another **additive** scene; the parent level scene stays in memory so returning is instant.

### 4.6 Block & Object Interactions
Blocks that need GameObject behavior (animated bumps, contained items, P-switches) are spawned as **runtime GameObjects** by an `InteractiveBlockSpawner` that scans the tilemap on level load and replaces marker tiles with prefabs. This keeps the tilemap as the source of truth for layout while letting blocks have their own scripts.

Required block behaviors:
- `?` block (releases coin / power-up / 1-up based on its `BlockContents` SO).
- Brick (breaks if Super+, bumps if Small).
- Note block / spring.
- Used block (inert).
- P-switch (timed: turns coins → bricks and vice versa for ~10s).
- Rotating block (spin-jump breaks it).

### 4.7 Enemy System
- Base `Enemy` MonoBehaviour + `EnemyData` ScriptableObject (HP, speed, stomp behavior, fire-vulnerable, can-be-eaten-by-yoshi, points awarded, color).
- Enemies move via the same kinematic 2D approach as the player to handle slopes correctly.
- Common stomp logic lives in `Enemy.OnStompedFromAbove(player)` and is overridden / parametrized by data.
- Enemies are **pooled** (§4.18) and despawn when far off-camera.

V1 enemy roster (smallest set that exercises every behavior shape):
- **Goomba/Galoomba** — walks, stomp kills.
- **Koopa Troopa** — walks, stomp turns into a sliding shell that can hit other enemies and ricochet off walls.
- **Piranha Plant** — emerges from pipe on a timer; cannot be stomped, killed by fireballs.
- **Bullet Bill** — spawned by an off-screen `BulletBillLauncher`; flies in a straight line.
- **Chargin' Chuck** — runs, takes 3 stomps, charges at the player.
- **Boo** — moves toward player only when player isn't looking; intangible to fireballs.

These six cover: stomping, shells, projectile spawners, multi-hit enemies, line-of-sight AI, fireball interaction.

### 4.8 Yoshi
- Yoshi is a `RideableMount` component that, when active, replaces the player's controller with a `YoshiController` while preserving the player state machine.
- Tongue: a short raycast attack that grabs the first eligible enemy and parents it to a "mouth" transform; pressing fire again swallows it.
- Color variants (Green/Red/Blue/Yellow) modify shell-eating behavior — encode as `YoshiData` SO.
- Dismount on damage: Yoshi runs offscreen for ~3 seconds before becoming uncatchable.

### 4.9 Pickups / Collectibles
- Single `Pickup` MonoBehaviour + `PickupData` SO (coin, dragon coin, 1-up mushroom, super mushroom, fire flower, cape feather, star).
- On player overlap → applies effect via a small `PickupEffect` strategy object (avoid one giant switch statement).
- Dragon coins increment a per-level counter; collecting all 5 awards a 1-up.

### 4.10 Power-up Spawning
- Power-ups emerging from `?` blocks need a small "rise out of block" animation, then act as walking pickups (mushrooms walk and bounce off walls; fire flower / feather are stationary).

### 4.11 Goal / Level Completion
- `GoalGate` component at the end of a level with a rotating bar collider. Touching it freezes input, awards points based on bar height (timing window), then transitions back to the overworld with a "level cleared" payload.
- Optional secret-exit `KeyHole` component triggered by the `Key` carry pickup — produces a separate completion flag on the level for branching map paths.

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
- `SaveManager` writes one `SaveData` JSON file (`Application.persistentDataPath/save.json`).
- Persisted: lives, score, current world position on map, completed levels (with normal/secret exit flags), switch palace states, collected dragon coins per level.
- Three save slots; serialization uses `JsonUtility` (unless we hit a class-hierarchy issue, then switch to Newtonsoft which is already pulled in transitively by Eflatun).

### 4.16 Audio System (stub)
**Critical:** this must be designed *as if* clips and music exist, even though none do.

```
AudioBus  ──▶  SfxChannel    (one-shot 2D sounds)
          ──▶  MusicChannel  (looped, with crossfade)
          ──▶  AmbientChannel
```

- All gameplay code calls `AudioBus.PlaySfx(SfxId.JUMP)` / `AudioBus.PlayMusic(MusicId.OVERWORLD)`. **No code references `AudioClip` directly.**
- `SfxId` and `MusicId` are enums; an `AudioCatalog` ScriptableObject maps id → `AudioClip` reference. While the catalog is empty, calls log at `Debug.Log` level (gated by a `verbose` flag) and return.
- `MusicChannel` supports stack-based push/pop (so a star powerup can override level music and pop back when it ends).
- Master/SFX/Music volume sliders feed an `AudioMixer` asset that already has the routing wired up.

When real assets land, the only work is filling out `AudioCatalog` entries — no gameplay code changes.

### 4.17 UI / HUD
- Single `HUD.uxml` (UI Toolkit) overlay scene loaded with the level scene. Shows: lives, coin count, score, timer, current power state, dragon coin count.
- Pause menu, title screen, file select also UI Toolkit.
- HUD reads from a `HudViewModel` that subscribes to game events (no `Update()` polling of player fields).

### 4.18 Object Pooling
- Use Unity's built-in `ObjectPool<T>` for: enemies, projectiles (fireballs, shells when thrown), pickups, particle bursts.
- Prefab spawning that does *not* go through a pool is a code smell — flag in review.

### 4.19 Procedural Visuals
- `PrimitiveShapeRenderer` component: holds a `ShapeKind` enum (`Square`, `Circle`, `Triangle`, `Capsule`) and a color, draws via a single quad with a generated mesh or via Unity's built-in shape sprites tinted.
- Animated states (walking, jumping) are conveyed by **color flashes**, **squash/stretch tweens** (PrimeTween), and **rotation**, never by sprite swapping.
- A `Palette` SO holds all entity colors so we can re-theme without touching prefabs.

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
- Create assembly definitions: `SMW.Runtime`, `SMW.Editor`, `SMW.Tests`.
- Bootstrap scene + persistent `Systems` scene + `GameServices` locator.
- `GameStateMachine` skeleton with empty Title/Overworld/Level states.
- `SceneLoader` with fade transition.
- `AudioBus` stub (logs only) + `AudioCatalog` empty SO.
- Empty `HUD` overlay + pause flow.
- Save/load round-trips an empty `SaveData`.

### Phase 1 — Player & physics
- `PlayerController` with kinematic Rigidbody2D, gravity, ground probe.
- Walk, run, jump (variable height + coyote + buffer), spin jump, skid, crouch.
- `LevelCamera` with forward bias and vertical lock.
- One test level: a flat tilemap with a few platforms. **Tune until it feels right** before moving on.

### Phase 2 — Tiles, blocks, pickups
- Custom `TileBase` types and `InteractiveBlockSpawner`.
- `?`, brick, used, note, P-switch, rotating block behaviors.
- Coin & dragon coin pickups, HUD counters.
- Slope tiles & slope physics.

### Phase 3 — Power-ups & combat
- `PlayerStateMachine` (Small/Super/Fire/Cape) with damage flow.
- Mushroom, Fire Flower, Cape Feather, Star pickups.
- Fireball projectile (pooled) + cape spin attack.
- Death + respawn + life counter.

### Phase 4 — Enemies
- Base `Enemy` + pooling.
- Six V1 enemies (§4.7).
- Stomp / fireball / shell-collision interaction matrix.
- Off-screen culling.

### Phase 5 — Yoshi
- Mount/dismount, tongue, swallow, color variants.
- Yoshi-specific damage rules.

### Phase 6 — Goal, sub-areas, overworld
- `GoalGate` + secret `KeyHole`.
- Pipe entry → sub-area additive load → return.
- `Overworld.unity` scene with nodes, pathfinding, level entry/exit, save persistence.

### Phase 7 — Content & polish
- Author the 5 test levels + 1 boss room.
- Title screen, file select, pause menu UI passes.
- Tune palettes, camera, jump curves with playtesting.

### Phase 8 — Hardening
- PlayMode tests for: jump arc, power-up transitions, stomp resolution, save/load, level state restoration.
- EditMode tests for SO data validation (every `EnemyData`, `PowerStateData`, `LevelData` is checked at editor time).
- Profile and pool any remaining `Instantiate`/`Destroy` hot spots.

## 7. Open Questions

These need a decision before their phase begins; flag them when reached.

- **Slopes:** match SMW's 22.5°/45° slope set exactly, or allow arbitrary angles? (Affects physics tuning.)
- **Cape flight:** full SMW glide/dive physics, or simplified hover? (Cape physics are notoriously tricky.)
- **Save format:** stick with `JsonUtility` or jump straight to Newtonsoft for cleaner polymorphism? (Newtonsoft is already in the project.)
- **UI Toolkit vs uGUI:** uGUI is more familiar to most Unity devs but UI Toolkit is the modern path. Defaulting to UI Toolkit; revisit if it slows us down.
