# SPEC_SKYLAGOON_ENEMIES

## Context

Sky Lagoon is the intro stage of Mega Man X4. Its enemy roster introduces the player to core combat before the eight Maverick stages. The existing enemy system ([Enemy.cs](Assets/_Project/Scripts/Enemy.cs) + [Health.cs](Assets/_Project/Scripts/Health.cs) + [HitBox.cs](Assets/_Project/Scripts/HitBox.cs)) handles lifecycle and damage. The only prefab so far is `TargetDummy`. This spec covers the seven regular enemies — Eregion (boss) is out of scope.

## Existing infrastructure

Components already available for composition:

| Component | Role |
|-----------|------|
| `Enemy` | Lifecycle: subscribes to `Health.Depleted` → `Destroy(gameObject)` |
| `Health` | HP pool, i-frames, damage events |
| `HitBox` | Deals damage on trigger/collision contact to any `HurtBox` |
| `HurtBox` | Receives hits, routes to `Health.ApplyDamage` |
| `Projectile` | Hit detection, damage, piercing flag, `Destroyed` event |
| `MoveForward` | Translates along `transform.right` at fixed speed |
| `MoveVertical` | Translates along `transform.up` (Up/Down enum) |
| `Lifetime` | Auto-destroy timer |

Layers: `Enemy` (body), `EnemyProjectile` (shots). Physics2D matrix already routes `EnemyProjectile ↔ Player, Environment` and `PlayerProjectile ↔ Enemy, Environment`.

## Enemy roster

---

### 1. Knot Beret B (blue soldier)

**In-game behavior:** The most common enemy. Walks along platforms on patrol. When the player enters range, stops, faces the player, and fires a 3-round burst from a rifle. Resumes patrol after firing. Falls off ledges if not edge-guarded.

| Stat | Value |
|------|-------|
| HP | 1 |
| Contact damage | 2 |
| Shot damage | 2 |
| Detection range | ~8 units horizontal |
| Patrol speed | ~2 u/s |

**New components needed:**

- **`PatrolWalk`** — Walks in the facing direction at `_speed`. Uses a short downward raycast at the front edge to detect ledges and a forward raycast to detect walls. On either: flip facing direction. Requires `Rigidbody2D` (Kinematic). Exposes `bool IsPatrolling` and `void Pause()` / `void Resume()` so other components can halt movement (e.g. while shooting).

- **`PlayerDetector`** — Casts horizontally in the facing direction up to `_range`. Fires `event Action PlayerDetected` when the player enters range and `event Action PlayerLost` when they leave. Exposes `bool CanSeePlayer` and `Vector2 PlayerPosition`. Uses `Physics2D.Raycast` on the Player layer; optionally blocked by Environment (line-of-sight).

- **`EnemyShoot`** — On detection, faces the player, pauses patrol, fires `_burstCount` projectiles at `_burstInterval` from a muzzle transform, then resumes patrol after `_cooldown`. Projectile prefab goes on `EnemyProjectile` layer. Reusable across any enemy that shoots.

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `PatrolWalk` + `PlayerDetector` + `EnemyShoot`

---

### 2. Knot Beret G (green soldier)

**In-game behavior:** Stationary variant. Stands in place, faces the player when detected, fires a single aimed shot on a cooldown. Does not patrol. Sometimes positioned on elevated platforms or behind cover.

| Stat | Value |
|------|-------|
| HP | 1 |
| Contact damage | 2 |
| Shot damage | 2 |
| Detection range | ~10 units horizontal |

**Implementation:** Same composition as Knot Beret B but without `PatrolWalk`. `EnemyShoot` configured with `_burstCount = 1` and a longer `_cooldown`. Different SVG (green palette).

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `PlayerDetector` + `EnemyShoot`

---

### 3. Tonboroid S (dragonfly)

**In-game behavior:** Hovering flying enemy. Bobs up and down in a sine-wave idle pattern. When the player comes within range, breaks from hover and dives diagonally toward the player's position at the moment of attack, then pulls back up and returns to hovering. One of the first airborne threats.

| Stat | Value |
|------|-------|
| HP | 1 |
| Contact damage | 3 |
| Hover amplitude | ~1 unit |
| Hover period | ~2 seconds |
| Swoop speed | ~8 u/s |

**New components needed:**

- **`HoverSine`** — Oscillates position vertically around a center point using `sin(Time.time * frequency) * amplitude`. Exposes `void Pause()` / `void Resume()` so swooping can take over movement.

- **`SwoopAttack`** — On `PlayerDetected`, records the player's current position as the target, pauses hover, dives toward that point at `_swoopSpeed`. On reaching the target (or a proximity threshold), reverses direction back to the original hover center. After returning, resumes hover and enters a cooldown before the next swoop.

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `PlayerDetector` + `HoverSine` + `SwoopAttack`

---

### 4. Spike Marl (ceiling mine)

**In-game behavior:** Spiky ball attached to the ceiling. Completely inert until the player walks directly beneath it, then detaches and drops straight down. Deals high contact damage. Destroyed on impact with the ground (or the player). Cannot be destroyed by normal shots while attached (invulnerable until triggered).

| Stat | Value |
|------|-------|
| HP | 1 (after dropping) |
| Contact damage | 4 |
| Drop trigger range | ~2 units horizontal below |
| Fall speed | gravity-driven |

**New component needed:**

- **`DropTrigger`** — Uses `Physics2D.OverlapBox` or a short downward raycast each frame to check for the player beneath. On detection: disables invulnerability on `Health`, enables gravity (set `Rigidbody2D.bodyType = Dynamic`, `gravityScale = 3`), and disables the trigger check. A separate `OnCollisionEnter2D` with Environment layer → `Destroy(gameObject)`.

**Composition:** `Enemy` + `Health(invulnerabilityDuration=999)` + `HurtBox` + `HitBox` + `DropTrigger`

Note: While attached, HP invulnerability prevents the player from destroying it. `DropTrigger` sets `invulnerabilityDuration = 0` on drop so it becomes vulnerable mid-fall.

---

### 5. Mad Bull 97 (charging mech)

**In-game behavior:** Large armored enemy. Spawns off-screen and charges straight toward the player at high speed. Does not stop, turn, or patrol — pure forward rush. High HP, high contact damage. Destroyed on hitting a wall if not killed first. The "oh crap" enemy of the stage.

| Stat | Value |
|------|-------|
| HP | 10 |
| Contact damage | 4 |
| Charge speed | ~10 u/s |

**Implementation:** No new component needed. `MoveForward` already handles constant forward translation. A wall raycast or `OnCollisionEnter2D` with Environment self-destructs it.

**New component needed:**

- **`DestroyOnWallContact`** — `OnCollisionEnter2D`: if the other collider is on the Environment layer, `Destroy(gameObject)`. Tiny single-purpose component.

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `MoveForward(speed=10)` + `DestroyOnWallContact`

Note: Spawned facing the player's direction by the stage layout (hand-placed or triggered by a spawn volume).

---

### 6. Trap Blast (stationary turret)

**In-game behavior:** Fixed cannon mounted on a wall or floor. Fires a single projectile at regular intervals in a fixed direction (not aimed). Cannot be destroyed — infinite HP or no `Health`. Pure hazard.

| Stat | Value |
|------|-------|
| HP | invulnerable |
| Shot damage | 2 |
| Fire interval | ~2 seconds |

**New component needed:**

- **`AutoShoot`** — Fires a projectile prefab from a muzzle transform at a fixed `_interval`. No player detection — always firing. Direction determined by the muzzle's `transform.right`. Simpler than `EnemyShoot` (no burst, no cooldown state, no detection dependency).

**Composition:** `HitBox` + `AutoShoot`. No `Enemy` or `Health` (invulnerable turret). If we want it destroyable in a future pass, add `Enemy` + `Health`.

---

### 7. Kyunnbyunn (swooping bird)

**In-game behavior:** Small bird enemy that spawns off-screen and flies in a sine-wave pattern horizontally toward the player. Does not stop or turn — flies across the screen and despawns. Often appears in groups of 2–3 from the same side. Low HP, low damage, but the wave pattern makes them tricky to hit.

| Stat | Value |
|------|-------|
| HP | 10 |
| Contact damage | 2 |
| Fly speed | ~6 u/s horizontal |
| Wave amplitude | ~1.5 units |
| Wave frequency | ~3 Hz |

**Implementation:** Combines existing `MoveForward` for horizontal travel with `HoverSine` for the vertical wave.

**Composition:** `Enemy` + `Health` + `HurtBox` + `HitBox` + `MoveForward(speed=6)` + `HoverSine(amplitude=1.5, frequency=3)` + `Lifetime(duration=10)`

`Lifetime` ensures cleanup if the bird flies off-screen without being killed.

---

## New components summary

| Component | Used by | Responsibility |
|-----------|---------|----------------|
| `PatrolWalk` | Knot Beret B | Walk + edge/wall turn + pausable |
| `PlayerDetector` | Knot Beret B, G, Tonboroid S | Range-based player detection with events |
| `EnemyShoot` | Knot Beret B, G | Burst-fire projectile spawner with cooldown |
| `HoverSine` | Tonboroid S, Kyunnbyunn | Sine-wave vertical oscillation, pausable |
| `SwoopAttack` | Tonboroid S | Dive-to-point and return |
| `DropTrigger` | Spike Marl | Detect player below, enable gravity |
| `DestroyOnWallContact` | Mad Bull 97 | Self-destruct on Environment collision |
| `AutoShoot` | Trap Blast | Periodic fire-and-forget projectile spawner |

All are single-responsibility MonoBehaviours. No inheritance hierarchies. Each enemy is a prefab composing the subset it needs.

## Phased implementation order

**Phase 1 — Foundation + simplest enemies:**
1. `PlayerDetector` — needed by most enemies; test it in isolation first.
2. `AutoShoot` — simplest shooter (no detection, no burst). Wire a Trap Blast prefab.
3. `DestroyOnWallContact` — trivial. Wire a Mad Bull 97 prefab with `MoveForward`.

**Phase 2 — Soldier enemies:**
4. `EnemyShoot` — burst-fire with cooldown, depends on `PlayerDetector`.
5. `PatrolWalk` — edge + wall detection patrol.
6. Wire Knot Beret G (stationary shooter) and Knot Beret B (patrolling shooter).

**Phase 3 — Flying enemies:**
7. `HoverSine` — sine-wave oscillation.
8. Wire Kyunnbyunn (forward + wave + lifetime — no new code beyond HoverSine).
9. `SwoopAttack` — dive-and-return, depends on `PlayerDetector` + `HoverSine`.
10. Wire Tonboroid S.

**Phase 4 — Spike Marl:**
11. `DropTrigger` — ceiling mine with gravity drop.
12. Wire Spike Marl prefab.

## SVG assets needed

One SVG per enemy, authored in the existing project style (procedural, flat shapes, layered glow/body/highlight). All authored facing right per project convention. Authored by hand or requested from Claude.

| Enemy | Dimensions | Notes |
|-------|-----------|-------|
| Knot Beret B | ~32×48 | Blue humanoid soldier with rifle |
| Knot Beret G | ~32×48 | Green variant of above |
| Tonboroid S | ~48×32 | Dragonfly, wings spread horizontally |
| Spike Marl | ~32×32 | Spiky ball/mine, symmetric |
| Mad Bull 97 | ~64×48 | Large armored charging mech |
| Trap Blast | ~32×32 | Compact cannon/turret |
| Kyunnbyunn | ~32×24 | Small bird, wings spread |

## Verification

For each enemy, place it in `Gameplay.unity` and confirm:
- Takes damage from player projectiles → flashes → destroyed at 0 HP.
- Deals contact damage to the player → knockback + i-frames.
- Unique behavior works (patrol turns at edges, detection triggers shooting, swoop dives and returns, mine drops, charger self-destructs on wall, turret fires on interval, bird follows wave pattern).
- No NullReferenceExceptions when components are missing (graceful null checks).
- Enemy projectiles are on `EnemyProjectile` layer and hit the player but not other enemies.
