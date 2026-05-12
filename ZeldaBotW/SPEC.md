# The Legend of Zelda: Breath of the Wild — Gameplay Systems Spec

Nintendo Switch / Wii U, March 3, 2017. Version specced: Switch 1.6.0 + both DLC packs (The Master Trials, The Champion's Ballad).

---

## 1. Core Gameplay Systems

### 1.1 Primary Gameplay Loop

Open-world action-adventure. The player controls Link, exploring the kingdom of Hyrule with full freedom of movement and objective ordering. The core loop is:

1. **Explore** — traverse terrain by climbing, gliding, running, swimming, horseback
2. **Discover** — find shrines, Korok seeds, towers, settlements, enemy camps, resources
3. **Solve** — complete shrine puzzles and environmental challenges using physics-based runes
4. **Fight** — engage enemies using melee weapons, bows, runes, and environmental interactions
5. **Upgrade** — trade Spirit Orbs for hearts/stamina, expand inventory, upgrade armor
6. **Repeat** — tackle Divine Beasts, main quests, or continue freeform exploration

The game can be completed immediately after the tutorial (Great Plateau) by going directly to the final boss. All intermediate content — Divine Beasts, shrines, side quests — is optional.

### 1.2 Combat System

Real-time action combat with melee weapons, bows, shields, and rune abilities.

**Health units:** 1 heart = 4 HP. Link with 30 hearts = 120 HP.

**Melee Weapon Types:**

| Type | Combo Hits | Final Hit | Charge Attack | Shield Use |
|---|---|---|---|---|
| One-handed | 4 | 2x damage | Spin attack (radius grows per charge tier) | Yes |
| Two-handed | 2 | Lands twice at full damage | Spin attack (each spin = full damage, final spin hits 2x) | No |
| Spear | 5 (5th strongest) | Knockback | Rapid stab flurry (up to 13 hits at max charge) | No |

**Damage Formula (calculation order):**
1. Apply attack buff multiplier to base weapon damage
2. Multiply by armor set bonus (if applicable)
3. Multiply by situational multiplier (crit/headshot/sneakstrike/frozen)
4. Add flat elemental damage last
5. Decimals round down

**Damage Link receives:** `max(1, enemy_innate_attack + enemy_weapon_attack - Link_armor_defense - Link_defense_buffs)`, in quarter-heart units. Each armor defense point blocks 1 quarter-heart of damage. Minimum damage is always 1 quarter-heart regardless of defense.

**Damage Multipliers:**

| Condition | Multiplier |
|---|---|
| Sneak Strike (from behind, undetected) | 8x |
| Frozen enemy (next hit after freeze) | 3x |
| Headshot (bow) | 2x |
| Critical hit (final combo hit) | 2x |
| Thrown weapon (breaks on impact) | 2x |
| Thrown boomerang (returns) | 1.5x |
| Mounted combat (horseback) | 2x (all attacks auto-crit) |
| Jump attack | Hits twice at full damage |

**Flurry Rush (Perfect Dodge):**
- Backflip against horizontal attacks or side-hop against vertical attacks with correct timing
- Timing window: ~0.1 seconds (exact frame data not publicly documented)
- Hits by weapon type: one-handed = 7, two-handed = 4, spear = 7–10
- Each flurry hit deals full weapon damage and consumes durability

**Perfect Parry (Perfect Guard):**
- Hold ZL (shield), press A just before an attack connects
- Successful parry: no shield durability loss, deflects projectiles including Guardian lasers
- Guardian laser parry: deals 500 damage (one-shots Decayed Guardians)
- Failed timing: shield takes full durability damage

**Attack Buffs (food/elixir):**

| Level | Multiplier |
|---|---|
| 1 | +20% |
| 2 | +30% |
| 3 | +50% |

**Defense Buffs (food/elixir):**

| Level | Flat Bonus |
|---|---|
| 1 | +4 |
| 2 | +12 |
| 3 | +24 |

**Armor Set Attack Bonuses:**
- Barbarian Set: +80% (all melee weapons)
- Ancient Set (Ancient Proficiency): +80% (ancient/guardian weapons only)
- Ancient weapons vs Guardians (inherent): +50%

### 1.3 Physics Engine / Chemistry Engine

The game uses a paired "Chemistry Engine" and Physics Engine (presented at GDC 2017). The physics engine handles movement and forces; the chemistry engine handles state changes.

**Three fundamental rules:**
1. Elements can change the state of materials (fire burns wood)
2. Elements interact with each other (fire + water = steam/extinguish)
3. Materials cannot influence each other's states directly

**Elements:** Fire, Water, Ice, Electricity, Wind

**Fire mechanics:**
- Spreads to flammable objects (grass, wood, leaves, wooden equipment)
- Burning grass/wood creates updraft columns rideable by Paraglider
- Rain extinguishes fires and prevents fire spread

**Lightning / Metal:**
- During thunderstorms, equipped metal gear sparks and attracts lightning
- Lightning strike: up to 14.5 hearts (58 HP), drops held weapons
- Metal objects thrown near enemies attract lightning to them
- Shock arrows in water/on metal: 40 fixed AoE damage

**Temperature system:**
- Temperature gauge on HUD, affected by: altitude, time of day, shade, proximity to heat/cold sources, equipped weapon element, region
- Cold damage without resistance: 0.5 hearts per 10 seconds
- Cold water: ~1 heart per second
- 5 hazard tiers: Cold Lv1, Cold Lv2, Heat Lv1, Heat Lv2, Volcanic (reads "ERROR")

**Object interactions:**
- Metal objects: affected by Magnesis, attract lightning
- Wooden objects: flammable, float on water
- Ice: created by Cryonis, melted by fire
- Stasis: freezes object in time, stores kinetic energy, launches on expiry
- Non-swimming enemies knocked into water drown

### 1.4 Rune Abilities

Runes are abilities granted by the Sheikah Slate, unlocked through the four Great Plateau shrines.

**Magnesis:**
- Affects all metallic objects (weapons, treasure chests, iron boxes, metal doors)
- No weight limit or upgrade available

**Remote Bomb (Round vs Cube):**

| Property | Round | Cube |
|---|---|---|
| Damage (base) | 12 | 12 |
| Damage (Bomb+) | 24 | 24 |
| Cooldown (base) | 6 seconds | 6 seconds |
| Cooldown (Bomb+) | 3 seconds | 3 seconds |
| Behavior | Rolls on slopes, affected by wind | Stays in place |

Round and Cube have independent cooldowns. Both detonated manually.

**Stasis:**
- Object freeze duration: 10 seconds
- Cooldown: 10 seconds
- Accumulated kinetic energy shown by directional arrow (yellow → red)
- Stasis+ (upgraded): can freeze enemies
  - Weak enemies (Red/Blue Bokoblins): ~5 seconds
  - Strong enemies (Black/Silver): ~1.5–2 seconds
  - All accumulated damage dealt at once on expiry + launch

**Cryonis:**
- Max pillars: 3 simultaneously (4th destroys 1st)
- Placement: any water surface (including waterfalls), not hot springs
- No upgrade available

**Camera Rune:**
- Takes photos for the Hyrule Compendium (385 entries)
- Unlocked at Hateno Ancient Tech Lab

**Master Cycle Zero (DLC):**
- Fuel-based motorcycle, filled by feeding materials
- Cannot be used in shrines, Divine Beasts, or certain areas
- Horseback combat rules apply (all attacks = 2x auto-crit)

### 1.5 Stamina System

**Base stamina:** 1 full wheel (1,000 internal units per community testing)

**Upgrades:**
- Stamina Vessel: trade 4 Spirit Orbs at a Goddess Statue
- 5 Stamina Vessels = 1 additional full wheel
- Maximum: 3 full wheels (10 Stamina Vessels = 40 Spirit Orbs)

**Temporary stamina (overfill):**
- Enduring meals/elixirs add yellow temporary stamina beyond max
- Max overfill: 2 extra wheels (e.g., 5 Endura Carrots)
- With max upgrade + max overfill: 5 total wheels possible

**Drain behavior:**
- Drains during: climbing, sprinting, swimming, paragliding, charged attacks, shield surfing
- Does NOT drain: standing, walking, floating stationary in water
- Recovery: automatic when not performing strenuous actions

### 1.6 Health System

**Starting hearts:** 3

**Heart Container sources:**
- 4 from Divine Beast bosses (1 each, automatic)
- Up to 30 from Spirit Orbs (120 base-game shrines ÷ 4 orbs per upgrade = 30 upgrades shared between hearts and stamina)
- 4 additional Spirit Orbs from DLC Champion's Ballad shrines (1 more upgrade)

**Maximum hearts:** 30 (hard cap). With 3 starting + 4 from Divine Beasts = 7 fixed, the maximum from Spirit Orbs toward hearts is 23 (= 92 orbs), leaving 28 orbs (7 Stamina Vessels = 1.4 extra wheels) for stamina.

**Maximum stamina:** 3 full wheels (10 Stamina Vessels = 40 orbs). This leaves 80 orbs (20 Heart Containers) for hearts, giving 27 total hearts (3 + 4 + 20).

**Cannot max both simultaneously.** The tradeoff is continuous — every heart traded for stamina costs 4 orbs either way.

**Horned Statue (Hateno Village):** Swaps Heart Containers ↔ Stamina Vessels for 20 rupees per swap.

**Temporary hearts:**
- Yellow bonus hearts from Hearty meals
- Bonus hearts from certain beds (soft beds, special inns)

**Fairy revival:**
- Fairies in inventory auto-trigger on death, restoring 5 hearts
- Manual fairy use: also 5 hearts
- Fairy Tonic (cooked fairy): 12 hearts but does NOT auto-revive
- Max 11 fairies in inventory; fairies stop spawning at fountains if Link holds 3+

**Mipha's Grace:** Full heal + 5 yellow bonus hearts on death (see §1.7).

### 1.7 Champion Abilities

Earned by completing Divine Beast dungeons. Each has limited uses before entering a cooldown.

| Ability | Uses | Cooldown | Castle Cooldown | DLC+ Cooldown | DLC+ Castle | Effect |
|---|---|---|---|---|---|---|
| Revali's Gale | 3 | 6 min | 2 min | 2 min | 40 sec | Creates massive updraft, launches Link |
| Daruk's Protection | 3 | 18 min | 6 min | 6 min | 2 min | Barrier blocks all damage, reflects Guardian beams |
| Urbosa's Fury | 3 | 12 min | 4 min | 4 min | 1 min 20 sec | Lightning AoE + stun on charged attack release |
| Mipha's Grace | 1 | 24 min | 8 min | 8 min | 2 min 40 sec | Auto-revive on death, full heal + 5 yellow hearts |

**Urbosa's Fury damage (flat, unaffected by buffs):**

| Target Type | Damage |
|---|---|
| Standard enemies / most bosses | 150 |
| Hinox, Stone Talus variants | 300 |
| Guardians (all), Lynels, Moldugas | 500 |
| Electric/Thunder Wizzrobes | 75 (no stun) |
| Monk Maz Koshia | 10 |

**Cooldown multiplier system (datamined):**
- Normal overworld: 1.0x
- Hyrule Castle: 3.0x
- DLC+ version: ~3.0x base (stacks with Castle for ~9.0x)

### 1.8 Environmental Damage

| Hazard | Damage | Notes |
|---|---|---|
| Fall (short) | 0 | Below ~2–3x Link's height |
| Fall (high) | Scales with distance | Cancelled by paraglider, shield surf, water landing, Revali's Gale |
| Fire (standing in) | Continuous ticks | Wooden equipment catches fire and is destroyed |
| Fire arrow / weapon hit | +10 flat | One-time, then burning ticks |
| Electric shock | +20 flat + stun | Stun drops held weapons; AoE in water = 40 |
| Lightning strike | Up to 14.5 hearts (58 HP) | Attracted by equipped metal gear |
| Ice attack | +10 flat + freeze | Frozen target takes 3x on next hit |
| Drowning | Instant respawn | When stamina fully depletes while swimming |
| Lava | Rapid fire damage | Flame Guard insufficient; requires Flamebreaker set bonus (tier 2+) |
| Cold (no resistance) | 0.5 hearts / 10 sec | Cold water: ~1 heart/sec |

### 1.9 Stealth & Detection

**Noise meter:** HUD indicator (bottom-right) showing sound waves. Larger/more waves = louder = enemies detect faster.

**What affects noise:**
- Crouching (L-stick click): significantly reduces noise
- Walking: moderate noise
- Sprinting: full noise (overrides all Stealth Up bonuses)
- Rain: muffles footsteps
- Stealth Set (full, tier 2+): effectively inaudible while walking/crouching (not sprinting)

**Tall grass:** crouching in tall grass hides Link from visual detection. Enemies can still hear loud actions.

**Enemy awareness states:**

| State | Indicator | Behavior |
|---|---|---|
| Unaware | None | Idle patrol, sleeping, sitting |
| Suspicious | Yellow "?" | Looks toward source, investigates; "?" fills toward red based on distance |
| Alert / Combat | Red "!" | Attacks on sight; must kill all or break line of sight and hide until de-escalation |

**Detection factors:**
- **Audio:** based on noise meter level; footsteps, combat sounds, weapon impacts
- **Visual:** line-of-sight, must be facing Link with unobstructed view; fill rate scales inversely with distance
- **Sneak Strike window:** approach undetected from behind → Y for 8x damage (see §1.2)

### 1.10 Status Effects

| Status | Trigger | Effect on Target | Duration | Notes |
|---|---|---|---|---|
| Burning | Fire weapons (+10), fire arrows, environmental fire | Continuous tick damage; enemies panic/run | While in fire source; brief DOT from weapon hits | Wooden equipment catches fire and is destroyed |
| Frozen | Ice weapons (+10), ice arrows, Cryonis indirect | Immobilized; next hit deals **3x damage** and shatters freeze | A few seconds if not hit (shorter on Gold enemies) | Instant-kills fire-element enemies |
| Shocked | Electric weapons (+20), shock arrows, lightning | Stunned; drops held weapon/bow/shield | Brief (shorter on Gold enemies) | Cannot stack — subsequent electric hits during stun deal only base damage |
| Wet | Rain, swimming, standing in water | Conducts electricity (vulnerable to 40-damage shock AoE); extinguishes burning | Fades after leaving water/rain | Metal equipment + wet + thunderstorm = lightning magnet |

**Elemental stacking rule:** while a target is under an elemental status, subsequent hits of the same element apply only base weapon damage — the flat elemental bonus does not re-apply until the status clears.

**Opposite-element instant kills:** Ice vs fire-element enemies and fire vs ice-element enemies = instant kill on basic elemental enemies (Wizzrobes, elemental ChuChus, elemental Keese).

---

## 2. Controls & Input

Specced for Nintendo Switch (Joy-Con and Pro Controller share the same mapping).

### 2.1 Standard Controls

| Input | Action |
|---|---|
| Left Stick | Move |
| Left Stick Click | Crouch / Stealth |
| Right Stick | Camera |
| Right Stick Click | Scope / Telescope |
| A | Interact / Talk / Pick up / Climb down |
| B | Cancel / Sprint (hold) / Close menu |
| X | Jump / Climb (on surfaces) |
| Y | Attack (melee) / Throw (hold) |
| L | Sheikah Rune use (hold for rune wheel) |
| ZL | Shield (hold) / Z-Target lock-on |
| R | Throw weapon (tap) / Whistle (hold) |
| ZR | Bow and Arrow (hold to aim, release to fire) |
| D-Pad Up | Change Rune / Arrow type |
| D-Pad Left | Change Shield |
| D-Pad Right | Change Weapon |
| D-Pad Down | Whistle |
| + | Pause / Inventory |
| - | Map / Adventure Log / Quests |

### 2.2 Context-Sensitive Controls

| Context | Controls |
|---|---|
| Horse riding | A = spur, Left Stick = steer, ZL = target, Y = melee, ZR = bow, L = soothe |
| Paragliding | X in midair to deploy, Left Stick = steer, ZR = bow (bullet-time), B = cancel |
| Swimming | A = dash (costs stamina), X = dive (with Zora Armor) |
| Climbing | X = jump up (burst stamina), B = let go |

### 2.3 Motion Controls

- Gyro aiming overlays on right-stick for bow, camera rune, and Sheikah Scope
- Required for certain apparatus shrine puzzles (cannot be completed without motion controls)
- Togglable in Options → Aim with Motion Controls

### 2.4 Accessibility

- Button customization limited to swapping Jump between X and B
- Pro HUD mode reduces on-screen elements (see §10)

---

## 3. World Structure

### 3.1 Map Overview

**Playable area:** ~80 km² (datamined from game coordinate data). Open world with seamless traversal — no loading screens between regions.

**8 geographic regions:** Akkala, Central Hyrule, Eldin, Faron, Gerudo, Hebra, Lanayru, Necluda.

**15 tower regions** (named after the Sheikah Tower in each): Great Plateau, Central, Dueling Peaks, Hateno, Lanayru, Eldin, Akkala, Woodland, Ridgeland, Tabantha, Hebra, Gerudo, Wasteland, Lake, Faron.

**Elevation:** Link can climb any outdoor surface as long as stamina remains. Rain makes surfaces slippery (see §3.6). Stamina upgrades serve as soft-gates for taller climbs.

**Soft-gating:** Gerudo Desert, Death Mountain, and Hebra Mountains are gated by temperature hazards requiring resistance gear or food. Gerudo Town has a separate social gate — only women are allowed entry, requiring Link to wear the Gerudo Vai disguise (see §9.6). The Great Plateau is physically isolated by cliffs, requiring the Paraglider to leave.

### 3.2 Sheikah Towers

**15 total.** Activating a tower (inserting the Sheikah Slate) reveals the topographic map for that region and creates a fast travel point.

- Great Plateau Tower is mandatory (first activation, gated by a cutscene)
- Remaining 14 can be activated in any order
- Each tower has environmental challenges to reach (enemies, Malice, weather, Guardians)

### 3.3 Shrines

**Total count:** 120 base game + 16 DLC (Champion's Ballad) = 136

**Shrine types:**
- **Puzzle shrines:** ~63 of 120 (rune usage, physics puzzles, environmental manipulation)
- **Combat shrines (Tests of Strength):** 20 total — 6 Minor (Guardian Scout II), 6 Modest (Guardian Scout III), 8 Major (Guardian Scout IV)
- **Blessing shrines:** ~37 total (trial is reaching the shrine; interior is just a chest and the monk)

**Spirit Orb system:**
- Each shrine awards 1 Spirit Orb
- 4 Spirit Orbs = 1 Heart Container or 1 Stamina Vessel (at any Goddess Statue)
- 120 orbs = 30 total upgrades (base game)
- Cannot max both hearts and stamina simultaneously

**Shrine Quests:** 42 total. Given by NPCs, requiring an overworld task to reveal a hidden shrine.

### 3.4 Divine Beasts

Four large dungeon-like creatures, each with 5 terminals to activate, a dungeon map, and a unique environmental control mechanic.

| Divine Beast | Region | Champion | Mechanic | Boss | Reward |
|---|---|---|---|---|---|
| Vah Ruta (elephant) | Lanayru | Mipha | Trunk redirects water flow | Waterblight Ganon | Mipha's Grace |
| Vah Medoh (eagle) | Hebra | Revali | Body roll tilts dungeon | Windblight Ganon | Revali's Gale |
| Vah Rudania (salamander) | Eldin | Daruk | Body rotates 90° | Fireblight Ganon | Daruk's Protection |
| Vah Naboris (camel) | Gerudo | Urbosa | 3 rings rotate to align circuits | Thunderblight Ganon | Urbosa's Fury |

Each freed Divine Beast reduces Calamity Ganon's starting HP by 12.5%. All four freed = Ganon starts at 50% HP. If none are freed, Link must fight all four Blights before Ganon.

### 3.5 Korok Seeds

**900 total.** Collected by solving environmental micro-puzzles scattered across Hyrule.

**Korok puzzle types:** rock lifting, rock pattern completion, rock circles, metal block matching (Magnesis), flower chasing, fruit offerings, acorn shooting, balloon shooting, diving rings, leaf pile burning, ice block melting, bomb walls, cube patterns.

**Hestu's inventory expansion:** Korok Seeds are traded to Hestu (first at Kakariko road, then Riverside Stable, then permanently at Korok Forest) to expand weapon/bow/shield slots.

| Category | Starting Slots | Max Slots | Total Seeds Required |
|---|---|---|---|
| Melee Weapons | 8 | 19 (+1 Master Sword) | 208 |
| Bows | 5 | 13 (+1 Bow of Light) | 73 |
| Shields | 4 | 20 | 160 |
| **Total** | — | — | **441 of 900** |

Remaining 459 seeds are cosmetic. Collecting all 900 awards "Hestu's Gift" — a golden ornament with no gameplay function.

### 3.6 Weather System

**Weather types:** Clear, Cloudy, Rain, Thunderstorm, Snow. Changes every 4 in-game hours (~4 real minutes). Region-dependent (Faron = frequent rain, Hebra = constant snow).

**Rain effects:**
- Surfaces become slippery; Link slips every 3–5 handholds of climbing
- Effectively impassable for tall surfaces without extensive stamina
- Does NOT affect ladder climbing

**Thunderstorms:** Metal equipment attracts lightning (see §1.8).

**Wind:** Headwinds slow paragliding; tailwinds increase glide distance. Fire spreads in wind direction.

### 3.7 Day/Night Cycle

**Duration:** 1 in-game minute = 1 real second. Full 24-hour cycle = **24 real minutes.**

**Key times:**
- Dawn: 5:00 AM
- Dusk: ~7:00 PM
- Night: 9:00 PM

**Blood Moon mechanics:**
- **Scheduled:** Every 7 in-game days (~2 hours 48 minutes real time)
- Triggers at midnight: all defeated enemies, ore deposits, overworld items respawn
- Does not occur until Link leaves the Great Plateau
- **Panic Blood Moon:** Technical failsafe triggered by memory pressure (rare, not a gameplay mechanic)
- Cooking during the Blood Moon window (11:30 PM – 12:15 AM) guarantees a critical cook bonus

**Star Fragments:**
- Rare material, falls from sky between 9:30 PM and 2:00 AM (primarily full moons)
- Moon follows an 8-day cycle (new → waxing crescent → first quarter → waxing gibbous → full → waning gibbous → third quarter → waning crescent)
- Disappears at 5:00 AM if not collected
- Used for high-level armor upgrades

**NPC schedules:** Most NPCs sleep at night. Shops operate daytime only. Stal-type skeleton enemies spawn at night and despawn at dawn.

### 3.8 Fast Travel

- **Points:** Activated Shrines, activated Sheikah Towers
- Can teleport from anywhere in the overworld (not from inside shrines/Divine Beasts)
- **Travel Medallion (DLC):** Places 1 custom fast travel point anywhere Link stands. Only one at a time; setting a new one deletes the old. Found at Lomei Labyrinth Island (Akkala).

### 3.9 Great Plateau (Tutorial)

Physically isolated elevated area. Link cannot leave without the Paraglider.

**Required progression:**
1. Awaken in Shrine of Resurrection, receive Sheikah Slate
2. Activate Great Plateau Tower (mandatory)
3. Complete 4 shrines (any order), each granting a rune:
   - Oman Au → Magnesis
   - Ja Baij → Remote Bomb
   - Owa Daim → Stasis
   - Keh Namut → Cryonis
4. The Old Man (King Rhoam) gives the Paraglider at the Temple of Time

Also introduces cooking, temperature management, combat basics, and Sheikah Slate scope usage. The Old Man gives Link the **Warm Doublet** (cold resistance armor) as a reward for cooking a Spicy Meat and Seafood Fry, or it can be found in his cabin — this is the player's first exposure to temperature-gated exploration.

### 3.10 Hyrule Castle

Multi-layered final dungeon. Exterior grounds and interior castle with many floors and wings, surrounded by a moat and defended by Guardians.

**Key areas:** Docks (rear entrance), Lockup/Prison (contains Hylian Shield), Library, King's Study (behind a metal bookcase — Magnesis), Armory, Dining Hall, Princess Zelda's Room, Sanctum (top — Calamity Ganon).

**Approach options:**
- Front gate (through ruins, heavy Guardian presence)
- By air (paraglide from high points; Revali's Gale bypasses most of the castle)
- Through water (swim the moat to Docks; Zora Armor allows waterfall swimming inside)
- Underground (Mine Cart tunnels, Lockup wall breaks)

No terminals to activate. The goal is simply to reach the Sanctum. Can skip most content by going directly to Ganon.

### 3.11 Eventide Island

**Quest:** "Stranded on Eventide" (Shrine Quest). Upon arrival, ALL of Link's equipment is stripped — weapons, armor, materials, food. He retains only the Sheikah Slate (all runes), hearts, and stamina.

**Objective:** Find 3 Ancient Orbs and place them on corresponding pedestals across the island.

**Restrictions:**
- Cannot fast travel during the trial
- Death = restart from the mainland
- Must use only items found on the island
- Enemy difficulty scaling is disabled (see §7.1)

**Completion:** All equipment returned. Korgu Chideh Shrine emerges (Blessing shrine). Reward: Spirit Orb + Gold Rupee (300).

---

## 4. Playable Characters

Link is the sole playable character. No classes or character selection.

### 4.1 Wolf Link (amiibo Companion)

- Summoned by scanning the Wolf Link amiibo
- HP = hearts from Twilight Princess Cave of Shadows save data (default 3)
- Attacks nearby enemies autonomously
- Cannot enter shrines or Divine Beasts
- Dismissed when HP reaches 0 or when Link enters restricted areas

---

## 5. Story & Progression

### 5.1 Main Story Structure

**Main Quests:** 15 base game + 5 DLC = 20 total.

Only 3 are required to finish: The Shrine of Resurrection, The Isolated Plateau, and Destroy Ganon. Link can go straight to Ganon from the Great Plateau.

**Core narrative beats:**
1. Link awakens in the Shrine of Resurrection with no memories after 100 years
2. King Rhoam's spirit provides exposition on the Great Plateau
3. Impa in Kakariko Village directs Link to free the four Divine Beasts and recover memories
4. Each Divine Beast quest involves allying with a Champion descendant and clearing the dungeon
5. Optionally retrieve the Master Sword from Korok Forest (requires 13 hearts)
6. Storm Hyrule Castle and defeat Calamity Ganon

### 5.2 Captured Memories

**18 total memories** telling the backstory of 100 years ago:
- 12 from Sheikah Slate photos (found at matching locations)
- 4 from Divine Beast storylines
- 1 from Impa (after finding all 12 photo memories)
- 1 from pulling the Master Sword

Finding all 18 unlocks a bonus post-credits cutscene showing Zelda and Link together.

### 5.3 Side Content

- **Side Quests:** 76 base game + 14 DLC = 90 total
- **Shrine Quests:** 42 total
- **120 Shrines** (base game), 136 with DLC
- **900 Korok Seeds**

### 5.4 DLC Content

#### DLC Pack 1: The Master Trials (June 30, 2017)

**Trial of the Sword:**
- Accessed by placing the Master Sword in its pedestal in Korok Forest
- Link enters with no equipment — must scavenge everything inside
- 3 stages, 54 total floors:
  - Beginning Trials: 13 floors (12 combat/puzzle + 1 rest)
  - Middle Trials: 17 floors (15 combat/puzzle + 2 rest)
  - Final Trials: 24 floors (21 combat/puzzle + 3 rest)
- **Reward:** Master Sword permanently upgraded — base 60 damage, durability 40 → ~188. Sword beam at full HP without durability loss. Still depletes energy and recharges in 10 real-time minutes.

**Master Mode:**
- All enemies promoted one tier (Red → Blue → Black → Silver). New Gold tier above Silver.
- All enemies regenerate health when not being attacked
- Floating wooden platforms with enemies and chests throughout the overworld
- Lynel placed on the Great Plateau
- Separate save file from Normal Mode

**Other DLC 1 items:** Hero's Path Mode (records last 200 hours of travel on map), Korok Mask (shakes near hidden Koroks), Phantom/Midna/Tingle/Majora's Mask armor sets.

#### DLC Pack 2: The Champion's Ballad (December 7, 2017)

**Prerequisite:** All 4 Divine Beasts freed.

**Phase 1 — One-Hit Obliterator:**
- Weapon that kills any enemy in one hit, but Link also dies in one hit while wielding it
- 2 charges, recharge over time
- Clear 4 enemy camps on the Great Plateau → reveals 4 new shrines

**Phase 2 — Champion Song Quests:**
- 4 quest lines (one per Champion), each with 3 overworld trials revealing 3 new shrines
- After each set, refight the corresponding Blight in an Illusory Realm with preset limited equipment (Blights have 1,500 HP)
- **Reward per Champion:** Ability upgraded to "+" version with halved cooldowns

**Phase 3 — Final Trial:**
- 5th Divine Beast dungeon beneath the Shrine of Resurrection
- Boss: Monk Maz Koshia (3 phases: fast teleporting melee → 9 clones → giant size with lightning attacks)
- **Reward:** Master Cycle Zero motorcycle rune

**Total DLC shrines:** 16 (4 from One-Hit Obliterator + 12 from Champion quests).

---

## 6. Items & Equipment

### 6.1 Weapon System

Five equipment categories: one-handed weapons, two-handed weapons, spears, bows, shields.

**Durability system:**
- Every weapon, bow, and shield has a hidden durability value (hit count before breaking)
- The game does NOT display exact numbers
- Feedback: sparkle icon at full durability; flashing red + "badly damaged" warning near breaking
- Each hit costs 1 durability; hitting nothing costs 0
- The final hit (breaking hit) deals 2x damage

**Weapon modifiers (random bonuses on late-game drops):**

| Modifier | Effect |
|---|---|
| Attack Up / Attack Up+ | Flat bonus to base attack |
| Durability Up / Durability Up+ | Bonus durability |
| Long Throw | Increased throw distance |
| Critical Hit | Chance of 2x on normal strikes |
| Guard Up / Guard Up+ (shields) | Increased Shield Guard |
| Shield Surf Up (shields) | Reduced surfing durability loss |

**Representative weapon stats (one-handed swords):**

| Weapon | Attack | Durability |
|---|---|---|
| Traveler's Sword | 5 | 20 |
| Soldier's Broadsword | 14 | ~26 |
| Knight's Broadsword | 26 | ~36 |
| Royal Broadsword | 36 | ~36 |
| Royal Guard's Sword | 48 | 14 |
| Savage Lynel Sword | 58 | 41 |

**Representative weapon stats (two-handed):**

| Weapon | Attack | Durability |
|---|---|---|
| Traveler's Claymore | 10 | 20 |
| Soldier's Claymore | 20 | 25 |
| Knight's Claymore | 38 | 30 |
| Royal Claymore | 52 | 40 |
| Savage Lynel Crusher | 78 | 35 |

**Representative weapon stats (spears):**

| Weapon | Attack | Durability |
|---|---|---|
| Traveler's Spear | 3 | 18 |
| Soldier's Spear | 7 | ~35 |
| Knight's Halberd | 13 | ~30 |
| Royal Halberd | 26 | ~28 |
| Savage Lynel Spear | 30 | 35 |

**Representative bow stats:**

| Bow | Damage | Durability | Range | Special |
|---|---|---|---|---|
| Wooden Bow | 4 | 20 | 20 | — |
| Traveler's Bow | 5 | 22 | 20 | — |
| Soldier's Bow | 14 | 36 | 20 | — |
| Phrenic Bow | 10 | 45 | 40 | Zoom (scope when aiming) |
| Knight's Bow | 26 | 48 | 20 | — |
| Falcon Bow | 20 | 50 | 40 | Quick-shot (Rito) |
| Royal Bow | 38 | 60 | 20 | — |
| Golden Bow | 14 | 60 | 40 | Long range (Gerudo) |
| Great Eagle Bow | 28 | 60 | 40 | 3-shot (Revali's; reforge-able) |
| Duplex Bow | 14 | 18 | 40 | 2-shot (Yiga) |
| Lynel Bow | 10 | 30 | 20 | 3-shot |
| Mighty Lynel Bow | 20 | 35 | 20 | 3-shot |
| Savage Lynel Bow | 32 | 45 | 20 | 3-shot (5-shot with modifier) |
| Ancient Bow | 44 | 120 | 40 | Near-zero arrow drop; +50% vs Guardians |
| Royal Guard's Bow | 50 | 20 | 30 | Glass cannon |
| Bow of Light | 100 | 100 | 500 | Final boss only |

**Range** is an internal stat affecting arrow travel distance before gravity takes over. Standard = 20; long range = 40. Multi-shot bows fire multiple arrows per shot but consume only 1 arrow.

**Master Sword:**

| Property | Base | Powered Up | Post-Trial of the Sword |
|---|---|---|---|
| Damage | 30 | 60 | 60 (permanent) |
| Durability | 40 | 40 | ~188 |
| Recharge | 10 min | 10 min | 10 min |

Powers up near Guardians, Malice, Divine Beasts, and in Hyrule Castle. Requires 13 hearts to pull from the pedestal in Korok Forest.

### 6.2 Arrow Types

| Type | Bonus Damage | Special Effect | Price (~per arrow) |
|---|---|---|---|
| Normal | +0 | — | ~4–5 rupees |
| Fire | +10 | Sets target/environment on fire | ~14 rupees |
| Ice | +10 | Freezes target (next hit = 3x) | ~14 rupees |
| Shock | +20 | Stuns, drops weapons; 40 AoE in water | ~14 rupees |
| Bomb | +50 | Explosion AoE; self-detonates in rain/extreme heat | ~35 rupees |
| Ancient | Special | Vaporizes non-boss enemies (no drops); one-shots Guardian eye; 1.5x vs bosses | ~90 rupees + materials |

Multi-shot bows: only one arrow applies the flat elemental damage bonus.

**Opposite-element kills:** Ice Arrow vs Fire enemy (or vice versa) = instant kill on basic elemental enemies.

### 6.3 Armor System

**Great Fairy Fountains:** 4 total. Unlock costs based on discovery order:
- 1st: 100 rupees
- 2nd: 500 rupees
- 3rd: 1,000 rupees
- 4th: 10,000 rupees

Each fairy enables one additional upgrade tier (max 4 tiers). Set bonuses activate when all 3 pieces are worn and each is upgraded to at least tier 2.

**Defense per piece:** Each point of armor defense reduces damage by 1/4 heart. Minimum damage is always 1/4 heart.

**Armor sets and set bonuses:**

| Set | Passive Effect | Set Bonus (tier 2+) |
|---|---|---|
| Hylian | — | — |
| Soldier's | — | — |
| Sheikah (Stealth) | Stealth Up | Night Speed Up |
| Climbing | Climb Speed Up | Climbing Jump Stamina Up |
| Barbarian | Attack Up | Charge Attack Stamina Up |
| Snowquill | Cold Resistance | Unfreezable |
| Flamebreaker | Flame Guard | Fireproof |
| Rubber | Shock Resistance | Unshockable |
| Zora | Swim Speed Up | Swim Dash Stamina Up |
| Desert Voe | Heat Resistance | Shock Damage Resist |
| Gerudo | Heat Resistance | — (disguise) |
| Radiant | Bone Attack Up | Stal Disguise + Bone Weapon Proficiency |
| Ancient | Guardian Resist Up | Ancient Proficiency (+80% ancient weapon damage) |
| Wild (all 120 shrines) | — | Master Sword Beam Up |

**Armor inventory limit:** 100 pieces total.

**Defense progression example (Soldier's Set, per piece):** Base 4 → tier 1: 7 → tier 2: 12 → tier 3: 18 → tier 4: 28. Max set total: 84.

### 6.4 Cooking System

Cook at any lit Cooking Pot by holding up to 5 ingredients and dropping them in.

**Rules:**
- Food ingredients → meals (heal hearts, may grant timed buff)
- Critters + monster parts → elixirs (buff only, no heart recovery)
- Mixed food + critters/monster parts → Dubious Food (small random heal, no effects)
- Wood/ore + food → Rock-Hard Food (1/4 heart)
- Single item over open flame → roasted (1.5x healing vs raw)

**Timed buff effects (only one active at a time; new one overwrites):**

| Effect | Keyword | Details |
|---|---|---|
| Attack Up | Mighty | Lv1: +20%, Lv2: +30%, Lv3: +50% |
| Defense Up | Tough | Lv1: +4, Lv2: +12, Lv3: +24 |
| Speed Up | Hasty | 3 tiers of movement speed |
| Stealth Up | Sneaky | 3 tiers of noise reduction |
| Cold Resistance | Spicy | 2 tiers |
| Heat Resistance | Chilly | 2 tiers |
| Electric Resistance | Electro | 3 tiers |
| Fireproof | Fireproof | Burning protection |

**Non-timed effects (do not conflict with timed buffs):**
- **Hearty** — full heal + yellow bonus hearts (no duration, lasts until lost)
- **Energizing** — restores stamina
- **Enduring** — restores + adds yellow temporary stamina

**Effect potency:** Each ingredient has hidden potency points. Tier thresholds: tier 1 = 1–4 pts, tier 2 = 5–6 pts, tier 3 = 7+ pts. Mixing different effect keywords cancels all effects.

**Duration:** Base from first effect ingredient; each additional matching ingredient adds 30 seconds. Max buff duration: 30 minutes. Dragon horn shards set duration to max.

**Critical cook:** Flat 10% chance. Blood Moon cooking (11:30 PM – 12:15 AM) guarantees critical. Five possible bonuses (equal probability): +3 bonus hearts, +1 yellow heart, +1 effect tier, +5 min duration, +2/5 stamina wheel.

### 6.5 Dragon Parts

Three spirit dragons (Dinraal, Naydra, Farosh) fly fixed paths. Shooting them drops one part based on hit location:

| Hit Location | Drop | Cooking Duration Bonus | Sell Price |
|---|---|---|---|
| Body | Scale | +1:30 | 150 rupees |
| Claw/foot | Claw | +3:30 | 180 rupees |
| Mouth/fang | Fang Shard | +10:30 | 250 rupees |
| Horn | Horn Shard | +30:00 (sets to max) | 300 rupees |

One drop per encounter. All three spawn around 5:00 AM game time. Naydra must first be freed from Malice on Lanayru Mountain.

### 6.6 Elemental Weapons

**Elemental blades (one-handed):**

| Weapon | Damage | Durability | Element | Passive |
|---|---|---|---|---|
| Flameblade | 24 | ~36 | Fire | Keeps Link warm |
| Frostblade | 20 | ~30 | Ice | Keeps Link cool |
| Thunderblade | 22 | ~36 | Electric | — |

**Elemental greatswords (two-handed):**

| Weapon | Damage | Durability | Element | Charged Attack |
|---|---|---|---|---|
| Great Flameblade | 34 | ~50 | Fire | Fire explosion AoE |
| Great Frostblade | 30 | ~40 | Ice | Freeze AoE |
| Great Thunderblade | 32 | ~50 | Electric | Electric discharge AoE |

**Elemental rods (one-handed, ranged-focused):**

| Weapon | Melee Dmg | Durability | Projectiles | Proj. Dmg |
|---|---|---|---|---|
| Fire Rod | 5 | 14 | 1 fireball | ~10 |
| Ice Rod | 5 | 14 | 1 ice orb | ~10 + freeze |
| Lightning Rod | 5 | 14 | 1 lightning ball | ~20 + shock |
| Meteor Rod | 10 | 32 | 3 fireballs | ~10 each |
| Blizzard Rod | 10 | 32 | 3 ice orbs | ~10 each + freeze |
| Thunderstorm Rod | 10 | 32 | 3 lightning balls | ~20 each + shock |

**Elemental mechanics:**
- Elemental blades briefly lose their elemental charge after an elemental hit; subsequent hits deal only base damage until recharged
- Rods recharge their elemental projectile after each cast (few seconds between casts); using rods as melee weapons drains durability faster
- Equipped fire weapons provide warmth; ice weapons provide cooling (affects temperature gauge)

### 6.7 Hylian Shield

| Property | Value |
|---|---|
| Shield Guard | 90 (highest in game) |
| Durability | 800 (highest in game; next-best shields are ~20–30) |
| Location | Hyrule Castle Lockup, after defeating a Stalnox |
| Replacement | Grante in Tarrey Town, 3,000 rupees (requires completing "From the Ground Up" + having found the original) |

Despite its extreme durability, the Hylian Shield can break. It does not auto-deflect Guardian lasers (unlike the Ancient Shield).

### 6.8 Rusty Weapon Conversion

Rock Octoroks (Death Mountain path, Gerudo Highlands) can convert **rusty weapons** into clean versions. When a Rock Octorok inhales, throw/drop a rusty weapon in front of it — it sucks the weapon in and spits out a random clean weapon (Traveler's through Royal tier). Each Rock Octorok can do this once per Blood Moon cycle. This does NOT repair non-rusty weapons or restore durability on used weapons.

---

## 7. Enemies & Opponents

### 7.1 Enemy Scaling System

A hidden "kill point" counter (internally `Ecosystem::LevelSensor`) tracks enemy kills. Each enemy type awards points on kill (capped at 10 kills per type). As cumulative points rise, overworld enemies upgrade color tiers.

**Color tier progression (Normal Mode):**
- Bokoblin: Red → Blue → Black → Silver
- Moblin: Red → Blue → Black → Silver
- Lizalfos: Green → Blue → Black → Silver (elemental variants do NOT scale)
- Lynel: Red → Blue → White-Maned → Silver

**Master Mode** adds a Gold tier above Silver for all four scalable species.

Scaling is disabled on Eventide Island and in the Trial of the Sword.

**Kill point values (select enemies, datamined):**

| Enemy | Points |
|---|---|
| Blue Bokoblin | 15 |
| Black Bokoblin | 25 |
| Blue Moblin | 18 |
| Black Moblin | 35 |
| Blue Lizalfos | 20 |
| Black Lizalfos | 40 |
| Red Lynel | 50 |
| Blue Lynel | 60 |
| White Lynel | 80 |
| Silver Lynel | 120 |
| Guardian Stalker/Skywatcher | 35 |
| Yiga Blademaster | 100 |
| Blight Ganon | 300 |
| Calamity Ganon | 800 |

### 7.2 Enemy Types and HP

**Bokoblin** (melee/ranged goblins, camp in groups):

| Tier | HP |
|---|---|
| Red | 13 |
| Blue | 72 |
| Black | 240 |
| Silver | 720 |
| Gold (Master Mode) | 1,080 |

**Moblin** (tall, heavy-hitting):

| Tier | HP |
|---|---|
| Red | 56 |
| Blue | 144 |
| Black | 360 |
| Silver | 1,080 |
| Gold (Master Mode) | ~1,620 |

**Lizalfos** (fast, camouflage, tongue lash):

| Tier | HP |
|---|---|
| Green | 50 |
| Blue | 120 |
| Black | 288 |
| Silver | 864 |
| Gold (Master Mode) | 1,296 |
| Fire/Ice/Electric (fixed) | 160 |

**Lynel** (centaur-like, most dangerous overworld enemy):

| Tier | HP | Weapon Tier |
|---|---|---|
| Red-Maned | 2,000 | Lynel (base) |
| Blue-Maned | 3,000 | Mighty Lynel |
| White-Maned | 4,000 | Savage Lynel |
| Silver | 5,000 | Savage Lynel |
| Gold (Master Mode) | 7,500 | Savage Lynel |

Lynel attack patterns: charge, fireball (single or triple), fire burst (expanding sphere), ground pound/shockwave, weapon combos, bow shots (including elemental), teleport (if Link retreats far).

**Mountable:** Stun a Lynel (headshot, Urbosa's Fury, or flurry rush) to mount. Mounted melee attacks deal full damage with NO durability loss.

**Lynel weapon stats:**

| Weapon | Base | Mighty | Savage |
|---|---|---|---|
| Sword (1H) | 24 | 36 | 58 |
| Crusher (2H) | ~36 | 54 | 78 |
| Spear | 14 | 25 | ~30 |
| Bow (3-shot) | 10×3 | 20×3 | 32×3 |
| Shield | 16 guard | 29 guard | 44 guard |

**Hinox** (giant cyclops):

| Tier | HP |
|---|---|
| Red | 600 |
| Blue | 800 |
| Black | 1,000 |
| Stalnox (skeletal, night) | 1,000 |

**Stone Talus** (rock golem, weak point = ore deposit):

| Variant | HP |
|---|---|
| Stone Talus | 300–900 (varies by subtype) |
| Luminous Talus | 600 |
| Rare Talus | 900 |
| Igneo Talus | 800 |
| Frost Talus | 800 |

**Molduga** (sand worm, Gerudo Desert): 1,500 HP. Attracted to vibrations; throw bombs on sand to lure.

**Guardian types:**

| Type | HP | Behavior |
|---|---|---|
| Guardian Stalker | 1,500 | 6-legged walker, patrols, fires laser |
| Guardian Skywatcher | 1,500 | Flies, searchlight, fires laser |
| Guardian Turret | 1,500 | Stationary, Hyrule Castle |
| Decayed Guardian | 500 | Immobile, fires laser |
| Guardian Scout I | 13 | Shrine, laser only |
| Guardian Scout II | 375 | Shrine, one weapon |
| Guardian Scout III | 1,500 | Shrine, two weapons |
| Guardian Scout IV | 3,000 | Shrine, three weapons (Major Test) |

**Guardian laser:** 52 physical + 6 fire = ~58 total damage. Perfect parry reflects for 500 damage. Ancient Shield auto-deflects (costs durability). In Master Mode, Stalkers sometimes delay charge-up timing.

**Other enemies:**

| Enemy | HP | Notes |
|---|---|---|
| Keese (all types) | 1 | Elemental variants drop element on contact |
| ChuChu (small/med/large) | 3/12/48 | Split on hit; elemental variants explode on death |
| Wizzrobe (basic) | 150 | Floats, changes local weather |
| Wizzrobe (advanced) | 300 | Meteo/Blizzrobe/Thunder Wizzrobe |
| Octorok (all types) | 8 | Ambush, hides in terrain |
| Yiga Footsoldier | ~32 | Disguised as travelers, teleports away (doesn't die) |
| Yiga Blademaster | 600 | Disguised, Windcleaver (40 dmg), vacuum projectiles |

### 7.3 Boss Fights

**Blight Ganons** — HP scales by encounter order:
- 1st fought: 800 HP
- 2nd: 1,200 HP
- 3rd: 1,600 HP
- 4th: 2,000 HP
- Hyrule Castle: always 2,000 HP
- DLC Illusory Realm: 1,500 HP

All Blights have two phases, switching at 50% HP.

| Boss | Dungeon | Phase 1 | Phase 2 |
|---|---|---|---|
| Waterblight Ganon | Vah Ruta | Spear thrusts, melee | Raises water, ice blocks (Cryonis to shatter) |
| Fireblight Ganon | Vah Rudania | Sword swings, fireballs | Massive fireball with suction (bomb interrupt) |
| Windblight Ganon | Vah Medoh | Arm cannon blasts, tornados | Summons 4 turret drones |
| Thunderblight Ganon | Vah Naboris | Fast dash attacks, electric axe | Drops metal pillars (Magnesis), lightning combos |

**Calamity Ganon:** 8,000 HP (reduced by 12.5% per freed Divine Beast).
- Phase 1: Attacks from all four Blights + Guardian laser
- Phase 2 (50% remaining): Orange energy shield — must use Perfect Parry or Urbosa's Fury to penetrate

**Dark Beast Ganon:** Cinematic horseback finale. Zelda gives the Bow of Light. Shoot 6 glowing weak points on body, then belly, then forehead. Cannot lose.

**Master Kohga:** 300 HP, 3 phases (boulder, spinning boulders, Magnesis metal ball).

**Monk Maz Koshia (DLC):** 3 phases — fast teleporting melee → 9 clones → giant size with lightning.

### 7.4 Ancient/Guardian Weapons

**Guardian weapons (dropped by Guardian Scouts):**

| Weapon | Base | + | ++ |
|---|---|---|---|
| Guardian Sword | 20 / 17 dur | 30 / 22 dur | 40 / 32 dur |
| Guardian Spear | 10 / 17 dur | 15 / 22 dur | 20 / 32 dur |
| Ancient Battle Axe | 30 / 17 dur | 45 / 22 dur | 60 / 32 dur |

**Crafted ancient weapons (Akkala Ancient Tech Lab):**

| Weapon | Attack | Durability |
|---|---|---|
| Ancient Short Sword | 40 | 54 |
| Ancient Bladesaw | 55 | 50 |
| Ancient Spear | 30 | 50 |
| Ancient Bow | 44 | 120 |
| Ancient Shield | 70 guard | 32 |

**Damage bonuses vs Guardians:**
- Guardian Scout weapons: +30% (innate)
- Crafted ancient weapons: +50% (innate)
- Ancient Proficiency set bonus: +80% (multiplicative)

---

## 8. Economy

### 8.1 Currency

**Rupee values:**

| Color | Value |
|---|---|
| Green | 1 |
| Blue | 5 |
| Red | 20 |
| Purple | 50 |
| Silver | 100 |
| Gold | 300 |

**Wallet cap:** 999,999 rupees. No wallet upgrades — flat cap from the start. Rupees earned beyond the cap are lost.

### 8.2 Gem Values

| Gem | Standard Sell Price |
|---|---|
| Flint | 5 |
| Amber | 30 |
| Opal | 60 |
| Luminous Stone | 70 |
| Topaz | 180 |
| Ruby | 210 |
| Sapphire | 260 |
| Diamond | 500 |

**Ramella (Goron City)** pays premium for bulk gem sales (10 of a type).

### 8.3 Shop Types

- **General Stores:** Every settlement; ingredients, arrows, sundries
- **Armor Shops:** Each settlement sells region-themed armor
- **Beedle (traveling merchant):** Every stable; bugs, ingredients, arrows
- **Akkala Ancient Tech Lab (Cherry):** Ancient armor, weapons, arrows (ancient materials + rupees)
- **Gerudo Secret Club:** Radiant Set
- **Tarrey Town:** Specialty goods (unlocked via From the Ground Up quest)
- **Grante (Tarrey Town):** Replacements for unique/one-of-a-kind armor pieces if lost

### 8.4 Rupee Farming

1. **Dragon Horn farming:** Farosh Horn Shard = 300 rupees, farmable every few minutes
2. **Stone Talus hunting:** Gems from defeated Taluses
3. **Snow bowling (Hebra):** 20 rupee entry, 300 payout for a strike
4. **Gourmet meat skewers:** 5× Raw Gourmet Meat cooked = sells for 490 rupees
5. **Excess monster/ancient part sales**

---

## 9. Minigames & Side Systems

### 9.1 Horses

**Taming:** Sneak up and mount. Wild-temperament horses buck — press L repeatedly to soothe. Soothe presses required: most horses = 15, 4-star speed = 18, White Horse / 5-star = 25, Giant Horse = 30.

**Horse stats (1–5 stars):**
- **Strength:** Horse HP
- **Speed:** Gallop speed
- **Stamina:** Number of spurs (recharge while trotting)
- **Temperament:** Gentle (easy) or Wild (better stats, harder to tame)

**Bond system:** Internal 0–100 value. Starts at 0 for wild-caught horses (rescued horses from Bokoblins start at 100).
- Low bond: horse disobeys steering, veers off course, slows down
- Max bond (100): full obedience, unlocks cosmetic customization (bridle, saddle, mane) at stables
- Bond is permanent once maxed — cannot decrease

| Action | Bond Increase |
|---|---|
| Soothe after gallop (L) | +3 |
| Feed (apple, carrot, etc.) | +10 |
| Soothe when horse disobeys | +3 |

Feeding an Endura Carrot grants +3 temporary spurs.

**Registration:** At any of 15 stables. Max 5 registered horses. Costs 20 rupees. Retrieved from any stable.

**Special horses:**

| Horse | Stats | Notes |
|---|---|---|
| Giant Horse | 5/2/0 | No spurs; Taobab Grassland; cannot change gear |
| Epona | 4/4/4 Gentle | SSB Link amiibo exclusive; max bond immediately |
| Lord of the Mountain | Max all, infinite stamina | Cannot be registered; Satori Mountain, rare nights |
| Stalhorse | — | Skeletal; cannot be registered; night only |

**Horse revival:** Malanya (Horse God) at Malanya Spring. 1,000 rupees to awaken. First revival free, subsequent cost an Endura Carrot.

**DLC horse gear:**
- Ancient Bridle: +2 bonus spurs
- Ancient Saddle: Whistle-teleport horse to Link

### 9.2 Sand Seals

Gerudo Desert traversal. Link holds a rope attached to a sand seal while shield-surfing behind it.

- **Rental:** 20 rupees from the Sand-Seal Rental Shop in Gerudo Town (+ 50 rupees for a loaner shield if needed)
- **Wild seals:** must sneak up on (excellent hearing); flee if they detect Link, and bolt if rider dismounts
- **Controls:** Left stick = steer, Left stick back = slow/stop, A = speed boost
- **Durability:** sand-seal surfing does NOT significantly consume shield durability (unlike regular shield surfing on rough terrain)
- **Terrain:** only works on sand in Gerudo Desert

### 9.3 Shield Surfing

Stand on shield and slide downhill. Costs shield durability (reduced by Shield Surf Up modifier). Faster on snow and sand; rougher terrain (rock, grass) costs more durability per second.

### 9.4 Beds & Inns

| Bed Type | Cost | Recovery |
|---|---|---|
| Regular Bed (any inn/stable) | 20 rupees | Full heart recovery |
| Soft Bed (stables) | 40 rupees | Full hearts + 1 temporary yellow heart |
| Premium Inn (race-specific) | 80 rupees | Full hearts + 3 temporary yellow hearts + 1 temporary stamina wheel |

**Premium inns** (all 80 rupees): Hotel Oasis (Gerudo Town), Seabed Inn (Zora's Domain), Swallow's Roost (Rito Village), Rollin' Inn (Goron City). All give identical bonuses. There are 15 stables, each with regular and soft beds.

### 9.5 Minigames

| Minigame | Location | Cost | Best Reward |
|---|---|---|---|
| Snowball Bowling | Pondo's Lodge, Hebra | 20 | 300 rupees (strike) |
| Shield Surfing Race | Selmie's Spot, Hebra | Free | Royal Shield |
| Boom Bam Golf | Tanagar Canyon | 20 | 100 rupees |
| Bird-Man Research Study | Ridgeland Tower | 20 | 300 rupees |
| Mounted Archery | Faron Grasslands | Free | Knight's Bridle & Saddle |
| Sand-Seal Racing | Gerudo Desert | Entry fee | 300 rupees |
| Mounted Obstacle Course | Highland Stable | 70 | Extravagant Bridle & Saddle |
| Chest Game (gambling) | Lurelin Village | 10–100 | 300 rupees |
| Super Gut Check | Gut Check Rock, Eldin | 100 | 300 rupees |
| Footrace | Hyrule Ridge | Free | 50 rupees |

### 9.6 Gerudo Town & Disguise

**Entry requirement:** Link must wear the Gerudo Vai outfit (Gerudo Veil + Gerudo Top + Gerudo Sirwal) to enter. Guards enforce the "no men" rule — removing any piece while inside causes immediate ejection.

**Obtaining:** Purchased from Vilia atop Kara Kara Bazaar for 600 rupees (full set) during the "Forbidden City Entry" main quest. Also available piece-by-piece inside Gerudo Town for 180 rupees each (540 total).

**Stats:** Level 1 Heat Resistance, 1 defense per piece (3 total). Sand Boots or Snow Boots can replace the Sirwal and maintain the disguise.

### 9.7 Sheikah Sensor & Compendium

**Camera Rune:** Photographs subjects for the Hyrule Compendium. Orange highlight = new, blue = logged.

**Hyrule Compendium:** 385 entries across 5 categories:
- Creatures: 83
- Monsters: 76
- Materials: 36
- Equipment: 185
- Treasure: 5

Entries can be purchased from Symin (Hateno Tech Lab) for 100 rupees each.

**Sheikah Sensor:** Base version beeps near undiscovered shrines. Sensor+ (upgrade: 3 Ancient Screws) can track any Compendium entry. Beeping frequency indicates directional accuracy.

### 9.8 amiibo

Each amiibo scannable once per real-world day. Non-Zelda amiibo drop generic supplies.

**Key Zelda-series amiibo rewards:**

| amiibo | Exclusive Reward |
|---|---|
| Link (SSB / Twilight Princess) | Epona + Twilight outfit set |
| Zelda (SSB) | Twilight Bow |
| Toon Link | Hero of Wind outfit + Sea-Breeze Boomerang |
| Ocarina of Time Link | Time outfit + Biggoron's Sword |
| Majora's Mask Link | Fierce Deity outfit + Fierce Deity Sword |
| Skyward Sword Link | Sky outfit + Goddess Sword |
| Wolf Link | Wolf Link companion (HP from TP save) |
| Champion amiibo (Urbosa/Revali/Mipha/Daruk) | Divine Beast Helms + faction weapons |

Divine Beast Helms function as part of the Ancient Armor set for set bonus purposes.

---

## 10. UI & HUD

### 10.1 Standard HUD

- **Hearts** (top-left): Current HP. Yellow temporary hearts from food shown separately.
- **Stamina Wheel** (beside hearts): Appears only during strenuous actions. Green circular wheel. Yellow extensions for temporary stamina.
- **Minimap** (bottom-right): Topography, pins, quest markers, nearby shrines/towers. Compass ring with cardinal directions.
- **Temperature Gauge** (bottom-right): Numeric + gauge. Blue = cold, red = hot. Flashes at dangerous thresholds.
- **Noise Meter** (bottom-right): Wave indicator for stealth — larger waves = louder.
- **Time of Day** (bottom-right): Clock dial with sun/moon position.
- **Equipped Loadout**: Current weapon, bow, shield, rune/arrow type indicators.
- **Champion Ability Icons**: Available/cooldown status.
- **Quest Tracker** (top): Active quest objective text.

### 10.2 Pro HUD Mode

Removes: minimap, temperature gauge, noise meter, time display, quest tracker, loadout icons. Keeps: hearts (always), stamina wheel (when active). Toggled in System Settings → HUD Mode.

### 10.3 Menu Structure

**Inventory (+ button):** Weapons / Bows / Shields / Armor / Materials / Food / Key Items tabs.

**Map (- button):** Full Hyrule map with stamps, pins, fast travel. Adventure Log (Main Quests / Side Quests / Shrine Quests tabs).

**System (+ then R):** Save / Load / Options / amiibo.

---

## 11. Engine & Presentation Systems

### 11.1 Save System

- **Manual save:** 1 slot per profile. Autosaves never overwrite manual saves.
- **Autosave:** Triggers ~every 5 minutes and on key events (opening chests, resting, approaching shrines/stables, completing objectives).
- **Save slots:** Normal Mode: 1 manual + 5 autosave = 6 total. Master Mode: 1 manual + 1 autosave = 2 total. Separate from Normal Mode.

### 11.2 Camera Behavior

- Third-person follow camera with right-stick control
- ZL centers camera behind Link (or locks onto targeted enemy)
- First-person scope mode via right-stick click
- Bullet-time: automatic slow-motion when aiming bow in midair (drains stamina)
- Camera pulls back during horseback riding for wider field of view

### 11.3 Audio System

- Dynamic music system: layers instruments based on context (exploration = sparse piano/ambient, combat = full orchestral, settlement = themed instrumentation)
- Music intensifies during combat and fades to ambient during exploration
- Enemy awareness states reflected in music (suspicious = building tension, alerted = full combat)
- Each region has distinct ambient instrumentation reflecting its culture and environment
- Blood Moon has a unique escalating theme
- Divine Beasts have interior ambient soundscapes tied to dungeon state

### 11.4 Difficulty Settings

No selectable difficulty in base game. **Master Mode** (DLC) is the hard mode — see §5.4.

---

## 12. Open Questions / Unverified

| Topic | Issue |
|---|---|
| Fall damage formula | Exact height-to-damage curve not publicly extracted from game data |
| Magnesis range | Functional range not documented in precise units |
| Cryonis pillar dimensions | Exact size not documented |
| Revali's Gale height | Exact altitude gain not documented |
| Stamina drain rates | Per-second drain for each activity not precisely documented (internal units) |
| Perfect dodge/parry frame windows | Approximate (~0.1s) but exact frame counts not publicly confirmed |
| Flurry rush hit counts | Sources vary slightly: spear listed as 7–10 hits |
| Starting weapon slots | Conflicting reports: 8 (most common) vs 5 (GameWith) |
| Max weapon/shield slots | Minor variance: 16–19 weapons, 19–20 shields depending on source |
| Master Sword post-Trial durability | Reported as both 188 and 200 |
| Golden Moblin HP | Widely cited as ~1,620 but no primary datamined source confirms exact value |
| Stone Talus HP variance | Base variant listed as 300–900 across sources (may reflect subtypes) |
| Map size | 80 km² (datamined coordinates) vs ~360 km² (early pre-release estimate, now considered inaccurate) |
| Golden Bow range | Conflicting: 20 (Samurai Gamers) vs 40 (Zelda Dungeon). 40 seems more consistent with its design as a Gerudo precision bow |
| Freeze/stun duration | Exact seconds/frames not publicly documented; varies by enemy tier (Gold enemies have reduced duration) |
| Burn tick rate | Exact damage-per-second for environmental fire not publicly documented |
| Detection ranges | No verified numeric range values (meters/units) for enemy visual/audio detection exist in public data |
| Elemental blade durability | Approximate values (~30–50) reported across sources with slight variance |
| Night detection | No confirmed reduction in detection ranges at night; commonly assumed but unverified |

---

## 13. References

### Datamining & Technical
- [ZeldaMods — Difficulty Scaling](https://zeldamods.org/wiki/Difficulty_scaling)
- [ZeldaMods — Blood Moon](https://zeldamods.org/wiki/Blood_moon)
- [ZeldaMods — Time System](https://zeldamods.org/w_botw/index.php?title=Time)
- [ZeldaMods — bgparamlist](https://zeldamods.org/wiki/Bgparamlist)
- [leoetlino/botw-re-notes — Difficulty Scaling (GitHub)](https://github.com/leoetlino/botw-re-notes/blob/master/difficulty_scaling.md)
- [Enemy & Weapon Scaling Gist (leoetlino)](https://gist.github.com/leoetlino/6347f5cb95dbd0c5cb3241bb77e4d266)
- [All Enemy Health Gist (m-byte918)](https://gist.github.com/m-byte918/1d1acc569ad1cd59ead8e151fddf4591)
- [BotW Damage Calculator (restite.org)](https://restite.org/damage/)

### Wikis
- [Zelda Wiki (Fandom)](https://zelda.fandom.com/wiki/)
- [Zelda Dungeon Wiki](https://www.zeldadungeon.net/wiki/)
- [StrategyWiki — BotW](https://strategywiki.org/wiki/The_Legend_of_Zelda:_Breath_of_the_Wild)

### Guides & FAQs
- [GameFAQs — BotW Damage Info Guide](https://gamefaqs.gamespot.com/switch/189707-the-legend-of-zelda-breath-of-the-wild/faqs/75488)
- [GameFAQs — Cooking Guide by Explopyro](https://gamefaqs.gamespot.com/switch/189707-the-legend-of-zelda-breath-of-the-wild/faqs/74776)
- [Game8 — BotW Guides](https://game8.co/games/Zelda-Breath-of-the-Wild)
- [GameWith — BotW Guides](https://gamewith.net/zelda-botw/)

### Technical Talks
- [GDC 2017 — Open-Air Adventure: Chemistry Engine (Takuhiro Dohta)](https://www.thumbsticks.com/gdc-17-breath-of-the-wild-science-lies/)

### Community Analysis
- [Game Rant — Attack/Defense Boost Calculations](https://gamerant.com/zelda-breath-of-the-wild-how-attack-boosts-defense-boosts-calculated/)
- [Map Size Analysis (TAColor)](https://www.tacolor.xyz/The_size_of_the_Breath_of_the_Wild_Map.html)
- [BotW Attack Calculations (Scribd)](https://www.scribd.com/document/446379785/BotW-Attack-Calculations)
