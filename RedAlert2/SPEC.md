# Command & Conquer: Red Alert 2 — Gameplay Systems Spec

PC, October 2000. Developed by Westwood Studios, published by EA. Base game only (not Yuri's Revenge expansion).

---

## 1. Core Gameplay Systems

### 1.1 Primary Gameplay Loop

Red Alert 2 is a real-time strategy game built on the **gather → build → produce → attack** loop:

1. **Gather**: Harvest ore and gems from the map using miners, converting raw resources into credits at a refinery.
2. **Build**: Construct base structures via the sidebar, unlocking new tiers of units and defenses.
3. **Produce**: Queue infantry, vehicles, naval units, and aircraft from production structures.
4. **Attack**: Deploy forces to destroy enemy bases, capture strategic positions, and deny enemy resources.

### 1.2 Faction Asymmetry

Two factions — **Allies** and **Soviets** — with fundamentally different design philosophies:

- **Allies**: Emphasize technology, speed, and special abilities (chronoshift teleportation, stealth, prism beam chaining). Units tend to be faster but lighter. Power Plants produce more energy per structure.
- **Soviets**: Emphasize raw firepower and durability. Units are slower but hit harder and have more HP. Access to mind control, Tesla technology, and nuclear weapons.

Allies have **5 subfactions** (countries), Soviets have **4** — 9 playable countries total, each with one unique unit or structure (§4).

### 1.3 Resource System

**Two resource types**, both mined from the map:

| Resource | Value per load (Soviet War Miner) | Value per load (Allied Chrono Miner) | Appearance |
|----------|-----------------------------------|--------------------------------------|------------|
| Ore | ~$500–$1000 | ~$250–$500 | Gold/yellow patches |
| Gems | ~$1000–$2000 | ~$500–$1000 | Blue/teal crystals |

- Gems are worth approximately **2× the value of ore**.
- Each refinery includes one free miner on construction.
- **Ore Purifier** (Allied, $2500): Increases refinery income by **25%** per load. One per player.
- Ore fields **slowly regenerate** over time via an ore growth mechanic. Gem fields do not regenerate.
- **Starting credits** in skirmish: configurable (Low: $5000, Medium: $10000, High: $20000). Campaign missions vary.

### 1.4 Veterancy System

Units gain experience by destroying enemies. Three ranks:

| Rank | Indicator | Damage | Armor | Speed | ROF | Other |
|------|-----------|--------|-------|-------|-----|-------|
| Rookie | None | — | — | — | — | Base stats |
| Veteran | 1 chevron | +25% | +25% | +30% | +20% | Increased sight range |
| Elite | 3 chevrons | +50% (cumulative) | +50% (cumulative) | +60% (cumulative) | +40% (cumulative) | Self-healing, may gain elite weapon |

- Experience is kill-based: destroying a unit awards XP proportional to the destroyed unit's cost.
- **Spy infiltration** of an enemy Barracks grants veteran status to all subsequently produced infantry. Spy infiltrating a War Factory does the same for vehicles.
- **Crate veterancy** (§9.1) promotes all units in a 3×3 cell area around the crate by one rank.

### 1.5 Combat System

#### Armor Types

11 armor classes, grouped by category:

| Category | Armor Types | Typical Users |
|----------|-------------|---------------|
| Infantry | none, flak, plate | Unarmored infantry, bulletproof infantry, metal-armored infantry |
| Vehicle | light, medium, heavy | Scout/support vehicles, miners/MCVs, tanks |
| Structure | wood, steel, concrete | Light structures, medium structures, heavy structures |
| Special | special_1, special_2 | Terror Drones, missiles/projectiles |

#### Warheads and Damage Calculation

**Damage = Weapon Damage × Warhead Verses% (for target armor type)**

Each warhead defines 11 Verses percentages — one per armor type, in order: none, flak, plate, light, medium, heavy, wood, steel, concrete, special_1, special_2.

Key warheads (verified from rules.ini):

| Warhead | Used By | none | flak | plate | light | med | heavy | wood | steel | concrete | sp1 | sp2 |
|---------|---------|------|------|-------|-------|-----|-------|------|-------|----------|-----|-----|
| SA (Small Arms) | Rifles, MGs | 100% | 80% | 80% | 50% | 25% | 25% | 75% | 50% | 25% | 100% | 100% |
| HollowPoint | Pistols, Snipers | 200% | 100% | 100% | 1% | 1% | 1% | 1% | 1% | 1% | 1% | 100% |

- **Verses = 0%**: Weapon cannot target this armor type at all (no force-fire, no retaliation, no auto-acquire).
- **Verses = 1%**: No auto-acquire, but force-fire and retaliation still work.
- **Verses > 100%**: Amplified damage (e.g. HollowPoint vs. unarmored infantry = 2× damage).
- **Negative Verses**: Heals the target instead of damaging it.
- **ProneDamage**: Some warheads deal reduced damage to deployed/prone infantry (e.g. SA: 70%).

#### Splash Damage

Warheads with **CellSpread > 0** deal area damage. Damage attenuates from the center based on **PercentAtMax** (damage percentage at the outer edge of the splash radius). Structures occupying multiple cells take damage once per cell within range.

#### Vehicle Crushing

Most vehicles and tanks **crush infantry** by driving over them. Exceptions:

- **Tesla Troopers** are immune to being crushed (their heavy suit grounds them).
- **Desolators** (plate armor) may resist crushing depending on the vehicle weight class.
- Vehicles cannot crush other vehicles (except the Battle Fortress in Yuri's Revenge — not present in base game).

### 1.6 Power System

Every structure consumes or produces power. The **power bar** in the HUD shows current supply vs. demand.

| Power Source | Faction | Cost | Power Output |
|-------------|---------|------|-------------|
| Power Plant | Allied | $600 | +200 |
| Tesla Reactor | Soviet | $600 | +150 |
| Nuclear Reactor | Soviet | $1000 | +1000 |

**Low power effects:**
- Defensive structures go **offline** (Tesla Coils, Patriot Missiles, Prism Towers stop firing).
- **Radar** is disabled (minimap goes dark).
- Production speed drops to approximately **50%** of normal.
- Superweapon charge timers pause.
- Gap Generators and Spy Satellite Uplinks stop functioning.

### 1.7 Building & Base Construction

**Construction Yard** is the heart of every base, built by deploying an MCV ($3000).

- **Sidebar construction**: Structures are built in the sidebar panel, then placed on the map. Only one structure can be built at a time per Construction Yard.
- **Build radius**: New structures must be placed within a certain distance of existing owned structures (adjacency requirement).
- **Multiple Construction Yards**: Each additional CY speeds up build time proportionally — two CYs = 2× build speed.
- **Selling**: Any owned structure can be sold for **50%** of its original cost. Infantry may emerge from the sold structure.
- **Repairing**: Click the repair icon, then click a structure to begin repairs. Repairs cost credits over time and restore HP.
- **Wall building**: Walls can be placed up to 4 segments at once by dragging.

**Build queues** are divided into 4 tabs:
- **Q**: Structures (Construction Yard queue)
- **W**: Defenses & Support Powers
- **E**: Infantry (Barracks queue)
- **R**: Vehicles/Aircraft (War Factory / Naval Yard / Airfield queue)

Unit production queues hold up to **30** items.

---

## 2. Controls & Input

Keyboard + mouse only. No gamepad support.

### 2.1 Mouse Controls

| Input | Action |
|-------|--------|
| Left-click | Select unit/structure |
| Right-click | Move / Attack (context-sensitive) |
| Left-click drag | Box-select units |
| Double-click unit | Select all visible units of same type |

### 2.2 Keyboard Shortcuts

| Key | Action |
|-----|--------|
| **Q** | Structures build tab |
| **W** | Defenses/support build tab |
| **E** | Infantry build tab |
| **R** | Vehicles build tab |
| **H** | Jump to primary base (Construction Yard) |
| **T** | Select all visible units of same type as current selection |
| **T T** | Select all units of same type on entire map |
| **P** | Select all combat units |
| **S** | Stop current orders |
| **G** | Guard mode |
| **X** | Scatter units / move one step |
| **Z** (hold) | Waypoint mode (click to set waypoints) |
| **K** | Repair mode |
| **L** | Sell mode |
| **D** | Deploy (GI sandbags, MCV, etc.) |
| **Tab** | Diplomacy menu |
| **Ctrl+1–9** | Assign control group |
| **1–9** | Select control group |
| **Ctrl+Left-click** | Force-fire on target (friendly fire, ground, bridges) |
| **Ctrl+Shift+Left-click** | Attack-move to position |
| **Ctrl+Alt+Left-click** | Guard area/unit/structure |
| **A** (with enemy unit selected) | Alliance toggle (multiplayer) |
| **F1–F4** | Set/recall map bookmarks |

---

## 3. World Structure

### 3.1 Map Layout

- Maps are flat 2D viewed from an **isometric perspective**.
- Terrain types: grass, snow, desert, urban, water.
- **Civilian structures** dot the map and can be garrisoned by infantry (§3.5).
- **Bridges** span water/ravines and can be destroyed (force-fire) and repaired (Engineer).
- **Cliffs** block ground movement; aircraft ignore them.
- **Water** is required for naval unit production and movement.

### 3.2 Shroud & Fog of War

Two visibility layers:

| Layer | Description |
|-------|-------------|
| **Shroud** (black) | Completely unexplored terrain. Removed permanently once any friendly unit gains line of sight. |
| **Fog of War** (dark) | Previously explored but currently unobserved terrain. Shows last-known terrain/structures but hides current enemy units. Clears when a friendly unit is nearby. |

- **Gap Generator** (Allied, $1000, −100 power): Re-applies shroud over a radius around itself, hiding the owner's base from enemy radar. Requires constant power.
- **Spy Satellite Uplink** (Allied, $1000, −100 power): Permanently reveals the entire map (except Gap Generator areas).
- **Psychic Sensor** (Soviet, $1000): Reveals enemy attack-move orders on the minimap before enemies arrive.

### 3.3 Ore Fields

- Ore appears as gold/yellow patches, gems as blue/teal crystals on the map.
- Ore slowly **regenerates** and spreads from existing ore cells over time. Gems do not regenerate.
- Multiple ore fields are scattered across each map; controlling them is a key strategic goal.
- Miners auto-harvest the nearest ore to their refinery.

### 3.4 Tech Buildings

Neutral structures scattered on certain maps. Captured by sending an Engineer inside.

| Tech Building | Effect |
|--------------|--------|
| **Oil Derrick** | Instant $1000 bonus on capture, then steady income of ~$20/second |
| **Hospital** | Heals infantry ordered to enter it |
| **Machine Shop** | Automatically repairs all of the owner's vehicles anywhere on the map |
| **Airport** | Grants the owner a Paradrop support power |
| **Outpost** | Repairs vehicles ordered inside; armed with a Patriot-style missile launcher |
| **Secret Lab** | Unlocks one random unit or structure not normally available to the owner's faction (predetermined per lab at match start) |

Tech buildings can be re-captured by enemy Engineers or destroyed. They do not contribute to the owner's tech tree.

### 3.5 Garrison System

Infantry can enter civilian buildings to use them as defensive positions:

- Only **basic infantry** can garrison: GI, Conscript (and equivalents). Advanced infantry cannot.
- Each building has a fixed number of **fire ports** (typically 4–8 windows).
- Garrisoned infantry fire their garrison weapon from the building's windows with increased protection.
- **Clearing garrisons**: Engineers can enter to capture, Navy SEALs/Tanya use C4 to destroy the building, Crazy Ivan can bomb it, or simply destroy the building with heavy weapons.
- Garrisoned units can still take casualties from attacks, especially from fire, explosives, and splash damage.

---

## 4. Factions & Countries

### 4.1 Allied Countries

| Country | Unique Unit/Structure | Cost | Description |
|---------|----------------------|------|-------------|
| **America** | Paratroopers (Airborne) | Free | Drops a squad of GIs via transport plane on a periodic cooldown (~4 minutes). Requires Airforce Command HQ. Plane can be shot down. |
| **Great Britain** | Sniper | $600 | Long-range infantry; one-shot kills on enemy infantry |
| **France** | Grand Cannon | $2000 | Powerful defensive artillery structure; extreme range vs. ground targets |
| **Germany** | Tank Destroyer | $900 | Anti-armor vehicle; devastating vs. vehicles, very weak vs. infantry/structures |
| **Korea** | Black Eagle | $1200 | Upgraded Harrier jet; more HP and damage than standard Harrier |

### 4.2 Soviet Countries

| Country | Unique Unit | Cost | Description |
|---------|------------|------|-------------|
| **Russia** | Tesla Tank | $1200 | Fires electric discharge; effective vs. units and structures; fires over walls |
| **Cuba** | Terrorist | $200 | Suicide infantry; explodes on contact with massive splash damage |
| **Iraq** | Desolator | $600 | Radiation infantry; deploy creates impassable irradiated zone killing nearby infantry |
| **Libya** | Demolition Truck | $1500 | Suicide vehicle with nuclear charge; enormous explosion radius |

---

## 5. Story & Progression

### 5.1 Campaign Structure

Two parallel campaigns of **12 missions each**, plus a tutorial. Campaigns are independent and present opposing perspectives on the same conflict.

**Premise**: Soviet Premier Romanov, advised by the psychic Yuri, launches a surprise invasion of the United States. The Allies fight to repel the invasion; the Soviets fight to conquer.

#### Allied Campaign

| # | Operation | Location | Objective |
|---|-----------|----------|-----------|
| 1 | Lone Guardian | New York Harbor | Destroy Soviet Dreadnought fleet, rescue Fort Bradley |
| 2 | Eagle Dawn | Colorado Springs | Capture Soviet Air Force Academy Chapel |
| 3 | Hail to the Chief | Washington D.C. | Destroy Psychic Beacon controlling the city |
| 4 | Last Chance | Chicago | Establish beachhead, destroy Psychic Amplifier |
| 5 | Dark Night | Poland | Infiltrate Battle Lab, neutralize two nuclear silos |
| 6 | Liberty | Washington D.C. | Reinforce Pentagon, destroy Soviet forces |
| 7 | Deep Sea | Hawaii | Destroy all Soviet naval forces |
| 8 | Free Gateway | St. Louis | Destroy Psychic Beacon within time limit |
| 9 | Sun Temple | Mexico | Capture/destroy Soviet prism tech base |
| 10 | Mirage | Black Forest, Germany | Protect Einstein's lab, destroy Soviet forces |
| 11 | Fallout | Cuba | Build Chronosphere, neutralize Soviet nuclear threat |
| 12 | Chrono Storm | Moscow | Destroy Romanov's elite guard at the Kremlin |

#### Soviet Campaign

| # | Operation | Location | Objective |
|---|-----------|----------|-----------|
| 1 | Red Dawn | Washington D.C. | Destroy the Pentagon |
| 2 | Hostile Shore | Florida | Force landing, establish base, destroy enemy forces |
| 3 | Big Apple | New York | Capture Battle Lab, build and defend Psychic Beacon |
| 4 | Home Front | Russia/Eastern Europe | Defend homeland from Allied counterattack |
| 5 | City of Lights | Paris | Power Paris Tower with Tesla Troopers |
| 6 | Sub-Divide | Hawaii | Establish base, destroy Allied navy |
| 7 | Chrono Defense | Ural Mountains | Defend Battle Lab from chrono attacks |
| 8 | Desecration | Washington D.C. | Capture the White House |
| 9 | The Fox and the Hound | San Antonio, Texas | Use Yuri to mind-control the President |
| 10 | Weathered Alliance | U.S. Virgin Islands | Capture Battle Lab, destroy Weather Control Device |
| 11 | Red Revolution | Moscow | Destroy Yuri's rogue forces at the Kremlin |
| 12 | Polar Storm | Alaska/Point Hope | Destroy the Allied Chronosphere |

### 5.2 Campaign Features

- **Live-action FMV briefings** between missions with actors (Udo Kier as Yuri, Ray Wise as President Dugan, Barry Corbin as General Carville, Kari Wuhrer as Lt. Eva).
- No mission branching — campaigns are linear.
- Some missions restrict available tech or units.
- No New Game+ or post-game content.

---

## 6. Units — Allied

Full unit roster with stats. HP = Strength from rules.ini. Speed is relative (higher = faster).

### 6.1 Allied Infantry

| Unit | Cost | HP | Armor | Speed | Weapon | Prerequisites | Special |
|------|------|----|-------|-------|--------|---------------|---------|
| GI | $200 | 125 | none | 4 | M60 (undeployed) / Para (deployed) | Barracks | Deploys into sandbag platform: increased range, damage, ROF. Can garrison buildings. |
| Engineer | $500 | 75 | none | 4 | — | Barracks | **Instant capture** of any enemy/neutral structure (no HP threshold in RA2). Repairs bridges. Defuses Crazy Ivan bombs. Consumed on use. |
| Attack Dog | $200 | 125 | none | 5 | Teeth (instant-kill vs. infantry) | Barracks | Detects spies; one-bite kill on all infantry |
| Rocketeer | $600 | 125 | none | 8 | 20mm cannon | Airforce Command HQ | Permanently airborne jet-pack unit; attacks air and ground |
| Spy | $1000 | 100 | flak | 4 | — | Battle Lab | Disguises as enemy infantry. Infiltrates structures for special effects (§6.5) |
| Navy SEAL | $1000 | 125 | flak | 5 | MP5 + C4 charges | Airforce Command HQ | Amphibious; rapid-fire vs. infantry; C4 destroys structures |
| Tanya | $1000 | 125 | flak | 5 | Dual pistols + C4 | Battle Lab | Hero unit (limit 1). Amphibious. Instant-kill on infantry. C4 destroys structures. |
| Chrono Legionnaire | $1500 | 125 | none | 5 | Neutron Rifle | Battle Lab | Teleports instead of walking. Weapon phases targets out of time: target is immobilized and gradually erased (duration scales with target HP — tanks take longer than infantry). Cannot be crushed while phasing in. |
| Sniper (GB) | $600 | 125 | none | 4 | AWP Rifle | Airforce Command HQ | One-shot kill on infantry at extreme range |

### 6.2 Allied Vehicles

| Unit | Cost | HP | Armor | Speed | Weapon | Prerequisites | Special |
|------|------|----|-------|-------|--------|---------------|---------|
| Chrono Miner | $1400 | 1000 | medium | 4 | — | Ore Refinery | Teleports back to refinery when full (no return trip). Lower ore capacity than War Miner. |
| Grizzly Battle Tank | $700 | 300 | heavy | 7 | 105mm cannon | War Factory | Fast, light tank. Faster but weaker than Soviet Rhino. |
| IFV | $600 | 200 | light | 8 | Hover Missile (default) | War Factory | Weapon changes based on garrisoned infantry (§6.4). Fast scout/support vehicle. |
| Mirage Tank | $1000 | 200 | light | 7 | Mirage Gun (heat beam) | Battle Lab | Disguises as a tree when stationary. Reveals when firing. |
| Prism Tank | $1200 | 150 | light | 4 | Prism beam | Battle Lab | Beam chains/refracts to nearby targets, dealing splash. Fragile. |
| Tank Destroyer (DE) | $900 | 400 | heavy | 5 | Armor-Piercing Cannon | Airforce Command HQ | Extreme damage vs. vehicles; near-useless vs. infantry and structures. |
| MCV | $3000 | 1000 | medium | 4 | — | Service Depot | Deploys into Construction Yard. |

### 6.3 Allied Naval

| Unit | Cost | HP | Armor | Speed | Weapon | Prerequisites | Special |
|------|------|----|-------|-------|--------|---------------|---------|
| Amphibious Transport | $900 | 200 | light | 6 | — | Naval Shipyard | Carries infantry and vehicles across water |
| Destroyer | $1000 | 600 | heavy | 6 | 155mm cannon + ASW launcher | Naval Shipyard | Anti-ship and anti-sub. Osprey helicopter auto-deploys vs. submerged targets. |
| Aegis Cruiser | $1200 | 800 | light | 4 | Medusa Missiles | Naval Shipyard | Anti-air only. Cannot attack ground/sea targets. |
| Dolphin | $500 | 200 | light | 8 | Sonic Pulse | Battle Lab | Anti-sub unit. Detects submerged units. |
| Aircraft Carrier | $2000 | 800 | heavy | 4 | Hornet Drones (3 aircraft) | Battle Lab | Launches 3 Hornet attack drones; long range. Hornets can be shot down and regenerate. |

### 6.4 Allied Aircraft

| Unit | Cost | HP | Armor | Speed | Weapon | Prerequisites | Special |
|------|------|----|-------|-------|--------|---------------|---------|
| Harrier | $1200 | 150 | light | 14 | Maverick Missiles | Airforce Command HQ | Strike aircraft. Requires landing pad (4 pads per AFCHQ). Returns to base after each attack run. |
| Black Eagle (KR) | $1200 | 200 | light | 14 | Maverick Missiles II | Airforce Command HQ | Upgraded Harrier — more HP and damage. Korea only. |
| Nighthawk Transport | $1000 | 175 | light | 14 | Blackhawk cannon (weak) | War Factory | Stealth helicopter transport. Invisible to enemy unless firing. |

### 6.5 Spy Infiltration Effects

| Target Structure | Effect |
|-----------------|--------|
| Power Plant / Tesla Reactor | Enemy power cut for **30 seconds** (75 seconds for Nuclear Reactor). Defenses go offline, production slowed. |
| Barracks | All infantry produced from the player's own Barracks gain **one veterancy rank** permanently. |
| War Factory | All vehicles/aircraft produced from the player's own War Factory gain **one veterancy rank** permanently. |
| Ore Refinery | Steals a large portion of the enemy's current credits (commonly cited as **50%**, though some sources report lower). |
| Battle Lab (Allied → Allied) | Unlocks **Chrono Commando** ($2000): teleportation + C4 + automatic rifle. |
| Battle Lab (Allied → Soviet) | Unlocks **Psi Commando** ($1000): mind control + C4 charges. |

### 6.6 IFV Weapon Combinations

The IFV changes its turret and weapon based on which infantry is loaded:

| Passenger | IFV Weapon | Role |
|-----------|-----------|------|
| Empty (default) | Hover Missile | Anti-air/light anti-ground |
| GI | Machine Gun turret | Anti-infantry |
| Engineer | Repair Drone | Heals nearby friendly vehicles (free, faster than Service Depot) |
| Rocketeer | — (standard) | No change |
| Navy SEAL | Machine Gun turret | Anti-infantry |
| Sniper (GB) | Long-range Sniper turret | Extreme range anti-infantry |
| Tanya | Pistol turret | Anti-infantry |
| Spy | — (standard) | No weapon change; gains spy detection |
| Chrono Legionnaire | Chrono beam turret | Temporal displacement weapon |
| Tesla Trooper | Tesla turret | Electric weapon, like a mini Tesla Tank |
| Flak Trooper | Flak turret | Anti-air flak cannon |
| Crazy Ivan | Demolition turret | Area explosive |
| Desolator | Radiation cannon turret | Radiation weapon |
| Psi-Corps Trooper (Yuri) | Psi-blast turret | Area psychic attack |
| Terrorist/Cow | Explodes | IFV detonates |
| Attack Dog | — (standard) | No weapon change; gains spy detection range |

---

## 7. Units — Soviet

### 7.1 Soviet Infantry

| Unit | Cost | HP | Armor | Speed | Weapon | Prerequisites | Special |
|------|------|----|-------|-------|--------|---------------|---------|
| Conscript | $100 | 125 | none | 4 | AKM rifle | Barracks | Cheapest infantry. Can garrison buildings but **cannot deploy** (unlike Allied GI). |
| Engineer | $500 | 75 | none | 4 | — | Barracks | Same as Allied Engineer — instant capture, consumed on use |
| Attack Dog | $200 | 100 | none | 5 | Teeth | Barracks | Same function as Allied Attack Dog |
| Flak Trooper | $300 | 150 | flak | 4 | Flak cannon | Radar Tower | Anti-air infantry. Also effective vs. light vehicles. |
| Tesla Trooper | $500 | 130 | plate | 4 | Tesla Zap | Radar Tower | Electric weapon. Immune to being crushed by vehicles. Can charge Tesla Coils (adds damage + acts as backup power). |
| Crazy Ivan | $600 | 125 | none | 4 | Dynamite | Radar Tower | Plants timed explosives on units/structures. Can bomb friendly units (e.g. send Attack Dogs as mobile bombs). |
| Terrorist (CU) | $200 | 75 | none | 5 | Suicide explosion | Radar Tower | Suicide infantry. Massive splash damage on death. Cuba only. |
| Desolator (IQ) | $600 | 150 | plate | 4 | Radiation cannon / Deploy | Radar Tower | Undeployed: rad beam vs. infantry. Deployed: irradiates surrounding area, creating impassable death zone. Iraq only. |
| Psi-Corps Trooper | $1200 | 100 | none | 4 | Mind Control | Battle Lab | Permanently mind-controls one enemy organic unit or vehicle. Cannot control structures, robots, or units already controlled. |

### 7.2 Soviet Vehicles

| Unit | Cost | HP | Armor | Speed | Weapon | Prerequisites | Special |
|------|------|----|-------|-------|--------|---------------|---------|
| War Miner | $1400 | 1000 | medium | 4 | 20mm cannon | Ore Refinery | Ore harvester. Armed with a mounted gun for self-defense. Drives back to refinery (no teleport). Higher ore capacity than Chrono Miner. |
| Rhino Heavy Tank | $900 | 400 | heavy | 5 | 120mm cannon | War Factory | Main battle tank. Slower but tougher than Allied Grizzly. |
| Flak Track | $500 | 180 | light | 8 | Flak cannon | War Factory | Anti-air APC. Carries up to 5 infantry. |
| Terror Drone | $500 | 100 | special_1 | 10 | Parasitic claws | War Factory | Leaps into enemy vehicles and dismantles them from inside. Removed only by Service Depot or Repair IFV. |
| V3 Rocket Launcher | $800 | 150 | light | 4 | V3 Rocket | Radar Tower | Long-range siege unit. Rockets can be shot down in flight. Very fragile. |
| Tesla Tank (RU) | $1200 | 300 | heavy | 5 | Tesla bolt | Radar Tower | Electric attack; fires over walls. Russia only. |
| Demolition Truck (LY) | $1500 | 100 | light | 6 | Nuclear charge | Radar Tower | Suicide vehicle. Detonation creates a small nuclear explosion with massive radius. Libya only. |
| Apocalypse Tank | $1750 | 800 | heavy | 4 | Twin 120mm cannons + SAM launchers | Battle Lab | Ultimate tank. Dual cannons vs. ground; missile launchers vs. air. Extremely slow. |
| MCV | $3000 | 1000 | medium | 4 | — | Service Depot | Deploys into Construction Yard. |

### 7.3 Soviet Naval

| Unit | Cost | HP | Armor | Speed | Weapon | Prerequisites | Special |
|------|------|----|-------|-------|--------|---------------|---------|
| Amphibious Transport | $900 | 200 | light | 6 | — | Naval Shipyard | 12 transport slots. Carries infantry and vehicles. |
| Typhoon Attack Sub | $1000 | 600 | heavy | 4 | Torpedoes | Naval Shipyard | Submerged by default; invisible unless detected by Destroyers, Dolphins, or Dolphins' sonar. Fires torpedoes while submerged. |
| Sea Scorpion | $600 | 400 | light | 6 | Flak cannon | Radar Tower | Anti-air/anti-ground naval unit. Versatile escort. |
| Giant Squid | $1000 | 200 | light | 6 | Tentacle grapple | Battle Lab | Grabs and slowly crushes enemy ships (immobilizes target). Submerged; invisible. Countered by Dolphins. |
| Dreadnought | $2000 | 800 | heavy | 4 | V3-class missiles (×2) | Battle Lab | Long-range siege ship. Fires 2 missiles per salvo. Missiles can be shot down. |

### 7.4 Soviet Aircraft

| Unit | Cost | HP | Armor | Speed | Weapon | Prerequisites | Special |
|------|------|----|-------|-------|--------|---------------|---------|
| Kirov Airship | $2000 | 2000 | light | 5 | Heavy bombs | Battle Lab | Massive HP. Drops devastating bombs directly below. Extremely slow. Main counter: massed anti-air. |

---

## 8. Structures

### 8.1 Allied Structures

| Structure | Cost | Power | Prerequisites | Purpose |
|-----------|------|-------|---------------|---------|
| Construction Yard | $3000 (MCV) | 0 | — | Enables building placement. Heart of the base. |
| Power Plant | $600 | +200 | — | Primary power source |
| Ore Refinery | $2000 | −50 | Power Plant | Processes ore into credits. Comes with 1 free Chrono Miner. |
| Barracks | $500 | −10 | Power Plant | Produces infantry |
| War Factory | $2000 | −25 | Ore Refinery + Barracks | Produces vehicles and Nighthawk Transport |
| Naval Shipyard | $1000 | −25 | Ore Refinery | Produces and repairs naval units. Must be placed in water. |
| Airforce Command HQ | $1000 | −50 | Ore Refinery | Provides radar. 4 jet pads for Harrier/Black Eagle production. |
| Service Depot | $800 | −25 | War Factory | Repairs vehicles/aircraft. Removes Terror Drones. |
| Battle Lab | $2000 | −100 | War Factory + Airforce Command HQ | Unlocks top-tier units and superweapons |
| Ore Purifier | $2500 | −200 | Battle Lab | +25% refinery income. Limit 1 per player. |

#### Allied Defenses

| Structure | Cost | Power | Prerequisites | Function |
|-----------|------|-------|---------------|----------|
| Fortress Wall | $100 | 0 | Barracks | Passive barrier. Blocks infantry and vehicles. |
| Pillbox | $500 | 0 | Barracks | Anti-infantry gun emplacement |
| Patriot Missile System | $1000 | −50 | Barracks | Anti-air defense. Effective vs. aircraft and missiles. |
| Prism Tower | $1500 | −75 | Airforce Command HQ | Anti-ground beam weapon. Adjacent Prism Towers **chain beams**: nearby towers redirect their beam into one tower, which fires an amplified shot. More towers in the chain = more damage. The supporting towers forfeit their own attack that cycle. A cluster of 4–5 towers can one-shot heavy tanks. |
| Grand Cannon (FR) | $2000 | −100 | Airforce Command HQ | Extreme-range anti-ground artillery. France only. Damages friendly units in blast. |

#### Allied Special Structures

| Structure | Cost | Power | Prerequisites | Function |
|-----------|------|-------|---------------|----------|
| Gap Generator | $1000 | −100 | Battle Lab | Creates shroud around itself, hiding base from enemy radar |
| Spy Satellite Uplink | $1000 | −100 | Battle Lab | Reveals entire map permanently (except Gap Generator areas) |
| Chronosphere | $2500 | −200 | Battle Lab | Superweapon. Teleports a group of vehicles anywhere on the map. **Kills infantry** caught in the field. Teleporting naval units onto land (or land units into water) destroys them. **5-minute charge.** Limit 1. |
| Weather Control Device | $5000 | −200 | Battle Lab | Superweapon. Creates devastating lightning storm at target location. Deals heavy damage over a wide area for several seconds. **10-minute charge.** Limit 1. |

### 8.2 Soviet Structures

| Structure | Cost | Power | Prerequisites | Purpose |
|-----------|------|-------|---------------|---------|
| Construction Yard | $3000 (MCV) | 0 | — | Enables building placement |
| Tesla Reactor | $600 | +150 | — | Primary power source (weaker than Allied Power Plant) |
| Ore Refinery | $2000 | −50 | Tesla Reactor | Processes ore. Comes with 1 free War Miner. |
| Barracks | $500 | −10 | Tesla Reactor | Produces infantry |
| War Factory | $2000 | −25 | Ore Refinery + Barracks | Produces vehicles |
| Naval Shipyard | $1000 | −20 | Ore Refinery | Produces and repairs naval units |
| Radar Tower | $1000 | −50 | Ore Refinery | Activates radar minimap. Equivalent to Airforce Command HQ for tech tree. |
| Service Depot | $800 | −20 | War Factory | Repairs vehicles. Removes Terror Drones. |
| Battle Lab | $2000 | −100 | War Factory + Radar Tower | Unlocks top-tier units and superweapons |
| Nuclear Reactor | $1000 | +1000 | Battle Lab | Massive power output (~7× Tesla Reactor). **Explodes like a small nuke when destroyed** — devastating to nearby structures. High risk/high reward. |
| Cloning Vats | $2500 | −200 | Battle Lab | Produces a **free clone** of every infantry unit trained at any Barracks (clone appears at the Vats). Effectively halves infantry cost. Limit 1 per player. |

#### Soviet Defenses

| Structure | Cost | Power | Prerequisites | Function |
|-----------|------|-------|---------------|----------|
| Fortress Wall | $100 | 0 | Barracks | Passive barrier |
| Sentry Gun | $500 | 0 | Barracks | Anti-infantry defense |
| Flak Cannon | $1000 | −50 | Barracks | Anti-air defense. Also effective vs. ground targets. |
| Tesla Coil | $1500 | −75 | Radar Tower | Powerful anti-ground electric bolt. High damage, moderate range. Tesla Troopers can target-fire a Tesla Coil to **charge** it: increases damage output and keeps the coil operational even during power outages. Multiple troopers stack. |

#### Soviet Special Structures

| Structure | Cost | Power | Prerequisites | Function |
|-----------|------|-------|---------------|----------|
| Psychic Sensor | $1000 | −50 | Battle Lab | Reveals enemy attack orders on minimap before they arrive |
| Iron Curtain Device | $2500 | −200 | Battle Lab | Superweapon. Makes a group of vehicles/structures in a small area **invulnerable** for ~30 seconds. **Kills infantry** caught in the area outright. **5-minute charge.** Limit 1. |
| Nuclear Missile Silo | $5000 | −200 | Battle Lab | Superweapon. Launches a nuclear missile at any point on the map. Massive area damage + leaves residual radiation zone. **10-minute charge.** Limit 1. |

---

## 9. Crates & Side Systems

### 9.1 Crates

Random pickup crates spawn on the map when enabled in game settings. A unit that moves over a crate collects it.

| Crate Type | Effect | Notes |
|------------|--------|-------|
| Money | +$2000 credits | — |
| Veterancy | +1 rank promotion | Area-of-effect: promotes all units in a 3×3 area. Can promote up to ~24 tanks in a 5×5 formation. |
| Heal | Full HP restore | Area-of-effect. Also removes Terror Drones from infected vehicles. |
| Armor Upgrade | +? armor bonus | Area-of-effect. Iron cube icon. |
| Firepower Upgrade | +? damage bonus | Area-of-effect. Flexing arm icon. |
| Speed Upgrade | +? speed bonus | Area-of-effect. Winged shoe icon. |
| Map Reveal | Reveals entire map | Like Spy Satellite Uplink, except Gap Generator areas remain hidden |
| Unit | Spawns a free random vehicle | Ground vehicles only (no air or naval). Can include MCVs and miners. |

### 9.2 Mind Control

Psi-Corps Troopers and the Psychic Beacon/Amplifier structures use mind control:

- One Psi-Corps Trooper controls **one** enemy unit at a time. Attempting to control a second releases the first.
- Controlled unit fights for the controller; killing the Psi-Corps Trooper releases the unit back to its original owner.
- **Immune to mind control**: Terror Drones (robotic), Attack Dogs (animal), other Psi-Corps Troopers, already mind-controlled units, buildings, aircraft in flight.
- Controlled units retain all their weapons, abilities, and veterancy.
- A mind control link is shown as a **purple beam** connecting controller to target.
- **Psychic Beacon** (campaign structure): Mind-controls all units in a large radius. Destroying the beacon releases all controlled units.

---

## 10. UI & HUD

### 10.1 HUD Layout

| Element | Position | Description |
|---------|----------|-------------|
| **Sidebar panel** | Right edge | Build tabs (Q/W/E/R), build queue, structure/unit icons |
| **Minimap/Radar** | Top-right (in sidebar) | Shows terrain, units, fog of war. Requires radar structure to function. |
| **Credits display** | Top of sidebar | Current credits, income indicator |
| **Power bar** | Left side of sidebar | Green/yellow/red bar showing power supply vs. demand |
| **Unit selection panel** | Bottom-center | Selected unit portrait, HP bar, veterancy rank, group info |
| **EVA alerts** | Top-center | Text/voice alerts ("Unit lost", "Building captured", "Nuclear launch detected") |
| **Tab buttons** | Top of sidebar | Q (structures), W (defenses), E (infantry), R (vehicles) |

### 10.2 HUD States

- **Normal gameplay**: Full HUD visible.
- **Low power**: Minimap goes dark. Power bar turns red.
- **Superweapon ready**: Flashing icon on sidebar. EVA announcement.
- **Superweapon launched (enemy)**: Full-screen warning: "Warning: Nuclear Missile Launched" / "Warning: Lightning Storm Created" / "Warning: Iron Curtain Activated" / "Warning: Chronosphere Activated".
- **FMV briefings**: HUD hidden entirely.

### 10.3 In-Game Indicators

- **Unit health bars**: Colored bar above each unit (green → yellow → red as HP decreases).
- **Veterancy chevrons**: Displayed next to unit; 1 chevron = veteran, 3 chevrons = elite.
- **Mind control lines**: Purple beam connecting controller to controlled unit.
- **Attack/move cursor**: Changes contextually — move arrow, attack crosshair, enter garrison icon, deploy icon, repair wrench.
- **Waypoint lines**: Yellow lines showing planned path when setting waypoints with Z key.
- **Rally point flag**: Marks where newly produced units auto-move after creation.

---

## 11. Multiplayer & Skirmish

### 11.1 Game Modes

- **Skirmish** (vs. AI): 1 human vs. 1–7 AI opponents.
- **Multiplayer** (LAN/Online): Up to 8 players. Supports team games and free-for-all.

### 11.2 Win Conditions

- **Destruction**: Destroy all enemy structures and units. Last player standing wins.
- No score victory, no time limit, no alternative win conditions in standard play.

### 11.3 Skirmish/Multiplayer Options

| Setting | Options |
|---------|---------|
| Starting Credits | Low ($5,000) / Medium ($10,000) / High ($20,000) |
| AI Difficulty | Easy / Normal / Hard / Brutal |
| Tech Level | 1–10 (restricts which tier of units/structures are available; 10 = everything) |
| Superweapons | On / Off |
| Crates | On / Off |
| Short Game | On (destroy all production structures to eliminate) / Off (destroy everything) |
| Shroud Regrows | On / Off (fog of war re-covers explored but unobserved areas) |
| Game Speed | Slowest through Fastest (affects all timers and movement proportionally) |
| Map Selection | Various maps for 2–8 players |

### 11.4 Multiplayer-Specific Features

- **Alliances**: Select an enemy unit and press **A** to propose alliance. Announced to all players.
- **Chat**: In-game text chat.
- **Observer mode**: Not available in base game.

---

## 12. Engine & Presentation Systems

### 12.1 Save System

- Campaign: Save/load at any time during a mission.
- Skirmish: Save/load available.
- Multiplayer: No save/load.

### 12.2 Rendering

- 2D sprite-based engine (Westwood's proprietary 2D isometric renderer, evolution of the Tiberian Sun engine).
- Supported resolutions: **640×480** and **800×600** (higher with unofficial patches).
- All units and structures are pre-rendered 2D sprites with directional frames.
- Terrain is tile-based with isometric diamond cells.

### 12.3 Camera

- Fixed **isometric** perspective.
- No camera rotation or zoom.
- Scroll by moving mouse to screen edges or using keyboard arrow keys.
- **H** key: Jump to primary Construction Yard.
- **F1–F4**: Bookmark/recall map positions.

### 12.4 Game Speed

- Adjustable from Slowest to Fastest.
- Affects all game timers proportionally: unit movement, build times, weapon fire rates, superweapon charge times.
- Online games typically played at default or slightly above.

### 12.5 Audio System

- **EVA** (Electronic Video Agent): Voice announcements for key events — "Construction complete", "Unit lost", "Our base is under attack", "Insufficient funds", "Nuclear launch detected".
- Units respond with **faction-appropriate voice lines** when selected and commanded.
- **Hell March 2** by Frank Klepacki is the main theme.
- Music dynamically shifts intensity based on combat state.

---

## 13. Open Questions / Unverified

The following values could not be confirmed from available online sources and would require direct rules.ini extraction or datamining to verify:

1. **Exact Soviet unit HP values**: The values listed in §7 (e.g. Rhino 400, Apocalypse 800, Kirov 2000) are widely cited in the community but could not be independently verified against original rules.ini in this research pass. Allied unit HP was verified via C&C Labs.
2. **Complete warhead Verses table**: Only SA (Small Arms) and HollowPoint warheads have verified Verses percentages. The full table of all ~30+ warheads (HE, AP, Fire, Electric, Super, Radiation, etc.) requires rules.ini extraction.
3. **Exact weapon Damage/ROF/Range values**: Only the M60 (GI primary: Damage 15, ROF 20, Range 4) has been verified. All other weapon stats need rules.ini data.
4. **Exact miner ore capacity**: Community consensus is War Miner carries ~2× per trip vs. Chrono Miner, but exact credit-per-load values per ore type are unverified.
5. **Iron Curtain exact duration**: Sources conflict — 30 seconds vs. 50 seconds.
6. **Spy Refinery steal percentage**: Sources cite both 20% and 50% of the enemy's credits.
7. **Tesla Trooper cost**: CNCNZ lists $600; other sources list $500. Needs rules.ini verification.
8. **Crate buff values**: Armor, firepower, and speed upgrade crate exact bonus percentages are unverified.
9. **Splash damage formulas**: Exact CellSpread and PercentAtMax values per warhead.
10. **Build time formulas**: How build time relates to cost and how multiple production structures scale it.
11. **Experience thresholds**: The VeteranRatio in rules.ini (reportedly 3.0) defines a cost multiplier — a unit is promoted after destroying 3× its own cost in enemies. Elite requires the same again. Needs confirmation.
12. **Ore regeneration rate**: Speed at which ore fields replenish and spread.
13. **Prism Tower chain scaling**: Exact damage multiplier per additional supporting tower.
14. **Paratroop cooldown**: America's Airborne ability cooldown is cited at ~4 minutes; exact value unverified.
15. **Desolator crush immunity**: Whether Desolators (plate armor) are immune to vehicle crushing like Tesla Troopers, or only Tesla Troopers have this property.

---

## 14. References

### Wikis & Databases
- [Unit Statistics — Red Alert 2](https://unitstatistics.com/red-alert2/) — Unit cost, HP, speed tables
- [C&C Labs — Allied Units](https://www.cnclabs.com/redalert2/allies/units.aspx) — Verified Allied unit stats (HP, armor, speed, cost, weapons)
- [CNCNZ — Allied Units](https://cncnz.com/games/red-alert-2/allied-units/) — Unit costs, prerequisites, descriptions
- [CNCNZ — Soviet Units](https://cncnz.com/games/red-alert-2/soviet-units/) — Unit costs, prerequisites, descriptions
- [CNCNZ — Allied Structures](https://cncnz.com/games/red-alert-2/allied-structures/) — Structure costs, power, prerequisites
- [CNCNZ — Soviet Structures](https://cncnz.com/games/red-alert-2/soviet-structures/) — Structure costs, power, prerequisites
- [ModEnc — Verses](https://modenc.renegadeprojects.com/Verses) — Damage multiplier system documentation
- [ModEnc — Armor Types](https://modenc.renegadeprojects.com/Armor_types) — Armor classification system
- [ModEnc — Warhead](https://modenc.renegadeprojects.com/Warhead) — Warhead properties documentation
- [ModEnc — IFV Weapon System](https://modenc.renegadeprojects.com/IFV_Weapon_System) — IFV turret mode mechanics
- [ModEnc — The YR Combat System](https://modenc.renegadeprojects.com/The_YR_Combat_System) — Combat calculation overview

### Guides & FAQs
- [GameFAQs — Guide by Varkovsky](https://gamefaqs.gamespot.com/pc/914165-command-and-conquer-red-alert-2/faqs/9349) — General walkthrough
- [GameFAQs — Rules.ini FAQ by AhKenshi](https://gamefaqs.gamespot.com/pc/914165-command-and-conquer-red-alert-2/faqs/15358) — Rules.ini editing reference
- [LP Archive — Units and Buildings](https://lparchive.org/Command-Conquer-Red-Alert-2/Update%2041/) — Unit descriptions with armor types
- [Revora Forums — Weapons Tutorial](https://forums.revora.net/topic/30334-weapons-ra2yr/) — Weapon/warhead configuration details
- [CnCmaps — Factions of Red Alert 2](http://cncmaps.net/factions-of-red-alert-2/) — Country-specific unit analysis
- [CnCmaps — Hotkeys](http://cncmaps.net/red-alert-2-hotkeys/) — Keyboard shortcuts reference
- [Red Alert 2 Online — Hotkeys](https://redalert2online.com/hotkeys.html) — Complete hotkey list
- [The Spoiler — Campaign Walkthrough](https://the-spoiler.com/STRATEGY/Westwood/red.alert.2.1.html) — Mission list and objectives

### Source Data
- [GitHub Gist — RA2 rules.ini](https://gist.github.com/bashkirtsevich/b2a5b2f32b39f8abb2f0f37c40f1294c) — Partial rules.ini (header sections only)
- [CnCmaps — Rules.ini Download](http://cncmaps.net/red-alert-2-rules-ini/) — Full rules.ini download link
