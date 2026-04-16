# AGENTS.md

Guidance for coding agents working in this repository.

## Project

This is a Unity 6 recreation of **Mega Man X4** focused on gameplay fidelity first. The project is still early-stage, so expect a mix of hand-authored Unity content and lightweight placeholder visuals.

Work only inside `MegamanX4/`. Do not traverse into sibling projects in the parent `games-unity/` folder.

## Tooling

- Unity version: `6000.3.12f1`
- Render pipeline: URP 2D (`com.unity.render-pipelines.universal` `17.3.0`)
- Input: Unity Input System (`com.unity.inputsystem` `1.19.0`)
- Tweening: PrimeTween (`com.kyrylokuzyk.primetween` `1.3.8`)
- Tests: Unity Test Framework (`com.unity.test-framework` `1.6.0`)
- Solution: `MegamanX4.slnx`

## Current layout

Key project content currently lives under `Assets/_Project/`.

- `Input/`
  - `InputSystem_Actions.inputactions`
- `Scenes/`
  - `Gameplay.unity`
- `Scripts/`
  - `Bootstrapper.cs`
  - `PlayerController.cs`
  - `BusterShot.cs`
  - `SystemsRoot.cs`
  - `Editor/FileExtensions.cs`
  - `Editor/BusterShotPrefabGenerator.cs`
- `Resources/`
  - `Systems.prefab`
- `Player/`
  - `MegamanX.prefab`
  - `Character/` sprite assets
  - `Shots/` shot sprites and prefabs
- `Settings/`
  - URP and renderer assets

Ignore generated Unity folders like `Library/`, `Logs/`, and `Temp/` unless a task explicitly requires them.

## Working agreements

- Do not script scene composition to disk.
  - Scenes, debug levels, and fixtures should be authored by hand in the Unity Editor.
  - Editor tooling that writes prefabs or ScriptableObjects is fine.
  - Ephemeral PlayMode test scenes built in memory are fine if they are not saved.
- Prefer a short plan before implementing non-trivial systems.
  - If a project-wide plan is needed, put it in `SPEC.md` at the repo root.
- Do not introduce real asset dependencies until they are explicitly added.
  - Use primitives, placeholder sprites, and stubbed systems where needed.
- Prefer ScriptableObjects for tunable game data.
  - Examples: weapon stats, enemy data, stage metadata, palettes.
- Preserve the code-driven bootstrap direction.
  - `Bootstrapper` runs via `RuntimeInitializeOnLoadMethod` before scene load.
  - `Assets/Resources/Systems.prefab` is the persistent systems root and is guarded by `SystemsRoot`.
  - Authored gameplay scenes such as `Gameplay.unity` should contain stage content and spawn markers like `PlayerStart`, not duplicate persistent systems roots.

## Codebase-specific notes

- The player currently uses a custom 2D kinematic controller in `Assets/_Project/Scripts/PlayerController.cs`.
- Buster shots are implemented in `Assets/_Project/Scripts/BusterShot.cs` and instantiated from prefabs.
- Shot prefabs currently live under `Assets/_Project/Player/Shots/Prefabs/`.
- The project now bootstraps persistent systems from `Assets/Resources/Systems.prefab` through `Assets/_Project/Scripts/Bootstrapper.cs`.
- `SystemsRoot` enforces that only one persistent systems root survives at runtime.
- Stage entry should resolve a `PlayerStart` marker and spawn the runtime player prefab by code rather than relying on a hand-placed canonical player instance.
- When changing gameplay code, keep inspector-facing tuning values serialized unless there is a clear reason to hard-code them.
- Prefer extending the existing input actions asset instead of adding ad hoc polling or bespoke input glue.

## Validation

- Prefer testing in the Unity Editor for iteration.
- Use Test Runner for EditMode and PlayMode tests when present.
- If adding editor utilities that create assets, make paths and overwrite behavior explicit and safe.

## Safety

- Never rewrite or mass-edit Unity YAML scene/prefab files blindly.
- Do not delete or regenerate user-authored assets unless the task explicitly calls for it.
- If the repository has unrelated local changes, leave them alone.
