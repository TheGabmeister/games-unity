# SPEC2 — Damage Knockback & Invincibility Frames

Status: **Design complete, unimplemented.**

Covers the "Damage knockback / invincibility frames" item in [README.md](README.md#1-core-player-physics--shared-by-x-and-zero) §1. This spec is a **prerequisite** for [SPEC.md](SPEC.md) §3.7 (ladder damage interaction), which assumes a working knockback/i-frame system. Land this first.

Touches three existing files plus one new component:

- [Health.cs](Assets/_Project/Scripts/Health.cs) — add invulnerability state + extend damage API
- [ContactDamage.cs](Assets/_Project/Scripts/ContactDamage.cs) — pass hit-source position
- [PlayerController.cs](Assets/_Project/Scripts/PlayerController.cs) — add knockback state + input gates
- `DamageFlash.cs` (new) — sprite-flash side-effect driven by Health events

---

## 1. Concepts and separation

Three concerns, three owners:

| Concern | Owner | Duration |
|---|---|---|
| Damage immunity (ignore subsequent hits) | `Health` | 1.0 s (~60 frames) |
| Sprite-flash blink | `DamageFlash` (subscribes to `Health`) | same as immunity |
| Control lock + pushback velocity | `PlayerController` | 0.35 s (~21 frames) |

The three run independently. Control returns to the player while i-frames are still active, letting them escape packed hit zones — the classic X-series feel.

---

## 2. Health changes

### 2.1 Extend damage API with source position

Current `Health.ApplyDamage(int)` — [Health.cs:46](Assets/_Project/Scripts/Health.cs#L46) — carries no spatial context. Extend:

```csharp
public event Action<int, Vector2> Damaged;   // (amount, sourcePosition)

public virtual void ApplyDamage(int amount, Vector2 sourcePosition)
{
    // existing body, but:
    Damaged?.Invoke(appliedDamage, sourcePosition);
    ...
}

// Keep a parameterless overload for damage sources that don't care:
public void ApplyDamage(int amount) => ApplyDamage(amount, transform.position);
```

The overload means existing callers (if any) keep compiling; new callers pass real source position. `ContactDamage` moves to the explicit form (§4).

### 2.2 Invulnerability state

Add to `Health`:

```csharp
[SerializeField] float invulnerabilityDuration = 1f;   // 60 frames @ 60 Hz

float invulnerableUntil;
public bool IsInvulnerable => Time.time < invulnerableUntil;
public float InvulnerabilityDuration => invulnerabilityDuration;

public event Action<bool> InvulnerabilityChanged;   // true when entering, false when leaving
```

`ApplyDamage` gates on it:

```csharp
public virtual void ApplyDamage(int amount, Vector2 sourcePosition)
{
    EnsureInitialized();

    if (IsInvulnerable || amount <= 0 || currentHealth <= 0) return;

    // ... existing damage math ...

    Damaged?.Invoke(appliedDamage, sourcePosition);
    HealthChanged?.Invoke(currentHealth, maxHealth);

    if (currentHealth == 0) { HandleDepleted(); return; }

    // Start i-frames. Skip if the hit was depleting — dead entities don't need immunity.
    invulnerableUntil = Time.time + invulnerabilityDuration;
    InvulnerabilityChanged?.Invoke(true);
}
```

An `Update` tick on `Health` raises `InvulnerabilityChanged(false)` exactly once when `Time.time` crosses `invulnerableUntil`. This lets `DamageFlash` cleanly un-hide the sprite without polling.

### 2.3 Non-goals for Health in this spec

- **No `Kill()` method yet.** Spike/pit instant-death (§7) will call `ApplyDamage(maxHealth, ...)` during this phase; a dedicated `Kill()` that bypasses i-frames can be added when needed.
- **Health does not touch SpriteRenderer.** Sprite flash is a separate component so enemies/bosses can opt in (or not) without Health growing dependencies.

---

## 3. PlayerController knockback

### 3.1 New state

Added under the existing `[Header]` blocks in [PlayerController.cs:19-48](Assets/_Project/Scripts/PlayerController.cs#L19-L48):

```csharp
[Header("Knockback")]
[SerializeField] float knockbackSpeedX = 5f;
[SerializeField] float knockbackSpeedY = 6f;
[SerializeField] float knockbackDuration = 0.35f;

float knockbackTimer;
bool IsKnockedBack => knockbackTimer > 0f;
```

### 3.2 Public entry point

```csharp
public void ApplyKnockback(Vector2 sourcePosition)
{
    // Direction: away from source. Wall-slide exception below.
    int dir;
    if (WallSliding)
        dir = -facing;                                    // peel off wall
    else
        dir = rb.position.x >= sourcePosition.x ? 1 : -1; // away from source X

    velocity = new Vector2(dir * knockbackSpeedX, knockbackSpeedY);
    knockbackTimer = knockbackDuration;

    // Cancel conflicting states
    dashTimer = 0f;
    wallJumpLockTimer = 0f;
    jumpBufferTimer = 0f;

    // Reset any in-progress charge (Q2 answer)
    if (isCharging)
    {
        isCharging = false;
        chargeTimer = 0f;
        if (spriteRenderer) spriteRenderer.color = baseSpriteColor;
    }

    // Update facing so the recovery pose points back at the hit source
    facing = -dir;
}
```

`ApplyKnockback` does **not** damage — `Health.ApplyDamage` is called independently by the damage source. This keeps the "damage happened" and "controls jolted" concerns orthogonal (important for sources that should stun without damaging, or damage without knockback, later).

### 3.3 Input gates in `FixedUpdate`

Current dash/input branch — [PlayerController.cs:251-255](Assets/_Project/Scripts/PlayerController.cs#L251-L255):

```csharp
if (IsDashing)
    velocity = new Vector2(dashDirection * dashSpeed, 0f);
else if (wallJumpLockTimer <= 0f)
    ApplyHorizontalInput();
```

Becomes:

```csharp
if (IsKnockedBack)
{
    // Velocity was set on hit; let it ride. Gravity still applies via ApplyGravity().
    // Move() resolves any wall contact and zeroes velocity.x on impact.
}
else if (IsDashing)
    velocity = new Vector2(dashDirection * dashSpeed, 0f);
else if (wallJumpLockTimer <= 0f)
    ApplyHorizontalInput();
```

### 3.4 Other gates

- **`Update`** — still updates `facing` from `moveInput.x`? **No.** Gate on `!IsKnockedBack`:
  ```csharp
  if (!IsKnockedBack)
  {
      if (moveInput.x >  0.1f) facing =  1;
      else if (moveInput.x < -0.1f) facing = -1;
  }
  ```
  Keeps the knockback-assigned facing stable for the stun duration.
- **`TryJump`** — early-return `false` when `IsKnockedBack`.
- **`TryStartDash`** — early-return when `IsKnockedBack`.
- **`OnAttackStarted` / `OnAttackCanceled`** — gate: if `IsKnockedBack`, ignore. Prevents starting a charge during stun.
- **Wall-slide clamp** — current `if (WallSliding) velocity.y = Mathf.Max(...)` — gate on `!IsKnockedBack`. Knockback-into-wall must not be clamped by wall-slide speed.
- **`jumpBufferTimer` consumption** — already naturally dies during stun because `TryJump` early-returns. Cleared explicitly in `ApplyKnockback` for belt-and-braces.

### 3.5 Timer tick

`knockbackTimer -= Time.deltaTime` in `Update`, alongside the other timers at [PlayerController.cs:216-220](Assets/_Project/Scripts/PlayerController.cs#L216-L220).

### 3.6 Wiring to Health

In `Awake` (or `Start`), after grabbing other refs:

```csharp
var health = GetComponent<Health>();
if (health) health.Damaged += OnHealthDamaged;
```

Symmetric unsubscribe in `OnDisable` (or `OnDestroy`).

```csharp
void OnHealthDamaged(int amount, Vector2 sourcePosition)
{
    ApplyKnockback(sourcePosition);
}
```

Note: `Health` already gates on `IsInvulnerable` before firing `Damaged`, so the player cannot be re-knocked during the 1 s immunity window. No extra check needed here.

---

## 4. ContactDamage update

[ContactDamage.cs:31](Assets/_Project/Scripts/ContactDamage.cs#L31) currently calls `health.ApplyDamage(damageAmount)`. Change to pass source position:

```csharp
health.ApplyDamage(damageAmount, transform.position);
```

`transform.position` is the enemy's position — the X component drives the knockback direction. For enemies with wide colliders, the collider center is a better proxy, but the transform is typically centered on the sprite so this is fine.

No other ContactDamage changes.

---

## 5. DamageFlash component (new)

New file: `Assets/_Project/Scripts/DamageFlash.cs`.

```csharp
using UnityEngine;

[RequireComponent(typeof(Health))]
public class DamageFlash : MonoBehaviour
{
    [SerializeField] SpriteRenderer target;
    [SerializeField] float period = 0.08f;

    Health health;
    bool flashing;
    float flashStart;

    void Awake()
    {
        health = GetComponent<Health>();
    }

    void OnEnable()
    {
        health.InvulnerabilityChanged += OnInvulnerabilityChanged;
    }

    void OnDisable()
    {
        health.InvulnerabilityChanged -= OnInvulnerabilityChanged;
        if (target) target.enabled = true;   // fail-open: always visible when disabled
    }

    void OnInvulnerabilityChanged(bool on)
    {
        flashing = on;
        flashStart = Time.time;
        if (!flashing && target) target.enabled = true;
    }

    void Update()
    {
        if (!flashing || !target) return;
        bool phase = Mathf.FloorToInt((Time.time - flashStart) / period) % 2 == 0;
        target.enabled = phase;
    }
}
```

- `target` serialized so the player prefab can wire it to the child `Visual`'s `SpriteRenderer` (root GameObject doesn't have one — see [CLAUDE.md](CLAUDE.md) §"Visual child / sprite flip").
- Period 0.08 s matches the existing charge-flash cadence ([PlayerController.cs:60](Assets/_Project/Scripts/PlayerController.cs#L60)).
- Toggles `SpriteRenderer.enabled` rather than color. Leaves the charge-flash color system untouched so charge + damage i-frames can coexist without fighting over the same channel.
- `OnDisable` restores visibility so a disabled component never leaves the sprite hidden.

---

## 6. Ladder interaction

Covered in [SPEC.md §3.7](SPEC.md). Summary: when `onLadder`, `OnHealthDamaged` should **not** apply a physical knockback — instead, drop the player off the ladder with `velocity = Vector2.zero` and let gravity take over. I-frames still apply (via `Health`) as normal.

Implementation is a simple branch in `OnHealthDamaged`:

```csharp
void OnHealthDamaged(int amount, Vector2 sourcePosition)
{
    if (onLadder) { ExitLadder(fall: true); return; }
    ApplyKnockback(sourcePosition);
}
```

This section is redundant with SPEC.md but cross-referenced so the knockback spec is self-contained.

---

## 7. Hazards (spikes + pits)

**Out of scope for implementation** in this spec — tracked in [README.md §6](README.md) as "Environmental hazards." Spec'd here only to the extent that knockback must coexist with them:

- **Spikes**: a future `SpikeHazard` component uses `Health.ApplyDamage(health.MaxHealth, transform.position)` on contact. The depletion path fires before i-frames engage, so spikes always kill.
- **Bottomless pits**: a trigger below the stage calls `health.ApplyDamage(health.MaxHealth, player.transform.position)` (source position = self, direction doesn't matter since player is dying anyway).
- **Knockback into a pit**: valid. The swept-cast `Move` carries the player off the ledge; the pit trigger fires and kills. No special case (Q2 answer: knockback is physical).

No code in this spec touches hazards. Ship the knockback system first; hazards slot in later.

---

## 8. Cross-cutting details

### 8.1 Interactions matrix

| State when hit | Result |
|---|---|
| Running on ground | Knockback arc backward from source; lands and recovers. |
| Mid-jump (ascending) | Knockback velocity **replaces** current velocity. Arc resumes from the hit point. |
| Falling | Same — velocity replaced; i-frames + stun tick regardless of airborne-ness. |
| Dashing | Dash canceled (`dashTimer = 0`), knockback takes over. |
| Wall-sliding | Direction = `-facing` (away from wall), vertical pop unchanged. Wall-slide suppressed during stun. |
| On ladder | Detach, `velocity = Vector2.zero`, gravity applies, i-frames tick. No pushback. |
| Mid-charge | Charge reset, color restored, stun begins. |
| Already invulnerable | No-op: Health gates `ApplyDamage` on `!IsInvulnerable`. |
| Ground-probing into a slope (edge of a ledge) | Same rules. Arc over edge is expected and will drop the player into pits. |

### 8.2 Knockback arc sanity check

With `knockbackSpeedY = 6`, `gravity = 40` — time to apex ≈ 0.15 s, apex height ≈ 0.45 units. Horizontal at 5 units/s over 0.35 s stun = 1.75 units. Recovery likely happens in the air (stun duration < total airtime), which is fine — control returns mid-fall, not only on landing.

### 8.3 Tuning home

All tunables are `[SerializeField]` on their respective components (Q-answers pattern match):

- Immunity: `invulnerabilityDuration` on `Health`.
- Knockback: `knockbackSpeedX`, `knockbackSpeedY`, `knockbackDuration` on `PlayerController`.
- Flash cadence: `period` on `DamageFlash`.

No ScriptableObject yet; revisit when X/Zero need divergent values.

---

## 9. Testing plan

New EditMode tests in `Assets/_Project/Tests/Editor/` (new folder):

- `HealthTests.cs`
  - **I-frame blocks damage.** `ApplyDamage` once → `IsInvulnerable == true` → second `ApplyDamage` does not decrement HP.
  - **I-frames expire.** After `invulnerabilityDuration`, `IsInvulnerable == false` and next `ApplyDamage` works.
  - **InvulnerabilityChanged fires exactly twice per hit** — true, then false.
  - **Lethal damage skips i-frame set** (HP reaches 0 → no lingering immunity — behavior to confirm in implementation).
  - **ApplyDamage overload routes source position** — the `Damaged` event carries the expected Vector2.
- `PlayerControllerTests.cs` (extended from the SPEC.md list)
  - **Knockback sets velocity** per `(knockbackSpeedX, knockbackSpeedY)` direction rules (hit from right = pushed left; wall-slide = -facing).
  - **IsKnockedBack gates `ApplyHorizontalInput`** — input ignored for the stun duration.
  - **Dash canceled on hit** — `dashTimer == 0` after `ApplyKnockback`.
  - **Charge canceled on hit** — `isCharging == false`, `chargeTimer == 0`, sprite color restored.
  - **`TryJump` / `TryStartDash` no-op during stun.**
  - **`knockbackTimer` decrements to 0** after duration, and normal input resumes.

PlayMode tests deferred (Q4 answer). Manual QA using the existing test enemy ([Enemy.cs](Assets/_Project/Scripts/Enemy.cs)).

---

## 10. Implementation order

1. Extend `Health.ApplyDamage` with source-position overload; keep parameterless overload forwarding to it. Update `Damaged` event signature.
2. Add `invulnerabilityDuration` + `IsInvulnerable` + `InvulnerabilityChanged` event to `Health`; add expiry tick in `Update`.
3. Update `ContactDamage` to pass source position.
4. Add `[Header("Knockback")]` block + `knockbackTimer` + `ApplyKnockback` + input gates to `PlayerController`.
5. Wire `Health.Damaged` → `PlayerController.OnHealthDamaged` in `Awake` / `OnDisable`.
6. Add `DamageFlash` component; attach on player prefab with `target` = `Visual`'s `SpriteRenderer`.
7. EditMode tests.
8. Manual QA: run into test enemy, confirm push + blink + ~1 s immunity + ~0.35 s stun. Run into pit after being pushed; confirm lethality.

Check off `README.md` §1 "Damage knockback / invincibility frames" once playable end-to-end.

---

## 11. Relationship to SPEC.md

SPEC.md (coyote / dash-jump / ladder) depends on this system for exactly one behavior: §3.7 "Damage interaction" on ladders. Land this spec **first**; then SPEC.md §3.7 reduces to a single `if (onLadder) ExitLadder(fall: true); else ApplyKnockback(sourcePosition);` branch with no scaffolding still to build.

If the user ships this without the rest of SPEC.md, the player already gains proper damage handling — a net improvement over the current contact-damage-only state.
