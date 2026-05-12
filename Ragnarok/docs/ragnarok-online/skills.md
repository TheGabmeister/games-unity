# Ragnarok Online v1.0 — Class Skill Reference

Companion to [SPEC.md](../../SPEC.md) §4. Lists every skill for each class with max level, type, SP cost, prerequisites, and key mechanical details. Pre-renewal (classic) values only.

---

## Novice

Job Level Cap: 10 | Skill Points: 9

| Skill | Max Lv | Type | SP | Prerequisites | Details |
|-------|--------|------|----|---------------|---------|
| Basic Skill | 9 | Passive | — | — | Unlocks per level: Lv1 Trade, Lv2 Emoticons, Lv3 Sit, Lv4 Chat rooms, Lv5 Join party, Lv6 Kafra storage, Lv7 Create party, Lv9 Job change eligible |
| First Aid | 1 | Supportive | 3 | Quest | Restores 5 HP |
| Play Dead (Trick Dead) | 1 | Active | 0 | Quest | Feigns death; aggressive monsters ignore user. Lost after class change |

All 9 points must go into Basic Skill to qualify for 1st class change.

---

## Swordman

Job Level Cap: 50 | Skill Points: 49

| Skill | Max Lv | Type | SP | Prerequisites | Details |
|-------|--------|------|----|---------------|---------|
| Sword Mastery | 10 | Passive | — | — | +ATK with 1H Swords and Daggers (+4 per level) |
| Two-Handed Sword Mastery | 10 | Passive | — | Sword Mastery 1 | +ATK with 2H Swords (+4 per level) |
| Increase HP Recovery | 10 | Passive | — | — | +natural HP regen (+5 per level flat, +2% item heal per level) |
| Bash | 10 | Offensive | 8–15 | — | Single-target melee. 130–400% ATK. Stun at Lv6+ with Fatal Blow quest skill |
| Provoke | 10 | Supportive | 4–13 | — | +ATK, −DEF on target for 30s. Range 9. 1s cooldown. Fails on Undead/Boss |
| Magnum Break | 10 | Offensive | 30 | Bash 5 | AoE fire splash (radius 2), knockback 2 cells. +20% Fire ATK bonus 10s. HP cost 20→16. 2s aftercast |
| Endure | 10 | Supportive | 10 | Provoke 5 | Flinch immunity 10–37s. +MDEF equal to skill level. 10s cooldown |
| Moving HP Recovery | 1 | Passive | — | Quest | HP regen while walking |
| Fatal Blow | 1 | Passive | — | Quest | Adds stun chance to Bash Lv6+ |
| Auto Berserk | 1 | Active | — | Quest | Auto-casts Provoke on self at ≤25% HP |

---

## Mage

Job Level Cap: 50 | Skill Points: 49

| Skill | Max Lv | Type | SP | Cast Time | Prerequisites | Details |
|-------|--------|------|----|-----------|---------------|---------|
| Increase SP Recovery | 10 | Passive | — | — | — | +natural SP regen |
| Sight | 1 | Active | 10 | — | — | Reveals hidden enemies in 3-cell radius for 10s. Fire element |
| Napalm Beat | 10 | Offensive | 9–18 | 1s | — | Ghost AoE (splash 1). Damage split among targets |
| Soul Strike | 10 | Offensive | 18–42 | 0.5s | Napalm Beat 4 | Ghost element. 1/1/2/2/3/3/4/4/5/5 hits per level |
| Safety Wall | 10 | Defensive | 30–40 | 4→1s | Napalm 7, Soul Strike 5 | Ground cell barrier blocking melee attacks. Blocks 2–11 hits. Duration 5–50s. Costs 1 Blue Gemstone |
| Cold Bolt | 10 | Offensive | 12–30 | 0.7–7s | — | Water element. 1–10 hits |
| Frost Diver | 10 | Offensive | 25→16 | 0.8s | Cold Bolt 5 | Water element. Freeze chance 38–83%. Duration 3–30s |
| Stone Curse | 10 | Active | 25→16 | 1s | — | Earth. Petrifies target. Costs 1 Red Gemstone |
| Fire Bolt | 10 | Offensive | 12–30 | 0.7–7s | — | Fire element. 1–10 hits |
| Fire Ball | 10 | Offensive | 25 | 1.5→1s | Fire Bolt 4 | Fire AoE (splash 2) |
| Fire Wall | 10 | Defensive | 40 | 2→0.65s | Fire Ball 5, Sight 1 | Ground fire barrier. Knockback 2. Duration 5–14s. Max 3 walls |
| Lightning Bolt | 10 | Offensive | 12–30 | 0.7–7s | — | Wind element. 1–10 hits |
| Thunderstorm | 10 | Offensive | 29–74 | 1–10s | Lightning Bolt 4 | Wind AoE ground-target. 1–10 hits |
| Energy Coat | 1 | Supportive | — | — | Quest | Reduces physical damage taken at cost of SP per hit |

---

## Archer

Job Level Cap: 50 | Skill Points: 49

| Skill | Max Lv | Type | SP | Prerequisites | Details |
|-------|--------|------|----|---------------|---------|
| Owl's Eye | 10 | Passive | — | — | +DEX (+1 per level) |
| Vulture's Eye | 10 | Passive | — | Owl's Eye 3 | +Range with Bows (+1 cell per level) and +HIT |
| Improve Concentration | 10 | Supportive | 25–70 | Vulture's Eye 1 | +AGI and +DEX (3–12%). Detects hidden in 3×3. Duration 60–240s |
| Double Strafe | 10 | Offensive | 12 | — | 2 hits on single target. 190–280% total ATK. Requires Bow + Arrow |
| Arrow Shower | 10 | Offensive | 15 | Double Strafe 5 | AoE (splash 2), knockback 2 cells. 80–170% ATK. Requires Bow + Arrow |
| Arrow Crafting | 1 | Active | — | Quest | Creates arrows from various items |
| Charge Arrow | 1 | Offensive | 15 | Quest | Long-range knockback arrow. 150% ATK |

---

## Thief

Job Level Cap: 50 | Skill Points: 49

| Skill | Max Lv | Type | SP | Prerequisites | Details |
|-------|--------|------|----|---------------|---------|
| Double Attack | 10 | Passive | — | — | 5% chance per level for 2 hits with Daggers. Also works with Katars |
| Improve Dodge | 10 | Passive | — | — | +FLEE (+1 per level, +3 at Lv10) |
| Steal | 10 | Active | 10 | — | Attempts to steal an item from a monster. Higher level = better chance |
| Hiding | 10 | Active | 10 | Steal 5 | Invisibility. Duration 30–300s. Broken by Sight/Ruwach, AoE, Detecting |
| Envenom | 10 | Offensive | 12 | — | Poison melee attack. +15–150 ATK bonus. Poison chance for 60s |
| Detoxify | 1 | Supportive | 10 | Envenom 3 | Cures Poison status. Range 9 |
| Sand Attack | 1 | Offensive | 9 | Quest | Blinds target for 12–17s |
| Back Sliding | 1 | Active | 7 | Quest | Instantly move 5 cells backward |
| Pick Stone | 1 | Active | 2 | Quest | Pick up a throwable stone |
| Throw Stone | 1 | Offensive | 2 | Quest | Throw stone at target. Range 7. Can stun |

---

## Merchant

Job Level Cap: 50 | Skill Points: 49

| Skill | Max Lv | Type | SP | Prerequisites | Details |
|-------|--------|------|----|---------------|---------|
| Enlarge Weight Limit | 10 | Passive | — | — | +200 max weight per level |
| Discount | 10 | Passive | — | Enlarge Weight 3 | Buy from NPC at reduced price (7–24%) |
| Overcharge | 10 | Passive | — | Discount 3 | Sell to NPC at higher price (7–24%) |
| Pushcart | 10 | Passive | — | Enlarge Weight 5 | Equip a cart. 8000 weight capacity. Each level reduces speed penalty |
| Item Appraisal | 1 | Active | 10 | — | Identifies unidentified items |
| Vending | 10 | Active | 30 | Pushcart 3 | Opens player shop. 3–12 item slots. Items sold from cart |
| Mammonite | 10 | Offensive | 5 | — | Powerful melee. 150–600% ATK. Consumes 100–1000z per use |
| Cart Revolution | 1 | Offensive | 12 | Quest | AoE knockback using cart. Damage scales with cart weight |
| Change Cart | 1 | Active | — | Quest | Changes cart appearance |
| Crazy Uproar (Loud Exclamation) | 1 | Active | 8 | Quest | +4 STR for 5 minutes |

---

## Acolyte

Job Level Cap: 50 | Skill Points: 49

| Skill | Max Lv | Type | SP | Cast Time | Prerequisites | Details |
|-------|--------|------|----|-----------|---------------|---------|
| Divine Protection | 10 | Passive | — | — | — | Reduces damage from Undead/Demon monsters (+3 per level) |
| Demon Bane | 10 | Passive | — | Divine Protection 3 | +ATK vs Undead/Demon (+3 per level) |
| Ruwach | 1 | Active | 10 | — | — | Reveals hidden enemies in 5×5 area for 10s. Holy element damage to revealed targets |
| Teleport | 2 | Active | 10/9 | — | Ruwach 1 | Lv1: random spot on map. Lv2: adds save point option |
| Warp Portal | 4 | Active | 35–26 | 1s | Teleport 2 | Creates warp gate to memorized location. Duration 5–20s. Costs Blue Gemstone. Max 3 memo slots |
| Pneuma | 1 | Defensive | 10 | — | Warp Portal 4 | Ground cell: blocks ALL ranged physical attacks for 10s. Cannot overlap Safety Wall |
| Heal | 10 | Supportive | 13–40 | — | — | Heals HP. Formula: [BaseLv + INT] / 8 × (4 + 8 × SkillLv). Holy element: damages Undead. 1s aftercast |
| Increase AGI | 10 | Supportive | 18–45 | 1s | Heal 3 | +AGI to target (3–12). Duration 60–240s. Costs 15 HP |
| Decrease AGI | 10 | Offensive | 15–33 | 1s | Increase AGI 1 | −AGI (3–12) + movement speed reduction. Duration 40–130s |
| Aqua Benedicta | 1 | Active | 10 | — | — | Creates Holy Water. Must stand on water cell |
| Signum Crucis | 10 | Active | 35 | 0.5s | Demon Bane 3 | AoE DEF reduction on Undead/Demon. Radius 15. Success chance 27–72% |
| Angelus | 10 | Supportive | 23–50 | — | Divine Protection 3 | +VIT DEF for party (5–50%). Duration 30–300s |
| Blessing | 10 | Supportive | 28–64 | — | Divine Protection 5 | +STR/INT/DEX (1–10) to target. Duration 60–240s. Cures Curse and Stone on Undead |
| Cure | 1 | Supportive | 15 | — | Heal 2 | Cures Silence, Confusion, Blind. 1s aftercast |
| Holy Light | 1 | Offensive | 15 | — | Quest | Holy element single-target. 125% MATK |

---

## Knight (from Swordman) — 2-1

Job Level Cap: 50 | Skill Points: 49

| Skill | Max Lv | Type | SP | Cast Time | Prerequisites | Details |
|-------|--------|------|----|-----------|---------------|---------|
| Spear Mastery | 10 | Passive | — | — | — | +ATK with Spears (+4 per level, +5 when mounted) |
| Pierce | 10 | Offensive | 7 | — | Spear Mastery 1 | Hits equal to target size (S=1, M=2, L=3). 110–200% ATK per hit |
| Spear Stab | 10 | Offensive | 9 | — | Pierce 5 | Single hit with knockback 6 cells. 120–300% ATK |
| Spear Boomerang | 5 | Offensive | 10 | — | Pierce 3 | Ranged spear throw. Range 3–11. 150–250% ATK. 1s aftercast |
| Brandish Spear | 10 | Offensive | 12 | 0.7s | Riding 1, Spear Stab 3 | Frontal AoE (expanding cone). Requires Peco mount. 100–400% ATK |
| Two-Hand Quicken | 10 | Supportive | 14–50 | — | 2H Sword Mastery 1 | +30% ASPD. Duration 60–300s. 2H Sword only |
| Auto Counter | 5 | Offensive | 3 | — | 2H Sword Mastery 1 | Auto-counterattack when hit. 0.4–2s aftercast |
| Bowling Bash | 10 | Offensive | 13–22 | 0.7s | Bash 10, Magnum Break 3, 2H Sword Mastery 5, 2H Quicken 10, Auto Counter 5 | Powerful AoE knockback. 250–700% ATK. Gutter line mechanic: targets hit into each other |
| Riding | 1 | Passive | — | — | Endure 1 | Enables Peco Peco mount. Movement speed +25% |
| Cavalry Mastery | 5 | Passive | — | Riding 1 | Reduces ASPD penalty while mounted (−20% base → −0% at Lv5) |
| One-Hand Quicken | 1 | Supportive | — | — | Quest (2H Quicken 10) | +ASPD with 1H Swords |
| Charge Attack | 1 | Offensive | 40 | — | Quest | Ranged charge attack. Range 14 |

---

## Wizard (from Mage) — 2-1

Job Level Cap: 50 | Skill Points: 49

| Skill | Max Lv | Type | SP | Cast Time | Prerequisites | Details |
|-------|--------|------|----|-----------|---------------|---------|
| Fire Pillar | 10 | Offensive | 75 | 0.3–3s | Fire Wall 1 | Ground trap. Fire element. 3–12 hits. Duration until triggered |
| Sightrasher | 10 | Offensive | 35–53 | 0.5s | Lightning Bolt 1, Sight 1 | Fire AoE around caster. Knockback 5 cells. 2s aftercast |
| Meteor Storm | 10 | Offensive | 20–64 | 15s | Sightrasher 2, Thunderstorm 1 | Massive Fire AoE. 1–5 meteors. Ground target. Stun chance |
| Jupitel Thunder | 10 | Offensive | 20–47 | 2.5–7s | Napalm Beat 1, Lightning Bolt 1 | Wind element. 3–12 hits. Knockback 2–7 cells |
| Lord of Vermilion | 10 | Offensive | 60–96 | 10.5–15s | Thunderstorm 1, Jupitel Thunder 5 | Massive Wind AoE. 4–40% chance to Blind. 5s aftercast |
| Water Ball | 5 | Offensive | 15–25 | 1–5s | Cold Bolt 1, Lightning Bolt 1 | Water element. 1–25 hits (scales with surrounding water cells) |
| Ice Wall | 10 | Defensive | 20 | — | Stone Curse 1, Frost Diver 1 | Creates 5-cell ice barrier. Blocks movement. Duration 5–50s |
| Frost Nova | 10 | Offensive | 27–45 | 4–6s | Ice Wall 1 | Water AoE around caster. Freeze chance. 1s aftercast |
| Storm Gust | 10 | Offensive | 78 | 6–15s | Frost Diver 1, Jupitel Thunder 3 | Massive Water AoE. 10 hits. Freeze after 3 hits. 5s aftercast |
| Earth Spike | 5 | Offensive | 12–20 | 0.7–3.5s | Stone Curse 1 | Earth element. 1–5 hits |
| Heaven's Drive | 5 | Offensive | 28–44 | 1–5s | Earth Spike 3 | Earth AoE ground target. 1–5 hits. Can hit Hidden targets |
| Quagmire | 5 | Supportive | 5–25 | — | Heaven's Drive 1 | Ground debuff: −AGI, −DEX, reduces movement speed. 1s aftercast |
| Estimation (Sense) | 1 | Active | 10 | — | — | Displays target monster's stats, element, race, size |
| Sight Blaster | 1 | Offensive | 40 | — | Quest | Orbiting fire around caster. Auto-attacks approaching enemies. Knockback |

---

## Hunter (from Archer) — 2-1

Job Level Cap: 50 | Skill Points: 49

| Skill | Max Lv | Type | SP | Prerequisites | Details |
|-------|--------|------|----|---------------|---------|
| Skid Trap | 5 | Trap | 10 | — | Knockback trap. Pushes target 3–7 cells |
| Land Mine | 5 | Trap | 10 | — | Earth damage trap. Stun chance |
| Ankle Snare | 5 | Trap | 12 | Skid Trap 1 | Immobilizes target for 4–12s (PvM) / 3–7s (PvP) |
| Shockwave Trap | 5 | Trap | 45 | Ankle Snare 1 | Drains target SP on trigger |
| Flasher | 5 | Trap | 12 | Skid Trap 1 | Blinds target for 10–30s |
| Sandman | 5 | Trap | 12 | Flasher 1 | Puts target to Sleep for 12–30s |
| Freezing Trap | 5 | Trap | 10 | Flasher 1 | Freezes target for 10–30s |
| Blast Mine | 5 | Trap | 10 | Land Mine 1, Sandman 1, Freezing Trap 1 | Wind AoE trap. 2-cell splash |
| Claymore Trap | 5 | Trap | 15 | Shockwave 1, Blast Mine 1 | Fire AoE trap. 3-cell splash. Highest trap damage |
| Remove Trap | 1 | Active | 5 | Land Mine 1 | Recovers a placed trap. Returns 1 Trap item |
| Talkie Box | 1 | Trap | 1 | Shockwave 1, Remove Trap 1 | Displays custom text message when triggered |
| Beast Bane | 10 | Passive | — | — | +ATK vs Brute/Insect (+4 per level) |
| Falcon Mastery | 1 | Passive | — | Beast Bane 1 | Enables falcon rental from NPC |
| Blitz Beat | 5 | Offensive | 10–22 | Falcon Mastery 1 | Falcon attack. 1–5 hits. Pierces DEF. AoE 3×3. Can auto-cast on normal attacks (chance = 0.5% × LUK) |
| Steel Crow | 10 | Passive | — | Blitz Beat 5 | +Falcon damage (+6 per level) |
| Detecting | 4 | Active | 8 | Improve Concentration 1, Falcon Mastery 1 | Reveals hidden targets in 3–9 cell range |
| Spring Trap | 5 | Active | 10 | Remove Trap 1, Falcon Mastery 1 | Falcon removes traps at range 4–8 |
| Phantasmic Arrow | 1 | Offensive | 10 | Quest | Ghost element arrow. No arrow consumption |

---

## Assassin (from Thief) — 2-1

Job Level Cap: 50 | Skill Points: 49

| Skill | Max Lv | Type | SP | Prerequisites | Details |
|-------|--------|------|----|---------------|---------|
| Righthand Mastery | 5 | Passive | — | — | Right-hand damage when dual-wielding: 50→100% |
| Lefthand Mastery | 5 | Passive | — | Righthand 2 | Left-hand damage when dual-wielding: 20→60% |
| Katar Mastery | 10 | Passive | — | — | +ATK with Katars (+3 per level). Katars have innate +Double Attack and 2× Critical |
| Cloaking | 10 | Active | 15 | Hiding 2 | Movement while invisible. Lv1–4 require wall adjacency; Lv5+ works anywhere. SP drain over time |
| Sonic Blow | 10 | Offensive | 16–34 | Katar Mastery 4 | 8 rapid hits on single target. Katar only. 300–900% total ATK. Stun chance. 2s aftercast |
| Grimtooth | 5 | Offensive | 3 | Cloaking 2, Sonic Blow 5 | Attack from Hiding/Cloaking without revealing. Range 3–7. Splash with Katar |
| Enchant Poison | 10 | Supportive | 20 | Envenom 1 | Gives weapon Poison property. Duration 40–220s. Poison chance on hit |
| Poison React | 10 | Defensive | 25–45 | Enchant Poison 3 | Auto-counter with Poison attack when hit by physical. Duration 20–200s |
| Venom Dust | 10 | Offensive | 20 | Enchant Poison 5 | Creates poison cloud on ground cell. 50s duration. Costs 1 Red Gemstone |
| Venom Splasher | 10 | Offensive | 12–30 | Poison React 5, Venom Dust 5 | Plants timed bomb on target (must be ≤75% HP). 1s cast. AoE explosion after delay. 500–1400% ATK |
| Sonic Acceleration | 1 | Passive | — | Quest | +50% damage on Sonic Blow |
| Venom Knife | 1 | Offensive | 15 | Quest | Ranged poison throw. Range 9 |

---

## Priest (from Acolyte) — 2-1

Job Level Cap: 50 | Skill Points: 49

| Skill | Max Lv | Type | SP | Cast Time | Prerequisites | Details |
|-------|--------|------|----|-----------|---------------|---------|
| Mace Mastery | 10 | Passive | — | — | — | +ATK with Maces (+3 per level) |
| Impositio Manus | 5 | Supportive | 13–25 | — | — | +ATK to target (+5 per level). Range 9. Duration 60s. 3s aftercast |
| Suffragium | 3 | Supportive | 8 | — | Impositio 2 | Reduces cast time of next spell on target (15/30/45%). 2s aftercast |
| Aspersio | 5 | Supportive | 14–30 | — | Aqua Benedicta 1, Impositio 3 | Enchants weapon with Holy property. Duration 60–180s. Costs 1 Holy Water |
| B.S. Sacramenti | 5 | Supportive | 20 | — | Gloria 3, Aspersio 5 | Enchants armor with Holy property. Requires 2 Acolyte-class in party |
| Sanctuary | 10 | Supportive | 15–42 | 5s | Heal 1 | Ground AoE heal zone (7×7). Duration 4–31s. Holy element: damages Undead. Heals per tick |
| Slow Poison | 4 | Supportive | 6–12 | — | — | Temporarily halts Poison damage. Duration 10–40s |
| Status Recovery | 1 | Supportive | 5 | — | — | Cures Frozen, Stun, Sleep, Stone Curse, Darkness, Confusion, Hallucination. 2s aftercast |
| Kyrie Eleison | 10 | Supportive | 20–35 | 2s | Angelus 2 | Barrier absorbing hits. Blocks 5–14 hits or 12–30% MaxHP total damage. Duration 120s. 2s aftercast |
| Magnificat | 5 | Supportive | 40 | 4s | — | 2× SP regen rate for party. Duration 30–90s |
| Gloria | 5 | Supportive | 20 | — | Kyrie 4, Magnificat 3 | +30 LUK to party. Duration 10–30s. 2s aftercast |
| Lex Divina | 10 | Offensive | 10–20 | — | Ruwach 1 | Silences target for 30–60s. Range 5. 3s aftercast. If target already Silenced, removes it |
| Turn Undead | 10 | Offensive | 20 | 1s | Resurrection 1, Lex Divina 3 | Chance to instantly kill Undead (formula: 20% × [Lv + INT + LUK] / 1000). Holy damage on fail |
| Lex Aeterna | 1 | Supportive | 10 | — | Lex Divina 5 | Next damage source to target deals 2× damage. Range 9. 3s aftercast |
| Magnus Exorcismus | 10 | Offensive | 40–58 | 15s | Safety Wall 1, Lex Aeterna 1, Turn Undead 3 | Massive Holy AoE cross pattern (7×7). 1–10 hits. Only damages Demon/Undead race. 4s aftercast |
| Resurrection | 4 | Supportive | 60 | 2–6s | Status Recovery 1, Increase SP Recovery 4 | Revives dead player. Lv1 = 10% HP. Lv4 = 80% HP |
| Redemptio | 1 | Supportive | — | — | Quest | Resurrects all dead party members. Caster loses EXP |

---

## Blacksmith (from Merchant) — 2-1

Job Level Cap: 50 | Skill Points: 49

| Skill | Max Lv | Type | SP | Prerequisites | Details |
|-------|--------|------|----|---------------|---------|
| Iron Tempering | 5 | Passive | — | — | Success rate for crafting Steel |
| Steel Tempering | 5 | Passive | — | Iron 1 | Enables Steel crafting from Iron Ore + Coal |
| Enchanted Stone Craft | 5 | Passive | — | Iron 1 | Crafts elemental stones (Flame Heart, Mystic Frozen, Rough Wind, Great Nature) |
| Oridecon Research | 5 | Passive | — | Enchanted Stone 1 | Higher tier weapon forging success |
| Weapon Forging (per type) | 3 each | Active | — | Chains from Dagger Forging | Forge weapons from ores. 7 weapon types: Dagger, Sword, 2H Sword, Axe, Mace, Knuckle, Spear |
| Hilt Binding | 1 | Passive | — | — | +ATK bonus to forged weapons |
| Finding Ore | 1 | Passive | — | Steel 1, Hilt 1 | Chance to find ores when killing monsters |
| Weapon Research | 10 | Passive | — | Hilt 1 | +HIT (+2 per level). Enables Repair |
| Repair Weapon | 1 | Active | 30 | Weapon Research 1 | Repairs broken equipment |
| Skin Tempering | 5 | Passive | — | — | +Fire and Neutral element resistance |
| Hammer Fall | 5 | Offensive | 10 | — | AoE stun chance (30–70% at Lv1–5). Radius 2 |
| Adrenaline Rush | 5 | Supportive | 20–32 | Hammer Fall 2 | +ASPD with Axes/Maces. Duration 30–150s. Party-wide |
| Weapon Perfection | 5 | Supportive | 10–18 | Weapon Research 2, Adrenaline 2 | 100% damage regardless of monster size. Duration 10–50s. Party-wide |
| Power Thrust (Overthrust) | 5 | Supportive | 10–18 | Adrenaline 3 | +ATK 5–25%. 0.1% weapon break chance per hit. Duration 20–180s. Party-wide |
| Maximize Power | 5 | Supportive | 10 | Weapon Perfection 3, Overthrust 2 | Always max weapon ATK variance (no random). Drains SP per second |
| Unfair Trick | 1 | Passive | — | Quest | Reduces Mammonite zeny cost by 10% |
| Greed | 1 | Active | 10 | Quest | Picks up all items in 5×5 area around caster |
| Full Adrenaline Rush | 1 | Supportive | 64 | Adrenaline 5 | Quest skill. Adrenaline Rush for ALL weapon types. Party-wide |

---

## Crusader (from Swordman) — 2-2

Job Level Cap: 50 | Skill Points: 49

| Skill | Max Lv | Type | SP | Cast Time | Prerequisites | Details |
|-------|--------|------|----|-----------|---------------|---------|
| Faith | 10 | Passive | — | — | — | +Max HP (+200 per level) and +Holy resistance (5% per level) |
| Auto Guard | 10 | Defensive | — | — | — | Chance to block melee attacks with shield (5–30%). Requires Shield |
| Shield Charge | 5 | Offensive | 10 | — | Auto Guard 5 | Shield bash with knockback 5 cells. 130–210% ATK |
| Shield Boomerang | 5 | Offensive | 12 | — | Shield Charge 3 | Ranged shield throw. Range 3–7. Damage scales with shield weight and refine |
| Reflect Shield | 10 | Defensive | 35–62 | — | Shield Boomerang 3 | Reflects 13–40% of melee damage back to attacker. Duration 300s. Requires Shield |
| Holy Cross | 10 | Offensive | 11–20 | — | Faith 7 | Holy element. Double hit. 135–400% ATK. Chance to Blind target |
| Grand Cross | 10 | Offensive | 37–64 | 3s | Holy Cross 6, Faith 10 | Massive Holy + Dark AoE cross pattern. Drains 20% current HP per cast. Cannot be interrupted. High damage to Undead/Demon |
| Devotion | 5 | Supportive | 25–45 | — | Reflect Shield 5, Grand Cross 4 | Takes damage for party member. Duration 30–90s. Range 7–11. Max level difference = 10 |
| Providence (Resistant Souls) | 5 | Supportive | 30 | 3s | Divine Protection 5, Heal 5 | +5% Holy and Demon resistance per level to target. Duration 60–180s |
| Defender | 5 | Defensive | 30 | — | Shield Boomerang 1 | Reduces ranged damage (5–25%). Heavily reduces movement speed. Toggle skill |
| Spear Quicken | 10 | Supportive | 24–60 | — | Spear Mastery 10 | +ASPD with 1H Spears. Duration 30–300s |
| Shrink | 1 | Active | 15 | — | Quest | Adds knockback to Auto Guard block trigger |

---

## Sage (from Mage) — 2-2

Job Level Cap: 50 | Skill Points: 49

| Skill | Max Lv | Type | SP | Cast Time | Prerequisites | Details |
|-------|--------|------|----|-----------|---------------|---------|
| Advanced Book | 10 | Passive | — | — | — | +ATK with Books (+3 per level) |
| Cast Cancel | 5 | Active | 2 | — | Advanced Book 2 | Cancel own cast mid-channel. Recovers 50–90% of SP |
| Magic Rod | 5 | Active | 2 | — | Advanced Book 4 | Absorbs incoming single-target spell. Recovers 3–15% of max SP. Blocks the spell |
| Spell Breaker | 5 | Active | 10 | — | Magic Rod 1 | Interrupts enemy cast. Odd levels deal MATK damage; even levels absorb SP. Range 9 |
| Free Cast | 10 | Passive | — | — | Cast Cancel 1 | Move while casting. Movement speed during cast: 30–100% of normal |
| Auto Spell (Hindsight) | 10 | Active | 35 | — | Free Cast 4 | Auto-casts Bolt spells on melee attacks. Higher levels unlock stronger spells. Duration 60–300s |
| Flame Launcher | 5 | Supportive | 28–44 | 3s | Fire Bolt 1, Advanced Book 5 | Enchants party member's weapon with Fire. Duration 20–60 min. Costs 1 Red Gemstone |
| Frost Weapon | 5 | Supportive | 28–44 | 3s | Cold Bolt 1, Advanced Book 5 | Enchants weapon with Water. Duration 20–60 min. Costs 1 Red Gemstone |
| Lightning Loader | 5 | Supportive | 28–44 | 3s | Lightning Bolt 1, Advanced Book 5 | Enchants weapon with Wind. Duration 20–60 min. Costs 1 Red Gemstone |
| Seismic Weapon | 5 | Supportive | 28–44 | 3s | Stone Curse 1, Advanced Book 5 | Enchants weapon with Earth. Duration 20–60 min. Costs 1 Red Gemstone |
| Dragonology | 5 | Passive | — | — | Advanced Book 9 | +INT vs Dragons (+1 per level). +Dragon race resistance |
| Volcano | 5 | Active | 50 | 3s | Flame Launcher 2 | Ground tile (7×7). +ATK bonus for Fire-weapon users in area. Fire damage to enemies. Duration 60–180s. Costs 1 Yellow Gemstone |
| Deluge | 5 | Active | 50 | 3s | Frost Weapon 2 | Ground tile (7×7). +MaxHP for Water-weapon users. Duration 60–180s. Costs 1 Yellow Gemstone |
| Violent Gale | 5 | Active | 50 | 3s | Lightning Loader 2 | Ground tile (7×7). +FLEE for Wind-weapon users. Duration 60–180s. Costs 1 Yellow Gemstone |
| Land Protector (Magnetic Earth) | 5 | Active | 66 | 3–7s | Volcano 3, Deluge 3, Violent Gale 3 | Ground tile: nullifies ALL ground-targeted magic (ally and enemy). Duration 150–450s. Costs 1 Yellow + 1 Blue Gemstone |
| Dispell | 5 | Active | 1 | 1–5s | Spell Breaker 3 | Removes all buffs from target (success 60–100%). Costs 1 Yellow Gemstone |
| Abracadabra (Hocus Pocus) | 10 | Active | 50 | — | Auto Spell 5, Dispell 1, Land Protector 1 | Casts a random skill from entire game skill pool. Costs 2 Yellow Gemstones |

---

## Bard (from Archer, Male only) — 2-2

Job Level Cap: 50 | Skill Points: 49

| Skill | Max Lv | Type | SP | Prerequisites | Details |
|-------|--------|------|----|---------------|---------|
| Musical Lesson | 10 | Passive | — | — | +ATK with Instruments (+3 per level). Increases performance effects |
| Musical Strike | 5 | Offensive | 1–5 | Musical Lesson 3 | Ranged attack with Instrument. 150–250% ATK |
| Dissonance | 5 | Offensive | — | Musical Lesson 1, Adaptation 1 | Continuous AoE damage around self during performance. 30–70 flat damage per tick |
| Frost Joker | 5 | Active | 12–20 | Encore 1 | Chance to Freeze ALL visible entities on screen (including party). 15–35% chance |
| Whistle | 10 | Performance | 24–60 | Dissonance 3 | +FLEE and +Perfect Dodge for party in range |
| Assassin Cross of Sunset | 10 | Performance | 38–74 | Dissonance 3 | +ASPD for party in range |
| Poem of Bragi | 10 | Performance | 40–76 | Dissonance 3 | Reduces cast time and after-cast delay for party in range |
| Apple of Idun | 10 | Performance | 40–76 | Dissonance 3 | +MaxHP and +HP regen for party in range |
| Adaptation to Circumstances | 1 | Active | — | — | Cancels current performance |
| Encore | 1 | Active | 1 | Adaptation 1 | Recasts last performance at half SP cost |

**Ensemble Skills** (require Bard + Dancer standing adjacent):

| Skill | Lv | Prerequisites | Details |
|-------|-----|---------------|---------|
| Lullaby | 1 | Whistle 10 | Chance to put all enemies in range to Sleep |
| Mr. Kim A Rich Man | 1 | Assassin Cross 10 | Increases EXP gain in area |
| Eternal Chaos | 1 | Dissonance 3 | Reduces DEF of all in area |
| A Drum on the Battlefield | 1 | Apple 10 | +ATK and +DEF in area |
| The Ring of Nibelungen | 1 | Bragi 10 | +ATK for Lv4 weapons in area |
| Loki's Veil | 1 | Whistle 10 | Cannot use skills in area (allies and enemies) |
| Into the Abyss | 1 | Apple 10 | No gemstone/catalyst consumption in area |
| Invulnerable Siegfried | 1 | Dissonance 3 | +Element resistance in area |

---

## Dancer (from Archer, Female only) — 2-2

Job Level Cap: 50 | Skill Points: 49

| Skill | Max Lv | Type | SP | Prerequisites | Details |
|-------|--------|------|----|---------------|---------|
| Dancing Lesson | 10 | Passive | — | — | +ATK with Whips (+3 per level). Increases performance effects |
| Throw Arrow | 5 | Offensive | 1–5 | Dancing Lesson 3 | Ranged attack with Whip. 150–250% ATK |
| Ugly Dance | 5 | Active | — | Dancing Lesson 1, Adaptation 1 | Continuous SP drain to enemies in range |
| Scream | 5 | Active | 12–20 | Encore 1 | Chance to Stun ALL visible on screen. 15–35% chance |
| Humming | 10 | Performance | 22–58 | Ugly Dance 3 | +HIT for party in range |
| Please Don't Forget Me | 10 | Performance | 22–58 | Ugly Dance 3 | Slows enemy movement speed and ASPD in range |
| Fortune's Kiss | 10 | Performance | 24–60 | Ugly Dance 3 | +Critical rate for party in range |
| Service for You | 10 | Performance | 40–76 | Ugly Dance 3 | +MaxSP and +SP regen for party in range |
| Adaptation to Circumstances | 1 | Active | — | — | Cancels current performance |
| Encore | 1 | Active | 1 | Adaptation 1 | Recasts last performance at half SP cost |
| Wink of Charm | 1 | Active | — | — | Quest skill. Chance to Confusion target |

Shares Ensemble Skills with Bard (see above).

---

## Rogue (from Thief) — 2-2

Job Level Cap: 50 | Skill Points: 49

| Skill | Max Lv | Type | SP | Prerequisites | Details |
|-------|--------|------|----|---------------|---------|
| Sword Mastery | 10 | Passive | — | — | +ATK with Swords and Daggers (inherited) |
| Vulture's Eye | 10 | Passive | — | — | +Bow range (inherited). Rogues can use bows |
| Double Strafing | 10 | Offensive | 12 | Vulture's Eye 10 | 2-hit ranged attack (inherited) |
| Remove Trap | 1 | Active | 5 | Double Strafing 5 | Removes Hunter traps (inherited) |
| Snatcher | 10 | Passive | — | Steal 1 | Auto-steal on normal attacks. 5–50% per attack to attempt Steal |
| Steal Coin | 10 | Active | 15 | Snatcher 4 | Steals Zeny from monster. Amount based on monster level and skill level |
| Back Stab | 10 | Offensive | 16 | Steal Coin 4 | Must attack from behind target. Never misses. Forced facing away. 340–700% ATK |
| Tunnel Drive | 5 | Active | — | Hiding 1 | Move while in Hiding (1–5 movement speed) |
| Raid | 5 | Offensive | 20 | Back Stab 2, Tunnel Drive 2 | AoE from Hiding state. Stun + Blind chance. Reveals self |
| Strip Helm | 5 | Active | 17 | Steal Coin 2 | Removes target headgear for 75–135s |
| Strip Shield | 5 | Active | 12 | Strip Helm 5 | Removes target shield for 75–135s |
| Strip Armor | 5 | Active | 17 | Strip Shield 5 | Removes target armor for 75–135s |
| Strip Weapon | 5 | Active | 17 | Strip Armor 5 | Removes target weapon for 75–135s |
| Intimidate (Snatch) | 5 | Offensive | 13–29 | Back Stab 4, Raid 5 | Attacks and warps both user+target to random location. 170–330% ATK |
| Plagiarism | 10 | Passive | — | Intimidate 5 | Memorizes last offensive skill used against you. Can use it as your own (Lv determines max skill level copyable) |
| Gangster's Paradise | 1 | Passive | — | Strip Shield 3 | Prevents monster aggro while sitting near another Rogue |
| Compulsion Discount | 5 | Passive | — | Gangster's Paradise 1 | NPC buy discount (5–25%) |
| Close Confine | 1 | Active | 25 | — | Quest skill. Locks both you and target in place. Duration 10s. +10 FLEE while active |

---

## Monk (from Acolyte) — 2-2

Job Level Cap: 50 | Skill Points: 49

| Skill | Max Lv | Type | SP | Cast Time | Prerequisites | Details |
|-------|--------|------|----|-----------|---------------|---------|
| Iron Fist | 10 | Passive | — | — | Demon Bane 10, Divine Protection 10 | +ATK with Knuckle weapons (+3 per level) |
| Call Spirits | 5 | Active | 8 | — | Iron Fist 2 | Summons Spirit Spheres. Max spheres = skill level. Duration 600s |
| Absorb Spirits | 1 | Active | 5 | — | Call Spirits 5 | Absorbs own or target's Spirit Spheres. Recovers 7 SP per sphere absorbed |
| Spirits Recovery | 5 | Passive | — | — | Blade Stop 2 | Enhanced HP/SP regen while sitting |
| Dodge | 10 | Passive | — | — | Iron Fist 5, Call Spirits 5 | +Perfect Dodge (+1 per 3 levels) |
| Triple Attack | 10 | Passive | — | — | Dodge 5 | 12–30% chance per attack for 3 rapid hits. Only with Knuckles or unarmed |
| Chain Combo | 5 | Offensive | 11–15 | — | Triple Attack 5 | 2-hit combo following Triple Attack. 200–400% ATK. Must chain within timing window |
| Combo Finish | 5 | Offensive | 11–15 | — | Chain Combo 3 | Strong single hit following Chain Combo. 240–440% ATK |
| Investigate (Occult Impaction) | 5 | Offensive | 10–22 | 0.5–1.5s | Call Spirits 5 | Single hit. Damage INCREASES with target DEF. Consumes 1 Spirit Sphere |
| Finger Offensive (Throw Spirit Sphere) | 5 | Offensive | 10–14 | — | Investigate 3 | Throws 1–5 Spirit Spheres as projectiles. Ranged. Each sphere deals separate damage |
| Blade Stop | 5 | Active | 10 | — | Dodge 5 | Catches enemy melee attack. Both locked in place for 20–60s. Can chain to other skills |
| Steel Body (Mental Strength) | 5 | Supportive | 200 | — | Combo Finish 3 | +90 Hard DEF and +90 Hard MDEF. Cannot move, attack, or use skills/items. Duration 30–150s |
| Explosion Spirits (Fury) | 5 | Supportive | 15 | — | Absorb Spirits 1 | Fury state. +250 Critical rate. Consumes all spheres. Duration 180s. Required for Asura Strike |
| Asura Strike (Extremity Fist) | 5 | Offensive | ALL | 4s→2s | Explosion Spirits 3, Finger Offensive 3 | **Consumes ALL remaining SP.** Damage = ATK × (8 + SP/10). Bypasses DEF. 5 minute SP regen lockout after use. Can chain from Combo Finish or Blade Stop. Single most powerful skill in classic RO |
| Body Relocation (Snap) | 1 | Active | 14 | — | Asura Strike 3, Spirits Recovery 2, Steel Body 3 | Instant teleport to target cell. Consumes 1 Spirit Sphere |
| Ki Translation | 1 | Supportive | — | — | Quest | Transfers 1 Spirit Sphere to ally |
| Ki Explosion | 1 | Offensive | — | — | Quest | AoE knockback consuming Spirit Spheres |

**Combo chain**: Triple Attack → Chain Combo → Combo Finish → Asura Strike (each must be used within the combo window of the previous skill)

---

## Alchemist (from Merchant) — 2-2

Job Level Cap: 50 | Skill Points: 49

| Skill | Max Lv | Type | SP | Cast Time | Prerequisites | Details |
|-------|--------|------|----|-----------|---------------|---------|
| Axe Mastery | 10 | Passive | — | — | — | +ATK with Axes (+3 per level) |
| Learning Potion | 10 | Passive | — | — | — | Increases effectiveness of potions used (+1–20% heal bonus) |
| Pharmacy (Prepare Potion) | 10 | Active | 5 | — | Learning Potion 5 | Brew potions, condensed potions, elemental converters, coat items. Higher level = higher success rate. Requires Medicine Bowl |
| Demonstration (Bomb) | 5 | Offensive | 10 | 1s | Pharmacy 4 | Fire ground AoE (3×3). Duration 40s. Deals Fire damage over time. Can break target weapon. Consumes 1 Bottle Grenade |
| Acid Terror | 5 | Offensive | 15 | 1s | Pharmacy 5 | Ranged attack (range 9). Chance to cause Bleeding. Chance to break target armor. Consumes 1 Acid Bottle. 70–310% ATK |
| Potion Pitcher | 5 | Supportive | 1 | 1s | Pharmacy 3 | Throws potion to heal ally at range. Effectiveness scales with INT and Learning Potion. Range 9 |
| Bio Cannibalize (Summon Flora) | 5 | Active | 20 | — | Pharmacy 6 | Summons plant monster to fight. Lv1 Mandragora, Lv2 Hydra, Lv3 Flora, Lv4 Parasite, Lv5 Geographer |
| Sphere Mine (Marine Sphere) | 5 | Active | 10 | — | Pharmacy 2 | Places Marine Sphere that explodes after delay or when attacked |
| Chemical Protection Helm | 5 | Supportive | 3 | 2s | Pharmacy 2 | Protects target headgear from being stripped or broken. Duration 120–600s |
| Chemical Protection Shield | 5 | Supportive | 3 | 2s | CP Helm 3 | Protects shield |
| Chemical Protection Armor | 5 | Supportive | 3 | 2s | CP Shield 3 | Protects armor |
| Chemical Protection Weapon | 5 | Supportive | 3 | 2s | CP Armor 3 | Protects weapon |
| Bioethics | 1 | Passive | — | — | Quest | Unlocks Homunculus system |
| Call Homunculus | 1 | Active | 10 | — | Rest 1 | Summons Homunculus companion. 4 types: Lif, Amistr, Filir, Vanilmirth |
| Rest | 1 | Active | — | — | Bioethics 1 | Recalls Homunculus to rest |
| Resurrect Homunculus | 5 | Active | — | — | Call Homunculus 1 | Revives dead Homunculus with 10–50% HP |
| Berserk Pitcher | 1 | Active | 10 | — | Quest | Throws stat-boost potion at ally |

**Homunculus Types:**
- **Lif**: Healer type. Has Emergency Avoid (FLEE buff) and Healing Hands
- **Amistr**: Tank type. Has Adamantium Skin (DEF buff) and Castling (swap position)
- **Filir**: Attack type. Has Moonlight (ranged attack) and Flitting (ASPD buff)
- **Vanilmirth**: Magic type. Has Caprice (random element bolt) and Chaotic Blessings (random heal/damage)

Each Homunculus has its own level, stats, and Intimacy meter. Intimacy increases by feeding and combat; at max Intimacy + specific conditions, Homunculus can evolve.

---

## Notes

- Quest skills are obtained through special NPCs (Platinum Skill NPCs) or specific quests, costing no skill points
- 1st class skill points and 2nd class skill points are separate pools
- Players keep all 1st class skills upon advancing to 2nd class
- Most classes cannot max all skills with 49 points, requiring build specialization
- Pre-renewal cast times use the formula: Actual Cast = Base × (1 − DEX/150)
