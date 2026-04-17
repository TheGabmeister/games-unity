# SPEC_SERVICES

## Context

The project already has a persistent runtime root:

- `Assets/_Project/Scripts/Services/Bootstrapper.cs` loads `Resources/GameServices.prefab` before scene load.
- `Assets/_Project/Scripts/Services/Services.cs` keeps that root alive with `DontDestroyOnLoad`.
- `Assets/Resources/GameServices.prefab` currently contains long-lived scene-independent objects such as `MusicManager`, `SfxManager`, `ScreenFader`, and the `EventSystem`.

Right now, `Services` is only a singleton marker for the root object. It does not yet act as a true service locator. Code that wants a long-lived manager would still need to find or serialize that component manually.

This spec describes how to evolve `Services` into a proper service locator for runtime-wide systems.

## Goals

- Make persistent runtime services discoverable from a single stable API.
- Avoid repeated `Find*` calls and ad hoc scene wiring for always-on managers.
- Keep the implementation simple and Unity-friendly.
- Support service access by interface or service type, not just by raw child object traversal.
- Preserve the existing `GameServices.prefab` bootstrap model.

## Non-goals

- This is not a full dependency injection container.
- This should not perform constructor injection or automatic object graph creation.
- Scene-local objects such as `StageSession`, player instances, enemies, and HUD should not be registered as global services.
- This should not replace inspector references where a local serialized dependency is already the clearest option.

## Current state

### What exists now

- `Bootstrapper` ensures one `GameServices` root exists before scene load.
- `Services.Instance` points to that root and destroys duplicates.
- `GameServices.prefab` is already the natural home for always-on systems.

### What is missing

- No typed lookup API such as `Get<T>()` or `TryGet<T>()`.
- No explicit registration model.
- No clear distinction between global services and ordinary child MonoBehaviours.
- No consistent policy for whether callers should depend on concrete classes or service interfaces.

## Proposed direction

Turn `Services` into a lightweight runtime registry plus typed facade.

High-level shape:

- `Services` remains a `MonoBehaviour` on the persistent root.
- Child service components register themselves with `Services` during `Awake` or `OnEnable`.
- Callers resolve services through `Services.Instance.Get<T>()` or `Services.TryGet<T>(out var service)`.
- Services are registered against an interface or service contract where that adds value.

This keeps object lifetime Unity-native while giving the project a clean service access layer.

## Service locator API

### Core API on `Services`

Recommended public API:

- `bool Has<T>()`
- `T Get<T>() where T : class`
- `bool TryGet<T>(out T service) where T : class`
- `void Register<T>(T service) where T : class`
- `void Unregister<T>(T service) where T : class`

Behavior:

- `Get<T>()` should log or throw a clear error if the service is missing.
- `TryGet<T>()` should be the safe path for optional services.
- `Register<T>()` should reject null.
- Registering the same contract twice should either:
  - error loudly, or
  - replace only if explicitly allowed.

For this project, the recommended default is: error on duplicate registration for the same contract.

### Backing storage

Use an in-memory dictionary owned by `Services`.

Recommended shape:

- `Dictionary<Type, object> _services`

This is enough for the project's current scale and keeps the implementation readable.

## Registration model

### Preferred pattern

Each global service registers itself with `Services` in its own lifecycle method.

Recommended base pattern:

- `Services.Instance.Register<IMusicService>(this);`
- `Services.Instance.Unregister<IMusicService>(this);`

This keeps ownership local to each service and avoids hard-coding every child service inside `Services`.

### Optional helper base class

If several services follow the same pattern, add a small helper such as:

- `ServiceBehaviour<TContract> : MonoBehaviour`

Responsibilities:

- register with `Services` on `Awake` or `OnEnable`
- unregister on `OnDestroy` or `OnDisable`

This is optional. If the generic base starts to feel magical, keep registration explicit in each manager.

## Service contracts

Prefer interfaces for true services that other systems should depend on indirectly.

Recommended first contracts:

- `ISfxService`
- `IMusicService`
- `ISceneLoaderService`
- `IScreenFaderService`

### Why interfaces

- They reduce coupling between callers and specific MonoBehaviour implementations.
- They make later refactors easier.
- They allow test doubles in PlayMode or EditMode tests if needed.

### When concrete types are okay

If a system is still highly local or unstable, resolving by concrete type is acceptable temporarily. The locator should support both patterns during migration.

## Initial migration target

The first pass should cover only the truly global systems already implied by `GameServices.prefab`.

Recommended initial registrations:

- `MusicManager`
- `SfxManager`
- `ScreenFader`
- `SceneLoader` once it is actually placed on the services root or intentionally moved there

Do not register:

- `StageSession`
- `HUD`
- `PlayerController`
- enemy components
- stage-specific scene content

## Usage rules

Use the locator only for systems that are:

- persistent across scenes
- unique at runtime
- shared by many unrelated systems

Do not use the locator for:

- parent-child references available in the same prefab
- per-scene orchestration objects
- short-lived gameplay entities

Good examples:

- playing SFX from a projectile or UI script
- fading the screen during scene transitions
- requesting a scene switch from menu or game flow code

Bad examples:

- looking up the player from an enemy through the locator
- resolving stage objects that belong in authored scenes

## Bootstrapping and ordering

`Bootstrapper` should remain responsible only for ensuring the root exists.

`Services` should remain responsible for:

- singleton enforcement
- `DontDestroyOnLoad`
- storing the registry

Individual service components should remain responsible for self-registration.

This separation keeps startup predictable:

1. `Bootstrapper` instantiates `GameServices`
2. `Services` becomes the active root singleton
3. child services register themselves
4. gameplay scenes start using typed service lookup

## Failure policy

The locator should fail loudly when a required service is missing.

Recommended behavior:

- `Get<T>()` logs a descriptive error including the missing contract type.
- Duplicate registration logs an error and keeps the first instance.
- Unregistering a different instance than the one currently registered should be ignored or warned.

This project is still early enough that loud failures are better than silent fallbacks.

## Suggested implementation phases

### Phase 1 - Registry foundation

- Add typed registry storage to `Services`
- Add `Get<T>()`, `TryGet<T>()`, `Register<T>()`, and `Unregister<T>()`
- Keep current bootstrap behavior unchanged

### Phase 2 - Register existing global managers

- Register `SfxManager`
- Register `MusicManager`
- Register `ScreenFader`
- Register `SceneLoader` if it is promoted onto the persistent root

### Phase 3 - Migrate callers

- Replace ad hoc global lookups with `Services.Instance.Get<T>()` or `TryGet<T>()`
- Prefer interface contracts where the service is genuinely shared

### Phase 4 - Tighten conventions

- Document which classes qualify as global services
- Optionally add a shared registration helper base if repeated boilerplate becomes noisy

## Verification

- Booting into `Gameplay` creates exactly one `Services` root.
- Registered services can be retrieved from any loaded scene through the typed API.
- Duplicate `GameServices` roots still destroy the newer instance.
- Missing required services fail with clear errors.
- Service unregistration does not leave stale entries after destruction.
- No stage-local object is added to the global registry by mistake.
