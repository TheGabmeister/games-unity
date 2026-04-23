# SPEC — Phase 0: Foundation

High-level spec for the bare minimum plumbing needed before Phase 1 can start. No gameplay, no systems that a later phase doesn't yet require.

## Goal

A runnable Unity project where pressing Play loads a bootstrap scene, brings up an input singleton, and lands in a blank test scene — with `InputService.Instance` safe to call from any `Awake`. Nothing else.

## What's in Phase 0

1. Bootstrap + Singletons
2. Input System

That's it. Audio, UI framework, debug console, and scene loader are deferred to the phase that first needs them (see `DigimonWorld1.md` — Implementation Order).

## Naming Conventions

- **Namespace:** `DigimonWorld.<Subsystem>` (e.g. `DigimonWorld.Input`)
- **Services:** `PascalCaseService` — a MonoBehaviour singleton (`InputService`); exposes `public static InputService Instance { get; private set; }`
- **MonoBehaviours:** `PascalCase` describing the object
- **ScriptableObjects:** `PascalCaseDefinition` or `PascalCaseData`
- **Enums:** `PascalCase` singular (`InputActionMap`)
- **Scenes:** `PascalCase.unity`; zone scenes prefixed with `Zone_` (`Zone_TestRoom.unity`)
- **Asset files:** match the class name (`InputActions.inputactions`)
- **Folders:** `PascalCase`
- **Private fields:** `_camelCase`; serialized fields `[SerializeField] private ...`

Note: services do **not** implement `I<Name>Service` interfaces. Call sites couple directly to the concrete singleton (`InputService.Instance.Move`). If a second implementation ever shows up, extract the interface then — not before.

## 1. Bootstrap + Singletons

**Purpose:** Give every other system a single, predictable place to live. No DI framework, no service locator — each service owns its own static `Instance`.

- A persistent `Bootstrap` scene holds a single `Bootstrapper` MonoBehaviour plus the service singletons (either on the same GameObject or on children — simplest is one GameObject per service).
- `Bootstrapper` has **Script Execution Order −1000** so its `Awake` runs before any other `Awake`. It calls `DontDestroyOnLoad` on the root so services survive scene loads.
- Each service sets `Instance = this` in its own `Awake` and asserts there is no prior instance. Services on the `Bootstrap` scene have unspecified `Awake` order relative to each other — if one service needs another during `Awake`, resolve it in `Start` instead, or set Script Execution Order explicitly.
- `EditorBootstrapLoader` is a static class with `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`; if `Bootstrap` isn't already loaded, it additively loads it before any other scene's `Awake`. Lets you press Play from any zone scene during iteration.
- Call sites read `InputService.Instance.Move` — direct, no indirection.

**Done when:** Pressing Play from any scene lands in that scene with `Bootstrap` additively loaded and `InputService.Instance` non-null.

**Scripts** (`DigimonWorld.Core`):
- `Bootstrapper` — applies `DontDestroyOnLoad`; does nothing else in Phase 0 (Phase 1+ will use it to kick the scene loader)
- `EditorBootstrapLoader` — `[RuntimeInitializeOnLoadMethod]` that auto-loads Bootstrap

## 2. Input System

**Purpose:** One place that owns "what the player pressed". Everything else reads from it.

- Unity's new Input System package
- Action maps: `Gameplay`, `UI` (add `Debug` in Phase 3 when the debug system lands)
- Core actions stubbed now: `Move`, `Look`, `Interact`, `Pause`
- Device support: keyboard + mouse, gamepad
- Rebinding UI deferred to Phase 6 — expose a hook only
- Exposed as a MonoBehaviour singleton on the `Bootstrap` scene; other systems read it via `InputService.Instance`

**Done when:** A test script logs action events for all core actions on both keyboard and gamepad, via `InputService.Instance`.

**Scripts** (`DigimonWorld.Input`):
- `InputService` — MonoBehaviour singleton, wraps generated actions, exposes `public static InputService Instance { get; private set; }`
- `InputActions.inputactions` — generated C# class `InputActions`
- `InputActionMap` — enum: `Gameplay`, `UI`
- `InputTestLogger` — debug-only MonoBehaviour that logs action events (can be deleted once Phase 1 player movement exists)

## Project Layout

```
Assets/
  _Project/
    Bootstrap/            # Bootstrap scene + bootstrapper
    Scenes/
      Bootstrap.unity
      Zone_TestRoom.unity
    Scripts/
      Core/               # Bootstrapper, EditorBootstrapLoader
      Input/
    Settings/
      Input/              # InputActions asset
```

Folders for `Audio/`, `UI/`, `Debug/`, and `Scenes/` (the loader) get added in the phases that introduce those systems. Don't pre-create empty folders.

## Service Access

- Services live on the `Bootstrap` scene and expose `public static <Service> Instance { get; private set; }` set in their own `Awake`.
- No service locator, no DI framework, no interfaces.
- `Bootstrapper` runs at **Script Execution Order −1000** and calls `DontDestroyOnLoad`. Service `Awake` order on the Bootstrap scene is unspecified; if service A needs service B during `Awake`, resolve it in `Start` or set A's Script Execution Order explicitly.
- `EditorBootstrapLoader` auto-loads the `Bootstrap` scene if you press Play from a zone scene.

## Out of Scope for Phase 0

Everything else. Specifically:

- Audio (Phase 4 — when combat SFX make silence hurt)
- UI framework, screen manager, pause screen, loading screen (Phase 2 — when dialogue + HUD arrive)
- Debug tools, in-game console, FPS overlay (Phase 3 — when care/stats need force-setting)
- Scene / Zone Loader (Phase 1 — when zone transitions need it)
- Settings menu, save/load, rebind UI (Phase 6)
- Any gameplay: no player, no camera, no partner, no Digimon

## Acceptance Checklist

- [ ] Project opens, `Bootstrap` scene is set as the first scene in Build Settings
- [ ] `Bootstrapper` has Script Execution Order −1000 and calls `DontDestroyOnLoad`
- [ ] Pressing Play from `Bootstrap` leaves `InputService.Instance` non-null and the scene active
- [ ] Pressing Play from `Zone_TestRoom` (Bootstrap not open) still works — `EditorBootstrapLoader` loads Bootstrap first
- [ ] `InputService.Instance` is non-null from any MonoBehaviour's `Awake`
- [ ] Input events fire from both keyboard and gamepad for all core actions
