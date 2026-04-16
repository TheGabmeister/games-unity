# SPEC_XWEAPONS — X Weapon System

Status: **In progress — Phase 1 (weapon switching + universal charging).**

## 1. Behavior

- The player carries an ordered list of weapons. Slot 0 is always the buster.
- **Q** cycles to the previous weapon, **E** cycles to the next. Wraps around.
- On switch, the player's sprite tints to the active weapon's color.
- Hold **Attack** to charge, release to fire — same press/hold/release rhythm as the buster, regardless of equipped weapon.
- Buster fires lemon / semi / full based on charge tier per its existing behavior in [PlayerBuster.cs](Assets/_Project/Scripts/PlayerBuster.cs).
- Special weapons spawn `WeaponData.prefab` once at the muzzle on release. The charge flash plays during the hold; a fully-charged release fires the same prefab as a tapped release. (Charged variants are deferred — see §4.) No animation lock.

## 2. New pieces

### WeaponData (ScriptableObject)

```csharp
[CreateAssetMenu(menuName = "MegamanX4/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string displayName;
    public Color tint = Color.white;
    public GameObject prefab;   // null for the buster slot — buster uses PlayerBuster's own logic
}
```

The user authors the projectile prefabs in the editor and drags them into the `prefab` field. This spec deliberately says nothing about prefab contents.

### WeaponInventory (MonoBehaviour, on the player)

- Serialized: ordered `List<WeaponData>` (slot 0 = buster).
- Tracks `int _activeIndex`.
- Subscribes to `WeaponNext` / `WeaponPrev` input actions in `OnEnable`, cycles `_activeIndex` on press, wraps with modulo.
- On switch: writes `ActiveWeapon.tint` to the player's `SpriteRenderer.color`.
- Exposes a public `WeaponData ActiveWeapon` and `bool IsBusterActive` so `PlayerBuster` can branch.

## 3. Integration

- **Input actions:** add `WeaponNext` (Keyboard E, Gamepad RB) and `WeaponPrev` (Keyboard Q, Gamepad LB) to [InputSystem_Actions.inputactions](Assets/_Project/Input/InputSystem_Actions.inputactions). No existing bindings change.
- **PlayerBuster** keeps the existing `StartCharge` → tick → `ReleaseCharge` state machine for *every* weapon. The charge timer and charge-flash visuals run regardless of which slot is active.
- On `ReleaseCharge`, PlayerBuster branches on `WeaponInventory.IsBusterActive`:
  - True → existing buster fire (lemon / semi / full based on `_chargeTimer`).
  - False → instantiate `WeaponInventory.ActiveWeapon.prefab` once at `MuzzleAnchor.position` / `MuzzleAnchor.rotation`. Ignore the charge tier — the same prefab fires whether the player tapped or held to full.
- **PlayerController** is untouched — it still owns input subscription and delegates to PlayerBuster as today.
- **Color tint and `DamageFlash`:** `DamageFlash` toggles `SpriteRenderer.enabled`, not `.color`, so they coexist. Charge-flash temporarily overrides color and is restored to the active weapon's tint (not white) on release — small adjustment in `PlayerBuster.RestoreColor`.

## 4. Out of scope (for this phase)


- Per-weapon energy pools and depletion-driven auto-switch.
- Charged variants.
- Animation locks during firing.
- On-screen shot caps for special weapons.
- HUD.
