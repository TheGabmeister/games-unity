# Ocarina of Time — Enemy & Boss Compendium

Companion doc to [SPEC.md](../SPEC.md) §7.

---

## Enemy HP & Weapon Damage Reference

### Weapon Damage Values

| Weapon | Damage |
|--------|--------|
| Kokiri Sword | 1 |
| Deku Stick (swing) | 2 |
| Master Sword | 2 |
| Megaton Hammer | 2 |
| Fairy Bow (Arrow) | 2 |
| Fire Arrow | 2 |
| Light Arrow | 2 |
| Giant's Knife / Biggoron's Sword | 4 |
| Ice Arrow | 4 |
| Din's Fire | 4 |
| Fairy Slingshot (Deku Seed) | 1 |
| Bomb / Bombchu | 2 |
| Hookshot / Longshot | 1 |
| Boomerang | 0–1 (stun) |
| Deku Nut | 0 (stun only) |
| Spin Attack (Kokiri Sword) | 2 |
| Spin Attack (Master Sword) | 4 |
| Jump Attack (doubles base) | 2× |

### Damage Modifiers

- **Jump Attack**: doubles the base weapon damage.
- **Spin Attack**: doubles the base weapon damage.
- Other modifiers (Enhanced Defense, Nayru's Love) covered in SPEC.md §1.2 and §6.6.

---

## Common Enemies

### Overworld — Child Link

| Enemy | HP | Location(s) | Behavior | Weakness |
|-------|-----|-------------|----------|----------|
| Deku Baba (withered) | 1 | Kokiri Forest, dungeons | Stationary, circles head | Any weapon |
| Deku Baba (lunging) | 2 | Various | Lunges when approached; stands rigid when struck | Sword, Slingshot |
| Deku Scrub | — | Various | Pops from ground, spits Deku Nuts | Reflect projectile back |
| Skulltula (wall) | 1 | Dungeons, overworld | Climbs walls; soft belly exposed when turning | Strike exposed belly |
| Big Skulltula (hanging) | 2 | Dungeons | Hangs from ceiling, spins | Wait for belly, then strike |
| Gold Skulltula | 2 | 100 locations worldwide | Clings to walls, produces token | Any weapon |
| Keese | 1 | Caves, dungeons | Bat; swoops at Link | Any weapon (one hit) |
| Fire Keese | 1 | Fire-related areas | Burns shield on contact | Ranged first, or sword |
| Ice Keese | 1 | Ice Cavern, cold areas | Freezes Link on contact | Ranged preferred |
| Stalchild | 2 | Hyrule Field (night, child only) | Emerges from ground at night | Sword; despawn at dawn |
| Octorok | 1 | Zora's River, water areas | Spits rocks from water | Reflect projectile or Slingshot |
| Red Tektite | 2 | Death Mountain Trail | Jumps toward Link | Sword |
| Blue Tektite | 4 | Lake Hylia, water areas | Jumps on water surfaces | Sword, arrows |
| Peahat | 6 | Hyrule Field | Flies using propeller leaves; roots are weak point | Strike roots when grounded |
| Mini Peahat | 1 | Spawned by Peahat | Homes toward Link | Any weapon |
| Guay | 1 | Hyrule Field (night), Lake Hylia | Swooping black birds | Any weapon |
| Wolfos | 8 | Lost Woods, Forest Temple area | Blocks frontal attacks; circles Link | Strike when guard drops; backstrike |
| Poe | 8 | Graveyard, Hyrule Field | Appears/disappears; lantern attack | Z-target, wait for materialization, arrow |
| Baby Dodongo | 1 | Dodongo's Cavern | Crawls toward Link; explodes on death | Sword (dodge explosion) |
| Beamos | 1–2 | Dodongo's Cavern, Spirit Temple | Stationary laser turret; 360° sweep | Bomb to destroy |
| Armos | 1 | Dodongo's Cavern, Spirit Temple | Statue that activates on contact; hops and explodes | Bomb, Deku Nut |
| Shabom | 1 | Jabu-Jabu's Belly | Floating bubble | Any weapon |
| Biri | 1 | Jabu-Jabu's Belly | Electric jellyfish | Boomerang (sword shocks Link) |
| Stinger | 2 | Jabu-Jabu's Belly | Manta ray on floor/ceiling | Boomerang, Sword |

### Overworld — Adult Link

| Enemy | HP | Location(s) | Behavior | Weakness |
|-------|-----|-------------|----------|----------|
| Big Deku Baba | 4 | Various | Larger, extended range | Sword |
| Moblin (spear) | 1 | Sacred Forest Meadow | Charges in straight line | Sidestep and strike |
| Moblin (club) | 6 | Sacred Forest Meadow | Ground-pound with hammer | Attack from behind |
| Skull Kid | — | Lost Woods | Fires projectiles | Sword/arrow; drops 200 Rupees |
| Gibdo | 8 | Shadow Temple, Beneath the Well | Mummy; paralyzing shriek | Sun's Song freezes; sword/fire |
| ReDead | 8 | Royal Tomb, Castle Town (adult) | Paralyzing shriek; jumps on Link | Sun's Song freezes; sword |
| Leever | 2 | Haunted Wasteland | Burrows underground, emerges to attack | Sword when surfaced |
| Big Poe | — | Hyrule Field (10 locations, on Epona) | Fast ghost; requires mounted archery | Two arrow hits on Epona |

### Dungeon Enemies — Adult Link

| Enemy | HP | Location(s) | Behavior | Weakness |
|-------|-----|-------------|----------|----------|
| Stalfos | 10 | Forest Temple, Spirit Temple, Ganon's Castle | Armored skeleton; skilled blocker; regenerates if partner not killed quickly | Shield + counter-attack; kill pairs fast |
| Lizalfos | 6 | Dodongo's Cavern, Water Temple | Tag-team swordfighter; jumps away | Z-target, counter after attack |
| Dinolfos | 12 | Gerudo Training Ground, Ganon's Castle | Evolved Lizalfos; fire breath | Deku Nut stun + jump attack |
| Wallmaster | 4 | Forest Temple, Shadow Temple | Drops from ceiling (shadow visible) | Move from shadow, strike when landed |
| Floormaster | 4 | Forest Temple, Shadow Temple | Crawling hand; splits into 3 minis on death | Kill minis quickly or they regenerate |
| Mini Floormaster | 2 | Forest Temple, Shadow Temple | Small hands from split Floormaster | Quick sword strikes |
| Like-Like | 4 | Various dungeons | Engulfs Link; steals Deku Shield, Hylian Shield, or tunic | Kill quickly to recover stolen items; arrows at range |
| Torch Slug | 4 | Fire Temple | Flame-covered slug | Hit to extinguish, then strike; Hookshot |
| Shell Blade | 1 | Water Temple | Clam that charges with open mouth | Strike inside open mouth |
| Spike (spiked ball) | 1 | Water Temple | Bouncing spiked sphere | Hookshot to flip, then sword |
| Freezard | 2–3 | Ice Cavern, Ganon's Castle | Ice creature; freezing breath | Din's Fire, sword, arrows |
| White Wolfos | 8 | Ice Cavern | Stronger wolf variant | Backstrike; jump attack |
| Blue Bubble | — | Forest Temple | Flaming skull; disables sword on contact | Shield block removes flame, then sword |
| Anubis | — | Spirit Temple | Mirrors Link's movement | Fire only (Din's Fire or lure into flame) |
| Iron Knuckle | 14–15 | Spirit Temple, Ganon's Castle | Heavily armored axe-wielder; slow but devastating | Bait attack, dodge, counter (2 phases: armored → fast) |
| Door Mimic | — | Fire Temple | Disguised as door; drops when opened | Bomb |

---

## Mini-Bosses

| Mini-Boss | HP | Dungeon | Mechanic | Strategy |
|-----------|-----|---------|----------|----------|
| Dead Hand | 10 | Beneath the Well, Shadow Temple | Hands (8 HP each) grab Link; head emerges to bite | Let hand grab, attack head when it lowers |
| Flare Dancer | 31 | Fire Temple | Spinning fire creature; core exposed when doused | Hookshot core out, then sword |
| Dark Link | — | Water Temple | Shadow doppelganger; mirrors sword attacks | Megaton Hammer (can't mirror); Din's Fire; Biggoron's Sword |

---

## Bosses

### Child Link Bosses

Dungeon Item = key item found mid-dungeon (not from the boss). Boss drops a Heart Container.

| Boss | Title | HP | Dungeon | Dungeon Item | Quest Reward |
|------|-------|-----|---------|-------------|--------------|
| Queen Gohma | Parasitic Armored Arachnid | 10 | Inside the Deku Tree | Fairy Slingshot | Kokiri's Emerald |
| King Dodongo | Infernal Dinosaur | 12 | Dodongo's Cavern | Bomb Bag | Goron's Ruby |
| Barinade | Bio-Electric Anemone | 13 | Inside Jabu-Jabu's Belly | Boomerang | Zora's Sapphire |

**Queen Gohma**: Single phase. Climbs ceiling and drops eggs (Gohma Larvae, 2 HP each). Stun with Slingshot/Deku Nut when eye turns red, then sword. 3 cycles on Normal.

**King Dodongo**: Rolls into ball and charges; breathes fire in arc. Throw Bomb into mouth when inhaling, then sword stunned body. 4 bomb cycles.

**Barinade**: Three phases. Phase 1: destroy tentacles anchoring to ceiling (Boomerang). Phase 2: spinning with jellyfish shield; Boomerang to stun, then sword. Phase 3: faster spinning, direct attacks.

### Adult Link Bosses

| Boss | Title | HP | Dungeon | Dungeon Item | Quest Reward |
|------|-------|-----|---------|-------------|--------------|
| Phantom Ganon | Evil Spirit from Beyond | ~25 | Forest Temple | Fairy Bow | Forest Medallion |
| Volvagia | Subterranean Lava Dragon | 8–12 | Fire Temple | Megaton Hammer | Fire Medallion |
| Morpha | Giant Aquatic Amoeba | 20 | Water Temple | Longshot | Water Medallion |
| Bongo Bongo | Phantom Shadow Beast | 36 | Shadow Temple | Hover Boots | Shadow Medallion |
| Twinrova | Sorceress Sisters | 4 each / ~24 combined | Spirit Temple | Mirror Shield | Spirit Medallion |

**Phantom Ganon**: Phase 1 — rides horse through paintings; shoot the real one emerging (not the fake). Phase 2 — tennis match: deflect energy ball with sword until it stuns him, then sword combo. Drops to ground periodically.

**Volvagia**: Emerges from lava holes. Strike head with Megaton Hammer when it surfaces, then follow with sword. Also swoops overhead dropping rocks; flies around arena trailing fire.

**Morpha**: Nucleus in water tentacle. Use Longshot to extract nucleus from tentacle, then sword on ground. Tentacles grab and throw Link. Multiple tentacles in later phase.

**Bongo Bongo**: Invisible (Lens of Truth required). Two disembodied hands drum the arena causing bouncing. Stun each hand with arrow/sword, then strike exposed eye. Charges if both hands aren't stunned simultaneously.

**Twinrova**: Phase 1 — Koume (fire) and Kotake (ice) attack separately. Use Mirror Shield to absorb one element, redirect at the other sister. Phase 2 — they merge into Twinrova; absorb 3 of the same element with Mirror Shield to auto-release a counter-blast, then sword while stunned.

### Final Bosses

**Ganondorf** (Ganon's Castle Tower):
- Tennis match with energy balls (sword deflect). After stunning, shoot Light Arrow → drop to platform → sword combo. He also fires a spread of 5 energy orbs (Spin Attack to reflect all, or Light Arrow preemptively). Arena edge platforms can collapse.

**Ganon** (Collapsed Castle):
- Phase 1: Massive beast form. Knocks Master Sword away behind fire ring. Use Light Arrow to stun face → roll between legs → strike tail with Biggoron's Sword or Megaton Hammer. Uses dual swords; stomps cause shockwave.
- Phase 2: Fire ring drops (Zelda's intervention); recover Master Sword. Continue Light Arrow + tail strikes. Final blow: Zelda holds Ganon with magic, Master Sword to the face.

---

## Enemy Behavior Notes

- **Stalchildren** only spawn on Hyrule Field at night as child Link; they do not appear if Link stands on the road or wears the Bunny Hood.
- **ReDeads and Gibdos** freeze in place when Sun's Song is played; effect is temporary.
- **Like-Likes** steal equipped Deku Shield or Hylian Shield and current tunic (not Kokiri Tunic). Kill before they digest to recover items.
- **Iron Knuckles** have two phases: armored (slow, 2 damage per hit) and unarmored (fast, armor plates break off after enough damage).
- **Poes** become invisible when Z-targeted; wait for them to materialize and attack, then counter.
- **Anubis** cannot be damaged by physical attacks; only fire kills them.
- **Gold Skulltulas** appear more frequently at night; many require specific items or songs to reach.
