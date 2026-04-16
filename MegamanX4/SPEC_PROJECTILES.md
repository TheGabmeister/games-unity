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
├── StraightMovement     (speed, direction \u2014 calls rb.MovePosition)
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

**Behavior components** own movement. Each grabs `Rigidbody2D` in `Awake` and calls `rb.MovePosition` in its own `FixedUpdate`. Exactly one behavior per prefab.

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
    [SerializeField] float offScreenMargin = 1f;

    float timer;
    Renderer cachedRenderer;

    public event Action Destroyed;

    void Awake()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        cachedRenderer = GetComponentInChildren<Renderer>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime) { Destroy(gameObject); return; }
        if (cachedRenderer && !cachedRenderer.isVisible && timer > 0.1f)
            Destroy(gameObject);
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
| Off-screen despawn | After a 0.1 s grace period (prevents despawn on the spawn frame before the renderer registers as visible), checks `Renderer.isVisible`. Destroys if off-screen. |
| Despawn event | `event Action Destroyed` fired from `OnDestroy`. Same pattern as current BusterShot. Spawner subscribes at spawn time to track live-shot count. No metadata — spawner already knows which slot the shot belongs to. |
| Kinematic enforcement | `Awake` forces Kinematic + gravityScale 0, same as BusterShot and PlayerController. |

### What Projectile does NOT do

- **Movement.** No `FixedUpdate`, no `MovePosition`, no velocity. Behavior components handle this.
- **Spawn logic.** Projectile doesn't know about WeaponData, spawn patterns, or facing. It's a runtime component on a live instance.
- **Visual effects.** No particles, no trail. Those are separate components or child objects on the prefab.

---

## 3. Behavior components — implemented now

### 3.1 StraightMovement

Moves in a fixed direction at constant speed. Covers: buster shots (all tiers), Twin Slasher blades, enemy bullets.

```csharp
[RequireComponent(typeof(Rigidbody2D))]
public class StraightMovement : MonoBehaviour
{
    [SerializeField] float speed = 18f;

    Rigidbody2D rb;
    Vector2 direction;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    public void Initialize(int facing, float angleDeg = 0f)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        direction = new Vector2(Mathf.Cos(rad) * facing, Mathf.Sin(rad));
        // Flip sprite to match direction.
        var s = transform.localScale;
        s.x = Mathf.Abs(s.x) * facing;
        transform.localScale = s;
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
    }
}
```

**Initialize parameters:**

- `facing`: +1 or -1 (from `PlayerController.Facing` or enemy facing).
- `angleDeg`: 0 for horizontal, +30 for upward diagonal, -30 for downward. Default 0 for straight-line shots.

### 3.2 StationaryHazard

Spawns in place, optionally rises/scales over a riseTime, persists for a duration, then despawns. Covers: Frost Tower, Lightning Web (if treated as zone). No `Rigidbody2D.MovePosition` — the object doesn't translate.

```csharp
public class StationaryHazard : MonoBehaviour
{
    [SerializeField] float riseTime = 0.15f;
    [SerializeField] float riseAxis = 1f;   // 1 = Y (vertical rise), 0 = uniform

    Vector3 targetScale;
    float timer;

    public void Initialize(int facing)
    {
        // facing can orient the hazard if needed (e.g., angled spike).
        var s = transform.localScale;
        s.x = Mathf.Abs(s.x) * facing;
        transform.localScale = s;
    }

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

Specced here so future weapon specs can reference them by name. Each follows the same pattern: `[RequireComponent(typeof(Rigidbody2D))]`, grabs `rb` in `Awake`, `Initialize(...)` sets parameters, `FixedUpdate` calls `rb.MovePosition`.

### 4.1 ArcMovement

Moves forward with simulated gravity. Covers: Rising Fire, enemy lobbed shots.

```
Initialize(int facing, float launchAngle, float arcGravity)
```

Maintains a velocity vector. Each FixedUpdate: `velocity.y -= arcGravity * dt`, then `MovePosition(pos + velocity * dt)`.

### 4.2 GroundCrawlMovement

Moves horizontally, sticking to the ground surface below. Covers: Ground Hunter.

```
Initialize(int facing)
```

Each FixedUpdate: cast downward to find ground, snap Y to surface, advance X at speed. Despawns if no ground found (ledge edge).

### 4.3 HomingMovement

Tracks a target and steers toward it. Covers: Aiming Laser.

```
Initialize(Transform target, float turnSpeed)
```

Each FixedUpdate: rotate direction toward target, advance at speed. `turnSpeed` controls how aggressively it tracks.

### 4.4 OrbitalMovement

Orbits around the player (or a point). Covers: Kuuenzan (Zero's spinning slash), Double Cyclone.

```
Initialize(Transform center, float radius, float angularSpeed)
```

Each FixedUpdate: advance angle, compute position on circle, `MovePosition`.

### 4.5 BoomerangMovement

Moves outward, decelerates, reverses, returns to spawn point. Covers: potential future weapons.

```
Initialize(int facing, float outSpeed, float returnSpeed, float hangTime)
```

---

## 5. BusterShot migration

[BusterShot.cs](Assets/_Project/Scripts/BusterShot.cs) is deleted. Its responsibilities split:

| BusterShot responsibility | New owner |
|---|---|
| `Awake`: Kinematic + gravityScale | `Projectile.Awake` |
| `Fire(direction)`: set scale + velocity | `StraightMovement.Initialize(facing)` |
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
var straight = go.GetComponent<StraightMovement>();
straight.Initialize(facing);
var projectile = go.GetComponent<Projectile>();
activeSmallShots.Add(projectile);
projectile.Destroyed += () => activeSmallShots.Remove(projectile);
```

`activeSmallShots` changes type from `List<BusterShot>` to `List<Projectile>`.

---

## 6. WeaponData spawn descriptor (updates SPEC_XWEAPONS)

`WeaponData` gains a serialized array describing what to spawn per fire:

```csharp
[System.Serializable]
public struct SpawnEntry
{
    public GameObject prefab;
    public float angleDeg;
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
spawnPattern: [{ prefab: SmallShot, angleDeg: 0, offset: (0,0) }]
chargedSpawnPattern: null   // handled specially by PlayerBuster tiers
```

**Twin Slasher** (uncharged):

```
spawnPattern: [
    { prefab: TwinSlasherBlade, angleDeg: +30, offset: (0, 0.1) },
    { prefab: TwinSlasherBlade, angleDeg: -30, offset: (0, -0.1) }
]
```

**Twin Slasher** (charged):

```
chargedSpawnPattern: [
    { prefab: TwinSlasherBlade, angleDeg: +30, offset: (0, 0.1) },
    { prefab: TwinSlasherBlade, angleDeg: -30, offset: (0, -0.1) },
    { prefab: TwinSlasherBlade, angleDeg: +15, offset: (0, 0.05) },
    { prefab: TwinSlasherBlade, angleDeg: -15, offset: (0, -0.05) }
]
```

**Frost Tower** (uncharged):

```
spawnPattern: [{ prefab: FrostTower, angleDeg: 0, offset: (0, -0.5) }]
// offset.y negative = spawn at feet
```

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

        // Initialize behavior (StraightMovement, StationaryHazard, etc.)
        if (go.TryGetComponent<StraightMovement>(out var straight))
            straight.Initialize(facing, entry.angleDeg);
        else if (go.TryGetComponent<StationaryHazard>(out var hazard))
            hazard.Initialize(facing);

        // Track for on-screen cap
        if (go.TryGetComponent<Projectile>(out var proj))
        {
            slot.liveShots.Add(go);
            proj.Destroyed += () => slot.liveShots.Remove(go);
        }
    }
}
```

The `TryGetComponent` cascade is intentionally explicit. An interface (`IProjectileBehavior`) was considered and rejected — it adds an abstraction layer with no current consumer beyond this one call site. If a fourth or fifth behavior type makes the cascade unwieldy, introduce the interface then.

---

## 7. Enemy projectiles

Enemies use the same `Projectile + Behavior` composition. The only difference is `hitLayers`:

- Player projectile prefabs: `hitLayers` = Enemy layer.
- Enemy projectile prefabs: `hitLayers` = Player layer.

Enemy spawning follows the same pattern: instantiate prefab, call `behavior.Initialize(facing, ...)`. Enemy scripts call this directly — no `WeaponInventory` needed on the enemy side.

```csharp
// Example: enemy turret fires a straight shot
var go = Instantiate(bulletPrefab, muzzle, Quaternion.identity);
go.GetComponent<StraightMovement>().Initialize(facing);
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
| `TwinSlasherBlade` | Projectile + StraightMovement | damage=2, speed=10, piercing=true, lifetime=0.6 |
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
  - FixedUpdate advances position by `direction * speed * dt`.
- **StationaryHazard**
  - Scale starts at 0 (Y axis), reaches full after `riseTime`.

Manual QA:
- Fire buster → shot behaves identically to old BusterShot.
- Equip Twin Slasher → fire → two blades at ±30°, pierce through enemies.
- Equip Frost Tower → fire → pillar rises, persists, damages on contact, shatters at lifetime.
- Fire into an empty screen → projectile despawns when off-camera.
- Enemy fires at player → same Projectile, hits Player layer.

---

## 11. Implementation order

1. **Projectile.cs** — create the component per §2.
2. **StraightMovement.cs** — create per §3.1.
3. **StationaryHazard.cs** — create per §3.2.
4. **Refactor buster prefabs** — remove `BusterShot`, add `Projectile + StraightMovement`. Update `PlayerBuster` (from SPEC_XWEAPONS) to use `Projectile` and `StraightMovement` references instead of `BusterShot`. Re-author the three shot prefabs.
5. **Delete BusterShot.cs** — only after step 4 is verified.
6. **Add SpawnEntry[] to WeaponData** — per §6.
7. **Update WeaponInventory.SpawnWeaponShots** — per §6 spawn loop.
8. **Author Twin Slasher prefab** — `Projectile + StraightMovement`, WeaponData asset with 2-entry spawn pattern.
9. **Author Frost Tower prefab** — `Projectile + StationaryHazard`, WeaponData asset with 1-entry spawn pattern.
10. Verify all buster + special weapon fire works end-to-end.

Steps 1–5 are the critical path. The projectile system must work with the existing buster before any specials are added. If buster behavior regresses, fix before proceeding.

---

## 12. Files summary

### New

| File | Type |
|---|---|
| `Assets/_Project/Scripts/Projectile.cs` | MonoBehaviour |
| `Assets/_Project/Scripts/StraightMovement.cs` | MonoBehaviour |
| `Assets/_Project/Scripts/StationaryHazard.cs` | MonoBehaviour |

### Modified

| File | Change |
|---|---|
| `WeaponData.cs` | Add `SpawnEntry` struct + `spawnPattern` / `chargedSpawnPattern` arrays |
| `WeaponInventory.cs` | Use `SpawnWeaponShots` loop with behavior initialization |
| `PlayerBuster.cs` | Track `List<Projectile>` instead of `List<BusterShot>` |

### Deleted

| File | Reason |
|---|---|
| `BusterShot.cs` | Superseded by `Projectile + StraightMovement` |
