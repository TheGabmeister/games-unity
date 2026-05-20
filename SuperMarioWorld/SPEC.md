# Super Mario World — Gameplay Systems Spec

Super Mario World, SNES, 1990 (NTSC-J) / 1991 (NTSC-U, PAL). Platform: SNES original.

---

## 1. Core Gameplay Systems

### 1.1 Primary Loop

Side-scrolling 2D platformer. Mario traverses linear and branching levels across the Dinosaur Land overworld, defeating enemies, collecting power-ups, and reaching the Giant Gate at each level's end. The meta-loop is: select a level on the overworld map → complete it (normal or secret exit) → unlock the next path → repeat until Bowser is defeated.

### 1.2 Movement Physics

The SNES runs at 60 fps (NTSC). The game uses a subpixel coordinate system: 1 pixel = 16 subpixels. Speeds below are in subpixels per frame.

**Horizontal Speed**

| State | Max Speed (subpx/f) | ~Pixels/frame |
|-------|---------------------|---------------|
| Walking | 21 | 1.3 |
| Running (hold Y/X) | 37 | 2.3 |
| Sprinting (P-meter full) | 49 | 3.1 |
| Swimming (no item) | 15–17 (oscillates) | ~1.0 |
| Swimming (carrying item + direction) | 31–33 (oscillates) | ~2.0 |
| Cape flight | 47–51 (oscillates) | ~3.1 |

Sprint speed oscillates through 5 values each frame: 48, 47, 48, 47, 49. Running speed oscillates similarly: 36, 35, 36, 35, 37.

**Acceleration:** From standstill, speed ramps through the sequence 0, 1, 3, 4, 6, 7, 9… Approximately 80 frames (~1.33 s) to fill the P-meter from standstill; ~90 frames (~1.5 s) to reach max sprint speed.

**P-Meter (Sprint Gauge):** Internal value, max 112. Fills while holding Y/X + direction on the ground. When it reaches 112, Mario enters sprint state (arms spread in run animation). There is no on-screen P-meter gauge (unlike SMB3); the only visual indicator is the arm-spread animation. The P-meter can be recharged mid-air by alternating direction release (6 frames) and direction + Y press (5 frames).

**Terminal Velocity:** Y-speed capped at 67–70 subpx/f depending on context.

**Coyote Time:** None. The original SNES version has no grace frames for jumping after leaving a ledge.

**Slope Sliding:** Press Down while on a slope to slide downhill. Mario gains speed as he descends and defeats most enemies on contact while actively sliding. The slide ends when Mario reaches flat ground or a wall.

### 1.3 Jump Physics

Jump initial Y-speed varies with current X-speed via a lookup table (indexed by X-speed ÷ 8). Holding the jump button extends jump height (variable-height jump); releasing early produces a short hop.

| Jump Type | Height (standing/walking) | Height (max sprint) |
|-----------|---------------------------|---------------------|
| Normal Jump (B) | ~5 tiles | ~6 tiles |
| Spin Jump (A) | ~4 tiles | ~5 tiles |

**Spin Jump:** Lower and floatier than normal jump. Destroys Rotating Blocks from above (Super Mario or larger). Allows safe bounce off normally hazardous enemies (Spinies, Piranha Plants, Lava Bubbles, Thwomps, Thwimps, Grinders, Munchers). Cannot safely bounce on: Circling Boo Buddies, Boo Buddy Snakes, Reznor, Bowser. As Fire Mario, spin jumping releases 2 fireballs (one left, one right).

**Enemy Bounce:** Stomping an enemy while holding the jump button gives a higher bounce than without.

**Yoshi Dismount Jump:** Pressing A while riding Yoshi mid-air ejects Mario upward with a jump significantly higher than normal. Yoshi continues on its prior trajectory (usually falls into a pit).

### 1.4 Swimming

Mario sinks naturally; pressing the jump button strokes upward. No air meter — Mario can stay underwater indefinitely. Forward momentum is preserved. See §1.2 speed table for swimming values.

### 1.5 Climbing

Mario grabs climbable surfaces (vines, fences, ropes, nets) automatically on contact with directional input. Move with D-pad, jump off with B. On chain-link fences: Y/X punches (defeats Koopas on the other side); punching a fence gate flips Mario to the opposite side. Pressing Up + B every other frame climbs at an accelerated rate.

### 1.6 Power-Up System

**Mario States:**

| State | Source | Properties |
|-------|--------|------------|
| Small Mario | Default / damage | Short hitbox. One hit = death. |
| Super Mario | Super Mushroom | Tall hitbox. Can break Rotating Blocks from below. One hit → Small Mario. |
| Fire Mario | Fire Flower | Throws fireballs (Y/X). Max 2 on screen. Enemies killed by fireballs turn into coins. One hit → Small Mario. |
| Cape Mario | Cape Feather | Flight, glide, spin attack, dive-bomb (see §1.7). One hit → Small Mario. |
| Balloon Mario | P-Balloon | Inflates Mario for free directional floating. Temporary (~4 s). Cannot ride Yoshi or grab items. Wears off with a pulsing warning; Mario drops straight down. Collecting any other power-up cancels it immediately. |

**Damage rules (SNES original):** Fire/Cape Mario hit → reverts directly to **Small Mario** (skips Super). Super Mario hit → Small Mario. Small Mario hit → lose a life. The GBA remake changed this to stepwise regression.

**Power-up acquisition:** Small Mario can skip Super and go directly to Fire or Cape from a single pickup. When a powered-up Mario collects a different power-up, the old power-up is displaced into the Item Reserve (see §6.1).

**Post-damage invincibility:** ~2 seconds of blinking i-frames after taking damage.

**Super Star:** Temporary invincibility (~12 seconds, matching the music duration). Mario defeats enemies on contact, using the consecutive kill chain (§1.9). Collecting another star while active extends the duration. Can still die from pits, lava, crushing, and time-out. Palette cycles rapidly during the effect.

### 1.7 Cape Mechanics

**Flight initiation:**
1. Build to full sprint (P-meter = 112) while holding Y/X.
2. Jump (B). Internal takeoff timer set to 80, decrements 1/frame.
3. During countdown Mario ascends. When timer hits 0, transitions to sustained flight.

**Flight control:**
- Press forward (Mario's facing direction): descend and accelerate.
- Press backward: ascend and decelerate. Greater prior descent yields higher ascent (momentum-based).
- Repeatedly pressing backward at the correct rhythm ("cape pumping") allows indefinite flight.

**Dive-bomb:** During flight, press forward to dive at high speed. On ground impact, triggers a POW-style earthquake that defeats all on-screen grounded enemies instantly.

**Cape spin attack:** On the ground, press Y/X to spin the cape in a circle around Mario, defeating enemies on contact. Touching an enemy while airborne triggers an automatic cape spin and fall rather than taking damage.

**Slow glide:** While falling (not in flight), hold Y/X to float down slowly.

### 1.8 Yoshi

**Mounting:** Find a Yoshi Egg in a ? Block; Yoshi hatches and Mario mounts. Yoshi cannot enter Ghost Houses, Castles, or Fortresses — he waits outside.

**Tongue:** Y/X extends Yoshi's tongue forward. Grabs enemies and items at range. Most enemies are swallowed immediately. Shells are held in mouth. Mouth-hold timer starts at 255, decrements every 4 frames; at 0 the shell is swallowed. Spitting resets the timer.

**Yoshi Colors and Shell Interactions:**

| | Green Shell | Red Shell | Blue Shell | Yellow Shell |
|---|---|---|---|---|
| **Green Yoshi** | Spit as projectile | Spit 3 fireballs | Grow wings, fly | Stomp sand clouds |
| **Red Yoshi** | Spit 3 fireballs | Spit 3 fireballs | Spit 3 fireballs + fly | Spit 3 fireballs + stomp sand clouds |
| **Blue Yoshi** | Grow wings, fly | Fly + spit 3 fireballs | Grow wings, fly | Fly + stomp sand clouds |
| **Yellow Yoshi** | Stomp sand clouds | Stomp sand clouds + spit 3 fireballs | Stomp sand clouds + fly | Stomp sand clouds |

Colored Yoshis receive their innate ability from **any** shell, **plus** the shell's native ability. A flashing/multicolor shell grants all three abilities (fireballs + flight + sand clouds) to any Yoshi.

**Baby Yoshi:** Found only in Star World. Red, Blue, and Yellow varieties. Feed 5 enemies/objects (shells, coins, Grab Blocks count) to grow into adult. Feeding a power-up causes instant growth. Baby Yoshis eat by walking into things (no tongue). Adult color matches the baby's color.

**Berry system:**
- Red Berries: eat 10 → Yoshi lays an egg containing a Super Mushroom. Each berry = 1 coin.
- Pink Berries: eat 2 → spawns a Yoshi Cloud that drops 10 coins, then a 1-UP if all coins are collected.
- Green Berries: adds 20 units to the level timer. Found only in Funky (9 in that level).

**Flutter:** Yoshi has a brief flutter when jumping, extending hang time slightly.

**Duplicate Yoshi:** If Mario already has Yoshi and finds another Yoshi Egg, it awards a 1-UP instead.

### 1.9 Scoring and Lives

**Consecutive stomp chain (without touching ground):**

| Stomp # | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8+ |
|---------|---|---|---|---|---|---|---|---|
| Reward | 200 | 400 | 800 | 1000 | 2000 | 4000 | 8000 | 1-UP each |

This chain applies to: consecutive stomps, shell-kicked enemy chains, star-invincibility kills, and slope-sliding kills. Touching the ground resets the chain.

**Coin points:** Coins from ? Blocks award 10 points each. Free-standing coins add to the coin counter but do not award score points (SNES original). Enemies turned to coins by fireballs award 200 points when collected.

**Time bonus:** At level completion, remaining time converts to score at 50 points per timer unit.

**Goal tape scoring:** Bonus Stars awarded by height (1–50). Hitting the top awards 50 stars and 3 extra 1-UPs. If the last two digits of the timer are a multiple of 11 (including 00) and match the tens digit of Bonus Stars received, an additional 1-UP is awarded.

**Extra life sources:**
- 100 coins (counter resets)
- 1-UP Mushrooms
- 8th+ consecutive enemy in a stomp chain
- All 5 Dragon Coins in a level
- 3-UP Moon (3 lives; 7 exist in the game)
- Goal tape maximum height (3 lives)
- 100 accumulated Bonus Stars → bonus game
- Duplicate Yoshi Egg
- Silver coin chain from Gray P-Switch (8th coin = 1-UP, 9th = 2-UP, 10th+ = 3-UP each)

**Instant death conditions:** falling into a pit or offscreen, touching lava, being crushed between moving objects, and timer reaching zero. These kill regardless of power-up state.

**Starting lives:** 5. **Max lives:** 99 (hard cap). **Game Over:** at 0 lives; Continue restarts from last save with 5 lives, score and coins reset.

---

## 2. Controls & Input

SNES controller. No remapping; no accessibility options.

| Button | Action |
|--------|--------|
| D-Pad Left/Right | Move; menu navigation; map movement |
| D-Pad Up | Enter doors; climb up; look up; swim upward |
| D-Pad Down | Crouch; enter pipes; descend on fences/ropes |
| B | Normal jump (variable height) |
| A | Spin jump; dismount Yoshi (mid-air) |
| Y | Run / hold items / Yoshi tongue / fireball / cape spin |
| X | Same as Y (duplicate binding) |
| L | Scroll camera left (disabled in auto-scroll, boss rooms) |
| R | Scroll camera right (same restrictions) |
| Start | Pause; confirm selections |
| Select | Deploy reserve item; return to map (paused, with Start) |

**Context-sensitive:**
- On Yoshi: A = dismount (ground dismount is neutral; air dismount preserves momentum for a high jump).
- Carrying items: Y/X to grab; release to throw; Up + release = throw upward.
- Pipes: Down to enter downward-facing pipes; Up for upward-facing pipes; Left/Right for horizontal pipes.
- Swimming: B/A to stroke upward; release to sink; D-pad for direction.
- Climbing fences: D-pad to move; Y/X to punch; B to jump off.

---

## 3. World Structure

### 3.1 Overworld Layout

Dinosaur Land is a single continuous overworld map (not separated world screens like SMB3). Mario moves along fixed node-based paths between level markers. No free roaming.

**9 Worlds:**

| # | World | Levels | Total Exits | Secret Exits |
|---|-------|--------|-------------|--------------|
| 1 | Yoshi's Island | 6 | 6 | 0 |
| 2 | Donut Plains | 10 | 15 | 5 |
| 3 | Vanilla Dome | 11 | 14 | 3 |
| 4 | Twin Bridges | 6 | 7 | 1 |
| 5 | Forest of Illusion | 9 | 14 | 5 |
| 6 | Chocolate Island | 9 | 11 | 2 |
| 7 | Valley of Bowser | 9 | 11 | 3 |
| 8 | Star World | 5 | 10 | 5 |
| 9 | Special Zone | 8 | 8 | 0 |
| | **Total** | **~73 playable** | **96** | **24** |

Non-playable map nodes: Yoshi's House (start point), Top Secret Area (power-up farming room, §9.2).

The game tracks a 96-exit counter. At 96 exits complete, the title screen displays "★96".

### 3.2 Path Unlocking

- Normal exit → reveals the standard path to the next level.
- Secret exit → reveals a branching path (to a secret level, Switch Palace, or Star Road).
- Level dot colors: **Yellow** = single exit. **Red** = has a secret exit (2 exits).
- Completed levels can be replayed at any time.
- Castles are destroyed after defeating the Koopaling (cutscene). In international versions, L+R on the ruins allows replay.
- Fortresses and Ghost Houses can always be replayed.

### 3.3 Secret Exit Mechanic

Most secret exits use a **Key + Keyhole** system. A gray key is hidden off the main path; carrying it to a keyhole triggers the secret exit (see Key in §6.3). Some secret exits instead use an alternate Giant Gate reached by an alternate route.

### 3.4 Switch Palaces

Four one-time levels, each containing a large colored switch. Pressing it permanently converts all matching Dotted-Line Block outlines into solid ! Blocks across every level in the game.

| Switch | World | Unlocked By | ! Block Contents |
|--------|-------|-------------|-----------------|
| Yellow | Yoshi's Island | Yoshi's Island 1 (normal exit) | Super Mushrooms |
| Green | Donut Plains | Donut Plains 2 (secret exit) | Cape Feathers |
| Red | Vanilla Dome | Vanilla Dome 2 (secret exit) | Solid platforms (no items) |
| Blue | Forest of Illusion | Forest of Illusion 2 (secret exit) | Solid platforms (no items) |

Switch Palaces cannot be replayed after completion.

### 3.5 Star Road

5 Star Road portals on the overworld warp to Star World. Each Star World level's normal exit returns Mario to the corresponding overworld portal (functioning as world-to-world warps). Each secret exit opens the path forward within Star World to the next Star World level.

| Star World Level | Overworld Portal Location | Baby Yoshi Color |
|------------------|---------------------------|-----------------|
| Star World 1 | Donut Plains | Red |
| Star World 2 | Vanilla Dome | Blue |
| Star World 3 | Twin Bridges | Yellow |
| Star World 4 | Forest of Illusion | Red |
| Star World 5 | Valley of Bowser | Yellow |

Each secret exit opens the path to the next Star World level; Star World 5's secret exit opens the path to the Special Zone.

### 3.6 Special Zone

8 extremely difficult levels accessed via Star World 5's secret exit. Level names: Gnarly, Tubular, Way Cool, Awesome, Groovy, Mondo, Outrageous, Funky. Single exit each, no secrets.

**Completion reward:** After clearing Funky, a Star Road warps Mario to Yoshi's House. The entire overworld palette swaps from spring/summer to autumn colors permanently. Cosmetic enemy sprite replacements occur (Koopas wear Mario masks, Piranha Plants get jack-o'-lantern heads, Bullet Bills become bird sprites). Behavior is unchanged.

### 3.7 Ghost Houses

6 Ghost Houses plus the Sunken Ghost Ship. Puzzle-oriented door-navigation design. Boo behavior is the core mechanic: Boos approach when Mario's back is turned, freeze when faced.

### 3.8 Level Gating

The Valley of Bowser is gated behind the Sunken Ghost Ship — completing it causes a Bowser-shaped cave entrance to rise from the water. Within worlds, castle completion is required to advance to the next world. Star Road and Special Zone require finding specific secret exits.

---

## 4. Playable Characters

Two characters, functionally identical in the SNES original:

| Character | Player | Visual Difference |
|-----------|--------|-------------------|
| Mario | Player 1 | Red hat and shirt, blue overalls |
| Luigi | Player 2 | Green hat and shirt, blue overalls |

No stat or ability differences between Mario and Luigi in the SNES version. The GBA remake added a Luigi mode with higher jump and lower traction.

---

## 5. Story & Progression

### 5.1 Structure

Linear world progression (Yoshi's Island → Donut Plains → … → Valley of Bowser) with branching secret paths. Each of the 7 main worlds ends with a Koopaling castle boss. The final boss is Bowser in Valley of Bowser.

### 5.2 Sequence

1. Yoshi's Island — Iggy's Castle
2. Donut Plains — Morton's Castle
3. Vanilla Dome — Lemmy's Castle
4. Twin Bridges — Ludwig's Castle
5. Forest of Illusion — Roy's Castle
6. Chocolate Island — Wendy's Castle
7. Valley of Bowser — Larry's Castle → Bowser's Castle (Front Door / Back Door)

### 5.3 Optional Content

- **Star World:** 5 levels, accessed via Star Road portals found through secret exits (§3.5).
- **Special Zone:** 8 levels, the game's hardest content. Accessed through Star World (§3.6).
- **Top Secret Area:** Hidden power-up room (§9.2).
- **Secret exits:** 24 total, unlocking alternate overworld paths.
- No New Game+ or post-game beyond the cosmetic palette swap from Special Zone completion.

---

## 6. Items & Equipment

### 6.1 Item Reserve Box

Displayed at top-center of the HUD. Holds exactly 1 power-up (Super Mushroom, Fire Flower, or Cape Feather).

- **Auto-fill:** Collecting a power-up while already powered up displaces the old power-up into reserve.
- **Auto-deploy:** When Mario takes damage and becomes Small, the reserve item drops automatically.
- **Manual deploy:** Press Select to release it at any time. It falls from the top of the screen, passes through platforms — must be caught before falling offscreen.
- **Priority rule:** Super Mushrooms always replace whatever is currently stored, regardless of relative value.

### 6.2 Block Types

| Block | Behavior |
|-------|----------|
| ? Block | Hit from below to reveal contents (coins, power-ups, 1-UPs). Power-up depends on Mario's state. Becomes Empty Block after use. |
| Rotating Block (Turn Block) | Hit from below: spins temporarily, allowing passage. Spin Jump from above (Super+): destroys permanently. Max 4 spinning simultaneously. |
| Note Block | Bounces Mario upward on landing. Timed jump press = higher bounce. |
| ON/OFF Switch | Toggles between ON and OFF states. Controls dotted-line blocks in the level (solid in one state, passable in the other). Activated by jump, cape, or shell. |
| Message Block | Hit from below to display tutorial text. |
| ! Block | Solid colored blocks, activated permanently by Switch Palaces (§3.4). Yellow contain Mushrooms, Green contain Feathers, Red and Blue are solid platforms. |

### 6.3 Interactive Items

| Item | Behavior |
|------|----------|
| Blue P-Switch | Turns coins into solid blocks and blocks into coins for ~10 seconds. Can be carried before activation. Dedicated music replaces level BGM during effect (§11.5). |
| Gray P-Switch | Turns most on-screen enemies into Silver Coins. Escalating coin reward chain (see §1.9). |
| P-Balloon | Inflates Mario into Balloon Mario (§1.6). Found in Donut Secret 1, Donut Secret 2, Forest of Illusion 1, and Tubular. |
| Springboard | Portable trampoline. Timed jump press on landing = super-high bounce. Can be carried and placed. |
| Grab Block | Can be picked up and thrown at enemies. Breaks after a few seconds of being held. Yoshi can swallow them. |
| Key | Gray key for secret exits (§3.3). Carry to a keyhole — the keyhole expands and "swallows" Mario. Can be used as a melee weapon while held. Yoshi can hold the key in his mouth and walk to the keyhole. |

### 6.4 Collectibles

| Collectible | Effect |
|-------------|--------|
| Coin | +1 to coin counter. From ? Blocks: 10 points. 100 coins = 1-UP (counter resets). |
| Dragon Coin | Large dragon-emblem coin. 5 per level. Point values: 1000, 2000, 4000, 8000, then 1-UP for the 5th. Each also counts as 1 regular coin. |
| 3-UP Moon | Awards 3 extra lives. 7 exist in the game (Yoshi's Island 1, Donut Plains 4, Vanilla Dome 3, Cheese Bridge Area, Forest Ghost House, Chocolate Island 1, Valley of Bowser 1). Disappears until game reset after collection. |
| 1-UP Mushroom | Green mushroom. Awards 1 extra life. |

---

## 7. Enemies & Opponents

Full enemy catalog in companion doc: [docs/enemies.md](docs/enemies.md).

### 7.1 Enemy Categories

- **Ground:** Galoombas, Koopa Troopas (4 colors), Rex, Chargin' Chuck (6 variants, 3 hits), Monty Mole, Mega Mole (rideable, invincible), Wiggler, Bob-omb, Sumo Bro, and more.
- **Aerial:** Super Koopas (flashing-cape variant drops Cape Feather), Lakitu, Bullet Bill, Banzai Bill.
- **Aquatic:** Cheep Cheep, Rip Van Fish, Porcupuffer, Torpedo Ted, Urchin (invincible).
- **Ghost House:** Boos (approach when unobserved), Big Boo, Block Boos, Eeries, Fishing Boo.
- **Castle/Fortress:** Thwomps, Dry Bones (regenerate), Magikoopa (transforms blocks into enemies), Grinders, Lava Bubbles, Mechakoopas.
- **Pipe:** Piranha Plants, Jumping Fire Piranha Plants, Volcano Lotus.

### 7.2 Koopa Shell Colors

Koopa Troopa color determines behavior when de-shelled and re-shelled:

| Color | Walking Behavior | Re-shelled Behavior |
|-------|-----------------|---------------------|
| Green | Walks off edges | Normal |
| Red | Turns at edges | Normal |
| Yellow | Walks off edges | Kicks the shell at Mario |
| Blue | Walks off edges | Chases Mario at high speed |

### 7.3 Koopaling Boss Fights

Each Koopaling guards a castle at the end of their world. Three fight archetypes:

**Tilting Platform (Iggy, Larry):**
- Arena: platform floating in lava that tilts left/right.
- Attack: fires magic projectiles at Mario.
- Mechanic: stomp or fireball the Koopaling to push them toward the edge; they slide in the tilt direction. Push off the platform to defeat.
- Iggy (Castle 1): basic version.
- Larry (Castle 7): adds 3 Lava Bubbles leaping from the lava.

**Wall Climber (Morton, Roy):**
- Arena: enclosed room.
- Mechanic: Koopaling runs toward Mario, climbs wall, drops from ceiling targeting Mario. 3 stomps to defeat. Ground-pound stun if Mario is grounded when they land.
- Morton (Castle 2): basic version. Speeds up after each hit.
- Roy (Castle 5): walls slowly close in over time, creating a time limit.

**Pipe Decoy (Lemmy, Wendy):**
- Arena: room with pipes.
- Mechanic: Koopaling and 2 decoys pop out of pipes simultaneously. Only the real one can be hit. 3 stomps on the real one to defeat.
- Lemmy (Castle 3): 1 bouncing Lava Bubble.
- Wendy (Castle 6): 2 diagonally-bouncing Lava Bubbles.

**Solo (Ludwig):**
- Castle 4. Spits up to 4 fireballs, then retreats into shell and spins across the floor, then leaps. 3 stomps to defeat. Most mechanically complex Koopaling.

### 7.4 Reznor (Fortress Boss)

Fought at all 4 fortresses (Vanilla, Forest, Chocolate, Valley). Identical encounter each time:
- 4 Reznors stand on 4 rotating platforms mounted on a wheel.
- They spit fireballs at Mario.
- Hit each from below (bump their platform) to knock them off. 1 hit per Reznor.
- After 2 are defeated, the floor collapses from the sides inward — Mario must jump onto a vacated rotating platform.
- Cape can deflect their fireballs.

### 7.5 Big Boo Boss

1 encounter in the game: **Donut Secret House.**
- Arena with a Grab Block floor.
- Big Boo + 2 normal Boos.
- Throw Grab Blocks at Big Boo's face. 3 hits to defeat.
- The normal Boos are the actual threat. Throwing Grab Blocks creates holes in the floor (fall = death).
- Defeating Big Boo opens a path to Star World.

### 7.6 Bowser (Final Boss)

Fought on the roof of Bowser's Castle. Bowser rides the **Koopa Clown Car** (helicopter-faced vehicle).

**Access routes:**
- **Front Door:** 8-room gauntlet with varied enemy encounters, then a dark corridor to the boss.
- **Back Door:** Secret exit from Valley of Bowser 2. Skips directly to the boss.

**Phase 1 — Mechakoopa Drop:**
Bowser flies left and right, drops 2 Mechakoopas. Mario must stomp a Mechakoopa to stun it, pick it up, and throw it upward to hit Bowser. **2 hits** to advance.

**Transition 1 — Fire Rain:**
Bowser flies offscreen. Flames rain from above. Princess Toadstool throws down a Super Mushroom.

**Phase 2 — Big Steelies:**
Bowser hovers above Mario. Clown Car flips upside down, drops Big Steelies (large iron balls) that roll toward Mario. Then drops 2 Mechakoopas. **2 hits** to advance.

**Transition 2 — Fire Rain + Mushroom:**
Same as Transition 1.

**Phase 3 — Ground Pound:**
Clown Car gets an angry face. Bowser bounces on the ground trying to crush Mario, intermittently dropping Mechakoopas. **2 hits** to defeat.

**Total: 6 Mechakoopa hits** across 3 phases.

---

## 8. Economy

No persistent economy. Coins serve only as a counter toward extra lives (100 coins = 1-UP) and as point modifiers from ? Blocks (10 points each). No shops, no currency, no trading.

---

## 9. Minigames & Side Systems

### 9.1 Bonus Star Game

Triggered when accumulated Bonus Stars (from goal tapes) reach 100. Stars carry across levels; counter resets to (total − 100) after the game.

- 9-block grid: 8 outer blocks cycle around 1 fixed center block.
- Center has a fixed item (Mushroom, Fire Flower, or Star).
- Outer blocks cycle through items: Star 3/8, Mushroom 3/8, Fire Flower 2/8.
- Hit blocks from below to stop them. Match 3 in a row (horizontal, vertical, diagonal) = 1-UP.
- Maximum: 8 extra lives per game. 7 is mathematically impossible due to grid symmetry.

### 9.2 Top Secret Area

Hidden room accessible from the Donut Plains overworld. Contains 5 ? Blocks: 2 Cape Feathers, 2 Fire Flowers, 1 Yoshi Egg (or 1-UP if Yoshi already present). Resets every time it is entered. Primary farming location for power-ups.

### 9.3 Two-Player Mode

Alternating turns, not simultaneous co-op. Player 1 = Mario, Player 2 = Luigi.

- Turns swap when the active player completes a level or loses a life.
- Each player has independent lives, coins, score, power-up state, and reserve item.
- Both share the same overworld map state (opened paths, completed levels).
- If one player activates a Midway Gate, the other player also starts there if entering the same level (shared checkpoints).
- L/R on the map screen allows life transfer between players.
- If one player reaches 0 lives, a life transfer prompt appears before Game Over.

---

## 10. UI & HUD

### 10.1 HUD Layout

The status bar spans the full screen width at the top:

| Left | Center | Right |
|------|--------|-------|
| Player name ("MARIO" / "LUIGI") | Item Reserve Box | Score (7 digits) |
| Lives counter ("×5") | | Time ("TIME 300") |
| Coin counter (coin icon + count) | | |

Dragon Coins are **not** tracked on the HUD in the SNES version. The GBA version added per-level tracking via a map menu.

### 10.2 HUD States

- Standard gameplay: full HUD always visible.
- Boss intros: HUD disappears briefly.
- Overworld map: different layout showing world name, player position, and level names.
- Pause screen: "PAUSE" text overlay; game freezes.

### 10.3 Indicators

- Power-up state: visually reflected by Mario's sprite (Small, Super, Fire, Cape).
- P-meter full: Mario's arms spread in running animation (no gauge).
- Star invincibility: rapid palette cycling.
- Damage i-frames: Mario blinks.
- Key held: visible in Mario's hands.
- Item Reserve: icon visible in HUD box (Mushroom, Flower, or Feather sprite).

---

## 11. Engine & Presentation Systems

### 11.1 Save System

**Save opportunities (SNES):** after completing Ghost Houses, Fortresses, Castles, or Switch Palaces. A prompt offers "SAVE AND CONTINUE" or "CONTINUE WITHOUT SAVING." Replaying a previously completed Ghost House/Fortress/Castle re-triggers the save prompt.

3 save file slots. Regular levels do not offer saves. The GBA version added save-anywhere from the map screen.

### 11.2 Checkpoint System

**Midway Gate:** mid-level checkpoint (blue-and-white striped posts with tape). One per level maximum. Passing through it activates the checkpoint and powers up Small Mario to Super. On death, respawn at the last activated gate. Absent from: auto-scrolling levels, underwater levels (SNES), Star World, Special Zone (SNES).

### 11.3 Camera

**Horizontal scrolling:**
- Mario is positioned slightly off-center in the direction of travel.
- When Mario reverses direction, the camera holds still until Mario reaches ~65% screen width, then makes a fast linear transition to re-center in the new direction.
- L/R buttons nudge the camera left/right slightly (disabled in auto-scroll and boss rooms).

**Vertical scrolling:** Two level-designer-selectable modes:
1. **Scroll at will:** camera centers on Mario vertically (within bounds).
2. **No vertical scroll unless triggered:** camera stays at fixed vertical points unless Mario is climbing, flying, swimming, or jumping at full speed. During a normal jump, the camera does **not** follow Mario upward as long as he hasn't fallen below his launch height — only adjusts when he lands at a different elevation.

**Auto-scroll levels:** some athletic, underground, and castle levels scroll automatically. L/R disabled.

### 11.4 Timer

1 timer unit = 41 frames (NTSC) / 35 frames (PAL). Standard values: 200 (short levels, Switch Palaces), 300 (most levels), 400 (Ghost Houses, some castles). No timer during the Bowser fight.

At 99 remaining: a 10-note warning jingle plays over the music, then the level music permanently speeds up. Time-out = lose a life.

### 11.5 Audio System

**Music architecture:** Koji Kondo composed a single core melodic theme arranged differently for each area type (overworld, athletic, underground, underwater, ghost house, castle). The melody appears in different keys, tempos, and instrumentation.

**Dynamic layers:**
- **Yoshi bongos:** mounting Yoshi dynamically adds a bongo percussion track to the current music. Applies to overworld, athletic, underground, and underwater themes. Ghost House and Castle themes are unaffected.

**State-driven music changes:**
- **Star invincibility:** replaces level music with the invincibility theme for the star's duration.
- **P-Switch:** replaces level music with a dedicated P-Switch theme for the effect's duration (~10 s).
- **Timer warning (99 remaining):** jingle plays over music, then music permanently speeds up.
- **Boss encounters:** dedicated boss BGM for fortresses/castles; Bowser has a separate Phase 2 theme.

**Map screen music:** each world region has its own theme. Special Zone map plays its standard theme for ~2 minutes, then transitions to an arrangement of the SMB1 ground theme.

### 11.6 Sprite Limits

Maximum 12 simultaneous sprites on screen (10 in primary slots, 2 in secondary slots). Excess sprites are not rendered. This is a hardware constraint affecting level design and enemy placement.

---

## 12. Open Questions / Unverified

- **Exact Star invincibility duration:** widely estimated at ~12 seconds / ~720 frames based on music length, but the precise frame count from the timer byte is not consistently documented across sources.
- **Blue P-Switch exact duration:** estimated at ~200 frames (~10 seconds), but the precise ROM value is inconsistently reported.
- **P-Balloon exact duration:** estimated at ~256 frames (~4.3 s) based on TAS resources, but not definitively confirmed.
- **Jump speed lookup table:** the full 8-entry table mapping X-speed brackets to Y-speed values is sourced from SMW Central threads but not all entries are independently verified against the disassembly.
- **Exact acceleration curve:** the ramp sequence (0, 1, 3, 4, 6, 7, 9…) is from TASVideos/SMW Central; sub-velocity fractional math may have additional nuance not fully documented.

**Naming note:** The round enemies called "Goomba" in the SNES manual were officially renamed to "Galoomba" in later games. This spec uses the modern canonical name.

---

## 13. References

### Wikis & Guides
- Super Mario Wiki (mariowiki.com) — primary reference for enemy behaviors, power-up rules, world structure, boss mechanics, item details
- StrategyWiki — level-by-level walkthroughs and control documentation
- GameFAQs — community Q&A threads for obscure mechanics

### Technical / Speedrunning Sources
- TASVideos Game Resources: SNES Super Mario World — exact speed values, oscillation patterns, RAM addresses, subpixel system
- SMW Central (smwcentral.net) — quantified player physics threads, RAM map, cape flight mechanics, P-meter documentation
- SMW Central ROM/RAM Map — memory addresses for speed, position, timers

### Disassembly / Reverse Engineering
- SMWDisX (GitHub: Dotsarecool/SMWDisX) — SNES disassembly reference

### Companion Documents
- [docs/enemies.md](docs/enemies.md) — full enemy catalog organized by type, with HP, behavior, and defeat methods
