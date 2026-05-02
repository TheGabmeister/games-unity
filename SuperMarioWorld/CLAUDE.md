# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Unity 6 (`6000.3.12f1`) project recreating *Super Mario World* (SNES, 1990) gameplay with placeholder primitives — no sprites, no textures, no audio assets. Mechanical fidelity, not visual fidelity. URP 2D, Physics2D, new Input System.

**The authoritative design document is [SPEC.md](SPEC.md) at the project root.** It covers architecture, subsystems, phases, and every non-trivial decision. Always read the relevant sections of SPEC.md before making architectural choices, adding packages, or diverging from patterns that are already established there. This file is a map to the spec and a list of rules that are easy to accidentally violate — not a replacement for it.

## Current state

**Phase 1 implementation landed; tuning gate open.** Phase 0 scaffolding (`GameServices` + service skeletons, physics layer matrix, Boot/Systems/Title/Overworld scenes, PlayerInputManager) plus Phase 1's player stack (`PlayerController` + `GroundProbe` + `PlayerInputBinding` + `PlayerCarry` placeholder), camera + level scaffolding (`LevelCamera`, `LevelBounds`, `SpawnMarker`, `LevelContext`), environment + player prefab generators, SVG placeholders with PPU=16 import defaults, and a hand-authored Movement Test scene are all in place. Phase 1's subjective tuning gate ("run + jump feels responsive") is the current open item — expect iteration on `PlayerController` constants. See [TASKS.md](TASKS.md).

## How to build / run

No command-line build for the game itself. Open in Unity Hub (`6000.3.12f1` exact) and run from the editor.

- **Target resolution: 1280×720 (16:9)**, already configured in `ProjectSettings/ProjectSettings.asset`.
- **Press Play from any scene** — `Bootstrapper.EnsureSystemsLoaded` (a `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`) loads `Systems` additively before any `Awake` runs, and `TitleRoot` / `OverworldRoot` / `LevelRoot` detect direct editor entry. Don't add early-return code that assumes `Boot` was the entry scene. See SPEC.md §4.14.

### Setup composites

Prefab + SO + Phase-0-scene regeneration is safe and idempotent — use these when a fresh clone or a broken project state needs rebuilding. (Debug and content scenes are hand-authored, not regenerated — see the scene rule in "Load-bearing rules.")

- `Tools → SMW → Setup → Run Full Phase 0 Setup` — regenerates Boot / Systems / Title / Overworld scenes, applies the 2D physics layer matrix per SPEC §4.19, and creates the `EditorTestSettings` + `AudioCatalog` default SOs.
- `Tools → SMW → Setup → Run Full Phase 1 Setup` — force-reimports art SVGs (so PPU=16 applies), regenerates environment + Player prefabs, re-runs the Phase 0 scene bootstrap so `PlayerInputManager.playerPrefab` picks up the fresh Player prefab. Idempotent; safe to rerun after adding a new SVG or tweaking a prefab generator.
- `Tools → SMW → Setup → Bootstrap Phase 0 Scenes` — scenes only.
- `Tools → SMW → Setup → Apply Physics2D Layer Matrix` — matrix only.
- `Tools → SMW → Generate → Prefabs → Environment — Create Missing Only` / `Regenerate All (Overwrite)` — individual prefab-family regen. Same two-mode pattern for Player.

### Running tests

Two paths:

- **Editor UI**: Window → General → Test Runner → EditMode / PlayMode.
- **Unity CLI (batchmode)** — works with the editor closed:
  ```
  "C:/Program Files/Unity/Hub/Editor/6000.3.12f1/Editor/Unity.exe" \
    -batchmode -nographics -projectPath "<repo>" \
    -runTests -testPlatform EditMode \
    -testResults "<repo>/Temp/TestReports/editmode.xml" \
    -logFile "<repo>/Temp/TestReports/editmode.log"
  ```
  Replace `EditMode` with `PlayMode` for the other suite. Exit code 0 = all passed, 2 = at least one failure. XML is NUnit-compatible. Unity holds an exclusive project lock — close the editor first.
- **Batchmode gotcha:** PlayMode tests in batchmode have been observed to hang after first-import scenarios (newly-added SVGs, orphaned script refs in scenes, cold Library cache). EditMode batchmode is reliable. If PlayMode hangs past ~3 minutes with no log growth, kill the process, open the editor interactively once to let assets settle, then retry. For development, prefer the Test Runner UI.

## Packages (current manifest)

Beyond built-in Unity modules:

- `com.unity.inputsystem` 1.19.0 — new Input System. Do **not** call `Input.GetKey` / `Input.*`.
- `com.unity.render-pipelines.universal` 17.3.0 with **2D Renderer**.
- `com.unity.feature.2d` 2.0.2 — Sprite Shape, 2D Animation, Aseprite Importer, Pixel Perfect. Tilemap is included in the bundle but **is not used** (see SPEC.md §4.5 — level geometry is GameObject prefabs).
- `com.unity.ugui` 2.0.0 — uGUI with TextMeshPro bundled. **uGUI, not UI Toolkit** (decision in SPEC.md §4.17).
- `com.unity.vectorgraphics` 3.0.0-preview.7 — SVG importer. Colors are **baked into the SVG source** (`fill="#C87137"` on Goomba, etc.); no palette layer, no `PaletteBinding`, no runtime tinting. See SPEC.md §4.18.
- `com.kyrylokuzyk.primetween` 1.3.8 — tween library. Prefer over coroutines for animation and UI transitions.
- `com.eflatun.scenereference` (git) — type-safe scene fields. Use `SceneReference`, never raw scene-name strings. Also pulls in `com.unity.nuget.newtonsoft-json` transitively, which is the serializer for the save system (SPEC.md §4.15).
- `com.unity.test-framework` 1.6.0 — used by `SMW.Tests.EditMode` and `SMW.Tests.PlayMode`.

## Assembly layout

Four `.asmdef`s, reached via folder placement (no GUID references):

- `SMW.Runtime` at `Assets/_Project/Scripts/SMW.Runtime.asmdef` — all gameplay code. Does **not** reference `UnityEditor`.
- `SMW.Editor` at `Assets/_Project/Scripts/Editor/SMW.Editor.asmdef` — editor tools (bootstrap generator, prefab generators, physics matrix setup, build preprocessor, SVG import postprocessor). `includePlatforms: ["Editor"]`. References `Unity.VectorGraphics.Editor` for the `SVGImporter` type used by [SvgImportDefaults.cs](Assets/_Project/Scripts/Editor/Setup/SvgImportDefaults.cs).
- `SMW.Tests.EditMode` at `Assets/_Project/Scripts/Tests/EditMode/…asmdef` — `includePlatforms: ["Editor"]`, references `SMW.Editor`.
- `SMW.Tests.PlayMode` at `Assets/_Project/Scripts/Tests/PlayMode/…asmdef` — all platforms, references `SMW.Runtime` only.

Both test asmdefs use `optionalUnityReferences: ["TestAssemblies"]` + `nunit.framework.dll` precompiled ref.

## Load-bearing rules (easy to violate)

These codify decisions in SPEC.md that conflict with Unity defaults or common tutorials. Re-read the cited section before breaking any of them.

- **Single flat `SMW` namespace for every script** (§2) — no subsystem sub-namespaces. Assembly + folder layout carry the separation. Prefix clashing types (`LevelState` / `LevelRunState` / `LevelRoot`) rather than reintroducing `SMW.Level.*`.
- **Thin prefab, fat SO** (§4.25). Prefabs are structural shells; per-variant tuning lives on ScriptableObjects. Applies to **pickups and question-block contents** — one `Pickup` class + different `PickupData` SOs per variant; one `Block_Question` prefab + different `BlockContents` SOs per instance. Each block *type* (brick, note, rotating, P-switch, …) is its own class + prefab. **Carve-out: enemies** (§4.7) — each archetype is its own MonoBehaviour class implementing capability interfaces. Per-variant color/stat differences still use `EnemyData` SOs.
- **Never hand-author `.prefab` YAML.** Use the prefab generators (§4.25). Hand-written prefab YAML silently desyncs with Unity's fileID/GUID coordination.
- **Prefab generators have two modes**: "Create Missing Only" (safe default) and "Regenerate All (Overwrite)" (explicit confirmation dialog).
- **Placeholder visuals use SVGs with colors baked directly into the `fill` attribute** (§4.18). No `Palette` SO, no `PaletteBinding`, no `PaletteRole` enum — these were explicitly removed. `SpriteRenderer.color` stays white; transient tints (hurt-flash, star-rainbow) are short-lived PrimeTween overrides that reset to white.
- **SVG pixels-per-unit = 16.** Any SVG under `Assets/_Project/Art/Sprites/` gets `SvgPixelsPerUnit = 16` stamped on import by [SvgImportDefaults.cs](Assets/_Project/Scripts/Editor/Setup/SvgImportDefaults.cs). Author SVGs so 16 SVG units = 1 world tile (a 16×16 viewBox renders as 1×1 world units; a 32×16 renders as 2×1, used for shallow-slope aspect). If you add an SVG and it renders tiny, your import raced the postprocessor — reimport once.
- **Input architecture** (§4.1 — multiplayer-ready even though V1 is single-player):
  - **One `PlayerInputManager`** on the `Input` GameObject in `Systems.unity` (`JoinPlayersManually`, `InvokeCSharpEvents`). Exposed as `GameServices.InputManager`.
  - **`PlayerInput` components live on the Player prefab** (Phase 1), **not on any Systems GameObject**. `LevelContext.Begin` calls `InputManager.JoinPlayer(...)` on level entry.
  - **UI input uses `InputSystemUIInputModule`** on the EventSystem — works without any `PlayerInput`, which is why Title/Overworld nav works before P1 joins.
  - **Map switching iterates `PlayerInput.all`** via `GameServices.SwitchMapOnAllPlayers(InputMapNames.Player / Overworld / UI)`, called by `GameStateMachine` state `OnEnter`. `PausedState` snapshots per-player maps (dictionary keyed by `playerIndex`) on enter and restores on exit — generalizes to co-op by construction.
  - **Do not introduce** a custom `InputRouter` — it existed once, got removed; Unity's `SwitchCurrentActionMap` + `PlayerInput.all` iteration replaces it.
- **Player controller uses dynamic Rigidbody2D with `gravityScale = 0`** (§4.2). Not kinematic, not CharacterController. Manual velocity integration each `FixedUpdate`. Don't "fix" this to use Unity's built-in gravity; the chosen pattern is the Unity 6 standard for tight platformer feel.
- **The `Action` button is one button with two semantics** (§4.1): held = run; pressed = context-sensitive (fireball / cape sweep / Yoshi tongue / pickup / throw). Always call it `Action` in code and docs.
- **Crouch is derived, not bound** — read as `Move.y < -0.5` inside `PlayerInputBinding`. No standalone `Crouch` binding.
- **Cape flight is simplified** (§4.2 and §7): ground sweep + airborne slow-fall (25% gravity while descending, jump held). No flight takeoff, no dive, no P-meter.
- **Slope set is locked to SMW's two angles** (§4.5 and §7): steep 45° and shallow ~26.57° (1:2). No arbitrary slopes, no ceiling slopes. Slopes are **variable-length `PolygonCollider2D` prefabs** (`Slope_Steep_L/R`, `Slope_Shallow_L/R`). [Slope.cs](Assets/_Project/Scripts/Runtime/Environment/Slope.cs)'s `OnValidate` (and `Configure(kind, length)` for programmatic wiring) regenerates the polygon + scales the transform uniformly when `length` changes.
- **State lives in exactly three layers** (§4.24): Persistent (`SaveData`), Session (`GameSession` on `GameServices`), Per-attempt (`LevelRunState` on `LevelRoot`). Every mutable field must belong to one of these layers. Death = level scene reload. `SaveManager.Save()` only fires at level clear / overworld move / switch palace activation / file-select — never mid-level.
- **Audio pauses with gameplay** when `AudioListener.pause = true` (§4.16 / §4.22). The only exception is a dedicated `UiSfxChannel` (`ignoreListenerPause = true`) for the pause SFX and menu-nav cues. No ducking, no fade-out. Music stack is preserved across pause and resumes from the same playback head.
- **`GameServices` and `LevelContext` are service locators, not DI containers** (§3). Register at Boot (for `GameServices`) or `LevelContext.Begin` (for per-level). Do not introduce Zenject / VContainer.
- **`FindAnyObjectByType` / `FindFirstObjectByType` are banned in hot paths.** Cache references at scene load via the locator. The deprecated `FindObjectOfType` is banned entirely.
- **PrimeTween > coroutines** for time-based behavior that isn't strictly `FixedUpdate` physics. Use `useUnscaledTime: true` for UI animations that must run while `Time.timeScale = 0`.
- **Rebinding UI is out of V1 scope** (§4.1). Don't build it, don't plan for it.
- **HUD canvases** (`HUDRoot`, `TitleCanvas`, `OverworldCanvas`) all share `CanvasScalerPresetApplier` targeting 1280×720 with `MatchWidthOrHeight` / `Match = 0.5` (§4.17). One place to change resolution assumptions.
- **Physics layers and the 2D collision matrix** (§4.19) are committed via `ProjectSettings/TagManager.asset` (layer names) and `ProjectSettings/Physics2DSettings.asset` (the `m_LayerCollisionMatrix` hex field). `DynamicsManager.asset` is the 3D physics file and is not used by this project.
- **No central scoring service** (§4.20). Score lives on `SaveData.score` and is mutated inline at each awarding site (coin pickup, stomp, goal, time bonus, shell kill, 1-up pickup). Each 1-up rule lives on its natural owner: stomp combo on `PlayerController`, 100-coin rollover next to the `SaveData.totalCoins` increment, 5-dragon-coin check on `LevelRunState`. **Do not reintroduce** a `ScoreService`, a `ScoreReason` enum, a `Scoring` static, or a central `Awarded` event — there are ~5 callsites in the whole game and each has an obvious local owner for the state it reads.
- **Enemy combat uses capability interfaces** (§4.7): `IStompable`, `IBumpable`, `IFireballHit`, `ICapeSweepHit`, `IShellImpact`, `IThrowable`, `IConditionallyTangible`. Attackers dispatch via `TryGetComponent<I…>`. Two concerns are modeled as MonoBehaviour components instead of interfaces: `ContactDamage` (attached to enemies/projectiles/hazards that hurt Mario on side-contact) and `SpinJumpSafe` (empty marker on Spiny/Spike Top). **Do not introduce** a `CombatResolver` static, virtual methods on an abstract `Enemy` base class, a `CombatOutcome` struct, or a `MovementKind` enum with switch statements.
- **No object pooling.** Dynamic spawns (projectiles, VFX, score popups, enemies) use plain `Instantiate` / `Destroy`. If profiling turns up a genuine hot spot, address it locally.

## Repository layout

Game-specific assets live under [Assets/_Project/](Assets/_Project/) (leading underscore for sort order). The full planned structure is in SPEC.md §4.25 / §4.26. Current:

- [Assets/_Project/Scripts/](Assets/_Project/Scripts/)
  - `Runtime/` — Core, State, Scene, Save, Audio, Feedback, Session, Player, Environment, UI, Data. All under `SMW.Runtime` asmdef.
  - `Editor/` — `FileExtensions.cs` plus `Setup/` (bootstrap tools, Phase setup composites, SVG import defaults) + `Generators/` (prefab generators only — scene generators were removed after Phase 1) + `Build/` (`StripDebugScenesOnBuild` preprocessor). All under `SMW.Editor` asmdef.
  - `Tests/EditMode/` and `Tests/PlayMode/` — split test asmdefs.
- [Assets/_Project/Scenes/](Assets/_Project/Scenes/) — `Boot.unity`, `Systems.unity`, `Title.unity`, `Overworld.unity`. `Scenes/Debug/MovementTest.unity` landed with Phase 1 (hand-authored). `Scenes/Levels/` lands in Phase 7.
- [Assets/_Project/Settings/](Assets/_Project/Settings/) — URP + 2D renderer + volume profile + `EditorTestSettings.asset`. Modify these, don't create parallel copies.
- [Assets/_Project/Input/](Assets/_Project/Input/) — `InputSystem_Actions.inputactions` with Player / Overworld / UI action maps, keyboard + gamepad bindings per SPEC.md §4.1.
- [Assets/_Project/Data/](Assets/_Project/Data/) — `AudioCatalog.asset` today. `Enemies/`, `Pickups/`, `Blocks/`, `PowerStates/`, `Levels/` subfolders land in their respective phases.

`Assets/_Project/Prefabs/{Player,Enemies,Pickups,Blocks,Environment,Rideables,Projectiles,VFX}/` lands in the phases that need them. Don't create these earlier or mirror them outside `_Project/`.

## Coding conventions (from SPEC.md §2)

- Single flat `SMW` namespace for every script — no subsystem sub-namespaces. Assembly boundaries (`SMW.Runtime` / `SMW.Editor` / `SMW.Tests.*`) and folder layout (`Runtime/Audio/`, `Runtime/Player/`, …) carry the separation. If a new type collides, prefix it (`LevelState` / `LevelRunState` / `LevelRoot`) rather than reintroducing nested namespaces.
- Inspector-driven fields: `[SerializeField] private` + property accessor. No bare `public` fields on MonoBehaviours.
- One `MonoBehaviour` per file. Data classes live as ScriptableObjects.
- Prefer composition over inheritance. Share behavior via helper components (`KinematicBody2D`, `PeriodicEmitter`, etc.), not base classes. For enemies specifically, each archetype is its own MonoBehaviour class implementing capability interfaces; per-variant tuning uses `EnemyData` SOs against the same class.

## Existing scaffolding of note

- [FileExtensions.cs](Assets/_Project/Scripts/Editor/FileExtensions.cs) — `[InitializeOnLoad]` editor utility that draws file extensions in the Project window. Predates Phase 0; not part of any game system.
- [Phase0SetupAll.cs](Assets/_Project/Scripts/Editor/Setup/Phase0SetupAll.cs) — composite entry point (`Tools → SMW → Setup → Run Full Phase 0 Setup`) invoked from the Unity CLI during CI test runs and by humans after a fresh clone.
- [SceneBootstrapGenerator.cs](Assets/_Project/Scripts/Editor/Setup/SceneBootstrapGenerator.cs) — regenerates Boot/Systems/Title/Overworld scenes and registers them in Build Settings. Idempotent — safe to re-run.
- [PrefabGeneratorBase.cs](Assets/_Project/Scripts/Editor/Generators/PrefabGeneratorBase.cs) — base for prefab generators. The two-mode pattern ("Create Missing Only" / "Regenerate All (Overwrite)") lives here. Phase 1 populated it with [EnvironmentPrefabGenerator.cs](Assets/_Project/Scripts/Editor/Generators/EnvironmentPrefabGenerator.cs) + [PlayerPrefabGenerator.cs](Assets/_Project/Scripts/Editor/Generators/PlayerPrefabGenerator.cs).
- [StripDebugScenesOnBuild.cs](Assets/_Project/Scripts/Editor/Build/StripDebugScenesOnBuild.cs) — `IPreprocessBuildWithReport` that removes `Scenes/Debug/*` from the build list at build time. Debug scenes are registered in Build Settings during development for direct-Play-from-scene; this filter keeps them out of shipped builds.
