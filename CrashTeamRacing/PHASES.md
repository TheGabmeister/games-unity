# Crash Team Racing — Implementation Phases

## Phase 1 — Kart & Track Foundation

The minimum to drive around a track and feel the basic handling.

- Kart controller: acceleration, braking, reverse, steering, gravity, hop
- Engine class physics (use Balanced class as default; class-driven parameters from MetaPhys)
- Single track: Crash Cove (simplest track, no hazards)
- Track surface collision and wall collision
- Lap detection (checkpoint/sector system, 3-lap race)
- Basic camera: behind-car follow with speed-based zoom-out
- Race HUD: elapsed timer, lap counter, speedometer
- Audio: engine idle loop, engine acceleration loop (pitch scales with speed), hop SFX, wall bump SFX, checkpoint chime

### Assets

**3D Models**
- Generic kart
- Crash Cove track (geometry, collision mesh, checkpoints)
- Wumpa Fruit crate (basic destructible)

**Audio**
- Engine idle loop
- Engine acceleration loop (pitch-shiftable)
- Hop SFX
- Wall bump SFX
- Checkpoint chime

**UI**
- Race timer display
- Lap counter
- Speedometer gauge

## Phase 2 — Power Slide & Speed System

The signature mechanic that makes CTR feel like CTR.

- Power Slide: hop + direction enters drift; turbo meter appears (green to red fill)
- Mini-turbo triggering: opposite shoulder button while meter is in red zone
- Triple boost chaining (3 consecutive mini-turbos within one slide, escalating boost)
- Backfire on overfill; spin-out after 60 drift frames without a successful boost
- Reserve system: invisible reserve timer, frame-by-frame depletion, exponential gain for late boosts
- Fire levels (None / Green / Yellow / Sacred Fire) with corresponding speed caps
- Reserve-preserving techniques: mid-air brake, U-Turn
- Turbo pads: grant Sacred Fire + reserves on contact, freeze reserve drain while on pad
- Wumpa Fruit: collect from crates/track, speed bonus per fruit, Juiced Up at 10
- Hang-time boost: airborne duration grants landing speed boost based on jump-o-meter thresholds
- Audio: drift loop, mini-turbo trigger SFX (escalating pitch per chain), backfire SFX, spin-out SFX, Wumpa collect SFX, turbo pad whoosh, fire level transition SFX, Juiced Up activation SFX

### Assets

**3D Models**
- Turbo pad (glowing arrow surface)
- Wumpa Fruit collectible
- Fruit crate

**VFX**
- Exhaust fire (green, yellow, red/Sacred Fire tiers)
- Drift tire sparks
- Turbo pad glow pulse
- Boost trail (per fire level)
- Wumpa collect sparkle

**Audio**
- Drift/slide loop (sustained)
- Mini-turbo trigger SFX (3 variants: 1st/2nd/3rd boost)
- Backfire SFX
- Spin-out SFX
- Wumpa Fruit collect SFX
- Turbo pad whoosh
- Fire level up transition SFX
- Juiced Up activation SFX
- Landing boost SFX

**UI**
- Turbo boost meter (green-to-red fill bar)
- Hang-time meter
- Wumpa counter (0–10 with animated icon, golden background at 10)

## Phase 3 — Items & Combat

Weapons, defense, and the chaos that makes races unpredictable.

- ? Crate placement on track and drive-through pickup
- Item roulette animation and HUD (held item window, Wumpa counter)
- Position-based item distribution (4 item sets weighted by race position)
- All 9 item types:
  - Tracking Missile (single and triple)
  - Bowling Bomb (single and triple, forward roll or drop behind)
  - TNT Crate (lands on head, shake off by hopping)
  - N. Brio's Beaker (green potion, drop or throw)
  - Power Shield (absorbs one hit, throwable, timer-based expiry)
  - Turbo (instant speed burst)
  - Aku Aku / Uka Uka Mask (invincibility, speed boost, off-road immunity)
  - N. Tropy Clock (all opponents spin out and slow down)
  - Warp Orb (homing strike on 1st place, hits anyone in path)
- Juiced Up variants for all items (Nitro Crate, Red Beaker, Blue Shield, enhanced effects)
- Hit recovery: tumble animation, Wumpa scatter, reserve loss, brief invincibility frames
- Special item rules: one Warp Orb at a time, Lap 1 item set downgrade
- Respawn system: mask pickup on fall-off, reserve/fire level reset
- Audio: crate pickup SFX, roulette shuffle loop, roulette stop SFX, per-item launch/deploy SFX, per-item impact/explosion SFX, mask activation jingle, mask invincibility music (replaces track music), hit tumble SFX, respawn SFX

### Assets

**3D Models**
- ? Crate
- Tracking Missile
- Bowling Bomb
- TNT Crate
- Nitro Crate
- Beaker (green)
- Beaker (red, Juiced variant)
- Power Shield bubble (green + blue variants)
- Warp Orb
- Aku Aku mask
- Uka Uka mask

**VFX**
- Explosion (bomb blast, TNT detonation, Nitro detonation)
- Missile smoke trail
- Warp Orb energy trail
- Shield bubble shimmer
- Mask invincibility aura
- Beaker splash/puddle
- Red Beaker rain cloud
- TNT countdown timer (on-head display)
- Wumpa scatter burst (on hit)
- Invincibility flash (i-frames)

**Audio**
- ? Crate pickup
- Item roulette shuffle loop
- Item roulette stop
- Missile launch
- Missile impact
- Bomb roll loop
- Bomb explosion
- TNT land-on-head
- TNT countdown tick
- TNT/Nitro detonation
- Beaker drop
- Beaker splash (opponent hit)
- Shield activate
- Shield break
- Turbo item use
- Mask activation jingle
- Mask invincibility music loop
- Clock activation
- Clock slowdown ambience
- Warp Orb launch
- Warp Orb hit
- Hit tumble SFX
- Wumpa scatter SFX
- Respawn SFX (mask pickup)

**UI**
- Item window (held item icon display)
- Item roulette animation
- Juiced Up item window background (shining effect)

## Phase 4 — AI Opponents & Race Flow

Complete the vertical slice: a full 8-racer race from countdown to results.

- Race start countdown: traffic light (red/yellow/green), voice cue, engine idle
- Start Boost: timed X press on green light, engine stall on early press
- AI driving: NavFrame path following (3 parallel paths per track, RNG assignment)
- AI item pickup and usage (weapon cooldown, simulated turbo meter)
- Difficulty system: 14-parameter table interpolated by currDifficulty value
- 8-racer field with real-time position tracking
- Ranked driver icons on HUD
- Race finish: results screen with positions
- Wrong way detection and warning
- Audio: countdown voice lines ("3", "2", "GO!"), start boost SFX, engine stall SFX, race finish jingle, wrong way warning SFX, results screen music, Crash Cove track music

### Assets

**3D Models**
- Traffic light (3-lamp post)
- Starting grid markers/lines
- 7 AI kart color variants (or character placeholders)

**VFX**
- Engine stall smoke puff
- Start boost flash

**Audio**
- Countdown voice — "3"
- Countdown voice — "2"
- Countdown voice — "GO!"
- Start boost SFX
- Engine stall SFX
- Race finish jingle (positive)
- "WRONG WAY" voice/SFX
- Results screen music
- Crash Cove track music loop

**UI**
- Traffic light countdown overlay
- Position display ("1st", "2nd", etc.)
- Ranked driver icons (right side, all 8 racers)
- Wrong way warning flash
- "FINISHED!" / "LOSER!" text
- Results screen (positions, times)

---

**Vertical slice checkpoint — Phases 1–4 deliver a complete race on Crash Cove: power sliding, items, AI opponents, countdown to finish, with full audio.**

---

## Phase 5 — Characters, Terrain & Core Modes

Flesh out the character roster, track surfaces, and standalone game modes.

- 3 starter characters across 3 engine classes: Crash (Balanced), Coco (Accel), Tiny (Speed)
- Character select screen with cosmetic stat bars
- Engine class system: class-driven physics parameters, stat bar display
- Alignment system: Good characters use Aku Aku, Evil use Uka Uka (functionally identical)
- Terrain types: grass/dirt, sand, mud, ice (reduced traction), shallow water, deep water/void
- Terrain friction applied per surface; Mask negates off-road friction
- Single Race mode: track select, lap count (3/5/7), difficulty (Easy/Medium/Hard)
- Time Trial mode: solo, no items, race clock, ghost recording and playback
- N. Tropy ghost times per track (beat all to unlock N. Tropy)
- Camera modes: Near / Far / First-Person cycle (L2), rear view (R2)
- Audio: per-terrain drive sounds, character voice lines (celebration, hit reaction, select), ghost playback ambience, menu navigation SFX, menu music

### Assets

**3D Models**
- 3 character driver models: Crash, Coco, Tiny
- 3 kart color/skin variants (one per character)
- Ghost kart (translucent variant)

**VFX**
- Grass/dirt spray
- Sand kick
- Water splash (shallow)
- Mud splatter
- Ice crystal spray
- Ghost trail shimmer

**Audio**
- Grass/dirt drive loop
- Sand drag loop
- Mud squelch loop
- Ice slide loop
- Shallow water splash loop
- Crash voice — select/confirm, celebration, hit reaction
- Coco voice — select/confirm, celebration, hit reaction
- Tiny voice — select/confirm, celebration, hit reaction
- Menu navigation SFX (cursor move, confirm, back)
- Character select music
- Track select music
- Time Trial countdown beep

**UI**
- Character select screen (roster grid, stat bars, alignment icon)
- Track select screen
- Difficulty select
- Lap count select
- Time Trial timer and ghost indicator
- Camera mode indicator

## Phase 6 — Adventure Mode

The single-player campaign with hub worlds and progression.

- Hub world navigation: on-foot player movement in 3D hub spaces
- 5 hub worlds (N. Sanity Beach, The Lost Ruins, Glacier Park, Citadel City, Gemstone Valley)
- Track portals: enter portals to load race tracks
- Trophy progression: win 1st to earn Trophy, Trophy count gates hub/boss access
- Boss races: 1v1 with unique weapon patterns per boss (Ripper Roo, Papu Papu, Komodo Joe, Pinstripe, Oxide)
- Boss difficulty scaling: base difficulty from bossID, reduced on each loss
- Oxide special behavior: early start, hovercraft (no spin-out), escalating weapon types
- CTR Challenge: C-T-R letters placed on track, collect all three + finish 1st for a CTR Token
- Relic Race: solo time trial with Time Crates (1/2/3 second freeze), all-crate bonus, Sapphire/Gold/Platinum thresholds
- Crystal Bonus Rounds: collect 20 crystals in arena within time limit, TNT/Nitro obstacles
- Boss Keys: earned per boss, unlock next hub and ultimately Oxide race
- Gem Cups: 5 cups unlocked by matching-color CTR Tokens, each awards a Gem + secret character
- Save/load system: adventure progress slots, collectible tracking
- Adventure endings: Bad (incomplete collection), Good (100%), Best (101% all Gold/Platinum Relics)
- Audio: hub ambient music per world, hub footstep SFX, portal entry SFX, boss intro fanfare, boss defeat jingle, trophy/key/gem/relic earned fanfares, CTR letter collect SFX, crystal collect SFX, time crate break SFX, clock freeze SFX, ending music themes

### Assets

**3D Models**
- 5 hub world environments (N. Sanity Beach, The Lost Ruins, Glacier Park, Citadel City, Gemstone Valley)
- Hub player character (on-foot Crash)
- Track portals (per hub style)
- Save point terminals (green-screen kiosks)
- Boss karts: Ripper Roo's kart, Papu Papu's kart, Komodo Joe's kart, Pinstripe's kart, Oxide's hovercraft
- CTR letter pickups (C, T, R — glowing 3D letters)
- Time Crates (variants: 1, 2, 3)
- Crystal collectible
- 4 bonus arenas: Skull Rock, Rampage Ruins, Rocky Road, Nitro Court

**VFX**
- Portal shimmer/glow
- Trophy earned burst
- Key earned burst
- Gem earned burst
- Relic earned burst (Sapphire/Gold/Platinum color)
- CTR letter collect flash
- Crystal collect sparkle
- Time crate break + clock freeze pulse
- Boss defeat explosion

**Audio**
- Hub music — N. Sanity Beach
- Hub music — The Lost Ruins
- Hub music — Glacier Park
- Hub music — Citadel City
- Hub music — Gemstone Valley
- Hub footstep SFX
- Portal entry SFX
- Boss intro fanfare
- Boss defeat jingle
- Trophy earned fanfare
- Boss Key earned fanfare
- CTR Token collect SFX
- Gem earned fanfare
- Relic earned fanfare
- CTR letter collect SFX
- Crystal collect SFX
- Time crate break SFX
- Clock freeze SFX
- Bad ending music
- Good ending music
- Best ending music (with confetti)
- Credits music

**UI**
- Hub world minimap/compass
- Collectible tracker (Trophies, Keys, Tokens, Gems, Relics)
- Boss intro splash screen
- Relic Race timer with time crate counter
- Crystal counter (X/20)
- Save/load menu (4 slots, character name entry)
- Adventure ending screens
- Credits sequence

## Phase 7 — Multiplayer & Battle Mode

Split-screen racing and arena combat.

- 2-player split-screen with adjusted camera zoom
- Versus mode: player-only races, no CPU opponents
- Cup Race mode: 4 Arcade Cups (Wumpa, Crystal, Nitro, Crash), cumulative points across 4 races
- Battle mode: 7 arenas (4 default + 3 unlocked via Arcade Cup wins)
- Battle rules: Point Limit, Time Limit, Life Limit with configurable thresholds
- Team configurations: 2v1, 2v2, 3v1, 1v1v2
- Battle-only items: Invisibility, Super Engine
- Weapon type customization before battle
- 3-4 player support
- Audio: battle arena music, point scored SFX, life lost SFX, battle results music, team select SFX, Invisibility activate SFX, Super Engine loop

### Assets

**3D Models**
- 3 unlockable battle arenas: Parking Lot, The North Bowl, Lab Basement

**VFX**
- Invisibility shimmer effect
- Super Engine boost glow
- Point scored burst
- Life lost flash

**Audio**
- Battle arena music (per arena or shared set)
- Point scored SFX
- Life lost SFX
- Battle countdown
- Battle results music
- Invisibility activate/deactivate SFX
- Super Engine loop
- Team select SFX

**UI**
- Split-screen viewport layouts (2P horizontal, 3P + spectator quadrant, 4P quadrants)
- Battle scoreboard (points/lives per player)
- Point delta animation (+1, −1)
- Battle timer
- Team select screen
- Battle rules config screen (type, thresholds)
- Weapon type select screen
- Cup Race standings (cumulative points across races)
- Cup Race results screen

## Phase 8 — Content Expansion & Polish

All 18 tracks, full roster, hazards, shortcuts, and final unlockables.

- Remaining 13 characters: Cortex, N. Gin, Dingodile, Polar, Pura, Ripper Roo, Papu Papu, Komodo Joe, Pinstripe, Fake Crash, N. Tropy, Penta Penguin, Nitros Oxide
- Turn and Max engine classes (Polar, Pura, Ripper Roo, Penta Penguin)
- Remaining 17 tracks built out (Roo's Tubes through Turbo Track)
- Track-specific hazards: turtles, fireballs, rolling barrels, mine carts, seals, man-eating plants, rolling armadillos, spiders, mud traps, boulders
- Track shortcuts: Sewer Speedway halfpipe hole, Tiger Temple statue teeth, Blizzard Bluff river jump, Dragon Mines rail hop, Papu's Pyramid jumps, etc.
- Super Turbo Pads on Hub 4 tracks (USF mechanic: no jump on contact, reserve requirement, rapid drain)
- Oxide ghost times per track (beat all Oxide ghosts to unlock Nitros Oxide)
- Unlockable characters: progression-gated via Gem Cups, Time Trial ghosts, and cheat codes
- Cheat code system: character unlocks and gameplay cheats entered at main menu
- Minimap toggle (Triangle): switch between speedometer and top-down track map with racer dots
- Audio: unique track music for all 18 tracks, per-hazard SFX, shortcut ambient sounds, main menu music, cheat code input/activated SFX

### Assets

**3D Models**
- 17 track environments: Roo's Tubes, Mystery Caves, Sewer Speedway, Coco Park, Tiger Temple, Papu's Pyramid, Dingo Canyon, Blizzard Bluff, Dragon Mines, Polar Pass, Tiny Arena, N. Gin Labs, Cortex Castle, Hot Air Skyway, Oxide Station, Slide Coliseum, Turbo Track
- Track hazards: turtles (Mystery Caves), fireballs (Mystery Caves), rolling toxic barrels (Sewer Speedway, N. Gin Labs), mine carts (Dragon Mines), seals (Polar Pass), man-eating plants (Papu's Pyramid), rolling armadillos (Dingo Canyon), spiders (Cortex Castle), rolling boulder (Blizzard Bluff), wooden corkscrew (Dragon Mines)
- Super Turbo Pads
- Shortcut geometry (ramps, alternate paths, breakable walls)
- 13 character driver models: Cortex, N. Gin, Dingodile, Polar, Pura, Ripper Roo, Papu Papu, Komodo Joe, Pinstripe, Fake Crash, N. Tropy, Penta Penguin, Nitros Oxide
- 13 kart color/skin variants (one per character)
- N. Tropy ghost kart model
- Oxide ghost kart model

**VFX**
- USF exhaust effect (larger flame, distinct from Sacred Fire)
- Per-hazard impact effects (fireball burn, barrel splat, mine cart crush, plant chomp, spider web)
- Shortcut-specific particles (wall break debris, hidden ramp glow)
- Confetti (101% best ending)

**Audio**
- Track music — Roo's Tubes
- Track music — Mystery Caves
- Track music — Sewer Speedway
- Track music — Coco Park
- Track music — Tiger Temple
- Track music — Papu's Pyramid
- Track music — Dingo Canyon
- Track music — Blizzard Bluff
- Track music — Dragon Mines
- Track music — Polar Pass
- Track music — Tiny Arena
- Track music — N. Gin Labs
- Track music — Cortex Castle
- Track music — Hot Air Skyway
- Track music — Oxide Station
- Track music — Slide Coliseum
- Track music — Turbo Track
- Per-hazard SFX: turtle bump, fireball whoosh/burn, barrel roll/hit, mine cart rumble/crush, seal bark/bump, plant chomp, armadillo roll/hit, spider drop/web, boulder roll
- Per-character voice lines (select/confirm, celebration, hit reaction) for all 13 remaining characters
- Main menu music
- Cheat code input SFX
- Cheat activated SFX

**UI**
- Per-track minimap (top-down layout with racer dots)
- Main menu screen
- Cheat code entry overlay
