# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Singleplayer recreation of C&C: Red Alert (1996) in Unity 6 (6000.3.12f1). 2D top-down RTS using Universal Render Pipeline with the 2D Renderer, New Input System (keyboard + mouse only, no gamepad), PrimeTween, and uGUI. Target resolution 1920×1080. See `SPEC.md` for gameplay design and `IMPL.md` for implementation plan.

## Build & Run

This is a Unity project — there is no CLI build. Open in Unity 6 (6000.3.12f1+) and use the Editor to build/run. Test framework (`com.unity.test-framework`) is installed but no tests exist yet.

## Architecture

### Systems Prefab
`Assets/_Project/Prefabs/Systems.prefab` holds all global managers (InputManager, PlayerManager, MapManager, EconomyManager, PowerManager, SelectionManager, CommandManager, PlacementManager, SellRepairManager, ConstructionManager, ProductionManager, SfxManager). It is placed directly in the Gameplay scene — no Bootstrapper, no Resources folder. `SidebarCanvas.prefab` is a separate uGUI canvas also placed in the scene.

### Scene Structure
One scene in build settings:
- **Gameplay** (index 0) — `Assets/_Project/Scenes/Gameplay.unity`

### Singleton Pattern
Managers use a static `Instance` property set in `Awake`. All `Awake` calls run before any `Start`, so singletons are safe to access from `Start` onward. `Update`/`LateUpdate` methods guard with `if (SomeManager.Instance == null) return;`. Camera.main is cached in `Awake` (not called per-frame) by SelectionManager and CommandManager.

### Initialization Order
1. **Awake** — all managers set `Instance`, MapManager builds the grid and renders both terrain and ore overlay tilemaps. Entity finds Health (RequireComponent).
2. **Start** — Entity registers itself with MapManager (including multi-cell footprints for buildings) and PlayerManager, initializes Health with MaxHP from UnitData, applies sprite, repositions multi-cell buildings to center over footprint. Harvester begins SeekOre. PowerManager.Start recalculates all players.
3. **First Update** — SidebarUI defers its initial RefreshBuildGrid to the first Update frame (not Start) because Entity.Start hasn't populated OwnedEntities yet when SidebarUI.Start runs.
4. **First LateUpdate** — EconomyManager initializes starting credits (after all Start calls have run).
5. **Update** — SelectionManager polls input for selection, CommandManager polls for orders, Mover follows paths, Attacker scans for targets, Harvester runs state machine, MapManager ticks ore regrowth (2 min interval). ConstructionManager/ProductionManager tick build progress and drain credits incrementally. PlacementManager shows ghost and handles placement clicks. SellRepairManager handles sell clicks and repair ticks.
6. **LateUpdate** — RTSCamera reads input for panning and clamps to map bounds (viewport shrunk for sidebar). SelectionManager prunes destroyed units from selection. ProductionManager retries spawning READY units if exit was blocked.

### Key Design Decisions
- **Grid is king.** All gameplay (pathfinding, fog of war, building placement, targeting, selection) operates on a cell grid via MapManager. No Unity Physics2D — no Rigidbody2D, no Collider2D, no collision layers.
- **Composition over inheritance.** Units are GameObjects with mix-and-match components (Entity, Health, Mover, Attacker, Harvester, etc.), not a class hierarchy. Entity has `[RequireComponent(typeof(Health))]`. Health owns HP state and fires events; HealthBar is a purely visual subscriber on an inactive child GO.
- **ScriptableObjects for data.** All unit/building/weapon stats live in SOs (UnitData, FactionData, MapData). UnitData covers both units and buildings — buildings use `Category = Building` and building-specific fields. Runtime mutable state lives on MonoBehaviour components.
- **Editor generators** in `Assets/_Project/Scripts/Editor/Generators/` follow a pattern: GeneratorWindow (Tools > RedAlert > Generator Window) with categorized buttons (Terrain, Sprites, Data, Economy, Buildings, UI, Prefabs, Scenes) → static `Generate()` methods → temp GO → configure → SerializedObject wiring → SaveAsPrefabAsset → DestroyImmediate in finally block. "Generate All" runs everything in dependency order. This is the primary way to set up or rebuild the project.
- **SVG → PNG pipeline.** SVG source files live in `Tools/sprites/`. Inkscape exports to PNG in `Assets/_Project/Sprites/`. Unity only sees PNGs. Run `bash Tools/export_sprites.sh` to re-export all. Building sprites export at footprint-scaled sizes (e.g., 3×3 = 192×192 px).
- **No camera zoom.** Fixed orthographic size, matching the original game.
- **Sidebar shrinks viewport.** RTSCamera sets `Camera.rect` to `(0, 0, 0.85, 1)`, leaving the right 15% for the sidebar (ScreenSpaceOverlay canvas). Edge scroll ignores mouse positions past the viewport boundary.

### Project Layout
All game content goes under `Assets/_Project/`. Editor-only scripts are in `Assets/_Project/Scripts/Editor/`. URP and rendering settings are in `Assets/_Project/Settings/`.

### UnitData: Units and Buildings
UnitData SO is used for both units and buildings. Key field groups:
- **Shared**: DisplayName, Sprite, Icon, Category, Faction, Cost, MaxHP, Armor, SightRange, Prefab
- **Unit-specific**: Locomotion, BaseSpeed, PrimaryWeapon, IsCrusher, NoMovingFire, IsCrewedVehicle, ExplodesOnDeath, DeathWarhead, DeathSound, BailOutUnit
- **Building-specific**: FootprintX/Y, StorageCapacity, PowerProduced, PowerConsumed, RequiresPower, IsWall, FreeUnit, ProducesCategory, ExitCellOffset, Prerequisites

Buildings are identified by `Category == UnitCategory.Building`. Entity.IsBuilding checks this.

### Multi-Cell Buildings
Buildings with `FootprintX > 1` or `FootprintY > 1` register all cells in `Entity.RegisterCells()`. The entity grid maps each occupied cell back to the same Entity. Entity.Start repositions the transform to center the sprite over the full footprint: `(Cell.x + FootprintX * 0.5, Cell.y + FootprintY * 0.5)`. Scene generators and PlacementManager must do the same offset when placing buildings.

Building sprites are exported at `FootprintX × 64` by `FootprintY × 64` pixels. The texture importer must use `SpriteMeshType.FullRect` and `SpriteImportMode.Single` to avoid Unity auto-trimming transparent pixels.

### Data Flow: Selection → Command → Movement/Combat
1. SelectionManager tracks selected units (click, box drag, double-click, E, control groups). Click enemy = inspect health only (not added to selection).
2. CommandManager listens for right-click → converts screen position to cell via MapManager.WorldToCell
3. Context-sensitive: right-click enemy → Attacker.AttackTarget; right-click ore → Harvester.SendToOre; right-click friendly Refinery → Harvester.SendToRefinery; right-click ground → Mover.MoveTo + Attacker.ClearOrders. Modifiers: Ctrl = force fire, Alt = force move, Q = attack-move.
4. Mover calls Pathfinder.FindPath (static A*, 8-directional, octile heuristic) → walks path cell-by-cell. IsCrusher vehicles kill enemy infantry on cell enter.
5. Attacker auto-targets nearest enemy in weapon range. Fires hitscan (instant) or spawns Projectile GO. DamageSystem applies damage × warhead modifier, splash with falloff.
6. Health.TakeDamage → fires OnHealthChanged (HealthBar subscribes for visual). On death → Entity.Die handles bail-out, explode-on-death, destroy.
7. Entity.SetCell updates the MapManager entity grid as the unit crosses cell boundaries

### Data Flow: Economy & Harvesting
1. Harvester auto-seeks nearest ore cell (6-cell radius, then 48-cell far-scan) via `MapManager.FindNearestOre`.
2. On arrival, enters Harvesting state — calls `MapManager.HarvestBail` per tick, collecting up to 28 bails. Ore = $25/bail, gems = $50/bail.
3. When full (or ore depleted), finds nearest friendly Refinery via `FindNearestRefinery` (prefers unoccupied). Paths to adjacent cell, docks when within 1.5 cells.
4. Depositing state — converts bails to credits via `EconomyManager.AddCredits`, capped by storage capacity. One bail per tick. Last bail gets the cargo remainder (avoids integer division rounding loss).
5. When empty, undocks and returns to step 1.
6. MapManager tracks per-cell ore density (0–4) and type (Ore/Gems). Ore regrows every 2 min (density++, spreads to adjacent cells at max density). Gems never regrow.
7. Ore overlay rendered on a second tilemap layer (4 density sprites per resource type).
8. Storage = sum of all entities' `UnitData.StorageCapacity`. Entity.OnDestroy triggers `EconomyManager.RecalculateStorage` if the entity had storage, clamping credits to new capacity.

### Data Flow: Construction & Production
1. SidebarUI shows buildable items filtered by FactionData + prerequisites. Structures on left column, units on right.
2. Click structure icon → ConstructionManager.StartBuild → credits drain incrementally over build time (`Cost / 1000 × 0.8 min`). Cancel mid-build = lose spent credits.
3. Build complete → state = READY → PlacementManager.EnterPlacement shows ghost sprite at mouse. Green = valid, red = invalid.
4. Placement validation: within 16-cell CY radius, within 2-cell adjacency to friendly building, footprint cells clear and passable.
5. Left-click places building, right-click cancels (refunds full cost since it was READY).
6. Click unit icon → ProductionManager starts build. Build time / factory count = effective time. Credits drain incrementally.
7. Unit complete → spawns at primary factory's ExitCellOffset. If exit blocked, retries each LateUpdate.
8. PowerManager: each building produces/consumes power. `IsLowPower` = consumed > produced. Brownout disables RequiresPower buildings.
9. Sell mode: click building → refund 50% × (HP ratio) × Cost, spawn infantry if crewed.
10. Repair: click building → 7 HP per tick, drains credits at rate proportional to 20% of Cost.
11. Building damage: ≤50% HP shows fire overlay. ≤25% HP = critical (Engineer-capturable in Phase 7).

### Grid Coordinate System
- 1 cell = 1 Unity unit. Sprites are 64×64 px at 64 PPU (buildings scale to footprint × 64).
- `MapManager.CellToWorld(cell)` returns cell center at `(cell.x + 0.5, cell.y + 0.5)`.
- `MapManager.WorldToCell(world)` floors to int.
- Terrain speed lookup: `TerrainMovement.GetSpeedMultiplier(LocomotionType, TerrainType)` — static table, 0 = impassable.

## Scope

This is the RedAlert project. Do not read, reference, or borrow code from other projects in this repo or parent directories. Only use files within `c:\dev\games-unity\RedAlert\` and the reference source at `D:\CnC_Red_Alert\CODE\`.

## Conventions

- Scripts use the global namespace (no C# namespaces).
- Private fields use underscore prefix (`_health`, `_rb`). Locals stay plain. Constants and static readonly use PascalCase.
- No custom assembly definitions — everything compiles into the default assemblies.
- **KISS** — simplest thing that works
- **YAGNI** — don't build for hypothetical needs
- **DRY** — remove real duplication, not shape-similar code
- **Locality of change** — adding a new entity or feature should touch as few files as possible

## Reference Source Code

The original C&C: Red Alert source code (EA GPL release) is at `D:\CnC_Red_Alert\CODE\`. Key files for gameplay data:
- `BDATA.CPP` — building definitions, footprint sizes, occupy/overlap lists
- `UDATA.CPP` — vehicle unit definitions, locomotion types
- `IDATA.CPP` — infantry unit definitions
- `VDATA.CPP` — naval vessel definitions
- `AADATA.CPP` — aircraft definitions
- `BULLET.CPP` — projectile behavior, homing, scatter, arcs
- `COMBAT.CPP` — splash damage, damage calculation, friendly fire
- `TECHNO.CPP` — targeting, Can_Fire logic, turret rotation
- `UNIT.CPP` — fire-while-moving, harvester AI, unit state machine
- `RULES.H` / `RULES.CPP` — all configurable game rules (ore values, growth rates, build speed, etc.)
- `DEFINES.H` — enums: BSizeType, SpeedType, ArmorType, WarheadType

## Asset Pipeline

- **Sprites**: SVG source files in `Tools/sprites/` (subdirs: terrain, units, buildings, overlays, ui) exported to PNG via Inkscape. Unit sprites are 64×64 px. Building sprites are `FootprintX × 64` by `FootprintY × 64` px (e.g., 3×3 CY = 192×192). The export script `Tools/export_sprites.sh` has a `get_building_size` function mapping building names to pixel dimensions.
  - Inkscape path: `"/c/Program Files/Inkscape/bin/inkscape.exe"`
  - Batch export: `bash Tools/export_sprites.sh` (re-exports all SVGs from `Tools/sprites/` to `Assets/_Project/Sprites/`)
  - Building sprite import: must use `SpriteImportMode.Single`, `SpriteMeshType.FullRect`, `spritePixelsPerUnit = 64` to avoid auto-trimming.
  - Python: C:/Users/Admin/AppData/Local/Python/pythoncore-3.14-64/python.exe
- **Sounds**: `python Tools/generate_combat_sounds.py` generates combat .wav files to `Assets/_Project/Sounds/Combat/`. Uses synthesized waveforms (noise bursts, frequency sweeps, envelopes).
- **Music**: Python scripts in `Tools/music/` use `midiutil` to generate MIDI → FluidSynth renders with a soundfont to WAV → ffmpeg converts to OGG. Tool paths: `D:/fluidsynth-v2.5.4-win10-x64-cpp11/bin/fluidsynth.exe`, `D:/ffmpeg-8.1-essentials_build/bin/ffmpeg.exe`, soundfont `D:/GeneralUser-GS/GeneralUser-GS.sf2`
