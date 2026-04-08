# AGENTS.md

Guidance for coding agents working in this repository.

## Project Summary

This project recreates the core mechanics of Final Fantasy 1 Pixel Remaster in Unity 6 using procedural visuals.

- Read [SPEC.md](/c:/dev/games-unity/FinalFantasy/SPEC.md) first.
- Treat `SPEC.md` as the primary source of truth for mechanics, data schemas, architecture, and scope.
- Preserve the project's central constraint: no imported sprites, textures, or audio assets for gameplay content.

## Tech Stack

- Unity 6 (`6000.3.12f1`)
- URP 2D
- Input System `1.19.0`
- PrimeTween for animation and timing
- Unity Awaitable for async flows
- Newtonsoft JSON for save serialization
- Tri-Inspector for editor tooling
- uGUI for runtime UI
- TextMeshPro for runtime text
- Eflatun.SceneReference for inspector-safe scene references

## Core Rules

- Use `async Awaitable` for gameplay flows such as scene loading, dialogue, and battle sequencing.
- Do not introduce coroutine-based animation flows when PrimeTween plus Awaitable already covers the use case.
- Use uGUI, not UI Toolkit, for menus and HUD.
- Use ScriptableObjects for game content data.
- Use `Eflatun.SceneReference` in serialized data instead of raw scene name strings.
- Keep mechanics data-driven where the spec intends that: classes, items, equipment, spells, encounters, shops, progression gates.
- Keep battle formulas and effect resolution isolated and testable.

## Architecture Expectations

The intended persistent manager structure is:

```text
GameManager
|- GameStateManager
|- PartyManager
|- InventoryManager
|- ProgressionManager
|- SaveManager
|- AudioManager
|- SceneLoader
|- InputManager
|- HUD
|- MainMenu
|- DebugCanvas
|- EventSystem
\- DataRepository
```

Important project assumptions:

- Battle loads as an additive scene on top of exploration.
- Exploration state remains alive underneath battle.
- World state persistence should be stable and explicit.
- `ProgressionManager` owns progression flags and world-state gating.
- `DataRepository` is the central read access point for content databases.
- `GameManager` discovers its child managers with `GetComponentInChildren<>()`; keep persistent runtime wiring compatible with that pattern.
- `FadeOverlay` is a separate persistent singleton-style object, not a child manager under `GameManager`.

## Repo Layout

Main working area:

```text
Assets/_Project/
|- Scripts/
|  |- Core/
|  |- Battle/
|  |- Exploration/
|  |- Party/
|  |- Inventory/
|  |- Magic/
|  |- UI/
|  |- Data/
|  |- Save/
|  |- Audio/
|  |- Rendering/
|  |- Editor/
|  \- Utility/
|- Data/
|- Scenes/
|- Prefabs/
|- Input/
|- Settings/
\- Materials/
```

Prefer changing files under `Assets/_Project/`.

Avoid editing generated or third-party content unless the task explicitly requires it:

- `Library/`
- `Temp/`
- `Logs/`
- `UserSettings/`
- `Packages/PackageCache/`

## Gameplay and Content Constraints

- Match Pixel Remaster behavior where the spec calls for it.
- If exact FF1 PR behavior is uncertain, prefer data-driven implementation with tunable config.
- Preserve the 4-member party model, class upgrades, tile-based exploration, and step-counter encounters.
- Keep visuals procedural and readable rather than asset-driven.
- Stub audio APIs are allowed; missing clips should not break gameplay flows.

## UI and Input

- Menus are cursor-based and controller/keyboard first.
- Input should follow the FF1-specific action map described in `SPEC.md`.
- Keep menu navigation explicit and deterministic.
- Maintain the blue-window FF1-inspired visual language unless the user requests a redesign.
- The project uses an `InputManager` wrapper over the `Gameplay` and `UI` action maps. Prefer its `MoveAction`, `ConfirmAction`, and `CancelAction` properties instead of reading raw maps directly.
- Do not infer active input mode from `gameplayMap.enabled`. `EnableUI()` re-enables debug actions individually, so the gameplay map can look enabled even when UI mode is active.
- If a screen switches to UI input mode, do not keep polling disabled gameplay actions from code. Manual input handling must read from the currently enabled action map, or the screen must intentionally keep the required map enabled.
- `InputSystemUIInputModule` and custom menu polling must agree on which actions are active. Avoid half-switched states where gameplay is disabled but menu code still reads gameplay actions.
- Opening debug or menu UI must not leak movement/confirm inputs into exploration.
- Debug overlay and debug console are expected to remain toggleable in UI mode via the dedicated debug actions.

## Saves and Data Safety

- Save data should be serializable, versionable, and robust against corruption.
- Prefer additive, backward-compatible save changes when possible.
- If changing saved data structures, consider migration impact immediately.
- Do not silently reset progression, inventory, or world state.
- When a phase adds player-facing state such as party members, levels, spells, equipment, inventory, gil, or progression, update save/load in the same phase. Do not ship a feature that appears complete in-session but is lost on load.
- `SaveHelper.CreateSaveData()` and `SaveHelper.ApplySaveData()` are the central snapshot/restore path. Keep them in sync whenever runtime state changes.
- Keep save previews, load flows, and Continue behavior resilient to corrupt or partial files. Bad saves should be skipped or reported cleanly rather than breaking the title flow.
- Preserve true atomic write behavior on Windows. Prefer `File.Replace` or an equivalent single-step replacement over delete-then-move sequences.
- Preserve the slot model unless the user asks otherwise: 4 manual slots, 1 auto-save slot, and 1 quick-save slot. Quick saves are consumed on load; Continue should prefer the most recent valid timestamp across all slots.

## Implementation Guardrails

- Treat editor-generated scenes, assets, and build settings as source-controlled deliverables. If a setup script changes runtime scenes, commit the resulting scene and build-setting updates instead of relying on a one-off local editor action.
- Equipment/stat preview code must be side-effect free. Previewing an item must not mutate live equipment, HP/MP, or any other persistent party state.
- Party order changes must update all dependent runtime presentation immediately, including the exploration leader representation.
- `PartyManager.OnPartyChanged` is the main hook for party-dependent presentation. If party composition, ordering, stats, or leader visuals change, make sure affected UI/runtime listeners refresh immediately.
- If a phase checklist or debug command is marked implemented in the spec, do not leave it as a stub unless the user explicitly agrees to defer it.

## Scene and Setup Workflow

- `FF1 > Setup Phase 1 Scenes` regenerates the core Boot/Title/PartyCreation/Exploration scenes, rewires the persistent hierarchy, and updates Build Settings.
- If you add or rename persistent managers, always-on UI, or boot-time systems, update `SceneSetup.cs` in the same change so a regenerated project stays valid.
- The setup flow wires the shared `InputSystem_Actions.inputactions` asset into both `InputManager` and `InputSystemUIInputModule`; keep those references aligned.

## Testing and Verification

When you make gameplay or architecture changes, verify as much as the environment reasonably allows.

Useful Unity CLI examples:

```powershell
"C:/Program Files/Unity/Hub/Editor/6000.3.12f1/Editor/Unity.exe" -batchmode -projectPath . -runTests -testResults ./TestResults.xml -quit
```

```powershell
"C:/Program Files/Unity/Hub/Editor/6000.3.12f1/Editor/Unity.exe" -batchmode -projectPath . -buildTarget Win64 -quit
```

If you cannot run Unity validation locally, say so clearly in your final handoff.

## Agent Working Style

- Make focused changes that fit the existing architecture.
- Prefer extending the spec-aligned systems over inventing parallel ones.
- Keep comments brief and only where they add clarity.
- Do not rewrite unrelated systems while solving a local task.
- If you find a spec contradiction that affects implementation, flag it instead of guessing silently.

## When Spec and Code Conflict

- Follow the user's latest instruction first.
- Otherwise prefer `SPEC.md`.
- If the codebase intentionally differs from the spec, preserve the working implementation and document the mismatch in your handoff.
