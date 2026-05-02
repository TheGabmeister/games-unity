# IMPL.md — Implementation Plan

High-level implementation plan for the Unity recreation of C&C: Red Alert. **Singleplayer only** (skirmish vs AI + campaigns). See SPEC.md for all gameplay data and numeric values.

---

## Table of Contents

- [Guiding Principles](#guiding-principles)
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
    public WeaponData PrimaryWeapon;
    public WeaponData SecondaryWeapon;
    public LocomotionType Locomotion; // Foot, Tracked, Wheeled, Float, Fly
    public BuildingData[] Prerequisites;
    public int PassengerCapacity;  // 0 for non-transports
    public bool SelfHeals;
    public float SelfHealThreshold; // e.g., 0.5 for Mammoth (heals to 50%)
    public bool Crewed;            // spawns infantry on death
    public bool ExplodesOnDeath;
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
    public int RateOfFire;     // lower = faster
    public float Range;        // in cells
    public WarheadData Warhead;
    public int BurstCount;     // shots per attack cycle
    public bool AntiAir;
    public bool AntiGround;
    public ProjectileType Projectile; // Invisible, Cannon, HeatSeeker, Ballistic, Torpedo, etc.
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
    public bool Splash;        // area of effect
    public float SplashRadius;
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
    public Sprite Icon;
    public GameObject Prefab;
    public UnitData FreeUnit;  // e.g., Ore Refinery gives free Ore Truck
}
```

---

## Grid & Map System

The map is a 2D cell grid backed by Unity's **Tilemap**. One cell = one game tile (24×24 px at original scale; we'll use 64×64 px sprite cells to match our sprite pipeline).

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
    private byte[] _fogState;       // per-player: 0=shroud, 1=fog, 2=visible

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

Replace the default InputSystem_Actions with an **RTS-specific Input Action Asset**:

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
    Returning,
    Guarding,
    Entering,   // entering a transport or building
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

**A\* on the cell grid.** No NavMesh — the grid is the authority.

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

1. Check if target is in range.
2. If not, issue a move-to-range order.
3. If in range, check ROF cooldown.
4. Fire: apply damage (instant for hitscan weapons like M1Carbine, TeslaZap) or spawn a projectile GO (for Cannon, HeatSeeker, Ballistic, Torpedo types).
5. Burst weapons fire `BurstCount` times per cycle.

### Projectiles

Non-hitscan weapons spawn a projectile prefab that travels toward the target. On arrival or collision, apply damage + splash if applicable. Ballistic projectiles (Artillery, Cruiser) use a parabolic arc and can miss if the target moves.

### Crushing

Tracked vehicles moving through a cell occupied by enemy infantry kill the infantry instantly. Check on cell enter.

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
| `Entity` | Identity, HP, owner. |
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
4. On click, the building spawns. On right-click, cancel placement (refund the cost).

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

### Unit Production (Per-Factory Queue)

Each production building (Barracks, War Factory, Naval Yard, Airfield, Helipad, Kennel) has its own `ProductionQueue`. Multiple same-type buildings share a single queue and split build time (build speed doubles with 2 Barracks, etc.).

When a unit finishes, it spawns at a rally point adjacent to the building.

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

A dedicated Tilemap layer on top of everything. Three tile types:
- **Shroud tile** — fully black, opaque.
- **Fog tile** — semi-transparent dark overlay.
- **Visible** — no tile (transparent).

For smoother visuals, use a RenderTexture approach: a camera renders unit sight as white circles onto a low-res texture, which is then used to mask the fog overlay. This gives soft edges instead of cell-snapped hard edges.

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

The right-hand sidebar is the primary RTS interface. Built with **UI Toolkit** or **uGUI** Canvas.

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
- When a unit finishes, it spawns immediately at the production building's rally point.
- Only **one structure** and **one unit** can be in production at a time (per Construction Yard / per production building type).

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

All art is SVG → PNG via Inkscape (see CLAUDE.md). Each unit and building needs:

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

---

## Implementation Phases

### Phase 1 — Grid, Camera, Selection

- MapManager with cell grid, terrain data, tilemap rendering.
- RTS camera (pan, zoom, clamp).
- Input actions for selection.
- Entity + Selectable components.
- SelectionManager (click, box, double-click, control groups).
- Placeholder unit prefab (colored square) that can be selected and displays a health bar.

### Phase 2 — Movement & Pathfinding

- A* pathfinder on the cell grid.
- Mover component: follow path, terrain speed modifiers.
- Right-click to move.
- Basic unit avoidance (occupied cells block, repath).
- Force move (Alt+click).

### Phase 3 — Combat

- WeaponData, WarheadData SOs.
- Attacker component: auto-target, ROF cooldown, range check.
- Damage calculation with warhead modifiers.
- Projectile spawning for non-hitscan weapons.
- Unit death (crewed bail-out, explodes-on-death).
- Force fire (Ctrl+click).
- Guard, Stop, Scatter commands.

### Phase 4 — Economy & Harvesting

- EconomyManager (credits, storage).
- Ore/gem cells on the map with density values.
- Harvester component (seek → harvest → return → deposit loop).
- Ore Refinery docking behavior.
- Ore regrowth timer.
- Credits display in UI.

### Phase 5 — Buildings & Construction

- BuildingData SOs for all buildings.
- Building placement system (ghost, validity checks, grid snapping).
- ConstructionManager (sidebar build queue, progress, placement mode).
- Selling and repairing.
- PowerManager (power produced vs consumed, brownout effects).
- Populate all building stats from SPEC.md.

### Phase 6 — Production & Tech Tree

- ProductionQueue component on Barracks, War Factory, etc.
- FactionData SOs with tech tree references.
- Sidebar UI: structure tab, unit tab, build icons filtered by prerequisites.
- Unit spawning at rally points.
- Multiple same-type buildings speed up production.

### Phase 7 — Fog of War

- FogState grid (per player).
- Sight range update loop.
- Fog tilemap rendering (shroud, fog, visible).
- Entity visibility toggling for enemies.
- Gap Generator effect.
- GPS Satellite (reveal all).

### Phase 8 — Full Unit & Building Roster

- Create all UnitData and BuildingData SOs from SPEC.md values.
- SVG sprites for every unit and building.
- Unique behaviors: Spy (disguise, infiltrate), Tanya (C4), Engineer (capture), Attack Dog (instant kill, detect spies), Submarine (cloak), Aircraft (ammo, rearm at pad).
- Transports (APC, Chinook, naval Transport).
- Naval movement on water cells.

### Phase 9 — Sidebar & UI Polish

- Full sidebar layout (power bar, credits, build grid, sell/repair buttons).
- Minimap with secondary camera.
- Selected unit info panel (portrait, HP, weapon stats).
- Cursor changes (move, attack, harvest, enter, no-go).
- Radar Dome requirement for minimap.

### Phase 10 — AI Opponent

- AIController with hardcoded build order.
- Team assembly and waypoint-following attack waves.
- Difficulty multipliers (Easy/Medium/Hard).
- AI ore harvesting.
- Basic AI defense (build turrets/coils near base).
- Skirmish setup screen (pick faction, map, AI count/difficulty, starting credits).

### Phase 11 — Superweapons & Support Powers

- Chronosphere (teleport vehicle, auto-return timer).
- Iron Curtain (temporary invulnerability).
- Nuclear Missile (big AoE + radiation zone).
- GPS Satellite (reveal map).
- Spy Plane (reveal area).
- Paratroopers (air drop infantry).
- Parabombs (air bombing run).
- Cooldown timers and sidebar activation buttons.

### Phase 12 — Campaign

- Campaign selection screen (map of Europe with mission markers).
- MissionData SOs for all 28 missions.
- Briefing screen (text + audio).
- Scripted triggers for each mission (reinforcements, objectives, events).
- Victory/defeat conditions and screen.
- Map-variant branching.

### Phase 13 — Crates & Polish

- Random crate spawning in skirmish.
- Crate pickup effects (money, heal, unit, map reveal, buff, explosion).
- Sound effects for all actions.
- Music tracks (menu, Allied, Soviet, combat).
- Building damage visuals (fire overlays at <50% HP).
- Screen shake on explosions.
- Victory/defeat fanfare.
