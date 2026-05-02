# SPEC.md — Command & Conquer: Red Alert (1996)

Gameplay specification for the Unity recreation of Westwood Studios' Command & Conquer: Red Alert. Base game only (not Counterstrike/Aftermath expansions). For exact numeric stats (HP, damage, ROF, cost, armor, speed, etc.), refer to the original source code at `D:\CnC_Red_Alert\CODE\` — see CLAUDE.md for key files.

---

## Table of Contents

- [Factions](#factions)
- [Tech Trees](#tech-trees)
- [Economy](#economy)
- [Base Building](#base-building)
- [Production Queues & Spawn Rules](#production-queues--spawn-rules)
- [Terrain & Map](#terrain--map)
- [Fog of War & Shroud](#fog-of-war--shroud)
- [Combat Mechanics](#combat-mechanics)
- [Targeting Rules](#targeting-rules)
- [Damage Side Effects](#damage-side-effects)
- [Special Unit Mechanics](#special-unit-mechanics)
- [Radar, Sensors, Stealth & Gap Mechanics](#radar-sensors-stealth--gap-mechanics)
- [Transport Rules](#transport-rules)
- [Controls & Unit Commands](#controls--unit-commands)
- [Superweapons & Support Powers](#superweapons--support-powers)
- [Crates](#crates)
- [AI Behavior](#ai-behavior)
- [Campaign — Allied](#campaign--allied)
- [Campaign — Soviet](#campaign--soviet)
- [Skirmish](#skirmish)

---

## Factions

Two playable factions, each with unique units, buildings, and superweapons.

**Allies** — Technologically advanced. Strengths: naval superiority (Cruisers, Destroyers), hero unit (Tanya), intelligence tools (Spies, Gap Generators), Chronosphere teleportation. Weaknesses: weaker tanks, no attack dogs, no Tesla technology.

**Soviets** — Raw firepower and armor. Strengths: Heavy and Mammoth Tanks, Tesla Coils, aircraft (MiGs, Yaks), submarines, Iron Curtain invulnerability. Weaknesses: weaker navy surface fleet, no stealth/intel tools, slower tech.

### Country Bonuses (Skirmish)

| Country   | Faction | Bonus                        |
|-----------|---------|------------------------------|
| England   | Allied  | +10% armor (durability)      |
| Germany   | Allied  | +10% firepower (damage)      |
| France    | Allied  | +10% rate of fire            |
| Greece    | Allied  | +10% attack range            |
| Turkey    | Allied  | (no bonus)                   |
| USSR      | Soviet  | −10% unit/building costs     |
| Ukraine   | Soviet  | +10% movement speed          |

---

## Tech Trees

### Allied Tech Tree

```
Power Plant ($300, +100 power)
├── Barracks ($300)
│   ├── Infantry: Rifle, Rocket Soldier, Engineer, Medic
│   ├── Pillbox ($400)
│   ├── Camo Pillbox ($600)
│   └── Gun Turret ($600)
├── Ore Refinery ($2,000) → free Ore Truck
│   ├── Ore Silo ($150)
│   ├── Radar Dome ($1,000)
│   │   ├── Spy (infantry)
│   │   ├── AA Gun ($600)
│   │   └── Helipad ($1,500) → free Longbow
│   └── War Factory ($2,000)
│       ├── Ranger, Light Tank, Medium Tank, APC, Artillery
│       ├── Service Depot ($1,200) → Mine Layer, MCV
│       └── Allied Tech Center ($1,500) [requires Radar Dome]
│           ├── Tanya, Thief (infantry)
│           ├── Mobile Gap Generator (vehicle)
│           ├── Cruiser (naval)
│           ├── Gap Generator ($500)
│           ├── Chronosphere ($2,800)
│           ├── Missile Silo ($2,500)
│           └── GPS Satellite (auto-launches)
├── Naval Yard ($650) → Transport, Gunboat, Destroyer
└── Advanced Power Plant ($500, +200 power)
```

### Soviet Tech Tree

```
Power Plant ($300, +100 power)
├── Barracks ($300)
│   ├── Infantry: Rifle, Grenadier, Rocket Soldier, Engineer
│   └── Kennel ($200) → Attack Dog
├── Ore Refinery ($2,000) → free Ore Truck
│   ├── Ore Silo ($150)
│   ├── Radar Dome ($1,000)
│   │   ├── V2 Rocket Launcher (vehicle)
│   │   ├── SAM Site ($750)
│   │   ├── Helipad ($1,500) → free Hind
│   │   └── Airfield ($600) → Yak, MiG, Spy Plane, Paratroopers
│   └── War Factory ($2,000)
│       ├── Heavy Tank
│       ├── Tesla Coil ($1,500)
│       ├── Service Depot ($1,200) → Mine Layer, MCV
│       └── Soviet Tech Center ($1,500) [requires Radar Dome]
│           ├── Mammoth Tank (vehicle)
│           ├── Flamethrower (infantry)
│           ├── Iron Curtain ($2,800)
│           └── Missile Silo ($2,500)
├── Sub Pen ($650) → Submarine, Transport
└── Advanced Power Plant ($500, +200 power)
```

---

## Economy

### Resources

Two resource types found on the map:

- **Ore** (yellow/brown) — $25 per bail, 28 bails per load = **$700 per full load**. Regenerates: ore grows denser and spreads to adjacent cells every 2 minutes. Each cell has 4 density levels.
- **Gems** (blue/purple) — $50 per bail, 28 bails per load = **$1,400 per full load**. Does NOT regenerate.

### Harvesting Loop

1. Ore Truck drives to nearest ore/gem field (near-scan radius: 6 cells)
2. Scoops resources one bail at a time (up to 28 bails)
3. Returns to nearest Ore Refinery to deposit
4. Credits added to player's account (capped by storage capacity)
5. Repeat — if field depleted, searches within far-scan radius (48 cells)

### Storage

- **Ore Refinery**: 2,000 credits storage. Comes with one **free Ore Truck**.
- **Ore Silo**: 1,500 credits storage.
- Total storage = sum of all Refineries + Silos. Excess beyond capacity is **lost**.
- Destroying a storage building loses a proportional share of stored credits.

### Refinery Docking

- Only **one harvester** can dock at a refinery at a time. Others queue/wait nearby.
- Refinery destroyed while returning: harvester tries to find another. If none, sits idle.
- Storage full: harvester can only unload available space. Excess is wasted.

### Starting Credits

Skirmish default: **$10,000** (configurable). Campaign missions have variable starting amounts.

---

## Base Building

### Construction Sidebar

- All construction is initiated from the right-side sidebar panel
- A progress bar fills as the Construction Yard assembles the building internally
- Once complete, the player places the building footprint on the map within valid range
- **One building at a time** per Construction Yard
- Owning multiple Construction Yards allows parallel construction queues
- Build time: `(Cost / 1000) × 0.8 minutes`

### Placement Rules

- Within roughly **16-cell radius** of a Construction Yard
- Within **2 cells** of an existing friendly building
- Walls: up to **7 cells** away
- Naval Yard / Sub Pen: up to **8 cells** away
- Cannot build on water (except naval buildings), cliffs, or occupied cells

### MCV Deployment

- MCV deploys into a Construction Yard (nearly instant)
- Construction Yard can be **re-packed** back into an MCV

### Selling Buildings

- Refund: **50%** of original cost, scaled by remaining HP
- Crewed buildings spawn an infantry unit when sold

### Repairing Buildings

- Costs ~**20%** of the building's original price to fully repair
- Repairs drain credits continuously; pauses if funds run out
- An **Engineer** entering a friendly building restores it to 100% instantly (consumes the Engineer)

### Building Damage States

- **Above 50% HP**: intact appearance.
- **At or below 50% HP**: damaged — shows fire/smoke overlay.
- **At or below 25% HP**: "red health" — Engineer capture becomes possible on enemy buildings.

### Power System

Each building produces or consumes power. Total base power = sum of all generators minus all consumers.

- **Sufficient power**: everything functions normally.
- **Brownout** (consumed > produced): Radar Dome goes offline (minimap disabled), Tesla Coils stop firing, AA Guns stop firing, Gap Generators stop re-shrouding. All `RequiresPower` buildings shut down.
- **Sidebar indicator**: power bar shows green (surplus), yellow (tight), red (brownout).
- Destroying or selling a Power Plant can trigger a brownout. AI sells buildings if power falls below 75%.

---

## Production Queues & Spawn Rules

### Queue Structure

One production queue per building **category**. Only one item builds at a time per category:

| Category | Buildings |
|----------|-----------|
| Structures | Construction Yard |
| Infantry | Barracks, Kennel |
| Vehicles | War Factory |
| Naval | Naval Yard, Sub Pen |
| Aircraft | Airfield, Helipad |

No unit queuing in the original game — player must click to start the next unit after each finishes.

### Multiple Factory Speed

Multiple factories of the same type act as a **speed multiplier**, not parallel queues. 2 factories = 2× build speed. Build time is divided by factory count.

### Unit Exit

- Units emerge from the "primary building" (player-settable when multiple factories exist).
- If the exit cell is blocked, the unit waits inside until the cell clears.
- No rally points in Red Alert 1.

### Aircraft Capacity

One aircraft per Helipad/Airfield. Need multiple structures for multiple aircraft. Aircraft return to their pad when out of ammo. Reload rate: **2.4 seconds per ammo point**.

---

## Terrain & Map

### Tile System

- Cell-based grid. Each cell is one square, fits one vehicle.
- Cell size: **24×24 pixels** at original resolution.
- Maximum map dimensions: **126×126 cells**.

### Terrain Types & Movement Speed

Speed values are percentages of a unit's base speed. 0% = impassable. Four locomotion types: **Foot**, **Tracked**, **Wheeled**, **Float** (naval).

| Terrain | Foot | Tracked | Wheeled | Float |
|---------|------|---------|---------|-------|
| Clear (grass) | 90% | 80% | 60% | 0% |
| Road | 100% | 100% | 100% | 0% |
| Rough (rocky) | 80% | 70% | 40% | 0% |
| Ore field | 90% | 70% | 50% | 0% |
| Beach/Sand | 80% | 70% | 40% | 0% |
| Water | 0% | 0% | 0% | 100% |
| River | varies | 0% | 0% | 100% |
| Bridge | ~100% | ~100% | ~100% | 0% |

### Terrain Features

- **Trees**: Block movement and line of sight. Destructible by HE, Fire, and Nuke warheads.
- **Cliffs/Ridges**: Impassable. Create chokepoints and natural defenses.
- **Roads**: Maximum speed for all ground units.
- **Bridges**: Allow land units to cross water. Destructible (HP: 1000). **Cannot be repaired** in RA1 — once destroyed, permanently gone.
- **Ore fields**: Yellow patches. Harvested for credits. Regenerate over time (4 density levels).
- **Gem fields**: Blue/purple crystals. Worth 2× ore. Do not regenerate.

---

## Fog of War & Shroud

Two-layer visibility system:

1. **Shroud (Black)**: Unexplored areas. Completely hidden — no terrain, structures, or units visible. Must be revealed by moving units into the area, Spy Plane, or GPS Satellite.

2. **Fog of War (Dimmed)**: Previously explored areas no longer in any unit's sight range. Terrain and structures visible as last seen, but enemy units are hidden. Structures may have changed since last observed.

3. **Clear Vision**: Areas within your units' current sight radius. Full real-time visibility.

**Gap Generator**: Re-shrouds a 10-cell radius around itself. Enemies entering the gap zone only see their immediate surroundings.

**GPS Satellite**: Permanently reveals the entire map (removes all shroud). Does not counter Gap Generators.

---

## Combat Mechanics

### Damage Calculation

```
Effective Damage = Weapon Damage × Warhead Modifier[target armor type]
```

Five armor types: **None** (infantry), **Wood** (buildings), **Light**, **Heavy**, **Concrete**. Eight warhead types: SA, HE, AP, Fire, HollowPoint, Super, Organic, Nuke — each with different multipliers per armor type. See `D:\CnC_Red_Alert\CODE\RULES.H` for exact values.

### Key Combat Rules

- **Auto-targeting**: Units auto-target the nearest enemy within range. No threat assessment.
- **No veterancy**: Units do not gain experience or improved stats from kills.
- **No stances**: No passive/aggressive/hold-position toggles. Units always engage enemies in range.
- **Crushing**: Tracked vehicles with `IsCrusher=yes` can crush infantry by driving over them (most tanks and APC, but not V2, Artillery, or Ore Truck).
- **Self-healing**: Mammoth Tanks and Ore Trucks regenerate HP up to 50%.
- **Crewed vehicles**: When destroyed, the driver bails out as a Rifle Infantry unit.
- **Explodes on death**: Grenadiers (HE) and Flamethrowers (Fire) explode when killed, damaging nearby units. Can chain-react.

---

## Targeting Rules

### Anti-Air Capable Units

Only these units/buildings can engage aircraft:

| Unit | Weapon | Notes |
|------|--------|-------|
| Rocket Soldier | RedEye (primary) | AA-only primary; Dragon (secondary) for ground |
| SAM Site | Nike | AA-only, cannot hit ground |
| AA Gun | ZSU-23 | AA-only, cannot hit ground |
| Mammoth Tank | MammothTusk (secondary) | Secondary targets air; primary (120mm) is ground-only |
| Destroyer | Stinger | Can hit air, ground, and sea |

### Anti-Submarine Capable

Only Destroyers, Gunboats, and Cruisers have `Sensors=yes` (detect subs). Destroyer and Gunboat have depth charges. Cruiser can detect but not attack subs.

### Fire While Moving

**Most units CAN fire while moving.** Only V2 Rocket Launcher and Artillery have `NoMovingFire=yes`. Turret-equipped units (tanks, Jeep) fire independently of body facing.

### Guard Range & Pursuit

- **Guard mode**: unit scans at its weapon range.
- **Guard Area mode**: patrol distance = weapon range from home position. Returns home if drifting beyond this.
- **Attack pursuit**: no explicit leash — units chase until target dies or becomes unreachable.
- **Aircraft guard range**: 30 cells.

---

## Damage Side Effects

### Splash Damage & Friendly Fire

All splash damage hits **everything** within radius, including friendly units. There is no friendly fire exemption — only the source unit itself is excluded.

- Splash radius: **1.5 cells** from impact point (scans 3×3 cell grid).
- Damage falls off with distance. The warhead's **Spread** value controls falloff (higher = slower falloff).
- **Fire warhead** (Spread=8) is the widest splash. **HE/Nuke** (Spread=6) are next. **SA/AP** (Spread=3) are small. **Organic** (Spread=0) has no splash.

### Projectile Behavior

- **Hitscan** (Invisible): instant hit. Used by rifles, pistols, chain guns, Tesla.
- **Direct-fire shells** (Cannon): straight-line, no homing, accurate.
- **Homing missiles** (HeatSeeker): slow tracking (ROT=5), inaccurate (scatter up to 2.0 cells). Used by Dragon, Maverick, Hellfire, MammothTusk.
- **Precision homing** (LaserGuided): fast tracking (ROT=20), accurate. Used by Destroyer's Stinger.
- **AA missiles** (AAMissile): fast tracking, AA-only (AG=no). Used by SAM, Rocket Soldier primary.
- **Ballistic** (arcing): gravity arc, inaccurate (scatter up to 1.0 cells). Used by Artillery, Cruiser.
- **FROG** (V2 rocket): inaccurate (scatter up to 2.0 cells), no homing.
- **Torpedoes**: underwater, ASW-capable.
- Projectiles that miss still **detonate at the impact point** and deal splash damage.

### Nuclear Missile

- **Damage**: 1000 (singleplayer) / 200 (multiplayer). Warhead: Nuke (Spread=6).
- **Destroys ore** in blast radius. Leaves radiation that kills infantry over time.
- **Recharge**: 13 minutes.

### Mines

- Anti-personnel: 1000 damage. Anti-vehicle: 1200 damage.

### Walls

All walls have 1 HP. Only warheads with `Wall=yes` can damage them (HE, AP, Nuke). Concrete walls cannot be crushed by vehicles and block tank shells.

---

## Special Unit Mechanics

### Spy Infiltration Effects

| Target Building | Effect |
|----------------|--------|
| Barracks | Reveals enemy infantry in production |
| War Factory | Reveals enemy vehicle in production |
| Naval Yard | Reveals enemy naval unit in production |
| Airfield / Helipad | Reveals aircraft production and ammo status |
| Radar Dome | Reveals shroud — you see what the enemy sees |
| Power Plant / Adv. Power | Reveals enemy power status |
| Ore Refinery / Silo | Reveals enemy credits and storage |
| Sub Pen | Grants one-time **Sonar Pulse** (reveals all subs, recharge 10 min) |
| Construction Yard | Reveals structure under construction |

### Engineer Capture

- Capture is **NOT instant** on full-health buildings.
- Each Engineer damages an enemy building by ~1/3 of max HP.
- Building must be at **≤25% HP** ("red health") to be captured.
- Full-health building typically requires **3–4 Engineers**.
- Entering a **friendly** building instantly restores it to full HP (consumes the Engineer).
- Engineers cannot capture hardened structures (turrets, pillboxes).
- Engineers **cannot repair bridges** in RA1.

### Tanya C4

- Plants C4 by moving adjacent to a building or ship, then jogs away.
- **C4 delay**: 1.8 seconds from placement to detonation.
- **Targets**: any building, bridges, ships. Destroys regardless of HP.
- Dual pistols (HollowPoint) = 100% vs infantry, 5% vs everything else.

### Attack Dog

- Instant-kill on any infantry (Organic warhead = 100% vs None armor, 0% vs all else).
- **NOT fooled by Spy disguises** — auto-detects and auto-attacks Spies on sight.
- **Guard range**: 7 cells.
- Leap attack, can chain between infantry. Very fragile (12 HP).

### Field Medic

- Auto-heals nearby injured friendly infantry within ~1.83 cells.
- **Cannot heal itself.** Two Medics can heal each other.

---

## Radar, Sensors, Stealth & Gap Mechanics

### Gap Generator (Building)

- **Radius**: 10 cells (re-shrouds area around itself)
- **Refresh interval**: 6 seconds between shroud regeneration cycles
- Requires power to function.

### Mobile Gap Generator

- Smaller radius than the stationary version. Same shroud effect, but mobile.

### Radar Jammer (MRJ)

- **Jam radius**: 15 cells
- Causes static on enemy radar/minimap within 15 cells of the enemy's Radar Dome.

### Sensor Units (Submarine Detection)

Only Destroyers, Cruisers, and Gunboats have `Sensors=yes`.

### Submarine Stealth

- Cloaked by default (submerged, invisible). Surfaces automatically to fire, then re-submerges.

### Camo Pillbox

- Hidden from enemy radar and visual display. Revealed when enemies get adjacent or when it fires.

---

## Transport Rules

| Transport | Capacity | Can Carry | Locomotion |
|-----------|----------|-----------|-----------|
| APC | 5 | Infantry only | Tracked (land) |
| Chinook | 5 | Infantry only | Fly (air) |
| Naval Transport (LST) | 5 | Infantry AND vehicles | Float (sea) |

- Units board/unload **one at a time**.
- Naval transports must touch a shoreline cell to unload.
- **APC destroyed**: passengers eject and survive.
- **Naval transport sunk at sea**: vehicles sink and die; infantry may survive.
- **Chinook shot down**: passengers generally die.

---

## Controls & Unit Commands

### Selection

- **Click**: Select individual unit
- **Drag-box**: Select multiple units
- **Double-click**: Select all same-type units on screen
- **E**: Select all visible units on screen

### Control Groups

- **Ctrl+1–9**: Assign selected units to group
- **1–9**: Recall/select group
- **Alt+1–9**: Jump camera to group location

### Orders

- **Right-click**: Context-sensitive move/attack
- **S**: Stop
- **G**: Guard mode — patrol area, engage enemies that enter radius
- **X**: Scatter — units spread out (dodge area damage)
- **Alt+Click**: Force move — move without stopping to engage
- **Ctrl+Click**: Force fire — attack ground, friendly units, trees, etc.
- **Q+Click**: Attack-move waypoints — fire at targets while moving along path

---

## Superweapons & Support Powers

### Superweapons

| Superweapon | Faction | Building | Cost | Recharge | Effect |
|-------------|---------|----------|------|----------|--------|
| Chronosphere | Allied | Chronosphere | $2,800 | ~90 sec | Teleports one vehicle anywhere on the map. Unit returns to origin after ~3 minutes. Cannot teleport infantry. |
| Iron Curtain | Soviet | Iron Curtain | $2,800 | ~7 min | Makes one vehicle or structure invulnerable for ~45 seconds. Kills infantry inside transports. |
| Nuclear Missile | Both | Missile Silo | $2,500 | ~13 min | Massive area damage. Destroys ore. Leaves radiation. |

### Support Powers

| Power | Faction | Prerequisite | Effect |
|-------|---------|-------------|--------|
| GPS Satellite | Allied | Tech Center | Permanently reveals entire map. One-time use. |
| Spy Plane | Soviet | Airfield | Sends recon plane to reveal a target area temporarily. |
| Paratroopers | Soviet | Airfield | Drops 5 Rifle Infantry anywhere on the map via Badger bomber. |
| Parabombs | Soviet | Airfield + Tech Center | Badger bomber drops bombs on target area. |
| Sonar Pulse | Allied | Spy infiltrates Sub Pen | Temporarily reveals all enemy submarines. |

---

## Crates

Wooden boxes that spawn randomly on skirmish maps. Contents are randomized:

| Crate Type | Effect |
|-----------|--------|
| Money | Bonus credits (water crates are always money) |
| Heal | Restores full health to all nearby friendly units and structures (AoE) |
| Map Reveal | Reveals the entire map |
| Unit | Spawns a random ground vehicle (can include MCVs, Ore Trucks) |
| Squad | Spawns a squad of 5 random infantry |
| Armor | Increases collecting unit's armor |
| Firepower | Increases collecting unit's damage |
| Speed | Increases collecting unit's movement speed |
| Cloak | Makes collecting unit invisible |
| Invulnerability | Temporary invincibility |
| Explosion | Crate detonates, damaging/killing the collecting unit |

---

## AI Behavior

### Overview

The AI is **script-driven**, not a dynamic decision-maker. It uses pre-defined team compositions sent along scripted waypoints.

### Build Order

1. Power Plant → Power Plant → Ore Refinery → military structures
2. AI sells buildings if power falls below 75%
3. AI has a credit reserve of $10,000 — won't repair structures if cash is below this

### Attack Patterns

- Builds squads (TeamTypes), sends them along scripted waypoints
- ~6 varieties of attack forces selected randomly
- First attack typically arrives around **3 minutes** (Hard difficulty, $10,000 starting credits)
- Retaliates sooner if attacked first
- Autocreate function continuously produces teams

### Limitations

The AI **will not build**: navy, Attack Dogs, Service Depots, Missile Silos, or air units (unless the player attacks with aircraft first). Open flat maps favor the AI; chokepoints jam it.

### Difficulty Levels

Three levels: **Easy, Medium, Hard**

Difficulty only changes **unit/structure stat multipliers** (HP, damage). It does **NOT** change AI behavior, build order, or attack patterns.

### Cheating

- No income cheating (no bonus credits)
- No full map vision — must scout like the player
- Campaign missions receive off-map reinforcements that are effectively "free"

---

## Campaign — Allied

14 missions. Most offer 2–3 map variants (the player picks a region on a map screen — same objectives, different terrain). Linear narrative despite map branching.

| # | Name | Objectives |
|---|------|-----------|
| 1 | Rescue the Good Professor | Use Tanya to infiltrate a Soviet base, rescue Einstein from a Tech Center, escort him to extraction. |
| 2 | Five to One | Clear Soviet troops blocking a mountain pass so an Allied supply convoy can pass. First base-building mission. |
| 3 | Dead End | Use Tanya and a strike group to destroy key bridges, preventing Soviet armor from advancing into Western Europe. |
| 4 | Hold the Pass | Defend the mountain pass from a strong Soviet assault with Heavy Tanks until reinforcements arrive. |
| 5 | Tanya's Tale | Tanya has been captured. Rescue her using artillery and a small strike team. |
| 6 | Infiltrate the Enemy | Spy/infiltration mission. Get agents behind Soviet lines to gather intelligence. |
| 7 | Capture the Radar | Use Engineers to capture the Soviet Radar Dome facility for Allied intelligence. |
| 8 | Protect the Chronosphere | Einstein completed the Chronosphere prototype. Defend it and the Tech Center from waves of Soviet attacks until the timer expires. |
| 9 | Extract Kosygin | Stalin's nuclear strategist Kosygin wants to defect. Infiltrate the Riga Compound with a Spy and escort Kosygin to your base. |
| 10 | Suspicion / Evidence | Destroy Soviet nuclear missile silos targeting Paris, Berlin, and London. Variant: infiltrate an underground complex and deactivate missiles before timer runs out. |
| 11 | Secure the River | Establish naval dominance and secure river crossings. Full combined-arms mission. |
| 12 | Takedown | Large-scale assault on a Soviet stronghold. Destroy all enemy forces and structures. |
| 13 | Focused Blast | Place explosive charges on all generators beneath the Iron Curtain base to destroy a secret weapons facility. |
| 14 | No Remorse | Invade Moscow. Destroy everything — no Soviet unit or structure can survive. Culminates in the death of Stalin. |

---

## Campaign — Soviet

14 missions. Also features map-variant branching. Linear narrative.

| # | Name | Objectives |
|---|------|-----------|
| 1 | Lesson in Blood | Tutorial. Destroy a civilian town and all resistance. Use Yaks to clear the way. |
| 2 | The Thin Red Line | Guard two bridges critical to the invasion of Germany. Eliminate all Allied forces. |
| 3 | Covert Cleanup | An Allied spy blew up a Soviet base. Chase and kill the Spy before he escapes. Protect cargo trucks. |
| 4 | Behind the Lines | Go behind enemy lines to disrupt Allied communications. Small infantry squad, indoor-style mission. Destroy the Allied base. |
| 5 | Khalkis Island | Amphibious assault. Capture the Allied Radar Dome, secure the middle island. First larger naval mission. |
| 6 | Bridge over the River Grotzny | Escort supply trucks across the river. If even one truck makes it through, the mission succeeds. |
| 7 | Core of the Matter | Assault a fortified Allied position. Full base-building and combined-arms. |
| 8 | Investigate Elba Island | Travel to Elba Island and destroy Allied forces and local rebels. |
| 9 | Liability Elimination | Destroy a defecting supply truck before it escapes the map. Defend your base from Allied counterattack simultaneously. |
| 10 | Overseer | Large-scale base-building mission. Establish Soviet dominance and crush Allied resistance. |
| 11 | Sunk Costs | Choose north or south approach. Attack Great Britain from the north or destroy Allied naval forces in the Mediterranean. Major naval warfare. |
| 12 | Capture the Tech Centers | Allies are retreating. Capture Allied Chronosphere in Switzerland using Engineers. |
| 13 | Capture the Chronosphere | Locate and capture the Allied Chronosphere device. Two approach routes. |
| 14 | Soviet Supremacy | Final mission. Invade England along the English Channel. Wipe out all defenders, push to London. |

---

## Skirmish

- 1 human vs 1–5 AI opponents
- Configurable: faction/country, AI difficulty (Easy/Medium/Hard), starting credits, map, player colors
- Victory condition: **annihilation** — destroy all enemy structures and units

---

## References

### Primary Data Sources

- Original source code: `D:\CnC_Red_Alert\CODE\` (see CLAUDE.md for key files)
- [OpenRA raclassic Rules.ini](https://github.com/OpenRA/raclassic/blob/master/ref/Rules.ini)

### Wiki & Guides

- [C&C Wiki — Command & Conquer: Red Alert](https://cnc.fandom.com/wiki/Command_%26_Conquer:_Red_Alert)
- [StrategyWiki — Command & Conquer: Red Alert](https://strategywiki.org/wiki/Command_&_Conquer:_Red_Alert)
- [GameFAQs — Red Alert Strategy Guide](https://gamefaqs.gamespot.com/pc/196962-command-and-conquer-red-alert/faqs/1699)
- [Andrew Armstrong — Red Alert Gameplay and Singleplayer AI](https://aarmstrong.org/journal/2008/09/07/red-alert-gameplay-and-singleplayer-ai)

### Community & Mechanics

- [ModEnc — Projectile documentation](https://modenc.renegadeprojects.com/Projectile)
- [ModEnc — Damage calculation over area](https://modenc.renegadeprojects.com/Damage_calculation_over_area)
- [CnCNet Hotkeys Reference](https://downloads.cncnet.org/Games/RedAlert/Hotkeys.txt)
