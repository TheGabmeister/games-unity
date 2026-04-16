# SPEC_HUD

## Context

The gameplay scene needs a minimal HUD so the player can read their current HP and (when wielding a special weapon) the active weapon's remaining energy. The `GameplayHUD.prefab` already has scaffolding (Canvas, a `Health` child, an `Energy` child) but no script is driving it — [HUD.cs](Assets/_Project/Scripts/HUD.cs) is an empty `MonoBehaviour` template.

This spec covers the minimum HUD for playtesting the weapon-energy system just landed in [WeaponInventory.cs](Assets/_Project/Scripts/WeaponInventory.cs). MMX4-authentic polish (portraits, bar segment ticks, boss HP, lives counter) is out of scope.

## Scope

**In:**
- Vertical HP bar, always visible, anchored top-left.
- Vertical weapon-energy bar, visible only when a special weapon is active (hidden when the buster is active, since buster energy is infinite).
- Bars colored per MMX convention: HP in a fixed warm color; energy tinted by `ActiveWeapon.tint`.
- Event-driven updates — no per-frame polling.

**Out:**
- Boss HP bar.
- Sub-tanks, lives counter, score, timers.
- Animated "draining" tween on damage (MMX4 ticks the bar down one segment at a time — defer).
- Portraits, weapon icons, pause-menu weapon select.
- Controller/keybind overlay.

## Visual

Two flat vertical bars, top-left corner of the screen. Each bar is a 2-rect composite:

```
┌─┐ ┌─┐
│█│ │█│   ← fill (scales along +Y; origin bottom)
│█│ │ │
│█│ │ │
└─┘ └─┘
HP   Energy
```

- Bar dimensions: ~8 px wide × 56 px tall in reference resolution. Spacing: ~4 px between the two bars, ~8 px from screen edge.
- Outline: 1 px white border (MMX convention). Authored as a background `Image` sitting behind the fill, slightly inflated.
- Fill: `Image` with `type = Filled`, `fillMethod = Vertical`, `fillOrigin = Bottom`. Drive via `Image.fillAmount = current / max`.
- Reference resolution: 1280×720 (CanvasScaler → Scale With Screen Size, match = 0.5).

### Colors

- **HP fill:** `#FFCC33` (MMX-style warm yellow). Hardcoded — health has no per-entity tint concept.
- **HP background:** solid black `#000000`, outline white `#FFFFFF`.
- **Energy fill:** `WeaponInventory.ActiveWeapon.tint` — reads directly from the active `WeaponData`.
- **Energy background:** same black + white outline as HP.

## Data flow

The HUD is a pure view. It reads state from two sources and re-renders only on event.

### Health

- Find the player's `Health` component once (via `StageSession` injection — see "Hookup" below).
- Subscribe to `Health.HealthChanged(int current, int max)` (already exists — [Health.cs](Assets/_Project/Scripts/Health.cs)).
- Update HP fill on event; unsubscribe on `OnDestroy`.

### Weapon energy

Requires small API additions to [WeaponInventory.cs](Assets/_Project/Scripts/WeaponInventory.cs). The core logic already tracks `_energy[]` internally; this exposes it:

```csharp
public int ActiveIndex => _activeIndex;
public int GetEnergy(int index) { ... }          // 0 if out of range
public int GetMaxEnergy(int index) { ... }       // reads _weapons[index].maxEnergy
public event Action EnergyChanged;               // fired after debit
public event Action ActiveWeaponChanged;         // fired on swap + on auto-switch
```

HUD behavior:
- On `ActiveWeaponChanged`: if `ActiveWeapon.maxEnergy == 0`, disable the Energy bar GameObject; else enable it, set fill color to `ActiveWeapon.tint`, refresh fill amount.
- On `EnergyChanged`: refresh fill amount from `GetEnergy(ActiveIndex) / GetMaxEnergy(ActiveIndex)`.

No polling in `Update`.

## Hookup

The HUD prefab and Player prefab are instantiated independently by [StageSession.cs](Assets/_Project/Scripts/StageSession.cs). After both spawn, StageSession calls a new `HUD.Bind(Health, WeaponInventory)` method to wire the references. Avoids `GameObject.Find` and keeps HUD decoupled from any singleton.

The existing `Energy` and `Health` children in [GameplayHUD.prefab](Assets/_Project/UI/GameplayHUD.prefab) are kept; HUD.cs exposes two `[SerializeField]` fields for their fill `Image` components.

## Phased rollout

**Phase 1 (this spec):**
1. Add public getters + events to `WeaponInventory` as listed above.
2. Fire `ActiveWeaponChanged` from `CycleWeapon` and from the auto-switch path in `TryFire`.
3. Fire `EnergyChanged` at the end of the energy-debit block in `TryFire`.
4. Rewrite `HUD.cs` as a view: `[SerializeField] Image _healthFill; [SerializeField] Image _energyFill; [SerializeField] GameObject _energyRoot;` + a `Bind(Health, WeaponInventory)` method + event subscriptions.
5. Restructure the HUD prefab's `Health` and `Energy` children into the background + fill pair described under "Visual." Authoring is by-hand in the Unity editor — no editor-script scene composition (project convention).
6. Wire `StageSession.Start` to call `Bind` after both `_playerPrefab` and `_hudPrefab` instantiate.

## Verification

- Play `Gameplay.unity`. HP bar renders top-left at full height.
- Take a hit from `TargetDummy` contact damage → HP fill shrinks immediately, no tween.
- Cycle to a special weapon with `E` → energy bar appears, colored with the weapon's tint.
- Fire the special weapon → energy fill shrinks per shot.
- Drain the weapon to 0 → auto-switch kicks in, bar disappears (buster re-activated).
- Cycle back to the special weapon → bar reappears at 0, immediately refills if you author a pickup later.

## Non-goals / guardrails

- No `GameObject.Find`, no singletons for HUD data access — use the `Bind` injection path.
- No per-frame polling of `Health` or `WeaponInventory` state — all updates event-driven.
- No coupling HUD → `WeaponData` asset layout beyond reading `.tint` and `.maxEnergy`. If `WeaponData` grows new fields, the HUD should not care.
- No editor-script scene composition. Prefab edits for the background/fill pair are authored by hand in the Unity editor.
