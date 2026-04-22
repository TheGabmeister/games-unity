# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repo purpose

Two Unity project templates (2D and 3D) used as the starting point for personal game projects, plus in-progress design docs for a gameplay-focused Unity recreation of Digimon World 1 built on top of the 3D template.

- `UnityTemplate2D/` and `UnityTemplate3D/` — mirror structures, differ only in packages (2D has `com.unity.feature.2d`; 3D has `com.unity.ai.navigation`).
- `DigimonWorld1.md` — phased roadmap for the Digimon World 1 recreation (6 phases, Phase 0 = Foundation).
- `SPEC_PHASE_00.md` — current phase spec with naming conventions, service list, and acceptance checklist. Treat this as the source of truth for Phase 0 architecture decisions.

## Unity version

**6000.3.12f1** (Unity 6). Both templates are pinned to this version via `ProjectSettings/ProjectVersion.txt`. Don't upgrade casually — package compatibility matters.

## Running, building, testing

There is no CLI build pipeline. Work happens inside the Unity Editor:

- **Open a template:** open `UnityTemplate2D` or `UnityTemplate3D` as a project in Unity Hub.
- **Run:** press Play in the editor. The first scene should be `Init.unity` (see bootstrap pattern below).
- **Tests:** Unity Test Framework (`com.unity.test-framework` 1.6.0) via `Window → General → Test Runner`. Edit Mode and Play Mode both available. No CI configured.
- **Linting/formatting:** none configured. Follow the naming conventions in `SPEC_PHASE_00.md`.

## Architecture: the bootstrap pattern

Both templates use a "global init via prefab" pattern instead of initialising systems in the first scene:

- `Assets/_Project/Scripts/Editor/Bootstrapper.cs` runs at `RuntimeInitializeLoadType.BeforeSceneLoad` and instantiates `Resources/Systems.prefab` with `DontDestroyOnLoad`. The `Systems` prefab is where long-lived services (input, audio, UI, scene loader, etc.) should live.
- This means **any scene can be the Play-mode entry point** during iteration — Bootstrapper guarantees `Systems` exists before any scene `Awake` runs.
- `Bootstrapper.cs` is currently under `Scripts/Editor/` but is a runtime script (not editor-only). Moving it out of `Editor/` is on the cleanup list when Phase 0 implementation begins.

References in the source: https://low-scope.com/unity-tips-1-dont-use-your-first-scene-for-global-script-initialization/

## Project layout (inside each template)

```
Assets/_Project/
  Input/       # InputSystem_Actions.inputactions
  Scenes/      # Init, Game/Gameplay, UI (3D only)
  Scripts/
    Editor/    # Bootstrapper, DisableDomainReload, FileExtensions
  Settings/    # URP render pipeline + renderer assets
Assets/Resources/
  Systems.prefab  # 3D only — holds long-lived services
```

The 2D template does not yet have a `Resources/Systems.prefab`; add one if you bring the bootstrap pattern over.

## Key packages

- **Input System** (`com.unity.inputsystem`) — new input system is the standard; do not use the legacy `Input` class.
- **URP** (`com.unity.render-pipelines.universal`) — both templates use it.
- **PrimeTween** (`com.kyrylokuzyk.primetween`) — preferred tweening library; use it instead of coroutine-based tweens.
- **Unity Toolbar Extender** — used by `DisableDomainReload.cs` to add a toolbar toggle.
- **Timeline, UGUI, Test Framework** — standard.
- **AI Navigation** (3D only), **feature.2d** (2D only).

## Digimon World 1 work (in progress)

If asked to work on the Digimon World 1 recreation:

- Check `DigimonWorld1.md` for the phase and scope.
- Check `SPEC_PHASE_00.md` for naming conventions, service locator pattern, and the list of Phase 0 systems (Bootstrap + Service Locator, Input) plus the `Bootstrapper` / `EditorBootstrapLoader` / `ServiceLocator` core scripts. Other infrastructure (Audio, UI framework, Debug tools, Scene/Zone loader) is intentionally deferred to the phase that first needs it — see the Implementation Order in `DigimonWorld1.md`.
- Namespace: `DigimonWorld.<Subsystem>`. Services implement `I<Name>Service`. Scene files for zones use the `Zone_` prefix.
- Services are accessed via service locator; `Bootstrapper` runs at Script Execution Order -1000 so `Game.I.<Service>` is safe from `Awake`.
- The Digimon World 1 implementation targets the 3D template (needs AI Navigation).

## Git

- Single remote: `TheGabmeister/Unity-Templates`.
- Default branch: `main`.
- `.gitattributes` sets `* text=auto` for LF normalisation. Unity meta files are tracked — don't delete them.
- `ignore.conf` inside each template mirrors the standard Unity gitignore; actual `.gitignore` is at the repo root level if present (not currently).

## Coding principles

- **KISS** — simplest thing that works. No clever patterns where a plain `if` does the job. If a class is under 50 lines and clear, don't split it.
- **YAGNI** — don't build for hypothetical needs. No interfaces with one implementation, no config knobs with one value, no abstraction layers "for later." Write what this phase needs; refactor when a second use case actually shows up.
- **DRY** — remove real duplication, not shape-similar code. Three copies of the same logic → extract. Two functions that happen to both take a `Vector3` → leave alone. Wrong abstraction costs more than repetition.

When in doubt, lean KISS over DRY. A bit of repetition is cheaper to read and change than the wrong shared helper.

## Things not in the repo (yet)

- No CI / GitHub Actions.
- No `.editorconfig`, no formatter config.
- No asmdefs — all scripts compile into the default assemblies. Add asmdefs per-subsystem when Phase 0 implementation lands.
- No unit tests written yet.
