# SPEC_PAUSEMENU

## Context

In *Mega Man X4*, pressing Start inside a stage opens a modal **weapon-select overlay**: the action freezes, a grid of acquired special weapons appears, the player picks one, and unpausing resumes play with that weapon active. HP / sub-tanks / armor-parts readouts flank the grid. It is not a separate scene — it is a UI layer drawn on top of the frozen stage.

That framing matters here because [SPEC_GAMESTATE.md](SPEC_GAMESTATE.md) explicitly leaves pause out of the top-level `GameState` enum. Pause is a Gameplay-local concern, owned by the already-persistent gameplay systems. This spec does not introduce a new top-level state.

---

## Target layout

End-state after Phase 2 + Phase 3. The frozen stage renders underneath; the overlay is drawn on top with a dimmed backdrop.

```
 +----------------------------------------------------------+
 |                       P A U S E D                        |
 |                                                          |
 |    HP              WEAPONS                 ENERGY        |
 |   +----+       +----+----+----+           +----+         |
 |   |####|       | BS | LW | AL |           |####|         |
 |   |####|       +----+----+----+           |####|         |
 |   |####|       | DC | RF | GH |           |####|         |
 |   |####|       +----+----+----+           |####|         |
 |   |    |       | TS |[FT]| SB |           |    |         |
 |   |    |       +----+----+----+           |    |         |
 |   +----+           cursor on FT           +----+         |
 |    16/32         FROST TOWER               24/28         |
 |                                                          |
 |          [Navigate]  [Submit]  [Cancel / Pause]          |
 +----------------------------------------------------------+
```

Cells: `BS` = Buster (slot 0, always authored), `LW` = Lightning Web, `AL` = Aiming Laser, `DC` = Double Cyclone, `RF` = Rising Fire, `GH` = Ground Hunter, `TS` = Twin Slasher, `FT` = Frost Tower, `SB` = Soul Body. Unauthored weapons render as `· ·` and are skipped by the cursor. The brackets around `[FT]` mark the current cursor position; the name + energy bar on the right update as the cursor moves. The HP column on the left reflects the live `Health` state captured the moment pause opened.

Phase 1 is just the bordered `P A U S E D` label in the center — no HP column, no grid, no energy column.

Later phases graft onto the same frame: Phase 4 adds sub-tank dots under the HP bar; Phase 5 adds an armor-parts strip below the grid; Phase 6 adds a Resume / Quit column on the far right.

---

## Current state *(Phase 1 — implemented)*

- **Input:** a `Pause` action exists on the `Player` action map in [InputSystem_Actions.inputactions](Assets/_Project/Input/InputSystem_Actions.inputactions); a full `UI` action map (`Navigate`, `Submit`, `Cancel`) is already authored and unused.
- **Toggle:** [StageSession.cs](Assets/_Project/Scripts/StageSession.cs) owns `_isPaused`, binds the `Pause` action on player spawn, calls `SetPaused(bool)` which sets `Time.timeScale` to 0 / 1 and notifies the HUD. Public `IsPaused` + `TogglePause()` expose the state.
- **View:** [HUD.cs](Assets/_Project/Scripts/HUD.cs) receives `SetPaused(bool)` and draws a centered "PAUSED" label via `OnGUI`.
- **Safety:** pause auto-clears on player death and on `OnDestroy` (scene reload). `TogglePause` no-ops while `_isReloading`.

This is the foundation every later phase builds on. It stays.

---

## Scope

- Pause is a **modal Gameplay overlay**. It does not become a `GameState`.
- `StageSession` remains the single source of truth for paused state. New UI is a **view** that subscribes to or is driven by `SetPaused`.
- While paused, gameplay input (movement, jump, attack, weapon-cycle) is suppressed; menu input (`Navigate` / `Submit` / `Cancel`) drives the overlay; `Pause` itself remains responsive for un-pausing.
- No asset dependencies beyond what each phase explicitly needs. Procedural UI (primitive `Image`s, SVG tiles, `TextMeshPro`) until real assets land.

---

## Ownership

Introduce a **`PauseMenu`** component that lives on `GameplayHUD.prefab` (the same prefab `StageSession` already instantiates and wires via [HUD.Bind](Assets/_Project/Scripts/HUD.cs#L18)). It is a peer of `HUD`, not a subclass.

Responsibilities:

- Own the pause-overlay UI subtree (grid, cursor, labels).
- Subscribe to gameplay data it displays: `WeaponInventory.EnergyChanged`, `WeaponInventory.ActiveWeaponChanged`, `Health.HealthChanged`. Mirror the event-driven HUD pattern — **no `Update` polling**.
- Show / hide on `SetPaused(bool)` (called by `StageSession` alongside the existing `HUD.SetPaused`).
- Handle menu input via the `UI` action map; commit selections by writing to `WeaponInventory.SetActiveIndex(i)` (new method, see Phase 2).

`StageSession` gains a single extra call in `SpawnPlayer`:

```csharp
_pauseMenu = hudGo.GetComponent<PauseMenu>();
if (_pauseMenu) _pauseMenu.Bind(_playerHealth, weapons);
```

…and an additional `_pauseMenu.SetPaused(paused)` in `SetPaused`. `HUD` itself stays as-is — its "PAUSED" `OnGUI` label is replaced by the new overlay being visible, and the OnGUI code is removed.

---

## Input switching

On pause enter / exit, `StageSession` switches `PlayerInput.SwitchCurrentActionMap`:

- `Gameplay → Paused` on enter: switch to the `UI` action map. Player prefab's gameplay actions stop firing; the held Jump / Attack bindings naturally cancel (standard Input System behavior).
- `Paused → Gameplay` on exit: switch back to the `Player` action map.

The `Pause` action needs to fire in **both** maps, so either:

1. Duplicate the `Pause` binding into the `UI` map (simplest), or
2. Keep `Pause` only on `Player` and bypass the action-map switch for just that action by reading it from the asset rather than the active map.

Go with option 1. One binding per map, both bound to the same `Start` / `Esc` physical inputs.

---

## Phase 2 — Weapon-select grid

The core of the MMX-style pause menu. Picks a new active weapon.

### UI layout

- Centered grid. Slot count = `WeaponInventory._weapons.Count` (serialized list, grows as weapons ship).
- Each slot shows the weapon's icon (tinted flat color from `WeaponData.tint` until icon SVGs exist) and its energy as a short vertical bar. Slots with `maxEnergy == 0` (buster) show infinity or no bar.
- Unauthored slots (null entries in `_weapons`) are drawn greyed-out and non-selectable.
- A selection cursor renders behind the currently-highlighted slot.

### Interaction

- Opening pause highlights the current `ActiveIndex`.
- `Navigate` (left / right / up / down) moves the cursor; unauthored slots are skipped.
- `Submit` commits: calls `WeaponInventory.SetActiveIndex(cursorIndex)`, which fires `ActiveWeaponChanged` as usual, then closes pause.
- `Cancel` closes pause without changing the selection.
- `Pause` (Start) also closes pause without changing the selection (same as Cancel).

### `WeaponInventory` additions

[WeaponInventory.cs](Assets/_Project/Scripts/Player/WeaponInventory.cs) exposes Q/E cycling today but not direct selection. Add:

```csharp
public void SetActiveIndex(int index);
```

Validates bounds, no-ops on invalid / unauthored / empty-energy slots, runs the same swap side-effects as Q/E (cancel charge, refresh sprite tint, fire `ActiveWeaponChanged`).

### Out of scope for Phase 2

- Sub-tank icons (Phase 4).
- Armor-parts status (Phase 5).
- Animation / ease-in of the overlay. First pass is instant show / hide.

---

## Phase 3 — HP + energy read-out

Numeric / bar readouts of current stats, flanking the weapon grid.

- **HP column:** shows `Health.CurrentHealth` / `Health.MaxHealth` as a vertical bar (mirrors the MMX HUD column) plus the numeric value.
- **Active weapon energy column:** current energy of the highlighted slot (updates as the cursor moves). Buster shows infinity.

Subscribes to `Health.HealthChanged` and `WeaponInventory.EnergyChanged`. No `Update` polling.

### Out of scope for Phase 3

- Separate displays per non-active weapon. The grid already shows per-slot energy bars from Phase 2.

---

## Deferred phases

Each of these is deferred behind a system that doesn't exist yet. Listing them here so the menu layout stays compatible with them when they land.

### Phase 4 — Sub-tank display *(deferred)*

Depends on sub-tank pickup / storage being implemented. Once that's in:

- Show the three sub-tank slots below HP.
- Each slot shows current fill fraction.
- `Submit` on a filled sub-tank pours it into HP up to `MaxHealth`; empty / full-HP sub-tanks are non-selectable.

### Phase 5 — Armor parts indicator *(deferred, X only)*

Depends on Fourth Armor pickups being implemented. Indicator strip for Helmet / Body / Arms / Legs showing acquired / not acquired. Purely informational — selection does nothing.

### Phase 6 — Resume / Quit menu *(deferred)*

Depends on the `LevelSelect` scene existing (see [SPEC_GAMESTATE.md](SPEC_GAMESTATE.md)). Adds a secondary column / sub-menu:

- **Resume** — close pause.
- **Quit to Stage Select** — call `GameStateController.RequestTransition(LevelSelect)` per SPEC_GAMESTATE's transition pattern.

Until LevelSelect exists, the pause menu's only exit is Resume / Cancel, which is already true today.

---

## Edge cases + invariants

- **Pause cannot open during a pending scene reload.** `StageSession._isReloading` already gates `TogglePause`; preserve that.
- **Pause cannot open during player death animation.** Same gate — `Died` flips `_isReloading`.
- **`Time.timeScale == 0` applies to all `Update` / `FixedUpdate` consumers.** `PauseMenu` must use `WaitForSecondsRealtime` / `Time.unscaledDeltaTime` if it ever needs a timer (e.g. cursor-repeat). Avoid timers entirely where possible.
- **Coroutines running on enemies / projectiles freeze during pause.** This is desired; the overlay is a true freeze.
- **`OnGUI` "PAUSED" label is removed** once Phase 2 lands. The visible overlay replaces it.

---

## Verification (per phase)

### Phase 2

- Opening pause highlights the active slot.
- Navigate skips null / unauthored slots.
- Submit on a different slot swaps weapon (tint updates on resume, `ActiveWeaponChanged` fires exactly once).
- Cancel / Pause close without touching `ActiveIndex`.
- While paused, the player does not move / jump / fire.
- Resuming restores gameplay input cleanly (held Jump doesn't auto-fire a jump after resume).

### Phase 3

- HP bar updates when `Health.HealthChanged` fires (test: open pause, use editor to call `ApplyDamage`, close pause — bar matches on next open).
- Energy column tracks the cursor, not the active weapon.

---

## Out of scope (whole document)

- Title / LevelSelect menus. Those are separate scenes with separate UI, owned per [SPEC_GAMESTATE.md](SPEC_GAMESTATE.md).
- Game-over screen. Currently a scene reload; not modeled here.
- Cutscene or dialogue pauses. Those would be separate modal states with their own input gating.
- Settings / remap menu. Out of scope until an in-game options system lands.
