# The Legend of Zelda: Ocarina of Time — Phased Implementation Plan

14 phases. Vertical slice through Phase 4, adult era content through Phase 11, side systems and finale through Phase 14.

---

## Phase 1 — Core Movement & Camera

Child Link third-person character controller with the full movement suite and camera system that defines moment-to-moment feel.

- Child Link movement: walk (8.25 units/frame), run (analog stick pressure), roll (forward + A, 13.5 units/frame)
- Auto-jump when running off ledges; distance scales with momentum
- Ledge grab, pull-up, and drop
- Ladder and vine climbing (fixed climb speed)
- Fall damage: 1/2 heart for short drops, up to 4 hearts for extreme drops; rolling on landing negates
- Context-sensitive A button with on-screen text label (Roll, Grab, Climb, Jump, Check)
- Third-person follow camera: auto-adjusts behind Link, wall avoidance, tight-space adjustment
- C-Up first-person look mode (stationary, free look)
- Crawl through small holes (child-exclusive traversal)

### Assets

**3D Models**
- Child Link
- Test environment (open field with ledges, ladders, vines, gaps, crawl spaces)

**Animations**
- Child Link: idle, walk, run, roll, auto-jump, land, hard land, ledge grab, pull-up, climb ladder, climb vine, crawl, fall, C-Up look

**Audio**
- Footstep SFX (grass, stone, dirt, wood)
- Roll SFX
- Jump / land SFX
- Ledge grab SFX
- Climb SFX
- Fall damage grunt

**UI**
- A-button context label (dynamic text overlay, top-right)

---

## Phase 2 — Combat & Z-Targeting

Melee combat with Z-Targeting lock-on, the Kokiri Sword move set, shield defense, and the health system — tested against two representative enemy types.

- Z-Targeting system
  - Z button locks onto nearest targetable entity; Navi flies to target
  - Camera centers between Link and target; Link auto-faces target
  - Strafing movement while locked (sidestep left/right, advance, retreat)
  - Target cycling: release and re-press Z
  - Yellow arrow indicator above targetable enemies; blue/green for friendly NPCs
- Kokiri Sword melee (B button):
  - Horizontal slash (B), vertical slash (Z + forward + B), thrust (Z + B still)
  - Jump attack (Z + A): 2× base damage
  - Spin attack (hold B + release): 2× base damage
- Deku Shield (R button):
  - Hold to block; absorbs frontal melee/projectile damage
  - Reflect certain projectiles (Deku Scrub nuts)
  - Burns on fire contact
- Defensive movement while Z-targeting:
  - Backflip (Z + back + A), side hop (Z + left/right + A)
- Health system:
  - 3 starting hearts; quarter/half/full heart damage increments
  - Recovery Heart drops from enemies and pots
  - Game Over screen: "Continue" (3 hearts) or "Save and Quit"
  - Low health warning beep at ≤1 heart
- Navi companion:
  - Hovers near Link; flies to Z-targeted objects/enemies
  - Color changes: yellow = targetable enemy, green = secret/interaction point
  - C-Up calls Navi for contextual enemy hints
  - Periodic "Hey!" / "Listen!" prompts; C-Up dismisses
- Test enemies: Deku Baba (lunging, 2 HP — tests melee timing) and Keese (1 HP — tests target tracking on moving aerial enemy)

### Assets

**3D Models**
- Kokiri Sword
- Deku Shield
- Navi (glowing sphere with wings)
- Deku Baba (lunging variant)
- Keese
- Recovery Heart pickup

**Animations**
- Child Link: all sword swings, shield raise/hold/block, backflip, side hop, jump attack, spin attack, take damage, knockback, death
- Deku Baba: idle sway, lunge, hit reaction, rigid stun, death
- Keese: flight loop, swoop, hit reaction, death

**VFX**
- Sword slash trail
- Hit spark (on enemy contact)
- Shield block flash
- Enemy death puff
- Navi glow/trail
- Z-target lock indicator (yellow arrow)
- Recovery Heart sparkle

**Audio**
- Sword swing SFX (3 variants: slash, vertical, thrust)
- Spin attack charge + release SFX
- Shield block impact SFX
- Link attack voice lines (grunts, jump attack yell)
- Link hurt voice lines
- Link death voice
- Navi voice lines ("Hey!", "Listen!", targeting chime, hint text chime)
- Deku Baba SFX (snap, death)
- Keese SFX (screech, wing flap, death)
- Low health warning beep
- Game Over jingle

**UI**
- Heart Container row (top-left, fractional damage display)
- B-button icon (top-right, shows Kokiri Sword)
- Z-target reticle overlay
- Game Over screen

---

## Phase 3 — First Dungeon: Inside the Deku Tree

The complete dungeon loop — enter, explore rooms, find the dungeon item, solve puzzles, defeat the boss, earn a quest reward — plus Kokiri Forest as the starting overworld area.

- Kokiri Forest overworld area:
  - Link's House, Mido's House, Know-It-All Brothers' House (interiors)
  - Kokiri NPC villagers with dialogue
  - Kokiri Sword chest (behind crawl space)
  - Kokiri Shop: Deku Shield (40), Deku Nuts (15/30), Deku Sticks (10), Recovery Heart (10), Deku Seeds (30)
  - Entrance to Deku Tree clearing
  - Exit to Hyrule Field (blocked by Mido until sword + shield obtained)
- NPC dialogue system: text boxes at screen bottom, A-button advance, Yes/No choice prompts
- Basic rupee system: Green (1), Blue (5), Red (20) pickups; on-screen counter; grass/pot drops
- Shop interface: browse items, purchase with rupees
- C-button item system: assign items to C-Left/Down/Right via pause menu; on-screen C-button icons
- Dungeon framework:
  - Room-based layout with door transitions
  - Small Keys (consumable, dungeon-specific), Boss Key, Dungeon Map, Compass
  - Dungeon HUD: Small Key counter (bottom-left)
  - Dungeon Map subscreen: floor selector, Link position, boss skull icon (with Compass), chest icons (with Compass)
- Fairy Slingshot (C-button item): first-person aiming, Deku Seed ammo (30 capacity), 1 damage
- Deku Stick (C-button item): 2 damage melee, breaks after one hit, carries fire from torches
- Puzzle elements: floor switches, crystal switches, pushable/pullable blocks, torches (lit/unlit), cobweb barriers (burn with fire), eye switches (shoot to activate)
- Inside the Deku Tree dungeon:
  - 3-floor vertical dungeon with central shaft
  - Webs, vines, water pool at basement
- Dungeon enemies: Deku Baba (withered, 1 HP — drops Deku Sticks), Skulltula (wall, 1 HP), Big Skulltula (hanging, 2 HP), Deku Scrub (reflect projectile), Gohma Larva (2 HP)
- Queen Gohma boss fight: 10 HP, climbs ceiling, drops Gohma Larvae eggs; stun with Slingshot when eye turns red → sword; ~3 cycles
- Boss intro title card ("Parasitic Armored Arachnid — Queen Gohma")
- Heart Container drop from boss
- Kokiri's Emerald (first Spiritual Stone) quest item
- Item get ceremony: Link holds item overhead with fanfare
- Piece of Heart item type (4 pieces = 1 new Heart Container)

### Assets

**3D Models**
- Kokiri Forest environment (village, houses, clearings, paths)
- Link's House interior
- Kokiri Shop interior
- Inside the Deku Tree (3-floor dungeon environment)
- Fairy Slingshot
- Deku Stick
- Deku Nut (throwable)
- Treasure chest (small, large)
- Small Key, Boss Key, Dungeon Map, Compass pickups
- Push block
- Floor switch, crystal switch, eye switch
- Torch (lit/unlit states)
- Cobweb barrier
- Skulltula (wall), Big Skulltula (hanging)
- Deku Scrub
- Gohma Larva
- Queen Gohma
- Kokiri's Emerald
- Heart Container, Piece of Heart
- Kokiri NPCs (male/female variants)
- Rupee pickups (Green, Blue, Red)
- Pot, bush (breakable props)

**Animations**
- Child Link: slingshot aim/fire, push/pull block, open chest, hold item overhead, pick up rupee
- Skulltula: wall crawl, turn (expose belly), death
- Big Skulltula: hang, spin, death
- Deku Scrub: emerge, spit nut, hit reaction, burrow
- Gohma Larva: crawl, lunge, hatch from egg, death
- Queen Gohma: ceiling crawl, descend, eye flash (red), stagger/stun, lay egg, swipe, death
- Treasure chest: open

**VFX**
- Deku Seed projectile trail
- Fire on Deku Stick (carry flame)
- Cobweb burn
- Boss intro title card
- Item get light column + sparkle
- Spiritual Stone glow
- Switch activation flash

**Audio**
- Slingshot fire + seed impact SFX
- Deku Stick break SFX
- Deku Nut flash SFX
- Fire crackle / torch light SFX
- Chest open jingle
- Small Key get jingle
- Boss Key get jingle
- Item get fanfare (short for keys/maps, long for dungeon items/quest items)
- Puzzle solved jingle
- Door unlock SFX
- Push/pull block scrape SFX
- Skulltula SFX (chittering, death)
- Deku Scrub SFX (pop, spit, flee)
- Gohma Larva SFX
- Queen Gohma SFX (screech, ceiling skitter, egg splat, death cry)
- Inside the Deku Tree dungeon BGM
- Queen Gohma boss BGM
- Boss defeated fanfare
- Kokiri Forest BGM
- Shop BGM
- Rupee pickup SFX
- Pot/bush break SFX
- NPC dialogue text scroll SFX

**UI**
- C-button item icons (C-Left, C-Down, C-Right — top-right)
- Dungeon Map subscreen (floor selector, room layout, icons)
- Dungeon HUD: Small Key count (bottom-left)
- Rupee counter (top-left)
- Boss intro title card overlay
- Item description text popup
- Shop browse/purchase interface

---

## Phase 4 — Overworld & World Systems

Hyrule Field as the central hub connecting regions, the day/night cycle, save system, minimap, and Castle Town as the first major populated area — completing the open-world feel.

- Hyrule Field:
  - Large open hub connecting Kokiri Forest (SE), Castle Town (N), Kakariko (NE), Zora's River (E), Lake Hylia (S), Lon Lon Ranch (center), Gerudo Valley (W)
  - Road paths, fences, trees, river
  - Region transition loading between areas
- Day/night cycle:
  - Day: ~2 min 30 sec real time; Night: ~2 min 10 sec
  - Sun/moon arc, gradual lighting shifts (dawn, day, dusk, night)
  - Time does not pass inside buildings or dungeons
- Night-specific behavior:
  - Stalchildren (2 HP) emerge from ground on Hyrule Field as child (off-road only; not on paths; not with Bunny Hood)
  - Peahat (6 HP, propeller flight, roots are weak point) and Mini Peahat (1 HP, homing) on Hyrule Field
  - Guay (1 HP, swooping black birds) on Hyrule Field at night and Lake Hylia area
  - Castle Town drawbridge raises at night
- Save system:
  - 3 save files; save anytime via Start menu
  - Load: child spawns at Link's House or Temple of Time; adult spawns at Temple of Time
  - Dungeon progress (keys, items) persists; position resets to dungeon entrance
  - Game Over → "Continue" (3 hearts) or "Save and Quit"
- Minimap (L button toggle): shows Link's position/direction, area layout
- World map subscreen: reveals visited regions
- Hyrule Castle Town (child era):
  - Market square (populated by day, empty at night)
  - Back alleys
  - Bazaar shop: Hylian Shield (80), Bombs (35), Arrows (20/60/90), Deku Sticks (10), Deku Nuts (15), Recovery Heart (10)
  - Potion Shop: Red Potion (30), Green Potion (30), Blue Potion (100), Deku Nuts (15)
  - Temple of Time exterior (enterable but Door of Time sealed — deferred to Phase 8)
  - Happy Mask Shop (building exterior, not yet functional)
  - Shooting Gallery, Bombchu Bowling, Treasure Chest Game (building exteriors, not yet functional)
- Dynamic music system:
  - Region-specific BGM
  - Hyrule Field theme fades near enemies; combat fanfare on engagement
  - Day/night music cross-fade

### Assets

**3D Models**
- Hyrule Field environment (terrain, roads, river, bridges, fences, trees)
- Hyrule Castle Town (market square, alleys, buildings, fountain)
- Bazaar interior
- Potion Shop interior
- Temple of Time exterior
- Stalchild
- Peahat, Mini Peahat
- Guay
- Drawbridge (open/closed states)
- Sun, moon (skybox elements)

**Animations**
- Stalchild: emerge from ground, walk, swipe, hit reaction, death
- Peahat: propeller spin, grounded idle, launch Mini Peahats
- Guay: flight loop, swoop, death
- Castle Town NPCs: idle, walk, browse
- Drawbridge: raise/lower

**VFX**
- Dawn/dusk sky color transitions
- Stalchild ground emergence dust

**Audio**
- Hyrule Field BGM (day)
- Hyrule Field BGM (night)
- Hyrule Castle Town Market BGM
- Temple of Time exterior BGM
- Combat encounter fanfare
- Day/night transition chime
- Drawbridge mechanical SFX
- Stalchild SFX (emerge, attack, death)
- Peahat SFX (propeller whir, root slam)
- Guay SFX (caw, swoop)
- Save confirmation jingle

**UI**
- Minimap overlay (bottom-right, L toggle)
- World map subscreen
- Save file select / save confirmation screen

---

**Vertical slice checkpoint — Kokiri Forest through Hyrule Field playable: core movement, Z-targeting combat, Inside the Deku Tree dungeon with Queen Gohma boss, day/night cycle, shops, and save system.**

---

## Phase 5 — Subscreens, Bottles & Economy

The full pause-menu subscreen system, bottle mechanics, and expanded economy — the inventory backbone that all future items and equipment plug into.

- Subscreen (Start menu) with four L/R-navigated tabs:
  1. **Map/Dungeon** — world map (visited regions) or dungeon floor viewer
  2. **Quest Status** — Spiritual Stone slots, Medallion slots, songs learned, Heart Piece count, Gold Skulltula count
  3. **Equipment** — swap swords, shields (tunics and boots slots present but only defaults available)
  4. **Items** — C-button item grid; drag-assign to C-Left/Down/Right
- Equipment swapping: Kokiri Sword ↔ (empty adult slots), Deku Shield ↔ Hylian Shield
- Bottle system:
  - 4 obtainable bottles (individual bottles obtained in later phases via quests)
  - Swing empty/full bottle with B as a melee attack; deflects energy projectiles
  - Bottling: scoop bugs, fish from environment; catch fairies, Poe souls
  - Contents: Red Potion (full health restore), Fairy (full restore, auto-revive on death), Lon Lon Milk (5 hearts, 2 uses), Fish, Bugs
  - Green Potion (full magic) and Blue Potion (both) deferred to Phase 8 when magic is introduced
- Expanded rupee economy:
  - Full denomination set: Green (1), Blue (5), Red (20), Purple (50), Huge (200)
  - Wallet tiers: Child's (99 default), Adult's (200, from 10 Skulltula Tokens — Phase 12), Giant's (500, from 30 Tokens — Phase 12)
  - Rupee counter color changes when wallet is nearly full
- Deku Nut item: throwable flash-bang, stuns most enemies, 20 base capacity
- Pot/grass/bush rupee and item drops
- Potion effects on use: Red restores all hearts; Fairy auto-triggers on death

### Assets

**VFX**
- Bottle swing trail
- Bottling capture sparkle
- Potion drink effect (health restore shimmer)
- Fairy release glow (auto-revive)
- Purple Rupee, Huge Rupee sparkle

**Audio**
- Bottle swing SFX
- Bottle catch SFX (liquid, fairy, bug)
- Potion drink SFX
- Fairy revive chime + voice
- Wallet full rejection SFX

**UI**
- Equipment subscreen (4-slot gear grid: sword, shield, tunic, boots)
- Item subscreen (C-button item grid with assignment)
- Quest Status subscreen (Medallion/Stone slots, song list, counters)
- Map subscreen (world map with region labels)
- Bottle contents icon overlays

---

## Phase 6 — Ocarina & Story Foundation

The Ocarina instrument system with note input and learned-song playback, plus the Hyrule Castle story sequence that sets the child-era quest in motion.

- Fairy Ocarina (C-button item, from Saria after Deku Tree):
  - Equip to C-button; press to enter Ocarina mode
  - Note input: C-Up, C-Left, C-Right, C-Down, A each produce a note
  - R = sharp, Z = flat, Control Stick = vibrato/pitch bend
  - Musical staff display: five-line staff shows notes in real time
  - Song recognition: when a learned song's note sequence is played, trigger its effect
- Learned songs system: songs display on Quest Status subscreen as they're learned
- Child-era songs:
  - Zelda's Lullaby (C←, C↑, C→, C←, C↑, C→) — activates Triforce symbols, opens Royal Family gates
  - Epona's Song (C↑, C←, C→, C↑, C←, C→) — summons Epona (adult, deferred); produces milk from cows
  - Song effects activate world interactions (Triforce symbols glow, doors open, cows produce milk)
- Hyrule Castle grounds:
  - Guard patrol stealth sequence (guards spot Link → throw him out)
  - Courtyard: meet Princess Zelda cutscene
  - Zelda's Letter quest item
  - Impa teaches Zelda's Lullaby; escorts Link out
  - Great Fairy Fountain (Hyrule Castle): Din's Fire spell (AoE fire blast, costs magic — magic meter deferred to Phase 8; item obtained but unusable until then)
- Lon Lon Ranch (child visit):
  - Talon, Malon, Ingo NPCs
  - Malon teaches Epona's Song
  - Young Epona in corral (can interact but not ride)
  - Lon Lon Milk (buyable)
- Death Mountain gate guard: show Zelda's Letter to pass

### Assets

**3D Models**
- Fairy Ocarina
- Hyrule Castle grounds (gardens, moats, guard posts, courtyard)
- Princess Zelda (child)
- Impa
- Castle guards (patrol routes)
- Great Fairy Fountain (interior)
- Great Fairy
- Din's Fire orb (item pickup)
- Lon Lon Ranch (corrals, barn, tower, Ingo's house)
- Talon, Malon (child), Ingo
- Young Epona
- Cows
- Zelda's Letter

**Animations**
- Zelda: idle, turn, dialogue gestures
- Impa: teach song gesture, escort walk
- Guards: patrol walk, spot reaction, throw-out grab
- Great Fairy: emerge, grant power, laugh
- Child Link: Ocarina play (hold up, fingers move with notes)
- Malon: singing idle, teach song
- Young Epona: idle, nuzzle Link (Epona's Song reaction)

**VFX**
- Ocarina note particles (per note color/shape)
- Song recognition flash (successful melody glow)
- Triforce symbol activation glow
- Din's Fire explosion radius
- Great Fairy magic swirl

**Audio**
- Ocarina note tones (5 base notes: A, C-Up, C-Down, C-Left, C-Right + sharp/flat variants)
- Zelda's Lullaby melody
- Epona's Song melody
- Song learned fanfare
- Correct song recognition chime
- Din's Fire blast SFX
- Great Fairy Fountain BGM
- Great Fairy laugh voice
- Hyrule Castle grounds BGM
- Lon Lon Ranch BGM
- Guard alert SFX
- Cow moo SFX

**UI**
- Ocarina musical staff overlay (five-line staff, bottom of screen)
- Song playback prompt (note sequence display when learning)
- Quest Status: song slots update when learned

---

## Phase 7 — Child Era Dungeons & Regions

The remaining two child dungeons with their regions, key items (Bombs, Boomerang), and the full child-era NPC world — completing Act 1 of the story.

- Kakariko Village:
  - Village layout: houses, windmill, graveyard entrance, well, watchtower
  - NPCs: villagers, guard at Death Mountain gate, Cucco Lady, Graveyard Boy
  - Cucco NPCs: can pick up and carry; Cucco revenge swarm if attacked ~3 times
  - Graveyard: tombstones, Royal Family Tomb (with ReDeads, 8 HP — learn Sun's Song)
  - Sun's Song (C→, C↓, C↑, C→, C↓, C↑): toggles day/night, paralyzes ReDeads/Gibdos
  - Poe enemies in Graveyard at night (8 HP, appear/disappear, lantern attack)
  - Hylian Shield: free from specific grave chest
- Death Mountain Trail + Goron City:
  - Trail: Tektites (Red, 2 HP), falling boulders
  - Goron City: multi-level interior, Goron NPCs, Darunia's chamber
  - Darunia: befriend by playing Saria's Song → gives Goron's Bracelet (pick up Bomb Flowers)
  - Goron Shop: Bombs (25/50/80/120), Goron Tunic (200), Red Potion (40), Recovery Heart (10)
  - Great Fairy Fountain (Death Mountain Summit): Magic Meter + Magic Spin Attack — magic system deferred to Phase 8; meter appears but spells not yet functional
- Sacred Forest Meadow (through Lost Woods):
  - Lost Woods: directional audio navigation (Saria's Song louder = correct path); Wolfos (8 HP, blocks frontal attacks, backstrike) as enemies
  - Meadow: Saria teaches Saria's Song (C↓, C→, C←, C↓, C→, C←)
  - Saria's Song: communicate with Saria for hints; cheers up Darunia
- Dodongo's Cavern:
  - Multi-room lava/cave dungeon with cracked walls and bomb-flower puzzles
  - Bomb Bag (dungeon item, 20 capacity): equip Bombs to C-button, throw/place, fuse timer, blast radius, destroys cracked walls/floors
  - Bombchu item: self-propelled bomb, climbs walls/ceilings
  - Enemies: Baby Dodongo (1 HP, explodes on death), Dodongo (4 HP, fire breath), Lizalfos (6 HP, tag-team swordfighters), Armos (1 HP, statue activates on contact), Beamos (1–2 HP, laser turret, bomb-only)
  - King Dodongo boss: 12 HP, rolls into ball charge, fire breath arc; throw Bomb into mouth when inhaling → sword stunned body; 4 bomb cycles
  - Goron's Ruby (second Spiritual Stone)
- Zora's River + Zora's Domain + Zora's Fountain:
  - River: Octorok enemies (1 HP, reflect projectile), waterfall entrance to Domain
  - Surface swimming: A-button dive, B taps for faster stroke
  - Zora's Domain: waterfall cavern interior, King Zora (on throne, slides aside to grant passage), Zora NPCs
  - Bombchu Shop (Back Alley, Castle Town, night only): Bombchu (10) for 100/100/180
  - Diving Game: 20 rupees; Silver Scale reward (dive depth ~6 meters)
  - Deliver Ruto's Letter in a Bottle to King Zora → Empty Bottle #1 + access to Zora's Fountain
  - Zora's Fountain: Lord Jabu-Jabu, Great Fairy Fountain (Farore's Wind — dungeon warp spell, deferred until magic in Phase 8)
  - Zora Shop: Arrows (20/60/90), Deku Nuts (15), Fish (200), Red Potion (50), Recovery Heart (10), Zora Tunic (300 — adult only, deferred)
  - Oxygen Gauge: countdown timer underwater without Zora Tunic (~40 sec), drowning on depletion
- Inside Jabu-Jabu's Belly:
  - Organic interior dungeon with rising/falling water, electrified surfaces
  - Offer Fish to Jabu-Jabu to enter
  - Boomerang (dungeon item): stuns enemies, retrieves distant items, returns to Link
  - Princess Ruto: follows Link, must be carried to certain locations
  - Enemies: Biri (1 HP, electric jellyfish — Boomerang only), Stinger (2 HP, floor/ceiling manta), Shabom (1 HP, floating bubble), Octorok
  - Barinade boss: 13 HP; 3 phases — destroy ceiling tentacles (Boomerang), spinning jellyfish shield (Boomerang stun → sword), faster direct attacks
  - Zora's Sapphire (third Spiritual Stone)
- Song of Time: learned from Zelda in cutscene after collecting all 3 Spiritual Stones; Ocarina of Time replaces Fairy Ocarina
  - Song of Time (C→, A, C↓, C→, A, C↓): opens Door of Time, moves/reveals Song of Time blocks

### Assets

**3D Models**
- Kakariko Village environment (houses, windmill, well, watchtower, paths)
- Graveyard environment (tombstones, Royal Family Tomb interior)
- Death Mountain Trail environment (rocky path, boulders)
- Goron City (multi-level cavern interior)
- Sacred Forest Meadow environment
- Lost Woods environment (forest rooms with directional exits)
- Dodongo's Cavern (lava/cave dungeon)
- Zora's River environment
- Zora's Domain (waterfall cavern)
- Zora's Fountain (open water area)
- Inside Jabu-Jabu's Belly (organic dungeon)
- Bomb Bag, Bomb, Bombchu, Bomb Flower
- Boomerang
- Goron's Bracelet, Silver Scale
- Goron's Ruby, Zora's Sapphire
- Ocarina of Time (replaces Fairy Ocarina)
- Ruto's Letter (in bottle)
- Darunia, King Zora, Princess Ruto (child)
- Goron NPCs, Zora NPCs, Kakariko NPCs
- Cucco
- Saria
- Baby Dodongo, Dodongo, Lizalfos, Armos, Beamos
- Red Tektite, Octorok, Poe, Wolfos
- Biri, Stinger, Shabom
- King Dodongo, Barinade
- ReDead
- Goron Shop interior, Zora Shop interior, Bombchu Shop interior

**Animations**
- Child Link: throw bomb, boomerang throw/catch, carry Ruto, swim (surface), dive
- Bomb: fuse spark → explosion
- Bombchu: scuttle along ground/wall
- Boomerang: spin in flight, return arc
- Lizalfos: sword swing, block, jump away, tag-team swap
- King Dodongo: roll, fire breath, inhale, stunned, death
- Barinade: tentacle whip, spin, jellyfish detach, death
- Princess Ruto: idle, carried, thrown, dialogue gestures
- Darunia: stomp dance (Saria's Song), give bracelet
- King Zora: slide aside (grant passage)
- Cucco: idle peck, carried flutter, revenge swarm flight
- Poe: appear, spin lantern, disappear, death
- ReDead: idle sway, shriek (paralyze), grab attack
- Wolfos: circle, guard, claw swipe, backstrike stagger, death

**VFX**
- Bomb explosion (blast wave + smoke)
- Bombchu trail sparks
- Cracked wall/floor destruction debris
- Boomerang spin trail
- Electric shock (Biri contact)
- Barinade electric field
- Bomb Flower regrow
- Lava bubble eruption (Dodongo's Cavern)
- Poe lantern flame
- ReDead paralyze scream wave
- Oxygen Gauge depletion warning flash

**Audio**
- Bomb fuse + explosion SFX
- Bombchu scuttle SFX
- Cracked wall break SFX
- Boomerang throw + whoosh + catch SFX
- Swim SFX (stroke, dive, surface)
- Saria's Song melody
- Sun's Song melody
- Song of Time melody
- Dodongo's Cavern dungeon BGM
- King Dodongo boss BGM
- Jabu-Jabu's Belly dungeon BGM
- Barinade boss BGM
- Kakariko Village BGM
- Death Mountain Trail BGM
- Goron City BGM
- Zora's Domain BGM
- Lost Woods BGM
- Baby Dodongo, Dodongo, Lizalfos, Armos, Beamos SFX
- Biri electric crackle, Stinger SFX, Shabom pop
- Poe SFX (laugh, lantern swing, death)
- ReDead shriek SFX
- Wolfos SFX (howl, claw swipe, death)
- Cucco cluck, revenge swarm SFX
- King Dodongo SFX (roar, roll rumble, fire breath, inhale, death)
- Barinade SFX (electric buzz, tentacle snap, spin whir, death)
- Ocarina of Time acquisition fanfare
- Oxygen Gauge warning beep
- Drowning SFX

**UI**
- Oxygen Gauge (below Magic Meter, underwater only)
- Spiritual Stone slots on Quest Status (3 stones tracked)

---

## Phase 8 — Time Travel & Adult Link

The Master Sword pull, 7-year time skip, Adult Link with adjusted movement, era switching, the magic system, Great Fairy spells, Hookshot, and Sheik — establishing the adult-era foundation.

- Temple of Time interior:
  - Door of Time opens when Song of Time is played (3 Spiritual Stones on altar)
  - Pedestal of Time: pull Master Sword → 7-year time skip cutscene
  - Chamber of Sages: meet Rauru (Light Sage → Light Medallion)
- Adult Link:
  - New model, taller proportions, deeper voice
  - Walk speed 9.0 units/frame (vs child 8.25)
  - Backwalk/roll 13.5 units/frame
  - Cannot crawl through small holes
  - Master Sword (2 damage base, replaces Kokiri Sword)
- Era switching: return Master Sword to pedestal → revert to child; pull again → adult
- Hyrule Castle Town (adult era): ruined, no shops, ReDeads roaming, Poe Collector NPC under guardhouse
- Magic system:
  - Magic Meter appears (green bar below hearts) — obtained from Great Fairy (Death Mountain Summit, visited in Phase 7 but reward activates now)
  - Magic Jar drops (small = partial, large = full restore)
  - Green Potion restores all magic; Blue Potion restores health + magic (available in shops)
  - Charged Spin Attack: hold B with magic → extended range magic spin (Great Fairy upgrade)
- Spells now functional:
  - Din's Fire (obtained Phase 6): AoE fire dome, costs magic
  - Farore's Wind (obtained Phase 7): set warp point in dungeon, reuse to return; costs magic
  - Nayru's Love: damage immunity barrier, continuous magic drain — deferred to Phase 11 (Desert Colossus Great Fairy)
- Hookshot (from Dampe's Grave Race in Kakariko Graveyard):
  - Equip to C-button, first-person aim
  - Grapple to wooden targets, hookable surfaces → pull Link to target
  - Stun/damage enemies (1 damage)
  - Required to access Forest Temple
- Sheik character:
  - Appears at scripted moments to teach warp songs and deliver story exposition
  - Prelude of Light (C↑, C→, C↑, C→, C←, C↑): warp to Temple of Time
- Song of Storms (A, C↓, C↑, A, C↓, C↑): learned from Phonogram Man in Kakariko Windmill (adult); creates rain, reveals grottos, drains well (child)
- Kakariko Village (adult): expanded layout, new NPCs, Windmill accessible
  - Granny's Potion Shop (behind Potion Shop): Blue Potion (100), Poe (30)
- Great Fairy system fully functional:
  - Play Zelda's Lullaby on Triforce pedestal to summon
  - Death Mountain Summit: Magic Meter + Magic Spin Attack (activates)
  - Hyrule Castle: Din's Fire (already obtained — activates)
  - Zora's Fountain: Farore's Wind (already obtained — activates)
  - Remaining fountains placed in Phases 9–13

### Assets

**3D Models**
- Temple of Time interior (altar, Door of Time, Pedestal of Time, stained glass)
- Adult Link
- Master Sword (in pedestal + equipped)
- Sheik
- Hookshot (with chain)
- Hookshot target (wooden post)
- Dampe's Ghost
- Hyrule Castle Town (ruined adult variant — rubble, decay)
- Kakariko Windmill interior
- Granny's Potion Shop interior
- Phonogram Man
- Granny (Potion Shop NPC)
- Rauru (sage)
- Chamber of Sages environment
- Magic Jar (small, large)

**Animations**
- Adult Link: full movement set (walk, run, roll, climb, swim), all sword attacks, shield, dodges, hookshot aim/fire/pull, take damage, death
- Master Sword pull (child → adult transition)
- Master Sword return (adult → child transition)
- Sheik: appear (flash), teach song, disappear (flash)
- Dampe's Ghost: float, toss fire, race movement
- Hookshot: chain extend, retract, pull Link

**VFX**
- Time travel light column (pull/return Master Sword)
- Chamber of Sages golden glow
- Magic Meter fill/drain
- Charged Spin Attack magic wave
- Hookshot chain trail
- Sheik flash appear/disappear (Deku Nut flash)
- Magic Jar pickup sparkle
- Song of Storms rain particles

**Audio**
- Master Sword pull SFX + time skip fanfare
- Temple of Time interior BGM
- Chamber of Sages BGM
- Adult Link voice lines (attack grunts, hurt, jump attack — deeper pitch)
- Hookshot fire + chain SFX + impact SFX
- Magic meter charge SFX
- Charged Spin Attack SFX
- Song of Storms melody
- Prelude of Light melody
- Sheik's theme BGM
- Dampe's Grave Race BGM
- Hyrule Castle Town (ruined) ambient BGM
- Magic Jar pickup SFX
- Kakariko Windmill BGM

**UI**
- Magic Meter bar (top-left, below hearts)
- Medallion slots on Quest Status (Light Medallion filled)
- Adult equipment slots active (swords: Master Sword; shields: Hylian Shield)

---

## Phase 9 — Forest Temple & Fire Temple

The first two adult temples with their dungeon items (Fairy Bow, Megaton Hammer), bosses, and surrounding overworld areas.

- Sacred Forest Meadow (adult):
  - Moblin guards: spear (1 HP, charges in line), club (6 HP, ground-pound)
  - Sheik teaches Minuet of Forest (A, C↑, C←, C→, C←, C→): warp to Sacred Forest Meadow
- Forest Temple:
  - Twisted corridors, courtyard, Poe Sister puzzle (4 torches)
  - Fairy Bow (dungeon item, 30 arrow capacity): first-person aim, 2 damage per arrow
  - Enemies: Stalfos (10 HP, skilled blocker, regenerates if partner alive), Wolfos (reused from Phase 7), Blue Bubble (flaming skull, disables sword on contact), Wallmaster (4 HP, ceiling drop — shadow telegraphs), Floormaster (4 HP, splits into 3 Mini Floormasters on death, 2 HP each), Big Deku Baba (4 HP, larger variant with extended range)
  - Poe Sisters (Joelle, Beth, Amy, Meg): each 8 HP, steal flames from torches; Meg clones herself — real one spins briefly
  - Phantom Ganon boss: ~25 HP; Phase 1 — rides horse through paintings, shoot real one with Bow; Phase 2 — energy tennis (sword deflect), stuns → sword combo
  - Forest Medallion; Saria awakens as Forest Sage
- Death Mountain Crater (adult):
  - Goron Tunic required (heat damage without it)
  - Goron Tunic: gift from Darunia's son in Goron City; or buy (200 Rupees)
  - Sheik teaches Bolero of Fire (C↓, A, C↓, A, C→, C↓, C→, C↓): warp to Death Mountain Crater
- Fire Temple:
  - Lava-filled multi-floor dungeon, falling platforms, timed fire jets
  - Megaton Hammer (dungeon item): 2 damage melee, breaks boulders, activates rusted switches, ground pound shockwave
  - Enemies: Torch Slug (4 HP, flame-covered), Fire Keese (1 HP, burns shield on contact), Like-Like (4 HP, engulfs Link, steals shield/tunic), Door Mimic (bomb-only), Flare Dancer mini-boss (31 HP, spinning fire creature, Hookshot core out → sword)
  - Volvagia boss: 8–12 HP; emerges from lava holes, strike head with Megaton Hammer → sword follow-up; swoops overhead dropping rocks, flies trailing fire
  - Fire Medallion; Darunia awakens as Fire Sage
- Great Fairy Fountain (Death Mountain Crater): Double Magic Meter

### Assets

**3D Models**
- Forest Temple environment (twisted halls, courtyard, basement, boss chamber)
- Fire Temple environment (lava chambers, fire jets, collapsing platforms)
- Death Mountain Crater environment
- Fairy Bow + Arrow
- Megaton Hammer
- Goron Tunic (Adult Link variant)
- Phantom Ganon (+ armored horse)
- Volvagia
- Poe Sisters (Joelle, Beth, Amy, Meg — color-coded)
- Stalfos, Blue Bubble, Wallmaster, Floormaster, Mini Floormaster
- Torch Slug, Fire Keese, Like-Like, Door Mimic
- Big Deku Baba
- Flare Dancer (+ core)
- Moblin (spear), Moblin (club)
- Saria (sage form), Darunia (sage form)
- Paintings (Forest Temple, for Phantom Ganon phase)

**Animations**
- Adult Link: bow aim/fire, hammer swing/ground pound
- Phantom Ganon: horse ride through painting, emerge, energy ball throw, stunned, death
- Volvagia: emerge from hole, head strike stagger, fly swoop, rock drop, fire trail, death
- Stalfos: sword combo, shield block, jump attack, regenerate
- Flare Dancer: spin, core exposed, reform
- Poe Sisters: float, steal flame, disappear, Meg clone split
- Like-Like: engulf, digest, death (items drop)
- Wallmaster: ceiling drop, crawl, death
- Moblin: patrol, charge, ground-pound

**VFX**
- Arrow trail
- Bow aim reticle
- Megaton Hammer ground shockwave
- Lava bubbles and splashes
- Fire jet bursts
- Phantom Ganon energy ball (tennis volley)
- Volvagia fire trail, rock debris
- Forest Temple torch flame (4 colors for Poe Sisters)
- Flare Dancer fire spin
- Like-Like engulf effect
- Sage awakening light (Forest, Fire)

**Audio**
- Bow draw + arrow fire SFX
- Arrow impact SFX
- Megaton Hammer swing + impact SFX + ground shockwave SFX
- Forest Temple dungeon BGM
- Forest Temple boss (Phantom Ganon) BGM
- Fire Temple dungeon BGM
- Fire Temple boss (Volvagia) BGM
- Minuet of Forest melody
- Bolero of Fire melody
- Phantom Ganon SFX (horse gallop, energy ball, laugh, death cry)
- Volvagia SFX (roar, emerge, fire breath, rock scatter, death)
- Stalfos SFX (sword clang, block, death rattle)
- Like-Like SFX (slurp, engulf, spit out)
- Moblin SFX (charge grunt, club slam)
- Flare Dancer SFX (spin whoosh, core clink)
- Sage awakening cutscene BGM
- Heat damage warning SFX (without Goron Tunic)

**UI**
- Arrow ammo counter (alongside Bow C-button icon)
- Quest Status: Forest Medallion + Fire Medallion slots

---

## Phase 10 — Water Temple & Shadow Temple

Two complex dungeons with their pre-requisite mini-dungeons (Ice Cavern, Bottom of the Well), underwater traversal, invisibility mechanics, and Fire Arrows.

- Lake Hylia (child + adult):
  - Child era: accessible from Hyrule Field; Lakeside Laboratory, scarecrow (for Scarecrow's Song in Phase 11), Fishing Pond area (functional in Phase 12)
  - Adult era: drained lake (refills after Water Temple)
  - Sheik teaches Serenade of Water (A, C↓, C→, C→, C←): warp to Lake Hylia
- Ice Cavern (mini-dungeon):
  - Ice puzzles, sliding blocks, frozen enemies
  - Iron Boots: sink underwater, walk on submerged floors, 4.5 units/frame on land
  - Enemies: Freezard (2–3 HP, freezing breath, Din's Fire effective), Ice Keese (1 HP, freezes on contact), White Wolfos (8 HP, backstrike)
- Underwater traversal:
  - Iron Boots + Zora Tunic: walk/fight on lake/river floor
  - Zora's Domain (adult): frozen over; King Zora encased in red ice
  - Blue Fire: bottleable substance from Ice Cavern, melts red ice; use on King Zora to unfreeze
  - Zora Tunic: gift from unfrozen King Zora (breathe underwater indefinitely); or buy 300 Rupees from Zora Shop
  - Underwater controls: analog movement, Hookshot and sword functional
- Water Temple:
  - Water level system: 3 levels (low/mid/high) controlled by playing Zelda's Lullaby at Triforce symbols
  - Longshot (dungeon item): double-range Hookshot, replaces Hookshot
  - Enemies: Spike (1 HP, spiked ball, Hookshot to flip), Shell Blade (1 HP, clam, strike inside mouth), Blue Tektite (4 HP, jumps on water)
  - Dark Link mini-boss: mirrors sword attacks, Megaton Hammer or Biggoron's Sword effective (can't mirror); Din's Fire alternative
  - Morpha boss: 20 HP; water tentacle with nucleus; use Longshot to extract nucleus → sword on ground; multiple tentacles in later phase
  - Water Medallion; Ruto awakens as Water Sage
- Fire Arrows: shoot arrow at sun between pillars at Lake Hylia at dawn → Fire Arrow pickup; arrows that ignite targets, light torches at range, costs magic per shot
- Kakariko Village burning cutscene (triggers after Water Temple):
  - Sheik encounter, Shadow Temple backstory
  - Sheik teaches Nocturne of Shadow (C←, C→, C→, A, C←, C→, C↓): warp to Kakariko Graveyard
- Bottom of the Well (mini-dungeon, child):
  - Requires Song of Storms as child to drain the well
  - Fake walls, invisible paths, acid pools
  - Dead Hand mini-boss (10 HP): hands (8 HP each) grab Link, head emerges to bite
  - Lens of Truth (dungeon item): reveals hidden objects, fake walls, invisible enemies; continuous magic drain
- Shadow Temple:
  - Invisible walls, fake floors, ferry boat ride, wind turbine fans
  - Hover Boots (dungeon item): float over gaps ~1.5 seconds, reduced traction
  - Requires Lens of Truth extensively; Din's Fire to light entrance torches
  - Enemies: Gibdo (8 HP, mummy, paralyzing shriek), ReDead (reused from Phase 7), Wallmaster/Floormaster (reused), Stalfos (reused)
  - Dead Hand mini-boss (reused from Bottom of the Well)
  - Bongo Bongo boss: 36 HP; invisible (Lens of Truth required), two disembodied hands drum the arena causing bouncing; stun each hand with arrow/sword, then strike exposed eye; charges if both hands not stunned simultaneously
  - Shadow Medallion; Impa awakens as Shadow Sage

### Assets

**3D Models**
- Lake Hylia environment (drained + filled variants)
- Lakeside Laboratory
- Ice Cavern environment (ice tunnels, frozen rooms)
- Water Temple environment (multi-level water chambers, Triforce symbols)
- Bottom of the Well environment (underground tunnels, acid pools)
- Shadow Temple environment (invisible maze, boat, wind tunnels, drum arena)
- Iron Boots
- Hover Boots
- Longshot (extended chain)
- Lens of Truth
- Fire Arrow (flaming arrowhead)
- Blue Fire (in bottle, on surfaces)
- Zora Tunic (Adult Link variant)
- Morpha (nucleus + water tentacle)
- Dark Link (shadow doppelganger)
- Bongo Bongo (invisible body, two hands, eye)
- Dead Hand (body + hands)
- Freezard, Ice Keese, White Wolfos
- Spike, Shell Blade, Blue Tektite
- Gibdo
- Ruto (sage form), Impa (sage form)
- Shadow Temple ferry boat
- Bongo drum arena platform

**Animations**
- Adult Link: Iron Boots walk (heavy, slow), Hover Boots float (glide off ledge), Lens of Truth hold-up, underwater walk/sword
- Morpha: tentacle rise/grab/slam, nucleus exposed, nucleus on ground
- Dark Link: mirror all of Link's animations, idle on water surface
- Bongo Bongo: hand drum, hand swipe, hand grab, eye charge, stunned
- Dead Hand: hands reach from floor, head emerge, bite, stunned
- Freezard: idle, breath attack, melt
- Water level rise/fall (Water Temple)
- Ferry boat glide (Shadow Temple)

**VFX**
- Water level transitions (rising/draining)
- Underwater ambient (bubbles, light rays, caustics)
- Blue Fire flame
- Red ice melt
- Iron Boots sink splash
- Hover Boots float shimmer
- Lens of Truth purple aura / reveal shimmer
- Fire Arrow flame trail + ignition
- Morpha water tentacle
- Dark Link shadow aura
- Bongo Bongo invisibility shimmer (visible only through Lens)
- Freezard breath frost cone
- Shadow Temple darkness/fog

**Audio**
- Iron Boots clank SFX (walk on ground, sink into water)
- Hover Boots float hum SFX
- Lens of Truth activation drone
- Fire Arrow ignite SFX
- Water level change rumble SFX
- Underwater ambient loop
- Ice Cavern dungeon BGM
- Water Temple dungeon BGM
- Water Temple boss (Morpha) BGM
- Bottom of the Well dungeon BGM
- Shadow Temple dungeon BGM
- Shadow Temple boss (Bongo Bongo) BGM
- Serenade of Water melody
- Nocturne of Shadow melody
- Morpha SFX (tentacle whoosh, nucleus clink, splash)
- Dark Link SFX (shadow echo of Link's attacks)
- Bongo Bongo SFX (drum pound, hand swipe, charge roar, death)
- Dead Hand SFX (hand grab, bite, death)
- Freezard SFX (breath, shatter)
- Kakariko burning cutscene BGM
- Blue Fire crackle SFX

**UI**
- Quest Status: Water Medallion + Shadow Medallion slots
- Lens of Truth magic drain indicator (meter depleting)
- Boot icon on equipment screen (Iron/Hover/Kokiri swap)
- Tunic icon on equipment screen (Goron/Zora/Kokiri swap)

---

## Phase 11 — Gerudo Region & Spirit Temple

The western desert region, Gerudo Fortress infiltration, the Spirit Temple's unique child/adult dual structure, and the final Sage awakening before Ganon's Castle.

- Gerudo Valley:
  - Broken bridge (cross with Longshot; Epona jump shortcut deferred to Phase 12)
  - Carpenter's tent
- Gerudo Fortress:
  - Infiltration/combat sequence: rescue 4 captured carpenters
  - Gerudo guard enemies: dual-scimitar fighters, throw Link in cell if caught
  - Gerudo Membership Card reward: access to Training Ground, Horseback Archery, Haunted Wasteland
- Gerudo Training Ground (mini-dungeon):
  - Multi-room combat/puzzle gauntlet using items from all prior dungeons
  - Ice Arrows reward: arrows that freeze targets, costs magic; optional
- Haunted Wasteland:
  - Sandstorm navigation (follow Poe guide or use Lens of Truth)
  - Leever enemies (2 HP, burrow/emerge)
  - Carpet Merchant: Bombchu (10) for 200
- Desert Colossus:
  - Spirit Temple entrance
  - Great Fairy Fountain: Nayru's Love (damage immunity barrier, continuous magic drain)
  - Sheik teaches Requiem of Spirit (A, C↓, A, C→, C↓, A): warp to Desert Colossus
- Spirit Temple (dual-era dungeon):
  - Child section: navigate as child Link (warp to Desert Colossus via Requiem, enter child-side)
  - Iron Knuckle mini-boss (child section): 14–15 HP, heavily armored axe-wielder, 2 phases (armored slow → unarmored fast)
  - Silver Gauntlets: push large silver blocks, lift gray rocks (obtained after child-section Iron Knuckle)
  - Adult section: light puzzles, mirror reflections, Anubis enemies
  - Mirror Shield (dungeon item): reflects light beams and certain magical attacks
  - Enemies: Anubis (fire only — mirrors Link's movement, Din's Fire or lure into flame), Iron Knuckle (reused), Armos (reused), Beamos (reused), Like-Like (reused), Floormaster (reused)
  - Twinrova boss: Phase 1 — Koume (fire, 4 HP) and Kotake (ice, 4 HP) attack separately, absorb one element with Mirror Shield, redirect at other sister; Phase 2 — merged Twinrova (~24 HP), absorb 3 of same element → auto-release counter-blast → sword while stunned
  - Spirit Medallion; Nabooru awakens as Spirit Sage
- Scarecrow's Song: teach 8-note melody to scarecrow at Lake Hylia as child, recall as adult → Pierre (hookable scarecrow) appears at specific spots

### Assets

**3D Models**
- Gerudo Valley environment (canyon, bridge, river, tent)
- Gerudo Fortress (multi-building complex, cells, rooftops)
- Gerudo Training Ground (combat/puzzle rooms)
- Haunted Wasteland environment (sand dunes, sandstorm, oasis)
- Desert Colossus environment (temple facade, fairy fountain)
- Spirit Temple environment (child and adult wings, light puzzle rooms, boss chamber)
- Mirror Shield
- Ice Arrow (frozen arrowhead)
- Silver Gauntlets
- Nayru's Love barrier orb
- Gerudo Membership Card
- Gerudo guards (dual-scimitar)
- Iron Knuckle (armored + unarmored states)
- Anubis
- Koume, Kotake, Twinrova (merged form)
- Leever
- Carpet Merchant
- Poe guide (Wasteland)
- Nabooru, Nabooru (sage form)
- Carpenters (4)
- Pierre (scarecrow)
- Light beam emitters, mirror puzzle reflectors

**Animations**
- Adult Link: Mirror Shield reflect (angle light beam), Silver Gauntlets push heavy block
- Iron Knuckle: axe swing, axe slam, armor break transition, unarmored fast attacks, death
- Anubis: hover, mirror Link's movement, fire death
- Koume/Kotake: broom flight, fire/ice magic attack
- Twinrova (merged): combined attack, stunned, death
- Gerudo guards: scimitar combo, spot Link, throw-in-cell
- Leever: burrow, emerge, spin
- Pierre: appear/disappear at Hookshot spots

**VFX**
- Mirror Shield light beam reflection
- Light beam puzzle (beam travels through room, hits targets)
- Ice Arrow freeze effect + frozen enemy
- Nayru's Love diamond barrier
- Sandstorm particles (Haunted Wasteland)
- Silver Gauntlets strength glow
- Koume fire magic, Kotake ice magic
- Twinrova combined beam
- Mirror Shield element absorption glow (fire orange, ice blue)
- Iron Knuckle armor shatter
- Anubis fire death

**Audio**
- Spirit Temple dungeon BGM
- Spirit Temple boss (Twinrova) BGM
- Gerudo Valley BGM
- Gerudo Fortress BGM
- Haunted Wasteland ambient BGM
- Desert Colossus BGM
- Requiem of Spirit melody
- Ice Arrow freeze SFX
- Nayru's Love activation + barrier hum SFX
- Mirror Shield reflect SFX
- Iron Knuckle SFX (armor clank, axe swing, armor break, death)
- Anubis SFX (hover hum, fire death screech)
- Twinrova SFX (cackle, merge, combined attack, death)
- Gerudo guard SFX (alert shout, scimitar clash, capture)
- Leever SFX (burrow, emerge, spin)
- Carpenter rescue jingle
- Sage awakening cutscene BGM

**UI**
- Quest Status: Spirit Medallion slot
- Gerudo Membership Card display on Quest Status
- Ice Arrow magic cost indicator

---

## Phase 12 — Epona, Minigames & Side Quests

The mounted gameplay system, all minigames, both trading sequences, and the three major collection systems — the optional content layer that rewards exploration.

- Epona mount system:
  - Summon with Epona's Song in Hyrule Field (adult)
  - 6-carrot meter: A-press consumes one carrot for burst (20.0 units/frame); carrots regen one at a time ~3 sec after last use; all 6 regen simultaneously after ~7 sec if fully depleted
  - Normal gallop: 12–14 units/frame
  - Steer with analog; no sword while mounted; Fairy Bow usable from horseback
  - Auto-jump fences when approaching head-on at gallop speed
- Lon Lon Ranch (adult):
  - Ingo race: 2 laps around corral; win → Epona; Ingo cheats at start; fence jump to escape
  - Super Cucco Game (child, Talon): find 3 special Cuccos in flock → Empty Bottle #2
- Minigames:
  - Bombchu Bowling (Castle Town, child, 30 Rupees): 10 Bombchus, hit 3 targets; rotating prizes: Bigger Bomb Bag (30 capacity), Piece of Heart, Bombs, Bombchu, Purple Rupee
  - Treasure Chest Game (Castle Town, child, night, 10 Rupees): pick correct chests; Lens of Truth guarantees wins; Piece of Heart prize
  - Shooting Gallery — child (Castle Town, 20 Rupees): Slingshot targets; Deku Seed Bag upgrade (40 capacity)
  - Shooting Gallery — adult (Kakariko, 20 Rupees): Bow targets; Big Quiver (40 arrows)
  - Horseback Archery (Gerudo Fortress, adult, 20 Rupees): 1000+ pts → Piece of Heart; 1500+ pts → Biggest Quiver (50 arrows)
  - Fishing Pond (Lake Hylia, 20 Rupees): child record >7 lbs → Piece of Heart; adult record >14 lbs → Golden Scale (dive depth ~10 meters)
  - Dampe's Grave Race — Piece of Heart for completing under 1 minute (Hookshot already obtained in Phase 8)
  - Marathon Man Race (Hyrule Field, adult): no actual prize (always loses by 1 second)
- Happy Mask Trading (child):
  - Happy Mask Shop (Castle Town): borrow mask → sell to NPC → pay shop → unlock next
  - Keaton Mask → Death Mountain Guard (sell 15, cost 10)
  - Skull Mask → Skull Kid in Lost Woods (sell 10, cost 20 — Link pays difference)
  - Spooky Mask → Graveyard Boy (sell 30, cost 30)
  - Bunny Hood → Running Man in Hyrule Field (sell fills wallet, cost 50)
  - Completing all 4 unlocks: Mask of Truth (read Gossip Stones), Goron Mask, Zora Mask, Gerudo Mask (cosmetic)
  - Deku Stick capacity upgrade: wear Skull Mask at Forest Stage (10 → 20); hidden grotto (20 → 30)
  - Deku Nut capacity upgrade: wear Mask of Truth at Forest Stage (20 → 30); hidden grotto (30 → 40)
- Biggoron's Sword Trading (adult):
  - Chain: Pocket Egg → Pocket Cucco → Cojiro → Odd Mushroom (timed) → Odd Potion → Poacher's Saw → Broken Goron's Sword → Prescription → Eyeball Frog (timed) → World's Finest Eye Drops (timed) → Claim Check → Biggoron's Sword
  - Biggoron's Sword: 4 damage, unbreakable, two-handed (no shield)
  - Giant's Knife available from Medigoron in Goron City (200 Rupees, breaks after ~8 hits) — exists as inferior alternative
- Gold Skulltula Collection:
  - 100 Gold Skulltulas placed throughout world (overworld, dungeons, grottos)
  - Kill → token drop; deliver tokens to Skulltula family in Kakariko Village
  - Rewards: 10 tokens → Adult's Wallet (200); 20 → Stone of Agony (rumble near grottos); 30 → Giant's Wallet (500); 40 → Bombchu (10); 50 → Piece of Heart; 100 → Huge Rupee (200, repeatable)
- Big Poe Collection:
  - 10 Big Poes at fixed trigger points in Hyrule Field (adult, on Epona)
  - Two arrow hits each; catch soul in bottle
  - Deliver all 10 to Poe Collector → Empty Bottle #4
- Magic Bean Planting:
  - 10 soft soil patches across Hyrule
  - Buy from Bean Seller at Zora's River (child): prices 10, 20, 30… 100 Rupees (550 total)
  - Plant as child → grown leaf platforms as adult → reach Heart Pieces and Skulltulas
- Cucco collection side quest (Kakariko, child): collect all 7 Cuccos for Cucco Lady → Empty Bottle #3
- 36 Piece of Heart placements across all regions, dungeons, minigames, and side quests
- Remaining capacity upgrades:
  - Deku Seed Bag: bullseye targets in Lost Woods (40 → 50)
  - Bomb Bag: Goron City rolling Goron stop (30), Bombchu Bowling (already listed above)

### Assets

**3D Models**
- Adult Epona (rideable)
- Epona saddle + carrot meter visual
- Fishing rod + fish (various sizes)
- Fishing Pond environment (dock, pond)
- Bombchu Bowling Alley interior
- Shooting Gallery interiors (child + adult variants)
- Treasure Chest Game interior
- Happy Mask Shop interior
- Masks: Keaton, Skull, Spooky, Bunny Hood, Mask of Truth, Goron, Zora, Gerudo
- Biggoron's Sword, Giant's Knife (+ broken variant)
- Biggoron (giant Goron atop Death Mountain)
- Trading sequence items (Pocket Egg, Cojiro, Odd Mushroom, Poacher's Saw, Broken Sword, Prescription, Eyeball Frog, Eye Drops, Claim Check)
- Gold Skulltula + Token
- Big Poe
- Magic Bean + grown leaf platform
- Soft soil patch
- Ingo (adult, racing outfit)
- Running Man, Marathon Man
- Skull Kid (Lost Woods)
- Cucco Lady
- Bean Seller
- Poe Collector (under guardhouse)

**Animations**
- Adult Link: mount Epona, ride (idle, gallop, sprint), dismount, horseback bow aim/fire
- Epona: walk, trot, gallop, sprint, jump fence, idle, rear up (whip), refuse (bad angle)
- Fishing: cast, reel, fish struggle, catch celebration
- Gold Skulltula: cling to wall, death, token drop
- Big Poe: fast float, circle, disappear/reappear
- Magic Bean plant: grow sequence (child → adult)
- Leaf platform: hover, ferry path

**VFX**
- Epona gallop dust
- Carrot use burst
- Fish splash (pond)
- Gold Skulltula token glow
- Big Poe soul capture (bottle)
- Magic Bean sprout growth
- Mask equip face overlay

**Audio**
- Epona gallop SFX (walk, trot, gallop, sprint — hoof variants)
- Epona neigh, snort
- Epona fence jump SFX
- Carrot whip SFX
- Fishing rod cast + reel SFX
- Fish bite + struggle SFX
- Fish caught fanfare
- Horseback Archery target hit SFX
- Bombchu Bowling lane SFX
- Shooting Gallery hit/miss SFX
- Gold Skulltula SFX (cling, death, token chime)
- Big Poe SFX (cackle, fast whoosh)
- Mask equip SFX
- Trading item exchange jingle
- Ingo race BGM
- Fishing Pond BGM
- Minigame win/lose jingles
- Lon Lon Ranch BGM (adult variant)

**UI**
- Carrot meter (6 carrots, bottom of screen while mounted)
- Fishing weight display
- Horseback Archery score counter
- Shooting Gallery hit counter
- Gold Skulltula Token counter on Quest Status
- Mask inventory on Item subscreen
- Trading item slot on Item subscreen
- Piece of Heart counter on Quest Status

---

## Phase 13 — Ganon's Castle & Finale

The endgame dungeon, both final boss fights, the castle collapse escape, and the ending sequence — the climax of the entire game.

- Light Arrows: received from Zelda at Temple of Time after all 5 temples complete; Sheik reveals as Zelda cutscene; Ganondorf captures Zelda
  - 2 damage, costs large magic chunk per shot
  - Enemies defeated by Light Arrows drop Purple Rupees
  - Required to break Ganon's Castle barriers and stun Ganondorf/Ganon
- Ganon's Castle:
  - Floating castle above ruined Castle Town, light bridge from sages
  - 6 barrier rooms (Forest, Fire, Water, Shadow, Spirit, Light): each uses items/skills from corresponding temple
  - Enemies: mixed roster from all prior dungeons (Stalfos, Dinolfos [12 HP, evolved Lizalfos with fire breath], Iron Knuckle, Like-Like, Freezard, Beamos, ReDeads, Torch Slug, Wallmaster, Floormaster, Armos)
  - Dinolfos (new enemy): 12 HP, fire breath, Deku Nut stun + jump attack effective
  - Golden Gauntlets: found in Shadow Barrier room; lift massive black granite pillars
  - Great Fairy Fountain (outside Ganon's Castle): Enhanced Defense (halves all incoming damage, white heart outlines)
  - Barrier dispel: Light Arrow at barrier core in each room → sage seals barrier
- Ganondorf boss fight (Castle Tower summit):
  - Energy tennis: deflect energy ball with sword until stuns him → shoot Light Arrow → platform drop → sword combo
  - Spread attack: 5 energy orbs → Spin Attack to reflect all, or preemptive Light Arrow
  - Arena edge platforms collapse
  - Organ BGM transitions to boss BGM on engagement
- Castle collapse escape:
  - 3-minute countdown timer
  - Zelda leads through crumbling halls; Link clears obstacles (boulders, barriers)
  - ReDead ambushes, Stalfos pair (must kill quickly)
  - Fire barriers Zelda opens with magic
  - If Link is hit by debris, Zelda stops and waits
- Ganon beast form:
  - Phase 1: massive beast with dual swords; knocks Master Sword behind fire ring
  - Light Arrow to stun face → roll between legs → strike tail with Biggoron's Sword or Megaton Hammer
  - Stomps cause shockwave; wide sword sweeps
  - Phase 2: fire ring drops (Zelda's intervention); recover Master Sword
  - Continue Light Arrow → tail strikes with any weapon
  - Final blow: Zelda holds Ganon with magic → Master Sword to the head (only Master Sword works for final blow)
- Ending sequence: sages seal Ganondorf, Zelda sends Link back in time, credits
- Post-credits: saving returns to last save before Ganon's Castle (no New Game+)

### Assets

**3D Models**
- Ganon's Castle exterior (floating castle, light bridge)
- Ganon's Castle interior (6 barrier rooms, central tower, staircase, boss chamber, collapse rubble)
- Golden Gauntlets
- Light Arrow (radiant arrowhead)
- Ganondorf (human form, caped, at organ)
- Ganon (beast form — massive bipedal boar, dual swords)
- Dinolfos
- Castle collapse debris (falling rocks, crumbling walls, fire barriers)
- Zelda (adult, white dress)
- Triforce visual
- Black granite pillars (Golden Gauntlets targets)

**Animations**
- Ganondorf: organ playing, float, energy ball throw, spread attack, Light Arrow stun, fall to platform, defeated
- Ganon: roar, dual sword swings, stomp shockwave, Master Sword knockaway, Light Arrow head stun, tail strike stagger, final blow reaction, sealed
- Zelda (adult): lead through halls, magic barrier open, hold Ganon with magic, send Link back
- Castle collapse: ceiling crumble, floor crack, wall fall
- Adult Link: final blow (Master Sword overhead strike)
- Sages: seal Ganondorf in Sacred Realm

**VFX**
- Light Arrow radiant trail + impact flash
- Ganondorf energy ball (yellow-white orb)
- Ganondorf spread attack (5 orb fan)
- Barrier room energy walls (6 colors per element)
- Barrier dispel explosion (per element)
- Sage seal beams (6 colored beams)
- Ganon transformation dark energy
- Ganon fire ring
- Castle collapse fire + dust + debris particles
- Zelda's magic hold (golden chains on Ganon)
- Triforce glow (on hands of Link, Zelda, Ganondorf)
- Enhanced Defense white heart overlay
- Ending sky clear / rainbow
- Credits backdrop

**Audio**
- Ganon's Castle dungeon BGM
- Ganondorf organ prelude BGM
- Ganondorf boss BGM
- Castle collapse BGM (urgent, timed)
- Ganon boss BGM
- Ending / credits BGM
- Light Arrow fire + radiant impact SFX
- Ganondorf energy ball SFX (throw, volley, stun)
- Ganondorf spread attack SFX
- Barrier dispel SFX
- Ganondorf defeated SFX (scream)
- Ganon transformation roar
- Ganon SFX (roar, sword swing, stomp shockwave, stun, tail hit, final blow scream)
- Castle collapse rumble + debris SFX
- Zelda voice lines (lead escape, hold Ganon, farewell)
- Sages seal chant
- Master Sword knockaway clatter
- Countdown timer tick SFX
- Dinolfos SFX (fire breath, sword, death)

**UI**
- Collapse countdown timer (3:00, center-top)
- Quest Status: all 6 Medallions filled
- Enhanced Defense white heart outlines on HUD

---

## Phase 14 — Content Completion & Polish

All remaining collectible placements, secret grottos, Gossip Stones, NPC dialogue, and the final audio/polish pass across every region.

- Secret grottos: all hidden holes placed across Hyrule (revealed by Bombs, Song of Storms, specific triggers); contents include fairy fountains, Gold Skulltulas, upgrade chests, small shops, enemy gauntlets
- Stone of Agony functionality: controller rumble when near hidden grottos (item obtained from Phase 12 Skulltula rewards)
- Gossip Stone placements across all regions: strike to show time, Mask of Truth to read hints, Bombs to launch
- Remaining NPC dialogue: all non-critical NPCs across all areas (both eras) with contextual responses to story progress
- Verification pass: all 36 Piece of Heart placements, 100 Gold Skulltula placements, capacity upgrades, Great Fairy Fountains, shop inventories, warp song destinations, and boss title cards functional and accessible
- Ambient SFX pass: wind, water, lava, insects, birds per region
- Region BGM day/night variant coverage audit

### Assets

**3D Models**
- Gossip Stone
- Grotto interiors (fairy pool variant, enemy gauntlet variant, shop variant, chest variant)
- Grotto entrance hole (hidden/revealed states)
- Remaining minor NPCs (Hyrule Field wanderers, Castle Town residents, region-specific)
- Forest Stage environment

**VFX**
- Gossip Stone hit flash (shows time)
- Gossip Stone bomb rocket launch trail
- Grotto entrance reveal (ground crumble)
- Stone of Agony rumble visual indicator

**Audio**
- Gossip Stone hit chime
- Gossip Stone Mask of Truth read SFX
- Gossip Stone bomb rocket SFX
- Grotto fall SFX
- Stone of Agony rumble SFX
- Ambient SFX per region: wind (Hyrule Field, Gerudo), water (Zora areas, Lake Hylia), lava (Death Mountain, Fire Temple), insects (Kokiri Forest, Lost Woods), birds (Kakariko)
- Remaining NPC voice barks where applicable
