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

Cells: `BS` = Buster (slot 0, always authored), `LW` = Lightning Web, `AL` = Aiming Laser, `DC` = Double Cyclone, `RF` = Rising Fire, `GH` = Ground Hunter, `TS` = Twin Slasher, `FT` = Frost Tower, `SB` = Soul Body. Cell visual state is one of three: *unauthored* (null slot — `· ·`, non-selectable), *empty-energy* (dimmed, non-selectable), *available* (full tint, selectable). The brackets around `[FT]` mark the current cursor position; the name + energy bar on the right update as the cursor moves. The HP column is event-driven on `Health.HealthChanged` — it reads live state, which happens to stay stable during pause because nothing damages at `timeScale=0`.

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

**No new component.** The existing `HUD` on `GameplayHUD.prefab` absorbs the pause-overlay role; the overlay is a new child subtree on the same Canvas, toggled on `SetPaused(bool)`.

A separate `PauseMenu` would share every reference (`Health`, `WeaponInventory`), share the same lifecycle (spawned with the player in `StageSession.SpawnPlayer`, destroyed on scene reload), and share the same prefab. The only thing it would "own" is menu input, and that gates cleanly inside existing callbacks via `if (!_isPaused) return;`. Not worth the duplicated `Bind`/subscribe/unsubscribe plumbing.

`HUD` responsibilities grow to:

- **Existing:** render HP fill, energy fill, active-weapon tint; subscribe to `Health.HealthChanged` and `WeaponInventory.EnergyChanged` / `ActiveWeaponChanged`.
- **New (Phase 2+):** show / hide the `Overlay` child subtree on `SetPaused`. Render the weapon grid + cursor. Subscribe to the `UI` action map's `Navigate` / `Submit` / `Cancel` and early-return the callbacks unless `_isPaused`.

`StageSession` changes: **none beyond what exists today**. Its existing `_hud.SetPaused(paused)` already flows through `HUD`, which now drives the overlay. The `OnGUI` "PAUSED" label and `CreatePauseLabelStyle` are deleted once Phase 2 lands.

---

## Input switching

Gameplay input must stop during pause, menu input must start, and `Pause` itself must remain responsive in both states.

### Held-input policy

Held buttons at the moment of pause are **consumed**. On resume, the player must re-press to re-trigger any gameplay action (jump, attack, dash). This matches the Input System's native behavior after `action.Disable()` and sidesteps the held-carry-over problem entirely — no snapshot / restore plumbing needed.

Rationale: MMX-style gameplay favors intentional re-presses after menu work; silently re-triggering a held jump on resume is worse UX than the one extra tap, and also dodges the state-desync trap where `PlayerController`'s `canceled` callbacks fire on pause but `started` never re-fires on resume.

### Map enable/disable, not `SwitchCurrentActionMap`

Do **not** use `PlayerInput.SwitchCurrentActionMap`. Instead, `Enable()` / `Disable()` the two maps individually in `StageSession.SetPaused`:

- On pause enter: `Player.Disable()`, then `UI.Enable()`.
- On pause exit: `UI.Disable()`, then `Player.Enable()`.

Both maps off-by-default for the one not in use — avoids cross-map binding collisions (e.g. Left-Stick double-firing `Move` and `Navigate`).

### Pause action moves to a `Meta` action map

Move the `Pause` action from the `Player` map to a new **`Meta`** action map in [InputSystem_Actions.inputactions](Assets/_Project/Input/InputSystem_Actions.inputactions). `Meta` stays enabled for the lifetime of the gameplay session; it is **never** disabled by the pause / resume toggles.

```csharp
// StageSession.Start, in place of the current BindPauseAction binding on "Player":
_metaMap = _playerInput.actions.FindActionMap("Meta");
_metaMap.Enable();
_pauseAction = _metaMap.FindAction("Pause");
_pauseAction.performed += OnPausePerformed;
```

Why a separate map instead of `_pauseAction.Enable()` on the `Player` map: `Player.Disable()` does disable individually-enabled actions on that map too, so an always-on solution via individual `Enable()` would require re-enabling `Pause` after every `Player.Disable()`. A dedicated `Meta` map avoids that runtime bookkeeping — a one-line asset edit beats recurring runtime state management.

`Cancel` on the `UI` map also closes pause (both paths route through `StageSession.SetPaused(false)`).

---

## Overlay UI architecture

The overlay is a subtree on the same Screen-Space-Overlay `Canvas` that already hosts the live HUD. Hierarchy under `GameplayHUD.prefab`:

```
GameplayHUD (prefab root)
└── Canvas (Screen Space - Overlay, existing)
    ├── HealthBar           (existing)
    ├── EnergyBar           (existing)
    └── Overlay             (new — SetActive(_isPaused))
        ├── Backdrop        (full-screen Image, black, alpha ≈ 0.65, raycastTarget=false)
        ├── PausedLabel     (TextMeshProUGUI, centered top)
        ├── HpColumn        (vertical fill + numeric label)
        ├── WeaponGrid      (GridLayoutGroup, one cell per WeaponData slot)
        │   └── Cursor      (Image, re-parented to the selected cell)
        └── EnergyColumn    (vertical fill + numeric label, tracks the cursor slot)
```

Key choices:

- **Single Canvas.** `Overlay` is the last child, so it draws on top of `HealthBar` / `EnergyBar` without a second Canvas or sort-order fiddling.
- **Dim via alpha, not post-processing.** A black `Image` at ~0.65 alpha is enough; keeps the overlay a part of the HUD (no camera override, no URP volume change).
- **`raycastTarget = false` on Backdrop.** This game drives menu nav through the Input System's `UI` action map, not pointer clicks — so nothing in the overlay needs to block raycasts.
- **Cursor movement via re-parent.** Moving the cursor re-parents its `RectTransform` under the selected cell and sets `localPosition = Vector3.zero`. GridLayoutGroup handles the world placement; no per-frame `Update` tween.
- **Procedural visuals.** Cell backgrounds are flat `Image` with `color = WeaponData.tint` (× 0.3 alpha for empty-energy, dots-placeholder for unauthored). TMP default font everywhere. Icon SVGs slot in later without spec changes.

---

## Phase 2 — Weapon-select grid

The core of the MMX-style pause menu. Picks a new active weapon.

### UI layout

- Centered `GridLayoutGroup` under `Overlay/WeaponGrid`. Slot count = `WeaponInventory._weapons.Count` (serialized list, grows as weapons ship).
- Each cell's visual state is one of three:
  - **Unauthored** — `_weapons[i] == null`. Rendered as a `· ·` placeholder, non-selectable.
  - **Empty-energy** — non-null with `maxEnergy > 0 && GetEnergy(i) == 0`. Rendered dimmed (tint × 0.3 alpha), non-selectable. Rationale: selecting a drained slot would flash `ActiveWeaponChanged` and then auto-switch back to buster on first fire attempt — worse UX than greying it out in the menu.
  - **Available** — otherwise. Full-alpha `WeaponData.tint`, selectable. Buster (slot 0) is always Available; its `maxEnergy == 0` renders `∞` in place of a bar.
- Cells carry a short vertical energy fill bar; the cursor `Image` renders behind the currently-highlighted cell.

### Interaction

- Opening pause places the cursor on the current `ActiveIndex` — always Available (it was firing a moment ago).
- `Navigate` (left / right / up / down) moves the cursor one cell at a time, skipping Unauthored and Empty-energy cells. If no selectable cell exists in that direction, **the cursor holds** — no wrap, no row-jump. The grid is small (3×3 at full roster), so wrap-around adds confusion without payoff.
- `Submit` commits: calls `WeaponInventory.SetActiveIndex(cursorIndex)`, then closes pause. `ActiveWeaponChanged` fires during the pause, tint writes synchronously, becomes visible on the first rendered frame after resume.
- `Cancel` closes pause, no changes.
- `Pause` (always-on) also closes pause, same semantics as Cancel.

### `WeaponInventory` additions

[WeaponInventory.cs](Assets/_Project/Scripts/Player/WeaponInventory.cs) exposes Q/E cycling today but not direct selection. Add:

```csharp
public void SetActiveIndex(int index);
```

Validates bounds, no-ops on invalid / null / empty-energy indices (defense-in-depth — the UI should already prevent those commits, but the runtime guard prevents drift if another caller is added later). On a valid swap, runs the same side-effects as Q/E: cancel charge, write tint to player `SpriteRenderer`, fire `ActiveWeaponChanged`.

### Out of scope for Phase 2

- Sub-tank icons (Phase 4).
- Armor-parts status (Phase 5).
- Animation / ease-in of the overlay. First pass is instant show / hide.

---

## Phase 3 — HP + energy read-out

Numeric / bar readouts of current stats, flanking the weapon grid.

- **HP column:** shows `Health.CurrentHealth` / `Health.MaxHealth` as a vertical bar plus the numeric value. Event-driven on `Health.HealthChanged`. During pause the value happens to be static because enemies can't damage at `timeScale=0` — that's not an implementation feature, just a side effect.
- **Active weapon energy column:** current energy of the **cursor-highlighted** slot (not the active slot). Re-reads from `WeaponInventory.GetEnergy(cursorIndex)` on every cursor move; also subscribes to `WeaponInventory.EnergyChanged` so capsule pickups / sub-tank refills (future) propagate without an extra hook. Buster (`maxEnergy == 0`) shows `∞`.

No `Update` polling.

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
- **`Time.timeScale == 0` applies to all `Update` / `FixedUpdate` consumers.** `HUD` must use `Time.unscaledDeltaTime` if it ever needs a timer (e.g. cursor auto-repeat on held `Navigate`). Avoid timers entirely where possible — Unity's `EventSystem` already handles initial repeat naturally via `Navigate` `performed` callbacks.
- **Coroutines running on enemies / projectiles freeze during pause** (provided they use `WaitForSeconds`, not `WaitForSecondsRealtime`). This is the intended behavior — a true freeze.
- **`OnGUI` "PAUSED" label in [HUD.cs](Assets/_Project/Scripts/HUD.cs) is deleted once Phase 2 lands**, along with `CreatePauseLabelStyle` and the `PauseLabel` / `PauseLabelFontSize` constants. The visible `Overlay` subtree replaces them.

---

## Verification (per phase)

### Phase 2

- Opening pause places the cursor on the current active slot.
- `Navigate` skips Unauthored and Empty-energy cells; holds at the edge when no selectable neighbor exists in that direction.
- `Submit` on a different slot: `ActiveWeaponChanged` fires exactly once during pause; sprite tint is correct on the first rendered frame after resume.
- `Cancel` / `Pause` close without changing `ActiveIndex`, with no event emissions.
- While paused, movement / jump / attack / weapon-cycle inputs all no-op on the player.
- On resume, held inputs do **not** auto-trigger (per the consumed-on-pause policy): holding Jump across pause then releasing produces no jump on resume; the next jump requires a fresh press.

### Phase 3

- HP bar matches `Health.CurrentHealth` / `MaxHealth` on every pause open. Verify by damaging the player, opening pause, and checking the bar / numeric value.
- Energy column tracks the cursor, not the active weapon. Verify by selecting a non-active weapon slot with `Navigate` and watching the energy readout switch without Submit.

---

## Out of scope (whole document)

- Title / LevelSelect menus. Those are separate scenes with separate UI, owned per [SPEC_GAMESTATE.md](SPEC_GAMESTATE.md).
- Game-over screen. Currently a scene reload; not modeled here.
- Cutscene or dialogue pauses. Those would be separate modal states with their own input gating.
- Settings / remap menu. Out of scope until an in-game options system lands.
