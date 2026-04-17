# SPEC_ENEMIES

## Context

Covers every regular (non-boss, non-sub-boss) enemy in Mega Man X4, across the intro stage (Sky Lagoon) and the eight Maverick stages. Goal: one spec covering all 40 enemies so component reuse, stat balance, and generator scope can be planned end-to-end rather than stage-by-stage.

Phase 1 (Sky Lagoon, 7 enemies) is already implemented. This spec treats those as reference entries — behavior is frozen, stats scaled to the new HP/damage range (player HP 100, default enemy HP 10).

**Scope exclusions:** all bosses (Eregion, Web Spider, Cyber Peacock, Storm Owl, Magma Dragoon, Jet Stingray, Slash Beast, Frost Walrus, Split Mushroom) and all sub-bosses (Generaid Core, DG-42L, Eyezard, Tentoroid). These are set-piece encounters and don't share the component-composition pattern used for regular enemies.

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

17 components ship today, grouped by folder.

**Enemy** ([Assets/_Project/Scripts/Enemy/](Assets/_Project/Scripts/Enemy/))

| Component | Role |
|-----------|------|
| `Enemy` | Lifecycle: subscribes to `Health.Depleted` → destroys GameObject |
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
| `DamageFlash` | Blinks `SpriteRenderer.enabled` during i-frames |

Layers (see [Layers.cs](Assets/_Project/Scripts/Layers.cs)): `Player`, `Environment`, `Enemy`, `Ladder`, `PlayerProjectile`, `PlayerProjectileNoClip`, `EnemyProjectile`. Physics2D matrix already routes `EnemyProjectile ↔ Player, Environment` and `PlayerProjectile ↔ Enemy, Environment`.

**Conventions** (see [CLAUDE.md](CLAUDE.md)): SVGs authored facing right (projectile convention) or authored facing left with flip-wrapper (enemy/character convention, renders right); all rotations flow from the root/muzzle `transform`, never from `localScale.x` flips for direction math; gravity is custom for AI-driven enemies (via `Gravity`), physics-driven only for pure hazards (Spike Marl post-drop).

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

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `Gravity` + `PatrolWalk` + `PlayerDetector` + `EnemyShoot` (burst=3).

### 2. Knot Beret G (green soldier) *(Sky Lagoon, Military Train)*

**Behavior:** Stationary variant. Stands in place, faces the player when detected, fires a single aimed shot on cooldown.

| Stat | Value |
|------|-------|
| HP | 15 |
| Contact damage | 10 |
| Shot damage | 10 |
| Detection range | ~10 units |

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `Gravity` + `PlayerDetector` + `EnemyShoot` (burst=1, longer cooldown).

### 3. Spike Marl (ceiling mine) *(Sky Lagoon, Cyber Space)*

**Behavior:** Ceiling-attached spiked ball. Inert until player passes below, then detaches and drops straight down. High contact damage. Destroyed on ground impact.

| Stat | Value |
|------|-------|
| HP (post-drop) | 10 |
| Contact damage | 30 |
| Trigger range below | ~2 units × 20 |

**Composition:** `Enemy` + `Health` + `HurtBox` (disabled until drop) + `HitBox` + `DropTrigger`. Post-drop: `Rigidbody2D` switches to Dynamic, `gravityScale = 3`.

### 4. Kyunnbyunn (swooping bird) *(Sky Lagoon, Jungle)*

**Behavior:** Flies horizontally in a sine-wave pattern across the screen. Does not turn — flies once and despawns. Often spawns in groups of 2–3.

| Stat | Value |
|------|-------|
| HP | 5 |
| Contact damage | 10 |
| Fly speed | ~6 u/s |
| Wave amplitude | ~1.5 units |
| Wave frequency | ~3 Hz |

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `MoveForward` + `HoverSine` + `Lifetime` (10s).

### 5. Blast Raster *(Jungle, Bio Laboratory)*

**Behavior:** Floating spherical enemy. Hovers in place. Periodically fires shots radially in 4 or 8 directions (cardinal/diagonal). Chaff-level HP but awkward to approach due to spread fire.

| Stat | Value |
|------|-------|
| HP | 10 |
| Contact damage | 10 |
| Shot damage | 8 |
| Fire interval | ~2.5 s |

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `HoverSine` (low amplitude) + `RadialShoot` *(new)*.

### 6. Hover Gunner *(Cyber Space, Marine Base, Bio Laboratory)*

**Behavior:** Helicopter-drone. Hovers at fixed altitude, tracks the player horizontally, fires aimed shots down-forward at the player on cooldown.

| Stat | Value |
|------|-------|
| HP | 25 |
| Contact damage | 15 |
| Shot damage | 10 |
| Track speed | ~3 u/s |

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `HoverSine` + `TrackPlayer` *(new, X-axis)* + `PlayerDetector` + `EnemyShoot`.

### 7. Giga Death *(Air Force, Volcano)*

**Behavior:** Large floating turret. Drifts slowly from above; fires heavy shots downward at the player. High HP, infrequent fire.

| Stat | Value |
|------|-------|
| HP | 50 |
| Contact damage | 25 |
| Shot damage | 20 |
| Fire interval | ~3 s |

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `MoveVertical` (slow down) + `TrackPlayer` *(new, X-axis, slow)* + `AutoShoot` (aimed muzzle).

### 8. Plasma Cannon *(Air Force, Military Train)*

**Behavior:** Wall-mounted cannon. Charges a plasma beam with visible telegraph, then fires a sustained beam across the screen for ~1 s. Invulnerable.

| Stat | Value |
|------|-------|
| HP | ∞ |
| Contact damage | — |
| Beam damage | 20 (per tick) |
| Cycle | ~3 s |

**Composition:** `HitBox` + `ChargedBeam` *(new)*. No `Enemy`/`Health`.

### 9. Batton Bone B81 *(Volcano, Military Train, Bio Laboratory)*

**Behavior:** Classic bat. Hangs upside-down on ceiling, inert. When the player approaches, activates: drops briefly, then flies in a zigzag sine path toward the player. Does not return.

| Stat | Value |
|------|-------|
| HP | 10 |
| Contact damage | 15 |
| Detection range | ~6 units |
| Fly speed | ~4 u/s |

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `PlayerDetector` + `ActivateOnDetect` *(new)* → enables `MoveForward` + `HoverSine`. Initial velocity is toward player direction.

### 10. Mettaur D2 *(Volcano, Military Train, Snow Base)*

**Behavior:** Hides under armored hat (invulnerable). Peeks out on cooldown, fires a 3-way spread (down-left, down, down-right), returns to hiding. Small and slow.

| Stat | Value |
|------|-------|
| HP (peeking) | 10 |
| Contact damage | 10 |
| Shot damage | 8 |
| Hide/peek cycle | ~2 s |

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `HideShell` *(new, drives cycle)* + `SpreadShoot` *(new, 3-way fan)*.

### 11. Spiky Mk-II *(Volcano, Bio Laboratory)*

**Behavior:** Spiked ball. Rolls along the floor in a patrol pattern. Spike-covered body = contact damage; usually placed in narrow corridors.

| Stat | Value |
|------|-------|
| HP | 20 |
| Contact damage | 20 |
| Roll speed | ~3 u/s |

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `Gravity` + `PatrolWalk`. *(Visual spin can be driven by an optional `SpinVisual` child — defer.)*

### 12. Raiden *(Volcano, Military Train)* — *uncertain, verify during review*

**Behavior:** Lightning-themed cannon. Fires a vertical or angled lightning bolt at regular intervals. Likely mounted on ceiling or wall.

| Stat | Value |
|------|-------|
| HP | 25 |
| Contact damage | 15 |
| Beam damage | 15 |
| Fire interval | ~2.5 s |

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `ChargedBeam` *(new)*.

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

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `HoverSine` + `PlayerDetector` + `SwoopAttack`.

### 14. Mad Bull 97 (charging mech)

**Behavior:** Large armored mech. Spawns off-screen, charges straight forward. Does not turn. Destroys itself on wall impact.

| Stat | Value |
|------|-------|
| HP | 50 |
| Contact damage | 25 |
| Charge speed | ~10 u/s |

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `MoveForward` + `DestroyOnWallContact`.

### 15. Trap Blast (stationary turret)

**Behavior:** Fixed cannon. Fires on interval in a fixed direction. Invulnerable.

| Stat | Value |
|------|-------|
| HP | ∞ |
| Shot damage | 10 |
| Fire interval | ~2 s |

**Composition:** `HitBox` + `AutoShoot`. No `Enemy`/`Health`.

---

## Enemy roster — Jungle unique

### 16. Kill Fisher — *uncertain, verify during review*

**Behavior:** Fishing enemy suspended in a tree. Dangles a hook/line downward that damages the player on contact. Static body, retracts + redangles.

| Stat | Value |
|------|-------|
| HP | 10 (body) |
| Hook damage | 15 |
| Cycle | ~2 s |

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` (body) + child `HitBox` on hook sprite + `PeriodicHazard` *(new, drives hook retract/extend)*.

### 17. Metal Gabyoall

**Behavior:** Spiked slab that walks along any surface — floor, ceiling, or wall. Slow. Classic-Mega-Man indestructible nuisance (or very high HP).

| Stat | Value |
|------|-------|
| HP | ∞ (or 99) |
| Contact damage | 20 |
| Crawl speed | ~1.5 u/s |

**Composition:** `HitBox` + `SurfaceCrawl` *(new — walks along current surface, rotates sprite to match)*. No `Health` if invulnerable; otherwise `Enemy` + `Health`.

### 18. King Poseidon

**Behavior:** Large fish. Swims in underwater sections, patrols lazily, charges toward player when detected.

| Stat | Value |
|------|-------|
| HP | 40 |
| Contact damage | 25 |
| Swim speed | ~3 u/s (charge ~6) |

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `SwimMovement` *(new — 2D free-swim, drag-based)* + `PlayerDetector` + `TrackPlayer` (charge mode).

### 19. Obiiru — *uncertain, verify during review*

**Behavior:** Jungle enemy. Possibly swings from vines or drops from canopy. Placeholder: pendulum swing that damages on contact, reverses on endpoints.

| Stat | Value |
|------|-------|
| HP | 15 |
| Contact damage | 15 |
| Swing period | ~2 s |

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `Swing` *(new, pendulum around anchor)*.

### 20. Mega Nest

**Behavior:** Stationary nest mounted on a tree/wall. Periodically spawns smaller flying enemies (e.g., hornets or Kyunnbyunn-like). Capped active spawns.

| Stat | Value |
|------|-------|
| HP | 40 |
| Contact damage | 10 |
| Spawn interval | ~3 s |
| Max active spawns | 3 |

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `Spawner` *(new)*.

### 21. Spider Core

**Behavior:** Ceiling-mounted spider. Descends on a line to player height, fires or strikes, retracts. Think of SwoopAttack but starting from a ceiling anchor instead of a hover point.

| Stat | Value |
|------|-------|
| HP | 30 |
| Contact damage | 20 |
| Cycle | ~3 s |

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `PlayerDetector` + `SwoopAttack` (reused — ceiling as origin, player as dive target).

---

## Enemy roster — Cyber Space unique

### 22. Miru Toraeru — *uncertain, verify during review*

**Behavior:** Cyber enemy. Placeholder description: teleports between positions, attacks from new location. Alternatively a shield-carrier (verify).

| Stat | Value |
|------|-------|
| HP | 15 |
| Contact damage | 15 |
| Teleport interval | ~2 s |

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `Teleport` *(new)* + `EnemyShoot` or contact.

### 23. TriScan

**Behavior:** Triangular scanner drone. Hovers in place, rotates, emits 3 beams in cardinal directions that sweep.

| Stat | Value |
|------|-------|
| HP | 20 |
| Contact damage | 10 |
| Beam damage | 15 |
| Sweep period | ~3 s |

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `HoverSine` (small) + `RadialShoot` *(new, 3-way)*.

### 24. Protecton

**Behavior:** Shield-carrying cyber soldier. Front-facing energy shield blocks all damage. Vulnerable from above or behind. Fires aimed shots forward on cooldown.

| Stat | Value |
|------|-------|
| HP | 25 |
| Contact damage | 15 |
| Shot damage | 10 |

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `DirectionalShield` *(new — toggles HurtBox `enabled` based on hit angle)* + `PlayerDetector` + `EnemyShoot`.

---

## Enemy roster — Air Force unique

### 25. Beam Cannon

**Behavior:** Large stationary cannon mounted on hull/wall. Fires sustained beam across the screen; same pattern as Plasma Cannon but longer charge + wider beam. Reuses `ChargedBeam`.

| Stat | Value |
|------|-------|
| HP | ∞ (or 50) |
| Beam damage | 25 |
| Cycle | ~4 s |

**Composition:** `HitBox` + `ChargedBeam`. Optionally `Enemy` + `Health` if destroyable variant is authored.

### 26. Metal Hawk

**Behavior:** Mechanical hawk. Flies in lazy horizontal passes, dives at player when in range. Similar structure to Tonboroid S.

| Stat | Value |
|------|-------|
| HP | 20 |
| Contact damage | 20 |
| Swoop speed | ~9 u/s |

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `HoverSine` + `PlayerDetector` + `SwoopAttack` (reused).

### 27. Walk Shooter

**Behavior:** Bipedal soldier. Walks a short patrol, detects and fires a single aimed shot. Essentially a Knot Beret B variant with different silhouette.

| Stat | Value |
|------|-------|
| HP | 20 |
| Contact damage | 15 |
| Shot damage | 10 |

**Composition:** Identical to Knot Beret B — candidate for **prefab variant** (see Scalability).

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

**Composition:** `HitBox` + `PeriodicHazard` *(new — toggles collider + visual on cycle)*.

---

## Enemy roster — Marine Base unique

### 29. Hornet

**Behavior:** Flying wasp. Hovers in small patterns, dives at player when in range. Functionally Tonboroid S's cousin.

| Stat | Value |
|------|-------|
| HP | 15 |
| Contact damage | 15 |
| Swoop speed | ~7 u/s |

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `HoverSine` + `PlayerDetector` + `SwoopAttack` — candidate for **prefab variant of Tonboroid S** (see Scalability).

---

## Enemy roster — Snow Base unique

### 30. E-AT

**Behavior:** Walking frost mech. Slow patrol, fires ice shards on detection. Tanky grunt.

| Stat | Value |
|------|-------|
| HP | 30 |
| Contact damage | 15 |
| Shot damage | 10 |

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `Gravity` + `PatrolWalk` + `PlayerDetector` + `EnemyShoot`.

### 31. Yukidarubon

**Behavior:** Snowman that rolls down slopes toward the player. On destruction, splits into 2–3 smaller snowmen that also roll. Growing menace if ignored.

| Stat | Value |
|------|-------|
| HP | 15 |
| Contact damage | 15 |
| Roll speed | ~4 u/s |

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `Gravity` + `PatrolWalk` (downhill bias) + `SpawnOnDestroy` *(new)*.

### 32. Knot Beret S *(snow variant)*

**Behavior:** Visual/palette variant of Knot Beret B with white/snow colors. Same AI.

| Stat | Value |
|------|-------|
| HP | 15 |
| Contact damage | 10 |
| Shot damage | 8 |

**Composition:** **Prefab variant of Knot Beret B** (see Scalability).

### 33. Fly Gunner

**Behavior:** Flying gunner. Hovers, tracks player, fires aimed shots. Snow-themed Hover Gunner analog.

| Stat | Value |
|------|-------|
| HP | 20 |
| Contact damage | 15 |
| Shot damage | 10 |

**Composition:** Identical to Hover Gunner — **prefab variant**.

### 34. Ice Wing

**Behavior:** Flying ice bird. Drops ice shards from directly overhead as it flies horizontally across the screen.

| Stat | Value |
|------|-------|
| HP | 15 |
| Contact damage | 15 |
| Shot damage | 10 |
| Fly speed | ~5 u/s |

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `MoveForward` + `HoverSine` (small) + `AutoShoot` (downward muzzle) + `Lifetime`.

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

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `HoverSine` + `AutoShoot` (aimed) or `PlayerDetector` + `EnemyShoot` if aim-at-player is needed.

### 36. Tentoroid BS (blue)

**Behavior:** Tougher Tentoroid. Higher HP, faster fire interval, possibly different shot pattern.

| Stat | Value |
|------|-------|
| HP | 30 |
| Contact damage | 10 |
| Shot damage | 12 |

**Composition:** **Prefab variant of Tentoroid RS.**

### 37. Togerics — *uncertain, verify during review*

**Behavior:** Bio-spiky enemy. Placeholder: leaps toward the player on detection, lands, cools down. Alternative is a simple ground spiker — verify.

| Stat | Value |
|------|-------|
| HP | 15 |
| Contact damage | 15 |
| Jump speed | ~6 u/s |
| Cooldown | ~2 s |

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `Gravity` + `PlayerDetector` + `JumpAttack` *(new)*.

### 38. Dejira

**Behavior:** Floating spike blob. Drifts slowly toward the player, purely a contact hazard.

| Stat | Value |
|------|-------|
| HP | 15 |
| Contact damage | 20 |
| Drift speed | ~2 u/s |

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `HoverSine` (small) + `TrackPlayer` *(new, slow drift)*.

### 39. Guardian

**Behavior:** Stationary lab sentry. Detects and fires aimed shots. Functionally Knot Beret G's lab-themed cousin.

| Stat | Value |
|------|-------|
| HP | 20 |
| Contact damage | 15 |
| Shot damage | 10 |

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `PlayerDetector` + `EnemyShoot`. Candidate **prefab variant of Knot Beret G**.

### 40. Death Guardian

**Behavior:** Beefier Guardian. Higher HP, harder-hitting shots, longer detection range.

| Stat | Value |
|------|-------|
| HP | 50 |
| Contact damage | 20 |
| Shot damage | 15 |

**Composition:** **Prefab variant of Guardian.**

---

## New components summary

14 new components. Grouped by concern.

### Movement / positioning

| Component | Used by | Responsibility |
|-----------|---------|----------------|
| `TrackPlayer` | Hover Gunner, Giga Death, Dejira, Fly Gunner | Match player's X (or Y) with speed cap; axis + max speed configurable |
| `SwimMovement` | King Poseidon | 2D free-swim with drag-based acceleration (underwater feel) |
| `Swing` | Obiiru | Pendulum motion around an anchor; period + amplitude configurable |
| `SurfaceCrawl` | Metal Gabyoall | Walk along any surface (floor/ceiling/wall); raycast-based surface detection, rotates sprite to match |
| `Teleport` | Miru Toraeru | Disappear → reappear at a chosen offset from player → brief vulnerability → teleport again |

### Attack patterns

| Component | Used by | Responsibility |
|-----------|---------|----------------|
| `SpreadShoot` | Mettaur D2 | Fire N projectiles in a cone/fan around muzzle direction; angle + count configurable |
| `RadialShoot` | Blast Raster, TriScan | Fire N projectiles evenly around 360° (or a partial arc) |
| `ChargedBeam` | Plasma Cannon, Raiden, Beam Cannon | Telegraph → sustained beam for a duration → cool down |
| `JumpAttack` | Togerics, *(Kill Fisher variant)* | Leap toward player with arc; gravity returns to ground; cooldown |

### State / lifecycle

| Component | Used by | Responsibility |
|-----------|---------|----------------|
| `HideShell` | Mettaur D2 | Cycle: hide (HurtBox off) → peek (HurtBox on + fire trigger) → hide |
| `ActivateOnDetect` | Batton Bone B81 | On `PlayerDetector.PlayerDetected`, enables a list of disabled components (for "sleep until player approaches" enemies) |
| `Spawner` | Mega Nest | Periodically instantiate spawn prefab with cap on active children via `Projectile`-style `Destroyed` event tracking |
| `SpawnOnDestroy` | Yukidarubon | On `Health.Depleted`, instantiate child prefabs at current position before the parent destroys |
| `PeriodicHazard` | Prominence, *(Kill Fisher)* | Toggle visual + collider on a cycle (active duration + cool duration) |

### Damage mediation

| Component | Used by | Responsibility |
|-----------|---------|----------------|
| `DirectionalShield` | Protecton | Toggles sibling `HurtBox.enabled` based on hit angle vs. a forward vector (hits from front = blocked) |

---

## Component revisions

Changes needed to existing components. All are additive — no breaking changes.

- **`EnemyShoot`** — Extract muzzle aim and projectile spawn into `protected` methods (`AimMuzzleAt(Vector2)`, `FireProjectile()`) so `SpreadShoot` and `ChargedBeam` can subclass rather than duplicate. No new public API.
- **`Gravity`** — Replace hardcoded `Vector2.down` with a serialized `_direction` field (default down). `SurfaceCrawl` reuses it with rotated direction.
- **`AutoShoot`** — No change. Already accepts a muzzle transform; direction flows from `muzzle.rotation`.
- **`HurtBox`** — No change. `DirectionalShield` writes to `HurtBox.enabled` externally; no revision needed.
- **`PlayerDetector`** — Add optional `_coneAngle` (0 = radial, >0 = directional cone in facing direction) so patrolling shooters stop detecting players behind them. Default 0 = current behavior.
- **`Projectile`** — No change. Add a separate `BeamProjectile` component for `ChargedBeam` — different lifecycle (sustained, doesn't self-destroy on hit), different damage model (tick-based). Don't fold into `Projectile`.
- **`SwoopAttack`** — Add optional `_returnTarget` override so Spider Core (ceiling-anchored) can return to a position other than its spawn point. Default keeps current behavior.
- **`DropTrigger`** — Possibly retire in favor of `ActivateOnDetect` + explicit Spike Marl drop behavior. Evaluate during implementation — don't commit yet.

---

## Scalability review

The 17-component composition model scales cleanly. The concerns are around code organization and content authoring, not runtime architecture.

### 1. Generator split per stage

**Trigger:** When [SkyLagoonEnemyGenerator.cs](Assets/_Project/Scripts/Editor/SkyLagoonEnemyGenerator.cs) is touched to add a sixth enemy or we start Jungle — whichever comes first.

**Action:** Refactor into per-stage generator files + a shared core.

- `Assets/_Project/Scripts/Editor/EnemyGeneratorCore.cs` — `NewEnemyRoot`, `AddMuzzle`, `SetField`, `LoadSprite`/`LoadPrefab`/`LoadMaterial`, `SavePrefab`, `AddGravity`. All current helpers move here.
- `SkyLagoonEnemyGenerator.cs`, `RecurringEnemyGenerator.cs`, `JungleEnemyGenerator.cs`, … — each registers its own `Tools/MegamanX4/Generate X Enemies` menu item.
- `AllEnemiesGenerator.cs` — a `Tools/MegamanX4/Generate All Enemies` meta-item that calls every generator.

At 40 enemies with ~15 lines each, that's ~600 lines of generator code split across 9 files instead of one 1500-line file. Much easier to jump to and review.

### 2. Prefab variants for palette/stat tweaks

**Trigger:** Next time we generate a variant (Knot Beret S, Hornet, Fly Gunner, Guardian/Death Guardian, Tentoroid RS/BS, Walk Shooter).

**Action:** Author the base enemy normally via the generator. For variants, use Unity's **Prefab Variant** feature — create from the base, override only the changed fields (sprite, tint, stats). Don't write a second generator method.

Benefits: shared-behavior edits on the base prefab propagate automatically; variant diffs are small and visible in inspector.

Affects: Knot Beret family (B → G → S), Tonboroid/Metal Hawk/Hornet, Guardian/Death Guardian, Tentoroid RS/BS, Hover Gunner/Fly Gunner, Knot Beret B / Walk Shooter.

### 3. ScriptableObject-driven stats *(defer)*

**Trigger:** First time the user asks for a balance tuning pass across many enemies.

**Action:** Introduce `EnemyData` SO with HP, contactDamage, shotDamage, shotSpeed, detectionRange, etc. Generator reads the SO, designer edits values without code changes.

Not needed yet. 40 enemies × 5 stats = 200 values; still fine to edit in code during iteration. Add the abstraction when it starts hurting.

### 4. What *doesn't* need changing

- **Component composition model** — proven. Adding 14 more components gives 31 total; still manageable, still discoverable by folder.
- **Physics matrix + layer routing** — no new layers needed. All enemies fit `Enemy` / `EnemyProjectile`.
- **Editor generator pattern** — `SerializedObject.FindProperty` + `PrefabUtility.SaveAsPrefabAsset` scales to any number of prefabs.
- **Base `Enemy` lifecycle** — remains `Depleted → Destroy`. No need for flinch, state-hub, or boss-style phases in regular enemies.

---

## Phased implementation order

Stage-by-stage rollout. Each stage lists its net-new components; anything unlisted is already implemented.

### Phase 1 — Foundation ✅ *implemented*

Sky Lagoon (7 enemies). Components: `PlayerDetector`, `AutoShoot`, `DestroyOnWallContact`, `EnemyShoot`, `PatrolWalk`, `HoverSine`, `SwoopAttack`, `DropTrigger`, `Gravity`.

### Phase 2 — Recurring extension

Build before any Maverick stage: Blast Raster, Hover Gunner, Giga Death, Plasma Cannon, Batton Bone B81, Mettaur D2, Spiky Mk-II, Raiden.

New components: `RadialShoot`, `SpreadShoot`, `HideShell`, `ActivateOnDetect`, `TrackPlayer`, `ChargedBeam`, `BeamProjectile`. Revisions: `EnemyShoot` extract protected methods.

Also Phase 2: **generator split refactor** (per Scalability §1) before adding these to avoid painting into the monolithic generator.

### Phase 3 — Jungle

Kill Fisher, Metal Gabyoall, King Poseidon, Obiiru, Mega Nest, Spider Core.

New components: `PeriodicHazard`, `SurfaceCrawl`, `SwimMovement`, `Swing`, `Spawner`. Revisions: `Gravity` direction field, `SwoopAttack` return-target override.

### Phase 4 — Cyber Space

Miru Toraeru, TriScan, Protecton.

New components: `Teleport`, `DirectionalShield`. `RadialShoot` reused.

### Phase 5 — Air Force

Beam Cannon, Metal Hawk, Walk Shooter.

No new components. All reuses. **First opportunity to use prefab variants** (Metal Hawk from Tonboroid, Walk Shooter from Knot Beret B).

### Phase 6 — Volcano + Marine Base

Prominence, Hornet.

No new components beyond Phase-2 carryover. Hornet is a Tonboroid variant.

### Phase 7 — Snow Base

E-AT, Yukidarubon, Knot Beret S, Fly Gunner, Ice Wing.

New components: `SpawnOnDestroy`. Variants: Knot Beret S (from B), Fly Gunner (from Hover Gunner).

### Phase 8 — Bio Laboratory

Tentoroid RS, Tentoroid BS, Togerics, Dejira, Guardian, Death Guardian.

New components: `JumpAttack`. Variants: Tentoroid BS (from RS), Death Guardian (from Guardian), Guardian (from Knot Beret G).

### Phase 9 — Polish

Per-enemy audio hooks, spawn volumes, stage-specific palettes, prefab variant cleanup.

---

## SVG assets needed

Existing SVGs (✅ already authored): Knot Beret B, G; Spike Marl; Kyunnbyunn; Tonboroid S; Mad Bull 97; Trap Blast.

New authored SVGs needed (33):

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
| King Poseidon | ~80×40 | Large fish, horizontal |
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

**Authoring convention:** SVG facing right (natural) OR authored facing left + flip-wrapper (`<g transform="translate(W 0) scale(-1 1)">`). Existing enemies follow the latter. Either is fine; consistency within a family is what matters.

---

## Verification

Per enemy, place in `Gameplay.unity` and confirm:

- Takes damage from player projectiles; `DamageFlash` blinks during i-frames; destroyed at 0 HP (or never, for invulnerable hazards).
- Deals listed contact/shot damage; player knockback + i-frames trigger.
- Unique behavior matches description: patrol turns at edges; detection triggers attack; swoop dives + returns; mine drops; charger self-destructs on wall; turret/cannon fires on interval; bird follows wave pattern; beam telegraphs before firing; hide/peek cycle; spawner respects cap; on-destroy splits into children; directional shield blocks front hits only.
- Projectiles on `EnemyProjectile` layer; hit player, not other enemies.
- No NullReferenceExceptions when optional components are absent (graceful null checks).

Per stage, confirm in `Gameplay.unity`:

- Stage-specific enemies spawn/destroy on entry/exit of trigger volumes.
- Recurring enemies reused across stages share a single prefab reference (or variant chain), not copy-pasted.

Per refactor (Phase 2 generator split, Phase 5 prefab variants):

- Re-running `Tools/MegamanX4/Generate *` menu items produces identical prefabs to the pre-refactor versions (diff-check before/after).
