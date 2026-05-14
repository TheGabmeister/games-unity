# Among Us — Gameplay Systems Spec

Among Us, PC / Mobile / Console, 2018 (InnerSloth). Social deduction game where Crewmates complete tasks aboard a shared environment while Impostors secretly sabotage and eliminate them. This spec covers the base game through the current live version (including all five maps, roles, and Hide n Seek mode).

---

## 1. Core Gameplay Loop

A lobby of 4–15 players is assigned roles: most become **Crewmates**, 1–3 become **Impostors**. The game takes place on a closed map with rooms, corridors, and interactive stations.

### 1.1 Crewmate Objective

- Complete all assigned tasks (fills a shared **task bar**) — instant Crewmate victory when the bar reaches 100%.
- Identify and vote out all Impostors via emergency meetings.

### 1.2 Impostor Objective

- **Kill** Crewmates until Impostors equal or outnumber living Crewmates — at parity, Impostors can no longer be outvoted, triggering an Impostor victory.
- **Sabotage** critical systems (Reactor Meltdown, O2 Depletion, etc.) — if the countdown expires unfixed, Impostors win instantly.

### 1.3 Round Flow

1. **Free roam** — Crewmates do tasks; Impostors fake tasks, kill, sabotage, and vent.
2. **Body discovered or emergency button pressed** — all players teleport to the meeting table.
3. **Discussion phase** — text chat (or external voice chat); no voting allowed during this timer.
4. **Voting phase** — each living player votes to eject one player or skips. The player with a plurality is ejected. Ties and majority-skip result in no ejection.
5. **Return to free roam** — kill cooldowns reset; the cycle repeats.

### 1.4 Kill Mechanics

- Kill has a configurable cooldown and distance range (§3).
- Killing **teleports** the Impostor to the victim's position instantly and plays a brief kill animation (tongue/knife stab).
- A body is left at the kill location. It persists until reported or the game ends.
- Bodies are only visible to players within their vision range — a body in an unvisited room can go undiscovered indefinitely.
- Reporting a body removes it from the map and triggers a meeting (§9).
- Kill cooldown **pauses** while the Impostor is: inside a vent, watching security cameras, viewing the admin table, checking vitals, or viewing door logs.
- At the very start of a game, the kill cooldown is fixed at 10 seconds regardless of the configured value. After the first meeting, the configured cooldown applies.

### 1.5 Vent System

- Impostors (and Engineers, §4) can enter and travel through vents — openings in the floor connected in map-specific networks (§5).
- Entering or exiting a vent plays a visible animation that nearby players can witness, making venting risky near others.
- While inside a vent, the player is hidden from all living players and can navigate to any connected vent in the same network.
- Kill cooldown pauses while inside a vent.

### 1.6 Vision System

- Vision defines a maximum radius of visibility around each player.
- Within that radius, walls and map geometry occlude line of sight — players cannot see through walls into adjacent rooms.
- Areas outside the vision radius or behind walls render as black fog.
- Crewmate and Impostor vision radii are independent settings (§3).
- Sabotaging Lights drastically reduces the Crewmate vision radius but does not affect Impostor vision.

### 1.7 Movement & Physics

- Players move in 2D at a configurable speed (§3).
- No player-to-player collision — characters pass through each other freely.
- Players collide with walls and map geometry only.
- At the start of a game, all players spawn together at the meeting table area.
- After each meeting, all players return to the meeting table area (not their pre-meeting positions). Exception: on The Airship, players choose one of three spawn locations after each meeting.

### 1.8 Win Conditions Summary

| Outcome | Trigger |
|---|---|
| Crewmate Victory (Tasks) | All Crewmate tasks completed (including ghosts) |
| Crewmate Victory (Vote) | All Impostors ejected |
| Impostor Victory (Kill) | Living Impostors >= living Crewmates |
| Impostor Victory (Sabotage) | Critical sabotage timer expires |

---

## 2. Controls & Input

### 2.1 Platforms

| Platform | Release |
|---|---|
| iOS / Android | June 15, 2018 |
| Windows (Steam) | November 16, 2018 |
| Nintendo Switch | December 15, 2020 |
| PS4 / PS5 / Xbox One / Xbox Series X\|S | December 14, 2021 |

Full crossplay across all platforms.

### 2.2 Input Schemes

**Mobile** — two modes selectable in settings:
- **Joystick** — virtual analog stick (bottom-left); action buttons (right side). Joystick size is configurable.
- **Touch** — tap-to-move; tap objects to interact.

**PC** — two modes selectable in settings:
- **Mouse Only** (default) — click-to-move, click action buttons on screen. Mirrors mobile touch.
- **Mouse + Keyboard**:

| Key | Action |
|---|---|
| W / A / S / D | Move up / left / down / right |
| Arrow Keys | Move (alternate) |
| E or Space | Use / Interact / Vent / Sabotage |
| Q | Kill (Impostor only, when in range) |
| R | Report dead body |
| Tab | Open map |
| Esc | Close menus / options |
| Alt + Enter | Toggle fullscreen |

**Console** — standard gamepad controls with analog stick movement and face-button actions.

### 2.3 Chat Systems

- **Free Chat** — unrestricted text input during meetings. Requires an account with verified age/email. Not available to accounts flagged as under 13.
- **Quick Chat** — a structured preset message system. Players select from categorized sentence fragments (e.g., "I saw [player] vent in [room]") to compose messages without typing. Available to all accounts regardless of age verification.
- No built-in voice chat — players use external tools (Discord, etc.) for voice communication.

### 2.4 Context-Sensitive Actions

The Use button changes contextually:
- Near a task station → opens the task minigame
- Near a vent (Impostor / Engineer) → enter vent
- Inside a vent → exit vent
- On the sabotage map (Impostor) → trigger selected sabotage
- Near emergency button → call meeting
- Near a surveillance device → open cameras / admin / vitals / door log

---

## 3. Game Settings (Lobby Options)

The lobby host configures all settings on the Customize laptop before starting. Settings are divided into Game, Role, and Hide n Seek categories.

### 3.1 Core Game Settings

| Setting | Range | Default | Increment |
|---|---|---|---|
| # Impostors | 1–3 | 1 | 1 |
| Confirm Ejects | On / Off | On | — |
| # Emergency Meetings (per player) | 0–9 | 1 | 1 |
| Emergency Cooldown | 0–60s | 15s | 5s |
| Discussion Time | 0–120s | 15s | 15s |
| Voting Time | 0–300s | 120s | 15s |
| Anonymous Votes | On / Off | Off | — |
| Player Speed | 0.5×–3.0× | 1.0× | 0.25× |
| Crewmate Vision | 0.25×–5.0× | 1.0× | 0.25× |
| Impostor Vision | 0.25×–5.0× | 1.5× | 0.25× |
| Kill Cooldown | 10–60s | 45s | 2.5s |
| Kill Distance | Short / Normal / Long | Normal | — |
| Task Bar Updates | Always / Meetings / Never | Always | — |
| Visual Tasks | On / Off | On | — |
| # Common Tasks | 0–2 | 1 | 1 |
| # Long Tasks | 0–3 | 1 | 1 |
| # Short Tasks | 0–5 | 2 | 1 |

---

## 4. Roles

Roles were introduced in a November 2021 update. The host can configure how many of each role appear per game and adjust role-specific settings. Each role has a **Max Count** (0–15) and a **Chance** (0–100%) setting.

### 4.1 Crewmate Roles

| Role | Ability | Key Settings |
|---|---|---|
| **Crewmate** (base) | Complete tasks, report bodies, call meetings, vote | — |
| **Engineer** | Can use vents like an Impostor | Vent Use Cooldown (10–60s, default 30s); Max Time In Vents (0–60s, default 15s) |
| **Scientist** | Can view a portable Vitals panel from anywhere on the map | Vitals Display Cooldown (10–60s, default 15s); Battery Duration (5–30s, default 10s) |
| **Guardian Angel** | After death, can place a temporary protective shield on a living Crewmate that blocks one kill attempt | Protect Cooldown (10–120s, default 35s); Protection Duration (10–30s, default 10s); Impostors Can See Protect (On/Off, default On) |
| **Tracker** | Can track one player at a time — tracked player's position appears on the Tracker's map | Tracking Cooldown (configurable); Tracking Duration (configurable) |
| **Noisemaker** | On death, emits a visible and audible alert at the kill location, revealing where the murder happened to all players | Alert Duration (configurable) |

### 4.2 Impostor Roles

| Role | Ability | Key Settings |
|---|---|---|
| **Impostor** (base) | Kill, sabotage, vent, fake tasks | — |
| **Shapeshifter** | Can shapeshift to look like any other player for a limited time. Leaves behind visible evidence (a skin of the mimicked player) at the shift location | Shapeshifting Cooldown (configurable); Shapeshifting Duration (configurable); Leave Shapeshifting Evidence (On/Off) |
| **Phantom** | Can temporarily turn invisible. Other players may see the vanish/reappear animation but cannot see the Phantom while invisible | Invisibility Cooldown (configurable); Invisibility Duration (configurable) |

### 4.3 Ghost

When a player is killed or ejected, they become a **Ghost**:
- Invisible to all living players; visible only to other ghosts.
- No collision — can pass through walls and obstacles.
- Unlimited vision — unaffected by Lights sabotage or walls.
- Ghost-only chat — messages are invisible to living players.
- **Crewmate ghosts** can still complete their remaining tasks (which count toward the task bar). However, ghost visual tasks do not produce visible animations.
- **Impostor ghosts** can still use Sabotage.
- Ghosts **cannot**: vote, call meetings, report bodies, fix sabotages, or interact with living players.
- Ghosts can **Haunt** (spectate) any other player, including other ghosts.

---

## 5. World Structure — Maps

Among Us has five maps. Each map has a unique layout, room set, vent network, sabotage options, and surveillance systems. Per-map sabotage and surveillance summaries below are brief — see §7 and §8 for full mechanics.

### 5.1 The Skeld

- **Setting**: Spaceship in deep space.
- **Size**: Medium (4th largest).
- **Rooms (14)**: Cafeteria, Weapons, Navigation, O2, Admin, Electrical, Lower Engine, Upper Engine, Reactor, Security, MedBay, Shields, Communications, Storage.
- **Emergency Button**: Cafeteria.
- **Visual Tasks**: Submit Scan (MedBay), Prime Shields (Shields), Clear Asteroids (Weapons), Empty Garbage (Storage/Cafeteria), Empty Chute (O2).

**Vent Network (4 networks)**:

| Network | Connected Rooms |
|---|---|
| 1 | Reactor ↔ Upper Engine ↔ Lower Engine |
| 2 | MedBay ↔ Electrical ↔ Security |
| 3 | Navigation ↔ Weapons ↔ Shields |
| 4 | Admin ↔ Cafeteria (right) ↔ Corridor north of Shields |

**Sabotages**: Reactor Meltdown (30s timer), O2 Depletion (30s timer), Fix Lights, Fix Communications, Close Doors (10s duration per room).

**Surveillance**: Security Cameras (4 fixed views), Admin Table (anonymous room occupancy).

### 5.2 MIRA HQ

- **Setting**: High-altitude sky headquarters above the clouds.
- **Size**: Smallest map.
- **Rooms (14)**: Cafeteria, Balcony, Storage, Communications, MedBay, Locker Room, Decontamination, Laboratory, Reactor, Launchpad, Admin, Office, Greenhouse, Hallway.
- **Emergency Button**: Cafeteria.
- **Visual Tasks**: Submit Scan (MedBay).

**Vent Network**: All vents are interconnected into **one single loop** — an Impostor can travel from any vent to any other vent on the map.

**Sabotages**: Reactor Meltdown (45s timer), O2 Depletion (45s timer), Fix Lights, Fix Communications. **No door sabotage** on MIRA HQ.

**Surveillance**: Admin Table (anonymous room occupancy), Door Log (tracks the last 20 players passing through 3 sensors on the Skywalk corridor: North/Blue, Southwest/Green, Southeast/Red). No security cameras.

### 5.3 Polus

- **Setting**: Planetary outpost on the surface of Polus.
- **Size**: 3rd largest.
- **Rooms (15)**: Office, Admin, Communications, Electrical, O2, Weapons, Boiler Room, Security, Dropship, Laboratory, MedBay, Specimen Room, Storage, Decontamination, Outside.
- **Emergency Button**: Office.
- **Visual Tasks**: Submit Scan (MedBay).
- **Unique Feature**: Two Decontamination airlocks that slow player movement between areas.

**Vent Network (12 vents, 4 networks)**:

| Network | Connected Rooms |
|---|---|
| 1 | Security ↔ Electrical ↔ O2 (outside) |
| 2 | Admin ↔ Laboratory (outside) |
| 3 | Office (right) ↔ Storage ↔ Communications (outside) |
| 4 | Laboratory ↔ above Electrical |

**Sabotages**: Seismic Stabilizers (60s timer — requires fixing at two locations), Fix Lights, Fix Communications, Close Doors.

**Surveillance**: Security Cameras (6 fixed views), Admin Table, Vitals (Office — shows alive/dead/disconnected status for each player).

### 5.4 The Airship

- **Setting**: Massive airship (Henry Stickmin universe).
- **Size**: Largest map.
- **Rooms (21)**: Cockpit, Engine Room, Main Hall, Gap Room, Meeting Room, Electrical, Armory, Kitchen, Security, Cargo Bay, Ventilation, Medical, Lounge, Records, Brig, Communications, Hall of Portraits, Viewing Deck, Vault, Showers, Outside.
- **Emergency Button**: Meeting Room.
- **Visual Tasks**: None (the only map with no visual tasks).
- **Unique Features**: Ladders between floors, a moving platform across Gap Room. After every meeting (not just game start), players choose one of three spawn locations before resuming play.

**Vent Network (12 vents, 4 networks)**:

| Network | Connected Rooms |
|---|---|
| 1 | Cockpit ↔ Vault, Cockpit ↔ Viewing Deck (Vault and Viewing Deck not directly connected) |
| 2 | Engine Room ↔ Kitchen ↔ Main Hall (south) |
| 3 | Gap Room (west) ↔ Gap Room (east) ↔ Main Hall (north) |
| 4 | Records ↔ Showers ↔ Cargo Bay |

**Sabotages**: Avert Crash Course (90s timer — two locations), Fix Lights (fixable at Gap Room, Viewing Deck, or Cargo Bay), Fix Communications, Close Doors.

**Surveillance**: Security Cameras (views Engine Room, Vault, Records, Security, Cargo Bay, Meeting Room), Admin Table (Cockpit).

### 5.5 The Fungle

- **Setting**: Mushroom-covered island/jungle.
- **Size**: 2nd largest.
- **Rooms (18)**: Beach, Cafeteria, Communications, Dock, Dropship, Greenhouse, Jungle, Kitchen, Laboratory, Lookout, Meeting Room, Mining Pit, Reactor, Splash Zone, Storage, The Cliffs, The Dorm, Upper Engine.
- **Emergency Button**: Meeting Room.
- **Visual Tasks**: Submit Scan.
- **Unique Features**: Vents are carnivorous plants that eat and spit out players. Purple Haze plants in the Jungle emit pink smoke that temporarily obscures nearby players. Zipline connects Storage and Communications.

**Vent Network (10 vents, 3 networks)**:

| Network | Connected Rooms |
|---|---|
| 1 | Cafeteria (exterior) ↔ Storage alcove ↔ Laboratory ↔ Splash Zone (loops back) |
| 2 | Kitchen ↔ Jungle (two connection points) |
| 3 | Reactor ↔ Communications ↔ Lookout |

**Sabotages**: Reactor Meltdown (60s timer), Mushroom Mixup (§7.2), Fix Communications, Close Doors. **No Lights sabotage** on The Fungle.

**Surveillance**: Telescope (Lookout — manually controlled panoramic view covering most of the map except mid-right cliffs). No cameras, admin table, or vitals.

---

## 6. Tasks

Tasks are Crewmate objectives. All Crewmate tasks (including ghost tasks) must be completed for a task victory.

Impostors receive a **fake task list** identical in structure to a real assignment. They can walk to task stations and open a fake task interface (the same UI as the real task), but "completing" a fake task does **not** fill the task bar. Observant Crewmates can catch an Impostor faking by watching for a task bar that doesn't move after the Impostor leaves a station. When Visual Tasks are enabled (§3), tasks like Submit Scan produce a visible animation — Impostors cannot trigger these visual cues, making visual tasks a definitive innocence test.

### 6.1 Task Types

| Type | Description |
|---|---|
| **Common** | Assigned identically to every player — if one player has it, all players have it. Useful for identifying Impostors who fake a common task nobody else has. |
| **Short** | Single-step tasks; quick to complete. |
| **Long** | Multi-step tasks requiring travel between rooms, waiting, or extended interaction. |
| **Visual** | Tasks with a visible animation when completed (e.g., scan beam, shield glow, asteroid shots). Can be used to prove innocence when Visual Tasks are enabled. |

### 6.2 Tasks by Map

Full task lists are in the companion document: [docs/tasks.md](docs/tasks.md).

**Task counts per map**:

| Map | Total Unique Tasks | Common | Visual |
|---|---|---|---|
| The Skeld | 18 | 2 (Fix Wiring, Swipe Card) | 5 (Submit Scan, Prime Shields, Clear Asteroids, Empty Garbage, Empty Chute) |
| MIRA HQ | 18 | 2 (Fix Wiring, Enter ID Code) | 1 (Submit Scan) |
| Polus | 22 | 3 (Fix Wiring, Insert Keys, Scan Boarding Pass) | 1 (Submit Scan) |
| The Airship | 23 | 2 (Fix Wiring, Enter ID Code) | 0 |
| The Fungle | 18 | 2 (Fix Wiring, Run Diagnostics) | 1 (Submit Scan) |

### 6.3 Notable Task Mechanics

- **Fix Wiring** — the only task appearing on every map (always Common). Requires connecting colored wires across three panels at random locations.
- **Upload/Download Data** — Long task; download at one location, then walk to a different location to upload. The download and upload steps take ~8 seconds each.
- **Swipe Card** (The Skeld) — notoriously timing-sensitive; the card must be swiped at just the right speed (not too fast, not too slow).
- **Start Reactor** — Simon Says-style memory pattern; 5 rounds of increasing length.
- **Submit Scan** — only one player can scan at a time; others must wait. Visual: a green beam appears over the player.
- **Clear Asteroids** — shoot 20 asteroids. Visual: cannon blasts are visible from outside.
- **Inspect Sample** — requires a **60-second wait** between pouring and selecting the sample.

---

## 7. Sabotage System

Only Impostors (living or ghost) can sabotage. Sabotage is triggered from the sabotage map overlay.

### 7.1 Sabotage Categories

**Critical Sabotages** — will kill all Crewmates if the timer expires (instant Impostor win):
- Reactor Meltdown / Seismic Stabilizers / Avert Crash Course
- O2 Depletion

**Utility Sabotages** — create disruption but don't directly win:
- Fix Lights
- Fix Communications
- Mushroom Mixup (The Fungle only)

**Door Sabotages** — lock doors to rooms temporarily.

### 7.2 Sabotage Details

| Sabotage | Maps | Timer | Resolution | Effect |
|---|---|---|---|---|
| Reactor Meltdown | Skeld, MIRA HQ, Fungle | 30s (Skeld), 45s (MIRA), 60s (Fungle) | Two players simultaneously hold hands on two scanners | Critical — kills all if unfixed |
| O2 Depletion | Skeld, MIRA HQ | 30s (Skeld), 45s (MIRA) | Enter codes at two keypads | Critical — kills all if unfixed |
| Seismic Stabilizers | Polus | 60s | Two players press and hold at two locations | Critical — equivalent to Reactor |
| Avert Crash Course | Airship | 90s | Two players interact at two locations | Critical — equivalent to Reactor |
| Fix Lights | Skeld, MIRA, Polus, Airship | None | Flip switches on a circuit breaker panel until all bulbs are green. Fix location: Electrical (Skeld, Polus), Office (MIRA), Gap Room / Viewing Deck / Cargo Bay (Airship) | Drastically reduces Crewmate vision; does not affect Impostor vision |
| Fix Communications | All maps | None | Skeld/Airship: adjust dial to match waveforms. MIRA: enter codes at two stations. Polus: adjust dial outdoors. Fungle: enter codes | Disables task list display, hides task bar progress, disables surveillance systems (§8) |
| Mushroom Mixup | Fungle only | 10s (self-resolving) | Cannot be fixed — expires automatically | Randomizes all player appearances (color, hat, skin) and hides names for the duration; enables unidentifiable kills |
| Close Doors | Skeld, Polus, Airship, Fungle | 10s (auto-reopen) | Skeld: auto-reopen only. Polus/Fungle: Crewmates can manually open by clicking door switches. Airship: Crewmates pull levers to open | Locks all doors to a chosen room; blocks entry/exit |

### 7.3 Sabotage Cooldowns

- **Non-door sabotages** share a cooldown that starts when the sabotage is resolved (not when triggered). Default: 30s.
- **Door sabotages** have their own independent cooldown that starts when triggered. Multiple doors can be closed simultaneously.
- A door sabotage does not prevent non-door sabotages (and vice versa), except that non-door sabotages are disabled while any door sabotage is actively closing.
- An Impostor can sabotage and kill in the same round — there is no mutual exclusion.

---

## 8. Surveillance Systems

Each map has a different combination of surveillance tools. All surveillance systems are disabled while Communications is sabotaged.

### 8.1 Security Cameras

- **Available on**: The Skeld, Polus, The Airship.
- **Not available on**: MIRA HQ, The Fungle.
- Show live feeds of fixed camera positions. A blinking red light on in-world cameras indicates someone is currently watching.
- The Skeld: 4 camera views (Y-corridor, MedBay hallway, Security-Reactor corridor, Navigation top).
- Polus: 6 camera views.
- The Airship: 6 camera views (Engine Room, Vault, Records, Security, Cargo Bay, Meeting Room).

### 8.2 Admin Table (Map)

- **Available on**: The Skeld, MIRA HQ, Polus, The Airship.
- **Not available on**: The Fungle.
- Displays a schematic of the map with anonymous icons showing how many players are in each room. Does not show exact positions, names, or hallway locations. Updates when players change rooms.

### 8.3 Vitals

- **Confirmed on**: Polus (Office). Possibly also on The Skeld (MedBay) — see §14.
- Shows each player's status: Alive (green heartbeat), Dead (red flatline, killed since last meeting), Disconnected (gray), or Dead from previous round (gray).

### 8.4 Door Log

- **Available on**: MIRA HQ (Communications).
- Records the last 20 entries of players passing through three directional sensors on the Skywalk corridor:
  - North (Blue)
  - Southwest (Green)
  - Southeast (Red)
- Each entry logs the player's color and which sensor they passed. 5-second cooldown per sensor per player.

### 8.5 Telescope

- **Available on**: The Fungle (Lookout).
- Manually controlled panoramic view. Covers most of the map except the middle and right-side cliffs.

---

## 9. Meeting System

### 9.1 Triggering a Meeting

Two ways to start a meeting:
1. **Report a body** — any living player who approaches a dead body can press Report. No cooldown; no limit.
2. **Press the Emergency Button** — located in the central room of each map. Limited per player per game (configurable, default 1). Subject to Emergency Cooldown after any meeting ends. The button is **disabled during any active sabotage** — Crewmates must resolve the sabotage before they can call a button meeting. Body reports still work during sabotages.

### 9.2 Meeting Flow

1. All living players are teleported to the meeting table.
2. An overview shows who called the meeting (or whose body was found) and who died since the last meeting.
3. **Discussion phase** — timer counts down; players chat via text (§2.3). No voting is allowed during this phase.
4. **Voting phase** — each living player can vote for one player to eject or press Skip. Voting ends when all living players have voted or the timer expires.
5. **Ejection** — the player with the most votes is ejected (killed and removed). If "Confirm Ejects" is on, the game reveals whether the ejected player was an Impostor. Ties and majority-Skip result in no ejection. Ejection animation varies by map: thrown out an airlock (Skeld), dropped from the HQ (MIRA), thrown into lava (Polus), thrown overboard (Airship), launched by mushroom (Fungle).
6. Players return to free roam at the meeting table area (not their pre-meeting positions). Kill cooldowns reset to the configured value.

### 9.3 Voting Rules

- Anonymous Votes (if enabled) hides who voted for whom — only totals are shown.
- Ghosts cannot vote or chat with living players during meetings.
- Skipping is always available as an option.

---

## 10. Hide n Seek Mode

An official alternative game mode released December 9, 2022. One player is the **Seeker** (Impostor); all others are **Hiders** (Crewmates).

### 10.1 Core Differences from Classic Mode

- The Seeker's identity is revealed to all players at game start (visible teeth/tongue animation).
- No meetings, no voting, no discussions, no ejections.
- No sabotage for the Seeker.
- The Seeker **cannot** use vents.
- Hiders **can** use vents (limited uses per game with short duration).

### 10.2 Timer Mechanics

- A countdown timer replaces the task bar. Hiders win if the timer reaches zero.
- Completing tasks **reduces** the remaining time, bringing Hider victory closer.

### 10.3 Flashlight Mode

- Optional: Hiders have a narrow cone of vision (flashlight) rather than a full circle, limiting their awareness.

### 10.4 Final Hide Phase

- When the main timer expires, the **Final Hide** phase begins.
- The Seeker receives a speed boost.
- Tasks are disabled.
- The timer bar turns red.
- **Pings** are emitted every ~6 seconds, revealing the approximate position of every living Hider on the Seeker's screen (similar to Noisemaker alerts).
- If any Hiders survive the Final Hide, Crewmates win.

### 10.5 Win Conditions

| Outcome | Trigger |
|---|---|
| Seeker (Impostor) Victory | All Hiders eliminated |
| Hider (Crewmate) Victory | Timer expires and at least one Hider survives Final Hide |

---

## 11. UI & HUD

### 11.1 In-Game HUD

- **Top-left**: Room name indicator.
- **Top**: Task bar (horizontal, fills from left to right as tasks are completed game-wide). Display mode configurable (Always / Meetings Only / Never).
- **Bottom-right**: Action buttons — context-sensitive set that includes:
  - **Use** (interact with task/object)
  - **Report** (report dead body — appears when near a body)
  - **Kill** (Impostor only — appears when a target is in range)
  - **Sabotage** (Impostor only — opens sabotage map overlay)
  - **Vent** (Impostor / Engineer — appears when near a vent)
  - **Admin** / **Security** / **Vitals** / **Door Log** (appears when near the respective device)
- **Bottom-left**: Map button (opens minimap showing room layout and task locations).
- **Top-right**: Settings gear, chat button.

### 11.2 Minimap

- Shows the full map layout with room names.
- Crewmate tasks are marked with yellow exclamation marks.
- Impostor fake tasks are also shown (for blending in).
- Sabotage map (Impostor overlay) shows sabotage buttons overlaid on room locations.

### 11.3 Meeting UI

- Central voting panel with each player listed by name and color.
- Nameplates are customizable cosmetics.
- Vote indicators (or anonymous if enabled) appear on each player's panel.
- Discussion and voting timer at the top.
- Text chat panel on the right.
- "I Voted" sticker appears on a player's panel after they vote.
- Dead players appear with an "X" and cannot vote or chat to living players.

### 11.4 End-Game Screen

- Displays "Victory" or "Defeat" with the winning team's color scheme.
- Lists all players by role (Crewmate / Impostor).
- Shows which ghosts completed their tasks.

---

## 12. Cosmetics & Customization

Cosmetics are purely aesthetic and do not affect gameplay.

### 12.1 Cosmetic Types

| Type | Description |
|---|---|
| **Hats** | Worn on the character's head |
| **Skins** | Body outfits worn over the character |
| **Visors** | Face accessories worn on the visor area |
| **Pets** | Small companions that follow the player around; remain at the death spot when the owner is killed |
| **Nameplates** | Decorative borders on the voting panel during meetings |

### 12.2 Player Color

Players select one of 18 available colors. Each color can only be used by one player per lobby:
Red, Blue, Green, Pink, Orange, Yellow, Black, White, Purple, Brown, Cyan, Lime, Maroon, Rose, Banana, Gray, Tan, Coral.

### 12.3 Currency System

| Currency | Source | Use |
|---|---|---|
| **Beans** | Earned through gameplay (tasks completed, wins) | Purchase standard items and Cosmicubes in the Shop |
| **Stars** | Premium currency (real-money purchase) | Purchase exclusive/licensed bundles and items |
| **Pods** | Earned within a specific Cosmicube | Unlock items within that Cosmicube's tree (non-transferable) |

**Cosmicubes** are progression trees — once purchased (with Beans or Stars), players earn Pods by playing and spend them to unlock cosmetics within that Cube's branching unlock path.

---

## 13. Engine & Presentation Systems

### 13.1 Save / Persistence

- No save system during a match — each game is a single session (~5–15 minutes).
- Player cosmetics, settings, and unlocked items persist across sessions via account/cloud save.
- Stats are tracked per player (games played, wins, tasks completed, kills, etc.).

### 13.2 Audio System

- Distinct audio cues for: kill, body report, meeting start, voting, ejection, task completion, sabotage alarms, vent open/close.
- Critical sabotages play a looping alarm with increasing urgency as the timer counts down.
- Ambient map-specific background music/sounds.
- Footstep sounds are not present (movement is silent).

### 13.3 Camera Behavior

- Fixed top-down 2D perspective centered on the player character.
- Camera follows the player's movement.
- Vision fog obscures areas outside the player's range and behind walls (§1.6).
- During meetings: camera shows the meeting table with all player avatars.

### 13.4 Networking

- Client-server architecture with a dedicated lobby host.
- Games are hosted on InnerSloth's servers (public matchmaking) or as private lobbies (code-based join).
- Region selection: North America, Europe, Asia.
- Max lobby size: 15 players.
- Min to start: 4 players.

---

## 14. Open Questions / Unverified

- **Exact setting ranges for newer role options** (Tracker, Noisemaker, Phantom) — specific min/max/increment values for these role settings are not consistently documented across community sources. The values listed for Engineer, Scientist, and Guardian Angel are approximate based on community reports.
- **Precise sabotage cooldown default** — most sources cite 30s but this may vary by game version.
- **Vitals availability** — some sources indicate Vitals were added to The Skeld in later updates (originally only on Polus). The current status on all maps needs verification.
- **Hide n Seek configurable settings** — exact ranges for Seeker speed boost, ping interval, vent charges for Hiders, and Final Hide duration are not well-documented.
- **Vision occlusion fidelity** — vision is wall-occluded (not a simple radius), but the exact shadow-casting algorithm (hard edges vs. soft penumbra, thin-wall handling) is not documented.

---

## 15. References

### Wikis
- [Among Us Wiki (Fandom)](https://among-us.fandom.com/wiki/Among_Us_Wiki) — primary community wiki
- [Among Us Wiki — Options](https://among-us.fandom.com/wiki/Options) — game settings reference
- [Among Us Wiki — Tasks](https://among-us.fandom.com/wiki/Tasks) — task list and descriptions
- [Among Us Wiki — Sabotage](https://among-us.fandom.com/wiki/Sabotage) — sabotage mechanics
- [Among Us Wiki — Roles](https://among-us.fandom.com/wiki/Roles) — role abilities and settings
- [Among Us Wiki — Maps](https://among-us.fandom.com/wiki/Maps) — map layouts and details
- [Among Us Wiki — Ghost](https://among-us.fandom.com/wiki/Ghost) — ghost mechanics
- [Among Us Wiki — Hide n Seek](https://among-us.fandom.com/wiki/Hide_n_Seek) — Hide n Seek mode

### Guides
- [InnerSloth Help Center — Beginner's Guide](https://innersloth.zendesk.com/hc/en-us/articles/7794240573460-Beginner-s-Guide-to-Among-Us)
- [InnerSloth Help Center — Roles Explanation](https://innersloth.zendesk.com/hc/en-us/articles/7913947403924-Roles-Explanation)
- [Steam Community — Among Us Complete Wiki Guide](https://steamcommunity.com/sharedfiles/filedetails/?id=2272246579)
- [PCGamesN — Among Us Maps Guide](https://www.pcgamesn.com/among-us/maps-layout-vents)
- [BlueStacks — Among Us Character Guide](https://www.bluestacks.com/blog/game-guides/among-us/au-character-guide-en.html)
- [Among Us Tasks Complete List](https://among-us.net/en/tasks-list/)

### Companion Documents
- [docs/tasks.md](docs/tasks.md) — Complete task lists organized by map
