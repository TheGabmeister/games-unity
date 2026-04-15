# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Unity 6 recreation of **Mega Man X4** (PlayStation / Saturn, 1997). Follows the same conventions as the user's other game recreations in the parent `games-unity/` monorepo (SMW, FF1, etc.): mechanical fidelity first, procedural/primitive visuals (SVG via `com.unity.vectorgraphics`), audio stubbed until assets exist.

The Claude workspace is scoped to `MegamanX4/` — `.claude/settings.json` denies reads/writes outside this folder. Do not traverse up into sibling projects.

## Tooling

- **Unity 6000.3.12f1** (URP 17.3, URP 2D Renderer). See [ProjectSettings/ProjectVersion.txt](ProjectSettings/ProjectVersion.txt).
- **Packages of note** ([Packages/manifest.json](Packages/manifest.json)):
  - `com.unity.inputsystem` — new Input System; action asset at [Assets/_Project/Input/InputSystem_Actions.inputactions](Assets/_Project/Input/InputSystem_Actions.inputactions).
  - `com.unity.vectorgraphics` — SVG importer; all visual assets are `.svg` and tessellate to sprites at import.
  - `com.kyrylokuzyk.primetween` — tweening lib (via npm scoped registry); prefer it over DOTween or coroutines for tweens.
  - `com.unity.feature.2d`, `com.unity.2d.aseprite`, `com.unity.2d.spriteshape`, `com.unity.2d.tilemap.extras`.
  - `com.unity.test-framework` — EditMode/PlayMode tests.
- **Solution:** `MegamanX4.slnx`. One assembly (`Assembly-CSharp`); no `.asmdef` files yet.

## Build / test

Unity project — no CLI build script is checked in. Open the project in the Unity Editor (`Unity 6000.3.12f1`) to play/build. Tests are run from **Window → General → Test Runner** (EditMode and PlayMode). For a CI-style run: `Unity.exe -batchmode -projectPath <path> -runTests -testPlatform {EditMode|PlayMode}`.

Generator utilities are exposed as editor menu items under **Tools/MegamanX4/** (e.g. `Generate Buster Shot Prefabs`). Run them from the menu bar; they are idempotent and overwrite existing output.

## Layout

```
Assets/_Project/
├── Input/       InputSystem_Actions.inputactions
├── Scenes/      Init.unity, Gameplay.unity
├── Scripts/
│   ├── PlayerController.cs       movement, input, sprite swap, charge shots
│   ├── BusterShot.cs             projectile runtime (Kinematic + MovePosition)
│   ├── DashSilhouetteTrail.cs    LateUpdate-driven sprite afterimage trail
│   └── Editor/
│       ├── FileExtensions.cs              Project-panel extension labels
│       └── BusterShotPrefabGenerator.cs   Menu: Tools/MegamanX4/…
├── Player/
│   ├── MegamanX.prefab                   player prefab (Rigidbody2D + Collider2D
│   │                                     + PlayerInput + PlayerController + child Visual)
│   ├── Character/  MegamanX_{Idle,Jump,Fall,Dash}.svg
│   └── Shots/      MegamanX_Shot_{Small,Semi,Full}.svg + Prefabs/
└── Settings/    URP / Renderer2D / Volume profile assets
```

## Architecture notes

### Player movement — Kinematic + swept cast

[PlayerController.cs](Assets/_Project/Scripts/PlayerController.cs) is the one source of truth for player state (movement, jumping, dashing, wall slide/jump, charge shots, sprite selection). Key choices that the next maintainer should not accidentally undo:

- **Rigidbody2D is `Kinematic` with `gravityScale = 0`**, forced in `Awake`. Don't rely on the inspector setting — the script enforces it.
- The controller maintains its own `Vector2 velocity`, applies its own `gravity`, and resolves movement via **`Rigidbody2D.Cast` swept collision** in `MoveAxis` (one axis at a time, trim travel by `skinWidth`, zero velocity axis on contact). This is the Celeste/Hollow-Knight pattern — replacing it with physics-driven Dynamic body is a regression.
- Ground + wall contact state comes from `Probe()` (short casts from the body) run at the **start** of FixedUpdate. `isGrounded` additionally gates on `velocity.y <= 0` so the first frame of a jump isn't still "grounded". `isTouchingWall` requires the player to be actively pressing into the wall.
- `int facing` (±1) is the source of truth for direction. Never read direction back from a transform's scale — the `Visual` child's scale is a rendering detail that *follows* `facing`, not the other way around.

### Visual child / sprite flip

The root GameObject never flips. Sprites live on a child `Visual` GameObject (drag into `PlayerController.visual`). Flip happens via `visual.localScale.x = ±Mathf.Abs(...)` in Update. This keeps colliders, `rb.Cast()` directions, and every other physical system at positive scale permanently.

SVG sprites are **authored facing left** and wrapped in `<g transform="translate(128 0) scale(-2 2)">` so they render facing right at 2× size. That means `facing == +1` corresponds to un-mirrored scale. If you ever author a new pose SVG, keep this wrapper convention so the existing flip math continues to work.

### Input — PlayerInput with Invoke C# Events

`PlayerInput` component on the prefab is set to **Invoke C Sharp Events** (not Send Messages / Unity Events). Pattern used throughout:

1. Cache `InputAction` references in `Awake` via `playerInput.actions["Move"]` (etc.).
2. Subscribe to `started`/`canceled` in `OnEnable`, unsubscribe symmetrically in `OnDisable`.
3. For Value-type actions (`Move`), poll each frame with `moveAction.ReadValue<Vector2>()`.

String lookups happen once at Awake. Don't switch to Send Messages for new features.

### Projectile pattern (BusterShot)

[BusterShot.cs](Assets/_Project/Scripts/BusterShot.cs) is the canonical "straight-line bullet" reference:

- Kinematic `Rigidbody2D`, `gravityScale = 0`, trigger `Collider2D`.
- **`rb.linearVelocity` does nothing on a Kinematic body** — the physics engine only reads it for collision-response math, not translation. Move via `rb.MovePosition(rb.position + ...)` in `FixedUpdate`.
- Exposes `public event Action Destroyed;` fired from `OnDestroy`, used by the player to track the 3-lemon cap without a scene scan.

### Prefab generation (allowed)

[BusterShotPrefabGenerator.cs](Assets/_Project/Scripts/Editor/BusterShotPrefabGenerator.cs) is the template pattern: temp `new GameObject`, add components, `PrefabUtility.SaveAsPrefabAsset`, `DestroyImmediate`. Use `SerializedObject` + `FindProperty` to write private `[SerializeField]` fields without exposing them publicly.

Scripted *scene* composition remains banned (see below) — prefab generators are fine.

## Authoring conventions (important)

- **Do not script scene composition.** Content scenes, debug scenes, and test level fixtures are authored by hand in the Unity editor. Editor scripts that build scenes and save them to disk are banned — they layer C# → scene YAML on top of AssetDatabase GUID timing and have historically produced hard-to-diagnose serialization bugs. Prefab generators and ScriptableObject authoring utilities are fine and encouraged. The only exception is ephemeral PlayMode test fixtures built in-memory that are torn down at teardown — never saved.
- **User prefers planning before implementation.** For any non-trivial system, produce a short plan / spec before writing code; phased roadmaps are the norm across the user's other recreations. A `SPEC.md` at the project root is the expected home for such plans once one exists.
- **No asset dependencies until explicitly added.** Procedural SVG visuals, stubbed audio (enum-keyed `SfxId`/`MusicId` resolved via a ScriptableObject catalog) until real assets land. Gameplay code should not reference `AudioClip` directly.
- **ScriptableObjects for game data** (enemy stats, weapon data, level metadata, palettes). Prefer SO-driven configuration over hard-coded constants for anything designers would tune.
- **Additive scene loading.** Boot into `Init.unity`, load `Gameplay.unity` (and future per-stage scenes) additively; persistent systems live in the bootstrap scene.

## Small gotchas worth remembering

- Renaming an `.svg` always renames its `.svg.meta` alongside it — the meta holds the GUID that prefabs reference. Use `mv src.svg dst.svg && mv src.svg.meta dst.svg.meta` (or the Unity Project panel).
- SVG sprite-sheets are not a thing with `com.unity.vectorgraphics` — one `.svg` = one `Sprite`. Author multiple poses as separate files and swap `SpriteRenderer.sprite`.
- When spawning projectiles, pass the position to `Instantiate(prefab, pos, rot)` directly — not via a setter after. Colliders activate in `Awake` before any post-instantiate method runs, so a wrong initial position can fire bogus trigger events.
