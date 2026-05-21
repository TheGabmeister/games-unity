# Super Mario World — Phased Implementation Plan

---

## Phase 1 — Player Movement & Camera

Core player physics and camera behavior — the foundation everything else builds on.

- Subpixel coordinate system (1 pixel = 16 subpixels, 60 fps NTSC)
- Walk (max 21 subpx/f), run with Y/X held (max 37 subpx/f), sprint at full P-meter (max 49 subpx/f)
- Speed oscillation at sprint (48, 47, 48, 47, 49 cycle) and run (36, 35, 36, 35, 37 cycle)
- Acceleration curve from standstill (0, 1, 3, 4, 6, 7, 9…); ~90 frames to reach max sprint
- P-Meter: internal value, max 112, fills while holding Y/X + direction on ground (no on-screen gauge)
- Normal Jump (B): variable height based on hold duration, initial Y-speed varies with X-speed via lookup table, ~5 tiles standing / ~6 tiles at sprint
- Spin Jump (A): lower (~4 tiles / ~5 tiles), same gravity as normal jump; block-breaking and enemy interactions deferred to later phases
- Crouch (Down), facing direction tracking
- Slope sliding: press Down on slope, gain speed downhill, ends at flat ground or wall
- Terminal velocity: Y-speed capped at 64 subpx/f
- No coyote time (original SNES behavior)
- Horizontal camera: Mario positioned off-center in travel direction; on reversal, camera holds until ~65% screen width, then fast linear transition to re-center
- Vertical camera: two per-level modes — (1) scroll-at-will centers on Mario vertically, (2) no-scroll-unless-triggered stays at fixed points unless climbing/flying/swimming/full-speed jump; during normal jump camera does NOT follow Mario upward unless he lands at a different elevation
- L/R buttons nudge camera left/right (disabled in auto-scroll and boss rooms)
- Debug level: flat ground, platforms at varying heights, slopes (gentle and steep), bottomless pits

### Assets

**Sprites**
- Small Mario: idle, walk, run, sprint (arms spread), jump, spin jump, crouch, slide, skid, turn, fall, look up

- Debug level layout (multi-screen horizontal with slope and platform sections)

**Audio**
- Overworld/ground BGM (stub)
- SFX: jump, spin jump, skid/turn, land

---

## Phase 2 — Level Infrastructure

Tilemap system, warp transitions, and all block types — the building blocks of level design.

- Ground tiles, slopes (multiple angles), one-way platforms (pass through from below, solid on top), moving platforms (horizontal, vertical, falling-on-step)
- Pipes: vertical (Down to enter downward, Up for upward) and horizontal (Left/Right to enter), warp destinations per pipe, exit animation
- Doors: press Up to enter, warp to linked destination room
- ? Block: hit from below to reveal contents — coins for now; power-up spawning wired in Phase 3; becomes Empty Block after use
- Rotating Block (Turn Block): hit from below → spins temporarily allowing passage; Spin Jump from above (Super+) destroys permanently (requires Super Mario from Phase 3); max 4 spinning simultaneously
- Note Block: bounces Mario upward on landing; timed jump press = higher bounce
- ON/OFF Switch block: toggles between ON/OFF states on hit (jump, cape, or shell); controls dotted-line blocks in level (solid in one state, passable in the other)
- Message Block: hit from below to display text overlay
- Dotted-Line Blocks: translucent outlines, pass-through until corresponding Switch Palace is activated (activation wired in Phase 9)
- Coins: free-standing collectible, +1 to coin counter; from ? Blocks = 10 points; 100-coin 1-UP wired in Phase 3 (lives system)
- Auto-scroll: forced camera movement at set speed, L/R disabled, Mario constrained within screen bounds
- Debug level extended: multi-screen with pipes (vertical + horizontal), doors, vertical section, all block types, coin trails, auto-scroll test segment, one-way platforms, moving platforms

### Assets

**Sprites**
- Pipe tiles (vertical entry, horizontal entry, connectors)
- Door sprite (open/close animation)
- ? Block (idle shimmer, hit, Empty Block state)
- Rotating Block, Note Block, ON/OFF Switch (two states), Message Block, Dotted-Line Blocks (4 colors, translucent)
- Coin (spin animation), coin-from-block pop animation
- Moving platform sprites
- One-way platform (mushroom-top style)

**Tilemaps**
- Extended debug level layout

**UI**
- Message Block text overlay

**Audio**
- SFX: ? Block hit, coin collect, Rotating Block spin/break, Note Block bounce, ON/OFF Switch toggle, pipe enter/exit, door enter

---

## Phase 3 — Power-Ups & HUD

Power-up state machine, fire and cape ground attacks, Item Reserve, Super Star, level timer, and the full HUD.

- Power-up state machine: Small → Super (Mushroom) → Fire (Fire Flower) / Cape (Cape Feather); Small can go directly to Fire or Cape from a single pickup (skipping Super)
- Damage regression (SNES): Fire/Cape → Small directly (skips Super); Super → Small; Small → lose a life
- P-Meter sprint visual: arm-spread animation at P-meter max (deferred from Phase 1 — requires Super Mario sprites)
- Post-damage invincibility: ~2 seconds of blinking i-frames
- ? Block power-up spawning: Small Mario → Super Mushroom; Super/Fire/Cape Mario → Fire Flower or Cape Feather (contextual per block)
- Super Mushroom: slides along ground after spawning, touch to collect
- Fire Flower: stationary after spawn, touch to collect
- Cape Feather: drifts slowly downward after spawn
- Fire Mario: fireball throw (Y/X), max 2 on screen, bounce along ground with gravity; Spin Jump as Fire Mario releases 2 fireballs (left + right); enemies killed by fireballs turn into coins (200 pts each)
- Cape Mario ground attacks: cape spin (Y/X on ground, sweeps around Mario, defeats enemies on contact); airborne enemy contact triggers auto cape-spin + fall instead of damage. Cape flight deferred to Phase 5
- Item Reserve Box (top-center HUD): holds exactly 1 power-up (Mushroom, Fire Flower, or Cape Feather); auto-fill when collecting a power-up while already powered; auto-deploy when Mario takes damage to Small; manual deploy via Select (falls from top of screen, passes through platforms); Super Mushroom priority rule (always replaces stored item)
- Super Star: temporary invincibility (~12 s), rapid palette cycling, defeat enemies on contact, extends on re-collect; can still die from pits, lava, crushing, time-out. Scoring uses consecutive kill chain (Phase 4)
- Level timer: 1 unit = 41 frames (NTSC); standard values 200/300/400 per level; warning jingle at 99 remaining, then permanent music speed-up; time-out = lose a life
- Scoring system: point values, 7-digit score counter; time bonus formula (50 pts per remaining timer unit — applied at level completion in Phase 5)
- Dragon Coins: large dragon-emblem coins, 5 per level; escalating point values (1000, 2000, 4000, 8000, then 1-UP for the 5th); each counts as 1 regular coin toward 100-coin counter
- Lives system: start at 5, max 99; extra life from 100 coins, 1-UP Mushrooms, 3-UP Moon (3 lives), all 5 Dragon Coins in a level; Game Over at 0 lives → Continue restarts from last save with 5 lives (save system in Phase 9)
- Instant death conditions: pit/offscreen, lava, crushing, time-out
- Full HUD: player name ("MARIO"), lives ("×5"), coin counter, Item Reserve Box, score (7 digits), TIME countdown
- Pause screen: "PAUSE" text overlay, game freeze
- Debug level extended: ? Blocks containing each power-up type, Star block, fire testing corridor, lava pit (instant death test), 1-UP Mushroom block, 3-UP Moon placement

### Assets

**Sprites**
- Super Mario: idle, walk, run, sprint, jump, spin jump, crouch, slide, skid, turn, fall, look up, break block from below
- Fire Mario: all above + fireball throw pose
- Cape Mario: all above + cape spin (ground)
- Fireball projectile (bounce animation)
- Super Mushroom (slide movement), Fire Flower, Cape Feather (drift), Super Star
- 1-UP Mushroom, 3-UP Moon, Dragon Coin
- Power-up transformation animation (grow, shrink, fire/cape acquire)
- Star invincibility palette cycle overlay
- Damage blink animation

**UI**
- HUD status bar (full layout: name, lives, coins, reserve box, score, time)
- Item Reserve Box (empty state, Mushroom/Flower/Feather icons)
- Pause screen overlay
- Points popup sprites (100, 200, 400, 800, 1000, 2000, 4000, 8000, 1-UP)

**Audio**
- SFX: power-up collect (Mushroom, Flower, Feather), fireball throw, fireball hit, damage/powerdown shrink, star collect, 1-UP collect, 3-UP Moon collect, Dragon Coin collect, coin from fireball kill, death, timer warning jingle
- Music: star invincibility theme

---

## Phase 4 — Enemies & Shell Mechanics

Enemy framework, ground and aerial enemies, shell physics, and the consecutive kill chain.

- Enemy spawning/despawning based on camera proximity
- Collision system: stomp (land on top), spin-jump interactions, fireball hit, cape spin/hit, Star contact kill
- Higher enemy bounce when holding jump button on stomp (-88 subpx/f held, -48 released, -8 spin kill; deferred from Phase 1)
- Slope sliding defeats enemies on contact (deferred from Phase 1)
- Galoomba: walks forward, falls off edges; stomp flips over (kickable), fireball/cape/shell/star defeat
- Koopa Troopa (Green): walks forward, falls off edges; stomp → shell; fireball → coin; cape/star defeat
- Koopa Troopa (Red): walks forward, turns at edges; otherwise same as Green
- Shell mechanics: kick (walk into or press Y/X), carry (hold Y/X), throw (release Y/X; Up + release = throw upward), bounce off walls; shells defeat enemies on contact; shell-into-enemies chain uses consecutive scoring
- Koopa Paratroopa (Green): bounces in arcs; stomp removes wings → becomes Green Koopa
- Koopa Paratroopa (Red): flies in set horizontal/vertical pattern; stomp removes wings → becomes Red Koopa
- Rex: walks toward Mario; 2 stomps (1st squishes flat, still active; 2nd defeats); fireball/cape = 1-hit kill
- Monty Mole: pops from ground when Mario approaches, chases
- Bob-omb: walks forward; stomp starts fuse, can be picked up and thrown; explodes after timer; fireball/cape defeat
- Piranha Plant: emerges from pipe periodically; does not emerge if Mario stands adjacent; fireball/cape/star defeat; stomp-immune
- Jumping Piranha Plant: leaps from pipe; same defeat methods
- Bullet Bill: fired from Bill Blaster cannon, flies straight; stomp/cape/star defeat; fireball-immune
- Banzai Bill: giant (~4× size), appears from screen edge; same defeat methods as Bullet Bill
- Consecutive stomp chain (without touching ground): 200 → 400 → 800 → 1000 → 2000 → 4000 → 8000 → 1-UP each; applies to stomps, shell-kicked chains, star kills, and slope-slide kills; ground resets chain
- Item carry system: Mario can carry one item (shell, Bob-omb) while running; Y/X to grab, release to throw
- Debug level extended: enemy placement zones (Galoombas, Koopas, Rex, Monty Mole), pipes with Piranha Plants, Bill Blaster cannon section, shell playground area, Bob-omb area

### Assets

**Sprites**
- Galoomba (walk, flipped, kicked)
- Koopa Troopa — Green, Red (walk, shell, de-shelled/underwear)
- Koopa Paratroopa — Green (bounce), Red (fly pattern)
- Shell sprites (Green, Red, Yellow, Blue — idle, spinning/kicked)
- Rex (walk, squished-flat, defeat)
- Monty Mole (emerge, chase)
- Bob-omb (walk, fuse-lit, carried, explosion)
- Piranha Plant (emerge/retract), Jumping Piranha Plant (leap)
- Bullet Bill, Banzai Bill, Bill Blaster cannon
- Enemy defeat poof animation
- Fireball-killed enemy → coin conversion animation

**Audio**
- SFX: stomp, shell kick, shell bounce/wall hit, enemy defeat (fireball), enemy defeat (cape), enemy defeat (star), Piranha Plant emerge, Bullet Bill fire, Bob-omb fuse/explosion, Monty Mole pop, 1-UP from stomp chain

---

## Phase 5 — Cape Flight & Level Completion

Full cape flight system and the level start-to-finish flow with scoring, checkpoints, and the Bonus Star game.

- Cape flight initiation: P-meter at max (112) + Jump → takeoff timer set to 80, decrements 1/frame; Mario ascends during countdown; at 0 transitions to sustained flight
- Flight control: press forward (facing direction) = descend + accelerate (speed oscillation 47–51 subpx/f); press backward = ascend + decelerate; momentum-based — greater prior descent yields higher ascent; cape pumping (rhythmic backward presses) allows indefinite flight
- Dive-bomb: press forward during flight to dive at high speed; ground impact triggers POW-style earthquake defeating all on-screen grounded enemies instantly
- Slow glide: while falling (not in sustained flight), hold Y/X to float down slowly
- P-meter recharge in air: alternating direction release (6 frames) and direction + Y press (5 frames)
- Giant Gate (goal tape): two tall posts with tape oscillating up and down; Bonus Stars awarded by height (1–50); top = 50 stars + 3 extra 1-UPs; missing tape entirely = 1 coin, 0 stars; all on-screen enemies/items → coins at touch; timer-based 1-UP (last two timer digits are multiple of 11 including 00, and match tens digit of Bonus Stars received); time bonus applied per Phase 3 scoring formula
- Midway Gate (checkpoint): blue-and-white striped posts with tape; one per level max; passing through activates checkpoint; powers up Small Mario to Super; on death, respawn at last activated gate; absent from auto-scroll levels, underwater levels, Star World, and Special Zone
- Iris-out transition on level completion
- Bonus Star accumulation: carries across levels; at 100 → Bonus Star Game triggers; counter resets to (total − 100)
- Bonus Star Game: 9-block grid (8 outer cycling around 1 fixed center); center = fixed item (Mushroom, Fire Flower, or Star); outer cycle: Star 3/8, Mushroom 3/8, Fire Flower 2/8; hit blocks to stop; match 3 in a row = 1-UP; max 8 lives per game
- Debug level extended: long horizontal flight corridor with vertical room, Giant Gate at end, Midway Gate at midpoint

### Assets

**Sprites**
- Cape Mario flight sprites: ascend, sustained glide, dive, cape-pump upstroke
- Giant Gate posts, moving tape, star burst on high-tape hit
- Midway Gate posts + tape (intact, broken after activation)

**VFX**
- Dive-bomb ground impact shockwave
- Iris-out screen transition
- Enemies/items → coin conversion burst at Giant Gate

**UI**
- Bonus Star Game screen: 9-block grid, cycling item icons, match highlight
- Bonus Star counter (popup after Giant Gate)
- Course clear results display

**Audio**
- SFX: cape flutter/whoosh, dive-bomb impact/earthquake, goal tape hit, Midway Gate activate, star tally
- Music: course clear fanfare, Bonus Star Game BGM

---

**Vertical slice checkpoint — Mario can traverse a multi-screen level with platforms, slopes, pipes, doors, and blocks; collect power-ups (Mushroom, Fire Flower, Cape Feather, Star); fight ground and aerial enemies with stomps, fireballs, cape attacks, and shells; fly with the cape via sprint-to-takeoff; reach a Giant Gate to complete the level with Bonus Star scoring. HUD tracks lives, coins, score, time, and reserve item. Bonus Star game triggers at 100 stars. No overworld map — levels are entered directly. Progress does not persist — save system is Phase 9.**

---

## Phase 6 — Swimming & Climbing

Water traversal and climbable surface mechanics with their associated enemies.

- Water physics: Mario sinks naturally; jump button strokes upward; no air meter (indefinite underwater); forward momentum preserved; swimming speeds per §1.2 table (15–17 subpx/f normal, 31–33 carrying item + direction)
- Aquatic enemies: Cheep Cheep (swim patterns or jump from water; fireball/cape/star defeat; stompable above water), Blurp (straight-line swim; underwater only), Rip Van Fish (sleeps until Mario approaches, then chases; very fast; fireball/cape/star), Fish Bone (charges in straight line when Mario in line of sight; fireball-immune; cape/star)
- Climbing: vines, ropes, chain-link fences; grab automatically on contact with directional input; D-pad to move along surface; B to jump off; accelerated climbing (Up + B every other frame)
- Chain-link fences: Y/X to punch (defeats Climbing Koopa on other side); punch a fence gate to flip Mario to opposite side
- Climbing Koopa: Koopa climbing fences; punch from other side to defeat; fireball/star also work
- Debug level extended: water section (pool with aquatic enemies, deep underwater passage), vine section (vertical climb with platforms), fence section (chain-link with gates and Climbing Koopas)

### Assets

**Sprites**
- Mario swimming (stroke, idle sink, carrying item)
- Mario climbing (vine grip, fence grip, fence punch, fence flip)
- Cheep Cheep (swim, jump), Blurp (swim), Rip Van Fish (sleep, chase), Fish Bone (idle, charge)
- Climbing Koopa (climb animation, defeat)
- Vine tiles, chain-link fence tiles (with gate), rope tiles

**Tilemaps**
- Water tileset (surface, deep, water-top edge)
- Fence/vine tileset additions to debug level

**Audio**
- SFX: water splash, swim stroke, vine grab, fence punch, fence gate flip
- Music: underwater BGM

---

## Phase 7 — Yoshi

Full Yoshi mount system with all colors, shell interactions, baby growth, and berries.

- Green Yoshi: hatch from Yoshi Egg in ? Block; Mario auto-mounts
- Tongue: Y/X extends tongue forward; grabs enemies (swallow immediately) and items at range; shells held in mouth (not swallowed)
- Mouth-hold timer: starts at 255, decrements every 4 frames; at 0 the shell is swallowed; spitting resets timer
- Spit shell as projectile (bounces off walls)
- Flutter jump: brief upward flutter extending hang time
- Dismount jump: press A mid-air; Mario ejects upward with higher-than-normal jump; Yoshi continues on prior trajectory
- Ground dismount (A on ground): neutral, no momentum
- Green Yoshi shell abilities: Green Shell = spit projectile; Red Shell = spit 3 fireballs; Blue Shell = grow wings and fly; Yellow Shell = stomp creates sand clouds on landing (stun/defeat nearby enemies)
- Colored Yoshis (Red, Blue, Yellow): innate ability from any shell PLUS the shell's native ability (see §1.8 matrix)
- Flashing shell: grants all 3 abilities (fireballs + flight + sand clouds) to any Yoshi
- Baby Yoshi (Star World only): Red, Blue, Yellow varieties; eat by walking into things (no tongue); feed 5 enemies/objects to grow to adult; feeding a power-up = instant growth; adult color matches baby color
- Berry system: Red Berries (eat 10 → Yoshi lays egg with Super Mushroom; each berry = 1 coin); Pink Berries (eat 2 → Yoshi Cloud drops 10 coins + 1-UP if all collected); Green Berries (+20 timer units; Funky only, 9 total)
- Duplicate Yoshi: finding another Yoshi Egg while already riding Yoshi → 1-UP
- Yoshi cannot enter Ghost Houses, Castles, or Fortresses — dismounts at entrance, waits outside
- Audio: Yoshi bongo percussion track added dynamically to overworld/athletic/underground/underwater BGM when mounted; Ghost House and Castle BGM unaffected
- Debug level extended: Yoshi Egg in ? Block, all 4 shell colors placed, berry bushes (Red, Pink, Green), Baby Yoshi test area with feedable enemies

### Assets

**Sprites**
- Green Yoshi: idle, walk, run, tongue extend/retract, flutter, mouth-full (shell), swallow, egg hatch, wings (Blue Shell flight)
- Red/Blue/Yellow Yoshi: recolored variants of all Green Yoshi sprites
- Baby Yoshi: 3 color variants (walk, eat, grow animation)
- Yoshi Egg (in-block, hatching)
- Berries: Red, Pink, Green
- Yoshi tongue sprite
- Yoshi fire spit projectile (3 fireballs)
- Yoshi Cloud (Pink Berry reward)

**VFX**
- Sand cloud stomps (Yellow Shell ability)
- Yoshi wing growth flash
- Baby → Adult growth burst

**Audio**
- SFX: Yoshi mount, tongue extend/grab, swallow, spit, flutter, egg hatch, berry eat, baby grow, dismount
- Yoshi bongo percussion layer (dynamic, additive to existing level BGM)

---

## Phase 8 — Interactive Items

P-Switches, Springboard, Grab Blocks, Key + Keyhole, and P-Balloon — all carried/activated items beyond shells.

- Blue P-Switch: can be carried (Y/X grab) before activation; jump on to activate; turns coins into solid blocks and blocks into coins for ~10 seconds; may reveal hidden ? Blocks or P-Switch Doors; dedicated P-Switch BGM replaces level music during effect
- Gray P-Switch: carry + activate same as Blue; turns most on-screen enemies into Silver Coins; escalating reward chain: points up to 8000, then 8th coin = 1-UP, 9th = 2-UP, 10th+ = 3-UP each
- Springboard: portable trampoline; carry (Y/X) and place anywhere; bounce on landing; timed jump press = super-high bounce
- Grab Block: pick up (Y/X), throw at enemies to defeat them; breaks after a few seconds of being held (flash warning); Yoshi can swallow them
- Key + Keyhole: carry Key (Y/X); functions as melee weapon while held (hit enemies); Yoshi can hold in mouth and walk to keyhole; bring Key to Keyhole → expand animation → "swallows" Mario → triggers secret exit flag. Overworld secret-path unlocking wired in Phase 9
- P-Balloon / Balloon Mario: extends Phase 3 power-up state machine; free directional floating for ~4 s; pulsing warning before expiry; Mario drops straight down when effect ends; cannot ride Yoshi or grab items while active; collecting any other power-up cancels immediately
- Jumping Fire Piranha Plant: extends Phase 4 pipe enemies; leaps from pipe and shoots 2 fireballs at apex (requires fireball projectile from Phase 3)
- Debug level extended: P-Switch test rooms (Blue: coin/block grid; Gray: enemy cluster), Springboard platforming section, Grab Block arena, Key + Keyhole placement, P-Balloon floating corridor, pipe with Jumping Fire Piranha Plant

### Assets

**Sprites**
- Blue P-Switch (idle, pressed/flat)
- Gray P-Switch (idle, pressed/flat)
- Silver Coin (from Gray P-Switch)
- Springboard (idle, compressed on land, stretch on bounce)
- Grab Block (idle, carried/flashing, thrown, break)
- Key (idle, carried)
- Keyhole (idle, expanding)
- Balloon Mario (inflated float, directional, pulsing deflate warning)
- P-Balloon item
- Jumping Fire Piranha Plant (leap, fireball spit)
- P-Switch Door (hidden, revealed)

**VFX**
- Coin ↔ block swap wave (Blue P-Switch)
- Keyhole expand + swallow spiral
- Balloon inflate/deflate burst

**Audio**
- SFX: P-Switch stomp, block-coin swap ambient, Springboard bounce, Grab Block pick up/throw/break, Key pickup, keyhole unlock, P-Balloon inflate/deflate
- Music: P-Switch BGM

---

## Phase 9 — Overworld Map & Save

The Dinosaur Land overworld, path progression, Switch Palaces, save system, and title screen.

- Overworld map: single continuous Dinosaur Land map; node-based movement along fixed paths; no free roaming
- Level markers: yellow dot (1 exit), red dot (2 exits), castle, fortress, ghost house, switch palace icons
- Path unlocking: normal exit reveals standard next-node path; secret exit (from Phase 8 Key+Keyhole) reveals branching path
- 96-exit counter tracked globally; title screen displays "★96" at completion
- World transitions between the 7 main worlds
- Castle destruction cutscene after Koopaling boss defeat; L+R on ruins to replay (international versions)
- Sunken Ghost Ship gating: completing it triggers Bowser-shaped cave entrance rising from water, opening Valley of Bowser (level content in Phase 11)
- Fortresses and Ghost Houses always replayable
- Yoshi's House: start node, non-playable
- Top Secret Area: hidden node in Donut Plains region; 5 ? Blocks (2 Cape Feathers, 2 Fire Flowers, 1 Yoshi Egg — or 1-UP if Yoshi present); resets every entry
- Switch Palaces: 4 one-time levels (Yellow in Yoshi's Island, Green in Donut Plains, Red in Vanilla Dome, Blue in Forest of Illusion); each contains a large colored switch; pressing it permanently converts all same-colored Dotted-Line Blocks (Phase 2) into solid ! Blocks game-wide; Yellow ! Blocks contain Super Mushrooms, Green contain Cape Feathers, Red and Blue are solid platforms; overworld icon changes to pressed state; cannot be replayed
- Save system: 3 file slots; save prompt after completing Ghost House, Fortress, Castle, or Switch Palace ("SAVE AND CONTINUE" / "CONTINUE WITHOUT SAVING"); replaying a completed save-eligible level re-triggers the prompt
- Save data: lives, coins, score, power-up state, reserve item, exit completion flags (96 exits), Switch Palace activation flags, overworld position, Bonus Star count
- Title screen: file select (New Game / Continue from saved file), "★96" display when applicable
- Map screen HUD: world name, player position marker, level name on hover
- Overworld playable segment: Yoshi's Island (6 nodes + Yellow Switch Palace) as the initial testable region; remaining world maps are content (Phase 13)
- Music per world region: Yoshi's Island map theme; remaining themes in Phase 13

### Assets

**Sprites**
- Mario overworld walk sprite
- Level node markers (yellow dot, red dot, castle, fortress, ghost house, switch palace — normal + pressed)
- Castle destruction animation (crumble + dust)
- Path reveal animation (road drawing)
- Yoshi's House node

**Tilemaps**
- Overworld map: Yoshi's Island region (grass, water, mountains, paths, bridges)
- Switch Palace interior tileset
- Colored switch sprites (Yellow, Green, Red, Blue — upright + pressed)
- ! Block sprites (4 colors — solid, with item for Yellow/Green)

**UI**
- Title screen (file select, ★96 badge)
- Save prompt overlay ("SAVE AND CONTINUE" / "CONTINUE WITHOUT SAVING")
- File select screen (3 slots, player name, exit count, world indicator)
- Map screen HUD (world name, level name)

**Audio**
- SFX: map walk step, node select, path reveal, castle crumble, switch press, file select confirm
- Music: Yoshi's Island map theme, title screen theme

---

## Phase 10 — Boss Encounters

All Koopaling fights, Reznor, Big Boo, and the three-phase Bowser finale.

- Boss arena framework: boss door transition, sealed room, boss intro sequence (HUD disappears briefly)
- **Tilting Platform archetype (Iggy, Larry):** platform floating in lava, tilts left/right; Koopaling fires magic projectiles; stomp or fireball pushes them toward the edge, they slide in tilt direction; push off platform into lava to defeat. Iggy (Castle 1): basic. Larry (Castle 7): adds 3 Lava Bubbles leaping from lava
- **Wall Climber archetype (Morton, Roy):** enclosed room; Koopaling runs toward Mario, climbs wall, drops from ceiling targeting Mario; if Mario is grounded when they land, Mario is briefly stunned; 3 stomps to defeat; speeds up after each hit. Morton (Castle 2): basic. Roy (Castle 5): walls slowly close in over time
- **Pipe Decoy archetype (Lemmy, Wendy):** room with pipes; Koopaling + 2 decoys pop out simultaneously; only the real one can be hit; 3 stomps to defeat. Lemmy (Castle 3): 1 bouncing Lava Bubble. Wendy (Castle 6): 2 diagonally-bouncing Lava Bubbles
- **Ludwig (Castle 4):** spits up to 4 fireballs, retreats into shell and spins across floor, then leaps toward Mario; 3 stomps to defeat
- **Reznor (4 fortresses — Vanilla, Forest, Chocolate, Valley):** 4 Reznors on rotating platforms mounted on a wheel; spit fireballs at Mario; hit each from below (bump platform) to knock off, 1 hit per Reznor; after 2 defeated, floor collapses from sides inward — must jump onto vacated rotating platform; cape deflects their fireballs
- **Big Boo boss (Donut Secret House):** arena with Grab Block floor (extends Phase 8 Grab Blocks); Big Boo + 2 normal Boos; throw Grab Blocks at Big Boo's face, 3 hits to defeat; Big Boo does not hide when faced (unlike normal Boos); normal Boos use gaze-tracking AI (approach when Mario faces away, freeze when faced — this AI is introduced here; Phase 11 extends it to full ghost house roster); throwing creates holes in floor (fall = death); defeating opens path to Star World
- **Bowser (Bowser's Castle):** rides Koopa Clown Car on castle roof. Front Door: 8-room gauntlet with varied enemies then dark corridor to boss. Back Door: secret exit from Valley of Bowser 2, skips to boss. Phase 1 — Mechakoopa Drop: Bowser flies left/right, drops 2 Mechakoopas; stomp to stun, pick up, throw upward to hit Bowser; 2 hits. Transition 1: Bowser offscreen, flames rain, Princess Toadstool throws Super Mushroom. Phase 2 — Big Steelies: Clown Car flips upside down, drops Big Steelies (rolling iron balls), then 2 Mechakoopas; 2 hits. Transition 2: same as Transition 1. Phase 3 — Ground Pound: Clown Car angry face, Bowser bounces on ground trying to crush Mario, drops Mechakoopas; 2 hits. Total: 6 Mechakoopa hits across 3 phases
- Debug: boss test rooms for each archetype (tilting platform, enclosed room, pipe room, rotating wheel, Grab Block floor, Bowser rooftop)

### Assets

**Sprites**
- 7 Koopalings: Iggy, Morton, Lemmy, Ludwig, Roy, Wendy, Larry (unique designs, attack animations, defeat animations)
- Koopaling magic projectile
- Koopaling decoys (Lemmy ×2, Wendy ×2)
- Reznor (on platform, spit, knocked off)
- Big Boo (boss variant — no hide, hit reaction)
- Bowser (in Koopa Clown Car)
- Koopa Clown Car (normal face, angry face, flipped upside-down, propeller spin)
- Mechakoopa (walk, stunned, carried)
- Big Steelie (rolling iron ball)
- Princess Toadstool (throws Mushroom from above, post-victory float down)
- Lava Bubble (jump from lava, arc, fall)

**Tilemaps**
- Boss room tilesets: tilting platform + lava, enclosed room + closing walls, pipe room, rotating wheel + collapsing floor, Grab Block arena, Bowser's Castle rooftop
- Front Door / Back Door gauntlet rooms (8 rooms)

**VFX**
- Koopaling defeat explosion
- Lava splash
- Floor collapse chunks
- Bowser fire rain
- Clown Car explosion + fly-away
- Boss intro text/animation

**Audio**
- SFX: boss hit, Koopaling defeat, Reznor knockoff/fireball, floor collapse, Bowser roar, Mechakoopa wind-up/stomp, Big Steelie roll, Clown Car hover, fire rain fall, wall close rumble
- Music: castle boss BGM, fortress/Reznor boss BGM, Bowser phase 2 theme, boss clear fanfare

---

## Phase 11 — Ghost House & Castle Enemies

Ghost House puzzle mechanics, Boo AI, and the full castle/fortress enemy roster.

- **Ghost House mechanics:** door-based puzzle navigation (multi-room layouts, correct door sequences, dead-end loops); extends Phase 10 Boo gaze-tracking AI to full ghost house environment
- Ghost House enemies: Boo (invincible normally; Star only), Boo Buddies/Boo Crew (Boos rotating in fixed circle formation; invincible), Boo Buddy Snake (chain of Boos in snake path; invincible), Block Boo (appears as solid platform when Mario faces it, becomes Boo when turned away; invincible), Disappearing Boo Buddies (appear/disappear in timed groups; invincible), Fishing Boo (ghost on cloud, blue flame on fishing line tracks Mario; invincible), Eerie (small ghost, straight line or wave flight; cape/star only, stomp-immune)
- **Castle/Fortress enemies:** Thwomp (stone block, slams down on proximity; invincible; Spin Jump safe bounce), Thwimp (small hopping stone in fixed arc; invincible), Dry Bones (skeletal Koopa; stomp collapses temporarily; reassembles after ~5 s; cape sends sliding; Star kills permanently), Magikoopa/Kamek (teleports, shoots magic that transforms blocks into enemies; 1 stomp/fireball/cape/star), Ball 'n' Chain (spiked ball orbiting block; invincible), Li'l Sparky (small spark following block edges; Star only), Hothead (larger spark following block edges; invincible), Grinder/Chainsaw (circular saw on wire/surface; invincible), Lava Bubble/Podoboo (fireball jumping periodically from lava; Star only; Spin Jump safe bounce — sprite from Phase 10), Blargg (lava monster lunges from lava pools; invincible; bone raft levels)
- Castle/Fortress level elements: lava tiles, bone raft (floating platform on lava), conveyor platforms
- Sunken Ghost Ship: underwater ghost-themed level; completing it opens the Valley of Bowser (Bowser-shaped cave entrance rises from water on overworld)
- Debug: ghost house test level (multi-door puzzle with Boo gauntlet, Block Boos as platforms, Fishing Boo corridor), castle test level (Thwomp corridor, lava + Blargg, Dry Bones + Magikoopa room, fence with Grinders)

### Assets

**Sprites**
- Boo (approach, freeze/hide face)
- Big Boo (non-boss variant, same Boo AI)
- Boo Buddies circle formation
- Boo Buddy Snake chain
- Block Boo (block form, Boo form, transition)
- Disappearing Boo Buddies (fade in/out)
- Fishing Boo (ghost + cloud + fishing line + blue flame)
- Eerie (straight/wave flight)
- Thwomp (idle, slam, stunned-face), Thwimp (hop)
- Dry Bones (walk, collapse, reassemble)
- Magikoopa (appear, teleport, magic bolt, disappear)
- Ball 'n' Chain (ball + chain + anchor block)
- Li'l Sparky (edge-follow animation), Hothead (edge-follow, larger)
- Grinder/Chainsaw (spin on wire)
- Blargg (submerged, lunge, retract)
- Bone raft, conveyor platform

**Tilemaps**
- Ghost house tileset (wooden floors/walls, doors, cobwebs, dim lighting)
- Castle/fortress tileset (stone walls, lava, metal grates, torches)
- Sunken Ghost Ship tileset (underwater + ghost house hybrid)

**Audio**
- SFX: Boo laugh/approach, Thwomp slam, Thwimp hop, Dry Bones collapse/reassemble, Magikoopa magic bolt/teleport, Grinder saw buzz, Blargg roar/lunge, conveyor hum
- Music: ghost house BGM, castle BGM

---

## Phase 12 — Expanded Enemy Roster

All remaining enemies from [docs/enemies.md](docs/enemies.md) not introduced in earlier phases, plus additional level type themes.

- **Ground enemies:** Chargin' Chuck (all 6 variants: running, jumping, kicking footballs, throwing baseballs, digging rocks, splitting into 3; 3 stomps / 5 fireballs / 1 cape hit), Wiggler (caterpillar; 1st stomp enrages — turns red, speeds up; star/Yoshi swallow only after enrage), Dino Rhino (large; stomp → becomes Dino-Torch) + Dino-Torch (small; breathes fire periodically; 1 hit), Pokey (4-segment cactus; fireball removes 1 segment; cape/star/Yoshi eat segments), Ninji (bouncing star-shaped; 1 hit; castles and Special Zone), Buzzy Beetle (walks, turns at edges; stomp → shell; fireball-immune; cape/star), Spike Top (Buzzy Beetle with spike; can't stomp; spin jump safe; cape/star/Yoshi), Bony Beetle (skeletal; walks, periodically extends spikes — invulnerable during spike phase; stomp collapses temporarily, regenerates ~5 s; star), Sumo Bro (on high platform; stomps to create lightning → ground fire below; can't stomp from above; cape/star), Mega Mole (huge, sunglasses; invincible; rideable on top as platform), Muncher (stationary black plant; Star only; spin jump safe bounce)
- **Parachute variants:** Parachute Galoomba (floats down, then walks), Parabomb (Bob-omb on parachute)
- **Aerial:** Super Koopa — Standard (caped Koopa, set fly pattern) + Flashing Cape variant (erratic flight; drops Cape Feather on defeat), Lakitu (rides cloud, throws Spiny Eggs; stomp can steal cloud for temporary ride) + Spiny (from eggs; walks with spikes; fireball/cape/star/shell; can't stomp; spin jump safe), Fishin' Lakitu (dangles 1-UP lure; taking 1-UP triggers Spiny throwing), Amazing Flyin' Hammer Brother (rides flying platform, throws hammers both directions; platform rideable after defeat), Swooper/bat (hangs from ceiling, swoops when Mario passes below)
- **Aquatic:** Porcupuffer (giant spiny fish, skims water surface; cape/star; can't stomp), Urchin (spiky sea creature, set movement pattern; invincible), Torpedo Ted (large torpedo from launcher; stomp/cape/star), Blooper (squid, chases underwater; fireball/cape/star)
- **Remaining Koopa variants:** Yellow Koopa Troopa (when re-shelled, kicks the shell at Mario), Blue Koopa Troopa (when re-shelled, chases Mario at high speed), Paragaloomba (Galoomba with wings, hops; stomp removes wings)
- **Pipe:** Volcano Lotus (stationary; shoots 4 fireballs upward in arc; cape/star only; fireball-immune; stomp-immune)
- **Athletic/underground level types:** athletic theme (sky platforms, moving platforms, auto-scroll variants), underground theme (cave tileset, lower lighting)
- Enemies already introduced in Phases 4/6/8/11 are not re-listed; all combat formulas and defeat interactions from those phases apply automatically to new enemies using the same framework

### Assets

**Sprites**
- Chargin' Chuck (6 variants: running, jumping, kicking football, throwing baseball, digging rock, splitting — each needs unique projectile/behavior sprites + stun/defeat)
- Football, baseball, rock projectiles
- Wiggler (calm walk, enraged red/fast)
- Dino Rhino (walk, stomp), Dino-Torch (walk, fire breath)
- Pokey (4 segments, segment removal animation)
- Ninji (bounce), Buzzy Beetle (walk, shell), Spike Top (walk)
- Bony Beetle (walk, spike extend/retract, collapse/regenerate)
- Sumo Bro (idle, stomp), lightning bolt, ground fire
- Mega Mole (walk with sunglasses)
- Muncher (stationary)
- Parachute Galoomba (float, land), Parabomb (float)
- Super Koopa — Standard + Flashing Cape (fly, defeat/feather drop)
- Lakitu (cloud ride, throw), Lakitu's Cloud (stealable), Spiny (walk), Spiny Egg (thrown, rolling, hatch)
- Fishin' Lakitu (cloud + fishing line + 1-UP lure)
- Amazing Flyin' Hammer Brother (flying platform, throw), hammer projectile
- Swooper (hang, swoop)
- Porcupuffer (surface swim), Urchin (set-path movement), Torpedo Ted (launch, fly), torpedo launcher, Blooper (chase swim)
- Yellow/Blue Koopa Troopa (walk, shell, de-shelled — unique re-shell behaviors), Paragaloomba (hop, wingless)
- Volcano Lotus (idle, fireball spit), arcing fireball projectile

**Tilemaps**
- Athletic tileset (sky background, cloud platforms, moving-platform rails)
- Underground tileset (cave walls, dim lighting, stalactites)

**Audio**
- SFX: Chargin' Chuck charge/whistle, Wiggler enrage, Dino-Torch fire breath, Sumo Bro lightning/stomp, Lakitu throw, Spiny Egg hatch, hammer throw, Swooper screech, Torpedo Ted launch, Blooper ink
- Music: athletic BGM, underground BGM

---

## Phase 13 — Content Expansion & Two-Player

All remaining world content, Star Road, Special Zone, cosmetic completion reward, and two-player alternating mode.

- **World content:** all 9 world overworld map regions (Yoshi's Island already in Phase 9; Donut Plains, Vanilla Dome, Twin Bridges, Forest of Illusion, Chocolate Island, Valley of Bowser, Star World, Special Zone)
- All ~73 levels built across all worlds, fulfilling the full 96-exit count (72 normal + 24 secret)
- **Star Road:** 5 Star Road portals on overworld (Donut Plains, Vanilla Dome, Twin Bridges, Forest of Illusion, Valley of Bowser); warp to Star World. Normal exits = return to corresponding overworld portal (world-to-world warps). Secret exits = chain forward to next Star World level. Star World 5 secret exit → Special Zone
- **Star World levels (5):** each features a Baby Yoshi (Phase 7) — SW1 Red, SW2 Blue, SW3 Yellow, SW4 Red, SW5 Yellow
- **Special Zone (8 levels):** Gnarly, Tubular, Way Cool, Awesome, Groovy, Mondo, Outrageous, Funky. Single exit each. Tubular relies heavily on P-Balloon (Phase 8). Funky contains 9 Green Berries (Phase 7)
- **Completion reward:** after clearing Funky, Star Road warps to Yoshi's House; overworld palette swaps spring → autumn permanently; cosmetic enemy sprite replacements: Koopa Troopa/Paratroopa → Mask Koopa (Mario mask), Piranha Plant → Pumpkin Plant (jack-o'-lantern), Jumping Piranha Plant → Jumping Pumpkin Plant, Bullet Bill → Pidgit Bill (bird sprite), Green Koopas recolored Yellow, Red Koopas recolored Blue. Behavior unchanged
- **Two-player mode:** alternating turns (Player 1 = Mario, Player 2 = Luigi); turns swap on level completion or life loss; each player has independent lives, coins, score, power-up state, and reserve item; both share overworld map state (opened paths, completed levels); shared Midway Gate checkpoints (if one player activates, other player starts there too); L/R on map screen = life transfer; at 0 lives, life transfer prompt appears before Game Over
- **Luigi:** functionally identical to Mario (SNES original); separate sprite set with green palette
- All levels assigned their proper timer values (200/300/400) and Midway Gate presence/absence per Phase 5 rules
- Dragon Coin placement: 5 per applicable level (absent from Castles, Ghost Houses, Fortresses)
- 3-UP Moon placement: 7 total (Yoshi's Island 1, Donut Plains 4, Vanilla Dome 3, Cheese Bridge Area, Forest Ghost House, Chocolate Island 1, Valley of Bowser 1)

### Assets

**Sprites**
- Luigi: full sprite set matching all Mario states (Small, Super, Fire, Cape, Balloon, swimming, climbing, Yoshi-riding) in green palette
- Mask Koopa (Koopa with Mario mask), Pumpkin Plant, Jumping Pumpkin Plant, Pidgit Bill (cosmetic replacement sprites)
- Star Road portal (overworld warp icon)

**Tilemaps**
- Overworld maps: Donut Plains, Vanilla Dome, Twin Bridges, Forest of Illusion, Chocolate Island, Valley of Bowser, Star World, Special Zone regions
- Autumn palette variant of entire overworld
- Level tilemaps for all ~73 levels across all worlds (using tilesets from Phases 2, 6, 11, 12)

**Audio**
- Music: Donut Plains map theme, Vanilla Dome map theme, Twin Bridges map theme, Forest of Illusion map theme, Chocolate Island map theme, Valley of Bowser map theme, Star World map theme (invincibility theme variant), Special Zone map theme (standard → SMB1 ground arrangement after ~2 minutes)
