# Genshin Impact v1.0 — Gameplay Systems Spec

Genshin Impact, PC / PS4 / Mobile, September 28 2020. Open-world action RPG with gacha monetization. Version 1.0 launch content only (Mondstadt and Liyue regions).

---

## 1. Core Combat Systems

### 1.1 Damage Formula

The full outgoing damage formula (community reverse-engineered, verified by KQM Theorycrafting Library):

```
DMG = BaseDMG * DMGBonusMult * CRITMult * EnemyDEFMult * EnemyRESMult * AmplifyingReactionMult
```

**Base DMG:**
```
BaseDMG = Talent% * ScalingStat
```
Where ScalingStat is typically ATK = (BaseATK_Character + BaseATK_Weapon) * (1 + ATK_Bonus%) + FlatATK. Most v1.0 talents scale on ATK; a few scale on DEF, Max HP, or EM.

**DMG Bonus Multiplier:**
```
DMGBonusMult = 1 + DMGBonus% - DMGReduction%
```
All damage bonus sources (Elemental/Physical DMG% goblet, weapon passives, set bonuses, character passives) are additive within this term.

**CRIT Multiplier:**
- On critical hit: `1 + CritDMG%`
- On non-crit: `1`
- Average: `1 + clamp(CritRate%, 0, 1) * CritDMG%`
- Base stats for all characters: 5% CRIT Rate, 50% CRIT DMG.

**Enemy DEF Multiplier:**
```
EnemyDEFMult = (LvlChar + 100) / ((LvlChar + 100) + (LvlEnemy + 100) * (1 - DEFReduction%) * (1 - DEFIgnore%))
```
At equal levels this is always 0.5 (50%). Lv80 character vs Lv90 enemy = 180/370 = 0.4865. DEF Reduction is hard-capped at 90%. v1.0 DEF reduction sources: Lisa A4 passive (-15%), Razor C4 (-15%).

**Enemy RES Multiplier (piecewise):**
```
If RES < 0:        ResMult = 1 - RES/2
If 0 ≤ RES < 0.75: ResMult = 1 - RES
If RES ≥ 0.75:     ResMult = 1 / (4*RES + 1)
```
No hard cap on resistance reduction.

### 1.2 Elemental System

Seven elements exist in Teyvat: **Pyro, Hydro, Electro, Cryo, Dendro, Anemo, Geo**. Dendro had no playable characters and minimal reactions in v1.0 (only Burning from Dendro Samachurl/Slime + Pyro). Anemo and Geo cannot be applied as auras on enemies.

### 1.3 Elemental Reactions

#### Amplifying Reactions

Multiply the triggering hit's total damage. Benefit from ATK, CRIT, DMG%, and all other damage stats.

| Reaction | Trigger | Base Multiplier |
|----------|---------|----------------|
| Forward Vaporize | Hydro on Pyro aura | 2.0x |
| Reverse Vaporize | Pyro on Hydro aura | 1.5x |
| Forward Melt | Pyro on Cryo aura | 2.0x |
| Reverse Melt | Cryo on Pyro aura | 1.5x |

```
AmplifyingMult = ReactionMultiplier * (1 + 2.78 * EM / (1400 + EM) + ReactionBonus%)
```

#### Transformative Reactions

Create separate, fixed damage instances. Scale only with character level and Elemental Mastery — no ATK, no CRIT, no DMG%. Cannot critically strike in v1.0.

| Reaction | Elements | Multiplier | Damage Type | Special Effect |
|----------|----------|-----------|-------------|----------------|
| Overloaded | Pyro + Electro | 2.75 | Pyro | AoE (5m radius), knocks back small/medium enemies |
| Superconduct | Cryo + Electro | 1.5 | Cryo | AoE (5m radius), reduces Physical RES by 40% for 12s |
| Electro-Charged | Electro + Hydro | 2.0 per tick | Electro | Ticks ~1/sec, both auras coexist, can chain to nearby wet enemies |
| Shatter | Blunt on Frozen | 3.0 | Physical | Triggered by Claymore, Plunge, Geo, explosions on Frozen targets |
| Swirl | Anemo + Pyro/Hydro/Electro/Cryo | 0.6 | Swirled element | AoE damage of swirled element to nearby enemies |
| Burning | Pyro + Dendro | 0.25 | Pyro | Continuous DoT, minimal in v1.0 |

```
TransformativeDMG = ReactionMult * LevelMult * (1 + 16 * EM / (2000 + EM) + ReactionBonus%) * EnemyResMult
```

**Level Multiplier (selected values):**

| Character Level | Multiplier |
|----------------|-----------|
| 1 | 17.17 |
| 40 | 97.24 |
| 60 | 323.60 |
| 70 | 492.88 |
| 80 | 765.64 |
| 90 | 1,446.85 |

#### Non-Damage Reactions

| Reaction | Elements | Effect |
|----------|----------|--------|
| Frozen | Hydro + Cryo | Immobilizes enemy; duration based on gauge. Vulnerable to Shatter from Blunt attacks. |
| Crystallize | Geo + Pyro/Hydro/Electro/Cryo | Creates elemental shield shard on ground (15s duration). Shield has 250% absorption bonus vs its own element. Shield HP = LevelMult * (1 + (40/9) * EM / (1400 + EM)). |

#### Electro-Charged Special Behavior

Both Hydro and Electro auras coexist on the target. Each tick consumes 0.4U from both gauges. Ticks continue until one aura depletes. Because both auras persist, applying a third element (e.g., Pyro) can trigger two reactions simultaneously (Overloaded + Vaporize).

### 1.4 Elemental Gauge Theory

When an ability applies an element, it applies a specific number of Gauge Units (U):
- **1U** (Weak): Most abilities
- **2U** (Strong): Certain skills/bursts
- **4U** (Very Strong): Rare

**Aura Tax:** First application as an aura is reduced by 20% (1U → 0.8U aura, 2U → 1.6U aura).

**Decay Duration:**

| Source Gauge | Aura After Tax | Duration |
|-------------|---------------|----------|
| 1U | 0.8U | 9.5s |
| 2U | 1.6U | 12.0s |
| 4U | 3.2U | 17.0s |

**Gauge Consumption by Reaction:**

| Reaction Type | Gauge Consumed (multiplier on trigger gauge) |
|--------------|----------------------------------------------|
| Forward Vaporize / Forward Melt | 2.0x (clears aura quickly) |
| Reverse Vaporize / Reverse Melt | 0.5x (preserves aura, allows multiple reactions) |
| Overloaded / Superconduct | 1.0x |
| Swirl / Crystallize | 0.5x |
| Electro-Charged | 0.4U per tick from both gauges |

### 1.5 Internal Cooldown (ICD)

**Standard ICD:** 2.5 seconds OR 3 hits, whichever comes first. Only the 1st hit in a sequence applies an element; next application occurs at the 4th hit or after 2.5s.

The 3-hit threshold does NOT reset the 2.5s timer. The 2.5s timer DOES reset the hit counter.

**ICD Groups:**
- Normal, Charged, and Plunging Attacks are generally separate ICD groups.
- **Exception:** Sword and Claymore characters share ICD between Normal and Charged Attacks.
- Elemental Skill and Elemental Burst each have their own separate ICD.
- ICD is tracked per-enemy and per-character independently.

### 1.6 Attack Types

**Normal Attack Chains:** Weapon-dependent hit counts (4–6 hits per combo). Each hit has its own talent multiplier (% of ATK). Combo resets after completing the chain or after a delay.

**Charged Attacks:** Stamina costs per weapon type are in §1.8.

| Weapon Type | Behavior |
|------------|----------|
| Sword | Rapid forward slashes |
| Claymore | Spinning drain + final slash |
| Polearm | Forward thrust |
| Catalyst | Enhanced elemental projectile |
| Bow | Aimed Shot — Lv1 charge = Physical, Lv2 (full) = Elemental |

**Plunging Attacks:** Press Normal Attack while airborne. Low Plunge (≤2.4m height) deals less than High Plunge (>2.4m). Claymore/Geo/Explosion plunges count as Blunt (trigger Shatter). No stamina cost.

**Elemental Skill:** Character-specific ability. Generates elemental particles on hit. Cooldowns range 1–30s. Some have tap/hold variants.

**Elemental Burst:** Ultimate ability requiring full energy. Cooldowns 12–20s. Energy costs: 40, 60, or 80 (v1.0).

### 1.7 Energy System

**Energy from Particles/Orbs (on-field character, 100% ER):**

| Source | Same Element | Different Element | Neutral |
|--------|-------------|-------------------|---------|
| Elemental Particle | 3.0 | 1.0 | 2.0 |
| Elemental Orb | 9.0 | 3.0 | 6.0 |

**Off-field penalty (multiplicative):**

| Party Size | Off-Field Multiplier |
|-----------|---------------------|
| 4 members | 0.6x |
| 3 members | 0.7x |
| 2 members | 0.8x |

Energy Recharge stat multiplies all particle/orb energy: `ActualEnergy = BaseEnergy * ER%`. Base ER is 100%, no soft cap.

**Burst Energy Costs (v1.0):** 40 (Amber, Kaeya), 60 (Diluc, Fischl, Barbara, Bennett, Xiangling, most characters), 80 (Beidou, Xingqiu, Noelle).

### 1.8 Stamina (Combat)

See §4.1 for the full stamina system (base/max values, regeneration, exploration costs). Combat-specific stamina costs:

| Combat Action | Stamina Cost |
|--------------|-------------|
| Dash/Dodge (tap) | 18 flat |
| Sprint (hold) | 18/sec |
| Sword Charged Attack | 20 |
| Polearm Charged Attack | 25 |
| Catalyst Charged Attack | 50 |
| Claymore Charged Attack | 40/sec (drain while spinning) |
| Bow Aimed Shot | 0 |

### 1.9 I-Frames

**Dash I-frames:** Start ~40ms after pressing dash. Duration ~300ms (18 frames at 60 FPS). Character hitbox disappears entirely.

**Elemental Burst I-frames:** Granted on cast. HP-locked (character takes no damage but hitbox remains). Duration varies per character. Some characters have extended idle i-frames that persist until the next player input.

### 1.10 Poise & Stagger

All units have hidden Poise. When Poise reaches 0, the unit enters a Vulnerable state and can be staggered. Attacks have an Impulse Level (0–9) determining knockback force. Larger enemies have higher Poise and lower Vulnerability.

"Resistance to Interruption" abilities (Beidou Tidecaller, Noelle Breastplate) reduce Vulnerability multiplier — not a binary immunity.

**Reaction Poise Damage:**
- Overloaded: High poise + knockback (Impulse Level 5–6)
- Shatter: High poise damage
- Superconduct: Moderate
- Swirl: Low

### 1.11 Enemy Resistances (Base Values)

| Enemy | Phys | Pyro | Hydro | Electro | Cryo | Anemo | Geo |
|-------|------|------|-------|---------|------|-------|-----|
| Hilichurls | 10% | 10% | 10% | 10% | 10% | 10% | 10% |
| Mitachurls | 30% | 10% | 10% | 10% | 10% | 10% | 10% |
| Stonehide Lawachurl | 50% | 10% | 10% | 10% | 10% | 10% | 70% |
| Frostarm Lawachurl | 50% | 10% | 10% | 10% | 70% | 10% | 10% |
| Slimes (own element) | 10% | Immune | Immune | Immune | Immune | Immune | Immune |
| Whopperflowers | 35% | 35% | 35% | 35% | 35% | 35% | 35% |
| Abyss Mages | 10% | 10% | 10% | 10% | 10% | 10% | 10% |
| Fatui Skirmishers | -20% | 10% | 10% | 10% | 10% | 10% | 10% |
| Fatui Pyro Agent | -20% | 50% | 10% | 10% | 10% | 10% | 10% |
| Treasure Hoarders | -20% | 10% | 10% | 10% | 10% | 10% | 10% |
| Ruin Guard | 70% | 10% | 10% | 10% | 10% | 10% | 10% |
| Ruin Hunter | 50% | 10% | 10% | 10% | 10% | 10% | 10% |

**v1.0 Resistance Shred Sources:**

| Source | Amount | Type |
|--------|--------|------|
| 4pc Viridescent Venerer | -40% | Swirled element |
| Superconduct | -40% | Physical (12s) |
| Geo Resonance (shielded) | -20% | Geo (15s) |

### 1.12 Elemental Resonance

When a party of 4 has at least 2 of the same element:

| Resonance | Requirement | Effect |
|-----------|------------|--------|
| Fervent Flames | 2x Pyro | ATK +25%. Affected by Cryo 40% less time. |
| Soothing Water | 2x Hydro | Incoming healing +30%. Affected by Pyro 40% less time. |
| High Voltage | 2x Electro | Superconduct/Overloaded/EC guaranteed Electro Particle (5s CD). Affected by Hydro 40% less time. |
| Shattering Ice | 2x Cryo | +15% CRIT Rate vs Frozen or Cryo-affected enemies. Affected by Electro 40% less time. |
| Impetuous Winds | 2x Anemo | Stamina consumption -15%. Movement SPD +10%. Skill CD -5%. |
| Enduring Rock | 2x Geo | Shield strength +15%. While shielded: +15% DMG, hitting enemies reduces Geo RES by 20% for 15s. |
| Protective Canopy | 4 unique elements | All Elemental RES +15%, Physical RES +15%. |

---

## 2. Controls & Input

### 2.1 Default Controls (PC — Keyboard + Mouse)

| Action | Key |
|--------|-----|
| Move | WASD |
| Sprint/Dodge | Left Shift (hold/tap) |
| Jump | Space |
| Normal Attack | Left Click |
| Charged Attack | Hold Left Click |
| Aimed Shot (Bow) | Hold Right Click (enters aim mode) |
| Elemental Skill | E |
| Elemental Burst | Q |
| Switch Character 1/2/3/4 | 1/2/3/4 |
| Open Map | M |
| Open Inventory/Backpack | B |
| Open Character Menu | C |
| Open Paimon Menu (Pause) | Esc |
| Open Wish Menu | F3 |
| Open Adventure Handbook | F1 |
| Open Events/Notices | F5 |
| Interact/Pick Up | F |
| Open Elemental Sight | Middle Mouse |
| Glide (while airborne) | Space |
| Plunge Attack (while gliding) | Left Click |
| Walk (toggle) | Left Ctrl |
| Camera Zoom | Scroll Wheel |
| Quick Gadget Use | Z |
| Quick Eat | NRE gadget (Z), otherwise through inventory |

### 2.2 Default Controls (PlayStation)

| Action | Button |
|--------|--------|
| Move | Left Stick |
| Sprint/Dodge | R1 (hold/tap) |
| Jump/Glide | X (Cross) |
| Normal Attack | ○ (Circle) |
| Elemental Skill | R2 |
| Elemental Burst | △ (Triangle) |
| Aimed Shot (Bow) | L2 (hold) |
| Switch Character | L1 + face button |
| Interact | □ (Square) |
| Open Map | Touchpad |
| Open Paimon Menu | Options |
| Elemental Sight | L1 |

### 2.3 Mobile Controls

Virtual joystick (left side) for movement. On-screen buttons for attack, skill, burst, jump, character swap. Tap-to-interact, drag to rotate camera. Auto-target is more aggressive on mobile due to imprecise aiming.

### 2.4 Context-Sensitive Inputs

- **Climbing:** Movement keys while against a climbable surface. Jump while climbing = climb leap. Space at ledge = vault up.
- **Swimming:** Same as ground movement. Sprint = fast swim (higher stamina drain).
- **Bow Aimed Mode:** Right Click enters first-person aim. Left Click fires. Scroll or hold fires charged shot.
- **Dialogue:** Left Click / F / Space to advance. Number keys or click to select dialogue options.

---

## 3. World Structure

### 3.1 Regions Available in v1.0

Two of Teyvat's seven nations were explorable at launch: **Mondstadt** (Anemo) and **Liyue** (Geo). Each region is subdivided into Areas, and each Area contains named Subareas.

**Not in v1.0:** Dragonspine (added v1.2), Inazuma (v2.0), Sumeru (v3.0), Fontaine (v4.0), Natlan (v5.0). The Chasm (v2.6). Serenitea Pot housing realm (v1.5). Reputation system (v1.1).

### 3.2 Mondstadt

The nation of freedom, ruled by the Anemo Archon Barbatos (Venti). Four Areas in v1.0 (Dragonspine, the fifth, was added in v1.2).

#### Starfell Valley (8 Subareas)

The starting region. Contains the City of Mondstadt on an island in Cider Lake.

| Subarea | Notes |
|---------|-------|
| **City of Mondstadt** | Main city. Knights of Favonius HQ, Cathedral, Angel's Share tavern, Good Hunter restaurant. Alchemy table, blacksmith, Adventurers' Guild (Katheryne). |
| **Cider Lake** | Lake surrounding Mondstadt. Dornman Port on north shore. |
| **Starfell Lake** | Small lake south of the city. Early quest location. |
| **Whispering Woods** | Forest south of Starfell Lake. Tutorial/starting area. |
| **Windrise** | Open field with the great tree (symbol of Mondstadt's freedom). Statue of the Seven location. |
| **Thousand Winds Temple** | Ruin complex east of Starfell Lake. Cryo Regisvine nearby. |
| **Stormbearer Mountains** | Northern mountains. Anemo Hypostasis boss location. Valberry farming. |
| **Stormbearer Point** | Northeastern coast. Traveler's arrival point (opening cutscene). Hidden island accessible via wind current. |
| **Starsnatch Cliff** | Coastal cliff northwest of Mondstadt. Cecilia flower farming. |

#### Galesong Hill (5 Subareas)

Southern Mondstadt. Contains the Spiral Abyss entrance.

| Subarea | Notes |
|---------|-------|
| **Windrise** | See Starfell Valley (borders both areas). |
| **Cape Oath** | Southern cape. Portal to Musk Reef (Spiral Abyss). Electro Hypostasis boss location. |
| **Dadaupa Gorge** | Valley with three Hilichurl tribes (Meaty, Sleeper, Eclipse). Contains Sword Cemetery and a sealed arena. |
| **Falcon Coast** | Eastern coast between Windrise and Cape Oath. |
| **Musk Reef** | Offshore island accessed via portal at Cape Oath. Spiral Abyss entrance. |

#### Windwail Highland (4 Subareas)

Central-south Mondstadt. Wine country and wolf territory.

| Subarea | Notes |
|---------|-------|
| **Dawn Winery** | Diluc's estate. Major NPC hub. Grape harvesting. |
| **Springvale** | Small village. Hunters and spring water. |
| **Wolvendom** | Dense forest ruled by Andrius (Wolf of the North). Weekly boss location. High enemy density. |
| **Drunkard Gorge** | Gorge area between Dawn Winery and Wolvendom. |

#### Brightcrown Mountains (2 Subareas)

Northwestern Mondstadt. Contains Old Mondstadt ruins.

| Subarea | Notes |
|---------|-------|
| **Stormterror's Lair** | Massive ruined city (Decarabian's Old Mondstadt). Wind barrier (removed during Archon Quest Prologue Act III). Weekly boss domain: Confront Stormterror. Multiple puzzle-locked chests and a unique upward wind current system. |
| **Brightcrown Canyon** | Canyon leading to Stormterror's Lair from the east. |

### 3.3 Liyue

The nation of contracts, ruled by the Geo Archon Morax (Zhongli). Five Areas in v1.0 (The Chasm was added later in v2.6).

#### Sea of Clouds (3 Subareas)

The harbor region. Contains the main city.

| Subarea | Notes |
|---------|-------|
| **Liyue Harbor** | Main city and largest port in Teyvat. Yuehai Pavilion (Qixing HQ), Northland Bank (Fatui), Wangsheng Funeral Parlor, Bubu Pharmacy. Alchemy table, blacksmith, Adventurers' Guild (Katheryne). |
| **Mt. Tianheng** | Mountain overlooking Liyue Harbor. Mining deposits. Mingyun Village boundary. |
| **Guyun Stone Forest** | Archipelago of seven large islands offshore. Ancient battlefield where Rex Lapis defeated sea monsters. Domain of Guyun (artifact domain). Geo Hypostasis boss location. |

#### Bishui Plain (6 Subareas)

Northern Liyue. Wetlands, marshes, and farmland.

| Subarea | Notes |
|---------|-------|
| **Dihua Marsh** | Wetland area inspired by Guilin/Yangshuo landscape. Stone peaks and dense waterways. |
| **Wangshu Inn** | Tall inn structure built above Dihua Marsh. Xiao's watch post. Teleport Waypoint on top level. |
| **Stone Gate** | Narrow pass connecting Mondstadt and Liyue. Trade route checkpoint. |
| **Qingce Village** | Elderly village in northern Liyue. Terraced farmland. Multiple world quests. |
| **Wuwang Hill** | Haunted hilltop area. Dense fog, Will-o'-the-Wisps. Oceanid (Rhodeia of Loch) boss location nearby. Hidden Palace of Zhou Formula (artifact domain). |
| **Sal Terrae** | Underground ruins accessible via a sealed door (requires quest). Ancient Archon-era site. |

#### Minlin (8 Subareas)

Mountainous western Liyue. Abode of the Adepti. Highest-elevation overworld area in v1.0.

| Subarea | Notes |
|---------|-------|
| **Jueyun Karst** | Sacred mountains where Adepti reside. Dense vertical terrain. |
| **Qingyun Peak** | Home of Moon Carver. Three Divine Birds puzzle (rotatable statues across Mt. Hulao, Mt. Aocang, and Qingyun Peak unlock Dwelling in the Clouds floating island). |
| **Mt. Aocang** | Cloud Retainer's abode. Elevated plateaus. |
| **Mt. Hulao** | Mountain Shaper's territory. Amber Rocks trap wildlife, enemies, and chests inside amber-colored Geo constructs. |
| **Huaguang Stone Forest** | Central Minlin plateau surrounded by the three Adepti mountains. |
| **Nantianmen** | Southern pass. Gateway to the abyss (deep canyon). Waypoint near the bottom. |
| **Cuijue Slope** | Central depression with the Nine Pillars of Peace (9 Stone of Remembrance puzzle, see Statue of Seven rewards). Pyro Regisvine boss location. |
| **Tianqiu Valley** | Valley with Tianqiu Treasure Trail puzzle domain. Completing the trial inside locks co-op until finished. |

#### Qiongji Estuary (4 Subareas)

Southeastern Liyue. Coastal and mining areas.

| Subarea | Notes |
|---------|-------|
| **Guili Plains** | Named after the ancient alliance of Guizhong and Morax (Liyue's pre-Archon War civilization). Ruins scattered throughout. |
| **Mingyun Village** | Abandoned mining village. Mining deposits (many dried up). |
| **Yaoguang Shoal** | Coastal shoal south of Mingyun Village. Fog and shallow water. |
| **Luhua Pool** | Twin waterfalls and pool area. World quest involving two statues. |

#### Lisha (3 Subareas)

Southwestern Liyue. Ancient ruins submerged until the end of the Archon War.

| Subarea | Notes |
|---------|-------|
| **Dunyu Ruins** | Ancient ruins. Nameless Treasure quest location (1 of 3). |
| **Qingxu Pool** | Ruins said to have been left by an evil god; previously underwater. Five Geo seals puzzle unlocks a Luxurious Chest containing a Nameless Treasure. |
| **Lingju Pass** | Ruined pass area. Nameless Treasure quest location (3 of 3). |

### 3.4 Teleport Waypoints

Fast-travel nodes scattered across the map. Activate by approaching and interacting. Free to use once unlocked. 61 Teleport Waypoints total in v1.0 (22 in Mondstadt, 39 in Liyue).

Each Statue of the Seven also functions as a Teleport Waypoint. Domain entrances serve as Teleport Waypoints once discovered.

### 3.5 Statues of the Seven

Statues found throughout each region. Serve as healing points (auto-heal party when nearby, draws from limited Statue HP pool that regenerates over time), Teleport Waypoints, and Oculi offering sites.

| Region | Statue Element | Statue Count | Oculi Type | Total Oculi | Oculi to Max (Lv10) | Extra Oculi |
|--------|---------------|-------------|------------|-------------|---------------------|-------------|
| Mondstadt | Anemo | 4 | Anemoculus | 66 | 65 | 1 spare |
| Liyue | Geo | 5 | Geoculus | 131 | 130 | 1 spare |

#### Mondstadt Statue of the Seven Rewards (Anemo)

| Level | Anemoculi Required | Stamina Increase | Primogems | Anemo Sigils | Adventure EXP |
|-------|-------------------|-----------------|-----------|-------------|---------------|
| 2 | 2 | +7 | 10 | 5 | 80 |
| 3 | 4 | +7 | 10 | 5 | 120 |
| 4 | 6 | +8 | 10 | 5 | 160 |
| 5 | 6 | +8 | 10 | 10 | 200 |
| 6 | 7 | +8 | 10 | 10 | 240 |
| 7 | 8 | +8 | 10 | 10 | 280 |
| 8 | 10 | +8 | 10 | 15 | 320 |
| 9 | 12 | +8 | 10 | 15 | 360 |
| 10 | 15 | +8 | 10 | 15 | 400 |
| **Total** | **65** | **+70** | **90** | **90** | **2,160** |

#### Liyue Statue of the Seven Rewards (Geo)

Follows same structure. 130 Geoculi to reach Level 10. Total rewards: 90 Geo Sigils, 90 Primogems, 2,160 Adventure EXP, +70 Max Stamina, 9 Stones of Remembrance (used for the Nine Pillars of Peace quest in Cuijue Slope).

**Combined Stamina from both Statues:** +140 total (70 Mondstadt + 70 Liyue), raising base 100 to 240 max.

---

## 4. Exploration Mechanics

### 4.1 Stamina System

Stamina is a shared resource across the active party (not per-character). Governs climbing, swimming, gliding, sprinting, and charged attacks.

| Parameter | Value |
|-----------|-------|
| **Base Stamina** | 100 |
| **Maximum Stamina** | 240 (after maxing both Mondstadt and Liyue Statues of the Seven) |
| **Regeneration Rate** | 25 per second (after 1.5 seconds of not consuming stamina) |
| **Full Regen Time (240)** | 9.6 seconds |

#### Stamina Consumption by Activity

| Activity | Cost | Notes |
|----------|------|-------|
| **Sprinting** | 18/sec | Dash (tap) costs a flat amount per dodge. Continuous sprint costs 18/sec. |
| **Climbing (moving)** | ~5/sec (vertical), varies with slope | Stationary on wall: minimal drain. Moving upward: moderate drain. |
| **Climb Jump** | ~25-30 per jump | Faster vertical progress but higher stamina per meter. Useful for short climbs. |
| **Swimming** | ~5.6/sec (forward), less when idle | Running out of stamina in water causes drowning (instant death, respawn at last safe position). |
| **Gliding** | ~3/sec (standard glide) | Plunging attack from glide cancels stamina drain. Wind currents provide free lift. |
| **Charged Attack (Melee)** | 20-40 per attack | Varies by weapon type. Claymore: 40. Sword: 20. Polearm: 25. |
| **Charged Attack (Catalyst)** | Continuous drain | Holds and channels; cost varies by character. |
| **Charged Attack (Bow)** | N/A | Aimed Shot does not consume stamina. |

#### Stamina-Reducing Effects

| Source | Effect |
|--------|--------|
| **Stamina-reduction foods** | -15% to -25% stamina consumption for specific activities (climbing, sprinting, swimming, gliding). Duration: 900s. Only one stamina food active at a time. |
| **Character Passives** | Amber: -20% gliding stamina for party. Kaeya: -20% sprinting stamina for party. Razor: -20% sprinting stamina for party. Venti: -20% gliding stamina for party. Beidou: -20% swimming stamina for party. |
| **Anemo Resonance** | Decreases Stamina Consumption by 15%, increases Movement SPD by 10%. |

### 4.2 Climbing

All characters can climb any vertical surface (walls, cliffs, buildings, trees). Climbing begins automatically when walking into a climbable surface or manually by pressing the jump button while against a surface.

| Mechanic | Description |
|----------|-------------|
| **Directional climbing** | Move in any direction on the surface. Upward movement costs the most stamina. |
| **Climb jump** | Press jump while climbing to leap upward. Covers more vertical distance faster but costs ~25-30 stamina per jump vs steady climbing. |
| **Ledge grab** | Characters automatically grab ledges at the top of surfaces. If stamina runs out mid-climb, the character falls. |
| **Rain/wet surfaces** | Climbing is not affected by rain in v1.0 (no wet surface mechanic on standard terrain). |
| **Unclimbable surfaces** | Some surfaces (ice walls in certain domains, specific boss arena boundaries) cannot be climbed. |

### 4.3 Swimming

Surface swimming only in v1.0 (no diving/underwater mechanics until v4.0+). Characters enter swimming automatically when entering water deeper than wading depth.

| Mechanic | Description |
|----------|-------------|
| **Forward swim** | Continuous stamina drain (~5.6/sec). |
| **Sprint swim** | Faster swimming with higher stamina drain. |
| **Idle float** | Minimal stamina drain when not moving. |
| **Stamina depletion** | Character drowns instantly. Respawns at last safe ground position. HP is not lost; it is treated as a teleport. |
| **Cryo on water** | Cryo characters (e.g., Kaeya) can freeze water surfaces to create walkable ice bridges, bypassing swimming entirely. |

### 4.4 Gliding

Activated by pressing jump while airborne. Uses the Wind Glider (obtained during Archon Quest Prologue Act I — "Amber's gift").

| Parameter | Value |
|-----------|-------|
| **Glide speed** | ~7.5 m/s (horizontal, no wind) |
| **Stamina cost** | ~3/sec |
| **Max glide distance (240 stamina)** | ~600m horizontal (still air, level altitude) |
| **Wind currents** | Upward drafts (natural or Anemo-generated) provide free altitude. No stamina cost while riding updrafts. |
| **Plunge attack** | Press attack while gliding to cancel glide and dive-attack. Deals AoE damage on landing based on fall height. Ends stamina consumption. |

### 4.5 Sprinting

Two modes: tap-dodge (short dash with i-frames) and hold-sprint (continuous running).

| Parameter | Value |
|-----------|-------|
| **Sprint speed** | ~7.0 m/s (vs ~5.0 m/s walking) |
| **Stamina cost** | 18/sec continuous |
| **Dash i-frames** | Brief invincibility window on tap-dodge start (~0.5s). Costs a flat stamina amount per dash. |

Certain characters have alternate sprint mechanics:
- **Mona:** Submerges into water-like dash. Faster on flat ground, applies Hydro on exit, but cannot change direction as quickly. Cannot sprint in shallow water.

### 4.6 Oculi Collection

Anemoculi and Geoculi are floating collectible orbs found throughout their respective regions. They hover in the air, often requiring climbing, gliding, or puzzle-solving to reach. Each is a one-time pickup (does not respawn). They appear on the minimap when within ~50m proximity.

| Oculi Type | Region | Total Available | Needed to Max Statue | Spare |
|------------|--------|----------------|---------------------|-------|
| Anemoculus | Mondstadt | 66 | 65 | 1 |
| Geoculus | Liyue | 131 | 130 | 1 |

The spare Oculus in each region cannot be used but serves as confirmation that all Oculi have been collected (inventory shows 1 remaining).

**Anemoculus/Geoculus Resonance Stones** (craftable items that mark the nearest uncollected Oculus on the map) were NOT available in v1.0. These were added with the Reputation system in v1.1.

---

## 5. Adventure Rank & World Level

### 5.1 Adventure Rank (AR) Overview

Adventure Rank is the account-level progression system. Gaining Adventure EXP (AR EXP) raises AR, which unlocks game systems, content, and increases World Level.

**Maximum AR:** 60. After AR 60, excess AR EXP converts to Mora at a 1:10 ratio.

### 5.2 Adventure EXP Sources

| Source | AR EXP per Action | Notes |
|--------|-------------------|-------|
| **Daily Commissions** | 175-250 each (scales with WL) + 500 bonus from Katheryne | 4 per day. Total ~1,200-1,500/day at higher WL. |
| **Domains** | 100 per 20 Resin | All domain types. |
| **Ley Line Outcrops** | 100 per 20 Resin | Blossom of Revelation or Wealth. |
| **Normal Bosses** | 200 per 40 Resin | Hypostases, Regisvines, Oceanid. |
| **Weekly Bosses** | 300 per claim | Dvalin, Andrius. |
| **Archon/Story Quests** | 100-575 per quest step | Major story quests give the most. |
| **World Quests** | 10-500 each | Varies widely by quest. |
| **Chests** | 10-60 per chest | Common: 10-20, Exquisite: 20-30, Precious: 30, Luxurious: 40-60. |
| **Teleport Waypoints** | 50 each | One-time, per waypoint. |
| **Adventurer Handbook** | 100 per milestone | Chapter completion bonuses. |
| **Random Events** | 10-15 each | Overworld random encounter commissions. |

### 5.3 AR Unlocks

| AR | Unlock |
|----|--------|
| 5 | Wishes (Gacha) system, Adventurer Handbook |
| 8 | Ley Line Outcrops (Blossom of Revelation — Character EXP) |
| 10 | Archon Quest Prologue Act II |
| 12 | Daily Commissions, Ley Line Outcrops (Blossom of Wealth — Mora), Expedition dispatch |
| 14 | Expeditions enhanced (more slots), Domains of Forgery unlock requirement |
| 16 | Co-Op Mode, Domains of Forgery (weapon ascension material domains) |
| 18 | Archon Quest Prologue Act III |
| 20 | Spiral Abyss, Battle Pass. **World Level 1** (automatic). |
| 22 | Domains of Blessing (artifact domains) |
| 23 | Archon Quest Chapter I Act I |
| 25 | **World Level 2** (Ascension Quest required). Archon Quest Chapter I Act II. |
| 26 | Story Keys system (1 Key per 8 commissions completed) |
| 27 | Domains of Mastery (talent material domains) |
| 30 | **World Level 3** (automatic). |
| 32 | Trounce Domain difficulty increase |
| 35 | **World Level 4** (Ascension Quest required). Archon Quest Chapter I Act III (v1.1 content). |
| 36 | Trounce Domain difficulty increase |
| 40 | **World Level 5** (automatic). |
| 45 | **World Level 6** (Ascension Quest required). 5-star artifact drops from domain Blessing become available (not guaranteed). |
| 50 | **World Level 7** (Ascension Quest required). |
| 55 | **World Level 8** (automatic). 5-star artifact drops from weekly bosses guaranteed. |
| 60 | Maximum AR. Excess AR EXP converts to Mora (1:10). |

### 5.4 Ascension Quests

At AR 25, 35, 45, and 50, the player must complete an Ascension Quest (a special domain challenge) before AR can continue accumulating. AR EXP is stored but hidden until the quest is completed. This prevents unwanted World Level increases.

| AR Gate | Quest Name | Domain Challenge |
|---------|-----------|-----------------|
| AR 25 | Ascend: Clear the Ruins | Combat trial (elemental puzzle + enemies) |
| AR 35 | Ascend: Clear the Ruins (II) | Harder combat trial |
| AR 45 | Ascend: Clear the Ruins (III) | Harder combat trial |
| AR 50 | Ascend: Clear the Ruins (IV) | Harder combat trial |

### 5.5 World Level System

World Level (WL) controls overworld enemy levels, boss levels, and loot quality. Higher WL means harder enemies but better rewards.

| World Level | AR Range | Enemy Level Range | Notes |
|-------------|----------|------------------|-------|
| 0 | 1-19 | 1-36 | Starting WL. |
| 1 | 20-24 | 9-39 | Automatic at AR 20. |
| 2 | 25-29 | 20-40 | Ascension Quest at AR 25. |
| 3 | 30-34 | 31-51 | Automatic at AR 30. |
| 4 | 35-39 | 42-62 | Ascension Quest at AR 35. |
| 5 | 40-44 | 53-73 | Automatic at AR 40. |
| 6 | 45-49 | 64-84 | Ascension Quest at AR 45. |
| 7 | 50-54 | 72-92 | Ascension Quest at AR 50. |
| 8 | 55-60 | 76-98 | Automatic at AR 55. Highest WL in v1.0. |

**WL 9** (AR 58, enemy levels 86-103) was added in v5.0, far after v1.0.

#### World Level Effects on Drops

| WL | Normal Boss Artifact Drops | Character Ascension Gem Drops | Weekly Boss Artifact Quality |
|----|---------------------------|------------------------------|------------------------------|
| 0-2 | 3-star only | Slivers | 3-star, rare 4-star |
| 3-4 | 3-star, chance of 4-star | Slivers + Fragments | 4-star guaranteed |
| 5-6 | 4-star guaranteed, chance of 5-star | Fragments + Chunks | 4-star guaranteed, chance of 5-star |
| 7-8 | 4-star guaranteed, higher 5-star chance | Chunks + rare Gemstone | 5-star guaranteed at WL 8 |

**WL can be lowered by 1 from the current level** starting at WL 5 (AR 40+), via the in-game menu. This was added post-v1.0 (v1.4). In v1.0, WL could not be manually adjusted.

---

## 6. Domains

### 6.1 Domain Types

| Domain Type | Reward Category | Resin Cost | Rotation | Unlock AR |
|-------------|----------------|------------|----------|-----------|
| **Domains of Forgery** | Weapon Ascension Materials | 20 | Mon/Thu, Tue/Fri, Wed/Sat, Sun=all | AR 16 |
| **Domains of Mastery** | Character Talent Level-Up Materials | 20 | Mon/Thu, Tue/Fri, Wed/Sat, Sun=all | AR 27 |
| **Domains of Blessing** | Artifact Sets | 20 | No rotation (always available) | AR 22 |
| **Trounce Domains** | Weekly Boss Materials (Talent Lv7+ mats, Billets) | 60 | Once per week | Quest-locked |
| **One-Time Domains** | Primogems, Adventure EXP, Mora | Free | One-time clear | Varies |

### 6.2 Domains of Forgery (Weapon Ascension Materials) - v1.0

#### Cecilia Garden (Mondstadt)

Location: Wolvendom, Windwail Highland.

| Day | Material Series | Weapon Examples |
|-----|----------------|-----------------|
| **Mon / Thu** | Tiles of Decarabian | Favonius weapons, The Bell, Prototype Rancour |
| **Tue / Fri** | Boreal Wolf Teeth | Wolf's Gravestone, Sacrificial weapons |
| **Wed / Sat** | Chains of the Dandelion Gladiator | Lion's Roar, The Flute, Rainslasher |
| **Sunday** | All three available | Player's choice |

Each material series has 4 tiers (green > blue > purple > gold), with higher domain difficulty levels dropping higher tiers.

#### Hidden Palace of Lianshan Formula (Liyue)

Location: Near Qingce Village, Bishui Plain.

| Day | Material Series | Weapon Examples |
|-----|----------------|-----------------|
| **Mon / Thu** | Guyun Pillars | Primordial Jade Winged-Spear, Crescent Pike |
| **Tue / Fri** | Mist Veiled Elixirs | Lost Prayer to the Sacred Winds, Lithic weapons |
| **Wed / Sat** | Aerosiderite | Prototype Archaic, Whiteblind |
| **Sunday** | All three available | Player's choice |

### 6.3 Domains of Mastery (Talent Level-Up Materials) - v1.0

#### Forsaken Rift (Mondstadt)

Location: Brightcrown Mountains, near Stormterror's Lair.

| Day | Talent Book Series | Characters (v1.0 launch) |
|-----|-------------------|-------------------------|
| **Mon / Thu** | Freedom | Amber, Barbara, Diona, Klee, Sucrose, Tartaglia (v1.1), Traveler (Anemo) |
| **Tue / Fri** | Resistance | Bennett, Diluc, Jean, Mona, Noelle, Razor |
| **Wed / Sat** | Ballad | Fischl, Kaeya, Lisa, Venti |
| **Sunday** | All three available | Player's choice |

Each talent book series has 3 tiers: Teachings (green, Lv2-4), Guide (blue, Lv5-6), Philosophies (purple, Lv7+).

#### Taishan Mansion (Liyue)

Location: Jueyun Karst, Minlin.

| Day | Talent Book Series | Characters (v1.0 launch) |
|-----|-------------------|-------------------------|
| **Mon / Thu** | Prosperity | Keqing, Ningguang, Qiqi, Traveler (Geo), Xiao (v1.3) |
| **Tue / Fri** | Diligence | Chongyun, Ganyu (v1.2), Xiangling, Xingqiu |
| **Wed / Sat** | Gold | Beidou, Xiao (v1.3) |
| **Sunday** | All three available | Player's choice |

### 6.4 Domains of Blessing (Artifact Domains) - v1.0

No daily rotation. Available every day. Different difficulty tiers unlock at different AR levels. Higher tiers drop higher-rarity artifacts and cost the same 20 Resin.

#### Mondstadt Artifact Domains

| Domain | Location | Artifact Sets |
|--------|----------|--------------|
| **Midsummer Courtyard** | Starfell Valley | Thundering Fury (4/5-star), Thundersoother (4/5-star) |
| **Valley of Remembrance** | Windwail Highland | Viridescent Venerer (4/5-star), Maiden Beloved (4/5-star) |

#### Liyue Artifact Domains

| Domain | Location | Artifact Sets |
|--------|----------|--------------|
| **Clear Pool and Mountain Cavern** | Minlin (Mt. Aocang) | Noblesse Oblige (4/5-star), Bloodstained Chivalry (4/5-star) |
| **Domain of Guyun** | Guyun Stone Forest | Archaic Petra (4/5-star), Retracing Bolide (4/5-star) |
| **Hidden Palace of Zhou Formula** | Wuwang Hill | Crimson Witch of Flames (4/5-star), Lavawalker (4/5-star) |

### 6.5 One-Time Domains

Story/exploration domains that reward Primogems, Adventure EXP, and Mora on first completion. No Resin cost. Cannot be repeated.

| Domain | Location | Notes |
|--------|----------|-------|
| Temple of the Lion | Mondstadt | Tutorial domain |
| Temple of the Wolf | Mondstadt | Archon Quest domain |
| Temple of the Falcon | Mondstadt | Archon Quest domain |
| Eagle's Gate | Stormterror's Lair | Puzzle domain |
| Tianqiu Treasure Trail | Tianqiu Valley | 3-floor trial domain. **Warning:** entering locks co-op until all 3 trials completed. |
| Lingju Pass Domain | Lingju Pass | Exploration domain |

---

## 7. Treasure & Chest System

### 7.1 Chest Types

| Chest Type | Primogems (Mondstadt/Liyue base) | Adventure EXP | Sigils | Other Rewards |
|------------|----------------------------------|---------------|--------|---------------|
| **Common** | 0-2 | 10-20 | 1-2 | Mora, 1-2 star weapons/artifacts, enhancement materials |
| **Exquisite** | 2-5 | 20-30 | 3-4 | Mora, 2-3 star weapons/artifacts, enhancement materials |
| **Precious** | 5-10 | 30 | 4-10 | Mora, 3-star weapons/artifacts, character EXP materials |
| **Luxurious** | 10-40 | 30-60 | 4-10 | Mora, 3-4 star artifacts, character EXP materials, Adventure EXP materials |
| **Shrine of Depths** | 40 | — | — | 1 premium artifact, Primogems. Requires Shrine of Depths Key (obtained from quests, domains, AR rewards). |

**Remarkable Chests** (contain furnishing blueprints) were added with the Serenitea Pot in v1.5; NOT in v1.0.

Mondstadt and Liyue chests give the minimum Primogem values in the ranges above. Later regions give higher values.

### 7.2 Chest Sources

Chests are found via:
- **Static placement** — sitting in the open or slightly hidden behind terrain.
- **Puzzle completion** — Seelie (guide to pedestal), pressure plates, torch lighting, elemental monuments, breakable rocks, dig spots.
- **Enemy camp clearing** — defeating all enemies in a camp spawns a chest.
- **Challenge markers** — time trials, combat challenges, gliding challenges (indicated by floating sigils on the ground).
- **Quest rewards** — some World Quests spawn chests on completion.

### 7.3 Chest Respawn

**Chests do NOT respawn in Genshin Impact.** Early community speculation suggested Common/Exquisite chests respawned on timers, but this was debunked. Once opened, a chest is permanently gone. New chests may be added in version updates, but existing chests do not regenerate.

### 7.4 Shrine of Depths

Locked ornate chests requiring a Shrine of Depths Key to open. Keys are region-specific (Mondstadt keys only open Mondstadt shrines).

| Region | Number of Shrines | Key Sources |
|--------|------------------|-------------|
| Mondstadt | 10 | AR level-up rewards, Archon/Story Quest rewards, One-Time Domain rewards |
| Liyue | 10 | AR level-up rewards, Archon/Story Quest rewards, One-Time Domain rewards |

Each Shrine contains a guaranteed 4-star artifact and 40 Primogems.

---

## 8. Resin System (v1.0)

See also: Economy section (§13) for broader economic context.

| Parameter | Value |
|-----------|-------|
| **Maximum capacity** | 120 (increased to 160 in v1.1) |
| **Regeneration rate** | 1 Resin per 8 minutes |
| **Full recharge time** | 16 hours (120 Resin) |
| **Daily natural regeneration** | 180 Resin/day |
| **Fragile Resin** | Consumable item, restores 60 Original Resin. Obtained from AR level-up rewards and Battle Pass. |
| **Condensed Resin** | NOT in v1.0. Added in v1.1. |

### Resin Costs

| Activity | Resin Cost | Reward Type |
|----------|-----------|-------------|
| Ley Line Outcrop (Blossom of Revelation) | 20 | Character EXP Materials |
| Ley Line Outcrop (Blossom of Wealth) | 20 | Mora |
| Domain (Forgery/Mastery/Blessing) | 20 | Weapon/Talent/Artifact materials |
| Normal Boss | 40 | Character Ascension Materials + Artifacts |
| Weekly Boss | 60 | Talent Level-Up Materials + Billets + Artifacts |

**Note on Weekly Boss cost:** In v1.0, all weekly boss claims cost 60 Resin each. The discounted 30-Resin rate for the first 3 weekly claims per week was introduced in v1.5.

### Primogem Resin Refill

Players can spend Primogems to instantly restore 60 Original Resin, up to 6 times per day. Cost escalates:

| Refill # | Primogem Cost |
|----------|--------------|
| 1st | 50 |
| 2nd | 100 |
| 3rd | 100 |
| 4th | 150 |
| 5th | 200 |
| 6th | 200 |

---

## 9. Systems NOT in v1.0

The following systems were frequently asked about but were NOT available at v1.0 launch:

| System | Version Added | Date |
|--------|--------------|------|
| **Reputation System** (Mondstadt/Liyue City Reputation, bounties, weekly requests) | v1.1 | November 11, 2020 |
| **Condensed Resin** | v1.1 | November 11, 2020 |
| **Resin cap increase (120 > 160)** | v1.1 | November 11, 2020 |
| **Childe Weekly Boss** (Enter the Golden House) | v1.1 | November 11, 2020 |
| **Dragonspine** (Mondstadt sub-region) | v1.2 | December 23, 2020 |
| **Serenitea Pot** (Housing system) | v1.5 | April 28, 2021 |
| **Portable Waypoint** | v1.1 (Reputation reward) | November 11, 2020 |
| **Oculus Resonance Stones** | v1.1 (Reputation reward) | November 11, 2020 |
| **World Level adjustment** (lower WL by 1) | v1.4 | March 17, 2021 |
| **Weekly Boss discount** (30 Resin for first 3) | v1.5 | April 28, 2021 |

---

## 10. Enemies & Opponents

### 10.1 Enemy Classification

Enemies are categorized into four tiers:

| Tier | Description | Examples |
|------|-------------|---------|
| **Common** | Overworld fodder, low HP, appear in groups | Hilichurls, Slimes, Treasure Hoarders |
| **Elite** | Tougher enemies with shields or special mechanics | Abyss Mages, Fatui Skirmishers, Lawachurls, Ruin Guards |
| **Normal Boss** | Open-world bosses requiring 40 Original Resin to claim rewards | Hypostases, Regisvines, Oceanid |
| **Weekly Boss** | Domain bosses claimable once per week, 60 Resin each in v1.0 | Stormterror Dvalin, Andrius |

### 10.2 Hilichurl Family

The core mob family. Hilichurls grow into Mitachurls, which grow into Lawachurls by accumulating elemental energy.

#### Basic Hilichurls

| Variant | Behavior |
|---------|----------|
| **Hilichurl** | Melee attacks with clubs. Lowest threat. |
| **Hilichurl Fighter** | More aggressive melee with slightly higher damage. |
| **Hilichurl Berserker** | Charges at player, deals higher damage, lower defense. |
| **Hilichurl Shooter** | Ranged attacks with crossbow. Pyro/Electro/Cryo arrows apply elements. |
| **Hilichurl Grenadier** | Throws Slimes as explosive projectiles. Element matches the Slime used. |
| **Hilichurl Guard** | Carries a wooden or rock shield. Shield blocks frontal attacks. Wooden shields are weak to Pyro; rock shields are weak to Geo/Claymore. |

#### Samachurls (Elemental Casters)

Spiritual leaders of Hilichurl tribes. Act as support/caster enemies.

| Variant | Abilities |
|---------|-----------|
| **Anemo Samachurl** | Summons tornadoes and vacuum fields that pull players. |
| **Geo Samachurl** | Summons Geo pillars/spikes from the ground. |
| **Hydro Samachurl** | Casts Healing Rain AoE that heals all nearby allies. Priority kill target. |
| **Cryo Samachurl** | Summons ice spikes from the ground. |
| **Dendro Samachurl** | Summons thorny vines that apply Dendro. |
| **Electro Samachurl** | Summons thunderstrikes from above. |

#### Mitachurls

| Variant | Mechanics |
|---------|-----------|
| **Wooden Shieldwall Mitachurl** | Large wooden shield blocks frontal attacks. Weak to Pyro (burns shield). Can charge. |
| **Rock Shieldwall Mitachurl** | Geo rock shield blocks frontal attacks. Weak to Claymore/Geo/Overloaded. |
| **Blazing Axe Mitachurl** | Pyro-infused axe attacks. Charges and slams. |
| **Ice Shieldwall Mitachurl** | Cryo ice shield. Weak to Pyro. |

#### Lawachurls

Largest Hilichurl evolution. Generate elemental armor when entering enhanced state. Armor grants massive resistance and enables stronger attacks. Armor decays naturally over time but can be broken faster with correct elements.

| Variant | Element | Armor Weakness | Available in v1.0 |
|---------|---------|---------------|-------------------|
| **Stonehide Lawachurl** | Geo | Claymore, Overloaded, Geo constructs | Yes |
| **Frostarm Lawachurl** | Cryo | Pyro | Yes |

### 10.3 Slimes

Elemental lifeforms. Immune to their own element. Come in Small and Large variants. Large variants have additional mechanics.

| Element | Immunity | Notable Mechanics |
|---------|----------|-------------------|
| **Pyro Slime** | Pyro | Burns on contact. Large variant: explodes when defeated while ignited. Extinguish with Hydro. |
| **Hydro Slime** | Hydro | Heals nearby allies (Large variant). |
| **Cryo Slime** | Cryo | Large variant generates a Cryo shield. Break with Pyro. |
| **Electro Slime** | Electro | Discharges Electro. Large Electro Slime + small Mutant Electro Slime create a circuit that deals continuous Electro damage. |
| **Anemo Slime** | Anemo | Floats. Large variant inhales and flies, then body-slams. |
| **Geo Slime** | Geo | Large variant has Geo armor. Break with Claymore/Overloaded. |
| **Dendro Slime** | Dendro | Burrows and hides in the ground disguised as plants. Burn with Pyro to flush out. |

### 10.4 Fatui

#### Fatui Skirmishers

Six variants. Each can generate an elemental shield that massively increases RES and enhances attacks. Shields are broken efficiently with specific counter-elements. Breaking a shield stuns the Skirmisher.

| Variant | Element | Shield Counter |
|---------|---------|---------------|
| **Pyroslinger Bracer** | Pyro | Hydro |
| **Hydrogunner Legionnaire** | Hydro | Electro |
| **Cryogunner Legionnaire** | Cryo | Pyro |
| **Electrohammer Vanguard** | Electro | Cryo |
| **Anemoboxer Vanguard** | Anemo | Absorbs elements; apply any non-Anemo element to trigger counter |
| **Geochanter Bracer** | Geo | Claymore/Overloaded (blunt damage) |

The Hydrogunner can heal allies. The Electrohammer has the most dangerous attacks. The Anemoboxer creates an Anemo barrier that absorbs and counters incoming elemental attacks.

#### Fatui Pyro Agent

Elite enemy. Does not generate a shield. Mechanics:
- Creates a ring of three circling Pyro knives on first attack.
- Turns invisible after shield animation. Visible as faint black smoke, footprints in sand, or swaying grass.
- Performs dash-slash combos while invisible.
- Shadow clones appear during combo attacks.

#### Fatui Electro Cicin Mage

Elite enemy available at v1.0 launch. Mechanics:
- Summons Electro Cicins (small flying creatures) that deal Electro damage.
- Shield strength proportional to number of active Cicins. Kill Cicins to weaken shield.
- Teleports frequently. Only Freeze can reliably prevent teleportation.
- Cicins regenerate over time.

**Note:** Cryo Cicin Mage was added in Version 1.1, not present at v1.0 launch.

### 10.5 Abyss Order

#### Abyss Mages

Elite enemies with elemental shields. Shields have very large HP pools; direct damage is ineffective compared to elemental reactions. When shield breaks, Abyss Mage is stunned for a long duration.

| Variant | Shield Element | Best Counter |
|---------|---------------|-------------|
| **Pyro Abyss Mage** | Pyro | Hydro |
| **Hydro Abyss Mage** | Hydro | Cryo (Freeze) or Electro |
| **Cryo Abyss Mage** | Cryo | Pyro |

All variants teleport, summon elemental attacks, and perform a spinning ring attack when their shield is active.

**Not in v1.0:** Abyss Heralds (added v1.4) and Abyss Lectors (added v1.5) were not present at launch.

### 10.6 Treasure Hoarders

Human enemies, no elemental abilities. Appear in groups.

| Variant | Role |
|---------|------|
| **Scout** | Melee — roundhouse kicks and throwing knives |
| **Marksman** | Ranged — crossbow attacks |
| **Handyman** | Melee — kicks and dropkicks |
| **Potioneer** (Pyro/Cryo/Electro/Hydro) | Throws elemental potions. Acts as ranged elemental support. |
| **Gravedigger** | Melee with shovel |
| **Crusher** | Heavy melee with large weapon |
| **Seaman** | Throws water bombs |
| **Boss** | Throws smoke bombs for escape, stronger attacks |

### 10.7 Whopperflowers

Mimetic plant enemies that disguise themselves as collectible plants (Sweet Flowers, Mist Flowers, Flaming Flowers). Available in Pyro and Cryo variants in v1.0. Electro Whopperflower was added in v2.0 (Inazuma).

| Mechanic | Description |
|----------|-------------|
| **Disguise** | Appear as normal plants. No white silhouette in Elemental Vision (unlike real plants). Interacting triggers ambush. |
| **Burrow** | Sink into ground and emerge at new location. Invulnerable during burrow animation. |
| **Elemental Shield** | Periodically charge up and generate elemental shield. |
| **Stun** | Breaking shield stuns Whopperflower for ~15 seconds. During stun, loses resistances. |
| **Element Immunity** | Immune to own element while shield is active. |

### 10.8 Ruin Machines

#### Ruin Guard

Large automaton. High HP, heavy physical damage. Two weak points: glowing eye (front) and back socket.

| Mechanic | Description |
|----------|-------------|
| **Weak Points** | Hit eye or back socket to stagger. Two successful hits in quick succession causes full deactivation (~10 seconds). |
| **Spin Attack** | Devastating spinning charge that stun-locks unshielded characters. Both weak points exposed during spin. Hit either to cancel. |
| **Missile Barrage** | Fires homing missiles. Dodge sideways. |
| **Jump Slam** | Leaps and slams, creating AoE shockwave. |

#### Ruin Hunter

Faster, airborne automaton. One weak point: glowing eye (only exposed while airborne).

| Mechanic | Description |
|----------|-------------|
| **Ground Mode** | Melee blade attacks. Fast combos. |
| **Bombardment Mode** | Triggered when player is at higher elevation or after enough time passes. Flies up and fires missiles/energy blasts. Eye weak point exposed. |
| **Stun** | Single hit on eye while airborne crashes it to ground, stunning it. |

### 10.9 Other Enemies

#### Geovishap Hatchling

Small lizard-like Geo creature. Available in v1.0.

- **Burrow:** Digs underground (invulnerable). Leaves 2 Geo crystals on surface that can be picked up for Crystallize shields.
- **Roll Attack:** After emerging, rolls uncontrollably around the area or charges at player.
- **Counter with Shield:** If it rolls into a player protected by a shield (from Geo crystals or character talents), it flips onto its back and is vulnerable for several seconds.
- **Frozen Effective:** Freeze reaction extends vulnerability windows.

**Note:** Full-sized Geovishaps were added in v1.3, not present at v1.0 launch.

#### Eye of the Storm

Floating Anemo entity. Elite enemy.

- **Immune to Anemo.** Requires non-Anemo damage.
- **Hovers out of melee reach** most of the time. Bow/Catalyst users recommended.
- **Vacuum Trap:** Creates wind current pulling player beneath it, then body-slams.
- **Tempest:** Channels Anemo wall around arena, dealing DoT to grounded players. Spawns upward drafts inside. Core is exposed during channel — hitting it cancels the attack.

---

## 11. Elite & Boss Enemies

### 11.1 Normal Bosses (40 Original Resin)

Normal bosses respawn a few minutes after defeat. Claiming the Trounce Blossom costs 40 Original Resin. Rewards: Character Ascension Materials (element-specific gemstones, boss-specific materials), Artifacts, Mora, Adventure EXP.

#### Hypostases

Elemental cube bosses. Core is invulnerable during attack animations; becomes exposed briefly after each attack sequence. At critical HP, enters revival phase requiring a specific counter to finish.

| Boss | Element | Location | Revival Mechanic |
|------|---------|----------|-----------------|
| **Anemo Hypostasis (Beth)** | Anemo | Stormbearer Mountains, Mondstadt | Releases 4 Anemo orbs via updrafts. Glide and collect orbs. Each uncollected orb restores 10-15% HP. |
| **Electro Hypostasis (Aleph)** | Electro | Cape Oath, Mondstadt | Spawns 3 Electro prisms. Destroy all 3 before timer expires or it regenerates HP. |
| **Geo Hypostasis (Gimel)** | Geo | Guyun Stone Forest, Liyue | Warps to center, summons 3 Geo pillars. Destroy all pillars (weak to Claymore/Geo/Overloaded) before it revives. |

All Hypostases are immune to their own element. Attack windows: after each attack pattern, the core is briefly exposed (3-5 seconds).

#### Regisvines

Giant plant bosses rooted in place. Two corolla weak points that cycle between root and crown positions.

| Boss | Element | Location | Weak Point Counter |
|------|---------|----------|--------------------|
| **Cryo Regisvine** | Cryo | Thousand Winds Temple, Mondstadt | Pyro (most effective). Anemo/Electro/Geo work but slower. |
| **Pyro Regisvine** | Pyro | Liyue, near Tianqiu Valley | Hydro (most effective). Cryo/Electro work but slower. |

**Shared Mechanics:**
1. **Phase 1 — Root Corolla:** Weak point at base. Melee can attack freely. Break corolla to stun boss (~12 seconds DPS window).
2. **Phase 2 — Crown Corolla:** After root corolla reform and break again, weak point moves to the flower head. Requires ranged (Bow/Catalyst) to hit while upright, or wait for head to dip during attack animation.
3. **Attacks:** Faceplant slam, sweeping beam, ice/fire seed barrage, ground AoE.

#### Oceanid (Rhodeia of Loch)

Hydro boss near Qingce Village, Liyue. Cannot be attacked directly. Fights entirely through Hydro Mimics.

| Mechanic | Description |
|----------|-------------|
| **Hydro Mimics** | Summons waves of water creatures (birds, boars, crabs, squirrels, frogs, ducks, hawks, etc.). Each mimic type has different attack patterns. |
| **Platform Sinking** | Arena consists of multiple platforms. After each 2 waves of mimics, Oceanid submerges 2 platforms, reducing available space. |
| **Enrage Timer** | If mimics are not killed fast enough, Oceanid creates a tracking whirlpool beneath the player dealing DoT, then explodes. |
| **Element Counters** | All mimics are Hydro. Cryo (Freeze), Electro (Electro-Charged), and Pyro (Vaporize) are effective. Different elements work better against different mimic types. |
| **Final Phase** | At critical HP, Oceanid floods remaining platforms with Hydro damage. |

### 11.2 Weekly Bosses (Trounce Domains)

Claimable once per week. Resin cost in v1.0: 60 per claim (no discount). The 30-Resin discount for the first 3 weekly claims was added in v1.5. Rewards: Talent Level-Up Materials (required for talents beyond Lv6), Gladiator's Finale and Wanderer's Troupe artifact sets, Northlander Billets (weapon crafting prototypes), Gemstones, Mora, Adventure EXP.

#### Stormterror Dvalin

Trounce Domain: Confront Stormterror. Located in Stormterror's Lair, Mondstadt. Unlocked via Archon Quest Prologue Act III.

**Arena:** Circular platform divided into segments. Platforms develop blue cracks (Caelestinum Finale Termini) that deal continuous Anemo damage when stood on. Wind currents at platform edges allow gliding to safe segments.

| Phase | Mechanics |
|-------|-----------|
| **Shield Phase** | Dvalin perches on platform edge. Attack claws to break his shield. Once broken, weak point on neck is exposed. Climb onto neck to deal damage. |
| **Aerial Phase** | Dvalin flies around arena. Fires wind projectiles at platforms. Move to adjacent platform to dodge. |
| **Bite Attack** | Dvalin bites at platform edge. Dodge to edge of platform. Small DPS window on neck after bite. |
| **Energy Bombs** | After damaging weak point, Dvalin retreats to center and fires pairs of energy bombs. Keep moving side-to-side. |
| **Meteor Shower** (sub-25% HP) | Constant meteor barrage. Watch for ground sigils indicating impact points. |

**Platform degradation** increases over 5 distinguishable phases, forcing players to use wind currents to reach safe platforms.

**Talent Materials:**

| Material | Characters (at launch) |
|----------|----------------------|
| Dvalin's Plume | Diluc, Jean, Bennett |
| Dvalin's Claw | Lisa, Noelle, Razor, Xiangling |
| Dvalin's Sigh | Amber, Beidou, Chongyun, Traveler |

#### Andrius, Lupus Boreas (Wolf of the North)

Open-world weekly boss in Wolvendom, Mondstadt. Unlocked via Razor's Story Quest (Lupus Minor Chapter Act I). **Unique:** fought in the overworld, not a domain. No walls — can be pushed out of arena.

**Immune to Cryo and Anemo.** Bring Pyro, Electro, Hydro, or Geo.

| Phase | Trigger | Mechanics |
|-------|---------|-----------|
| **Phase 1 (Cryo)** | Start of fight | Primarily Cryo attacks. Ice spike formations, freezing ground AoE. Relatively stationary. |
| **Phase 2 (Cryo/Anemo)** | ~50% HP | Dramatic transformation. Incorporates Anemo attacks alongside Cryo. Tornadoes that lift players (fall damage). Ice storms covering large arena portions. More aggressive movement. |
| **Icicle Rain** | Both phases | Small AoE markers on floor. Moderate damage per hit. |
| **Frozen Path** | Both phases | Freezes ground in a line, followed by ice spikes traveling along the path. |
| **Dash Attacks** | Both phases | Fast dashes across arena. |

**Talent Materials:**

| Material | Characters (at launch) |
|----------|----------------------|
| Tail of Boreas | Traveler, Fischl, Klee, Keqing |
| Ring of Boreas | Kaeya, Mona, Venti, Xingqiu |
| Spirit Locket of Boreas | Barbara, Diluc, Ningguang, Razor |

---

## 12. Spiral Abyss

Endgame combat challenge domain located on Musk Reef (accessed via a portal in Cape Oath, Mondstadt). Unlocked at Adventure Rank 20.

### 12.1 Structure

| Component | Floors | Description |
|-----------|--------|-------------|
| **Abyss Corridor** | 1-8 | Introductory floors. Single-team for floors 1-4. Two-team split starting floor 5. Rewards are one-time only. |
| **Abyssal Moon Spire** | 9-12 | Endgame floors. Always two-team split. Rewards reset on the 1st and 16th of each month (biweekly in v1.0). Enemy compositions rotate with game version updates. |

Each floor has **3 Chambers**. Each chamber has a **First Half** and **Second Half** (floors 5+), requiring two separate teams of up to 4 characters. Characters cannot be shared between teams.

### 12.2 Star Rating System

Each chamber awards 0-3 Abyssal Stars based on completion time or other conditions (e.g., protect a monolith). Maximum 9 stars per floor (3 chambers x 3 stars).

**Progression:** Requires 6+ stars on the current floor to unlock the next floor.

### 12.3 Rewards

#### Abyss Corridor (Floors 1-8) — One-Time Rewards

Star's Bounty (per 3-star milestone): Primogems, Mora, character EXP materials, weapon EXP materials.

#### Abyssal Moon Spire (Floors 9-12) — Resetting Rewards

| Stars per Floor | Primogems |
|----------------|-----------|
| 3 stars | 50 |
| 6 stars | 50 |
| 9 stars | 100 |
| **Total per floor** | **200** |
| **Total (floors 9-12)** | **800** |

Additional rewards per star milestone: Mora, Artifact fodder.

### 12.4 Benedictions

Before each Chamber, player chooses 1 of 3 randomly offered Benedictions. Effects can be chamber-only or floor-wide. Examples: stat buffs, elemental damage bonuses, energy regeneration, healing effects.

### 12.5 Ley Line Disorders (v1.0 Original Layout)

Ley Line Disorders are floor-wide buffs/debuffs that shape team composition requirements.

| Floor | Ley Line Disorder |
|-------|-------------------|
| Floor 9 | Pyro and Electro DMG +75%. Overloaded DMG +200%. |
| Floor 10 | Periodic Smoldering Flames (Pyro DoT until cleansed). Incoming Healing Bonus +50%. |
| Floor 11 | Electro and Cryo DMG +75%. Superconduct DMG +300%, additional Physical RES reduction -20%. |
| Floor 12 | First Half: Cryo Elemental Node buffs enemy RES/DEF until destroyed. Second Half: Electro Elemental Node buffs enemy RES/CD reduction until destroyed. Periodic Condensed Ice debuff on player (increased Stamina consumption, no natural Stamina regen until cleansed). Pyro DMG +75%. |

Floor 12's Condensed Ice debuff combined with the Pyro damage bonus made Pyro DPS characters (especially Diluc and Bennett) effectively mandatory at launch.

### 12.6 Enemy Composition (v1.0)

Floors 1-8 (Abyss Corridor) feature progressively harder combinations of common and elite enemies: Hilichurls, Slimes, Treasure Hoarders (early floors), adding Abyss Mages, Fatui Skirmishers, Mitachurls, and Ruin Guards on higher floors.

Floors 9-12 (v1.0 original rotation, September 28 2020 - January 1 2021) featured Fatui Skirmishers, Abyss Mages, Ruin Guards, Lawachurls, and Whopperflowers as primary threats, with the Elemental Nodes on Floor 12 adding a puzzle-like element to team composition.

---

## 13. Economy & Currencies

### 13.1 Currency Overview

| Currency | Type | Primary Source | Primary Sink |
|----------|------|---------------|-------------|
| **Mora** | General gold | Everything: quests, chests, Ley Lines, domains, events | Character/weapon leveling, talent upgrades, artifact upgrades, crafting, forging, shop purchases |
| **Primogems** | Premium (earnable) | Daily Commissions (60/day), Spiral Abyss, chests, quests, achievements, events, mail | Convert to Fates (160 per Fate) |
| **Genesis Crystals** | Premium (paid only) | Real-money top-up only | Convert to Primogems (1:1), purchase outfit cosmetics |
| **Intertwined Fate** | Wish currency | Primogem conversion, events, Battle Pass, Paimon's Bargains | Character/Weapon Event Wish banners |
| **Acquaint Fate** | Wish currency | Primogem conversion, AR level-up rewards, character ascension rewards, Battle Pass, Paimon's Bargains | Standard (Wanderlust Invocation) banner |
| **Masterless Stardust** | Wish byproduct | 15 per 3-star weapon from wishes | Paimon's Bargains (Stardust Exchange) — monthly Fates, materials |
| **Masterless Starglitter** | Wish byproduct (rare) | 4-star weapons: 2 each. 4-star duplicate characters: 2 (or 5 if max constellation). 5-star duplicate characters: 10 (or 25 if max constellation). | Paimon's Bargains (Starglitter Exchange) — characters (34), weapons (24), Fates (5 each) |
| **Original Resin** | Energy/stamina | Regenerates 1 per 8 minutes. Cap: 120 in v1.0. | Claiming boss rewards (40 normal, 60 weekly in v1.0), domains, Ley Line Outcrops |
| **Fragile Resin** | Resin supplement | AR level-up rewards, Battle Pass, events | Restores 60 Original Resin instantly |
| **Anemo Sigils** | Regional currency | Chests, offerings in Mondstadt | Mondstadt Souvenir Shop |
| **Geo Sigils** | Regional currency | Chests, offerings in Liyue | Liyue Souvenir Shop |
| **Companionship EXP** | Character bond | Commissions, Resin-gated activities, events | Raising character Friendship Level (1-10), unlocking character voice lines and story entries |

### 13.2 Mora Sources & Sinks

**Sources:** Ley Line Outcrops (Blossom of Wealth, 20 Resin each), Daily Commissions, quests, chest opening, selling artifacts/materials, events, Spiral Abyss, Battle Pass.

**Sinks (approximate costs at higher levels):**
- Character level 80 > 90: ~170,000 Mora
- Weapon level 80 > 90: ~150,000 Mora
- Talent level 9 > 10: ~700,000+ Mora
- Artifact upgrade to +20: ~270,000 Mora
- Total to fully build one character: ~2-4 million Mora

### 13.3 Primogem Income (v1.0 Monthly Estimate)

| Source | Primogems/Month (approx.) |
|--------|--------------------------|
| Daily Commissions (60/day) | ~1,800 |
| Spiral Abyss (9-12 full stars, 2 resets) | ~1,200 (biweekly reset at v1.0 launch) |
| Events | Variable (~300-600) |
| Maintenance compensation | ~300 |
| New content/exploration | Variable (diminishes over time) |
| **Estimated monthly total (F2P)** | **~3,600-4,200** |

At 160 Primogems per Fate, this yields roughly 22-26 wishes per month from Primogems alone.

---

## 14. Wish / Gacha System

### 14.1 Banner Types (v1.0)

| Banner | Currency | 5-Star Pool | 4-Star Pool | Notes |
|--------|----------|-------------|-------------|-------|
| **Character Event Wish** | Intertwined Fate | Featured character + standard 5-star characters | 3 featured 4-stars + standard 4-stars | Limited-time. 1 banner active at a time. |
| **Weapon Event Wish (Epitome Invocation)** | Intertwined Fate | 2 featured 5-star weapons + standard 5-star weapons | 5 featured 4-star weapons + standard 4-stars | Limited-time. No Epitomized Path in v1.0 (added v2.0). |
| **Standard Wish (Wanderlust Invocation)** | Acquaint Fate | Standard 5-star characters and weapons (mixed pool) | Standard 4-star characters and weapons | Permanent. Never changes. |
| **Beginner's Wish (Novice Wishes)** | Acquaint Fate | Standard pool | Standard pool | 20 wishes max (2x 10-pulls). Discounted to 8 Fates per 10-pull. Guaranteed Noelle on first 10-pull. |

### 14.2 Base Drop Rates

| Banner | 5-Star Base Rate | 4-Star Base Rate | 3-Star Rate |
|--------|-----------------|-----------------|-------------|
| Character Event Wish | 0.6% | 5.1% | 94.3% |
| Weapon Event Wish | 0.7% | 6.0% | 93.3% |
| Standard Wish | 0.6% | 5.1% | 94.3% |

### 14.3 Pity System

#### 5-Star Pity

| Banner Type | Soft Pity Start | Hard Pity (Guaranteed) | Notes |
|-------------|----------------|----------------------|-------|
| Character Event Wish | Pull 74-75 (rate ramps up ~6% per pull) | Pull 90 | Consolidated rate including pity: ~1.6% (avg ~62.5 pulls per 5-star) |
| Weapon Event Wish | Pull 63-65 | Pull 80 | |
| Standard Wish | Pull 74-75 | Pull 90 | |

#### 4-Star Pity

Guaranteed 4-star or higher item every 10 pulls across all banners. If 9 consecutive pulls yield only 3-stars, the 10th is guaranteed 4-star+.

#### 50/50 System (Character Event Wish)

When a 5-star drops on the Character Event Wish:
1. **50% chance** it is the featured character.
2. **50% chance** it is a standard 5-star character (Diluc, Jean, Keqing, Mona, Qiqi at launch).
3. If you lose the 50/50 (get a standard character), your **next** 5-star on the Character Event Wish is **guaranteed** to be the featured character.

This guarantee carries across different Character Event Wish banners.

#### Weapon Banner Odds (v1.0)

When a 5-star drops on the Weapon Event Wish:
1. **75% chance** it is one of the two featured 5-star weapons.
2. **25% chance** it is a standard 5-star weapon.
3. If you get a standard weapon, the next 5-star is guaranteed to be one of the two featured weapons (but you cannot choose which one).

**No Epitomized Path in v1.0.** There was no way to guarantee a specific featured weapon. Epitomized Path was added in v2.0.

#### 4-Star Featured Guarantee

On Character/Weapon Event Wish, if a 4-star item is not one of the featured 4-stars, the next 4-star is guaranteed to be one of the featured items. Maximum 20 pulls to guarantee a specific featured 4-star type.

### 14.4 Pity Counter Behavior

- Pity counters are **per banner type**, not per specific banner. Switching between different Character Event Wish banners preserves pity.
- Pity counters for Character Event Wish, Weapon Event Wish, and Standard Wish are completely independent.
- 50/50 guarantee state also carries across banner rotations within the same banner type.
- Beginner's Wish has its own separate counter (but only 20 total pulls are available).

### 14.5 Maximum Cost to Guarantee

| Target | Maximum Pulls | Maximum Primogems | Maximum USD (at $1.99/100 Genesis Crystal rate, first-time bonus) |
|--------|--------------|-------------------|------------------------------------------------------------------|
| Featured 5-star character (worst case) | 180 (lose 50/50 at 90, win at 90) | 28,800 | ~$360-$450 |
| Featured 5-star character (average) | ~105 (with soft pity) | ~16,800 | ~$200-$250 |

---

## 15. Quest System

### 15.1 Archon Quests (Main Story)

The main storyline following the Traveler and Paimon searching for the Traveler's lost sibling across Teyvat.

#### Prologue: Mondstadt

| Act | Title | AR Requirement |
|-----|-------|---------------|
| Act I | The Outlander Who Caught the Wind | None (tutorial) |
| Act II | For a Tomorrow Without Tears | AR 10 |
| Act III | Song of the Dragon and Freedom | AR 18 |

**Story Summary:** Traveler arrives in Mondstadt, meets Paimon, encounters the Knights of Favonius, and helps free Stormterror (Dvalin) from Abyss Order corruption. Introduces Venti as the Anemo Archon Barbatos.

#### Chapter I: Liyue (Partial in v1.0)

| Act | Title | AR Requirement | Version |
|-----|-------|---------------|---------|
| Act I | Of the Land Amidst Monoliths | AR 23 | v1.0 |
| Act II | Farewell, Archaic Lord | AR 25 | v1.0 |
| Act III | A New Star Approaches | AR 35 | v1.1 (NOT in v1.0) |

**Story Summary (v1.0 portion):** Traveler arrives in Liyue Harbor for the Rite of Descension. Rex Lapis (the Geo Archon Morax) appears to die. Traveler is framed and must investigate, encountering the Liyue Qixing, Adepti, and the Fatui. Act II ends on a cliffhanger with the Traveler still investigating.

### 15.2 Story Quests (Character Quests)

Character-specific quest lines unlocked with Story Keys. Each requires 1 Story Key (earned every 8 Daily Commissions completed, i.e., every 2 days). Minimum AR requirement varies per quest.

#### v1.0 Launch Story Quests

| Character | Chapter | Act Title | AR Req |
|-----------|---------|-----------|--------|
| Amber | Lepus Chapter: Act I | Wind, Courage, and Wings | — |
| Kaeya | Pavo Ocellus Chapter: Act I | Secret Pirate Treasure | — |
| Lisa | Tempus Fugit Chapter: Act I | Troublesome Work | — |
| Razor | Lupus Minor Chapter: Act I | The Meaning of Lupical | AR 21 |
| Xiangling | Trulla Chapter: Act I | Mondstadt Gastronomy Trip | AR 26 |
| Xingqiu | Fabulae Textile Chapter: Act I | Bookworm Swordsman | AR 26 |
| Klee | Trifolium Chapter: Act I | True Treasure | AR 32 |
| Jean | Leo Minor Chapter: Act I | Master's Day Off | AR 34 |
| Diluc | Noctua Chapter: Act I | Darknight Hero's Alibi | AR 34 |
| Venti | Carmen Dei Chapter: Act I | Should You Be Trapped in a Windless Land | AR 36 |

**Note:** Mona's Story Quest (Astrolabos Chapter: Act I) was added October 26, 2020, shortly after launch but technically post-v1.0.

### 15.3 World Quests

Scattered throughout Mondstadt and Liyue, given by NPCs. Some are standalone, others form quest chains. Rewards: Primogems, Mora, Adventure EXP, materials, sometimes recipes or blueprints. No Story Key required.

### 15.4 Daily Commissions

| Parameter | Value |
|-----------|-------|
| Number per day | 4 |
| Unlock requirement | AR 12 |
| Reset time | Daily server reset (4:00 AM server time) |
| Rewards per commission | 10 Primogems + Mora + Adventure EXP + Companionship EXP (scales with World Level) |
| Bonus reward (Katheryne) | 20 Primogems + 500 Adventure EXP + Mora (after completing all 4 and reporting to Katheryne) |
| **Total daily Primogems** | **60** |

Commissions are randomly assigned from a pool specific to the selected region (Mondstadt or Liyue). Mix of combat, collection, escort, and NPC interaction tasks. Some commissions are multi-part chains that only progress one step per day.

---

## 16. Co-Op System

### 16.1 Unlock & Basics

| Parameter | Value |
|-----------|-------|
| Unlock requirement | AR 16 + complete Archon Quest Prologue Act I |
| Max players | 4 |
| Matchmaking | Direct UID join, friend list, or random co-op matching |
| World Level restriction | Can only join worlds at same or lower World Level than your own |

### 16.2 Character Limits in Co-Op

| Player Count | Characters per Player |
|-------------|----------------------|
| 2 players | 2 each |
| 3 players | Host: 2, Guests: 1 each |
| 4 players | 1 each |

Duplicate characters are allowed in the overworld but not inside Domains.

### 16.3 Available Activities

| Activity | Available | Notes |
|----------|-----------|-------|
| Overworld exploration | Yes | Fight enemies, gather resources (some instanced per player) |
| Domains (Artifacts, Talents, Weapons) | Yes | Host initiates |
| Normal Bosses (Hypostases, Regisvines, Oceanid) | Yes | Host initiates |
| Weekly Boss: Andrius | Yes | Host initiates |
| Weekly Boss: Stormterror | **No** | Single-player only due to unique platform mechanics |
| Ley Line Outcrops | Yes | Host initiates |
| Daily Commissions | Yes | Host's commissions only |
| Spiral Abyss | **No** | Single-player only |
| Archon Quests | **No** | Co-op disabled during story quests |
| Story Quests | **No** | Co-op disabled during character quests |
| Open chests | **No** | Host only |
| Statue of the Seven interaction | **No** | Host only |

### 16.4 Resource Collection in Co-Op

- **Shared:** Ore (except Cor Lapis), enemy drops from common/elite enemies.
- **Host only:** Chests, most plants and specialty items, cooking ingredients from the overworld.

---

## 17. Battle Pass

### 17.1 Structure

| Parameter | Value |
|-----------|-------|
| Levels | 1-50 |
| EXP per level | 1,000 BP EXP |
| Total EXP required | 50,000 BP EXP |
| Duration | ~6 weeks per BP period |
| EXP sources | Daily missions (up to ~1,200/day), Weekly missions (up to ~4,500/week, adjusted from 1,600 Resin to 1,200 in v1.1), Period missions (one-time objectives) |

### 17.2 Tiers

| Tier | Cost | Description |
|------|------|-------------|
| **Sojourner's Battle Pass** | Free | Basic rewards: Mora, character/weapon EXP materials, talent books, artifact EXP materials |
| **Gnostic Hymn** | $9.99 USD | Unlocks premium reward track with enhanced rewards. Includes BP weapon choice at level 30. |
| **Gnostic Chorus** | $19.99 USD ($11.99 if upgrading from Hymn) | Everything in Gnostic Hymn + 10 instant BP levels + exclusive namecard + Fragile Resin x5 + Furnishing Blueprint |

### 17.3 Key Rewards

#### Free Track (Sojourner's)
- 5 Acquaint Fates (1 every 10 levels)
- 680 Primogems
- Mora (~270,000+)
- Character EXP, Weapon EXP, Artifact EXP materials
- Fragile Resin (at levels 5, 15, 25, 35, 45)

#### Paid Track (Gnostic Hymn)
- 4 Intertwined Fates
- 680 Primogems
- Additional Mora, talent books, enhancement materials
- **BP Weapon choice at Level 30** (one of five, see below)

### 17.4 Battle Pass Weapons (v1.0)

All 4-star weapons with CRIT Rate substat. Player chooses one at BP Level 30 (Gnostic Hymn required). These are the original 5 weapons available from launch:

| Weapon | Type | Passive Effect |
|--------|------|---------------|
| **The Black Sword** | Sword | Increases DMG dealt by Normal and Charged Attacks. Regenerates HP on Normal/Charged Attack hits. |
| **Serpent Spine** | Claymore | While on field, DMG dealt increases by 6% every 4 seconds (max 5 stacks). Taking DMG removes 1 stack. |
| **Deathmatch** | Polearm | If 2+ enemies nearby, ATK and DEF increase. If fewer than 2 enemies, ATK increases by a larger amount. |
| **Solar Pearl** | Catalyst | Normal Attack hits increase Elemental Skill/Burst DMG for 6s. Elemental Skill/Burst hits increase Normal Attack DMG for 6s. |
| **The Viridescent Hunt** | Bow | On Normal/Aimed Attack hit, creates a cyclone that pulls nearby enemies and deals DMG. |

---

## 18. Shop Systems

### 18.1 Paimon's Bargains

Accessible from the Wish menu. Two sub-shops using Wish byproduct currencies.

#### Stardust Exchange (Masterless Stardust)

Resets monthly (1st of each month).

| Item | Cost | Monthly Limit |
|------|------|--------------|
| Intertwined Fate | 75 Stardust | 5 |
| Acquaint Fate | 75 Stardust | 5 |
| Character EXP (Hero's Wit) | 10 Stardust | 20 |
| Character EXP (Adventurer's Experience) | 5 Stardust | 20 |
| Mystic Enhancement Ore | 10 Stardust | 20 |
| Fine Enhancement Ore | 5 Stardust | 20 |
| Mora (10,000) | 10 Stardust | 20 |

**Priority:** Always buy the 10 monthly Fates first (750 total Stardust).

#### Starglitter Exchange (Masterless Starglitter)

**Characters (34 Starglitter each):** Fixed 6-month rotation of twelve 4-star characters, 2 available per month.

| Month | Characters |
|-------|-----------|
| January / July | Xiangling, Xingqiu |
| February / August | Beidou, Noelle |
| March / September | Ningguang, Barbara |
| April / October | Razor, Amber |
| May / November | Bennett, Lisa |
| June / December | Fischl, Kaeya |

**Weapons (24 Starglitter each):** Alternating between Royal and Blackcliff weapon series every 2 months. One weapon of each type (Sword, Claymore, Polearm, Catalyst, Bow).

| Series | Available Months | Substat |
|--------|-----------------|---------|
| Royal Series | Odd months | ATK% |
| Blackcliff Series | Even months | CRIT DMG |

**Fates:** Intertwined Fate and Acquaint Fate available for 5 Starglitter each (no monthly limit, but Starglitter is scarce).

### 18.2 Souvenir Shops

Region-specific shops that accept Elemental Sigils. Items do not restock (one-time purchases) except for unlimited Mora exchange.

#### Mondstadt — "With Wind Comes Glory" (Marjorie)

Currency: Anemo Sigils. Unlocked at AR 9 via World Quest "Collector of Anemo Sigils."

| Item Category | Examples |
|---------------|---------|
| Traveler Constellation Material | Memory of Roving Gales (225 Anemo Sigils, limit 1) |
| Elemental Ascension Slivers | Agnidus Agate, Varunada Lazurite, Vayuda Turquoise, Shivada Jade, Prithiva Topaz, Vajrada Amethyst |
| Weapon Ascension Materials | Tile of Decarabian's Tower, Boreal Wolf's Milk Tooth, Fetters of the Dandelion Gladiator |
| Northlander Billets | Northlander Sword Billet |
| Mora | 1,600 Mora per 2 Anemo Sigils (unlimited) |

#### Liyue — "Mingxing Jewelry" (Xingxi)

Currency: Geo Sigils.

| Item Category | Examples |
|---------------|---------|
| Traveler Constellation Material | Memory of Immovable Crystals |
| Elemental Ascension Slivers | Same elements as Mondstadt |
| Weapon Ascension Materials | Luminous Sands from Guyun, Mist Veiled Lead Elixir, Grain of Aerosiderite |
| Northlander Billets | Bow, Claymore, Catalyst, Polearm (1 each) |
| Mora | 1,600 Mora per 2 Geo Sigils (unlimited) |

### 18.3 General Goods Shops

NPC merchants in Mondstadt and Liyue sell basic materials for Mora:
- **Food ingredients:** Flour, Sugar, Butter, Milk, Cream, Pepper, Salt, etc.
- **Cooking recipes:** Specific to each merchant.
- **Specialty items:** Region-specific crafting materials.

### 18.4 Blacksmith

Forges weapons and processes ore. Found in both Mondstadt (Wagner) and Liyue (Master Zhang).
- **Weapon Forging:** Craft 4-star weapons using Northlander Billets + materials + Mora.
- **Ore Processing:** Convert raw ore into enhancement materials.
- **Daily Limit:** 30 Mystic Enhancement Ore per day from forging.

---

## 19. Playable Characters (v1.0 Roster)

Version 1.0 launched with 24 playable characters (counting the Traveler's Anemo and Geo forms as one character). 20 characters were available on launch day (September 28, 2020). Venti was the first limited 5-star banner character (Ballad in Goblets, September 28 - October 18). Klee was the second limited 5-star banner character (Sparkling Steps, October 20 - November 10). Both banners fell within v1.0 (v1.1 launched November 11, 2020).

**Not in v1.0:** Xinyan, Diona, Zhongli, and Tartaglia/Childe were added in v1.1. Ganyu and Albedo were added in v1.2.

### 19.1 Roster Overview

| # | Character | Rarity | Element | Weapon | Ascension Stat |
|---|-----------|--------|---------|--------|---------------|
| 1 | Traveler (Anemo/Geo) | 5-star | Anemo / Geo | Sword | ATK% 24.0% |
| 2 | Amber | 4-star | Pyro | Bow | ATK% 24.0% |
| 3 | Barbara | 4-star | Hydro | Catalyst | HP% 24.0% |
| 4 | Beidou | 4-star | Electro | Claymore | Electro DMG Bonus 24.0% |
| 5 | Bennett | 4-star | Pyro | Sword | Energy Recharge 26.7% |
| 6 | Chongyun | 4-star | Cryo | Claymore | ATK% 24.0% |
| 7 | Diluc | 5-star | Pyro | Claymore | CRIT Rate 19.2% |
| 8 | Fischl | 4-star | Electro | Bow | ATK% 24.0% |
| 9 | Jean | 5-star | Anemo | Sword | Healing Bonus 22.2% |
| 10 | Kaeya | 4-star | Cryo | Sword | Energy Recharge 26.7% |
| 11 | Keqing | 5-star | Electro | Sword | CRIT DMG 38.4% |
| 12 | Klee | 5-star | Pyro | Catalyst | Pyro DMG Bonus 28.8% |
| 13 | Lisa | 4-star | Electro | Catalyst | Elemental Mastery 96 |
| 14 | Mona | 5-star | Hydro | Catalyst | Energy Recharge 32.0% |
| 15 | Ningguang | 4-star | Geo | Catalyst | Geo DMG Bonus 24.0% |
| 16 | Noelle | 4-star | Geo | Claymore | DEF% 30.0% |
| 17 | Qiqi | 5-star | Cryo | Sword | Healing Bonus 22.2% |
| 18 | Razor | 4-star | Electro | Claymore | Physical DMG Bonus 30.0% |
| 19 | Sucrose | 4-star | Anemo | Catalyst | Anemo DMG Bonus 24.0% |
| 20 | Venti | 5-star | Anemo | Bow | Energy Recharge 32.0% |
| 21 | Xiangling | 4-star | Pyro | Polearm | Elemental Mastery 96 |
| 22 | Xingqiu | 4-star | Hydro | Sword | ATK% 24.0% |

Standard 5-star characters (permanent pool, obtainable from any banner): Diluc, Jean, Keqing, Mona, Qiqi.
Limited 5-star characters (only during their featured banners): Venti, Klee.
The Traveler is obtained for free during the prologue.
Noelle is guaranteed on the first Beginner's Wish 10-pull.
Barbara is given free at AR 18. Xiangling is given free for clearing Spiral Abyss Floor 3 Chamber 3.

### 19.2 Base Stats

All stats shown are **base stats** without weapons, artifacts, or buffs.

#### Level 1 Base Stats

| Character | HP | ATK | DEF |
|-----------|---:|----:|----:|
| Traveler | 912 | 18 | 57 |
| Amber | 793 | 19 | 50 |
| Barbara | 821 | 13 | 56 |
| Beidou | 1,094 | 19 | 54 |
| Bennett | 1,039 | 16 | 65 |
| Chongyun | 921 | 19 | 54 |
| Diluc | 1,011 | 26 | 61 |
| Fischl | 770 | 20 | 50 |
| Jean | 1,144 | 19 | 60 |
| Kaeya | 976 | 19 | 66 |
| Keqing | 1,020 | 25 | 62 |
| Klee | 801 | 24 | 48 |
| Lisa | 802 | 19 | 48 |
| Mona | 810 | 22 | 51 |
| Ningguang | 821 | 18 | 48 |
| Noelle | 1,012 | 16 | 67 |
| Qiqi | 963 | 22 | 72 |
| Razor | 1,003 | 20 | 63 |
| Sucrose | 775 | 14 | 59 |
| Venti | 820 | 20 | 52 |
| Xiangling | 912 | 19 | 56 |
| Xingqiu | 857 | 17 | 64 |

#### Level 90 Base Stats (Fully Ascended)

| Character | HP | ATK | DEF | Ascension Stat Value |
|-----------|-----:|----:|----:|---------------------|
| Traveler | 10,875 | 212 | 683 | ATK% 24.0% |
| Amber | 9,461 | 223 | 601 | ATK% 24.0% |
| Barbara | 9,787 | 159 | 669 | HP% 24.0% |
| Beidou | 13,050 | 225 | 648 | Electro DMG 24.0% |
| Bennett | 12,397 | 191 | 771 | Energy Recharge 26.7% |
| Chongyun | 10,984 | 223 | 648 | ATK% 24.0% |
| Diluc | 12,981 | 335 | 784 | CRIT Rate 19.2% |
| Fischl | 9,189 | 244 | 594 | ATK% 24.0% |
| Jean | 14,695 | 239 | 769 | Healing Bonus 22.2% |
| Kaeya | 11,636 | 223 | 792 | Energy Recharge 26.7% |
| Keqing | 13,103 | 323 | 799 | CRIT DMG 38.4% |
| Klee | 10,287 | 311 | 615 | Pyro DMG 28.8% |
| Lisa | 9,570 | 232 | 573 | Elemental Mastery 96 |
| Mona | 10,409 | 287 | 653 | Energy Recharge 32.0% |
| Ningguang | 9,787 | 212 | 573 | Geo DMG 24.0% |
| Noelle | 12,071 | 191 | 799 | DEF% 30.0% |
| Qiqi | 12,368 | 287 | 922 | Healing Bonus 22.2% |
| Razor | 11,962 | 234 | 751 | Physical DMG 30.0% |
| Sucrose | 9,244 | 170 | 703 | Anemo DMG 24.0% |
| Venti | 10,531 | 263 | 669 | Energy Recharge 32.0% |
| Xiangling | 10,875 | 225 | 669 | Elemental Mastery 96 |
| Xingqiu | 10,222 | 202 | 758 | ATK% 24.0% |

### 19.3 Character Details

---

#### Traveler (Anemo) — 5-star Anemo Sword

The player character. Can switch between Anemo and Geo by resonating with the corresponding Statue of the Seven. Both forms share the same base stats but have entirely different Elemental Skills, Bursts, passives, and constellations. Constellations are unlocked via Adventure Rank rewards and Souvenir Shop items, not through the Wish system.

**Normal Attack — Foreign Ironwind:** 5-hit combo. Charged Attack: 2 rapid sword strikes consuming Stamina.

**Elemental Skill — Palm Vortex (6s CD):** Press to release a palm-sized vortex dealing Anemo DMG and launching enemies. Hold to continuously pull in nearby objects/enemies, dealing Anemo DMG continuously, then release for a final blast. Absorbs one element (Pyro/Hydro/Cryo/Electro) for additional elemental DMG.

**Elemental Burst — Gust Surge (60 Energy, 15s CD):** Summons a forward-moving tornado that pulls in objects and enemies, dealing continuous Anemo DMG. Can absorb one element for additional DMG of that element.

**Passive Talents:**
- **A1 — Slitting Wind:** The last hit of the Normal Attack combo releases a wind blade, dealing 60% ATK as Anemo DMG to enemies in its path.
- **A4 — Second Wind:** Palm Vortex kills regenerate 2% HP over 5s. Triggers once every 5s.

**Constellations (Anemo):**
- **C1 — Raging Vortex:** Palm Vortex pulls enemies within a 5m radius toward the vortex center.
- **C2 — Uprising Whirlwind:** Energy Recharge increased by 16%.
- **C3 — Sweeping Gust:** Gust Surge level +3 (max 15).
- **C4 — Cherishing Breezes:** Reduces DMG taken by 10% during Palm Vortex.
- **C5 — Vortex Stellaris:** Palm Vortex level +3 (max 15).
- **C6 — Intertwined Winds:** Enemies hit by Gust Surge suffer -20% Anemo RES. If elemental absorption occurred, enemies also suffer -20% RES to the absorbed element.

---

#### Traveler (Geo) — 5-star Geo Sword

**Normal Attack — Foreign Rockblade:** 5-hit combo. Charged Attack: 2 rapid sword strikes.

**Elemental Skill — Starfell Sword (8s CD):** Summons a meteorite from above, dealing AoE Geo DMG on impact. Creates a climbable Geo Construct that blocks attacks. Hold to aim placement. Only one meteorite exists at a time.

**Elemental Burst — Wake of Earth (60 Energy, 15s CD):** Sends a wave of shockwaves forward, launching enemies and dealing AoE Geo DMG. Stone walls erupt at the shockwave edges, forming a barrier of Geo Constructs.

**Passive Talents:**
- **A1 — Shattered Darkrock:** Reduces Starfell Sword CD by 2s.
- **A4 — Frenzied Rockslide:** The final Normal Attack hit triggers a collapse dealing 60% ATK as AoE Geo DMG.

**Constellations (Geo):**
- **C1 — Invincible Stonewall:** Party members within Wake of Earth's barrier gain +10% CRIT Rate and increased resistance to interruption.
- **C2 — Rockcore Meltdown:** When a meteorite is destroyed (by reaching max duration or being replaced), it explodes, dealing additional AoE Geo DMG equal to the skill's damage.
- **C3 — Will of the Rock:** Wake of Earth level +3 (max 15).
- **C4 — Reaction Force:** Each enemy hit by Wake of Earth shockwaves regenerates 5 Energy for the Traveler (max 25 per use).
- **C5 — Meteorite Impact:** Starfell Sword level +3 (max 15).
- **C6 — Everlasting Boulder:** Wake of Earth barrier duration extended by 5s. Meteorite duration extended by 10s.

---

#### Amber — 4-star Pyro Bow

Mondstadt's outrider. One of the three starter characters obtained during the prologue.

**Normal Attack — Sharpshooter:** 5-hit bow combo. Aimed Shot: fully-charged shots deal Pyro DMG.

**Elemental Skill — Explosive Puppet (15s CD):** Deploys Baron Bunny, a dancing decoy that taunts enemies and draws fire. Explodes dealing AoE Pyro DMG when its HP is depleted or after 8 seconds.

**Elemental Burst — Fiery Rain (40 Energy, 12s CD):** Fires a shower of arrows dealing continuous AoE Pyro DMG over a wide area.

**Passive Talents:**
- **A1 — Every Arrow Finds Its Target:** Increases Fiery Rain's CRIT Rate by 10% and widens its AoE by 30%.
- **A4 — Precise Shot:** Aimed Shot hits on weak spots increase ATK by 15% for 10s.
- **Utility — Gliding Champion:** Decreases all party members' gliding Stamina consumption by 20%. Does not stack with other identical passives.

**Constellations:**
- **C1 — One Arrow to Rule Them All:** Fires 2 arrows per Aimed Shot. Second arrow deals 20% of the first arrow's DMG.
- **C2 — Bunny Triggered:** Manual detonation of Baron Bunny via Aimed Shot deals 200% additional DMG.
- **C3 — It Burns!:** Fiery Rain level +3 (max 15).
- **C4 — It's Not Just Any Doll...:** Reduces Explosive Puppet CD by 20% and adds 1 additional charge.
- **C5 — It's Baron Bunny!:** Explosive Puppet level +3 (max 15).
- **C6 — Wildfire:** Fiery Rain increases all party members' Movement SPD by 15% and ATK by 15% for 10s.

---

#### Barbara — 4-star Hydro Catalyst

Mondstadt's deaconess and idol. Dedicated healer. Given free at Adventure Rank 18.

**Normal Attack — Whisper of Water:** 4-hit Hydro splash combo. Charged Attack: AoE Hydro DMG after short casting time.

**Elemental Skill — Let the Show Begin (32s CD, 15s duration):** Creates a Melody Loop orbiting the active character. Normal Attacks heal all party members for a percentage of Barbara's Max HP. Charged Attacks heal 4x the normal amount. Applies Hydro to the active character periodically (can cause self-Freeze with Cryo enemies).

**Elemental Burst — Shining Miracle (80 Energy, 20s CD):** Instantly heals all party members for a massive amount based on Barbara's Max HP.

**Passive Talents:**
- **A1 — Glorious Season:** Stamina consumption of characters within the Melody Loop is reduced by 12%.
- **A4 — Encore:** When a character gains an Elemental Orb/Particle, Let the Show Begin duration is extended by 1s (max 5s extension).
- **Utility — With My Whole Heart:** When cooking a dish with restorative effects perfectly, 12% chance to obtain double the product.

**Constellations:**
- **C1 — Gleeful Songs:** Barbara regenerates 1 Energy every 10s.
- **C2 — Vitality Burst:** Let the Show Begin CD reduced by 15%. Active character gains 15% Hydro DMG Bonus during Melody Loop.
- **C3 — Star of Tomorrow:** Shining Miracle level +3 (max 15).
- **C4 — Attentiveness Be My Power:** Each enemy hit by Barbara's Charged Attack regenerates 1 Energy (max 5 per Charged Attack).
- **C5 — The Purest Companionship:** Let the Show Begin level +3 (max 15).
- **C6 — Dedicating Everything to You:** When Barbara is in the party, a fallen character is automatically revived with full HP. Can only occur once every 15 minutes.

---

#### Beidou — 4-star Electro Claymore

Captain of the Crux Fleet. Counter-based sub-DPS playstyle.

**Normal Attack — Oceanborne:** 5-hit claymore combo. Charged Attack: continuous spinning slashes that drain Stamina, ending with a powerful slash.

**Elemental Skill — Tidecaller (7.5s CD):** Press: swings dealing Electro DMG. Hold: creates a shield (scales with Max HP) that absorbs damage. When released, deals Electro DMG that scales with the number of times Beidou was hit while shielded (max 2 hits for full bonus). Perfect counter: releasing at the exact moment of being hit grants maximum damage instantly.

**Elemental Burst — Stormbreaker (80 Energy, 20s CD, 15s duration):** Reduces incoming DMG and creates lightning discharges that jump between enemies when Normal and Charged Attacks hit, dealing Electro DMG. Lightning can bounce to up to 2 additional targets.

**Passive Talents:**
- **A1 — Retribution:** Counterattacking with Tidecaller at the precise moment grants max DMG bonus.
- **A4 — Lightning Storm:** After triggering a max-damage Tidecaller: Normal and Charged Attack DMG increased by 15%, ATK SPD increased by 15% for 10s.
- **Utility — Conqueror of Tides:** Decreases all party members' swimming Stamina consumption by 20%.

**Constellations:**
- **C1 — Sea Beast's Scourge:** Stormbreaker creates a shield absorbing up to 16% Max HP for 15s.
- **C2 — Upon the Turbulent Sea, the Thunder Arises:** Stormbreaker's lightning discharges can bounce to 2 additional enemies.
- **C3 — Summoner of Storm:** Tidecaller level +3 (max 15).
- **C4 — Stunning Revenge:** Within 10s of taking DMG, Normal Attacks gain 20% additional Electro DMG.
- **C5 — Crimson Tidewalker:** Stormbreaker level +3 (max 15).
- **C6 — Bane of Evil:** Stormbreaker decreases nearby enemies' Electro RES by 15%.

---

#### Bennett — 4-star Pyro Sword

Adventure Team leader of Benny's Adventure Team. Widely considered the strongest support character in the game. Provides ATK buff, healing, and Pyro application.

**Normal Attack — Strike of Fortune:** 5-hit sword combo. Charged Attack: 2 rapid sword strikes.

**Elemental Skill — Passion Overload (5s CD press, 7.5/10s hold):** Press: quick Pyro strike. Hold Level 1: two consecutive Pyro strikes that launch enemies. Hold Level 2: three strikes with an explosion that also launches Bennett (can be mitigated by shields or A4 passive).

**Elemental Burst — Fantastic Voyage (60 Energy, 15s CD, 12s duration):** Bennett leaps and creates an Inspiration Field. Characters inside the field: below 70% HP gain continuous HP regeneration (scales with Bennett's Max HP); above 70% HP gain a flat ATK bonus (scales with Bennett's Base ATK). Applies Pyro to characters inside the field periodically.

**Passive Talents:**
- **A1 — Rekindle:** Decreases Passion Overload CD by 20%.
- **A4 — Fearnaught:** Within Fantastic Voyage's Inspiration Field, Passion Overload CD is reduced by 50% and Bennett cannot be launched by its explosions.
- **Utility — It Should Be Safe...:** Mondstadt expedition time reduced by 25%.

**Constellations:**
- **C1 — Grand Expectation:** Fantastic Voyage's ATK bonus no longer has the 70% HP threshold restriction. Additionally grants 20% of Bennett's Base ATK as additional ATK bonus.
- **C2 — Impasse Conqueror:** When Bennett's HP falls below 70%, Energy Recharge increased by 30%.
- **C3 — Unstoppable Fervor:** Passion Overload level +3 (max 15).
- **C4 — Unexpected Odyssey:** Using Normal Attack within Passion Overload's Hold Level 1 second hit triggers a follow-up attack dealing 135% of the second hit's DMG.
- **C5 — True Explorer:** Fantastic Voyage level +3 (max 15).
- **C6 — Fire Ventures with Me:** Within Fantastic Voyage, Sword/Claymore/Polearm characters gain 15% Pyro DMG Bonus and their Normal/Charged Attacks are infused with Pyro. **Warning:** C6 infusion overrides many elemental infusions, making it detrimental for Physical DPS characters. C6 activation is irreversible in v1.0.

---

#### Chongyun — 4-star Cryo Claymore

Exorcist from Liyue. Cryo infusion support and burst DPS.

**Normal Attack — Demonbane:** 4-hit claymore combo. Charged Attack: continuous spinning slashes ending with powerful slash.

**Elemental Skill — Spirit Blade: Chonghua's Layered Frost (15s CD, field lasts 10s):** Strikes the ground dealing AoE Cryo DMG. Creates a frost field that converts Sword/Claymore/Polearm Normal and Charged Attacks to Cryo DMG for characters standing in the field.

**Elemental Burst — Spirit Blade: Cloud-Parting Star (40 Energy, 12s CD):** Summons 3 giant spirit blades that fall and explode one by one, each dealing AoE Cryo DMG.

**Passive Talents:**
- **A1 — Steady Breathing:** Sword/Claymore/Polearm characters within the frost field gain 8% Normal ATK SPD increase.
- **A4 — Rimechaser Blade:** When the frost field expires, a spirit blade strikes nearby enemies dealing Cryo DMG and reducing their Cryo RES by 10% for 8s.
- **Utility — Gallant Journey:** Liyue expedition time reduced by 25%.

**Constellations:**
- **C1 — Ice Unleashed:** The last Normal Attack hit releases 3 ice blades, each dealing 50% of ATK as Cryo DMG.
- **C2 — Atmospheric Revolution:** Elemental Skills and Bursts cast within the frost field have their CD reduced by 15%.
- **C3 — Cloudburst:** Cloud-Parting Star level +3 (max 15).
- **C4 — Frozen Skies:** Regenerates 1 Energy when hitting enemies affected by Cryo or Frozen (once per 2s).
- **C5 — The True Path:** Chonghua's Layered Frost level +3 (max 15).
- **C6 — Rally of Four Blades:** Cloud-Parting Star deals 15% more DMG to enemies with lower HP percentage than Chongyun. Summons 1 additional spirit blade (4 total).

---

#### Diluc — 5-star Pyro Claymore

Darknight Hero of Mondstadt. Premier Pyro main DPS at launch. Highest base ATK among v1.0 characters (335).

**Normal Attack — Tempered Sword:** 4-hit claymore combo. Charged Attack: continuous spinning slashes draining Stamina.

**Elemental Skill — Searing Onslaught (10s CD, starts after third use):** Performs a forward slash dealing Pyro DMG. Can be used 3 times consecutively within a short window, with each use being a different slash animation. CD begins after the third use (or after a brief delay if fewer than 3 are used).

**Elemental Burst — Dawn (40 Energy, 12s CD):** Releases intense flames knocking back nearby enemies, then summons a Phoenix that flies forward dealing massive Pyro DMG to all enemies in its path. Creates a wave of fire on the ground along the path. After the burst, Diluc's Normal and Charged Attacks are infused with Pyro for a duration.

**Passive Talents:**
- **A1 — Relentless:** Charged Attack Stamina cost decreased by 50% and maximum duration increased by 3s.
- **A4 — Blessing of Phoenix:** The Pyro infusion granted by Dawn lasts 4s longer. During the infusion, Diluc gains 20% Pyro DMG Bonus.
- **Utility — Tradition of the Dawn Knight:** Refunds 15% of ore used when crafting Claymore-type weapons.

**Constellations:**
- **C1 — Conviction:** Diluc deals 15% more DMG to enemies above 50% HP.
- **C2 — Searing Ember:** When Diluc takes DMG, his ATK increases by 10% and ATK SPD by 5% for 10s. Stacks up to 3 times. Can trigger once every 1.5s.
- **C3 — Fire and Steel:** Searing Onslaught level +3 (max 15).
- **C4 — Flowing Flame:** Casting Searing Onslaught in sequence within 2s of each other increases the next cast's DMG by 40%.
- **C5 — Phoenix, Harbinger of Dawn:** Dawn level +3 (max 15).
- **C6 — Flaming Sword, Nemesis of Dark:** After casting Searing Onslaught, the next 2 Normal Attacks within 6s gain 30% increased ATK SPD and 30% increased DMG.

---

#### Fischl — 4-star Electro Bow

Self-proclaimed Prinzessin der Verurteilung. Top-tier off-field Electro applicator through Oz.

**Normal Attack — Bolts of Downfall:** 5-hit bow combo. Aimed Shot: fully-charged shots deal Electro DMG.

**Elemental Skill — Nightrider (25s CD, Oz lasts 10s):** Summons Oz the night raven, dealing AoE Electro DMG on arrival. Oz continuously fires Electro attacks (Freikugel) at nearby enemies. Recasting while Oz is active repositions him.

**Elemental Burst — Midnight Phantasmagoria (60 Energy, 15s CD):** Fischl transforms into Oz, gaining greatly increased Movement SPD and striking nearby enemies with lightning dealing Electro DMG. Each enemy is struck only once. Oz remains on the field after the transformation ends.

**Passive Talents:**
- **A1 — Stellar Predator:** When a fully-charged Aimed Shot hits Oz, Oz fires AoE Electro DMG equal to 152.7% of the arrow's DMG.
- **A4 — Undone Be Thy Sinful Hex:** When a character triggers an Electro-related elemental reaction while Oz is on the field, Oz fires Thundering Retribution dealing Electro DMG equal to 80% of Fischl's ATK.
- **Utility — Mein Hausgarten:** Mondstadt expedition time reduced by 25%.

**Constellations:**
- **C1 — Gaze of the Deep:** Even when Oz is not on the field, Oz fires a joint attack with Normal Attacks dealing 22% of ATK as Electro DMG.
- **C2 — Devourer of All Sins:** Nightrider deals 200% additional ATK DMG on cast with 50% increased AoE.
- **C3 — Wings of Nightmare:** Nightrider level +3 (max 15).
- **C4 — Her Pilgrimage of Bleak:** Midnight Phantasmagoria deals 222% ATK as additional Electro DMG when cast. When the transformation ends, Fischl regenerates 20% HP.
- **C5 — Against the Fleeing Light:** Midnight Phantasmagoria level +3 (max 15).
- **C6 — Evernight Raven:** Oz's active duration extended by 2s. Oz attacks alongside the active character's Normal Attacks, dealing 30% of Fischl's ATK as Electro DMG.

---

#### Jean — 5-star Anemo Sword

Acting Grand Master of the Knights of Favonius. Versatile healer/sub-DPS/crowd-controller. Highest base HP among v1.0 5-star characters (14,695).

**Normal Attack — Favonius Bladework:** 5-hit sword combo. Charged Attack: launches enemy upward using wind power, consuming Stamina.

**Elemental Skill — Gale Blade (6s CD):** Press: releases a miniature storm dealing Anemo DMG and launching enemies. Hold: continuously pulls surrounding enemies toward Jean while dealing sustained Anemo DMG. Released enemies take fall damage from being launched.

**Elemental Burst — Dandelion Breeze (80 Energy, 20s CD, field lasts 10s):** Creates a Dandelion Field that launches surrounding enemies and deals Anemo DMG. Instantly heals all party members for a large amount (scales with Jean's ATK). The field continuously heals characters inside and imbues them with Anemo, while dealing Anemo DMG to enemies entering/exiting.

**Passive Talents:**
- **A1 — Wind Companion:** Normal Attack hits have a 50% chance to regenerate HP for all party members equal to 15% of Jean's ATK.
- **A4 — Let the Wind Lead:** Using Dandelion Breeze regenerates 20% of its Energy cost (16 Energy returned).
- **Utility — Guiding Breeze:** When cooking a dish with restorative effects perfectly, 12% chance to obtain double the product.

**Constellations:**
- **C1 — Spiraling Tempest:** Holding Gale Blade increases pulling speed by 40% and DMG dealt by 40%.
- **C2 — People's Aegis:** When Jean picks up an Elemental Orb/Particle, all party members gain 15% Movement SPD and 15% ATK SPD for 15s.
- **C3 — When the West Wind Arises:** Dandelion Breeze level +3 (max 15).
- **C4 — Lands of Dandelion:** Within the Dandelion Field, all enemies have their Anemo RES decreased by 40%.
- **C5 — Outbursting Gust:** Gale Blade level +3 (max 15).
- **C6 — Lion's Fang, Fair Protector of Mondstadt:** Within the Dandelion Field, incoming DMG is decreased by 35%. This effect persists for 3 attacks or 10s after leaving the field.

---

#### Kaeya — 4-star Cryo Sword

Cavalry Captain of the Knights of Favonius. Starter character. Reliable Cryo sub-DPS with strong energy generation.

**Normal Attack — Ceremonial Bladework:** 5-hit sword combo. Charged Attack: 2 rapid sword strikes.

**Elemental Skill — Frostgnaw (6s CD):** Unleashes a frigid blast dealing Cryo DMG to enemies in front of Kaeya. Generates a significant number of Elemental Particles.

**Elemental Burst — Glacial Waltz (60 Energy, 15s CD, 8s duration):** Summons 3 icicles that revolve around Kaeya, dealing Cryo DMG to enemies they contact. Icicles persist when switching characters.

**Passive Talents:**
- **A1 — Cold-Blooded Strike:** Every hit from Frostgnaw regenerates HP equal to 15% of Kaeya's ATK.
- **A4 — Glacial Heart:** Enemies Frozen by Frostgnaw drop additional Elemental Particles (max 2 additional per use).
- **Utility — Hidden Strength:** Decreases all party members' sprinting Stamina consumption by 20%.

**Constellations:**
- **C1 — Excellent Blood:** Normal and Charged Attacks gain 15% CRIT Rate against enemies affected by Cryo.
- **C2 — Never-Ending Performance:** Glacial Waltz duration extended by 2.5s for each enemy defeated during its duration (max 15s total).
- **C3 — Dance of Frost:** Frostgnaw level +3 (max 15).
- **C4 — Frozen Kiss:** When Kaeya's HP falls below 20%, a shield is automatically generated absorbing 30% of Max HP for 20s with 250% Cryo absorption. Triggers once every 60s.
- **C5 — Frostbiting Embrace:** Glacial Waltz level +3 (max 15).
- **C6 — Glacial Whirlwind:** Glacial Waltz gains 1 additional icicle (4 total). On cast, regenerates 15 Energy.

---

#### Keqing — 5-star Electro Sword

Yuheng of the Liyue Qixing. Fast Electro DPS with high CRIT scaling and teleport mobility.

**Normal Attack — Yunlai Swordsmanship:** 5-hit rapid sword combo. Charged Attack: 2 rapid sword strikes.

**Elemental Skill — Stellar Restoration (7.5s CD):** Throws a Lightning Stiletto, marking the hit point. Recasting or using a Charged Attack triggers one of two effects: recast teleports Keqing to the Stiletto's position (Blink) dealing AoE Electro DMG; Charged Attack performs a slash at the Stiletto's position dealing AoE Electro DMG. After teleporting, Keqing's Normal and Charged Attacks are infused with Electro for 5s (from A1 passive). If the Stiletto is not activated, it explodes after its duration dealing AoE Electro DMG.

**Elemental Burst — Starward Sword (40 Energy, 12s CD):** Keqing unleashes Electro power, dealing an initial AoE Electro slash, followed by a series of thunderclap strikes, and concluding with a massive final AoE Electro slash. Grants CRIT Rate and Energy Recharge buffs for 8s after use (from A4 passive).

**Passive Talents:**
- **A1 — Thundering Penance:** After recasting Stellar Restoration (Blink), Keqing's Normal and Charged Attacks are converted to Electro DMG for 5s.
- **A4 — Aristocratic Dignity:** When casting Starward Sword, Keqing gains 15% CRIT Rate and 15% Energy Recharge for 8s.
- **Utility — Land's Overseer:** Liyue expedition time reduced by 25%.

**Constellations:**
- **C1 — Thundering Might:** Recasting Stellar Restoration (Blink) deals 50% ATK as AoE Electro DMG at both the start and end points.
- **C2 — Keen Extraction:** Normal and Charged Attacks against Electro-affected enemies have a 50% chance to produce an Elemental Particle (once per 5s).
- **C3 — Foreseen Reformation:** Starward Sword level +3 (max 15).
- **C4 — Attunement:** After Keqing triggers an Electro-related reaction, her ATK is increased by 25% for 10s.
- **C5 — Beckoning Stars:** Stellar Restoration level +3 (max 15).
- **C6 — Tenacious Star:** When initiating a Normal Attack, Charged Attack, Elemental Skill, or Elemental Burst, Keqing gains 6% Electro DMG Bonus for 8s. Each trigger source is counted independently (max 24% from 4 sources).

---

#### Klee — 5-star Pyro Catalyst

Spark Knight of Mondstadt. Child prodigy explosives expert. Added October 20, 2020 via Sparkling Steps banner (still v1.0).

**Normal Attack — Kaboom!:** 3-hit combo throwing explosive projectiles dealing AoE Pyro DMG. Charged Attack: deals heavy AoE Pyro DMG (high Stamina cost).

**Elemental Skill — Jumpy Dumpty (20s CD, 2 charges):** Throws a bouncing bomb that bounces 3 times, dealing AoE Pyro DMG on each bounce. After the third bounce, splits into 8 mines that detonate on enemy contact or after a short duration, dealing Pyro DMG.

**Elemental Burst — Sparks 'n' Splash (60 Energy, 15s CD, 10s duration):** Continuously summons sparks that attack nearby enemies dealing AoE Pyro DMG. Deactivates when Klee leaves the field.

**Passive Talents:**
- **A1 — Pounding Surprise:** When Jumpy Dumpty or Normal Attacks deal DMG, Klee has a 50% chance to obtain an Explosive Spark. The next Charged Attack consumes the Spark, costing no Stamina and dealing 50% increased DMG.
- **A4 — Sparkling Burst:** When Klee's Charged Attack results in a CRIT Hit, all party members gain 2 Elemental Energy.
- **Utility — All Of My Treasures!:** Displays Mondstadt-region specialties on the mini-map.

**Constellations:**
- **C1 — Chained Reactions:** Attacks and Skills have a chance to summon a spark that deals DMG equal to 120% of Sparks 'n' Splash's DMG.
- **C2 — Explosive Frags:** Enemies hit by Jumpy Dumpty's mines have their DEF decreased by 23% for 10s.
- **C3 — Exquisite Compound:** Jumpy Dumpty level +3 (max 15).
- **C4 — Sparkly Explosion:** If Klee leaves the field during Sparks 'n' Splash, the departure triggers an explosion dealing 555% of her ATK as AoE Pyro DMG.
- **C5 — Nova Burst:** Sparks 'n' Splash level +3 (max 15).
- **C6 — Blazing Delight:** While Sparks 'n' Splash is active, other party members continuously regenerate Energy. Using Sparks 'n' Splash grants all party members 10% Pyro DMG Bonus for 25s.

---

#### Lisa — 4-star Electro Catalyst

Librarian of the Knights of Favonius. Starter character. Stacking Electro mechanic with DEF shred.

**Normal Attack — Lightning Touch:** 4-hit Electro combo. Charged Attack: AoE Electro DMG after short casting time.

**Elemental Skill — Violet Arc (1s CD press, 16s CD hold):** Press: releases a homing Lightning Orb dealing Electro DMG and applying a Conductive stack (max 3 stacks). Hold: after a long casting animation, calls down lightning dealing massive Electro DMG in AoE. DMG scales dramatically with the number of Conductive stacks on enemies (stacks are consumed).

**Elemental Burst — Lightning Rose (80 Energy, 20s CD, 15s duration):** Summons a Lightning Rose that continuously emits lightning bolts at nearby enemies dealing Electro DMG.

**Passive Talents:**
- **A1 — Induced Aftershock:** Lisa's Charged Attacks apply a Conductive stack to enemies hit.
- **A4 — Static Electricity Field:** Enemies hit by Lightning Rose have their DEF decreased by 15% for 10s.
- **Utility — General Pharmaceutics:** 20% chance to refund a portion of crafting materials when crafting potions.

**Constellations:**
- **C1 — Infinite Circuit:** Hold Violet Arc regenerates 2 Energy for every enemy hit. Maximum 10 Energy per use.
- **C2 — Electromagnetic Field:** Holding Violet Arc increases Lisa's DEF by 25% and resistance to interruption.
- **C3 — Resonant Thunder:** Lightning Rose level +3 (max 15).
- **C4 — Plasma Eruption:** Lightning Rose releases 1-3 additional lightning bolts per attack.
- **C5 — Electrocute:** Violet Arc level +3 (max 15).
- **C6 — Pulsating Witch:** When Lisa enters the field, she applies 3 Conductive stacks to nearby enemies (once every 5s).

---

#### Mona — 5-star Hydro Catalyst

Astrologer from Mondstadt. Powerful damage amplifier and Hydro applicator with unique sprint.

**Normal Attack — Ripple of Fate:** 4-hit Hydro combo. Charged Attack: AoE Hydro DMG after short casting time.

**Alternate Sprint — Illusory Torrent:** Instead of a normal sprint, Mona flows into a torrent of water, gaining increased Movement SPD and applying Hydro to nearby enemies. Cannot attack during sprint.

**Elemental Skill — Mirror Reflection of Doom (12s CD):** Creates a Phantom of Fate that continuously taunts enemies and deals Hydro DMG. When its duration expires or HP is depleted, the Phantom explodes dealing AoE Hydro DMG. Hold: Mona dashes backward before summoning the Phantom.

**Elemental Burst — Stellaris Phantasm (60 Energy, 15s CD):** Creates a reflection of the starry sky, applying Illusory Bubble to enemies in a large AoE. Trapped enemies cannot move. When the bubble is popped by an attack, it deals Hydro DMG and applies the Omen debuff, which increases DMG taken by the enemy for its duration.

**Passive Talents:**
- **A1 — Come 'n' Get Me, Hag!:** After using Illusory Torrent for 2s, a Phantom is automatically created, dealing 50% of Mirror Reflection of Doom's explosion DMG.
- **A4 — Waterborne Destiny:** Increases Mona's Hydro DMG Bonus by 20% of her Energy Recharge rate.
- **Utility — Principium of Astrology:** 25% chance to refund a portion of materials when crafting Weapon Ascension Materials.

**Constellations:**
- **C1 — Prophecy of Submersion:** For 8s after hitting an Omen-affected enemy with a Hydro-related reaction, the following reaction effects are enhanced: Electro-Charged DMG +15%, Vaporize DMG +15%, Hydro Swirl DMG +15%, Freeze duration +15%.
- **C2 — Lunar Chain:** Normal Attacks have a 20% chance to automatically trigger a Charged Attack (once per 5s).
- **C3 — Restless Revolution:** Stellaris Phantasm level +3 (max 15).
- **C4 — Prophecy of Oblivion:** All party members gain 15% CRIT Rate against enemies affected by Omen.
- **C5 — Mockery of Fortuna:** Mirror Reflection of Doom level +3 (max 15).
- **C6 — Rhetorics of Calamitas:** After entering Illusory Torrent, the next Charged Attack gains 60% DMG bonus per second of movement (max 180% bonus). Effect lasts up to 8s.

---

#### Ningguang — 4-star Geo Catalyst

Tianquan of the Liyue Qixing. Geo main DPS with screen mechanic and Star Jade system.

**Normal Attack — Sparkling Scatter:** Fires a gem dealing Geo DMG. Each hit grants 1 Star Jade (max 3). Charged Attack: fires a large gem dealing Geo DMG; if Ningguang possesses Star Jades, they are all launched alongside the Charged Attack dealing additional Geo DMG. **Unique:** Ningguang's Normal Attacks track enemies and have no travel-time limitation.

**Elemental Skill — Jade Screen (12s CD):** Creates a Jade Screen dealing AoE Geo DMG on placement. The screen blocks enemy projectiles. Screen HP scales with Ningguang's Max HP. Only one screen can exist at a time.

**Elemental Burst — Starshatter (40 Energy, 12s CD):** Fires a barrage of homing gem projectiles dealing massive Geo DMG. If a Jade Screen is present when cast, the screen fires additional gem projectiles before shattering.

**Passive Talents:**
- **A1 — Backup Plan:** When Ningguang possesses Star Jades, her Charged Attacks do not consume Stamina.
- **A4 — Strategic Reserve:** Characters passing through the Jade Screen gain 12% Geo DMG Bonus for 10s.
- **Utility — Trove of Marvelous Treasures:** Displays nearby ore vein locations on the mini-map.

**Constellations:**
- **C1 — Piercing Fragments:** Normal Attacks deal AoE DMG (small splash radius on hit).
- **C2 — Shock Effect:** When Jade Screen is shattered, its CD resets (once per 6s).
- **C3 — Majesty Be the Array of Stars:** Starshatter level +3 (max 15).
- **C4 — Exquisite Be the Jade, Outshining All Beneath:** Characters near the Jade Screen gain 10% Elemental RES to all elements.
- **C5 — Invincible Be the Jade Screen:** Jade Screen level +3 (max 15).
- **C6 — Grandeur Be the Seven Stars:** When Starshatter is used, Ningguang gains 7 Star Jades.

---

#### Noelle — 4-star Geo Claymore

Maid of the Knights of Favonius. Shielding, healing, and Geo DPS hybrid. Guaranteed on first Beginner's Wish 10-pull. Self-sufficient character that scales with DEF.

**Normal Attack — Favonius Bladework - Maid:** 4-hit claymore combo. Charged Attack: continuous spinning slashes draining Stamina.

**Elemental Skill — Breastplate (24s CD, 12s duration):** Creates a shield dealing Geo DMG to nearby enemies. Shield absorption scales with DEF. While the shield is active, Normal and Charged Attacks have a chance to heal all party members (scales with Noelle's DEF). Shield has 250% Geo absorption efficiency.

**Elemental Burst — Sweeping Time (60 Energy, 15s CD, 15s duration):** Deals AoE Geo DMG to surrounding enemies. Noelle's attack range is greatly expanded and her attacks are converted to Geo DMG. Gains ATK bonus based on DEF for the duration.

**Passive Talents:**
- **A1 — Devotion:** When the active character's HP falls below 30%, a shield is automatically created absorbing 400% of Noelle's DEF for 20s (triggers once every 60s). Works even when Noelle is off-field.
- **A4 — Nice and Clean:** Every 4 Normal or Charged Attack hits decrease Breastplate's CD by 1s.
- **Utility — Maid's Knighthood:** When cooking a DEF-boosting dish perfectly, 12% chance to obtain double the product.

**Constellations:**
- **C1 — I Got Your Back:** When both Breastplate and Sweeping Time are active, healing activation chance is increased to 100%.
- **C2 — Combat Maid:** Charged Attack Stamina consumption decreased by 20%, Charged Attack DMG increased by 15%.
- **C3 — Invulnerable Maid:** Breastplate level +3 (max 15).
- **C4 — To Be Cleaned:** When Breastplate ends or is destroyed, it deals 400% ATK as Geo DMG to nearby enemies.
- **C5 — Favonius Sweeper Master:** Sweeping Time level +3 (max 15).
- **C6 — Must Be Spotless:** Sweeping Time's ATK bonus from DEF is increased by an additional 50%. Defeating enemies during Sweeping Time extends its duration by 1s (max 10s extension).

---

#### Qiqi — 5-star Cryo Sword

Zombie pharmacist from Liyue. Strongest single-target healer. Highest base DEF among v1.0 characters (922).

**Normal Attack — Ancient Sword Art:** 5-hit rapid sword combo. Charged Attack: 2 rapid sword strikes.

**Elemental Skill — Adeptus Art: Herald of Frost (30s CD, 15s duration):** Summons the Herald of Frost dealing Cryo DMG on cast. Herald orbits the active character: when attacking enemies, heals all party members (scales with Qiqi's ATK). Herald also periodically deals Cryo DMG to nearby enemies.

**Elemental Burst — Adeptus Art: Preserver of Fortune (80 Energy, 20s CD):** Deals AoE Cryo DMG and marks nearby enemies with a Fortune-Preserving Talisman. When marked enemies take DMG from any character, that character regenerates HP (scales with Qiqi's ATK).

**Passive Talents:**
- **A1 — Life-Prolonging Methods:** When Qiqi triggers an Elemental Reaction, incoming healing bonus for the triggering character is increased by 20% for 8s.
- **A4 — A Glimpse into Arcanum:** When Qiqi hits enemies with Normal or Charged Attacks, there is a 50% chance to apply a Fortune-Preserving Talisman to the enemy for 6s (once per 30s).
- **Utility — Former Life Memories:** Displays Liyue-region specialties on the mini-map.

**Constellations:**
- **C1 — Ascetics of Frost:** When Herald of Frost hits enemies marked with a Fortune-Preserving Talisman, Qiqi regenerates 2 Energy.
- **C2 — Frozen to the Bone:** Qiqi's Normal and Charged Attacks deal 15% additional DMG against enemies affected by Cryo.
- **C3 — Ascendant Praise:** Herald of Frost level +3 (max 15).
- **C4 — Divine Suppression:** Enemies marked with a Fortune-Preserving Talisman have their ATK decreased by 20%.
- **C5 — Crimson Lotus Bloom:** Preserver of Fortune level +3 (max 15).
- **C6 — Rite of Resurrection:** Using Preserver of Fortune revives all fallen nearby party members and restores 50% of their HP. Can only occur once every 15 minutes.

---

#### Razor — 4-star Electro Claymore

Wolf boy of Wolvendom. Physical DPS carry with Electro Burst enhancement.

**Normal Attack — Steel Fang:** 4-hit claymore combo. Charged Attack: continuous spinning slashes draining Stamina.

**Elemental Skill — Claw and Thunder (6s press, 10s hold):** Press: swings claws dealing Electro DMG. Grants an Electro Sigil (max 3) that increases Energy Recharge. Hold: gathers lightning dealing massive Electro DMG in AoE. All Electro Sigils are consumed, each converting to Energy.

**Elemental Burst — Lightning Fang (80 Energy, 20s CD, 15s duration):** Summons a spectral Wolf companion dealing AoE Electro DMG on cast. During the burst: Normal ATK SPD increased, Electro RES increased, resistance to Electro-Charged DMG. The Wolf strikes alongside Razor's Normal Attacks dealing Electro DMG. Charged Attacks are disabled during burst. Switching characters ends the burst.

**Passive Talents:**
- **A1 — Awakening:** Decreases Claw and Thunder CD by 18%. Using Lightning Fang resets Claw and Thunder's CD.
- **A4 — Hunger:** When Razor's Energy is below 50%, Energy Recharge is increased by 30%.
- **Utility — Wolvensprint:** Decreases all party members' sprinting Stamina consumption by 20%.

**Constellations:**
- **C1 — Wolf's Instinct:** Picking up an Elemental Orb/Particle increases DMG dealt by 10% for 8s.
- **C2 — Suppression:** Increases CRIT Rate by 10% against enemies below 30% HP.
- **C3 — Soul Companion:** Lightning Fang level +3 (max 15).
- **C4 — Bite:** Claw and Thunder (press) decreases enemy DEF by 15% for 7s.
- **C5 — Sharpened Claws:** Claw and Thunder level +3 (max 15).
- **C6 — Lupus Fulguris:** Every 10s, Razor's next Normal Attack triggers a lightning strike dealing 100% ATK as Electro DMG. When not using Lightning Fang, a successful lightning strike grants an Electro Sigil.

---

#### Sucrose — 4-star Anemo Catalyst

Alchemist of Mondstadt. Elemental Mastery buffer and crowd-controller. Key enabler for reaction-based teams.

**Normal Attack — Wind Spirit Creation:** 4-hit Anemo combo. Charged Attack: AoE Anemo DMG after casting time.

**Elemental Skill — Astable Anemohypostasis Creation - 6308 (15s CD):** Creates a small Wind Spirit that deals AoE Anemo DMG, pulling enemies toward the center and launching them.

**Elemental Burst — Forbidden Creation - Isomer 75 / Type II (80 Energy, 20s CD, 6s duration):** Throws an unstable concoction creating a Large Wind Spirit that continuously pulls and launches nearby enemies, dealing AoE Anemo DMG. Can absorb one element (Hydro/Pyro/Cryo/Electro) for additional elemental DMG.

**Passive Talents:**
- **A1 — Catalyst Conversion:** When Sucrose triggers a Swirl reaction, all party members with the corresponding element gain 50 Elemental Mastery for 8s.
- **A4 — Mollis Favonius:** When Sucrose hits enemies with her Elemental Skill or Burst, all other party members gain 20% of Sucrose's Elemental Mastery for 8s.
- **Utility — Astable Invention:** 10% chance to obtain double the product when crafting Character and Weapon Enhancement Materials.

**Constellations:**
- **C1 — Clustered Vacuum Field:** Astable Anemohypostasis Creation gains 1 additional charge.
- **C2 — Beth: Unbound Form:** Forbidden Creation - Isomer 75 duration extended by 2s.
- **C3 — Flawless Alchemistress:** Astable Anemohypostasis Creation level +3 (max 15).
- **C4 — Alchemania:** Every 7 Normal or Charged Attack hits, Sucrose's Elemental Skill CD is reduced by 1-7s (random).
- **C5 — Caution: Standard Flask:** Forbidden Creation - Isomer 75 level +3 (max 15).
- **C6 — Chaotic Entropy:** If Forbidden Creation - Isomer 75 absorbs an element, all party members gain 20% DMG Bonus for the corresponding element for the burst's duration.

---

#### Venti — 5-star Anemo Bow

The Anemo Archon Barbatos. The premier crowd-control character at launch. First limited 5-star banner character (Ballad in Goblets, September 28 - October 18, 2020).

**Normal Attack — Divine Marksmanship:** 6-hit bow combo. Aimed Shot: fully-charged shots deal Anemo DMG.

**Elemental Skill — Skyward Sonnet (6s press, 15s hold):** Press: summons a Wind Domain at the enemy's location, dealing AoE Anemo DMG and launching enemies upward. Creates a small upward wind current. Hold: Venti rises into the air, creating a much larger Wind Domain dealing AoE Anemo DMG and launching enemies higher.

**Elemental Burst — Wind's Grand Ode (60 Energy, 15s CD, 8s duration):** Fires a wind arrow creating a massive Stormeye (vacuum vortex) that continuously pulls in enemies and objects, dealing Anemo DMG. Can absorb one element (Hydro/Pyro/Cryo/Electro) for additional elemental DMG and reactions.

**Passive Talents:**
- **A1 — Embrace of Winds:** Holding Skyward Sonnet creates an upward wind current that lasts 20s, enabling gliding exploration.
- **A4 — Stormeye:** After Wind's Grand Ode ends, regenerates 15 Energy for Venti. If elemental absorption occurred, also regenerates 15 Energy for all characters matching the absorbed element.
- **Utility — Windrider:** Decreases all party members' gliding Stamina consumption by 20%.

**Constellations:**
- **C1 — Splitting Gales:** Fires 2 additional arrows per Aimed Shot. Each additional arrow deals 33% of the original arrow's DMG.
- **C2 — Breeze of Reminiscence:** Skyward Sonnet decreases nearby enemies' Anemo RES by 12% for 10s. Enemies launched by Skyward Sonnet suffer an additional 12% Anemo RES and Physical RES decrease while airborne.
- **C3 — Ode to Thousand Winds:** Wind's Grand Ode level +3 (max 15).
- **C4 — Hurricane of Freedom:** When Venti picks up an Elemental Orb/Particle, he gains 25% Anemo DMG Bonus for 10s.
- **C5 — Concerto dal Cielo:** Skyward Sonnet level +3 (max 15).
- **C6 — Storm of Defiance:** Enemies hit by Wind's Grand Ode have their Anemo RES decreased by 20%. If elemental absorption occurred, enemies also have their RES to the absorbed element decreased by 20%.

---

#### Xiangling — 4-star Pyro Polearm

Chef from Liyue. Given free for completing Spiral Abyss Floor 3-3. One of the strongest off-field Pyro DPS characters in the game, particularly with her Elemental Burst.

**Normal Attack — Dough-Fu:** 5-hit polearm combo. Charged Attack: lunging thrust consuming Stamina.

**Elemental Skill — Guoba Attack (12s CD, Guoba lasts ~7s):** Summons Guoba the panda who breathes fire at nearby enemies dealing AoE Pyro DMG in 4 waves.

**Elemental Burst — Pyronado (80 Energy, 20s CD, 10s duration):** Sends a Pyronado spinning around the active character, dealing Pyro DMG to all enemies in its path. Pyronado follows the active character when switching and deals DMG independently.

**Passive Talents:**
- **A1 — Crossfire:** Increases Guoba's flame range by 20%.
- **A4 — Beware, It's Super Hot!:** When Guoba's effect ends, it drops a chili pepper. Picking it up grants 10% ATK increase for 10s.
- **Utility — Chef de Cuisine:** When cooking an ATK-boosting dish perfectly, 12% chance to obtain double the product.

**Constellations:**
- **C1 — Crispy Outside, Tender Inside:** Enemies hit by Guoba's flames suffer 15% Pyro RES reduction for 6s.
- **C2 — Oil Meets Fire:** The last Normal Attack hit triggers an Implode explosion dealing 75% ATK as AoE Pyro DMG.
- **C3 — Deepfry:** Pyronado level +3 (max 15).
- **C4 — Slowbake:** Pyronado duration increased by 40% (14s total).
- **C5 — Guoba Mad:** Guoba Attack level +3 (max 15).
- **C6 — Condensed Pyronado:** All party members gain 15% Pyro DMG Bonus during Pyronado's duration.

---

#### Xingqiu — 4-star Hydro Sword

Young lord of the Feiyun Commerce Guild. Premier Hydro sub-DPS and enabler for Vaporize/Freeze teams. One of the most impactful 4-star characters.

**Normal Attack — Guhua Style:** 5-hit sword combo. Charged Attack: 2 rapid sword strikes.

**Elemental Skill — Guhua Sword: Fatal Rainscreen (21s CD):** Performs 2 rapid Hydro slashes and creates Rain Swords that orbit the active character. Rain Swords reduce incoming DMG (reduction scales with Xingqiu's Hydro DMG Bonus, capped at 24% additional reduction). Applies Hydro to the active character on hit.

**Elemental Burst — Guhua Sword: Raincutter (80 Energy, 20s CD, 15s duration):** Activates Rainbow Bladework: the active character's Normal Attacks trigger waves of sword rain dealing Hydro DMG. Maximum Rain Swords are maintained throughout the duration. Persists when switching characters.

**Passive Talents:**
- **A1 — Hydropathic:** When a Rain Sword shatters or expires, the active character regenerates HP equal to 6% of Xingqiu's Max HP.
- **A4 — Blades Amidst Raindrops:** Xingqiu gains 20% Hydro DMG Bonus.
- **Utility — Flash of Genius:** 25% chance to refund a portion of Talent Level-Up Materials when crafting.

**Constellations:**
- **C1 — The Scent Remained:** Maximum Rain Swords increased by 1.
- **C2 — Rainbow Upon the Azure Sky:** Raincutter duration extended by 3s. Enemies hit by sword rain suffer 15% Hydro RES reduction for 4s.
- **C3 — Weaver of Verses:** Raincutter level +3 (max 15).
- **C4 — Evilsoother:** During Raincutter, Fatal Rainscreen's DMG is increased by 50%.
- **C5 — Embrace of Rain:** Fatal Rainscreen level +3 (max 15).
- **C6 — Hence, Call Them My Own Verses:** Raincutter's sword rain pattern is enhanced: every third wave deals greatly increased DMG and restores 3 Energy per enemy hit.

---

## 20. Weapons & Equipment

### 20.1 Weapon Types

Five weapon types, each with distinct attack patterns and charged attack behavior (see §1.6):

| Type | Characters (v1.0) | Attack Style |
|------|-------------------|-------------|
| **Sword** | Traveler, Bennett, Jean, Kaeya, Keqing, Qiqi, Xingqiu | Fast multi-hit combos. Charged: rapid forward slashes (20 stamina). |
| **Claymore** | Beidou, Chongyun, Diluc, Noelle, Razor | Slow, heavy hits. Blunt damage (triggers Shatter, breaks Geo shields). Charged: continuous spin (40 stamina/sec). |
| **Polearm** | Xiangling | Fast, long-reach thrusts. Charged: forward thrust (25 stamina). |
| **Bow** | Amber, Fischl, Venti | Ranged normal attacks. Aimed Shot: Lv1 = Physical, Lv2 = Elemental (0 stamina). Weak-point hits stun certain enemies. |
| **Catalyst** | Barbara, Klee, Lisa, Mona, Ningguang, Sucrose | All attacks deal elemental damage (matches character's element). Charged: enhanced projectile (50 stamina). |

### 20.2 Weapon Rarity & Level Cap

| Rarity | Max Level | Max Ascension Phase | Refinement |
|--------|----------|-------------------|-----------|
| 1-star | 70 | Phase 4 | No |
| 2-star | 70 | Phase 4 | No |
| 3-star | 90 | Phase 6 | R1–R5 |
| 4-star | 90 | Phase 6 | R1–R5 |
| 5-star | 90 | Phase 6 | R1–R5 |

Weapons ascend at levels 20, 40, 50, 60, 70, and 80. Each ascension requires Mora, weapon ascension materials (domain drops), and enemy drops.

### 20.3 Weapon Enhancement

| Enhancement Ore | EXP Provided |
|----------------|-------------|
| Enhancement Ore (1-star) | 400 |
| Fine Enhancement Ore (2-star) | 2,000 |
| Mystic Enhancement Ore (3-star) | 10,000 |

Enhancement costs 1 Mora per 10 EXP. Feeding weapons as fodder provides EXP based on rarity. Enhanced weapons return 80% of invested EXP as fodder.

**Total EXP to Level 90:**

| Rarity | Total EXP | Approximate Mora |
|--------|----------|-----------------|
| 5-star | 9,064,450 | ~906,480 |
| 4-star | 6,042,650 | ~604,260 |
| 3-star | 3,988,200 | ~398,820 |

### 20.4 Weapon Refinement

Weapons 3-star and above can be refined R1–R5 using duplicate copies. Each refinement rank increases passive effect values (typically +25% of R1 value per rank). If a weapon at R2+ is used as fodder, ranks combine (R2 + R2 = R4). Excess refinement beyond R5 is lost.

### 20.5 v1.0 Weapon Count

| Type | 5★ | 4★ | 3★ | 2★ | 1★ | Total |
|------|---:|---:|---:|---:|---:|------:|
| Sword | 2 | 10 | 6 | 1 | 1 | 20 |
| Claymore | 2 | 8 | 5 | 1 | 1 | 17 |
| Polearm | 2 | 7 | 3 | 1 | 1 | 14 |
| Bow | 2 | 9 | 5 | 1 | 1 | 18 |
| Catalyst | 2 | 8 | 5 | 1 | 1 | 17 |
| **Total** | **10** | **42** | **24** | **5** | **5** | **86** |

### 20.6 Notable 5-Star Weapons

| Weapon | Type | Base ATK (Lv90) | Sub Stat | Passive |
|--------|------|----------------|----------|---------|
| Aquila Favonia | Sword | 674 | Phys DMG 41.3% | ATK +20%. On hit taken: heal 100% ATK + 200% ATK AoE DMG (15s CD) |
| Skyward Blade | Sword | 608 | ER 55.1% | CRIT Rate +4%. Post-burst: +10% Move SPD, +10% ATK SPD, +20% Normal/Charged DMG for 12s |
| Wolf's Gravestone | Claymore | 608 | ATK% 49.6% | ATK +20%. Hitting enemies <30% HP: party ATK +40% for 12s (30s CD) |
| Skyward Pride | Claymore | 674 | ER 36.8% | All DMG +8%. Post-burst: Normal/Charged hits create vacuum blades (80% Phys DMG, 8 blades) |
| Primordial Jade Winged-Spear | Polearm | 674 | CRIT Rate 22.1% | On hit: ATK +3.2% for 6s, max 7 stacks. Full stacks: DMG +12% |
| Skyward Spine | Polearm | 674 | ER 36.8% | CRIT Rate +8%, Normal ATK SPD +12%. 50% chance vacuum blade (40% ATK, 2s CD) |
| Amos' Bow | Bow | 608 | ATK% 49.6% | Normal/Aimed DMG +12%. Arrow DMG +8% per 0.1s flight time (max 5 stacks) |
| Skyward Harp | Bow | 674 | CRIT Rate 22.1% | CRIT DMG +20%. 60% chance AoE physical DMG = 100% ATK (4s CD) |
| Lost Prayer to the Sacred Winds | Catalyst | 608 | CRIT Rate 33.1% | Move SPD +10%. In combat: +8% Elemental DMG every 4s, max 4 stacks |
| Skyward Atlas | Catalyst | 674 | ATK% 33.1% | Elemental DMG +12%. 50% chance cloud ally dealing 160% ATK DMG for 15s (30s CD) |

### 20.7 4-Star Weapon Sources

| Source | Examples |
|--------|---------|
| **Gacha** | Sacrificial series (Sword/Greatsword/Fragments/Bow), Favonius series (all 5 types), The Flute, Lion's Roar, Rainslasher, The Bell, Dragon's Bane, Rust, The Stringless, The Widsith, Eye of Perception |
| **Crafting** (Northlander Billet + materials) | Prototype Rancour, Iron Sting (Sword); Prototype Archaic, Whiteblind (Claymore); Crescent Pike, Prototype Starglitter (Polearm); Prototype Crescent, Compound Bow (Bow); Mappa Mare, Prototype Amber (Catalyst) |
| **Battle Pass** (Lv30 Gnostic Hymn) | The Black Sword, Serpent Spine, Deathmatch, The Viridescent Hunt, Solar Pearl |
| **Starglitter Exchange** (Paimon's Bargains) | Royal series (ATK% sub), Blackcliff series (CRIT DMG sub) — rotate monthly |
| **PS4 Exclusive** | Sword of Descension |

Full weapon data tables with all passives and stats are in the companion doc [docs/weapons.md](docs/weapons.md).

---

## 21. Artifact System

The artifact system is Genshin Impact's primary endgame gear progression. Artifacts are equippable stat items that provide both raw stats (via main stat and sub-stats) and set bonuses (via 2-piece and 4-piece effects). Full data tables are in the companion doc [docs/artifacts.md](docs/artifacts.md).

### 21.1 Artifact Slots

Every character has five artifact slots, each with a fixed name and fixed or variable main stat:

| Slot | Main Stat | Notes |
|------|-----------|-------|
| **Flower of Life** | Flat HP (always) | Only slot with a guaranteed main stat |
| **Plume of Death** | Flat ATK (always) | Only slot with a guaranteed main stat |
| **Sands of Eon** | HP%, ATK%, DEF%, EM, ER% | Variable — one random main stat per piece |
| **Goblet of Eonothem** | HP%, ATK%, DEF%, EM, 7 Elemental DMG%, Physical DMG% | Variable — 13 possible main stats |
| **Circlet of Logos** | HP%, ATK%, DEF%, EM, CRIT Rate%, CRIT DMG%, Healing Bonus% | Variable — 7 possible main stats |

### 21.2 Artifact Rarity

| Rarity | Max Level | Starting Sub-Stats | Sub-Stat Upgrade Points | Obtainable From |
|--------|-----------|-------------------|------------------------|-----------------|
| 1-Star | +4 | 0 | +4 | Overworld investigation spots |
| 2-Star | +4 | 0-1 | +4 | Overworld investigation spots |
| 3-Star | +12 | 1-2 | +4, +8, +12 | Overworld, chests, domains (low AR) |
| 4-Star | +16 | 2-3 | +4, +8, +12, +16 | Domains, bosses, chests |
| 5-Star | +20 | 3-4 | +4, +8, +12, +16, +20 | AR 45+ domains (guaranteed), weekly bosses, elite bosses |

### 21.3 Sub-Stats

Every artifact can have up to 4 sub-stats. Sub-stats are drawn from a pool of 10: Flat HP, Flat ATK, Flat DEF, HP%, ATK%, DEF%, Elemental Mastery, Energy Recharge%, CRIT Rate%, CRIT DMG%.

**Constraints:**
- A sub-stat cannot duplicate the artifact's main stat (e.g., an ATK% Sands cannot roll ATK% as a sub-stat).
- No duplicate sub-stats on a single artifact.

At each upgrade milestone (+4, +8, +12, +16, +20 for 5-star), one sub-stat event occurs:
- If the artifact has fewer than 4 sub-stats, a **new** sub-stat is added.
- If the artifact already has 4 sub-stats, one **existing** sub-stat is randomly selected and increased by a random roll value.

Each sub-stat roll selects from 4 possible tiers (approximately 70%/80%/90%/100% of max). See [docs/artifacts.md](docs/artifacts.md) for the complete roll value table.

### 21.4 5-Star Main Stat Values at +20

| Main Stat | Value at +20 |
|-----------|-------------|
| Flat HP (Flower) | 4,780 |
| Flat ATK (Plume) | 311 |
| HP% | 46.6% |
| ATK% | 46.6% |
| DEF% | 58.3% |
| Elemental Mastery | 187 |
| Energy Recharge% | 51.8% |
| Elemental DMG Bonus% | 46.6% |
| Physical DMG Bonus% | 58.3% |
| CRIT Rate% | 31.1% |
| CRIT DMG% | 62.2% |
| Healing Bonus% | 35.9% |

### 21.5 Leveling & EXP

- Artifacts gain EXP by consuming other artifacts or by using Sanctifying Unction/Enhancement Ore (artifact-specific).
- **1 EXP = 1 Mora** in enhancement cost.
- Total EXP for a 5-star artifact from +0 to +20: **270,475** EXP (and 270,475 Mora).
- Total for a full 5-piece set: **1,352,375** EXP and Mora.
- When an enhanced artifact is consumed as fodder, **80%** of its previously invested EXP is refunded (the refunded portion costs no additional Mora).
- There is a random chance for 2x EXP (~10%) or 5x EXP (~1%) bonus during enhancement.
- See [docs/artifacts.md](docs/artifacts.md) for the per-level EXP table.

### 21.6 v1.0 Artifact Sets

v1.0 launched with the following artifact set categories:

**12 endgame sets** (available as 4-star and 5-star):
- **Boss-only world drops:** Gladiator's Finale, Wanderer's Troupe
- **Clear Pool and Mountain Cavern:** Noblesse Oblige, Bloodstained Chivalry
- **Valley of Remembrance:** Viridescent Venerer, Maiden Beloved
- **Hidden Palace of Zhou Formula:** Crimson Witch of Flames, Lavawalker
- **Midsummer Courtyard:** Thundering Fury, Thundersoother
- **Domain of Guyun:** Archaic Petra, Retracing Bolide

**9 mid-game sets** (max 4-star): Resolution of Sojourner, Tiny Miracle, Berserker, Instructor, The Exile, Defender's Will, Martial Artist, Gambler, Scholar

**3 early-game sets** (max 3-star): Adventurer, Lucky Dog, Traveling Doctor

**4 one-piece sets** (Prayers series, circlet-only, max 4-star): Prayers for Illumination (Pyro), Prayers for Destiny (Hydro), Prayers for Wisdom (Electro), Prayers to Springtime (Cryo) -- each reduces the corresponding element's application duration on the wearer by 40%.

See [docs/artifacts.md](docs/artifacts.md) for all set bonuses, drop weights, and domain pairings.

### 21.7 Artifact Sources

See §6.4 for the full domain-to-set mapping table.

Gladiator's Finale and Wanderer's Troupe do **not** drop from artifact domains. They are exclusive to Elite Bosses (Hypostases, Regisvines, Oceanid) and Weekly Bosses (Stormterror, Andrius). 5-star artifacts are guaranteed only from AR 45+ domain difficulty tiers.

---

## 22. Cooking System

The cooking system allows players to prepare food at cooking stations (stoves found in cities, camps, and near Statues of the Seven). Food provides healing, revival, stat buffs, and stamina benefits. Full food data tables are in the companion doc [docs/cooking.md](docs/cooking.md).

### 22.1 How Cooking Works

1. **Interact with a stove** to open the cooking menu.
2. **Select a recipe** (must have been learned first; some are known from the start, others are obtained from chests, quests, NPC vendors, or reputation rewards).
3. **Manual cooking mini-game:** A circular indicator sweeps around a ring with zones:
   - **Outer/fail zone:** Produces Suspicious quality (weakest effects).
   - **Yellow zone:** Produces Normal quality (standard effects).
   - **Orange/narrow zone:** Produces Delicious quality (strongest effects).
4. **Proficiency system:** Each manual cook of a recipe adds proficiency. Once a recipe reaches maximum proficiency, **Auto-Cook** is unlocked for that recipe. Auto-Cook always produces Normal quality (no chance of Suspicious or Delicious).

### 22.2 Food Quality

| Quality | Result | Icon Border |
|---------|--------|-------------|
| **Suspicious** | Weakest stat values. Can only be produced by failing the manual cook. | Dark |
| **Normal** | Standard values. Produced by hitting the yellow zone or via Auto-Cook. | Standard |
| **Delicious** | Strongest stat values. Produced by hitting the orange/perfect zone. | Gold shimmer |

### 22.3 Buff Categories & Stacking

Food buffs are grouped into **four categories**: ATK-Boosting, DEF-Boosting, Stamina, and Elemental. Only **one food buff per category** can be active at a time. Using a new food from the same category replaces the previous buff (even if weaker).

Players can have up to **4 simultaneous food buffs** (one from each category).

**Healing and revival foods** are instant-use, do not provide ongoing buffs, and do not occupy a buff slot.

Revival foods have a **120-second cooldown per character** (the same character cannot be revived twice within 120s).

### 22.4 Special Character Dishes

When a character with a matching specialty cooks the base dish and achieves Delicious quality, there is a chance to produce a **Special Dish** instead. Special Dishes have unique names, descriptions, and effects that are equal to or stronger than the Delicious version.

**v1.0 characters with Special Dishes:** Amber, Kaeya, Lisa, Barbara, Diluc, Jean, Venti, Fischl, Sucrose, Mona, Chongyun, Xingqiu, Xiangling, Keqing, Qiqi, Ningguang, Beidou, Razor, Bennett, Noelle, Klee.

**Characters without Special Dishes:** Traveler (Lumine/Aether) -- no special dish at any version.

See [docs/cooking.md](docs/cooking.md) for the complete character-to-dish mapping and all food recipe effects.

### 22.5 Key v1.0 Foods by Category

**Best ATK-Boosting (Delicious):**
- Adeptus' Temptation (5-Star): +372 ATK, +12% CRIT Rate, 300s
- Jade Parcels (4-Star): +320 ATK, +10% CRIT Rate, 300s
- "Pile 'Em Up" (4-Star): +20% CRIT Rate, 300s

**Best DEF-Boosting (Delicious):**
- Golden Crab (4-Star): +308 DEF, +10% Healing Effects, 300s
- Moon Pie (4-Star): +35% Shield Strength, +235 DEF, 300s

**Best Healing (Delicious):**
- Universal Peace / Squirrel Fish (4-Star): 34% Max HP + regen
- Mondstadt Hash Brown (3-Star): 34% Max HP + 1,900 flat HP

**Best Revival (Delicious):**
- Crab, Ham & Veggie Bake (4-Star): Revive + 20% Max HP + 150 HP

**Best Stamina (Delicious):**
- Barbatos Ratatouille (3-Star): -25% glide + sprint stamina, 900s
- Sticky Honey Roast (3-Star): -25% climb + sprint stamina, 900s

---

## 23. Crafting & Alchemy System

Alchemy is performed at **Crafting Benches** located in Mondstadt (next to the blacksmith) and Liyue Harbor (near Alchemy NPC Timaeus/Xingxi). It allows players to convert lower-tier materials into higher-tier ones, craft potions, and (in later versions) convert between elemental gemstone types.

### 23.1 Material Tier Conversion

The core alchemy recipe: **3 lower-tier materials + Mora = 1 higher-tier material** of the same type.

This applies to:
- **Elemental Gemstones** (character ascension): Sliver -> Fragment -> Chunk -> Gemstone
- **Common Enemy Drops** (e.g., Slime Condensate -> Slime Secretions -> Slime Concentrate)
- **Talent Books** (e.g., Teachings of Freedom -> Guide to Freedom -> Philosophies of Freedom)
- **Weapon Ascension Materials**

| Conversion | Input | Mora Cost |
|-----------|-------|-----------|
| Tier 1 -> Tier 2 | 3x Tier 1 | 300 Mora |
| Tier 2 -> Tier 3 | 3x Tier 2 | 900 Mora |
| Tier 3 -> Tier 4 | 3x Tier 3 | 2,700 Mora |

### 23.2 Character Ascension Materials

Every character requires four categories of materials to ascend through six phases:

1. **Elemental Gemstones** — Element-specific gems in 4 tiers (Sliver/Fragment/Chunk/Gemstone). Obtained from Elite Bosses, Weekly Bosses, and alchemy crafting.
2. **Normal Boss Drops** — Unique 4-star materials from Elite Bosses (e.g., Hurricane Seed from Anemo Hypostasis, Hoarfrost Core from Cryo Regisvine). 40 Resin per boss fight.
3. **Local Specialties** — Region-specific plants/minerals gathered from the overworld (e.g., Windwheel Aster, Cor Lapis, Cecilia).
4. **Common Enemy Drops** — Tiered materials dropped by regular enemies (e.g., Slime Condensate/Secretions/Concentrate).

#### Ascension Cost Per Phase

| Phase | Level Cap | Mora | Gemstones | Boss Material | Local Specialty | Common Drops |
|-------|-----------|------|-----------|---------------|-----------------|--------------|
| 1 | 20 -> 40 | 20,000 | 1x Sliver | 0 | 3 | 3x Tier 1 |
| 2 | 40 -> 50 | 40,000 | 3x Fragment | 2 | 10 | 15x Tier 1 |
| 3 | 50 -> 60 | 60,000 | 6x Fragment | 4 | 20 | 12x Tier 2 |
| 4 | 60 -> 70 | 80,000 | 3x Chunk | 8 | 30 | 18x Tier 2 |
| 5 | 70 -> 80 | 100,000 | 6x Chunk | 12 | 45 | 12x Tier 3 |
| 6 | 80 -> 90 | 120,000 | 6x Gemstone | 20 | 60 | 24x Tier 3 |
| **Total** | | **420,000** | **1 Sliver, 9 Fragment, 9 Chunk, 6 Gemstone** | **46** | **168** | **18 T1, 30 T2, 36 T3** |

**Note:** Phase 1 (Lv. 20 ascension) does not require boss materials.

### 23.3 Talent Level-Up Materials

Each combat talent (Normal Attack, Elemental Skill, Elemental Burst) can be leveled from 1 to 10 independently. Passive talents do not require leveling. All three talent types share the same cost structure per level.

**Material categories for talents:**
1. **Talent Books** — 3 tiers: Teachings (green, 2-star), Guide (blue, 3-star), Philosophies (purple, 4-star). Obtained from Talent Material domains on specific days of the week.
2. **Common Enemy Drops** — Same tiered drops as ascension (Tier 1/2/3).
3. **Weekly Boss Drops** — Unique talent materials from Stormterror (Dvalin) and Wolf of the North (Andrius) in v1.0. Required for levels 7-10.
4. **Crown of Insight** — Rare event-exclusive material. Required for level 10.

#### Talent Level-Up Cost Per Level

| Level | Mora | Books | Common Drops | Weekly Boss | Crown |
|-------|------|-------|-------------|-------------|-------|
| 1 -> 2 | 12,500 | 3x Teachings | 6x Tier 1 | -- | -- |
| 2 -> 3 | 17,500 | 2x Guide | 3x Tier 2 | -- | -- |
| 3 -> 4 | 25,000 | 4x Guide | 4x Tier 2 | -- | -- |
| 4 -> 5 | 30,000 | 6x Guide | 6x Tier 2 | -- | -- |
| 5 -> 6 | 37,500 | 9x Guide | 9x Tier 2 | -- | -- |
| 6 -> 7 | 120,000 | 4x Philosophies | 4x Tier 3 | 1 | -- |
| 7 -> 8 | 260,000 | 6x Philosophies | 6x Tier 3 | 1 | -- |
| 8 -> 9 | 450,000 | 12x Philosophies | 9x Tier 3 | 2 | -- |
| 9 -> 10 | 700,000 | 16x Philosophies | 12x Tier 3 | 2 | 1 |
| **Total** | **1,652,500** | **3 Teachings, 21 Guide, 38 Philosophies** | **6 T1, 22 T2, 31 T3** | **6** | **1** |

**Total for all 3 talents to level 10:** 4,957,500 Mora, 9 Teachings, 63 Guide, 114 Philosophies, 18 T1 common drops, 66 T2 common drops, 93 T3 common drops, 18 Weekly Boss drops, 3 Crowns of Insight.

### 23.4 Talent Book Domains (v1.0)

| Domain | Location | Monday/Thursday | Tuesday/Friday | Wednesday/Saturday | Sunday |
|--------|----------|----------------|---------------|-------------------|--------|
| Forsaken Rift | Mondstadt | Freedom | Resistance | Ballad | All three |
| Taishan Mansion | Liyue | Prosperity | Diligence | Gold | All three |

### 23.5 Elemental Gemstone Conversion

**In v1.0:** Gemstone conversion between elements was **not available** at launch. The Dust of Azoth conversion system was introduced in v1.3 (alongside the Lantern Rite event).

**Post-v1.0 Dust of Azoth costs (for reference):**
- Sliver conversion: 1 Dust of Azoth
- Fragment conversion: 3 Dust of Azoth
- Chunk conversion: 9 Dust of Azoth
- Gemstone conversion: 27 Dust of Azoth

### 23.6 Potion Crafting

Elemental potions are crafted at the Crafting Bench (not cooked at a stove). They provide elemental buffs that fall under the **Elemental** food buff category (see §22.3). Two types exist:

- **Essential Oils** (DMG bonus): +25% elemental DMG for 300s. Available for Pyro, Cryo, Electro, Hydro.
- **Resistance Potions** (RES bonus): +25% elemental RES for 300s. Available for Anemo, Geo, Electro, Cryo, Pyro, Hydro.

Only one Elemental food buff can be active at a time. See [docs/cooking.md](docs/cooking.md) for the full potion list.

### 23.7 Character Crafting Passives

Some v1.0 characters have Utility Passive talents that affect alchemy:

| Character | Passive | Effect |
|-----------|---------|--------|
| **Xingqiu** | Flash of Genius | When crafting Talent Level-Up Materials, has a 25% chance to refund a portion of the crafting materials used. |
| **Sucrose** | Astable Anemohypostasis Creation - 6308 | When crafting Character and Weapon Enhancement Materials, has a 10% chance to obtain double the product. |
| **Mona** | Principium of Astrology | When crafting Weapon Ascension Materials, has a 25% chance to refund a portion of the crafting materials used. |
| **Jean** | When the West Wind Arises | (Not crafting-related -- cooking passive: has a 12% chance to obtain double the product when cooking a perfect dish) |
| **Ningguang** | Trove of Marvelous Treasures | (Not crafting -- displays nearby ore deposits on the minimap) |

---

## 24. UI & HUD

### 24.1 Gameplay HUD

| Element | Position | Description |
|---------|----------|-------------|
| **HP bars** | Top-left | Active character HP bar + 3 party member portraits with HP indicators. Character element icon shown. |
| **Stamina wheel** | Center (contextual) | Appears around character when stamina is being consumed (sprinting, climbing, swimming, charged attacks). Disappears when full. |
| **Minimap** | Top-left corner | Rotates with camera. Shows nearby Teleport Waypoints, quest markers, NPCs, domains, bosses. Compass ring indicates cardinal directions. |
| **Active quests** | Left side | Current quest objectives with navigation markers. |
| **Elemental Skill/Burst** | Bottom-right | Skill icon with cooldown timer. Burst icon with energy gauge (fills as energy is gained, glows when full). |
| **Party swap** | Right side | 4 character portraits (1–4 key indicators on PC). Active character highlighted. Cooldown overlay when character was recently swapped (1s swap CD). |
| **Adventure info** | Top-right (contextual) | Current region name, time of day, weather indicator. |

### 24.2 Combat Indicators

| Indicator | Description |
|-----------|-------------|
| **Damage numbers** | Float above hit targets. White = Physical. Colored = Elemental (Pyro red, Hydro blue, etc.). Larger font for CRIT hits. Yellow for reaction damage. |
| **Elemental aura icons** | Small element icon appears above affected enemies showing the applied aura. |
| **Elemental reaction text** | Reaction name briefly flashes on screen when triggered (e.g., "VAPORIZE", "OVERLOADED"). |
| **Enemy HP bars** | Appear above enemies when damaged. Elite/boss enemies have larger, named HP bars. |
| **Shield indicators** | Elemental shields on enemies shown as colored overlay on HP bar. |
| **Interaction prompts** | "F" or controller button appears when near interactable objects. |

### 24.3 Boss Health Bars

Normal bosses and weekly bosses display a large HP bar at the top of the screen with the boss name. Weekly bosses show distinct phase transitions. Hypostases show revival gauge during revival phase. No armor/phase segmentation on the HP bar in v1.0.

### 24.4 Menu Screens

| Menu | Access | Content |
|------|--------|---------|
| **Paimon Menu** | Esc / Options | Game time, UID, links to all sub-menus, co-op, settings, feedback, exit |
| **Map** | M | Full Teyvat map with zoom. Markers for waypoints, domains, bosses, quest objectives. Player-placed pins (up to 150). |
| **Character** | C | Character stats, talents (with level-up), constellations, friendship, artifacts, weapon |
| **Inventory/Backpack** | B | Items sorted by: Weapons, Artifacts, Materials (with sub-tabs), Furnishings, Food, Gadgets, Quest Items |
| **Wish** | F3 | Banner selection, pull history, pity counter (hidden — must count manually in v1.0) |
| **Adventure Handbook** | F1 | Boss tracking (respawn timers), enemy guides, progression milestones |
| **Battle Pass** | F5 | Daily/weekly/period missions, reward tracks |
| **Settings** | Paimon Menu > Settings | Graphics, audio, controls, language, account |

### 24.5 Dialogue System

Dialogue is presented in a visual-novel style bottom-of-screen text box with character portrait. Player choices appear as numbered options. Camera angles shift to frame the speaking character. Some dialogues are fully voiced (Archon Quests, Story Quests); others are text-only with grunts/reactions.

### 24.6 Save System

**Automatic save only.** No manual save. Progress saves continuously as the player explores, completes quests, and collects items. Server-side save (always-online game). Logging out at any point preserves current state. No multiple save slots — one save per account.

### 24.7 Camera

Third-person camera with player-controlled rotation (mouse/right stick). Camera distance adjustable via scroll wheel. Camera auto-adjusts when entering tight spaces. During Elemental Bursts, the camera performs a cinematic zoom/pan unique to each character's burst animation. Bow Aimed Shot switches to over-the-shoulder first-person aiming view.

### 24.8 Difficulty

No selectable difficulty setting. Difficulty scales automatically via the World Level system (§5.5). All players at the same World Level face the same enemy stats. The only difficulty control in v1.0 was avoiding AR Ascension Quests to stay at a lower World Level.

---

## 25. Open Questions / Unverified

| Question | Status |
|----------|--------|
| Exact enemy compositions for v1.0 Spiral Abyss floors 9-12 (chamber-by-chamber with enemy counts) | Archived on wiki but could not retrieve (403). The wiki page `Spiral_Abyss/Floors/2020-09-28` contains the full data. |
| Exact AR requirements for some v1.0 Story Quests (Amber, Kaeya, Lisa may have had lower or no AR gate) | Sources inconsistent on whether earliest story quests required AR or just quest completion. |
| Whether Electro Abyss Mage existed in v1.0 or was added later | Likely not in v1.0 (Electro Abyss Mage was added much later, around v2.7). Only Pyro, Hydro, Cryo variants at launch. |
| Precise soft pity start pull for character banner (sources cite 74, 75, or 76) | Community consensus is pull 74 is where rate increase begins, but exact ramp function varies by source. |
| Exact Gnostic Hymn/Sojourner reward table per BP level | Multiple minor revisions over time make it hard to confirm exact v1.0 level-by-level rewards. |
| Treasure Hoarder sub-variant availability at v1.0 (e.g., Gravedigger, Crusher, Seaman may have been added later) | Some sub-variants may have been added with Inazuma or later regions. Core types (Scout, Marksman, Potioneer, Handyman) were in v1.0. |
| Exact per-second stamina cost for climbing and swimming | Wiki confirms sprint=18/sec and glide=3/sec precisely. Climbing (~5/sec) and swimming (~5.6/sec) values come from community testing, not official data. |
| Starsnatch Cliff: parent area | Listed under Starfell Valley per wiki, but some sources associate it with Galesong Hill. The Fandom wiki categorizes it under Starfell Valley. |
| Liyue Statue of Seven per-level Geoculi requirements and reward breakdown | Total confirmed (130 Geoculi, 90 Primogems, 90 Geo Sigils, 2160 AR EXP, +70 stamina, 9 Stones of Remembrance) but per-level breakdown could not be retrieved from blocked wiki. |
| Exact v1.0 Teleport Waypoint count | 61 total commonly cited (22 Mondstadt + 39 Liyue), but some sources count domain entrances separately. |
| Character ascension Phase 1 exact gemstone quantity | Sources vary between "1 Sliver" and "3 Slivers" for Phase 1 (Lv. 20 ascension). Game8 source shows 3 Fragments for Phase 1 which conflicts with the standard 1 Sliver. The table in §23.2 uses wiki-standard values but should be verified in-game. |
| Artifact sub-stat initial roll count probability | Whether 3 vs 4 starting sub-stats on 5-star artifacts is 50/50 or weighted is not officially documented. Community estimates suggest roughly equal probability. |
| Complete v1.0 food recipe list | The exact set of cooking recipes available at v1.0 launch (vs. added in v1.1+) is difficult to pin down as most sources show the current recipe list. The food tables in docs/cooking.md cover confirmed v1.0 recipes but may be incomplete. |
| Essential Oil and Potion exact availability in v1.0 | Some elemental potions may have been added in later patches. The core set (Pyro/Cryo/Electro/Hydro oils and resistance potions) is confirmed for v1.0, but Anemo/Geo DMG oils may not have existed at launch. |
| Artifact 2x/5x EXP bonus exact rates | Community estimates 10% for 2x and 1% for 5x, but official rates were never published. |

---

## 26. References

### Wikis
- [Genshin Impact Wiki (Fandom)](https://genshin-impact.fandom.com/) — Primary source for enemy data, quest lists, version history
- [Genshin Impact Wiki (FextraLife)](https://genshinimpact.wiki.fextralife.com/) — Boss mechanics, enemy details
- [Honey Hunter World](https://genshin.honeyhunterworld.com/) — Datamined stats and enemy data

### Theorycrafting & Combat Mechanics
- [KQM Damage Formula](https://library.keqingmains.com/combat-mechanics/damage/damage-formula) — Full damage formula derivation
- [KQM Elemental Gauge Theory](https://library.keqingmains.com/combat-mechanics/elemental-effects/elemental-gauge-theory) — Gauge units, aura tax, decay
- [KQM Internal Cooldown](https://library.keqingmains.com/combat-mechanics/internal-cooldown) — ICD rules and exceptions
- [KQM Energy](https://library.keqingmains.com/combat-mechanics/energy) — Particle/orb values, off-field penalties
- [KQM Poise](https://library.keqingmains.com/combat-mechanics/poise) — Stagger system and impulse levels
- [KQM Enemy Resistances](https://library.keqingmains.com/combat-mechanics/enemy-mechanics/enemy-resistances) — Base RES values per enemy
- [KQM Frames](https://library.keqingmains.com/combat-mechanics/frames) — I-frame data, animation frames
- [KQM Shields](https://library.keqingmains.com/combat-mechanics/damage/shields) — Crystallize and character shield formulas
- [Elemental Reaction Level Scaling (Fandom)](https://genshin-impact.fandom.com/wiki/Elemental_Reaction/Level_Scaling) — Level multiplier table

### Strategy Guides
- [Game8 Genshin Impact](https://game8.co/games/Genshin-Impact/) — Boss guides, Spiral Abyss guides, Battle Pass guides
- [GameWith Genshin Impact](https://gamewith.net/genshin-impact/) — Shield weakness guides, shop guides
- [Keqing Mains Theorycrafting Library](https://library.keqingmains.com/) — Gacha rates, enemy shield/armor mechanics, verified formulas

### Community Resources
- [Spiral Abyss Analytics (spiralabyss.org)](https://spiralabyss.org/) — Floor tracking and team composition data
- [HoYoLAB](https://www.hoyolab.com/) — Official community hub, developer discussions, player guides
- [Genshin Impact v1.0 Spiral Abyss Floors (Fandom)](https://genshin-impact.fandom.com/wiki/Spiral_Abyss/Floors/2020-09-28) — Archived v1.0 floor data

### World Structure & Exploration Sources
- [Mondstadt (Fandom)](https://genshin-impact.fandom.com/wiki/Mondstadt) — Region/area/subarea hierarchy
- [Liyue (Fandom)](https://genshin-impact.fandom.com/wiki/Liyue) — Region/area/subarea hierarchy
- [Subarea (Fandom)](https://genshin-impact.fandom.com/wiki/Subarea) — Master list of all subareas by region
- [Stamina (Fandom)](https://genshin-impact.fandom.com/wiki/Stamina) — Stamina system details
- [Adventure Rank (Fandom)](https://genshin-impact.fandom.com/wiki/Adventure_Rank) — AR levels, unlocks, EXP requirements
- [Original Resin (Fandom)](https://genshin-impact.fandom.com/wiki/Original_Resin) — Resin system and change history
- [Original Resin/Change History (Fandom)](https://genshin-impact.fandom.com/wiki/Original_Resin/Change_History) — Resin cap changes across versions
- [Domain/List (Fandom)](https://genshin-impact.fandom.com/wiki/Domain/List) — Complete domain list with version history
- [Chest (Fandom)](https://genshin-impact.fandom.com/wiki/Chest) — Chest types, rewards, and respawn mechanics
- [Statue of The Seven (Fandom)](https://genshin-impact.fandom.com/wiki/Statue_of_The_Seven) — Oculi requirements and rewards
- [Mondstadt Statue of the Seven (Icy Veins)](https://www.icy-veins.com/genshin-impact/mondstadt-statue-of-the-seven) — Per-level reward breakdown
- [Genshin World Level Guide (BitTopup)](https://news.bittopup.com/news/genshin-world-level-guide-ar25-58-ascension-quest-tips) — WL/AR/enemy level table
- [Genshin Domain Schedule (Game Rant)](https://gamerant.com/genshin-impact-domain-schedule-guide/) — Domain rotation details
- [Version 1.0 (Fandom)](https://genshin-impact.fandom.com/wiki/Version/1.0) — v1.0 launch content verification
- [Version 1.1 (Fandom)](https://genshin-impact.fandom.com/wiki/Version/1.1) — Reputation, Resin cap increase, Condensed Resin confirmation

### Artifact System Sources
- [Artifact/Sets (Fandom)](https://genshin-impact.fandom.com/wiki/Artifact/Sets) — All artifact set bonuses
- [Artifact/Stats (Fandom)](https://genshin-impact.fandom.com/wiki/Artifact/Stats) — Main stat and sub-stat pools
- [Artifact/Scaling (Fandom)](https://genshin-impact.fandom.com/wiki/Artifact/Scaling) — Level scaling tables
- [Artifact EXP (Fandom)](https://genshin-impact.fandom.com/wiki/Artifact_EXP) — EXP costs and fodder values
- [Artifact/Distribution (Fandom)](https://genshin-impact.fandom.com/wiki/Artifact/Distribution) — Drop probabilities and main stat weights
- [KQM Artifacts Guide](https://keqingmains.com/misc/artifacts/) — Sub-stat roll values and Roll Value methodology
- [Genshin Substat Chart](https://genshin-substat-chart.vercel.app/) — Community sub-stat roll value visualizer
- [Artifact EXP Calculator (Brandon Fowler)](https://www.brandonfowler.me/artifact-exp-calc/) — Per-level EXP breakdown
- [BitTopup Artifact Main Stats Guide](https://news.bittopup.com/news/genshin-impact-artifact-main-stats-guide-15-25-dps-boost) — Main stat values at +20
- [BitTopup Artifact Stats Guide](https://news.bittopup.com/news/genshin-impact-artifact-stats-main-vs-sub-for-200-400-dps-boost) — Main stat numerical values

### Cooking System Sources
- [Food (Fandom)](https://genshin-impact.fandom.com/wiki/Food) — Food categories, buff stacking rules
- [Cooking (Fandom)](https://genshin-impact.fandom.com/wiki/Cooking) — Cooking mechanics, proficiency system
- [Special Dish (Fandom)](https://genshin-impact.fandom.com/wiki/Special_Dish) — Character special dish list
- [Game8 Special Dishes](https://game8.co/games/Genshin-Impact/archives/315338) — Character-to-dish mapping
- [RPG Site Cooking Guide](https://www.rpgsite.net/feature/10300-genshin-impact-cooking-guide-recipe-locations-ingredients-and-character-specialty-dishes) — Recipe locations and effects
- [BitTopup Food Buff Stacking Guide](https://news.bittopup.com/news/best-genshin-impact-food-buffs-stack-4-categories-guide) — Buff category stacking rules

### Crafting & Ascension Sources
- [Character Ascension Material (Fandom)](https://genshin-impact.fandom.com/wiki/Character_Ascension_Material) — Gemstone tiers and requirements
- [Character Talent Material (Fandom)](https://genshin-impact.fandom.com/wiki/Character_Talent_Material) — Talent book types and domains
- [Alchemy (Fandom)](https://genshin-impact.fandom.com/wiki/Alchemy) — Crafting mechanics
- [Dust of Azoth (Fandom)](https://genshin-impact.fandom.com/wiki/Dust_of_Azoth) — Gemstone conversion (v1.3+)
- [Game8 Character Ascension](https://game8.co/games/Genshin-Impact/archives/301576) — Ascension phase cost table
- [Game8 Talent Materials](https://game8.co/games/Genshin-Impact/archives/311474) — Talent level-up costs
- [TheGamer Talents Guide](https://www.thegamer.com/genshin-impact-talents-guide/) — Mora costs per talent level
- [Theria Games Elemental Gemstones](https://theriagames.com/guide/genshin-impact-elemental-ascension-gemstone/) — Gemstone quantities per phase
- [Screen Rant Mora Costs](https://screenrant.com/genshin-impact-how-much-mora-level-90-cost/) — Total ascension Mora (420,000)

### Companion Data Files
- [docs/artifacts.md](docs/artifacts.md) — Complete artifact set bonus tables, sub-stat roll values, EXP tables, drop weights
- [docs/cooking.md](docs/cooking.md) — Complete food recipe tables, character special dishes, elemental potions
- [docs/weapons.md](docs/weapons.md) — Weapon data tables
