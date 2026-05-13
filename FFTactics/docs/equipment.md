# Final Fantasy Tactics — Equipment Tables

Companion document to [SPEC.md](../SPEC.md).

---

## Job-to-Equipment Matrix (Generic Jobs)

| Job | Weapons | Shield | Head | Body |
|---|---|---|---|---|
| Squire | Sword, Dagger, Axe, Flail | No | Hat | Clothes |
| Chemist | Dagger, Gun | No | Hat | Clothes |
| Knight | Sword, Knight Sword | Yes | Helmet | Armor, Robe |
| Archer | Bow, Crossbow | Yes | Hat | Clothes |
| Monk | None (barehanded) | No | Hair Adornment | Clothes |
| White Mage | Staff | No | Hat | Robe, Clothes |
| Black Mage | Rod | No | Hat | Robe, Clothes |
| Time Mage | Staff | No | Hat | Robe, Clothes |
| Summoner | Rod, Staff | No | Hat | Robe, Clothes |
| Thief | Dagger | No | Hat | Clothes |
| Orator | Dagger, Gun | No | Hat | Clothes, Robe |
| Mystic | Rod, Staff, Book, Pole | No | Hat | Clothes, Robe |
| Geomancer | Sword, Axe | Yes | Hat | Clothes, Robe |
| Dragoon | Polearm/Spear | Yes | Helmet | Armor, Robe |
| Samurai | Katana | No | Helmet | Armor, Robe |
| Ninja | Ninja Blade, Dagger, Flail | No | Hat | Clothes |
| Arithmetician | Book, Pole | No | Hat | Clothes, Robe |
| Bard | Instrument, Bag | No | Hat | Clothes |
| Dancer | Cloth, Dagger, Bag | No | Hat | Clothes |
| Mime | None | None | None | None |

---

## Shields

| Shield | P-Ev | M-Ev | Price | Special |
|---|---|---|---|---|
| Escutcheon | 10% | 3% | 400 | — |
| Buckler | 13% | 3% | 800 | — |
| Bronze Shield | 16% | 0% | 1200 | — |
| Round Shield | 19% | 0% | 1800 | — |
| Mythril Shield | 22% | 5% | 2500 | — |
| Gold Shield | 25% | 0% | 3600 | — |
| Ice Shield | 28% | 0% | 5000 | Absorb Ice, Half Fire, Weak Lightning |
| Flame Shield | 31% | 0% | 5000 | Absorb Fire, Half Ice, Weak Water |
| Diamond Shield | 34% | 15% | 8000 | — |
| Platina Shield | 37% | 10% | 12000 | — |
| Crystal Shield | 40% | 15% | 16000 | — |
| Genji Shield | 43% | 0% | Rare | — |
| Kaiser Plate | 46% | 20% | Rare | Strengthen Fire/Lightning/Ice |
| Venetian Shield | 50% | 25% | Rare | Half Fire/Lightning/Ice |
| Aegis Shield | 10% | 50% | Rare | Best magical shield |
| Escutcheon II | 75% | 50% | Rare | Best overall shield |

Shield evasion applies to front and side attacks only. Back attacks bypass shield evasion.

---

## Weapon Categories — Summary

### Swords

Standard melee weapons. PA × WP formula.

| Weapon | WP | Price | Element/Special |
|---|---|---|---|
| Broad Sword | 4 | 200 | — |
| Long Sword | 5 | 500 | — |
| Iron Sword | 6 | 900 | — |
| Mythril Sword | 7 | 1600 | — |
| Blood Sword | 6 | Rare | Drains HP |
| Coral Sword | 7 | 4000 | Lightning |
| Ancient Sword | 8 | 4500 | Chance: Don't Act |
| Sleep Blade | 8 | 5000 | Chance: Sleep |
| Icebrand | 9 | 7500 | Ice, 19% spell proc |
| Platinum Sword | 10 | 9000 | — |
| Diamond Sword | 11 | 12000 | — |
| Rune Blade | 12 | 14000 | Half: all elements |
| Moonblade | 13 | Rare | — |
| Save the Queen | 16 | Rare | Protect + Shell always |
| Defender | 14 | Rare | — |
| Nagrarock | 1 | Rare | Chance: Frog |

### Knight Swords

Brave-dependent: [(Br/100) × PA] × WP. Some have spell proc chance (19%).

| Weapon | WP | Price | Element/Special |
|---|---|---|---|
| Defender | 14 | Rare | — |
| Save the Queen | 16 | Rare | Auto Protect + Shell |
| Excalibur | 21 | Rare | Holy, Haste |
| Ragnarok | 20 | Rare | Auto Shell |
| Chaos Blade | 24 | Rare | Auto Regen, inflicts Petrify on wielder |

### Katanas

Brave-dependent: [(Br/100) × PA] × WP. Used for Samurai's Iaido abilities.

| Weapon | WP | Price | Element/Special |
|---|---|---|---|
| Ashura | 7 | 1600 | — |
| Kotetsu | 8 | 3000 | — |
| Osafune | 9 | 4000 | — |
| Murasame | 10 | 5000 | — |
| Ama-no-Murakumo | 11 | 7000 | — |
| Kiyomori | 12 | 8000 | — |
| Muramasa | 14 | 10000 | — |
| Kikuichimonji | 15 | 15000 | — |
| Masamune | 16 | Rare | Haste |
| Chirijiraden | 18 | Rare | — |

### Daggers / Knives

Speed-hybrid: [(PA + Sp) / 2] × WP.

| Weapon | WP | Price | Special |
|---|---|---|---|
| Dagger | 3 | 100 | — |
| Mythril Knife | 4 | 500 | — |
| Blind Knife | 4 | 800 | Chance: Blind |
| Mage Masher | 5 | 1500 | Chance: Silence |
| Platinum Dagger | 6 | 1800 | — |
| Main Gauche | 7 | 3000 | +30% W-Ev |
| Assassin's Dagger | 8 | Rare | Chance: Death Sentence |
| Air Knife | 9 | Rare | Wind element |
| Zorlin Shape | 10 | Rare | — |
| Zwill Straightblade | 12 | Rare | — |

### Guns

Flat damage: WP × WP. Ignores PA, evasion, and facing. Straight-line trajectory.

| Weapon | WP | Price | Special |
|---|---|---|---|
| Romandan Pistol | 6 | 5000 | — |
| Mythril Gun | 8 | 8000 | — |
| Stone Gun | 10 | 12000 | Chance: Petrify |
| Glacier Gun | 14 | Rare | Ice |
| Blaze Gun | 14 | Rare | Fire |
| Blast Gun | 14 | Rare | Lightning |

### Bows

Speed-hybrid: [(PA + Sp) / 2] × WP. Arc trajectory; height extends range.

| Weapon | WP | Price | Special |
|---|---|---|---|
| Long Bow | 4 | 1000 | — |
| Silver Bow | 5 | 2500 | — |
| Ice Bow | 6 | 3500 | Ice |
| Lightning Bow | 6 | 3500 | Lightning |
| Mythril Bow | 7 | 5000 | — |
| Windslash Bow | 8 | 7000 | Wind |
| Yoichi Bow | 9 | Rare | — |
| Perseus Bow | 10 | Rare | — |
| Artemis Bow | 12 | Rare | — |

### Polearms / Spears

PA × WP. 2 tile horizontal reach. Jump damage ×1.5.

| Weapon | WP | Price | Special |
|---|---|---|---|
| Javelin | 6 | 1000 | — |
| Spear | 7 | 1500 | — |
| Mythril Spear | 8 | 2500 | — |
| Partisan | 9 | 4000 | — |
| Ice Lance | 10 | 5000 | Ice |
| Obelisk | 10 | 6000 | — |
| Holy Lance | 12 | Rare | Holy |
| Dragon Whisker | 14 | Rare | — |
| Javelin II | 16 | Rare | — |

### Staves

MA × WP. Boost MA stat.

| Weapon | WP | MA+ | Price | Special |
|---|---|---|---|---|
| Oak Staff | 3 | +1 | 200 | — |
| White Staff | 3 | +1 | 800 | — |
| Healing Staff | 4 | +1 | 1500 | Heals target instead of damaging |
| Wizard Staff | 4 | +2 | 4000 | — |
| Gold Staff | 5 | +2 | 7000 | — |
| Staff of the Magi | 5 | +3 | Rare | — |

### Rods

PA × WP for basic attacks (despite boosting MA stat). Rods can also be broken as items to cast their associated spell.

| Weapon | WP | MA+ | Price | Special |
|---|---|---|---|---|
| Rod | 3 | +1 | 200 | — |
| Thunder Rod | 3 | +1 | 500 | Lightning, casts Bolt |
| Ice Rod | 3 | +1 | 500 | Ice, casts Blizzard |
| Flame Rod | 3 | +1 | 500 | Fire, casts Fire |
| Wizard Rod | 4 | +2 | 8000 | — |
| Dragon Rod | 5 | +3 | Rare | — |
| Faith Rod | 5 | +3 | Rare | — |

---

## Accessory Categories

### Mantles / Cloaks

Provide accessory evasion (applies to physical and magical attacks, all directions including back).

| Accessory | P-Ev | M-Ev | Price | Special |
|---|---|---|---|---|
| Small Mantle | 10% | 10% | 300 | — |
| Leather Mantle | 15% | 15% | 800 | — |
| Wizard Mantle | 18% | 18% | 2000 | MA +1 |
| Elf Mantle | 25% | 25% | 8000 | — |
| Dracula Mantle | 28% | 28% | 15000 | — |
| Feather Mantle | 40% | 30% | 20000 | — |
| Vanish Mantle | 35% | 0% | Rare | Always Invisible |

### Shoes / Boots

Provide movement bonuses.

| Accessory | Price | Effect |
|---|---|---|
| Battle Boots | 1000 | Move +1 |
| Spike Shoes | 1200 | Jump +1 |
| Rubber Shoes | 1500 | Cancel Don't Move; null Lightning |
| Feather Boots | 2500 | Float |
| Germinas Boots | 5000 | Move +1, Jump +1 |
| Sprint Shoes | 7000 | Speed +1 |
| Red Shoes | 10000 | MA +1, Move +1 |

### Gauntlets

Provide stat bonuses.

| Accessory | Price | Effect |
|---|---|---|
| Power Wrist | 5000 | PA +1 |
| Magic Gauntlet | 20000 | MA +2 |
| Bracer | 50000 | PA +3 |
| Genji Gauntlet | Rare | PA +2, MA +2 |

### Rings

Provide status immunity or passive buffs.

| Accessory | Price | Effect |
|---|---|---|
| Defense Ring | 5000 | Cancel Sleep, Death Sentence |
| Magic Ring | 10000 | Cancel Silence, Berserk |
| Reflect Ring | 10000 | Always Reflect |
| Angel Ring | 20000 | Cancel Death, Blind; Auto-Reraise |
| Cursed Ring | Rare | PA/MA/Speed up; Undead status |

### Armlets

Provide status immunity.

| Accessory | Price | Effect |
|---|---|---|
| Diamond Armlet | 5000 | PA +1, MA +1; Cancel Slow |
| Defense Armlet | 7000 | Cancel Don't Move, Don't Act |
| N-Kai Armlet | 10000 | Cancel Petrify, Stop |
| 108 Gems | 15000 | Cancel many statuses; strengthen all elements |

### Perfumes (Female Only)

| Accessory | Price | Effect |
|---|---|---|
| Cherche | 30000 | Float + Reflect |
| Salty Rage | 30000 | Protect + Shell |
| Chantage | 30000 | Reraise + Regen |
| Setiemson | 30000 | Haste + Invisible; MA +1 |
