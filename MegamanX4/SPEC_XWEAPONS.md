# SPEC_XWEAPONS — X Weapon System

Status: **In progress — Phase 1 (weapon switching + universal charging, unified inventory).**

## 1. Behavior

- The player carries an ordered list of weapons. Slot 0 is the buster (X's default arm cannon).
- **Q** cycles to the previous weapon, **E** cycles to the next. Wraps around.
- On switch, the player's sprite tints to the active weapon's color, and any in-progress charge is cancelled.
- Hold **Attack** to charge, release to fire. Charge timing and charge-flash visuals are identical across every weapon.
- On release, the active weapon fires one of three prefabs based on `_chargeTimer`:
  - Full charge → `fullPrefab`
  - Semi charge → `semiPrefab`
  - Tap (or too-short hold) → `smallPrefab`
- **Convention for special weapons:** they don't have a distinct semi tier. Authors set `semiPrefab = smallPrefab` in the inspector so a semi-charge release fires the same shot as a tap. `fullPrefab` can either match `smallPrefab` (no charged variant) or be a distinct charged shot — weapon author's call per asset.
- No animation lock during firing.

## 2. New pieces

### WeaponData (ScriptableObject)

```csharp
[CreateAssetMenu(menuName = "MegamanX4/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string displayName;
    public Color tint = Color.white;
    public GameObject smallPrefab;
    public GameObject semiPrefab;
    public GameObject fullPrefab;
}
```

Same shape for every weapon — buster included. The three prefab slots are the weapon's public interface; what's inside each prefab is an authoring concern and is out of scope here.

### WeaponInventory (MonoBehaviour, on the player)

Owns both the inventory list and the charge state machine — absorbs what PlayerBuster currently does.

- Serialized: ordered `List<WeaponData>` (slot 0 = buster).
- State: `int _activeIndex`, `bool _isCharging`, `float _chargeTimer`, `Color _baseSpriteColor`, `readonly List<Projectile> _activeSmallShots`.
- Subscribes in `OnEnable` to `Attack` (started + canceled), `WeaponNext.started`, `WeaponPrev.started`.
- Drives charge flash in `Update` (ticks `_chargeTimer` while `_isCharging`, cycles `SpriteRenderer.color` during semi/full thresholds, restores to `ActiveWeapon.tint` when idle).
- **On Attack press:** if not knocked back, `_isCharging = true`, reset timer.
- **On Attack release:** picks the tier prefab from `ActiveWeapon` by `_chargeTimer`, instantiates it once at `MuzzleAnchor.position` / `MuzzleAnchor.rotation`. For the small tier, enforces the 3-on-screen cap by tracking `Projectile.Destroyed`. Resets timer and color.
- **On weapon swap (Q/E):** cancels any in-progress charge, advances `_activeIndex`, writes `ActiveWeapon.tint` to the player's `SpriteRenderer.color`.
- **On knockback:** PlayerController calls `CancelCharge()` (same hook it uses today).
- Exposes `public WeaponData ActiveWeapon` and `public bool IsKnockedBack` pass-through if PlayerController needs it.

## 3. Integration

- **PlayerBuster is deleted.** All of its state and methods fold into WeaponInventory. Call sites in [PlayerController.cs](Assets/_Project/Scripts/PlayerController.cs) are renamed:
  - `_buster = GetComponent<PlayerBuster>()` → `_inventory = GetComponent<WeaponInventory>()`
  - `_buster.Initialize(_spriteRenderer)` → `_inventory.Initialize(_spriteRenderer)`
  - `_buster.StartCharge()` in `OnAttackStarted` → `_inventory.StartCharge()`
  - `_buster.ReleaseCharge()` in `OnAttackCanceled` → `_inventory.ReleaseCharge()`
  - `_buster.CancelCharge()` in `ApplyKnockback` → `_inventory.CancelCharge()`
  - `[RequireComponent(typeof(PlayerBuster))]` → `[RequireComponent(typeof(WeaponInventory))]`
- **PlayerController** otherwise unchanged — it still owns input subscription for `Attack` and still handles ladder shoot-lock inline in `OnAttackCanceled`.
- **Input actions:** add `WeaponNext` (Keyboard E, Gamepad RB) and `WeaponPrev` (Keyboard Q, Gamepad LB) to [InputSystem_Actions.inputactions](Assets/_Project/Input/InputSystem_Actions.inputactions). `WeaponInventory` subscribes to these directly.
- **Color tint and `DamageFlash`:** `DamageFlash` toggles `SpriteRenderer.enabled`, not `.color`, so they coexist. Charge-flash temporarily overrides color and is restored to the active weapon's tint (not white) when charge ends.

## 4. Out of scope (for this phase)

- Per-weapon energy pools and depletion-driven auto-switch.
- Animation locks during firing.
- On-screen shot caps for special weapons (only the buster's 3-lemon cap carries over).
- HUD.
