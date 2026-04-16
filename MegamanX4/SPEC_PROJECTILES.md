# SPEC_PROJECTILES — Projectile System

Status: **Core components implemented.** [Projectile.cs](Assets/_Project/Scripts/Projectile.cs), [Lifetime.cs](Assets/_Project/Scripts/Lifetime.cs), [MoveForward.cs](Assets/_Project/Scripts/MoveForward.cs), and [MoveVertical.cs](Assets/_Project/Scripts/MoveVertical.cs) are live in the codebase. Weapon prefabs (buster migration, Twin Slasher, Frost Tower) still pending per [SPEC_XWEAPONS.md](SPEC_XWEAPONS.md).

Replaces the standalone [BusterShot.cs](Assets/_Project/Scripts/BusterShot.cs) with a composable projectile system used by both player and enemy weapons. A shared `Projectile` component handles damage, hit detection, and despawn. Separate small behavior components handle movement patterns. Prefabs compose the two.

**Relationship to other specs:**

- [SPEC_XWEAPONS.md](SPEC_XWEAPONS.md) — the weapon system spawns projectile prefabs defined here. BusterShot migration, WeaponData spawn patterns, and on-screen cap tracking are covered there.
- [SPEC2.md](SPEC2.md) — `Projectile` calls `Health.ApplyDamage(damage, transform.position)` using the source-position overload added there.

---

## 1. Architecture overview

```
Prefab: MegamanX_Shot_Small   (layer: PlayerProjectile)
├── Projectile           (damage, piercing)
├── Lifetime             (duration=0.6s)
├── MoveForward          (speed — advances along transform.right)
├── Rigidbody2D          (Kinematic, gravityScale=0)
├── Collider2D           (isTrigger=true)
└── SpriteRenderer       (SVG authored facing right, +X = leading edge)

Prefab: FrostTower            (layer: PlayerProjectile)
├── Projectile           (damage, piercing=true)
├── Lifetime             (duration=1.5s)
├── Rigidbody2D          (Kinematic, gravityScale=0)
├── Collider2D           (isTrigger=true)
└── SpriteRenderer
```

**Projectile** is the shared core — every projectile has it. It never moves the object. It only detects hits, applies damage, and fires the `Destroyed` event.

**Lifetime** is a general-purpose auto-destroy timer. Usable on any GameObject — projectiles, VFX, dash afterimages, temporary spawns. When duration expires, it calls `Destroy(gameObject)`. On a projectile, this triggers `Projectile.OnDestroy` → `Destroyed` event.

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
    [SerializeField] bool piercing;

    public event Action Destroyed;

    int environmentLayer;

    void Awake()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        environmentLayer = LayerMask.NameToLayer("Environment");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == environmentLayer)
        {
            Destroy(gameObject);   // walls stop all projectiles, including piercing
            return;
        }

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
| Damage amount | `[SerializeField]` on Projectile. Baked per prefab. The prefab is the source of truth for what it does. |
| Hit filtering | Handled by the Physics2D collision matrix via dedicated layers (`PlayerProjectile`, `PlayerProjectileNoClip`, `EnemyProjectile`). The Projectile component itself does no layer filtering — the matrix gates which pairs ever fire `OnTriggerEnter2D`. Each prefab sets its GameObject layer per its team/clip behavior. |
| Wall stopping | Explicit check on `Environment` layer in `OnTriggerEnter2D`: walls destroy any projectile on contact, regardless of `piercing`. Piercing means "pass through enemies," not "pass through walls." For genuinely wall-piercing projectiles (Soul Body, charged Twin Slasher), put them on `PlayerProjectileNoClip` — the matrix disables their Environment collisions entirely, so the callback never fires. |
| Piercing | `bool piercing`. Twin Slasher = true (passes through enemies). Buster = false (destroys on first hit). |
| Lifetime | Extracted into a separate `Lifetime` component (§2.1). Projectile does not handle its own timer. |
| Off-screen despawn | Deferred. Lifetime-only for now; off-screen cleanup addressed later. |
| Despawn event | `event Action Destroyed` fired from `OnDestroy`. Same pattern as current BusterShot. Spawner subscribes at spawn time to track live-shot count. No metadata — spawner already knows which slot the shot belongs to. |
| Kinematic enforcement | `Awake` forces Kinematic + gravityScale 0, same as BusterShot and PlayerController. |

### What Projectile does NOT do

- **Lifetime.** No timer. A separate `Lifetime` component handles auto-destroy (§2.1).
- **Movement.** No `FixedUpdate`, no `MovePosition`, no velocity. Behavior components handle this.
- **Spawn logic.** Projectile doesn't know about WeaponData, spawn patterns, or facing. It's a runtime component on a live instance.
- **Visual effects.** No particles, no trail. Those are separate components or child objects on the prefab.

### 2.1 Lifetime component

General-purpose auto-destroy timer. Not projectile-specific — usable on any GameObject (VFX, dash afterimages, temporary spawns, debris).

```csharp
public class Lifetime : MonoBehaviour
{
    [SerializeField] float duration = 1f;

    float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= duration) Destroy(gameObject);
    }
}
```

On a projectile, `Destroy(gameObject)` triggers `Projectile.OnDestroy` → `Destroyed` event. The two components are fully decoupled — `Lifetime` doesn't know about `Projectile`, and `Projectile` doesn't know about `Lifetime`.

---

## 3. Behavior components — implemented now

### 3.1 MoveForward

Moves along `transform.right` at constant speed. Covers: buster shots (all tiers), Twin Slasher blades, enemy bullets. No `Rigidbody2D` dependency — also usable for background scrolling, visual effects, or any object that needs constant-velocity linear motion.

**No Initialize method, no angle or facing field.** Direction is entirely determined by the transform's world rotation, set by the spawner at `Instantiate`. The script itself has a single serialized field — `speed`.

```csharp
public class MoveForward : MonoBehaviour
{
    [SerializeField] float speed = 18f;

    void Update()
    {
        transform.position += transform.right * (speed * Time.deltaTime);
    }
}
```

**Spawner rotates the transform** to set direction:

- Fire right: `Quaternion.identity` (or Z = 0°). `transform.right` = world `+X`.
- Fire left: `Quaternion.Euler(0, 0, 180)`. `transform.right` = world `−X`.
- Angled spread (e.g., Twin Slasher blade at +30° upward while facing right): `Quaternion.Euler(0, 0, 30)`.
- Same angle while facing left: `Quaternion.Euler(0, 0, 150)` (i.e., `180 − angle`).

This replaces the older `localScale.x` flip convention — projectile roots are never scaled to flip direction. Projectile SVGs are authored facing right (+X leading edge), so the un-rotated prefab fires right and `transform.right` + SVG orientation stay in sync.

When used on a projectile prefab alongside `Projectile` (which requires `Rigidbody2D` + `Collider2D`), the kinematic body's collider follows the transform automatically — trigger callbacks fire as normal.

### 3.2 MoveVertical

Moves along `±transform.up` at constant speed. Covers: Frost Tower ice shards, vertical enemy bullets, falling debris — anything that needs strictly vertical motion regardless of the transform's Z rotation.

SerializeFields: `float speed`, `enum Direction { Up, Down }`. Each Update: `transform.position += ±transform.up * speed * dt`.

Present in the codebase alongside `MoveForward`. Choose based on what "forward" means for the weapon — `MoveForward` aligns with transform rotation (rotate to aim), `MoveVertical` ignores horizontal facing entirely (always strictly up or down).

---

## 4. Behavior components — planned (not implemented)

Specced here so future weapon specs can reference them by name. Each follows the same pattern: no `Rigidbody2D` dependency, no `Initialize` method, all config baked via `[SerializeField]`, direction set by the spawner via transform rotation at `Instantiate` (matching `MoveForward` §3.1), movement via `transform.position` in `Update`.

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

## 5. Enemy projectiles

Enemies use the same `Projectile + Behavior` composition. The only difference is the GameObject layer:

- Player projectile prefabs: layer = `PlayerProjectile` (or `PlayerProjectileNoClip` for wall-piercing).
- Enemy projectile prefabs: layer = `EnemyProjectile`.

The Physics2D collision matrix handles the rest (see §2 "Hit filtering").

Enemy spawning follows the same pattern: instantiate prefab, rotate to set fire direction. Enemy scripts call this directly — no `WeaponInventory` needed on the enemy side.

```csharp
// Example: enemy turret fires a straight shot toward the player
int facing = playerIsLeft ? -1 : +1;
float zRot = facing >= 0 ? 0f : 180f;
Instantiate(bulletPrefab, muzzle, Quaternion.Euler(0f, 0f, zRot));
```

No changes to `Projectile` or behaviors. Layer configuration in the prefab handles friend/foe.

---

## 6. Prefab catalog

### Implemented in this spec

Each row is a single leaf prefab. Multi-blade/multi-shot spreads (e.g., Twin Slasher's 2/4 blades) are produced by the spawner firing the leaf prefab multiple times with different rotations, per [SPEC_XWEAPONS.md §6](SPEC_XWEAPONS.md) — no composite-parent prefabs.

| Prefab | Components | Notes |
|---|---|---|
| `MegamanX_Shot_Small` | Projectile + Lifetime + MoveForward | damage=1, speed=18, piercing=false |
| `MegamanX_Shot_Semi` | Projectile + Lifetime + MoveForward | damage=2, speed=18, piercing=false |
| `MegamanX_Shot_Full` | Projectile + Lifetime + MoveForward | damage=4, speed=14, piercing=true, lifetime=0.8 |
| `TwinSlasherBlade` | Projectile + Lifetime + MoveForward | damage=2, speed=10, piercing=true, duration=0.6. Spawner fires this twice (±30°) for uncharged. |
| `TwinSlasherBladeCharged` | Projectile + Lifetime + MoveForward | damage=3, speed=10, piercing=true (layer: `PlayerProjectileNoClip`), duration=0.6. Spawner fires four times (±30°, ±15°). |
| `FrostTower` | Projectile + Lifetime | damage=3, piercing=true, lifetime=1.5, stationary |
| `FrostTowerCharged` | Projectile + Lifetime | damage=5, piercing=true, lifetime=2.5, stationary, taller |

### Future (one prefab per weapon, added with their specs)

| Weapon | Behavior | Notes |
|---|---|---|
| Lightning Web | Projectile + Lifetime or custom | Web zone persists on surface, stationary |
| Aiming Laser | HomingMovement | Locks onto nearest enemy |
| Double Cyclone | MoveForward (spawner rotates upward) or OrbitalMovement | Two tornadoes, upward angle |
| Rising Fire | ArcMovement | Arcs upward with gravity |
| Ground Hunter | GroundCrawlMovement | Surface-following |
| Soul Body | MoveForward | Clone travels forward, slow (layer: `PlayerProjectileNoClip`) |

---

## 7. Cross-cutting

### DamageFlash

Projectile calls `Health.ApplyDamage(damage, transform.position)` — the source-position overload from [SPEC2.md](SPEC2.md). If the target has `DamageFlash`, the hit feedback fires automatically.

---

## 8. Testing plan

EditMode tests (requires `.asmdef` setup from README §12):

- **Projectile**
  - Enemy contact applies damage via `Health.ApplyDamage(damage, position)`.
  - Environment contact destroys the projectile regardless of `piercing`.
  - `piercing = false` → destroy on first enemy hit.
  - `piercing = true` → survives trigger hit.
  - `Destroyed` event fires on destroy.
- **Lifetime**
  - Auto-destroys after `duration` seconds.
  - Triggers `Projectile.Destroyed` on the same GameObject.
- **MoveForward**
  - Update advances position by `transform.right * speed * dt`.
  - Rotating the transform (Z = 180°) reverses travel direction without any script-side facing logic.
- **MoveVertical**
  - Update advances position by `±transform.up * speed * dt` per `Direction` enum.

Manual QA:
- Drop a buster shot prefab in the scene → it moves, hits enemies, despawns on contact or lifetime.
- Drop a Frost Tower prefab → it sits at full scale, damages on contact, despawns at lifetime.
- Drop an enemy bullet prefab → hits Player layer, ignores Enemy layer.

---

## 9. Implementation order

Steps 1–3 are already done in the codebase. Step 4 (prefab composition) is the remaining work, driven by [SPEC_XWEAPONS.md](SPEC_XWEAPONS.md).

1. ~~**Projectile.cs** — create the component per §2.~~ Done.
2. ~~**Lifetime.cs** — create per §2.1.~~ Done.
3. ~~**MoveForward.cs** / **MoveVertical.cs** — create per §3.~~ Done.
4. **Author prefabs** — compose components onto prefabs per §6. Drop in scene to verify.

BusterShot migration (replacing `BusterShot.cs` with these components in the weapon pipeline) is covered in [SPEC_XWEAPONS.md](SPEC_XWEAPONS.md).

---

## 10. Files summary

### Existing (already in the codebase)

| File | Type |
|---|---|
| [Assets/_Project/Scripts/Projectile.cs](Assets/_Project/Scripts/Projectile.cs) | MonoBehaviour |
| [Assets/_Project/Scripts/Lifetime.cs](Assets/_Project/Scripts/Lifetime.cs) | MonoBehaviour (general-purpose auto-destroy timer) |
| [Assets/_Project/Scripts/MoveForward.cs](Assets/_Project/Scripts/MoveForward.cs) | MonoBehaviour (advances along `transform.right`) |
| [Assets/_Project/Scripts/MoveVertical.cs](Assets/_Project/Scripts/MoveVertical.cs) | MonoBehaviour (advances along `±transform.up`) |
