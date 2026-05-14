# The Legend of Zelda: Ocarina of Time — Gameplay Systems Spec

Nintendo 64, 1998. Speccing the original N64 NTSC release (v1.0–v1.2).

---

## 1. Core Gameplay Systems

### 1.1 Primary Loop

Explore overworld → discover dungeon → solve puzzles & defeat enemies → acquire dungeon item → defeat boss → earn quest reward (Spiritual Stone or Sage Medallion) → unlock new overworld access → repeat.

The game is split into two eras separated by a 7-year time skip. Child Link (age ~10) collects three Spiritual Stones; Adult Link (age ~17) awakens six Sages across five temples plus Ganon's Castle.

### 1.2 Health System

- Health is measured in **Hearts** (Heart Containers).
- Link starts with **3 Hearts**.
- Each dungeon boss drops **1 Heart Container** (8 total: 3 child dungeons + 5 adult temples). Ganon's Castle does not award one.
- **36 Pieces of Heart** are scattered throughout the world; every 4 pieces form 1 Heart Container (9 additional containers).
- Maximum health: **20 Hearts** (3 starting + 8 boss + 9 from pieces).
- Damage can be taken in quarter-heart (1/4), half-heart (1/2), or full-heart increments depending on enemy attack.
- **Enhanced Defense** (Great Fairy upgrade): halves all incoming damage; visually indicated by white outlines on hearts.

### 1.3 Magic System

- **Magic Meter**: unlocked from Great Fairy on Death Mountain summit.
- **Double Magic**: upgrade from Great Fairy in Death Mountain Crater; doubles meter capacity.
- Spells and special arrows consume magic per use.
- Magic is restored by collecting green Magic Jars dropped by enemies/pots, or by Green/Blue Potions.

### 1.4 Combat System

**Z-Targeting** (lock-on):
- Press Z to lock onto the nearest targetable enemy or NPC. Navi flies to the target. Camera centers on the target; Link auto-faces the target and can strafe.
- While locked, sword attacks are directed at the target.
- Cycle targets by releasing and re-pressing Z.

**Melee Attacks** (B button / context):
| Attack | Input | Effect |
|--------|-------|--------|
| Horizontal Slash | B | Standard swing |
| Vertical Slash | Z + forward + B | Overhead chop |
| Thrust/Stab | Z + B (standing still) | Forward stab |
| Jump Attack | Z + A | Leaping overhead slash; 2× base damage |
| Spin Attack | Hold B, release (or rotate stick + B) | 360° sweep; 2× base damage |
| Charged Spin Attack | Hold B longer (with magic) | Extended range magic spin (requires Great Fairy upgrade) |

**Ranged Attacks**: Fairy Slingshot (child), Fairy Bow (adult), Hookshot/Longshot, Boomerang (child), Din's Fire.

**Defensive Options**:
| Action | Input | Effect |
|--------|-------|--------|
| Shield (hold) | R | Block attacks; reflect certain projectiles |
| Backflip | Z + back + A | Quick backward dodge |
| Side Hop | Z + left/right + A | Quick lateral dodge |
| Roll | Forward + A | Dodge through some attacks; 1.5× run speed |

**Context-Sensitive A Button**: Changes dynamically based on situation — Open, Grab, Climb, Talk, Check, Jump, Dive, Throw, Drop, Mount. On-screen text label updates to show current action.

### 1.5 Movement & Traversal

| Movement Type | Speed (units/frame) | Notes |
|---------------|---------------------|-------|
| Walk (Adult, Kokiri Boots) | 9.0 | Base movement |
| Walk (Child) | 8.25 | Slightly slower |
| Walk (Iron Boots) | 4.5 | Very slow; sinks underwater |
| Walk (Hover Boots) | 8.25 | Reduced traction |
| Backwalk / Roll | 13.5 (adult) | 1.5× walk speed |
| Sidehop | 12.75 | Z-target evasion |
| Epona Gallop | 12–14 | Standard riding |
| Epona Sprint (carrot) | 20.0 | Burst speed |

- **Auto-Jump**: Link automatically jumps when running off ledges. Distance scales with momentum.
- **Climbing**: Link grabs ledges, ladders, vines, and walls automatically. Climb speed is fixed.
- **Swimming**: Surface swimming with A-button dive. B taps for faster strokes. Underwater traversal requires Iron Boots + Zora Tunic.
- **Diving**: Silver Scale (child) and Golden Scale (adult) extend max dive depth. Without Zora Tunic, an **Oxygen Gauge** appears underwater — Link drowns when it depletes (~40 seconds). Zora Tunic grants unlimited air.
- **Fall Damage**: Link takes damage from long falls. Damage scales with distance: short drops deal 1/2 heart, extreme drops deal up to 4 hearts. Rolling on landing negates fall damage.
- **Epona (mounted)**: 6-carrot meter. Each A-press consumes one carrot for a speed burst. Carrots regenerate one at a time, ~3 seconds after the last whip. If all 6 are depleted, all regenerate simultaneously after ~7 seconds. Epona auto-jumps fences when approaching head-on at gallop speed.

---

## 2. Controls & Input

Platform: Nintendo 64 controller.

### 2.1 Button Layout

| Button | Function |
|--------|----------|
| Control Stick | Move Link / navigate menus |
| A | Context action (Jump, Roll, Open, Talk, Climb, Grab, Dive) |
| B | Sword attack; cancel in menus |
| Z (trigger) | Z-Targeting lock-on; flatten note pitch on Ocarina |
| R | Raise shield; sharpen note pitch on Ocarina |
| L | Toggle minimap on/off |
| C-Up | First-person look (Navi view); call Navi for hints |
| C-Left / C-Down / C-Right | Use equipped item (3 assignable slots) |
| Start | Pause / subscreen menu |
| D-Pad | Not used in gameplay |

### 2.2 Context-Sensitive States

**On foot (default)**: A = Roll (while running), Talk, Open, Grab, Check.
**Z-Targeting**: A = Jump Attack; stick directions = strafe/backflip/sidehop.
**Swimming (surface)**: A = Dive; B = faster swim stroke.
**Underwater (Iron Boots)**: Analog movement; Hookshot/Sword functional.
**Riding Epona**: A = use carrot for speed burst (see §1.5 for carrot system); analog = steer; no sword; can use Bow from horseback.
**Ocarina mode**: C-buttons + A = notes; R = sharp; Z = flat; Control Stick = vibrato/pitch bend.

---

## 3. World Structure

### 3.1 Overworld Layout

Hyrule is a contiguous overworld centered on **Hyrule Field**, a large open hub that connects all major regions.

| Region | Direction from Hyrule Field | Key Features |
|--------|---------------------------|--------------|
| Kokiri Forest | Southeast | Starting area; Deku Tree dungeon; Lost Woods beyond |
| Hyrule Castle Town / Temple of Time | North | Market, shops, Castle, Temple of Time |
| Kakariko Village | Northeast (base of Death Mountain) | Town, Graveyard, Windmill, Bottom of the Well |
| Death Mountain | Northeast (above Kakariko) | Goron City, Death Mountain Trail, Crater, Fire Temple |
| Zora's River / Zora's Domain | East | Waterfall entrance, Zora's Fountain, Jabu-Jabu |
| Lake Hylia | South | Fishing Pond, Water Temple, Lakeside Laboratory |
| Lon Lon Ranch | Center of Hyrule Field | Epona, Ingo race, milk |
| Gerudo Valley / Fortress | West | Bridge, Gerudo Fortress, Haunted Wasteland |
| Desert Colossus | Far West (past Wasteland) | Spirit Temple, Great Fairy |

The **Lost Woods** connects Kokiri Forest to the Sacred Forest Meadow (Forest Temple entrance) and Goron City via a winding path with directional audio cues.

### 3.2 Time of Day Cycle

- Day lasts approximately **2 minutes 30 seconds** real time.
- Night lasts approximately **2 minutes 10 seconds** real time.
- Full day/night cycle: ~4 minutes 40 seconds.
- Sun's Song instantly toggles between day and night.
- Night triggers: Stalchildren on Hyrule Field (child), Poes in Graveyard, shops close in Castle Town, drawbridge raises.
- Time does not pass inside dungeons, buildings, or certain areas.

### 3.3 Time Travel

- Pulling the Master Sword from the Pedestal of Time warps Link forward 7 years (child → adult).
- Returning the sword to the pedestal warps back (adult → child).
- Certain puzzles require solving parts in both eras (notably Spirit Temple).
- Warp songs learned as adult allow instant travel to temple regions in either era (see §6.4).

### 3.4 Dungeon Structure

Each dungeon contains:
- **Dungeon Map**: reveals room layout.
- **Compass**: reveals chest locations, boss room, and Link's position.
- **Small Keys**: consumable keys for locked doors (dungeon-specific).
- **Boss Key**: single key to unlock the boss door.
- **Dungeon Item**: unique item found mid-dungeon that enables further progress.

#### Dungeon Sequence

| # | Dungeon | Era | Dungeon Item | Boss | Quest Reward |
|---|---------|-----|-------------|------|--------------|
| 1 | Inside the Deku Tree | Child | Fairy Slingshot | Queen Gohma | Kokiri's Emerald |
| 2 | Dodongo's Cavern | Child | Bomb Bag | King Dodongo | Goron's Ruby |
| 3 | Inside Jabu-Jabu's Belly | Child | Boomerang | Barinade | Zora's Sapphire |
| 4 | Forest Temple | Adult | Fairy Bow | Phantom Ganon | Forest Medallion |
| 5 | Fire Temple | Adult | Megaton Hammer | Volvagia | Fire Medallion |
| 6 | Water Temple | Adult | Longshot | Morpha | Water Medallion |
| 7 | Shadow Temple | Adult | Hover Boots | Bongo Bongo | Shadow Medallion |
| 8 | Spirit Temple | Both | Mirror Shield | Twinrova | Spirit Medallion |
| 9 | Ganon's Castle | Adult | — (all prior items) | Ganondorf / Ganon | — |

**Mini-Dungeons** (no boss key / medallion):
- **Bottom of the Well** (child): yields Lens of Truth.
- **Ice Cavern** (adult): yields Iron Boots.
- **Gerudo Training Ground** (adult): yields Ice Arrows (optional).

### 3.5 Overworld Systems

- **Cucco Revenge**: Attacking a Cucco ~3 times triggers an invincible swarm of Cuccos that deal rapid damage until Link leaves the area.
- **Secret Grottos**: Hidden holes in the ground revealed by Bombs, Song of Storms, or specific triggers. Contain fairy fountains, Gold Skulltulas, item upgrades, small shops, or enemy gauntlets. Stone of Agony (§6.7) causes controller rumble near hidden grottos.
- **Gossip Stones**: Scattered stone monuments. Strike with sword to display the time. Wearing the Mask of Truth lets Link read their hints. Bombs cause them to launch like rockets.

### 3.6 Gating Mechanics

Progression is gated by items and story triggers:
- Bombs open cracked walls; Hookshot reaches distant targets and platforms.
- Goron Tunic allows entry to volcanic areas; Zora Tunic enables underwater exploration.
- Iron Boots required for Water Temple underwater sections.
- Lens of Truth required to navigate Shadow Temple illusions.
- Mirror Shield required for Spirit Temple light puzzles.
- Songs gate travel and access (Zelda's Lullaby opens Royal Family doors, Song of Time moves blocks).
- Silver Gauntlets (Spirit Temple) and Golden Gauntlets (Ganon's Castle) required to move massive blocks (§6.1).
- Light Arrows required to dispel barriers in Ganon's Castle.

---

## 4. Playable Character

Link is the sole playable character, appearing in two forms:

| Attribute | Child Link | Adult Link |
|-----------|-----------|------------|
| Age | ~10 | ~17 |
| Default Sword | Kokiri Sword | Master Sword |
| Shield Options | Deku Shield, Hylian Shield (worn on back; too large to crouch behind) | Hylian Shield, Mirror Shield |
| Exclusive Items | Slingshot, Boomerang, Deku Sticks, Magic Beans | Fairy Bow, Hookshot/Longshot, Megaton Hammer, Hover Boots, Iron Boots |
| Shared Items | Bombs, Bombchu, Ocarina, Lens of Truth, Din's Fire, Farore's Wind, Nayru's Love, Bottles |
| Can Ride Epona | No | Yes |
| Crawl Spaces | Yes (small holes) | No |

---

## 5. Story & Progression

### 5.1 Act Structure

**Act 1 — Child Link (Spiritual Stones)**:
1. Kokiri Forest: receive quest from Great Deku Tree; clear Deku Tree dungeon → Kokiri's Emerald.
2. Hyrule Castle: meet Princess Zelda; receive Zelda's Letter; learn Zelda's Lullaby from Impa.
3. Death Mountain: befriend Gorons; clear Dodongo's Cavern → Goron's Ruby.
4. Zora's Domain: deliver letter to King Zora; enter Jabu-Jabu → Zora's Sapphire.
5. Temple of Time: play Song of Time to open Door of Time; pull Master Sword → 7-year time skip.

**Act 2 — Adult Link (Sage Medallions)**:
1. Awaken as adult in Chamber of Sages; meet Rauru (Light Sage → Light Medallion).
2. Forest Temple → awaken Saria (Forest Sage).
3. Fire Temple → awaken Darunia (Fire Sage).
4. Water Temple → awaken Ruto (Water Sage).
5. Shadow Temple → awaken Impa (Shadow Sage). Preceded by Kakariko Village burning.
6. Spirit Temple → awaken Nabooru (Spirit Sage). Requires child/adult era switching.

**Act 3 — Ganon's Castle**:
1. Six barrier rooms corresponding to six medallions.
2. Ganondorf boss fight (energy tennis + Light Arrows).
3. Castle collapse escape sequence (3-minute timer; Zelda leads, Link clears obstacles and enemies).
4. Ganon beast form final battle.

### 5.2 The Seven Sages

| Sage | Element | Race | Medallion |
|------|---------|------|-----------|
| Rauru | Light | Hylian | Light Medallion |
| Saria | Forest | Kokiri | Forest Medallion |
| Darunia | Fire | Goron | Fire Medallion |
| Ruto | Water | Zora | Water Medallion |
| Impa | Shadow | Sheikah | Shadow Medallion |
| Nabooru | Spirit | Gerudo | Spirit Medallion |
| Zelda | Time/Wisdom | Hylian | Reveals as 7th Sage at finale |

### 5.3 No Alternate Endings / No New Game+

Single ending. No New Game+ mode. Saving after final boss returns to last save point before Ganon's Castle.

---

## 6. Items & Equipment

### 6.1 Equipment (Gear Subscreen)

#### Swords

| Sword | Damage | Availability | How to Obtain |
|-------|--------|-------------|---------------|
| Kokiri Sword | 1 | Child only | Found in Kokiri Forest (chest behind crawl space) |
| Master Sword | 2 | Adult only | Pull from Pedestal of Time |
| Giant's Knife | 4 | Adult only | Buy from Medigoron in Goron City (200 Rupees); breaks after ~8 hits |
| Biggoron's Sword | 4 | Adult only | Adult Trading Sequence reward; unbreakable; two-handed (no shield) |

#### Shields

| Shield | Properties | How to Obtain |
|--------|-----------|---------------|
| Deku Shield | Burns on fire contact; can be eaten by Like-Like | Buy (40 Rupees) Kokiri Shop |
| Hylian Shield | Fireproof; too large for child (wears on back) | Buy (80 Rupees) Bazaar; free in Graveyard |
| Mirror Shield | Reflects light beams and certain magic | Spirit Temple |

#### Tunics

| Tunic | Effect | How to Obtain |
|-------|--------|---------------|
| Kokiri Tunic | None (default) | Default |
| Goron Tunic | Heat resistance (survive Death Mountain Crater / Fire Temple) | Gift from Darunia's son; or buy (200 Rupees) Goron Shop |
| Zora Tunic | Breathe underwater indefinitely | Gift from King Zora (unfreeze with Blue Fire); or buy (300 Rupees) Zora Shop |

#### Boots

| Boots | Effect | How to Obtain |
|-------|--------|---------------|
| Kokiri Boots | None (default) | Default |
| Iron Boots | Sink underwater; walk on submerged floors; very slow on land | Ice Cavern |
| Hover Boots | Float over gaps for ~1.5 seconds; reduced ground traction | Shadow Temple (after Dead Hand mini-boss) |

#### Gauntlets (Strength Upgrades)

| Gauntlets | Effect | How to Obtain |
|-----------|--------|---------------|
| Goron's Bracelet | Pick up Bomb Flowers | Gift from Darunia (child) |
| Silver Gauntlets | Push large silver blocks; lift gray rocks | Spirit Temple (child section, chest after Iron Knuckle) |
| Golden Gauntlets | Lift massive black granite pillars | Ganon's Castle (Shadow Barrier room) |

#### Scales (Dive Upgrades)

| Scale | Effect | How to Obtain |
|-------|--------|---------------|
| Silver Scale | Dive deeper (~6 meters) | Zora's Domain Diving Game (child) |
| Golden Scale | Dive deepest (~10 meters) | Fishing Pond record catch as adult (>14 lbs) |

### 6.2 C-Button Items (Usable)

#### Child-Only Items

| Item | Found In | Function |
|------|----------|----------|
| Fairy Slingshot | Deku Tree | Ranged weapon; fires Deku Seeds |
| Boomerang | Jabu-Jabu's Belly | Stuns enemies; retrieves distant items; returns to Link |
| Deku Stick | Various | Melee weapon (2 damage, breaks after one hit); can carry fire |

#### Adult-Only Items

| Item | Found In | Function |
|------|----------|----------|
| Fairy Bow | Forest Temple | Ranged weapon; fires Arrows |
| Fire Arrow | Lake Hylia (shoot sun between pillars) | Arrows that ignite; light torches at range; costs magic |
| Ice Arrow | Gerudo Training Ground | Arrows that freeze; costs magic; optional |
| Light Arrow | Temple of Time (from Zelda) | Most powerful arrow; required vs Ganondorf/Ganon; costs magic |
| Hookshot | Kakariko Graveyard (Dampe's Grave race) | Grapple to wooden targets / hookable surfaces; stuns enemies |
| Longshot | Water Temple | Double-range Hookshot; replaces Hookshot |
| Megaton Hammer | Fire Temple | Melee weapon (2 damage); breaks boulders; activates rusted switches |

#### Shared Items (Both Eras)

| Item | Found In | Function |
|------|----------|----------|
| Bombs | Dodongo's Cavern (Bomb Bag) | Explode after fuse; destroy cracked walls/floors |
| Bombchu | Various shops/chests | Self-propelled mouse-shaped bomb; climbs walls |
| Ocarina (Fairy → Ocarina of Time) | Saria / Zelda | Play songs for magical effects (§6.4) |
| Lens of Truth | Bottom of the Well | Reveals hidden objects, fake walls, invisible enemies; drains magic |
| Din's Fire | Great Fairy (Hyrule Castle) | AoE fire blast; required to light Shadow Temple torches |
| Farore's Wind | Great Fairy (Zora's Fountain) | Set/return to warp point within a dungeon; optional |
| Nayru's Love | Great Fairy (Desert Colossus) | Damage immunity shield; drains magic; optional |
| Deku Nut | Various | Thrown flash-bang; stuns most enemies |
| Magic Beans | Bean Seller (Zora's River) | Plant in soft soil as child; grows into rideable leaf platform as adult |

### 6.3 Bottles

4 Empty Bottles obtainable:

| # | How to Obtain |
|---|---------------|
| 1 | Deliver Ruto's Letter to King Zora (main quest) |
| 2 | Win Talon's Super Cucco Game at Lon Lon Ranch |
| 3 | Collect all 7 Cuccos for the woman in Kakariko Village |
| 4 | Deliver 10 Big Poes to the Poe Collector in Hyrule Castle Town (adult) |

**Bottle Contents**: Red/Green/Blue Potion, Lon Lon Milk (2 uses), Fairy (auto-revive), Fish, Bugs, Blue Fire, Poe Soul, Big Poe Soul.

**Combat use**: A swung bottle can deflect energy projectiles (same as sword).

### 6.4 Ocarina Songs

| Song | Notes (N64) | Learned From | Effect |
|------|-------------|-------------|--------|
| Zelda's Lullaby | C←, C↑, C→, C←, C↑, C→ | Impa | Activates Triforce symbols; proves Royal Family connection |
| Epona's Song | C↑, C←, C→, C↑, C←, C→ | Malon (Lon Lon Ranch) | Summons Epona (adult); produces milk from cows |
| Saria's Song | C↓, C→, C←, C↓, C→, C← | Saria (Sacred Forest Meadow) | Communicate with Saria for hints; cheers up certain NPCs |
| Sun's Song | C→, C↓, C↑, C→, C↓, C↑ | Royal Family Tomb (Graveyard) | Toggles day/night; paralyzes ReDeads/Gibdos |
| Song of Time | C→, A, C↓, C→, A, C↓ | Zelda (cutscene at Temple of Time) | Opens Door of Time; moves/reveals Song of Time blocks |
| Song of Storms | A, C↓, C↑, A, C↓, C↑ | Phonogram Man (Kakariko Windmill, adult) | Creates rain; reveals some secret grottos; drains well (child) |
| Minuet of Forest | A, C↑, C←, C→, C←, C→ | Sheik (Forest Temple entrance) | Warp to Sacred Forest Meadow |
| Bolero of Fire | C↓, A, C↓, A, C→, C↓, C→, C↓ | Sheik (Death Mountain Crater) | Warp to Death Mountain Crater |
| Serenade of Water | A, C↓, C→, C→, C← | Sheik (Lake Hylia, after Ice Cavern) | Warp to Lake Hylia |
| Nocturne of Shadow | C←, C→, C→, A, C←, C→, C↓ | Sheik (Kakariko Village, burning) | Warp to Kakariko Graveyard |
| Requiem of Spirit | A, C↓, A, C→, C↓, A | Sheik (Desert Colossus) | Warp to Desert Colossus |
| Prelude of Light | C↑, C→, C↑, C→, C←, C↑ | Sheik (Temple of Time, adult) | Warp to Temple of Time |
| Scarecrow's Song | Player-composed (8 notes) | Teach to scarecrow as child; recall as adult | Summons Pierre (hookable scarecrow) at specific spots |

### 6.5 Capacity Upgrades

| Item | Base | Upgrade 1 | Upgrade 2 | How |
|------|------|-----------|-----------|-----|
| Deku Seeds | 30 | 40 | 50 | Hit bullseye targets in Lost Woods |
| Deku Sticks | 10 | 20 | 30 | Forest Stage (Skull Mask) + hidden grotto |
| Deku Nuts | 20 | 30 | 40 | Forest Stage (Mask of Truth) + hidden grotto |
| Bombs | 20 | 30 | 40 | Bomb Bag upgrades from Goron City / Bombchu Bowling |
| Arrows | 30 | 40 | 50 | Big Quiver (Kakariko Shooting Gallery) / Biggest Quiver (Horseback Archery) |
| Wallet | 99 | 200 (Adult's) | 500 (Giant's) | 10 / 30 Gold Skulltula Tokens |
| Magic | Base | Double Magic | — | Great Fairy (Death Mountain Crater) |

### 6.6 Great Fairy Upgrades

| Location | Reward |
|----------|--------|
| Death Mountain Summit | Magic Meter + Magic Spin Attack |
| Death Mountain Crater | Double Magic Meter |
| Hyrule Castle Grounds | Din's Fire |
| Zora's Fountain | Farore's Wind |
| Desert Colossus | Nayru's Love |
| Outside Ganon's Castle | Enhanced Defense (halves all damage) |

All require playing Zelda's Lullaby on a Triforce pedestal to summon the Great Fairy.

### 6.7 Quest Status Items

Items displayed on the Quest Status subscreen that aren't equippable to C-buttons or the gear screen.

| Item | How to Obtain | Effect |
|------|---------------|--------|
| Gerudo Membership Card | Free all 4 carpenters from Gerudo Fortress | Access to Gerudo Training Ground, Horseback Archery, and Haunted Wasteland |
| Stone of Agony | 20 Gold Skulltula Tokens | Rumble Pak vibrates near hidden grottos and secrets |

---

## 7. Enemies & Opponents

See companion doc: [docs/enemies.md](docs/enemies.md) for the full enemy compendium, HP tables, weapon damage values, and boss breakdowns.

### 7.1 Enemy Design Summary

- ~50 distinct enemy types across child and adult eras.
- Enemies generally telegraph attacks; Z-targeting + shield + counter is the core combat loop.
- Many enemies have hard-counter items (Anubis = fire only; Biri = Boomerang; Beamos = Bomb).
- Bosses are puzzle-fights: use the dungeon's new item to expose the weak point, then attack with sword.
- Mini-bosses guard dungeon items or appear as gatekeepers (Dead Hand, Flare Dancer, Dark Link, Iron Knuckle).

### 7.2 Difficulty Scaling

- Child dungeons have simpler enemy AI and lower HP pools.
- Adult temples increase enemy count, HP, and introduce enemies immune to basic sword (Anubis, Freezard groups).
- Ganon's Castle combines enemies from all prior temples.
- No selectable difficulty mode.

---

## 8. Economy

See companion doc: [docs/shops.md](docs/shops.md) for full shop inventories, rupee denominations, wallet tiers, potion effects, and income sources.

**Rupees** are the sole currency (Green=1, Blue=5, Red=20, Purple=50, Huge=200). Wallet capacity upgrades are in §6.5.

### 8.1 Key Purchases

Most key equipment can be obtained for free through quests; shop purchase is a fallback or alternative:

| Item | Shop Price | Free Alternative |
|------|-----------|-----------------|
| Deku Shield | 40 | None (must buy at game start) |
| Hylian Shield | 80 | Kakariko Graveyard chest |
| Goron Tunic | 200 | Gift from Darunia's son |
| Zora Tunic | 300 | Gift from King Zora (unfreeze with Blue Fire) |
| Giant's Knife | 200 | Biggoron's Sword via trading quest (superior, unbreakable) |
| Blue Potion | 100 | — |
| Bombchu (10) | 100–200 | Dungeon chests, Bombchu Bowling prizes |

---

## 9. Minigames & Side Systems

### 9.1 Minigames

| Minigame | Location | Cost | Reward(s) |
|----------|----------|------|-----------|
| Bombchu Bowling | Hyrule Castle Town (child) | 30 | Bigger Bomb Bag, Piece of Heart, Bombs, Bombchu, Purple Rupee (rotating prizes) |
| Treasure Chest Game | Hyrule Castle Town (child, night) | 10 | Up to Piece of Heart (requires Lens of Truth for guaranteed wins) |
| Shooting Gallery (child) | Hyrule Castle Town | 20 | Deku Seed Bag upgrade, repeat: free Deku Seeds |
| Shooting Gallery (adult) | Kakariko Village | 20 | Big Quiver (40 arrows) |
| Horseback Archery | Gerudo Fortress (adult) | 20 | 1000+ pts: Piece of Heart; 1500+ pts: Biggest Quiver (50 arrows) |
| Fishing Pond | Lake Hylia | 20 | Child record (>7 lbs): Piece of Heart; Adult record (>14 lbs): Golden Scale |
| Dampe's Grave Race | Kakariko Graveyard (adult) | Free | Hookshot (first clear); Piece of Heart (under 1 minute) |
| Marathon Man Race | Hyrule Field / Gerudo Valley (adult) | Free | No actual prize (he always beats you by 1 second) |
| Super Cucco Game | Lon Lon Ranch (child) | Free | Empty Bottle |
| Diving Game | Zora's Domain (child) | 20 | Silver Scale (first clear), Purple Rupee (repeat) |

### 9.2 Trading Sequences

**Child — Happy Mask Trading**:
Borrow masks from Happy Mask Shop → sell to NPCs → pay back shop → unlock next mask.

| Order | Mask | Sell To | Sell Price | Cost |
|-------|------|---------|-----------|------|
| 1 | Keaton Mask | Death Mountain Guard | 15 | 10 |
| 2 | Skull Mask | Skull Kid (Lost Woods) | 10 | 20 (Link pays difference) |
| 3 | Spooky Mask | Graveyard Boy (Kakariko) | 30 | 30 |
| 4 | Bunny Hood | Running Man (Hyrule Field) | Fills wallet | 50 |

Completing all four unlocks: **Mask of Truth** (read Gossip Stones), plus Goron Mask, Zora Mask, Gerudo Mask (cosmetic only).

**Deku Stick upgrade**: wear Skull Mask at Forest Stage.
**Deku Nut upgrade**: wear Mask of Truth at Forest Stage.

**Adult — Biggoron's Sword Trading Sequence**:
Chain of 10 timed/untimed item trades across Hyrule. Several items have time limits (warping voids timed items). Final reward: **Biggoron's Sword** (4 damage, unbreakable, two-handed).

Trade chain: Pocket Egg → Pocket Cucco → Cojiro → Odd Mushroom (timed) → Odd Potion → Poacher's Saw → Broken Goron's Sword → Prescription → Eyeball Frog (timed) → World's Finest Eye Drops (timed) → Claim Check → Biggoron's Sword.

### 9.3 Gold Skulltula Collection

100 Gold Skulltulas hidden throughout the world (overworld, dungeons, grottos). Killing one drops a token; tokens cure members of the cursed Skulltula family in Kakariko Village.

| Tokens | Reward |
|--------|--------|
| 10 | Adult's Wallet (200 capacity) |
| 20 | Stone of Agony (rumble near hidden grottos) |
| 30 | Giant's Wallet (500 capacity) |
| 40 | Bombchu (10) |
| 50 | Piece of Heart |
| 100 | Huge Rupee (200) — repeatable |

### 9.4 Big Poe Collection

10 Big Poes spawn at fixed trigger points in Hyrule Field (adult, on Epona). Catch their souls in bottles, deliver to the Poe Collector under the ruined Castle Town guardhouse. Delivering all 10 rewards an **Empty Bottle**.

### 9.5 Magic Bean Planting

10 soft soil patches across Hyrule. Buy Magic Beans from the Bean Seller at Zora's River as child (price increases per purchase: 10, 20, 30... up to 100 Rupees, 10 total for 550 Rupees). Plant as child → returns as adult to find grown leaf platforms that ferry Link to unreachable areas (often containing Heart Pieces or Gold Skulltulas).

---

## 10. UI & HUD

### 10.1 Gameplay HUD

| Element | Position | Description |
|---------|----------|-------------|
| Heart Containers | Top-left | Row of hearts; fractional damage shown; white outline = Enhanced Defense |
| Magic Meter | Top-left (below hearts) | Green bar; appears after obtaining magic; doubles in length with upgrade |
| B Button icon | Top-right | Shows current sword; context-sensitive (Put Away when sheathed) |
| A Button icon | Top-right (below B) | Shows current context action text (Open, Grab, Talk, Climb, etc.) |
| C-Left / C-Down / C-Right icons | Top-right | Show assigned items with icons |
| Rupee Counter | Top-left (below magic) | Current rupee count with rupee icon |
| Key Counter | Bottom-left | Small Key count for current dungeon (only inside dungeons) |
| Minimap | Bottom-right | Area minimap; toggled with L; shows Link's position and direction |

### 10.2 Dungeon Map Screen

- Floor selector on left side; current floor marked with Link icon.
- Skull icon marks boss room (requires Compass).
- Chest icons show treasure locations (requires Compass).
- Rooms revealed by Dungeon Map acquisition.
- Small Key count and Boss Key indicator displayed.

### 10.3 Subscreen (Start Menu)

Four tabs navigated with L/R:
1. **Map/Dungeon** — World map or dungeon floor viewer.
2. **Quest Status** — Medallions, Spiritual Stones, Songs learned, Heart Piece count, Gold Skulltula count.
3. **Equipment** — Swap swords, shields, tunics, boots.
4. **Items** — Grid of C-button items; assign to C-Left/Down/Right.

### 10.4 Contextual UI

- **Navi indicator**: C-Up icon flashes when Navi has a hint; pressing C-Up shows Navi's text.
- **Z-Target reticle**: yellow arrow above targetable entities; turns blue/green for friendly NPCs.
- **Boss HP bar**: not displayed (bosses show damage via visual/audio feedback, not a health bar).
- **Ocarina staff**: five-line musical staff appears when playing the Ocarina; notes display in real-time.

---

## 11. Engine & Presentation Systems

### 11.1 Camera

- Third-person follow camera by default; auto-adjusts behind Link.
- Z-Targeting locks camera between Link and target.
- C-Up enters first-person look mode (no movement).
- Camera cannot clip through walls; auto-adjusts in tight spaces.
- Dungeon rooms often use fixed or semi-fixed camera angles that shift as Link moves between zones.

### 11.2 Save System

- **3 save files**.
- Save any time via Start menu.
- On reload: child Link spawns at Link's House (Kokiri Forest) or Temple of Time; adult Link spawns at Temple of Time. Dungeon progress (keys, items) is saved, but position resets to dungeon entrance.
- No autosave. No quicksave.
- Game Over screen offers "Continue" (restart from last save with 3 hearts) or "Save and Quit."

### 11.3 Dialogue System

- Text boxes at screen bottom; NPC dialogue advances with A button.
- Some dialogues have binary choices (Yes/No) affecting NPC response but not story branching.
- Navi provides contextual hints via C-Up; Saria provides hints via Saria's Song.
- Gossip Stones provide lore/hints (see §3.5).
- Sheik appears at pre-scripted moments to teach warp songs and deliver story exposition.

### 11.4 Audio System Behavior

- **Overworld themes**: region-specific BGM (Hyrule Field, Kokiri Forest, Gerudo Valley, etc.).
- **Dynamic music**: Hyrule Field theme fades based on proximity to enemies; enemy encounter triggers combat fanfare.
- **Dungeon ambience**: each dungeon has a unique atmospheric track.
- **Ocarina**: real-time audio feedback; played notes produce actual musical tones with pitch/vibrato control.
- **Day/Night transition**: music cross-fades between day and night variants.
- **Boss intro**: unique intro cinematic with title card and boss name.
- **Low health beep**: repeating alarm when Link is at 1 heart or below; stops when healed.

### 11.5 Fairy Companion (Navi)

- Navi hovers near Link; flies to targetable objects/enemies when Z-targeting.
- Pressing C-Up calls Navi for contextual advice about the current area, enemy, or puzzle.
- Navi changes color near points of interest: green = hidden secret or interaction point; yellow = targetable enemy/object.
- Navi periodically calls "Hey!" or "Listen!" to prompt the player; pressing C-Up dismisses.

---

## 12. Open Questions / Unverified

- **Exact enemy damage-to-player values**: community sources list general ranges but some specific enemy attack damage values (e.g., individual Ganondorf attack damage) are unverified across game versions.
- **Swimming speed values**: speedrun community has not published verified swimming speed in units/frame.
- **Dark Link HP**: reported as "mirrors Link's hearts" or "variable"; exact mechanic unconfirmed across sources. May be 40 HP based on decomp data but community sources conflict.
- **Hover Boots float duration**: commonly cited as ~1.5 seconds but exact frame count varies across sources.
- **Nayru's Love duration**: reported as consuming approximately half the magic meter; exact frame duration unverified.
- **Big Poe point values**: Poe Collector reward thresholds and per-Poe point values have minor inconsistencies across guides.
- **Magic Bean price sequence**: most sources say 10→20→30→40→50→60→70→80→90→100 (total 550); some list slightly different sequences.

---

## 13. References

### Wikis & Guides
- Zelda Wiki (Fandom): https://zelda.fandom.com/wiki/
- Zelda Dungeon Wiki: https://www.zeldadungeon.net/wiki/
- StrategyWiki — OoT: https://strategywiki.org/wiki/The_Legend_of_Zelda:_Ocarina_of_Time
- Wikibooks — OoT: https://en.wikibooks.org/wiki/The_Legend_of_Zelda:_Ocarina_of_Time
- Zeldapedia (Archive): https://zelda-archive.fandom.com/wiki/

### Speedrun / Technical
- ZeldaSpeedRuns — Movement Speeds: https://www.zeldaspeedruns.com/oot/generalknowledge/movement-speeds
- ZSR Forums — Enemy HP/Damage: https://forums.zeldaspeedruns.com/index.php?topic=406.0
- CloudModding OoT Wiki — Damage Charts: https://wiki.cloudmodding.com/oot/Damage_Charts

### Decompilation
- zeldaret/oot (GitHub): https://github.com/zeldaret/oot

### Companion Docs
- [docs/enemies.md](docs/enemies.md) — Full enemy compendium, HP tables, boss breakdowns
- [docs/shops.md](docs/shops.md) — Shop inventories, rupee denominations, wallet tiers, pricing
