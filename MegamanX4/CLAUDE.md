# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Unity 6 recreation of **Mega Man X4** (PlayStation / Saturn, 1997). Greenfield as of this writing — the Assets tree is essentially a fresh URP 2D template plus a couple of empty scenes. Follows the same conventions as the user's other game recreations in the parent `games-unity/` monorepo (SMW, FF1, etc.): mechanical fidelity first, procedural/primitive visuals, audio stubbed until assets exist.

The Claude workspace is scoped to `MegamanX4/` — `.claude/settings.json` denies reads/writes outside this folder. Do not traverse up into sibling projects.

## Tooling

- **Unity 6000.3.12f1** (URP 17.3, URP 2D Renderer). See [ProjectSettings/ProjectVersion.txt](ProjectSettings/ProjectVersion.txt).
- **Packages of note** ([Packages/manifest.json](Packages/manifest.json)):
  - `com.unity.inputsystem` — the new Input System; action map at [Assets/_Project/Input/InputSystem_Actions.inputactions](Assets/_Project/Input/InputSystem_Actions.inputactions).
  - `com.kyrylokuzyk.primetween` — tweening lib (via npm scoped registry); prefer it over DOTween or coroutines for tweens.
  - `com.unity.feature.2d`, `com.unity.2d.aseprite`, `com.unity.2d.spriteshape`, `com.unity.2d.tilemap.extras`.
  - `com.unity.test-framework` — EditMode/PlayMode tests.
- **Solution:** `MegamanX4.slnx`. Only one assembly right now (default `Assembly-CSharp` via root-level scripts); no `.asmdef` files yet.

## Build / test

Unity project — no CLI build script is checked in. Open the project in the Unity Editor (`Unity 6000.3.12f1`) to play/build. Tests are run from **Window → General → Test Runner** (EditMode and PlayMode). When an Editor build is needed from the command line, use `Unity.exe -batchmode -projectPath <path> -runTests -testPlatform {EditMode|PlayMode}` — but prefer the in-editor runner while iterating.

## Current layout

```
Assets/_Project/
├── Input/       InputSystem_Actions.inputactions
├── Scenes/      Init.unity, Gameplay.unity   (hand-authored, empty)
├── Scripts/
│   └── Editor/  FileExtensions.cs            (Project-panel extension labels)
└── Settings/    URP / Renderer2D / Volume profile assets
```

Everything else under `Assets/` that ships with Unity defaults (e.g. `TutorialInfo`) is ignorable.

## Authoring conventions (important)

- **Do not script scene composition.** Content scenes, debug scenes, and test level fixtures are authored by hand in the Unity editor. Editor scripts that build scenes and save them to disk are banned — they layer C# → scene YAML on top of AssetDatabase GUID timing and have historically produced hard-to-diagnose serialization bugs. Prefab generators and ScriptableObject authoring utilities are fine and encouraged. The only exception is ephemeral PlayMode test fixtures built in-memory (e.g. via a `SceneBuildHelpers`-style helper) that are torn down at teardown — never saved.
- **User prefers planning before implementation.** For any non-trivial system, produce a short plan / spec before writing code; phased roadmaps (Phase 0 foundation → Phase 1 player → …) are the norm across the user's other recreations. A `SPEC.md` at the project root is the expected home for such plans once one exists.
- **No asset dependencies until explicitly added.** Expect procedural primitives for visuals and a stubbed audio bus (enum-keyed `SfxId`/`MusicId` resolved through a ScriptableObject catalog) until real assets land. Gameplay code should not reference `AudioClip` directly.
- **ScriptableObjects for game data** (enemy stats, weapon data, level metadata, palettes). Prefer SO-driven configuration over hard-coded constants or MonoBehaviour inspector fields for anything designers would tune.
- **Additive scene loading.** Boot into `Init.unity`, load `Gameplay.unity` (and future per-stage scenes) additively; persistent systems live in the bootstrap scene.
