# Genshin Impact v1.0 — Cooking & Food Data Tables

Companion to [SPEC.md](../SPEC.md) §22 (Cooking System).

---

## Food Categories & Buff Stacking

Food buffs are grouped into four categories. Only **one buff per category** can be active at a time. Using a new food from the same category replaces the previous buff, even if the new buff is weaker.

| Category | Buff Types Included | Typical Duration |
|----------|-------------------|-----------------|
| **ATK-Boosting (Offensive)** | ATK flat, ATK%, CRIT Rate%, CRIT DMG%, Physical DMG Bonus% | 300s (5 min) |
| **DEF-Boosting (Defensive)** | DEF flat, DEF%, Shield Strength%, Healing Bonus% | 300s (5 min) |
| **Stamina (Adventurer's)** | Sprint stamina reduction, Climb stamina reduction, Glide stamina reduction, Stamina restoration | 900s (15 min) |
| **Elemental** | Elemental DMG Bonus%, Elemental RES% | 300s (5 min) |

**Revival foods** and **healing foods** are instant-use and do not provide ongoing buffs, so they do not conflict with any category. Revival foods have a **120-second cooldown per character** before the same character can be revived again.

Up to **4 simultaneous food buffs** are possible (one from each category).

---

## Food Quality Tiers

Every cooked dish has three quality variants based on cooking performance:

| Quality | How to Get | Effect Level |
|---------|-----------|--------------|
| **Suspicious** | Failed timing (outside the target zone) | Weakest effect |
| **Normal** | Hit the yellow zone | Standard effect |
| **Delicious** | Hit the orange/perfect zone | Strongest effect |

**Auto-Cook:** After cooking a recipe enough times to reach maximum proficiency, the player can auto-cook that recipe. Auto-cook always produces the **Normal** quality (never Suspicious or Delicious).

**Special Dishes:** When a character with a matching specialty cooks the corresponding base dish and achieves **Delicious** quality, there is a chance to produce a **Special Dish** instead. Special Dishes have effects equal to or better than the Delicious version. The Traveler does not have a specialty dish.

---

## v1.0 Food Recipes by Category

### ATK-Boosting Foods

| Name | Stars | Effect (Suspicious / Normal / Delicious) |
|------|-------|----------------------------------------|
| **Jade Parcels** | 4-Star | Increases all party members' ATK by 224/272/320 and CRIT Rate by 6/8/10% for 300s |
| **Adeptus' Temptation** | 5-Star | Increases all party members' ATK by 260/316/372 and CRIT Rate by 8/10/12% for 300s |
| **"Pile 'Em Up"** | 4-Star | Increases all party members' CRIT Rate by 10/15/20% for 300s |
| **"Come and Get It"** | 3-Star | Increases all party members' CRIT Rate by 10/15/20% for 300s |
| **Cold Cut Platter** | 3-Star | Increases all party members' Physical DMG by 20/30/40% for 300s |
| **Qingce Stir Fry** | 3-Star | Increases all party members' ATK by 160/194/228 for 300s |
| **Sauteed Matsutake** | 3-Star | Increases all party members' ATK by 160/194/228 for 300s |
| **Almond Tofu** | 2-Star | Increases all party members' ATK by 66/81/95 for 300s |
| **Satisfying Salad** | 2-Star | Increases all party members' CRIT Rate by 6/9/12% for 300s |
| **Flaming Red Bolognese** | 2-Star | Increases all party members' ATK by 66/81/95 for 300s |
| **Noodles with Mountain Delicacies** | 3-Star | Increases all party members' ATK by 160/194/228 for 300s |
| **Adventurer's Breakfast Sandwich** | 3-Star | Increases all party members' ATK by 160/194/228 for 300s |

### DEF-Boosting Foods

| Name | Stars | Effect (Suspicious / Normal / Delicious) |
|------|-------|----------------------------------------|
| **Golden Crab** | 4-Star | Increases all party members' DEF by 215/261/308 and Healing Effects by 6/8/10% for 300s |
| **Moon Pie** | 4-Star | Increases all party members' Shield Strength by 25/30/35% and DEF by 165/200/235 for 300s |
| **Calla Lily Seafood Soup** | 3-Star | Increases all party members' DEF by 165/200/235 for 300s |
| **Jewelry Soup** | 3-Star | Increases all party members' DEF by 165/200/235 for 300s |
| **Fisherman's Toast** | 3-Star | Increases all party members' DEF by 88/107/126 for 300s |
| **Cream Stew** (Note: Stamina) | 2-Star | See Stamina Foods below (this is actually a stamina food despite the name) |

### Healing / Recovery Foods

| Name | Stars | Effect (Suspicious / Normal / Delicious) |
|------|-------|----------------------------------------|
| **Universal Peace** | 4-Star | Restores 30/32/34% of Max HP and regenerates 500/620/740 HP every 5s for 30s to the selected character |
| **Squirrel Fish** | 4-Star | Restores 30/32/34% of Max HP and regenerates 550/680/800 HP every 5s for 30s to the selected character |
| **Black-Back Perch Stew** | 3-Star | Restores 26/28/30% of Max HP and regenerates 450/620/790 HP every 5s for 30s to the selected character |
| **Bamboo Shoot Soup** | 3-Star | Restores 26/28/30% of Max HP and regenerates 450/620/790 HP every 5s for 30s to the selected character |
| **Mushroom Pizza** | 3-Star | Restores 26/28/30% of Max HP and regenerates 450/620/790 HP every 5s for 30s to the selected character |
| **Mondstadt Hash Brown** | 3-Star | Restores 30/32/34% of Max HP and regenerates 600/1,250/1,900 HP to the selected character |
| **Crystal Shrimp** | 3-Star | Restores 20/22/24% of Max HP and regenerates 390/470/550 HP every 5s for 30s to the selected character |
| **Sweet Madame** | 2-Star | Restores 20/22/24% of Max HP and regenerates 900/1,200/1,500 HP to the selected character |
| **Grilled Tiger Fish** | 2-Star | Restores 8/9/10% of Max HP and regenerates 800/1,000/1,200 HP to the selected character |
| **Chicken-Mushroom Skewer** | 1-Star | Restores 8/9/10% of Max HP and regenerates 800/1,000/1,200 HP to the selected character |

### Revival Foods

| Name | Stars | Effect (Suspicious / Normal / Delicious) |
|------|-------|----------------------------------------|
| **Crab, Ham & Veggie Bake** | 4-Star | Revives a character and restores 10/15/20% of Max HP and an additional 150 HP. 120s cooldown per character. |
| **Tea Break Pancake** | 3-Star | Revives a character and restores 250/400/550 HP. 120s cooldown per character. |
| **Steak** | 1-Star | Revives a character and restores 50/100/150 HP. 120s cooldown per character. |
| **Teyvat Fried Egg** | 1-Star | Revives a character and restores 50/100/150 HP. 120s cooldown per character. |
| **Mora Meat** | 1-Star | Revives a character and restores 50/100/150 HP. 120s cooldown per character. |

### Stamina Foods

| Name | Stars | Effect (Suspicious / Normal / Delicious) |
|------|-------|----------------------------------------|
| **Barbatos Ratatouille** | 3-Star | Decreases Stamina depleted by gliding and sprinting for all party members by 15/20/25% for 900s |
| **Sticky Honey Roast** | 3-Star | Decreases Stamina depleted by climbing and sprinting for all party members by 15/20/25% for 900s |
| **Lotus Seed and Bird Egg Soup** | 2-Star | Decreases Stamina depleted by sprinting for all party members by 15/20/25% for 900s |
| **Cream Stew** | 2-Star | Decreases Stamina depleted by sprinting for all party members by 15/20/25% for 900s |
| **Northern Smoked Chicken** | 2-Star | Restores 40/50/60 Stamina. 300s cooldown. |
| **Radish Veggie Soup** | 1-Star | Decreases Stamina depleted by sprinting for all party members by 15/20/25% for 900s |

### Elemental Potions (Crafted via Alchemy, not Cooking)

Potions are crafted at the Crafting Bench, not cooked at a stove. They fall under the Elemental food buff category.

| Name | Stars | Effect |
|------|-------|--------|
| **Flaming Essential Oil** | 2-Star | Increases all party members' Pyro DMG by 25% for 300s |
| **Frosting Essential Oil** | 2-Star | Increases all party members' Cryo DMG by 25% for 300s |
| **Shocking Essential Oil** | 2-Star | Increases all party members' Electro DMG by 25% for 300s |
| **Gushing Essential Oil** | 2-Star | Increases all party members' Hydro DMG by 25% for 300s |
| **Windbarrier Potion** | 2-Star | Increases all party members' Anemo RES by 25% for 300s |
| **Dustproof Potion** | 2-Star | Increases all party members' Geo RES by 25% for 300s |
| **Insulation Potion** | 2-Star | Increases all party members' Electro RES by 25% for 300s |
| **Frostshield Potion** | 2-Star | Increases all party members' Cryo RES by 25% for 300s |
| **Heatshield Potion** | 2-Star | Increases all party members' Pyro RES by 25% for 300s |
| **Desiccant Potion** | 2-Star | Increases all party members' Hydro RES by 25% for 300s |

---

## v1.0 Character Special Dishes

When a character with a matching specialty cooks the base dish and achieves a Perfect Cook, there is a chance to produce their Special Dish instead of the Delicious version.

| Character | Base Dish | Special Dish | Effect |
|-----------|-----------|--------------|--------|
| **Amber** | Steak | Outrider's Champion Steak! | Revives a character and restores 15% of Max HP plus 550 HP |
| **Kaeya** | Chicken-Mushroom Skewer | Fruity Skewers | Restores 16% of Max HP and regenerates 1,350 HP every 5s for 30s |
| **Lisa** | Flaming Red Bolognese | Mysterious Bolognese | Increases all party members' ATK by 114 for 300s |
| **Barbara** | Cream Stew | Spicy Stew | Decreases Stamina depleted by sprinting for all party members by 30% for 900s |
| **Diluc** | "Pile 'Em Up" | "Once Upon a Time in Mondstadt" | Increases all party members' CRIT Rate by 20% and CRIT DMG by 20% for 300s |
| **Jean** | Mushroom Pizza | Invigorating Pizza | Restores 34% of Max HP and regenerates 980 HP every 5s for 30s |
| **Venti** | Barbatos Ratatouille | A Buoyant Breeze | Decreases Stamina depleted by gliding and sprinting for all party members by 25% for 1,500s |
| **Fischl** | Cold Cut Platter | Die Heilige Sinfonie | Increases all party members' Physical DMG by 55% for 300s |
| **Sucrose** | Crab, Ham & Veggie Bake | Nutritious Meal (V.593) | Revives a character and restores 20% of Max HP plus 1,500 HP |
| **Mona** | Satisfying Salad | Der Weisheit Letzter Schluss (Life) | Increases all party members' CRIT Rate by 16% for 300s |
| **Chongyun** | Noodles with Mountain Delicacies | Cold Noodles with Mountain Delicacies | Increases all party members' ATK by 274 for 300s |
| **Xingqiu** | Crystal Shrimp | All-Delicacy Parcels | Restores 26% of Max HP and regenerates 570 HP every 5s for 30s |
| **Xiangling** | Black-Back Perch Stew | Wanmin Restaurant's Boiled Fish | Restores 34% of Max HP and regenerates 980 HP every 5s for 30s |
| **Keqing** | Grilled Tiger Fish | Survival Grilled Fish | Restores 12% of Max HP and regenerates 1,200 HP every 5s for 30s |
| **Qiqi** | "Come and Get It" | "No Tomorrow" | Increases all party members' CRIT Rate by 20% and CRIT DMG by 20% for 300s |
| **Ningguang** | Mora Meat | Qiankun Mora Meat | Revives a character and restores 10% of Max HP plus 150 HP |
| **Beidou** | Stir-Fried Filet | Flash-Fried Filet | Increases all party members' ATK by 274 for 300s |
| **Razor** | Mondstadt Hash Brown | Puppy-Paw Hash Brown | Restores 34% of Max HP and regenerates 1,200 HP to the selected character |
| **Bennett** | Teyvat Fried Egg | Teyvat Charred Egg | Revives a character and restores 10% of Max HP plus 150 HP. (Notably one of the worst specials) |
| **Noelle** | Tea Break Pancake | Lighter-Than-Air Pancake | Revives a character and restores 20% of Max HP plus 1,500 HP |
| **Klee** | Fisherman's Toast | Fish-Flavored Toast | Restores 16% of Max HP and regenerates 1,350 HP every 5s for 30s |

**Note:** The Traveler (Lumine/Aether) does not have a Special Dish.
