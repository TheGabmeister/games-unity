# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Unity 6 recreation of **Mega Man X4** (PlayStation / Saturn, 1997). Follows the same conventions as the user's other game recreations in the parent `games-unity/` monorepo (SMW, FF1, etc.): mechanical fidelity first, procedural/primitive visuals (SVG via `com.unity.vectorgraphics`), audio stubbed until assets exist.

The Claude workspace is scoped to `MegamanX4/` — `.claude/settings.json` denies reads/writes outside this folder. Do not traverse up into sibling projects.

## Tooling

- **Unity 6000.3.12f1** (URP 17.3, URP 2D Renderer). See [ProjectSettings/ProjectVersion.txt](ProjectSettings/ProjectVersion.txt).
- **Packages of note** ([Packages/manifest.json](Packages/manifest.json)):
  - `com.unity.inputsystem` — new Input System; action asset at [Assets/_Project/Input/InputSystem_Actions.inputactions](Assets/_Project/Input/InputSystem_Actions.inputactions).
  - `com.unity.vectorgraphics` — SVG importer; all visual assets are `.svg` and tessellate to sprites at import.
  - `com.kyrylokuzyk.primetween` — tweening lib (via npm scoped registry); prefer it over DOTween or coroutines for tweens.
  - `com.unity.feature.2d`, `com.unity.2d.aseprite`, `com.unity.2d.spriteshape`, `com.unity.2d.tilemap.extras`.
  - `com.unity.test-framework` — EditMode/PlayMode tests.
- **Solution:** `MegamanX4.slnx`. Two assemblies: `MegamanX4.Runtime` (refs `Unity.InputSystem`, `PrimeTween.Runtime`) and `MegamanX4.Editor`.

## Build / test

Unity project — no CLI build script is checked in. Open the project in the Unity Editor (`Unity 6000.3.12f1`) to play/build. Tests are run from **Window → General → Test Runner** (EditMode and PlayMode). For a CI-style run: `Unity.exe -batchmode -projectPath <path> -runTests -testPlatform {EditMode|PlayMode}`.

Generator utilities are exposed as editor menu items under **Tools/MegamanX4/** (e.g. `Generate Buster Shot Prefabs`). Run them from the menu bar; they are idempotent and overwrite existing output.

## Layout

```
Assets/_Project/
├── Enemies/       TargetDummy.prefab
├── Input/         InputSystem_Actions.inputactions
├── Interactables/ Ladder.prefab
├── Player/
│   ├── Character/ MegamanX_{Idle,Jump,Fall,Dash}.svg
│   └── MegamanX.prefab   (Rigidbody2D + Collider2D + PlayerInput + PlayerController
│                          + Health + DamageFlash + child Visual)
├── Scenes/        Gameplay.unity
├── Scripts/
│   ├── PlayerController.cs       movement, input, sprite swap, knockback, ladder
│   ├── Health.cs                 HP, damage with source position, i-frames
│   ├── ContactDamage.cs          enemy-to-player contact damage trigger
│   ├── DamageFlash.cs            SpriteRenderer blink during i-frames
│   ├── Enemy.cs                  base enemy (Health + Depleted → Destroy)
│   ├── BusterShot.cs             current lemon runtime (pending migration per SPEC_XWEAPONS)
│   ├── Projectile.cs             composable projectile: damage + hit detection + Destroyed event
│   ├── Lifetime.cs               general-purpose auto-destroy timer
│   ├── MoveForward.cs            movement behavior: advances along transform.right
│   ├── DashSilhouetteTrail.cs    LateUpdate-driven sprite afterimage trail
│   ├── HUD.cs                    in-scene gameplay HUD root
│   ├── StageSession.cs           per-stage runtime state
│   ├── Bootstrapper.cs           [RuntimeInitializeOnLoadMethod] → loads Resources/Systems
│   ├── SystemsRoot.cs            singleton DontDestroyOnLoad root for persistent systems
│   ├── Description.cs            editor-only annotation (TextArea on any GO)
│   ├── Systems/                  GameManager, MusicManager, SfxManager, ScreenFader, SceneLoader/
│   ├── Editor/
│   │   ├── FileExtensions.cs              Project-panel extension labels
│   │   ├── BusterShotPrefabGenerator.cs   Menu: Tools/MegamanX4/…
│   │   └── MegamanX4.Editor.asmdef
│   └── MegamanX4.Runtime.asmdef
├── Settings/      URP / Renderer2D / Volume profile assets
├── UI/            GameplayHUD.prefab
└── Weapons/
    ├── Buster/        BusterShot_{Small,Semi,Full}.{svg,prefab}
    ├── DoubleCyclone/ (placeholder)
    ├── FrostTower/    FrostTower.svg
    ├── GroundHunter/  (placeholder)
    ├── RisingFire/    (placeholder)
    ├── SoulBody/      (placeholder)
    └── TwinSlasher/   TwinSlasher.svg
```

## Architecture notes

### Player movement — Kinematic + swept cast

[PlayerController.cs](Assets/_Project/Scripts/PlayerController.cs) is the one source of truth for player state (movement, jumping, dashing, wall slide/jump, knockback, ladder climb, dash-jump, sprite selection). Charge/shot logic will be extracted into a `PlayerBuster` component per [SPEC_XWEAPONS.md](SPEC_XWEAPONS.md). Key choices that the next maintainer should not accidentally undo:

- **Rigidbody2D is `Kinematic` with `gravityScale = 0`**, forced in `Awake`. Don't rely on the inspector setting — the script enforces it.
- The controller maintains its own `Vector2 velocity`, applies its own `gravity`, and resolves movement via **`Rigidbody2D.Cast` swept collision** in `MoveAxis` (one axis at a time, trim travel by `skinWidth`, zero velocity axis on contact). This is the Celeste/Hollow-Knight pattern — replacing it with physics-driven Dynamic body is a regression.
- Ground + wall contact state comes from `Probe()` (short casts from the body) run at the **start** of FixedUpdate. `isGrounded` additionally gates on `velocity.y <= 0` so the first frame of a jump isn't still "grounded". `isTouchingWall` requires the player to be actively pressing into the wall.
- `int facing` (±1) is the source of truth for direction. Never read direction back from a transform's scale — the `Visual` child's scale is a rendering detail that *follows* `facing`, not the other way around.
- **Knockback** (`knockbackTimer`, `IsKnockedBack`) gates all input: `TryJump`, `TryStartDash`, `OnAttackStarted`, facing updates, `ApplyHorizontalInput`, wall-slide clamp. Triggered by `Health.Damaged` event via `ApplyKnockback(sourcePosition)`. Charge is canceled on hit.
- **Ladder** (`onLadder`, `currentLadder`) — player snaps to ladder center X on grab, climbs via `moveInput.y * climbSpeed`. Detected via `Physics2D.OverlapBox` on a `Ladder` layer. Shoot-on-ladder halts climbing and locks facing briefly. Jump off ladder gives normal jump height + optional horizontal from stick. Damage on ladder → drop off, no knockback push.
- **Dash-jump** (`dashJumpLock`) — jumping during an active dash preserves horizontal speed for the entire airtime. Cleared on ground contact, wall-jump, ladder grab, or knockback.

### Visual child / sprite flip (characters)

The root GameObject never flips. Sprites live on a child `Visual` GameObject (drag into `PlayerController.visual`). Flip happens via `visual.localScale.x = ±Mathf.Abs(...)` in Update. This keeps colliders, `rb.Cast()` directions, and every other physical system at positive scale permanently.

**Character** SVG sprites are authored facing left and wrapped in `<g transform="translate(128 0) scale(-2 2)">` so they render facing right at 2× size. That means `facing == +1` corresponds to un-mirrored scale. If you ever author a new pose SVG, keep this wrapper convention so the existing flip math continues to work. **This is the character convention only** — projectile SVGs follow a different rule (see below).

### Projectile SVGs — authored facing right

Projectile SVGs (buster shots, Twin Slasher blade, Frost Tower pillar, etc.) are authored facing right directly — **no** flip-wrapper. "Facing right" means the leading/cutting edge is on +X: pellet head on +X with tail wisps on -X; crescent blade with convex cutting edge on +X and concave trailing side on -X. The weapon spawner sets direction by rotating the transform (not by scaling), so the canonical sprite must be in its natural right-facing form. For symmetric verticals (Frost Tower), facing doesn't matter.

### Input — PlayerInput with Invoke C# Events

`PlayerInput` component on the prefab is set to **Invoke C Sharp Events** (not Send Messages / Unity Events). Pattern used throughout:

1. Cache `InputAction` references in `Awake` via `playerInput.actions["Move"]` (etc.).
2. Subscribe to `started`/`canceled` in `OnEnable`, unsubscribe symmetrically in `OnDisable`.
3. For Value-type actions (`Move`), poll each frame with `moveAction.ReadValue<Vector2>()`.

String lookups happen once at Awake. Don't switch to Send Messages for new features.

### Health / damage system

Three decoupled concerns:

- **`Health`** — HP pool, `ApplyDamage(int amount, Vector2 sourcePosition)`, invulnerability timer (`invulnerabilityDuration`, `IsInvulnerable`), events: `Damaged(int, Vector2)`, `Healed(int)`, `HealthChanged(int, int)`, `Depleted`, `InvulnerabilityChanged(bool)`. Set `invulnerabilityDuration = 0` on entities that shouldn't have i-frames (regular enemies).
- **`DamageFlash`** — subscribes to `Health.InvulnerabilityChanged`, toggles `SpriteRenderer.enabled` at 0.08 s cadence. Uses `.enabled` (not `.color`) so it coexists with weapon-tint and charge-flash color cycling.
- **`ContactDamage`** — on enemies; calls `Health.ApplyDamage(amount, transform.position)` on Player-layer contacts.

`PlayerController` subscribes to `Health.Damaged` and calls `ApplyKnockback(sourcePosition)` (or `ExitLadder` if on a ladder). Knockback is player-only; enemies don't flinch.

### Bootstrapper / persistent systems

`Bootstrapper` uses `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` to instantiate `Resources/Systems`. `SystemsRoot` on that prefab is a singleton that calls `DontDestroyOnLoad`. This guarantees persistent systems exist regardless of which scene is loaded first (play-from-any-scene workflow). Per-stage gameplay lives in `Gameplay.unity`.

### Projectile system

Composable design: one shared `Projectile` component + a `Lifetime` timer + one movement behavior (currently `MoveForward`). Each piece has a single responsibility:

- **`Projectile`** — damage, trigger-based hit detection, `Destroyed` event. Does not move the object. Enforces Kinematic + `gravityScale = 0` in Awake. `OnTriggerEnter2D` has an explicit Environment-layer branch: **walls destroy any projectile regardless of `piercing`**. Piercing means "pass through enemies," not "pass through walls."
- **`Lifetime`** — general-purpose auto-destroy timer. Usable on any GameObject (VFX, afterimages), not just projectiles.
- **`MoveForward`** — advances along `transform.right` at a fixed speed. The spawner rotates the transform to set direction; the script reads no facing/angle fields.

**Hit filtering is handled by the Physics2D collision matrix**, not per-prefab `LayerMask` fields. Dedicated layers: `PlayerProjectile`, `PlayerProjectileNoClip`, `EnemyProjectile`. The matrix is configured so:

- `PlayerProjectile` ↔ `Enemy`, `Environment`.
- `PlayerProjectileNoClip` ↔ `Enemy` only — wall-piercing weapons (Soul Body, charged Twin Slasher) live here. Environment contacts never fire.
- `EnemyProjectile` ↔ `Player`, `Environment`.
- All other pairs involving projectile layers are disabled.

The `Projectile` script has no `hitLayers` / `hitTargets` field — the matrix is the single source of truth for layer filtering.

[BusterShot.cs](Assets/_Project/Scripts/BusterShot.cs) is still the current runtime for the basic lemon, pending the weapon-system migration tracked in [SPEC_XWEAPONS.md](SPEC_XWEAPONS.md). Its responsibilities (Kinematic + gravity 0, trigger layer check, destroy-on-hit, `Destroyed` event) map onto `Projectile` + `Lifetime` + `MoveForward` once the migration runs.

### Prefab generation (allowed)

[BusterShotPrefabGenerator.cs](Assets/_Project/Scripts/Editor/BusterShotPrefabGenerator.cs) is the template pattern: temp `new GameObject`, add components, `PrefabUtility.SaveAsPrefabAsset`, `DestroyImmediate`. Use `SerializedObject` + `FindProperty` to write private `[SerializeField]` fields without exposing them publicly.

Scripted *scene* composition remains banned (see below) — prefab generators are fine.

## Authoring conventions (important)

- **Do not script scene composition.** Content scenes, debug scenes, and test level fixtures are authored by hand in the Unity editor. Editor scripts that build scenes and save them to disk are banned — they layer C# → scene YAML on top of AssetDatabase GUID timing and have historically produced hard-to-diagnose serialization bugs. Prefab generators and ScriptableObject authoring utilities are fine and encouraged. The only exception is ephemeral PlayMode test fixtures built in-memory that are torn down at teardown — never saved.
- **Composition over inheritance.** Prefer component + ScriptableObject composition over class hierarchies for gameplay systems. `Health` + `ContactDamage` + `DamageFlash` as independent MonoBehaviours is the template, not `EnemyBase → FlyingEnemy → Bat`.
- **User prefers planning before implementation.** For any non-trivial system, produce a short plan / spec before writing code; phased roadmaps are the norm across the user's other recreations. Active specs at the project root:
  - [SPEC.md](SPEC.md) — coyote time, dash-jump, ladder climb
  - [SPEC2.md](SPEC2.md) — damage knockback + invincibility frames
  - [SPEC_XWEAPONS.md](SPEC_XWEAPONS.md) — X weapon system, PlayerBuster extraction, WeaponData SO, BusterShot migration
  - [SPEC_PROJECTILES.md](SPEC_PROJECTILES.md) — composable projectile system, layer-based filtering, environment-destroys-on-contact
- **No asset dependencies until explicitly added.** Procedural SVG visuals, stubbed audio (enum-keyed `SfxId`/`MusicId` resolved via a ScriptableObject catalog) until real assets land. Gameplay code should not reference `AudioClip` directly.
- **ScriptableObjects for game data** (enemy stats, weapon data, level metadata, palettes). Prefer SO-driven configuration over hard-coded constants for anything designers would tune.

## Small gotchas worth remembering

- Renaming an `.svg` always renames its `.svg.meta` alongside it — the meta holds the GUID that prefabs reference. Use `mv src.svg dst.svg && mv src.svg.meta dst.svg.meta` (or the Unity Project panel).
- SVG sprite-sheets are not a thing with `com.unity.vectorgraphics` — one `.svg` = one `Sprite`. Author multiple poses as separate files and swap `SpriteRenderer.sprite`.
- When spawning projectiles, pass the position to `Instantiate(prefab, pos, rot)` directly — not via a setter after. Colliders activate in `Awake` before any post-instantiate method runs, so a wrong initial position can fire bogus trigger events.
- New projectile prefabs go on layer `PlayerProjectile` (default), `PlayerProjectileNoClip` (wall-piercers like Soul Body or charged Twin Slasher), or `EnemyProjectile`. The `Projectile` script carries no `LayerMask` field — the Physics2D collision matrix gates which pairs ever fire `OnTriggerEnter2D`.
- Environment collisions destroy any projectile in the matrix-allowed set, regardless of `piercing`. To truly pass through walls, put the projectile on `PlayerProjectileNoClip` — the matrix disables the pair, so the callback never fires.
- Projectile SVGs face right by default (leading/cutting edge on +X). Character SVGs use the opposite convention (authored facing left, flip-wrapped). Don't mix them up.
