# Crash Team Racing — Gameplay Systems Spec

PlayStation 1, Naughty Dog, 1999. This spec covers the original **NTSC-U** release (SCUS-94426). PAL and NTSC-J differences are noted where they affect mechanics.

---

## 1. Core Gameplay Systems

### 1.1 Primary Loop

CTR is an item-based kart racer. The player selects a character, races 3 laps on a closed-circuit track against 7 opponents, and uses collected power-ups to attack rivals or defend. Finishing 1st earns a Trophy (Adventure) or points (Cup Race).

### 1.2 Driving Model

| Mechanic | Input | Description |
|---|---|---|
| Accelerate | X | Hold to drive forward. Each engine class has a distinct acceleration curve. |
| Brake | Square | Decelerates the kart. Enables **Hard Turn** when combined with a direction. |
| Reverse | Square (from standstill) | Hold brake while stationary to drive backward. |
| Hop | L1 or R1 | Small vertical jump. Initiates a Power Slide when combined with a steering direction. |
| Power Slide | L1/R1 + direction | Controlled drift; see §1.3. |
| Fire Item | Circle | Fire forward. Circle + Down drops behind. |
| Toggle Map/Speedo | Triangle | Switches between minimap and speedometer overlay. |
| Camera Cycle | L2 | Cycles: Near → Far → First-Person. |
| Rear View | R2 (hold) | Look behind the kart. |

### 1.3 Power Slide & Turbo Boost

The signature mechanic. During a Power Slide a **Turbo Boost Meter** appears at the bottom-right, filling from green → red.

1. Hop (L1 or R1) while steering left or right to enter the slide.
2. While the meter is in the **red zone** (exhaust smoke turns black), press the **opposite shoulder button** to trigger a mini-turbo.
3. Up to **3 consecutive mini-turbos** can be chained within a single slide. Each successive boost is stronger.
4. Waiting too long (meter overfills) causes a **backfire** — the boost opportunity is lost. Sliding for too long without triggering a boost causes a **spin-out** (60 frames of continuous drift without a successful mini-turbo = spin-out, per decompiled `DriftFramesTillSpinout`).

**Turbo meter thresholds (from MetaPhys):**
- Meter "empty" threshold: 30 frames
- Meter "filled/red" threshold: 15 frames

### 1.4 Fire Levels & Speed Tiers

CTR has a layered speed system with visible exhaust fire colors. Higher tiers grant progressively faster top speed.

| Fire Level | Source | Visual Cue |
|---|---|---|
| None | Base driving | No exhaust fire |
| Green (1–2) | 1–2 mini-turbos in a slide, or hang-time of 66–99 on the jump-o-meter | Small green exhaust |
| Yellow | 3 mini-turbos (perfect triple boost), or hang-time of 100–149 | Yellow exhaust |
| Sacred Fire (SF) | Turbo pads, or hang-time of 150–255 | Red/orange exhaust |
| Ultimate Sacred Fire (USF) | Super Turbo Pads only (specific pads on Hub 4 tracks) | Larger exhaust flame (see §12 — visual distinction is subtle on PS1) |

**Speed caps per fire level (from decompiled MetaPhys, fixed-point units — identical across all engine classes):**

| Parameter | Value |
|---|---|
| Single Turbo Max Speed | 2048 |
| Sacred Fire Max Speed | 4096 |
| Mask Speed | 3900 |
| Fire speed formula | `SingleTurboSpeed + (fireLevel × 8)` |

**USF rules:** Must NOT jump when crossing a Super Turbo Pad. Any non-USF boost or fire-level change cancels USF. Must have reserves built up beforehand. USF consumes reserves rapidly.

### 1.5 Reserve System

Reserves are the hidden backbone of CTR's speed system.

- Every successful mini-turbo adds to an invisible **reserve timer** (stored as a `short` in the Driver struct).
- While reserves remain, the kart maintains its current fire/speed tier.
- Reserves **deplete every frame** when not actively being replenished.
- The turbo bar fills on a **linear** time basis, but reserve gains are **exponential** — a perfectly-timed late boost gives disproportionately more reserves than an early one.
- Full-bar reserve gain constant: 60 (per `turboFullBarReserveGain`).
- Turbo pads add reserves on the first contact frame, then **freeze** reserve depletion while still on the pad.

**What drains reserves:**
- Hitting a wall
- Getting hit by a weapon/obstacle
- Falling off the track
- Braking while on the ground
- Natural frame-by-frame depletion

**What preserves reserves:**
- **Mid-air braking**: Brake while airborne without losing reserves (release before landing).
- **U-Turn**: Brake + Down + direction. The kart performs a sharp turn (up to 180°) without losing reserves.
- **Continuous boost chaining**: Start a new Power Slide immediately after the previous one ends.

### 1.6 Wumpa Fruit

- Maximum of **10 Wumpa Fruit** held at any time. Collected from Fruit Crates or found loose on tracks.
- Each of the first 9 fruit adds approximately **30 speed units** to base kart speed (~2.79% cumulative at 9 fruit).
- The 10th fruit activates **"Juiced Up"** status — all power-ups become their enhanced version (see §4.2). The 10th fruit does not add further speed.
- Getting hit causes Wumpa Fruit to scatter (lose some count), potentially dropping below Juiced Up threshold.

### 1.7 Other Speed Boosts

| Boost Type | Description |
|---|---|
| Start Boost | Press and hold X during the final moment of the countdown — just as the traffic light turns green. Too early = engine stall (brief delay). Too late = no boost. Optimal window is a few frames before the green light. |
| Hang-Time Boost | Airborne time from ramps/ledges grants a landing speed boost scaled by air duration (see §1.4 thresholds). |
| Turbo Pad | Glowing arrows on the track surface. Grants Sacred Fire + reserves on contact. |
| Super Turbo Pad | Rare pads on Hub 4 tracks. Grants USF if conditions are met (see §1.4). |

### 1.8 Terrain & Off-Road

The track surface affects kart speed. Driving off the intended racing line onto different terrain types applies a friction penalty that decelerates the kart.

| Terrain | Effect |
|---|---|
| Road / Tarmac | Normal speed. No penalty. |
| Grass / Dirt | Moderate speed penalty. Common alongside track edges. |
| Sand | Moderate-to-heavy speed penalty. Found on beach tracks (Crash Cove). |
| Mud | Heavy speed penalty. Can trap the kart momentarily. Found on Tiny Arena. |
| Ice | Reduced traction — steering is sluggish, kart slides. No direct speed penalty but harder to maintain racing line. Found on Glacier Park tracks. |
| Water (shallow) | Heavy speed penalty. Found on Dingo Canyon and Polar Pass. |
| Water / Void (deep) | Kart falls off the track and is respawned (see §1.10). |

**Off-road immunity:** The **Aku Aku / Uka Uka Mask** item negates all terrain friction while active — the kart drives at full speed on any surface. Turbo boosts and Sacred Fire do **not** negate off-road friction; only the Mask does.

### 1.9 Hit Recovery & Respawn

**When hit by a weapon:**
1. The kart enters a tumble/spin animation. The player loses control during this animation.
2. Some Wumpa Fruit is lost (scattered onto the track). The amount varies — typically 2–4 fruit per hit.
3. The player's currently held item is **not** lost (except when hit by a Red Beaker, which replaces the held item).
4. All reserves are lost. The kart drops back to base speed (no fire level).
5. After recovery, the player has a brief period of invincibility frames to prevent chain-stuns.

**Falling off the track:**
- When the kart falls into a pit, water, or off the track edge, the character's mask (Aku Aku or Uka Uka) picks the kart up and places it back on the nearest valid track surface.
- All reserves and fire level are lost.
- Any active Mask invincibility is cancelled.
- Wumpa Fruit is lost (same as weapon hit).
- The respawn costs several seconds — roughly equivalent to the tumble animation from a weapon hit.

### 1.10 Race Start Countdown

Races begin with a **traffic light** and voice countdown:
1. Red light — "3" (engines idle, karts in starting grid)
2. Yellow light — "2"
3. Green light — "GO!" — race begins

The Start Boost (§1.7) must be timed to the green light transition. If the player presses X too early during the red or yellow phases, the engine stalls briefly at the start.

### 1.11 Advanced Techniques

| Technique | Input | Effect |
|---|---|---|
| U-Turn | Down + Square + direction | Sharp turn without losing reserves. |
| Mid-Air Turbo | Power Slide in the air | Combines hang-time boost with slide boost on landing. |
| Froggy | Rapidly mash L1/R1 | Frame-dependent rapid hopping; input only registers on draw frames. |
| Air Brake Turn | Jump → brake mid-air → release before landing | Adjust trajectory without reserve loss. |
| Boost Chaining (Turbo Sliding) | Chain triple-boost slides back-to-back | Reserves accumulate across slides, maintaining fire level indefinitely. |

---

## 2. Characters

### 2.1 Engine Classes

The game labels characters as Beginner, Intermediate, or Advanced on the select screen, but internally there are **5 engine classes** with distinct physics. Characters within the same class share identical physics parameters.

**MetaPhys values per engine class (from decompiled source, fixed-point):**

| Parameter | Balanced | Accel | Speed | Turn | Max |
|---|---|---|---|---|---|
| Acceleration (no reserves) | 480 | 544 | 448 | 512 | 544 |
| Acceleration (with reserves) | 1152 | 1152 | 1152 | 1152 | 1152 |
| Top Speed (CLASS_SPEED) | 13140 | 13520 | 13900 | 12950 | 13900 |
| Speedometer Display Speed | 14640 | 15020 | 15400 | 14450 | 15400 |
| Turn Rate | 28 | 26 | 24 | 30 | 30 |
| Gravity | 900 | 900 | 900 | 900 | 900 |
| Jump Force | 4596 | 4596 | 4596 | 4596 | 4596 |
| Brake Friction | 260 | 260 | 260 | 260 | 260 |
| Terminal Velocity | 20480 | 20480 | 20480 | 20480 | 20480 |
| Single Turbo Max Speed | 2048 | 2048 | 2048 | 2048 | 2048 |
| Sacred Fire Max Speed | 4096 | 4096 | 4096 | 4096 | 4096 |
| Mask Speed | 3900 | 3900 | 3900 | 3900 | 3900 |
| Turbo Meter Empty Threshold | 30 | 30 | 30 | 30 | 30 |
| Turbo Meter Filled Threshold | 15 | 15 | 15 | 15 | 15 |
| Drift Frames Till Spinout | 60 | 60 | 60 | 60 | 60 |

The Acceleration class has **higher top speed than Balanced** (13520 vs 13140) while also having higher acceleration (544 vs 480) and only slightly less turn (26 vs 28). This makes Accel-class characters strictly superior to Balanced in competitive play.

### 2.2 Full Roster

**Turn Class** (game label: "Beginner") — High Turn, Low Speed:

| Character | Alignment | Unlock |
|---|---|---|
| Polar | Good (Aku Aku) | Starter |
| Pura | Good (Aku Aku) | Starter |
| Ripper Roo | Evil (Uka Uka) | Win Red Gem Cup |

**Balanced Class** (game label: "Intermediate") — Even Stats:

| Character | Alignment | Unlock |
|---|---|---|
| Crash Bandicoot | Good (Aku Aku) | Starter |
| Dr. Neo Cortex | Evil (Uka Uka) | Starter |
| Komodo Joe | Evil (Uka Uka) | Win Blue Gem Cup |
| Fake Crash | Good (Aku Aku) | Win Purple Gem Cup |
| Nitros Oxide | Evil (Uka Uka) | See §12 |

**Acceleration Class** — High Accel, Mid Speed:

| Character | Alignment | Unlock |
|---|---|---|
| Coco Bandicoot | Good (Aku Aku) | Starter |
| Dr. N. Gin | Evil (Uka Uka) | Starter |
| Pinstripe Potoroo | Evil (Uka Uka) | Win Yellow Gem Cup |

**Speed Class** (game label: "Advanced") — High Speed, Low Turn:

| Character | Alignment | Unlock |
|---|---|---|
| Tiny Tiger | Evil (Uka Uka) | Starter |
| Dingodile | Evil (Uka Uka) | Starter |
| Papu Papu | Evil (Uka Uka) | Win Green Gem Cup |
| N. Tropy | Evil (Uka Uka) | Beat all N. Tropy Time Trial ghosts |

**Max Class** — Top speed + top turn (PAL only):

| Character | Alignment | Unlock |
|---|---|---|
| Penta Penguin (PAL) | Evil (Uka Uka) | Cheat code only |

In NTSC-U/J, Penta Penguin uses the **Turn** class instead of Max.

Good-aligned characters use the **Aku Aku** mask; Evil-aligned characters use the **Uka Uka** mask. Both function identically (see §4.2).

### 2.3 In-Game Stat Display

The character select screen shows Speed/Acceleration/Turning bars on a visual scale. These bars are **cosmetic** and do not map 1:1 to internal values. The actual performance is entirely class-driven per the MetaPhys table above.

### 2.4 Cheat Codes (Character Unlocks)

All codes entered at the main menu while holding **L1 + R1**:

| Character | Code Sequence |
|---|---|
| Ripper Roo | Right, Circle, Circle, Down, Up, Down, Right |
| Papu Papu | Left, Triangle, Right, Down, Right, Circle, Left, Left, Down |
| Komodo Joe | Down, Circle, Left, Left, Triangle, Right, Down |
| Pinstripe | Left, Right, Triangle, Down, Right, Down |
| N. Tropy | Down, Left, Right, Up, Down, Right, Right |
| Penta Penguin | Down, Right, Triangle, Down, Left, Triangle, Up |
| Fake Crash | Circle, Down, Down, Up, Circle, Circle, Down, Left, Right |

### 2.5 Gameplay Cheat Codes

All entered at the main menu while holding **L1 + R1**:

| Cheat | Code Sequence | Effect |
|---|---|---|
| Super Turbo Pads | Triangle, Right, Right, Circle, Left | All turbo pads grant maximum boost. |
| Unlimited Wumpa Fruit | Down, Right, Right, Down, Down | Wumpa count stays at 10; permanently Juiced Up. |
| Infinite Masks | Left, Triangle, Right, Left, Circle, Right, Down, Down | Permanent Mask invincibility. |
| Super Hard Mode | Up, Up, Down, Right, Right, Left, Right, Triangle | AI difficulty set to maximum (0x140). |
| Icy Tracks | Down, Left, Right, Down, Right, Circle, Triangle, Down | All track surfaces have ice-like reduced traction. |
| Turbo Counter | Triangle, Down, Down, Circle, Up | Displays an on-screen counter of consecutive turbo boosts. |
| One-Lap Races | Down, Up, Down, Down, Right, Up, Up, Left, Right | All races are 1 lap instead of 3. |
| Invisible Racers | Up, Up, Down, Right, Right, Up | All karts are invisible during races. |

---

## 3. World Structure — Adventure Mode

### 3.1 Overview

Adventure Mode is the single-player campaign. The player navigates interconnected **hub worlds** on foot, entering portals to access race tracks, boss garages, bonus arenas, and Gem Cups. Progression requires collecting Trophies, Boss Keys, CTR Tokens, Relics, and Gems.

### 3.2 Hub Worlds

5 hub worlds containing 18 race tracks (16 standard + 2 in Gemstone Valley), 4 boss races, and 4 bonus arenas.

#### Hub 1: N. Sanity Beach

| Track | Unlock Requirement | CTR Token Color |
|---|---|---|
| Crash Cove | Available from start | Red |
| Roo's Tubes | Available from start | Green |
| Mystery Caves | 1 Trophy | Red |
| Sewer Speedway | 3 Trophies | Blue |

- **Boss:** Ripper Roo — raced on Roo's Tubes (requires 4 N. Sanity Beach trophies)
- **Bonus Arena:** Skull Rock — crystal challenge, time limit 1:23, awards Purple CTR Token

#### Hub 2: The Lost Ruins

Accessible after defeating Ripper Roo (Boss Key 1).

| Track | Unlock Requirement | CTR Token Color |
|---|---|---|
| Coco Park | Available upon entering hub | Green |
| Tiger Temple | Available upon entering hub | Blue |
| Papu's Pyramid | 6 Trophies (total) | Red |
| Dingo Canyon | 7 Trophies (total) | Yellow |

- **Boss:** Papu Papu — raced on Papu's Pyramid (requires 4 Lost Ruins trophies)
- **Bonus Arena:** Rampage Ruins — crystal challenge, time limit 1:25, TNT crate obstacles, awards Purple CTR Token

#### Hub 3: Glacier Park

Accessible after defeating Papu Papu (Boss Key 2).

| Track | Unlock Requirement | CTR Token Color |
|---|---|---|
| Blizzard Bluff | Available upon entering hub | Red |
| Dragon Mines | 9 Trophies (total) | Blue |
| Polar Pass | 10 Trophies (total) | Green |
| Tiny Arena | 11 Trophies (total) | Yellow |

- **Boss:** Komodo Joe — raced on Dragon Mines (requires 4 Glacier Park trophies, 12 total)
- **Bonus Arena:** Rocky Road — crystal challenge, time limit 1:20, Nitro crate obstacles, awards Purple CTR Token

#### Hub 4: Citadel City

Accessible after defeating Komodo Joe (Boss Key 3).

| Track | Unlock Requirement | CTR Token Color |
|---|---|---|
| Cortex Castle | Available upon entering hub | Green |
| N. Gin Labs | Available upon entering hub | Blue |
| Hot Air Skyway | 14 Trophies (total) | Yellow |
| Oxide Station | 15 Trophies (total) | Yellow |

- **Boss:** Pinstripe Potoroo — raced on Hot Air Skyway (requires all 16 Trophies)
- **Bonus Arena:** Nitro Court — crystal challenge, time limit 2:00, TNT + Nitro obstacles, awards Purple CTR Token

#### Hub 5: Gemstone Valley

Connected to N. Sanity Beach and The Lost Ruins. Contains Gem Cup portals and two bonus tracks.

| Track | Unlock Requirement |
|---|---|
| Slide Coliseum | 10 Relics |
| Turbo Track | All 5 Gems |

- **Final Boss:** Nitros Oxide — raced on Oxide Station (requires all 4 Boss Keys + all 16 Trophies)

### 3.3 Collectibles Summary

| Collectible | Total | Purpose |
|---|---|---|
| **Trophies** | 16 | Win 1st place in each standard race. Gate progression through hubs and bosses. |
| **Boss Keys** | 4 | Defeat each hub boss. Unlock the next hub world and ultimately the final Oxide race. |
| **CTR Tokens** | 20 | 4 each of Red, Green, Blue, Yellow (from CTR Challenges) + 4 Purple (from bonus arenas). Unlock Gem Cups. |
| **Gems** | 5 | Win each Gem Cup (Red, Green, Blue, Yellow, Purple). 5 Gems unlock Turbo Track. |
| **Relics** | 18 | Complete Relic Races on all 18 tracks. 10 Relics unlock Slide Coliseum. All 18 required for true ending. |

### 3.4 Adventure Sub-Challenges

**Trophy Race:** Standard 8-racer race. Finish 1st to earn the Trophy.

**CTR Challenge:** Unlocked after winning the Trophy for that track. Race against 7 opponents, but the letters **C**, **T**, and **R** are placed at specific locations around the track. Collect all three letters **and** finish 1st to earn a colored CTR Token.

**Relic Race:** Unlocked after defeating the hub's boss. Solo time trial (no opponents). Complete 3 laps as fast as possible. **Time Crates** are scattered throughout the track:
- Time Crates display a number (1, 2, or 3) — seconds frozen from the clock when broken.
- Breaking **all** Time Crates on the track awards a **10-second bonus** subtracted from final time.
- Earn Sapphire, Gold, or Platinum Relic based on final time vs. thresholds (see §5.2).

**Crystal Bonus Round:** Played in the bonus arenas. Collect **20 crystals** scattered around the arena within a time limit, navigating TNT/Nitro obstacles. Awards a Purple CTR Token.

### 3.5 Adventure Mode Endings

| Condition | Ending |
|---|---|
| Beat Oxide without full collection | **Bad Ending:** Oxide demands you collect all Relics. |
| 100% (all Trophies, Keys, Tokens, Gems, Relics at Sapphire+) | **Good Ending:** Oxide retreats to Gasmoxia. Credits with character epilogue text. |
| 101% (all above + all Gold or Platinum Relics) | **Best Ending:** Same as Good Ending with confetti during credits. |

---

## 4. Items & Power-Ups

### 4.1 Item Distribution

Items are obtained by driving through **? Crates** (weapon crates on every track). Distribution is weighted by race position. In an 8-racer field, positions map to item sets:

| Position Range | Item Set | Bias |
|---|---|---|
| 1st–2nd | Set 1 | Defensive (Beakers 40%, TNT 40%) |
| 3rd–4th | Set 2 | Mixed |
| 5th–6th | Set 3 | Offensive (Mask 25%, Warp Orb 15%) |
| 7th–8th | Set 4 | Powerful (Mask 40%, Warp Orb 25%) |

**Full item probability table (from decompiled source):**

| Item | Set 1 (1st–2nd) | Set 2 (3rd–4th) | Set 3 (5th–6th) | Set 4 (7th–8th) |
|---|---|---|---|---|
| Beaker | 8/20 (40%) | 10/52 (19%) | 1/20 (5%) | 0/20 (0%) |
| TNT Crate | 8/20 (40%) | 8/52 (15%) | 1/20 (5%) | 0/20 (0%) |
| Bowling Bomb | 2/20 (10%) | 5/52 (10%) | 1/20 (5%) | 0/20 (0%) |
| Bowling Bomb ×3 | 0/20 (0%) | 3/52 (6%) | 2/20 (10%) | 0/20 (0%) |
| Turbo | 1/20 (5%) | 5/52 (10%) | 2/20 (10%) | 2/20 (10%) |
| Power Shield | 1/20 (5%) | 3/52 (6%) | 1/20 (5%) | 1/20 (5%) |
| Tracking Missile | 0/20 (0%) | 3/52 (6%) | 2/20 (10%) | 1/20 (5%) |
| Tracking Missile ×3 | 0/20 (0%) | 2/52 (4%) | 1/20 (5%) | 1/20 (5%) |
| Warp Orb | 0/20 (0%) | 5/52 (10%) | 3/20 (15%) | 5/20 (25%) |
| Aku Aku / Uka Uka | 0/20 (0%) | 7/52 (13%) | 5/20 (25%) | 8/20 (40%) |
| N. Tropy Clock | 0/20 (0%) | 1/52 (2%) | 1/20 (5%) | 2/20 (10%) |

**Special item rules:**
- Only one Warp Orb can exist at a time; if someone already holds one, 3× Missiles are given instead.
- At most 2 players can hold 3× Missiles simultaneously (3+ player mode).
- On **Lap 1**, Item Set 4 is downgraded to Set 3.
- **Boss races** use a special table. Items improve with loss count: 0–2 losses → Clock/Mask/Warp Orb replaced with 3× Missiles; 3 losses → Clock/Mask replaced; 4 losses → only Clock replaced; 5+ losses → full table unlocked.

### 4.2 Power-Up Table

| Item | Normal Effect | Juiced-Up (10 Wumpa) Effect |
|---|---|---|
| **Tracking Missile** | Homes in on the nearest opponent ahead. Can be held as single or triple. | Faster tracking, longer spin-out. |
| **Bowling Bomb** | Rolls forward along the ground; can be aimed or dropped behind. Single or triple. | Larger blast radius, longer tumble. |
| **TNT Crate** | Dropped behind the kart. Lands on the victim's head and detonates after ~3 seconds. Victim can hop repeatedly to shake it off. | Becomes a **Nitro Crate**: explodes instantly on contact, cannot be shaken off. |
| **N. Brio's Beaker** | Green potion bottle. Dropped behind or thrown forward. Spins out anyone who touches it. | Becomes a **Red Beaker**: spins out + creates a rain cloud over the victim that slows them and replaces their held item. |
| **Power Shield** | Green bubble shield. Absorbs one incoming hit. Can be thrown forward as a projectile. Expires on a timer. | Becomes a **Blue Shield**: does NOT expire on a timer. Persists until hit or manually fired. |
| **Turbo** | Instant short speed burst. Stacks with turbo pads. | Longer, stronger burst. |
| **Aku Aku / Uka Uka** | Temporary invincibility + speed boost (3900 speed units). Immune to off-road friction. Running into opponents flattens them. Lost if falling off the track. Replaces track music with mask theme. | Extended duration, greater speed boost. |
| **N. Tropy Clock** | All opponents spin out, then drive at reduced speed for several seconds. Cannot use items during the effect. | Extended slowdown duration. |
| **Warp Orb** | A homing orb that travels the track layout to strike the 1st-place racer. Hits anyone in its path. Can be blocked by Shield or Mask. | Hits **all racers** ahead of the user, not just 1st place. |

### 4.3 Battle-Mode-Only Items

| Item | Effect |
|---|---|
| **Invisibility** | Full kart invisibility. Missiles cannot track. Position arrow disappears from opponents' screens. |
| **Super Engine** | Constant turbo-level speed while accelerating. Reserves are set (not added) to the item's value. Juiced version extends duration. |

---

## 5. Track Reference

### 5.1 All 18 Tracks

| # | Track | Hub | Theme | Notable Hazards | Key Shortcuts |
|---|---|---|---|---|---|
| 1 | **Crash Cove** | N. Sanity Beach | Tropical beach | None | Pool-to-cliff jump |
| 2 | **Roo's Tubes** | N. Sanity Beach | Underwater tunnels | Whale-bone obstacles | Dirt-path cutoff near finish |
| 3 | **Mystery Caves** | N. Sanity Beach | Cave / lava | Turtles (moving platforms), fireballs | Secret sandy track on early bend |
| 4 | **Sewer Speedway** | N. Sanity Beach | Sewer halfpipe | Rolling toxic barrels | Halfpipe hole-in-the-wall (requires boost/mask); right pipe jump |
| 5 | **Coco Park** | The Lost Ruins | Garden / park | Minimal | None |
| 6 | **Tiger Temple** | The Lost Ruins | Jungle ruins | Fire-breathing stone idols | Statue's Teeth shortcut (opened by hitting statue with weapon/mask) |
| 7 | **Papu's Pyramid** | The Lost Ruins | Aztec pyramid | Man-eating plants | Pillar Jump, Ledge Jump (via turbo pad), Sharp Turn Jump |
| 8 | **Dingo Canyon** | The Lost Ruins | Desert canyon | Rolling armadillos, water sections | Two-path junction |
| 9 | **Blizzard Bluff** | Glacier Park | Snow / ice | Rolling boulder, icy surfaces | "Impossible River Jump" (snow ramp to house tunnel; requires boost) |
| 10 | **Dragon Mines** | Glacier Park | Underground mine | Mine carts (crushing hazard), wooden corkscrew | Mine cart rail hop; stone track hop |
| 11 | **Polar Pass** | Glacier Park | Arctic / ice | Seals crossing path, ice surfaces | Figure-eight left path; wall jump over cave wall |
| 12 | **Tiny Arena** | Glacier Park | Coliseum / mud | Mud terrain (slows karts) | None (longest track in game) |
| 13 | **N. Gin Labs** | Citadel City | Laboratory | Rolling toxic barrels, ramps | None |
| 14 | **Cortex Castle** | Citadel City | Gothic castle | Spiders, 90° turns | Stair jump; ramp jump into blue room |
| 15 | **Hot Air Skyway** | Citadel City | Sky / blimp | Gaps / falls, tight turns | Secret ramp jump to short ramp on right |
| 16 | **Oxide Station** | Citadel City | Space station | Long jumps, sharp turns, low-gravity segment | None |
| 17 | **Slide Coliseum** | Gemstone Valley | Arena | None | Multiple corner-cutting lines |
| 18 | **Turbo Track** | Gemstone Valley | Speed circuit | None | Enhanced turbo pads throughout |

### 5.2 Relic Race Times

All times in M:SS.cc format.

| Track | Sapphire | Gold | Platinum |
|---|---|---|---|
| Crash Cove | 1:17.00 | 1:05.00 | 0:52.00 |
| Roo's Tubes | 1:15.00 | 1:05.00 | 0:55.00 |
| Mystery Caves | 1:55.00 | 1:44.00 | 1:32.00 |
| Sewer Speedway | 1:33.00 | 1:05.00 | 0:37.00 |
| Coco Park | 1:35.00 | 1:12.00 | 0:49.00 |
| Tiger Temple | 1:20.00 | 1:02.00 | 0:43.00 |
| Papu's Pyramid | 1:34.00 | 1:09.00 | 0:42.00 |
| Dingo Canyon | 1:25.00 | 1:09.00 | 0:53.00 |
| Blizzard Bluff | 1:30.00 | 1:08.00 | 0:45.00 |
| Dragon Mines | 1:28.00 | 1:11.00 | 0:54.00 |
| Polar Pass | 3:00.00 | 2:33.00 | 2:05.00 |
| Tiny Arena | 3:45.00 | 3:22.00 | 2:58.00 |
| N. Gin Labs | 2:15.00 | 1:34.00 | 0:53.00 |
| Cortex Castle | 2:35.00 | 2:04.00 | 1:32.00 |
| Hot Air Skyway | 3:05.00 | 2:34.00 | 2:02.00 |
| Oxide Station | 3:17.00 | 2:56.00 | 2:34.00 |
| Slide Coliseum | 1:55.00 | 1:45.00 | 1:40.00 |
| Turbo Track | 1:45.00 | 1:32.00 | 1:19.00 |

---

## 6. Game Modes

### 6.1 Adventure Mode

Single-player campaign. See §3 for full structure.

### 6.2 Arcade Mode

Available from the main menu, supports 1–4 players (Multitap required for 3–4).

**Single Race:** Choose any track, number of laps (3/5/7), difficulty (Easy/Medium/Hard), and character. Race against CPU opponents.

**Cup Race:** Four-race series with cumulative points.

| Position | Points |
|---|---|
| 1st | 9 |
| 2nd | 6 |
| 3rd | 3 |
| 4th | 1 |
| 5th–8th | 0 |

**Arcade Cups:**

| Cup | Track 1 | Track 2 | Track 3 | Track 4 |
|---|---|---|---|---|
| Wumpa Cup | Crash Cove | Coco Park | Tiger Temple | Blizzard Bluff |
| Crystal Cup | Roo's Tubes | Sewer Speedway | Dingo Canyon | Dragon Mines |
| Nitro Cup | Mystery Caves | Papu's Pyramid | Tiny Arena | Cortex Castle |
| Crash Cup | Polar Pass | N. Gin Labs | Hot Air Skyway | Slide Coliseum |

Winning all 4 cups on a given difficulty unlocks a battle arena:
- **Easy:** Parking Lot
- **Medium:** The North Bowl
- **Hard:** Lab Basement

### 6.3 Versus Mode

1–4 player races (Multitap for 3–4). Same as Arcade but strictly player vs. player — no CPU opponents.

### 6.4 Time Trial

Solo mode. Race any unlocked track against the clock with no opponents and no items. A **ghost** of the player's best run is recorded and can be raced against.

**N. Tropy Ghosts:** After completing a track in Time Trial, an N. Tropy ghost time becomes available. Beat all N. Tropy ghosts across all 18 tracks to unlock **N. Tropy** as a playable character.

**Oxide Ghosts:** After beating N. Tropy's ghost on a track, Nitros Oxide's ghost appears. Beat all Oxide ghosts to unlock **Nitros Oxide** (see §12 for platform uncertainty).

### 6.5 Battle Mode

Multiplayer arena combat for 2–4 players (no AI opponents). Played in enclosed battle arenas.

**Battle Arenas (7 total):**

| Arena | Unlock Condition |
|---|---|
| Skull Rock | Available by default |
| Rampage Ruins | Available by default |
| Rocky Road | Available by default |
| Nitro Court | Available by default |
| Parking Lot | Win all 4 Arcade Cups on Easy |
| The North Bowl | Win all 4 Arcade Cups on Medium |
| Lab Basement | Win all 4 Arcade Cups on Hard |

**Battle Types:**

| Type | Rule | Settings |
|---|---|---|
| Point Limit | First to reach the point target wins. | 5, 10, or 15 points |
| Time Limit | Most points when time expires wins. | 3, 6, or 9 minutes |
| Life Limit | Limited lives; last standing wins. | 3, 6, or 9 lives; timer: 3 min, 6 min, or Forever |

**Team Configurations:** 2v1, 2v2, 3v1, 1v1v2. Weapon types can be customized before the match.

---

## 7. Boss Races

All boss races are one-on-one (player vs. boss, no other racers). The boss has unlimited access to specific weapons and does not use ? Crates. Defeating the boss earns a **Boss Key**.

| Boss | Hub | Track | Weapons Used | Special Behavior |
|---|---|---|---|---|
| **Ripper Roo** | N. Sanity Beach | Roo's Tubes | TNT crates | Continuously drops TNT behind him. Easiest boss. |
| **Papu Papu** | The Lost Ruins | Papu's Pyramid | Green + Red Beakers | Drops both normal and red beakers. Red beakers apply prolonged speed reduction. |
| **Komodo Joe** | Glacier Park | Dragon Mines | TNT + Nitro crates | Drops both TNT (delayed) and Nitro (instant). Stops attacking during the corkscrew section. |
| **Pinstripe** | Citadel City | Hot Air Skyway | Bowling Bombs (backward) | Rolls bowling bombs backward. Dangerous on straightaways. |
| **Nitros Oxide** | Gemstone Valley | Oxide Station | All weapons (beakers, TNT, Nitro, backward bowling bombs) | Starts before the green light. Drives a hovercraft (does not spin out when hit). Uses progressively more weapon types as race continues. |

**Boss difficulty scaling (from decompiled `BOTS_Adv_AdjustDifficulty`):** Base difficulty is `bossID × 5`, where bossID ranges 0–4. Each loss reduces the difficulty modifier, making the boss easier. With the Adventure difficulty cheat active, formula changes to `bossID × 7`.

---

## 8. Gem Cups

Gem Cups are special 4-race cup series accessed from **Gemstone Valley**. Each cup is unlocked by collecting 4 CTR Tokens of the matching color. Winning a Gem Cup awards the corresponding Gem and unlocks a secret character.

### 8.1 CTR Token Distribution

| Color | Track 1 | Track 2 | Track 3 | Track 4 |
|---|---|---|---|---|
| Red | Crash Cove | Mystery Caves | Papu's Pyramid | Blizzard Bluff |
| Green | Roo's Tubes | Coco Park | Polar Pass | Cortex Castle |
| Blue | Sewer Speedway | Tiger Temple | Dragon Mines | N. Gin Labs |
| Yellow | Dingo Canyon | Tiny Arena | Hot Air Skyway | Oxide Station |
| Purple | Skull Rock | Rampage Ruins | Rocky Road | Nitro Court |

### 8.2 Gem Cup Compositions & Rewards

| Gem Cup | Tracks | Gem | Character Unlocked |
|---|---|---|---|
| Red | Crash Cove, Mystery Caves, Papu's Pyramid, Blizzard Bluff | Red Gem | Ripper Roo |
| Green | Roo's Tubes, Coco Park, Polar Pass, Cortex Castle | Green Gem | Papu Papu |
| Blue | Sewer Speedway, Tiger Temple, Dragon Mines, N. Gin Labs | Blue Gem | Komodo Joe |
| Yellow | Dingo Canyon, Tiny Arena, Hot Air Skyway, Oxide Station | Yellow Gem | Pinstripe Potoroo |
| Purple | Roo's Tubes, Papu's Pyramid, Dragon Mines, Hot Air Skyway | Purple Gem | Fake Crash |

The Purple Gem Cup features the four boss characters as opponents (without their infinite item advantages). Scoring uses the same system as Arcade Cups (§6.2).

---

## 9. AI & Difficulty

### 9.1 Difficulty System (from decompiled source)

The AI uses a **14-parameter difficulty table** per track that interpolates between two data sets (`params1` and `params2`). The interpolation factor is a `currDifficulty` value.

**Difficulty scaling by mode:**

| Mode | Formula | Notes |
|---|---|---|
| Arcade Easy/Medium/Hard | Progressive fixed values | Hidden "Super Hard" cheat sets to 0x140 (320) |
| Adventure (races) | `maxDifficulty = (trophyCount × 35) / 4` | Each loss (up to 10 per track) reduces difficulty |
| Adventure (bosses) | `bossID × 5` | Each loss reduces difficulty. Cheat mode: `bossID × 7` |
| Adventure (cups) | `trackIndex × 5` | Purple Gem Cup uses steeper parameters |
| Cup modes (all) | Base + 0x50 bonus | AI is slightly harder in cups |

### 9.2 AI Navigation

Bots follow pre-recorded **NavFrame** paths baked into each track. Each NavFrame stores: position, rotation, flags (turbo, drift left/right, jump, skid), terrain type, and pathChange opcodes. **Three parallel paths** exist per track; AI is assigned to paths via RNG.

### 9.3 AI Item Usage

Bots have their own item system (`BotData.item`), a `weaponCooldown` timer, and fire level tracking (`ai_fireLevel`). AI bots draw from the same position-based item tables as players and use a simulated turbo meter (`ai_turboMeter`).

### 9.4 Rubber-Banding

CTR does **not** use classic teleportation rubber-banding. Instead, it is a continuous interpolation of AI stats between the two parameter sets, getting harder as the player collects more trophies and easier each time the player loses. The primary catch-up mechanic for all players (human and AI) is position-weighted item distribution (§4.1).

---

## 10. UI & HUD

### 10.1 Race HUD (from decompiled UI_44_RenderFrame_Racing)

| Element | Position | Description |
|---|---|---|
| Race Clock | Top-left | Total elapsed time and current lap time. |
| Lap Counter | Top-left | "Lap X/3" |
| Position | Top-left | Current race position with suffix ("1st", "2nd", etc.) |
| Item Window | Top-center | Currently held power-up icon. Shining background when Juiced Up. |
| Item Roulette | Top-center | Animated shuffle when picking up a ? Crate. |
| Wumpa Counter | Top area | 0–10 count with animated Wumpa icon. Golden background at 10. |
| Turbo Boost Meter | Bottom-right | Appears only during Power Slides. Fills green → red. |
| Hang-Time Meter | Center | Appears during airborne sections. Indicates landing boost magnitude. |
| Speedometer / Minimap | Toggled (Triangle) | Speedometer needle gauge or top-down track map with racer position dots. In 3-player, map is always shown in the 4th quadrant. |
| Ranked Driver Icons | Right side | All 8 racers in order of position. Hidden in Time Trial, Relic Race, and Battle. |
| Wrong Way Warning | Center | Flashes "WRONG WAY!" when driving backward for too long (`distanceDrivenBackwards > 0x1F5`). |
| Race End | Center | "FINISHED!" if not last human player; "LOSER!" if last. |

### 10.2 Mode-Specific HUD Changes

- **Battle Mode:** Replaces lap counter with score display; shows point deltas with animation; countdown clock for timed battles.
- **Relic Race:** Shows time crate counter instead of Wumpa counter.
- **3+ Player:** Uses large rank icons instead of text; reduced HUD elements per viewport.

---

## 11. Engine & Presentation Systems

### 11.1 Camera (from decompiled namespace_Camera)

Camera modes cycled with L2:
- **Mode 0 (Near):** Default behind-car view. Uses `NearCam4x3` zoom data (1P) or `NearCam8x3` (2P split-screen). Camera distance scales with kart speed — zooms out at higher speeds.
- **Mode 1 (Far):** Wider zoom, same follow logic.
- **Mode 0xF (First Person):** Camera at driver position with offset.

**Rear View:** R2 sets flag `0x10000` on CameraDC.flags, flipping the camera to look behind.

**Hit Recovery:** When a player is hit, the camera stores its state and performs an 8-frame lerp back to the kart after the blast animation.

**Split-Screen:** 2P uses wider aspect zoom data. 3P/4P modes use reduced-polygon track meshes for performance.

### 11.2 Save System (from decompiled namespace_Memcard)

- Uses **1 block** on PS1 memory card.
- **4 Adventure Progress slots** (player name up to 18 chars, character ID, hub level, per-level loss counters).
- **GameProgress struct** (0x1494 bytes): tracks unlocked, trophies, relics, CTR tokens, gems, keys, ghost times, all unlockable flags.
- **GameOptions**: SFX/Music/Voice volume, audio mode (mono/stereo).
- Save points are green-screened terminals in the Adventure hub overworld, one per hub area.

### 11.3 Audio System

- Track music is replaced by the mask theme when Aku Aku / Uka Uka is active.
- Item roulette plays SFX 0x5d during the shuffle animation.
- Juiced Up activation plays SFX 0x41.

### 11.4 Multiplayer Technical

- Up to **4 players** with PlayStation Multitap; 2 without.
- Adventure Mode and Time Trial are single-player only.
- Lower-polygon track meshes are used in split-screen to maintain framerate.
- Kart tires are rendered as 2D camera-facing sprites (billboard optimization).
- Oxide Station is unavailable in multiplayer due to PS1 memory constraints.

### 11.5 Version Differences

| Difference | NTSC-U | PAL | NTSC-J |
|---|---|---|---|
| Jump timing | 0.29–0.30 seconds | 0.23 seconds | 0.29–0.30 seconds |
| Penta Penguin class | Turn (same as Polar/Pura) | Max (best stats in game) | Turn |
| Turbo chain counter | Hidden (cheat only) | Hidden (cheat only) | Tracks consecutive turbo high score per race |

---

## 12. Open Questions / Unverified

| Topic | Issue |
|---|---|
| **Nitros Oxide unlock (NTSC-U)** | Some sources state Oxide is only unlockable via GameShark in NTSC-U, while others claim beating all Oxide Time Trial ghosts works. PAL may differ. Needs ROM verification. |
| **Penta Penguin stats across all regions** | Confirmed Turn class in NTSC-U and Max class in PAL. NTSC-J assignment unverified beyond community claims. |
| **Exact reserve depletion rate per frame** | The reserve system's frame-by-frame drain constant is not consistently documented. The decompilation confirms the struct layout but the exact per-frame decrement value needs further extraction from CTR-ModSDK. |
| **U-Turn reserve behavior** | §1.11 states U-Turns preserve reserves; some speedrun sources indicate they drain reserves at a reduced rate rather than preserving them entirely. |
| **USF visual distinction on PS1** | Many sources describe USF as "white/blue" or "bright red" exhaust, but these descriptions may conflate the 2019 Nitro-Fueled remake (which has distinct color-coded fire) with the original PS1 (where fire level differences are subtler). The PS1 exhaust flame size increases with fire level, but color coding is less pronounced than commonly described. Needs frame-by-frame PS1 capture verification. |
| **Super Turbo Pad locations** | Confirmed on Hub 4 tracks (Cortex Castle, N. Gin Labs, Hot Air Skyway, Oxide Station). Whether any exist outside Hub 4 is unverified. |
| **Oxide Station multiplayer availability** | Spec states unavailable in multiplayer due to PS1 memory. Some sources claim it is available in 2-player split-screen but excluded from 3–4 player. Needs verification. |
| **Start Boost exact frame window** | The precise input window for the Start Boost (§1.7) is described inconsistently across sources — some say a single timed press, others say multiple presses during the countdown. The decompiled startup logic should clarify this. |
| **Relic Race time thresholds** | The times in §5.2 are sourced from community guides. Some values (e.g., Sewer Speedway Platinum at 0:37.00) seem very aggressive and may warrant verification against the decompiled relic threshold data. |
| **Cheat code sequences** | The gameplay cheat codes in §2.5 are compiled from multiple community sources with minor variations. Some codes may be specific to NTSC-U and not work on PAL/NTSC-J. |

---

## 13. References

### Decompilation / Source Code
- [CTR-ModSDK Decompilation (GitHub)](https://github.com/CTR-tools/CTR-ModSDK) — ~90% decompiled C source of original PS1 game. Primary source for MetaPhys values, AI parameters, item tables, HUD functions, camera, save system.

### Technical / Speedrun Community
- [TASVideos — Game Resources / PSX / Crash Team Racing](https://tasvideos.org/GameResources/PSX/CrashTeamRacing) — fire levels, reserve mechanics, advanced techniques, Wumpa speed values.
- [Speedrun.com — Crash Team Racing](https://www.speedrun.com/ctr) — character classification forums, Sacred Fire mechanics.
- [The Cutting Room Floor — Crash Team Racing](https://tcrf.net/Crash_Team_Racing) — unused content, prototype differences.

### Wikis & Encyclopedias
- [Bandipedia — CTR: Crash Team Racing (Fandom)](https://crashbandicoot.fandom.com/wiki/CTR:_Crash_Team_Racing)
- [Crash Team Racing — Wikipedia](https://en.wikipedia.org/wiki/Crash_Team_Racing)

### Strategy Guides & Walkthroughs
- [CTR FAQ/Walkthrough — GameFAQs (by TSC)](https://gamefaqs.gamespot.com/ps/196989-ctr-crash-team-racing/faqs/10932)
- [Crash Mania — CTR Overview](https://www.crashmania.net/en/games/crash-team-racing/overview/)
- [GamesAtlas — CTR Tracks/Weapons/Cups](https://www.gamesatlas.com/crash-team-racing/)
