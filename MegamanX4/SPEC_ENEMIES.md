# SPEC_ENEMIES

## Context

Covers every regular (non-boss, non-sub-boss) enemy in Mega Man X4, across the intro stage (Sky Lagoon) and the eight Maverick stages. Goal: one spec covering all 40 enemies so component reuse, stat balance, and generator scope can be planned end-to-end rather than stage-by-stage.

Phase 1 (Sky Lagoon, 7 enemies) is already implemented. This spec treats those as reference entries — behavior is frozen, stats scaled to the new HP/damage range (player HP 100, default enemy HP 10).

**Scope exclusions:** all bosses (Eregion, Web Spider, Cyber Peacock, Storm Owl, Magma Dragoon, Jet Stingray, Slash Beast, Frost Walrus, Split Mushroom) and all sub-bosses (Generaid Core, DG-42L, Eyezard, Tentoroid). These are set-piece encounters and don't share the component-composition pattern used for regular enemies.

**Deferred:** King Poseidon (needs a water system that doesn't exist yet). Metal Gabyoall's full wall/ceiling crawl (shipped as floor-only `PatrolWalk` for now; revisit later if stage layout demands surface crawling).

## Stat scale reference

Player X has 100 HP. Buster: tap ~5 dmg, semi-charge ~15, full-charge ~30. Special weapons scale up to ~40.

| Tier | HP | Typical contact | Typical shot |
|------|----|-----------------|--------------|
| Chaff (1-shot) | 5–10 | 5–10 | — |
| Grunt (2–3 shots) | 15–25 | 10–15 | 5–10 |
| Heavy (4–6 shots) | 30–50 | 15–25 | 10–20 |
| Tank (8+ shots) | 60–120 | 25–40 | 15–30 |
| Invulnerable | ∞ | 20–30 | 10–25 |

## Existing infrastructure

18 components ship today, grouped by folder.

**Enemy** ([Assets/_Project/Scripts/Enemy/](Assets/_Project/Scripts/Enemy/))

| Component | Role |
|-----------|------|
| `DestroyOnDepleted` | Lifecycle: subscribes to `Health.Depleted` → destroys GameObject (reusable across enemies + destructibles) |
| `PlayerDetector` | Range-based player detection (radial `OverlapCircle`) with optional LoS raycast; fires `PlayerDetected`/`PlayerLost`; exposes `CanSeePlayer`, `PlayerPosition` |
| `PatrolWalk` | Walk at `_speed`, flip on wall/ledge raycast, pausable, flips root `localScale.x` to face |
| `EnemyShoot` | Polls `CanSeePlayer`, aims muzzle, fires burst with cooldown; pauses sibling `PatrolWalk` during burst |
| `AutoShoot` | Fire-and-forget: spawn projectile at `_interval`, no detection, muzzle direction is the shot direction |
| `SwoopAttack` | State machine Idle/Diving/Returning/Cooldown; dives to captured player position, returns, resumes `HoverSine` |
| `DropTrigger` | Overlap-below detection; on trigger: enable Dynamic RB + gravity, enable paired HurtBox |
| `DestroyOnWallContact` | `OnCollisionEnter2D`/`OnTriggerEnter2D` with Environment → Destroy |

**Behavior** ([Assets/_Project/Scripts/Behavior/](Assets/_Project/Scripts/Behavior/))

| Component | Role |
|-----------|------|
| `MoveForward` | Translate along `transform.right` at fixed speed |
| `MoveVertical` | Translate along ±`transform.up` at fixed speed |
| `HoverSine` | Sine-wave Y oscillation around a recorded center; pausable; `SetCenter(y)` for dive-return reset |
| `Gravity` | Kinematic downward pull with ground raycast + max fall speed clamp |
| `Lifetime` | Auto-destroy timer |

**Damage** ([Assets/_Project/Scripts/Damage/](Assets/_Project/Scripts/Damage/))

| Component | Role |
|-----------|------|
| `Health` | HP pool, i-frames, `Damaged`/`Depleted`/`HealthChanged`/`InvulnerabilityChanged` events |
| `HitBox` | On contact (trigger or collision) calls `HurtBox.ReceiveHit` with `_damage` |
| `HurtBox` | Routes hits to parent `Health.ApplyDamage` |
| `InvulnerabilityBlinker` | Blinks `SpriteRenderer.enabled` during i-frames (player-style hit gating) |
| `DamageFlash` | On `Health.Damaged`, briefly sets `SpriteRenderer.color` to white; restores on timeout |

Layers (see [Layers.cs](Assets/_Project/Scripts/Layers.cs)): `Player`, `Environment`, `Enemy`, `Ladder`, `PlayerProjectile`, `PlayerProjectileNoClip`, `EnemyProjectile`. Physics2D matrix already routes `EnemyProjectile ↔ Player, Environment` and `PlayerProjectile ↔ Enemy, Environment`.

**Conventions** (see [CLAUDE.md](CLAUDE.md)): SVGs authored facing right, either natively or via flip-wrapper for grandfathered assets; all rotations flow from the root/muzzle `transform`, never from `localScale.x` flips for direction math; gravity is custom for AI-driven enemies (via `Gravity`), physics-driven only for pure hazards (Spike Marl post-drop). Standard enemy composition core: `DestroyOnDepleted` + `Health` + `HurtBox` + `HitBox` + `DamageFlash` — shorthand `(core)` below.

---

## Enemy roster — Recurring

Enemies that appear in multiple stages. Listed first because they form the backbone of the bestiary.

### 1. Knot Beret B (blue soldier) *(Sky Lagoon, Military Train)*

**Behavior:** Most common enemy. Walks along platforms on patrol. When the player enters range, stops, faces the player, and fires a 3-round burst. Resumes patrol after. Falls off ledges if not edge-guarded.

| Stat | Value |
|------|-------|
| HP | 15 |
| Contact damage | 10 |
| Shot damage | 8 |
| Detection range | ~8 units |
| Patrol speed | ~2 u/s |

**Composition:** (core) + `Gravity` + `PatrolWalk` + `PlayerDetector` + `EnemyShoot` (burst=3).

### 2. Knot Beret G (green soldier) *(Sky Lagoon, Military Train)*

**Behavior:** Stationary variant. Stands in place, faces the player when detected, fires a single aimed shot on cooldown.

| Stat | Value |
|------|-------|
| HP | 15 |
| Contact damage | 10 |
| Shot damage | 10 |
| Detection range | ~10 units |

**Composition:** (core) + `Gravity` + `PlayerDetector` + `EnemyShoot` (burst=1, longer cooldown).

### 3. Spike Marl (ceiling mine) *(Sky Lagoon, Cyber Space)*

**Behavior:** Ceiling-attached spiked ball. Inert until player passes below, then detaches and drops straight down. High contact damage. Destroyed on ground impact.

| Stat | Value |
|------|-------|
| HP (post-drop) | 10 |
| Contact damage | 30 |
| Trigger range below | ~2 units × 20 |

**Composition:** `DestroyOnDepleted` + `Health` + `HurtBox` (disabled until drop) + `HitBox` + `DamageFlash` + `DropTrigger`. Post-drop: `Rigidbody2D` switches to Dynamic, `gravityScale = 3`.

### 4. Kyunnbyunn (swooping bird) *(Sky Lagoon, Jungle)*

**Behavior:** Flies horizontally in a sine-wave pattern across the screen. Does not turn — flies once and despawns. Often spawns in groups of 2–3.

| Stat | Value |
|------|-------|
| HP | 5 |
| Contact damage | 10 |
| Fly speed | ~6 u/s |
| Wave amplitude | ~1.5 units |
| Wave frequency | ~3 Hz |

**Composition:** (core) + `MoveForward` + `HoverSine` + `Lifetime` (10s).

### 5. Blast Raster *(Jungle, Bio Laboratory)*

**Behavior:** Floating spherical enemy. Hovers in place. Periodically fires shots radially in 4 or 8 directions (cardinal/diagonal). Chaff-level HP but awkward to approach due to spread fire.

| Stat | Value |
|------|-------|
| HP | 10 |
| Contact damage | 10 |
| Shot damage | 8 |
| Fire interval | ~2.5 s |

**Composition:** (core) + `HoverSine` (low amplitude) + `RadialShoot` *(new reusable)*.

### 6. Hover Gunner *(Cyber Space, Marine Base, Bio Laboratory)*

**Behavior:** Helicopter-drone. Hovers at fixed altitude, tracks the player horizontally, fires aimed shots down-forward at the player on cooldown.

| Stat | Value |
|------|-------|
| HP | 25 |
| Contact damage | 15 |
| Shot damage | 10 |
| Track speed | ~3 u/s |

**Composition:** (core) + `HoverSine` + `TrackPlayer` *(new reusable, X-axis)* + `PlayerDetector` + `EnemyShoot`.

### 7. Giga Death *(Air Force, Volcano)*

**Behavior:** Large floating turret. Drifts slowly from above; fires heavy shots downward at the player. High HP, infrequent fire.

| Stat | Value |
|------|-------|
| HP | 50 |
| Contact damage | 25 |
| Shot damage | 20 |
| Fire interval | ~3 s |

**Composition:** (core) + `MoveVertical` (slow down) + `TrackPlayer` (X-axis, slow) + `AutoShoot` (aimed muzzle).

### 8. Plasma Cannon *(Air Force, Military Train)*

**Behavior:** Wall-mounted cannon. Charges a plasma beam with visible telegraph, then fires a sustained beam across the screen for ~1 s. Invulnerable.

| Stat | Value |
|------|-------|
| HP | ∞ |
| Contact damage | — |
| Beam damage | 20 (per tick) |
| Cycle | ~3 s |

**Composition:** `HitBox` + `PlasmaCannon.cs` *(single-use script — charge/fire/cooldown state machine + beam collider toggle)*. No `DestroyOnDepleted`/`Health`.

### 9. Batton Bone B81 *(Volcano, Military Train, Bio Laboratory)*

**Behavior:** Classic bat. Hangs upside-down on ceiling, inert. When the player approaches, activates: drops briefly, then flies in a zigzag sine path toward the player. Does not return.

| Stat | Value |
|------|-------|
| HP | 10 |
| Contact damage | 15 |
| Detection range | ~6 units |
| Fly speed | ~4 u/s |

**Composition:** (core) + `PlayerDetector` + `BattonBone.cs` *(single-use — sleep → drop → fly. Directly drives position, can delegate to `MoveForward`+`HoverSine` after activation if clean.)*.

### 10. Mettaur D2 *(Volcano, Military Train, Snow Base)*

**Behavior:** Hides under armored hat (invulnerable). Peeks out on cooldown, fires a 3-way spread (down-left, down, down-right), returns to hiding. Small and slow.

| Stat | Value |
|------|-------|
| HP (peeking) | 10 |
| Contact damage | 10 |
| Shot damage | 8 |
| Hide/peek cycle | ~2 s |

**Composition:** (core) + `Mettaur.cs` *(single-use — hide/peek state machine, toggles sibling `HurtBox.enabled`, triggers fire)* + `SpreadShoot` (3-way).

### 11. Spiky Mk-II *(Volcano, Bio Laboratory)*

**Behavior:** Spiked ball. Rolls along the floor in a patrol pattern. Spike-covered body = contact damage; usually placed in narrow corridors.

| Stat | Value |
|------|-------|
| HP | 20 |
| Contact damage | 20 |
| Roll speed | ~3 u/s |

**Composition:** (core) + `Gravity` + `PatrolWalk`.

### 12. Raiden *(Volcano, Military Train)* — *uncertain, verify during review*

**Behavior:** Lightning-themed cannon. Fires a vertical or angled lightning bolt at regular intervals. Likely mounted on ceiling or wall.

| Stat | Value |
|------|-------|
| HP | 25 |
| Contact damage | 15 |
| Beam damage | 15 |
| Fire interval | ~2.5 s |

**Composition:** (core) + `Raiden.cs` *(single-use — lightning telegraph + beam spawn)*.

---

## Enemy roster — Sky Lagoon unique

### 13. Tonboroid S (dragonfly)

**Behavior:** Hovering flyer. Sine-bobs idly. When the player enters range, dives diagonally toward the recorded player position, then returns to hover.

| Stat | Value |
|------|-------|
| HP | 20 |
| Contact damage | 15 |
| Hover amplitude | ~1 unit |
| Swoop speed | ~8 u/s |

**Composition:** (core) + `HoverSine` + `PlayerDetector` + `SwoopAttack`.

### 14. Mad Bull 97 (charging mech)

**Behavior:** Large armored mech. Spawns off-screen, charges straight forward. Does not turn. Destroys itself on wall impact.

| Stat | Value |
|------|-------|
| HP | 50 |
| Contact damage | 25 |
| Charge speed | ~10 u/s |

**Composition:** (core) + `MoveForward` + `DestroyOnWallContact`.

### 15. Trap Blast (stationary turret)

**Behavior:** Fixed cannon. Fires on interval in a fixed direction. Invulnerable.

| Stat | Value |
|------|-------|
| HP | ∞ |
| Shot damage | 10 |
| Fire interval | ~2 s |

**Composition:** `HitBox` + `AutoShoot`. No `DestroyOnDepleted`/`Health`/`DamageFlash` (invulnerable turret — no hit feedback needed).

---

## Enemy roster — Jungle unique

### 16. Kill Fisher — *uncertain, verify during review*

**Behavior:** Fishing enemy suspended in a tree. Dangles a hook/line downward that damages the player on contact. Static body, retracts + redangles.

| Stat | Value |
|------|-------|
| HP | 10 (body) |
| Hook damage | 15 |
| Cycle | ~2 s |

**Composition:** (core) + child `HitBox` on hook sprite + `KillFisher.cs` *(single-use — drives hook extend/retract timing + visibility)*.

### 17. Metal Gabyoall *(simplified — floor-only; wall/ceiling crawl deferred)*

**Behavior:** Spiked slab that walks along the floor. Slow. Classic-Mega-Man indestructible nuisance (or very high HP). Original MMX4 behavior includes wall/ceiling crawling — deferred to a later pass with dedicated surface-crawl design.

| Stat | Value |
|------|-------|
| HP | ∞ (or 99) |
| Contact damage | 20 |
| Crawl speed | ~1.5 u/s |

**Composition:** `HitBox` + `PatrolWalk` + `Gravity`. No `DestroyOnDepleted`/`Health` if fully invulnerable.

### 18. King Poseidon — **DEFERRED**

Needs a water system (buoyancy, drag, water-layer physics, entry/exit transitions) that doesn't exist in the project today. Revisit after water physics land. Provisional design: (core) + water-specific movement script + `TrackPlayer` for charge mode.

### 19. Obiiru — *uncertain, verify during review*

**Behavior:** Jungle enemy. Possibly swings from vines or drops from canopy. Placeholder: pendulum swing that damages on contact, reverses on endpoints.

| Stat | Value |
|------|-------|
| HP | 15 |
| Contact damage | 15 |
| Swing period | ~2 s |

**Composition:** (core) + `Obiiru.cs` *(single-use — pendulum motion around an anchor point)*.

### 20. Mega Nest

**Behavior:** Stationary nest mounted on a tree/wall. Periodically spawns smaller flying enemies (e.g., hornets or Kyunnbyunn-like). Capped active spawns.

| Stat | Value |
|------|-------|
| HP | 40 |
| Contact damage | 10 |
| Spawn interval | ~3 s |
| Max active spawns | 3 |

**Composition:** (core) + `MegaNest.cs` *(single-use — periodic spawn with cap via spawn-tracking list, decremented on child destroy)*.

### 21. Spider Core

**Behavior:** Ceiling-mounted spider. Descends to player height, strikes, retracts. Reuses the swoop pattern with a ceiling anchor instead of a hover point.

| Stat | Value |
|------|-------|
| HP | 30 |
| Contact damage | 20 |
| Cycle | ~3 s |

**Composition:** (core) + `PlayerDetector` + `SwoopAttack` (with `_returnTarget` override — see revisions).

---

## Enemy roster — Cyber Space unique

### 22. Miru Toraeru — *uncertain, verify during review*

**Behavior:** Cyber enemy. Placeholder description: teleports between positions, attacks from new location.

| Stat | Value |
|------|-------|
| HP | 15 |
| Contact damage | 15 |
| Teleport interval | ~2 s |

**Composition:** (core) + `MiruToraeru.cs` *(single-use — disappear → reappear near player → attack → teleport again)*.

### 23. TriScan

**Behavior:** Triangular scanner drone. Hovers in place, rotates, emits 3 beams in cardinal directions that sweep.

| Stat | Value |
|------|-------|
| HP | 20 |
| Contact damage | 10 |
| Beam damage | 15 |
| Sweep period | ~3 s |

**Composition:** (core) + `HoverSine` (small) + `RadialShoot` (3-way).

### 24. Protecton

**Behavior:** Shield-carrying cyber soldier. Front-facing energy shield blocks all damage. Vulnerable from above or behind. Fires aimed shots forward on cooldown.

| Stat | Value |
|------|-------|
| HP | 25 |
| Contact damage | 15 |
| Shot damage | 10 |

**Composition:** (core) + `Protecton.cs` *(single-use — toggles sibling `HurtBox.enabled` based on hit angle via `Health.Damaged` event)* + `PlayerDetector` + `EnemyShoot`.

---

## Enemy roster — Air Force unique

### 25. Beam Cannon

**Behavior:** Large stationary cannon mounted on hull/wall. Fires sustained beam across the screen; same pattern as Plasma Cannon but longer charge + wider beam.

| Stat | Value |
|------|-------|
| HP | ∞ (or 50) |
| Beam damage | 25 |
| Cycle | ~4 s |

**Composition:** `HitBox` + `BeamCannon.cs` *(single-use — wider/longer beam variant of PlasmaCannon. Extract a shared `BeamProjectile` helper once the three beam enemies have concrete implementations.)*.

### 26. Metal Hawk

**Behavior:** Mechanical hawk. Flies in lazy horizontal passes, dives at player when in range. Similar structure to Tonboroid S.

| Stat | Value |
|------|-------|
| HP | 20 |
| Contact damage | 20 |
| Swoop speed | ~9 u/s |

**Composition:** **Prefab variant of Tonboroid S** — same composition, different SVG + stat tweaks.

### 27. Walk Shooter

**Behavior:** Bipedal soldier. Walks a short patrol, detects and fires a single aimed shot. Essentially a Knot Beret B silhouette variant.

| Stat | Value |
|------|-------|
| HP | 20 |
| Contact damage | 15 |
| Shot damage | 10 |

**Composition:** **Prefab variant of Knot Beret B.**

---

## Enemy roster — Volcano unique

### 28. Prominence

**Behavior:** Fire pillar that erupts from lava or a vent. Active/inactive cycle: telegraph → rise (contact hazard) → retract. Invulnerable.

| Stat | Value |
|------|-------|
| HP | ∞ |
| Contact damage | 25 |
| Active duration | ~1 s |
| Cycle | ~3 s |

**Composition:** `HitBox` + `Prominence.cs` *(single-use — cycle timer that toggles sprite + collider)*.

---

## Enemy roster — Marine Base unique

### 29. Hornet

**Behavior:** Flying wasp. Hovers in small patterns, dives at player when in range. Functionally Tonboroid S's cousin.

| Stat | Value |
|------|-------|
| HP | 15 |
| Contact damage | 15 |
| Swoop speed | ~7 u/s |

**Composition:** **Prefab variant of Tonboroid S.**

---

## Enemy roster — Snow Base unique

### 30. E-AT

**Behavior:** Walking frost mech. Slow patrol, fires ice shards on detection. Tanky grunt.

| Stat | Value |
|------|-------|
| HP | 30 |
| Contact damage | 15 |
| Shot damage | 10 |

**Composition:** (core) + `Gravity` + `PatrolWalk` + `PlayerDetector` + `EnemyShoot`.

### 31. Yukidarubon

**Behavior:** Snowman that rolls down slopes toward the player. On destruction, splits into 2–3 smaller snowmen that also roll.

| Stat | Value |
|------|-------|
| HP | 15 |
| Contact damage | 15 |
| Roll speed | ~4 u/s |

**Composition:** (core) + `Gravity` + `PatrolWalk` (downhill bias) + `Yukidarubon.cs` *(single-use — subscribes to `Health.Depleted`, instantiates N child prefabs at current position before destroy)*.

### 32. Knot Beret S *(snow variant)*

**Behavior:** Visual/palette variant of Knot Beret B with white/snow colors. Same AI.

| Stat | Value |
|------|-------|
| HP | 15 |
| Contact damage | 10 |
| Shot damage | 8 |

**Composition:** **Prefab variant of Knot Beret B.**

### 33. Fly Gunner

**Behavior:** Flying gunner. Hovers, tracks player, fires aimed shots. Snow-themed Hover Gunner analog.

| Stat | Value |
|------|-------|
| HP | 20 |
| Contact damage | 15 |
| Shot damage | 10 |

**Composition:** **Prefab variant of Hover Gunner.**

### 34. Ice Wing

**Behavior:** Flying ice bird. Drops ice shards from directly overhead as it flies horizontally across the screen.

| Stat | Value |
|------|-------|
| HP | 15 |
| Contact damage | 15 |
| Shot damage | 10 |
| Fly speed | ~5 u/s |

**Composition:** (core) + `MoveForward` + `HoverSine` (small) + `AutoShoot` (downward muzzle) + `Lifetime`.

---

## Enemy roster — Bio Laboratory unique

### 35. Tentoroid RS (red)

**Behavior:** Floating orb enemy. Hovers, fires aimed shots on interval. Bio Lab's stock turret-flyer.

| Stat | Value |
|------|-------|
| HP | 20 |
| Contact damage | 10 |
| Shot damage | 10 |
| Fire interval | ~2 s |

**Composition:** (core) + `HoverSine` + `AutoShoot` (aimed muzzle), or `PlayerDetector` + `EnemyShoot` if aim-at-player is needed.

### 36. Tentoroid BS (blue)

**Behavior:** Tougher Tentoroid. Higher HP, faster fire interval, possibly different shot pattern.

| Stat | Value |
|------|-------|
| HP | 30 |
| Contact damage | 10 |
| Shot damage | 12 |

**Composition:** **Prefab variant of Tentoroid RS.**

### 37. Togerics — *uncertain, verify during review*

**Behavior:** Bio-spiky enemy. Placeholder: leaps toward the player on detection, lands, cools down.

| Stat | Value |
|------|-------|
| HP | 15 |
| Contact damage | 15 |
| Jump speed | ~6 u/s |
| Cooldown | ~2 s |

**Composition:** (core) + `Gravity` + `PlayerDetector` + `Togerics.cs` *(single-use — leap with arc, cooldown on land)*.

### 38. Dejira

**Behavior:** Floating spike blob. Drifts slowly toward the player, purely a contact hazard.

| Stat | Value |
|------|-------|
| HP | 15 |
| Contact damage | 20 |
| Drift speed | ~2 u/s |

**Composition:** (core) + `HoverSine` (small) + `TrackPlayer` (slow drift).

### 39. Guardian

**Behavior:** Stationary lab sentry. Detects and fires aimed shots. Functionally Knot Beret G's lab-themed cousin.

| Stat | Value |
|------|-------|
| HP | 20 |
| Contact damage | 15 |
| Shot damage | 10 |

**Composition:** **Prefab variant of Knot Beret G** (lab palette, slight stat bump).

### 40. Death Guardian

**Behavior:** Beefier Guardian. Higher HP, harder-hitting shots, longer detection range.

| Stat | Value |
|------|-------|
| HP | 50 |
| Contact damage | 20 |
| Shot damage | 15 |

**Composition:** **Prefab variant of Guardian.**

---

## New reusable components *(3 items)*

Genuinely shared behavior used by 2+ enemies. Lives in [Assets/_Project/Scripts/Enemy/](Assets/_Project/Scripts/Enemy/) (or `Behavior/` for non-enemy-specific items).

| Component | Used by | Responsibility |
|-----------|---------|----------------|
| `TrackPlayer` | Hover Gunner, Giga Death, Dejira, Fly Gunner | Match player's X (or Y) with speed cap; axis + max speed configurable |
| `SpreadShoot` | Mettaur D2 (internal), *(Blast Raster variant possible)* | Fire N projectiles in a cone/fan around muzzle direction |
| `RadialShoot` | Blast Raster, TriScan | Fire N projectiles evenly around 360° (or a partial arc) |

## New single-use enemy AI scripts *(13 items)*

One script per enemy, encapsulating its signature choreography as a small state machine or behavior. Each is ~30–80 lines, composes existing infrastructure for shared parts (`Health`, `HurtBox`, `HitBox`, `DamageFlash`, `PlayerDetector`, etc.), and reads as a single file you can navigate top-to-bottom.

Lives in **[Assets/_Project/Scripts/Enemy/AI/](Assets/_Project/Scripts/Enemy/AI/)** (new folder) so reusable components in `Enemy/` stay easy to find.

| Script | Enemy | What it does |
|--------|-------|--------------|
| `Mettaur.cs` | Mettaur D2 | Hide/peek/fire state machine; toggles sibling `HurtBox.enabled`; delegates fire to `SpreadShoot` |
| `BattonBone.cs` | Batton Bone B81 | Sleep on ceiling → drop briefly → fly in sine-wave toward player |
| `MegaNest.cs` | Mega Nest | Periodic spawn with active-count cap (tracks children via `OnDestroy` callback) |
| `Yukidarubon.cs` | Yukidarubon | On `Health.Depleted`, instantiates N child prefabs before destroy |
| `Togerics.cs` | Togerics | Leap-toward-player with arc, cooldown on land |
| `Protecton.cs` | Protecton | Front-angle hit filter via `Health.Damaged` source position vs. facing — toggles `HurtBox.enabled` per-hit |
| `Obiiru.cs` | Obiiru | Pendulum swing around anchor point *(uncertain behavior — verify)* |
| `MiruToraeru.cs` | Miru Toraeru | Teleport + attack cycle *(uncertain behavior — verify)* |
| `KillFisher.cs` | Kill Fisher | Hook dangle/retract cycle + child `HitBox` on the hook |
| `Prominence.cs` | Prominence | Active/inactive hazard cycle (toggles sprite + collider on timer) |
| `PlasmaCannon.cs` | Plasma Cannon | Charge telegraph → sustained beam for duration → cooldown |
| `Raiden.cs` | Raiden | Lightning bolt on interval *(uncertain behavior — verify)* |
| `BeamCannon.cs` | Beam Cannon | Wider/longer sustained beam |

**Beam architecture note:** The three beam enemies (`PlasmaCannon`, `Raiden`, `BeamCannon`) each ship as their own script first. Once all three have concrete implementations, inspect for shared shape and extract a `BeamProjectile` helper only if the extraction is clean. Don't design the shared helper upfront; let it emerge (or not) from the three concrete cases.

---

## Component revisions

Minor, additive changes to existing components. No breaking changes.

- **`EnemyShoot`** — Extract muzzle aim and projectile spawn into `protected` methods (`AimMuzzleAt(Vector2)`, `FireProjectile()`) so `SpreadShoot` and `RadialShoot` can reuse the aim logic instead of duplicating. Optional — if `SpreadShoot`/`RadialShoot` don't inherit, skip.
- **`PlayerDetector`** — Add optional `_coneAngle` (0 = radial, >0 = directional cone in facing direction) so patrolling shooters stop detecting the player behind them. Default 0 preserves current behavior.
- **`SwoopAttack`** — Add optional `_returnTarget` (Transform or Vector2 override) so Spider Core can anchor return to a fixed ceiling point rather than its spawn position. Default preserves current behavior (return to spawn).

That's it. The scope-heavy revisions from the earlier draft (`Gravity` direction field, `BeamProjectile` new type, `HurtBox` angle filter, `DropTrigger` retirement) all get dropped because the work they enabled moved into single-use scripts that don't need base-component changes.

---

## Scalability review

The 18-component base + 3 new reusable components + 13 single-use AI scripts scales cleanly to 40 enemies. The key insight: **the composition-vs-single-use decision is itself a scalability lever**. Apply it consistently and the component library stays small, the enemy scripts stay readable, and effort estimates stop lying.

### 1. Rule of three for components

New code becomes a reusable component only when **3+ enemies genuinely need it**. Below that threshold: write a single-use enemy script that composes existing components. This is why the new-component count dropped from 14 to 3 in this revision.

Why it matters: single-use components masquerading as reusable abstractions bloat the library, spread choreography logic across event-coupled files, and hide day-scale work in hours-scale table rows. A single-file `Mettaur.cs` is easier to understand, debug, and rewrite than `HideShell + SpreadShoot + glue` communicating via events.

### 2. Folder separation

- [Assets/_Project/Scripts/Enemy/](Assets/_Project/Scripts/Enemy/) — reusable components (existing + `TrackPlayer`, `SpreadShoot`, `RadialShoot`).
- [Assets/_Project/Scripts/Enemy/AI/](Assets/_Project/Scripts/Enemy/AI/) — single-use AI scripts, one per enemy.

That separation makes it obvious what's reusable (don't gate-keep; encouraged) vs. what's enemy-specific (don't reach for when authoring a new enemy — start with components instead).

### 3. Generator split per stage

**Trigger:** When [SkyLagoonEnemyGenerator.cs](Assets/_Project/Scripts/Editor/SkyLagoonEnemyGenerator.cs) is touched to add a sixth enemy or we start Jungle — whichever comes first.

**Action:** Refactor into per-stage generator files + a shared core.

- `Assets/_Project/Scripts/Editor/EnemyGeneratorCore.cs` — `NewEnemyRoot`, `AddMuzzle`, `SetField`, `LoadSprite`/`LoadPrefab`/`LoadMaterial`, `SavePrefab`, `AddGravity`. All current helpers move here.
- `SkyLagoonEnemyGenerator.cs`, `RecurringEnemyGenerator.cs`, `JungleEnemyGenerator.cs`, … — each registers its own `Tools/MegamanX4/Generate X Enemies` menu item.
- `AllEnemiesGenerator.cs` — a `Tools/MegamanX4/Generate All Enemies` meta-item that calls every generator.

### 4. Prefab variants for palette/stat tweaks

**Trigger:** Next time we generate a variant (Knot Beret S, Hornet, Metal Hawk, Fly Gunner, Walk Shooter, Guardian/Death Guardian, Tentoroid BS).

**Action:** Author the base enemy normally via the generator. For variants, use Unity's **Prefab Variant** feature — create from the base, override only the changed fields (sprite, tint, stats). Don't write a second generator method.

### 5. ScriptableObject-driven stats *(defer)*

**Trigger:** First time the user asks for a balance tuning pass across many enemies.

Not needed yet. 40 enemies × 5 stats = 200 values; still fine to edit in code during iteration. Add the abstraction when it starts hurting.

### 6. What doesn't need changing

- **Component composition model** — proven.
- **Physics matrix + layer routing** — no new layers needed.
- **Editor generator pattern** — `SerializedObject.FindProperty` + `PrefabUtility.SaveAsPrefabAsset` scales to any number of prefabs.
- **`DestroyOnDepleted` lifecycle** — stays `Depleted → Destroy`.

---

## Phased implementation order

Stage-by-stage rollout. Each stage lists its net-new work; anything unlisted is already implemented or covered by an earlier phase.

### Phase 1 — Foundation ✅ *implemented*

Sky Lagoon (7 enemies). Components: `PlayerDetector`, `AutoShoot`, `DestroyOnWallContact`, `EnemyShoot`, `PatrolWalk`, `HoverSine`, `SwoopAttack`, `DropTrigger`, `Gravity`, `DamageFlash`, `InvulnerabilityBlinker`.

### Phase 2 — Recurring extension

Build before any Maverick stage so the bestiary backbone is solid.

- **Reusable components:** `TrackPlayer`, `SpreadShoot`, `RadialShoot`.
- **Revisions:** `EnemyShoot` (extract protected methods — only if SpreadShoot/RadialShoot inherit).
- **AI scripts:** `Mettaur.cs`, `BattonBone.cs`, `PlasmaCannon.cs`, `Raiden.cs`.
- **Generator split refactor** (Scalability §3) happens before Phase 3.

Enemies shipped: Blast Raster, Hover Gunner, Giga Death, Plasma Cannon, Batton Bone B81, Mettaur D2, Spiky Mk-II, Raiden.

### Phase 3 — Jungle

- **AI scripts:** `KillFisher.cs`, `MegaNest.cs`, `Obiiru.cs`.
- **Revisions:** `SwoopAttack` return-target override (for Spider Core).
- **Deferred:** King Poseidon (water system), Metal Gabyoall wall-crawl (ship floor-only).

Enemies shipped: Kill Fisher, Metal Gabyoall (simplified), Obiiru, Mega Nest, Spider Core.

### Phase 4 — Cyber Space

- **AI scripts:** `MiruToraeru.cs`, `Protecton.cs`.
- **Revisions:** `PlayerDetector` cone angle (for Protecton + any future directional shooter).

Enemies shipped: Miru Toraeru, TriScan, Protecton.

### Phase 5 — Air Force

- **AI scripts:** `BeamCannon.cs`. At this point all three beam enemies exist — inspect for shared shape, extract `BeamProjectile` helper if clean.
- **Prefab variants:** Metal Hawk (from Tonboroid), Walk Shooter (from Knot Beret B).

Enemies shipped: Beam Cannon, Metal Hawk, Walk Shooter.

### Phase 6 — Volcano + Marine Base

- **AI scripts:** `Prominence.cs`.
- **Prefab variants:** Hornet (from Tonboroid).

Enemies shipped: Prominence, Hornet.

### Phase 7 — Snow Base

- **AI scripts:** `Yukidarubon.cs`.
- **Prefab variants:** Knot Beret S (from Knot Beret B), Fly Gunner (from Hover Gunner).

Enemies shipped: E-AT, Yukidarubon, Knot Beret S, Fly Gunner, Ice Wing.

### Phase 8 — Bio Laboratory

- **AI scripts:** `Togerics.cs`.
- **Prefab variants:** Tentoroid BS (from RS), Guardian (from Knot Beret G), Death Guardian (from Guardian).

Enemies shipped: Tentoroid RS, Tentoroid BS, Togerics, Dejira, Guardian, Death Guardian.

### Phase 9 — Polish

Per-enemy audio hooks, spawn volumes, stage-specific palettes, prefab variant cleanup, surface-crawl subsystem revisit (Metal Gabyoall), water system + King Poseidon.

---

## SVG assets needed

Existing SVGs (✅ already authored): Knot Beret B, G; Spike Marl; Kyunnbyunn; Tonboroid S; Mad Bull 97; Trap Blast.

New authored SVGs needed (32 — King Poseidon deferred):

| Enemy | Dimensions | Notes |
|-------|-----------|-------|
| Blast Raster | ~48×48 | Red floating orb, layered glow |
| Hover Gunner | ~48×40 | Helicopter drone, rotor on top |
| Giga Death | ~80×64 | Heavy armored turret, downward-facing cannon |
| Plasma Cannon | ~48×40 | Wall turret with charge socket |
| Batton Bone B81 | ~32×32 | Bat: wings-folded "hanging" pose; separate open-wing pose for flight (swap) |
| Mettaur D2 | ~32×32 | Classic dome with eyes |
| Spiky Mk-II | ~32×32 | Spiked ball, symmetric |
| Raiden | ~48×48 | Lightning-themed cannon / orb |
| Kill Fisher | ~32×40 + hook | Tree-hanging fisher + separate hook-line sprite |
| Metal Gabyoall | ~32×24 | Spiked slab, symmetric |
| Obiiru | ~32×48 | Swinging jungle creature |
| Mega Nest | ~64×48 | Stationary nest |
| Spider Core | ~48×48 | Spider silhouette, legs spread |
| Miru Toraeru | ~32×32 | Cyber enemy, fades in/out |
| TriScan | ~48×48 | Triangular scanner, 3 emitter nodes |
| Protecton | ~40×48 | Soldier + forward shield |
| Beam Cannon | ~64×48 | Large cannon, ship-mounted |
| Metal Hawk | ~48×32 | Hawk with spread wings |
| Walk Shooter | ~32×48 | Bipedal soldier with rifle (distinct from Knot Beret) |
| Prominence | ~32×64 | Fire pillar, tall |
| Hornet | ~40×32 | Wasp, horizontal |
| E-AT | ~48×48 | Frost mech, stocky |
| Yukidarubon | ~32×32 (+ small variant ~16×16) | Snowman; authored large + small for split |
| Knot Beret S | — | Prefab variant; reuse Knot Beret B SVG with palette swap |
| Fly Gunner | — | Prefab variant of Hover Gunner |
| Ice Wing | ~40×32 | Ice bird, wings spread |
| Tentoroid RS | ~40×40 | Red orb with red accents |
| Tentoroid BS | — | Prefab variant (blue palette) |
| Togerics | ~32×32 | Spiky bio-creature |
| Dejira | ~40×40 | Spike blob |
| Guardian | — | Prefab variant of Knot Beret G (lab palette) |
| Death Guardian | — | Prefab variant of Guardian |

**Authoring convention:** SVGs are authored facing right — same invariant as projectiles and the player. Positive X = forward = canonical render direction. Enemies that should face left in a scene are flipped at the prefab/scene level via `localScale.x = -1` (or `PatrolWalk._initialFacing = -1`), not by re-authoring the SVG. Existing left-authored enemies (Knot Beret B/G, Kyunnbyunn, Mad Bull 97, Tonboroid S, Trap Blast) use a flip-wrapper (`<g transform="translate(W 0) scale(-1 1)">`) to render right — preserved for GUID stability, but new SVGs should be drawn right directly.

---

## Verification

Per enemy, place in `Gameplay.unity` and confirm:

- Takes damage from player projectiles; `DamageFlash` white-flashes on every hit; destroyed at 0 HP (or never, for invulnerable hazards).
- Deals listed contact/shot damage; player knockback + i-frames trigger.
- Unique behavior matches description: patrol turns at edges; detection triggers attack; swoop dives + returns; mine drops; charger self-destructs on wall; turret/cannon fires on interval; bird follows wave pattern; beam telegraphs before firing; hide/peek cycle; spawner respects cap; on-destroy splits into children; directional shield blocks front hits only.
- Projectiles on `EnemyProjectile` layer; hit player, not other enemies.
- No NullReferenceExceptions when optional components are absent (graceful null checks).

Per stage, confirm in `Gameplay.unity`:

- Stage-specific enemies spawn/destroy on entry/exit of trigger volumes.
- Recurring enemies reused across stages share a single prefab reference (or variant chain), not copy-pasted.

Per refactor (Phase 2 generator split, Phase 5 prefab variants):

- Re-running `Tools/MegamanX4/Generate *` menu items produces identical prefabs to the pre-refactor versions (diff-check before/after).
