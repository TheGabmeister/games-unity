# SPEC_XWEAPONS — X Weapon System + PlayerBuster Extraction

Status: **Design complete, unimplemented.**

Covers [README.md §2](README.md) (Mega Man X — combat): special weapons, weapon energy, weapon swap, charged specials, and the `PlayerBuster` extraction from `PlayerController`. Two representative weapons — **Twin Slasher** and **Frost Tower** — are fully specced; the remaining six follow the same patterns and are added incrementally.

---

## 1. Scope

### In scope

- Extract buster/charge system from `PlayerController` into `PlayerBuster` component.
- `WeaponData` ScriptableObject (one asset per weapon, ~9 total: base buster + 8 specials).
- `WeaponInventory` component — tracks unlocked slots, active weapon, energy pools, swap input, attack routing.
- Weapon energy: per-weapon pool, shared max 28, energy-cost deduction, auto-switch to buster on depletion.
- Weapon swap: new `WeaponNext`/`WeaponPrev` input actions, shoulder buttons / Q+E. Charge resets on swap.
- Weapon color tint: `SpriteRenderer.color` driven by active `WeaponData.weaponTint`.
- Charged specials: `WeaponData.chargedPrefab` + `chargedEnergyCost`. Same global charge timing (full at 1.2 s). Semi-charge tier is buster-only.
- Per-weapon on-screen shot cap via `WeaponData.maxOnScreen`.
- Two weapons implemented end-to-end: **Twin Slasher** (spread) + **Frost Tower** (stationary).

### Out of scope

- Remaining 6 special weapons (added incrementally per this pattern).
- Weapon-energy HUD (README §11).
- Weapon acquisition cutscene / boss-defeat flow (README §9).
- Zero's saber / techniques (README §4).
- Weapon-energy pickups (README §5).

---

## 2. New files

| File | Type | Purpose |
|---|---|---|
| `Assets/_Project/Scripts/WeaponData.cs` | ScriptableObject | Per-weapon configuration asset |
| `Assets/_Project/Scripts/PlayerBuster.cs` | MonoBehaviour | Extracted buster charge/fire/cap/flash |
| `Assets/_Project/Scripts/WeaponInventory.cs` | MonoBehaviour | Weapon slots, swap, energy, attack routing |
| `Assets/_Project/Scripts/TwinSlasherProjectile.cs` | MonoBehaviour | Spread projectile for Twin Slasher |
| `Assets/_Project/Scripts/FrostTowerProjectile.cs` | MonoBehaviour | Stationary ice pillar for Frost Tower |
| `Assets/_Project/Data/` | Folder | Home for `WeaponData` SO assets |

### Existing files modified

- `PlayerController.cs` — remove buster code, add `PlayerBuster` / `WeaponInventory` coordination hooks.
- `InputSystem_Actions.inputactions` — add `WeaponNext`, `WeaponPrev` actions.

---

## 3. WeaponData ScriptableObject

```csharp
[CreateAssetMenu(fileName = "NewWeapon", menuName = "MegamanX4/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string displayName;
    public Color weaponTint = Color.white;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public GameObject chargedPrefab;       // null = no charged form
    public int maxOnScreen = 3;

    [Header("Energy")]
    public int energyCost = 1;
    public int chargedEnergyCost = 4;

    [Header("Audio (stubbed)")]
    public SfxId fireSfx;
    public SfxId chargedFireSfx;
}
```

### Asset instances

| Asset | Tint | Prefab | maxOnScreen | energyCost | chargedCost |
|---|---|---|---|---|---|
| `BaseBuster` | white | (handled by PlayerBuster, not used) | 3 | 0 | 0 |
| `TwinSlasher` | purple | TwinSlasherProjectile prefab | 2 | 2 | 4 |
| `FrostTower` | ice blue | FrostTowerProjectile prefab | 1 | 3 | 6 |
| (remaining 6) | per-weapon | — | — | — | — |

`BaseBuster` is a sentinel entry in the inventory (slot 0). Its `energyCost = 0` means it never depletes. `PlayerBuster` handles the actual fire logic for this slot — `WeaponInventory` delegates to `PlayerBuster` when slot 0 is active.

---

## 4. PlayerBuster extraction

Move the following from `PlayerController` into a new `PlayerBuster` MonoBehaviour:

### Fields extracted

From [PlayerController.cs](Assets/_Project/Scripts/PlayerController.cs):

- `[Header("Shooting")]` block (lines 63–73): `smallShotPrefab`, `semiShotPrefab`, `fullShotPrefab`, `muzzleAnchor`, `semiChargeTime`, `fullChargeTime`, `maxSmallShots`, `semiFlashColor`, `fullFlashColor`, `flashPeriod`.
- Private state: `isCharging`, `chargeTimer`, `baseSpriteColor`, `activeSmallShots`.
- Methods: `Spawn`, `UpdateChargeFlash`, the charge-related parts of `OnAttackStarted`/`OnAttackCanceled`.

### PlayerBuster API

```csharp
[RequireComponent(typeof(PlayerController))]
public class PlayerBuster : MonoBehaviour
{
    // [SerializeField] fields from the extraction list above...

    PlayerController controller;
    SpriteRenderer spriteRenderer;

    public bool IsCharging => isCharging;
    public float ChargeTimer => chargeTimer;
    public int ActiveSmallShotCount => activeSmallShots.Count;

    public void Initialize(SpriteRenderer sr)
    {
        spriteRenderer = sr;
        baseSpriteColor = sr ? sr.color : Color.white;
    }

    public void StartCharge()
    {
        if (controller.IsKnockedBack) return;
        isCharging = true;
        chargeTimer = 0f;
    }

    public void ReleaseCharge()
    {
        if (!isCharging) return;
        isCharging = false;
        // existing fire logic: full → semi → small ...
        chargeTimer = 0f;
        RestoreColor();
    }

    public void CancelCharge()
    {
        isCharging = false;
        chargeTimer = 0f;
        RestoreColor();
    }

    void Update()
    {
        if (isCharging) chargeTimer += Time.deltaTime;
        UpdateChargeFlash();
    }

    // Spawn, UpdateChargeFlash, etc. — moved from PlayerController.
}
```

`PlayerBuster` gets the `SpriteRenderer` reference via `Initialize()` called by `PlayerController.Awake`. It reads `controller.facing` and `controller.MuzzleAnchor` (new public property exposing the existing `muzzleAnchor`) for shot direction and spawn position.

### What stays in PlayerController

- `muzzleAnchor` stays as a serialized field (physical anchor on the prefab). Exposed via a public `MuzzleAnchor` property.
- `facing` is already readable — add `public int Facing => facing;` if not already public.
- `IsKnockedBack` — add `public bool IsKnockedBack => knockbackTimer > 0f;` (currently a private property).
- `OnLadder` — add `public bool OnLadder => onLadder;`.
- Knockback's `ApplyKnockback` no longer resets charge directly — it calls `PlayerBuster.CancelCharge()` instead.
- `OnAttackStarted`/`OnAttackCanceled` are **removed** from PlayerController. Input routing moves to WeaponInventory.

---

## 5. WeaponInventory component

```csharp
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerBuster))]
public class WeaponInventory : MonoBehaviour
{
    [SerializeField] WeaponData baseBusterData;
    [SerializeField] int maxEnergy = 28;

    PlayerController controller;
    PlayerBuster buster;
    SpriteRenderer spriteRenderer;

    readonly List<WeaponSlot> slots = new();
    int activeIndex;

    InputAction attackAction;
    InputAction weaponNextAction;
    InputAction weaponPrevAction;

    public WeaponData ActiveWeapon => slots[activeIndex].data;
    public int ActiveEnergy => slots[activeIndex].energy;
    public int SlotCount => slots.Count;

    // --- public API ---

    public void Unlock(WeaponData weapon)
    {
        if (slots.Exists(s => s.data == weapon)) return;
        slots.Add(new WeaponSlot(weapon, maxEnergy));
    }

    public void RefillEnergy(WeaponData weapon, int amount) { ... }

    // --- internal ---

    struct WeaponSlot
    {
        public WeaponData data;
        public int energy;
        public readonly List<GameObject> liveShots;
        public WeaponSlot(WeaponData d, int maxE) { data = d; energy = maxE; liveShots = new(); }
    }
}
```

### Lifecycle

- **Awake:** Grabs `PlayerController`, `PlayerBuster`, `SpriteRenderer` (from visual child). Inserts `baseBusterData` as slot 0. Caches input actions (`Attack`, `WeaponNext`, `WeaponPrev`).
- **OnEnable:** Subscribes to `attackAction.started/canceled`, `weaponNextAction.started`, `weaponPrevAction.started`.
- **OnDisable:** Symmetric unsubscribe.

---

## 6. Attack input routing

`WeaponInventory` is the sole subscriber to `attackAction`. PlayerController and PlayerBuster no longer subscribe directly.

### On `attackAction.started`:

```
if controller.IsKnockedBack → return
if activeIndex == 0 (buster):
    buster.StartCharge()
else:
    buster.StartCharge()    // specials also use charge timer for charged variants
```

### On `attackAction.canceled`:

```
if activeIndex == 0 (buster):
    buster.ReleaseCharge()
else:
    slot = slots[activeIndex]
    if buster.ChargeTimer >= fullChargeTime && slot.data.chargedPrefab != null:
        if TryConsumeEnergy(slot, slot.data.chargedEnergyCost):
            SpawnWeaponShot(slot, charged: true)
    else:
        if TryConsumeEnergy(slot, slot.data.energyCost):
            SpawnWeaponShot(slot, charged: false)
    buster.CancelCharge()   // always cancel charge state after special fire
```

### `SpawnWeaponShot`:

```csharp
void SpawnWeaponShot(ref WeaponSlot slot, bool charged)
{
    if (slot.liveShots.Count >= slot.data.maxOnScreen) return;
    var prefab = charged ? slot.data.chargedPrefab : slot.data.projectilePrefab;
    if (!prefab) return;
    Vector2 pos = controller.MuzzleAnchor ? (Vector2)controller.MuzzleAnchor.position
                                          : (Vector2)controller.transform.position;
    var go = Instantiate(prefab, pos, Quaternion.identity);
    slot.liveShots.Add(go);
    // Track destruction to remove from liveShots.
    // If the projectile has a known lifetime, use OnDestroy event.
}
```

### `TryConsumeEnergy`:

```csharp
bool TryConsumeEnergy(ref WeaponSlot slot, int cost)
{
    if (slot.data.energyCost == 0) return true;   // buster: infinite
    if (slot.energy < cost)
    {
        SwitchToBuster();  // auto-switch on depletion
        return false;
    }
    slot.energy -= cost;
    if (slot.energy <= 0) SwitchToBuster();
    return true;
}
```

### On ladder

When `controller.OnLadder`, the existing shoot-on-ladder behavior stays. `WeaponInventory` routes to `PlayerBuster` or `SpawnWeaponShot` as normal — the shoot-halt-and-lock-facing logic in `PlayerController.OnAttackCanceled` moves to `WeaponInventory` (since it now owns the attack action).

---

## 7. Weapon energy

- **Pool size:** 28 per weapon (matches X4). `WeaponSlot.energy` field, initialized to `maxEnergy` on `Unlock`.
- **Base buster:** `energyCost = 0`, always fires regardless of pool. Slot 0.
- **Depletion:** When fire would drain below 0, `TryConsumeEnergy` returns false and triggers `SwitchToBuster()`.
- **Auto-switch:** Sets `activeIndex = 0`, applies buster tint (white), cancels any charge.
- **Refill:** `WeaponInventory.RefillEnergy(WeaponData, int)` called by future pickup code. Clamps to `maxEnergy`.
- **Full refill on acquisition:** `Unlock(weapon)` initializes the slot at `maxEnergy`.

---

## 8. Weapon swap + color tint

### Swap

```csharp
void OnWeaponNext(InputAction.CallbackContext _) => CycleWeapon(+1);
void OnWeaponPrev(InputAction.CallbackContext _) => CycleWeapon(-1);

void CycleWeapon(int direction)
{
    if (slots.Count <= 1) return;
    buster.CancelCharge();     // charge resets on swap
    activeIndex = (activeIndex + direction + slots.Count) % slots.Count;
    ApplyWeaponTint();
}
```

### Color tint

```csharp
void ApplyWeaponTint()
{
    if (!spriteRenderer) return;
    spriteRenderer.color = slots[activeIndex].data.weaponTint;
}
```

`baseSpriteColor` in `PlayerBuster` tracks the *original* color (before any tint). `PlayerBuster.UpdateChargeFlash` must account for the weapon tint: during charge, flash cycles between `weaponTint` and `chargeFlashColor`; outside charge, `spriteRenderer.color` is `weaponTint` (not white). `PlayerBuster` reads the active tint from `WeaponInventory.ActiveWeapon.weaponTint` (or `WeaponInventory` exposes a `Color ActiveTint` property).

`DamageFlash` is unaffected — it toggles `SpriteRenderer.enabled`, not `.color`.

---

## 9. Charged special weapons

- Charge timing is **global** — the same `fullChargeTime` (1.2 s) applies to all weapons.
- **Semi-charge tier is buster-only.** Specials use two outcomes: release before full charge → uncharged fire; release at/after full charge → charged fire. No "semi-charged" state for specials.
- `WeaponData.chargedPrefab == null` means the weapon has no charged form; releasing at full charge still fires the uncharged version.
- `WeaponData.chargedEnergyCost` is deducted for a charged shot. If insufficient energy, uncharged version fires instead (with its own `energyCost`).

### Charge flash for specials

While a special is equipped and charging, `PlayerBuster.UpdateChargeFlash` uses the same flash cadence and thresholds. The "semi" flash stage still plays (visual feedback that charge is building) even though releasing there fires uncharged. Only the "full" flash stage means a charged shot will come.

---

## 10. Twin Slasher

### Behavior

- **Uncharged:** Two crescent-shaped projectiles spawn simultaneously from the muzzle — one angled ~30° upward, one ~30° downward. Each travels in a straight line at moderate speed. Damage per blade.
- **Charged:** Four blades — two at ±30° and two at ±15°. Same speed, higher individual damage. Costs more energy.
- Both versions pierce through enemies (no despawn on contact; despawn on timer or off-screen).

### TwinSlasherProjectile.cs

```csharp
public class TwinSlasherProjectile : MonoBehaviour
{
    [SerializeField] float speed = 10f;
    [SerializeField] float lifetime = 0.6f;
    [SerializeField] int damage = 2;

    Rigidbody2D rb;
    float timer;
    Vector2 direction;

    public void Fire(int facing, float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        direction = new Vector2(Mathf.Cos(rad) * facing, Mathf.Sin(rad));
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
        timer += Time.fixedDeltaTime;
        if (timer >= lifetime) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var health = other.GetComponentInParent<Health>();
        if (health) health.ApplyDamage(damage, transform.position);
        // no destroy — piercing
    }
}
```

`WeaponInventory.SpawnWeaponShot` for Twin Slasher spawns **two** instances (calling `Fire(facing, +30f)` and `Fire(facing, -30f)`). Charged spawns four. The spawn-count logic lives in a small `ITwinSlasherSpawner` helper or in `SpawnWeaponShot` with a type check — but to stay composition-based, the simpler approach: the Twin Slasher **prefab itself** is a parent with two child projectile objects, both activated on `Fire()`. One prefab = one logical shot = one `liveShots` entry.

Alternative: `WeaponInventory` calls a spawn method on the projectile's root script. `TwinSlasherProjectile` has a `Fire(int facing)` that instantiates its own child blades internally.

**Chosen approach:** The prefab root has `TwinSlasherProjectile` which spawns sub-projectiles internally. `WeaponInventory` instantiates one prefab and calls `Fire(facing)`. The root despawns when all children are gone.

### Prefab structure

```
TwinSlasher (root)
├── TwinSlasherProjectile.cs (spawns blades as children)
└── (children instantiated at Fire time)
```

### WeaponData asset

- `displayName`: "Twin Slasher"
- `weaponTint`: purple `(0.7f, 0.2f, 0.9f)`
- `projectilePrefab`: TwinSlasher prefab
- `chargedPrefab`: TwinSlasherCharged prefab (same script, 4 blades)
- `maxOnScreen`: 2
- `energyCost`: 2
- `chargedEnergyCost`: 4

---

## 11. Frost Tower

### Behavior

- **Uncharged:** A tall ice pillar spawns at the player's feet. It rises from the ground over ~0.15 s, stays for ~1.5 s, then shatters (destroy). Damages enemies on contact while active. Stationary — does not follow the player.
- **Charged:** Same pillar but much taller (2x height), wider hitbox, higher damage, longer duration (~2.5 s).
- Only one Frost Tower can exist at a time (`maxOnScreen = 1`). Firing again while one exists does nothing (energy isn't consumed).

### FrostTowerProjectile.cs

```csharp
public class FrostTowerProjectile : MonoBehaviour
{
    [SerializeField] float riseTime = 0.15f;
    [SerializeField] float duration = 1.5f;
    [SerializeField] int damage = 3;
    [SerializeField] float fullHeight = 2f;

    float timer;
    Vector3 targetScale;

    public void Fire(int facing)
    {
        // Spawn at player's feet, facing doesn't matter for a vertical pillar.
        targetScale = transform.localScale;
        transform.localScale = new Vector3(targetScale.x, 0f, targetScale.z);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer <= riseTime)
        {
            float t = timer / riseTime;
            transform.localScale = new Vector3(targetScale.x, targetScale.y * t, targetScale.z);
        }
        if (timer >= riseTime + duration)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var health = other.GetComponentInParent<Health>();
        if (health) health.ApplyDamage(damage, transform.position);
    }
}
```

### Prefab structure

```
FrostTower (root)
├── FrostTowerProjectile.cs
├── Rigidbody2D (Kinematic, gravityScale=0)
├── BoxCollider2D (isTrigger=true, sized to full pillar)
└── SpriteRenderer (ice pillar SVG)
```

### WeaponData asset

- `displayName`: "Frost Tower"
- `weaponTint`: ice blue `(0.5f, 0.85f, 1f)`
- `projectilePrefab`: FrostTower prefab
- `chargedPrefab`: FrostTowerCharged prefab (taller, longer duration)
- `maxOnScreen`: 1
- `energyCost`: 3
- `chargedEnergyCost`: 6

---

## 12. Input action changes

Add to [InputSystem_Actions.inputactions](Assets/_Project/Input/InputSystem_Actions.inputactions):

| Action | Type | Bindings |
|---|---|---|
| `WeaponNext` | Button | Gamepad: R1 / Right Shoulder. Keyboard: E |
| `WeaponPrev` | Button | Gamepad: L1 / Left Shoulder. Keyboard: Q |

These are **new actions** in the existing action map. No existing bindings are modified.

---

## 13. PlayerController changes

### Removed

- `[Header("Shooting")]` block — all fields move to `PlayerBuster`.
- `isCharging`, `chargeTimer`, `baseSpriteColor`, `activeSmallShots` — move to `PlayerBuster`.
- `Spawn()`, `UpdateChargeFlash()` — move to `PlayerBuster`.
- `OnAttackStarted`, `OnAttackCanceled` — move to `WeaponInventory`.
- `attackAction` cached reference — moves to `WeaponInventory`.
- Charge-related code in `ApplyKnockback` (`isCharging = false`, `chargeTimer = 0f`, color restore) — replaced with `buster.CancelCharge()`.
- `isCharging` timer tick in `Update` — moves to `PlayerBuster.Update`.
- `UpdateChargeFlash()` call in `Update` — `PlayerBuster` owns it.

### Added

```csharp
public int Facing => facing;
public bool OnLadder => onLadder;
public Transform MuzzleAnchor => muzzleAnchor;
```

`muzzleAnchor` serialized field stays on `PlayerController` (physical anchor on the prefab). `PlayerBuster` and `WeaponInventory` read it via the property.

### Knockback interaction

`ApplyKnockback` currently resets charge directly. After extraction:

```csharp
// In ApplyKnockback, replace:
if (isCharging) { isCharging = false; chargeTimer = 0f; ... }
// With:
buster.CancelCharge();
```

Where `buster` is a reference grabbed in `Awake`:

```csharp
PlayerBuster buster;
// in Awake:
buster = GetComponent<PlayerBuster>();
```

### Ladder shoot-lock

The `OnAttackCanceled` code that sets `climbingShootLock` on ladder moves to `WeaponInventory`'s attack handler. `WeaponInventory` checks `controller.OnLadder` after firing and sets the shoot lock. Requires `PlayerController` to expose a method:

```csharp
public void TriggerLadderShootLock() { climbingShootLock = true; ladderShootLockUntil = Time.time + ladderShootLockTime; }
```

---

## 14. Cross-cutting details

### Knockback

`ApplyKnockback` → `buster.CancelCharge()`. Weapon swap does not happen on knockback — active weapon stays equipped.

### Damage flash

No conflict — `DamageFlash` toggles `enabled`, weapon tint uses `color`. They coexist.

### Ladder

Shooting on ladder works identically — `WeaponInventory` fires the active weapon (buster or special) and then calls `controller.TriggerLadderShootLock()`.

### Dash-jump

No interaction with weapons. `dashJumpLock` doesn't affect firing.

### Existing callers

`BusterShot.Destroyed` event — `PlayerBuster` subscribes to this for the lemon cap, same as before. `WeaponInventory` uses its own `liveShots` list for special weapons and listens to `OnDestroy` on those projectiles.

---

## 15. Testing plan

EditMode tests (same infra constraint as other specs — requires `.asmdef` setup):

- **WeaponInventory**
  - Unlock adds slot; duplicate unlock ignored.
  - CycleWeapon wraps correctly.
  - TryConsumeEnergy deducts; returns false and auto-switches at 0.
  - Slot 0 (buster) never depletes.
  - maxOnScreen cap prevents over-spawn.
- **PlayerBuster**
  - StartCharge / ReleaseCharge produces correct shot tier.
  - CancelCharge zeroes timer.
  - 3-lemon cap enforced.

Manual QA:
- Equip Twin Slasher → fire → two blades at ±30°. Charged fire → four blades.
- Equip Frost Tower → fire → pillar rises, persists, shatters. Second fire while pillar active → blocked.
- Swap weapons mid-charge → charge resets, tint changes.
- Deplete weapon energy → auto-switch to buster.
- Fire on ladder → shoot-lock triggers regardless of active weapon.

---

## 16. Implementation order

1. **WeaponData SO** — create `WeaponData.cs` + `Assets/_Project/Data/` folder + `BaseBuster` asset.
2. **PlayerBuster extraction** — move code, add public API, verify buster works identically.
3. **PlayerController cleanup** — remove attack code, add public properties, update `ApplyKnockback` to call `buster.CancelCharge()`.
4. **WeaponInventory** — slots, swap, energy, attack routing. Test with buster-only (slot 0).
5. **Input actions** — add `WeaponNext`/`WeaponPrev` to action asset. Bind Q/E + L1/R1.
6. **Twin Slasher** — `TwinSlasherProjectile.cs` + prefab + `WeaponData` asset. Unlock in editor for testing.
7. **Frost Tower** — `FrostTowerProjectile.cs` + prefab + `WeaponData` asset.
8. **Weapon tint** — `ApplyWeaponTint` on swap. Integrate with charge flash.
9. **Charged variants** — charged prefabs for Twin Slasher + Frost Tower.
10. Update [README.md](README.md) checklist.

Steps 2–4 are the critical path — the system must work with just the base buster before any specials are added. If buster behavior regresses after extraction, fix before proceeding.
