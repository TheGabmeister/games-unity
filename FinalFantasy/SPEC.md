# Final Fantasy 1 — Mechanics Recreation in Unity

## 1. Project Overview

Recreate the core mechanics of Final Fantasy 1 (Pixel Remaster version) in Unity 6. This is **not** a pixel-by-pixel visual clone — it focuses on faithful mechanical reproduction using procedural/primitive visuals. No imported sprites, textures, or audio assets.

**Target:** Playable from party creation through the full game loop (explore, fight, shop, progress, boss, credits).

### What We Are Building
- Turn-based RPG with 4-party system, 6 job classes with upgrades
- Tile-based world map, towns, and dungeons
- Random encounter battle system with full damage formulas
- Equipment, inventory, magic, and shop systems
- Data-driven content pipeline for all game entities
- Procedural rendering for all visual elements
- Stub audio system ready for future asset integration

### What We Are NOT Building
- Online/multiplayer features
- Pixel Remaster's bestiary gallery, music player, or achievement system
- Boost/cheat options from the remaster
- Cutscenes or FMVs

---

## 2. Technical Stack & Architecture

### Engine & Packages (already configured)
| Component | Choice | Notes |
|---|---|---|
| Engine | Unity 6 (6000.3.12f1) | LTS target |
| Render Pipeline | URP 2D | Already configured with Renderer2D |
| Input | Input System 1.19.0 | Already installed; needs FF1-specific action map |
| Tweening | PrimeTween 1.3.8 | Already installed; use for all animations |
| UI | uGUI 2.0.0 | Already installed; use for all menus and HUD |

### High-Level Architecture

```
GameManager (singleton, DontDestroyOnLoad)
├── GameStateManager        — FSM: Title, Exploration, Battle, Menu, Dialogue, Transition
├── PartyManager            — Party data, formation, class state
├── InventoryManager        — Items, equipment, gold
├── ProgressionManager      — Story flags, key items, world state
├── SaveManager             — Serialization, slots, auto-save
├── AudioManager            — Stub system with play/stop/volume API
├── SceneLoader             — Async scene loading with transitions
└── DataRepository          — Central access to all ScriptableObject databases
```

### Scene Strategy
| Scene | Purpose |
|---|---|
| `Boot` | Initializes singletons, loads persistent data, transitions to Title |
| `Title` | Title screen, New Game / Continue |
| `WorldMap` | Overworld exploration |
| `Town_{Name}` | One scene per town (Cornelia, Pravoka, Elfheim, etc.) |
| `Dungeon_{Name}` | One scene per dungeon floor / area |
| `Battle` | Loaded additively on encounter; unloaded on resolution |

**Why additive battle scene:** Preserves exploration scene state (player position, NPC state) without serialization overhead. The battle scene layers on top, runs its own camera, and unloads when done.

### Folder Structure
```
Assets/_Project/
├── Scripts/
│   ├── Core/               — GameManager, FSM, singletons
│   ├── Battle/             — BattleManager, TurnSystem, DamageCalculator, AI
│   ├── Exploration/        — PlayerController, TileMovement, Encounters
│   ├── Party/              — PartyMember, ClassDefinition, StatBlock
│   ├── Inventory/          — Item, Equipment, Inventory logic
│   ├── Magic/              — Spell, SpellEffect, targeting
│   ├── UI/                 — All UI controllers and widgets
│   ├── Data/               — ScriptableObject definitions
│   ├── Save/               — Serialization, save slots
│   ├── Audio/              — AudioManager, SFX/BGM stub
│   ├── Rendering/          — Procedural visuals, shape factories
│   └── Utility/            — Extensions, helpers
├── Data/
│   ├── Classes/            — SO instances for each job class
│   ├── Enemies/            — SO instances for each enemy
│   ├── Items/              — SO instances for items
│   ├── Equipment/          — SO instances for weapons/armor
│   ├── Spells/             — SO instances for spells
│   ├── Encounters/         — SO encounter tables per region
│   ├── Maps/               — Tilemap data, warp definitions
│   └── Shops/              — SO shop inventories
├── Scenes/
├── Prefabs/
├── Input/
├── Settings/
└── Materials/              — Procedural materials/shaders
```

---

## 3. Core Systems — Detailed Breakdown

### 3.1 Game State Machine

```
Title → PartyCreation → Exploration ⇄ Battle
                            ↕
                          Menu
                            ↕
                        Dialogue
                            ↕
                         Shop
```

The top-level FSM controls what input is routed where and what UI is visible. States are exclusive — you cannot be in Battle and Menu simultaneously.

**Critical detail:** The `Exploration` state has sub-states for movement mode (Walking, Canoe, Ship, Airship). Each has different movement rules, encounter tables, and tile passability.

### 3.2 Party & Class System

#### Party Creation
- Player selects 4 members from 6 classes (duplicates allowed — you can run 4 Warriors)
- Each member gets a player-assigned name (max 6 characters, matching FF1)
- No respec — class choice is permanent until class upgrade event

#### The 6 Base Classes → Upgraded Classes
| Base | Upgrade | Stat Focus | Equipment Access | Magic |
|---|---|---|---|---|
| Warrior | Knight | HP, Str, Vit | Heavy armor, swords, axes | Gains White 1-3 |
| Thief | Ninja | Agi, Luck | Light armor, daggers | Gains Black 1-4 |
| Monk | Master | Str, Vit, HP | Bare hands scale, no heavy armor | None |
| Red Mage | Red Wizard | Balanced | Medium armor, swords | White/Black 1-7 (not 8) |
| White Mage | White Wizard | Int, Vit | Staves, robes | White 1-8 |
| Black Mage | Black Wizard | Int | Staves, robes | Black 1-8 |

#### Stat Model (per character)
```
Base Stats (from class):  HP, MP, Strength, Agility, Vitality, Intellect, Luck
Level:                    1-99 (Pixel Remaster cap)
Experience:               Tracked per character (shared from battles)
Equipment Bonuses:        Additive modifiers to stats
Status Effects:           Bitfield of active conditions
```

#### Stat Growth
Each class has a growth table: per-level-up stat increments with variance. Pixel Remaster uses fixed tables with small random variance (±1-2 per stat per level). Store as `ClassGrowthTable` ScriptableObject.

**Edge case — Monk/Master bare-hand scaling:** Monk's attack power is `Level * 2` when unarmed, and their absorb (defense) is also level-based when unarmored. This makes them uniquely gear-independent. Must be special-cased in the damage formula, not handled by equipment stats.

**Edge case — duplicate classes:** All 4 party members could be the same class. UI, targeting, and equipment screens must handle this (distinguish by name/slot, not class).

### 3.3 Exploration System

#### Tile-Based Movement
- Grid-locked movement on all maps (world map, towns, dungeons)
- 4-directional only (no diagonal)
- Movement is smooth interpolation between grid cells (PrimeTween lerp over ~0.15s)
- Hold direction to keep moving; tap to move one tile and stop
- Collision check before movement: query tile passability + NPCs + interactables

#### Tilemap Approach
Use Unity's built-in `Tilemap` system with **programmatically generated `Tile` assets** — no imported sprites. Each tile type gets a procedurally colored square (grass = green, water = blue, mountain = brown, etc.). Details in Section 6 (Rendering).

#### Tile Data
Each tile needs metadata beyond visual:
```csharp
[Flags]
public enum TilePassability
{
    None        = 0,
    OnFoot      = 1 << 0,
    Canoe       = 1 << 1,
    Ship        = 1 << 2,
    Airship     = 1 << 3,  // Airship can land here
    AirshipFly  = 1 << 4,  // Airship can fly over (almost everything)
}
```

Passability is checked against current movement mode. Water tiles allow Ship but not OnFoot. River tiles allow Canoe but not Ship.

#### World Map Specifics
- Wrapping: The FF1 world map wraps horizontally and vertically (it's a torus). The tilemap must wrap or use teleport triggers at edges.
- Vehicles: Ship and Airship are "owned" objects placed in the world. Player boards/exits them. Ship is placed where you disembark. Airship returns to where you land it.
- Airship shadow: Airship flies above terrain. Need a visual indicator of ground position for landing.

**Hard problem — world map wrapping:** Unity Tilemaps don't natively wrap. Options:
1. **Ghost border:** Duplicate edge tiles on opposite sides (wastes memory for large maps).
2. **Camera + position modulo:** Wrap player position mathematically and shift the camera. Tilemap doesn't know about wrapping.
3. **Chunked loading with virtual coordinates:** Most robust but most complex.

**Recommendation:** Option 2 — modulo position wrapping. The world map is 256x256 tiles in FF1. At that size, a single Tilemap fits in memory. Wrap player coordinates and let the camera follow.

#### Town & Dungeon Specifics
- No wrapping, bounded maps
- NPCs with schedules (FF1 NPCs are mostly static, some walk set paths)
- Treasure chests: opened state tracked in `ProgressionManager`
- Warp tiles: stairs, exits, entrances — defined as `WarpDefinition` ScriptableObjects linking coordinates between scenes
- Locked doors: Key items gate access (Mystic Key, etc.)
- Damage floors: specific dungeon tiles deal damage per step (lava in Gurgu Volcano, etc.)
- Conveyor/teleport tiles: Ice Cave and some late dungeons have tiles that push the player

### 3.4 Random Encounter System

#### Encounter Rate
FF1 uses a **step counter** system, not pure random chance:
- Each zone has a base encounter rate (steps between fights, e.g., 20-30 on world map grass)
- A hidden counter decrements per step. At 0, an encounter triggers and the counter resets
- Counter range has variance (min/max steps)
- Different terrain types have different rates (forest = more encounters, plains = fewer)

```csharp
[CreateAssetMenu]
public class EncounterTable : ScriptableObject
{
    public int MinSteps;
    public int MaxSteps;
    public EncounterEntry[] Encounters;  // weighted list of enemy formations
}

[System.Serializable]
public class EncounterEntry
{
    public EnemyFormation Formation;
    public int Weight;
}
```

#### Enemy Formations
A formation defines:
- Which enemies appear (1-9 enemies)
- Enemy positions in the battle scene
- Whether it's a "boss" formation (no flee allowed)
- Potential for preemptive strike or ambush

**Edge case — preemptive/ambush:** Based on party agility vs enemy agility. Preemptive = party gets a free round. Ambush = enemies get a free round. Must factor into turn order initialization.

### 3.5 Battle System

This is the most complex system. FF1 Pixel Remaster uses a **turn-based system with speed-based ordering** (not ATB).

#### Battle Flow
```
1. Encounter triggered → load Battle scene additively
2. Determine surprise/preemptive/normal
3. TURN LOOP:
   a. Calculate turn order (all actors sorted by effective agility)
   b. For each party member (in party order): show command menu, collect action
   c. Re-sort all actions by speed
   d. Execute actions in order:
      - Check if actor is alive/able to act (status effects)
      - Check if target is still valid
      - Execute action (attack/spell/item/defend/flee)
      - Apply results (damage, healing, status, death)
      - Check for battle end (all enemies dead OR all party dead OR fled)
   e. Apply end-of-turn effects (poison tick, regen, etc.)
   f. If battle not over → next turn (back to step a)
4. Victory: award EXP, Gil, item drops
5. Defeat: Game Over screen
6. Unload Battle scene
```

#### Command Menu (per party member)
```
Attack  — physical attack on one enemy
Magic   — opens spell list (White/Black by level)
Item    — use consumable from inventory
Defend  — halve incoming physical damage this turn, guaranteed to act first
Flee    — entire party attempts to run (based on luck vs enemy level; bosses block)
```

#### Damage Formulas

**Physical Attack:**
```
Attack Power   = Weapon.Attack + Strength.Bonus
Hit Count      = (Accuracy / 32) + 1   (each "hit" is rolled independently)
Per-Hit Damage = Attack Power - target.Defense  (minimum 1)
Hit Chance     = 168 + HitRate - target.Evasion  (capped 0-200, rolled vs d200)
Critical Rate  = WeaponCritBonus  (percentage)
Critical Hit   = ignores defense
Total Damage   = sum of all successful hits
```

**Magical Attack:**
```
Spell Power    = spell.BasePower
Damage         = Spell Power + (Intellect / 2) + random(0, Spell Power)
Resistance     = target.MagicDefense + elemental_resist_modifier
Final Damage   = max(1, Damage - Resistance)
Hit Chance     = 148 + spell.Accuracy - target.MagicEvasion
```

**Hard problem — multi-hit system:** FF1's physical attacks aren't single rolls. A high-level Warrior might have 8+ hit counts, each rolled independently. This creates high damage variance and makes the battle feel very different from single-roll systems. Critical hits also apply per-hit. This must be faithfully reproduced.

**Hard problem — exact formula sourcing:** The Pixel Remaster changed some formulas from the NES original. We should document which version we're targeting per formula and allow tuning via `BattleConfig` ScriptableObject.

#### Elemental System
8 elements: Fire, Ice, Lightning, Earth, Water, Wind, Poison/Dark, Holy/Light

Interactions per target:
- **Weak:** 2x damage
- **Resist:** 0.5x damage (rounded down)
- **Absorb:** Heals instead of damages
- **Null:** 0 damage

Equipment can grant elemental resistance. Some weapons deal elemental damage.

**Edge case — multi-element spells:** Some spells have multiple elements (rare in FF1 but exists). If a target resists one element but is weak to another on the same spell, which wins? FF1 rule: weakness takes priority over resistance.

#### Status Effects

| Status | Effect | Cure | Battle/Persistent |
|---|---|---|---|
| KO | Cannot act, removed from combat | Life spell, Phoenix Down | Persistent |
| Poison | Lose HP per step (field) and per turn (battle) | Antidote, Poisona | Persistent |
| Stone | Cannot act, effectively dead in battle | Soft, Stona | Persistent |
| Blind | Reduced physical hit rate | Eye Drops, Blindna | Persistent |
| Silence | Cannot cast spells | Echo Herbs, Vox | Persistent |
| Sleep | Cannot act, wakes on taking damage | Physical hit, Basuna | Battle only |
| Paralysis | Cannot act | Basuna | Battle only |
| Confuse | Acts randomly (may attack allies) | Physical hit from ally | Battle only |
| Mini | Greatly reduced attack/defense (PR addition) | Mini spell, Basuna | Battle only |
| Death | Instant KO effect (from spells) | Same as KO | — |

**Edge case — full party KO:** If all 4 party members are KO'd or Stone'd, it's Game Over. Stone counts as "dead" for this check.

**Edge case — Confuse targeting:** Confused allies use basic attacks on random targets (ally or enemy). They don't use spells or items. Physical hit from ally cures it but also deals damage.

**Edge case — Poison field damage:** Poison ticks damage while walking. If a character hits 0 HP from poison while walking, they become KO'd. If all party members die from field poison, Game Over triggers.

#### Enemy AI
FF1 enemies use **weighted random action selection**, not complex decision trees:
```csharp
public class EnemyAIProfile : ScriptableObject
{
    public EnemyAction[] Actions;  // each with: action, weight, conditions
}

[System.Serializable]
public class EnemyAction
{
    public ActionType Type;         // Attack, CastSpell, UseAbility
    public SpellData Spell;         // if applicable
    public int Weight;              // relative probability
    public AICondition Condition;   // e.g., HP < 50%, specific turn count
}
```

Most regular enemies just attack. Bosses have multi-phase AI (change action weights based on HP thresholds).

**Edge case — boss scripted sequences:** Some bosses (Lich, Marilith, Kraken, Tiamat) have predictable rotation patterns mixed with random selection. Must support "use ability X every N turns" alongside weighted random.

#### Target Retargeting (Critical Design Decision)

In the NES original, if a character targeted an enemy that died before their turn, the attack was "ineffective" (wasted). The Pixel Remaster **auto-retargets** to another living enemy.

**Decision: Implement auto-retarget** (Pixel Remaster behavior). Store this as a config toggle in case we want to support "classic" mode later.

#### Flee Mechanics
- Flee attempt is per-turn, whole party
- Success rate: `(PartyAvgLevel * 2 + 80 - EnemyAvgLevel) / 256` per attempt
- Boss encounters: flee is disabled (button grayed out)
- On success: no EXP/Gil, return to exploration
- On failure: enemies get a free round of attacks

### 3.6 Magic System

#### Spell Structure
```csharp
public class SpellData : ScriptableObject
{
    public string SpellName;
    public MagicSchool School;       // White or Black
    public int Level;                // 1-8
    public int MPCost;
    public SpellTarget Targeting;    // Single, AllEnemies, SingleAlly, AllAllies, Self
    public SpellEffect[] Effects;    // Damage, Heal, Buff, Debuff, StatusInflict, StatusCure
    public Element Element;
    public int BasePower;
    public int Accuracy;
}
```

#### Spell Learning
In Pixel Remaster, spells are **purchased from shops** (White/Black magic shops in towns), not learned by level. Each class has a list of learnable spell levels. A character can know up to 3 spells per level.

**Edge case — spell slots full:** If a character already knows 3 spells at a given level, they cannot buy another at that level. The shop must check and prevent purchase, or allow replacing.

**Edge case — class upgrade spell access:** When a class upgrades (e.g., Warrior → Knight), they gain access to new spell levels. But they don't automatically learn spells — they still need to buy them.

### 3.7 Equipment System

#### Slots
Each character has: **Weapon**, **Shield**, **Helmet**, **Armor** (body)

#### Equipment Data
```csharp
public class EquipmentData : ScriptableObject
{
    public string Name;
    public EquipmentSlot Slot;
    public int Attack;        // weapons only
    public int Defense;       // armor/shield/helmet
    public int Evasion;       // shields mainly
    public int MagicDefense;
    public int Accuracy;      // weapons
    public int CritRate;      // weapons
    public Element[] ElementalResist;
    public StatusEffect[] StatusResist;
    public ClassFlag AllowedClasses;   // bitfield of which classes can equip
    public SpellData CastableSpell;    // some equipment casts spells when used in battle
    public int BuyPrice;
    public int SellPrice;             // usually half buy price
    public StatModifier[] StatBonuses; // Str+5, etc.
}
```

**Edge case — equipment spell casting:** Some weapons/armor can be "used" in battle to cast a free spell (no MP cost). This adds a hidden "Use" command in battle when such equipment is worn. Must be surfaced in the battle UI.

**Edge case — optimizing equipment:** The Pixel Remaster has an "Optimize" button that auto-equips best gear. "Best" is not obvious — it optimizes for attack power (weapons) and defense (armor). Must implement a sorting heuristic.

### 3.8 Inventory System

- Consumable items: Potions, Ethers, Phoenix Down, status cures, etc.
- Key items: Separate list, not consumable, gate story progression
- Stack limit: 99 per item type
- Total inventory: uncapped in Pixel Remaster (NES had limits)
- Items can be used in field (healing) or battle (healing, damage, status cure)

### 3.9 Shop System

#### Shop Types
- **Weapon shops** — sell weapons
- **Armor shops** — sell shields, helmets, armor
- **White magic shops** — sell White spells (by level)
- **Black magic shops** — sell Black spells (by level)
- **Item shops** — sell consumables

#### Shop UI Flow
```
Enter Shop → Browse items (show equip comparison for gear) → Buy/Sell → Confirm → Update gold
```

**Edge case — spell shop for wrong class:** Player might try to buy a Black Magic spell with no Black Mage in the party. Allow purchase anyway (they might class-upgrade later? No — FF1 doesn't allow this). **Decision:** Show which party members can learn each spell. Allow purchase only if at least one member can learn it.

**Edge case — already owned spell:** Spells are learned permanently. If a character already knows a spell, the shop must indicate this and prevent re-purchase for that character.

### 3.10 Progression & Story System

#### Progression Flags
FF1's progression is gated by key items and boss defeats. Implement as a `HashSet<string>` of flag IDs:

```
FLAG_GARLAND_DEFEATED
FLAG_BRIDGE_BUILT
FLAG_SHIP_OBTAINED
FLAG_MYSTIC_KEY
FLAG_CANOE_OBTAINED
FLAG_AIRSHIP_OBTAINED
FLAG_EARTH_CRYSTAL_LIT
... etc.
```

#### Key Progression Sequence (roughly)
1. Rescue Princess → Bridge Built → Access to wider world
2. Get Ship → Ocean travel
3. Get Canoe → River travel
4. Get Airship → Full world access
5. Defeat 4 Fiends → Restore 4 Crystals
6. Enter Temple of Chaos → Final Boss

NPCs check flags to determine dialogue. Warp tiles / doors check flags for access.

**Hard problem — soft locks:** FF1's design allows some sequence flexibility. The flag system must not create impossible states. All flag prerequisites should be documented and validated.

### 3.11 Dialogue System

- Simple sequential text boxes (no branching dialogue trees)
- NPC dialogue changes based on progression flags
- Some NPCs give key items or trigger events
- Text speed: instant or typewriter-style (player preference)

```csharp
[System.Serializable]
public class DialogueEntry
{
    public string RequiredFlag;    // empty = default
    public string[] Lines;         // sequential text boxes
    public string SetFlagOnComplete;
    public KeyItem GiveItem;
}
```

### 3.12 Save System

#### Save Slots
- 3-4 manual save slots (matching Pixel Remaster)
- 1 auto-save slot (saves on map transitions)
- Quick save (save anywhere, consumed on load — prevents save scumming)

#### Serialized State
```
SaveData
├── PartyData[]           — stats, level, EXP, class, equipment, known spells, status
├── InventoryData         — all items and quantities
├── Gold                  — current gil
├── ProgressionFlags      — all set flags
├── WorldState            — opened chests, defeated bosses, NPC states
├── LocationData          — current scene, player grid position, facing direction
├── VehiclePositions      — ship and airship world coordinates
├── PlayTime              — total elapsed time
└── Settings              — text speed, battle speed, etc.
```

**Format:** JSON for readability during development. Can switch to binary for release.

**Edge case — save corruption:** Write to temp file first, then atomic rename. Keep a backup of the previous save. Validate save data on load (version check, checksum).

**Edge case — mid-battle saving:** FF1 Pixel Remaster only allows saving at save points (world map, specific spots in towns). Never mid-battle, never mid-dungeon (except quick save). Enforce save location restrictions.

---

## 4. Procedural Rendering Strategy

### 4.1 Design Philosophy

Everything is rendered with **colored geometric primitives and Unity's built-in drawing tools**. The visual language is: "readable board game, not placeholder programmer art." Every element should be **instantly recognizable** for what it represents, even without sprites.

### 4.2 Tilemap Rendering

Generate `Sprite` assets at runtime using `Texture2D`:

| Tile Type | Visual |
|---|---|
| Grass | Green square with slight shade variation |
| Forest | Dark green square with triangle tree shapes |
| Mountain | Brown square with zigzag peak pattern |
| Water (ocean) | Blue square with wavy line pattern |
| Water (river) | Light blue square, narrower |
| Desert | Yellow/tan square with dot pattern |
| Town entrance | Gray square with door/arch shape |
| Dungeon entrance | Dark gray square with cave shape |
| Floor (indoor) | Beige/gray square |
| Wall | Dark square |
| Lava | Orange-red square with flicker (animated) |
| Chest | Brown small square on floor tile |

Implementation: `ProceduralTileFactory` class that creates `Texture2D` at startup, caches as `Sprite`, assigns to tilemap. All colors defined in a `TilePalette` ScriptableObject for easy theming.

### 4.3 Character Rendering

Party members on the exploration map:
- **Colored circle** with a 1-2 letter abbreviation of their class (W, Th, Mk, RM, WM, BM)
- Lead character visible on map; others hidden (FF1 only shows leader)
- Facing direction indicated by a small triangle/arrow on the circle's edge

Party members in battle:
- Larger colored rectangles with class abbreviation and HP bar
- Distinct color per class (red=Warrior, green=Thief, orange=Monk, purple=RedMage, white=WhiteMage, blue=BlackMage)
- Idle animation: subtle bob (PrimeTween)
- Attack animation: lunge forward and back
- Damage animation: flash red
- KO state: grayed out, slumped (rotated slightly)

### 4.4 Enemy Rendering

Enemies are **composite geometric shapes** assembled from basic primitives:
- Small enemies: single colored shape (circle, triangle, diamond)
- Medium enemies: 2-3 shapes composed (body + head, etc.)
- Large enemies/bosses: larger composite with more shapes, distinct color scheme

Each `EnemyData` ScriptableObject includes a `ProceduralAppearance` field:
```csharp
[System.Serializable]
public class ProceduralAppearance
{
    public ShapeComposite[] Shapes;
    public Color PrimaryColor;
    public Color SecondaryColor;
    public float Scale;
}

[System.Serializable]
public class ShapeComposite
{
    public ShapeType Type;   // Circle, Rect, Triangle, Diamond, Hexagon
    public Vector2 Offset;
    public Vector2 Size;
    public Color Color;
}
```

**Tradeoff:** This will look abstract. It's intentional — the game is readable, and the system is easy to extend when real art is eventually added. The appearance data is fully decoupled from gameplay data.

### 4.5 Spell & Battle Effects

- **Damage numbers:** Floating text that rises and fades (PrimeTween)
- **Physical attack:** Quick lunge animation + white flash on target
- **Fire spells:** Orange/red particle burst on target
- **Ice spells:** Blue/white particle burst
- **Lightning spells:** Yellow flash + screen shake
- **Healing:** Green rising particles
- **Buff/debuff:** Colored ring pulse around target
- **Death/instant KO:** Target fades out

Use Unity's built-in `ParticleSystem` with simple shapes (no imported textures — use default particle sprite).

### 4.6 Screen Shake & Juice

Even without art, game feel matters:
- Screen shake on big hits and boss attacks
- Flash on critical hits
- Slight camera zoom on spell cast
- PrimeTween for all of these

---

## 5. UI/UX Design

### 5.1 UI Framework Choice

**Use uGUI** (already installed). Do NOT use UI Toolkit — uGUI has better runtime creation support for procedural content and wider community support for game UIs in Unity 6.

All UI uses **solid-colored panels with borders** — no sprite-based frames. A consistent "UI skin" defined by:
- Background: dark blue (`#000044`)
- Border: white, 2px
- Text: white, with shadow
- Selection cursor: white arrow/triangle
- Font: Unity's default monospace or any bundled open-license pixel font

This mimics the FF1 "blue window" aesthetic without needing any textures.

### 5.2 Screen Layouts

#### Title Screen
```
┌────────────────────────────────┐
│                                │
│      FINAL FANTASY             │
│                                │
│        ▶ New Game              │
│          Continue              │
│                                │
└────────────────────────────────┘
```

#### Party Creation
```
┌─ Choose your Warriors of Light ─┐
│                                  │
│  Slot 1: [Warrior ▶]  Name: ___ │
│  Slot 2: [Thief   ▶]  Name: ___ │
│  Slot 3: [Monk    ▶]  Name: ___ │
│  Slot 4: [W.Mage  ▶]  Name: ___ │
│                                  │
│  ┌─ Preview ──────────┐         │
│  │ HP: 35  STR: 20    │         │
│  │ MP: 0   AGI: 5     │         │
│  │ VIT: 10 INT: 1     │         │
│  └────────────────────┘         │
│              [Confirm]           │
└──────────────────────────────────┘
```

#### Exploration HUD
Minimal overlay:
```
┌──────┐
│ Gil  │                    ┌─────┐
│ 1240 │                    │ Map │ (minimap toggle)
└──────┘                    └─────┘
```

#### Main Menu (pause menu over exploration)
```
┌─────────────────────────────────────────────┐
│ ▶ Items      │ Warrior  Lv 12  HP 245/245  │
│   Magic      │ Thief    Lv 11  HP 189/210  │
│   Equipment  │ W.Mage   Lv 12  HP 156/156  │
│   Status     │ B.Mage   Lv 11  HP 134/148  │
│   Config     │                              │
│   Save       │ Gil: 24,580                  │
│              │ Time: 4:32:10                │
└─────────────────────────────────────────────┘
```

#### Battle Screen
```
┌─────────────────────────────────────────────────┐
│                                                  │
│   [Enemy shapes]              [Party members]    │
│   Goblin A                    Warrior  HP 245    │
│   Goblin B                    Thief    HP 189    │
│   Wolf                        W.Mage   HP 156    │
│                               B.Mage   HP 134    │
│                                                  │
├──────────────────────────────────────────────────┤
│ ▶ Attack                                         │
│   Magic      Warrior's turn                      │
│   Item                                           │
│   Defend                                         │
│   Flee                                           │
└──────────────────────────────────────────────────┘
```

#### Shop Screen
```
┌─ Weapon Shop ──────────────────────────────────┐
│ ▶ Broadsword   1500g  ATK+15  [W  K  RW]      │
│   Longsword    1000g  ATK+10  [W  K  RW  Th]  │
│   Dagger        200g  ATK+5   [ALL]            │
│                                                 │
│ ┌─ Equipped ─────────┐ ┌─ Stats ─────────────┐ │
│ │ Warrior: Rapier    │ │ ATK: 25 → 35 (+10) │ │
│ └────────────────────┘ └─────────────────────┘ │
│                            Gil: 24,580          │
└─────────────────────────────────────────────────┘
```

### 5.3 Input Mapping (FF1-specific)

Replace the current template input actions with:

| Action | Keyboard | Gamepad | Context |
|---|---|---|---|
| Move | WASD / Arrow keys | D-pad / Left stick | Exploration |
| Confirm | Z / Enter | A (South) | Menu, Dialogue, Battle select |
| Cancel | X / Backspace | B (East) | Menu back, Battle cancel |
| Menu | Escape / C | Start | Open/close pause menu |
| Run | Left Shift (hold) | B (hold) | Exploration (walk faster) |
| Toggle Minimap | M | Select | Exploration |

### 5.4 Navigation & Accessibility

- All menus are **cursor-based** (list navigation, not mouse pointer)
- Mouse/touch NOT supported initially (FF1 feel is controller/keyboard)
- Wrap cursor at list boundaries (pressing up on first item goes to last)
- Confirm/Cancel sounds (stub audio calls)
- Text box auto-advance option and manual advance

---

## 6. Audio System (Stub)

### 6.1 Architecture

Build the full audio routing and API now so audio drops in seamlessly later.

```csharp
public class AudioManager : MonoBehaviour
{
    // BGM
    public void PlayBGM(MusicTrack track, float fadeDuration = 1f);
    public void StopBGM(float fadeDuration = 1f);
    public void PauseBGM();
    public void ResumeBGM();

    // SFX
    public void PlaySFX(SoundEffect sfx);

    // Volume
    public float MasterVolume { get; set; }
    public float BGMVolume { get; set; }
    public float SFXVolume { get; set; }
}

public enum MusicTrack
{
    Title, PartyCreation, WorldMap, Battle, BattleVictory, BossBattle,
    Town, Dungeon, Castle, Ship, Airship, GameOver, FinalBoss,
    CrystalRoom, Ending,
    // Per-town and per-dungeon overrides as needed
}

public enum SoundEffect
{
    Confirm, Cancel, Cursor, Hit, CriticalHit, Miss, MagicCast,
    Heal, Buff, Debuff, EnemyDeath, CharacterDeath, LevelUp,
    ChestOpen, DoorOpen, ShopBuy, ShopSell, Error, EncounterStart,
    FleeSuccess, FleeFail, SaveGame, LoadGame,
}
```

### 6.2 Behavior Without Assets

When no `AudioClip` is assigned to a track/effect:
- **Log a debug message** (not a warning/error — it's expected during development)
- **Do not throw, do not no-op silently** — the log helps verify audio calls are in the right places
- All volume/fade logic still executes (just on null clips) so it's testable

### 6.3 Integration Points

Every system that would play audio calls `AudioManager` with the appropriate enum. When real clips are ready, they're assigned in an `AudioDatabase` ScriptableObject that maps enums to clips. Zero code changes needed.

---

## 7. Edge Cases & Known Quirks

### 7.1 Battle Edge Cases

| Case | Behavior |
|---|---|
| Target dies before action | Auto-retarget to random living enemy (PR behavior) |
| All enemies die mid-turn | End battle immediately, skip remaining actions |
| Heal an enemy (confused ally) | Apply healing to enemy (yes, this can happen) |
| Cast offensive spell on party (reflect/confuse) | Apply damage to ally as normal |
| Overkill (more damage than remaining HP) | HP floors at 0, display full damage number |
| Revive in battle then die again same turn | Allowed; no invincibility frames |
| Flee from boss | Disable flee button entirely for boss encounters |
| Full party Petrified | Game Over (stone = functionally dead) |
| Self-targeting confusion | Confused character can hit themselves |
| Status effect on immune target | "Ineffective" message, no effect applied |
| Double status application | No stacking; re-applying Sleep on sleeping target is wasted |

### 7.2 Exploration Edge Cases

| Case | Behavior |
|---|---|
| Walk into water without ship/canoe | Blocked (tile impassable) |
| Disembark ship onto impassable tile | Block disembark; must find valid adjacent tile |
| Airship land on forest/mountain | Block landing; must find open tile |
| All party KO on world map | Game Over |
| Open already-opened chest | "Already empty" message or chest visually opened |
| Use key item at wrong location | Nothing happens; no error message |
| NPC blocks path | Player cannot walk through NPCs |
| World map edge (wrapping) | Seamless wrap to opposite side |

### 7.3 Inventory Edge Cases

| Case | Behavior |
|---|---|
| Buy item at 99 stack | "Inventory full" — block purchase |
| Sell last of a stack | Remove item entry entirely |
| Use last Potion in battle | Item removed from list; cursor adjusts |
| Equip item that another member is using | Each character has their own equipment (no sharing) |
| Remove weapon on Monk | Attack power recalculates to bare-hand formula |
| Key item used automatically | Remove from key items after event trigger |

### 7.4 Save/Load Edge Cases

| Case | Behavior |
|---|---|
| Save during poison damage | Save with current HP (don't tick poison) |
| Load corrupted save | Display error, don't crash, offer other slots |
| Quick save then crash | Quick save persists immediately to disk |
| Load mid-dungeon quick save | Restore exact position, despawned chests, etc. |
| Save version mismatch | Migration code or reject with version error |

---

## 8. Concerns & Risks

### 8.1 Scope Creep
FF1 has **~80 enemy types, ~64 spells, ~100+ equipment pieces, ~20 maps, ~150 NPCs**. The data entry alone is massive. Risk: Getting bogged down entering data before systems are testable.

**Mitigation:** Build systems first with 3-5 representative entries per database. Get the full loop working (explore → encounter → battle → victory → explore). Populate remaining data afterward.

### 8.2 Procedural Visuals Readability
Without sprites, complex battle scenes with 6+ enemies could become a mess of overlapping shapes.

**Mitigation:** Strict layout grid for enemy positions. Size differentiation. Name labels always visible. Test with maximum enemy count (9 small enemies) early.

### 8.3 Formula Accuracy
The Pixel Remaster's exact formulas aren't publicly documented with 100% certainty. Community reverse-engineering exists but may have errors.

**Mitigation:** Implement formulas as isolated, unit-testable functions behind a `BattleConfig` ScriptableObject. Make coefficients tweakable without code changes. Prioritize "feels right" over "bit-perfect."

### 8.4 Performance: Tilemap Size
FF1's world map is ~256x256 tiles. At runtime procedural generation, creating 65,536 tile sprites could cause a startup spike.

**Mitigation:** There are only ~15 unique tile types. Generate 15 sprites, reuse across the tilemap. The tilemap itself is fine — Unity handles 256x256 tilemaps without issue.

### 8.5 Scene Transition Jank
Loading scenes additively (battle) while keeping exploration alive can cause frame hitches.

**Mitigation:** Keep the battle scene lightweight (no complex geometry). Preload battle prefabs at game start. Use async loading with a brief transition screen (screen fade).

### 8.6 State Consistency
Many systems interact: equipping a weapon changes battle stats, learning a spell changes battle options, progression flags change NPC dialogue and door access. A bug in one system can cascade.

**Mitigation:** Clear ownership of data. Party stats are computed (base + equipment + buffs), never cached incorrectly. Use events (`OnEquipmentChanged`, `OnFlagSet`) to propagate changes reactively rather than polling.

### 8.7 No Undo in Party Creation
FF1 has no respec. If we don't communicate this clearly, players will be frustrated by a bad party composition.

**Mitigation:** Show clear class descriptions and stat previews during party creation. Consider adding a "are you sure?" confirmation.

---

## 9. Tradeoffs & Decisions

### 9.1 ScriptableObjects vs JSON for Game Data

| | ScriptableObjects | JSON / CSV |
|---|---|---|
| Unity integration | Native — inspector editing, asset references | Requires custom importer |
| Version control | Binary by default (bad diffs) | Plain text (good diffs) |
| Runtime modification | Read-only at runtime (correct for game data) | Requires deserialization |
| Type safety | Full C# typing | String-based, needs validation |
| Tooling | Unity Inspector (designers love it) | External editors |

**Decision: ScriptableObjects.** Force text serialization in Editor Settings (already the Unity 6 default) so diffs are readable. The inspector workflow is too valuable to give up, and we have no external designers needing CSV access.

### 9.2 One Scene Per Area vs Streaming

| | Scene-Per-Area | Streaming/Chunks |
|---|---|---|
| Simplicity | Simple — clear boundaries | Complex chunk management |
| Memory | Loads one area at a time | Can preload adjacent |
| Transition | Brief load screen between areas | Seamless (if it works) |
| FF1 accuracy | FF1 has clear screen transitions | Pixel Remaster has brief fades |

**Decision: Scene-per-area with fade transitions.** FF1 has discrete areas. No benefit to streaming complexity.

### 9.3 Tilemap vs Custom Grid

| | Unity Tilemap | Custom Grid System |
|---|---|---|
| Collision | Built-in TilemapCollider2D | Manual collision checks |
| Painting | Tile Palette editor | Custom editor tool |
| Metadata | Limited (need companion data) | Full control |
| Wrapping | Not supported | Can implement |

**Decision: Unity Tilemap for rendering + custom `GridData` class for metadata.** Use Tilemap for visual placement and TilemapCollider2D for basic collision, but store passability/encounter/warp data in a parallel `GridData` ScriptableObject per map. This avoids fighting the Tilemap API for game logic while still using it for what it's good at.

### 9.4 Additive Battle Scene vs In-Scene Battle

| | Additive Scene | Same-Scene Overlay |
|---|---|---|
| Isolation | Full — separate camera, lighting | Must hide/disable exploration objects |
| State preservation | Exploration scene untouched | Everything still loaded |
| Complexity | Scene loading management | Object management |
| Camera control | Independent battle camera | Must swap cameras |

**Decision: Additive scene.** Cleaner separation. Battle has its own camera and canvas. Exploration state is naturally preserved.

### 9.5 Auto-Retarget vs "Ineffective" on Dead Target

**Decision: Auto-retarget** (Pixel Remaster behavior). Wasting a turn because your target died feels bad and isn't interesting strategic depth. Config toggle available for purists.

### 9.6 Encounter Step Counter vs Pure Random

**Decision: Step counter with variance** (authentic FF1). Pure random creates frustrating clusters. Step counter guarantees minimum steps between fights, which is better game feel.

### 9.7 Quick Save Implementation

| | Save State Snapshot | Checkpoint Flag |
|---|---|---|
| Fidelity | Exact state — position, HP, everything | Only records progress flags |
| Complexity | Must serialize full game state | Simple |
| Player expectation | "Save anywhere" | "Save progress" |

**Decision: Full state snapshot.** Players expect quick save to restore exactly where they were.

---

## 10. Implementation Phases

### Phase 1 — Foundation (Skeleton)
- GameManager singleton + state machine
- Scene loading infrastructure (Boot → Title → Exploration)
- Input system with FF1-specific action map
- Procedural tile factory + test map (small 20x20 grid)
- Player grid movement with collision
- Camera follow
- UI framework (blue windows, cursor navigation)
- Audio manager stub
- Save/load infrastructure (JSON serialization)

### Phase 2 — Party & Data
- Class definitions (all 6 base classes as SOs)
- Party creation screen (class select + naming)
- Stat system (base stats, growth tables, level-up)
- PartyManager with full stat computation
- Main menu (Items, Equipment, Magic, Status, Config)
- Equipment system (equip/unequip, stat recalc, class restrictions)
- Inventory system (add, remove, use items)

### Phase 3 — Battle System
- Battle scene (additive loading, layout, cameras)
- Turn order calculation
- Command input (Attack, Magic, Item, Defend, Flee)
- Physical damage formula (multi-hit system)
- Magical damage formula
- Enemy AI (weighted random)
- Status effects (at least KO, Poison, Sleep)
- Battle rewards (EXP, Gil, level-up)
- Flee mechanics
- Battle animations (lunge, flash, particles)
- Damage numbers

### Phase 4 — World Building
- World map tilemap (full 256x256 or representative subset)
- World map wrapping
- Town scenes (Cornelia as first town)
- Dungeon scenes (Temple of Chaos as first dungeon)
- NPC system + dialogue
- Shops (items, weapons, armor, magic)
- Treasure chests
- Warp tiles (stairs, town exits, dungeon transitions)
- Encounter tables per region
- Vehicles (ship, canoe, airship)

### Phase 5 — Content & Polish
- All enemy data (formations, AI profiles, appearances)
- All spell data
- All equipment and item data
- All maps, NPCs, shops, chests
- Class upgrades (Bahamut event)
- Boss encounters with scripted AI
- Progression flags for full game
- Key items and gating
- Damage floors, conveyor tiles, special dungeon mechanics
- Game Over screen
- Victory/ending sequence
- Config screen (text speed, battle speed, volume)
- Minimap

### Phase 6 — Hardening
- Save migration / versioning
- Edge case testing (all cases in Section 7)
- Balance pass (if formulas feel off, tune coefficients)
- Performance profiling (world map, large battles)
- Memory leak audit (scene loads/unloads)

---

## Appendix A: FF1 Class Stat Reference (Base Stats at Level 1)

| Class | HP | MP | STR | AGI | VIT | INT | LCK |
|---|---|---|---|---|---|---|---|
| Warrior | 35 | 0 | 20 | 5 | 10 | 1 | 5 |
| Thief | 30 | 0 | 5 | 15 | 5 | 5 | 15 |
| Monk | 33 | 0 | 10 | 5 | 10 | 3 | 5 |
| Red Mage | 30 | 10 | 10 | 8 | 5 | 10 | 5 |
| White Mage | 28 | 10 | 5 | 5 | 5 | 15 | 5 |
| Black Mage | 25 | 10 | 1 | 5 | 1 | 20 | 5 |

*Note: These are approximate values based on community reverse-engineering. Final values should be tuned during Phase 5.*

## Appendix B: Element Interaction Matrix (Template)

```
             Fire  Ice  Lit  Earth Water Wind  Dark  Holy
Fire Elem:    -    Weak  -    -     Weak  -     -     -
Ice Elem:   Weak    -    -    -      -    Weak  -     -
Lit Elem:     -     -    -   Weak    -     -    -     -
Earth Elem:   -     -   Weak   -     -    Weak  -     -
Undead:       Weak  -    -    -      -     -   Resist Weak
```

*Full matrix to be completed during data entry phase.*
