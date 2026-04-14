# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Unity 6 (`6000.3.12f1`) project recreating *Super Mario World* (SNES, 1990) gameplay with placeholder primitives — no sprites, no textures, no audio assets. Mechanical fidelity, not visual fidelity. URP 2D, Physics2D, new Input System.

**The authoritative design document is [SPEC.md](SPEC.md) at the project root.** It covers architecture, subsystems, phases, and every non-trivial decision. Always read the relevant sections of SPEC.md before making architectural choices, adding packages, or diverging from patterns that are already established there. This file is a map to the spec and a list of rules that are easy to accidentally violate — not a replacement for it.

## Current state

Early / pre-Phase-0. Only the single editor utility at [FileExtensions.cs](Assets/_Project/Scripts/Editor/FileExtensions.cs) exists; everything else described in SPEC.md (Bootstrapper, GameServices, SceneLoader, audio stub, HUD, prefab generators, debug scene generator, physics layer matrix, etc.) still needs to be built. See [SPEC.md §6 Implementation Phases](SPEC.md) for the build order — Phase 0 foundation first, then Phase 1 player/physics, then tiles/blocks, etc.

## How to build / run

No command-line build. Open in Unity Hub (`6000.3.12f1` exact) and run from the editor.

- Target resolution is **1280×720 (16:9)**. Configure in Player Settings as part of Phase 0.
- Scenes do not yet exist beyond `Main.unity`. Phase 0 introduces `Boot`, `Systems` (persistent, additively loaded), `Title`, `Overworld`, plus generated debug scenes under `Scenes/Debug/`. Content levels land in Phase 7.
- No test runner is set up yet. `com.unity.test-framework` is in the manifest but assembly definitions do not exist. Tests land in Phase 8; creating `SMW.Runtime`, `SMW.Editor`, `SMW.Tests` `.asmdef` files is a Phase 0 prerequisite.
- Press Play from any scene should work once Phase 0 ships — a `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` in `Bootstrapper` loads `Systems` additively before any `Awake` runs, and scene roots (`LevelRoot`, `TitleRoot`, `OverworldRoot`) detect direct editor entry. Don't add early-return code that assumes `Boot` was the entry scene. See SPEC.md §4.14.

## Packages (current manifest)

Beyond built-in Unity modules:

- `com.unity.inputsystem` 1.19.0 — new Input System. Do **not** call `Input.GetKey` / `Input.*`.
- `com.unity.render-pipelines.universal` 17.3.0 with **2D Renderer**.
- `com.unity.feature.2d` 2.0.2 — Tilemap, Sprite Shape, 2D Animation, Aseprite Importer, Pixel Perfect.
- `com.unity.ugui` 2.0.0 — uGUI with TextMeshPro bundled. **uGUI, not UI Toolkit** (decision in SPEC.md §4.17).
- `com.unity.vectorgraphics` 3.0.0-preview.7 — SVG importer. All placeholder visuals are SVG sprites with flat white fills tinted via the palette (SPEC.md §4.18).
- `com.kyrylokuzyk.primetween` 1.3.8 — tween library. Prefer over coroutines for animation and UI transitions.
- `com.eflatun.scenereference` (git) — type-safe scene fields. Use `SceneReference`, never raw scene-name strings. Also pulls in `com.unity.nuget.newtonsoft-json` transitively, which is the serializer for the save system (SPEC.md §4.15).
- `com.unity.test-framework` 1.6.0 — available but no `.asmdef` files yet.

## Load-bearing rules (easy to violate)

These codify decisions in SPEC.md that conflict with Unity defaults or common tutorials. Re-read the cited section before breaking any of them.

- **Thin prefab, fat SO** (§4.25). Prefabs are structural shells; per-variant tuning lives on ScriptableObjects. `Enemy.Awake` reads `EnemyData` and applies values to its components. Don't add `[SerializeField]` fields to behavior scripts for things that belong on the SO.
- **Never hand-author `.unity` or `.prefab` YAML.** Use the prefab generators (§4.25) and the debug scene generator (§4.26). Hand-written YAML silently desyncs with Unity's fileID/GUID coordination.
- **Prefab generators have two modes**: "Create Missing Only" (safe default, skips existing) and "Regenerate All (Overwrite)" (explicit, shows a confirmation). Debug scenes are always regeneratable — hand-edits to a debug scene are lost on next regeneration by design.
- **Content levels are hand-authored in Unity; debug scenes are generator-owned** (§4.26). Don't blur the line. V1 content levels under `Scenes/Levels/` are built with Tilemap Palette + prefab drag-and-drop, not by a generator.
- **Player controller uses dynamic Rigidbody2D with `gravityScale = 0`** (§4.2). Not kinematic, not CharacterController. Manual velocity integration each `FixedUpdate`. Don't "fix" this to use Unity's built-in gravity; the chosen pattern is the Unity 6 standard for tight platformer feel.
- **The `Action` button is one button with two semantics** (§4.1): held = run; pressed = context-sensitive (fireball / cape sweep / Yoshi tongue / pickup / throw). Always call it `Action` in code and docs — never "Run", "Fire", "Attack", or "Y button".
- **Crouch is derived, not bound** — read as `Move.y < -0.5` inside `PlayerInputBinding`. No standalone `Crouch` binding.
- **Cape flight is simplified** (§4.2 and §7): ground sweep + airborne slow-fall (25% gravity while descending, jump held). No flight takeoff, no dive, no P-meter. Don't restore the full SMW flight mechanics unless scope explicitly changes.
- **Slope set is locked to SMW's two angles** (§4.5 and §7): steep 45° and shallow ~26.57° (1:2, two-tile slope). No arbitrary slopes, no ceiling slopes. `SlopeTile.GetTileData` must set `colliderType = Tile.ColliderType.Sprite` with a triangular Custom Physics Shape.
- **State lives in exactly three layers** (§4.24): Persistent (`SaveData`), Session (`GameSession` on `GameServices`), Per-attempt (`LevelRunState` on `LevelRoot`). Every mutable field must belong to one of these layers. Death = level scene reload. `SaveManager.Save()` only fires at level clear / overworld move / switch palace activation / file-select — never mid-level.
- **Audio pauses with gameplay** when `AudioListener.pause = true` (§4.16 / §4.22). The only exception is a dedicated `UiSfxChannel` (`ignoreListenerPause = true`) for the pause SFX and menu-nav cues. No ducking, no fade-out. Music stack is preserved across pause and resumes from the same playback head.
- **`GameServices` and `LevelContext` are service locators, not DI containers** (§3). Register at Boot (for `GameServices`) or `LevelContext.Begin` (for per-level). Do not introduce Zenject / VContainer — conscious choice.
- **`FindAnyObjectByType` / `FindFirstObjectByType` are banned in hot paths.** Cache references at scene load via the locator. The deprecated `FindObjectOfType` is banned entirely.
- **PrimeTween > coroutines** for time-based behavior that isn't strictly `FixedUpdate` physics. Use `useUnscaledTime: true` for UI animations that must run while `Time.timeScale = 0`.
- **Rebinding UI is out of V1 scope** (§4.1). Don't build it, don't plan for it.
- **HUD canvases** (`HUDRoot`, `TitleCanvas`, `OverworldCanvas`) all share a `CanvasScalerPresetApplier` targeting 1280×720 with `MatchWidthOrHeight` / `Match = 0.5` (§4.17). One place to change resolution assumptions.
- **Physics layers and the 2D collision matrix** (§4.19) must be set up before Phase 1 — the player controller and ground probe depend on it. Commit `ProjectSettings/DynamicsManager.asset`.

## Repository layout

Game-specific assets live under [Assets/_Project/](Assets/_Project/) (leading underscore for sort order). The full planned structure is in SPEC.md §4.25 / §4.26. Current:

- [Assets/_Project/Scripts/](Assets/_Project/Scripts/) — only `Editor/FileExtensions.cs` today. Phase 0 adds `SMW.Runtime` / `SMW.Editor` / `SMW.Tests` asmdef roots.
- [Assets/_Project/Scenes/](Assets/_Project/Scenes/) — only `Main.unity` today. Phase 0 adds `Boot`, `Systems`, `Title`, `Overworld` + the `Scenes/Debug/` subfolder. `Scenes/Levels/` follows in Phase 7.
- [Assets/_Project/Settings/](Assets/_Project/Settings/) — URP + 2D renderer + volume profile. Modify these, don't create parallel copies.
- [Assets/_Project/Input/](Assets/_Project/Input/) — `InputSystem_Actions.inputactions`. Phase 0 populates the action maps per SPEC.md §4.1 (keyboard + gamepad bindings both).

Phase 0 also creates `Assets/_Project/Prefabs/{Player,Enemies,Pickups,Blocks,Projectiles,VFX}/` and `Assets/_Project/Data/{Enemies,Pickups,Blocks,PowerStates,Levels}/`. Don't create these earlier or mirror them outside `_Project/`.

## Coding conventions (from SPEC.md §2)

- Namespaces rooted at `SMW.<Subsystem>` (`SMW.Player`, `SMW.Enemies`, `SMW.Audio`, ...).
- Inspector-driven fields: `[SerializeField] private` + property accessor. No bare `public` fields on MonoBehaviours.
- One `MonoBehaviour` per file. Data classes live as ScriptableObjects.
- Prefer composition over inheritance. Enemy variants are `EnemyData` SOs for the same `Enemy` MonoBehaviour; share `KinematicBody2D`-style helpers rather than introducing a Player/Enemy base class.

## Existing code

- [Assets/_Project/Scripts/Editor/FileExtensions.cs](Assets/_Project/Scripts/Editor/FileExtensions.cs) — `[InitializeOnLoad]` editor utility that draws file extensions next to asset names in the Project window. Editor tooling only, not part of any game system. Sourced from a public gist (link in the file header). Safe to leave as-is; unaffected by any SPEC.md decisions.
