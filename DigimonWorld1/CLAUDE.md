# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Gameplay-focused Unity recreation of Digimon World 1. Not a pixel-perfect remake — faithful mechanics with placeholder 3D models (no animations), placeholder textures, placeholder audio, single-player. The phased roadmap lives in [DigimonWorld1.md](DigimonWorld1.md) (6 phases, Phase 0 = Foundation). Treat that doc as the source of truth for scope and sequencing.

Status: early Phase 0. The bootstrap pattern is wired up (scene + singleton pattern + Play-from-any-scene loader) and the first service prefab (`AudioSystem`) exists. Everything past that is still ahead.

## Unity version & running

**Unity 6000.3.12f1** (Unity 6), pinned via `ProjectSettings/ProjectVersion.txt`. Don't upgrade casually — package compatibility matters.

No CLI build pipeline. All work happens in the Unity Editor:

- **Open:** open this folder as a project in Unity Hub.
- **Run:** press Play from any scene. `Bootstrapper` has a `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` that additively loads `_Bootstrap.unity` before any scene's `Awake` — so pressing Play from a zone scene still gets you a bootstrapped world.
- **Tests:** Unity Test Framework (`com.unity.test-framework` 1.6.0) via `Window → General → Test Runner`. No tests written yet, no CI configured.
- **Linting/formatting:** none configured.

## Architecture: bootstrap + singletons (implemented)

The pattern is in place — match it when adding new services.

- [_Bootstrap.unity](Assets/_Project/Scenes/_Bootstrap.unity) is the persistent scene. It's additively loaded before any other scene's `Awake` by [Bootstrapper.cs](Assets/_Project/Scripts/Bootstrapper.cs), which is a **plain class** (not a MonoBehaviour) carrying only a `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` + `[DefaultExecutionOrder(-1000)]`. Its single job is to call `SceneManager.LoadScene("_Bootstrap", Additive)` if the scene isn't already loaded.
- Each service is a **MonoBehaviour singleton** inheriting from [PersistentSingleton.cs](Assets/_Project/Scripts/PersistentSingleton.cs), which handles `Instance` assignment + `DontDestroyOnLoad` + duplicate-destruction. See [AudioSystem.cs](Assets/_Project/Scripts/AudioSystem.cs) for the pattern. Call sites read `AudioSystem.Instance.Foo` directly.
- **No service locator, no DI framework, no `I<Name>Service` interfaces.** Extract an interface only when a second implementation shows up.
- Each service lives as its own prefab under [Assets/_Project/Prefabs/](Assets/_Project/Prefabs/). `_Bootstrap.unity` is populated by instantiating those prefabs — either via the scene generator or hand-edited and regenerated.
- Service `Awake` order is unspecified relative to each other. If service A needs service B during `Awake`, resolve it in `Start` or set A's Script Execution Order explicitly.

## Editor workflow: prefab generators + scene generator

**Everything in `_Bootstrap.unity` gets there via prefabs instantiated from an editor script — never inline `new GameObject` + `AddComponent` directly in a scene.** The workflow is a strict two-pass:

1. **Prefab pass.** Menu items under `Tools → DigimonWorld → Prefabs → *` each generate one prefab under `Assets/_Project/Prefabs/`. See [GenerateBootstrapPrefabs.cs](Assets/_Project/Scripts/Editor/Generators/GenerateBootstrapPrefabs.cs). Each prefab gets its own menu item. Generators call `PrefabUtility.SaveAsPrefabAsset` followed by `AssetDatabase.SaveAssets` + `AssetDatabase.Refresh` so GUIDs are committed before the scene pass reads them.
2. **Scene pass.** `Tools → DigimonWorld → Generate Bootstrap Scene` (see [GenerateBootstrapScene.cs](Assets/_Project/Scripts/Editor/Generators/GenerateBootstrapScene.cs)) loads the prefabs by path, creates a new empty scene, `PrefabUtility.InstantiatePrefab`s them in (never raw `Instantiate` — breaks the prefab link), saves, and idempotently adds the scene to `EditorBuildSettings.scenes` at index 0.

Non-negotiables for this workflow:

- **One menu item per prefab** — add a new service by adding a const path, a `[MenuItem]` method, and one call to the shared `SavePrefab("Name", Path, go => go.AddComponent<T>())` helper.
- **Never mix prefab creation and scene composition in one pass.** The prefab pass must run `AssetDatabase.SaveAssets` before any scene generator touches the prefab — otherwise you get `fileID: 0` null-reference writes when Unity hasn't finished serializing the prefab before the scene save path runs.
- **Generators are idempotent** — re-running overwrites in place. Use `AssetDatabase.LoadAssetAtPath` to detect existing assets.
- **Generator scripts live under [Assets/_Project/Scripts/Editor/Generators/](Assets/_Project/Scripts/Editor/Generators/)** so they never ship in a build.
- **Scene generator registers scenes with Build Settings** — `SceneManager.LoadScene("_Bootstrap", Additive)` at runtime requires the scene to be enabled in Build Settings. The idempotent `AddSceneToBuildSettings` helper in the scene generator handles this.

ScriptableObject authoring tools (bulk-create `DigimonDefinition` / `TechniqueDefinition` / `ItemDefinition` from CSV) are fine and follow the same shape — they feed the prefab generators when/if a prefab needs an SO reference.

PlayMode test fixtures are the one exception: they're built in memory and torn down at teardown, never saved to disk.

## Naming & conventions

- **Namespaces:** not used. Scripts live in the global namespace. Revisit if assembly/naming collisions actually show up.
- **Services:** `PascalCase` MonoBehaviour inheriting `PersistentSingleton<Self>` (e.g. `AudioSystem : PersistentSingleton<AudioSystem>`). No `I<Name>Service` interface.
- **Scenes:** `PascalCase.unity`; scenes that are *not* player-facing gameplay (bootstrap, intro, debug) are prefixed with `_` (`_Bootstrap.unity`, `_Intro.unity`). Zone scenes will use `Zone_` prefix when they arrive.
- **Prefabs:** `PascalCase.prefab`, filename matches the dominant component (`AudioSystem.prefab` contains `AudioSystem`).
- **ScriptableObjects:** `PascalCaseDefinition` or `PascalCaseData`.
- **Private fields:** `_camelCase`; serialized fields `[SerializeField] private ...`.
- **Folders:** `PascalCase`.

## Phased system introduction (the core rule)

Each system enters the plan at the phase where gameplay first actually needs it — not upfront. Audio, UI framework, scene loader, debug tools are all deferred to their first real consumer. See the Implementation Order in `DigimonWorld1.md`.

Concretely: don't build a UI framework in Phase 0 because "we'll need it." Build it in Phase 2 when dialogue + HUD arrive. Don't add `AudioSource.Play` ad-hoc in Phase 3 to sneak audio in before Phase 4 — pull the audio item forward instead.

(`AudioSystem.cs` exists as a stub earlier than its nominal Phase 4 slot — it's currently the validation target for the bootstrap-singleton-generator pipeline, not a real service yet.)

## Project layout (current)

```
Assets/
  _Project/
    Input/       # InputSystem_Actions.inputactions — Unity's default new-input-system asset, not yet wrapped by a service
    Prefabs/     # AudioSystem.prefab — generated
    Scenes/      # _Bootstrap.unity (generated), _Intro.unity (hand-authored, plays IntroVideo.mp4)
    Scripts/
      Bootstrapper.cs              # [RuntimeInitializeOnLoadMethod] additively loads _Bootstrap
      PersistentSingleton.cs       # generic MonoBehaviour singleton base
      AudioSystem.cs               # first service stub (singleton pattern reference)
      Editor/
        FileExtensions.cs          # Project panel shows file extensions
        Generators/
          GenerateBootstrapPrefabs.cs   # one [MenuItem] per prefab
          GenerateBootstrapScene.cs     # builds _Bootstrap.unity from prefabs + registers in Build Settings
    Settings/    # URP assets (PC_RPAsset, Mobile_RPAsset, renderers, volume profile)
    Videos/      # IntroVideo.mp4 (used by _Intro.unity)
  Resources/     # empty
```

Folders for `UI/`, `Debug/`, zone scenes, etc. get created by the phase that introduces them. Don't pre-create empty folders.

## Key packages

- **Input System** (`com.unity.inputsystem` 1.19.0) — new input system is the standard; do not use the legacy `Input` class.
- **URP** (`com.unity.render-pipelines.universal` 17.3.0) — both PC and Mobile render pipeline assets exist under `_Project/Settings/`.
- **AI Navigation** (`com.unity.ai.navigation` 2.0.11) — for partner follow / enemy AI in later phases.
- **PrimeTween** (`com.kyrylokuzyk.primetween` 1.3.8) — preferred tweening library; use it instead of coroutine-based tweens.
- **Eflatun.SceneReference** (git package, 5.0.0) — typed scene references for the Phase 1 scene/zone loader (avoid raw string scene names).
- **Timeline** (1.8.11) — reserved for the Phase 6 cutscene system.

## Coding principles

- **KISS** — simplest thing that works. No clever patterns where a plain `if` does the job. If a class is under 50 lines and clear, don't split it.
- **YAGNI** — don't build for hypothetical needs. No interfaces with one implementation, no config knobs with one value, no abstraction layers "for later." This is why services are singletons with no `I<Name>Service` interface — extract one only when a second implementation shows up.
- **DRY** — remove real duplication, not shape-similar code. Three copies of the same logic → extract. Two functions that happen to both take a `Vector3` → leave alone. Wrong abstraction costs more than repetition.

When in doubt, lean KISS over DRY. A bit of repetition is cheaper to read and change than the wrong shared helper.

## Git

- Remote: `TheGabmeister/games-unity` — this project lives as a sub-directory of that repo.
- Default branch: `main`.
- Unity meta files are tracked — don't delete them. `.gitignore` follows the standard Unity template.

## Things not in the repo yet

- No CI / GitHub Actions.
- No `.editorconfig`, no formatter config.
- No asmdefs — everything compiles into the default assemblies.
- No unit tests.
- No InputSystem service wrapper around `InputSystem_Actions.inputactions` — arrives with Phase 1's player movement.
