# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Unity 6 recreation of **Mega Man X4** (PlayStation / Saturn, 1997). Follows the same conventions as the user's other game recreations in the parent `games-unity/` monorepo (SMW, FF1, etc.): mechanical fidelity first, procedural/primitive visuals (SVG via `com.unity.vectorgraphics`), audio stubbed until assets exist.

The Claude workspace is scoped to `MegamanX4/` — `.claude/settings.json` denies reads/writes outside this folder. Do not traverse up into sibling projects.

See [README.md](README.md) for tooling, build / test instructions, and the `Assets/_Project/` layout.

## Architecture notes

### Player movement — Kinematic + swept cast

[PlayerController.cs](Assets/_Project/Scripts/Player/PlayerController.cs) is the one source of truth for player state (movement, jumping, dashing, wall slide/jump, knockback, ladder climb, dash-jump, sprite selection). Charge/shot logic + weapon switching live in [WeaponInventory.cs](Assets/_Project/Scripts/Player/WeaponInventory.cs) — the controller delegates `OnAttackStarted`/`OnAttackCanceled` to it and calls `_inventory.CancelCharge()` from `ApplyKnockback`. Per-weapon data lives in [WeaponData.cs](Assets/_Project/Scripts/Player/WeaponData.cs) ScriptableObject assets. Key choices that the next maintainer should not accidentally undo:

- **Rigidbody2D is `Kinematic` with `gravityScale = 0`**, forced in `Awake`. Don't rely on the inspector setting — the script enforces it.
- The controller maintains its own `Vector2 velocity`, applies its own `gravity`, and resolves movement via **`Rigidbody2D.Cast` swept collision** in `MoveAxis` (one axis at a time, trim travel by `skinWidth`, zero velocity axis on contact). This is the Celeste/Hollow-Knight pattern — replacing it with physics-driven Dynamic body is a regression.
- Ground + wall contact state comes from `Probe()` (short casts from the body) run at the **start** of FixedUpdate. `isGrounded` additionally gates on `velocity.y <= 0` so the first frame of a jump isn't still "grounded". `isTouchingWall` requires the player to be actively pressing into the wall.
- `int _facing` (±1) is the source of truth for direction, exposed publicly as `Facing`. Never read direction back from a transform's rotation or scale — the `Visual` child's rotation is a rendering detail that *follows* `_facing`, not the other way around.
- **Knockback** (`_knockbackTimer`, public `IsKnockedBack`) gates all input: `TryJump`, `TryStartDash`, `OnAttackStarted`, facing updates, `ApplyHorizontalInput`, wall-slide clamp. Triggered by `Health.Damaged` event via `ApplyKnockback(sourcePosition)`. Charge is canceled on hit.
- **Ladder** (`_onLadder`, `_currentLadder`) — player snaps to ladder center X on grab, climbs via `_moveInput.y * _climbSpeed`. Detected via `Physics2D.OverlapBox` on a `Ladder` layer. Shoot-on-ladder halts climbing and locks facing briefly. Jump off ladder gives normal jump height + optional horizontal from stick. Damage on ladder → drop off, no knockback push.
- **Dash-jump** (`_dashJumpLock`) — jumping during an active dash preserves horizontal speed for the entire airtime. Cleared on ground contact, wall-jump, ladder grab, or knockback.

### Visual child / sprite flip (characters)

The root GameObject never flips. Sprites live on a child `Visual` GameObject (drag into `PlayerController._visual`). Flip happens via `_visual.localRotation = _facing >= 0 ? Quaternion.identity : Quaternion.Euler(0, 180, 0)` in Update — a 180° rotation around Y mirrors the quad horizontally. This keeps colliders, `rb.Cast()` directions, and every other physical system at positive scale permanently. It also means the muzzle anchor (parented under `Visual`) has its world rotation flipped with the character, which is how projectile firing direction flows naturally from the muzzle's transform — no facing parameter passed down.

**Character** SVG sprites are authored facing left and wrapped in `<g transform="translate(128 0) scale(-2 2)">` so they render facing right at 2× size. That means `_facing == +1` corresponds to un-rotated visual, and flipping the `Visual`'s Y-rotation mirrors the wrapped sprite back to facing left when `_facing == -1`. If you ever author a new pose SVG, keep this wrapper convention so the flip math continues to work. **This is the character convention only** — projectile SVGs follow a different rule (see below).

### Projectile SVGs — authored facing right

Projectile SVGs (buster shots, Twin Slasher blade, Frost Tower pillar, etc.) are authored facing right directly — **no** flip-wrapper. "Facing right" means the leading/cutting edge is on +X: pellet head on +X with tail wisps on -X; crescent blade with convex cutting edge on +X and concave trailing side on -X. The weapon spawner sets direction by rotating the transform (not by scaling), so the canonical sprite must be in its natural right-facing form. For symmetric verticals (Frost Tower), facing doesn't matter.

### Enemy SVGs — same invariant (facing right)

Enemy SVGs follow the projectile rule: positive X = canonical render direction. This keeps `_facing`, `localScale.x`, `transform.right`, and muzzle orientation all pointing the same way across every moving entity. The first batch of enemies (Knot Beret B/G, Kyunnbyunn, Mad Bull 97, Tonboroid S, Trap Blast) were originally authored facing left and are grandfathered via a `<g transform="translate(W 0) scale(-1 1)">` flip-wrapper for GUID stability — **new enemy SVGs should be drawn facing right directly**, no wrapper. Enemies that should face left in a scene are flipped at prefab/scene level via `localScale.x = -1` (or `PatrolWalk._initialFacing = -1`), not by re-authoring the SVG.

### Input — PlayerInput with Invoke C# Events

`PlayerInput` component on the prefab is set to **Invoke C Sharp Events** (not Send Messages / Unity Events). Pattern used throughout:

1. Cache `InputAction` references in `Awake` via `playerInput.actions["Move"]` (etc.).
2. Subscribe to `started`/`canceled` in `OnEnable`, unsubscribe symmetrically in `OnDisable`.
3. For Value-type actions (`Move`), poll each frame with `moveAction.ReadValue<Vector2>()`.

String lookups happen once at Awake. Don't switch to Send Messages for new features.

Actions in [InputSystem_Actions.inputactions](Assets/_Project/Input/InputSystem_Actions.inputactions) that gameplay code subscribes to today: `Move`, `Jump`, `Sprint`, `Attack`, `WeaponNext` (E / RB), `WeaponPrev` (Q / LB). `PlayerController` owns subscription for movement + attack + jump and delegates to `WeaponInventory`; `WeaponInventory` subscribes directly to `WeaponNext` / `WeaponPrev`.

### Weapon system

[WeaponInventory.cs](Assets/_Project/Scripts/Player/WeaponInventory.cs) owns both the weapon list and the universal charge state machine (previously split across a separate `PlayerBuster` component that has been absorbed). Shape:

- Serialized `List<WeaponData> _weapons` — slot 0 is the buster.
- `_activeIndex` cycles with Q/E. On swap: `CancelCharge()` zeroes any in-progress charge, then `ActiveWeapon.tint` is written to the player's `SpriteRenderer.color`.
- Hold-Attack/release-Attack works for every weapon. `_chargeTimer` ticks while `_isCharging`, charge-flash visuals play during semi/full thresholds, and `RestoreColor` returns the sprite to `ActiveWeapon.tint` (not white) on release.
- On `ReleaseCharge`, the spawner picks from `ActiveWeapon.fullPrefab` / `semiPrefab` / `smallPrefab` based on charge timer. Instantiation is one call: `Instantiate(prefab, muzzle.position, muzzle.rotation)`. Direction flows entirely from the muzzle transform.
- The 3-lemon on-screen cap (`_activeSmallShots` tracked via `Projectile.Destroyed`) only applies to the buster slot (`_activeIndex == 0`). Special weapons currently have no cap.
- **Per-weapon energy pools + depletion auto-switch.** `WeaponInventory` owns an `int[] _energy` array parallel to `_weapons`, seeded from each `WeaponData.maxEnergy` in Awake (so fresh-instantiate = refill). On fire, non-buster weapons debit their tier cost (`smallCost` / `semiCost` / `fullCost`); if the pool empties, `_activeIndex` snaps back to 0 (buster) and `ActiveWeaponChanged` fires. `maxEnergy == 0` = infinite (buster, or any unauthored non-buster weapon).
- Events exposed: `EnergyChanged` (after debit), `ActiveWeaponChanged` (on Q/E swap or auto-switch). Getters: `ActiveIndex`, `GetEnergy(i)`, `GetMaxEnergy(i)`. HUD subscribes to these.

`WeaponData` (ScriptableObject) shape: `displayName`, `tint`, `smallPrefab`/`semiPrefab`/`fullPrefab`, plus energy block (`maxEnergy`, `smallCost`, `semiCost`, `fullCost`). **Convention for special weapons:** set `semiPrefab = smallPrefab` so a semi-charge release fires the same projectile as a tap (specials don't have a middle tier). `fullPrefab` can either match `smallPrefab` (no charged variant) or be distinct (charged variant for that weapon) — author's call per asset.

### Health / damage system

Three decoupled concerns:

- **`Health`** — HP pool, `ApplyDamage(int amount, Vector2 sourcePosition)`, invulnerability timer (`invulnerabilityDuration`, `IsInvulnerable`), events: `Damaged(int, Vector2)`, `Healed(int)`, `HealthChanged(int, int)`, `Depleted`, `InvulnerabilityChanged(bool)`. Set `invulnerabilityDuration = 0` on entities that shouldn't have i-frames (regular enemies).
- **`InvulnerabilityBlinker`** — subscribes to `Health.InvulnerabilityChanged`, toggles `SpriteRenderer.enabled` at 0.08 s cadence. Uses `.enabled` (not `.color`) so it coexists with weapon-tint and charge-flash color cycling. Used on the player (i-frame visibility gating); not on enemies.
- **`DamageFlash`** — subscribes to `Health.Damaged`, briefly overrides `SpriteRenderer.color` to white (default `_flashDuration = 0.1f`), then restores the prior color. Used on enemies and destructibles so hits read visually even without i-frames. Don't put on the player — it would fight the weapon tint and charge-flash color cycling.
- **`HitBox`** — on enemies or any damage source; on trigger/collision overlap, finds `HurtBox` on the other collider and calls `hurtBox.ReceiveHit(damage, transform.position)`. No hardcoded layer checks — the Physics2D collision matrix controls which pairs interact.
- **`HurtBox`** — on any entity that can receive damage (player, enemies). Caches `Health` via `GetComponentInParent<Health>()` in Awake. `ReceiveHit(int damage, Vector2 sourcePosition)` forwards to `Health.ApplyDamage`.

`PlayerController` subscribes to `Health.Damaged` and calls `ApplyKnockback(sourcePosition)` (or `ExitLadder` if on a ladder). Knockback is player-only; enemies don't flinch.

### HUD — pure view, event-driven, Bind-injected

[HUD.cs](Assets/_Project/Scripts/HUD.cs) is a view-only component on `GameplayHUD.prefab`. It subscribes to `Health.HealthChanged` and to `WeaponInventory.EnergyChanged` / `ActiveWeaponChanged`; no `Update` polling. [StageSession.SpawnPlayer](Assets/_Project/Scripts/StageSession.cs) instantiates the player + HUD prefabs, then calls `hud.Bind(health, weaponInventory)` to wire references. This avoids `GameObject.Find` and keeps the HUD decoupled from any singleton. If either component is missing on the player, `Bind` logs a warning and silently renders nothing for that bar. The Energy bar GameObject is toggled off whenever `ActiveWeapon.maxEnergy == 0` (buster has infinite energy → no bar shown).

### Bootstrapper / persistent systems

`Bootstrapper` uses `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` to instantiate `Resources/Systems`. `SystemsRoot` on that prefab is a singleton that calls `DontDestroyOnLoad`. This guarantees persistent systems exist regardless of which scene is loaded first (play-from-any-scene workflow). Per-stage gameplay lives in `Gameplay.unity`.

### Projectile system

Composable design: one shared `Projectile` component + a `Lifetime` timer + one movement behavior (currently `MoveForward`). Each piece has a single responsibility:

- **`Projectile`** — lifecycle only: `Destroyed` event, wall collision, piercing self-destruct. Does not deal damage itself — a sibling `HitBox` component handles that. Enforces Kinematic + `gravityScale = 0` in Awake. `OnTriggerEnter2D` has an explicit Environment-layer branch: **walls destroy any projectile regardless of `piercing`**. Piercing means "pass through enemies," not "pass through walls."
- **`Lifetime`** — general-purpose auto-destroy timer. Usable on any GameObject (VFX, afterimages), not just projectiles.
- **`MoveForward`** — advances along `transform.right` at a fixed speed. The spawner rotates the transform to set direction; the script reads no facing/angle fields.

**Hit filtering is handled by the Physics2D collision matrix**, not per-prefab `LayerMask` fields. Dedicated layers: `PlayerProjectile`, `PlayerProjectileNoClip`, `EnemyProjectile`. The matrix is configured so:

- `PlayerProjectile` ↔ `Enemy`, `Environment`.
- `PlayerProjectileNoClip` ↔ `Enemy` only — wall-piercing weapons (Soul Body, charged Twin Slasher) live here. Environment contacts never fire.
- `EnemyProjectile` ↔ `Player`, `Environment`.
- All other pairs involving projectile layers are disabled.

The `Projectile` script has no `hitLayers` / `hitTargets` field — the matrix is the single source of truth for layer filtering.

The basic lemon has migrated onto this system: the `BusterShot_{Small,Semi,Full}.prefab` assets compose `HitBox` + `Projectile` + `Lifetime` + `MoveForward`, and [WeaponInventory.cs](Assets/_Project/Scripts/Player/WeaponInventory.cs) spawns them at `muzzle.position` / `muzzle.rotation` — direction flows from the muzzle's world rotation, which itself is driven by `Visual`'s Y-flip when facing changes. No `BusterShot.cs` component exists anymore; no facing parameter is passed to projectiles.

### Enemy system — composition + generator

Enemies are **composition-first**: no inheritance hierarchy. Every enemy is a prefab combining small single-responsibility components from [Assets/_Project/Scripts/Enemy/](Assets/_Project/Scripts/Enemy/), [Assets/_Project/Scripts/Behavior/](Assets/_Project/Scripts/Behavior/), and [Assets/_Project/Scripts/Damage/](Assets/_Project/Scripts/Damage/). The full roster and roadmap lives in [SPEC_ENEMIES.md](SPEC_ENEMIES.md).

**Standard composition core** (shorthand `(core)` in the spec): `DestroyOnDepleted` + `Health` + `HurtBox` + `HitBox` + `DamageFlash`. Add gravity + ground-walk via `Gravity` + `PatrolWalk`. Add flying/hovering via `HoverSine` + optionally `SwoopAttack` (with `PlayerDetector`). Add shooting via `EnemyShoot` (aimed, detection-gated) or `AutoShoot` (fire-and-forget, unaimed).

**`PlayerDetector`** is radial by default (`Physics2D.OverlapCircle` on the Player layer in `FixedUpdate`), with an optional LoS raycast against Environment. Fires edge-triggered `PlayerDetected`/`PlayerLost` *and* exposes `CanSeePlayer`/`PlayerPosition` for polling — consumers like `EnemyShoot` and `SwoopAttack` poll so they can re-trigger after cooldowns instead of being one-shot on the edge event.

**Direction**: `PatrolWalk` flips the enemy's root `localScale.x = facing` to mirror the sprite. This is acceptable for enemies (unlike the player, they don't use swept-cast movement, and their colliders are symmetric around center). Muzzle direction still flows from `muzzle.rotation`, not scale — so spawners pass `muzzle.position, muzzle.rotation` to `Instantiate` exactly like the player's weapon system.

**Two-tier rule** (per [SPEC_ENEMIES.md](SPEC_ENEMIES.md) scalability review): new behavior becomes a reusable component only at **3+ genuine uses**. Below that threshold, write a single-use AI script named after the enemy (e.g. `Mettaur.cs`, `BattonBone.cs`) that lives in `Assets/_Project/Scripts/Enemy/AI/`. A single-file state machine per enemy beats event-coupled micro-components that each serve one caller.

**Generator**: [SkyLagoonEnemyGenerator.cs](Assets/_Project/Scripts/Editor/SkyLagoonEnemyGenerator.cs) is the canonical example. It exposes `Tools/MegamanX4/Generate Sky Lagoon Enemies`. Pattern: `NewEnemyRoot(name, hp, contact, isTrigger)` builds the standard core; each per-enemy `GenerateX()` method adds enemy-specific behaviors via `go.AddComponent<T>() + SetField(c, "_fieldName", value)`. `SetField` uses `SerializedObject.FindProperty` so private `[SerializeField]` fields stay private. SVG sprites load via `AssetDatabase.LoadAssetAtPath<Sprite>(...)`. Material: `Packages/com.unity.vectorgraphics/Runtime/Materials/Unlit_Vector.mat` is assigned to every `SpriteRenderer` so SVG tessellation renders correctly.

As the roster grows, split the monolithic generator into per-stage files + a shared `EnemyGeneratorCore.cs` (see spec §Scalability).

### Prefab generation (allowed)

When a generator is needed, the pattern is: temp `new GameObject`, add components, `PrefabUtility.SaveAsPrefabAsset`, `DestroyImmediate`. Use `SerializedObject` + `FindProperty("_fieldName")` (note the underscore prefix) to write private `[SerializeField]` fields without exposing them publicly.

Scripted *scene* composition remains banned (see below) — prefab generators are fine.

## Authoring conventions (important)

- **Do not script scene composition.** Content scenes, debug scenes, and test level fixtures are authored by hand in the Unity editor. Editor scripts that build scenes and save them to disk are banned — they layer C# → scene YAML on top of AssetDatabase GUID timing and have historically produced hard-to-diagnose serialization bugs. Prefab generators and ScriptableObject authoring utilities are fine and encouraged. The only exception is ephemeral PlayMode test fixtures built in-memory that are torn down at teardown — never saved.
- **Composition over inheritance.** Prefer component + ScriptableObject composition over class hierarchies for gameplay systems. `Health` + `HurtBox` + `HitBox` + `DamageFlash` + `InvulnerabilityBlinker` as independent MonoBehaviours is the template, not `EnemyBase → FlyingEnemy → Bat`.
- **Private field naming: underscore prefix.** Class-level private fields use `_camelCase` (including `[SerializeField]` fields): `_rb`, `_facing`, `[SerializeField] int _maxHealth`. Public properties and methods stay PascalCase (`IsKnockedBack`, `ApplyKnockback`). `const` and `static readonly` stay PascalCase too. Local variables and parameters stay plain (no underscore). When editing an existing file that doesn't yet follow this, rename its private fields to match while you're there.
- **User prefers planning before implementation.** For any non-trivial system, produce a short plan / spec before writing code; phased roadmaps are the norm across the user's other recreations. Active specs at the project root:
  - [SPEC_HUD.md](SPEC_HUD.md) — HP + active-weapon-energy bars, event-driven view, `Bind`-injected
  - [SPEC_ENEMIES.md](SPEC_ENEMIES.md) — full-campaign enemy roster (40 non-boss enemies across Sky Lagoon + 8 Maverick stages). Phase 1 (Sky Lagoon, 7 enemies) is implemented. Later phases use the two-tier rule (reusable component vs single-use AI script) to keep the component library small. King Poseidon deferred pending a water system; Metal Gabyoall shipped as floor-only until surface-crawl is revisited.
- **No asset dependencies until explicitly added.** Procedural SVG visuals, stubbed audio (enum-keyed `SfxId`/`MusicId` resolved via a ScriptableObject catalog) until real assets land. Gameplay code should not reference `AudioClip` directly.
- **ScriptableObjects for game data** (enemy stats, weapon data, level metadata, palettes). Prefer SO-driven configuration over hard-coded constants for anything designers would tune.

## Physics collision gotchas

When debugging "X collides with Y unexpectedly" or "X doesn't damage Y", check in this order:

1. **`PlayerController._environmentLayers` (serialized `LayerMask`) overrides the physics matrix for the player's movement.** The player doesn't use the Physics2D layer matrix for its own swept cast — it uses `Rigidbody2D.Cast` with a `ContactFilter2D` whose mask is `_environmentLayers`. Default `~0` (all layers) causes the player to physically collide with projectiles/enemies/everything. Keep this narrow (`Environment` only, optionally `Ladder`).
2. **Rigidbody2D is required for trigger/collision callbacks.** A projectile prefab with `MoveForward` but no `Rigidbody2D` silently fires no `OnTriggerEnter2D` / `OnCollisionEnter2D` — Unity treats it as a static collider being teleported. Always add `Rigidbody2D` (Kinematic, gravity 0) to anything that moves and needs to trigger damage. `Projectile` enforces this in `Awake` but only if the component is on the prefab.
3. **Trigger vs non-trigger is per-use-case, not per-component.** `HitBox` handles both `OnTriggerEnter2D` and `OnCollisionEnter2D`. Trigger = other object passes through (projectiles). Non-trigger = physical interaction (Frost Tower ice pillar, which physically blocks enemies *and* gets blocked by walls).
4. **Layer index access goes through [Layers.cs](Assets/_Project/Scripts/Layers.cs)**, a hand-written mirror of `ProjectSettings/TagManager.asset`. Do not use `LayerMask.NameToLayer("…")` — it fails silently on typos. If you reorder layers in the TagManager, update `Layers.cs` to match (the compiler won't catch drift since the values are just `int`).
5. **Physics2D Layer Collision Matrix** (Edit → Project Settings → Physics 2D) governs which pairs *can* interact at all. Key enabled pairs: `Player ↔ Enemy`, `Player ↔ Environment`, `PlayerProjectile ↔ Enemy/Environment`, `EnemyProjectile ↔ Player/Environment`. `Player ↔ PlayerProjectile*` must be disabled.

## Small gotchas worth remembering

- Renaming an `.svg` always renames its `.svg.meta` alongside it — the meta holds the GUID that prefabs reference. Use `mv src.svg dst.svg && mv src.svg.meta dst.svg.meta` (or the Unity Project panel).
- SVG sprite-sheets are not a thing with `com.unity.vectorgraphics` — one `.svg` = one `Sprite`. Author multiple poses as separate files and swap `SpriteRenderer.sprite`.
- When spawning projectiles, pass the position to `Instantiate(prefab, pos, rot)` directly — not via a setter after. Colliders activate in `Awake` before any post-instantiate method runs, so a wrong initial position can fire bogus trigger events.
- New projectile prefabs go on layer `PlayerProjectile` (default), `PlayerProjectileNoClip` (wall-piercers like Soul Body or charged Twin Slasher), or `EnemyProjectile`. The `Projectile` script carries no `LayerMask` field — the Physics2D collision matrix gates which pairs ever fire `OnTriggerEnter2D`.
- Environment collisions destroy any projectile in the matrix-allowed set, regardless of `piercing`. To truly pass through walls, put the projectile on `PlayerProjectileNoClip` — the matrix disables the pair, so the callback never fires.
- **SVG facing rule**: projectiles and (new) enemies authored facing right directly. Character SVGs (MegamanX_*) are authored facing left and flip-wrapped via `<g transform="translate(128 0) scale(-2 2)">` so they render right at 2× size. The first batch of enemies (Knot Beret B/G, Kyunnbyunn, Mad Bull 97, Tonboroid S, Trap Blast) are grandfathered as flip-wrapped too — but new enemy SVGs should be drawn right directly, no wrapper. Don't mix conventions in new work.
