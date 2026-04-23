# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Gameplay-focused Unity recreation of Digimon World 1. Not a pixel-perfect remake — faithful mechanics with placeholder 3D models (no animations), placeholder textures, placeholder audio, single-player. The phased roadmap lives in [DigimonWorld1.md](DigimonWorld1.md) (6 phases, Phase 0 = Foundation). Treat that doc as the source of truth for scope and sequencing.

Status: Phase 0 complete, Phase 1 in progress. Bootstrap pattern, scene flow, input system, player movement, partner follow AI, interaction system, dialogue, and zone transitions are all working.

## Unity version & running

**Unity 6000.3.12f1** (Unity 6), pinned via `ProjectSettings/ProjectVersion.txt`. Don't upgrade casually — package compatibility matters.

No CLI build pipeline. All work happens in the Unity Editor:

- **Open:** open this folder as a project in Unity Hub.
- **Run:** press Play from any scene. `Bootstrapper` has a `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` that additively loads `_Bootstrap.unity` before any scene's `Awake`. If the active scene is `_Gameplay` or a zone scene (path contains `/Zones/`), it also loads `_Gameplay` — so pressing Play from a zone scene gets you a fully bootstrapped world.
- **Tests:** Unity Test Framework (`com.unity.test-framework` 1.6.0) via `Window → General → Test Runner`. No tests written yet, no CI configured.
- **Linting/formatting:** none configured.

## Architecture: bootstrap + singletons

- [_Bootstrap.unity](Assets/_Project/Scenes/_Bootstrap.unity) is the persistent scene, additively loaded before any other scene's `Awake` by [Bootstrapper.cs](Assets/_Project/Scripts/Bootstrapper.cs) — a **plain class** (not a MonoBehaviour) with `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` + `[DefaultExecutionOrder(-1000)]`. Bootstrapper reads scene paths from [BootstrapConfig.cs](Assets/_Project/Scripts/BootstrapConfig.cs), a `ScriptableObject` in `Resources/` — no hardcoded scene names.
- One singleton base class: [Singleton.cs](Assets/_Project/Scripts/Singleton.cs). No `DontDestroyOnLoad` — scene lifetimes are managed explicitly by `GameManager` and `Bootstrapper`. Services live as long as their scene does. `OnDestroy()` nulls the static instance.
- **No service locator, no DI framework, no `I<Name>Service` interfaces.** Extract an interface only when a second implementation shows up. (`IInteractable` is an interface for world objects, not a service wrapper — that's fine.)
- Each service lives as its own prefab under `Assets/_Project/Prefabs/`.
- Service `Awake` order is unspecified relative to each other. If service A needs service B during `Awake`, resolve it in `Start` or set A's Script Execution Order explicitly.

### Current services in _Bootstrap

| Service | Role |
|---------|------|
| `AudioSystem` | Stub — validation target for the singleton-generator pipeline |
| `GameManager` | Orchestrator. Holds `SceneReference` fields for all scenes, `ZoneData` references for zones. Manages scene transitions with fade (`ScreenFader.Instance`), delegates async loading to `SceneLoader`. Tracks `_currentZone` and handles zone swaps via `LoadZone(ZoneData)` |
| `ScreenFader` | Full-screen Canvas overlay (sortingOrder 999). `FadeOut()` / `FadeIn()` are `async Awaitable` using `Time.unscaledDeltaTime` |
| `SceneLoader` | Thin async wrapper around `SceneManager`. `LoadScene(SceneReference)` and `UnloadScene(SceneReference)`. Fires `OnSceneLoadStarted` / `OnSceneLoadCompleted` events. No orchestration logic — just load and unload |

### Additional services in _Gameplay

`_Gameplay.unity` holds gameplay-scoped singletons alongside the player, partner, and camera. Zone scenes are loaded/unloaded additively on top of it.

| Service | Role |
|---------|------|
| `InputManager` | Owns the single `InputSystem_Actions` instance. All gameplay systems read input via `InputManager.Instance.Actions`. `SetPlayerInputEnabled(bool)` sets a flag that `PlayerController` checks to gate movement/interaction — the action map stays enabled so systems like `DialogueManager` can still read Interact |
| `DialogueManager` | Owns dialogue UI Canvas (sortingOrder 100). `StartDialogue(DialogueData)` calls `InputManager.SetPlayerInputEnabled(false)`, shows lines one at a time, E key advances. `EndDialogue()` re-enables player input. `IsActive` property for guard checks |
| `TimeSystem` | In-game clock. 1 real second = 1 in-game minute. Tracks `Hour`, `Minute`, `Day`. Uses `Time.deltaTime` so clock pauses with `timeScale = 0`. `SetPaused(bool)` for explicit pause. `OnHourChanged` event for future time-gated hooks |
| `HUD` | Screen overlay Canvas (sortingOrder 50). Displays `TimeSystem.TimeString` as "HH:MM" in top-right corner |

## Scene flow

```
_Bootstrap (persistent, auto-loaded)
    ↓ GameManager
_Splashscreen → _Intro → _MainMenu → _Name → _Gameplay + Zone scene
```

Each non-gameplay scene has a controller MonoBehaviour that drives its logic and calls `GameManager.Instance.LoadXxxScene()` to advance:

| Scene | Controller | Behavior |
|-------|-----------|----------|
| `_Splashscreen` | `SplashscreenController` | Shows placeholder logo via OnGUI, waits `_duration` seconds |
| `_Intro` | `IntroController` | Plays VideoPlayer, skippable via any key |
| `_MainMenu` | `MainMenuController` | uGUI: "Press Start" (blinking) → 4-option menu (New Game, Continue, Delete, Battle Mode) |
| `_Name` | `NameController` | uGUI: two TMP_InputFields + Confirm button |
| `_Gameplay` | (no controller) | Player, partner Agumon, camera, InputManager, DialogueManager, TimeSystem, HUD. Zone scenes loaded additively on top |

### Zone system

- [ZoneData.cs](Assets/_Project/Scripts/ZoneData.cs) — `ScriptableObject` with `SceneReference` + `Vector3 CameraPosition`. Created via `Create → DigimonWorld → ZoneData`. Assets in `Assets/_Project/Data/Zones/`.
- [ZoneTrigger.cs](Assets/_Project/Scripts/ZoneTrigger.cs) — `BoxCollider` trigger in zone scenes. On player enter, calls `GameManager.Instance.LoadZone(destinationZone)`.
- `GameManager.LoadZone()` — fades out, unloads current zone, loads new zone, repositions camera, fades in. `_isTransitioning` flag prevents re-entry during fade.
- `GameManager.Start()` detects which zone is already loaded (for press-Play-from-zone-scene) by iterating `_allZones[]`.
- Zone scenes contain only environment: terrain, props, NPCs, interactables, zone triggers. No camera, no player.

## Gameplay systems

### Player movement
[PlayerController.cs](Assets/_Project/Scripts/PlayerController.cs) — `CharacterController`-based, camera-relative horizontal movement. Reads input from `InputManager.Instance.Actions`. Walk/sprint, gravity, rotates to face movement direction. Includes interaction detection via `SphereCast`. Early-returns from `Update()` when `InputManager.PlayerInputEnabled` is false.

### Camera
[GameplayCamera.cs](Assets/_Project/Scripts/GameplayCamera.cs) — fixed position, `LookAt` player in `LateUpdate`. Digimon World 1 style: camera doesn't follow, just tracks. Position set per-zone via `ZoneData.CameraPosition`.

### Partner follow
[DigimonFollow.cs](Assets/_Project/Scripts/DigimonFollow.cs) — `CharacterController`-based AI. Follows when distance > `_followDistance`, slows as it approaches `_stopDistance`, stops when close. Includes gravity.

### Interaction
[IInteractable.cs](Assets/_Project/Scripts/IInteractable.cs) — interface: `InteractPrompt`, `Interact()`, `ShowPrompt()`, `HidePrompt()`. PlayerController does a `SphereCast` each frame and manages show/hide transitions. [TestInteractable.cs](Assets/_Project/Scripts/TestInteractable.cs) is a test cube that changes color on interact with a billboard TextMeshPro prompt. [NPCInteractable.cs](Assets/_Project/Scripts/NPCInteractable.cs) triggers dialogue via `DialogueManager`.

### Dialogue
[DialogueData.cs](Assets/_Project/Scripts/DialogueData.cs) — `ScriptableObject` with a `DialogueLine[]` (each line has `Speaker` + `Text`). Created via `Create → DigimonWorld → DialogueData`. [DialogueManager.cs](Assets/_Project/Scripts/DialogueManager.cs) — `Singleton` in `_Gameplay`. `StartDialogue()` calls `InputManager.SetPlayerInputEnabled(false)`, shows a bottom-screen panel, E key advances lines. A `_justOpened` flag prevents the triggering E press from advancing past line 0.

### Input
[InputManager.cs](Assets/_Project/Scripts/InputManager.cs) — `Singleton` in `_Gameplay`. Owns the single `InputSystem_Actions` instance; all gameplay systems read from `InputManager.Instance.Actions`. `PlayerInputEnabled` flag gates `PlayerController` without disabling the action map, so `DialogueManager` can still read Interact. `InputSystem_Actions.inputactions` has C# code generation enabled. Player action map has: Move, Look, Sprint, Attack, Interact, Jump, Crouch, Previous, Next. Menu controllers (`MainMenuController`, `IntroController`) use `Keyboard.current` directly — they exist in isolated non-gameplay scenes.

## Editor workflow: generators

**Two-pass workflow — prefabs first, then scenes.** All menu items live under `Tools → DigimonWorld`.

### Generator files

| File | Responsibility |
|------|---------------|
| [PrefabGeneratorUtils.cs](Assets/_Project/Scripts/Editor/Generators/PrefabGeneratorUtils.cs) | Shared helpers: `SavePrefab`, `CreateCanvasRoot`, `SaveAndCleanup`, `CreatePanel`, `CreateText`, `CreateInputField`, `SetSceneReference`, `CreateOrLoadMaterial`, `ApplyMaterialToRenderers`, `EnsureFolder` |
| [GeneratePrefabs.cs](Assets/_Project/Scripts/Editor/Generators/GeneratePrefabs.cs) | Simple prefabs + data assets: Bootstrapper, AudioSystem, InputManager, SceneLoader, ScreenFader, GameManager, SplashscreenController, IntroController, Player, Agumon, NPC, TimeSystem. Data: TestDialogue, BootstrapConfig, ZoneData assets |
| [GenerateMainMenuPrefab.cs](Assets/_Project/Scripts/Editor/Generators/GenerateMainMenuPrefab.cs) | MainMenuController with full Canvas + uGUI hierarchy |
| [GenerateNamePrefab.cs](Assets/_Project/Scripts/Editor/Generators/GenerateNamePrefab.cs) | NameController with Canvas + InputFields + Confirm button |
| [GenerateDialoguePrefab.cs](Assets/_Project/Scripts/Editor/Generators/GenerateDialoguePrefab.cs) | DialogueManager with Canvas (sortingOrder 100) + bottom panel + speaker/body text |
| [GenerateHUDPrefab.cs](Assets/_Project/Scripts/Editor/Generators/GenerateHUDPrefab.cs) | HUD with Canvas (sortingOrder 50) + time text in top-right corner |
| [GenerateScenes.cs](Assets/_Project/Scripts/Editor/Generators/GenerateScenes.cs) | All scenes: Bootstrap, Splashscreen, Intro, MainMenu, Name, Gameplay, Zone1, Zone2, plus GenerateAll. Zone scenes include `ZoneTrigger` creation via `CreateZoneTrigger` helper |

### Rules

- **One menu item per prefab.** Add a new service/prefab by adding a const path, a `[MenuItem]` method, and one call to the shared `SavePrefab` helper (or manual creation for complex UI prefabs).
- **Never mix prefab creation and scene composition in one pass.** The prefab pass must `AssetDatabase.SaveAssets` before any scene generator reads the prefab.
- **Generators are idempotent** — re-running overwrites in place.
- **Use `PrefabUtility.InstantiatePrefab`** in scenes (not raw `Instantiate` — that breaks the prefab link).
- **Scene generator registers scenes in Build Settings** — `_Bootstrap` at index 0, others appended.
- **Complex UI prefabs get their own generator file** (e.g. `GenerateMainMenuPrefab.cs`) to keep `GeneratePrefabs.cs` focused on simple prefabs.

### Materials

`PrefabGeneratorUtils.CreateOrLoadMaterial(path, color)` creates a URP Lit material at the given asset path, using `_BaseColor` (not legacy `_Color`). Materials live alongside their assets:
- `Assets/_Project/Digimons/Agumon/Agumon.mat`
- `Assets/_Project/Props/Ground.mat`
- `Assets/_Project/Props/NPC.mat`
- Zone materials: `Zone1Ground.mat`, `Zone2Ground.mat`, etc. in `Assets/_Project/Props/`

## 3D model generation (Blender)

Placeholder models are generated via headless Blender Python scripts under `Assets/_Project/Scripts/Editor/Generators/BlenderScripts/`. Blender path: `C:\Program Files\Blender Foundation\Blender 5.1\blender.exe`.

```bash
blender.exe --background --python generate_agumon.py -- "output/path.fbx"
```

Key export settings for Unity compatibility (no -90° rotation):
```python
bpy.ops.export_scene.fbx(
    axis_forward='-Z', axis_up='Y',
    use_space_transform=True, bake_space_transform=True,
)
```

Pivot points must be at the feet (shift mesh vertices so min_z = 0, keep origin at world origin).

Current models:
- `Assets/_Project/Player/Player.fbx` — human figure (~1.8m)
- `Assets/_Project/Digimons/Agumon/Agumon.fbx` — stocky dinosaur (~1m)

## Naming & conventions

- **Namespaces:** not used. Scripts live in the global namespace.
- **Services:** `PascalCase` MonoBehaviour inheriting `Singleton<Self>`.
- **Scenes:** `PascalCase.unity`; non-gameplay scenes prefixed with `_` (`_Bootstrap.unity`, `_Intro.unity`). Zone scenes use plain names in `Scenes/Zones/` (`Zone1.unity`).
- **Prefabs:** `PascalCase.prefab`, filename matches the dominant component.
- **ScriptableObjects:** `PascalCaseData` (e.g. `DialogueData`, `ZoneData`).
- **Private fields:** `_camelCase`; serialized fields `[SerializeField] private ...`.
- **Folders:** `PascalCase`.

## Phased system introduction (the core rule)

Each system enters at the phase where gameplay first needs it — not upfront. Don't build a UI framework in Phase 0 because "we'll need it." Build it in Phase 2 when dialogue + HUD arrive.

## Key packages

- **Input System** (`com.unity.inputsystem` 1.19.0) — new input system with code-generated wrapper. Do not use the legacy `Input` class.
- **URP** (`com.unity.render-pipelines.universal` 17.3.0) — materials must use `Universal Render Pipeline/Lit` shader and `_BaseColor` property.
- **AI Navigation** (`com.unity.ai.navigation` 2.0.11) — for partner follow / enemy AI in later phases.
- **PrimeTween** (`com.kyrylokuzyk.primetween` 1.3.8) — preferred tweening library; use it instead of coroutine-based tweens.
- **Eflatun.SceneReference** (git package, 5.0.0) — typed scene references used by `GameManager` and `BootstrapConfig`. Serializes by GUID internally; construct via `new SceneReference(guid)` or assign in Inspector.
- **Timeline** (1.8.11) — reserved for the Phase 6 cutscene system.

## Coding principles

- **KISS** — simplest thing that works. No clever patterns where a plain `if` does the job.
- **YAGNI** — don't build for hypothetical needs. No interfaces with one implementation, no config knobs with one value.
- **DRY** — remove real duplication, not shape-similar code. Wrong abstraction costs more than repetition.

When in doubt, lean KISS over DRY.

## Git

- Remote: `TheGabmeister/games-unity` — this project lives as a sub-directory of that repo.
- Default branch: `main`.
- Unity meta files are tracked — don't delete them. `.gitignore` follows the standard Unity template.

## Things not in the repo yet

- No CI / GitHub Actions.
- No `.editorconfig`, no formatter config.
- No asmdefs — everything compiles into the default assemblies.
- No unit tests.
- No loading screen.
