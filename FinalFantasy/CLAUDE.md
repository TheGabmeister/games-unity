# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Final Fantasy 1 (Pixel Remaster) mechanical recreation in Unity 6. Faithful game mechanics with procedural/primitive visuals — no imported sprites, textures, or audio assets. **SPEC.md is the single source of truth** for all mechanics, data schemas, architecture decisions, and implementation phases.

## Engine & Tooling

- **Unity 6** (6000.3.12f1) with **URP 2D** renderer
- **PrimeTween** for all animations — tweens are directly awaitable via `await tween`. Never use coroutines for animation.
- **Unity Awaitable** for async flows (scene loading, battle sequencing, dialogue). No UniTask.
- **Newtonsoft JSON** (3.2.2) for save system serialization (supports Dictionary, HashSet, polymorphism that JsonUtility cannot).
- **Tri-Inspector** for editor attributes (`[ShowIf]`, `[Required]`, `[ReadOnly]`, etc.). Not NaughtyAttributes (broken on Unity 6).
- **Eflatun.SceneReference** for type-safe scene references in the Inspector. Never use raw string scene names in ScriptableObjects.
- **uGUI** for all UI. Not UI Toolkit.
- **Input System** 1.19.0 with `InputSystemUIInputModule` for menu navigation. No third-party input plugins.

## Architecture

Singleton-based manager hierarchy under `GameManager` (DontDestroyOnLoad):

```
GameManager
├── GameStateManager        — Top-level FSM (Title, Exploration, Battle, Menu, Dialogue, Shop)
├── PartyManager            — Party data, formation, class state, stat computation
├── InventoryManager        — Consumable items, equipment, Gil
├── ProgressionManager      — Story flags (HashSet<string>), key items, world state (PersistentID lookups)
├── SaveManager             — JSON serialization, 4 manual + 1 auto + 1 quick save slot
├── AudioManager            — Stub with full API (enum → null AudioClip, logs on call)
├── SceneLoader             — Async scene loading with fade transitions
└── DataRepository          — Central access to all ScriptableObject databases
```

**Battle scenes load additively** on top of the exploration scene. The exploration scene is not unloaded — it pauses underneath and resumes after battle. Each scene has its own camera.

**All game data is ScriptableObjects** stored in `Assets/_Project/Data/`. Classes, enemies, spells, equipment, items, encounter tables, shops — everything is an SO instance.

**World state persistence** uses `PersistentID` MonoBehaviours on scene objects (chests, boss triggers, locked doors). Each auto-generates a GUID; the save system maps `PersistentID.ID → bool`.

## Folder Structure

```
Assets/_Project/
├── Scripts/{Core,Battle,Exploration,Party,Inventory,Magic,UI,Data,Save,Audio,Rendering,Utility}/
├── Data/{Classes,Enemies,Items,Equipment,Spells,Encounters,Maps,Shops,Dialogue}/
├── Scenes/
├── Prefabs/
├── Input/          — InputSystem action maps
├── Settings/       — URP renderer, volume profiles
└── Materials/      — Procedural materials/shaders
```

## Key Patterns

- **Battle action resolution:** Command pattern with a pre-execution validator chain (ActorAliveCheck → StatusCheck → ConfuseOverride → SilenceCheck → TargetValidation), then pure-function damage/healing calculators that return `BattleResult` structs, then apply + animate.
- **Menu navigation:** Built on uGUI Selectables with explicit navigation. Three tiers: raw Selectables for simple menus, reusable `CursorList` for dynamic lists, custom handler for battle target selection.
- **Progression gating:** `ProgressionManager` owns both flags and key items. Systems check flags, not key item lists. Key items set their flag on acquisition; consumed items leave the flag.
- **Dialogue:** Centralized `DialogueDatabase` SOs (one per region), not inline on NPC objects. NPCs store only their `NPCID` + database reference. Variants are flag-priority-ordered, evaluated top-to-bottom, first match wins.
- **Procedural visuals:** All tiles, characters, and enemies are colored geometric primitives. Colors and shapes defined in ScriptableObjects for easy theming.
- **Scene references:** Always use `Eflatun.SceneReference` in SOs and MonoBehaviours. Never store scene names as strings — they break on rename.

## Build & Test

No custom build scripts yet. Use Unity Editor (File → Build and Run) or:

```bash
# Unity CLI build (adjust path to your Unity installation)
"C:/Program Files/Unity/Hub/Editor/6000.3.12f1/Editor/Unity.exe" -batchmode -projectPath . -buildTarget Win64 -quit

# Run tests via Unity Test Runner CLI
"C:/Program Files/Unity/Hub/Editor/6000.3.12f1/Editor/Unity.exe" -batchmode -projectPath . -runTests -testResults ./TestResults.xml -quit
```

Tests go in `Assets/_Project/Tests/` with a dedicated `.asmdef` referencing the modules under test.

**Testing constraints:** Unity locks the project to one editor instance. If the user has the Editor open, CLI test/build commands will fail. Edit Mode tests (pure C# logic — damage formulas, stat calculation, turn ordering, save serialization) can run in batchmode without a game window. Play Mode tests requiring scene loading and rendering need the Editor.

## Code Conventions

- C# with Unity 6 / .NET Standard 2.1 APIs
- `async Awaitable` for async methods, not `async Task` or coroutines
- ScriptableObjects use `[CreateAssetMenu]` with clear `menuName` paths
- Equipment/class compatibility uses `WeaponType`/`ArmorType` enums + per-class whitelists
- Elemental interactions and status effects are data-driven, not hardcoded switch blocks
- Debug overlay (F1) and debug console (backtick) available in play mode — see SPEC.md Section 10 for commands per phase
- `PersistentID` on every scene object with persistent state (chests, boss triggers, doors). Auto-generates GUID in `OnValidate()`. Editor script detects duplicates from copy/paste.
