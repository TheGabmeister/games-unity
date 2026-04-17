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
  - `HUD.cs`
  - `StageSession.cs`
  - `Description.cs`
  - `Behavior/`
    - `Lifetime.cs`
    - `MoveForward.cs`
    - `MoveVertical.cs`
  - `Damage/`
    - `Health.cs`
    - `HitBox.cs`
    - `HurtBox.cs`
    - `DamageFlash.cs`
  - `Enemy/`
    - `Enemy.cs`
    - `AutoShoot.cs`
    - `DestroyOnWallContact.cs`
    - `DropTrigger.cs`
    - `EnemyShoot.cs`
    - `PatrolWalk.cs`
    - `PlayerDetector.cs`
    - `SwoopAttack.cs`
  - `Player/`
    - `PlayerController.cs`
    - `WeaponInventory.cs`
    - `WeaponData.cs`
    - `DashSilhouetteTrail.cs`
  - `Services/`
    - `Bootstrapper.cs`
    - `Services.cs`
    - `MusicManager.cs`
    - `SfxManager.cs`
    - `ScreenFader.cs`
    - `SceneLoader/SceneLoader.cs`
  - `Editor/FileExtensions.cs`
- `Resources/`
  - `GameServices.prefab`
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
  - `Assets/Resources/GameServices.prefab` is the persistent services root loaded by `Bootstrapper`.
  - `Services` enforces that only one persistent services root survives at runtime.
  - Authored gameplay scenes such as `Gameplay.unity` should contain stage content and spawn markers like `PlayerStart`, not duplicate the persistent services root.
- Prefer narrow edits when refreshing instructions or specs.
  - Update `AGENTS.md` to reflect the current codebase, not aspirational architecture.
  - If a spec and the code disagree, verify the code before changing repo guidance.

## Codebase-specific notes

- The player currently uses a custom 2D kinematic controller in `Assets/_Project/Scripts/Player/PlayerController.cs`.
- X-Buster and weapon swapping behavior currently live in `Assets/_Project/Scripts/Player/WeaponInventory.cs`, which handles charge timing, muzzle spawning, weapon energy, and the small-shot on-screen cap.
- Projectiles are implemented through composition in `Assets/_Project/Scripts/Projectile.cs` plus movement helpers such as `Assets/_Project/Scripts/Behavior/MoveForward.cs` and `Assets/_Project/Scripts/Behavior/MoveVertical.cs`.
- Shot prefabs currently live under `Assets/_Project/Player/Shots/Prefabs/`.
- The project bootstraps persistent services from `Assets/Resources/GameServices.prefab` through `Assets/_Project/Scripts/Services/Bootstrapper.cs`.
- `Services` is the persistent root singleton for always-on managers and shared runtime services.
- `StageSession` currently spawns the player prefab at a `PlayerStart` tag if present, otherwise falls back to the active Scene view camera position in the editor, then instantiates the HUD prefab.
- `StageSession` also handles the current simple death flow: on player depletion it waits for `_deathReloadDelay` and reloads the active scene. Checkpoints are not implemented yet.
- When changing gameplay code, keep inspector-facing tuning values serialized unless there is a clear reason to hard-code them.
- Prefer extending the existing input actions asset instead of adding ad hoc polling or bespoke input glue.
- `PlayerController` depends on `WeaponInventory`; keep charge-shot, weapon-energy, and projectile-spawn changes in `WeaponInventory` unless the change is specifically about locomotion/input flow.
- Projectile movement uses transform-based motion. `MoveForward` advances along `transform.right`; orient projectile instances via rotation at spawn time rather than adding script-side facing state.
- `MoveVertical` exposes an enum-based up/down choice in the inspector for strictly vertical motion.
- HUD work is still lightweight and code-driven; prefer small, inspector-wired UI components over heavy framework additions unless the scope clearly calls for them.

## Validation

- Prefer testing in the Unity Editor for iteration.
- Use Test Runner for EditMode and PlayMode tests when present.
- If adding editor utilities that create assets, make paths and overwrite behavior explicit and safe.

## Safety

- Never rewrite or mass-edit Unity YAML scene/prefab files blindly.
- Do not delete or regenerate user-authored assets unless the task explicitly calls for it.
- If the repository has unrelated local changes, leave them alone.
