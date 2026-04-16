# SPEC_GAMEMODE - Gameplay Session, Player Ownership, HUD Ownership

Status: **Design note, unimplemented.**

This document captures the current design direction for a Unity-side equivalent of an Unreal `GameMode` + `HUD` setup.

It does **not** commit us to full framework code yet. The goal is to lock down ownership and data flow before we add:

- Life bar HUD
- Weapon-energy HUD
- Boss HUD
- Death + respawn at checkpoint
- Future player swapping / campaign boot differences (X vs Zero)

This spec is a companion to:

- [README.md](README.md) section 5 ("Health, damage, pickups")
- [README.md](README.md) section 1 ("Death + respawn at checkpoint")
- [SPEC2.md](SPEC2.md) for damage / i-frames

---

## 1. Core idea

The project should have a **gameplay-level coordinator** that plays the role Unreal's `GameMode` often plays:

- identify or spawn the current player
- track the active checkpoint / spawn point
- own session-level references needed by gameplay UI
- bind the HUD to the active player
- handle death -> respawn flow

Recommended Unity name:

- `GameplaySession`

That name fits Unity better than forcing the Unreal term directly, while keeping the same mental model.

---

## 2. Ownership model

### 2.1 Recommended owners

| Concern | Owner |
|---|---|
| Current HP / max HP | `Health` |
| Weapon energy / ammo state | weapon-specific runtime component |
| Enemy death response | `Enemy` |
| Player movement / actions | `PlayerController` |
| Current player instance | `GameplaySession` or a `PlayerRegistry` it owns |
| HUD lifetime + binding | `GameplaySession` |
| HUD rendering | `GameplayHudController` |
| Checkpoint / respawn selection | `GameplaySession` |

### 2.2 What should *not* own the HUD

The HUD should **not** be owned by:

- `PlayerController`
- `Health`
- weapon components
- enemy components

Those systems should expose gameplay state, not UI rules.

---

## 3. Unreal analogy

Closest mapping:

| Unreal concept | Unity concept in this project |
|---|---|
| `GameMode` | `GameplaySession` |
| `Pawn` | Player prefab / player GameObject |
| `HUD` | `GameplayHudController` on a Canvas |
| `PlayerController` | partly Unity `PlayerInput`, partly our existing `PlayerController` |

Important note:

Our existing [PlayerController.cs](Assets/_Project/Scripts/PlayerController.cs) is closer to "character brain + movement + input" on the player object than Unreal's split between `PlayerController` and `Pawn`.

So we should copy the **ownership pattern**, not Unreal's class naming 1:1.

---

## 4. Scene ownership

Project direction from `AGENTS.md`:

- `Init.unity` is the bootstrap / persistent systems scene
- `Gameplay.unity` and future stage scenes load additively

Recommended ownership split:

### 4.1 Near-term

For the current project state, `GameplaySession` can live in `Gameplay.unity`.

That is the simplest version because:

- there is only one gameplay scene right now
- the player and checkpoint logic are stage-local
- the first HUD pass only needs the active player

### 4.2 Longer-term

If stage transitions start preserving run-level state, add a higher-level persistent coordinator in `Init.unity`, such as:

- `RunSession`
- `GameBootstrap`

Then:

- persistent boot object in `Init.unity` owns campaign/run state
- per-stage `GameplaySession` owns active player, checkpoint, boss bindings, and HUD for that stage

That gives us a clean two-level model instead of trying to make one object own everything forever.

---

## 5. HUD ownership

### 5.1 Recommended shape

The HUD should be a dedicated presentation system:

- `GameplaySession` owns HUD lifetime and tells it who the current player is
- `GameplayHudController` owns screen widgets and subscriptions
- `GameplayHudController` binds to the current player's `Health` and other runtime components

Recommended HUD pieces:

- `GameplayHudController`
- `PlayerHudPanel`
- `BossHudPanel`

### 5.2 Binding rule

The HUD should not hold a permanent serialized reference straight to one player object in the scene.

Instead:

- `GameplaySession` knows the current player
- HUD asks `GameplaySession` for the current player, or listens for a player-registration event
- on respawn, HUD rebinds to the new player automatically

This keeps the HUD resilient to:

- death / respawn
- replacing the player prefab
- eventually switching between X and Zero
- stage reloads or additive scene transitions

---

## 6. How the HUD should reference the player

Recommended pattern:

### Option A - direct session ownership

`GameplaySession` exposes:

```csharp
public PlayerController CurrentPlayer { get; private set; }
public event Action<PlayerController> PlayerChanged;
```

HUD subscribes to `PlayerChanged` and binds to the new player.

### Option B - small registry owned by the session

Introduce:

- `PlayerRegistry`

Then:

- `GameplaySession` owns a `PlayerRegistry`
- player spawn / respawn code registers the active player
- HUD listens to `PlayerRegistry`

This is slightly more modular and makes testing easier.

### Recommendation

Use **Option A first**, unless we immediately need multiple systems to listen for player swap independently.

Use **Option B** once checkpoint, camera, HUD, boss intro flow, and pause systems all want the same source of truth.

---

## 7. Proposed runtime flow

Recommended flow for gameplay startup:

1. `GameplaySession` wakes up.
2. It finds or spawns the player at the current start point.
3. It stores that player as the current player.
4. It creates or locates `GameplayHudController`.
5. It tells the HUD to bind to the current player.
6. HUD subscribes to `Health`, weapon energy, and later boss state.

Recommended flow for death / respawn:

1. Player `Health` depletes.
2. `GameplaySession` receives the death signal, directly or through a player-side component.
3. `GameplaySession` runs death / respawn flow.
4. Old player instance is cleaned up or disabled.
5. New player instance is spawned at the active checkpoint.
6. `GameplaySession` updates `CurrentPlayer`.
7. HUD rebinds automatically.

---

## 8. Checkpoint ownership

Checkpoint state should belong to `GameplaySession`, not to the player.

Reason:

- checkpoints outlive a single player instance
- respawn is a session rule, not a movement rule
- future cutscenes / teleports may move the respawn point without involving player code directly

Recommended shape:

```csharp
Transform currentSpawnPoint;
```

Later this can evolve into a dedicated checkpoint type or data struct if needed.

---

## 9. Boss HUD ownership

Boss HUD should still be owned by the same gameplay HUD layer, but the data source should be boss-side.

Recommended rule:

- boss owns boss health
- `GameplaySession` decides which boss is currently relevant to the HUD
- `BossHudPanel` binds to that boss's `Health`

This lets us support:

- intro sequence before boss activation
- fill-up intro animation
- hiding the boss HUD outside boss rooms

without teaching generic HUD code how bosses work internally.

---

## 10. What `GameplaySession` should and should not do

### 10.1 Good responsibilities

- resolve current spawn point
- spawn / register current player
- keep track of session-level references
- connect HUD to runtime actors
- respond to player death
- trigger respawn
- optionally coordinate boss registration and pause flow later

### 10.2 Bad responsibilities

`GameplaySession` should not become a god-object that directly owns:

- player movement logic
- damage calculations
- weapon-fire behavior
- enemy AI
- animation state machines
- raw HUD drawing details

It should coordinate systems, not absorb them.

---

## 11. First-pass implementation target

When we start implementing this, keep the first pass intentionally small.

### 11.1 Minimal version

Add:

- `GameplaySession`
- `GameplayHudController`

First-pass responsibilities:

- locate or spawn player
- expose `CurrentPlayer`
- bind HUD to player `Health`
- update life bar HUD

### 11.2 Defer for later

Do **not** include these in the first pass unless they become necessary:

- persistent `RunSession` in `Init.unity`
- generalized service locator
- multi-player support
- campaign selection flow
- full weapon-switch HUD
- boss HUD orchestration
- save/load integration

---

## 12. Suggested component sketch

Conceptually:

```csharp
public class GameplaySession : MonoBehaviour
{
    public PlayerController CurrentPlayer { get; private set; }
    public event Action<PlayerController> PlayerChanged;

    public void RegisterPlayer(PlayerController player) { ... }
    public void RespawnPlayer() { ... }
}
```

```csharp
public class GameplayHudController : MonoBehaviour
{
    [SerializeField] GameplaySession session;

    void OnEnable() { ... }
    void BindPlayer(PlayerController player) { ... }
}
```

```csharp
public class PlayerHudPanel : MonoBehaviour
{
    public void Bind(Health health) { ... }
}
```

This is enough structure for a good first implementation without overbuilding.

---

## 13. Open questions

These do not block the first HUD pass, but they will matter soon:

- Should the initial player be spawned by code, or should `GameplaySession` adopt a hand-placed player already in `Gameplay.unity`?
- Should the HUD Canvas live in `Gameplay.unity` now, or in `Init.unity` once stage flow expands?
- Should `GameplaySession` own checkpoint transforms directly, or should we add a `Checkpoint` component early?
- When Zero lands, do we want one shared HUD controller with mode-specific panels, or separate X / Zero HUD panels?

Current recommendation:

- adopt-or-spawn is acceptable for first pass
- keep HUD in `Gameplay.unity` for now
- keep checkpoints simple at first
- use one shared HUD controller with swappable panel bindings

---

## 14. Decision summary

Current recommended decisions:

- Use `GameplaySession` as the Unity-side GameMode analogue.
- Keep `GameplaySession` in `Gameplay.unity` for the first implementation.
- Let `GameplaySession` own the current player reference and respawn flow.
- Let `GameplayHudController` own HUD rendering and subscriptions.
- Bind HUD to player state through the session, not by hard-linking HUD to one scene player object.
- Keep gameplay state in gameplay components (`Health`, weapon systems, boss systems), not in the HUD.

This should be enough structure to implement:

- life bar HUD first
- weapon energy HUD next
- death + respawn after that

without painting ourselves into a corner.
