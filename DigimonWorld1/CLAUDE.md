# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Gameplay-focused Unity recreation of Digimon World 1. Not a pixel-perfect remake — faithful mechanics with placeholder 3D models (no animations), placeholder textures, placeholder audio, single-player.

Two roadmap docs own the scope and sequencing; treat them as source of truth:

- [DigimonWorld1.md](DigimonWorld1.md) — 6-phase roadmap. Phase 0 = Foundation, Phase 1 = player in a world, etc.
- [SPEC_PHASE_00.md](SPEC_PHASE_00.md) — current phase's spec: naming conventions, service list, acceptance checklist.

The project is at the start of Phase 0 — most of what the specs describe does not yet exist in `Assets/`. Only scaffolding (folder layout, input actions asset, one editor utility) is checked in. Do not assume a system exists because a spec mentions it — check the tree.

## Unity version & running

**Unity 6000.3.12f1** (Unity 6), pinned via `ProjectSettings/ProjectVersion.txt`. Don't upgrade casually — package compatibility matters.

There is no CLI build pipeline. All work happens in the Unity Editor:

- **Open:** open this folder as a project in Unity Hub.
- **Run:** press Play. Once Phase 0 lands, `Bootstrap.unity` is the canonical first scene, but `EditorBootstrapLoader` is designed so Play-from-any-scene works.
- **Tests:** Unity Test Framework (`com.unity.test-framework` 1.6.0) via `Window → General → Test Runner`. No tests written yet, no CI configured.
- **Linting/formatting:** none. Follow naming conventions in `SPEC_PHASE_00.md`.

## Architecture (per the Phase 0 spec — not yet implemented)

The intended pattern, per `SPEC_PHASE_00.md`:

- `Bootstrap` scene holds a single `Bootstrapper` MonoBehaviour at **Script Execution Order −1000**, so its `Awake` runs before any other `Awake`.
- `Bootstrapper` registers services (starting with `IInputService`) into a static `ServiceLocator` (just `Register<T>` / `Get<T>` — no DI framework).
- Consumers access services via `Game.I.<Service>` (e.g. `Game.I.Input.Move`), safe from any `Awake`.
- `EditorBootstrapLoader` — `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` — additively loads `Bootstrap` if it isn't already loaded, so you can press Play from any zone scene during iteration.
- Scripts under `DigimonWorld.Core` (bootstrap, service locator) and `DigimonWorld.<Subsystem>` (e.g. `DigimonWorld.Input`).

When implementing Phase 0, follow the acceptance checklist at the bottom of `SPEC_PHASE_00.md`.

## Naming conventions (from SPEC_PHASE_00.md)

- **Namespace:** `DigimonWorld.<Subsystem>` (e.g. `DigimonWorld.Input`)
- **Interfaces:** `IPascalCase` (`IInputService`); services implement `I<Name>Service`
- **Services:** `PascalCaseService` — the MonoBehaviour implementation (`InputService`)
- **ScriptableObjects:** `PascalCaseDefinition` or `PascalCaseData`
- **Enums:** `PascalCase` singular (`InputActionMap`)
- **Scenes:** `PascalCase.unity`; zone scenes prefixed `Zone_` (`Zone_TestRoom.unity`)
- **Private fields:** `_camelCase`; serialized fields `[SerializeField] private ...`
- **Folders:** `PascalCase`

## Phased system introduction (the core rule)

Each system enters the plan at the phase where gameplay first actually needs it — not upfront. If infrastructure isn't required until Phase 3, it lands in Phase 3. Audio, UI framework, scene loader, debug tools are all deferred to their first real consumer. See the Implementation Order in `DigimonWorld1.md`.

Concretely: don't build a UI framework in Phase 0 because "we'll need it." Build it in Phase 2 when dialogue + HUD arrive. Don't add `AudioSource.Play` ad-hoc in Phase 3 to sneak audio in before Phase 4 — pull the audio item forward instead.

## Project layout (current, minimal)

```
Assets/
  _Project/
    Input/       # InputSystem_Actions.inputactions (pre-existing scaffold — Phase 0 will replace with DigimonWorld.Input's InputActions.inputactions)
    Scripts/
      Editor/    # FileExtensions.cs (editor utility — shows file extensions in Project panel)
    Settings/    # URP assets (PC_RPAsset, Mobile_RPAsset, renderers, volume profile)
  Resources/     # empty — Systems.prefab will live here once Phase 0 adds it (if used)
```

Folders for `Bootstrap/`, `Scenes/`, `Scripts/Core/`, `Scripts/Input/`, `Settings/Input/`, `Audio/`, `UI/`, `Debug/` get created by the phase that introduces them. Don't pre-create empty folders.

## Key packages

- **Input System** (`com.unity.inputsystem` 1.19.0) — new input system is the standard; do not use the legacy `Input` class.
- **URP** (`com.unity.render-pipelines.universal` 17.3.0) — both PC and Mobile render pipeline assets exist under `Assets/_Project/Settings/`.
- **AI Navigation** (`com.unity.ai.navigation` 2.0.11) — for partner follow / enemy AI in later phases.
- **PrimeTween** (`com.kyrylokuzyk.primetween` 1.3.8) — preferred tweening library; use it instead of coroutine-based tweens.
- **Eflatun.SceneReference** (git package, 5.0.0) — typed scene references for the Phase 1 scene/zone loader (avoid raw string scene names).
- **Timeline** (1.8.11) — reserved for the Phase 6 cutscene system.

## Editor-driven content: prefabs and scenes

This project generates **both prefabs and scenes** via editor scripts. The mandatory order is:

1. **Generate prefabs first.** Editor scripts create and save prefab assets (Digimon, items, NPCs, zone props, etc.) from ScriptableObject definitions or other data sources. Use `PrefabUtility.SaveAsPrefabAsset` and follow with `AssetDatabase.SaveAssets` + `AssetDatabase.Refresh` so the GUIDs are committed before step 2 reads them.
2. **Generate scenes second, referencing the prefabs created in step 1.** Editor scripts compose scenes by instantiating those prefabs (`PrefabUtility.InstantiatePrefab` — never `Instantiate`, which breaks the prefab link) and saving via `EditorSceneManager.SaveScene`. Reference prefabs by asset path or via a serialized `GameObject` field on a generator SO; do not hard-code GUIDs.

Non-negotiables for this workflow:

- **Never build scene content inline** (raw `GameObject` + `AddComponent` calls that aren't wrapping a prefab instantiation). If a scene needs an object, it needs a prefab first.
- **Separate the two passes.** A single editor menu item that "generates everything" must run the prefab pass to completion — including `AssetDatabase.SaveAssets` — before touching a scene. Mixing the two in one pass is how you get `fileID: 0` null-reference writes when Unity hasn't finished serializing the prefab before the scene save path runs.
- **Generators are idempotent.** Re-running the generator on an existing prefab/scene should update it in place, not duplicate it. Use `AssetDatabase.LoadAssetAtPath` to detect existing assets and overwrite deterministically.
- **Generator scripts live under `Assets/_Project/Scripts/Editor/Generators/`** so they're clearly distinguishable from runtime code and never ship in a build.
- **ScriptableObject authoring tools are fine too** — bulk-create `DigimonDefinition`, `TechniqueDefinition`, `ItemDefinition` assets from CSV or similar. These feed the prefab generators in step 1.

PlayMode test fixtures are still built in memory and torn down at teardown — never saved to disk, even though scene generation is otherwise allowed.

## Coding principles

- **KISS** — simplest thing that works. No clever patterns where a plain `if` does the job. If a class is under 50 lines and clear, don't split it.
- **YAGNI** — don't build for hypothetical needs. No interfaces with one implementation, no config knobs with one value, no abstraction layers "for later." (The one explicit exception is service interfaces — `IInputService` etc. — because the service locator design calls for them even at one implementation.)
- **DRY** — remove real duplication, not shape-similar code. Three copies of the same logic → extract. Two functions that happen to both take a `Vector3` → leave alone. Wrong abstraction costs more than repetition.

When in doubt, lean KISS over DRY. A bit of repetition is cheaper to read and change than the wrong shared helper.

## Git

- Remote: `TheGabmeister/games-unity` (this project lives in a sub-directory of that repo).
- Default branch: `main`.
- Unity meta files are tracked — don't delete them. `.gitignore` follows the standard Unity template.

## Things not in the repo yet

- No CI / GitHub Actions.
- No `.editorconfig`, no formatter config.
- No asmdefs — everything compiles into the default assemblies. Add asmdefs per-subsystem when Phase 0 implementation introduces `DigimonWorld.Core` / `DigimonWorld.Input`.
- No unit tests.
- No `Bootstrap.unity`, `Bootstrapper.cs`, `ServiceLocator.cs`, or `Game.cs` yet — these are the Phase 0 deliverable.
