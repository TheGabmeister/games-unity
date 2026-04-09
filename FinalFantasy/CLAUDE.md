# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Final Fantasy 1 (Pixel Remaster) mechanical recreation in Unity 6. Faithful game mechanics with procedural/primitive visuals — no imported sprites, textures, or audio assets. **SPEC.md is the single source of truth** for all mechanics, data schemas, architecture decisions, and implementation phases (Sections 1-10). When code conflicts with SPEC.md, follow the user's latest instruction first, then SPEC.md.

**Current status:** Phase 3 (Battle System) is implemented. Core battle loop, damage formulas, enemy AI, turn ordering, command menus, victory/defeat/flee, and debug commands are functional. Phase 4 (World Building) is next.

## Engine & Tooling

- **Unity 6** (6000.3.12f1) with **URP 2D** renderer
- **PrimeTween** for all animations — tweens are directly awaitable via `await tween`. Never use coroutines for animation.
- **Unity Awaitable** for async flows (scene loading, battle sequencing, dialogue). No UniTask. No `async Task`.
- **Newtonsoft JSON** (3.2.2) for save system serialization (supports Dictionary, HashSet, polymorphism that JsonUtility cannot).
- **Tri-Inspector** for editor attributes (`[ShowIf]`, `[Required]`, `[ReadOnly]`, etc.). Not NaughtyAttributes (broken on Unity 6).
- **Eflatun.SceneReference** for type-safe scene references in the Inspector. Never use raw string scene names in ScriptableObjects.
- **uGUI** for all UI. Not UI Toolkit.
- **Input System** 1.19.0 with `InputSystemUIInputModule` for menu navigation. No third-party input plugins. Old `UnityEngine.Input` API is disabled — use `Keyboard.current` or InputAction for direct key checks.

## Architecture

Singleton-based manager hierarchy under `GameManager` (DontDestroyOnLoad). All managers are child GameObjects discovered via `GetComponentInChildren<>()` in Awake:

```
GameManager
├── GameStateManager        — Top-level FSM (Title, Exploration, Battle, Menu, Dialogue, Shop)
├── PartyManager            — Party data, formation, class state, stat computation
├── InventoryManager        — Consumable items, equipment, Gil (cap 999,999)
├── ProgressionManager      — Story flags (HashSet<string>), key items, world state
├── SaveManager             — JSON serialization, 4 manual + 1 auto + 1 quick save slot
├── AudioManager            — Stub with full API (enum → Debug.Log on call)
├── SceneLoader             — Async scene loading with fade transitions
├── InputManager            — Wraps InputActionAsset, swaps Gameplay/UI action maps
├── DataRepository          — Central access to all ScriptableObject databases (stub)
├── BattleManager           — Battle lifecycle, turn execution, auto-battle (static Instance)
├── HUD                     — Gil display overlay (HudUI)
├── MainMenu                — Pause menu + sub-screens (MainMenuUI)
├── DebugCanvas             — DebugOverlay (F1) + DebugConsole (backtick)
└── EventSystem             — InputSystemUIInputModule
```

Also DontDestroyOnLoad: **FadeOverlay** (separate singleton, sort order 999).

### Input Routing

`InputManager` has two action maps: **Gameplay** (Move, Confirm, Cancel, Menu, Run, DebugOverlay, DebugConsole) and **UI** (Navigate, Submit, Cancel, Point, Click). The properties `MoveAction`, `ConfirmAction`, `CancelAction` auto-resolve based on an internal `uiMode` flag. `RunAction` and `MenuAction` are gameplay-only. Call `EnableGameplay()` or `EnableUI()` to switch maps.

**Critical gotcha:** `EnableUI()` re-enables `DebugOverlayAction` and `DebugConsoleAction` individually so F1/backtick work in menus. This makes `gameplayMap.enabled` return `true` even though the map was disabled. Never check `gameplayMap.enabled` to determine which map is active — use the `uiMode` flag instead. The auto-resolving properties (`MoveAction`, `ConfirmAction`, `CancelAction`) already handle this correctly.

**Key bindings** (from `InputSystem_Actions.inputactions`):
- Move/Navigate: WASD, Arrow keys, Gamepad dpad/stick
- Confirm/Submit: Z, Enter, Gamepad South
- Cancel: X, Backspace, Escape, Gamepad East
- Menu (gameplay-only): Escape, C, Gamepad Start
- Run: Left Shift, Gamepad Right Trigger
- Debug Overlay: F1 | Debug Console: Backtick

### Scene Flow

```
Boot → Title → PartyCreation → Exploration ⇄ Battle (additive)
                                 ↕  ↕  ↕
                               Menu Dialogue Shop
```

Battle scenes load additively on top of exploration. The exploration scene is not unloaded — it stays underneath and resumes after battle. Each scene has its own camera. **Spatial separation:** Battle objects are placed at Y=100 in world space so the battle camera (at Y=100) can't see the exploration scene (near Y=0) and vice versa. No layers or object hiding needed.

### Stat System

`PartyMember` (plain serializable class, not MonoBehaviour) stores base stats accumulated from level-ups. Computed stats = base + all equipment bonuses, recalculated via `RecalculateStats()`. Key formulas: `HitCount = Accuracy/32 + 1`, `MagicEvasion = Agility/4`, `Attack = Strength/2 + weapon.Attack`. `PreviewEquip()` is pure — it computes hypothetical stats without mutating live state.

## Folder Structure

```
Assets/_Project/
├── Scripts/{Core,Battle,Exploration,Party,Inventory,Magic,UI,Data,Save,Audio,Rendering,Utility,Editor}/
├── Data/                — ScriptableObject asset instances
├── Scenes/              — Boot, Title, PartyCreation, Exploration, Battle
├── Prefabs/
├── Input/               — InputSystem_Actions.inputactions (Gameplay + UI maps)
├── Settings/            — URP renderer, volume profiles
└── Materials/           — Procedural materials/shaders
```

Editor scripts go in `Scripts/Editor/` — these are excluded from builds.

## Key Patterns

- **All game data is ScriptableObjects** with `[CreateAssetMenu]`: ClassDefinition, EquipmentData, ItemData, SpellData, KeyItemData, TilePalette. Test instances created at runtime via `ScriptableObject.CreateInstance<>()` when no asset is assigned.
- **UI built programmatically** at runtime. `UIWindow` adds the FF1 blue window styling (dark blue `#000044` + white Outline border). TMPro for all text. No prefab dependencies for menus. Use only ASCII characters in UI text (LiberationSans SDF lacks Unicode arrows/symbols like ▶◀— — use `>`, `<`, `-` instead).
- **Menu sub-screens** implement `IMenuSubScreen` (Initialize, Show, Hide) and are managed by `MainMenuUI`. Sub-screens are added as components on the MainMenu GameObject at runtime (`AddComponent<>()`). Each screen handles its own input polling in Update, guarded by `rootPanel.activeSelf`. `Show()` receives an `Action onClose` callback that MainMenuUI uses to restore state when the sub-screen exits.
- **Progression gating:** `ProgressionManager` owns both flags and key items. Systems check flags, not key item lists. Key items set their flag on acquisition; consumed items leave the flag.
- **Save system:** `SaveHelper.CreateSaveData()` snapshots all live state; `SaveHelper.ApplySaveData()` restores it. Atomic writes via `File.Replace()`. Equipment/items saved by asset name (string).
- **Procedural visuals:** `ProceduralTileFactory` generates 16x16 pixel Texture2D sprites at runtime. Player is a colored circle matching party leader's class color.
- **Debug console commands** provide test coverage for systems before content exists. Commands that reference game data (equipment, items, spells, enemies) first search loaded assets via `Resources.FindObjectsOfTypeAll<>()`, then fall back to creating runtime test instances via `ScriptableObject.CreateInstance<>()` with stats inferred from name keywords.
  - Phase 1: `help`, `state`, `pos`, `save`, `load`, `scene`
  - Phase 2: `setlevel`, `addgil`, `levelup`, `learnspell`, `addequip`, `additem`
  - Phase 3: `encounter <name> [count]`, `kill`, `godmode`, `setstat <slot> <stat> <value>`, `inflict <slot> <status>`, `cure <slot>`, `nobattles`
- **Starter weapons** are created at runtime in `PartyCreationUI.GetDefaultClasses()` and auto-equipped by `PartyMember.Create()`. Each class except Monk gets a weapon (Warrior: Broadsword, Thief: Dagger, Red Mage: Rapier, White/Black Mage: Staff).

### Battle System

- **`BattleActor`** wraps either a `PartyMember` or `EnemyData` with runtime HP/status/buffs. Stat properties resolve from backing data + buff state. `SyncToPartyMember()` writes battle state back to the underlying PartyMember.
- **`BattleManager`** orchestrates the full battle lifecycle as an `async Awaitable` loop: command input → agility-sorted execution → end-of-turn effects → repeat. Phases: Inactive, Starting, CommandInput, Executing, Victory, Defeat, Fleeing.
- **`DamageCalculator`** is static — implements all FF1 formulas: multi-hit physical (per-hit crit/hit rolls), magical damage, healing, elemental modifiers (weak/resist/null/absorb), undead healing inversion, buff/debuff resolution, flee rolls.
- **`TurnSystem`** sorts actions by agility with random tiebreak. Flee always resolves first; Defend gets max priority.
- **`EnemyAI`** uses weighted random action selection from `EnemyData.Actions[]`. Supports boss scripted patterns via `EveryNTurns` field.
- **`EncounterSystem`** (in Exploration scene) decrements a step counter per tile move. Creates a default encounter table at runtime if none is assigned.
- **`BattleSceneSetup`** (in Battle scene) creates the battle camera, procedural background, and positions enemy/party visuals. All positioned at `BattleYOffset = 100f` for spatial separation from exploration.
- **`BattleUI`** handles command menu, spell/item/Use submenus, target selection, damage popups, and battle log. All built programmatically like other UI.
- **`BattleConfig`** ScriptableObject holds tunable constants (hit chances, elemental multipliers, buff values, flee formula). Falls back to runtime defaults if no asset exists.

## Build & Test

No custom build scripts yet. Use Unity Editor (File → Build and Run) or:

```bash
# Unity CLI build (adjust path to your Unity installation)
"C:/Program Files/Unity/Hub/Editor/6000.3.12f1/Editor/Unity.exe" -batchmode -projectPath . -buildTarget Win64 -quit

# Run tests via Unity Test Runner CLI
"C:/Program Files/Unity/Hub/Editor/6000.3.12f1/Editor/Unity.exe" -batchmode -projectPath . -runTests -testResults ./TestResults.xml -quit
```

**Scene setup:** Run menu item **FF1 > Setup Phase 1 Scenes** (in `SceneSetup.cs`) to regenerate Boot/Title/PartyCreation/Exploration/Battle scenes with all managers wired. Must re-run after adding new managers to GameManager.

**Testing constraints:** Unity locks the project to one editor instance. If the user has the Editor open, CLI test/build commands will fail.

## Code Conventions

- C# with Unity 6 / .NET Standard 2.1 APIs. No namespaces (global namespace).
- `async Awaitable` for async methods, not `async Task` or coroutines
- `[SerializeField]` for inspector fields, `[Header("Section")]` for organization
- ScriptableObjects use `[CreateAssetMenu]` with clear `menuName` paths under "FF1/"
- Equipment/class compatibility uses `WeaponType`/`ArmorType` enums + per-class whitelists
- Elemental interactions and status effects are data-driven (flags enums, not switch blocks)
- `PersistentID` on every scene object with persistent state. Auto-generates GUID in `OnValidate()`. Editor script `PersistentIDValidator` detects duplicates from copy/paste.
