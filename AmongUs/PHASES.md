# Among Us — Phased Implementation Plan

Based on [SPEC.md](SPEC.md) and [docs/tasks.md](docs/tasks.md).

---

## Phase 1 — Networking, The Skeld & Movement

Establishes the multiplayer foundation and the first playable map — players can connect, move around The Skeld, and see each other through the vision system.

- Networking: lobby creation (public matchmaking + private code-based join), host/join flow, player synchronization, region selection (North America, Europe, Asia), 4–15 player lobbies
- The Skeld map: 14 rooms (Cafeteria, Weapons, Navigation, O2, Admin, Electrical, Lower Engine, Upper Engine, Reactor, Security, MedBay, Shields, Communications, Storage), wall collision, corridors, room name zones
- Player character: bean-shaped crewmate, all 18 colors (Red, Blue, Green, Pink, Orange, Yellow, Black, White, Purple, Brown, Cyan, Lime, Maroon, Rose, Banana, Gray, Tan, Coral), color selection in lobby (one per player)
- Movement: 2D top-down at configurable speed (0.5×–3.0×, default 1.0×), no player-to-player collision, wall/geometry collision only
- Vision system: configurable radius per team (Crewmate 0.25×–5.0×, default 1.0×; Impostor 0.25×–5.0×, default 1.5×), wall-occluded line of sight, black fog outside radius/behind walls
- Role assignment at game start: Crewmate or Impostor (1–3 Impostors configurable)
- Spawn: all players at Cafeteria
- All 17 core game settings from §3.1 on the Customize laptop, with full ranges, defaults, and increments
- Keyboard + Mouse input scheme (WASD/arrows movement, E/Space use, Q kill, R report, Tab map, Esc close)

### Assets

**Sprites / 2D Art**
- Crewmate bean character (idle, walk cycle) — 18 color variants
- The Skeld tilemap/environment (14 rooms + corridors)
- The Skeld minimap graphic
- Vision fog overlay

**VFX**
- Fog of war rendering (wall-occluded shadow mask)

**Audio**
- Ambient Skeld background hum
- Footstep silence confirmation (movement is intentionally silent)

**UI**
- Main menu (Host / Join / Private Code)
- Lobby screen (player list, color selection, Customize laptop with settings panel)
- Minimap overlay (room names, toggle via Tab)
- Room name indicator (top-left HUD)

---

## Phase 2 — Tasks, Kill & Vent

Adds the two halves of the core gameplay loop — Crewmates can complete tasks to fill the shared bar, Impostors can kill and use vents.

- Task assignment system: randomly distribute from the map's pool based on configured counts (# Common 0–2, # Long 0–3, # Short 0–5)
- All 18 Skeld tasks per [docs/tasks.md](docs/tasks.md): Fix Wiring, Swipe Card, Align Engine Output, Calibrate Distributor, Chart Course, Clean O2 Filter, Clear Asteroids, Divert Power, Empty Garbage, Prime Shields, Stabilize Steering, Unlock Manifolds, Fuel Engines, Inspect Sample (60s wait), Start Reactor (5-round Simon Says), Submit Scan (10s, one at a time), Upload Data (~8s download + ~8s upload), Empty Chute
- Task bar: shared horizontal progress, fills as any Crewmate completes tasks. Configurable update mode (Always / Meetings / Never)
- Visual task animations when Visual Tasks setting is On: Submit Scan (green beam), Clear Asteroids (cannon blasts visible externally), Prime Shields (shield glow externally), Empty Garbage (trash falls from ship), Empty Chute (leaf debris externally)
- Fake task list for Impostors: identical structure to real assignments, same task UI opens, but completing does not fill bar and visual animations do not trigger
- Kill mechanic: configurable cooldown (10–60s, default 45s) and distance (Short / Normal / Long, default Normal). Killing teleports the Impostor to the victim and plays a kill animation. A body is left at the kill location, visible only within vision range, persists until reported
- Kill cooldown: fixed at 10s at game start regardless of setting; configured value applies after first meeting. Pauses while in a vent or using a surveillance device (§4)
- Vent system on The Skeld — 4 networks: Reactor ↔ Upper Engine ↔ Lower Engine; MedBay ↔ Electrical ↔ Security; Navigation ↔ Weapons ↔ Shields; Admin ↔ Cafeteria (right) ↔ corridor north of Shields. Enter/exit plays visible animation. Player hidden while inside, can navigate to connected vents
- Context-sensitive Use button: task station → minigame, vent → enter/exit, emergency button → call meeting (deferred to Phase 3), surveillance → open device (deferred to Phase 4)

### Assets

**Sprites / 2D Art**
- Dead body sprite (per color)
- Vent sprites (open/closed) — 4 networks' worth on Skeld
- Task station interaction markers (yellow exclamation marks on minimap)

**Animations**
- Kill animation (tongue/knife stab)
- Vent enter/exit animation
- Visual task animations: scan beam, cannon blasts, shield glow, garbage chute, leaf chute

**VFX**
- Task completion sparkle
- Kill blood splatter

**Audio**
- Kill SFX (stab/splat)
- Vent open/close SFX
- Task completion chime
- Individual task interaction sounds (card swipe, wire connect, button press, asteroid shots, reactor beeps, scanner hum)
- Task bar progress tick

**UI**
- Task bar (top of screen, horizontal fill)
- Action buttons (bottom-right): Use, Report, Kill, Vent — context-sensitive visibility
- Sabotage button placeholder (Impostor, opens overlay — sabotage deferred to Phase 4)
- Task list panel (left side, shows assigned tasks with checkmarks)
- 18 task minigame UIs (wiring panel, card swipe, asteroid shooting, reactor memory game, scanner progress, data transfer, etc.)
- Minimap task markers (yellow exclamation marks for incomplete tasks)
- Impostor fake task UI (identical to real tasks)

---

## Phase 3 — Meetings, Voting & Ghost

Completes the social deduction loop — players can report bodies or call meetings, discuss via text chat, vote to eject, and continue as ghosts after death.

- Emergency button in Cafeteria: per-player limit (0–9, default 1), subject to Emergency Cooldown (0–60s, default 15s) after any meeting. Disabled during active sabotages (Phase 4 wires this up); body reports always work
- Body report: pressing Report near a body removes it from the map and triggers a meeting
- Meeting overview: shows who called the meeting (or whose body was found) and who died since the last meeting
- Discussion phase: configurable timer (0–120s, default 15s), no voting allowed. Text chat: Free Chat (unrestricted typing, age-verified accounts) + Quick Chat (preset sentence fragments like "I saw [player] vent in [room]", all accounts)
- Voting phase: configurable timer (0–300s, default 120s; 0 = unlimited). Each living player votes for one player or skips. Voting ends when all vote or timer expires
- Ejection: player with plurality is ejected. Ties and majority-Skip = no ejection. Confirm Ejects setting reveals Impostor status. Animation: thrown out an airlock (Skeld)
- Anonymous Votes setting: hides individual votes, only shows totals
- After meeting: all players return to Cafeteria (not pre-meeting positions). Kill cooldowns reset to configured value
- Ghost system: killed or ejected players become ghosts — invisible to living players, visible to other ghosts. No collision, pass through walls. Unlimited vision (no fog, no wall occlusion). Ghost-only chat invisible to living players. Crewmate ghosts can complete remaining tasks (count toward bar) but visual tasks do not animate. Impostor ghosts can sabotage (wired in Phase 4). Cannot vote, call meetings, report bodies, or fix sabotages. Can Haunt (spectate) any player or ghost
- Win conditions — all four checked after every state change: all Crewmate tasks completed (including ghosts) → Crewmate win; all Impostors ejected → Crewmate win; living Impostors >= living Crewmates → Impostor win; critical sabotage timer expires (wired in Phase 4) → Impostor win
- End-game screen: Victory/Defeat with team color scheme, all players listed by role, ghost task completion shown

### Assets

**Sprites / 2D Art**
- Ghost character sprite (semi-transparent bean, per color)
- Emergency button object (Cafeteria table)
- Ejection scene art: Skeld airlock background, floating crewmate in space

**Animations**
- Ejection animation (Skeld airlock throw)
- Ghost fade-in on death
- "I Voted" sticker appearance

**Audio**
- Emergency button slam SFX
- Body report alarm SFX
- Meeting start jingle
- Discussion ambient tone
- Vote cast click SFX
- Ejection whoosh SFX
- Victory fanfare (Crewmate win)
- Defeat sting (Impostor win)
- Ghost ambient (ethereal background loop)

**UI**
- Meeting overview panel (caller portrait, death portraits)
- Voting panel (player list by name + color, vote buttons, Skip button, vote/anonymous indicators, "I Voted" sticker, timer, dead player "X" overlay)
- Text chat panel (right side — Free Chat text input + Quick Chat category picker)
- Ejection cutscene overlay (name + Impostor reveal text if Confirm Ejects on)
- End-game screen (Victory/Defeat banner, role list, ghost task indicators)
- Ghost Haunt UI (spectate target selector)

---

## Phase 4 — Sabotage & Surveillance

Gives Impostors their disruption toolkit and Crewmates their information-gathering tools — the final piece of the core gameplay loop.

- Sabotage map overlay: Impostor-only, opened via Sabotage button. Shows sabotage targets overlaid on room locations. Ghost Impostors can also sabotage
- Reactor Meltdown: 30s timer (Skeld). Two players must simultaneously hold hand scanners in Reactor to fix. If timer expires → Impostor win
- O2 Depletion: 30s timer (Skeld). Codes must be entered at two keypads (O2 + Admin). If timer expires → Impostor win
- Fix Lights: no timer. Flip switches on a circuit breaker panel in Electrical until all bulbs reach green. While active, Crewmate vision drastically reduced; Impostor vision unaffected
- Fix Communications: no timer. Adjust dial to match waveforms in Communications. While active, task list displays "Comms Sabotaged", task bar progress hidden, all surveillance systems disabled
- Close Doors: 10s auto-reopen. Locks all doors to a chosen room, blocking entry/exit. Skeld doors auto-reopen only (no manual open)
- Sabotage cooldowns: non-door sabotages share a 30s cooldown starting when resolved. Door sabotages have independent cooldowns starting when triggered. Multiple doors can close simultaneously. Non-door sabotages disabled while a door is actively closing
- Emergency button disabled during any active sabotage — body reports still work (wires up Phase 3 placeholder)
- Security Cameras: fixed views in Security room — Y-corridor, MedBay hallway, Security-Reactor corridor, Navigation top. A blinking red light on in-world camera objects indicates someone is watching. Kill cooldown pauses while viewing
- Admin Table: in Admin room. Shows map schematic with anonymous yellow icons per room indicating player count. No exact positions, names, or hallway locations. Updates when players change rooms. Kill cooldown pauses while viewing

### Assets

**Sprites / 2D Art**
- Sabotage map overlay graphic (Skeld schematic with sabotage buttons)
- Reactor hand-scanner panels (left/right)
- O2 keypad panels (two locations)
- Electrical circuit breaker panel (Lights fix)
- Communications dial/waveform panel (Comms fix)
- Door closed/locked sprite overlays per room
- Security camera in-world objects (with red blink state)
- Admin Table map display (room silhouettes + yellow player icons)

**Animations**
- Sabotage alarm flash (screen edge red pulse for critical sabotages)
- Door lock/unlock animation
- Camera red light blink

**VFX**
- Reduced Crewmate vision effect (Lights sabotaged — smaller fog radius)

**Audio**
- Reactor Meltdown alarm (looping, increasing urgency as timer drops)
- O2 Depletion alarm (looping, increasing urgency)
- Lights sabotage flicker SFX
- Communications static/interference SFX
- Door slam shut SFX
- Door reopen SFX
- Sabotage trigger SFX (Impostor confirms sabotage)
- Sabotage resolved chime
- Camera view switch click
- Admin Table ambient hum

**UI**
- Sabotage map overlay (full-screen, room-positioned buttons)
- Reactor fix minigame (two hand-scanner hold panels)
- O2 fix minigame (keypad code entry at two locations)
- Lights fix minigame (switch panel with bulb indicators)
- Communications fix minigame (dial + waveform matching)
- Security camera viewer (4-panel split view, exit button)
- Admin Table viewer (map schematic with live room occupancy)
- Sabotage countdown timer (top of screen during critical sabotages)
- "Comms Sabotaged" task list replacement text

---

**Vertical slice checkpoint — A complete Among Us match on The Skeld: 4–15 networked players, Crewmate tasks and task bar, Impostor kill/vent/sabotage, meetings with text chat and voting, ejection, ghosts, surveillance (cameras + admin table), all four win conditions, and full lobby settings.**

---

## Phase 5 — Roles

Adds the seven special roles that deepen the social deduction with unique abilities for both teams.

- Role assignment system: host configures each role's Max Count (0–15) and Chance (0–100%) in lobby settings. Roles distributed at game start alongside base Crewmate/Impostor assignment
- **Engineer** (Crewmate): can use vents like an Impostor. Vent Use Cooldown (10–60s, default 30s), Max Time In Vents (0–60s, default 15s). Vent button appears when near a vent (same as Impostor)
- **Scientist** (Crewmate): can open a portable Vitals panel from anywhere on the map. Vitals Display Cooldown (10–60s, default 15s), Battery Duration (5–30s, default 10s). Shows all players' alive/dead/disconnected status
- **Guardian Angel** (Crewmate, post-death): after dying, can place a temporary shield on a living Crewmate that blocks one kill attempt. Protect Cooldown (10–120s, default 35s), Protection Duration (10–30s, default 10s), Impostors Can See Protect (On/Off, default On)
- **Tracker** (Crewmate): can track one player at a time — tracked player's position appears as an arrow/icon on the Tracker's minimap. Tracking Cooldown and Duration (configurable)
- **Noisemaker** (Crewmate): on death, emits a visible and audible alert at the kill location revealing where the murder happened to all living players. Alert Duration (configurable)
- **Shapeshifter** (Impostor): can disguise as any other living player for a limited time. Leaves behind visible evidence (a discarded skin) at the shift location. Shapeshifting Cooldown and Duration (configurable), Leave Shapeshifting Evidence (On/Off)
- **Phantom** (Impostor): can temporarily turn invisible. Vanish/reappear animations are visible to nearby players but the Phantom is invisible in between. Invisibility Cooldown and Duration (configurable). Cannot kill while invisible

### Assets

**Sprites / 2D Art**
- Guardian Angel shield bubble (on protected Crewmate)
- Shapeshifter evidence skin (discarded on ground)
- Tracker arrow/icon (on minimap for tracked target)
- Noisemaker death alert indicator (directional ping at kill location)

**Animations**
- Guardian Angel shield placement animation
- Guardian Angel shield block animation (kill absorbed)
- Shapeshifter morph animation (shift into target appearance)
- Shapeshifter revert animation (return to true form)
- Phantom vanish puff animation
- Phantom reappear puff animation
- Noisemaker death alert pulse animation

**VFX**
- Shield shimmer on protected Crewmate
- Phantom semi-transparency during vanish/reappear

**Audio**
- Guardian Angel shield placement SFX
- Guardian Angel shield block SFX (kill absorbed)
- Shapeshifter morph SFX
- Shapeshifter revert SFX
- Phantom vanish SFX
- Phantom reappear SFX
- Noisemaker death alarm SFX (audible to all players)
- Scientist portable Vitals open/close SFX

**UI**
- Role settings panel in lobby (per-role Max Count + Chance sliders)
- Scientist portable Vitals overlay (same layout as Polus Vitals — Phase 7 reuses this for the physical device)
- Tracker minimap arrow overlay
- Role reveal splash at game start (shows assigned role + brief description)

---

## Phase 6 — MIRA HQ

Adds the smallest map with its unique single-loop vent network and Door Log surveillance system.

- MIRA HQ map: 14 rooms (Cafeteria, Balcony, Storage, Communications, MedBay, Locker Room, Decontamination, Laboratory, Reactor, Launchpad, Admin, Office, Greenhouse, Hallway)
- Emergency button in Cafeteria
- Vent network: all vents interconnected into one single loop — Impostor/Engineer can travel from any vent to any other vent
- Sabotages: Reactor Meltdown (45s timer), O2 Depletion (45s timer), Fix Lights (Office), Fix Communications (enter codes at two stations). No door sabotage on MIRA HQ
- Surveillance: Admin Table (anonymous room occupancy), Door Log in Communications — records last 20 players passing through 3 Skywalk sensors (North/Blue, Southwest/Green, Southeast/Red), logs player color + sensor, 5s per-sensor per-player cooldown
- All 18 MIRA HQ tasks: Fix Wiring (reused), Enter ID Code, Assemble Artifact, Buy Beverage, Chart Course, Clean O2 Filter, Clear Asteroids, Divert Power, Measure Weather, Prime Shields, Process Data, Run Diagnostics, Sort Samples, Submit Scan (visual), Start Reactor, Unlock Manifolds, Upload Data, Water Plants
- Ejection animation: dropped from the HQ
- Map selection added to lobby (The Skeld / MIRA HQ)

### Assets

**Sprites / 2D Art**
- MIRA HQ tilemap/environment (14 rooms + corridors, sky headquarters theme)
- MIRA HQ minimap graphic
- Door Log display panel
- Door Log sensor indicators (3 colored markers on Skywalk)

**Animations**
- Ejection animation: MIRA HQ drop

**Audio**
- MIRA HQ ambient background (high-altitude wind/hum)
- Door Log sensor trigger SFX
- MIRA-specific task interaction sounds (vending machine, weather station)

**UI**
- 10 new task minigame UIs (Enter ID Code keypad, Assemble Artifact drag, Buy Beverage vending, Measure Weather button, Process Data progress, Run Diagnostics select, Sort Samples bins, Water Plants watering can — Chart Course and Clean O2 Filter reuse Skeld variants)
- Door Log viewer (scrollable log of sensor entries with color + direction)
- Map selection dropdown in lobby

---

## Phase 7 — Polus

Adds the planetary outpost with Decontamination airlocks, the Vitals surveillance device, and Seismic Stabilizers.

- Polus map: 15 rooms (Office, Admin, Communications, Electrical, O2, Weapons, Boiler Room, Security, Dropship, Laboratory, MedBay, Specimen Room, Storage, Decontamination, Outside). Two Decontamination airlocks slow movement between areas
- Emergency button in Office
- Vent network: 12 vents, 4 networks (Security ↔ Electrical ↔ O2 outside; Admin ↔ Laboratory outside; Office right ↔ Storage ↔ Communications outside; Laboratory ↔ above Electrical)
- Sabotages: Seismic Stabilizers (60s timer, two-location simultaneous fix — functionally equivalent to Reactor), Fix Lights (Electrical), Fix Communications (adjust dial outdoors), Close Doors
- Surveillance: Security Cameras (6 fixed views), Admin Table, Vitals in Office — shows each player's status: Alive (green heartbeat), Dead (red flatline, killed since last meeting), Disconnected (gray), Dead from prior round (gray). Kill cooldown pauses while viewing. Scientist portable Vitals (Phase 5) reuses this UI
- All 22 Polus tasks: Fix Wiring (reused), Insert Keys, Scan Boarding Pass, Align Telescope, Chart Course, Clear Asteroids, Fill Canisters, Fix Weather Node, Monitor Tree, Record Temperature, Repair Drill, Reboot WiFi (60s wait), Inspect Sample (60s wait), Divert Power, Open Waterways, Replace Water Jug, Start Reactor, Store Artifacts, Submit Scan (visual), Fuel Engines, Unlock Manifolds, Upload Data
- Ejection animation: thrown into lava

### Assets

**Sprites / 2D Art**
- Polus tilemap/environment (15 rooms + outside areas, planetary outpost theme)
- Polus minimap graphic
- Decontamination airlock door/chamber sprites
- Vitals monitor panel display
- Seismic Stabilizer fix panels (two locations)

**Animations**
- Decontamination airlock sequence (doors close, spray, doors open)
- Ejection animation: Polus lava throw
- Vitals heartbeat/flatline/disconnect animations per player slot

**Audio**
- Polus ambient background (wind, outdoor planet atmosphere)
- Decontamination airlock hiss SFX
- Seismic Stabilizer alarm (looping, increasing urgency)
- Vitals heartbeat beep / flatline tone
- Polus-specific task sounds (key turn, boarding pass scan, telescope rotate, valve turn, drill repair, WiFi reboot)

**UI**
- Vitals viewer (player list with heartbeat/flatline/disconnect indicators)
- Seismic Stabilizer fix minigame (hold panels at two locations)
- 12 new task minigame UIs (Insert Keys, Scan Boarding Pass, Align Telescope, Fill Canisters, Fix Weather Node maze, Monitor Tree, Record Temperature dial, Repair Drill, Reboot WiFi switch, Open Waterways valves, Replace Water Jug carry, Store Artifacts shelves — others reuse existing variants)

---

## Phase 8 — The Airship

Adds the largest map with ladders, moving platforms, spawn selection, and the most tasks of any map.

- The Airship map: 21 rooms (Cockpit, Engine Room, Main Hall, Gap Room, Meeting Room, Electrical, Armory, Kitchen, Security, Cargo Bay, Ventilation, Medical, Lounge, Records, Brig, Communications, Hall of Portraits, Viewing Deck, Vault, Showers, Outside)
- Ladders between floors (vertical traversal)
- Moving platform across Gap Room (automatic ferry between sides)
- Spawn location selection: after every meeting (not just game start), players choose one of three spawn locations before resuming play
- Emergency button in Meeting Room
- Vent network: 12 vents, 4 networks (Cockpit ↔ Vault + Cockpit ↔ Viewing Deck, Vault/Viewing Deck not directly connected; Engine Room ↔ Kitchen ↔ Main Hall south; Gap Room west ↔ Gap Room east ↔ Main Hall north; Records ↔ Showers ↔ Cargo Bay)
- Sabotages: Avert Crash Course (90s timer, two-location fix — functionally equivalent to Reactor), Fix Lights (fixable at Gap Room, Viewing Deck, or Cargo Bay), Fix Communications (dial matching), Close Doors (Crewmates pull levers to open)
- Surveillance: Security Cameras (6 views: Engine Room, Vault, Records, Security, Cargo Bay, Meeting Room), Admin Table in Cockpit
- No visual tasks on The Airship
- All 23 Airship tasks: Fix Wiring (reused), Enter ID Code, Calibrate Distributor, Chart Course, Clean Toilet, Decontaminate, Dress Mannequin, Divert Power, Empty Garbage, Pick Up Towels, Polish Ruby, Put Away Pistols, Put Away Rifles, Sort Records, Stabilize Steering, Unlock Safe, Upload Data, Develop Photos, Drain Fuel, Make Burger, Reset Breakers, Rewind Tapes, Start Fans
- Ejection animation: thrown overboard

### Assets

**Sprites / 2D Art**
- The Airship tilemap/environment (21 rooms + corridors, massive airship theme)
- The Airship minimap graphic
- Ladder sprites (vertical connection points)
- Moving platform sprite (Gap Room ferry)
- Avert Crash Course fix panels (two locations)
- Door lever sprites (manual open for Crewmates)

**Animations**
- Ladder climb animation
- Moving platform transit animation
- Ejection animation: Airship overboard throw

**Audio**
- The Airship ambient background (engine drone, wind)
- Ladder climb SFX
- Moving platform mechanical SFX
- Crash Course alarm (looping, increasing urgency)
- Airship-specific task sounds (toilet plunge, mannequin dress, gem polish, gun rack, safe dial, photo develop, tape rewind, fan spin-up, burger sizzle)

**UI**
- Spawn location selection screen (three options presented after each meeting)
- 14 new task minigame UIs (Clean Toilet, Decontaminate wait, Dress Mannequin, Pick Up Towels, Polish Ruby, Put Away Pistols, Put Away Rifles, Sort Records, Unlock Safe dial, Develop Photos, Drain Fuel, Make Burger, Reset Breakers, Rewind Tapes, Start Fans — others reuse existing variants)
- Avert Crash Course fix minigame (two-location interaction)

---

## Phase 9 — The Fungle

Adds the mushroom island with its unique carnivorous-plant vents, Purple Haze obscuring plants, Mushroom Mixup sabotage, zipline, and telescope surveillance.

- The Fungle map: 18 rooms (Beach, Cafeteria, Communications, Dock, Dropship, Greenhouse, Jungle, Kitchen, Laboratory, Lookout, Meeting Room, Mining Pit, Reactor, Splash Zone, Storage, The Cliffs, The Dorm, Upper Engine)
- Vents are carnivorous plants that eat and spit out players (same mechanics as standard vents, unique visual presentation)
- Purple Haze plants in the Jungle: emit pink smoke that temporarily obscures nearby players when stepped on
- Zipline connecting Storage and Communications (traversal shortcut)
- Emergency button in Meeting Room
- Vent network: 10 vents, 3 networks (Cafeteria exterior ↔ Storage alcove ↔ Laboratory ↔ Splash Zone loop; Kitchen ↔ Jungle two connection points; Reactor ↔ Communications ↔ Lookout)
- Sabotages: Reactor Meltdown (60s timer), Mushroom Mixup (10s self-resolving — randomizes all player appearances including color/hat/skin, hides names, cannot be fixed, expires automatically), Fix Communications (enter codes), Close Doors (Crewmates click door switches to open). No Lights sabotage on The Fungle
- Surveillance: Telescope in Lookout — manually controlled panoramic view covering most of the map except mid-right cliffs. No cameras, admin table, or vitals
- All 18 Fungle tasks: Fix Wiring (reused), Run Diagnostics, Calibrate Distributor, Crank Generator, Collect Samples, Grind Gems, Help Critter (~30s egg incubation), Mine Ores (rhythm-based), Monitor Mushroom, Play Video Game (retro mini-game), Polish Gem, Retrieve Food, Test Samples, Collect Stick (beach → campfire marshmallow roast), Hoist Supplies (multi-stage cliff climb), Repair Drill, Submit Scan (visual, at Dropship), Upload Data
- Ejection animation: launched by mushroom

### Assets

**Sprites / 2D Art**
- The Fungle tilemap/environment (18 rooms, mushroom island/jungle theme)
- The Fungle minimap graphic
- Carnivorous plant vent sprites (open mouth/closed)
- Purple Haze plant sprites (Jungle area)
- Zipline cable + platform sprites
- Telescope device sprite (Lookout)
- Mushroom Mixup randomized appearance overlays (scrambled colors + cosmetics for all players)

**Animations**
- Carnivorous plant eat/spit animation (vent enter/exit variant)
- Purple Haze pink smoke puff on trigger
- Zipline ride animation
- Telescope pan animation
- Mushroom Mixup activation (purple haze screen wash, appearance scramble, 10s revert)
- Ejection animation: Fungle mushroom launch

**VFX**
- Purple Haze smoke cloud (Jungle area)
- Mushroom Mixup screen-wide purple haze overlay

**Audio**
- The Fungle ambient background (jungle sounds, insects, mushroom spores)
- Carnivorous plant chomp/spit SFX
- Purple Haze puff SFX
- Zipline whoosh SFX
- Telescope pan mechanical SFX
- Mushroom Mixup activation SFX (mushroom burst + confusion tone)
- Mushroom Mixup end SFX (revert chime)
- Fungle-specific task sounds (gem grind, ore rhythm hits, critter egg warmth, campfire crackle, generator crank, video game beeps, drill repair)

**UI**
- Telescope viewer (manually panned panoramic view, exit button)
- 13 new task minigame UIs (Run Diagnostics, Crank Generator, Collect Samples, Grind Gems, Help Critter incubation, Mine Ores rhythm, Monitor Mushroom, Play Video Game retro, Polish Gem, Retrieve Food selection, Test Samples, Collect Stick + marshmallow roast, Hoist Supplies multi-stage — others reuse existing variants)

---

## Phase 10 — Hide n Seek Mode

Adds the official alternative game mode where one revealed Seeker hunts Hiders who can vent and complete tasks to accelerate the timer.

- Game mode selection in lobby: Classic / Hide n Seek
- Seeker (Impostor): identity revealed to all at game start with permanent teeth/tongue animation. Cannot vent. Cannot sabotage. Receives speed boost during Final Hide
- Hiders (Crewmates): can use vents (limited uses per game, short duration per use). Complete tasks to reduce the countdown timer
- Countdown timer replaces the task bar — Hiders win if it reaches zero
- Flashlight mode (optional): Hiders have a narrow cone of vision instead of a full circle
- Final Hide phase: triggers when the main timer expires. Seeker gains speed boost, tasks disabled, timer bar turns red. Pings emitted every ~6 seconds revealing approximate positions of all living Hiders on the Seeker's screen
- Win conditions: Seeker wins if all Hiders eliminated; Hiders win if at least one survives Final Hide
- Hide n Seek settings in lobby configuration (configurable alongside existing game settings)
- Playable on all five maps

### Assets

**Sprites / 2D Art**
- Seeker teeth/tongue overlay on character sprite
- Hider ping indicator (location pulse visible to Seeker)

**Animations**
- Seeker reveal animation (game start teeth/tongue morph)
- Ping pulse animation (Final Hide phase, every ~6s)
- Timer bar transition to red (Final Hide activation)

**VFX**
- Flashlight cone vision effect (narrow cone replacing circular fog)
- Final Hide red screen-edge vignette

**Audio**
- Seeker reveal sting (game start)
- Countdown timer tick (ambient urgency)
- Final Hide activation alarm
- Ping SFX (every ~6s during Final Hide)
- Hider victory fanfare
- Seeker victory sting

**UI**
- Game mode selector in lobby (Classic / Hide n Seek)
- Hide n Seek countdown timer bar (replaces task bar, turns red during Final Hide)
- Hide n Seek settings panel in lobby
- Hider vent charge indicator (remaining uses)

---

## Phase 11 — Cosmetics, Currency & Polish

Adds the full cosmetic customization system, economy, and final presentation polish.

- Cosmetic types: Hats (worn on head), Skins (body outfits), Visors (face accessories), Pets (follow player, remain at death spot when owner killed), Nameplates (decorative borders on voting panel)
- Starter cosmetic set: ~5 hats, ~3 skins, ~3 visors, ~2 pets, ~3 nameplates
- Cosmetic equip in lobby customization screen
- Currency: Beans (earned through gameplay — tasks completed, wins), Stars (premium real-money purchase), Pods (earned within a specific Cosmicube, non-transferable)
- Cosmicube progression trees: purchased with Beans or Stars, earn Pods by playing, spend Pods to unlock cosmetics within each Cube's branching path
- Shop: accessible from main menu, purchase cosmetics and Cosmicubes with Beans or Stars
- Player stats tracking: games played, wins, tasks completed, kills — persisted via account/cloud save
- Account persistence: cosmetics, settings, unlocks, stats saved across sessions
- Pet behavior: follows owner during gameplay, stays at death location when owner is killed or ejected
- Nameplate display: visible on voting panel during meetings

### Assets

**Sprites / 2D Art**
- Starter hat sprites (~5: e.g., party hat, chef hat, crown, flower, brain slug)
- Starter skin sprites (~3: e.g., astronaut suit, mechanic outfit, military uniform)
- Starter visor sprites (~3: e.g., glasses, plague mask, monobrow)
- Starter pet sprites with walk/idle animations (~2: e.g., mini crewmate, Brainslug)
- Starter nameplate borders (~3 designs)
- Cosmicube tree layout graphics
- Shop item thumbnails
- Currency icons (Beans, Stars, Pods)

**Animations**
- Pet walk/idle cycle (follows owner)
- Pet left-behind idle at death location
- Cosmicube node unlock animation
- Shop purchase confirmation animation

**Audio**
- Shop browse SFX
- Purchase confirmation SFX
- Cosmicube unlock SFX
- Pet ambient sounds (per pet type)
- Cosmetic equip SFX

**UI**
- Cosmetic customization screen in lobby (hat/skin/visor/pet/nameplate equip slots with preview)
- Shop screen (item grid, Cosmicube selection, currency balances, purchase flow)
- Cosmicube viewer (branching tree with locked/unlocked nodes, Pod spending)
- Stats screen (games played, wins, tasks completed, kills, etc.)
- Nameplate rendering on meeting voting panel
