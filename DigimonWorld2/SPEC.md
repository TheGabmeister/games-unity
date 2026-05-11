# Digimon World 2 — Gameplay Systems Spec

PlayStation 1, 2000 (Japan: July 27, 2000; NA: May 19, 2001). Developed by BEC. Published by Bandai.

---

## 1. Core Gameplay Systems

### 1.1 Primary Loop

Digimon World 2 is a dungeon-crawling RPG. The player controls Akira, a Digimon Tamer who pilots a tank called the Digi-Beetle through grid-based dungeon floors (Domains), fights wild Digimon in 3v3 turn-based battles, and fuses Digimon through DNA Digivolution to build stronger parties.

**Loop:** Accept mission at Guard Team HQ &rarr; equip/upgrade Digi-Beetle at Device Dome &rarr; load items from Server to Beetle inventory &rarr; enter Domain &rarr; explore grid floors in Digi-Beetle &rarr; fight wild Digimon, disarm traps, open chests &rarr; defeat floor boss &rarr; exit via portal &rarr; return to Digital City &rarr; rank up via Colosseum &rarr; repeat.

### 1.2 Guard Team Selection

At game start the player joins one of three Guard Teams, which determines the starter Digimon, initial recruitment restrictions, and DNA Digivolution type restrictions (lifted after Mission 5).

| Team | Attribute | Starter | Leader | Restriction |
|------|-----------|---------|--------|-------------|
| Gold Hawk | Vaccine | Agumon | Vandar | Cannot DNA Digivolve with Data types initially |
| Blue Falcon | Data | Patamon | Cecilia | Cannot DNA Digivolve with Virus types initially |
| Black Sword | Virus | DemiDevimon | Skull | Cannot DNA Digivolve with Vaccine types initially |

### 1.3 Naming Limits

- Player name: 5 characters
- Digi-Beetle name: 7 characters
- Digimon name: 13 characters

---

## 2. Digimon Raising

### 2.1 Stats

Every Digimon has five stats:

| Stat | Abbreviation | Role |
|------|-------------|------|
| HP | HP | Health points |
| MP | MP | Used by techniques; partially restored by Guard action |
| Offense | Att | Physical/technique damage output |
| Defense | Def | Damage reduction |
| Speed | Spd | Turn order priority; minor randomness at close values |

### 2.2 Evolution Levels (EL)

| Stage (JP) | Stage (EN) | EL Threshold |
|------------|------------|-------------|
| Child | Rookie | Start |
| Adult | Champion | EL 11 |
| Perfect | Ultimate | EL 21 |
| Ultimate (JP) | Mega | EL 31 |

This spec uses English stage names throughout. Note the JP/EN naming collision: Japanese "Ultimate" = English "Mega".

Each normal Digivolution adds a flat **+30 HP** and **+30 MP**.

### 2.3 Stat Growth

Each Digimon species has a growth rating per stat: **Low**, **Normal**, or **High**.

- HP/MP share one growth formula. Offense/Defense share another. Speed has its own.
- Normal and High growth share the same maximum per-level gain, but High has a higher probability of rolling the maximum.
- If a Digimon is not Digivolved as soon as it hits the EL threshold, Offense and Defense gains drop to 0-1 per level regardless of growth rating.

Sample growth ranges (Ultimate stage, High rating, EL 22-31):
- HP/MP: +4-5 per level
- Att/Def: +2-5 per level (higher early, settling to 2-3)
- Speed: +2-3 per level (when total Speed &le; 50)

### 2.4 Level Cap & Max Level

- Wild-caught Digimon have low max levels (~13-15 for Rookies).
- The **only** way to raise max level is DNA Digivolution (see &sect;3).
- Absolute max level: **999** (reachable through many successive DNA Digivolutions).

### 2.5 Leveling

- Leveling up **fully restores HP and MP**.
- EXP is awarded to all 3 active party members after battle.
- BIT (currency) is also awarded per battle.

### 2.6 Experience Table

| EL | XP for Level | Cumulative XP |
|----|-------------|---------------|
| 1 | 0 | 0 |
| 2 | 6 | 6 |
| 3 | 10 | 16 |
| 5 | 24 | 57 |
| 10 | 95 | 369 |
| 11 | 114 | 483 |
| 15 | 300 | 1,281 |
| 21 | 1,200 | 5,883 |
| 25 | 1,800 | 11,880 |
| 31 | 4,500 | 31,080 |
| 35 | 5,740 | 51,640 |
| 40 | 9,940 | 91,740 |
| 45 | 17,140 | 161,840 |
| 50 | 27,440 | 277,040 |
| 55 | 40,540 | 452,040 |

### 2.7 Recruitment (Gift System)

Wild Digimon are recruited using **Gift** items before battle. Gifts are typed and graded:

**Type-specific gifts:**

| Type | Grade E (100 BIT) | Grade A (3,400 BIT) |
|------|-------------------|---------------------|
| Data | Card Game | Laptop PC |
| Vaccine | Wristwatch | DVD Player |
| Virus | Kickboard | Surfboard |

**All-type gifts:**

| Gift | Price | Grade |
|------|-------|-------|
| Toy Car | 500 | E |
| Toy Boat | ~1,500 | D |
| Toy Plane | 8,000 | A |

- Multiple gifts can be stacked on one enemy.
- A heart icon appears above the target: small, medium, or large heart indicates capture probability. At maximum affinity, capture chance is approximately **7/8 (~87.5%)**.
- Must be **4+ squares** away from the Digimon to give a gift.
- The **last Digimon killed** in battle is the one that may offer to join.
- Capture can still fail even at large heart.
- Early game: only your Guard Team's specialty type can be recruited. This restriction lifts after Mission 5.
- All caught Digimon start at **0 DP**.

---

## 3. DNA Digivolution (Jogress)

The core progression mechanic. Two Digimon permanently fuse into one new Digimon. This is **mandatory** for progression — Digimon hit their level cap and cannot gain EXP until DNA Digivolved.

### 3.1 Result Stage

The resulting Digimon drops one or more evolution tiers:

| Parent 1 | Parent 2 | Result Stage | Starting EL |
|----------|----------|-------------|-------------|
| Champion | Champion | Rookie | 1 |
| Champion | Ultimate | Rookie | 1 |
| Champion | Mega | Rookie | 1 |
| Ultimate | Ultimate | Champion | 11 |
| Ultimate | Mega | Champion | 11 |
| Mega | Mega | Ultimate | 21 |

### 3.2 Max Level Formula

```
New Max EL = Higher Parent's Current EL + Floor(Lower Parent's Current EL / 5)
```

Uses each parent's **current level at the time of fusion**, not their max level. Optimal strategy is to grind both parents to their respective max levels before fusing.

Example: Greymon (EL 15, maxed) + Airdramon (EL 13, maxed) = 15 + Floor(13/5) = 15 + 2 = **EL 17 max**.

### 3.3 DP (Digivolving Points)

```
New DP = Highest Parent DP + 1
```

DP determines which form a Digimon Digivolves into at each stage. Can be manually adjusted with **DNA UpChip** (+1 DP) and **DNA DnChip** (-1 DP). High DP thresholds (e.g., DP 20+) unlock rare evolutions like Omnimon and Diaboromon.

### 3.4 Type Dominance

The resulting Digimon's type follows dominance rules:

| Combination | Result | Rule |
|-------------|--------|------|
| Vaccine + Vaccine | Vaccine | Same type |
| Data + Data | Data | Same type |
| Virus + Virus | Virus | Same type |
| Vaccine + Virus | Vaccine | Vaccine dominates Virus |
| Data + Vaccine | Data | Data dominates Vaccine |
| Virus + Data | Virus | Virus dominates Data |

The new Digimon resembles the parent of the dominant type.

### 3.5 Result Species Determination

The resulting Digimon's species is determined by a fixed lookup table mapping every possible pair of parent species to a result. This is independent of DP — DP only affects future Digivolution paths, not the DNA fusion result itself. The full combination table (DigimonCombine.json) contains thousands of pairings.

### 3.6 Stat & Technique Inheritance

- Stats are **averaged** from both parents and carried to the result, making it substantially stronger than a wild-caught Digimon of the same species at the same level.
- The result inherits **all techniques** from both parents (up to the **12-technique cap**).

---

## 4. Digivolution Paths

Each Digimon's evolution is determined by its DP value. DP ranges map to specific target forms. When a Digimon reaches the required EL (11 for Champion, 21 for Ultimate, 31 for Mega), it Digivolves into the form matching its current DP.

### 4.1 Example Evolution Lines

**Agumon (Vaccine, Rookie):**
```
Agumon -> Greymon (Champion)
  Greymon -> MetalGreymon (DP 0-5) or MasterTyrannomon (DP 6-8) or SkullGreymon (DP 9+) (Ultimate)
    MetalGreymon -> WarGreymon (DP 0-19) or Omnimon (DP 20+) (Mega)
```

**Patamon (Data, Rookie):**
```
Patamon -> Ninjamon (DP 0-2) | Starmon (DP 3-5) | Wizardmon (DP 6-7) | Angemon (DP 8+)
  Angemon -> Andromon (DP 0-5) | MagnaAngemon (DP 6+) (Ultimate)
    Andromon -> Seraphimon (Mega)
    MagnaAngemon -> Seraphimon (Mega)
```

**DemiDevimon (Virus, Rookie):**
```
DemiDevimon -> IceDevimon (DP 0-3) or Devimon (DP 4+) (Champion)
  Devimon -> Myotismon (Darkness, Ultimate)
    Myotismon -> V-Myotismon (Mega)
```

The complete evolution path table (216 entries) with DP ranges is in [docs/DigimonWorld2/evolution-paths.md](docs/DigimonWorld2/evolution-paths.md).

---

## 5. Combat System

### 5.1 Structure

3-on-3 turn-based battles. The player selects 3 Digimon from their Digi-Beetle roster (up to 4-12 carried, depending on RAM) as the active battle party via the Digi-Line menu during dungeon exploration. Swapping cannot be done mid-battle. **No auto-retargeting:** if a target dies before your attack resolves, the attack is wasted.

**KO mechanics:** When a Digimon's HP reaches 0, it is KO'd and cannot act. KO'd Digimon can be revived mid-battle with the **Crimson Flame** Assist tech (full HP revival, 50 MP) or type-specific **ROM** items. If all 3 active Digimon are KO'd, the battle is lost and the player is returned to Digital City.

### 5.2 Action Types

Each Digimon selects one action per turn:

| Action | Description |
|--------|-------------|
| **Attack (technique)** | Use an offensive technique. Costs MP. Subdivided into 4 tech types (see &sect;5.3). |
| **Guard** | Reduces incoming damage by **40%** and restores **10% of max MP**. |
| **Item** | Use a battle item. |
| **Run** | Attempt to flee. Not always successful. |

The Digi-Beetle's mounted weapons can also be fired from a separate **Cannon** menu (see &sect;6.5).

### 5.3 Technique Types

| Type | Turn Priority | Behavior |
|------|--------------|----------|
| **Attack** | Normal (Speed-based) | Standard offensive move, may have bonus effects (status, AoE). |
| **Counter** | After Attack-type moves | Deals bonus/special effects only when the user is attacked that turn. Misses if not triggered. |
| **Interrupt** | Before target's action | Pre-emptive strike. Cannot be used if the target also uses an Interrupt. |
| **Assist** | Normal | Non-damaging. Buffs allies, debuffs enemies, heals, cures status. |

### 5.4 Technique Learning

- Each Digimon species has exactly **1 signature technique**.
- A Digimon can know up to **12 techniques** total.
- Techniques accumulate through Digivolution and DNA Digivolution — the resulting Digimon inherits all parent techniques (up to the 12-slot cap).
- This is the primary way to build a versatile moveset.

### 5.5 Elemental System (Specialty)

Five elements plus None. Each element deals **1.2x** damage to the next in the cycle and **0.8x** damage to the element two steps ahead.

| Element | Deals 1.2x to | Deals 0.8x to | Takes 1.2x from |
|---------|---------------|---------------|-----------------|
| Fire | Nature | Machine | Water |
| Nature | Machine | Darkness | Fire |
| Machine | Darkness | Water | Nature |
| Darkness | Water | Fire | Machine |
| Water | Fire | Nature | Darkness |
| None | — | — | — |

Cycle: **Fire &rarr; Nature &rarr; Machine &rarr; Darkness &rarr; Water &rarr; Fire**.

### 5.6 Attribute Triangle

Separate from elemental matchups. Applies to the attacking and defending Digimon's types:

| Advantage | Multiplier |
|-----------|-----------|
| Vaccine vs Virus | 1.2x |
| Virus vs Data | 1.2x |
| Data vs Vaccine | 1.2x |
| Disadvantaged | 0.8x |
| Neutral / same | 1.0x |

### 5.7 Damage Formula

```
Damage = Floor(
  Floor(TypeBonus * SpecialtyBonus * AttackPower * TileBonus_Attacker)
  * AttackerOffense
  / Floor(DefenderDefense * TileBonus_Defender)
)
```

Where `Floor()` = truncate to integer (drop decimals).

| Variable | Description | Values |
|----------|-------------|--------|
| TypeBonus | Attribute triangle (Vaccine/Data/Virus) | 0.8, 1.0, 1.2 |
| SpecialtyBonus | Element of skill vs element of target | 0.8, 1.0, 1.2 |
| AttackPower | Per-technique power value | Varies (e.g., Terra Force = 60.0, Puppet Pummel = 50.0) |
| TileBonus | Elemental floor tile matching | 0.8, 1.0, 1.2 |

**Tile Bonus:** Colored dungeon floor tiles boost matching elements. If the tile matches the attack's element, attacker gets 1.2x. If the tile matches the defender's element, defender gets 1.2x defense. Neutral = 1.0x.

Tile colors: Green = Nature, Red = Fire, Blue = Water, Black = Darkness, Gold = Machine.

### 5.8 Sample Techniques (by power tier)

| Technique | Type | Element | Power | MP | Target | Notable Effect |
|-----------|------|---------|-------|----|--------|----------------|
| Blue Blaster | Attack | Fire | 10.0 | 4 | One Enemy | — |
| Pepper Breath | Attack | Fire | 17.5 | 8 | One Enemy | — |
| Nova Blast | Attack | Fire | 27.5 | 12 | One Enemy | — |
| Flower Cannon | Attack | Nature | 35.0 | 16 | One Enemy | — |
| Energy Blast | Attack | Machine | 37.5 | 60 | All Enemies | — |
| Terra Force | Attack | None | 60.0 | 40 | One Enemy | Highest standard single-target |
| X-Scissor Claw | Attack | None | 55.0 | 40 | One Enemy | — |
| Puppet Pummel | Attack | Machine | 50.0 | 32 | One Enemy | No Counter |
| Fire Tornado | Attack | None | 45.0 | 80 | All Enemies | — |
| Giga Cannon | Attack | Machine | 30.0 | 60 | Random Enemy | Attacks till MP runs out |
| Shadow Scythe | Attack | Darkness | 37.5 | 20 | One Enemy | Re-attacks if foe defeated |
| SubzeroIcePunch | Attack | Water | 25.0 | 10 | One Enemy | Each use adds 2.5 power |
| Horn Buster | Interrupt | None | 32.5 | 20 | One Enemy | Lowers Offense greatly |
| Electro-Shocker | Interrupt | Nature | 22.5 | 12 | One Enemy | Lowers attack 20% |
| Venom Infusion | Interrupt | None | 0.0 | 40 | One Enemy | Prevents enemy attack |
| GAIA Gear | Counter | None | 67.5 | 48 | One Enemy | Counter-only; misses if not triggered |
| Smiley Warhead | Counter | Machine | 40.0 | 40 | All Enemies | Counter 1.5x damage |
| Full HP Cure | Assist | None | — | 80 | All Allies | Full HP all allies |
| Crimson Flame | Assist | None | — | 50 | One Ally | Full HP revival (even if KO'd) |
| Invincibility | Assist | None | — | 20 | One Ally | Nullifies all attacks |
| Kongou | Assist | None | — | 16 | Self | Voids all damage |

The complete technique list (203 techniques) is in [docs/DigimonWorld2/techniques.md](docs/DigimonWorld2/techniques.md).

### 5.9 Status Effects

| Status | Effect | Inflicted By | Cured By |
|--------|--------|-------------|----------|
| Poison | HP drains each turn; higher-stage Digimon lose more per tick | Nature/Insect-type attacks (Fungus Cruncher, Needle Spray, Poison Ivy) | Anti-Dote (300 BIT); Recovery Power (Assist tech) |
| Confusion | Digimon attacks itself or allies | Various techs (Evil Charm, Sonic Crusher, Buffalo Breath) | Anti-Mixup (300 BIT); Recovery Power tech |
| Paralysis | Attacks miss most of the time; may lose entire turn | Electric/Machine attacks (E-Stun Blast, Stun Flame Shot, Thunder Ball counter) | Anti-Freeze (300 BIT) |
| Stun | Lose turn entirely | Fire attacks (Fire Tower), certain Counter moves | Wears off after 1 turn; Recovery Power tech |

Additional debuffs:
- **Motivation reduction** — prevents use of strongest techniques. Distinct from standard status effects. Inflicted by Concert Crush, Pretty Attack, Ocean Love.
- **Stat reduction** — 20% reduction to Offense, Defense, or Speed per application. Inflicted by Tidal Wave (Att), Coral Crusher/Evil Wind (Spd), Duo ScissorClaw/Scissor Claw (Def), Destabilizer Ray (Def all), Reduction Ray (Att).

---

## 6. Digi-Beetle

A tank vehicle used to traverse dungeon floors. Managed at the Digi-Beetle Garage in Digital City.

### 6.1 Resources

| Resource | Description | Depletion Effect |
|----------|-------------|-----------------|
| HP | Hull integrity. Damaged by traps (mines, electro-spores). | If 0, auto-return to city. |
| EP | Movement fuel. Costs **1 EP per grid square moved**. | If 0, auto-return with burnt-out engine. |

### 6.2 Body Chassis (Story Upgrades)

| # | Body | Source |
|---|------|--------|
| 1 | Steel Body | Default |
| 2 | Titanium Body | Titanium Core (Modem Domain reward, Mission 6) |
| 3 | Adamantium Body | Adamantium Core (File Island reward, Mission 13) |

Each body enables compatibility with better components.

### 6.3 Parts

**Engines (determines Beetle HP):**

| Part | HP | Cost (BIT) |
|------|-----|-----------|
| Wolf EG-1 | 800 | 1,000 |
| Bear EG-1 | 1,200 | 3,000 |
| Rhino EG-1 | 1,600 | 5,000 |
| Elephant EG-1 | 2,400 | 15,000 |
| Dragon EG-1 | 3,200 | 45,000 |
| Giant EG-1 | 3,600 | 95,000 |
| Mammoth EG-1 | 4,000 | 99,000 |
| Maximus EG | Best | Post-game (Tera Domain) |

**Batteries (determines EP / movement range):**

| Part | EP | Cost (BIT) |
|------|-----|-----------|
| Crab BAT-1 | 100 | 500 |
| Turtle BAT-1 | 500 | 2,000 |
| Whale BAT-1 | 1,000 | 5,000 |
| Orca BAT-1 | 3,000 | 10,000 |
| Whale BAT | Best | Post-game (Tera Domain) |

**RAM (Digimon capacity):**

| Part | Slots | Cost (BIT) |
|------|-------|-----------|
| Ant RAM | 4 | 300 |
| Spider RAM | 5 | 3,000 |
| Bee RAM | 6 | 8,000 |
| Scorpion RAM | 7 | 14,000 |
| Hornet RAM | 8 | 20,000 |
| Beetle RAM | 12 | Post-game (Tera Domain) |

**Item Box (inventory capacity):**

| Part | Slots | Cost (BIT) |
|------|-------|-----------|
| Dodo BOX | 8 | 500 |
| Penguin BOX | 16 | 3,000 |
| Condor BOX | 24 | 8,000 |
| Owl BOX | 32 | 14,000 |
| Hawk BOX | 40 | 20,000 |
| Eagle BOX | 48 | Post-game (Tera Domain) |

**Tires (acid swamp traversal, by color):**

| Part | Cost (BIT) | Neutralizes |
|------|-----------|-------------|
| Ring | 2,000 | Yellow swamps |
| Bigfoot | 6,000 | + Blue swamps |
| Chain | 12,000 | + Green swamps |
| Hover | 18,000 | + Red swamps |
| Gravi | 22,000 | + Purple swamps |

**Arms (mine detection/disarming):**

| Part | Cost (BIT) |
|------|-----------|
| Shovel | 3,000 |
| Drill | 8,000 |
| Hammer | 14,000 |
| Cutter | 18,000 |
| Magnum | 23,000 |

**Hands (treasure chest security levels):**

| Part | Cost (BIT) |
|------|-----------|
| Mech | 1,000 |
| Silver | 4,000 |
| Gold | 8,000 |
| Platinum | 12,000 |
| Ultra | 16,000 |

**Weapons:**

| Part | Cost (BIT) | Function |
|------|-----------|----------|
| Shooter Gun | 1,000 | Basic attack |
| Bug Zapper | 3,000 | Removes bugs |
| Missile Gun | 4,000 | Ranged attack |
| R-Cannon | 5,000 | Stronger attack |
| Z-Cannon | 6,000 | Strongest attack |

**Sweeps:**

| Part | Cost (BIT) | Function |
|------|-----------|----------|
| MineSWEEP-1 | 1,000 | Detects mines |
| BugSWEEP-1 | 2,000 | Detects Bug Nests |

**Other:**

| Part | Cost (BIT) | Function |
|------|-----------|----------|
| DMTransfer | 10,000 | Transfers Digimon to/from server mid-dungeon |

### 6.4 Dungeon Hazards

**Mines** — Damage Beetle HP, can disable parts. Better Arms = easier disarming.

**Electro-Spores** (5 colors by strength) — Energy barriers blocking passage. Destroyed with Magnetic Missiles (300-1,500 BIT by color) or Wave Missiles (500-2,500 BIT, destroy spores AND rocks).

**Big Rocks** (5 colors) — Block pathways. Destroyed with Drill Missiles (300-1,500 BIT) or Wave Missiles.

**Acid Swamps** (5 colors: Yellow through Purple) — Damage Beetle HP per step. Tire upgrades neutralize specific colors.

**Treasure Chests** — Some are trapped. Better Hands = access to more secure chests. Failed disarm can damage Beetle parts.

**Bugs** (4 types, 3 strength tiers each):

| Bug Type | Effect | Tiers |
|----------|--------|-------|
| Bit Bug | Drains BIT per step | Yellow, Blue, Red |
| Memory Bug | Occupies a Digimon slot | Yellow, Blue, Red |
| EP Bug | Drains EP per step | Yellow, Blue, Red |
| Return Bug | Sends a Digimon back to Digital City | Yellow, Blue, Red |

Bugs emerge from Bug Nests (invisible without BugSWEEP). Removed with type-specific Bug Zap items (3 tiers: 300/600/900 BIT) or with the Bug Zapper weapon.

### 6.5 Digi-Beetle Combat

From the battle menu's Cannon sub-menu, the Digi-Beetle's mounted weapons can fire:

| Ammo | Cost (BIT) | Target |
|------|-----------|--------|
| Z-Bomb | 300 | Single enemy |
| Z-Bomb-A | 900 | All enemies |
| Ray Bombs | 300 each | Stat boost/lower (various) |

---

## 7. World Structure

### 7.1 Three Continents

| Continent | Access | Description |
|-----------|--------|-------------|
| Directory Continent | Start | Contains Digital City hub; most story Domains |
| File Island | Mid-game (Ship Key + Navi-Disk) | Four main Domains + three Chaos General Domains |
| Kernel Zone | Endgame (Chief Tamer rank) | Core Tower + Chaos Tower (final dungeon) |

### 7.2 Digital City Hub

| Location | Function |
|----------|----------|
| Guard Team HQ (x3) | Mission briefings, type-specific shops |
| Device Dome | General shop, Digi-Beetle upgrades, DNA Digivolution lab |
| Colosseum | Tournament battles for rank progression |
| Digimon Center | Digimon storage (Server), NPC trades |
| Tamer's Club | Social hub, story events |
| Meditation Dome | Sanctuary; Angemon provides story guidance |
| Archive Port | Transport to File Island (unlocked mid-game) |
| Shuttle Port | Transport to Kernel Zone (unlocked endgame) |

### 7.3 Domain Structure

Domains are multi-floor grid-based mazes. Floors are mostly **randomly generated** (except tutorial and certain fixed floors).

- Map reveals progressively as explored (green = explored rooms).
- Floor portals advance to next level. **No backtracking** to previous floors.
- Boss floors identified by wall pattern changes and colored hallways.
- After defeating the boss, an Exit Portal appears.

### 7.4 Domain Progression

**Act 1 — Training (Directory Continent):**

| Domain | Floors | Boss(es) | Mission |
|--------|--------|----------|---------|
| Boot Domain | ~2 | Leomon (tutorial) | 1 |
| SCSI Domain | 4 | Hagurumon | 2 |
| Video Domain | 5 | Kokatorimon | 3 |
| Disk Domain | 4 | Numemon, Sukamon, DemiDevimon | 3 |
| BIOS Domain | 6 | Centarumon, Starmon, Gabumon | 4 |
| Drive Domain | 7 | Birdramon, Flarizamon, Candlemon | 5 |
| Web Domain | 7 | Kuwagamon, Gesomon | 5 |
| Modem Domain | 8 | Greymon + Kim (Wizardmon, Seadramon, Akatorimon) | 6 |

**Act 2 — Blood Knights Invasion (Revisited Domains):**

| Domain | Floors | Boss(es) | Mission |
|--------|--------|----------|---------|
| SCSI Domain | 6 | Darkrizamon, Bakemon, Soulmon | 7 |
| Video Domain | 7 | Raremon, Devimon, Soulmon | 7 |
| Disk Domain | 7 | Bertran + Commander Damien teams | 7 |
| BIOS/Web/Drive | 8-9 | Blood Knight commanders | 8 |
| DVD Domain | ~8 | Ben Oldman | 9 |
| Code Domain | 11 | Commander Damien (Cherrymon, Etemon) | 10 |
| Laser Domain | 12 | BK Officer, Damien, Crimson (SkullMammothmon, Deltamon, ExTyrannomon) | 11 |
| Power Domain | varies | Tough enemies; Ship Key obtained | 12 |

**Act 3 — File Island:**

| Domain | Floors | Boss(es) | Mission |
|--------|--------|----------|---------|
| Diode Domain | 13 | BK Commander (Bakemon, Devidramon, Phantomon) | 13 |
| Giga Domain | ~14 | BK Commander; requires Electroder item | 13 |
| Port Domain | ~14 | BK Commander (SkullMeramon, Flarizamon, Mamemon) | 13 |
| Scan Domain | 15 | BK Commander (Pumpkinmon, Woodmon, Kiwimon) | 13 |

**Act 4 — Chaos Generals (File Island):**

| Domain | Floors | Boss | Specialty Floor |
|--------|--------|------|----------------|
| Patch Domain | 15 | **ChaosBlackWarGreymon** + Triceramon, Scorpiomon | Fire |
| Mega Domain | 15 | **ChaosMetalSeadramon** + Megadramon, Gigadramon | Water |
| Data Domain | 15 | **ChaosPiedmon** + Giromon, Andromon (+ 6 tamer sub-bosses) | Nature |

Collecting all 3 Chaos Rings reveals Soft Domain.

**Act 5 — Soft Domain & Late Game:**

| Domain | Floors | Boss(es) | Mission |
|--------|--------|----------|---------|
| Soft Domain | 16+ | **Chaos Lord** (HP 747) + Crimson | 15 |
| Bug Domain | 17 | 3 midbosses + Commander Damien | 16 |
| RAM Domain | 17 | 3 midbosses + Commander Damien | 16 |
| ROM Domain | 17 | **NeoCrimson** (HP 825, highest in game) | 17 |

**Act 6 — Kernel Zone:**

| Domain | Floors | Boss |
|--------|--------|------|
| Core Tower | 24 | Guardian (varies by team) |
| Chaos Tower | 20 | **OverLord GAIA** (final boss, 2 phases) |

**Post-Game:**

| Domain | Floors | Notes |
|--------|--------|-------|
| Tera Domain | 99 | All Mega-level encounters. F98: breather floor (no enemies, Toy Planes in chests). F99: 6 chests with stat Chips. Best Beetle parts obtainable. Kimeramon allegedly 1/666 chance on F99 (unverified). |

---

## 8. Story & Progression

### 8.1 Premise

Akira joins a Guard Team in Digital City to protect the Digital World. The Blood Knights — a rogue tamer faction led by Crimson — invade, seeking to control OverLord GAIA, a computer system that created the Digital World to study digital evolution.

### 8.2 Key NPCs

| NPC | Role |
|-----|------|
| Akira | Protagonist (default name) |
| Crimson | Blood Knights leader; transforms into NeoCrimson |
| Commander Damien | Blood Knights second-in-command; bomb plot against Digital City |
| Esteena | Mysterious amnesiac girl; reveals truth about GAIA |
| Kim | Thief; sister Techna-Donna upgrades Digi-Beetle |
| Angemon | Advisor at Meditation Dome |
| Jijimon | File Island leader, rescued mid-game |
| Professor Piyotte | Researcher; needs DNA Patches |

### 8.3 Blood Knights Hierarchy

1. **OverLord GAIA** — true antagonist; computer controlling the Digital World
2. **Crimson** — former Digital City tamer; becomes NeoCrimson (fused with DNA of all 3 Chaos Generals + Chaos Lord)
3. **Commander Damien** — boastful second-in-command
4. Various Blood Knight Commanders/Officers

### 8.4 Major Boss Stats

| Boss | HP | Location | Key Attacks |
|------|-----|----------|-------------|
| Chaos Lord | 747 | Soft Domain F16 | Chaos Cannon (Machine, 1-3 shots/turn) |
| NeoCrimson | 825 | ROM Domain (final floor) | Fungus Cruncher (Counter), GigaScissorClaw (Interrupt), Blind Attack |
| OverLord GAIA Phase 1 | 432 | Chaos Tower F20 | GAIA Gear (Counter, 67.5 power), Titan Laser (All, 47.5), Fantasmic Ray (All, 45.0), Fantasmic Bomb (All, 42.5) |
| OverLord GAIA Phase 2 | 648 (body) + 270/arm (x2) | Chaos Tower F20 | Body: Tubular Attack (Counter, 47.5 all). Arms: Reduction Ray (-20% Att), Destabilizer Ray (-20% Def all) |

---

## 9. Items & Equipment

### 9.1 Recovery Items

| Item | Cost (BIT) | Effect |
|------|-----------|--------|
| HP Disk-1 | 200 | Restores 40 HP to one |
| HP Disk-2 | 400 | Restores 80 HP to one |
| HP Disk-3 | 800 | Restores 160 HP to one |
| HP Driver-1 | 500 | Restores 40 HP to all |
| HP Driver-2 | 1,000 | Restores 80 HP to all |
| HP Driver-3 | 2,000 | Restores 160 HP to all |
| MP Disk-1 | 300 | Restores 40 MP to one |
| MP Disk-2 | 600 | Restores 80 MP to one |
| MP Disk-3 | 1,200 | Restores 160 MP to one |
| MP Driver-1 | 800 | Restores 40 MP to all |
| MP Driver-2 | 1,600 | Restores 80 MP to all |
| MP Driver-3 | 3,000 | Restores 160 MP to all |
| EX Driver | 2,000 | Full HP to one |
| Max Driver | 3,000 | Full HP to all |

### 9.2 ROM Items (Type-Specific Full Heals)

| Item | Effect |
|------|--------|
| V-ROM | Full HP + MP restore and revive for one Vaccine Digimon |
| D-ROM | Full HP + MP restore and revive for one Data Digimon |
| HP-ROM (VAC/DAT/VIR) | Full HP restore for one Digimon of matching type |

Available from Guard Team HQ shops (each team sells ROMs matching their attribute).

### 9.3 Status Cure Items

| Item | Cost (BIT) | Effect |
|------|-----------|--------|
| Anti-Dote | 300 | Cures Poison |
| Anti-Freeze | 300 | Cures Paralysis |
| Anti-Mixup | 300 | Cures Confusion |

### 9.4 Stat Chips (Permanent Boosters)

| Chip | Effect |
|------|--------|
| HP Chip | Permanent HP increase |
| MP Chip | Permanent MP increase |
| Power Chip | Permanent Offense increase |
| Armor Chip | Permanent Defense increase |
| Speed Chip | Permanent Speed increase |
| EXP Chip | Boosts experience gained |
| DNAup-Chip | Raises DP by 1 |
| DNAdn-Chip | Lowers DP by 1 |

Obtained from treasure chests, Colosseum rewards, and story rewards. Tera Domain F99 contains 6 chests each with a different stat Chip.

### 9.5 Dungeon Items

| Item | Cost (BIT) | Effect |
|------|-----------|--------|
| EP Pack | varies | Restores Digi-Beetle EP |
| Parts Fix | varies | Repairs one broken Beetle part |
| MechFix-EX | varies | Repairs all broken Beetle parts |
| Auto Pilot | 1,000 | Safe return to city (does not count as clearing Domain) |
| Magnetic Missiles | 300-1,500 | Destroy Electro-Spores (by color) |
| Drill Missiles | 300-1,500 | Destroy Big Rocks (by color) |
| Wave Missiles | 500-2,500 | Destroy both Spores and Rocks |
| Bug Zap items | 300/600/900 | Remove bugs (by type and tier) |

### 9.6 Key Items

| Item | Purpose |
|------|---------|
| Entry Pass | Unlocks Colosseum (after BIOS Domain) |
| Navi-Disk | Navigation data for File Island |
| DNA Patches (VAC/VIR/DATA) | Required for virus cure storyline |
| Ship Key | Unlocks File Island travel |
| Electroder | Restores light in Giga Domain |
| Chaos Rings (x3) | One from each Chaos General; combined to reveal Soft Domain |
| Titanium Core | Upgrades Digi-Beetle to Titanium Body |
| Adamantium Core | Upgrades Digi-Beetle to Adamantium Body |

Total items in game: **161**.

---

## 10. Economy

### 10.1 Currency

**BIT** is the sole currency.

### 10.2 Income Sources

| Source | Amount |
|--------|--------|
| Early battles | ~50-210 BIT |
| Mid-game battles | ~500-1,000 BIT |
| Late-game battles | ~1,000-5,460 BIT |
| Selling items | 50% of buy price (capped at 4,999 BIT) |
| Colosseum rewards | Items (not direct BIT) |
| Treasure chests | Variable |

### 10.3 Major Expenses

| Category | Range (BIT) |
|----------|-------------|
| Digi-Beetle engines | 1,000 - 99,000 |
| Digi-Beetle batteries | 500 - 10,000 |
| Digi-Beetle RAM | 300 - 20,000 |
| Tires | 2,000 - 22,000 |
| Item Boxes | 500 - 20,000 |
| Gift items | 100 - 8,000 |
| Healing items | 200 - 3,000 |
| Colosseum entry | 500 - 4,000 |
| Auto Pilot return | 1,000 |

### 10.4 Shops

| Shop | Location | Sells |
|------|----------|-------|
| Guard Team HQ | Each team's base | Type-specific gifts, ROMs |
| Device Dome | Digital City | General items, Beetle parts, DNA Digivolution |
| Item Vendor | Digital City | HP/MP Disks, status cures, EP Packs |
| Ammo Vendor | Digital City | Shooter ammo, missiles, cannon items |

---

## 11. Colosseum & Tamer Ranks

### 11.1 Rank Progression

Unlocked after BIOS Domain (Mission 4) via the Entry Pass. Each tournament is 3 rounds.

| Rank | Title | Entry Fee (BIT) | Reward |
|------|-------|----------------|--------|
| 1 | Beginner | — | Starting rank |
| 2 | Amateur | 500 | HP Driver-1 |
| 3 | Rookie | 1,000 | Toy Boat |
| 4 | Normal | 1,500 | DNAup-Chip |
| 5 | Pro | 2,000 | Toy Plane |
| 6 | Expert | 2,500 | EX Driver |
| 7 | Elite | 3,000 | Max Driver |
| 8 | Commander | 3,500 | Attack Chip |
| 9 | Chief | 4,000 | EXP Chip |

### 11.2 Chief Tournament (Rank 9)

The final tournament is a gauntlet — fight all 3 Guard Team leaders back-to-back with **no healing between fights** and **no item usage**:

| Order | Leader | Team | Digimon |
|-------|--------|------|---------|
| 1 | Skull | Black Sword | Hagurumon, DemiDevimon, Puppetmon |
| 2 | Vandar | Gold Hawk | MetalMamemon, Mamemon, P-Mamemon |
| 3 | Cecilia | Blue Falcon | Jijimon, Magnadramon, MarineAngemon |

Reaching Chief Tamer rank (9) is **required** to access ROM Domain and the Kernel Zone endgame.

---

## 12. Side Systems

### 12.1 Trading

Located at the Digimon Center. An NPC offers specific trades that unlock progressively:

| Give | Receive | When Available |
|------|---------|----------------|
| ToyAgumon | SnowAgumon | After Video/Disk Domains |
| Crabmon | Wizardmon | After BIOS Domain |
| Numemon | Megadramon | After Modem Domain |
| Garurumon | MagnaAngemon | After Blood Knights in SCSI/Video/Disk |
| N-Drimogemon | MetalMamemon | After Code Domain |

### 12.2 Battle Mode (Multiplayer)

A 2-player versus mode accessible from the title screen via PS1 link cable. Each player selects 3 Digimon from their save file and battles under standard combat rules. The soundtrack includes dedicated Battle Mode, VS Screen, and Duel tracks.

### 12.3 Post-Game: Tera Domain

- 99-floor bonus dungeon; all Mega-level encounters from the start.
- Floor 98: breather floor (no enemies); every chest contains a Toy Plane.
- Floor 99: 6 treasure chests, each containing a different stat Chip.
- Best Digi-Beetle parts obtainable here (Maximus EG, Whale BAT, Beetle RAM, Eagle BOX).
- **Kimeramon** (ID 181, Data/Darkness) exists in game data but has an alleged 1/666 chance to appear on F99 — never verified through legitimate play.

---

## 13. Controls & Input

PS1 controller. Story mode is single-player. A 2-player **Battle Mode** exists via PS1 link cable (see &sect;12.2).

### 13.1 Dungeon Navigation

| Button | Action |
|--------|--------|
| D-Pad | Move Digi-Beetle one grid square per press |
| X | Confirm, investigate adjacent square, open chests (each action = 1 turn, advancing enemy movement) |
| O | Open menu / Select in menus / Disarm traps |
| Square | Examine nearby enemy Digimon (displays type, species, EL, count); opens gift menu |
| Triangle | Cancel / Back / Menu access |
| Start | Enhanced map view |
| Select | Toggle minimap display |
| L1/R1 | Stop Digi-Beetle |

### 13.2 Battle Navigation

| Button | Action |
|--------|--------|
| D-Pad | Navigate menu options |
| X | Confirm selection |
| Triangle | Cancel / Back |

Target selection is manual. **No auto-retargeting.**

---

## 14. UI & HUD

### 14.1 Dungeon Exploration HUD

- **Top-left:** Domain name and current floor number
- **Top-right:** Digi-Beetle HP and EP display
- **Right side:** Minimap (reveals progressively as explored, green = explored rooms)
- **Center:** Digi-Beetle on the dungeon grid (top-down/overhead perspective)
- **Floor colors:** Brown = standard; Green = boss chamber; Colored patterns = elemental specialty (Red/Blue/Green/Black/Gold)

### 14.2 Battle HUD

- 3D polygon battle scene showing up to 3 Digimon per side
- **Top-level menu:** Give Orders | Cannon | Run Away
- **Under "Give Orders":** Each Digimon assigned one of: Attack / Counter / Interrupt / Assist technique, Guard, or Item
- Turn order displayed based on Speed stat

### 14.3 Menu Screens

Accessed via O button:

| Screen | Function |
|--------|----------|
| Status | Character, vehicle, Digimon stats |
| Items | Inventory management |
| Digimon | Individual Digimon data |
| Digi-Line | Battle formation (set which 3 fight) |
| Transfer | Move items/Digimon between Beetle and Server |
| Save / Auto Pilot | Save (city only) / Return to city (dungeons) |

---

## 15. Engine & Presentation Systems

### 15.1 Save System

- Can **only save** in Digital City or on the world map. **No mid-dungeon save.**
- 3 save slots per memory card.
- In dungeons, the Save option becomes **Auto Pilot** — safe return to city. Fully heals all Digimon and revives KO'd ones, but costs 1,000 BIT and does **not** count as clearing the Domain.

### 15.2 Camera

- **Dungeons:** Fixed top-down/overhead perspective on a 2D grid. The Digi-Beetle is centered. Camera does not rotate.
- **Battles:** Angled 3D perspective showing both teams. Camera is fixed.
- **City:** Top-down/overhead view, navigating between labeled buildings.

### 15.3 Dialogue System

- Text-box dialogue with character portraits.
- Binary Yes/No choices for most interactions.
- Story progression gated by speaking to specific NPCs (Guard Team leader after completing Domain missions).
- Guard Team choice at game start is permanent and irreversible.

### 15.4 Difficulty

No selectable difficulty setting. Fixed difficulty curve.

- Extremely steep leveling curve — most random encounters award fewer than 100 EXP early on.
- Average playthrough: **60-90 hours** (much of this is grinding).
- Level caps force DNA Digivolution to progress.
- Encounter sprites are visible on dungeon floors — not truly random, but often unavoidable due to room size.
- Enemy movement speed varies: most move 1 square per player step, some move 2 per step, some move 1 per 2 player steps, some flee.
- Battle animations are unskippable and notably slow — characters pose and announce attacks every time. Widely considered the game's biggest pacing issue.

### 15.5 Audio

36 tracks total (~2 hours). Each Guard Team base, hub building, and domain group has its own theme. Battle music changes by encounter type:

| Context | Track |
|---------|-------|
| Wild encounter | Wild Digimon Battle |
| Early bosses | First Boss Theme |
| Later bosses | Second Boss Theme |
| Guardian | Guardian Battle |
| GAIA Phase 1 | Overlord Gaia 1 |
| GAIA Phase 2 | Overlord Gaia 2 |
| Victory | Victory fanfare |

No official soundtrack release; all available versions are gamerips.

---

## 16. Digimon Roster

195 Digimon total (including boss-only entries).

| Stage | Count | ID Range |
|-------|-------|----------|
| Rookie | 31 | 1-31 |
| Champion | 72 | 32-103, 182 |
| Ultimate | 50 | 104-153 |
| Mega | 28 | 154-181 |
| Boss-only | 13 | 183-195 |

**Attribute distribution:**
- Vaccine: ~55 Digimon
- Data: ~60 Digimon
- Virus: ~67 Digimon

**Element distribution:**
- None: ~80 Digimon
- Darkness: ~30 Digimon
- Machine: ~25 Digimon
- Nature: ~25 Digimon
- Water: ~25 Digimon
- Fire: ~10 Digimon

The complete roster with types, elements, and evolution paths is in [docs/DigimonWorld2/evolution-paths.md](docs/DigimonWorld2/evolution-paths.md).

---

## 17. Wild Encounters

### 17.1 Overworld Behavior

- Wild Digimon appear as visible sprites on the dungeon grid; they move toward the Digi-Beetle.
- Movement is turn-based: each player step triggers one enemy movement tick.
- **Enemy speed varies by species:** most move 1 square per player step, some move 2 (e.g., Bakemon, Gururumon), some move 1 per 2 player steps, and some move away from the player.
- Contact with an enemy sprite initiates battle. Encounters are **not avoidable** in small rooms.
- Each domain floor has a fixed encounter group table defining which species appear, at what levels, with which skills, and how much EXP/BIT they award.

### 17.2 Battle Encounter Rules

- Encounters are 3v3: player's 3 active Digimon vs 1-3 wild Digimon.
- Some enemies cannot be fled from (e.g., Bakemon).
- The **last Digimon defeated** in battle is the candidate for recruitment if gifts were given (see &sect;2.7).
- Boss encounters are scripted: specific Digimon at fixed levels on designated floors. Boss floors are visually distinct (colored hallways, different wall patterns).

---

## 18. Open Questions / Unverified

- **Exact stat Chip boost amounts:** Sources discuss HP/MP/Power/Armor/Speed chip values but specific numbers not confirmed.
- **Kimeramon encounter:** Exists in game data (DigimonId 181) but the alleged 1/666 encounter rate on Tera Domain F99 has never been verified through legitimate play. Only confirmed via save hacking.
- **Exact floor counts** for DVD Domain, Power Domain, and Giga Domain: not explicitly stated in accessible sources; estimates based on progression patterns.
- **Guard action MP restoration:** Confirmed as 10% of max MP by multiple sources, but some guides cite flat values.
- **Speed tie-breaking:** Minor randomness confirmed, but exact formula for tie resolution at equal Speed values is unknown.
- **Stat growth formulas:** Growth ratings (Low/Normal/High) and general ranges are known, but the exact RNG roll formula per level-up is not documented outside the decompilation project.
- **Capture probability formula:** Heart size correlates with capture chance (~87.5% at max), but the exact calculation from gift grade + type match + Digimon level is not fully documented.
- **Ultimates without Mega paths:** Several Ultimate-stage Digimon (Triceramon, Vermilimon, MasterTyrannomon, and others) have no Mega evolution entry in the datamined DigimonEvolution table. Whether they are true evolutionary dead ends or simply reach Mega forms only through DNA Digivolution needs verification.

---

## 19. References

### Datamined Sources
- [MetalKid's DW2 Database (GitHub)](https://github.com/MetalKid/DigimonWorld2_Database) — Complete raw game data in JSON: Digimon stats, skills, evolution tables, DNA combination tables, encounter groups, experience curves, specialty matchups
- [MetalKid's DW2 Website](https://dev.metalkid.info/DigimonWorld2/) — Interactive calculators for damage, DNA Digivolve, skill lookup
- [DW2 Decompilation (GitHub)](https://github.com/nmfernan/DMW2_decomp) — Early-stage decompilation targeting SLUS_011.93
- [DW2-TT Utility Tool (GitHub)](https://github.com/acemon33/DW2-TT) — Save editing, EXP/BIT multiplier, file extraction
- [The Cutting Room Floor](https://tcrf.net/Digimon_World_2) — Unused domains, unused File City graphics, hidden variants

### Wikis & Guides
- [Wikimon — Digimon World 2](https://wikimon.net/Digimon_World_2)
- [Wikipedia — Digimon World 2](https://en.wikipedia.org/wiki/Digimon_World_2)
- [StrategyWiki — Digimon World 2](https://strategywiki.org/wiki/Digimon_World_2)
- [LP Archive — Digimon World 2](https://lparchive.org/Digimon-World-2/)
- [RPG Classics FAQ](http://alexandria.rpgclassics.com/PSX/digimonworld2/)
- [Almar's Guides — DW2 Walkthrough](https://almarsguides.com/retro/walkthroughs/PS1/Games/DigimonWorld2/FullPlaythrough/)
- [Neoseeker — DW2 Damage Formula](https://www.neoseeker.com/forums/1084/t1601646-figured-out-damage-formula/)

### Companion Documents
- [docs/DigimonWorld2/techniques.md](docs/DigimonWorld2/techniques.md) — Complete technique list (203 techniques with power, MP, type, element, target, effects)
- [docs/DigimonWorld2/evolution-paths.md](docs/DigimonWorld2/evolution-paths.md) — Complete Digimon roster and evolution paths with DP ranges
