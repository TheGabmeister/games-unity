# Final Fantasy Tactics — Gameplay Systems Spec

Final Fantasy Tactics, PlayStation 1, 1997 (Square). Tactical RPG.

**Version note:** This spec covers the original PS1 release (US). War of the Lions (PSP, 2007) and Ivalice Chronicles (multiplatform remaster, 2025) differences are noted where relevant. PS1 localization names are used with WotL equivalents in parentheses on first mention.

---

## 1. Core Gameplay Systems

### 1.1 Primary Loop

Battles on isometric 3D grid maps. Between battles: party management, shops, world map travel. Story-driven linear progression with optional random encounters on the world map.

### 1.2 Turn System — Charge Time (CT)

FFT uses a **tick-based CT system**, not strict turns.

- Every **clock tick**, each unit's CT increases by their **Speed** stat.
- When a unit's CT reaches **100 or higher**, that unit gets an **Active Turn (AT)**.
- If multiple units reach 100+ CT on the same tick, the unit with the higher CT acts first. Speed ties are broken by deployment order (formation slot).

**CT reduction after acting:**

| Action Taken | CT Cost |
|---|---|
| Move + Act | -100 CT |
| Move only (no Act) | -80 CT |
| Act only (no Move) | -80 CT |
| Wait (neither Move nor Act) | -60 CT |

Excess CT above the threshold is **not** carried over — CT is set to the remainder after subtraction (e.g., if CT was 108 and you Move+Act, CT becomes 8).

**Haste:** CT gain per tick = Speed × 1.5 (some sources say doubled; verified as +50% in datamined data). Duration: 32 clock ticks.

**Slow:** CT gain per tick = Speed × 0.5 (halved, rounded down). Duration: 24 clock ticks.

**Quick (Time Magic):** Immediately sets the target's CT to 100, granting an instant turn.

**Stop:** CT is frozen; the unit cannot act, evade, or use reactions. Duration: 20 clock ticks.

### 1.3 Spell Charge Times (Slow Actions)

Many abilities (magic, summons) have a **Cast Speed** value. When a unit begins casting, a separate countdown determines when the spell resolves:

**Clock ticks until resolution = ceil(100 / Spell Speed)**

Examples:
- Fire (Speed 25): ceil(100/25) = 4 ticks
- Fire 2/Fira (Speed 20): ceil(100/20) = 5 ticks
- Fire 3/Firaga (Speed 15): ceil(100/15) = 7 ticks
- Fire 4/Firaja (Speed 10): ceil(100/10) = 10 ticks
- Bahamut (Speed 10): 10 ticks

While charging, the unit enters a **"Readying"** state:
- Takes **50% additional physical damage**
- Has **zero evasion bonuses**
- The spell targets a **panel**, not a unit — targets can move away before it resolves
- Charging is **not affected** by the caster's Speed, Haste, or Slow (exception: Jump)

**Swiftness (Short Charge) support ability:** Halves charge time (ceil value halved).

### 1.4 Action Economy Per Turn

Each Active Turn allows:
1. **Move** — traverse up to Move stat in panels
2. **Act** — use one ability (Attack, Item, job ability, etc.)
3. **Wait** — end turn, choose facing direction

Move and Act can be done in either order. A unit may choose to skip Move, Act, or both.

**Defend:** Instead of acting, a unit can choose **Defend** to double all evasion rates until their next turn. This consumes the Act phase.

### 1.5 Facing

Each unit faces one of four cardinal directions. Facing is chosen at the end of the turn (during Wait). Facing affects evasion (see §3) and some ability interactions.

---

## 2. Damage Formulas

### 2.1 Physical Attack — Weapon Type Formulas

Physical damage is calculated as **XA × WP**, where XA varies by weapon type:

| Weapon Type | XA (Attack Stat) | Formula |
|---|---|---|
| **Sword, Crossbow, Spear/Polearm** | PA | PA × WP |
| **Rod** | PA | PA × WP (Rods boost MA when equipped, but basic attack damage uses PA) |
| **Knight Sword, Katana** | (Br/100) × PA | [(Br/100) × PA] × WP |
| **Knife/Dagger, Bow, Ninja Blade** | (PA + Sp) / 2 | [(PA + Sp) / 2] × WP |
| **Staff, Stick/Pole** | MA | MA × WP |
| **Gun** | WP | WP × WP |
| **Axe, Flail, Bag** | Random(1..PA) | Random(1..PA) × WP |
| **Harp, Book, Cloth/Rug** | (PA + MA) / 2 | [(PA + MA) / 2] × WP |
| **Bare Hands (no Martial Arts)** | (Br/100) × PA | [(Br/100) × PA] × PA |
| **Bare Hands (with Martial Arts)** | (Br/100) × PA × 1.5 | [(Br/100) × PA] × PA × 1.5 |

Key:
- **PA** = Physical Attack stat
- **WP** = Weapon Power (equipment property)
- **Br** = Bravery stat (0-100)
- **Sp** = Speed stat
- **MA** = Magic Attack stat

**Bare hands:** When unarmed, the formula uses PA as both the multiplier and the "weapon power." Martial Arts (Monk support ability) adds a 1.5× multiplier.

### 2.2 Magical Damage Formula

**Damage = MA × Q × (Caster Faith / 100) × (Target Faith / 100)**

Where:
- **MA** = Caster's Magic Attack stat
- **Q** = Spell Power constant (per-spell value)
- **Faith** = 0-100 stat affecting magical interactions

Both caster and target Faith are multiplied, so low-Faith targets take less magical damage but also receive less magical healing.

**Spell Power (Q) values — Black Magic tier pattern:**

| Tier | PS1 Name | WotL Name | Q Value | MP Cost | Cast Speed |
|---|---|---|---|---|---|
| 1 | Fire / Ice / Bolt | Fire / Blizzard / Thunder | 14 | 6 | 25 |
| 2 | Fire 2 / Ice 2 / Bolt 2 | Fira / Blizzara / Thundara | 18 | 12 | 20 |
| 3 | Fire 3 / Ice 3 / Bolt 3 | Firaga / Blizzaga / Thundaga | 24 | 24 | 15 |
| 4 | Fire 4 / Ice 4 / Bolt 4 | Firaja / Blizzaja / Thundaja | 32 | 48 | 10 |

**Flare:** Q ~40, 60 MP, Speed 15. Non-elemental, single target, ignores Reflect.

**Summon Q values (verified):**

| Summon | Q | Element |
|---|---|---|
| Moogle (Mog) | 12 | None (healing) |
| Shiva | 24 | Ice |
| Ramuh | 24 | Lightning |
| Ifrit | 24 | Fire |
| Titan | 28 | Earth |
| Cyclops | 32 | Non-elemental |
| Odin | 40 | Non-elemental |
| Leviathan | 38 | Water |
| Salamander | 36 | Fire |
| Bahamut | 46 | Non-elemental |
| Zodiac (Zodiark) | 98 | Non-elemental |

**White Magic (Healing):** Uses the same base formula but restores HP. Holy uses a high Q value (~50), Speed 17, 56 MP, Holy element.

### 2.3 Magical Gun Formula

Magical guns bypass standard physical calculation:

**Damage = (Caster Faith / 100) × (Target Faith / 100) × WP × Q**

Where Q is rolled randomly:
- 60% chance: Q = 14
- 30% chance: Q = 18
- 10% chance: Q = 24

### 2.4 Geomancy Damage Formula

**Damage = [(PA + 2) / 2] × MA**

Geomancy ignores Faith entirely. It is also unaffected by Silence. 100% hit rate, no charge time, no MP cost. Zodiac compatibility and elemental affinity apply. Status effect chance: **25%** per attack.

### 2.5 Jump Damage Formula

- With Polearm: **(PA × WP) × 1.5**
- Without Polearm: **PA × WP**
- Barehanded: **PA × PA × (Brave / 100)**

Jump attacks are always non-elemental regardless of equipped weapon. They cannot be evaded, cannot be Reflected, and cannot be countered. While airborne, the unit is completely untargetable.

### 2.6 Damage Modifiers

**Support ability modifiers:**

| Ability | Effect |
|---|---|
| Attack Up (PA Save) | Physical damage × 4/3 (~+33%) |
| Magic Attack Up (MA Save) | Magical damage × 4/3 (~+33%) |
| Defense Up (PA Save) | Physical damage taken × 2/3 (~-33%) |
| Magic Defense Up (MA Save) | Magical damage taken × 2/3 (~-33%) |

**Status effect modifiers:**

| Status | Effect on Damage |
|---|---|
| Protect | Attacker's PA reduced by 1/3 for physical attacks |
| Shell | Caster's MA reduced by 1/3 for magical attacks |
| Berserk | PA increased by 50% (but forced to only use basic Attack) |
| Sleep | 1.5× physical damage taken |
| Chicken (Br < 10) | 1.5× physical damage taken |
| Toad | 1.5× physical damage taken |
| Oil | Fire damage is doubled |

### 2.7 Critical Hits

- **Chance:** ~5% on basic Attack command (also applies to Monk abilities, some sword abilities, and certain monster attacks)
- **Formula:** On critical, XA is replaced by XA + Random(1..XA) - 1
  - Minimum critical damage = normal damage
  - Maximum critical damage = (2 × XA - 1) × WP (slightly less than double)
- **Knockback:** Chance to push the target back one panel (if an unoccupied panel exists behind them). Knockback chance = Attacker Brave / (Attacker Brave + Target Brave).

### 2.8 Elemental Modifiers

Eight elements exist: **Fire, Ice, Lightning, Water, Wind, Earth, Holy, Dark**

| Affinity | Effect |
|---|---|
| **Weak** | Damage doubled (×2.0) |
| **Half** | Damage decreased by 50% (×0.5) |
| **Null (Negate)** | Damage reduced to 0 |
| **Absorb** | Damage converted to healing |
| **Boost** | Damage dealt with that element increased by 25% (×1.25) |

Modifiers **do not stack with themselves** (e.g., two sources of "Weak to Fire" still only apply 2×).

**Weather effects:**
- **Thunderstorm:** Fire damage -25%, Lightning damage +25%
- **Snowstorm:** Ice damage +25%

**Oil status:** Fire damage is doubled (removed after fire damage is taken).

**Float status:** Grants immunity to Earth-element attacks.

### 2.9 Zodiac Compatibility

Every unit has a zodiac sign. Compatibility between attacker and defender modifies damage, healing, and hit rates:

| Compatibility | Modifier |
|---|---|
| **Best** | ×1.50 (+50%) |
| **Good** | ×1.25 (+25%) |
| **Neutral** | ×1.00 (no change) |
| **Bad (Poor)** | ×0.75 (-25%) |
| **Worst** | ×0.50 (-50%) |

Zodiac compatibility is determined by the astrological wheel:
- **Good (Trine):** Signs 120° apart (e.g., Aries-Leo-Sagittarius, Taurus-Virgo-Capricorn)
- **Bad (Square):** Signs 90° apart (e.g., Aries-Cancer, Aries-Capricorn)
- **Best/Worst (Opposition):** Signs 180° apart — Best if opposite gender, Worst if same gender

Full compatibility chart in [docs/monsters.md](docs/monsters.md).

### 2.10 Height and Range

**Height does not directly modify damage.** However, height affects:

- **Bow range extension:** Maximum range = base range + floor((Caster Height - Target Height) / 2). Firing upward reduces range.
- **Melee vertical reach:** Most melee weapons can reach 2h up, 3h down. Polearms/Poles: 2 tile horizontal range with 3h up, 4h down.
- **Counter abilities:** Some reaction abilities (Counter) cannot trigger if the height difference exceeds a certain threshold.

---

## 3. Evasion System

### 3.1 Evasion Types

Four independent evasion sources:

| Type | Abbreviation | Source |
|---|---|---|
| **Class Evasion** | C-Ev | Innate to the job class |
| **Shield Evasion** | S-Ev | Equipped shield (physical and magical values) |
| **Accessory Evasion** | A-Ev | Equipped accessory (mantles/cloaks) |
| **Weapon Evasion** | W-Ev | Equipped weapon (only active with **Parry/Weapon Guard** reaction ability) |

### 3.2 Physical Evasion by Facing Direction

Evasion is **multiplicative** — each source is checked independently. The direction of the attack determines which evasion sources apply:

| Direction | C-Ev | S-Ev | A-Ev | W-Ev |
|---|---|---|---|---|
| **Front** | Yes | Yes | Yes | Yes |
| **Side** | **No** | Yes | Yes | Yes |
| **Back** | **No** | **No** | Yes | **No** |

**Hit% formula (physical, from front):**

```
Hit% = BaseHit% × (100 - C-Ev)/100 × (100 - S-Ev)/100 × (100 - A-Ev)/100 × (100 - W-Ev)/100
```

**Example:** 100% base hit vs. a unit with 10% C-Ev, 40% S-Ev, 25% A-Ev, no W-Ev:
- Front: 100 × 0.90 × 0.60 × 0.75 = 40.5% hit
- Side: 100 × 0.60 × 0.75 = 45% hit (no C-Ev)
- Back: 100 × 0.75 = 75% hit (only A-Ev)

### 3.3 Magical Evasion

Magical evasion **ignores facing direction**. Only S-Ev (magical shield evasion) and A-Ev (magical accessory evasion) apply. C-Ev and W-Ev do not apply to magic.

### 3.4 Evasion Feedback

- **"Missed!"** — attack evaded by C-Ev
- **"Guarded!"** — attack evaded by S-Ev or A-Ev
- **"Blocked!"** — attack evaded by W-Ev (Weapon Guard)

### 3.5 Class Evasion Values

| Job | C-Ev |
|---|---|
| Squire | 5% |
| Chemist | 5% |
| Knight | 10% |
| Archer | 10% |
| Monk | 20% |
| Thief | 25% |
| Ninja | 30% |
| Dragoon | 15% |
| Samurai | 15% |
| Geomancer | 10% |
| Mime | 5% |
| Most mage jobs | 5% |

**Blindness (Darkness) status:** All defender evasion chances are **doubled**.

### 3.6 Status Magic Hit Rate

**Base hit rate = ability base% + caster MA, then × (Caster Faith / 100) × (Target Faith / 100).**

If result exceeds 100%, subtract target's total magical evasion, capped at 100%.

---

## 4. Stat System

### 4.1 Stats Overview

**Primary stats:**
- **HP** — Hit Points
- **MP** — Magic Points
- **PA** — Physical Attack
- **MA** — Magic Attack
- **Speed (Sp)** — Determines CT gain rate and some weapon formulas
- **Brave (Br)** — 0-100; affects physical damage with certain weapons, Reaction ability trigger chance, Treasure Hunter rare item rates
- **Faith (Fa)** — 0-100; affects all magical damage/healing (as both caster and target)

**Fixed stats (job-dependent, not growable):**
- **Move** — Panels traversable per turn
- **Jump** — Height units climbable; horizontal jump distance = Jump / 2
- **C-Ev** — Class Evasion (see §3.5)

### 4.2 Raw Stats and Displayed Stats

The game tracks **Raw Stats** (hidden internal values) and **Displayed Stats** (what the player sees).

**Displayed Stat = (Raw Stat × Job Multiplier) / 1,638,400**

Initial raw stat ranges (level 1):
- Male: HP 491,520-524,287; MP 229,376-245,759; PA 81,920; MA 65,536; Sp 98,304
- Female: HP 458,752-491,519; MP 245,760-262,143; PA 65,536; MA 81,920; Sp 98,304
- Males have higher base HP and PA; Females have higher base MP and MA.

### 4.3 Stat Growth on Level Up

When leveling up, the raw stat increases by:

**Raw Stat Bonus = Current Raw Stat / (Character Level + Growth Constant)**

Where:
- **Character Level** = the level the unit is AT (before leveling)
- **Growth Constant (C)** = a per-job constant; **lower C = faster growth**

Growth is proportional to the current stat and inversely proportional to level. At low levels with low C values, stats grow fastest.

### 4.4 Job Stat Multipliers (M) — All Generic Jobs

Higher multiplier = higher displayed stat for the same raw stat.

| Job | HP M | MP M | Sp M | PA M | MA M |
|---|---|---|---|---|---|
| Squire | 100 | 75 | 100 | 90 | 80 |
| Chemist | 80 | 75 | 100 | 75 | 80 |
| Knight | 120 | 80 | 100 | 120 | 80 |
| Archer | 100 | 65 | 100 | 110 | 80 |
| Monk | 135 | 80 | 110 | 129 | 80 |
| White Mage (Priest) | 80 | 120 | 110 | 90 | 110 |
| Black Mage (Wizard) | 75 | 120 | 100 | 60 | 150 |
| Time Mage | 75 | 120 | 100 | 50 | 130 |
| Summoner | 70 | 125 | 90 | 50 | 125 |
| Thief | 90 | 50 | 110 | 100 | 60 |
| Orator (Mediator) | 80 | 70 | 100 | 75 | 75 |
| Mystic (Oracle) | 75 | 110 | 100 | 50 | 120 |
| Geomancer | 110 | 95 | 100 | 110 | 105 |
| Dragoon (Lancer) | 120 | 50 | 100 | 120 | 50 |
| Samurai | 75 | 75 | 100 | 128 | 90 |
| Ninja | 70 | 50 | 120 | 120 | 75 |
| Arithmetician (Calculator) | 65 | 80 | 50 | 50 | 70 |
| Bard | 55 | 50 | 100 | 30 | 115 |
| Dancer | 60 | 50 | 100 | 110 | 95 |
| Mime | 140 | 50 | 120 | 120 | 115 |
| Dark Knight (WotL) | 80 | 90 | 100 | 140 | 80 |

### 4.5 Job Growth Constants (C) — All Generic Jobs

Lower C value = faster stat growth when leveling in that job.

| Job | HP C | MP C | Sp C | PA C | MA C |
|---|---|---|---|---|---|
| Squire | 11 | 15 | 100 | 60 | 50 |
| Chemist | 12 | 16 | 100 | 75 | 50 |
| Knight | 10 | 15 | 100 | 40 | 50 |
| Archer | 11 | 16 | 100 | 45 | 50 |
| Monk | 9 | 13 | 100 | 48 | 50 |
| White Mage | 10 | 10 | 100 | 50 | 50 |
| Black Mage | 12 | 9 | 100 | 60 | 50 |
| Time Mage | 12 | 10 | 100 | 65 | 50 |
| Summoner | 13 | 8 | 100 | 70 | 50 |
| Thief | 11 | 16 | 90 | 50 | 50 |
| Orator | 11 | 18 | 100 | 55 | 50 |
| Mystic | 12 | 10 | 100 | 60 | 50 |
| Geomancer | 10 | 11 | 100 | 45 | 50 |
| Dragoon | 10 | 15 | 100 | 40 | 50 |
| Samurai | 12 | 14 | 100 | 45 | 50 |
| Ninja | 12 | 13 | 80 | 43 | 50 |
| Arithmetician | 14 | 10 | 100 | 70 | 50 |
| Bard | 20 | 20 | 100 | 80 | 50 |
| Dancer | 20 | 20 | 100 | 50 | 50 |
| Mime | 6 | 30 | 100 | 35 | 40 |
| Dark Knight (WotL) | 12 | 20 | 100 | 40 | 50 |

**Notable growth observations:**
- **Best HP growth:** Mime (C=6), Monk (C=9), Knight/Dragoon/Geomancer/White Mage (C=10)
- **Best MP growth:** Summoner (C=8), Black Mage/Mystic/Time Mage/Arithmetician (C=9-10)
- **Best Speed growth:** Ninja (C=80), Thief (C=90) — all other jobs have C=100
- **Best PA growth:** Mime (C=35), Knight/Dragoon (C=40), Ninja (C=43)
- **Best MA growth:** Mime (C=40), all other jobs have C=50

### 4.6 Unique Character Stat Multipliers

| Character | HP M | MP M | Sp M | PA M | MA M |
|---|---|---|---|---|---|
| Ramza | 120 | 105 | 100 | 110 | 100 |
| Mustadio | 100 | 75 | 115 | 95 | 100 |
| Agrias | 140 | 100 | 100 | 100 | 100 |
| Rapha | 90 | 100 | 115 | 80 | 100 |
| Marach | 100 | 110 | 110 | 105 | 100 |
| Orlandu (T.G. Cid) | 160 | 120 | 110 | 120 | 90 |
| Meliadoul | 125 | 80 | 105 | 120 | 90 |
| Reis (Human) | 140 | 115 | 120 | 120 | 110 |
| Beowulf | 122 | 145 | 105 | 125 | 105 |
| Cloud | 125 | 116 | 100 | 123 | 120 |

### 4.7 Unique Character Growth Constants

| Character | HP C | MP C | Sp C | PA C | MA C |
|---|---|---|---|---|---|
| Ramza | 11 | 11 | 95 | 50 | 48 |
| Mustadio | 11 | 13 | 100 | 50 | 50 |
| Agrias | 10 | 11 | 100 | 50 | 50 |
| Rapha | 11 | 11 | 100 | 50 | 50 |
| Marach | 10 | 11 | 100 | 50 | 50 |
| Orlandu | 10 | 11 | 100 | 42 | 42 |
| Meliadoul | 10 | 15 | 100 | 39 | 50 |
| Reis (Human) | 5 | 10 | 95 | 39 | 38 |
| Beowulf | 10 | 11 | 100 | 48 | 45 |
| Cloud | 11 | 11 | 100 | 42 | 46 |

### 4.8 Level Down/Up Trick (Stat Optimization)

Because stat growth is permanent (added to raw stats) and job-dependent, players can exploit the Degenerator trap or enemy abilities that reduce level to **level down** in a class with poor growth, then **level up** in a class with excellent growth, repeatedly gaining the better class's stat bonuses. This works because:

1. Level down removes stats based on current job's growth constant
2. Level up adds stats based on current job's growth constant
3. If you level down as Bard (poor growth, high C) and level up as Mime/Ninja/Monk (excellent growth, low C), you gain more stats than you lost

### 4.9 Brave and Faith

**Brave (0-100):**
- Affects physical damage for Bare Hands, Knight Swords, and Katanas (Br/100 multiplier)
- Determines Reaction ability trigger chance (Brave% = activation rate)
- Treasure Hunter: rare item chance = (100 - Brave)%
- Below **10 Brave:** unit enters **Chicken** status (flees, 1.5× physical damage taken, gains 1 Brave per turn until ≥ 10)
- Below **5 Brave** at battle end: unit permanently leaves the party (except Ramza)
- **Permanent changes:** For every 4 points of in-battle Brave modification, 1 point changes permanently after battle

**Faith (0-100):**
- Multiplier for both dealing and receiving magical damage/healing
- Faith status effect (temporary): treats unit as 100 Faith
- Innocent/Atheist status (temporary): treats unit as 0 Faith
- **95 or above** permanent Faith at battle end: unit permanently leaves the party "to seek God" (except Ramza, who can safely reach 97)
- **Permanent changes:** Same 4:1 ratio as Brave

**How to modify Brave/Faith:**
- Orator's Praise: Brave +5 (temporary); Intimidate: Brave -20 (temporary)
- Orator's Preach: Faith +5 (temporary); Enlighten: Faith -20 (temporary)
- Ramza's Cheer Up: Brave +5; Ramza's Shout: all stats up (including Brave)
- Mystic's Belief: temporary 100 Faith status; Disbelief: temporary 0 Faith status

---

## 5. Job System

### 5.1 Job Basics

Each human unit has a **current job** that determines:
- Stat multipliers applied to raw values
- Growth constants controlling per-level stat increases
- Fixed stats (Move, Jump, C-Ev)
- Available equipment types
- Main action ability set

### 5.2 Ability Slots

Each unit equips:
1. **Primary Action** — determined by current job (always available)
2. **Secondary Action** — any learned action ability set from another job
3. **Reaction Ability** — triggers automatically on certain events (activation chance = Brave%)
4. **Support Ability** — passive buff
5. **Movement Ability** — passive movement modifier

### 5.3 Job Fixed Stats

| Job | Move | Jump | C-Ev |
|---|---|---|---|
| Squire | 4 | 3 | 5% |
| Chemist | 3 | 3 | 5% |
| Knight | 3 | 3 | 10% |
| Archer | 3 | 3 | 10% |
| Monk | 3 | 4 | 20% |
| White Mage | 3 | 3 | 5% |
| Black Mage | 3 | 3 | 5% |
| Time Mage | 3 | 3 | 5% |
| Summoner | 3 | 3 | 5% |
| Thief | 4 | 4 | 25% |
| Orator | 3 | 3 | 5% |
| Mystic | 3 | 3 | 5% |
| Geomancer | 4 | 3 | 10% |
| Dragoon | 3 | 3 | 15% |
| Samurai | 3 | 3 | 15% |
| Ninja | 4 | 4 | 30% |
| Arithmetician | 3 | 3 | 5% |
| Bard (male only) | 3 | 3 | 5% |
| Dancer (female only) | 3 | 3 | 5% |
| Mime | 4 | 4 | 5% |

### 5.4 JP (Job Points) System

- Units earn JP by performing actions in battle
- JP is used to learn abilities within the current job
- **JP earned per action:** 8 + 2 × (Job Level) + floor(Character Level / 4)
- **Spillover JP:** Allies on the field earn 25% of JP gained, applied to whatever job the acting unit was using
- **JP Boost (support ability):** +50% JP earned
- **Job Level:** Increases as more JP is spent within that job (not by character level). Higher Job Level = more JP per action.

### 5.5 Job Unlock Requirements

Jobs are unlocked by reaching specific Job Levels in prerequisite jobs.

| Job | Prerequisites |
|---|---|
| Squire | Default |
| Chemist | Default |
| Knight | Squire Lv 2 |
| Archer | Squire Lv 2 |
| Monk | Knight Lv 2 |
| White Mage | Chemist Lv 2 |
| Black Mage | Chemist Lv 2 |
| Thief | Archer Lv 2 |
| Mystic | White Mage Lv 2 |
| Time Mage | Black Mage Lv 2 |
| Orator | Mystic Lv 2 |
| Summoner | Time Mage Lv 2 |
| Geomancer | Monk Lv 3 |
| Dragoon | Thief Lv 3 |
| Samurai | Knight Lv 3, Monk Lv 4, Dragoon Lv 2 |
| Ninja | Archer Lv 3, Thief Lv 4, Geomancer Lv 2 |
| Arithmetician | White Mage Lv 4, Black Mage Lv 4, Time Mage Lv 3, Mystic Lv 3 |
| Bard | Summoner Lv 4, Orator Lv 4 (male only) |
| Dancer | Dragoon Lv 4, Geomancer Lv 4 (female only) |
| Mime | Squire Lv 8, Chemist Lv 8, + every other generic job Lv 4+ |
| Onion Knight (WotL) | Squire Lv 6, Chemist Lv 6 |
| Dark Knight (WotL) | Master Knight, Master Black Mage, Dragoon Lv 8, Samurai Lv 8, Geomancer Lv 8, Ninja Lv 8, + 20 enemy kills |

### 5.6 Job Descriptions

**Squire:** Starting physical job. Focus/Accumulate for self-buff, Tailwind/Yell for Speed buffing, JP Boost for faster learning. Ramza's unique Squire gains Shout (all stats up).

**Chemist:** Starting support job. Uses consumable items to heal and cure. Auto-Potion reaction, Throw Items extends range.

**Knight:** Frontline tank. Rend abilities destroy enemy equipment and reduce stats. Equip Shields/Swords support for other jobs.

**Archer:** Ranged attacker. Charge abilities add flat WP bonus at the cost of charge time. Concentrate ignores evasion.

**Monk:** Versatile melee fighter. Highest base HP growth. Chakra self-heals HP/MP. Revive resurrects without items. First Strike (Hamedo) is the strongest reaction ability in the game — preemptive counter that lands before the enemy's attack.

**White Mage:** Primary healer. Cure/Raise/Reraise/Protect/Shell/Holy. Arcane Defense reduces magic damage taken.

**Black Mage:** Offensive magic. Fire/Ice/Lightning tiers, Frog, Death, Flare. Arcane Strength boosts magic damage.

**Time Mage:** Utility magic. Haste/Slow/Stop/Quick for CT manipulation. Demi for % HP damage. Meteor for heavy AoE. Swiftness halves all charge times. Teleport movement ability.

**Summoner:** AoE magic with ally-safe targeting — each summon has a toggle to hit only enemies within the AoE or all units (allies + enemies). Enemy-only mode costs more MP. Golem creates a party-wide physical damage barrier (absorbs total physical damage up to caster's max HP, shared pool). Halve MP support.

**Mystic:** Status effect specialist. All abilities inflict debuffs (Blind, Silence, Sleep, Petrify, Berserk, Confuse, etc.). Faith-dependent. Harmony removes all buffs from target.

**Thief:** Speed-focused. Steal abilities take enemy equipment. Poach converts monster kills to shop items. Move +2 is a key movement ability.

**Orator:** Recruitment and Brave/Faith manipulation. Entice/Invite recruits enemies. Praise/Intimidate modify Brave. Preach/Enlighten modify Faith. Speechcraft ignores Faith.

**Geomancer:** Terrain-based attacks with no MP cost or charge time. Attack Boost support. Ignore Terrain movement. See §15 for terrain mapping.

**Dragoon:** Jump attacks — leap off-screen and land for high damage. Untargetable while airborne. Dragonheart grants Reraise when hit. Ignore Elevation movement.

**Samurai:** Iaido/Draw Out uses katana spirits for AoE effects. Can break the katana on use (break chance = (100 - Brave)% — high Brave reduces breakage). Shirahadori (Blade Grasp) reaction: physical evasion = Brave%. Doublehand support doubles one-handed weapon damage.

**Ninja:** Fastest job (Move 4, Jump 4, Speed M 120). Throw consumes weapons from inventory for ranged damage (Speed × WP). Dual Wield support allows two weapons (see §12.4). Waterwalking movement.

**Arithmetician:** Cast any learned spell for free, instantly, targeting all units matching a mathematical condition (CT/Level/EXP/Height × multiple of 3/4/5/Prime). Affects allies and enemies. Worst Speed multiplier in the game (50) as trade-off.

**Bard (male only):** Global ally buffs. Performing state — effects persist each tick while singing. Stat increases stack over time.

**Dancer (female only):** Global enemy debuffs. Performing state. Drains stats/HP/MP from all enemies simultaneously.

**Mime:** No equippable abilities. Automatically copies the last allied action for free (no MP, no CT). Cannot mimic other Mimes, monsters, or Item usage. Highest stat multipliers of any generic job.

**Onion Knight (WotL):** Cannot equip any abilities. Can equip all equipment types. Stats scale with number of jobs mastered.

**Dark Knight (WotL):** Extremely difficult to unlock. Darkness abilities consume own HP for powerful attacks.

### 5.7 Complete Ability Lists

Full ability tables with JP costs, types, and effects: [docs/abilities.md](docs/abilities.md).

---

## 6. Playable Characters

### 6.1 Ramza Beoulve (Protagonist)

Ramza has a unique Squire job with enhanced stats and exclusive abilities:
- **Chapter 1:** Squire. Gains Focus, Rush, Stone, Tailwind, Cheer Up, Steel.
- **Chapter 2-3:** Squire (enhanced). Gains Shout (PA +1, MA +1, Speed +1, Brave +10 — self only).
- **Chapter 4:** Further stat upgrades.
- Must be deployed in every battle. If KO'd and death counter expires, Game Over.
- Can safely reach 97 permanent Faith without desertion (others desert at 95+).

### 6.2 Story-Mandatory Special Characters

| Character | Job | Chapter | Unique Ability Set |
|---|---|---|---|
| Agrias Oaks | Holy Knight | Ch. 2 | Holy Sword: Judgment Blade (AoE + Stop), Cleansing Strike (remove buffs), Northswain's Strike (AoE + Death), Hallowed Bolt (AoE + Silence/Confuse) |
| Mustadio Bunansa | Machinist (Engineer) | Ch. 2 | Snipe: Seal Evil (Petrify undead), Leg Shot (Don't Move), Arm Shot (Don't Act) |
| Rapha Galthena | Skyseer (Heaven Knight) | Ch. 3 | Sky Mantra: random-hit AoE magic within target area |
| Marach Galthena | Netherseer (Hell Knight) | Ch. 3 | Nether Mantra: random-hit AoE dark magic |
| Meliadoul Tengille | Divine Knight | Ch. 4 | Unyielding Blade: destroy target's equipment (Shellbust Stab, Hellcry Punch, Icewolf Bite, Shadowstitch) |
| Cidolfus Orlandu | Sword Saint | Ch. 4 | Swordplay: combines ALL Holy Sword + Unyielding Blade + Dark Sword abilities in one command |

Orlandu is widely considered the most powerful unit in the game — he joins with extremely high stats, the best stat multipliers (160 HP, 120 PA), and access to three complete swordskill sets.

### 6.3 Optional / Secret Characters

All secret character chains require **Mustadio** (recruited in Ch. 2) as a prerequisite. His father's workshop in Goug Machine City is the catalyst.

| Character | Job | Recruitment |
|---|---|---|
| Beowulf Cadmus | Templar | Ch. 4: Read "Haunted Mine" rumor at Goland tavern → visit Lesalia → complete 4 consecutive Goland mine battles |
| Reis Duelar (Dragon) | Dragonkin / Holy Dragon | Ch. 4: Joins with Beowulf after mine battles. Protect her in final mine battle. |
| Reis Duelar (Human) | Dragonkin | Ch. 4: Bring both Beowulf and Reis to Nelveska Temple; both must survive. She transforms to human form. |
| Construct 8 (Worker 8) | Steel Giant (Automaton) | Ch. 4: Obtain Aquarius Zodiac Stone (Beowulf quest) → return to Goug → auto-joins |
| Cloud Strife | Soldier | Ch. 4: Accept flower from Flower Girl in Sal Ghidos → get Cancer Stone from Nelveska Temple → return to Goug → fight recruitment battle in Sal Ghidos. Needs Materia Blade to use Limit abilities. |
| Byblos | Byblos (monster) | Ch. 4: Complete all 10 floors of Deep Dungeon. Keep Byblos alive in final battle against Elidibus. |

**Construct 8** cannot change jobs, equip items, or learn abilities. Fixed powerful physical attacks.

**Cloud** has all FF7 Limit Break abilities but requires the rare Materia Blade weapon and has very long charge times, making him impractical without Swiftness support.

### 6.4 Guest Characters

Guest units join temporarily for specific story battles. They are AI-controlled and cannot be managed by the player.

| Guest | Job | Chapters |
|---|---|---|
| Delita Heiral | Holy Knight | Ch. 1 (several battles) |
| Algus / Argath | Squire | Ch. 1 |
| Gafgarion | Dark Knight | Ch. 2 (start only) |
| Alma Beoulve | Cleric | Ch. 1, Ch. 4 (final battle) |
| Rafa / Rapha | Skyseer | Ch. 3 (before recruitment) |

---

## 7. Monster Units

### 7.1 Monster Families

17 families with 3 tiers each. Monsters cannot change jobs, equip items, or use human abilities. They level up and gain stat improvements but cannot learn new abilities.

Full monster family table and poaching drops: [docs/monsters.md](docs/monsters.md).

### 7.2 Monster Recruitment

**Orator's Entice/Invite:** Persuade enemy monsters (or humans) to join the party.
- Success rate depends on target's Faith, user's MA, and zodiac compatibility
- Only works on monsters and human generics (not story characters)
- Recruited monsters retain their level and stats

**Thief's Tame (Train):** Recruit critically wounded monsters (support ability).

### 7.3 Monster Breeding / Egg System

- Recruited monsters in the party produce eggs over time as the player travels the world map
- Eggs hatch after several map movements (~6-16 in-game days)
- Hatched monster is from the same family as the parent
- Can produce any tier within the family — breeding is the primary way to obtain higher-tier monsters (e.g., Red Chocobo through Chocobo breeding)
- Hatched monster stats: Brave 40-70, Faith 40-70 (random), level matches a random party member

### 7.4 Beastmaster

The Beastmaster (Monster Skill) support ability (learned from Squire, 200 JP) unlocks hidden bonus abilities for allied monsters when the Beastmaster stands adjacent (within 1 tile, height difference ≤ 3).

---

## 8. Status Effects

### 8.1 Positive/Buff Statuses

| Status | Effect | Duration |
|---|---|---|
| Haste | CT gain increased by 50% | 32 ticks |
| Protect | Attacker's PA reduced by 1/3 for physical attacks | 32 ticks |
| Shell | Caster's MA reduced by 1/3 for magical attacks | 32 ticks |
| Regen | Recover MaxHP / 8 per Active Turn | 36 ticks |
| Reraise | Auto-revive at 1/10 max HP upon KO | Until triggered |
| Float | +1 elevation, ignore terrain effects, Earth-immune | Permanent until dispelled |
| Reflect | Redirects single-target magic back at caster | Permanent until dispelled |
| Invisible | Cannot be targeted by enemies; attacks are unblockable; broken on acting or taking damage | Until broken |
| Faith | Faith set to 100 | 32 ticks |

### 8.2 Negative/Debuff Statuses

| Status | Effect | Duration |
|---|---|---|
| Slow | CT gain halved | 24 ticks |
| Stop | CT frozen, cannot act/evade/react | 20 ticks |
| Poison | Lose MaxHP / 8 per Active Turn | 36 ticks |
| Blind (Darkness) | Defender's evasion chances doubled | Permanent until cured |
| Silence | Cannot use any magic | Permanent until cured |
| Berserk | PA +50%, forced to use only Attack, uncontrollable | Permanent until cured |
| Confusion | Acts randomly — may attack allies, enemies, or do nothing; cannot evade | Permanent until cured |
| Charm | Fights for the opposing side (enemy-controlled); cannot evade | Permanent until cured |
| Sleep | 1.5× physical damage taken; CT frozen; cannot evade or react | 60 ticks |
| Don't Move (Immobilize) | Move command sealed | 24 ticks |
| Don't Act (Disable) | Cannot act, evade, or use reactions | 24 ticks |
| Toad (Frog) | Can only use Attack or Toad spell; 1.5× physical damage taken | Permanent until cured |
| Petrify (Stone) | Immobilized, cannot act, no damage taken, cannot be targeted; treated as dead for victory/loss conditions | Permanent until cured |
| Doom (Death Sentence) | Countdown timer; KO at 0 (Undead immune) | 3 Active Turns |
| Oil | Fire damage doubled; removed after taking fire damage | Permanent until triggered |
| Undead | Healing damages; Phoenix Down kills; 50% revival chance on death counter expiry; Death spell restores full HP | Permanent until cured |
| Chicken | Triggered when Brave < 10; flees enemies; 1.5× physical damage taken; gains 1 Brave per turn | Until Brave ≥ 10 |
| Vampire (WotL) | Cannot receive orders, randomly attacks allies | Permanent until cured |

### 8.3 Status Cancellation Pairs

- Regen cancels Poison (and vice versa)
- Haste cancels Slow (and vice versa)
- Faith cancels Innocent/Atheist (and vice versa)
- Reraise + Doom: unit dies then immediately revives

### 8.4 Status Removal

| Method | Removes |
|---|---|
| Antidote | Poison |
| Eye Drops | Blind |
| Echo Grass | Silence |
| Maiden's Kiss | Frog |
| Gold Needle (Soft) | Petrify |
| Holy Water | Undead, Vampire |
| Remedy | Petrify, Blind, Confusion, Silence, Oil, Frog, Poison, Sleep |
| Esuna (White Magic) | Most negative statuses |
| Harmony/Dispel (Mystic) | All positive statuses (offensive use) |
| Taking HP damage | Sleep, Confusion, Charm |
| Choco Esuna | Stop, and other statuses not curable by items |

---

## 9. Permadeath System

### 9.1 KO and Death Counter

When a unit's HP reaches 0, they are **Knocked Out (KO)**:
- The unit falls and a countdown of **3** appears above them
- The KO'd unit's CT continues to accumulate based on their Speed
- Each time the KO'd unit's CT reaches 100, the counter decreases by 1 instead of granting a turn
- Fast units' counters tick down faster than slow units'

### 9.2 Crystallization / Treasure Box

When the counter reaches **0**, the unit permanently dies and transforms into either:
- **Crystal** — stepping on it allows the claiming unit to either:
  - Learn one of the dead unit's learned abilities (if the claimant can use that job)
  - Fully restore own HP and MP
- **Treasure Box** — contains one piece of equipment the dead unit had equipped

**Undead exception:** Undead units have a **50% chance** to revive with random HP instead of crystallizing.

**Ramza exception:** If Ramza is KO'd and his counter reaches 0, it is an immediate **Game Over**.

### 9.3 Revival Methods

To prevent permadeath, revive the unit before the counter hits 0:
- **Raise / Raise 2 (White Magic)** — resurrect with partial/full HP
- **Phoenix Down (Item)** — resurrect with low HP
- **Monk's Revive** — resurrect adjacent ally
- **Reraise status** — auto-triggers on KO
- Completing the battle before the counter expires saves the unit

---

## 10. Story & Progression

### 10.1 Chapter Structure

| Chapter | PS1 Title | Key Events |
|---|---|---|
| Prologue | — | Tutorial battle at Orbonne Monastery |
| Chapter 1 | The Meager | Ramza as military cadet. Gariland through Fort Zeakden. ~9 story battles |
| Chapter 2 | The Manipulator and the Subservient | Political intrigue. Dorter revisit through Lionel Castle. ~11 story battles. Agrias and Mustadio join. |
| Chapter 3 | The Valiant | War escalation. Goland through Riovanes Castle. ~12 story battles. Rapha and Marach join. |
| Chapter 4 | Somebody to Love | Final chapter. Doguola Pass through Graveyard of Airships. ~17 story battles + 7 endgame battles. Orlandu, Meliadoul, and optional characters join. |

**Total: ~57 mandatory story battles.**

### 10.2 Victory Conditions

Victory conditions vary by battle:
- **Defeat all enemies** — most common
- **Defeat [specific boss]** — boss battles
- **Save [NPC] or Defeat all enemies** — protection battles (e.g., protect Mustadio, protect Rafa)

### 10.3 Endgame Gauntlet

The final sequence (Underground Book Storage → Murond Death City → Lost Sacred Precincts → Graveyard of Airships ×3) is a **continuous gauntlet with no save points**. The player must complete all battles in succession. Getting stuck here with an underleveled party is a known softlock scenario.

### 10.4 Experience and Leveling

- **Level cap:** 99
- **EXP per action:** 10 + (Target Level - Actor Level)
- **Level up:** Every 100 EXP (counter resets to 0)
- **Kill bonus:** +10 EXP for killing a target
- EXP is gained from any successful action — attacking, healing, buffing, even self-targeting
- **Story battle enemy levels:** Fixed per battle (Ch. 1: ~Lv 1-9, Ch. 2: ~Lv 10-23, Ch. 3: ~Lv 25-36, Ch. 4: ~Lv 41-60+)
- **Random encounter scaling:** Enemy levels ≈ party average level + 1-5 (PS1). WotL scales based on highest-level unit in entire roster.

### 10.5 Post-Game

**There is no New Game+.** After credits, the save reloads to just before the endgame gauntlet. The player can complete remaining side quests, Deep Dungeon, errands, and continue leveling.

---

## 11. World Structure

### 11.1 World Map Navigation

The world map is a fixed overhead view of Ivalice with **node-based travel** — the player moves along predefined paths between location nodes.

**Location dot colors:**
- **Blue dots:** Towns/cities (safe; offer shops, bars, soldier offices)
- **Green dots:** Wilderness/travel points where random encounters may trigger (~50% chance)
- **Red dots:** Story battle locations (forced battle upon entering)

New locations appear on the world map as the story progresses. Some unlock via rumors heard at tavern bars.

### 11.2 Town Services

- **Bar/Tavern:** Listen to rumors (advance story, unlock side quests), accept Errands/Propositions
- **Shop (Outfitter):** Buy/sell equipment and items. Inventory progresses with story chapter.
- **Soldier Office:** Recruit generic units (costs Gil), dismiss units
- **Fur Shop / Poachers' Den:** Buy poached monster items (unlocked in Chapter 3 at trade cities)

**Shop categories by city type:**
- **Castle cities:** Heavy armor, helmets, melee weapons (swords, knight swords, spears)
- **Town cities:** Light armor, hats, mage gear (staves, rods, robes)
- **Trade cities:** Specialty gear (ninja blades, katanas, guns, bags)

### 11.3 Random Encounters

- Occur on green dots with ~50% probability when passing through
- Each green dot has multiple possible battle configurations depending on approach direction
- Enemy composition scales with party level (see §10.4)
- In Chapter 4, rare special encounters can appear at certain wilderness locations

### 11.4 Roster Limits

- Maximum **16 characters** in the overall roster at any time
- Maximum **5 units** deployed per battle (including Ramza, who must always be deployed)
- Some battles reduce deployment to 4 or split the party into smaller groups

---

## 12. Items & Equipment

### 12.1 Equipment Slots

Each unit can equip:
- **Right Hand** — weapon
- **Left Hand** — shield or second weapon (requires Ninja's Dual Wield)
- **Head** — helmet or hat
- **Body** — armor, robe, or clothing
- **Accessory** — ring, mantle, boots, etc.

Equipment availability is **job-restricted** — each job has a specific list of equippable weapon and armor categories.

### 12.2 Weapon Categories

| Category | Formula Type | Notable Properties |
|---|---|---|
| Sword | PA × WP | Standard physical |
| Knight Sword | (Br/100×PA) × WP | Brave-dependent; some have 19% spell proc |
| Katana | (Br/100×PA) × WP | Brave-dependent; Iaido abilities |
| Dagger/Knife | (PA+Sp)/2 × WP | Speed-hybrid |
| Ninja Blade | (PA+Sp)/2 × WP | Speed-hybrid; dual-wield |
| Bow | (PA+Sp)/2 × WP | Arc trajectory; height extends range |
| Crossbow | PA × WP | Straight line; blocked by terrain |
| Gun | WP × WP | Ignores PA, evasion, facing; straight line |
| Rod | PA × WP | Boosts MA stat; can break to cast associated spell |
| Staff | MA × WP | Magic-based; MA bonus |
| Pole/Stick | MA × WP | Magic-based; 2-tile range |
| Axe | Random(1..PA) × WP | Random damage |
| Flail | Random(1..PA) × WP | Random damage; range 2 |
| Bag | Random(1..PA) × WP | Random damage |
| Harp/Instrument | (PA+MA)/2 × WP | Hybrid; Bard only |
| Book/Dictionary | (PA+MA)/2 × WP | Hybrid; exact 3-tile range |
| Cloth/Rug | (PA+MA)/2 × WP | Hybrid; Dancer only |
| Spear/Polearm | PA × WP | Standard; Jump bonus (1.5×) |

### 12.3 Consumable Items (Chemist)

| Item | Effect |
|---|---|
| Potion | Restore ~30 HP |
| Hi-Potion | Restore ~70 HP |
| X-Potion | Restore ~150 HP |
| Ether | Restore ~20 MP |
| Hi-Ether | Restore ~50 MP |
| Elixir | Restore all HP and MP |
| Phoenix Down | Revive with low HP |
| Antidote | Cure Poison |
| Eye Drop | Cure Blind |
| Echo Grass | Cure Silence |
| Maiden's Kiss | Cure Frog |
| Soft (Gold Needle) | Cure Petrify |
| Holy Water | Cure Undead/Vampire |
| Remedy | Cure multiple status effects |

Items resolve **instantly** — no charge time.

### 12.4 Dual Wield

The Ninja's **Dual Wield (Two Swords)** support ability allows equipping a weapon in the left hand instead of a shield. On a basic Attack command, both weapons strike independently:
- Right-hand weapon attacks first with the standard formula
- Left-hand weapon attacks second with a separate hit/damage calculation
- Each hand can trigger its own critical hit, status proc, and evasion check
- Reaction abilities can trigger on either hit
- The unit loses all shield evasion (S-Ev) since no shield is equipped

Dual Wield stacks multiplicatively with Doublehand — a unit cannot equip both simultaneously (Doublehand requires a two-handed grip, Dual Wield requires two weapons).

### 12.5 AoE Targeting

Abilities have defined **Range** (how far the center can be placed) and **Effect** (how many tiles around the center are hit):

| AoE Shape | Description |
|---|---|
| **Panel** | Single tile only (e.g., basic Attack, most status spells) |
| **Diamond** | Radiates from center in a diamond pattern; Effect 2 = center + all tiles within 2 manhattan distance |
| **Linear** | Straight line from caster to max range (e.g., Shockwave/Earth Slash) |
| **Cross** | Four tiles adjacent to center (no diagonals) |
| **Self-centered** | Radiates from the caster's own tile (e.g., Cyclone/Spin Fist, Chakra) |
| **Global** | Hits all units on the map (Bard songs, Dancer dances, Arithmeticks) |

Height differences can block AoE spread — if the height difference between the center tile and an adjacent tile exceeds the ability's vertical tolerance, the effect does not reach that tile.

### 12.6 Complete Equipment Tables

Full weapon stats, shield evasion values, armor, and accessory tables: [docs/equipment.md](docs/equipment.md).

---

## 13. Economy

### 13.1 Currency

**Gil** — single currency for buying and selling equipment.

### 13.2 Income Sources

- **Battle rewards** — Gil earned from completing story and random battles
- **Selling equipment** — 50% of buy price
- **Poaching** — killing monsters with the Poach (Secret Hunt) support ability converts them to items sold at the Fur Shop/Poachers' Den. Each monster has a common (7/8 chance) and rare (1/8 chance) drop. Full drop table: [docs/monsters.md](docs/monsters.md).
- **Treasure Hunter (Move-Find Item)** — step on specific tiles to find hidden items. Rare item chance = (100 - Brave)%. Low Brave = better treasure.
- **Propositions / Errands** — send idle units on timed missions for Gil and items (see §14.2)
- **Thief's Steal Gil** — steal Gil directly from enemies

### 13.3 Shops

Shop inventory progresses as the story advances (tied to specific story battle completion thresholds). Different town types carry different equipment categories (see §11.2).

**Fur Shop / Poachers' Den:** Opens in Chapter 3 at trade cities (Dorter, Warjilis/Sal Ghidos). A unit with Poach must deliver the killing blow to a monster via normal Attack. The monster vanishes (no crystal/chest) and its pelt appears at the Fur Shop.

---

## 14. Side Quests & Optional Content

### 14.1 Deep Dungeon / Midlight's Deep

**Unlock:** Complete all battles at Mullonde/St. Murond Temple in Chapter 4, then visit Warjilis for a cutscene.

**Structure:** 10 consecutive floors, each an independent battlefield.

| Floor | Name | Notable Treasure |
|---|---|---|
| 1 | NOGIAS | Blaze Gun, Glacier Gun, Kiyomori |
| 2 | TERMINATE | Blood Sword, Save the Queen |
| 3 | DELTA | Zeus Mace, Yoichi Bow |
| 4 | VALKYRIES | Rod of Faith, Faerie Harp, Kaiser Shield |
| 5 | MLAPAN | Excalibur, Iga Blade |
| 6 | TIGER | Rare weapons (varies by source) |
| 7 | BRIDGE | Staff of the Magi, Koga Blade |
| 8 | VOYAGE | Lordly Robe, Ragnarok, Perseus Bow |
| 9 | HORROR | Venetian Shield, Maximillian, Grand Helm |
| 10 | END | Chaos Blade, Chirijiraden |

**Darkness mechanic:** Floors start in darkness — terrain is invisible. Enemy crystallization illuminates nearby tiles.

**Exit mechanic:** Each floor has a hidden exit tile that must be stepped on to proceed.

**Rewards:** Floor 10 grants the Zodiark/Zodiac summon (strongest in the game, Q=98) and recruits Byblos.

### 14.2 Propositions / Errands

Available at tavern bars starting in Chapter 2. ~90-100 errands across the game.

**How they work:**
1. Accept an errand from the tavern board
2. Assign 1-3 generic human characters (no story characters, no monsters, no Ramza)
3. Characters are removed from the roster for the errand's duration (~6-16 in-game days)
4. Time passes via world map movement and battles
5. Return to the tavern to collect results

**Errand types:** Investigation, Exploration, Combat, Salvage, Mining, Odd Jobs.

**Rewards:** JP for participating characters, Gil, sometimes rare Artefacts (lore collectibles) or Wonders (world discoveries). Success influenced by assigned units' job classes, Brave, and Faith.

**Outcomes:** Failure, Success, or Great Success (better rewards).

### 14.3 Treasure Hunting (Move-Find Item)

Units with the Treasure Hunter (Move-Find Item) movement ability can discover hidden items by stepping on specific tiles during battle. Each tile has:
- A **common item** found with probability = Brave%
- A **rare item** found with probability = (100 - Brave)%

Low-Brave units find rare items more often, creating a deliberate trade-off with Brave's other functions.

---

## 15. Geomancy — Terrain Abilities

The ability used depends on the terrain tile the Geomancer is standing on:

| Ability | Terrain Triggers | Element | Status Effect |
|---|---|---|---|
| Pitfall (Sinkhole) | Natural Surface, Wasteland, Road | None | Don't Move |
| Water Ball (Torrent) | Waterway, River, Lake, Sea, Waterfall | Water | Frog |
| Hell Ivy (Tanglevine) | Grassland, Thicket, Water Plant, Ivy | None | Stop |
| Carve Model (Contortion) | Gravel, Stone Floor, Stone Wall, Mud Wall, Tombstone | None | Petrify |
| Local Quake (Tremor) | Rocky Cliff, Lava Rocks | Earth | Confusion |
| Kamaitachi (Wind Slash) | Book, Tree, Brick, Bridge, Furniture, Iron Plate, Moss, Coffin | Wind | Don't Act |
| Demon Fire (Ignis Fatuus) | Wooden Floor, Rug, Box, Stairs, Deck | Fire | Sleep |
| Quicksand | Swamp, Marsh, Poisoned Marsh | Water | Blind |
| Sand Storm (Sandstorm) | Sand Area, Stalactite, Salt | Wind | Blind |
| Blizzard | Snow, Ice | Ice | Silence |
| Gusty Wind (Wind Blast) | Roof, Sky, Chimney | Wind | Slow |
| Lava Ball (Molten Earth) | Lava, Machinery | Fire | Death (instant kill chance) |

All Geomancy abilities share: **Range 4 (+height bonus), AoE 2, no MP cost, no charge time, 100% damage hit rate, 25% status hit rate.**

---

## 16. AI System

### 16.1 AI Decision Architecture

Enemy AI uses a **weighted priority scoring system**, not scripted behavior. The AI evaluates all possible action + movement combinations and scores them.

### 16.2 Scoring Factors

The AI considers:
- **Target HP percentage** — lower HP targets receive higher priority
- **Target threat** — casters with MP for powerful abilities are prioritized
- **Target defense** — well-defended targets are deprioritized
- **Incoming danger** — AI evaluates whether moving into a tile puts it in range of charged AoE spells
- **Elemental absorption** — AI will not use elemental attacks that would heal the target
- **Status effects** — AI checks if abilities are guaranteed to fail
- **Ally safety** — AoE targeting avoids hitting allies when possible

### 16.3 AI Behavioral Patterns

- Enemies **ignore** units with Invisibility or Doom
- Enemies **know** about Reflect and will avoid or exploit reflected magic
- Enemies **disregard** Reaction abilities and will frequently trigger them
- Enemies **will not** use attacks guaranteed to fail
- All enemies have **infinite items** in their inventory
- Enemies **prioritize reviving allies** above most other actions
- Low-HP enemies **attempt to flee** when isolated
- Critical/story characters are often permanently flagged as **RUN** behavior

### 16.4 Player Auto-Battle AI Commands

When setting allies to auto-battle:
- **Normal** — balanced offensive/defensive behavior
- **Protect (PROT)** — protects a designated friendly unit
- **Run (RUN)** — flees to the nearest corner away from enemies

---

## 17. Movement and Terrain

### 17.1 Movement Rules

- Units move in **four cardinal directions** (no diagonal movement)
- **Move stat** determines maximum panels traversable per turn
- **Jump stat** determines maximum height change per step
- Units cannot pass through enemy-occupied panels
- Units can pass through ally-occupied panels but cannot stop on them

### 17.2 Terrain Types

Terrain affects:
- **Geomancy ability** used (see §15)
- **Movement cost** — most terrain costs 1 Move per panel; swamp and deep water may cost 2
- **Status infliction** — some tiles inflict Poison (Poisoned Marsh)
- **Special terrain:** Lava damages without Lavawalking; deep water requires Float/Swim

### 17.3 Weather Effects

Weather is set at battle start and does not change mid-battle. Indoor battles have no weather.

| Condition | Fire | Ice | Lightning | Wind | Other |
|---|---|---|---|---|---|
| Sunny/Cloudy | Normal | Normal | Normal | Normal | — |
| Rain | -25% | Normal | +25% | Normal | Bow accuracy reduced |
| Thunderstorm | -25% | Normal | +25% | +25% | Swamp costs 3 Move/tile |
| Snow | -25% | +25% | Normal | Normal | — |

### 17.4 Movement Abilities

| Ability | Source Job | Effect |
|---|---|---|
| Move +1 | Squire | +1 Move stat |
| Move +2 | Thief | +2 Move stat |
| Move +3 | Bard | +3 Move stat |
| Jump +1 | Archer | +1 Jump stat |
| Jump +2 | Dragoon | +2 Jump stat |
| Jump +3 | Dancer | +3 Jump stat |
| Teleport | Time Mage | Warp anywhere; success decreases 10% per tile beyond Move range |
| Fly | Bard / Dancer | Ignore terrain height entirely |
| Levitate | Time Mage | Permanent Float status |
| Lifefont (Move-HP Up) | Monk | Recover HP per panel moved |
| Manafont (Move-MP Up) | Mystic | Recover MP per panel moved |
| Treasure Hunter | Chemist | Find hidden items on tiles |
| Ignore Terrain | Geomancer | No terrain movement penalties |
| Ignore Elevation | Dragoon | Move to any height tile |
| Lavawalking | Geomancer | Walk on lava without damage |
| Waterwalking | Ninja | Walk on water tiles |
| Accrue EXP | Arithmetician | Gain EXP per panel moved |
| Accrue JP | Arithmetician | Gain JP per panel moved |

---

## 18. UI & HUD

### 18.1 Battle HUD

- **CT bar / turn order** — shows upcoming turn sequence
- **Unit stats** — HP, MP, CT, status effects displayed when selecting a unit
- **Terrain info** — tile type, height shown on cursor
- **Action menu** — Move, Act (with sub-menu of abilities), Wait
- **Damage numbers** — displayed as floating text on hit
- **Status indicators** — icons above affected units
- **Charge timer** — displays cast countdown for charging abilities
- **Death counter** — countdown number above KO'd units

### 18.2 Menu Screens

- **Formation** — arrange party before battle; set jobs, equip abilities and equipment
- **Brave Story** — story log, character biographies, timeline
- **Party Roster** — view all recruited units, stats, abilities learned
- **Shop** — buy/sell in towns
- **Bar / Tavern** — accept Propositions/Errands, listen to rumors

---

## 19. Save System

- **World map save:** Save at the world map between battles via the "Data" menu
- **Suspend save:** Available mid-battle; creates a single temporary save that is deleted on load
- **Save slots:** 2 (PS1 original), expanded in later versions
- **Softlock warning:** Players can get permanently stuck if they save before a difficult story battle with an underleveled party and no second save file. The endgame gauntlet (see §10.3) is the most common softlock point.

---

## 20. Multiplayer

**The original PS1 version has no multiplayer features.** It is entirely single-player.

Multiplayer was added in the PSP port War of the Lions (2007):
- **Melee:** PvP 1v1 battles with customizable rules
- **Rendezvous:** Cooperative missions with unique rewards

---

## 21. Open Questions / Unverified

1. **Haste multiplier:** Most datamined sources indicate +50% (×1.5). Some older community guides say doubled (×2). Needs definitive ROM verification.

2. **Crystal vs Treasure Box probability:** Whether the outcome is 50/50 or weighted differently when a unit crystallizes is not consistently documented.

3. **Spell Q values for White Magic and Time Magic:** Exact Q constants for Cure series, Raise series, and Time Magic utility spells are not as well-documented as Black Magic tiers. The tier pattern (14/18/24/32) may apply to other magic schools but this needs verification.

4. **AI scoring weights:** The exact numerical weights in the AI priority scoring system are documented at assembly level on FFHacktics but not presented in human-readable form.

5. **Protect/Shell exact formula:** Most sources say "reduces by 1/3" but some describe it as "reduces to 5/8." These are close but not identical (66.7% vs 62.5%).

6. **Same-sign zodiac compatibility:** Whether same zodiac sign is treated as Neutral, Good (same gender), or Bad (opposite gender) varies by source.

7. **Job unlock requirements — PS1 vs WotL:** Some community guides mix PS1 and WotL unlock requirements. The values in §5.5 are the most commonly cited PS1 values, but some sources report higher requirements (e.g., Monk requiring Knight Lv 3 instead of Lv 2).

8. **Silence and Blind duration:** Most guides list these as permanent until cured in PS1. Some sources report tick-based durations (Silence: 36 ticks, Blind: varies). May depend on the specific ability that inflicted the status.

9. **Golem barrier HP pool:** Most sources say Golem absorbs up to the caster's max HP in physical damage, shared across the party. Some sources say it's based on current HP. Needs verification.

---

## 22. References

### Mechanics Guides
- [AeroStar's Battle Mechanics Guide (GameFAQs)](https://gamefaqs.gamespot.com/ps/197339-final-fantasy-tactics/faqs/3876) — definitive PS1 mechanics reference
- [FFHacktics Wiki — Formulas](https://ffhacktics.com/wiki/Formulas) — datamined formula IDs from ROM
- [FFHacktics Wiki — Physical Evade Calculation](https://ffhacktics.com/wiki/Physical_Evade_Calculation)
- [FFHacktics Wiki — AI Documentation](https://ffhacktics.com/wiki/AI_Initial_Targeting_Selection)
- [Tactics League — Knowledge Base](https://tacticsleague.com/learn/knowledge/) — PvP-focused mechanics reference
- [Tactics League — Elemental Affinities](https://tacticsleague.com/elemental-affinities/)

### Stat Growth References
- [Bright Rock Media — FFT Stat Growths & Multipliers](https://brightrockmedia.com/final-fantasy-tactics-fft-stat-growths-and-multipliers-guide/) — complete M and C value tables
- [Hack the Minotaur — Stat Growth Rankings](https://hacktheminotaur.com/final-fantasy-tactics-the-ivalice-chronicles/stat-growth-ranking-for-all-jobs/)

### Community Guides
- [Steam Community — FFT Basic Mechanic and Job Guide](https://steamcommunity.com/sharedfiles/filedetails/?id=3581203978)
- [Final Fantasy Wiki (Fandom) — FFT Stats](https://finalfantasy.fandom.com/wiki/Final_Fantasy_Tactics_stats)
- [GameFAQs — Zodiac Compatibility Guide](https://gamefaqs.gamespot.com/ps/197339-final-fantasy-tactics/faqs/23143)
- [Game8 — FFT Guides](https://game8.co/games/Final-Fantasy-Tactics)
- [RPG Site — Jobs Guide](https://www.rpgsite.net/guide/18138-final-fantasy-tactics-the-ivalice-chronicles-jobs-guide)
- [RPG Site — Zodiac Compatibility Guide](https://www.rpgsite.net/guide/18604-final-fantasy-tactics-the-ivalice-chronicles-zodiac-compatibility-guide)
- [RPG Site — Poaching Guide](https://www.rpgsite.net/guide/18616-final-fantasy-tactics-the-ivalice-chronicles-poaching-guide)

### Datamining / Hacking
- [FFHacktics Community](https://ffhacktics.com/) — primary ROM hacking and datamining community for FFT
- [FFHacktics — Geomancy Tiles Table](https://ffhacktics.com/wiki/Geomancy_tiles_type_to_ability_table)
- [FFHacktics — AI Ability Use Decisions](https://ffhacktics.com/wiki/AI_Ability_Use_Decisions)

### Companion Documents
- [docs/abilities.md](docs/abilities.md) — complete ability lists per job with JP costs
- [docs/equipment.md](docs/equipment.md) — weapon stats, shield evasion, armor, accessories, job equip matrix
- [docs/monsters.md](docs/monsters.md) — monster families, poaching drops, zodiac compatibility chart
