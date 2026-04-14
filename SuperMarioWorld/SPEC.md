# Super Mario World Recreation — SPEC

## 1. Project Overview

Recreate the **gameplay** of *Super Mario World* (SNES, 1990) in Unity 6 (URP 2D). The goal is mechanical fidelity, not visual fidelity. All sprites, textures, and audio are replaced with placeholders so the project can be built end-to-end without external assets, while remaining structured to drop those assets in later.

### Goals
- Faithful recreation of SMW's core platformer feel: physics, jumping, power-ups, enemies, blocks, level flow.
- A complete **vertical slice of the gameplay loop**: overworld map → enter level → play level → exit at goal → return to map. The *loop* is the slice, not the feature count — V1 deliberately exercises every major system (Yoshi, cape, P-switches, save slots, secret exits) so the architecture is validated end-to-end before content authoring begins. This means V1 is wider than a minimal slice would be; that is intentional and called out in §5.
- The minimal playable slice (one level, Mario only, no Yoshi/cape/overworld branching) is reachable by the end of Phase 6 — *that* is the slice gate. Phase 7 then adds the remaining V1 content on top.
- Architecture clean enough that adding the missing presentation layer (sprites, animations, audio) is purely additive — no rewrites.

### Non-Goals
- Pixel-perfect visuals or 1:1 SNES-accurate frame counts. We approximate physics constants and tune for feel.
- Full level content. We ship a small, hand-authored set of test levels that exercise every system; we are not recreating all 96 SMW levels.
- Multiplayer / Luigi co-op.
- Save battery / SRAM accuracy. We use JSON save files via `Newtonsoft.Json`.

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
| Vector art | **`com.unity.vectorgraphics` 3.0.0-preview.7** — SVG → Sprite / UI Image importer. Used for all placeholder visuals (see §4.18). |
| Target resolution | **1280×720** (16:9). Game view, default standalone build resolution, and all CanvasScaler reference resolutions use this. Configured in `ProjectSettings/ProjectSettings.asset` (Player → Resolution & Presentation). World-space units are still tile-based (1 world unit = 1 tile = 16 SVG units); resolution only affects rendering scale and HUD layout. |
| Tests | Unity Test Framework (PlayMode + EditMode), once `.asmdef` files exist |

### Visual placeholder rules
- All in-game visuals are **flat-fill SVG primitives** imported as Unity Sprites via the Vector Graphics package. One `.svg` per entity (Mario, Goomba, brick, coin, etc.) authored as solid white shapes; tinting comes from the palette ScriptableObject via `SpriteRenderer.color` / `Image.color`. See §4.18.
- A central `Palette` ScriptableObject in [Assets/_Project/Art/Procedural/](Assets/_Project/) defines the color per entity role so the look is consistent and re-skinnable.
- SVGs live alongside the palette in the same folder. When real sprite assets land later, the swap is *just* changing the `Sprite` reference on prefabs — no code path changes.
- No `.png` or `.aseprite` imports for placeholders. The Aseprite/PSD importers stay installed but unused until real art arrives.
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

- **Scene per top-level state**: `Boot`, `Systems` (persistent), `Title`, `Overworld`, `Level`. Levels are loaded **additively** on top of `Systems`. `Title` and `Overworld` are also loaded additively on top of `Systems` and unloaded on transition.
- **`Paused` and `GameOver` are NOT scenes** — they are modal states overlaid on whatever scene is currently active (almost always `Level`). Their UI lives on the persistent HUDRoot canvas (§4.17) so it can appear regardless of which level is loaded.
- **`Title` and `Overworld` own their own Canvases and cameras inside their scenes.** The persistent HUDRoot canvas is *only* used for in-level HUD, the pause menu, and the game-over overlay. This is the canonical rule — see §4.17 for the panel breakdown.
- **Bootstrap**: a `Boot.unity` scene with a single `Bootstrapper` GameObject loads the persistent `Systems.unity` scene additively, then transitions to `Title`.
- **No `FindAnyObjectByType` / `FindFirstObjectByType` in hot paths** (and never the deprecated `FindObjectOfType`). Cross-system references are resolved once at scene load via `GameServices` or per-level `LevelContext` and cached.

## 4. Core Systems

### 4.1 Input
- `InputSystem_Actions.inputactions` already exists. Define action maps:
  - `Player`: `Move (Vector2)`, `Jump (Button)`, `SpinJump (Button)`, `Action (Button)`, `Crouch (Button)`, `Pause (Button)`.
  - `Overworld`: `Move (Vector2)`, `Confirm (Button)`, `Cancel (Button)`.
  - `UI`: standard navigation set.
- Switch maps via a single `InputRouter` component so subsystems never enable/disable maps directly.

#### Bindings (locked — no rebinding UI in V1)

Every action is bound to both keyboard and gamepad simultaneously, so the New Input System routes events from whichever device the player is currently using. These are authored into `InputSystem_Actions.inputactions` during Phase 0 and are considered final for V1 — we do not ship a rebinding UI. If a player wants different bindings, they edit the asset (or we revisit post-V1).

**Player map**

| Action | Keyboard | Gamepad (generic) | SMW equivalent |
|---|---|---|---|
| `Move` (Vector2) | WASD **and** Arrow keys (composite 2D) | Left Stick **and** D-Pad | D-Pad |
| `Jump` | Space | South button (Xbox A / PS Cross) | B |
| `SpinJump` | Left Shift | East button (Xbox B / PS Circle) | A |
| `Action` | Z, and J as alternate | West button (Xbox X / PS Square), and Left Shoulder as alternate | Y / X |
| `Crouch` | (derived from `Move.y < -0.5`) | (derived from `Move.y < -0.5`) | Down on D-Pad |
| `Pause` | Escape | Start button (Xbox Menu / PS Options) | Start |

Notes:
- **`Crouch` is not a separate binding** — it's read as a `Move.y < -0.5` threshold inside `PlayerInputBinding`. This matches SMW where Down is the crouch "button" and avoids a duplicate keyboard binding on `S` / Down that would fight `Move`.
- **Two bindings for `Action`** (West + Left Shoulder on gamepad; Z + J on keyboard). Intentional — SMW mapped the same function to both Y and X, and having the face button + a shoulder button lets the player pick a comfortable combo for running-while-jumping (hold shoulder → press face button for Jump).
- **Diagonal inputs**: the composite `Move` Vector2 is fed to the controller as-is. Gamepad sticks give full analog range, but the controller treats `|Move.x| > 0.5` as "walking" and raw stick values as the magnitude for gradual acceleration. No explicit deadzone — the Input System's default stick deadzone (≈0.125) is enough.

**Overworld map**

| Action | Keyboard | Gamepad |
|---|---|---|
| `Move` (Vector2) | WASD **and** Arrow keys | Left Stick **and** D-Pad |
| `Confirm` | Space, Enter | South button |
| `Cancel` | Escape | East button |

**UI map**

Standard `InputSystemUIInputModule` bindings (auto-populated by the Input System sample UI asset): `Navigate` → stick/D-Pad/arrows, `Submit` → South button/Space/Enter, `Cancel` → East button/Escape, `Point` → mouse, `Click` → left mouse. No customization needed.

#### Out of scope for V1

- **Rebinding UI** — fixed bindings per the tables above. We won't ship `InputActionRebindingExtensions` flows, a controls screen, or rebinding persistence.
- **Touch input / on-screen controls** — not targeted. If mobile is ever targeted, the New Input System supports `<Touchscreen>` paths that would bolt on without controller-code changes.
- **Haptics / rumble** — no controller rumble in V1. `Gamepad.current.SetMotorSpeeds` exists for later if desired.
- **The `Action` button is overloaded by design** — this matches SMW's Y button. `PlayerInputBinding` exposes both the **hold state** and **press/release events** of the same `Action` input:
  - **Hold** → run (gradual acceleration past walk cap, fills P-meter; see §4.2).
  - **Press** → context-sensitive single-shot: throw fireball (Fire Mario), cape sweep (Cape Mario, on ground), Yoshi tongue (mounted on Yoshi), pickup carryable (next to a stunned shell / P-switch / springboard), drop / throw held object.
  - The controller resolves which press-event meaning applies via priority: held-object-throw > Yoshi-tongue > power-state attack > pickup. Only one fires per press.
- Every other system in this spec refers to this single button as **"Action"**. Avoid using "Run", "Fire", or "Y button" in code or docs — it's `Action` everywhere.

### 4.2 Player Controller (Mario)
SMW physics are deliberately *non-Newtonian*. Use a **dynamic Rigidbody2D with `gravityScale = 0`**, then write velocity directly each `FixedUpdate` from the controller. We let Physics2D handle solid-vs-collider penetration but never let it apply gravity, friction, or impulses to the player. (Pure kinematic was considered but loses `OnCollision*` callbacks and forces us to reimplement depenetration; the dynamic-zero-gravity pattern is the standard Unity 6 approach for tight platformer feel.)

Components on the `Player` prefab:
- `PlayerController` — owns velocity, applies gravity, drives the rigidbody via `rb.linearVelocity = …` (note: `velocity` is renamed to `linearVelocity` in Unity 6).
- `PlayerStateMachine` — see §4.3.
- `PlayerInputBinding` — translates `InputAction` callbacks into intent fields the controller reads each `FixedUpdate`.
- `GroundProbe` — separate component that does `Physics2D.BoxCast` / `RaycastNonAlloc` to detect ground, ceiling, walls, and slope angle. Uses a dedicated `Solid` layer mask, never `Physics2D.OverlapCircleAll` against everything.
- `PlayerCarry` — manages a held object (shell, P-switch, springboard) and its release/throw vector.
- `PlayerVisuals` — owns the `SpriteRenderer` and the per-state SVG sprite + palette role; subscribes to `PlayerStateChanged` events to swap sprite / collider size / tint.

Required mechanics (these are the things that *make it feel like SMW*; do not skip any):
- **Variable jump height** based on jump button hold duration (≈ 0–18 frames @ 60Hz cuts gravity). Hold-time is measured in `FixedUpdate` ticks, not real seconds, so behavior stays stable across frame rates.
- **Coyote time** (~6 frames after walking off a ledge) and **jump buffering** (~6 frames before landing).
- **Run speed gate**: holding `Action` (§4.1) gradually accelerates Mario past the walk cap; only at full sprint can he perform a *long jump* (extra horizontal velocity). The long-jump trigger is a velocity threshold read directly from the rigidbody — there is no separate P-meter resource in V1 (see cape notes below).
- **Spin jump** (separate input): slightly lower jump arc, can break rotating/used blocks from above, **kills most enemies on contact like a stomp** (with no rebound height), and lets Mario safely land on spiked enemies (Buzzy Beetle, spike-shelled Koopa) that would otherwise damage him.
- **Slope handling**: walking down slopes preserves grounded state (no airborne flicker); running down slopes accelerates.
- **Skidding**: turning while at high velocity produces a deceleration state with its own visual.
- **Crouch / duck slide** (Super+ only): Small Mario cannot crouch. Ducking while sliding on a slope produces a damaging slide.
- **Ceiling hit cancels upward velocity** without rebounding.
- **Pickup & throw**: pressing `Action` (§4.1) while next to a stunned shell, P-switch, or springboard picks it up. While carrying, releasing `Action` drops it gently; tapping `Action` while moving throws it horizontally. Held objects move with the carry transform and disable their own collisions until release.
- **Pipe entry** (down/right pipes): triggered by holding the directional input over a flagged pipe segment; smoothly tweens position into the pipe via PrimeTween, then triggers a sub-area transition (§4.5).
- **Star invincibility** (overlay state): timer-based, kills any enemy on touch including normally-invulnerable ones (except Boo and bosses), grants no flight or block-break privileges, and replaces music via a music stack push (§4.16). Implemented as a *modifier* on top of the power-up FSM, not a state in it.
- **Cape Mario abilities** (simplified from SMW — cape flight is explicitly scoped down; see resolution in §7):
  - **Ground sweep**: `Action` press while Cape Mario and grounded emits a brief arc collider for ~0.25s that knocks enemies back, breaks bricks below/next to Mario, and reflects enemy projectiles (fireballs from Hammer Bros, etc. — not in V1 enemy roster, but the matrix entry is wired up). Has a short cooldown so `Action` can't be spammed.
  - **Slow-fall glide**: while Cape Mario is airborne AND descending (velocity.y < 0), holding `Jump` reduces descent to ~25% of normal gravity. Releasing `Jump` restores normal gravity. There is no forward propulsion, no diving, no upward boost, no takeoff from the ground, and no P-meter. This preserves "cape feels floaty" without the SMW flight state machine.
  - What is **not** implemented in V1: running take-off into flight, diving (down + Jump burst), dive-rebound, cape flutter during mid-air spin, holding-a-shell-while-gliding edge cases. If full flight is ever needed post-V1, it bolts on as a separate `CapeFlightState` component without affecting the simplified path.

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
- States are ScriptableObject configs (`PowerStateData`) holding: collider size, base color, can-shoot-fireball, can-cape-sweep, can-slow-fall, hit-from-above-breaks-bricks, max-fireballs-on-screen, etc. (Note: `can-fly` is replaced by the more specific `can-slow-fall` — we no longer have a generic flight flag, consistent with the simplified cape in §4.2.)
- Transitions emit a `PlayerStateChanged(prev, next)` event so visuals/audio/UI/collider-resize all react without polling.
- After damage, brief invulnerability (~2s) implemented as a flashing tween via PrimeTween *and* a temporary layer swap to `PlayerInvulnerable` so enemy contact is ignored.

### 4.4 Camera
- `CinemachineCamera2D`-style follow is overkill for V1 — implement a simple `LevelCamera` with:
  - **Forward bias** (camera leads Mario when running).
  - **Vertical lock** unless Mario is on the ground above/below the lock window (SMW's "the camera doesn't follow you mid-air" rule).
  - **Hard bounds** from a `LevelBounds` component (BoxCollider2D used as a marker).
- **Orthographic size** is set so the camera shows **~14 tiles vertically**, matching SMW's vertical game-area feel. At 1 world unit = 1 tile, this is `Camera.orthographicSize = 7`. At 1280×720 this also gives ~24.9 tiles of horizontal visibility (14 × 16/9), which is wider than SMW's ~16-tile width — intentional and fine for 16:9.
- At 1280×720 with `orthographicSize = 7`, each world tile renders as ~51×51 screen pixels. SVG-imported sprites (PPU=16, viewBox 16×16 per tile) scale cleanly to this size with no sub-pixel issues because they're tessellated meshes, not texture samples.
- Pixel-perfect package is installed but only enable it later if/when *pixel-art* sprite assets land — SVG meshes don't benefit from it and it would actually interfere with smooth scaling.

### 4.5 Level / World System
- A **`LevelData` ScriptableObject** is the canonical reference for a level. Fields: `levelId` (stable string), `displayName`, `sceneRef` (`SceneReference`), `timeLimitSeconds`, `musicId`, `themePalette` (`Palette` SO), `entryPoints` (named spawn markers for normal/secret/sub-area returns), `subAreas` (list of named offset regions within the scene), `unlocksOnNormalExit` / `unlocksOnSecretExit` (other `LevelData` references for overworld branching). Levels are added to the game by creating one of these — never by hardcoding a scene name anywhere else.
- One `Level.unity` scene template containing: `Grid` + tilemaps (`Background`, `Solid`, `OneWay`, `Hazard`, `Decoration`, `Interactive`), `LevelBounds`, `LevelRoot` (holds a `LevelData` reference), spawn markers, `MidwayGate`s, `GoalGate`.
- Solid geometry uses `TilemapCollider2D` + `CompositeCollider2D` (set to *Polygons*) for performance.
- **Tile types** are custom `TileBase` subclasses for purely-static layout: `SolidTile`, `OneWayTile`, `SlopeTile`, `PipeTile` (visual only — actual pipe interaction is a GameObject trigger, see below).
- **Interactive tiles** (brick, `?`, note block, used, rotating, P-switch) are *not* TileBase types. They're marker tiles painted on the `Interactive` tilemap layer that the `InteractiveBlockSpawner` (§4.6) replaces with prefab GameObjects on level load. Per-tile mutable state belongs to those GameObjects, not the tilemap.
- **Slopes**: Unity's `TilemapCollider2D` does not natively produce slope shapes — every tile becomes a box. To get real slope physics, `SlopeTile.GetTileData` must set `colliderType = Tile.ColliderType.Sprite` and supply a sprite whose **Custom Physics Shape** is a triangle. The `CompositeCollider2D` will then merge adjacent slope sprites into a continuous polygon edge. Document this in the slope tile's tooltip — it's the most likely thing to forget when authoring new slope variants.
- **Slope angle set is locked to SMW's two angles — no arbitrary slopes.** This keeps physics tuning tractable and prevents level authors from introducing angles the controller hasn't been tuned against:
  - **Steep (45°)** — 1 tile rise per 1 tile run. One sprite, one triangular physics shape.
  - **Shallow (~26.57°, commonly mislabeled "22.5°")** — 1 tile rise per 2 tiles run, so the slope spans two adjacent tiles. This requires **two** sprite variants: a lower-half tile (rises from floor to mid) and an upper-half tile (rises from mid to ceiling). Both have their own triangular Custom Physics Shape; `CompositeCollider2D` merges them into one continuous edge when painted side-by-side.
  - Each of the two angles exists in **both directions** (floor rising to the right, floor rising to the left). Implemented as separate sprite/tile assets rather than relying on `TileData.transform` flip, because `ColliderType.Sprite` physics shapes do not always mirror reliably with tile transforms. Four sprites total: steep-L, steep-R, shallow-L (×2 halves), shallow-R (×2 halves) — six sprite variants, four logical slope types.
  - Ceiling slopes (upside-down variants) are **not** in V1 — SMW uses them sparingly and they complicate the `GroundProbe` normal logic. Flag for post-V1 if a boss room needs them.
  - `SlopeTile` exposes its angle as a public float so `GroundProbe` can read it for movement adjustments (walking-down preserves grounded state, running-down accelerates per §4.2). The controller reads the angle from the surface normal returned by `BoxCast`, not by type-checking the tile — this is what keeps slope behavior consistent with the collider shape.
- **Sub-areas** (pipe / door destinations): for V1, sub-areas live as **separate regions of the same Level scene** at a large coordinate offset (e.g. `+10000` units on Y). Pipe entry tweens Mario into the pipe, then snaps the camera + Mario to the destination region's `SpawnMarker` and tweens out. This avoids additive scene loads on every pipe and keeps level state in one place. Each sub-area has its own `LevelBounds` so the camera respects the right rectangle. Switch to additive scenes only if a level grows large enough to hurt scene-load time, which won't happen in V1.
- **Checkpoints / midway**: a `MidwayGate` GameObject placed in the level. Crossing it stores its `CheckpointId` on the `LevelRunState` (§4.24), NOT on the player or the save file. On death, the level scene reloads and Mario respawns at the matching `SpawnMarker` for that checkpoint. Crossing a midway when Small also auto-grants a Mushroom (SMW behavior). Midway state is wiped when the player exits the level (whether by goal, voluntary back-to-overworld, or game over) — see §4.24.

### 4.6 Block & Object Interactions
Blocks that need GameObject behavior (animated bumps, contained items, P-switches, switch-palace blocks) are spawned as **runtime GameObjects** by an `InteractiveBlockSpawner` that scans the `Interactive` tilemap layer on level load, instantiates the matching prefab at the tile's world position, and **clears the marker tile** so it doesn't double up. This keeps the tilemap as the source of truth for layout while letting blocks have their own scripts.

Each block type gets its own generated prefab (`Block_Question.prefab`, `Block_Brick.prefab`, …) under the §4.25 thin-prefab / fat-SO rule. `?` block contents (coin / specific power-up / 1-up) are a `BlockContents` SO that the prefab references — a single `Block_Question.prefab` is reused for every `?` block in the game with only the `BlockContents` SO varying. P-switch duration, brick coin-count limits, and similar tunables all live on their respective SOs, not on the prefabs.

Required block behaviors:
- **`?` block** — releases coin / power-up / 1-up based on its `BlockContents` SO. Becomes a "used" block (inert sprite + collider) after triggering. Power-up contents are context-aware: if Mario is Small, give Mushroom; if Super or higher, give whatever the SO declares (Flower/Feather).
- **Brick** — breaks into shards if hit by Super+ Mario from below or stomped by spin jump; bumps (small upward tween) if hit by Small Mario; can also contain coins (`MultiCoinBrick` releases up to 10 coins on a timer).
- **Note block / springboard** — bounces Mario upward; Note variant gives extra height if jump is held on contact.
- **Rotating block** — spins on hit; breakable by spin-jump from above only.
- **P-switch** — when stomped, starts a global timer (~10s) that swaps every coin tile/object → brick and every brick → coin in the active region. Music pitches up as a stack-pushed track. State reverts when the timer expires.
- **Switch Palace blocks** — colored "dotted" outline blocks (Yellow/Green/Red/Blue). Pass-through until the corresponding switch palace has been completed; then become solid blocks globally. State lives in the save file (§4.15) and is read by every block on `Awake`. No per-block listener — the global flag is queried once.
- **Used block** — inert sprite + collider; spawned as the "after" state of `?` blocks and `?`-with-coin bricks.

### 4.7 Enemy System

**Design model: capability interfaces + per-archetype enemy classes.** Enemies declare what can happen to them by implementing C# interfaces (`IStompable`, `IFireballHit`, `ICapeSweepHit`, etc.). Attackers (player, fireball, cape sweep, moving shell) resolve interactions via `TryGetComponent<ICapability>` on the hit GameObject and invoke the interface method if present. No central `CombatResolver`, no abstract `Enemy` base class, no virtual-method polymorphism. The combat matrix is *emergent* — each cell is simply whether an enemy implements a given capability.

**This intentionally departs from the thin-prefab / fat-SO rule (§4.25) for enemies specifically.** Pickups and blocks still use thin-prefab / fat-SO because their variance is purely data (coin/mushroom/flower are the same `Pickup` + different SOs; `?`/brick/note-block are the same `InteractiveBlock` + different SOs). Enemies differ too much per-archetype for that to hold — a walker, a ballistic projectile, a ghost, and a teleporting Magikoopa have nothing in common at the code level. Each archetype gets its own MonoBehaviour class. Per-variant tuning (Koopa colors, Cheep-Cheep types) still uses `EnemyData` SOs against a single archetype class.

**Per-enemy class shape:**
- Each archetype is its own MonoBehaviour (`Galoomba`, `KoopaWalker`, `KoopaShell`, `PiranhaPlant`, `ChargingChuck`, `Boo`, `BulletBill`, etc.) implementing whichever capability interfaces apply.
- Each class reads an `EnemyData` SO for per-variant tuning: HP, speed, points, sprite, collider size, color/palette role, drops.
- Shared movement primitives live in a `KinematicBody2D` helper component (dynamic-zero-gravity Rigidbody2D + ground probe + slope handling), not a base class. Grounded enemies (Galoomba, KoopaWalker, Chuck, ...) attach it; Bullet Bill and Boo don't.
- `EnemyDespawn` helper on every enemy that should despawn off-camera. Off-camera spawners (`BulletBillLauncher`) re-emit when their position re-enters the camera frustum + hysteresis margin.
- Dynamic spawns (projectiles, VFX, score popups) use plain `Instantiate` / `Destroy` — no pooling.

**`StompCombo`** lives on the player. Tracks consecutive enemy kills without touching the ground. Each successive kill in a chain awards more points (200 → 400 → 800 → 1000 → … → 1-up at 8). Resets on landing. Notched by the player collision dispatch on any successful `IStompable.OnStomped` call.

#### Full SMW enemy roster (reference)

Not all of these ship in V1 — see the V1 roster below. This list exists to ground architecture decisions (especially combat resolution) in the actual breadth of behavior, not just the V1 subset. Organized by **combat archetype**, because that's the axis the code has to handle. Variants within an archetype are usually expressible as `EnemyData` flags.

Each enemy is mapped to its MonoBehaviour class and the capability interfaces it implements. Format: `ClassName : IInterface, ...`. Variants that differ only in data share a class + different `EnemyData` SO; variants that differ in behavior get their own class. Sibling-component swaps (Koopa walk↔shell) are shown as `ClassA [swap] ClassB` both on the same prefab.

**Walkers** (stompable from above, side contact hurts Mario):
- Galoomba — flipped on stomp, can be picked up → `Galoomba : IStompable, IContactDamage, IFireballHit, IShellImpact, IThrowable`
- Rex — 2 stomps; first flattens → `Rex : IStompable, IContactDamage, IFireballHit, IShellImpact` (internal HP = 2; first stomp swaps to flat sprite + smaller collider)
- Dino-Torch / Dino-Rhino — breathes fire; Dino-Rhino splits into two Dino-Torches on damage → `DinoTorch : IStompable, IContactDamage, IFireballHit, IShellImpact` + `PeriodicEmitter` component. `DinoRhino` is its own class with `splitOnDeath` logic.
- Mega Mole — giant, Mario can ride on top → `MegaMole : IStompable, IContactDamage, IFireballHit, IRideable`
- Wiggler — 2 stomps, aggros on first → `Wiggler : IStompable, IContactDamage, IFireballHit, IShellImpact`

**Shelled walkers** (stomp → shell that can be kicked, carried, ricocheted):
- Koopa Troopa (4 colors) → `KoopaWalker : IStompable, IContactDamage, IFireballHit, IShellImpact` [swap] `KoopaShell : IBumpable, IThrowable, IShellImpact, IContactDamage`. Color is `KoopaData` SO variance: Green `ledgeTurn = false`, Red `ledgeTurn = true`, Yellow `kicksDust`, Blue `kicksShells`.
- Paratroopa → `ParatroopaFlier : IStompable, IContactDamage` [swap] `KoopaWalker` [swap] `KoopaShell` (three-stage).
- Buzzy Beetle → same pattern as Koopa but Buzzy's walker class **does not implement `IFireballHit`** (so fireballs extinguish on it without damage). `BuzzyWalker : IStompable, IContactDamage, IShellImpact` [swap] `BuzzyShell` (shared class with `KoopaShell` if behavior allows, else own).
- Spike Top → `SpikeTopWalker : IContactDamage, ISpinJumpSafe, IShellImpact` (wall-crawling; no `IStompable` — spin-jump is the only safe attack) [swap on spin-jump] `SpikeTopShell : IBumpable, IThrowable, IShellImpact, IContactDamage`.

**Spiked**:
- Spiny → `Spiny : IContactDamage, ISpinJumpSafe, IFireballHit, IShellImpact` (no `IStompable`; spin-jump bounces safely)
- Urchin → `Urchin : IContactDamage, IFireballHit` (no `ISpinJumpSafe` — spikes all sides)
- Porcu-Puffer → `PorcuPuffer : IContactDamage, IFireballHit`
- Sumo Brother → `SumoBrother : IContactDamage, IFireballHit` (stationary; lightning spawn logic in class)

**Un-stompable**:
- Piranha Plant (standing / upside-down) → `PiranhaPlant : IContactDamage, IFireballHit, ICapeSweepHit`
- Jumping Piranha → `JumpingPiranha : IContactDamage, IFireballHit, ICapeSweepHit` (different emerge pattern; own class)
- Fire Piranha → `FirePiranha : IContactDamage, IFireballHit, ICapeSweepHit` + `PeriodicEmitter` (fireballs)
- Fishin' Boo → `FishinBoo : IContactDamage, ICapeSweepHit` (fireball passes through — no `IFireballHit`)

**Intangible-based**:
- Boo → `Boo : IConditionallyTangible, ICapeSweepHit, IContactDamage` (no `IFireballHit` — cape/star only)
- Big Boo → `BigBoo : IConditionallyTangible, ICapeSweepHit, IContactDamage` (HP > 1, internal)
- Eerie → `Eerie : IContactDamage, ICapeSweepHit` (flies a fixed path; cape/star kill only)
- Boo Buddies (circle) → `BooBuddyMember : IConditionallyTangible, IContactDamage, ICapeSweepHit` on each member + `BooBuddyCircle` driving orbit on the parent
- Boo Block → `BooBlock : IBumpable` initially; on trigger, reveals → spawns a `Boo`

**Ballistic**:
- Bullet Bill → `BulletBill : IStompable, IContactDamage, IFireballHit`
- Banzai Bill → `BanzaiBill : IStompable, IContactDamage, IFireballHit` (larger, same interfaces)
- Torpedo Ted → `TorpedoTed : IStompable, IContactDamage, IFireballHit` (underwater variant)

**AI-heavy spawners / special attackers**:
- Lakitu → `Lakitu : IStompable, IContactDamage, IFireballHit` + `PeriodicEmitter` (Spinies). Cloud-steal handled by `OnStomped` logic (parent the cloud to player).
- Magikoopa → `Magikoopa : IStompable, IContactDamage, IFireballHit, ICapeSweepHit` (teleport + wand projectile logic in class)
- Amazing Flyin' Hammer Brother → `HammerBroPlatform : IStompable, IContactDamage, IFireballHit` (platform is the target) + `HammerBroThrower` component on the Bro that spawns hammers
- Monty Mole → `MontyMoleAmbush` (trigger-only, no damage interfaces while hidden) [swap on emerge] `MontyMoleWalker : IStompable, IContactDamage, IFireballHit, IShellImpact`
- Fire Piranha → see Un-stompable above

**Multi-stomp / stateful**:
- Chargin' Chuck (basic) → `ChargingChuck : IStompable, IContactDamage, IFireballHit, ICapeSweepHit` (internal `hp = 3`, walk↔charge mini-FSM)
- Clappin' / Jumpin' / Bouncin' Chucks → same interfaces, own classes for behavior variance (or a single `ChuckVariant` enum-switched class if behaviors are close enough — judgment call per variant)
- Splittin' Chuck → own class; spawns smaller Chucks on stomp
- Diggin' Chuck → own class; throws rocks from fixed hole (uses `PeriodicEmitter`)
- Football / Puntin' / Whistlin' / Swimmin' Chucks → each its own class for distinct attacks
- Bob-omb → `BobOmb : IStompable, IContactDamage, IFireballHit, IThrowable` (stomp lights fuse; throwable while lit; explosion on expiry)

**Environmental hazards**:
- Thwomp → `Thwomp : IContactDamage` (invulnerable; idle → fall-on-proximity → recover FSM)
- Thwimp → `Thwimp : IContactDamage, IFireballHit` (bouncing arc)
- Fuzzy → `Fuzzy : IContactDamage, ICapeSweepHit` (track-following electric hazard)
- Ninji → `Ninji : IStompable, IContactDamage, IFireballHit` (periodic in-place jumps)
- Swoopin' Stu → `SwoopinStu : IStompable, IContactDamage, IFireballHit` (ceiling drop on proximity)
- Grinder → `Grinder : IContactDamage` (invulnerable spinning hazard on a path)

**Aquatic**:
- Cheep-Cheep (both colors) → `CheepCheep : IStompable, IContactDamage, IFireballHit` (Green = path, Red = chase; `CheepCheepData` SO selects)
- Blurp → same class as Cheep-Cheep with data variance, or own class if motion differs enough
- Rip Van Fish → `RipVanFish : IStompable, IContactDamage, IFireballHit` (internal sleep→wake→chase FSM)
- Dolphin → not an enemy; `RideableDolphin : IRideable` on a water-path prefab

**Bosses** — out of scope for shared interfaces. Each is its own scripted MonoBehaviour (`IggyFight`, `ReznorFight`, `BowserFight`, …) with its own phase machine. They may implement a subset of interfaces where relevant (e.g., `Reznor : IFireballHit` since it's fireball-only), but their fight logic lives entirely in their own class.

**Distinct enemy classes required (count check):**

| Tier | Count | Classes |
|---|---|---|
| Shared class, data-variant only (1 class, N SOs) | ~2 | `Koopa*` (walker + shell; 4 color SOs), `CheepCheep` (green/red SOs) |
| Own class, reusing interface set | ~15 | `Galoomba`, `Rex`, `Wiggler`, `MegaMole`, `DinoTorch`, `Spiny`, `Urchin`, `PorcuPuffer`, `BulletBill`, `BanzaiBill`, `TorpedoTed`, `Thwimp`, `Ninji`, `SwoopinStu`, `Blurp`, `RipVanFish` |
| Own class, bespoke logic | ~12–15 | `PiranhaPlant`, `JumpingPiranha`, `FirePiranha`, `FishinBoo`, `Boo`, `BigBoo`, `Eerie`, `BooBuddyMember`, `BooBlock`, `Lakitu`, `Magikoopa`, `HammerBroPlatform`, `MontyMole*`, `SumoBrother`, `Thwomp`, `Fuzzy`, `Grinder`, `BobOmb`, Chuck variants (~5) |
| Shared helper components | ~3 | `KinematicBody2D`, `PeriodicEmitter`, `EnemyDespawn` |
| Capability interfaces | ~10 | `IStompable`, `IBumpable`, `IFireballHit`, `ICapeSweepHit`, `IShellImpact`, `IThrowable`, `IContactDamage`, `ISpinJumpSafe`, `IRideable`, `IConditionallyTangible` |
| Boss scripts | ~11 | 7 Koopalings + Big Boo + Reznor + Bowser + minibosses |

**Total for full roster: ~30–35 enemy classes + ~11 boss scripts + 10 interfaces + 3 helpers.**

**Observations** (facts, not conclusions):
- Most enemies have 3–5 interfaces. Reading the class declaration line tells you what it is and what can hurt it.
- The bespoke long tail exists in any architecture — Lakitu's cloud-steal, Magikoopa's teleport, Thwomp's fall-FSM have to live somewhere. Each lives in its own class with clear boundaries.
- The only shared-class-many-SOs case is Koopa/Paratroopa (color variants) and Cheep-Cheep (path/chase variants). Everything else is its own class because movement/AI differs enough that SO flags would become a branching god-class.
- V1 ships 6 enemies → 6 classes + maybe 2 helper classes + ~8 interfaces. The full roster's shape informs architecture; V1's scope is much smaller.
- My confidence on exotic variants (Chuck sub-variants, Grinder, some Dinos, Booster enemies) remains lower — treat those counts as estimates.

V1 enemy roster (smallest set that exercises every behavior shape):
- **Goomba/Galoomba** — walks, stomp kills.
- **Koopa Troopa** — walks, stomp turns into a sliding shell that can hit other enemies and ricochet off walls.
- **Piranha Plant** — emerges from pipe on a timer; cannot be stomped, killed by fireballs.
- **Bullet Bill** — spawned by an off-screen `BulletBillLauncher`; flies in a straight line.
- **Chargin' Chuck** — runs, takes 3 stomps, charges at the player.
- **Boo** — moves toward player only when player isn't looking; intangible to fireballs.

These six cover: stomping, shells, projectile spawners, multi-hit enemies, line-of-sight AI, fireball interaction.

#### Combat via capability interfaces

All combat goes through C# interfaces in `Assets/_Project/Scripts/Runtime/Combat/Capabilities/`:

```csharp
public interface IStompable             { void OnStomped(PlayerController p, StompKind kind); }
public interface IBumpable              { void OnBumpedFromBelow(PlayerController p); }
public interface IFireballHit           { FireballReaction OnHitByFireball(Fireball f); }
public interface ICapeSweepHit          { void OnHitByCapeSweep(Vector2 dir); }
public interface IShellImpact           { void OnHitByShell(Shell s); }
public interface IThrowable             { void OnPickedUp(PlayerController p); void OnThrown(Vector2 v); }
public interface IContactDamage         { DamageInfo ContactDamage { get; } }           // side-touch hurts Mario
public interface ISpinJumpSafe          { }                                              // marker: spin-jump bounces, no damage
public interface IRideable              { Vector3 SaddleOffset { get; } }
public interface IConditionallyTangible { bool IsTangibleTo(PlayerController p); }       // Boo-style gating

public enum StompKind         { Normal, Spin }
public enum FireballReaction  { Absorbed, Passes, Reflects }   // only cross-component outcome type; fireball-scoped
```

Every capability is **optional**. An enemy that can't be stomped doesn't implement `IStompable`. An enemy that can't be fireballed (Boo) doesn't implement `IFireballHit`. The matrix is the set of interfaces per enemy — readable on the class declaration line.

**Player collision dispatch** lives in a `PlayerCollisionRouter` component on the player prefab:

```csharp
void OnCollisionEnter2D(Collision2D col) {
    var go = col.collider.gameObject;
    if (go.TryGetComponent<IConditionallyTangible>(out var t) && !t.IsTangibleTo(player)) return;

    var n = col.GetContact(0).normal;
    var fromAbove = n.y > 0.7f && rb.linearVelocity.y <= 0;
    var spin = player.SpinJumpActive;

    if (fromAbove) {
        var stomp = GetActiveComponent<IStompable>(go);
        if (stomp != null) {
            stomp.OnStomped(player, spin ? StompKind.Spin : StompKind.Normal);
            player.ApplyStompRebound(spin);
            player.StompCombo.Notch();
            return;
        }
        if (spin && go.GetComponent<ISpinJumpSafe>() != null) {
            player.ApplyStompRebound(spin: true);
            return;
        }
    }
    if (go.TryGetComponent<IContactDamage>(out var d)) player.TakeDamage(d.ContactDamage);
}
```

`GetActiveComponent<T>` filters disabled components: `GetComponents<T>().FirstOrDefault(c => c is not Behaviour b || b.enabled)`. Needed because `GetComponent<T>` returns disabled Behaviours by default, and state transitions (Koopa walk↔shell) rely on toggling `.enabled`.

**Attacker sides are equally thin:**
- **Fireball** — `OnTriggerEnter2D` → `TryGetComponent<IFireballHit>`; branch on the returned `FireballReaction` (extinguish / pass through / reflect).
- **Cape sweep** — arc trigger collider active ~0.25s; for each overlapping collider, `TryGetComponent<ICapeSweepHit>` and call.
- **Moving shell** — `OnCollisionEnter2D` → `TryGetComponent<IShellImpact>` for enemies, `TryGetComponent<IBumpable>` for blocks.

**State transitions (Koopa walk → shell, Paratroopa flier → walker → shell):** sibling components on the same GameObject, toggle `.enabled`:

```csharp
[RequireComponent(typeof(KoopaWalker), typeof(KoopaShell))]
public class Koopa : MonoBehaviour { /* shared data, sprite, collider */ }

public class KoopaWalker : MonoBehaviour, IStompable, IContactDamage, IFireballHit, IShellImpact {
    public void OnStomped(PlayerController p, StompKind k) {
        GetComponent<KoopaShell>().enabled = true;
        enabled = false;   // swap
    }
    // ... walking logic
}

public class KoopaShell : MonoBehaviour, IBumpable, IThrowable, IShellImpact, IContactDamage {
    // ... sliding-shell logic
}
```

Paratroopa is the same pattern with three components (`ParatroopaFlier` → `KoopaWalker` → `KoopaShell`), each enabling the next on stomp.

**Shared helpers (not base classes):**
- `KinematicBody2D` — movement + ground/wall/slope probing. Reused by any grounded enemy.
- `PeriodicEmitter` — composable component that spawns a projectile prefab on a timer. Used by Dino-Torch (fire spit), Fire Piranha (fireballs), Lakitu (Spinies). Decouples "emits projectiles" from each enemy's main behavior.
- `EnemyDespawn` — off-camera culling with hysteresis.

**What's NOT in this design** (for clarity, because earlier drafts of the spec had these):
- No `CombatResolver` static class.
- No abstract `Enemy` base class, no `EnemyBehavior` strategy base class, no virtual combat methods.
- No `CombatOutcome` struct — `FireballReaction` is the only cross-component outcome type, and it's scoped to fireball only.
- No enemy-class hierarchy (`KoopaEnemy : Enemy`, etc.). Each archetype is a standalone MonoBehaviour.

**Adding a new attack type post-V1** (e.g., Ice Flower freezing enemies): add `IIceVulnerable`, implement on the enemies that should freeze, update the ice projectile to `TryGetComponent<IIceVulnerable>`. No existing enemy class is touched.

**Adding a new enemy:** create a MonoBehaviour class, pick the interfaces it implements, create a prefab and an `EnemyData` SO. No generator machinery — enemy prefabs are hand-authored in the Unity editor (this is the thin-prefab-rule carve-out).

### 4.8 Yoshi
- Yoshi is a `RideableMount` component. When active, the player rigidbody is parented to Yoshi's saddle transform and the `PlayerController`'s physics step is suspended; a `YoshiController` drives Yoshi's body, which forwards `Jump` input to a Yoshi-specific jump (with the well-known "extended jump" — Mario can jump again from Yoshi's apex). The player power-up state machine and HUD remain unchanged.
- **Tongue**: a short box-cast attack on `Action` press (§4.1). The first eligible target (enemy, edible berry, certain pickups) is parented to a `Mouth` transform and tweened in. Pressing `Action` again swallows it; the swallow effect depends on the target.
- **Yoshi color** (`YoshiData` SO: Green/Red/Blue/Yellow) determines passive abilities (e.g. Blue gives flight when holding any shell). Yoshi color is intrinsic — it does NOT come from the held shell. *In SMW the held shell can grant a temporary borrowed ability to Green Yoshi*; that's a separate `BorrowedShellAbility` flag set when Green Yoshi has a colored shell in mouth.
- **V1 scope**: Green Yoshi only. The `YoshiData` plumbing exists so other colors can be added without code changes, but Phase 5 ships only Green and the swallow → spit-fireball / wings-from-shell logic for Green-with-colored-shell. Other colors are a stretch goal.
- **Dismount on damage**: Mario takes a hit while mounted → he is forcibly dismounted with no power-state loss; Yoshi enters a *panic* state, runs in the direction Mario was facing, flashes, and is **catchable for ~3 seconds**. If Mario touches Yoshi during the panic window, he remounts. If the panic timer expires or Yoshi crosses a `LevelBounds` edge, Yoshi is lost for the rest of the attempt.

### 4.9 Pickups / Collectibles
- Follows the thin-prefab / fat-SO pattern (§4.25): a single `Pickup_Base.prefab` hosts the `Pickup` MonoBehaviour, collider, and sprite; every variant is just a different `PickupData` SO (coin, dragon coin, 1-up mushroom, super mushroom, fire flower, cape feather, star).
- `Pickup.Awake` reads the SO to set sprite, collider size, and the `PickupEffect` strategy reference.
- On player overlap → applies effect via a small `PickupEffect` strategy object (avoid one giant switch statement). `PickupEffect` is a polymorphic field on `PickupData` so each SO carries its own effect behavior without per-variant prefab wiring.
- Dragon coins increment a per-level counter on `LevelRunState` (§4.24); collecting all 5 awards a 1-up.

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
- `SceneLoader` service wraps `SceneManager.LoadSceneAsync` with: fade-out, additive load, swap, fade-in. Fade is delegated to a separate `ScreenFader` MonoBehaviour — see the scene loader mechanics subsection below.
- All scene references are `SceneReference` (Eflatun) — never raw strings.

#### Scene entry points (Play from any scene)
A strict "always enter through Boot" model breaks editor iteration — testing a level would require opening `Boot.unity` and navigating through Title → Overworld → Level every time. We solve this with a two-piece pattern that works identically in both builds and the editor:

**Piece 1 — auto-bootstrap via `[RuntimeInitializeOnLoadMethod]`.** A static method in `Bootstrapper` runs **before any scene's `Awake`**, regardless of which scene is the active entry:

```csharp
public static class Bootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsureSystemsLoaded()
    {
        if (!SceneManager.GetSceneByName("Systems").isLoaded)
            SceneManager.LoadScene("Systems", LoadSceneMode.Additive);
    }
}
```

- **In builds**: `Boot.unity` is at build-index 0, so it's the entry. `Boot`'s own `Awake` normally loads `Systems` additively and transitions to `Title`. `EnsureSystemsLoaded` is effectively a no-op because Boot's own logic already loaded Systems. No behavior change.
- **In the editor** with any other scene open and Play pressed: `EnsureSystemsLoaded` runs before that scene's `Awake`, so `Systems` is loaded and `GameServices` is populated by the time anything else wakes up.

**Piece 2 — `LevelRoot` detects direct-entry and simulates the normal flow.** Each Level scene has a `LevelRoot` MonoBehaviour holding a `LevelData` reference. Its `Awake`:

```csharp
void Awake()
{
    if (GameServices.GameState.Current is LevelState)
    {
        // Normal flow: LevelState.OnEnter() already set up LevelContext.
        return;
    }
    #if UNITY_EDITOR
        // Direct editor entry — fabricate the flow.
        GameServices.GameState.EnterDirectLevel(levelData, defaultEntryPoint);
    #else
        Debug.LogError($"Level {levelData.levelId} loaded without GameStateMachine entry. Build config bug.");
    #endif
}
```

- `GameStateMachine.EnterDirectLevel(LevelData, string entryPoint)` is an editor-only helper that:
  1. Loads test `SaveData` — either a fresh `SaveData` (5 lives, 0 score, no level completions) or save slot 1, controlled by an `EditorTestSettings` ScriptableObject at `Assets/_Project/Settings/EditorTestSettings.asset`. Default: fresh.
  2. Pushes `LevelState` directly onto the `GameStateMachine`, skipping Title and Overworld.
  3. Calls `LevelContext.Begin(levelData, entryPoint)` identically to how `LevelState.OnEnter` would.
  4. Resolves `entryPoint` to `LevelData.entryPoints["default"]` (the level's main spawn marker) when the caller doesn't specify.

**Equivalent behavior for Title and Overworld scenes**: both have a scene-root MonoBehaviour (`TitleRoot` / `OverworldRoot`) with the same direct-entry detection pattern — if the `GameStateMachine` isn't already in the matching state, transition to it. This makes every scene in the project hit-Play-runnable.

#### Play-from-scene matrix

| Editor entry scene | What happens |
|---|---|
| `Boot.unity` | Normal flow. Same as a build. |
| `Systems.unity` alone | `EnsureSystemsLoaded` logs a helpful error ("don't Play from Systems — use Boot or a Level"). No gameplay is possible from this scene alone. |
| `Title.unity` | Systems loads additively → `TitleRoot` detects no `TitleState` active → transitions to Title. |
| `Overworld.unity` | Systems loads additively → `OverworldRoot` transitions to Overworld, with fresh or slot-1 `SaveData` per `EditorTestSettings`. |
| `Level01.unity` … `LevelNN.unity` | Systems loads additively → `LevelRoot` calls `EnterDirectLevel` → Mario spawns at default marker, HUD works, pause works, audio stub works. |

#### Build Settings hygiene

- `Boot.unity` MUST be at build-index 0 (entry scene in builds).
- `Systems.unity`, `Title.unity`, `Overworld.unity`, and every `LevelXX.unity` MUST be present in Build Settings, or the `SceneManager.LoadScene` calls above fail silently.
- `LevelData.OnValidate()` emits an editor warning if its `sceneRef` points at a scene that isn't in Build Settings. Don't rely on humans to remember.
- Missing Build Settings registration is the #1 reason "Play on my level scene doesn't work" — flag it loudly.

#### Creating `Boot.unity` and `Systems.unity` (Phase 0, one-time)

Hand-authored in the Unity editor, not generator-owned. These are infrastructure scenes — they don't hold SO-driven content, they rarely change, and nothing else in the project regenerates them. The prefab/debug-scene generators (§4.25 / §4.26) exist because those artifacts need to stay in sync with data — Boot and Systems don't. Create once, commit, move on.

Step-by-step:

1. **`Boot.unity`** — File → New Scene → Empty. Delete the default `Main Camera` and `Directional Light`. Add one GameObject named `Bootstrapper` with the `Bootstrapper` MonoBehaviour. Save to `Assets/_Project/Scenes/Boot.unity`. File → Build Profiles → add to Scenes list at **index 0**.
2. **`Systems.unity`** — File → New Scene → Empty. Delete default camera/light. Add, at the scene root:
   - `GameServices` GameObject with the `GameServices` locator component (and child components for each service registered at boot: `SaveManager`, `SceneLoader`, `GameStateMachine`, `ScoreService`, `FeedbackService`, audio bus — see §3).
   - `HUDRoot` Canvas (Screen Space - Overlay) with `CanvasScaler` + `CanvasScalerPresetApplier` (§4.17), child `HudPanel` / `PauseMenuPanel` / `GameOverPanel` placeholders.
   - `TransitionCanvas` Canvas (Screen Space - Overlay, `sortingOrder` above `HUDRoot`) with a full-screen black `Image` child and the `ScreenFader` MonoBehaviour on the canvas root. Used by `SceneLoader` for fade transitions — see the scene loader mechanics subsection below.
   - `AudioBus` GameObject with child `AudioSource`s for each channel per §4.16 (`MusicChannel`, `SfxChannel`, `JingleChannel`, `AmbientChannel`, `UiSfxChannel` with `ignoreListenerPause = true`).
   - `EventSystem` with `InputSystemUIInputModule` (Input System UI module, not the legacy Standalone Input Module).
   Save to `Assets/_Project/Scenes/Systems.unity` and add to Build Settings (any index > 0).
3. **`Title.unity` / `Overworld.unity`** — empty shells in Phase 0 containing only a `TitleRoot` / `OverworldRoot` MonoBehaviour (per §4.14 direct-entry pattern) + a Canvas. Content lands in Phases 6/7.
4. **Verify from Play-from-scene matrix**: pressing Play from `Boot.unity` loads Systems additively and transitions to Title; pressing Play from `Title.unity` alone also works (Bootstrapper's `[RuntimeInitializeOnLoadMethod]` loads Systems, `TitleRoot` detects direct entry).

After Phase 0 these scenes are effectively frozen. Subsequent phases add components to `GameServices` or panels under `HUDRoot`, but the scene structure itself doesn't churn.

#### Scene loader + fade transition

Split into two components with separate concerns, both registered on `GameServices`:

- **`SceneLoader`** — pure service, no scene dependencies. Wraps `SceneManager.LoadSceneAsync` / `UnloadSceneAsync` / `SetActiveScene` and orchestrates the transition sequence. Accepts `SceneReference` (Eflatun, already in manifest) so inspector fields are type-safe and validated against Build Settings at edit time.
- **`ScreenFader`** — MonoBehaviour on a dedicated `TransitionCanvas` GameObject in `Systems.unity`. Owns a full-screen black `Image` (`raycastTarget = true` to block input during transitions), Screen Space - Overlay, `sortingOrder` above every other canvas so it covers HUD, pause menu, and game-over overlay. Exposes `Task FadeInAsync(float duration)` / `Task FadeOutAsync(float duration)`, driven by PrimeTween with `useUnscaledTime: true` so it works regardless of `Time.timeScale`.

`SceneLoader.LoadAsync` composes the two:

```csharp
public async Task LoadAsync(SceneReference target, SceneLoadOptions opts = default) {
    if (_isTransitioning) { Debug.LogWarning("SceneLoader re-entered during transition"); return; }
    _isTransitioning = true;

    await _fader.FadeOutAsync(opts.FadeOutDuration);           // 1. fade to black
    await SceneManager.LoadSceneAsync(target.Name, LoadSceneMode.Additive);  // 2. load target additively
    OnTransitionPeak?.Invoke(target, opts.Payload);            // 3. mid-transition hook (LevelContext.Begin, HUD rebind, camera snap)
    if (opts.UnloadPrevious) await SceneManager.UnloadSceneAsync(_previous);  // 4. unload old
    SceneManager.SetActiveScene(SceneManager.GetSceneByName(target.Name));    // 5. set active for lighting / default instantiation
    await _fader.FadeInAsync(opts.FadeInDuration);             // 6. fade back up

    _isTransitioning = false;
}
```

**API surface:**

```csharp
public sealed class SceneLoader {
    public Task LoadAsync(SceneReference target, SceneLoadOptions opts = default);
    public Task ReloadLevelAsync();   // death / midway-respawn flow per §4.24
    public event Action<SceneReference, object> OnTransitionPeak;  // fires while screen is black
}
```

**Why split** (instead of one combined `SceneTransitionController`):
- `SceneLoader` is pure logic — making it a MonoBehaviour just to host the fader would pull it into the scene hierarchy for no reason. As a pure service, it's unit-testable with a stub fader.
- `ScreenFader` is reusable for non-load scenarios (boss intro flash, debug teleport hide) that don't want to touch scene loading.
- Different shapes (service vs. scene-content MonoBehaviour) belong in different classes.

**Why not split further** (no `IScreenFader` interface, no loader/swapper/event-bus breakdown): YAGNI — introduce abstraction when the second use case actually arrives.

**Edge cases:**
- **Re-entry is rejected**, not stacked. A second `LoadAsync` during a transition logs a warning and returns.
- **Death reload** ([§4.24](SPEC.md)) uses `ReloadLevelAsync` — same machinery, target = current scene, payload carries `LevelRunState.checkpointId` so the peak hook respawns at the midway marker.
- **Audio during transition** — `AudioListener.pause` is NOT used (reserved for pause menu per §4.22). Music plays through unless a caller explicitly runs `AudioBus.FadeMusicOut` in parallel.
- **Transition durations** are caller-controlled via `SceneLoadOptions` (defaults: 0.3s each way). Faster for death reload (~0.15s), slower for title → overworld establishing shot.

### 4.15 Save System
- `SaveManager` writes one JSON file per slot at `Application.persistentDataPath/save_{slot}.json` (slots 1–3).
- A small `SaveIndex.json` tracks which slot is "current" + last-played metadata for the file select screen.
- Persisted in `SaveData`: lives, score, total coin count (the rolling counter that triggers 1-ups every 100), current overworld node id, completed-level map (normal/secret exit flags per level id), switch palace states (4 bools), collected-dragon-coins bitmask per cleared level, audio volume preferences. **Midway checkpoints are NOT in save data** — they're per-attempt state (§4.24).
- Serialization uses **`Newtonsoft.Json`** (`com.unity.nuget.newtonsoft-json`, pulled in transitively via Eflatun.SceneReference — no manifest change needed). This gives us:
  - Native `Dictionary<TKey, TValue>` round-trip, which matches the natural shape of `SaveData` (e.g. `Dictionary<string, LevelCompletionFlags>` keyed by `levelId`, `Dictionary<string, ulong>` for dragon-coin bitmasks).
  - Polymorphic type handling via `TypeNameHandling.Auto` when we need it (e.g. if `PickupData`-style hierarchies ever need to persist — not expected in V1, but the door's open).
  - Enum-as-string conversion via `StringEnumConverter` so save files stay human-readable and diff-able. Treat save JSON as text we'd be willing to open in a text editor for debugging.
- All numeric-keyed maps use `string` keys in the persisted form (e.g. `levelId` is already a string). No attempt to preserve `int` / `enum` keys in JSON — round-trip via strings.
- A single `SaveSerializer` static wraps `JsonConvert.SerializeObject` / `DeserializeObject` with our standard `JsonSerializerSettings` (pretty-printed, `StringEnumConverter`, `NullValueHandling.Ignore`, `MissingMemberHandling.Ignore` so old saves survive adding new fields).
- **Save versioning**: `SaveData` has a top-level `int saveVersion` field. On load, if the stored version is less than the current, run registered `ISaveMigration` steps in order before deserializing into the current types. V1 ships at `saveVersion = 1` with the migration chain empty.
- File format: UTF-8, pretty-printed JSON. No compression or encryption in V1 — not worth the complexity for a single-player offline game, and readable saves help debugging.
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
- **Pause behavior** (canonical — see also §4.22): when `AudioListener.pause = true` is set by the pause flow, *all* channels (`SfxChannel`, `MusicChannel`, `JingleChannel`, `AmbientChannel`) pause. The `MusicChannel` stack is preserved as-is — Unity's per-`AudioSource` playback head freezes — so on unpause everything resumes from where it stopped. There is no ducking, no fade-out, no music-stack manipulation on pause. Music simply stops and resumes.
- The only exception is a single dedicated **`UiSfxChannel`** `AudioSource` with `ignoreListenerPause = true`, used for the pause-toggle SFX itself, menu navigation cues, and any other UI sound that must play *while* `AudioListener.pause` is true. Gameplay code never touches this channel directly — it's reached via `AudioBus.PlayUiSfx(...)`.
- Crossfades on `MusicChannel` / `JingleChannel` use unscaled time (`Time.unscaledDeltaTime`) so a crossfade kicked off just before pause doesn't get stuck mid-blend, and so menu transitions in non-Level scenes (where there's no `Time.timeScale = 0`) work uniformly.
- Master/SFX/Music volume sliders feed an `AudioMixer` asset that already has the routing wired up. Volume settings persist via the save system.

When real assets land, the only work is filling out `AudioCatalog` entries — no gameplay code changes.

### 4.17 UI / HUD
- **uGUI** (Unity's `Canvas`-based UI). Package `com.unity.ugui` 2.0.0 is already in the manifest and ships TextMeshPro bundled in Unity 6, so TMP is available without an extra import.
- UI is split across three canvases by ownership (matching the scene model in §3):
  - **`HUDRoot`** — a persistent `Canvas` in the `Systems` scene. *Screen Space - Overlay*, `CanvasScaler` set to *Scale With Screen Size*, reference resolution **1280×720**, *Screen Match Mode* = `MatchWidthOrHeight` with `Match = 0.5` (balanced between width and height, so non-16:9 windows letterbox/pillarbox gracefully instead of clipping or stretching the HUD). Hosts only the in-level UI: `HudPanel`, `PauseMenuPanel`, `GameOverPanel`. These panels live here because they need to appear over whichever Level scene is currently loaded without re-instantiating per level.
  - **`TitleCanvas`** — lives in `Title.unity`. Hosts `TitlePanel` and `FileSelectPanel`. Disappears when the Title scene unloads.
  - **`OverworldCanvas`** — lives in `Overworld.unity`. Hosts `OverworldHudPanel` (lives, score, level name) and the level-entry confirmation popup.
  - All three canvases use the **same `CanvasScaler` settings** (1280×720, `MatchWidthOrHeight`, `Match = 0.5`) so UI layout stays consistent across scene transitions. This is enforced by a shared `CanvasScalerPresetApplier` helper component at `Awake` rather than duplicating the settings in the inspector, so the reference resolution lives in exactly one place.
- Panels are GameObjects with a `CanvasGroup` for fade/visibility. Only the panel matching the current `GameStateMachine` state is interactive at any time.
- The `HudPanel` shows: lives, coin count, score, timer (countdown), current power state indicator, dragon coin count, P-switch / star timers when active. Built from `Image` (background bars, icons drawn as procedural primitive sprites) and `TextMeshProUGUI` (numeric counters).
- Each panel has a small `XxxView` MonoBehaviour that holds `[SerializeField]` references to its `TextMeshProUGUI` / `Image` children. The view subscribes to events from a `HudViewModel` (`OnLivesChanged`, `OnCoinsChanged`, `OnPowerStateChanged`, `OnTimerTick`, etc.) and writes to the UI components in the handler — no `Update()` polling of player fields.
- Buttons in menu panels use `Button` + Input System's `InputSystemUIInputModule` (already part of the input system package) so gamepad navigation works. Selection state is driven by `EventSystem.SetSelectedGameObject` on panel show.
- All HUD-driving events are also routed through the `AudioBus` for cue sounds (coin collect, 1-up jingle, low-time warning), so audio and UI stay in lockstep.

### 4.18 Procedural Visuals (SVG-backed placeholders)
- All placeholder visuals are **SVG sprites** authored by hand, stored in [Assets/_Project/Art/Procedural/](Assets/_Project/), and imported via `com.unity.vectorgraphics` 3.0.0-preview.7. The importer tessellates each SVG into a triangle mesh at edit time; runtime cost is identical to any sprite mesh.
- **One SVG per entity / shape**, named by role (`mario_small.svg`, `mario_super.svg`, `goomba.svg`, `coin.svg`, `brick.svg`, `tile_solid.svg`, `slope_steep_r.svg`, etc.). Each SVG uses a single solid `fill="white"` so palette tinting via `SpriteRenderer.color` / `Image.color` produces the final color.
- **`viewBox` matches the tile grid**: `0 0 16 16` for a 1×1-tile entity, `0 0 16 32` for tall Mario, `0 0 32 16` for wide entities like Banzai Bill. The SVG importer's *Pixels Per Unit* setting is configured to match (16) so 1 SVG unit = 1 world unit / 16th-of-a-tile, and tiles align cleanly.
- **No `PrimitiveShapeRenderer` component.** Just `SpriteRenderer` (or uGUI `Image`) with a `Sprite` reference + a `PaletteBinding` MonoBehaviour that writes the palette color to the renderer on `Awake` and on palette change.
- **Slope tile sprites** (§4.5) are SVGs too — the triangular `<polygon>` doubles as both the rendered shape and the source for Unity's auto-generated *Custom Physics Shape*. Verify after import: the importer should produce a triangular physics shape; if not, set it manually in the Sprite Editor.
- **SVG → uGUI Image** also works (the Vector Graphics package supports both `SpriteRenderer` and uGUI `Image` import targets), so HUD icons (life icon, coin icon, dragon-coin slots) use the same SVG assets as world sprites. One source of truth per shape.
- Animated states (walking, jumping, hurt) are conveyed by **color flashes**, **squash/stretch tweens** (PrimeTween), and **rotation** of the sprite transform — not by swapping to different SVGs every frame. Sprite swapping is reserved for *state* transitions (Small ↔ Super, walking ↔ skidding pose, etc.), not per-frame animation.
- A `Palette` ScriptableObject holds all entity colors keyed by an enum (`PaletteRole.PlayerSmall`, `PaletteRole.Goomba`, `PaletteRole.Brick`, ...). Prefabs reference the palette + a role, never a hardcoded `Color`. Re-theming = swapping the palette asset.

#### Real-asset migration path
When real sprite assets eventually arrive, the swap is purely additive: drop the new `.png` / `.aseprite` next to (or replacing) the SVG, repoint the `Sprite` field on the prefab, and decide per-entity whether `PaletteBinding` should keep tinting or be removed. **No gameplay or controller code changes.** This is the central reason for the SVG approach — it preserves the same `Sprite` data shape as the eventual real assets, so prefabs never need restructuring.

### 4.19 Layers & Physics Matrix
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

### 4.20 Score & Combo
A single `ScoreService` registered on `GameServices`. All point awards funnel through `ScoreService.Award(ScoreReason reason, int basePoints, Vector3 worldPos)` so we can:
- show floating-point popups at the world position via the particle system (§4.23),
- apply combo multipliers (stomp combo, see §4.7),
- centralize 1-up triggers (every 100 coins, every 8-stomp combo, every 5 dragon coins, scored 1-up pickups),
- log to a debug overlay during development.

`ScoreReason` is an enum so we never pass magic numbers around.

### 4.21 Level Timer
- `LevelTimer` component on the level scene root, configured by `LevelData.timeLimitSeconds` (default 300 in-game seconds).
- In-game seconds tick faster than real seconds (~0.4s real per tick, matching SMW). Use `Time.deltaTime` accumulation, not real time.
- Fires `OnLowTime` (~100s remaining) which pushes a "hurry up" jingle and pitches the music up via the `AudioBus`.
- At zero: kill the player via the same path as enemy contact while Small (no special "time over" state needed).
- Pauses with `Time.timeScale = 0` automatically.

### 4.22 Pause & Time
- Pause sets `Time.timeScale = 0` AND `AudioListener.pause = true`. All gameplay simulation, particle systems, and audio (music + SFX + jingles) freeze together. The pause-toggle SFX and menu navigation cues are played via `AudioBus.PlayUiSfx(...)` which routes to a `ignoreListenerPause = true` source — see §4.16 for the canonical rules.
- Pause-menu UI animations (slide-in, button highlights) use PrimeTween with `useUnscaledTime: true` so they keep running while `Time.timeScale = 0`.
- Unpausing reverts both flags. Music resumes from the exact playback head it was paused at; the music stack is unchanged.
- The pause flow lives on `GameStateMachine.Push(PausedState)`. Pause is only allowed while in the `Level` state.
- Avoid coroutines in gameplay code that need to keep running while paused — use PrimeTween with unscaled time instead.

### 4.23 Particles & Feedback
- A `FeedbackService` on `GameServices` exposes `Spawn(FeedbackId id, Vector3 pos)` for one-shot visuals: brick shards, coin sparkle, stomp puff, score popup, dust kick, splash.
- Each effect is a small prefab using `ParticleSystem` with shape-only rendering (no textures — built-in default particle is fine, tinted by palette role). The prefab self-destructs on `Stop` via `ParticleSystem.MainModule.stopAction = Destroy`.
- Score popups (`+200`, `+1UP`) are a world-space `Canvas` prefab (set to *World Space*, not Overlay) containing a `TextMeshProUGUI` that fades and rises via PrimeTween, then destroys itself. TMP is bundled with `com.unity.ugui` in Unity 6, so no extra package is needed.

### 4.24 Run Lifecycle & State Layers
The single most important model in this spec. State lives in exactly **three** layers, and every piece of mutable data must be assigned to one of them. When in doubt, ask: *what should happen to this when the player dies / when the player exits to overworld / when the player closes the game?*

| Layer | Lifetime | Stored on | Examples |
|---|---|---|---|
| **Persistent** | survives game restart | `SaveData` (JSON file via `SaveManager`) | lives, score, total coin count, completed-level flags (normal + secret), saved dragon-coin bitmask per cleared level, switch palace states, current overworld node, audio volume |
| **Session** | survives death + level reload, lost on app close or game-over-to-title | `GameSession` singleton on `GameServices` | current `LevelRunState` reference (only set while inside a level), pending overworld payloads |
| **Per-attempt** | reset on every level entry AND every death-respawn | `LevelRunState` MonoBehaviour on `LevelRoot` | midway checkpoint id, in-attempt dragon coins collected, broken bricks, used `?` blocks, dead enemies, P-switch swap state, level timer, player power state, in-level coin pickups |

#### Death & respawn
On player death (`PlayerController.Die`):
1. The level scene **unloads** and **reloads** (`SceneLoader.ReloadLevel()`), additively. This is the simplest way to reset every interactive block, dragon coin, dead enemy, and tilemap mutation in one shot — no per-system "reset" logic to maintain.
2. `LevelContext.Begin(LevelData, entryPoint)` runs again with `entryPoint = LevelRunState.checkpointId ?? defaultEntry`. Note `LevelRunState` is rebuilt **except** for the `checkpointId` field, which is carried across the reload via `GameSession`.
3. Lives counter on `SaveData` decrements. If lives < 0 → game over → return to title; `SaveData` is saved at title return.
4. Power state respawns as Small (SMW behavior — even if you crossed the midway as Super, you respawn Small at the checkpoint).

#### Level exit (success)
On `GoalGate` / `KeyHole` trigger:
1. `LevelRunState.dragonCoinsCollectedThisAttempt` is merged into `SaveData.dragonCoinsByLevel[levelId]` (bitwise OR — collecting any single coin "saves" it for next time).
2. Completed-exit flag (normal/secret) is set on `SaveData`.
3. Score, total coin count, lives all persist (they were already on `SaveData` and were updated live during the run).
4. `GameSession.LevelRunState` is cleared.
5. `SaveManager.Save()` is called.

#### Voluntary exit to overworld (rare — pause menu)
1. Same as level exit but no completed-exit flag. Dragon coins collected this attempt are **discarded**, not merged. (SMW behavior — you only "earn" dragon coins by completing the level.)
2. Lives, score, total coins persist.
3. `GameSession.LevelRunState` is cleared.

#### Coin counters — clarification
- The **total coin count** on `SaveData` increments live as Mario picks up coins, even before level exit. It is what drives the every-100-coins 1-up. This persists on death.
- The **per-level dragon coin set** on `SaveData` is only updated on successful level exit. In-attempt dragon coin progress lives on `LevelRunState` and resets on death.
- The HUD reads from both: total-coin counter from `SaveData`, dragon-coin row from `LevelRunState` (which initializes from the saved set on level entry).

This is also the reason `SaveManager.Save()` only fires at level exit / overworld move / switch palace activation / file-select (§4.15) — there is never any persistent state that needs flushing mid-level, because mid-level state is per-attempt by definition.

### 4.25 Content Authoring (Prefabs & Data)

**Core rule: thin prefab, fat SO.** Any value that varies between variants of the same entity family lives on a ScriptableObject. The prefab is a structural shell that reads values from its assigned SO at `Awake` / `OnValidate` and applies them to components. This rule is load-bearing for two reasons: it keeps prefab regeneration safe (see below), and it pushes all tuning into `.asset` files that are stable YAML and version-control friendly.

#### Where does this value live?

| Value kind | Lives on | Examples |
|---|---|---|
| Which components are attached | Prefab | `Rigidbody2D`, `BoxCollider2D`, `SpriteRenderer`, the behavior script |
| Layer / tag | Prefab | `Enemy` layer, `Pickup` layer |
| Component settings shared across all variants | Prefab | `Rigidbody2D.gravityScale = 0`, `CollisionDetectionMode2D.Continuous` |
| Component settings that vary per variant | SO | speed, HP, collider size, sprite reference, color, drops, damage, sounds |
| Genuinely structural differences between variants | Prefab Variant | e.g. Chargin' Chuck has an extra child GameObject Goomba doesn't |
| Per-instance placement values | Scene | position, rotation, spawn direction, spawn-area marker |

If you catch yourself wanting to edit a prefab directly to tune a variant, that value belongs on the SO. Move it there, expose it in the SO's inspector, have the behavior script read it in `Awake`. Regeneration becomes safe and the prefab becomes reusable.

Example — `Enemy.cs` (sketch):
```csharp
[SerializeField] EnemyData data;
[SerializeField] BoxCollider2D col;
[SerializeField] SpriteRenderer sr;
[SerializeField] Rigidbody2D rb;

void Awake()
{
    col.size = data.colliderSize;
    sr.sprite = data.sprite;
    gameObject.layer = LayerMask.NameToLayer(data.layerName);
    rb.mass = data.mass;
    // Stats (HP, speed, stompable, fire-vulnerable, ...) stay on `data` and are
    // read by behavior methods on demand — not copied out to fields.
}
```

#### Prefab Variants — escape hatch for structural differences only

Prefer adding an SO field and branching in code (`if (data.hasChargeAttack) { … }`) over reaching for Prefab Variants. Variants are only for when the GameObject hierarchy itself must genuinely differ between variants — extra child GameObjects, different component sets, different collider arrangements.

When you do need one:
1. The base prefab (e.g. `GroundEnemy_Base.prefab`) is owned by the generator.
2. Right-click the base → **Create → Prefab Variant** → name descriptively (e.g. `ChargingChuck.prefab`).
3. Open the variant, add the structural bits.
4. **Scene placement uses the variant, never the base.** Convention: base prefabs carry the `_Base` suffix; variants don't. If you see a `_Base` name in a scene, it's a bug.
5. When the generator regenerates the base, the variant inherits the updated base and preserves its overrides.

#### Generators

One editor script per entity family: `EnemyPrefabGenerator.cs`, `PickupPrefabGenerator.cs`, `BlockPrefabGenerator.cs`, `ProjectilePrefabGenerator.cs`, etc. Each lives in `Assets/_Project/Scripts/Editor/Generators/` and exposes **two menu items**:

- `Tools → Generate → <Family> → Create Missing Only` *(safe default)*  
  Iterates the expected prefab list, skips any that already exist, creates only the missing ones. Use after a fresh clone, after adding a new entry to the generator, or after you deleted a prefab intentionally and want it back.

- `Tools → Generate → <Family> → Regenerate All (Overwrite)`  
  Shows a confirmation dialog listing every prefab that will be overwritten. Use when the generator definition itself changed (e.g. added a required component) and you want bases refreshed. Prefab Variants inherit the new base state automatically.

Both modes log a one-line summary to the console: *"Created 2, overwrote 0, skipped 6."*

**Generator contract**: declarative. The generator describes the desired prefab structure in C# (`AddComponent`, configure shared fields, assign `Sprite` / SO references via `AssetDatabase.LoadAssetAtPath`), then calls `PrefabUtility.SaveAsPrefabAsset`. Unity handles all YAML, fileIDs, GUIDs, and `.meta` files. **No hand-authored prefab YAML anywhere in this project.**

#### Folder layout

Generator-produced base prefabs and hand-created variants share the family folder:

```
Assets/_Project/Prefabs/
├── Player/
│   └── Player.prefab                 (generator-owned; single instance, not a base)
├── Enemies/
│   ├── GroundEnemy_Base.prefab       (generator-owned)
│   ├── FlyingEnemy_Base.prefab       (generator-owned)
│   ├── ShelledEnemy_Base.prefab      (generator-owned)
│   └── ChargingChuck.prefab          (Prefab Variant, hand-owned)
├── Pickups/
│   └── Pickup_Base.prefab            (generator-owned)
├── Blocks/
│   ├── Block_Question.prefab         (generator-owned)
│   ├── Block_Brick.prefab            (generator-owned)
│   └── …
├── Projectiles/
│   └── Fireball.prefab               (generator-owned)
└── VFX/
    └── …                             (feedback prefabs, see §4.23)
```

ScriptableObjects live in a parallel tree under `Assets/_Project/Data/`, authored directly (no generator — `.asset` files are stable YAML):

```
Assets/_Project/Data/
├── Enemies/
│   ├── EnemyData_Goomba.asset
│   ├── EnemyData_Koopa.asset
│   └── …
├── Pickups/
│   ├── PickupData_Coin.asset
│   └── …
├── Blocks/
│   └── BlockContents_QuestionDefault.asset
├── PowerStates/
│   ├── PowerStateData_Small.asset
│   ├── PowerStateData_Super.asset
│   ├── PowerStateData_Fire.asset
│   └── PowerStateData_Cape.asset
├── Levels/
│   ├── LevelData_01.asset
│   └── …
└── Palette.asset
```

#### Invariants

- Behavior scripts must be drivable entirely from their assigned SO. If a test needs a Goomba with half speed, it should be a new `EnemyData` variant, not a prefab edit.
- The generator output directory (`Assets/_Project/Prefabs/…`) can be deleted and `Create Missing Only` will rebuild it. That's the correctness test for this pipeline.
- Prefab `.meta` GUIDs are committed to git. Deleting and regenerating a prefab gives it a new GUID, which breaks references — use `Regenerate All (Overwrite)` instead, which preserves the existing `.meta` file and thus the GUID.

### 4.26 Debug & Test Scenes

Unity's scene authoring tools (Tilemap Palette, prefab drag-and-drop, the hierarchy panel) are the right tools for creative level design. **Content levels are hand-authored in Unity, not generated.** Generators would fight against Unity's strengths.

The exception is **debug/test scenes** — diagnostic environments used throughout development (Phase 1 through Phase 8) to validate specific systems in isolation. These are not gameplay content; they're tooling. They're generator-owned for the same reasons prefabs are: consistency, regeneratability, and the ability to re-verify behavior after tuning changes.

#### Scope boundary

| Scene kind | Source of truth | Where it lives | Authored by |
|---|---|---|---|
| **Content levels** (V1 levels 1–5, boss room, overworld, title, etc.) | The `.unity` file itself | `Assets/_Project/Scenes/Levels/`, `Assets/_Project/Scenes/Title.unity`, … | Hand, in Unity editor |
| **Debug/test scenes** | The generator script | `Assets/_Project/Scenes/Debug/` | Editor tool (`DebugSceneGenerator`) |
| **PlayMode test fixtures** (Phase 8) | The test setup code | In-memory only, no `.unity` file | `TestSceneBuilder` helper |

If a content level ever gets regenerated from a script, we're doing it wrong. If a debug scene ever gets hand-edited in Unity, we're also doing it wrong (those changes are lost on next regeneration). If a PlayMode test fixture ever gets saved to disk, we're again doing it wrong (fixtures are ephemeral by design).

#### What debug scenes are for

Examples of the kind of scene the generator produces:
- **Phase 1 Movement Test** — flat ground + platforms at stepped heights + one of each slope variant. No enemies. Used to tune jump arcs, coyote/buffer timing, and slope physics.
- **All Blocks** — one of each interactive block (`?`, brick, multi-coin brick, note, rotating, P-switch, switch-palace × 4, used) lined up left to right. Used to verify block behaviors after any §4.6 change.
- **All Enemies** — every V1 enemy variant in a wide flat corridor. Used to verify §4.7 capability interactions (stomp / fireball / cape / shell-impact) and regressions.
- **Power-up Transitions** — a corridor with every power-up pickup in sequence, enemies between them. Validates §4.3 state machine.
- **Yoshi Test** — a corridor with a Yoshi egg, a few enemies, berries, and a colored shell pickup. Validates §4.8.
- **Slope Battery** — every combination of slope direction × power state × speed × landing-surface-type. Pure physics test.

None of these are fun. They're diagnostic. They stay in the repo throughout development.

#### Generator structure

`DebugSceneGenerator.cs` in `Assets/_Project/Scripts/Editor/Generators/`. Exposes menu items per scene:

- `Tools → Generate → Debug Scenes → Phase 1 Movement Test`
- `Tools → Generate → Debug Scenes → All Blocks`
- `Tools → Generate → Debug Scenes → All Enemies`
- `Tools → Generate → Debug Scenes → Power-up Transitions`
- `Tools → Generate → Debug Scenes → Yoshi Test`
- `Tools → Generate → Debug Scenes → Slope Battery`
- `Tools → Generate → Debug Scenes → Regenerate All`

Each method creates a new scene, adds the standard level infrastructure (`LevelRoot`, tilemaps, `LevelBounds`, `LevelCamera`, spawn marker), paints the diagnostic layout via `Tilemap.SetTile` / `PrefabUtility.InstantiatePrefab`, points `LevelRoot.levelData` at a shared `LevelData_Debug.asset`, and saves to `Assets/_Project/Scenes/Debug/<Name>.unity`. The scene is also added to Build Settings under a "debug" tier (higher indices than content scenes).

Debug scenes use the same direct-entry machinery as content levels (§4.14) — press Play from any debug scene and it works, with fresh `SaveData` defaults from `EditorTestSettings`.

#### Build Settings + production stripping

Debug scenes ARE registered in Build Settings during development (required for `SceneManager.LoadScene` to find them, and for direct-Play from each to work). An `IPreprocessBuildWithReport` hook called `StripDebugScenesOnBuild` runs automatically before production builds and removes scenes under `Assets/_Project/Scenes/Debug/` from the Build Settings list so they don't ship to players. The hook is purely additive — it doesn't modify the `ProjectSettings/EditorBuildSettings.asset` file, it just filters the in-memory build list for that build.

#### Invariants

- Debug scenes are regeneratable at any time. There is no "I made a useful manual tweak to the All Enemies scene" case — tweaks go into the generator code or into the referenced prefabs/SOs.
- `Assets/_Project/Scenes/Debug/` is listed in the project's scene hygiene rules: any commit that adds a `.unity` file there must also update the generator that produced it.
- PlayMode test fixtures are a *strict subset* of debug scenes — built programmatically at test setup, torn down at teardown, never saved. They reuse the same infrastructure code as `DebugSceneGenerator` (tilemap placement helpers, spawn utilities) through a shared `SceneBuildHelpers` static class.

## 5. Game Content (V1 scope)

V1 is intentionally wider than a minimal slice — it ships every major system end-to-end so architecture issues surface during content authoring rather than after. If the project needs to ship sooner, Phase 6 is the natural cut line: at end of Phase 6 there is a single playable level → goal → overworld → next-level loop, with no Yoshi or branching.

- **1 overworld** with ~5 connected nodes (`Yoshi's Island 1`-style learning curve), including one branching path that requires the secret exit from level 5.
- **5 hand-authored content levels** (under `Assets/_Project/Scenes/Levels/`, built in the Unity editor using Tilemap Palette + prefab drag-and-drop per §4.26 — NOT generated), each focused on exercising one subsystem:
  1. Movement & jumps only — no enemies. Validates §4.2 / §4.4 tuning.
  2. Enemies (Goomba, Koopa) + shells + brick blocks. Validates §4.6 / §4.7 capability interactions.
  3. Power-ups + fire flower combat + Piranha Plants + cape feather. Validates §4.3 / cape sweep / fireball.
  4. Green Yoshi mount + tongue + swallow + dismount-on-damage. Validates §4.8. *(Colored-Yoshi variants are out of V1 scope per §4.8.)*
  5. Goal post + secret exit (via carryable key) + sub-area pipe + midway checkpoint. Validates §4.5 / §4.11 / §4.24 reset behavior.
- **1 boss room** (placeholder Bowser-shaped primitive that takes 3 stomps). Reached from the overworld via level 5's secret exit.

Levels are stored as `.unity` scenes referenced by `LevelData` ScriptableObjects. Adding a level = creating the SO + scene, no code changes.

## 6. Implementation Phases

Build order, per-phase tasks, automated tests, and manual verification lists live in [TASKS.md](TASKS.md). That file is the working checklist for development; this document stays focused on architecture. Phase headings referenced in other sections (e.g., "Phase 0 sets up …", "lands in Phase 7") all correspond to the phases defined in TASKS.md.

## 7. Open Questions

These need a decision before their phase begins; flag them when reached.

- ~~**Slopes:**~~ **Resolved:** locked to SMW's two-angle set — steep (45°) and shallow (~26.57°, the "1:2" two-tile slope). No arbitrary angles. No ceiling slopes in V1. Full sprite/tile breakdown and physics-shape details in §4.5.
- ~~**Cape flight:**~~ **Resolved:** simplified. V1 Cape Mario has a ground sweep attack and a jump-held slow-fall (25% gravity while descending). No flight take-off, no dive, no dive-rebound, no P-meter. Full SMW cape flight is explicitly out of scope and can bolt on later as a separate component if ever needed. See §4.2 for the full cape ability list.
- ~~**Save format:**~~ **Resolved:** Newtonsoft.Json. Already pulled in transitively via Eflatun.SceneReference, handles `Dictionary<,>` and enums cleanly, saves stay human-readable for debugging. See §4.15 for the full serialization rules and `saveVersion` migration chain.
- ~~**UI Toolkit vs uGUI:**~~ **Resolved:** uGUI. `com.unity.ugui` 2.0.0 is in the manifest with TMP bundled, gamepad nav via `InputSystemUIInputModule` works out of the box, and world-space canvases give us the score-popup pattern for free.
