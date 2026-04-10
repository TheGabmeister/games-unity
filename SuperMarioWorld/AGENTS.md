# AGENTS.md

## Project Snapshot

- Engine: Unity 6 `6000.3.12f1`
- Root solution: `SuperMarioWorld.slnx`
- Render pipeline: Universal Render Pipeline (2D)
- Input: Unity Input System
- Current custom code footprint is small and centered in `Assets/_Project`

## Source Layout

- `Assets/_Project/Scenes/Main.unity`: main scene
- `Assets/_Project/Scripts/Editor/FileExtensions.cs`: editor-only utility that decorates Project window file names
- `Assets/_Project/Input/InputSystem_Actions.inputactions`: input action asset
- `Assets/_Project/Settings/`: URP and rendering assets
- `Packages/manifest.json`: package dependencies
- `ProjectSettings/`: Unity project configuration

## Working Agreements

- Treat `Assets/_Project` as the primary place for project-authored content unless the task clearly belongs elsewhere.
- Keep editor-only code under `Assets/_Project/Scripts/Editor`.
- Runtime scripts should live under `Assets/_Project/Scripts` outside the `Editor` folder.
- Preserve Unity `.meta` files. When adding, moving, renaming, or deleting assets, make the matching `.meta` change too.
- Do not hand-edit generated folders such as `Library`, `Logs`, `Temp`, or `UserSettings`.
- Avoid broad edits to scene or asset YAML unless the task requires it; these files are easy to destabilize accidentally.

## Dependencies In Use

From `Packages/manifest.json`, the project currently depends on:

- `com.unity.render-pipelines.universal`
- `com.unity.inputsystem`
- `com.unity.feature.2d`
- `com.unity.ugui`
- `com.unity.timeline`
- `com.unity.test-framework`
- `com.kyrylokuzyk.primetween`
- `com.eflatun.scenereference`

## Editing Notes

- There are no project asmdefs under `Assets/_Project` right now, so scripts compile into Unity's default assemblies.
- Follow the existing C# style in touched files: `var` where obvious, braces on new lines, concise comments only when helpful.
- Prefer minimal, targeted changes over project-wide cleanup.
- Check for a dirty worktree before editing and avoid overwriting unrelated user changes.

## Validation

- If code changes are made, prefer validating in the Unity Editor for compile errors and asset import issues.
- If automated tests are added later, document how to run them here.
- If you cannot run Unity in the current environment, state that clearly in your handoff.
