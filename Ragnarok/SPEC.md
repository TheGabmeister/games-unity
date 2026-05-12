# Ragnarok Online v1.0 — Gameplay Systems Spec

PC MMORPG, Gravity Corp, 2002. Korean commercial launch August 2002.

**Scope**: Episodes 1–4 of kRO (Aug 2002 – May 2003), representing the complete classic Rune-Midgarts experience before the Schwarzvald expansion. This includes all Novice, 1st, and 2nd job classes; the card system; PvP; War of Emperium; and all Rune-Midgarts maps. Pre-renewal mechanics only (the 2010 Renewal overhaul is out of scope). Transcendent/Rebirth classes, 3rd classes, Expanded classes, instances, and Battlegrounds are all post-v1.0.

**Episode timeline within scope**:

| Episode | kRO Date | Content |
|---------|----------|---------|
| Ep 1 – Start of the Adventure | 2002-08-03 | Base game: 6 cities, Novice + 6 first classes, core dungeons |
| Ep 2 – Lutie | 2002-12-17 | Aldebaran, Lutie, Glast Heim, Clock Tower, Ant Hell. 2-1 second classes. Card system. PvP rooms |
| Ep 3 – Comodo | 2003-02-04 | Comodo + beach dungeons. 2-2 second classes (Crusader, Monk, Sage, Bard/Dancer, Rogue, Alchemist) |
| Ep 4 – War of Emperium | 2003-05-02 | Guild siege system. Turtle Island dungeon |

---

## 1. Core Gameplay Systems

### 1.1 Gameplay Loop

Players create a character as a **Novice**, level up by killing monsters in fields and dungeons, advance through a class tree (Novice → 1st Class → 2nd Class), acquire equipment and cards, and pursue endgame activities (MVP hunting, War of Emperium guild siege, PvP). There is no instanced content or main storyline quest chain — progression is entirely driven by grinding monsters, questing for equipment, and social/guild play.

### 1.2 Dual Level System

Characters have two independent level tracks:

- **Base Level** (cap 99): Determines stat points, HP/SP scaling, and overall combat formulas. Gained from Base EXP.
- **Job Level** (cap varies by class tier): Determines skill points. Gained from Job EXP.

Monsters give separate Base EXP and Job EXP on kill. Both fill independently.

**Stat points per Base Level**: Going from level N to N+1 grants `floor(N / 5) + 3` points.

| Level Range | Points Per Level | Cumulative Examples |
|-------------|-----------------|---------------------|
| 1 → 2 | 3 | 3 total |
| 10 → 11 | 5 | ~40 total |
| 50 → 51 | 13 | ~450 total |
| 98 → 99 | 22 | ~1,225 total |

Total stat points from level 1 to 99: **1,225** from leveling + **48** from character creation = **1,273**.

**Skill points**: 1 per Job Level gained (starting at Job Level 2). Novice gets 9, 1st/2nd class each get 49 at max Job Level 50.

### 1.3 Stat System

Six stats, all starting at 1, cap at 99. Distributed manually by the player at each Base Level up.

**Stat point cost**: Raising a stat from N to N+1 costs `floor((N − 1) / 10) + 2` points.

| Stat Range | Cost Per Point |
|-----------|---------------|
| 1–9 | 2 |
| 10–19 | 3 |
| 20–29 | 4 |
| 30–39 | 5 |
| 40–49 | 6 |
| 50–59 | 7 |
| 60–69 | 8 |
| 70–79 | 9 |
| 80–89 | 10 |
| 90–99 | 11 |

Total cost to raise one stat from 1 to 99: **628** points.

#### What Each Stat Affects

**STR (Strength)**
- Primary melee StatusATK: +1 per point
- Secondary ranged StatusATK: +1 per 5 points
- Weapon ATK bonus: +0.5% of weapon base ATK per point
- Weight limit: +30 per point

**AGI (Agility)**
- FLEE: +1 per point
- ASPD: primary stat (see §1.8)

**VIT (Vitality)**
- Max HP: +1% per point
- Soft DEF: +1 per 2 points
- HP healing item effectiveness: +2% per point
- Status ailment resistance: +1% per point

**INT (Intelligence)**
- MATK: primary stat (see §1.5)
- Max SP: +1% per point
- Soft MDEF: +1 per 2 points
- SP recovery rate: +1 per 6 points

**DEX (Dexterity)**
- HIT: +1 per point
- Primary ranged StatusATK: +1 per point
- Secondary melee StatusATK: +1 per 5 points
- Cast time reduction: primary stat (see §1.10)
- Minor ASPD contribution

**LUK (Luck)**
- Critical rate: +0.3% per point (1 crit per 3 LUK)
- Perfect Dodge: +1 per 10 points
- StatusATK: +1 per 3 points
- StatusMATK: +1 per 3 points
- HIT: +1 per 3 points
- FLEE: +1 per 5 points

### 1.4 Physical Combat

#### StatusATK

Melee: `floor(BaseLv / 4) + STR + floor(DEX / 5) + floor(LUK / 3)`
Ranged: `floor(BaseLv / 4) + floor(STR / 5) + DEX + floor(LUK / 3)`

#### WeaponATK

`((BaseATK + Variance + StatBonus + RefineBonus) × SizePenalty) × ATK% × Size% × Race% × Element%`

- **Variance**: Random value between −(WeaponLv × 5) and +(WeaponLv × 5) for weapons, or fixed for unarmed
- **RefineBonus**: See §6.4 for per-level values
- **SizePenalty**: Depends on weapon type vs target size (see §6.2)

#### Total Physical Damage

`TotalATK = StatusATK × 2 + WeaponATK + EquipATK + AmmoATK + MasteryATK`

After defense: `Damage = floor(TotalATK × HardDEF_reduction) − SoftDEF`

Minimum damage is 1 (attacks always deal at least 1 damage).

#### Defense System

**Hard DEF** (equipment-based, percentage reduction):
`Damage × (4000 + HardDEF) / (4000 + HardDEF × 10)`

| Hard DEF | Reduction |
|----------|-----------|
| 100 | ~18% |
| 275 | ~33% |
| 500 | ~50% |
| 1000 | ~71% |

**Soft DEF** (stat-based, flat subtraction):
`SoftDEF = floor(VIT / 2) + floor(AGI / 5) + floor(BaseLv / 2)`

Applied after Hard DEF as flat subtraction.

#### MDEF System

**Hard MDEF** (equipment-based):
`MagicDamage × (1000 + HardMDEF) / (1000 + HardMDEF × 10)`

**Soft MDEF**: Flat subtraction after Hard MDEF.

### 1.5 Magical Combat

#### MATK Formula

`MATK_Max = INT + floor(INT / 5)²`
`MATK_Min = INT + floor(INT / 7)²`

Actual MATK is randomly chosen between min and max each cast.

Example with 99 INT: MATK_Max = 99 + 19² = 460. MATK_Min = 99 + 14² = 295.

Bolt spells (Fire/Cold/Lightning Bolt) deal `SkillLevel` hits, where each hit deals one MATK roll (independently chosen between MATK_Min and MATK_Max). Total damage ≈ MATK × SkillLevel, but variance per hit matters because each hit is individually reduced by MDEF.

### 1.6 Accuracy (HIT / FLEE)

**Player HIT**: `175 + BaseLv + DEX + floor(LUK / 3) + bonuses`
**Player FLEE**: `100 + BaseLv + AGI + floor(LUK / 5) + bonuses`

**Hit chance**: `80 + AttackerHIT − DefenderFLEE`
- Capped at 5% minimum, 95% maximum

**FLEE penalty** (multiple attackers): When attacked by more than 2 monsters simultaneously, FLEE is reduced by 10% per additional attacker.

### 1.7 Critical Hits

**Critical rate**: `floor(LUK / 3) + CritBonuses`
**Anti-crit** (target resistance): `floor(TargetLUK / 5)`
**Effective crit**: `AttackerCrit − TargetAntiCrit`

Critical hit properties:
- Damage: **140%** of normal (1.4× multiplier)
- Bypasses FLEE check entirely (always hits)
- Does NOT bypass DEF for player attacks (monster crits DO bypass player Hard DEF)
- Does NOT bypass Perfect Dodge
- Katars have innate 2× critical rate

### 1.8 ASPD (Attack Speed)

Each class/weapon combination has a base attack delay. ASPD ranges from ~130 (unarmed Novice) to **190** (hard cap, unreachable without extreme gear/buffs). Practical cap is ~185–189 for optimized builds.

`Attack Interval (ms) = (200 − ASPD) × 10`

| ASPD | Attacks/sec | Interval |
|------|-----------|----------|
| 150 | 2.0 | 500ms |
| 170 | 3.3 | 300ms |
| 180 | 5.0 | 200ms |
| 185 | 6.7 | 150ms |
| 189 | 9.1 | 110ms |
| 190 | 10.0 | 100ms |

**Base ASPD by class** (unarmed, no bonuses): Novice ~146, Swordman ~156, Thief ~156, Archer ~146, Mage ~140, Merchant ~146, Acolyte ~146. Adding a weapon generally reduces ASPD by 10–50 depending on weapon weight/type.

Key modifiers:
- AGI: primary contributor to ASPD
- DEX: minor contribution
- Skills: Two-Hand Quicken (+30%), Adrenaline Rush, Increase AGI
- Equipment: Shields reduce ASPD by 5–10; heavier weapons reduce more
- Mounted: ASPD penalty (reduced by Cavalry Mastery, −20% at Lv0 → 0% at Lv5)

### 1.9 Perfect Dodge

`PerfectDodge = 1 + floor(LUK / 10) + PD_bonuses`

- Checked separately from normal FLEE
- Works only against physical normal attacks (not skills, not magic)
- Bypasses critical hits
- No hard cap

### 1.10 Cast Time

`ActualCastTime = BaseCastTime × (1 − DEX / 150)`

**Instant cast at 150 DEX** (achievable with buffs). This is the pre-renewal formula; Renewal changed this to a split variable/fixed cast system.

Skills also have **after-cast delay** (cooldown after cast completes) which is NOT reduced by DEX but can be reduced by Poem of Bragi (Bard performance). Suffragium (Priest skill) reduces the next spell's cast time, not after-cast delay.

### 1.11 Movement

- **Isometric 2.5D** perspective: 3D pre-rendered environments with 2D character sprites (8 directional facings)
- **Grid-based**: Every map is a cell grid with X,Y coordinates
- **Click-to-move**: Players click a destination; client pathfinds along the grid
- **Default speed**: 150ms per cell traversal
- **AGI does NOT affect move speed** — only skills (Increase AGI gives no move bonus) and items change it
- Speed Potions and certain skills (Peco mount, Tunnel Drive) modify speed
- **Hit stun**: Taking physical damage briefly halts movement; Endure skill grants flinch immunity

### 1.12 Death Penalty

**PvE death** (killed by monsters):
- Base EXP loss: 1% of current level's required EXP
- Job EXP loss: 1% of current level's required EXP
- Cannot lose levels (EXP floors at 0% of current level)
- Level 99 characters do not lose EXP

**PvP death**: No EXP loss in standard (Yoyo) PvP mode. Nightmare mode applies EXP loss + 1% chance to drop an equipped item.

### 1.13 Weight System

Every item has a weight value. Characters have a max weight capacity: `2000 + (STR × 30)`.

| Weight Threshold | Effect |
|-----------------|--------|
| 0–50% | Normal — full HP/SP regeneration, all actions available |
| 50–90% | **Overweight** — natural HP/SP regeneration stops completely. All other actions still available |
| 90–100% | **Critical overweight** — cannot attack, use skills, or pick up items. Only movement and consumable use allowed |

Weight management is a core survival consideration. High-STR builds carry more, but caster builds (INT/DEX) have tight weight budgets and must manage potion and gemstone loads carefully.

### 1.14 Natural HP/SP Recovery

HP and SP regenerate passively over time. Rate depends on stance and stats.

**HP Recovery**:
- Standing: every 6 seconds
- Sitting: every 4 seconds
- Base amount: `MaxHP / 200` (standing), doubled when sitting
- VIT bonus: additional HP per tick
- Increase HP Recovery skill (Swordman): adds flat HP per tick

**SP Recovery**:
- Standing: every 8 seconds
- Sitting: every 4 seconds
- Base amount: `1 + (MaxSP / 100)` (standing), doubled when sitting
- INT bonus: additional SP per tick
- Increase SP Recovery skill (Mage/Acolyte): adds flat SP per tick

**Recovery is disabled entirely** when weight exceeds 50%. Sitting grants faster recovery and is essential for SP-dependent classes between fights.

---

## 2. Controls & Input

PC only, mouse + keyboard.

### Primary Controls

| Input | Action |
|-------|--------|
| Left-click (ground) | Move to cell |
| Left-click (monster) | Attack / Target |
| Left-click (NPC) | Interact |
| Left-click (player) | Select / Trade / Party invite |
| Right-click | Camera rotation (hold + drag) |
| Scroll wheel | Zoom in/out |
| F1–F9 | Hotkey bar (skills and items) |
| F10 | Toggle second hotkey bar |
| F12 | Switch hotkey bar set (rotate) |
| Alt + (key) | Alternate hotkey bar row |
| Ctrl + click | Force attack (attack players in PvP, attack non-aggressive monsters) |
| Shift + click | Force attack without moving |
| Insert | Sit / Stand toggle |
| Home | Camera reset (face north) |
| PgUp / PgDn | Zoom in / out |

### Chat Commands

| Command | Effect |
|---------|--------|
| /where | Shows current map name and coordinates |
| /memo | Memorizes current location for Warp Portal (Acolyte) |
| /sit | Sit down |
| /stand | Stand up |
| /effect | Toggle visual effects |
| /mineffect | Reduce skill effects |
| /bm | Toggle battle mode (all keys become hotkeys) |
| /noctrl | Auto-attack mode toggle |
| /guild (name) | Create guild (consumes Emperium) |
| /organize (name) | Create party |
| /pvp | Check PvP mode |

### Chat System

- **Normal chat**: Visible to nearby players (speech bubble above head)
- **Party chat**: Prefix `%` — visible only to party members
- **Guild chat**: Prefix `$` — visible only to guild members
- **Whisper**: `/w "PlayerName" message` — private message to specific player
- **Global/Shout**: Prefix `!` — visible to entire map (costs zeny)
- **Chat rooms**: Player-created rooms where people can sit and chat

---

## 3. World Structure

### 3.1 Cities

All cities are safe zones — no monsters, no EXP loss, NPC services.

| City | Description | Key NPCs / Services |
|------|-------------|---------------------|
| **Prontera** | Capital of Rune-Midgarts. Central hub. | Knight Guild, Acolyte Sanctuary, Crusader Guild. Main market area |
| **Izlude** | Satellite of Prontera. | Swordman Guild. Arena. Boat to Byalan Island |
| **Geffen** | Mage city on an island. | Mage Academy, Wizard Guild. Tower leads to Geffen Dungeon |
| **Payon** | Mountain/forest, Korean architecture. | Archer Guild. Gate to Payon Cave |
| **Morroc** | Desert frontier city. | Thief Guild, Assassin Guild. Access to Pyramid and Sphinx |
| **Alberta** | Port city. | Merchant Guild. Boat to Sunken Ship and Turtle Island |
| **Aldebaran** | Venice-inspired water city (Ep 2). | Alchemist Guild, Kafra HQ. Clock Tower dungeon |
| **Lutie** | Christmas village (Ep 2). | Toy Factory dungeon. Accessible only via NPC warp from Aldebaran |
| **Comodo** | Beach town (Ep 3). | Bard/Dancer Guild. 3 beach cave dungeons |

### 3.2 Field Maps

Outdoor maps connecting cities, with monster spawns scaled to approximate level ranges. Maps connect at edges via invisible warp portals, creating a continuous grid-world.

| Region | Map Prefix | Count | Level Range | Notable |
|--------|-----------|-------|-------------|---------|
| Prontera Fields | prt_fild | 12 | 1–30 | Starter area. Porings, Lunatics |
| Mt. Mjolnir | mjolnir_ | 12 | 15–50 | Connects Prontera to Geffen/Aldebaran. Mistress MVP |
| Geffen Fields | gef_fild | 15 | 20–60 | Orc Lord MVP (fild10), Orc Hero MVP (fild14) |
| Payon Fields | pay_fild | 11 | 10–45 | Eddga MVP (fild11) |
| Sograt Desert (Morroc) | moc_fild | 21 | 15–55 | Largest field region. Phreeoni MVP (fild15) |
| Comodo Fields | cmd_fild | 9 | 30–60 | Beach areas (Ep 3) |

### 3.3 Dungeons

| Dungeon | Floors | Map Prefix | Level Range | MVPs | Episode |
|---------|--------|-----------|-------------|------|---------|
| Prontera Culverts | 4 | prt_sewb | 10–40 | Golden Thief Bug (F4) | 1 |
| Geffen Dungeon | 4 | gef_dun | 35–85 | Doppelganger (F3), Dracula (F1) | 1 |
| Payon Cave | 5 | pay_dun | 10–55 | Moonlight Flower (F5) | 1 |
| Pyramid | 6 | moc_pryd | 20–60 | Osiris (F4), Amon Ra (B2) | 1 |
| Sphinx | 5 | in_sphinx | 30–75 | Pharaoh (F5) | 1 |
| Byalan Island | 5 | iz_dun | 15–50 | None | 1 |
| Sunken Ship | 2 | treasure | 30–50 | Drake (F2) | 1 |
| Hidden Temple | 3 | prt_maze | 40–70 | Baphomet (F3) | 1 |
| Orc Dungeon | 2 | orcsdun | 20–40 | None (Orc Lord is on field) | 1 |
| Coal Mine | 3 | mjo_dun | 20–40 | None | 1 |
| Glast Heim | 16+ | gl_* | 50–80 | Dark Lord (Churchyard) | 2 |
| Clock Tower | 4+ | alde_dun | 47–80+ | None (mini-bosses) | 2 |
| Ant Hell | 2 | anthell | 25–50 | Maya (F2) | 2 |
| Toy Factory | 2 | xmas_dun | 40–60 | Stormy Knight (F2) | 2 |
| Turtle Island | 4 | tur_dun | 55–85 | Turtle General (F4) | 4 |
| Beach Dungeons | 3×2 | beach_dun, cmd_dun | 40–65 | Tao Gunka | 3 |

**Glast Heim** is the largest dungeon complex — a hub-and-spoke layout with Castle, Churchyard, Church, Prison, Sewers, and Chivalry sub-areas all accessed from a central courtyard map.

### 3.4 Travel

**Edge warps**: Walking to map edges transitions to the adjacent field map, creating a seamless grid-world.

**Kafra Service** (NPC in every town):
- Storage: 40z per access, shared across account, up to 600 slots
- Teleport: Paid warp to other cities (price varies by distance)
- Save Point: Sets respawn location (free)
- Cart Rental: Merchant class only

**Warp Portal** (Acolyte/Priest skill): Costs 1 Blue Gemstone. Player memorizes up to 3 locations via `/memo`. Creates a portal usable by up to 8 people.

**Teleport** (Acolyte skill): Lv1 random warp on current map, Lv2 adds return-to-save-point option.

**Fly Wing / Butterfly Wing** (items): Consumable equivalents of Teleport Lv1 / Lv2.

---

## 4. Playable Characters / Classes

Full skill tables for every class are in the companion file: [docs/ragnarok-online/skills.md](docs/ragnarok-online/skills.md).

### 4.1 Class Progression

```
Novice (Job Lv 1–10)
├── Swordman (Job Lv 1–50)
│   ├── Knight (2-1)      Job Lv 1–50
│   └── Crusader (2-2)    Job Lv 1–50
├── Mage (Job Lv 1–50)
│   ├── Wizard (2-1)      Job Lv 1–50
│   └── Sage (2-2)        Job Lv 1–50
├── Archer (Job Lv 1–50)
│   ├── Hunter (2-1)      Job Lv 1–50
│   └── Bard ♂ / Dancer ♀ (2-2)  Job Lv 1–50
├── Thief (Job Lv 1–50)
│   ├── Assassin (2-1)    Job Lv 1–50
│   └── Rogue (2-2)       Job Lv 1–50
├── Merchant (Job Lv 1–50)
│   ├── Blacksmith (2-1)   Job Lv 1–50
│   └── Alchemist (2-2)   Job Lv 1–50
└── Acolyte (Job Lv 1–50)
    ├── Priest (2-1)       Job Lv 1–50
    └── Monk (2-2)         Job Lv 1–50
```

**Class change requirements**:
- Novice → 1st Class: Job Level 10, Basic Skill maxed (Lv 9)
- 1st Class → 2nd Class: Job Level 40 minimum (50 recommended for max skill points). Each change involves a class-specific quest

Upon class change, Job Level resets to 1. All 1st class skills carry over to 2nd class. 1st and 2nd class skill point pools are separate.

**Stat/Skill Reset**: Available via special NPCs or consumable items. Stat Reset returns all stat points for reallocation. Skill Reset returns all skill points (1st class points must be reallocated before 2nd class points). Availability and cost are server-dependent.

**Character creation**: 48 stat points distributed across 6 stats on paired axes: STR↔INT, AGI↔LUK, VIT↔DEX (increasing one decreases its pair). Each stat starts between 1–9.

### 4.2 Class Overview

#### First Job Classes

| Class | Role | Primary Stats | Key Mechanics |
|-------|------|--------------|---------------|
| **Swordman** | Melee tank/DPS | STR, VIT, AGI | Bash (single-target burst), Magnum Break (AoE fire), Endure (flinch immunity), Provoke (aggro) |
| **Mage** | Ranged magic DPS | INT, DEX | Elemental Bolt spells (Fire/Cold/Lightning × 10 levels), Thunderstorm (AoE), Safety Wall (melee shield), Fire Wall (ground barrier) |
| **Archer** | Ranged physical DPS | DEX, AGI | Double Strafe (single-target), Arrow Shower (AoE), Improve Concentration (self-buff) |
| **Thief** | Fast melee/stealth | AGI, STR | Double Attack (passive multi-hit with daggers), Hiding (invisibility), Steal, Envenom (poison) |
| **Merchant** | Economy/melee | STR, VIT | Mammonite (powerful melee consuming zeny), Vending (player shop), Pushcart, Discount/Overcharge |
| **Acolyte** | Healer/support | INT, DEX, VIT | Heal (scales with INT/BaseLv), Blessing (+STR/INT/DEX), Increase AGI, Teleport, Warp Portal, Pneuma (ranged block) |

#### Second Job Classes — 2-1

| Class | From | Role | Signature Mechanics |
|-------|------|------|-------------------|
| **Knight** | Swordman | Heavy melee, mounted | Bowling Bash (AoE knockback), Two-Hand Quicken (+ASPD), Brandish Spear (mounted AoE), Pierce (size-scaled), Peco Peco mount (+move speed) |
| **Wizard** | Mage | AoE magic destroyer | Storm Gust (ice AoE, freeze), Meteor Storm (fire AoE), Lord of Vermilion (wind AoE), Jupitel Thunder (knockback), Quagmire (ground debuff) |
| **Hunter** | Archer | Ranged DPS, trapper | 11 trap types (Ankle Snare, Claymore, Blast Mine, etc.), Falcon companion (Blitz Beat piercing attack), Steel Crow |
| **Assassin** | Thief | Burst damage, stealth | Sonic Blow (8-hit Katar burst), Cloaking (mobile stealth), Grimtooth (attack from stealth), Enchant Poison, dual-wield or Katar build |
| **Priest** | Acolyte | Primary healer/buffer | Kyrie Eleison (hit barrier), Lex Aeterna (2× next damage), Magnus Exorcismus (Holy AoE), Resurrection, Sanctuary (ground heal), Aspersio (Holy weapon), Magnificat (2× SP regen) |
| **Blacksmith** | Merchant | Weapon crafter, party buffer | Weapon forging from ores, Adrenaline Rush (+ASPD party), Weapon Perfection (ignore size), Power Thrust (+ATK party), Maximize Power (max ATK roll) |

#### Second Job Classes — 2-2

| Class | From | Role | Signature Mechanics |
|-------|------|------|-------------------|
| **Crusader** | Swordman | Holy tank/support | Auto Guard (shield block), Grand Cross (Holy AoE, costs HP), Devotion (take damage for ally), Reflect Shield, Defender (ranged reduction) |
| **Sage** | Mage | Anti-mage, enchanter | Endow skills (Fire/Water/Wind/Earth weapon enchant), Dispell (strip buffs), Land Protector (nullify ground magic), Free Cast (move while casting), Auto Spell (bolt on melee), Spell Breaker |
| **Bard** ♂ | Archer | Party support (music) | Performance songs: Poem of Bragi (−cast time/delay), Apple of Idun (+MaxHP), Assassin Cross of Sunset (+ASPD), Whistle (+FLEE). Ensemble skills with Dancer |
| **Dancer** ♀ | Archer | Party support (dance) | Performance dances: Please Don't Forget Me (slow enemies), Humming (+HIT), Fortune's Kiss (+Crit), Service for You (+MaxSP). Ensemble skills with Bard |
| **Rogue** | Thief | PvP specialist | Plagiarism (copy enemy skill), Strip skills (remove equipment), Back Stab (never-miss from behind), Tunnel Drive (move in Hiding), Intimidate (warp both), Close Confine |
| **Monk** | Acolyte | Combo burst melee | Spirit Spheres, Combo chain (Triple Attack → Chain Combo → Combo Finish → Asura Strike). **Asura Strike**: consumes ALL SP for massive damage = ATK × (8 + SP/10). Investigate (damage scales with target DEF). Steel Body (+90 DEF/MDEF, immobilized) |
| **Alchemist** | Merchant | Potion crafter, summoner | Pharmacy (brew potions/items), Acid Terror (ranged + armor break), Potion Pitcher (ranged heal), Homunculus companion (4 types: Lif, Amistr, Filir, Vanilmirth) with own level/stats |

### 4.3 Bard/Dancer Performance System

Performances are area-effect buffs/debuffs that persist while the Bard/Dancer holds position and the skill remains active. Cancelling or moving ends the performance. Each Bard/Dancer can have only one solo performance active.

**Ensemble Skills** require a Bard and Dancer to stand adjacent and activate simultaneously. Both are immobilized during the Ensemble. These include: Lullaby (AoE Sleep), A Drum on the Battlefield (+ATK/DEF), The Ring of Nibelungen (+ATK for Lv4 weapons), Into the Abyss (no catalyst consumption), Invulnerable Siegfried (+element resist), Loki's Veil (no skills in area).

### 4.4 Monk Combo System

Monk's primary combat mechanic chains skills in a strict sequence with timing windows:

1. **Triple Attack** (passive, triggers on normal attack) → opens combo window
2. **Chain Combo** (must activate during Triple Attack window) → opens next window
3. **Combo Finish** (must activate during Chain Combo window) → opens next window
4. **Asura Strike** (can chain from Combo Finish OR Blade Stop, requires Fury state)

Each link in the chain must be activated within a brief timing window (~1s) or the combo drops. Asura Strike also requires Explosion Spirits (Fury) state active and consumes all remaining SP. After Asura Strike, SP regeneration is locked out for 5 minutes.

### 4.5 Homunculus System (Alchemist)

Requires Bioethics quest skill. Alchemists hatch one of 4 Homunculus types from an Embryo (crafted via Pharmacy). Each Homunculus:
- Has independent Base Level, HP, SP, and 6 stats
- Gains EXP alongside the Alchemist from kills
- Has 2 active skills + 1 passive skill unique to its type
- Has an **Intimacy** meter (increases by feeding, decreases by starvation/death)
- Can evolve at max Intimacy under specific conditions

| Type | Role | Key Skills |
|------|------|-----------|
| **Lif** | Support | Healing Hands (heal owner), Emergency Avoid (FLEE buff) |
| **Amistr** | Tank | Adamantium Skin (DEF/MDEF buff), Castling (swap position with owner) |
| **Filir** | Attacker | Moonlight (ranged attack), Flitting (ASPD buff) |
| **Vanilmirth** | Magic | Caprice (random element bolt), Chaotic Blessings (random heal/damage) |

### 4.6 Pet System

Players can tame certain monsters using class-specific taming items. Once tamed, the pet follows the player as a companion.

**Taming**: Use a taming item on the correct monster species. Success rate varies by monster level and pet type (typically 20–50%). Failed attempts consume the item.

**Pet management**:
- **Intimacy** meter (0–1000): Increases by feeding the pet its preferred food, decreases from starvation or the pet dying. Intimacy determines the pet's mood and whether it provides bonuses
- **Hunger** meter (0–100): Decreases over time. Feed the pet when hungry to maintain Intimacy. Overfeeding or underfeeding reduces Intimacy
- **Pet Accessory**: Each pet type has a unique accessory (equippable item) that activates a passive bonus when the pet's Intimacy is "Loyal" (900+)

**Notable pets and bonuses**:

| Pet | Taming Item | Food | Accessory Bonus (Loyal) |
|-----|-------------|------|------------------------|
| Poring | Unripe Apple | Apple Juice | +2 LUK, +1% Critical |
| Lunatic | Rainbow Carrot | Carrot Juice | +3 Critical Rate, +3 ATK |
| Drops | Orange Juice | Yellow Herb | +3 HIT, +3 ATK |
| Poporing | Bitter Herb | Green Herb | +2 LUK, +5% Poison Resist |
| Isis | Armlet of Obedience | Pet Food | +10% Shadow Resist |
| Deviruchi | Contract in Shadow | Shoot | +6 ATK, +1 MHP |
| Baphomet Jr. | Book of the Devil | Honey | +1 DEF, +1 MDEF, −1% Crit |
| Munak | No Recipient | Pet Food | +1 INT, +2 DEF |
| Sohee | Silver Knife of Chastity | Pet Food | +3% SP, +3 MDEF |

Pets are cosmetic + minor stat bonuses. They do not fight, tank, or have active abilities.

---

## 5. Elements & Status Effects

Full element damage tables for all 4 defense levels are in: [docs/ragnarok-online/elements.md](docs/ragnarok-online/elements.md).

### 5.1 Element System

10 elements: **Neutral, Water, Earth, Fire, Wind, Poison, Holy, Shadow, Ghost, Undead**.

Key relationships:
- **Elemental cycle**: Water → Fire → Earth → Wind → Water (each deals 150% to the next at Lv1, scaling to 200% at Lv3–4). The reverse direction deals only 50% at Lv1, dropping to 0% at higher levels. Wind → Water is an outlier at 175% instead of 150%
- **Holy ↔ Shadow**: Mutually strong (Holy attacks deal up to 200% to Shadow Lv4; Shadow deals up to 200% to Holy Lv4)
- **Ghost**: Neutral attacks deal only 25% to Ghost Lv1, 0% at Lv3–4. Use Ghost-element attacks or any non-Neutral element
- **Undead**: Weak to Holy (150% at Lv1, up to 200%) and Fire (125% at Lv1, up to 200%). Healed by Poison and Shadow at higher levels
- **Same element**: Strongly resisted (25% at Lv1, negative/healing at higher levels)

**Defense element levels** (1–4): Monsters range from Lv1–4. Players are always Lv1 regardless of source. Higher levels amplify both weaknesses and resistances dramatically.

**Weapon element sources**: Sage Endow skills, Aspersio (Holy), Enchant Poison, Cursed Water (Shadow), Elemental Converters (consumable), Blacksmith forging (permanent), innate weapon element, and cards.

**Armor element sources**: Default is Neutral Lv1. Changed by armor cards (e.g., Swordfish → Water, Ghostring → Ghost, Angeling → Holy, Evil Druid → Undead).

### 5.2 Status Effects

#### Negative Status Effects

| Status | Effect | Duration | Cure | Sources |
|--------|--------|----------|------|---------|
| **Poison** | −25% HP regen, HP drains over time, ATK −10% | Until cured or death | Green Potion, Panacea, Detoxify, Cure | Envenom, Enchant Poison, Poison-element attacks |
| **Freeze** | Cannot move, attack, or use skills. DEF +100%, MDEF −50%. Element becomes Water Lv1 | 3–30s (varies) | Fire-element attack, Status Recovery | Frost Diver, Storm Gust (3 hits), Freezing Trap |
| **Stone Curse** | 2-phase: slow phase (can still move slowly, 5s) then petrified (cannot act, DEF +100%, MDEF −50%, Earth Lv1, HP drain 1% per tick) | Until HP reaches 1 or cured | Any attack, Status Recovery | Stone Curse skill |
| **Stun** | Cannot move, attack, or use skills. Cannot flee. | 1–5s | Status Recovery (wears off quickly) | Bash + Fatal Blow, Hammer Fall, various skills |
| **Blind (Darkness)** | HIT −25%, FLEE −25%. Screen darkened for players. | 10–30s | Green Potion, Cure, Status Recovery | Sand Attack, Flasher trap, Raid |
| **Silence** | Cannot use skills. | 30–60s | Green Potion, Cure, Status Recovery | Lex Divina |
| **Curse** | LUK = 0, movement speed −25%, ATK −25% | Until cured | Holy Water, Blessing, Status Recovery | Cursed weapons, certain monsters |
| **Sleep** | Cannot act. Woken by any damage or Status Recovery. | 10–30s | Any damage, Status Recovery | Sandman trap, Lullaby ensemble |
| **Confusion (Chaos)** | Movement direction is randomized/reversed. | Until cured | Cure, Status Recovery | Certain monster skills |
| **Bleeding** | HP drains over time. Cannot regen HP naturally. Cannot use healing items. | Until cured | Cure, Status Recovery | Acid Terror, certain skills |

#### Positive Status Effects (Buffs)

| Buff | Effect | Duration | Source |
|------|--------|----------|--------|
| **Blessing** | +STR/INT/DEX (1–10) | 60–240s | Acolyte/Priest |
| **Increase AGI** | +AGI (3–12) | 60–240s | Acolyte/Priest |
| **Angelus** | +VIT DEF (5–50%) | 30–300s | Acolyte |
| **Kyrie Eleison** | Hit barrier (5–14 hits or MaxHP %) | 120s | Priest |
| **Magnificat** | 2× SP regen rate | 30–90s | Priest |
| **Gloria** | +30 LUK | 10–30s | Priest |
| **Aspersio** | Weapon = Holy element | 60–180s | Priest |
| **Impositio Manus** | +ATK (5–25) | 60s | Priest |
| **Two-Hand Quicken** | +30% ASPD (2H Sword) | 60–300s | Knight |
| **Adrenaline Rush** | +ASPD (Axes/Maces) | 30–150s | Blacksmith (party) |
| **Weapon Perfection** | 100% damage all sizes | 10–50s | Blacksmith (party) |
| **Power Thrust** | +ATK 5–25% (weapon break risk) | 20–180s | Blacksmith (party) |
| **Endure** | Flinch immunity | 10–37s | Swordman |
| **Enchant Poison** | Weapon = Poison element + poison chance | 40–220s | Assassin |
| **Explosion Spirits (Fury)** | +250 Critical rate | 180s | Monk |
| **Auto Guard** | Shield block chance (5–30%) | Toggle | Crusader |
| **Defender** | −Ranged damage (5–25%) + move slow | Toggle | Crusader |
| **Reflect Shield** | Reflect 13–40% melee damage | 300s | Crusader |
| **Sage Endow** | Weapon = specific element | 20–60 min | Sage |
| **Energy Coat** | −Physical damage at SP cost | Until SP depleted | Mage |

---

## 6. Items & Equipment

### 6.1 Equipment Slots

10 equipment positions per character:

| Slot | Notes |
|------|-------|
| Upper Headgear | Visible on sprite. May have 0–1 card slot |
| Middle Headgear | Glasses/masks. Rarely has card slot |
| Lower Headgear | Mouth items. Cannot hold cards |
| Armor (Body) | Major DEF piece. 0–1 card slot |
| Weapon (Right Hand) | 2H weapons block Shield slot |
| Shield (Left Hand) | Cannot equip with 2H weapons. 0–1 card slot |
| Garment (Cape/Manteau) | 0–1 card slot |
| Footgear (Shoes/Boots) | 0–1 card slot |
| Accessory 1 | 0–1 card slot |
| Accessory 2 | 0–1 card slot |

### 6.2 Weapon Types & Size Modifiers

Weapon damage is modified by target size:

| Weapon Type | Small | Medium | Large | Primary Classes |
|-------------|-------|--------|-------|----------------|
| Dagger | 100% | 75% | 50% | All |
| 1H Sword | 75% | 100% | 75% | Swordman, Knight, Crusader, Rogue |
| 2H Sword | 75% | 75% | 100% | Swordman, Knight |
| 1H Spear | 75% | 75% | 100% | Swordman, Knight, Crusader |
| 2H Spear | 75% | 75% | 100% | Knight, Crusader |
| Spear (mounted) | 100% | 100% | 100% | Knight, Crusader on Peco |
| Mace | 75% | 100% | 100% | Swordman, Acolyte, Priest, Merchant |
| 1H Axe | 50% | 75% | 100% | Merchant, Blacksmith, Alchemist |
| 2H Axe | 50% | 75% | 100% | Merchant, Blacksmith, Alchemist |
| Bow | 100% | 100% | 75% | Archer, Hunter, Bard, Dancer, Rogue |
| Rod/Staff | 100% | 100% | 100% | Mage, Wizard, Sage |
| Katar | 75% | 100% | 75% | Assassin only |
| Knuckle/Fist | 100% | 100% | 75% | Monk only |
| Book | 100% | 100% | 50% | Sage, Priest |
| Instrument | 75% | 100% | 75% | Bard only |
| Whip | 75% | 100% | 50% | Dancer only |
| Unarmed | 100% | 100% | 100% | All (very low damage) |

### 6.3 Weapon Levels

| Property | Weapon Lv1 | Weapon Lv2 | Weapon Lv3 | Weapon Lv4 |
|----------|-----------|-----------|-----------|-----------|
| Upgrade material | Phracon | Emveretarcon | Oridecon | Oridecon |
| Material NPC price | 200z | 1,000z | Drop only | Drop only |
| Refine fee | 50z | 200z | 5,000z | 20,000z |
| ATK per refine (safe) | +2 | +3 | +5 | +7 |
| ATK per refine (over-upgrade) | +3 | +5 | +8 | +13 |
| Safe refine limit | +7 | +6 | +5 | +4 |
| ATK variance | ±5 | ±10 | ±15 | ±20 |

### 6.4 Refinement System

Equipment can be upgraded from +0 to +10 at Upgrade NPCs.

**Refine success rates**:

| Level | Wep Lv1 | Wep Lv2 | Wep Lv3 | Wep Lv4 | Armor |
|-------|---------|---------|---------|---------|-------|
| +1 to safe | 100% | 100% | 100% | 100% | 100% |
| +5 | 100% | 100% | 100% | 60% | 60% |
| +6 | 100% | 100% | 60% | 40% | 40% |
| +7 | 100% | 60% | 50% | 40% | 40% |
| +8 | 60% | 40% | 20% | 20% | 20% |
| +9 | 40% | 20% | 20% | 20% | 20% |
| +10 | 20% | 20% | 20% | 10% | 10% |

**On failure**: Equipment is **permanently destroyed** along with all cards compounded in it. No downgrade — outright destruction.

**Armor refinement**: Uses Elunium (crafted from 5 Rough Elunium). Fee: 2,000z. Safe limit: +4.

### 6.5 Card System

Cards are rare monster drops (base rate: **0.01%**, 1 in 10,000 kills) that can be permanently compounded into slotted equipment to grant bonuses.

**Rules**:
- Each card specifies which slot type it goes in (weapon, armor, shield, garment, footgear, headgear, or accessory)
- Once inserted, a card **cannot be removed**
- If the equipment is destroyed (failed refine), the card is destroyed with it
- Equipment displays slot count as a suffix: "Pike [3]" = 3 card slots
- Weapons can have 0–4 slots. Other equipment: 0–1 slots
- Cards add a prefix or suffix to the equipment name (e.g., "Bloody" from Hydra Card)

**Key weapon cards**:

| Card | Effect | Stacking |
|------|--------|----------|
| Hydra | +20% damage vs Demi-Human | Yes (4 Hydra = +80%) |
| Andre | +20 ATK | Yes |
| Skel Worker | +15% damage vs Medium, +5 ATK | Yes |
| Minorous | +15% damage vs Large, +5 ATK | Yes |
| Desert Wolf | +15% damage vs Small, +5 ATK | Yes |
| Vadon | +20% damage vs Fire element | Yes |
| Drainliar | +20% damage vs Water element | Yes |
| Stormy Knight | Auto-cast Storm Gust Lv1, Freeze chance | — |
| Turtle General | +20% damage to all targets | Yes |
| Phreeoni | +100 HIT | — |

**Key defensive cards**:

| Card | Slot | Effect |
|------|------|--------|
| Thara Frog | Shield | −30% damage from Demi-Human |
| Raydric | Garment | +20% Neutral resistance |
| Marc | Armor | Freeze immunity, Water Lv1 armor |
| Ghostring | Armor | Ghost Lv1 armor (Neutral attacks deal only 25%), −HP regen |
| Angeling | Armor | Holy Lv1 armor |
| Evil Druid | Armor | Undead Lv1 armor (Freeze/Stone immunity, Heal damages) |
| Pupa | Armor | +700 Max HP |
| Matyr | Footgear | +10% Max HP, +1 AGI |
| Zerom | Accessory | +3 DEX |

### 6.6 Consumables

| Item | Effect | NPC Price | Weight |
|------|--------|-----------|--------|
| Red Potion | Heals 45–65 HP | 50z | 7 |
| Orange Potion | Heals 105–145 HP | 200z | 10 |
| Yellow Potion | Heals 175–235 HP | 550z | 13 |
| White Potion | Heals 325–405 HP | 1,200z | 15 |
| Blue Potion | Restores 40–60 SP | 5,000z | 15 |
| Green Potion | Cures Poison/Silence/Blind/Confusion | 40z | 7 |
| Yggdrasil Berry | Full HP + SP restore | Not NPC-sold (rare drop) | 15 |
| Yggdrasil Leaf | Resurrects dead player | Not NPC-sold (rare drop) | 10 |
| Fly Wing | Random teleport on current map | 40z | 5 |
| Butterfly Wing | Return to save point | 300z | 5 |
| Awakening Potion | Prevents Sleep, +ASPD | 1,500z | 15 |
| Speed Potion | +25% movement speed | Not NPC-sold | 10 |

**Condensed Potions** (crafted by Alchemist only — lighter weight):

| Item | Effect | Weight |
|------|--------|--------|
| Condensed Red Potion | Heals 45–65 HP | 1 |
| Condensed Yellow Potion | Heals 175–235 HP | 2 |
| Condensed White Potion | Heals 325–405 HP | 2 |

### 6.7 Gemstones (Skill Catalysts)

| Gemstone | NPC Price | Skills That Consume It |
|----------|-----------|----------------------|
| Blue Gemstone | ~600z | Warp Portal, Safety Wall, Pneuma, Sage Endow skills |
| Red Gemstone | Exchange only | Stone Curse, Venom Dust, Sage Endow skills |
| Yellow Gemstone | Exchange only | Volcano, Deluge, Violent Gale, Land Protector, Dispell, Abracadabra |

Gemstone Exchange NPC (Payon): 2 Blue = 1 Red, 2 Red = 1 Yellow, 2 Yellow = 1 Blue.

Mistress Card (headgear) eliminates gemstone consumption but adds +25% SP cost.

### 6.8 Arrows (Archer Ammo)

Archer-line classes (Archer, Hunter, Bard, Dancer, Rogue with bow) must equip arrows to attack with bows. Arrows are consumed one per normal attack and per skill use. Different arrow types provide different attack elements.

| Arrow | Element | NPC Price | Weight | Source |
|-------|---------|-----------|--------|--------|
| Arrow | Neutral | 1z | 0.1 | NPC |
| Silver Arrow | Holy | 3z | 0.2 | NPC |
| Fire Arrow | Fire | 3z | 0.2 | NPC / Arrow Crafting |
| Crystal Arrow | Water | — | 0.2 | Arrow Crafting |
| Stone Arrow | Earth | — | 0.2 | Arrow Crafting |
| Wind Arrow (of Wind) | Wind | — | 0.2 | Arrow Crafting |
| Immaterial Arrow | Ghost | — | 0.2 | Arrow Crafting |
| Shadow Arrow (of Darkness) | Shadow | — | 0.2 | Arrow Crafting |
| Cursed Arrow | Curse status | — | 0.2 | Arrow Crafting |
| Rusty Arrow | Neutral | — | 0.2 | Arrow Crafting (chance to cause Stun) |
| Oridecon Arrow | Neutral | — | 0.2 | Arrow Crafting (higher ATK) |
| Steel Arrow | Neutral | — | 0.2 | Arrow Crafting (higher ATK) |
| Iron Arrow | Neutral | 2z | 0.2 | NPC |

Arrow Crafting is a Hunter quest skill that converts items into arrows (e.g., Red Blood → Fire Arrow, Crystal Blue → Crystal Arrow).

### 6.9 Identify System

Some equipment dropped by monsters comes as **Unidentified** (shown with a "?" icon). Unidentified items cannot be equipped until identified via:
- **Magnifier** item: Purchased from NPC for 40z. Consumed on use.
- **Item Appraisal** skill: Merchant class. Free, no consumable.

### 6.10 Equipment Breaking

Weapons and armor can break during gameplay, rendering them useless (0 stats) until repaired:

**Break sources**:
- Power Thrust (Blacksmith buff): 0.1% chance per hit to break the user's weapon
- Acid Terror (Alchemist): chance to break target's armor
- Demonstration (Alchemist): chance to break target's weapon
- Certain monster attacks
- **Failed refinement**: Equipment is outright destroyed (not repairable)

**Repair**: Blacksmith's Repair Weapon skill restores broken equipment. Destroyed equipment (from failed refine) is gone permanently.

**Protection**: Alchemist's Chemical Protection skills (Helm/Shield/Armor/Weapon) prevent breaking and stripping for the duration.

### 6.11 Notable Equipment

| Item | Type | Level | Notable Effect |
|------|------|-------|---------------|
| Stiletto [1] | Dagger (Lv1) | — | Popular early slotted dagger |
| Main Gauche [4] | Dagger (Lv1) | — | 4 card slots, very popular for carding |
| Katana [3] | 2H Sword (Lv1) | — | 3 slots, Knight Bowling Bash builds |
| Pike [3] | Spear (Lv1) | — | 3 slots, Pierce builds |
| Composite Bow [4] | Bow (Lv1) | — | 4 slots, Archer/Hunter staple |
| Orcish Axe [4] | Axe (Lv3) | — | 4 slots, Blacksmith staple |
| Chain [3] | Mace (Lv2) | — | 3 slots |
| Jur [3] | Katar (Lv3) | — | 3 slots, Assassin staple |
| Fist [3] | Knuckle (Lv2) | — | 3 slots, Monk |
| Buckler [1] | Shield | — | Popular slotted shield |
| Mink Coat [1] | Garment | — | Slotted garment |
| Boots [1] | Footgear | — | Slotted footgear |
| Clip [1] | Accessory | — | Slotted accessory |

---

## 7. Enemies & Opponents

Complete MVP stats and drops in: [docs/ragnarok-online/mvps.md](docs/ragnarok-online/mvps.md).

### 7.1 Monster Properties

Every monster has: Level, HP, SP, ATK (min–max), DEF (hard + soft), MDEF (hard + soft), 6 base stats (STR/AGI/VIT/INT/DEX/LUK), Element (type + level 1–4), Race, Size, movement speed, attack range, attack delay, and AI behavior flags.

### 7.2 Monster Races (10)

| Race | Examples | Countered By |
|------|----------|-------------|
| Formless | Poring, Angeling, Stormy Knight | — |
| Undead | Zombie, Skeleton, Drake, Osiris | Holy element, Heal, Turn Undead, Magnus Exorcismus |
| Brute | Wolf, Peco Peco, Eddga, Phreeoni | Beast Bane (Hunter) |
| Plant | Mandragora, Flora, Geographer | — |
| Insect | Thief Bug, Andre, Maya, Mistress | Beast Bane (Hunter) |
| Fish | Hydra, Marc, Swordfish | — |
| Demon | Baphomet, Dark Lord, Doppelganger | Demon Bane, Divine Protection (Acolyte) |
| Demi-Human | Orc, Kobold, Orc Hero, Pharaoh | Hydra Card (+20%) |
| Angel | Angeling, Arc Angeling | — |
| Dragon | Petite, Sidewinder, Mutant Dragonoid | Dragonology (Sage) |

### 7.3 Monster Sizes

| Size | Weapon Advantage | Weapon Disadvantage |
|------|-----------------|-------------------|
| Small | Dagger (100%), Bow (100%), Knuckle (100%), Staff (100%) | Axe (50%), 1H Sword (75%) |
| Medium | 1H Sword (100%), Mace (100%), Katar (100%) | Dagger (75%) |
| Large | 2H Sword (100%), Spear (100%), Axe (100%), Mace (100%) | Dagger (50%), Katar (75%), Bow (75%) |

### 7.4 Monster AI Behaviors

Monsters have bitwise behavior flags:

| Behavior | Description |
|----------|-------------|
| Passive | Won't attack unless provoked |
| Aggressive | Attacks players on sight within detection range |
| Looter | Picks up items from ground when idle |
| Assist | Helps nearby same-type monsters that are being attacked |
| Cast Sensor | Targets players who cast spells nearby |
| Change Target (Melee) | Switches to player that hits it during combat |
| Change Target (Chase) | Switches to closer player during pursuit |
| Target Weak | Only aggressive to players 5+ levels below it |
| Detector | Can see Hidden/Cloaked players |
| Boss Protocol | Immune to certain status effects and knockback |
| MVP Protocol | MVP reward system, tomb on death |

### 7.5 MVP System

MVPs are the strongest monsters in the game. See [docs/ragnarok-online/mvps.md](docs/ragnarok-online/mvps.md) for full stats.

- **Boss Protocol**: Immune to Coma, Stone Curse, Freeze, Sleep, and most crowd-control status effects. Immune to knockback
- **MVP Reward**: Player dealing most total damage receives MVP announcement + 3 special drop slots (separate from normal drops) + bonus EXP
- **Respawn**: Fixed base timer + random variance after death. Ranges from 27 minutes (Amon Ra) to 310 minutes (Tao Gunka)
- **Slave summons**: Many MVPs summon helper monsters (e.g., Baphomet summons Dark Lord slaves, Orc Lord summons Orc Warriors)
- **MVP cards**: 0.01% drop rate, most powerful cards in the game (e.g., Baphomet Card = splash on normal attacks, GTB Card = block all magic)

---

## 8. Economy

### 8.1 Currency

**Zeny**: Sole in-game currency. Max per character: 2,000,000,000.

Primary sources: selling monster drops to NPCs, direct zeny drops from monsters, player-to-player trading.

Primary sinks: Mammonite skill, refining fees, Kafra services, NPC consumables, guild creation (Emperium).

### 8.2 NPC Shops

- **NPC buy price**: Listed price on item
- **NPC sell price**: Exactly **50%** of buy price
- Every town has: Tool Dealer (consumables), Weapon Dealer, Armor Dealer
- Specialty shops vary by town (magic items in Geffen, nautical in Alberta)

### 8.3 Player Vending

Merchant class with Vending skill can open a player shop:
- Requires Pushcart (items sold from cart inventory)
- Maximum item slots: 3–12 (scales with Vending level)
- Maximum price per item: 99,999,999z
- Merchant sits on ground with chat bubble showing shop name
- Other players click to browse and buy
- No auction house or centralized market — vending is entirely manual and location-based

### 8.4 Merchant Economy Skills

**Discount** (buy from NPC cheaper): 7–24% reduction at Lv1–10.
**Overcharge** (sell to NPC higher): 7–24% bonus at Lv1–10.

With both at Lv10: Buy at 76% price, sell at 124% price — combined effect increases NPC trade margin from 50% to ~81%.

### 8.5 Player Trading

Direct player-to-player trading via a trade window:
- Both players must agree to initiate the trade (requires Basic Skill Lv 1)
- Trade window shows both sides with item slots and a zeny field
- Both players must click "OK" then "Trade" to confirm
- Can trade items and zeny simultaneously
- No cross-server trading. No mail system in v1.0

### 8.6 Blacksmith Forging

Blacksmith class can forge weapons from raw materials. Forged weapons have the crafter's character name permanently displayed.

**Materials required**: Base ores (Iron, Steel, Iron Ore) + weapon-type-specific additional materials. Adding **Elemental Stones** during forging imbues a permanent element:
- Flame Heart → Fire weapon
- Mystic Frozen → Water weapon
- Rough Wind → Wind weapon
- Great Nature → Earth weapon

Adding **Star Crumbs** during forging grants flat ATK bonuses (+5 per Star Crumb, max 3).

**Success rate** depends on DEX, LUK, Job Level, and relevant forging skill levels. Failed forging consumes all materials. Blacksmith forging creates Lv1–3 weapons (Daggers, Swords, 2H Swords, Axes, Maces, Knuckles, Spears).

### 8.7 Item Drops

Each monster has **8 normal drop slots** + **1 card slot** (9th). MVP monsters additionally have **3 MVP-exclusive drop slots** given only to the MVP winner.

Drop rate mechanics:
- Each slot rolls independently (0.01%–100% per slot, varies per item)
- **Bubble Gum** item: 2× drop rates for 30 minutes
- **Old Card Album**: Consumable that generates a random non-MVP card

---

## 9. Multiplayer

### 9.1 Party System

- **Max party size**: 12 players
- **Creation**: Requires Basic Skill Lv 7
- **Level range for Even Share**: Members must be within **15 base levels** of each other and on the same map

**EXP sharing modes**:

| Mode | How It Works |
|------|-------------|
| **Each Take** | Each member only gets EXP for monsters they personally damaged. No sharing, no bonus |
| **Even Share** | EXP pooled and divided equally among same-map members. Bonus: +25% total EXP per additional member (2 players = 125% / 2, 3 = 150% / 3, etc.) |

### 9.2 Guild System

- **Creation**: Requires an **Emperium** item (consumed, not refunded)
- **Default capacity**: 16 members (including guild master)
- **Max capacity**: Up to 76 via Guild Extension skill
- **Guild level cap**: 50 (levels gained from member EXP tax)
- **Guild skill points**: 1 per guild level

**Key guild skills**:

| Skill | Effect |
|-------|--------|
| Official Guild Approval | Enables WoE participation (attack Emperiums) |
| Guild Extension | +4 max members per level (up to 10 levels) |
| Kafra Contract | Hires Kafra NPCs in owned castles |
| Guardian Research | Allows hiring Guardian monsters to defend castles |
| Battle Command | Active buff (Guild Master only) |

### 9.3 PvP Arenas (Episode 2+)

- Accessible via Inns in cities. Entry fee: 500z
- **Level requirement**: Base Level 31+
- **Scoring**: Start with 5 points. +1 per kill, −5 per death. Kicked out at 0
- **Rankings**: Top 10 get visual indicators beneath their characters
- **Yoyo Mode** (standard): No EXP loss, no item drops
- **Nightmare Mode**: EXP loss + 1% item drop on death

### 9.4 War of Emperium (Episode 4)

Guild vs guild castle siege system. Occurs during scheduled time windows (server-dependent, typically 2 hours).

**Structure**:
- 4 realms: Prontera (Valkyrie), Geffen (Britoniah), Payon (Greenwood Lake), Aldebaran (Luina)
- 5 castles per realm = **20 castles total**

**Objective**: Destroy the defending guild's **Emperium** to claim the castle. Defend it until the WoE period ends to keep it.

**Emperium stats**:
- Level 90, HP 100, Race: Angel, Size: Small, Element: Holy Lv1
- Takes only 1 damage from ALL sources
- Only normal attacks can damage it — all skills miss
- Holy element attacks deal 0%. Shadow/Undead deal bonus damage
- Gives 0 EXP

**WoE rules**:
- Guild and allied members cannot attack each other inside castles
- Castle defenders can place Guardians (NPCs) and Barricades
- The guild landing the final hit on the Emperium claims ownership
- Skills like Teleport and Fly Wing are disabled inside WoE castles

**Castle benefits for owning guild**:
- Access to exclusive Guild Dungeon with high-level monsters
- Treasure chest spawns (Zeny/items) daily, scaling with castle economy investment

---

## 10. UI & HUD

### 10.1 HUD Layout

- **HP/SP bars**: Bottom-left. Numerical display with percentage fill. HP bar color shifts green → yellow → red
- **Base/Job EXP bars**: Bottom of screen, horizontal. Shows percentage to next level
- **Minimap**: Top-right corner. Shows current map with player dot, party member dots, NPC dots, and warp portal indicators
- **Chat window**: Bottom of screen. Tabbed for All/Party/Guild/Whisper/Battle Log
- **Hotkey bar**: Bottom-center. F1–F9 slots for skills and items. Two rows (toggle with Alt). Multiple sets (cycle with F12)
- **Character name + guild**: Above player sprite
- **Status icons**: Below minimap. Shows active buffs/debuffs as small icons with remaining duration

### 10.2 HUD States

- **Normal**: Full HUD visible
- **NPC dialogue**: Modal dialogue window center-screen. Choices presented as clickable text
- **Dead**: Screen grays out. "Return to Save Point" button appears
- **Vending**: Merchant sits with shop chat bubble. Browser window shows items/prices
- **Chat room**: Hovering chat room window with text input and occupant list

### 10.3 Menu Screens

| Menu | Shortcut | Contents |
|------|----------|----------|
| Equipment | Alt+Q | Paper-doll view with 10 equipment slots. Drag-and-drop equipping |
| Inventory | Alt+E | Tabbed: Equipment, Consumable, Etc. Weight display. Drag to equip or use |
| Skill | Alt+S | Skill tree view. Drag skills to hotkey bar. Skill point allocation interface |
| Status | Alt+A | Stats display. Stat point allocation buttons. Derived stats (ATK, DEF, HIT, FLEE, ASPD, etc.) |
| Map | Ctrl+Tab | Full world map showing city locations |
| Party | Alt+Z | Party member list with HP bars, class icons |
| Guild | Alt+G | Guild info, member list, positions, emblem, skill tree, notice board |
| Friends | Alt+H | Friend list with online status |
| Options | — | Sound, music, effect toggles. Chat filter. |

### 10.4 Damage Display

- **White numbers**: Normal damage dealt (physical and magical)
- **Yellow numbers**: Critical hit damage
- **Green numbers**: HP healed
- **"Miss"**: Attack dodged (FLEE or Perfect Dodge)
- **"Lucky"**: Attack dodged via Perfect Dodge specifically
- Numbers float upward from the target and fade
- Damage taken by the player uses a distinct color/position from damage dealt

### 10.5 Interaction Prompts

- **NPC interaction**: Click NPC → dialogue window with "Next" / numbered choices
- **Monster targeting**: Click monster → character walks to range and begins attacking
- **Player interaction**: Right-click player → context menu (Trade, Party Invite, Whisper, Check Equipment, Block)

---

## 11. Engine & Presentation

### 11.1 Visual Style

- **2.5D isometric**: Pre-rendered 3D terrain/environments with 2D sprite characters
- **Character sprites**: 8-directional facing. Animated for idle, walk, attack, casting, dead, sitting
- **Sprite customization**: Hair style, hair color, cloth color (palette-swapped based on class). Equipment visually changes headgear and weapon sprite
- **Skill effects**: 2D animated overlays (fire pillars, ice crystals, lightning, holy crosses)

### 11.2 Camera

- **Fixed isometric angle**: ~30° downward tilt, 45° rotation
- **Player-controlled rotation**: Right-click + drag rotates camera around character in 45° increments
- **Zoom**: Scroll wheel or PgUp/PgDn, limited range
- **No free-look or first-person mode**

### 11.3 Audio System

- **BGM**: Each map has an assigned background music track. Music changes on map transition. Town themes, field themes, dungeon themes
- **SFX**: Skill sound effects, attack sounds, ambient sounds per environment
- **No voice acting** (original v1.0)
- Volume controls: Separate BGM and SFX sliders

### 11.4 Save System

- **Persistent server-side**: All character data (stats, inventory, equipment, position, quest progress) stored on game server
- **Save point**: Set via Kafra NPC or spawned at on death
- **No manual save/load**: MMORPG — character state is always live
- **Multiple characters**: Up to ~12 characters per account per server (varies by server configuration)

### 11.5 Dialogue System

- **NPC dialogue**: Modal windows with text and choices. Sequential "Next" pages for long dialogue
- **Quest tracking**: No quest log in original v1.0 — players must remember or reference external guides
- **Emoticons**: Alt+1 through Alt+0 display emote icons above character head

### 11.6 Networking

- **Client-server architecture**: Client sends inputs, server validates all actions
- **Tick rate**: Server processes at ~100ms intervals
- **Persistent world**: Maps exist continuously; all players share the same map instances
- **No instancing**: All content is open-world (instances added in much later episodes)
- **Channels**: Later implementations added multiple channels per map for population management, but original v1.0 was single-channel

---

## 12. Open Questions / Unverified

1. **Exact ASPD formula**: Multiple formulations exist in community sources and server emulators (rAthena vs Hercules). The pre-renewal ASPD formula involves weapon delay tables per class/weapon combo that vary between emulators. The general shape (AGI primary, DEX secondary, weapon-class table) is consistent but exact coefficients differ.

2. **Episode 1 class availability**: Sources conflict on whether 2-1 second classes (Knight, Wizard, etc.) were available at the August 2002 commercial launch or only from Episode 2 (December 2002). The beta had them, but the commercial launch content boundary is ambiguous.

3. **Original card drop rate**: While 0.01% is the standard rate on modern official and private servers, some sources suggest early kRO may have had slightly different rates. The 0.01% figure comes from rAthena defaults and iRO Wiki.

4. **Exact stat point formula at character creation**: The 48-point total with paired axes (STR↔INT, etc.) is documented, but exact minimum/maximum per stat at creation and the constraint formula vary between sources (some say 1–9, others 1–12).

5. **Pre-renewal Hard DEF formula**: Two competing formulas exist:
   - Simple: `Damage × (100 − DEF) / 100` (direct percentage)
   - Complex: `Damage × (4000 + DEF) / (4000 + DEF × 10)` (asymptotic)
   The asymptotic formula is from rAthena and handles high DEF values better; the simple formula may have been the original.

6. **Homunculus availability**: The Homunculus system may have been added slightly after the 2-2 classes. Exact kRO episode boundary is unclear.

7. **WoE treasure chest mechanics**: Exact number of treasure chests per castle tier, contents, and economic investment mechanics are poorly documented for original v1.0.

8. **Status effect exact formulas**: Duration and success rate formulas for many status effects (Freeze, Stun, etc.) involve VIT/LUK resistance checks whose exact coefficients vary between sources.

9. **Element table exact values**: The element tables in this spec are sourced from rAthena's `db/pre-re/attr_fix.yml`. Community wiki sources (iRO Wiki, RateMyServer) report different values for several cells — particularly Ghost vs Neutral at Lv1 (25% in rAthena vs 70% in some wikis) and Earth self-resistance at Lv1 (100% in rAthena vs 25% in some wikis). The directional cycle (Water → Fire → Earth → Wind → Water) is consistent across all sources, but exact multipliers for edge cases should be verified against the official kRO client or packet data.

10. **Pet system exact bonuses**: Pet accessory bonuses vary between official servers and private servers. The values listed are from iRO Wiki and may differ from original kRO v1.0 values.

11. **Natural recovery exact formulas**: HP/SP recovery tick rates and amounts vary between sources. The formulas given are approximate — exact coefficients depend on class, base level, and server configuration.

---

## 13. References

### Wikis
- [iRO Wiki](https://irowiki.org/wiki/) — Primary reference for pre-renewal mechanics
- [iRO Wiki Classic](https://irowiki.org/classic/) — Classic/pre-renewal specific pages
- [Ragnarok Fandom Wiki](https://ragnarok.fandom.com/) — Episode timeline, lore, locations
- [StrategyWiki — Ragnarok Online](https://strategywiki.org/wiki/Ragnarok_Online)

### Community Databases
- [RateMyServer](https://ratemyserver.net/) — Monster DB, skill DB, refinement tables, equipment search
- [Divine Pride](https://www.divine-pride.net/) — Monster/item databases with kRO data
- [RagnaPlace](https://ragnaplace.com/) — iRO Wiki mirror with additional tools

### Emulator Source Code
- [rAthena (GitHub)](https://github.com/rathena/rathena) — Open-source server emulator. Pre-renewal database files in `db/pre-re/`. Definitive source for formulas in `src/map/status.cpp`, `src/map/battle.cpp`
- [Hercules (GitHub)](https://github.com/HerculesWS/Hercules) — Alternative emulator with similar pre-renewal support
- [Ragnarok Research Lab](https://ragnarokresearchlab.github.io/) — Client reverse-engineering documentation

### Companion Files
- [Class Skill Reference](docs/ragnarok-online/skills.md) — Full skill tables for all 19 classes
- [Element Damage Tables](docs/ragnarok-online/elements.md) — Complete 10×10 tables for defense levels 1–4
- [MVP & Mini-Boss Reference](docs/ragnarok-online/mvps.md) — All boss stats, locations, drops, and spawn mechanics
