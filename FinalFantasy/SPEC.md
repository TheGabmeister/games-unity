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
| Input | Input System 1.19.0 | Built-in — no third-party input plugin needed. Replace template action map with FF1-specific actions. `InputSystemUIInputModule` drives uGUI menu navigation. |
| Tweening | PrimeTween 1.3.8 | Already installed; use for all animations |
| UI | uGUI 2.0.0 | Already installed; use for all menus and HUD |
| JSON | Newtonsoft JSON 3.2.1 | For save system serialization (Dictionary, HashSet, polymorphism) |
| Inspector | Tri-Inspector 1.14.1 | Editor attributes ([ShowIf], [Required], [Dropdown], validation) for SO editing |
| Async | Unity Awaitable (built-in) | async/await for battle flow, scene loading, dialogue, transitions. PrimeTween has native `GetAwaiter()` support — tweens are directly awaitable. |

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
                          ↕  ↕  ↕
                        Menu Dialogue Shop
```

The top-level FSM controls what input is routed where and what UI is visible. States are exclusive — you cannot be in Battle and Menu simultaneously. Menu, Dialogue, and Shop are **sibling sub-states** of Exploration, not a chain — each is entered independently (Menu from pause input, Dialogue from NPC interaction, Shop from shop NPC).

**Critical detail:** The `Exploration` state has sub-states for movement mode (Walking, Canoe, Ship, Airship). Each has different movement rules, encounter tables, and tile passability.

### 3.2 Party & Class System

#### Party Creation
- Player selects 4 members from 6 classes (duplicates allowed — you can run 4 Warriors)
- Each member gets a player-assigned name (max 6 characters, matching FF1)
- No respec — class choice is permanent until class upgrade event

#### Starting Game State
After party creation confirms, the game initializes with:
| Field | Value |
|---|---|
| Location | Cornelia Castle, throne room (post-rescue-princess opening in PR) or Cornelia town entrance |
| Party level | 1 (all members) |
| Gil | 400 |
| Equipment | Class-default starter weapon per member (Warrior: Broadsword, Thief: Dagger, Monk: none, Red Mage: Rapier, White Mage: Staff, Black Mage: Staff). No armor/shield/helmet. |
| Inventory | 0 consumables (player buys from Cornelia item shop) |
| Spells | None (player buys from Cornelia magic shops) |
| Key items | None |
| Progression flags | None set |
| Vehicles | Ship: not yet obtained. Canoe: not yet obtained. Airship: not yet obtained. |
| Encounter counter | Randomized initial value within Cornelia region's MinSteps-MaxSteps range |

*Exact values (Gil, starter weapons) should match Pixel Remaster. These are approximate — finalize during Phase 5 data entry.*

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
Experience:               Shared equally among living members after battle (dead members get 0)
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

#### Interaction
The player presses Confirm to interact. The system checks two locations:
1. **Current tile** (under the player) — for: Airship boarding (player is standing on the landed airship).
2. **Facing tile** (one tile in front) — for everything else:
   - NPC → trigger dialogue
   - Treasure chest → open and grant contents
   - Door → check key item requirements, open if met
   - Ship → board the ship
   - Shop counter → enter shop
   - Nothing → no response

Current-tile check runs first. This is a single "Interact" action, not separate buttons per interaction type.

#### Run Speed
Holding the Run button doubles movement speed (tile transition time halved from ~0.15s to ~0.075s). On the world map, running also doubles encounter step counter decrement rate — you fight more often per real-time second but cover more ground. In towns, no random encounters so run is purely a convenience.

#### Camera
- **World map:** Smooth follow centered on the player. Pixel-perfect rendering (snap camera to pixel grid to prevent sub-pixel jitter). Camera viewport shows ~15x10 tiles.
- **Towns/dungeons:** Same smooth follow, but camera is clamped to map bounds so it never shows void beyond the edges.
- **Battle:** Static camera, no follow. Ortho size set to frame the full battle layout.

Use URP's `PixelPerfectCamera` component (included in the 2D feature package) for crisp tile rendering.

#### Scene Transitions
- **Town/dungeon entry/exit:** Screen fades to black (0.3s), load new scene, fade in (0.3s). PrimeTween handles the fade via a full-screen UI overlay.
- **Battle encounter:** FF1 PR uses a distinctive screen flash + spiral/swirl effect. Implement as: white flash (0.1s) → screen shatters/dissolves to black (0.4s) → load battle scene → fade in battle (0.3s). Post-battle: fade out (0.2s) → restore exploration → fade in (0.2s).
- **Game Over:** Fade to red tint → "Game Over" text → fade to black → return to Title screen. Offer "Continue" which loads the most recent save.

#### Encounter Suppression
FF1 suppresses random encounters within a radius of ~2-4 tiles from town/dungeon entrances on the world map. Implement as a `SafeZone` flag on entrance-adjacent tiles or a distance check from warp points. This prevents the frustrating experience of getting ambushed immediately outside a town.

#### Tilemap Approach
Use Unity's built-in `Tilemap` system with **programmatically generated `Tile` assets** — no imported sprites. Each tile type gets a procedurally colored square (grass = green, water = blue, mountain = brown, etc.). Details in Section 6 (Rendering).

#### Tile Data
Each tile needs metadata beyond visual:
```csharp
[Flags]
public enum TilePassability
{
    None         = 0,
    OnFoot       = 1 << 0,
    Canoe        = 1 << 1,
    Ship         = 1 << 2,
    AirshipLand  = 1 << 3,  // Airship can land on this tile (flat open ground)
    AirshipFly   = 1 << 4,  // Airship can fly over this tile (nearly everything except void)
}
```

When airborne, movement checks `AirshipFly`. When landing (Confirm pressed), the tile under the shadow must have `AirshipLand`. Typical assignments: grass has both flags, forest has `AirshipFly` only, ocean has `AirshipFly` only, mountains have `AirshipFly` only.

Passability is checked against current movement mode. Water tiles allow Ship but not OnFoot. River tiles allow Canoe but not Ship.

#### World Map Specifics
- Wrapping: The FF1 world map wraps horizontally and vertically (it's a torus). The tilemap must wrap or use teleport triggers at edges.
- **Canoe:** NOT a placeable vehicle. Once obtained (key item), the player automatically enters canoe mode when walking onto a river tile and exits when stepping onto land. No boarding/exiting interaction needed. Canoe works on rivers only, not open ocean.
- **Ship:** A physical object placed in the world. Player boards by pressing Confirm while facing it, exits by pressing Confirm when adjacent to a walkable land tile. Ship is left where you disembark — you must return to it to sail again.
- **Airship:** A physical object. Board by pressing Confirm while standing on it (current-tile interaction, see Interaction section above). Flies above all terrain (checks `AirshipFly` passability). Land by pressing Confirm — only on tiles with the `AirshipLand` flag (flat open ground; not forests, mountains, or water). **No random encounters while airborne.** Airship stays where you land it.
- Airship shadow: Airship flies above terrain. Need a visual indicator of ground position for landing.

**Hard problem — world map wrapping:** Unity Tilemaps don't natively wrap. Options:
1. **Ghost border:** Duplicate edge tiles on opposite sides (wastes memory for large maps).
2. **Camera + position modulo:** Wrap player position mathematically and shift the camera. Tilemap doesn't know about wrapping.
3. **Chunked loading with virtual coordinates:** Most robust but most complex.

**Recommendation:** Option 1 (ghost border), refined. The world map is 256x256 tiles. The camera viewport is ~15x10 tiles. Duplicate a border strip of 8 tiles (half-viewport) on all four edges of the Tilemap, creating a 272x272 tile Tilemap where the outer 8 tiles on each side mirror the opposite edge. This costs 272² - 256² = ~8,000 extra tiles — negligible.

Player position uses modulo math (e.g., `x = ((x % 256) + 256) % 256`). When the player crosses from tile 255 to tile 0, the position wraps and the camera sees the ghost border tiles seamlessly. The `GridData` metadata also wraps via the same modulo lookup.

Collision at the seam works naturally because the ghost border tiles carry the same passability data as their source tiles. NPCs and vehicles near the seam are the one remaining concern — restrict vehicle parking and NPC patrol paths to at least 8 tiles from any map edge to avoid duplication logic.

#### Persistent IDs for World State

Every scene object that has persistent state (treasure chests, boss triggers, one-time NPC events, locked doors) gets a `PersistentID` MonoBehaviour:

```csharp
public class PersistentID : MonoBehaviour
{
    [SerializeField, ReadOnly] string id;   // auto-generated GUID, never hand-edited

    public string ID => id;

    #if UNITY_EDITOR
    void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
            id = System.Guid.NewGuid().ToString();
    }
    #endif
}
```

Complemented by an editor script (`PersistentIDValidator`) that:
- Runs on scene save and on play mode entry
- Scans all `PersistentID` components in the scene for duplicate `id` values
- Logs an error with the GameObjects involved so duplicates (from copy/paste) are caught immediately

The save system uses these IDs as keys: `WorldState[id] = true` means "this chest is opened" or "this boss is defeated." Because IDs are GUIDs serialized into scene data, they survive GameObject renames, hierarchy moves, and scene refactors. The only thing that breaks them is deleting and recreating the object — which is fine, since that's intentional.

#### Town & Dungeon Specifics
- No wrapping, bounded maps
- NPCs with schedules (FF1 NPCs are mostly static, some walk set paths)
- Treasure chests: opened state tracked in `ProgressionManager` via `PersistentID`
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
    public BattleBackground Background;   // procedural background used for all encounters in this table
    public EncounterEntry[] Encounters;    // weighted list of enemy formations
}

[System.Serializable]
public class EncounterEntry
{
    public EnemyFormation Formation;
    public int Weight;
}
```

#### Enemy Formations
```csharp
[CreateAssetMenu]
public class EnemyFormation : ScriptableObject
{
    public FormationSlot[] Slots;          // which enemies and where
    public bool IsBoss;                    // true = flee disabled, unique battle music
    public BattleBackground BackgroundOverride;  // null = use EncounterTable's default background
    public int SurpriseResistance;         // modifier to preemptive/ambush calculation (bosses = high)
}

[System.Serializable]
public class FormationSlot
{
    public EnemyData Enemy;
    public int Count;                     // how many of this enemy (e.g., 3x Goblin)
    public Vector2[] Positions;           // layout positions in battle scene (one per Count)
}
```

- Formations hold 1-9 total enemies across all slots (e.g., 3x Goblin + 2x Wolf = 5 enemies)
- `Positions` are normalized coordinates (0-1 range) mapped to the battle scene's enemy area. This allows the battle renderer to scale positions to screen size.
- Duplicate enemies within a slot are auto-labeled (Goblin A, Goblin B, Goblin C) by the battle UI.

**Edge case — preemptive/ambush:** Based on party average agility vs formation's average agility ± `SurpriseResistance`. Preemptive = party gets a free round (enemies skip turn 1). Ambush = enemies get a free round (party skips command input, enemies act first). Must factor into turn order initialization.

### 3.5 Battle System

This is the most complex system. FF1 Pixel Remaster uses a **turn-based system with speed-based ordering** (not ATB).

#### Battle Flow
```
1. Encounter triggered → load Battle scene additively
2. Determine surprise/preemptive/normal
3. TURN LOOP:
   a. For each party member (in party slot order 1→4): show command menu, collect action
   b. Assign enemy actions (AI selects based on weighted random)
   c. Sort ALL queued actions (party + enemy) by effective agility (ties broken randomly)
   d. Execute actions in order:
      - Check if actor is alive/able to act (status effects)
      - Check if target is still valid
      - Execute action (attack/spell/item/defend/flee)
      - Apply results (damage, healing, status, death)
      - Check for battle end (all enemies dead OR all party dead OR fled)
   e. Apply end-of-turn effects (poison tick, regen, etc.)
   f. If battle not over → next turn (back to step a)
4. Victory:
   a. Award EXP equally to all living party members (KO'd/Stone'd get nothing)
   b. Award Gil (flat amount per enemy in formation)
   c. Roll item drops (per-enemy drop table, % chance)
   d. Check level-ups for each member — display stat gains
4e. Level-up display: for each member who leveled, show a stat comparison panel:
      ```
      Warrior reached Level 13!
      HP: 245 → 268 (+23)
      STR: 32 → 33 (+1)
      AGI: 12 → 12
      ...
      ```
      Player presses Confirm to dismiss each level-up. Multiple level-ups from a single battle are shown sequentially.
5. Defeat: Game Over screen → return to Title (offer Continue from last save)
6. Unload Battle scene, restore exploration camera and input
```

#### Command Menu (per party member)
```
Attack  — physical attack on one enemy
Magic   — opens spell list (White/Black by level)
Use     — cast a free spell from equipped item (only visible if character has equipment with CastableSpell)
Item    — use consumable from inventory
Defend  — halve incoming physical damage this turn, guaranteed to act first
Flee    — entire party attempts to run (see Flee Mechanics below)
```

The **Use** command is conditionally shown — it only appears if at least one of the character's equipped items has a non-null `CastableSpell`. Selecting it opens a sub-list of castable equipment (e.g., "Healing Staff → Cure"). The spell is cast at no MP cost, targeting follows the spell's normal `SpellTarget` rules. During **auto-battle**, Use is never auto-selected — auto-battle only repeats Attack or Magic commands.

#### Auto-Battle (Pixel Remaster Feature)
Toggled via a dedicated button (e.g., Right Trigger / Tab). When active:
- Each party member **repeats their last-used command** automatically
- If no previous command exists (first turn), default to basic Attack on a random enemy
- Auto-battle skips the command input phase entirely — turns execute at accelerated speed
- Player can cancel auto-battle at any time by pressing the toggle or any command input
- Auto-battle does NOT auto-use items or auto-flee — only Attack and Magic
- If a party member's repeated spell has insufficient MP, they fall back to Attack

This is a core QoL feature of the Pixel Remaster and should be implemented in Phase 3.

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

**Healing Formula:**
```
Heal Amount = spell.BasePower + (Intellect / 2) + random(0, spell.BasePower)
```
Healing always hits (no accuracy roll). Multi-target heals (Healara on AllAllies) use the same formula per target.

**Undead Healing Inversion:** Cure-type spells and healing items deal damage to Undead enemies instead of healing. The damage equals what the heal amount would have been. This also works in reverse — an Undead enemy using a Cure spell on itself takes damage (relevant for AI edge cases).

**Buff/Debuff Spells:**
| Spell Type | Mechanic | Duration | Stacking |
|---|---|---|---|
| Haste | Doubles hit count | Until battle ends | No — reapplying refreshes, doesn't double again |
| Temper (NulDeath-era: TMPR) | +14 Attack per cast | Until battle ends | Yes — stacks additively, capped at +56 (4 casts) |
| Protect (FOG) | +8 Defense per cast | Until battle ends | Yes — stacks additively |
| NulFire/NulIce/NulLit | Grants resistance to element | Until battle ends | No — on/off |
| Saber | +16 Attack + weapon becomes elemental | Until battle ends | No |
| Slow | Halves hit count (enemies) | Until battle ends | No |

Buffs are tracked as a `BattleBuffState` per actor, cleared when battle ends. Persistent status effects (Poison, Blind) carry over to the field; battle-only buffs do not.

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

#### Enemy Data Structure
Each enemy type is a single `EnemyData` ScriptableObject that ties together stats, rewards, AI, and visuals:
```csharp
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string EnemyName;
    public EnemyCategory Category;        // Normal, Boss, Undead, Dragon, etc.

    [Header("Stats")]
    public int MaxHP;
    public int Attack;
    public int Defense;
    public int Accuracy;
    public int Evasion;
    public int MagicDefense;
    public int MagicEvasion;
    public int CritRate;
    public int HitCount;                  // enemies also use multi-hit

    [Header("Elemental Profile")]
    public ElementalAffinity[] Affinities; // Weak, Resist, Null, Absorb per element
    public StatusEffect[] StatusImmunities;

    [Header("Rewards")]
    public int EXPReward;
    public int GilReward;
    public ItemDrop[] DropTable;          // item + drop % chance

    [Header("Behavior & Appearance")]
    public EnemyAIProfile AIProfile;
    public ProceduralAppearance Appearance;
}

[System.Serializable]
public class ItemDrop
{
    public ItemData Item;
    public float DropRate;               // 0.0 - 1.0
}
```

The `Category` field matters for mechanics: `Undead` enemies take damage from healing magic and Cure items, `Boss` enemies block flee and are immune to instant-death.

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
Flee is a **whole-party action**, not a per-character action. This creates a special case in the per-actor command input flow:

1. When **any** party member selects Flee, command input for remaining party members is **skipped** — all party members are committed to the flee attempt.
2. The flee action is always resolved **first** in the turn, before any other actions (it ignores agility sorting).
3. Success rate: `(PartyAvgLevel * 2 + 80 - EnemyAvgLevel) / 256` per attempt.
4. On **success:** Battle ends immediately. No EXP, no Gil, no drops. Return to exploration.
5. On **failure:** The flee attempt wastes the party's entire turn. All queued party actions are discarded. Enemies still execute their full round of actions.
6. Boss encounters: Flee option is **hidden** (not greyed out — removed entirely) from the command menu.
7. If the first party member selects Flee, the player can still Cancel back and choose a different command before confirming.

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
    public bool UsableInField;           // Cure, Poisona, etc. can be used outside battle
    public bool UsableInBattle;          // most spells are battle-only; some buffs are field-usable
}
```

#### Spell Learning
In Pixel Remaster, spells are **purchased from shops** (White/Black magic shops in towns), not learned by level. Each class has a list of learnable spell levels. The Pixel Remaster **removed the NES 3-spells-per-level limit** — a character can learn all available spells at each level for their class.

**Edge case — already known spell:** If a character already knows a spell, the shop must indicate this and prevent re-purchase for that character. Other party members who can learn it should still be offered the option.

**Edge case — class upgrade spell access:** When a class upgrades (e.g., Warrior → Knight), they gain access to new spell levels. But they don't automatically learn spells — they still need to buy them.

**Edge case — no valid learner:** If no party member can learn a spell (wrong school or already known by all eligible members), grey it out in the shop but still show it so players know it exists.

### 3.7 Equipment System

#### Slots
Each character has: **Weapon**, **Shield**, **Helmet**, **Armor** (body)

#### Equipment Type Classification
```csharp
public enum WeaponType
{
    Sword, Dagger, Axe, Hammer, Staff, Nunchaku, Katana, Fist  // Fist = Monk bare hands (virtual)
}
public enum ArmorType
{
    Shield, LightArmor, HeavyArmor, Robe, Helmet, HeavyHelmet, Hat, Armlet, Gloves
}
```
Each class defines a whitelist of allowed `WeaponType` and `ArmorType` values. This is more maintainable than per-item class flags and lets us validate equipment compatibility at the type level.

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
    public bool TwoHanded;    // if true, blocks shield slot when equipped
    public Element[] ElementalResist;
    public StatusEffect[] StatusResist;
    public WeaponType WeaponType;     // only set when Slot == Weapon
    public ArmorType ArmorType;       // only set when Slot == Shield, Helmet, or Armor
    public ClassFlag AllowedClasses;   // derived from class weapon/armor whitelists at edit time; stored per item for runtime lookup
    public SpellData CastableSpell;    // some equipment casts spells when used in battle
    public int BuyPrice;
    public int SellPrice;             // usually half buy price
    public StatModifier[] StatBonuses; // Str+5, etc.
}
```

**Edge case — two-handed weapons:** Some weapons (e.g., Masamune, some axes/hammers) are two-handed — equipping one removes the shield slot. The equipment UI must enforce this: selecting a two-handed weapon auto-unequips the shield and greys out the shield slot. Conversely, equipping a shield with a two-handed weapon equipped should prompt a swap.

**Edge case — equipment spell casting:** Some weapons/armor can be "used" in battle to cast a free spell (no MP cost). This adds a hidden "Use" command in battle when such equipment is worn. Must be surfaced in the battle UI.

**Edge case — optimizing equipment:** The Pixel Remaster has an "Optimize" button that auto-equips best gear. "Best" is not obvious — it optimizes for attack power (weapons) and defense (armor). Must implement a sorting heuristic. Two-handed weapons should be considered if their attack power exceeds weapon+shield combined benefit.

### 3.8 Inventory & Party Management

#### Inventory
- Consumable items: Potions, Ethers, Phoenix Down, status cures, etc.
- Key items are **not** stored in inventory — they live in `ProgressionManager` (see Section 3.10)
- Stack limit: 99 per item type
- Total inventory: uncapped in Pixel Remaster (NES had limits)
- Items can be used in field (healing) or battle (healing, damage, status cure)
- **Gil cap:** 999,999 (Pixel Remaster cap). Earning Gil beyond this is silently discarded. Display in UI should accommodate 6 digits.

#### Party Order
The main menu includes a **party reorder** option (called "Formation" or "Order"). This changes:
- Which character sprite appears on the exploration map (always slot 1)
- The order of command input in battle (slot 1 first)
- Targeting priority for enemy single-target attacks (front slots slightly more likely)

Reorder is drag/swap between the 4 slots. It does NOT affect stats or abilities — purely positional.

### 3.9 Shop System

#### Shop Types
- **Weapon shops** — sell weapons
- **Armor shops** — sell shields, helmets, armor
- **White magic shops** — sell White spells (by level)
- **Black magic shops** — sell Black spells (by level)
- **Item shops** — sell consumables
- **Inn** — pay Gil to fully restore all party HP and MP, cure all status effects, **and revive KO'd members at full HP**. Each town has a different Inn price. Inn also acts as a "soft save point" — prompt to save after resting.
- **Clinic** — revive KO'd party members for a per-character fee (alternative to Phoenix Down). Available in some towns.

#### Shop UI Flow
```
Enter Shop → Buy/Sell toggle → Browse items → Select item → Choose quantity (1-99) → Confirm → Update gold
```

For equipment shops, selecting an item shows a **stat comparison** per party member (current equipped vs. new item, with green/red delta). For magic shops, show which party members can learn each spell and whether they already know it.

**Multi-quantity:** Item shops and sell mode support buying/selling multiple at once. Equipment and magic are always single transactions (you buy one sword, not five).

**Edge case — spell shops:** Show which party members can learn each spell inline. See Section 3.6 for full spell purchase edge cases (already owned, no valid learner, class upgrade access).

### 3.10 Progression & Story System

#### Progression Flags & Key Items

**Single source of truth:** `ProgressionManager` owns both flags and key items. The inventory system does NOT store key items — it only stores consumables and equipment. Key items are a `List<KeyItemData>` on `ProgressionManager`, displayed in a dedicated "Key Items" tab in the menu UI (read-only, no use/drop actions).

Each `KeyItemData` ScriptableObject defines:
```csharp
public class KeyItemData : ScriptableObject
{
    public string ItemName;
    public string Description;
    public string GrantsFlag;            // flag auto-set when this item is obtained (e.g., "FLAG_MYSTIC_KEY")
    public bool ConsumedOnUse;           // true = removed after triggering its event (e.g., Star Ruby). false = permanent (e.g., Mystic Key, Canoe)
}
```

When a key item is obtained, `ProgressionManager` adds it to the key item list AND sets its `GrantsFlag`. Systems that check access (locked doors, vehicle availability, NPC dialogue) read **flags**, not the key item list directly. This means consumed key items still leave their flag behind — the door stays unlocked even after the key is gone.

FF1's progression is additionally gated by boss defeats and story events. All gates are stored as a `HashSet<string>` of flag IDs:

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

#### Save Types & Availability

| Save Type | Slots | When Available | Behavior |
|---|---|---|---|
| **Manual save** | 4 numbered slots | World map only, or at designated save points in towns (via the inn prompt or specific NPCs). **Never** in dungeons, battles, or menus. | Persistent. Overwrites selected slot. |
| **Auto-save** | 1 hidden slot | Triggers automatically on: entering/exiting a town or dungeon, resting at an inn, completing a boss fight. | Persistent. Silently overwrites. Player cannot write to this slot manually. |
| **Quick save** | 1 hidden slot | Available anywhere during Exploration state (world map, towns, dungeons). **Never** during battle. Accessed from the pause menu. | **Consumed on load** — deleted after restoring. Prevents save-scumming. |

The pause menu **Save** option is context-sensitive:
- On the world map or at a save point: shows manual save slot picker + quick save option.
- In a dungeon or non-save-point town area: shows quick save option only.
- During battle: Save option is hidden entirely.

**Continue (Title screen)** loads the most recent save across all types, determined by timestamp. The Title screen shows: slot name, location, play time, and level for each occupied slot.

#### Serialized State
```
SaveData
├── SaveType              — Manual / Auto / Quick (so loader knows if to consume)
├── SaveVersion           — schema version for migration
├── Timestamp             — UTC datetime of save
├── PartyData[]           — stats, level, EXP, class, equipment, known spells, status
├── InventoryData         — all items and quantities
├── KeyItems[]            — owned key items (see Section 3.10)
├── Gold                  — current gil
├── ProgressionFlags      — all set flags
├── WorldState            — opened chests, defeated one-time enemies, NPC trigger states
├── LocationData          — current scene, player grid position, facing direction
├── VehiclePositions      — ship and airship world coordinates
├── EncounterCounter      — current step counter value (don't reset on load)
├── PlayTime              — total elapsed time
└── Settings              — text speed, battle speed, etc.
```

**Format:** JSON (Newtonsoft) for readability during development. Can switch to binary for release.

**Edge case — save corruption:** Write to temp file first, then atomic rename. Keep a backup of the previous save. Validate save data on load (version check, checksum).

**Edge case — quick save consumed on crash:** Quick save writes to disk immediately. If the game crashes after loading a quick save but before the deletion completes, the quick save survives. This is acceptable — the alternative (deleting before loading) risks data loss on crash.

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

### 4.5 Battle Background

The battle scene needs a background that conveys the environment where the encounter occurred. Since we have no art:

| Context | Background |
|---|---|
| World map (grass) | Solid green gradient, bottom-to-top (dark→light) |
| World map (forest) | Dark green with triangle tree silhouettes |
| World map (ocean/ship) | Blue gradient with horizontal wave lines |
| Dungeon (cave) | Dark gray/brown with rough edge shapes on sides |
| Dungeon (castle) | Dark blue-gray with vertical column shapes |
| Dungeon (volcano) | Dark red/orange with flickering glow at bottom |
| Boss room | Black background with subtle animated particle effect |

The background is a `BattleBackground` ScriptableObject referenced by each `EncounterTable`. A simple `SpriteRenderer` or UI `Image` with a procedurally generated gradient texture. Keep it minimal — the focus is on the actors, not the backdrop.

### 4.6 Spell & Battle Effects

- **Damage numbers:** Floating text that rises and fades (PrimeTween)
- **Physical attack:** Quick lunge animation + white flash on target
- **Fire spells:** Orange/red particle burst on target
- **Ice spells:** Blue/white particle burst
- **Lightning spells:** Yellow flash + screen shake
- **Healing:** Green rising particles
- **Buff/debuff:** Colored ring pulse around target
- **Death/instant KO:** Target fades out

Use Unity's built-in `ParticleSystem` with simple shapes (no imported textures — use default particle sprite).

### 4.7 Screen Shake & Juice

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
│   Order      │                              │
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
| Auto-Battle | Tab | Right Trigger | Battle (toggle) |

### 5.4 Menu Navigation Architecture

Build on **uGUI's Selectable system** with `InputSystemUIInputModule`. This gives us keyboard, gamepad, and mouse support from the same code. Input mode hot-switches automatically — if the player moves the mouse, hover/click works; if they press d-pad or arrow keys, cursor-based selection activates. No manual mode switching needed.

#### Three navigation tiers

**Tier 1 — Simple menus (use raw Selectables):**
Title screen, pause menu, config screen, inn/clinic confirmations, yes/no dialogs. Each menu item is a `Button` with `Navigation` set to `Explicit`. Wire Up/Down/Left/Right in the Inspector. Call `EventSystem.SetSelectedGameObject()` to set initial selection on screen open.

No custom code beyond wiring. Mouse clicks and gamepad/keyboard both work via uGUI's built-in routing.

**Tier 2 — Dynamic list menus (reusable `CursorList` component):**
Item inventory, shop stock, equipment list, spell list. These are scrollable lists where entries change at runtime (buying/selling items, filtering equipment by class+slot).

`CursorList` responsibilities:
- Spawns `Selectable` entries from a data source
- Wires explicit navigation between entries (re-wires when list changes)
- Handles wrap-around (last item → first item and vice versa)
- Manages scroll viewport (keeps selected item visible)
- Fires `OnItemSelected(int index)` and `OnItemConfirmed(int index)` events
- Supports greyed-out but navigable entries (e.g., spells you can't learn)

One component, reused across every list-based screen. Each screen provides its own data binding and item template.

**Tier 3 — Battle target selector (fully custom):**
Navigates enemy/ally positions in 2D space, not a list. Highlights the geometric shape under the cursor. Supports switching between single-target and multi-target (all-enemies, all-allies) based on the selected spell/item. Cycles through valid targets with Left/Right (enemies) or Up/Down (allies). Mouse click on a target also works via collider raycasts.

This shares nothing with menu navigation — it is its own input handler active only during target selection within the battle command flow.

#### General navigation rules
- Wrap cursor at list boundaries (pressing up on first item goes to last)
- Confirm/Cancel audio calls on every selection (stub AudioManager)
- Cancel always returns to the parent context (never closes multiple layers at once)
- Opening any menu forces an initial selection (no "nothing selected" state)
- Text boxes: Confirm to advance, auto-advance option in config, typewriter effect skippable by pressing Confirm mid-line

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
| Cure spell on Undead enemy | Deals damage equal to heal amount (see Section 3.5) |
| Cure item (Potion) on Undead | Also deals damage — same inversion rule |
| Buff on dead character | Wasted — buffs require living target |
| Haste on Hasted character | Refreshes duration, does NOT double again |
| Silence on character mid-spell-select | In FF1 PR, Silence is checked at execution time, not input time — spell fizzles |
| Enemy uses spell with 0 remaining targets | Action skipped entirely |
| Auto-battle with insufficient MP | Falls back to basic Attack |

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
| Enter canoe tile without canoe key item | Blocked (river tile treated as impassable) |
| Board ship from wrong side | Must be directly adjacent and facing the ship tile |
| Airship flying — no encounters | Step counter paused while airborne |
| Walk onto damage floor with 1 HP | Character KO'd; check for party wipe |
| Party leader is KO'd | Show KO'd sprite on map; can still move (party isn't dead until all 4 are) |

### 7.3 Inventory Edge Cases

| Case | Behavior |
|---|---|
| Buy item at 99 stack | "Inventory full" — block purchase |
| Sell last of a stack | Remove item entry entirely |
| Use last Potion in battle | Item removed from list; cursor adjusts |
| Equip item that another member is using | Each character has their own equipment (no sharing) |
| Remove weapon on Monk | Attack power recalculates to bare-hand formula |
| Key item with `ConsumedOnUse` triggered | Remove from `ProgressionManager.KeyItems`, but flag remains set (see Section 3.10) |

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

### 8.7 EXP Tables & Multi-Level-Up
Each class has its own EXP-to-level curve (Warriors need less EXP than Mages to reach the same level). These are ~99-entry tables per class that must be data-driven. A single high-EXP battle can trigger **multiple level-ups at once** (e.g., fighting a boss 10 levels above you). The level-up display must handle showing stat gains for each level sequentially, and stat growth must be applied per-level (not just final delta) since growth includes random variance per level.

### 8.8 No Undo in Party Creation
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
- GameManager singleton + state machine (with sub-states for exploration modes)
- Scene loading infrastructure (Boot → Title → Exploration) with fade transitions
- Input system with FF1-specific action map (replace template actions)
- Procedural tile factory + test map (small 20x20 grid)
- Player grid movement with collision + interaction system (Confirm to interact)
- Run speed toggle
- Camera follow (pixel-perfect, clamped to bounds)
- UI framework (blue windows, cursor navigation, text box with typewriter effect)
- Audio manager stub
- Save/load infrastructure (JSON serialization, atomic writes)

**Debug tools built in Phase 1:**
- **Debug Overlay (toggle with F1):** An on-screen panel showing current game state (FSM state, current scene, player grid position, facing direction, FPS). Always accessible during play mode. Built with uGUI, renders on top of everything.
- **Debug Console (toggle with backtick/tilde):** A text input field that executes debug commands. Minimal initial command set, expanded each phase.

**Phase 1 verification:**
- [ ] Boot → Title → Exploration flow runs without errors
- [ ] Player moves on 20x20 grid, collides with walls, cannot walk out of bounds
- [ ] Confirm interaction facing an empty tile does nothing; facing a wall does nothing
- [ ] Run toggle doubles movement speed visibly
- [ ] Camera stays within map bounds (no void visible at edges)
- [ ] Fade transitions play on scene change (no pop-in)
- [ ] Save a file to disk, quit play mode, re-enter, load it — player position restores
- [ ] All input works on both keyboard and gamepad; switching mid-session works
- [ ] Audio stub logs debug messages when play/stop are called (verify in Console)
- [ ] Debug overlay shows correct state, position, and FPS

### Phase 2 — Party & Data
- Class definitions (all 6 base classes as SOs, including EXP tables and equipment whitelists)
- Party creation screen (class select + naming)
- Stat system (base stats, growth tables, level-up with variance)
- PartyManager with full stat computation
- Main menu (Items, Equipment, Magic, Status, Order, Config)
- Equipment system (equip/unequip, stat recalc, class restrictions, two-handed)
- Inventory system (add, remove, use items, Gil cap)
- Field spell/item usage (Cure, Antidote outside battle)

**Debug tools added in Phase 2:**
- **Debug overlay additions:** Show party summary (names, classes, levels, HP/MP) and Gil.
- **Debug console commands:**
  - `setlevel <slot> <level>` — set a party member's level (triggers stat recalc)
  - `addgil <amount>` — add Gil (test Gil cap at 999,999)
  - `additem <id> <count>` — add items to inventory
  - `addequip <id>` — add equipment
  - `levelup <slot>` — force a level-up and display stat gains

**Phase 2 verification:**
- [ ] Party creation: select 4 classes (try all-same-class), name each, confirm → spawns on map
- [ ] Pause menu opens, shows correct party stats, Gil, play time
- [ ] Equipment screen: equip a weapon → stat preview shows correct delta → confirm → stats update
- [ ] Equipment screen: try equipping wrong class item → blocked
- [ ] Equipment screen: equip two-handed weapon → shield slot greys out, shield unequipped
- [ ] Inventory: use Potion in field → HP increases → item count decreases
- [ ] Inventory: use Potion at full HP → still consumed (FF1 behavior)
- [ ] Gil cap: `addgil 999999`, buy an item, verify Gil doesn't exceed 999,999
- [ ] `setlevel 0 50` → verify stat block recalculates, HP/MP update
- [ ] Party order: reorder slots → map sprite changes to new slot 1 character

### Phase 3 — Battle System
- Battle scene (additive loading, layout, cameras, procedural backgrounds)
- Encounter transition effect (flash + dissolve)
- Turn order calculation (agility sort with random tiebreak)
- Command input (Attack, Magic, Item, Defend, Flee)
- Auto-battle toggle (repeat last commands)
- Physical damage formula (multi-hit system, critical hits)
- Magical damage formula + healing formula
- Buff/debuff system (Haste, Temper, Protect, Saber, Slow)
- Undead healing inversion
- Elemental weakness/resistance/null/absorb
- Enemy AI (weighted random + boss scripted patterns)
- Status effects (all persistent + battle-only effects)
- Battle rewards (shared EXP, Gil, item drops, level-up display)
- Flee mechanics
- Auto-retargeting (with classic mode toggle)
- Battle animations (lunge, flash, particles)
- Damage numbers + "Miss" / "Ineffective" text

**Debug tools added in Phase 3:**
- **Debug overlay additions (battle mode):** Show turn order queue, current actor, action being executed, all actor HP/status, buff states, per-hit damage rolls.
- **Debug console commands:**
  - `encounter <formation_id>` — force a specific encounter immediately
  - `kill` — kill all enemies instantly (end battle with rewards)
  - `godmode` — party takes 0 damage (toggle)
  - `setstat <slot> <stat> <value>` — override a stat for testing formulas
  - `inflict <slot> <status>` — apply a status effect to a party member
  - `cure <slot>` — remove all status effects from a party member
  - `nobattles` — disable random encounters (toggle)

**Phase 3 verification:**
- [ ] Walking on encounter tiles triggers battle after step counter reaches 0
- [ ] Encounter transition plays cleanly (no frame hitch, no camera jitter)
- [ ] Battle scene loads additively — exploration scene is paused underneath
- [ ] Turn order: give one character high agility via `setstat` → they act first consistently
- [ ] Attack command: select enemy, attack hits, damage number appears, multi-hit rolls visible in debug overlay
- [ ] Magic command: cast Fire on enemy weak to Fire → 2x damage shown
- [ ] Magic command: cast Cure on undead enemy → deals damage instead
- [ ] Item command: use Potion on ally → HP restored, item consumed
- [ ] Use command: equip item with CastableSpell → Use appears in menu → cast works at 0 MP cost
- [ ] Defend: character defends → incoming physical damage halved (verify via debug overlay)
- [ ] Flee: attempt flee on non-boss → success exits battle, failure → enemies get free round
- [ ] Flee: attempt on boss → Flee option is hidden from menu
- [ ] Auto-battle: toggle on → party repeats last commands → toggle off mid-turn returns control
- [ ] Auto-retarget: kill an enemy mid-turn → next attacker targeting that enemy redirects to living enemy
- [ ] Status: inflict Sleep → character skips turn; hit sleeping character → they wake up
- [ ] Status: inflict Confuse → character attacks random target (ally or enemy)
- [ ] Status: inflict Silence → character tries to cast spell → "Silenced!" → action fizzles
- [ ] Status: inflict Poison → damage ticks at end of turn in battle
- [ ] All party KO'd → Game Over screen → return to title
- [ ] Victory: EXP split among living members, KO'd get 0 (verify with debug overlay)
- [ ] Victory: level-up display shows correct stat deltas per level
- [ ] `encounter <formation>` with 9 enemies → verify layout isn't overlapping
- [ ] Battle ends → exploration resumes at exact same position, step counter intact

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

**Debug tools added in Phase 4:**
- **Debug overlay additions:** Show current encounter table name, step counter value, current tile passability flags, vehicle state.
- **Debug console commands:**
  - `warp <scene> <x> <y>` — teleport to any scene at any grid position
  - `setflag <flag_id>` — set a progression flag
  - `clearflag <flag_id>` — remove a progression flag
  - `flags` — list all set flags
  - `giveship` / `givecanoe` / `giveairship` — grant vehicles
  - `openall` — mark all chests in current scene as opened
  - `noclip` — walk through walls (toggle)

**Phase 4 verification:**
- [ ] World map wrapping: walk off right edge → appear on left edge; same for top/bottom
- [ ] World map wrapping: camera shows no visual seam at the wrap boundary
- [ ] Town entry: walk onto town tile → fade → town scene loads → player at entrance
- [ ] Town exit: walk to exit tile → fade → world map loads → player at correct position
- [ ] Dungeon entry/exit: warp tiles move between floors correctly (verify with `warp`)
- [ ] NPC dialogue: talk to NPC → text box appears → Confirm advances → box closes
- [ ] NPC dialogue: set a progression flag → talk to same NPC → dialogue changes
- [ ] Shop (weapon): browse list → stat comparison shows per-member → buy → Gil deducted, item in inventory
- [ ] Shop (magic): shows learnable spells per member → buy → character knows spell → re-entering shop shows "learned"
- [ ] Shop: attempt to buy with insufficient Gil → blocked
- [ ] Inn: rest → all HP/MP restored, status cured, KO'd revived → Gil deducted → save prompt
- [ ] Treasure chest: open → item granted → chest visually open → save/reload → chest still open
- [ ] Treasure chest: open already-opened chest → "Already empty" message
- [ ] Chest PersistentID: copy a chest in editor → editor script logs duplicate ID warning
- [ ] Locked door: interact without Mystic Key → nothing. `setflag FLAG_MYSTIC_KEY` → interact → door opens
- [ ] Ship: board from adjacent tile → movement switches to ocean tiles → disembark on land → ship stays
- [ ] Canoe: walk onto river with canoe key item → auto-enters canoe mode → step onto land → exits
- [ ] Airship: board by standing on it + Confirm → fly mode → land on grass tile → airship stays
- [ ] Airship: attempt landing on forest/mountain/water → blocked
- [ ] Airship: no encounters while flying (verify step counter paused in debug overlay)
- [ ] Encounter suppression: walk near town entrance on world map → no encounter for first ~3 tiles
- [ ] Damage floor: walk on lava tile → HP decreases per step (verify in debug overlay)
- [ ] `warp Town_Cornelia 5 5` → teleports correctly, camera adjusts

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

**Debug tools added in Phase 5:**
- **Debug console commands:**
  - `upgrade <slot>` — force class upgrade on a party member
  - `allspells <slot>` — teach all learnable spells to a party member
  - `allkeys` — grant all key items
  - `skipto <milestone>` — set all flags up to a progression milestone (e.g., `skipto fiend_earth` sets all flags needed to reach Earth Crystal)

**Phase 5 verification:**
- [ ] Every enemy type: force encounter via `encounter` → verify appearance is distinct, AI works, rewards are correct
- [ ] Every spell: cast each → verify damage/healing/status/element/targeting all match spell data
- [ ] Every equipment piece: equip → verify stat change matches SO data → verify class restriction works
- [ ] Every shop: verify stock matches expected items for that town
- [ ] Every map: walk every accessible tile → no collision holes, no stuck points
- [ ] Class upgrade: trigger Bahamut event → classes upgrade → new spell levels accessible → equipment whitelist updates
- [ ] Boss fight (each Fiend): force encounter → full fight → verify AI pattern, rewards, flag set
- [ ] Full progression: `skipto` each milestone → verify NPC dialogue, door access, vehicle availability changes
- [ ] Minimap: toggle on → shows correct map with player position → toggle off
- [ ] Config: change text speed → dialogue respects new speed. Change volume → audio stub logs new volume.
- [ ] Game Over → title → Continue loads correct most-recent save
- [ ] Victory/ending sequence plays after final boss

### Phase 6 — Hardening
- Save migration / versioning
- Edge case testing (all cases in Section 7)
- Balance pass (if formulas feel off, tune coefficients)
- Performance profiling (world map, large battles)
- Memory leak audit (scene loads/unloads)

**Debug tools used in Phase 6:**
All previously built tools. No new tools — Phase 6 is about using existing tools to stress-test.

**Phase 6 verification:**
- [ ] Save version migration: create a save, change `SaveVersion`, load old save → migration runs or error is shown cleanly
- [ ] Save corruption: truncate a save file mid-write → load → error displayed, other slots still accessible
- [ ] Quick save lifecycle: quick save in dungeon → load → position restored → quick save slot is consumed
- [ ] Run every edge case from Section 7 tables (battle, exploration, inventory, save/load)
- [ ] Performance: world map 256x256 → maintain 60 FPS while walking
- [ ] Performance: battle with 9 enemies → maintain 60 FPS during animations
- [ ] Memory: enter and exit 20 battles in a row → no memory growth (check Profiler)
- [ ] Memory: transition between 10 different scenes → no leaked GameObjects (check Profiler)
- [ ] Full playthrough: New Game → party creation → Cornelia → Temple of Chaos → first boss → verify complete loop

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
