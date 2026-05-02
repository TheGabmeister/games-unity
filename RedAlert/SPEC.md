# SPEC.md — Command & Conquer: Red Alert (1996)

Gameplay specification for the Unity recreation of Westwood Studios' Command & Conquer: Red Alert. This document covers the base game only (not Counterstrike/Aftermath expansions). All numeric values are sourced from the original `rules.ini`.

---

## Table of Contents

- [Factions](#factions)
- [Economy](#economy)
- [Base Building](#base-building)
- [Buildings — Shared](#buildings--shared)
- [Buildings — Allied](#buildings--allied)
- [Buildings — Soviet](#buildings--soviet)
- [Tech Trees](#tech-trees)
- [Infantry](#infantry)
- [Vehicles](#vehicles)
- [Naval Units](#naval-units)
- [Aircraft](#aircraft)
- [Weapons & Warheads](#weapons--warheads)
- [Terrain & Map](#terrain--map)
- [Fog of War & Shroud](#fog-of-war--shroud)
- [Combat Mechanics](#combat-mechanics)
- [Controls & Unit Commands](#controls--unit-commands)
- [Superweapons & Support Powers](#superweapons--support-powers)
- [Crates](#crates)
- [AI Behavior](#ai-behavior)
- [Campaign — Allied](#campaign--allied)
- [Campaign — Soviet](#campaign--soviet)
- [Multiplayer & Skirmish](#multiplayer--skirmish)

---

## Factions

Two playable factions, each with unique units, buildings, and superweapons.

**Allies** — Technologically advanced. Strengths: naval superiority (Cruisers, Destroyers), hero unit (Tanya), intelligence tools (Spies, Gap Generators), Chronosphere teleportation. Weaknesses: weaker tanks, no attack dogs, no Tesla technology.

**Soviets** — Raw firepower and armor. Strengths: Heavy and Mammoth Tanks, Tesla Coils, aircraft (MiGs, Yaks), submarines, Iron Curtain invulnerability. Weaknesses: weaker navy surface fleet, no stealth/intel tools, slower tech.

### Country Bonuses (Multiplayer)

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

## Economy

### Resources

Two resource types found on the map:

| Resource | Appearance     | Value per Bail | Bails per Load | Full Load Value | Regenerates |
|----------|----------------|----------------|----------------|-----------------|-------------|
| Ore      | Yellow/brown   | $25            | 28             | $700            | Yes — grows and spreads every 2 minutes |
| Gems     | Blue/purple    | $50            | 28             | $1,400          | No — once harvested, gone permanently |

### Harvesting Loop

1. Ore Truck drives to nearest ore/gem field
2. Scoops up resources (up to 28 bails)
3. Returns to nearest Ore Refinery to deposit
4. Credits added to player's account (capped by storage capacity)
5. Repeat

### Storage

| Structure     | Storage Capacity |
|---------------|-----------------|
| Ore Refinery  | 2,000 credits   |
| Ore Silo      | 1,500 credits   |

- Total storage = sum of all Refineries + all Silos
- Excess credits beyond storage capacity are **lost**
- Destroying a storage building loses a proportional share of stored credits

### Starting Credits

Skirmish default: **$10,000** (configurable in lobby). Campaign missions have variable starting amounts.

---

## Base Building

### Construction Sidebar

- All construction is initiated from the right-side sidebar panel
- A progress bar fills as the Construction Yard assembles the building internally
- Once complete, the player places the building footprint on the map within valid range
- **One building at a time** per Construction Yard
- Owning multiple Construction Yards allows parallel construction queues

### Build Time Formula

```
Build Time (minutes) = (Cost / 1000) × 0.8
```

A $1,000 building takes ~48 seconds. Multiple same-type production buildings (e.g., 2 War Factories) reduce unit build time proportionally.

### Placement Rules

- Buildings must be placed within a roughly **16-cell radius** of a Construction Yard
- New buildings must also be placed **within 2 cells** of an existing friendly building
- Walls can be placed up to **7 cells** away
- Naval Yard / Sub Pen can be placed up to **8 cells** away
- Cannot build on water (except naval buildings), cliffs, or occupied cells

### MCV Deployment

- MCV deploys into a Construction Yard (nearly instant)
- Construction Yard can be **re-packed** back into an MCV
- MCV cost: $2,500 | HP: 600 | Armor: Light | Speed: 6

### Selling Buildings

- Any building can be sold via the sidebar Sell button
- Refund: **50%** of original cost, scaled by remaining HP
- Crewed buildings spawn an infantry unit when sold

### Repairing Buildings

- Click the Repair wrench icon, then click a damaged building
- Repairs cost ~**20%** of the building's original price to go from near-zero to full HP
- Repairs drain credits continuously; pauses if funds run out
- An **Engineer** entering a friendly building restores it to 100% instantly (consumes the Engineer)

---

## Buildings — Shared

Buildings available to both Allied and Soviet factions.

### Construction Yard (FACT)

| Stat | Value |
|------|-------|
| HP | 1,000 |
| Cost | $2,500 (deployed from MCV) |
| Power | 0 |
| Armor | Heavy |
| Prerequisites | None (deploy MCV) |

Enables construction of all other buildings. Can be re-packed into an MCV. Capturable by Engineers.

### Power Plant (POWR)

| Stat | Value |
|------|-------|
| HP | 400 |
| Cost | $300 |
| Power | +100 |
| Armor | Wood |
| Prerequisites | Construction Yard |

Basic power supply. Required for almost all other buildings.

### Advanced Power Plant (APWR)

| Stat | Value |
|------|-------|
| HP | 700 |
| Cost | $500 |
| Power | +200 |
| Armor | Wood |
| Prerequisites | Power Plant |

Double the power of a standard Power Plant at lower per-unit cost.

### Ore Refinery (PROC)

| Stat | Value |
|------|-------|
| HP | 900 |
| Cost | $2,000 |
| Power | −30 |
| Armor | Wood |
| Storage | 2,000 credits |
| Prerequisites | Power Plant |

Converts ore/gems into credits. Comes with one **free Ore Truck**.

### Ore Silo (SILO)

| Stat | Value |
|------|-------|
| HP | 300 |
| Cost | $150 |
| Power | −10 |
| Armor | Wood |
| Storage | 1,500 credits |
| Prerequisites | Ore Refinery |

Additional ore storage. Excess credits beyond total storage are lost.

### Radar Dome (DOME)

| Stat | Value |
|------|-------|
| HP | 1,000 |
| Cost | $1,000 |
| Power | −40 |
| Armor | Wood |
| Sight | 10 |
| Prerequisites | Ore Refinery |

Provides the radar minimap. Requires power to function. Has sensors (detects cloaked units). Unlocks air units, SAM/AA, and advanced structures.

### War Factory (WEAP)

| Stat | Value |
|------|-------|
| HP | 1,000 |
| Cost | $2,000 |
| Power | −30 |
| Armor | Light |
| Prerequisites | Ore Refinery |

Builds ground vehicles. Required for Tech Centers and Service Depots.

### Service Depot (FIX)

| Stat | Value |
|------|-------|
| HP | 800 |
| Cost | $1,200 |
| Power | −30 |
| Armor | Wood |
| Prerequisites | War Factory |

Repairs damaged vehicles and aircraft when driven/landed onto it. Costs credits proportional to damage repaired.

### Helipad (HPAD)

| Stat | Value |
|------|-------|
| HP | 800 |
| Cost | $1,500 |
| Power | −10 |
| Armor | Wood |
| Prerequisites | Radar Dome |

Builds helicopters and provides rearming. Comes with one **free helicopter** (Longbow for Allies, Hind for Soviets).

### Missile Silo (MSLO)

| Stat | Value |
|------|-------|
| HP | 400 |
| Cost | $2,500 |
| Power | −100 |
| Armor | Heavy |
| Prerequisites | Tech Center |

Launches nuclear missiles. ~14-minute recharge. Available to both factions in multiplayer.

### Concrete Wall (BRIK)

| Stat | Value |
|------|-------|
| HP | Special (wall rules) |
| Cost | $100 |
| Armor | Concrete |

Strongest wall. Blocks all ground movement and tank ordnance. Must be destroyed by heavy weapons.

---

## Buildings — Allied

### Barracks / Tent (TENT)

| Stat | Value |
|------|-------|
| HP | 800 |
| Cost | $300 |
| Power | −20 |
| Armor | Wood |
| Prerequisites | Power Plant |

Trains all Allied infantry.

### Naval Yard (SYRD)

| Stat | Value |
|------|-------|
| HP | 1,000 |
| Cost | $650 |
| Power | −30 |
| Armor | Light |
| Prerequisites | Power Plant |

Builds Allied naval vessels. Must be placed on water.

### Pillbox (PBOX)

| Stat | Value |
|------|-------|
| HP | 400 |
| Cost | $400 |
| Power | −15 |
| Armor | Wood |
| Prerequisites | Barracks |

Anti-infantry defense. Rapid-fire Vulcan cannon. Effective vs infantry, weak vs vehicles.

### Camouflaged Pillbox (HBOX)

| Stat | Value |
|------|-------|
| HP | 600 |
| Cost | $600 |
| Power | −15 |
| Armor | Wood |
| Prerequisites | Barracks |

Invisible on enemy radar until it fires or enemies get close. Same weapon as Pillbox but with more HP.

### Gun Turret (GUN)

| Stat | Value |
|------|-------|
| HP | 400 |
| Cost | $600 |
| Power | −40 |
| Armor | Heavy |
| Prerequisites | Barracks |

Anti-vehicle defense. Primary Allied anti-armor structure.

### Anti-Aircraft Gun (AGUN)

| Stat | Value |
|------|-------|
| HP | 400 |
| Cost | $600 |
| Power | −50 |
| Armor | Heavy |
| Prerequisites | Radar Dome |

Rapid-fire AA defense. Requires power to function.

### Allied Tech Center (ATEK)

| Stat | Value |
|------|-------|
| HP | 400 |
| Cost | $1,500 |
| Power | −200 |
| Armor | Wood |
| Sight | 10 |
| Prerequisites | War Factory + Radar Dome |

Unlocks highest-tier Allied units: Cruiser, GPS Satellite, Gap Generator, Chronosphere, Tanya, Thief.

### Gap Generator (GAP)

| Stat | Value |
|------|-------|
| HP | 1,000 |
| Cost | $500 |
| Power | −60 |
| Armor | Wood |
| Sight | 10 |
| Prerequisites | Allied Tech Center |

Re-shrouds a wide radius around itself, hiding your base from enemy radar. Requires power.

### Chronosphere (PDOX)

| Stat | Value |
|------|-------|
| HP | 400 |
| Cost | $2,800 |
| Power | −200 |
| Armor | Wood |
| Prerequisites | Allied Tech Center |

Allied superweapon. Teleports a single vehicle anywhere on the map. Unit returns to origin after ~3 minutes. Requires power.

### Sandbag Wall (SBAG)

| Stat | Value |
|------|-------|
| Cost | $25 |

Cheapest wall. Stops infantry and non-tracked vehicles. Easily destroyed.

---

## Buildings — Soviet

### Barracks (BARR)

| Stat | Value |
|------|-------|
| HP | 800 |
| Cost | $300 |
| Power | −20 |
| Armor | Wood |
| Prerequisites | Power Plant |

Trains Soviet infantry.

### Kennel (KENN)

| Stat | Value |
|------|-------|
| HP | 400 |
| Cost | $200 |
| Power | −10 |
| Armor | Wood |
| Prerequisites | Barracks |

Trains Attack Dogs.

### Sub Pen (SPEN)

| Stat | Value |
|------|-------|
| HP | 1,000 |
| Cost | $650 |
| Power | −30 |
| Armor | Light |
| Prerequisites | Power Plant |

Builds Soviet naval vessels. Must be placed on water.

### Airfield (AFLD)

| Stat | Value |
|------|-------|
| HP | 1,000 |
| Cost | $600 |
| Power | −30 |
| Armor | Heavy |
| Sight | 7 |
| Prerequisites | Radar Dome |

Builds fixed-wing aircraft (Yak, MiG). Provides Spy Plane, Paratroopers, and Parabombs support powers.

### Flame Tower (FTUR)

| Stat | Value |
|------|-------|
| HP | 400 |
| Cost | $600 |
| Power | −20 |
| Armor | Heavy |
| Prerequisites | Barracks |

Short-range anti-infantry/anti-vehicle defense. Fireballs. Can damage friendly units.

### SAM Site (SAM)

| Stat | Value |
|------|-------|
| HP | 400 |
| Cost | $750 |
| Power | −20 |
| Armor | Heavy |
| Prerequisites | Radar Dome |

Soviet anti-air defense. Guided missiles. Does not require continuous power (unlike Allied AA Gun).

### Tesla Coil (TSLA)

| Stat | Value |
|------|-------|
| HP | 400 |
| Cost | $1,500 |
| Power | −150 |
| Armor | Heavy |
| Sight | 8 |
| Prerequisites | War Factory |

Most powerful Soviet defense. Devastating electrical bolts that deal 100% damage to all armor types. Requires power to function. Goes offline when base power is insufficient.

### Soviet Tech Center (STEK)

| Stat | Value |
|------|-------|
| HP | 600 |
| Cost | $1,500 |
| Power | −100 |
| Armor | Wood |
| Prerequisites | War Factory + Radar Dome |

Unlocks highest-tier Soviet units: Mammoth Tank, Flamethrower infantry, Iron Curtain, Missile Silo.

### Iron Curtain (IRON)

| Stat | Value |
|------|-------|
| HP | 400 |
| Cost | $2,800 |
| Power | −200 |
| Armor | Wood |
| Prerequisites | Soviet Tech Center |

Soviet superweapon. Makes a single vehicle or structure temporarily invulnerable for ~45 seconds. Kills infantry inside transports. Requires power.

### Wire Fence (FENC)

| Stat | Value |
|------|-------|
| Cost | $25 |

Soviet equivalent of Allied Sandbags. Cheap barrier.

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

## Infantry

### Rifle Infantry (E1) — Both Factions

| Stat | Value |
|------|-------|
| HP | 50 |
| Armor | None |
| Speed | 4 |
| Cost | $100 |
| Sight | 4 |
| Weapon | M1Carbine — 15 dmg, ROF 20, Range 3, SA warhead |
| Prerequisites | Barracks |

Basic combat unit. Cheap and expendable.

### Grenadier (E2) — Soviet

| Stat | Value |
|------|-------|
| HP | 50 |
| Armor | None |
| Speed | 5 |
| Cost | $160 |
| Sight | 4 |
| Weapon | Grenade — 50 dmg, ROF 60, Range 4, HE warhead (splash) |
| Prerequisites | Barracks |

Effective vs buildings and infantry groups. Explodes on death.

### Rocket Soldier (E3) — Allied (both in multiplayer)

| Stat | Value |
|------|-------|
| HP | 45 |
| Armor | None |
| Speed | 3 |
| Cost | $300 |
| Sight | 4 |
| Primary | RedEye — 50 dmg, ROF 50, Range 7.5, AP warhead (anti-air) |
| Secondary | Dragon — 35 dmg, ROF 50, Range 5, AP warhead (anti-ground) |
| Prerequisites | Barracks |

Anti-air and anti-armor infantry.

### Flamethrower (E4) — Soviet

| Stat | Value |
|------|-------|
| HP | 40 |
| Armor | None |
| Speed | 3 |
| Cost | $300 |
| Sight | 4 |
| Weapon | Flamer — 70 dmg, ROF 50, Range 3.5, Fire warhead |
| Prerequisites | Barracks + Soviet Tech Center |

Devastating vs infantry and buildings at short range. Explodes on death.

### Engineer (E6) — Both Factions

| Stat | Value |
|------|-------|
| HP | 25 |
| Armor | None |
| Speed | 4 |
| Cost | $500 |
| Sight | 4 |
| Weapon | None |
| Prerequisites | Barracks |

Captures enemy buildings or fully repairs friendly ones. Consumed on use. Unarmed.

### Spy — Allied

| Stat | Value |
|------|-------|
| HP | 25 |
| Armor | None |
| Speed | 4 |
| Cost | $500 |
| Sight | 5 |
| Weapon | None |
| Prerequisites | Barracks + Radar Dome |

Disguises as enemy infantry. Infiltrates buildings for intel: reveals map, resets superweapon timers, steals tech. Detected by Attack Dogs.

### Thief — Allied

| Stat | Value |
|------|-------|
| HP | 25 |
| Armor | None |
| Speed | 4 |
| Cost | $500 |
| Sight | 5 |
| Weapon | None |
| Prerequisites | Barracks + Allied Tech Center |

Steals half the credits from enemy Ore Refineries or Silos. Consumed on use.

### Tanya (E7) — Allied

| Stat | Value |
|------|-------|
| HP | 100 |
| Armor | None |
| Speed | 5 |
| Cost | $1,200 |
| Sight | 6 |
| Weapon | Dual Colt45 — 50 dmg each, ROF 5, Range 5.75, HollowPoint warhead |
| Prerequisites | Barracks + Allied Tech Center |

Hero unit (limit 1). Instantly kills infantry with dual pistols. Can plant C4 on buildings for instant destruction. HollowPoint warhead = 100% vs infantry, 5% vs everything else.

### Field Medic — Allied

| Stat | Value |
|------|-------|
| HP | 80 |
| Armor | None |
| Speed | 4 |
| Cost | $800 |
| Sight | 3 |
| Weapon | Heal — restores 50 HP, ROF 80, Range 1.83 |
| Prerequisites | Barracks |

Heals nearby friendly infantry. Unarmed.

### Attack Dog — Soviet

| Stat | Value |
|------|-------|
| HP | 12 |
| Armor | None |
| Speed | 4 |
| Cost | $200 |
| Sight | 5 |
| Weapon | DogJaw — 100 dmg, ROF 10, Range 2.2, Organic warhead |
| Prerequisites | Kennel |

Instant-kill vs all infantry (Organic warhead = 100% vs None armor, 0% vs everything else). Detects disguised Spies. Useless vs vehicles and buildings.

---

## Vehicles

### Ranger / Jeep (JEEP) — Allied

| Stat | Value |
|------|-------|
| HP | 150 |
| Armor | Light |
| Speed | 10 |
| Cost | $600 |
| Sight | 6 |
| Weapon | M60mg — 15 dmg, ROF 20, Range 4, SA warhead |
| Prerequisites | War Factory |

Fastest ground combat vehicle. Good scout. Driver bails out on destruction.

### Light Tank (1TNK) — Allied

| Stat | Value |
|------|-------|
| HP | 300 |
| Armor | Heavy |
| Speed | 9 |
| Cost | $700 |
| Sight | 4 |
| Weapon | 75mm — 25 dmg, ROF 40, Range 4, AP warhead |
| Prerequisites | War Factory |

Fastest tank. Good cost-efficiency.

### Medium Tank (2TNK) — Allied

| Stat | Value |
|------|-------|
| HP | 400 |
| Armor | Heavy |
| Speed | 8 |
| Cost | $800 |
| Sight | 5 |
| Weapon | 90mm — 30 dmg, ROF 50, Range 4.75, AP warhead |
| Prerequisites | War Factory |

Allied mainstay. Balanced firepower, armor, and speed.

### Heavy Tank (3TNK) — Soviet

| Stat | Value |
|------|-------|
| HP | 400 |
| Armor | Heavy |
| Speed | 7 |
| Cost | $950 |
| Sight | 5 |
| Primary | 105mm — 30 dmg, ROF 70, Range 4.75, AP warhead |
| Secondary | 105mm (dual barrel — fires twice = 60 effective dmg per volley) |
| Prerequisites | War Factory |

Soviet workhorse. Dual cannons double its effective damage output.

### Mammoth Tank (4TNK) — Soviet

| Stat | Value |
|------|-------|
| HP | 600 |
| Armor | Heavy |
| Speed | 4 |
| Cost | $1,700 |
| Sight | 6 |
| Primary | 120mm — 40 dmg, ROF 80, Range 4.75, AP warhead, Burst 2 (80 dmg/volley) |
| Secondary | MammothTusk — 75 dmg, ROF 80, Range 5, HE warhead, Burst 2 (150 dmg/volley, anti-air capable) |
| Prerequisites | War Factory + Soviet Tech Center |

Heaviest unit. Self-heals to 50% HP. Dual cannons + dual missile launchers. Can engage air. Slowest tank.

### V2 Rocket Launcher (V2RL) — Soviet

| Stat | Value |
|------|-------|
| HP | 150 |
| Armor | Light |
| Speed | 7 |
| Cost | $700 |
| Sight | 5 |
| Weapon | SCUD — 600 dmg, ROF 400, Range 10, HE warhead |
| Prerequisites | War Factory + Radar Dome |

Extreme damage per shot, very slow reload. Can destroy most buildings in 2 hits. Cannot fire while moving. Fragile.

### Artillery (ARTY) — Allied

| Stat | Value |
|------|-------|
| HP | 75 |
| Armor | Light |
| Speed | 6 |
| Cost | $600 |
| Sight | 5 |
| Weapon | 155mm — 150 dmg, ROF 65, Range 6, HE warhead (indirect/ballistic) |
| Prerequisites | War Factory + Radar Dome |

Long-range indirect fire. Devastating vs infantry and structures. Extremely fragile. Cannot fire while moving.

### APC — Allied

| Stat | Value |
|------|-------|
| HP | 200 |
| Armor | Heavy |
| Speed | 10 |
| Cost | $800 |
| Sight | 5 |
| Weapon | M60mg — 15 dmg, ROF 20, Range 4, SA warhead |
| Passengers | 5 infantry |
| Prerequisites | War Factory + Barracks |

Fast infantry transport.

### Mobile Radar Jammer (MRJ) — Allied

| Stat | Value |
|------|-------|
| HP | 110 |
| Armor | Light |
| Speed | 9 |
| Cost | $600 |
| Sight | 7 |
| Weapon | None |
| Prerequisites | War Factory + Radar Dome |

Jams enemy radar within radius. Unarmed.

### Mobile Gap Generator (MGG) — Allied

| Stat | Value |
|------|-------|
| HP | 110 |
| Armor | Light |
| Speed | 9 |
| Cost | $600 |
| Sight | 4 |
| Weapon | None |
| Prerequisites | War Factory + Allied Tech Center |

Creates shroud around itself hiding friendly units from enemy radar. Unarmed.

### Mine Layer (MNLY) — Both Factions

| Stat | Value |
|------|-------|
| HP | 100 |
| Armor | Heavy |
| Speed | 9 |
| Cost | $800 |
| Sight | 5 |
| Weapon | Deploys 5 mines (anti-tank for Allies, anti-personnel for Soviets) |
| Prerequisites | War Factory + Service Depot |

### Ore Truck / Harvester (HARV) — Both Factions

| Stat | Value |
|------|-------|
| HP | 600 |
| Armor | Heavy |
| Speed | 6 |
| Cost | $1,400 (one free with Ore Refinery) |
| Sight | 4 |
| Weapon | None |

Self-heals. Collects ore/gems and returns to Refinery. Very durable.

### Mobile Construction Vehicle (MCV) — Both Factions

| Stat | Value |
|------|-------|
| HP | 600 |
| Armor | Light |
| Speed | 6 |
| Cost | $2,500 |
| Sight | 4 |
| Weapon | None |
| Prerequisites | War Factory + Service Depot |

Deploys into Construction Yard. Most important unit in the game.

---

## Naval Units

### Transport (LST) — Both Factions

| Stat | Value |
|------|-------|
| HP | 350 |
| Armor | Heavy |
| Speed | 14 |
| Cost | $700 |
| Sight | 6 |
| Weapon | None |
| Passengers | 5 ground units (vehicles or infantry) |
| Prerequisites | Naval Yard / Sub Pen |

Fastest naval unit. Unarmed.

### Gunboat (PT) — Allied

| Stat | Value |
|------|-------|
| HP | 200 |
| Armor | Heavy |
| Speed | 9 |
| Cost | $500 |
| Sight | 7 |
| Primary | 2Inch — 25 dmg, ROF 60, Range 5.5, AP warhead |
| Secondary | DepthCharge — 80 dmg, ROF 60, Range 5, AP warhead (anti-sub) |
| Prerequisites | Naval Yard |

Lightest Allied warship. Has sensors (detects submarines).

### Destroyer (DD) — Allied

| Stat | Value |
|------|-------|
| HP | 400 |
| Armor | Heavy |
| Speed | 6 |
| Cost | $1,000 |
| Sight | 6 |
| Primary | Stinger — 30 dmg, ROF 60, Range 9, AP warhead, Burst 2 (60 dmg/volley) |
| Secondary | DepthCharge — 80 dmg, ROF 60, Range 5, AP warhead (anti-sub) |
| Prerequisites | Naval Yard |

Has sensors. Fast-firing homing missiles. Can engage land, sea, and air targets.

### Cruiser (CA) — Allied

| Stat | Value |
|------|-------|
| HP | 700 |
| Armor | Heavy |
| Speed | 4 |
| Cost | $2,000 |
| Sight | 7 |
| Primary | 8Inch — 500 dmg, ROF 160, Range 22, HE warhead |
| Secondary | 8Inch (dual turrets — 1,000 dmg/volley) |
| Prerequisites | Naval Yard + Allied Tech Center |

Longest range in the game (22). Devastating naval bombardment. Cannot target submarines. Slow.

### Submarine (SS) — Soviet

| Stat | Value |
|------|-------|
| HP | 120 |
| Armor | Light |
| Speed | 6 |
| Cost | $950 |
| Sight | 6 |
| Weapon | TorpTube — 90 dmg, ROF 60, Range 9, AP warhead |
| Prerequisites | Sub Pen |

Cloaked when submerged. Must surface briefly to fire. Only attacks naval targets. Detected by units with Sensors.

---

## Aircraft

### Longbow Helicopter (HELI) — Allied

| Stat | Value |
|------|-------|
| HP | 225 |
| Armor | Heavy |
| Speed | 16 |
| Cost | $1,200 (free with Helipad) |
| Ammo | 6 |
| Weapon | Hellfire — 40 dmg, ROF 60, Range 4, AP warhead |
| Prerequisites | Helipad |

Anti-armor specialist. Returns to Helipad to rearm after 6 shots.

### Hind Helicopter (HIND) — Soviet

| Stat | Value |
|------|-------|
| HP | 225 |
| Armor | Heavy |
| Speed | 12 |
| Cost | $1,200 (free with Helipad) |
| Ammo | 12 |
| Weapon | ChainGun — 40 dmg, ROF 3, Range 5, SA warhead |
| Prerequisites | Helipad |

Extremely fast rate of fire. Effective vs infantry and light vehicles.

### Chinook Transport (TRAN) — Soviet

| Stat | Value |
|------|-------|
| HP | 90 |
| Armor | Light |
| Speed | 12 |
| Cost | $1,200 |
| Passengers | 5 infantry |
| Weapon | None |
| Prerequisites | Helipad |

Air infantry transport. Fragile.

### Yak Attack Plane (YAK) — Soviet

| Stat | Value |
|------|-------|
| HP | 60 |
| Armor | Light |
| Speed | 16 |
| Cost | $800 |
| Ammo | 15 |
| Weapon | ChainGun — 40 dmg, ROF 3, Range 5, SA warhead |
| Prerequisites | Airfield |

Strafing aircraft. Effective vs infantry. Vulnerable to AA.

### MiG Attack Plane (MIG) — Soviet

| Stat | Value |
|------|-------|
| HP | 50 |
| Armor | Light |
| Speed | 20 |
| Cost | $1,200 |
| Ammo | 3 |
| Primary | Maverick — 50 dmg, ROF 3, Range 6, AP warhead |
| Secondary | Maverick (dual — fires twice) |
| Prerequisites | Airfield |

Fastest unit in the game. Powerful anti-armor missiles. Only 3 shots before rearming.

### Spy Plane (U2) — Soviet (support power)

| Stat | Value |
|------|-------|
| HP | 2,000 (effectively invulnerable) |
| Speed | 40 |

Reveals fog of war over target area. Summoned via Airfield. Cannot be shot down.

### Badger Bomber (BADR) — Soviet (support power)

| Stat | Value |
|------|-------|
| HP | 60 |
| Speed | 16 |
| Ammo | 5 |
| Weapon | ParaBomb — 300 dmg, ROF 4, Range 4.5, HE warhead |

Delivers Paratrooper drops and Parabomb strikes. Not player-controllable.

---

## Weapons & Warheads

### Warhead Damage Modifiers

Damage is calculated as: **Weapon Damage × Warhead modifier for target armor type**

Five armor types: **None** (infantry), **Wood** (buildings), **Light**, **Heavy**, **Concrete**

| Warhead | vs None (Infantry) | vs Wood (Buildings) | vs Light | vs Heavy | vs Concrete |
|---------|-------------------|-------------------|----------|----------|-------------|
| SA (Small Arms) | 100% | 50% | 60% | 25% | 25% |
| HE (High Explosive) | 90% | 75% | 60% | 25% | 100% |
| AP (Armor Piercing) | 30% | 75% | 75% | 100% | 50% |
| Fire (Napalm) | 90% | 100% | 60% | 25% | 50% |
| HollowPoint | 100% | 5% | 5% | 5% | 5% |
| Super (Tesla) | 100% | 100% | 100% | 100% | 100% |
| Organic (Dog) | 100% | 0% | 0% | 0% | 0% |
| Nuke | 90% | 100% | 60% | 25% | 50% |

### Weapon Reference Table

| Weapon | Used By | Damage | ROF | Range | Warhead |
|--------|---------|--------|-----|-------|---------|
| M1Carbine | Rifle Infantry | 15 | 20 | 3 | SA |
| Grenade | Grenadier | 50 | 60 | 4 | HE |
| RedEye | Rocket Soldier (AA) | 50 | 50 | 7.5 | AP |
| Dragon | Rocket Soldier (ground) | 35 | 50 | 5 | AP |
| Flamer | Flamethrower | 70 | 50 | 3.5 | Fire |
| Colt45 | Tanya (×2) | 50 | 5 | 5.75 | HollowPoint |
| DogJaw | Attack Dog | 100 | 10 | 2.2 | Organic |
| Heal | Field Medic | −50 | 80 | 1.83 | Organic |
| M60mg | Ranger, APC | 15 | 20 | 4 | SA |
| 75mm | Light Tank | 25 | 40 | 4 | AP |
| 90mm | Medium Tank | 30 | 50 | 4.75 | AP |
| 105mm | Heavy Tank (×2) | 30 | 70 | 4.75 | AP |
| 120mm | Mammoth Tank | 40 | 80 | 4.75 | AP |
| MammothTusk | Mammoth Tank (secondary) | 75 | 80 | 5 | HE |
| 155mm | Artillery | 150 | 65 | 6 | HE |
| SCUD | V2 Rocket Launcher | 600 | 400 | 10 | HE |
| TeslaZap | Tesla Coil | 100 | 120 | 8.5 | Super |
| ChainGun | Hind, Yak | 40 | 3 | 5 | SA |
| Hellfire | Longbow | 40 | 60 | 4 | AP |
| Maverick | MiG (×2) | 50 | 3 | 6 | AP |
| TorpTube | Submarine | 90 | 60 | 9 | AP |
| Stinger | Destroyer | 30 | 60 | 9 | AP |
| DepthCharge | Destroyer, Gunboat | 80 | 60 | 5 | AP |
| 2Inch | Gunboat | 25 | 60 | 5.5 | AP |
| 8Inch | Cruiser (×2) | 500 | 160 | 22 | HE |
| ParaBomb | Badger Bomber | 300 | 4 | 4.5 | HE |
| FireballLauncher | Flame Tower | 125 | 50 | 4 | Fire |
| Nike | SAM Site | 50 | 20 | 7.5 | AP |
| TurretGun | Gun Turret | 40 | 50 | 6 | AP |

**ROF** = Rate of Fire (lower = faster). **Range** = in cells. **Burst** = shots per attack cycle (listed in parentheses where >1).

---

## Terrain & Map

### Tile System

- Cell-based grid. Each cell is one square, fits one vehicle.
- Cell size: **24×24 pixels** at original resolution.
- Maximum map dimensions: **126×126 cells**.

### Terrain Types & Movement Speed

Speed values are percentages of a unit's base speed. 0% = impassable.

| Terrain | Foot | Tracked | Wheeled | Float (Naval) |
|---------|------|---------|---------|---------------|
| Clear (grass) | 90% | 80% | 60% | 0% |
| Road | 100% | 100% | 100% | 0% |
| Rough (rocky) | 80% | 70% | 40% | 0% |
| Ore field | 90% | 70% | 50% | 0% |
| Beach/Sand | 80% | 70% | 40% | 0% |
| Water | 0% | 0% | 0% | 100% |
| River | varies | 0% | 0% | 100% |
| Bridge | ~100% | ~100% | ~100% | 0% |

### Terrain Features

- **Trees**: Block movement and line of sight. Destructible by weapons fire.
- **Cliffs/Ridges**: Impassable. Create chokepoints and natural defenses.
- **Roads**: Maximum speed for all ground units.
- **Bridges**: Allow land units to cross water. Destructible (Engineers can repair them).
- **Ore fields**: Yellow patches. Harvested for credits. Regenerate over time.
- **Gem fields**: Blue/purple crystals. Worth 2× ore. Do not regenerate.

---

## Fog of War & Shroud

Two-layer visibility system:

1. **Shroud (Black)**: Unexplored areas. Completely hidden — no terrain, structures, or units visible. Must be revealed by moving units into the area, Spy Plane, or GPS Satellite.

2. **Fog of War (Dimmed)**: Previously explored areas no longer in any unit's sight range. Terrain and structures visible as last seen, but enemy units are hidden. Structures may have changed since last observed.

3. **Clear Vision**: Areas within your units' current sight radius. Full real-time visibility.

**Gap Generator**: Re-shrouds a wide area around itself. Enemies entering the gap zone only see their immediate surroundings.

**GPS Satellite**: Permanently reveals the entire map (removes all shroud). Does not counter Gap Generators.

---

## Combat Mechanics

### Damage Calculation

```
Effective Damage = Weapon Damage × Warhead Modifier[target armor type]
```

See the [Warhead Damage Modifiers](#warhead-damage-modifiers) table.

### Key Combat Rules

- **Auto-targeting**: Units auto-target the nearest enemy within range. No threat assessment.
- **No veterancy**: Units do not gain experience or improved stats from kills.
- **No stances**: No passive/aggressive/hold-position toggles. Units always engage enemies in range.
- **Crushing**: Tanks (tracked vehicles) can crush infantry by driving over them.
- **Self-healing**: Mammoth Tanks and Ore Trucks regenerate HP up to 50%.
- **Crewed vehicles**: When destroyed, the driver bails out as a Rifle Infantry unit (Ranger, Light Tank, Medium Tank, Heavy Tank, etc.).
- **Explodes on death**: Grenadiers and Flamethrowers explode when killed, damaging nearby units.

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
| Nuclear Missile | Both (MP) | Missile Silo | $2,500 | ~9 min | Massive area damage. Destroys most units, heavily damages structures. Leaves radiation that harms infantry. |

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

Wooden boxes that spawn randomly on skirmish/multiplayer maps. Contents are randomized:

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

Difficulty only changes **unit/structure stat multipliers** (HP, damage). It does **NOT** change AI behavior, build order, or attack patterns. The AI acts identically at all difficulties — only the numbers change.

### Cheating

- No income cheating (no bonus credits)
- No full map vision — must scout like the player
- Campaign missions receive off-map reinforcements that are effectively "free"
- Advantage comes from perfect micro (instant reaction) and no UI overhead

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

## Multiplayer & Skirmish

### Skirmish Mode

- 1 human vs 1–5 AI opponents
- Configurable: faction/country, AI difficulty (Easy/Medium/Hard), starting credits, map, player colors
- Victory condition: **annihilation** — destroy all enemy structures and units

### Multiplayer

- Up to **8 players** (6 originally, 8 with patches/expansions)
- Connection types: LAN (IPX/SPX), Serial/Modem, Internet (Westwood Online; today CnCNet)
- Victory condition: annihilation only (no alternate win conditions in the original)

---

## References

### Primary Data Sources

- [OpenRA raclassic Rules.ini](https://github.com/OpenRA/raclassic/blob/master/ref/Rules.ini) — original game configuration file with all unit/building/weapon stats
- [PortableRA rules_.ini](https://github.com/mvdhout1992/PortableRA/blob/master/rules_.ini) — alternate rules.ini reference
- [Red Alert Remastered Mod Toolkit — default_rules.ini](https://github.com/DavidWalshe93/Red-Alert-Remastered-Mod-Toolkit/blob/master/default_rules.ini) — remastered edition data

### Unit & Building Stats

- [Unit Statistics — Red Alert](https://unitstatistics.com/red-alert/) — community stat database
- [CNCNZ — Allied Units](https://cncnz.com/games/red-alert/allied-units/)
- [CNCNZ — Soviet Units](https://cncnz.com/games/red-alert/soviet-units/)
- [CNCNZ — Allied Structures](https://cncnz.com/games/red-alert/allied-structures/)
- [CNCNZ — Soviet Structures](https://cncnz.com/games/red-alert/soviet-structures/)
- [CnCNet Forums — Tank Comparison Table](https://forums.cncnet.org/topic/2384-tank-comparison-table/)
- [Steam Community — Red Alert Tank Breakdown](https://steamcommunity.com/sharedfiles/filedetails/?id=2125164149)

### Wiki & General Reference

- [C&C Wiki — Red Alert 1 Allied Arsenal](https://cnc.fandom.com/wiki/Category:Red_Alert_1_Allied_arsenal)
- [C&C Wiki — Red Alert 1 Soviet Arsenal](https://cnc.fandom.com/wiki/Category:Red_Alert_1_Soviet_arsenal)
- [C&C Wiki — Red Alert 1 Allied Missions](https://cnc.fandom.com/wiki/Category:Red_Alert_1_Allied_missions)
- [C&C Wiki — Red Alert 1 Soviet Missions](https://cnc.fandom.com/wiki/Category:Red_Alert_1_Soviet_missions)
- [C&C Wiki — Superweapons (Red Alert)](https://cnc.fandom.com/wiki/Superweapon_(Red_Alert))
- [C&C Wiki — Chronosphere (Red Alert 1)](https://cnc.fandom.com/wiki/Chronosphere_(Red_Alert_1))
- [C&C Wiki — Iron Curtain (Red Alert 1)](https://cnc.fandom.com/wiki/Iron_Curtain_(Red_Alert_1))
- [C&C Wiki — Nuclear Missile Silo (Red Alert 1)](https://cnc.fandom.com/wiki/Nuclear_missile_silo_(Red_Alert_1))
- [C&C Wiki — Crate](https://cnc.fandom.com/wiki/Crate)
- [C&C Wiki — Fog of War](https://cnc.fandom.com/wiki/Fog_of_war)
- [C&C Wiki — Build Time](https://cnc.fandom.com/wiki/Build_time)
- [C&C Wiki — Construction Yard (Red Alert 1)](https://cnc.fandom.com/wiki/Construction_Yard_(Red_Alert_1))
- [C&C Wiki — Command & Conquer: Red Alert](https://cnc.fandom.com/wiki/Command_%26_Conquer:_Red_Alert)
- [Wikipedia — Command & Conquer: Red Alert](https://en.wikipedia.org/wiki/Command_%26_Conquer:_Red_Alert)
- [StrategyWiki — Command & Conquer: Red Alert](https://strategywiki.org/wiki/Command_&_Conquer:_Red_Alert)
- [ModEnc — Armor Types](https://modenc.renegadeprojects.com/Armor)

### Gameplay & Strategy Guides

- [GameFAQs — Red Alert Strategy Guide](https://gamefaqs.gamespot.com/pc/196962-command-and-conquer-red-alert/faqs/1699)
- [Cheatbook — Red Alert rules.ini Walkthrough](https://www.cheatbook.de/wfiles/ccredrin.htm)
- [Gamer Walkthroughs — Red Alert](https://gamerwalkthroughs.com/command-and-conquer-red-alert/)
- [KeenGamer — Allies Campaign Walkthrough](https://www.keengamer.com/articles/guides/command-and-conquer-remastered-allies-campaign-walkthrough-guide/)
- [KeenGamer — Soviet Campaign Walkthrough](https://www.keengamer.com/articles/guides/command-and-conquer-remastered-soviet-campaign-walkthrough-guide/)

### AI & Mechanics

- [Andrew Armstrong — Red Alert Gameplay and Singleplayer AI](https://aarmstrong.org/journal/2008/09/07/red-alert-gameplay-and-singleplayer-ai)
- [Steam Discussions — Skirmish AI Behavior](https://steamcommunity.com/app/1213210/discussions/0/2965021152080984393/)
- [Steam Discussions — AI Difficulty Only Changes Stats](https://steamcommunity.com/app/1213210/discussions/0/2641874742490185100/)
- [CnCNet Forums — Country Bonuses](https://forums.cncnet.org/topic/3845-countries-bonuses/)

### Controls & Multiplayer

- [CnCNet Hotkeys Reference](https://downloads.cncnet.org/Games/RedAlert/Hotkeys.txt)
- [Steam Community — Red Alert Hotkeys Discussion](https://steamcommunity.com/app/1213210/discussions/0/2290590708533538626/)
- [CnCNet Forums — Map Size](https://forums.cncnet.org/topic/4553-map-size/)
- [C&C Wiki — Skirmish](https://cnc.fandom.com/wiki/Skirmish)
- [CNCNZ — Red Alert Terrain Editor Tips](https://cncnz.com/features/technical-support-help-guides/red-alert-terrain-editor-basic-tricks-and-tips/)
