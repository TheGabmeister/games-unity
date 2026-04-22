# SPEC — Phase 0: Foundation

High-level spec for the bare minimum plumbing needed before Phase 1 can start. No gameplay, no systems that a later phase doesn't yet require.

## Goal

A runnable Unity project where pressing Play loads a bootstrap scene, registers an input service, and lands in a blank test scene — with `Game.I.<Service>` safe to call from any `Awake`. Nothing else.

## What's in Phase 0

1. Bootstrap + Service Locator
2. Input System

That's it. Audio, UI framework, debug console, and scene loader are deferred to the phase that first needs them (see `DigimonWorld1.md` — Implementation Order).

## Naming Conventions

- **Namespace:** `DigimonWorld.<Subsystem>` (e.g. `DigimonWorld.Input`)
- **Interfaces:** `IPascalCase` (`IInputService`)
- **Services:** `PascalCaseService` — the MonoBehaviour implementation of an interface (`InputService`)
- **MonoBehaviours:** `PascalCase` describing the object
- **ScriptableObjects:** `PascalCaseDefinition` or `PascalCaseData`
- **Enums:** `PascalCase` singular (`InputActionMap`)
- **Scenes:** `PascalCase.unity`; zone scenes prefixed with `Zone_` (`Zone_TestRoom.unity`)
- **Asset files:** match the class name (`InputActions.inputactions`)
- **Folders:** `PascalCase`
- **Private fields:** `_camelCase`; serialized fields `[SerializeField] private ...`

## 1. Bootstrap + Service Locator

**Purpose:** Give every other system a single, predictable place to live and a simple way to find each other. No DI framework — keep it flat.

- A persistent `Bootstrap` scene holds a single `Bootstrapper` MonoBehaviour
- `Bootstrapper` has **Script Execution Order −1000** so its `Awake` runs before any other `Awake`, making `Game.I.<Service>` safe to call from `Awake` everywhere else
- `ServiceLocator` is a static registry: `Register<T>`, `Get<T>`. That's the whole API.
- `EditorBootstrapLoader` is a static class with `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`; if `Bootstrap` isn't already loaded, it additively loads it before any other scene's `Awake`. Lets you press Play from any zone scene during iteration.
- `Game.I` is a short-hand accessor (`public static class Game { public static ServiceLocator I => ...; }`) so call sites read `Game.I.Input.Move` rather than `ServiceLocator.Get<IInputService>().Move`.

**Done when:** Pressing Play from any scene lands in that scene with `Bootstrap` additively loaded and at least one service registered and reachable via `Game.I`.

**Scripts** (`DigimonWorld.Core`):
- `Bootstrapper` — registers services and loads the first zone via whatever loader exists (Phase 0: none; Phase 1+ uses the scene loader)
- `ServiceLocator` — static `Register<T>` / `Get<T>`
- `Game` — `Game.I` accessor
- `EditorBootstrapLoader` — `[RuntimeInitializeOnLoadMethod]` that auto-loads Bootstrap

## 2. Input System

**Purpose:** One place that owns "what the player pressed". Everything else listens.

- Unity's new Input System package
- Action maps: `Gameplay`, `UI` (add `Debug` in Phase 3 when the debug system lands)
- Core actions stubbed now: `Move`, `Look`, `Interact`, `Pause`
- Device support: keyboard + mouse, gamepad
- Rebinding UI deferred to Phase 6 — expose a hook only
- Exposed as a single service so other systems don't touch the raw API

**Done when:** A test script logs action events for all core actions on both keyboard and gamepad, via `Game.I.Input`.

**Scripts** (`DigimonWorld.Input`):
- `IInputService` — interface
- `InputService` — MonoBehaviour, wraps generated actions, registered with the service locator
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
      Core/               # Service locator / bootstrapper
      Input/
    Settings/
      Input/              # InputActions asset
```

Folders for `Audio/`, `UI/`, `Debug/`, and `Scenes/` (the loader) get added in the phases that introduce those systems. Don't pre-create empty folders.

## Service Access

- Registered services: `IInputService` (plus whatever later phases add)
- No dependency injection framework
- `Bootstrapper` runs at **Script Execution Order −1000** so it registers services before any other `Awake` — consumers can safely use `Game.I.<Service>` from `Awake`
- `EditorBootstrapLoader` auto-loads the `Bootstrap` scene if you press Play from a zone scene

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
- [ ] `Bootstrapper` has Script Execution Order −1000
- [ ] Pressing Play from `Bootstrap` registers `IInputService` and stays in a blank test scene
- [ ] Pressing Play from `Zone_TestRoom` (Bootstrap not open) still works — `EditorBootstrapLoader` loads Bootstrap first
- [ ] `Game.I.Input` is non-null from any MonoBehaviour's `Awake`
- [ ] Input events fire from both keyboard and gamepad for all core actions
