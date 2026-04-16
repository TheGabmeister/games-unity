# SPEC — Coyote Time, Dash-Jump, Ladder Climb

Status: **Design complete, unimplemented (ladder + dash-jump).** Coyote / buffer exist; audit pass only.

This document specs three items from `README.md` §1 (Core player physics):

- Coyote time / jump buffer
- Dash-jump
- Ladder climb

All three land in [PlayerController.cs](Assets/_Project/Scripts/PlayerController.cs). A new small type (`LadderZone`) accompanies the ladder work. No scenes are scripted; ladder prefabs + test rooms are hand-authored.

---

## 1. Coyote time & jump buffer

### 1.1 Current state

Both mechanics are already implemented:

- `coyoteTime = 0.1f`, `jumpBufferTime = 0.1f` — [PlayerController.cs:27-28](Assets/_Project/Scripts/PlayerController.cs#L27-L28)
- `coyoteTimer` refreshes in `FixedUpdate` whenever grounded — [PlayerController.cs:241](Assets/_Project/Scripts/PlayerController.cs#L241)
- `jumpBufferTimer` is set on jump press and consumed on successful jump — [PlayerController.cs:144](Assets/_Project/Scripts/PlayerController.cs#L144), [PlayerController.cs:243-244](Assets/_Project/Scripts/PlayerController.cs#L243-L244)
- Coyote is cleared on consumption and on wall-jump — [PlayerController.cs:321](Assets/_Project/Scripts/PlayerController.cs#L321), [PlayerController.cs:328](Assets/_Project/Scripts/PlayerController.cs#L328)

### 1.2 Required changes

**Tuning.** Lock both values to `0.1f` (= 6 frames @ 60 Hz). Already the current value — no code change, just a comment documenting the decision:

```csharp
// 6 frames @ 60 Hz. See SPEC.md §1 — both values are intentionally paired;
// if one moves, the other should move with it.
[SerializeField] float coyoteTime = 0.1f;
[SerializeField] float jumpBufferTime = 0.1f;
```

**Scope: walk-off-ledge only.** Coyote must be granted *only* when the player leaves the ground without jumping. Audit required:

- `isGrounded` loss due to `velocity.y > 0` (jump press) — **must not** leave `coyoteTimer` > 0. Current code is correct because `coyoteTimer` is only refreshed while `isGrounded == true`, and `isGrounded` is already gated on `velocity.y <= 0.0001f` ([PlayerController.cs:279](Assets/_Project/Scripts/PlayerController.cs#L279)).
- `isGrounded` loss due to dash off ledge — **must not** preserve coyote (per Q1 answers). Add a dash-start side effect: `coyoteTimer = 0f` inside `TryStartDash`.
- Wall-slide dropoff — **no** wall-coyote grace (Q4). No change needed; wall-jump is gated on live `isTouchingWall`, not a timer.
- Knockback/damage — out of scope here; revisit when damage knockback lands (README §1).

### 1.3 Acceptance

- Run off a ledge → first 6 frames still allow a normal jump; frame 7+ does not.
- Press Jump up to 6 frames before landing → jump fires on touchdown.
- Ground-dash off a ledge → no jump available after dash ends (coyote cleared).
- Wall-slide, stop pressing into wall, slide off bottom → no wall-jump after detach.

---

## 2. Dash-jump

### 2.1 Behavior

When the player presses Jump **while `IsDashing == true`**, the resulting jump preserves the dash's horizontal speed for the **entire airtime**, regardless of stick input (Q2).

- **Trigger:** Jump pressed during active dash (`dashTimer > 0f`).
- **Effect:** Dash ends immediately, jump fires with `velocity.y = jumpSpeed`, `velocity.x = dashDirection * dashSpeed`, and a new `dashJumpLock` flag is set.
- **Duration of speed lock:** Until *any* ground contact — solid or future semisolid (Q3). Reset on `isGrounded == true` next FixedUpdate, on wall contact that triggers a wall-jump, or on ladder grab.
- **Mid-air behavior while locked:** `ApplyHorizontalInput()` is suppressed. `velocity.x` stays pinned to `dashDirection * dashSpeed` even if the player releases the stick or presses the opposite direction. Gravity and vertical behavior unchanged.
- **Wall contact:** Player can wall-slide and wall-jump as normal. **Wall-jump resets horizontal to standard `wallJumpVelocity.x`** — the lock does not propagate across walls (Q2b).
- **Air-dash:** Still available once per airtime (Q2c). Triggering air-dash *does not* extend the lock beyond the dash itself; when air-dash ends, `velocity.x` stays locked at `dashDirection * dashSpeed` using the dash-jump lock (air-dash and ground-dash speeds are equal, so this is a no-op in current tuning — flag kept for future divergence).
- **Jump-cut:** `jumpCutMultiplier` still applies to `velocity.y`. Horizontal lock is independent.
- **Ladder:** Grabbing a ladder clears the lock.

### 2.2 State

```csharp
bool dashJumpLock;
```

Added alongside the existing dash fields. Cleared in:

- `FixedUpdate` when `isGrounded` becomes true
- `TryJump` wall-jump branch (before writing `velocity`)
- Ladder grab (`EnterLadder`)

### 2.3 Control flow changes in `FixedUpdate`

Current — [PlayerController.cs:251-255](Assets/_Project/Scripts/PlayerController.cs#L251-L255):

```csharp
if (IsDashing)
    velocity = new Vector2(dashDirection * dashSpeed, 0f);
else if (wallJumpLockTimer <= 0f)
    ApplyHorizontalInput();
```

Replace with:

```csharp
if (IsDashing)
    velocity = new Vector2(dashDirection * dashSpeed, velocity.y);   // keep vertical; dash-jump leaves dash active for 0 frames so this is OK
else if (dashJumpLock)
    velocity.x = dashDirection * dashSpeed;
else if (wallJumpLockTimer <= 0f)
    ApplyHorizontalInput();
```

Note the first branch changes from `(x, 0)` to `(x, velocity.y)`. Required because gravity must still take effect during the fleeting post-jump-press frame when dash + airborne coexist. Dash is normally ground-only; review carefully.

### 2.4 Jump path

In `TryJump`, add a dash-jump branch **before** coyote:

```csharp
if (IsDashing)
{
    velocity = new Vector2(dashDirection * dashSpeed, jumpSpeed);
    dashTimer = 0f;
    dashJumpLock = true;
    coyoteTimer = 0f;
    return true;
}
```

### 2.5 Acceptance

- Ground-dash, press Jump mid-dash → player leaves ground at dash speed; releasing stick mid-air does not slow horizontal.
- Dash-jump into a wall, wall-jump off → post-wall-jump horizontal is `wallJumpVelocity.x` (not dash speed).
- Dash-jump, air-dash → air-dash works once; speed stays locked.
- Dash-jump, land on ground → next walk/run uses normal acceleration (`ApplyHorizontalInput` active).
- Plain jump (not during dash) → no lock; `ApplyHorizontalInput` active as today.

---

## 3. Ladder climb

### 3.1 Data representation

No script. A ladder is a `BoxCollider2D` with `isTrigger = true` on a new `Ladder` layer, added in Tags & Layers. One collider per authored ladder. A `Ladder.prefab` template is checked in so designers duplicate a known-good configuration (collider + layer + any visual) rather than hand-building each.

All values the player needs come from the collider's world-space bounds, read at query time:

- Center X: `col.bounds.center.x` (used for snap-to-center on grab). Preferred over `transform.position.x` so an offset `BoxCollider2D` or a nested parent transform doesn't break snap alignment.
- Top Y: `col.bounds.max.y` (auto-dismount threshold).
- Bottom Y: `col.bounds.min.y` (drop-off threshold).

Player queries with a short `Physics2D.OverlapBox` on the `Ladder` layer each FixedUpdate — does not use `rb.Cast` because that filter excludes triggers ([PlayerController.cs:106](Assets/_Project/Scripts/PlayerController.cs#L106)).

If per-ladder behavior is ever needed (climb-speed override, one-way ladders, on-attach events), add a `LadderZone` component later and `GetComponent<LadderZone>()` at the single query site in `PlayerController`. One-line change; defer until demanded.

### 3.2 Player state

Add to `PlayerController`:

```csharp
[Header("Ladder")]
[SerializeField] LayerMask ladderLayer;
[SerializeField] float climbSpeed = 5f;

Collider2D currentLadder;
bool onLadder;
bool climbingShootLock;   // shoot halts climb, locks facing
float ladderShootLockUntil;
```

### 3.3 Grab conditions (per Q3)

In `FixedUpdate`, after `Probe()`, before anything else — detect ladder overlap:

```csharp
Collider2D ladder = QueryLadder();   // OverlapBox on ladderLayer
bool pressingUp   = moveInput.y >  0.5f;
bool pressingDown = moveInput.y < -0.5f;

if (!onLadder && ladder != null)
{
    bool grabFromAir    = pressingUp || (pressingDown && !isGrounded);
    bool grabFromGround = pressingUp;   // Down at ground does nothing
    bool grabFromTop    = pressingDown && isGrounded && AtLadderTop(ladder);
    if (grabFromAir || grabFromGround || grabFromTop) EnterLadder(ladder);
}
```

`EnterLadder(Collider2D ladder)`:

- `currentLadder = ladder; onLadder = true;`
- Snap `rb.position.x = ladder.bounds.center.x` (Q3a). Instantaneous, no tween.
- `velocity = Vector2.zero`.
- Clear `dashJumpLock`, `wallJumpLockTimer`, `coyoteTimer`.
- Cancel any active dash (`dashTimer = 0f`).

### 3.4 Climb behavior

While `onLadder`:

- **Movement:** `velocity = new Vector2(0, moveInput.y * climbSpeed)` unless shoot-locked, then `velocity = Vector2.zero`.
- **Gravity:** suppressed. Short-circuit `ApplyGravity`.
- **Horizontal input:** ignored for movement, but Left/Right at the top rung dismounts (see §3.5).
- **Dash:** `TryStartDash` early-returns when `onLadder`. No dashing on ladder.
- **Shooting:** `OnAttackStarted` still builds charge as normal (Q5b). On `OnAttackCanceled` (fire), set `climbingShootLock = true` for a short window (`ladderShootLockUntil = Time.time + 0.2f`) and lock `facing` to its current value. Climbing motion is zeroed while locked. After the window elapses, resume climbing.
- **Facing:** normally ladder climbing does not update `facing` from input (prevents facing flicker mid-shot-window). Exception: if the player presses Left or Right while *not* at the top rung, `facing` updates so the next shot aims that way, but no horizontal motion occurs.
- **Sprite:** new climb sprite (see §3.8). Shooting plays a separate shoot-on-ladder pose.
- **Wall contact / wall-slide:** suppressed while `onLadder`. `isTouchingWall` logic gated on `!onLadder`.
- **Facing flip on Visual:** standard rule still applies via the `facing` int.

### 3.5 Release / dismount (per Q3c, Q3d)

| Trigger | Resulting state |
|---|---|
| Jump pressed | Exit ladder. `velocity.y = jumpSpeed`. `velocity.x = moveInput.x * moveSpeed` if held, else `0` (Q3d). No dash-jump from ladder. |
| Reach top rung (`rb.position.y >= currentLadder.bounds.max.y`) | Auto-dismount (Q3b). Snap `rb.position.y = top + halfHeight`, clear ladder, set `velocity = Vector2.zero`. |
| Pressing Down at bottom rung (`rb.position.y <= currentLadder.bounds.min.y` and `pressingDown`) | Release, fall with gravity. Standard airborne state. |
| Pressing Left/Right while overlapping floor of the top rung (legacy climb-up alternative) | Dismount sideways. Matches user answer for "Left/Right at ladder top". |
| Take damage (`Health.OnDamage`) | Release, brief i-frames, no horizontal knockback (Q3b). See §3.7. |
| Overlap with ladder collider ends (design error / edge) | Release, fall. |
| Shoot button | **Does not release.** Shoot triggers the climb halt + facing lock. Consistent with Q1a and Q5b. |

### 3.6 FixedUpdate early-return structure

Ladder state bypasses the normal physics branches:

```csharp
void FixedUpdate()
{
    Probe();                                // still runs — keeps isGrounded accurate for dismount-on-jump
    var ladder = QueryLadder();
    if (!onLadder) TryGrabLadder(ladder);

    if (onLadder)
    {
        TickLadder();
        return;
    }

    // ... existing flow ...
}
```

### 3.7 Damage interaction

`Health` raises an `OnDamaged` event (existing component — see [Health.cs](Assets/_Project/Scripts/Health.cs)). `PlayerController` subscribes and, if `onLadder`, calls `ExitLadder(fall: true)` which clears ladder state and leaves gravity to take over. Knockback is explicitly not applied on ladder (Q3b). This is the only ladder-specific damage hook; standard knockback will apply off-ladder when implemented.

### 3.8 Visuals

New SVG assets:

- `Assets/_Project/Player/Character/MegamanX_Climb.svg` — 2-frame climb (top half / bottom half). Swap at a 0.2 s cadence driven by `rb.position.y` delta (only advance while moving).
- `Assets/_Project/Player/Character/MegamanX_ClimbShoot.svg` — one-handed shoot pose while clinging.

Authored facing-left, wrapped in `<g transform="translate(128 0) scale(-2 2)">` per the existing convention (see [CLAUDE.md](CLAUDE.md) §"Visual child / sprite flip"). No new flip code required.

`UpdateSprite` gains a new branch:

```csharp
if (onLadder)
    s = climbingShootLock ? climbShootSprite : climbSprite;
```

Added before the existing `IsDashing` branch.

### 3.9 Acceptance

- Overlap a ladder, press Up → player snaps to ladder center, begins climbing.
- Overlap a ladder standing on ground, press Down → no-op (unless at ladder-top).
- Climb up; reach top rung → auto-dismount, idle on platform.
- Climb; press Down at bottom → drop off, fall.
- Climb; press Jump → normal jump arc; if Right held, player arcs right at walk speed; otherwise straight up.
- Climb; press Attack → charge builds; release → shot fires, facing frozen, climb halts for 0.2 s.
- Climb; take damage from contact enemy → drop off, i-frames tick, gravity applies.
- Press Dash while on ladder → ignored.
- Dash-jump *into* a ladder (airborne, Up held) → grab works; dash-jump lock cleared.

---

## 4. Cross-cutting concerns

### 4.1 Tuning location (Q4c)

All new tunables are `[SerializeField]` fields on `PlayerController`, matching the existing pattern ([PlayerController.cs:19-48](Assets/_Project/Scripts/PlayerController.cs#L19-L48)):

- `climbSpeed`, `ladderLayer` (new `[Header("Ladder")]` block)
- No new coyote / dash-jump fields — dash-jump uses existing `dashSpeed` and `jumpSpeed`, and lock is a `bool` flag.

Revisit as `PlayerTuning` ScriptableObject when X and Zero need divergent values. Not now.

### 4.2 Input

No new actions. Reuses existing `Move` (Vector2), `Jump`, `Sprint` (dash), `Attack`. Up/Down on the Move action drives ladder grab and climb.

### 4.3 Layers

Add `Ladder` layer in **Edit → Project Settings → Tags and Layers**. Update `environmentLayers` default (currently `~0`) to exclude `Ladder` so ground probing doesn't see ladder triggers. Ladder colliders are triggers, so in practice `useTriggers = false` already filters them out from `rb.Cast` ([PlayerController.cs:106](Assets/_Project/Scripts/PlayerController.cs#L106)) — the layer split is defense-in-depth for designers who might author a non-trigger ladder by accident.

### 4.4 Non-goals / deferred

- **Zero-specific ladder attacks** (saber slash on ladder) — out of scope per Q4b. Revisit when Zero's moveset lands.
- **Dash-jump from ladder** — explicitly disabled per Q5c.
- **Moving ladders / ladders on moving platforms** — not needed for any X4 stage this phase.
- **Ladder → wall-jump chain** — wall-jump is suppressed while on ladder; no chain possible.
- **Semisolid platforms** — dash-jump lock ends on any ground contact regardless (Q3).

---

## 5. Testing plan (Q5d)

EditMode only. New test file: `Assets/_Project/Tests/Editor/PlayerControllerTests.cs`. Targets pure timing/math; no scene instantiation.

Tests factor out the fragments below into small internal/testable helpers or call through an instantiated-but-detached `PlayerController` + reflection on private fields (Unity test convention):

- **Coyote window.** After simulated `isGrounded = true → false`, `TryJump` succeeds within 6 fixed frames of dt=1/60, fails at frame 7.
- **Jump buffer.** `jumpBufferTimer` set, then `isGrounded = true` within 6 frames → jump fires; beyond 6 frames → does not.
- **Dash-jump speed lock.** With `dashJumpLock = true`, `FixedUpdate` pins `velocity.x = dashDirection * dashSpeed` regardless of `moveInput.x`. Clearing `dashJumpLock` re-enables `ApplyHorizontalInput`.
- **Dash-jump clears on ground.** One FixedUpdate with `isGrounded = true` clears the lock.
- **Wall-jump resets dash-jump.** Manually set lock + wall-contact + trigger `TryJump` → `velocity.x == -facing * wallJumpVelocity.x`, lock cleared.
- **Coyote cleared on dash start.** `TryStartDash` zeroes `coyoteTimer`.

PlayMode integration tests deferred (Q5d). Ladder behavior will be QA'd in-editor against a hand-authored test room in `Gameplay.unity`.

---

## 6. Out of scope for this spec

Tracked in [README.md](README.md#1-core-player-physics--shared-by-x-and-zero) but not in this document:

- Damage knockback / i-frames (general — not the ladder-specific case above)
- Death + respawn at checkpoint
- Pause menu

These three ladder/dash-jump mechanics do not depend on them.

---

## 7. Implementation order

When the user signals "go," suggested order:

1. Audit coyote/buffer values, add `TryStartDash` coyote clear, add comment. (Trivial.)
2. `dashJumpLock` flag + `FixedUpdate` branch + dash-jump path in `TryJump`. (Small; ~20 LOC.)
3. Add `Ladder` layer + author `Ladder.prefab` (trigger `BoxCollider2D`, correct layer). (Trivial.)
4. Ladder state + grab/tick/release in `PlayerController`. (Medium; ~80 LOC.)
5. Climb / climb-shoot SVG assets + sprite swap branch.
6. Health → PlayerController damage-on-ladder hook.
7. EditMode tests.
8. Hand-author a ladder-and-pits test room inside `Gameplay.unity` for manual QA.

Check off `README.md` §1 items (Coyote, Dash-jump, Ladder climb) once playable end-to-end.
