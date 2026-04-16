# SPEC_PROJECTILES — Projectile System

Status: **Design complete, unimplemented.**

Replaces the standalone [BusterShot.cs](Assets/_Project/Scripts/BusterShot.cs) with a composable projectile system used by both player and enemy weapons. A shared `Projectile` component handles damage, hit detection, lifetime, and despawn. Separate small behavior components handle movement patterns. Prefabs compose the two.

**Relationship to other specs:**

- [SPEC_XWEAPONS.md](SPEC_XWEAPONS.md) — the weapon system spawns projectile prefabs. This spec defines what those prefabs are made of. `TwinSlasherProjectile.cs` and `FrostTowerProjectile.cs` from SPEC_XWEAPONS are superseded: Twin Slasher becomes `Projectile + StraightMovement`, Frost Tower becomes `Projectile + StationaryHazard`. `WeaponData` gains a `SpawnEntry[]` array (§6).
- [SPEC2.md](SPEC2.md) — `Projectile` calls `Health.ApplyDamage(damage, transform.position)` using the source-position overload added there.

---

## 1. Architecture overview

```
Prefab: MegamanX_Shot_Small
├── Projectile           (damage, hitLayers, piercing, lifetime, despawn)
├── StraightMovement     (speed, direction \u2014 moves via transform.position)
├── Rigidbody2D          (Kinematic, gravityScale=0)
├── Collider2D           (isTrigger=true)
└── SpriteRenderer

Prefab: FrostTower
├── Projectile           (damage, hitLayers, piercing=true, lifetime=1.5s)
├── StationaryHazard     (riseTime, fullHeight)
├── Rigidbody2D          (Kinematic, gravityScale=0)
├── Collider2D           (isTrigger=true)
└── SpriteRenderer
```

**Projectile** is the shared core — every projectile has it. It never moves the object. It only detects hits, applies damage, and manages lifetime.

**Behavior components** own movement. Each moves via `transform.position` in `Update` (or `FixedUpdate` where physics-step alignment matters). No `Rigidbody2D` dependency — behaviors work on any GameObject, including non-physics objects like background elements or visual FX. Exactly one behavior per prefab.

This is composition, not inheritance. No base class is shared between behaviors. They are independent MonoBehaviours.

---

## 2. Projectile component

```csharp
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] int damage = 1;
    [SerializeField] LayerMask hitLayers = ~0;
    [SerializeField] bool piercing;
    [SerializeField] float lifetime = 0.6f;
    float timer;

    public event Action Destroyed;

    void Awake()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if ((hitLayers.value & (1 << other.gameObject.layer)) == 0) return;

        var health = other.GetComponentInParent<Health>();
        if (health)
            health.ApplyDamage(damage, transform.position);

        if (!piercing)
            Destroy(gameObject);
    }

    void OnDestroy() => Destroyed?.Invoke();
}
```

### Key decisions

| Concern | Resolution |
|---|---|
| Damage amount | `[SerializeField]` on Projectile. Baked per prefab. WeaponData does not carry damage — the prefab is the source of truth for what it does. |
| Hit filtering | `hitLayers` LayerMask. Player projectiles filter to Enemy layer; enemy projectiles filter to Player layer. Inspector-configured. |
| Piercing | `bool piercing`. Twin Slasher = true (passes through enemies). Buster = false (destroys on first hit). |
| Lifetime | Timer in `Update`. Auto-destroys after `lifetime` seconds. |
| Off-screen despawn | Deferred. Lifetime-only for now; off-screen cleanup addressed later. |
| Despawn event | `event Action Destroyed` fired from `OnDestroy`. Same pattern as current BusterShot. Spawner subscribes at spawn time to track live-shot count. No metadata — spawner already knows which slot the shot belongs to. |
| Kinematic enforcement | `Awake` forces Kinematic + gravityScale 0, same as BusterShot and PlayerController. |

### What Projectile does NOT do

- **Movement.** No `FixedUpdate`, no `MovePosition`, no velocity. Behavior components handle this.
- **Spawn logic.** Projectile doesn't know about WeaponData, spawn patterns, or facing. It's a runtime component on a live instance.
- **Visual effects.** No particles, no trail. Those are separate components or child objects on the prefab.

---

## 3. Behavior components — implemented now

### 3.1 StraightMovement

Moves in a fixed direction at constant speed. Covers: buster shots (all tiers), Twin Slasher blades, enemy bullets. No `Rigidbody2D` dependency — also usable for background scrolling, visual effects, or any object that needs constant-velocity linear motion.

**No Initialize method.** Direction is fully determined by two prefab-baked values:

- `angleDeg` — `[SerializeField]`, baked per prefab. 0 for horizontal, +30 for upward diagonal, etc.
- `facing` — derived from `transform.lossyScale.x` at `Start`. The spawner flips `localScale.x` on the root to set direction. For composite prefabs (Twin Slasher), flipping the root flips all children.

```csharp
public class StraightMovement : MonoBehaviour
{
    [SerializeField] float speed = 18f;
    [SerializeField] float angleDeg;

    Vector2 direction;

    void Start()
    {
        int facing = (int)Mathf.Sign(transform.lossyScale.x);
        float rad = angleDeg * Mathf.Deg2Rad;
        direction = new Vector2(Mathf.Cos(rad) * facing, Mathf.Sin(rad));
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }
}
```

When used on a projectile prefab alongside `Projectile` (which requires `Rigidbody2D` + `Collider2D`), the kinematic body's collider follows the transform automatically — trigger callbacks fire as normal.

### 3.2 StationaryHazard

Spawns in place, optionally rises/scales over a riseTime, persists for a duration, then despawns. Covers: Frost Tower, Lightning Web (if treated as zone). No `Rigidbody2D` dependency — the object doesn't translate, only scales.

**No Initialize method.** Facing is derived from `transform.lossyScale.x` (set by the spawner flipping `localScale.x` on the root). All configuration is baked via `[SerializeField]`.

```csharp
public class StationaryHazard : MonoBehaviour
{
    [SerializeField] float riseTime = 0.15f;
    [SerializeField] float riseAxis = 1f;   // 1 = Y (vertical rise), 0 = uniform

    Vector3 targetScale;
    float timer;

    void Start()
    {
        targetScale = transform.localScale;
        transform.localScale = new Vector3(
            targetScale.x,
            riseAxis > 0.5f ? 0f : targetScale.y,
            targetScale.z);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer <= riseTime)
        {
            float t = Mathf.Clamp01(timer / riseTime);
            var s = targetScale;
            if (riseAxis > 0.5f) s.y = targetScale.y * t;
            transform.localScale = s;
        }
    }
}
```

Lifetime and despawn are handled by `Projectile.lifetime` — StationaryHazard doesn't duplicate that logic.

---

## 4. Behavior components — planned (not implemented)

Specced here so future weapon specs can reference them by name. Each follows the same pattern: no `Rigidbody2D` dependency, no `Initialize` method, all config baked via `[SerializeField]`, facing derived from `transform.lossyScale.x` in `Start`, movement via `transform.position` in `Update`.

### 4.1 ArcMovement

Moves forward with simulated gravity. Covers: Rising Fire, enemy lobbed shots.

SerializeFields: `float speed`, `float launchAngle`, `float arcGravity`.

Maintains a velocity vector. Each Update: `velocity.y -= arcGravity * dt`, then `transform.position += velocity * dt`.

### 4.2 GroundCrawlMovement

Moves horizontally, sticking to the ground surface below. Covers: Ground Hunter.

SerializeFields: `float speed`, `LayerMask groundLayers`.

Each Update: `Physics2D.Raycast` downward to find ground, snap Y to surface, advance X at speed. Despawns if no ground found (ledge edge).

### 4.3 HomingMovement

Tracks a target and steers toward it. Covers: Aiming Laser.

SerializeFields: `float speed`, `float turnSpeed`. Target acquired at `Start` via nearest enemy query (or assigned by spawner via a public `Target` property — the one exception where a setter is needed, since the target can't be baked into a prefab).

Each Update: rotate direction toward target, advance at speed.

### 4.4 OrbitalMovement

Orbits around a point. Covers: Kuuenzan (Zero's spinning slash), Double Cyclone.

SerializeFields: `float radius`, `float angularSpeed`. Center point set by spawner via a public `Center` property (can't be baked — needs the player's live transform).

Each Update: advance angle, compute position on circle, set `transform.position`.

### 4.5 BoomerangMovement

Moves outward, decelerates, reverses, returns to spawn point. Covers: potential future weapons.

SerializeFields: `float outSpeed`, `float returnSpeed`, `float hangTime`.

---

## 5. BusterShot migration

[BusterShot.cs](Assets/_Project/Scripts/BusterShot.cs) is deleted. Its responsibilities split:

| BusterShot responsibility | New owner |
|---|---|
| `Awake`: Kinematic + gravityScale | `Projectile.Awake` |
| `Fire(direction)`: set scale + direction | Spawner flips `localScale.x`; `StraightMovement.Start` reads it |
| `OnTriggerEnter2D`: layer check + damage + destroy | `Projectile.OnTriggerEnter2D` |
| `Destroyed` event | `Projectile.Destroyed` |
| `speed`, `lifetime`, `damage`, `hitLayers` fields | Split: `speed` on `StraightMovement`, rest on `Projectile` |

### Prefab updates

The three buster shot prefabs (`MegamanX_Shot_{Small,Semi,Full}`) are re-authored:

- Remove `BusterShot` component.
- Add `Projectile` component (configure damage, hitLayers, lifetime, piercing=false).
- Add `StraightMovement` component (configure speed).

### PlayerBuster / WeaponInventory impact

`PlayerBuster`'s lemon-cap tracking currently references `BusterShot`. After migration:

```csharp
// Before:
var shot = go.GetComponent<BusterShot>();
shot.Fire(facing);
activeSmallShots.Add(shot);
shot.Destroyed += () => activeSmallShots.Remove(shot);

// After:
var s = go.transform.localScale;
s.x = Mathf.Abs(s.x) * facing;
go.transform.localScale = s;
var projectile = go.GetComponent<Projectile>();
activeSmallShots.Add(projectile);
projectile.Destroyed += () => activeSmallShots.Remove(projectile);
```

`activeSmallShots` changes type from `List<BusterShot>` to `List<Projectile>`.

---

## 6. WeaponData spawn descriptor (updates SPEC_XWEAPONS)

`WeaponData` gains a serialized array describing what to spawn per fire. Angles and behavior config are baked into prefabs — `SpawnEntry` only carries the prefab reference and a position offset:

```csharp
[System.Serializable]
public struct SpawnEntry
{
    public GameObject prefab;
    public Vector2 positionOffset;   // relative to muzzle, in facing-local space
}

// In WeaponData:
[Header("Spawn Pattern")]
public SpawnEntry[] spawnPattern = { new() };
public SpawnEntry[] chargedSpawnPattern;
```

### Examples

**Base Buster** (small lemon):

```
spawnPattern: [{ prefab: SmallShot, offset: (0,0) }]
chargedSpawnPattern: null   // handled specially by PlayerBuster tiers
```

**Twin Slasher** (uncharged) — single composite prefab:

```
spawnPattern: [{ prefab: TwinSlasher, offset: (0,0) }]
```

The `TwinSlasher` prefab is a composite with two child blades (see §8):

```
TwinSlasher (root)
├── DestroyWhenChildless     (cleans up root when all blades expire)
├── Blade_Up (child)         angleDeg=+30 baked in StraightMovement
│   ├── Projectile + StraightMovement + Rigidbody2D + Collider2D
├── Blade_Down (child)       angleDeg=-30 baked in StraightMovement
│   ├── Projectile + StraightMovement + Rigidbody2D + Collider2D
```

**Twin Slasher** (charged) — composite with four blades:

```
chargedSpawnPattern: [{ prefab: TwinSlasherCharged, offset: (0,0) }]
```

Same structure, four children at ±30° and ±15°.

**Frost Tower** (uncharged):

```
spawnPattern: [{ prefab: FrostTower, offset: (0, -0.5) }]
// offset.y negative = spawn at feet
```

### DestroyWhenChildless

Small utility for composite projectile prefabs. Cleans up the empty root after all children are destroyed:

```csharp
public class DestroyWhenChildless : MonoBehaviour
{
    bool started;

    void Update()
    {
        if (!started && transform.childCount > 0) started = true;
        if (started && transform.childCount == 0) Destroy(gameObject);
    }
}
```

Only needed on composite prefabs (Twin Slasher). Single-object prefabs (buster, Frost Tower) don't use it.

### WeaponInventory spawn loop

```csharp
void SpawnWeaponShots(WeaponSlot slot, bool charged)
{
    var pattern = charged ? slot.data.chargedSpawnPattern : slot.data.spawnPattern;
    if (pattern == null || pattern.Length == 0) return;
    if (slot.liveShots.Count >= slot.data.maxOnScreen) return;

    Vector2 muzzle = controller.MuzzleAnchor
        ? (Vector2)controller.MuzzleAnchor.position
        : (Vector2)controller.transform.position;
    int facing = controller.Facing;

    foreach (var entry in pattern)
    {
        Vector2 offset = new(entry.positionOffset.x * facing, entry.positionOffset.y);
        var go = Instantiate(entry.prefab, muzzle + offset, Quaternion.identity);

        // Flip scale to set facing; behaviors read lossyScale.x in Start.
        var s = go.transform.localScale;
        s.x = Mathf.Abs(s.x) * facing;
        go.transform.localScale = s;

        // Track for on-screen cap (check root and children for Projectile)
        foreach (var proj in go.GetComponentsInChildren<Projectile>())
        {
            slot.liveShots.Add(go);
            proj.Destroyed += () => slot.liveShots.Remove(go);
        }
    }
}
```

No behavior-specific initialization calls. The spawner's only job beyond `Instantiate` is flipping `localScale.x` for facing.

---

## 7. Enemy projectiles

Enemies use the same `Projectile + Behavior` composition. The only difference is `hitLayers`:

- Player projectile prefabs: `hitLayers` = Enemy layer.
- Enemy projectile prefabs: `hitLayers` = Player layer.

Enemy spawning follows the same pattern: instantiate prefab, flip `localScale.x` for facing. Enemy scripts call this directly — no `WeaponInventory` needed on the enemy side.

```csharp
// Example: enemy turret fires a straight shot
var go = Instantiate(bulletPrefab, muzzle, Quaternion.identity);
var s = go.transform.localScale;
s.x = Mathf.Abs(s.x) * facing;
go.transform.localScale = s;
```

No changes to `Projectile` or behaviors. Layer configuration in the prefab handles friend/foe.

---

## 8. Prefab catalog

### Implemented in this spec

| Prefab | Components | Notes |
|---|---|---|
| `MegamanX_Shot_Small` | Projectile + StraightMovement | Refactored from BusterShot. damage=1, speed=18, piercing=false |
| `MegamanX_Shot_Semi` | Projectile + StraightMovement | damage=2, speed=18, piercing=false |
| `MegamanX_Shot_Full` | Projectile + StraightMovement | damage=4, speed=14, piercing=true, lifetime=0.8 |
| `TwinSlasher` | Root: DestroyWhenChildless. 2 children: Projectile + StraightMovement (±30°) | damage=2, speed=10, piercing=true, lifetime=0.6 |
| `TwinSlasherCharged` | Root: DestroyWhenChildless. 4 children: Projectile + StraightMovement (±30°, ±15°) | damage=3, speed=10, piercing=true, lifetime=0.6 |
| `FrostTower` | Projectile + StationaryHazard | damage=3, piercing=true, lifetime=1.5 |
| `FrostTowerCharged` | Projectile + StationaryHazard | damage=5, piercing=true, lifetime=2.5, taller |

### Future (one prefab per weapon, added with their specs)

| Weapon | Behavior | Notes |
|---|---|---|
| Lightning Web | StationaryHazard or custom | Web zone persists on surface |
| Aiming Laser | HomingMovement | Locks onto nearest enemy |
| Double Cyclone | StraightMovement (angled) or OrbitalMovement | Two tornadoes, upward angle |
| Rising Fire | ArcMovement | Arcs upward with gravity |
| Ground Hunter | GroundCrawlMovement | Surface-following |
| Soul Body | StraightMovement | Clone travels forward, slow |

---

## 9. Cross-cutting

### DamageFlash

Projectile calls `Health.ApplyDamage(damage, transform.position)` — the source-position overload from [SPEC2.md](SPEC2.md). If the target has `DamageFlash`, the hit feedback fires automatically.

### On-screen cap

`WeaponInventory` tracks `slot.liveShots` (a `List<GameObject>`). Before spawning, checks `liveShots.Count >= data.maxOnScreen`. On `Projectile.Destroyed`, removes the entry. `PlayerBuster` tracks its own `activeSmallShots` for the buster lemon cap (3), using `List<Projectile>` instead of the old `List<BusterShot>`.

### Knockback

Projectiles are not affected by player knockback. They live independently once spawned. If the player is knocked back mid-charge, `PlayerBuster.CancelCharge()` fires (per SPEC2), but any already-spawned projectiles continue their trajectory.

### Ladder

Shooting on ladder fires the active weapon as normal. The shoot-halt-and-lock-facing behavior is handled by `WeaponInventory` / `PlayerController.TriggerLadderShootLock()` (per SPEC.md §3.4). Projectiles spawn from `muzzleAnchor` regardless of player state.

---

## 10. Testing plan

EditMode tests (requires `.asmdef` setup from README §12):

- **Projectile**
  - Hit detection respects `hitLayers`.
  - `piercing = false` → destroy on first trigger hit.
  - `piercing = true` → survives trigger hit.
  - `Destroyed` event fires on destroy.
  - Lifetime auto-destroy.
- **StraightMovement**
  - `Initialize(1, 0f)` → direction = (1, 0). `Initialize(-1, 30f)` → correct angle.
  - Update advances position by `direction * speed * dt`.
- **StationaryHazard**
  - Scale starts at 0 (Y axis), reaches full after `riseTime`.

Manual QA:
- Fire buster → shot behaves identically to old BusterShot.
- Equip Twin Slasher → fire → two blades at ±30°, pierce through enemies.
- Equip Frost Tower → fire → pillar rises, persists, damages on contact, shatters at lifetime.
- Fire into an empty screen → projectile despawns after lifetime expires.
- Enemy fires at player → same Projectile, hits Player layer.

---

## 11. Implementation order

1. **Projectile.cs** — create the component per §2.
2. **StraightMovement.cs** — create per §3.1.
3. **StationaryHazard.cs** — create per §3.2.
4. **DestroyWhenChildless.cs** — create per §6.
5. **Refactor buster prefabs** — remove `BusterShot`, add `Projectile + StraightMovement` (`angleDeg=0`). Update `PlayerBuster` (from SPEC_XWEAPONS) to flip `localScale.x` and track `List<Projectile>` instead of `List<BusterShot>`. Re-author the three shot prefabs.
6. **Delete BusterShot.cs** — only after step 5 is verified.
7. **Add SpawnEntry[] to WeaponData** — per §6.
8. **Update WeaponInventory.SpawnWeaponShots** — per §6 spawn loop (scale-flip, no Initialize calls).
9. **Author Twin Slasher composite prefab** — root with `DestroyWhenChildless`, two children each with `Projectile + StraightMovement` (±30° baked). WeaponData asset with 1-entry spawn pattern.
10. **Author Frost Tower prefab** — `Projectile + StationaryHazard`, WeaponData asset with 1-entry spawn pattern.
11. Verify all buster + special weapon fire works end-to-end.

Steps 1–5 are the critical path. The projectile system must work with the existing buster before any specials are added. If buster behavior regresses, fix before proceeding.

---

## 12. Files summary

### New

| File | Type |
|---|---|
| `Assets/_Project/Scripts/Projectile.cs` | MonoBehaviour |
| `Assets/_Project/Scripts/StraightMovement.cs` | MonoBehaviour |
| `Assets/_Project/Scripts/StationaryHazard.cs` | MonoBehaviour |
| `Assets/_Project/Scripts/DestroyWhenChildless.cs` | MonoBehaviour (utility for composite prefabs) |

### Modified

| File | Change |
|---|---|
| `WeaponData.cs` | Add `SpawnEntry` struct + `spawnPattern` / `chargedSpawnPattern` arrays |
| `WeaponInventory.cs` | Use `SpawnWeaponShots` loop with scale-flip for facing |
| `PlayerBuster.cs` | Track `List<Projectile>` instead of `List<BusterShot>` |

### Deleted

| File | Reason |
|---|---|
| `BusterShot.cs` | Superseded by `Projectile + StraightMovement` |
