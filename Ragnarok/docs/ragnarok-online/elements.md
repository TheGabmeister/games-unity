# Ragnarok Online v1.0 — Element Damage Tables

Companion to [SPEC.md](../../SPEC.md) §5. Full element damage multiplier tables for all 4 defense levels.

Values are percentages. 100 = normal damage. >100 = bonus damage. <0 = target is healed. Source: rAthena `db/pre-re/attr_fix.yml`.

---

## Defense Level 1

Player armor is always Level 1. Most early-game monsters are Level 1.

| ATK \ DEF | Neutral | Water | Earth | Fire | Wind | Poison | Holy | Shadow | Ghost | Undead |
|-----------|---------|-------|-------|------|------|--------|------|--------|-------|--------|
| Neutral   | 100 | 100 | 100 | 100 | 100 | 100 | 100 | 100 | 25  | 100 |
| Water     | 100 | 25  | 100 | 150 | 50  | 100 | 75  | 100 | 100 | 100 |
| Earth     | 100 | 100 | 100 | 50  | 150 | 100 | 75  | 100 | 100 | 100 |
| Fire      | 100 | 50  | 150 | 25  | 100 | 100 | 75  | 100 | 100 | 125 |
| Wind      | 100 | 175 | 50  | 100 | 25  | 100 | 75  | 100 | 100 | 100 |
| Poison    | 100 | 100 | 125 | 125 | 125 | 0   | 75  | 50  | 100 | -25 |
| Holy      | 100 | 100 | 100 | 100 | 100 | 100 | 0   | 125 | 100 | 150 |
| Shadow    | 100 | 100 | 100 | 100 | 100 | 50  | 125 | 0   | 100 | -25 |
| Ghost     | 25  | 100 | 100 | 100 | 100 | 100 | 75  | 75  | 125 | 100 |
| Undead    | 100 | 100 | 100 | 100 | 100 | 50  | 100 | 0   | 100 | 0   |

---

## Defense Level 2

Common among mid-tier dungeon monsters.

| ATK \ DEF | Neutral | Water | Earth | Fire | Wind | Poison | Holy | Shadow | Ghost | Undead |
|-----------|---------|-------|-------|------|------|--------|------|--------|-------|--------|
| Neutral   | 100 | 100 | 100 | 100 | 100 | 100 | 100 | 100 | 25  | 100 |
| Water     | 100 | 0   | 100 | 175 | 25  | 100 | 50  | 75  | 100 | 100 |
| Earth     | 100 | 100 | 50  | 25  | 175 | 100 | 50  | 75  | 100 | 100 |
| Fire      | 100 | 25  | 175 | 0   | 100 | 100 | 50  | 75  | 100 | 150 |
| Wind      | 100 | 175 | 25  | 100 | 0   | 100 | 50  | 75  | 100 | 100 |
| Poison    | 100 | 75  | 125 | 125 | 125 | 0   | 50  | 25  | 75  | -50 |
| Holy      | 100 | 100 | 100 | 100 | 100 | 100 | -25 | 150 | 100 | 175 |
| Shadow    | 100 | 100 | 100 | 100 | 100 | 25  | 150 | -25 | 100 | -50 |
| Ghost     | 0   | 75  | 75  | 75  | 75  | 75  | 50  | 50  | 150 | 125 |
| Undead    | 100 | 75  | 75  | 75  | 75  | 25  | 125 | 0   | 100 | 0   |

---

## Defense Level 3

Boss monsters and high-level dungeon creatures.

| ATK \ DEF | Neutral | Water | Earth | Fire | Wind | Poison | Holy | Shadow | Ghost | Undead |
|-----------|---------|-------|-------|------|------|--------|------|--------|-------|--------|
| Neutral   | 100 | 100 | 100 | 100 | 100 | 100 | 100 | 100 | 0   | 100 |
| Water     | 100 | -25 | 100 | 200 | 0   | 100 | 25  | 50  | 100 | 125 |
| Earth     | 100 | 100 | 0   | 0   | 200 | 100 | 25  | 50  | 100 | 75  |
| Fire      | 100 | 0   | 200 | -25 | 100 | 100 | 25  | 50  | 100 | 175 |
| Wind      | 100 | 200 | 0   | 100 | -25 | 100 | 25  | 50  | 100 | 100 |
| Poison    | 100 | 50  | 100 | 100 | 100 | 0   | 25  | 0   | 50  | -75 |
| Holy      | 100 | 100 | 100 | 100 | 100 | 125 | -50 | 175 | 100 | 200 |
| Shadow    | 100 | 100 | 100 | 100 | 100 | 0   | 175 | -50 | 100 | -75 |
| Ghost     | 0   | 50  | 50  | 50  | 50  | 50  | 25  | 25  | 175 | 150 |
| Undead    | 100 | 50  | 50  | 50  | 50  | 0   | 150 | 0   | 100 | 0   |

---

## Defense Level 4

Strongest boss monsters (e.g., Dark Lord Undead 4, Maya Earth 4, Mistress Wind 4).

| ATK \ DEF | Neutral | Water | Earth | Fire | Wind | Poison | Holy | Shadow | Ghost | Undead |
|-----------|---------|-------|-------|------|------|--------|------|--------|-------|--------|
| Neutral   | 100 | 100 | 100 | 100 | 100 | 100 | 100 | 100 | 0   | 100 |
| Water     | 100 | -50 | 100 | 200 | 0   | 75  | 0   | 25  | 100 | 150 |
| Earth     | 100 | 100 | -25 | 0   | 200 | 75  | 0   | 25  | 100 | 50  |
| Fire      | 100 | 0   | 200 | -50 | 100 | 75  | 0   | 25  | 100 | 200 |
| Wind      | 100 | 200 | 0   | 100 | -50 | 75  | 0   | 25  | 100 | 100 |
| Poison    | 100 | 25  | 75  | 75  | 75  | 0   | 0   | -25 | 25  | -100 |
| Holy      | 100 | 75  | 75  | 75  | 75  | 125 | -100 | 200 | 100 | 200 |
| Shadow    | 100 | 75  | 75  | 75  | 75  | -25 | 200 | -100 | 100 | -100 |
| Ghost     | 0   | 25  | 25  | 25  | 25  | 25  | 0   | 0   | 200 | 175 |
| Undead    | 100 | 25  | 25  | 25  | 25  | -25 | 175 | 0   | 100 | 0   |

---

## Key Patterns

- **Elemental cycle**: Water → Fire → Earth → Wind → Water (each deals 150% to the next at Lv1, scaling to 200% at Lv4). The reverse direction deals only 50% at Lv1, dropping to 0% at Lv3–4
- **Wind → Water bonus**: Wind deals 175% to Water at Lv1 (stronger than the standard 150% in the cycle)
- **Holy vs Shadow**: Mutually strong. At Lv4: Holy → Shadow = 200%, Shadow → Holy = 200%
- **Ghost defense**: Neutral attacks deal only 25% at Lv1, 0% at Lv3–4. Use Ghost-element attacks or any other non-Neutral element
- **Undead defense**: Weak to Holy (150–200%) and Fire (125–200%). Healed by Poison and Shadow at higher levels. Immune to Undead
- **Same element**: Same-element attacks are resisted (25% at Lv1, down to −50% / heals at Lv4). Exception: Earth self-resistance at Lv1 is 100% — no reduction
- **Negative values**: Target is healed by that percentage instead of damaged
- **Players always defend at Level 1** regardless of source (card, skill, etc.)

---

## How to Apply Elements

### Armor Element (Defense)
- Default: Neutral Lv1
- Changed by: Armor cards (Swordfish = Water, Pasana = Fire, Dokebi = Wind, Sandman = Earth, Ghostring = Ghost, Angeling = Holy, Bathory = Shadow, Evil Druid = Undead)
- Changed by skills: Aspersio / B.S. Sacramenti (Holy)
- Player armor element is always **Level 1**

### Weapon Element (Attack)
- Default: Neutral (unless weapon has innate element)
- Changed by: Sage Endow skills (Flame Launcher, Frost Weapon, Lightning Loader, Seismic Weapon)
- Changed by: Aspersio (Holy), Enchant Poison (Poison), Cursed Water (Shadow)
- Changed by: Elemental Converters (consumable)
- Changed by: Cards that give elemental property to weapon
- Blacksmith forging can create permanently elemental weapons
