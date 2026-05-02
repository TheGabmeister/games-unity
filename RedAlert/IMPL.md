# IMPL.md — Implementation Plan

High-level implementation plan for the Unity recreation of C&C: Red Alert. **Singleplayer only** (skirmish vs AI + campaigns). See SPEC.md for all gameplay data and numeric values.

---

## Table of Contents

- [Guiding Principles](#guiding-principles)
- [Player State](#player-state)
- [Scene Layout](#scene-layout)
- [Data Architecture](#data-architecture)
- [Grid & Map System](#grid--map-system)
- [Camera](#camera)
- [Input](#input)
- [Units](#units)
- [Pathfinding](#pathfinding)
- [Combat](#combat)
- [Buildings](#buildings)
- [Construction & Production](#construction--production)
- [Economy & Harvesting](#economy--harvesting)
- [Tech Tree](#tech-tree)
- [Fog of War](#fog-of-war)
- [Power System](#power-system)
- [AI Opponent](#ai-opponent)
- [Top Bar](#top-bar)
- [Sidebar UI](#sidebar-ui)
- [Minimap](#minimap)
- [Campaign System](#campaign-system)
- [Audio](#audio)
- [Visuals & Sprites](#visuals--sprites)
- [Implementation Phases](#implementation-phases)

---

## Guiding Principles

- **One source of truth per stat.** Every unit/building stat lives in a ScriptableObject. No magic numbers in MonoBehaviours.
- **Composition over inheritance.** Units are GameObjects with mix-and-match components (Health, Weapon, Mover, Harvester, etc.), not a deep class hierarchy.
- **Systems prefab.** Global managers (GameManager, SelectionManager, FogOfWar, etc.) live on the `Resources/Systems` prefab, instantiated by the Bootstrapper before any scene loads.
- **Grid is king.** The map is a cell grid. Pathfinding, fog of war, building placement, ore fields, and terrain speed modifiers all operate on this grid.
- **Singleplayer only.** No netcode, no lobby, no host/client split. The human is always Player 0; AI opponents are Player 1–5.
- **Source code as reference.** The original C&C: Red Alert source is at `D:\CnC_Red_Alert\CODE\`. Grep it for exact values rather than hardcoding from memory.

---

## Player State

Each player (human + AI) is represented by a `PlayerState` managed by a `PlayerManager` on the Systems prefab.

```csharp
public class PlayerState
{
    public int PlayerIndex;        // 0 = human, 1–5 = AI
    public Faction Faction;
    public Country Country;        // for skirmish bonuses
    public int Credits;
    public int StorageCapacity;
    public int PowerProduced;
    public int PowerConsumed;
    public List<Entity> OwnedEntities;
    public List<BuildingData> OwnedBuildingTypes; // for prerequisite checks
}
```

The `PlayerManager` holds the array of `PlayerState` and provides lookups used by EconomyManager, PowerManager, tech tree checks, and fog of war.

---

## Scene Layout

**Init** (build index 0) — Main menu, campaign select, skirmish setup. Loads Gameplay scene when the player starts a match.

**Gameplay** (build index 1) — The RTS match. Contains the Tilemap, Camera rig, UI Canvas, and per-match managers. Cleaned up and reloaded for each new match.

The `Systems` prefab (loaded by Bootstrapper) persists across both scenes via `DontDestroyOnLoad` and holds global managers that don't need per-match state.

---

## Data Architecture

All unit/building/weapon stats are **ScriptableObjects** (SOs). Runtime instances never modify the SO — mutable state lives on MonoBehaviour components.

### Core SOs

```
Assets/_Project/Data/
  Units/
    RifleInfantry.asset      // UnitData SO
    HeavyTank.asset
    ...
  Buildings/
    PowerPlant.asset          // BuildingData SO
    TeslaCoil.asset
    ...
  Weapons/
    M1Carbine.asset           // WeaponData SO
    Cannon120mm.asset
    ...
  Warheads/
    SmallArms.asset           // WarheadData SO
    ArmorPiercing.asset
    ...
  Factions/
    Allied.asset              // FactionData SO — tech tree, available units/buildings
    Soviet.asset
  Terrain/
    TerrainType_Clear.asset   // TerrainData SO — speed modifiers per locomotion
    TerrainType_Road.asset
    ...
```

### UnitData SO

```csharp
[CreateAssetMenu(menuName = "RA/Unit")]
public class UnitData : ScriptableObject
{
    public string DisplayName;
    public Faction Faction;        // Allied, Soviet, Both
    public UnitCategory Category;  // Infantry, Vehicle, Naval, Aircraft
    public int MaxHealth;
    public ArmorType Armor;        // None, Wood, Light, Heavy, Concrete
    public float Speed;
    public int Cost;
    public int SightRange;
    public int ROT;                // body/turret rotation speed (higher = faster)
    public bool HasTurret;
    public WeaponData PrimaryWeapon;
    public WeaponData SecondaryWeapon;
    public LocomotionType Locomotion; // Foot, Tracked, Wheeled, Float, Fly
    public BuildingData[] Prerequisites;
    public int PassengerCapacity;  // 0 for non-transports
    public int Ammo;               // aircraft ammo, 0 = unlimited
    public bool CanCrush;          // can crush infantry (not all tracked can)
    public bool NoMovingFire;      // V2, Artillery — must stop to fire
    public bool SelfHeals;
    public float SelfHealThreshold; // e.g., 0.5 for Mammoth (heals to 50%)
    public bool Crewed;            // spawns infantry on death
    public bool ExplodesOnDeath;
    public bool HasSensors;        // detects subs (Destroyer, Gunboat, Cruiser)
    public bool Cloakable;         // submarine, camo pillbox
    public Sprite Icon;
    public GameObject Prefab;
}
```

### WeaponData SO

```csharp
[CreateAssetMenu(menuName = "RA/Weapon")]
public class WeaponData : ScriptableObject
{
    public int Damage;
    public int RateOfFire;        // lower = faster
    public float Range;           // in cells
    public WarheadData Warhead;
    public int BurstCount;        // shots per attack cycle
    public ProjectileData Projectile;
}
```

### ProjectileData SO

```csharp
[CreateAssetMenu(menuName = "RA/Projectile")]
public class ProjectileData : ScriptableObject
{
    public ProjectileType Type;   // Invisible, Cannon, HeatSeeker, Ballistic, Torpedo, etc.
    public float Speed;           // 0 = hitscan
    public int HomingROT;         // 0 = no homing, 5 = slow tracking, 20 = fast tracking
    public bool Inaccurate;       // applies scatter on launch
    public float MaxScatter;      // max scatter in cells (2.0 for homing, 1.0 for ballistic)
    public bool AntiAir;
    public bool AntiGround;
    public bool AntiSub;          // ASW (depth charges, torpedoes)
    public bool Arcing;           // ballistic arc (gravity = 3)
}
```

### WarheadData SO

```csharp
[CreateAssetMenu(menuName = "RA/Warhead")]
public class WarheadData : ScriptableObject
{
    [Range(0f, 1f)] public float VsNone;     // infantry
    [Range(0f, 1f)] public float VsWood;     // buildings
    [Range(0f, 1f)] public float VsLight;
    [Range(0f, 1f)] public float VsHeavy;
    [Range(0f, 1f)] public float VsConcrete;
    public int Spread;         // splash falloff (0=none, 3=small, 6=wide, 8=widest)
    public bool DamagesWalls;  // only HE, AP, Nuke can damage walls
    public bool DestroysOre;   // nuke warhead destroys ore fields
}
```

### BuildingData SO

```csharp
[CreateAssetMenu(menuName = "RA/Building")]
public class BuildingData : ScriptableObject
{
    public string DisplayName;
    public Faction Faction;
    public int MaxHealth;
    public ArmorType Armor;
    public int Cost;
    public int PowerProduced;  // positive = generates, negative = consumes
    public int StorageCapacity;
    public BuildingData[] Prerequisites;
    public WeaponData Weapon;  // null for non-defensive buildings
    public float WeaponRotationSpeed;
    public bool RequiresPower; // goes offline when base power is insufficient
    public bool Capturable;
    public bool WaterBound;    // Naval Yard, Sub Pen
    public Vector2Int FootprintSize; // e.g., 3x2 cells
    public bool HasBib;        // concrete foundation pad extending 1 row below
    public bool Crewed;        // spawns infantry when sold/destroyed
    public Sprite Icon;
    public GameObject Prefab;
    public UnitData FreeUnit;  // e.g., Ore Refinery gives free Ore Truck
}
```

---

## Grid & Map System

The map is a 2D cell grid backed by Unity's **Tilemap**. One cell = one game tile (24×24 px at original scale; we'll use 64×64 px sprite cells to match our sprite pipeline).

### Map Authoring

Maps are **hand-painted** in Unity's Tile Palette editor. An editor window (`Tools/RedAlert/Generator Window`) provides buttons for generating prefabs, SOs, and scene scaffolding — following the same pattern as `Assets/_Project/Scripts/Editor/Generators/` from previous projects:

- `PrefabGeneratorUtils.cs` — shared helpers: `SavePrefab()`, `EnsureFolder()`, `CreateCanvasRoot()`, `CreatePanel()`.
- `GeneratorWindow.cs` — single `EditorWindow` with categorized buttons (Data, Prefabs, Scenes). Each button calls a static `Generate()` method. Has a "Generate All" that runs them in dependency order.
- Generator classes (e.g., `GenerateUnitPrefab.cs`) — each follows the pattern: create temp GO → configure components → wire references via `SerializedObject` → `PrefabUtility.SaveAsPrefabAsset()` → `DestroyImmediate()` in `finally` block.
- Scene generators use `EditorSceneManager.NewScene()` → instantiate prefabs → wire cross-references → save.

### Layers

Multiple Tilemaps stacked on one Grid:

| Tilemap Layer | Purpose |
|---|---|
| Terrain | Ground tiles (grass, road, rough, sand, water, ore, gems) |
| Buildings | Building footprints (occupancy) |
| Fog | Fog of war overlay |
| UI Overlay | Build-placement ghost, selection highlights |

### MapManager

A component on the Systems prefab that owns the grid data:

```csharp
public class MapManager : MonoBehaviour
{
    public int Width;
    public int Height;

    // Per-cell data, indexed by [x + y * Width]
    private TerrainData[] _terrain;
    private int[] _oreDensity;      // 0 = empty, 1–4 density levels
    private int[] _gemDensity;
    private Entity[] _occupant;     // unit or building occupying the cell, null if empty
    private byte[,] _fogState;     // [cellIndex, playerIndex]: 0=shroud, 1=fog, 2=visible

    public TerrainData GetTerrain(Vector2Int cell) { ... }
    public bool IsPassable(Vector2Int cell, LocomotionType loco) { ... }
    public float GetSpeedModifier(Vector2Int cell, LocomotionType loco) { ... }
}
```

### Ore Regrowth

A coroutine or timer in MapManager. Every 2 minutes (matching `GrowthRate=2` from SPEC), existing ore cells densify and spread to adjacent empty cells. Gems never regrow.

---

## Camera

Top-down RTS camera. Orthographic. Controlled by edge-of-screen panning, WASD/arrow keys, and scroll-wheel zoom.

```
Camera rig (empty GO)
  └── Main Camera (orthographic, URP 2D Renderer)
```

- **Pan**: Edge scroll (mouse near screen border) + keyboard. Clamped to map bounds.
- **Zoom**: Scroll wheel. Clamp between a close zoom (seeing ~20 cells wide) and a far zoom (~60 cells wide).
- **Jump to group**: Alt+1–9 snaps camera to the control group's centroid.

---

## Input

Replace the default InputSystem_Actions with an **RTS-specific Input Action Asset**. Keyboard + mouse only, no gamepad:

| Action | Binding | Notes |
|--------|---------|-------|
| Select | Left Click | Context: select unit, place building |
| Command | Right Click | Context: move, attack, harvest, enter building |
| DragSelect | Left Click + Drag | Box selection |
| ForceAttack | Ctrl + Left Click | Force fire on ground/friendly |
| ForceMove | Alt + Left Click | Move without engaging |
| Stop | S | Stop all selected units |
| Guard | G | Area guard |
| Scatter | X | Units spread out |
| ControlGroup Assign | Ctrl + 1–9 | Assign control group |
| ControlGroup Select | 1–9 | Recall control group |
| ControlGroup Jump | Alt + 1–9 | Jump camera to group |
| SelectAll | E | Select all visible units |
| CameraPan | WASD / Arrow Keys | Camera movement |
| CameraZoom | Scroll Wheel | Zoom in/out |
| Sell | (sidebar button) | Sell mode toggle |
| Repair | (sidebar button) | Repair mode toggle |

---

## Units

Each unit is a prefab: a root GameObject with a `SpriteRenderer` and a collection of components.

### Component Breakdown

| Component | Responsibility |
|---|---|
| `Entity` | Identity: owner (player index), UnitData/BuildingData ref, current HP. Shared by units and buildings. |
| `Selectable` | Handles selection highlight, click detection, health bar display. |
| `Mover` | Pathfinding movement. Reads `LocomotionType` and speed from UnitData. Applies terrain speed modifiers. |
| `Attacker` | Fires weapons. Handles ROF cooldown, range checking, target acquisition, burst fire. |
| `Harvester` | Ore truck behavior: find ore → harvest → return to refinery → deposit → repeat. |
| `Transport` | Passenger loading/unloading. Tracks list of carried Entities. |
| `Aircraft` | Flight behavior: takeoff, fly to target, return to Helipad/Airfield for rearming. Ammo tracking. |
| `Submarine` | Cloak/surfacing logic. Only visible to enemies with Sensors. |
| `Engineer` | Capture/repair building on enter. |
| `Spy` | Disguise logic. Infiltration effects per building type. |
| `SelfHeal` | Regenerate HP up to a threshold over time (Mammoth Tank, Ore Truck). |

### Unit State Machine

Each unit has a simple FSM for its current order:

```
Idle → MoveTo → AttackTarget → ReturnToBase → Guard → ...
```

States are an enum, not separate MonoBehaviours. The `Mover` and `Attacker` components check the current state each frame.

```csharp
public enum UnitState
{
    Idle,
    Moving,
    Attacking,
    Harvesting,
    Returning,     // harvester returning to refinery
    Depositing,    // harvester docked at refinery
    Guarding,
    Entering,      // entering a transport or building
    Rearming,      // aircraft landed on pad
    Deploying,     // MCV deploying into Construction Yard
    Dead
}
```

### Selection System

A `SelectionManager` on the Systems prefab:

- Tracks a `List<Entity> Selected`.
- **Click select**: Raycast at mouse position → find Entity.
- **Box select**: On drag, cast into the grid area → collect all owned Entities in the rect.
- **Double-click**: Select all same-type units on screen.
- **E key**: Select all owned units on screen.
- **Ctrl+1–9**: Assign selected to group. **1–9**: Recall group.

---

## Pathfinding

**A\* on the cell grid.** No NavMesh — the grid is the authority. Single-threaded, simple implementation. Profile and optimize (Job System / Burst) only if it becomes a bottleneck.

### Implementation

- A `Pathfinder` static class or System-prefab component.
- Input: start cell, goal cell, `LocomotionType`.
- Output: `List<Vector2Int>` waypoints.
- Passability check: `MapManager.IsPassable(cell, loco)`. Water cells are passable only for `Float`; land cells only for `Foot`/`Tracked`/`Wheeled`.
- Cost: inverse of speed modifier (slower terrain = higher cost). This makes units prefer roads naturally.
- Buildings and other units block cells (dynamic obstacles). Recalculate on demand.

### Steering

The `Mover` component follows the path cell-by-cell, interpolating position smoothly. When the path is blocked (another unit occupies the next cell), wait briefly then repath.

### Unit Avoidance

Simple: units occupy cells. A cell can hold one vehicle or a small group of infantry (original RA1 lets multiple infantry share a cell). Vehicles that bump into each other repath around.

---

## No Physics / No Collision Layers

This project does **not** use Unity's Physics2D system. No Rigidbody2D, no Collider2D, no collision layers, no physics matrix. Everything is grid-based:

- **Mouse picking**: Convert screen position to cell coordinate via `Camera.ScreenToWorldPoint` → `Tilemap.WorldToCell`. Ask `MapManager` who occupies that cell.
- **Box selection**: Convert drag rect corners to cell coordinates, iterate entities and check if their cell falls inside.
- **Range checks**: Cell distance between attacker and target.
- **Passability**: `MapManager.IsPassable(cell, loco)`.
- **Crushing**: Check cell occupant type on cell enter.

---

## Combat

### Damage Formula

```csharp
public static int CalculateDamage(WeaponData weapon, ArmorType targetArmor)
{
    float modifier = weapon.Warhead.GetModifier(targetArmor);
    return Mathf.RoundToInt(weapon.Damage * modifier);
}
```

Where `GetModifier` reads the appropriate field (`VsNone`, `VsWood`, `VsLight`, `VsHeavy`, `VsConcrete`) from the WarheadData SO.

### Target Acquisition

The `Attacker` component auto-targets the nearest enemy within weapon range. No threat priority — just closest. Force-fire (Ctrl+Click) overrides auto-targeting and can target ground, friendlies, or trees.

### Attack Loop

1. If `NoMovingFire` and currently moving, stop first.
2. Check if target is in range. If not, issue a move-to-range order.
3. If turret-equipped, rotate turret toward target (at `ROT` speed). Wait until facing before firing non-homing projectiles. Homing projectiles can fire without precise aim.
4. Check ROF cooldown.
5. Fire: apply damage (instant for hitscan weapons like M1Carbine, TeslaZap) or spawn a projectile GO (for Cannon, HeatSeeker, Ballistic, Torpedo types).
6. Burst weapons fire `BurstCount` times per cycle.

### Projectiles

Non-hitscan weapons spawn a projectile prefab that travels toward the target. On arrival or collision, apply damage + splash if applicable. Ballistic projectiles (Artillery, Cruiser) use a parabolic arc and can miss if the target moves.

### Splash & Friendly Fire

All splash damage hits everything in radius (1.5 cells), **including friendly units**. Only the firing unit itself is excluded. Damage falls off with distance, controlled by the warhead's `Spread` value.

### Crushing

Vehicles with `CanCrush=true` moving through a cell occupied by enemy infantry kill them instantly. Check on cell enter. Not all tracked vehicles can crush — V2, Artillery, and Ore Truck cannot.

### Death

- Crewed vehicles spawn a Rifle Infantry at the death position.
- ExplodesOnDeath units (Grenadier, Flamethrower) deal AoE damage around them.
- Play death animation/effect, then destroy the GO.

---

## Buildings

Buildings are placed on the grid, occupying a footprint of cells (e.g., 3×2 for a War Factory). They share the `Entity` component with units.

### Building Components

| Component | Responsibility |
|---|---|
| `Entity` | Identity, HP, owner, damage state (intact >50%, damaged ≤50%, critical ≤25%). |
| `Selectable` | Click detection, health bar. |
| `PowerSource` | Reports `PowerProduced` (positive or negative) to the PowerManager. |
| `ProductionQueue` | Builds units. Has a queue of UnitData, tracks build progress. |
| `DefenseWeapon` | For armed buildings (Pillbox, Tesla Coil, etc.). Same Attacker logic as units but immobile. |
| `ResourceStorage` | For Refineries and Silos. Reports storage capacity to EconomyManager. |
| `Refinery` | Docking point for Ore Trucks. Handles ore deposit + credit conversion. |
| `Radar` | Enables the minimap when powered. |
| `GapGenerator` | Applies shroud in a radius around itself to enemy players. |
| `SuperweaponController` | Cooldown timer, activation targeting for Chronosphere/Iron Curtain/Nuke. |

### Building Placement

1. Player clicks a building icon in the sidebar → construction begins (progress bar fills).
2. When complete, the cursor enters **placement mode**: a ghost sprite follows the mouse, snapping to grid cells.
3. Green if valid, red if invalid. Validity rules:
   - Within 16 cells of a Construction Yard.
   - Within 2 cells of any existing friendly building.
   - All footprint cells are passable ground (or water for naval buildings).
   - No overlap with existing buildings or units.
4. On click, the building spawns. On right-click, cancel placement (building returns to "READY" state — player can place it later).

### Selling

Sell mode toggle. Click a friendly building → destroy it, refund `50% × (currentHP / maxHP) × cost`. Crewed buildings spawn an infantry unit.

### Repairing

Repair mode toggle. Click a friendly building → begin repair. Costs ~20% of the building's cost to fully repair. Drains credits continuously. Restores 7 HP per tick.

---

## Construction & Production

### Building Construction (Sidebar Queue)

Each player's Construction Yard drives a single build queue for **structures**. Only one structure builds at a time.

```csharp
public class ConstructionManager : MonoBehaviour
{
    private BuildingData _currentBuilding;
    private float _buildProgress;    // 0 to 1
    private bool _awaitingPlacement;

    void Update()
    {
        if (_currentBuilding == null || _awaitingPlacement) return;

        float buildTime = _currentBuilding.Cost * 0.0008f * 60f; // ~0.8 min per 1000 credits
        _buildProgress += Time.deltaTime / buildTime;

        if (_buildProgress >= 1f)
            _awaitingPlacement = true;
    }
}
```

### Unit Production (Per-Category Queue)

One production queue per building **category** (Infantry, Vehicles, Naval, Aircraft — see SPEC.md). Only one unit builds at a time per category. Multiple same-type buildings act as a speed multiplier (2 Barracks = 2× infantry build speed), not parallel queues.

When a unit finishes, it spawns at the exit cell of the "primary building" (player-selectable when multiple factories of the same type exist). If the exit is blocked, the unit waits inside. There are no rally points in RA1.

---

## Economy & Harvesting

### EconomyManager

On the Systems prefab. Tracks per-player credits and storage.

```csharp
public class EconomyManager : MonoBehaviour
{
    private int[] _credits;         // per player
    private int[] _storageCapacity; // per player (sum of all Refinery + Silo capacity)

    public bool TrySpend(int player, int amount) { ... }
    public void Deposit(int player, int amount) { ... } // clamped to storage
    public int GetCredits(int player) { ... }
}
```

### Harvester Behavior

The `Harvester` component is a state machine:

1. **SeekOre**: Pathfind to nearest ore/gem cell. Prefer closest.
2. **Harvesting**: Stand on the ore cell, tick a timer. Each tick: reduce cell ore density by 1, add 1 bail to cargo. When cargo reaches 28 bails or field is empty, go to ReturnToRefinery.
3. **ReturnToRefinery**: Pathfind to the nearest friendly Refinery. On arrival, enter the Refinery dock.
4. **Depositing**: Wait at refinery. Credits deposited over time: `bails × value_per_bail`. Then return to SeekOre.

Ore value: $25/bail. Gem value: $50/bail.

---

## Tech Tree

The `FactionData` SO holds the full tech tree. Each `BuildingData` and `UnitData` has a `Prerequisites` array of `BuildingData` references.

A unit/building appears in the sidebar **only when** all its prerequisites are met (the player owns at least one of each prerequisite building, and it matches the player's faction).

```csharp
public static bool ArePrerequisitesMet(BuildingData[] prereqs, List<BuildingData> ownedBuildings)
{
    foreach (var req in prereqs)
    {
        if (!ownedBuildings.Contains(req)) return false;
    }
    return true;
}
```

---

## Fog of War

Two-layer system rendered as a Tilemap overlay.

### Per-Cell State (per player)

```csharp
public enum FogState : byte
{
    Shroud = 0,   // black — never explored
    Fog = 1,      // dimmed — explored but no current vision
    Visible = 2   // clear — inside a unit's sight radius
}
```

### Update Loop

Each frame (or every few frames for performance):

1. Set all `Visible` cells to `Fog`.
2. For each friendly unit and building, mark all cells within their `SightRange` as `Visible`.
3. Apply Gap Generator effect: for each enemy Gap Generator, force cells in its radius back to `Shroud` for the current player.
4. Update the Fog tilemap's tiles to match.

### Rendering

A dedicated Tilemap layer on top of everything. Cell-snapped tiles:
- **Shroud tile** — fully black, opaque.
- **Fog tile** — semi-transparent dark overlay.
- **Visible** — no tile (transparent).

### Entity Visibility

Units and buildings owned by enemies are hidden (`SpriteRenderer.enabled = false`) when their cell is `Shroud` or `Fog` for the human player. Buildings in `Fog` remain visible as "last seen" ghosts (stale sprite, no health bar).

---

## Power System

A `PowerManager` on the Systems prefab. Tracks per-player power produced vs consumed.

```csharp
public class PowerManager : MonoBehaviour
{
    private int[] _produced;  // per player — sum of all positive PowerProduced
    private int[] _consumed;  // per player — sum of all negative PowerProduced (absolute value)

    public bool HasSufficientPower(int player) => _produced[player] >= _consumed[player];
}
```

When power is insufficient:
- Radar goes offline (minimap disabled).
- Tesla Coils, AA Guns, Gap Generators stop functioning.
- The sidebar power bar flashes red.

Destroying or selling a Power Plant reduces `_produced`, potentially triggering a brownout.

---

## AI Opponent

Script-driven AI matching the original's behavior. Each AI player is a `AIController` component.

### Build Order

Hardcoded initial sequence:

```
PowerPlant → PowerPlant → OreRefinery → Barracks → WarFactory → RadarDome → ...
```

After the opening, the AI evaluates what to build based on simple priorities: power deficit → more power plants, need money → more refineries, need army → production buildings.

### Team Composition

The AI has ~6 predefined attack team templates (e.g., "5 Heavy Tanks + 3 Grenadiers", "2 V2s + 8 Rifle Infantry"). It picks one randomly, queues those units for production, and when the team is assembled, sends it along a waypoint path toward the human's base.

### Attack Timing

First attack at ~3 minutes. Subsequent attacks at regular intervals. If the human attacks the AI's base, the AI retaliates sooner.

### Limitations (Matching Original)

The AI does **not** build: navy, dogs, Service Depots, Missile Silos. It does not build air units unless attacked by air first. It sells buildings when nearly destroyed and out of money.

### Difficulty

Easy/Medium/Hard only affect stat multipliers (HP, damage) on AI units — not behavior.

---

## Top Bar

A thin bar across the top of the screen:

```
┌─────────────────────────────────────────────────────────┐
│  [Options]                                 $15,521      │
└─────────────────────────────────────────────────────────┘
```

- **Options button** (left) — opens pause/settings menu.
- **Credits display** (right) — current credits, ticks up/down when gaining/spending.

---

## Sidebar UI

The right-hand sidebar is the primary RTS interface. Built with **uGUI** (Canvas + RectTransform).

### Layout

```
┌───────────────────────────┐
│        Minimap            │
│   (top of sidebar)        │
├───────────┬───────────────┤
│ Power Bar │ [Sell][Repair] │
│ (vertical)│ [Map]         │
├───────────┴───────────────┤
│  Structures │   Units     │
│  (left col) │ (right col) │
│ ┌─────────┐ │ ┌─────────┐ │
│ │POWER    │ │ │ARTILLERY│ │
│ │PLANT    │ │ │         │ │
│ ├─────────┤ │ ├─────────┤ │
│ │SANDBAGS │ │ │MINE     │ │
│ │         │ │ │LAYER    │ │
│ ├─────────┤ │ ├─────────┤ │
│ │CONCRETE │ │ │THIEF    │ │
│ │WALL     │ │ │         │ │
│ ├─────────┤ │ ├─────────┤ │
│ │ORE      │ │ │CHRONO-  │ │
│ │REFINERY │ │ │SHIFT    │ │
│ │         │ │ │ READY   │ │
│ └─────────┘ │ └─────────┘ │
│  (scrolls)  │  (scrolls)  │
└─────────────┴─────────────┘
```

### Sidebar Behavior

- **Minimap** is at the **top** of the sidebar. Click to jump camera, drag to pan.
- **Power bar** is a thin vertical strip on the left edge below the minimap. Green = surplus, yellow = tight, red = brownout.
- **Sell / Repair / Map** are small icon buttons near the minimap.
- **Two columns** — structures on the left, units on the right. No tabs. Both visible simultaneously.
- Each column scrolls independently if there are more items than fit.
- Icons show the building/unit name. Only items whose prerequisites are met appear.
- Clicking an icon starts construction. A progress bar fills over the icon as it builds.
- When a structure finishes, the icon shows **"READY"** — clicking it enters placement mode.
- When a unit finishes, it spawns at the primary building's exit cell. No rally points.
- Only **one structure** and **one unit per category** can be in production at a time.

---

## Minimap

A small top-down view of the entire map in the **top-right** of the sidebar. Rendered with a secondary orthographic camera pointed at the map, output to a RenderTexture displayed in the UI.

- Terrain colors: green (grass), tan (sand), blue (water), yellow (ore), purple (gems).
- Friendly units/buildings: green dots.
- Enemy units/buildings (if visible): red dots.
- Fog/shroud: dark overlay matching the fog system.
- Click on minimap → camera jumps to that location.
- Drag on minimap → camera pans.
- Radar Dome required + powered for the minimap to be active.

---

## Campaign System

### MissionData SO

```csharp
[CreateAssetMenu(menuName = "RA/Mission")]
public class MissionData : ScriptableObject
{
    public string MissionName;
    public Faction PlayerFaction;
    public int StartingCredits;
    public TextAsset BriefingText;
    public AudioClip BriefingAudio;
    public string SceneName;         // or a MapData reference
    public MissionObjective[] Objectives;
    public MissionData[] MapVariants; // branching map choices
}
```

### MissionObjective

```csharp
[System.Serializable]
public class MissionObjective
{
    public string Description;
    public ObjectiveType Type;  // DestroyAll, CaptureBuilding, EscortUnit, Survive, ReachZone, etc.
    public bool IsPrimary;
}
```

### Mission Flow

1. **Campaign screen**: Shows a map of Europe. Player clicks a mission region (picks variant if multiple).
2. **Briefing**: Text + voiceover describing the situation and objectives.
3. **Gameplay**: Match loads with pre-placed units, scripted triggers, and objectives.
4. **Victory/Defeat**: Check objective completion. On victory, unlock the next mission. On defeat, offer retry.

### Scripted Triggers

Campaign missions use trigger zones and timers for events (reinforcement arrivals, dialogue, objective updates). A lightweight `MissionTrigger` component with conditions (timer elapsed, unit enters zone, building captured) and actions (spawn units, show dialogue, mark objective complete).

---

## Audio

### Sound Effects

Generated with rfxgen per the asset pipeline in CLAUDE.md. Key categories:
- Unit acknowledgements (select, move, attack)
- Weapon fire sounds (per weapon type)
- Explosions (small, medium, large)
- Building construction / placement / selling
- UI clicks
- Ambient environment

### Music

Generated via the MIDI → FluidSynth → OGG pipeline in `Tools/music/`. Military/march style tracks. Separate tracks for menu, Allied gameplay, Soviet gameplay, combat intensity.

---

## Visuals & Sprites

### Sprite Pipeline

SVG source files live in `Tools/` (not imported by Unity). Inkscape exports them to PNG, which goes into `Assets/_Project/Sprites/`. The `bash Tools/export_sprites.sh` script re-exports all SVGs in batch. Unity only sees the PNGs.

Each unit and building needs:

| Asset | Size | Notes |
|---|---|---|
| Unit idle | 64×64 | Facing right by default. 8 rotations generated by rotating the SVG. |
| Unit move animation | 512×64 (8 frames) | Horizontal strip. Per rotation. |
| Unit attack animation | 256×64 (4 frames) | Per rotation. |
| Unit death | 256×64 (4 frames) | Single strip, no rotation needed. |
| Building | 64×64 to 192×128 | Matches footprint size. Single sprite + damage overlay. |
| Building construction | 256×64 (4 frames) | Build-up animation. |
| Terrain tiles | 64×64 | Each terrain type. Autotile variants for edges (water/shore, cliff edges). |
| Projectiles | 32×32 or 64×64 | Bullets, rockets, shells, lightning bolts. |
| UI icons | 48×48 | Sidebar build icons for each unit and building. |

### Rotations

RTS units face 8 directions. Rather than 8 separate hand-drawn sprites, generate rotated versions from a single SVG source using Inkscape transforms in the export script.

### Building Damage States

Buildings have two visual states: **intact** and **damaged** (below 50% HP, show fire/damage overlay sprite on top).

### Creation Schedule

Sprites are created **incrementally** as each phase needs them, not all at once:

| Phase | Sprites Needed |
|-------|---------------|
| 1 — Foundation | Terrain tiles (placeholder colors OK), generic unit placeholder square |
| 3 — Combat | A few distinct units (e.g., Rifle Infantry, Light Tank, Heavy Tank) + projectiles |
| 4 — Economy | Ore Truck, Ore Refinery, Ore Silo, ore/gem density overlays |
| 5 — Buildings | All buildings (intact + damaged overlay) + sidebar icons (48×48) |
| 7 — Unit Roster | All remaining units (every infantry, vehicle, naval, aircraft — full 8 rotations + animations) |

---

## Implementation Phases

### Phase 1 — Foundation

Editor tooling, core infrastructure, and a playable test map.

- GeneratorWindow + PrefabGeneratorUtils (editor tooling framework).
- RTS Input Action Asset (keyboard + mouse only): Select, Command, DragSelect, ForceAttack, ForceMove, Stop, Guard, Scatter, control groups, camera pan/zoom.
- PlayerState + PlayerManager (player index, faction, owned entities).
- MapManager with cell grid, terrain data, tilemap rendering.
- **Sprites**: terrain tiles (grass, road, rough, sand, water, ore, gems) — placeholder colors are fine.
- Hand-paint a small test map (~40×40) with mixed terrain.
- RTS camera (pan, zoom, edge scroll, clamp to map).
- Entity + Selectable components.
- SelectionManager (click, box select, double-click same-type, E for all, control groups).
- **Sprites**: placeholder unit (colored square per faction) that can be selected and shows a health bar.

**Testable**: Place units on the test map, select them, move camera around.

### Phase 2 — Movement & Pathfinding

- A* pathfinder on the cell grid (single-threaded).
- Mover component: follow path cell-by-cell with smooth interpolation.
- Terrain speed modifiers applied per locomotion type (Foot/Tracked/Wheeled).
- Right-click to move.
- Basic unit avoidance (occupied cells block, wait then repath).
- Force move (Alt+click — move without engaging).

**Testable**: Place different unit types, right-click to move them across varied terrain, watch them prefer roads.

### Phase 3 — Combat

- **Sprites**: a few distinct unit SVGs (e.g., Rifle Infantry, Light Tank, Heavy Tank) so combat is visually readable. 8 rotations each.
- **Sprites**: projectiles (bullet, shell, rocket, fireball) — simple shapes.
- WeaponData, ProjectileData, WarheadData SOs.
- Attacker component: auto-target nearest enemy in range, ROF cooldown, burst fire.
- Turret rotation (ROT speed) — wait for facing before firing non-homing projectiles.
- NoMovingFire — V2/Artillery stop before shooting.
- Damage calculation: `damage × warhead modifier[armor type]`.
- Hitscan weapons (instant damage) vs projectile weapons (spawn GO, travel, impact).
- Splash damage hitting everything in 1.5-cell radius including friendlies. Falloff by Spread value.
- Crushing: `CanCrush` vehicles kill infantry on cell enter.
- Unit death: crewed bail-out (spawn Rifle Infantry), explodes-on-death (Grenadier/Flamethrower AoE).
- Force fire (Ctrl+click — target ground/friendlies/trees).
- Stop (S), Guard (G), Scatter (X) commands.

**Testable**: Place Allied and Soviet units, watch them auto-engage. Test splash friendly fire, crushing, force fire on ground.

### Phase 4 — Economy & Harvesting

- **Sprites**: Ore Truck, Ore Refinery, Ore Silo. Ore/gem tile overlays (4 density levels each).
- EconomyManager (per-player credits, storage capacity).
- Ore/gem cells with 4 density levels. Ore regrowth timer (2 min).
- Ore Refinery building (docking point, storage, free Ore Truck).
- Ore Silo building (additional storage).
- Harvester component: SeekOre → Harvesting → ReturnToRefinery → Depositing → repeat.
- Single harvester docking per refinery, others queue.
- Storage overflow: excess credits lost.
- Edge cases: refinery destroyed while returning (find another or idle), all ore depleted (far-scan 48 cells).

**Testable**: Place a refinery + ore field, watch harvester loop. Destroy the refinery, verify retargeting. Fill storage, verify overflow.

### Phase 5 — Buildings, Construction & Sidebar

Buildings, construction, production, and the sidebar are tightly coupled — build them together.

- **Sprites**: all buildings (Construction Yard, Power Plant, Barracks, War Factory, Radar Dome, Tech Center, defenses, walls, superweapon buildings, etc.). Each needs intact sprite + damaged overlay. Sidebar icons (48×48) for every buildable item.
- BuildingData SOs for core buildings (populate stats from source code).
- Top bar: Options button + credits display (ticking counter).
- Sidebar layout (uGUI Canvas):
  - Minimap placeholder (top).
  - Power bar (vertical strip).
  - Sell / Repair buttons.
  - Two-column build grid: structures (left), units (right). Filtered by prerequisites.
  - Build progress bar on icons. "READY" label on finished structures.
- ConstructionManager: one structure queue per Construction Yard. Build time formula. Placement mode (ghost, green/red validity, grid snap). Cancel returns to READY.
- Building placement rules: 16-cell CY radius, 2-cell adjacency, footprint checks.
- Selling: 50% × (currentHP / maxHP) refund. Crewed buildings spawn infantry.
- Repairing: 20% of cost to fully repair. Drains credits. 7 HP per tick.
- Building damage states: intact (>50%), damaged (≤50% — fire overlay), critical (≤25% — Engineer capturable).
- PowerManager: produced vs consumed, brownout disables RequiresPower buildings. Power bar colors (green/yellow/red).
- ProductionQueue: one queue per category (Infantry, Vehicles, Naval, Aircraft). Multiple same-type buildings = speed multiplier. Unit spawns at primary building exit cell. Blocked exit = wait.
- FactionData SOs: Allied + Soviet tech trees, prerequisite filtering.
- Primary building selection (click to set which factory is the exit point).

**Testable**: Full build loop — construct buildings from sidebar, place them, build units, sell/repair. Power brownout from losing a power plant. Tech tree filtering.

### Phase 6 — Fog of War

- FogState grid: per-player, per-cell (Shroud/Fog/Visible).
- Sight range update loop (every few frames for performance).
- Fog tilemap rendering: shroud (black), fog (semi-transparent), visible (no tile).
- Entity visibility: enemies hidden in shroud/fog. Buildings in fog shown as "last seen" ghost.
- Gap Generator: re-shrouds 10-cell radius, 6-second refresh.

**Testable**: Move units around, watch shroud reveal. Walk away, see fog. Place a Gap Generator, verify re-shrouding.

### Phase 7 — Unit Roster & Special Behaviors

Create all remaining UnitData SOs from source code. **Sprites**: all remaining units not yet created (every infantry type, all vehicles, naval units, aircraft — 8 rotations, move/attack/death animations). Implement unique behaviors:

**Infantry specials:**
- Engineer: capture enemy buildings (≤25% HP required, ~1/3 HP damage per engineer), repair friendly buildings (instant full heal).
- Spy: disguise as enemy infantry, infiltration effects per building type.
- Tanya: dual pistols (HollowPoint), C4 on buildings/bridges/ships (1.8s delay, instant kill).
- Attack Dog: instant-kill infantry (Organic warhead), auto-detect spies (7-cell guard range).
- Field Medic: auto-heal nearby friendly infantry (~1.83 cells), cannot self-heal.

**Vehicle specials:**
- MCV: deploy into Construction Yard / re-pack.
- Mammoth Tank + Ore Truck: SelfHeal to 50% HP.
- Mine Layer: deploy mines.

**Naval:**
- Float locomotion on water cells.
- Submarine: cloaked when submerged, surface to fire, detected by Sensors units.
- Naval Transport: carry infantry + vehicles, shoreline unloading.

**Aircraft:**
- Fly locomotion (ignore terrain). Ammo tracking.
- Helicopters hover at target, return to Helipad to rearm.
- Fixed-wing (MiG, Yak) do strafing runs, return to Airfield.
- Rearming: 2.4 sec per ammo point.

**Transports (APC, Chinook, Naval Transport):**
- Boarding/unloading one at a time.
- Passenger fate on destruction (APC: eject, Chinook: die, Naval at sea: vehicles die / infantry may survive).

**Testable**: Each special unit behavior works in isolation on the test map.

### Phase 8 — Minimap & UI Polish

- Minimap: secondary orthographic camera → RenderTexture in sidebar. Terrain colors, unit dots (green/red), fog overlay. Click to jump, drag to pan. Requires Radar Dome + power.
- Selected unit info panel (portrait, HP bar, weapon stats).
- Cursor changes: move, attack, harvest, enter building, no-go, sell, repair.

**Testable**: Minimap reflects game state. Click minimap to jump camera. Radar Dome destroyed = minimap offline.

### Phase 9 — AI Opponent

- AIController component per AI player.
- Hardcoded build order: Power → Power → Refinery → Barracks → War Factory → Radar → ...
- Priority evaluation after opener: power deficit → power plants, low cash → refineries, need army → production.
- AI harvesting: assigns ore trucks, builds additional refineries.
- Team assembly: ~6 predefined attack templates. Queue units, wait until team is full, send along waypoints.
- Attack timing: first wave ~3 minutes, subsequent at intervals. Retaliates sooner if attacked.
- AI limitations (matching original): no navy, no dogs, no Service Depots, no Missile Silos. Air only if attacked by air.
- Difficulty: Easy/Medium/Hard stat multipliers (HP, damage) only — no behavior change.
- AI sells buildings when nearly dead and out of money.
- Skirmish setup screen (Init scene): pick faction/country, map, AI count, difficulty, starting credits.

**Testable**: Start a skirmish match, AI builds a base, harvests, and attacks at ~3 minutes.

### Phase 10 — Superweapons & Support Powers

- Chronosphere: teleport one vehicle, auto-return after ~3 minutes. Cannot teleport infantry.
- Iron Curtain: temporary invulnerability (~45 sec). Kills infantry inside transports.
- Nuclear Missile: 1000 damage AoE, Nuke warhead, destroys ore, radiation zone. 13-min recharge.
- GPS Satellite: permanently reveal entire map (one-time, Allied Tech Center).
- Spy Plane: reveal target area temporarily (Soviet Airfield).
- Paratroopers: drop 5 Rifle Infantry anywhere (Soviet Airfield).
- Parabombs: bombing run on target area (Soviet Airfield + Tech Center).
- Cooldown timers in sidebar. Click to activate → click map to target.

**Testable**: Build superweapon buildings, wait for charge, use each one. Verify effects and cooldowns.

### Phase 11 — Campaign

- Campaign selection screen (map of Europe with mission markers, map-variant branching).
- MissionData SOs for all 28 missions (14 Allied + 14 Soviet).
- Briefing screen (text + audio).
- MissionTrigger component: conditions (timer, unit enters zone, building captured) → actions (spawn units, show dialogue, mark objective complete).
- Victory/defeat detection and result screen.
- Mission unlock progression.

**Testable**: Play through at least the first 2-3 missions of each campaign end-to-end.

### Phase 12 — Crates, Audio & Polish

- Random crate spawning in skirmish. 11 crate types (money, heal, unit, map reveal, buffs, explosion).
- Sound effects: unit select/move/attack acknowledgements, weapon fire, explosions, building placement/sell, UI clicks.
- Music: MIDI → FluidSynth → OGG pipeline. Tracks for menu, Allied gameplay, Soviet gameplay.
- Building damage visuals: fire/smoke overlay at ≤50% HP.
- Screen shake on large explosions.
- Victory/defeat fanfare.
- Country bonuses for skirmish (damage, armor, speed, cost, ROF multipliers).
