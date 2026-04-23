# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Gameplay-focused Unity recreation of Digimon World 1. Not a pixel-perfect remake — faithful mechanics with placeholder 3D models (no animations), placeholder textures, placeholder audio, single-player. The phased roadmap lives in [DigimonWorld1.md](DigimonWorld1.md) (6 phases). Treat that doc as the source of truth for scope and sequencing.

Status: Phase 0–1 complete, Phase 2–3 in progress. Working: bootstrap, scene flow, input, player movement, partner follow AI, interaction, dialogue, zone transitions, time system, HUD with partner stats, care system (hunger/tiredness/sleep), inventory with items, training facilities, pause/inventory/status screens.

## Unity version & running

**Unity 6000.3.12f1** (Unity 6), pinned via `ProjectSettings/ProjectVersion.txt`.

- **Run:** press Play from any scene. `Bootstrapper` auto-loads `_Bootstrap.unity` + `_Gameplay.unity` (if in a zone scene) before any `Awake`.
- **Tests:** none written yet. Test Framework available via `Window → General → Test Runner`.
- **Linting/formatting:** none configured.

## Architecture: bootstrap + singletons

- `_Bootstrap.unity` is the persistent scene, auto-loaded by [Bootstrapper.cs](Assets/_Project/Scripts/Bootstrapper.cs) (`[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`). Scene paths come from [BootstrapConfig.cs](Assets/_Project/Scripts/BootstrapConfig.cs) in `Resources/`.
- One singleton base class: [Singleton.cs](Assets/_Project/Scripts/Singleton.cs). No `DontDestroyOnLoad` — services live as long as their scene. `OnDestroy()` nulls the static instance.
- **No service locator, no DI framework, no `I<Name>Service` interfaces.** Extract an interface only when a second implementation shows up.
- Prefabs organized under `Assets/_Project/Prefabs/`: `Services/`, `UI/`, `Controllers/`, `Characters/`, `Interactables/`.
- Service `Awake` order is unspecified. If service A needs B during `Awake`, resolve in `Start` or set Script Execution Order.

### _Bootstrap services

`GameManager` (scene orchestrator, zone transitions with `ScreenFader` fade), `SceneLoader` (async load/unload), `ScreenFader` (sortingOrder 999), `AudioSystem` (stub).

### _Gameplay services & systems

`_Gameplay.unity` holds gameplay-scoped singletons + player + partner + camera. Zone scenes load additively on top.

- **InputManager** — owns `InputSystem_Actions`. `SetPlayerInputEnabled(bool)` gates `PlayerController` without disabling the action map.
- **TimeSystem** — in-game clock (1 real second = 1 in-game minute). `OnHourChanged` event. Uses `Time.deltaTime` so pauses with `timeScale = 0`.
- **CareSystem** — subscribes to `OnHourChanged`. Ticks hunger/tiredness, manages sleep (21:00–06:00), tracks care mistakes. Exposes `Feed()`, `Praise()`, `Scold()`.
- **Inventory** — stackable items (max 20 slots) + Bits currency. `UseItem` routes food through `CareSystem.Feed()`, non-food applies effects directly to `DigimonInstance`.
- **DialogueManager** — bottom-screen panel, E key advances, disables player input while active.
- **HUD** — top-right: time + day. Top-left: partner stats.
- **InventoryScreen** (Tab/I), **StatusScreen** (C), **PauseScreen** (Escape) — toggleable UI screens, mutually exclusive. PauseScreen sets `timeScale = 0`.

### Canvas sorting order

HUD: 50 → InventoryScreen/StatusScreen: 80 → PauseScreen: 90 → DialogueManager: 100 → ScreenFader: 999

## Scene flow

```
_Bootstrap (persistent) → _Splashscreen → _Intro → _MainMenu → _Name → _Gameplay + Zone
```

Each non-gameplay scene has a controller (`SplashscreenController`, `IntroController`, `MainMenuController`, `NameController`) that calls `GameManager.Instance.LoadXxxScene()` to advance.

### Zone system

`ZoneData` SO (`SceneReference` + `CameraPosition`). `ZoneTrigger` (`BoxCollider` trigger) calls `GameManager.LoadZone()` which fades, unloads old zone, loads new, repositions camera. Zone scenes contain only environment — no camera, no player.

## Gameplay systems

**Player:** `PlayerController` — `CharacterController`-based, camera-relative movement, `SphereCast` interaction detection. `GameplayCamera` — fixed position per zone, tracks player.

**Partner Digimon:** single `PartnerDigimon.prefab` with `DigimonFollow` (follow AI) + `DigimonInstance` (runtime mutable state). Species swappable via `DigimonSpeciesData` reference.

**DigimonInstance:** holds current HP/MP, training bonuses (`_bonusOffense`/`_bonusDefense`/`_bonusSpeed`/`_bonusBrains`), care stats (hunger, tiredness, happiness, discipline, care mistakes, virus gauge, weight, age). `TrainStat(TrainableStat, int)` for training. `InitializeFromSpecies()` resets all state.

**Interaction:** `IInteractable` interface (`Interact()`, `ShowPrompt()`, `HidePrompt()`). Implementations: `NPCInteractable` (dialogue), `TrainingFacility` (stat training, instant success — mini-game deferred to Phase 6), `TestInteractable` (debug).

**Data model SOs:** `DigimonSpeciesData` (species identity + base stats + techniques), `TechniqueData`, `ItemData` (flat effect fields), `TrainingData`, `DialogueData`. All use `[CreateAssetMenu(menuName = "DigimonWorld/...")]`. Assets in `Assets/_Project/Data/` subdirectories.

**Enums:** `DigimonStage`, `DigimonAttribute`, `TrainableStat`, `TechniqueCategory` (in `DigimonEnums.cs`). `ItemCategory` (in `ItemCategory.cs`).

## Editor workflow: generators

**Two-pass: data/prefabs first, then scenes.** Use the **Generator Window** (`Tools → DigimonWorld → Generator Window`) — dockable panel with all generators by category + "Generate All" button.

### Rules

- **One menu item per prefab.** Simple prefabs use `PrefabGeneratorUtils.SavePrefab()` in `GeneratePrefabs.cs`. Complex UI prefabs get their own file (e.g. `GenerateHUDPrefab.cs`).
- **Never mix prefab creation and scene composition in one pass.**
- **Generators are idempotent** — re-running overwrites in place.
- **Use `PrefabUtility.InstantiatePrefab`** in scenes (not `Instantiate`).
- Materials: `PrefabGeneratorUtils.CreateOrLoadMaterial(path, color)` — URP Lit shader, `_BaseColor` property.

## 3D model generation (Blender)

Headless Blender scripts in `Assets/_Project/Scripts/Editor/Generators/BlenderScripts/`. Blender path: `C:\Program Files\Blender Foundation\Blender 5.1\blender.exe`.

Key export settings: `axis_forward='-Z', axis_up='Y', use_space_transform=True, bake_space_transform=True`. Pivot at feet (min_z = 0).

## Naming & conventions

- **Namespaces:** not used.
- **Services:** `PascalCase` MonoBehaviour inheriting `Singleton<Self>`.
- **Scenes:** `_PascalCase.unity` (non-gameplay), plain names in `Scenes/Zones/`.
- **ScriptableObjects:** `PascalCaseData`.
- **Private fields:** `_camelCase` with `[SerializeField] private`.

## Key packages

- **Input System** (1.19.0) — code-generated wrapper. Do not use legacy `Input` class.
- **URP** (17.3.0) — `Universal Render Pipeline/Lit` shader, `_BaseColor` property.
- **Eflatun.SceneReference** (5.0.0) — typed scene refs by GUID.
- **PrimeTween** (1.3.8) — preferred over coroutine tweens.
- **AI Navigation** (2.0.11) — for later phases.

## Coding principles

- **KISS** — simplest thing that works. No clever patterns where a plain `if` does the job.
- **YAGNI** — don't build for hypothetical needs. No interfaces with one implementation, no config knobs with one value.
- **DRY** — remove real duplication, not shape-similar code. Wrong abstraction costs more than repetition.
- **Phased introduction** — each system enters at the phase where gameplay first needs it.

## Git

- Remote: `TheGabmeister/games-unity` (project is a sub-directory).
- Default branch: `main`.
- Unity meta files are tracked — don't delete them.
- No CI, no `.editorconfig`, no asmdefs, no unit tests.
