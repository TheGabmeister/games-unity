# Phase 5 — Progression & Story

## Overview

This phase adds the core progression loop: evolution, quests, recruitment, city growth, and a shop. After Phase 5 the game has a playable "life cycle" — raise a Digimon, evolve it, complete quests, recruit NPCs, watch the city grow, buy/sell items, and eventually see the partner die and rebirth.

---

## 5-A. Evolution System

### What the original game does

Evolution in Digimon World 1 is stat-and-care gated, not level-based. When the partner reaches a certain age, the game checks stats, care mistakes, weight, discipline, and other hidden thresholds against an evolution table. If requirements are met, evolution fires automatically. If multiple targets qualify, array order acts as tiebreaker (designer controls priority). If nothing qualifies, the Digimon stays at its current stage until its lifespan expires. After death, the Digimon becomes a Digitama and hatches as a Fresh with inherited stat bonuses. There is no mid-life devolution — the death/reincarnation cycle is the only reset path.

### Data

**EvolutionRequirement** (serializable struct, not a SO — embedded in a table SO):

| Field | Type | Notes |
|---|---|---|
| `_targetSpecies` | `DigimonSpeciesData` | What you evolve into |
| `_minAge` | `int` | Hours alive |
| `_minHP/MP/Offense/Defense/Speed/Brains` | `int` | Stat thresholds (base + bonus). 0 = no requirement |
| `_maxCareMistakes` | `int` | Must be ≤ this. -1 = no requirement |
| `_minWeight` | `int` | 0 = no requirement |
| `_maxWeight` | `int` | 0 = no requirement |
| `_minHappiness` | `int` | 0 = no requirement |
| `_minDiscipline` | `int` | 0 = no requirement |
| `_bonusCondition` | `string` | For display/editor — human-readable note on any special flag. Runtime logic can be added per-case later |

**EvolutionTable** (`ScriptableObject`, one per species, `[CreateAssetMenu(menuName = "DigimonWorld/EvolutionTable")]`):

| Field | Type |
|---|---|
| `_fromSpecies` | `DigimonSpeciesData` |
| `_requirements` | `EvolutionRequirement[]` |

A separate SO per species keeps the inspector manageable and matches how the generator already creates per-species data.

### DigimonInstance changes

New fields:
- `_totalLives` (int) — how many life cycles completed
- `_inheritedStatBonuses` (per-stat ints) — carried from previous life

New methods:
- `CheckEvolution(EvolutionTable table) → DigimonSpeciesData?` — iterates requirements, returns first match or null. Tiebreaker: order in the array (designer controls priority).
- `Evolve(DigimonSpeciesData newSpecies)` — swaps `_species` reference, resets HP/MP to new max, keeps training bonuses and care stats, wipes techniques and re-seeds from new species' learnable list (matches original game).
- `Die() → DigimonInheritance` — snapshots stat bonuses into an inheritance data struct.
- `Reincarnate(DigimonSpeciesData freshSpecies, DigimonInheritance inheritance)` — resets to Fresh stage with inherited bonuses applied.

### DigimonSpeciesData changes

Add a field:
- `_evolutionTable` (reference to `EvolutionTable` SO) — nullable. Fresh/Ultimate may not have one.

### Evolution trigger

`CareSystem` already ticks every hour. Add an evolution check:

1. Every hour tick, if `_partner.Species.EvolutionTable != null`, call `CheckEvolution`.
2. If a target species is returned, fire an `OnEvolutionReady` event.
3. `GameplayManager` subscribes, triggers the evolution sequence:
   - Disable player input
   - Fade out
   - Call `DigimonInstance.Evolve(newSpecies)`
   - Show a simple overlay text: "{OldName} evolved into {NewName}!"
   - Fade in
   - Re-enable input

No separate evolution screen — just a fade + text overlay. Cutscene-quality presentation deferred to Phase 6.

### Death & reincarnation trigger

When `_age >= _species.LifespanHours`:
1. CareSystem fires `OnPartnerDied` event.
2. GameplayManager handles: fade, show "Your partner has passed away..." text, call `Die()` to snapshot inheritance, call `Reincarnate(botamonSpecies, inheritance)`, fade back.
3. Partner is now a Fresh-stage Digimon with inherited bonuses.

### Resolved decisions

- **Techniques on evolution:** Wipe and re-seed from new species' learnable list (matches original).
- **Multiple valid targets:** Array-order priority — designer controls via ordering in the EvolutionTable SO.
- **No devolution:** The death/reincarnation cycle is the only reset. No mid-life stage regression.

---

## 5-B. Quest System

### What the original game does

Quests in DW1 are mostly implicit: NPCs ask you to do something (defeat a Digimon, fetch an item, visit a location), and when the condition is met, you return and they reward you (often by joining the city). There is no quest log in the original — the player just remembers. We stay faithful to the original: no quest log UI.

### Data

**QuestObjective** (serializable struct):

| Field | Type | Notes |
|---|---|---|
| `_description` | `string` | "Defeat Ogremon in Zone 3" |
| `_type` | `QuestObjectiveType` | enum: `DefeatDigimon`, `CollectItem`, `TalkToNPC`, `VisitZone`, `DeliverItem` |
| `_targetId` | `string` | Identifier for the target (species name, item name, NPC id, zone name). Matched at runtime |
| `_requiredCount` | `int` | How many (kills, items, etc). Default 1 |

**QuestReward** (serializable struct):

| Field | Type |
|---|---|
| `_bits` | `int` |
| `_rewardItems` | `ItemReward[]` (struct: `ItemData` + `int count`) |
| `_recruitId` | `string` | If non-empty, completing this quest triggers recruitment of this NPC |

**QuestData** (`ScriptableObject`, `[CreateAssetMenu(menuName = "DigimonWorld/QuestData")]`):

| Field | Type |
|---|---|
| `_questId` | `string` |
| `_questName` | `string` |
| `_description` | `string` |
| `_objectives` | `QuestObjective[]` |
| `_rewards` | `QuestReward` |
| `_introDialogue` | `DialogueData` | Dialogue when quest is offered |
| `_completeDialogue` | `DialogueData` | Dialogue on completion |
| `_prerequisiteQuestIds` | `string[]` | Quest IDs that must be complete first |

### Runtime state

**QuestInstance** (plain C# class):

| Field | Type |
|---|---|
| `Quest` | `QuestData` |
| `State` | `QuestState` enum: `Available`, `Active`, `Completed` |
| `ObjectiveProgress` | `int[]` | Parallel to `QuestData._objectives`, tracks current count |

### QuestManager (plain MonoBehaviour on _Gameplay, not a singleton)

Wired via `GameplayManager` like other systems.

**Fields:**
- `_activeQuests: List<QuestInstance>`
- `_completedQuestIds: HashSet<string>`

**Methods:**
- `StartQuest(QuestData)` — creates QuestInstance, sets Active
- `IsQuestComplete(string questId) → bool`
- `IsQuestActive(string questId) → bool`
- `ArePrerequisitesMet(QuestData) → bool`
- `ReportProgress(QuestObjectiveType type, string targetId, int count = 1)` — called by game systems when something relevant happens. Increments matching objective progress. If all objectives met, marks quest completable.
- `CompleteQuest(QuestInstance)` — grants rewards, moves to Completed, fires `OnQuestCompleted` event.

**Events:**
- `OnQuestStarted(QuestData)`
- `OnQuestCompleted(QuestData)`
- `OnObjectiveProgress(QuestData, int objectiveIndex)`

### Integration hooks

Systems call `QuestManager.ReportProgress(...)`:
- **BattleSystem.EndBattle** — on Win, report `DefeatDigimon` with species name.
- **Inventory.AddItem** — report `CollectItem` with item name.
- **DialogueManager** or NPC — report `TalkToNPC` with NPC id.
- **GameplayManager.LoadZone** — report `VisitZone` with zone name.
- **Inventory.RemoveItem** (when delivering) — report `DeliverItem`.

These are one-line calls. Systems don't need to know about quests — they just report facts.

### NPC quest integration

NPCs gain an optional `_quest: QuestData` field. Interaction flow:
1. If quest is `Available` and prerequisites met → show intro dialogue, then start quest.
2. If quest is `Active` and all objectives met → show complete dialogue, grant rewards.
3. If quest is `Active` and objectives not met → show a "keep going" fallback line.
4. If quest is `Completed` or no quest → show default dialogue.

This replaces the current single-dialogue interaction with a small state machine inside `NPCInteractable`.

---

## 5-C. Recruitment / Befriending

### What the original game does

Most recruitable Digimon join after you complete their quest (defeat them, bring them an item, etc.). Some join after losing a battle. Once recruited, they move to File City and provide a service.

### Data

**RecruitData** (serializable struct, embedded in `NPCData` or used standalone):

| Field | Type | Notes |
|---|---|---|
| `_recruitId` | `string` | Matches `QuestReward._recruitId` |
| `_digimonName` | `string` | Display name |
| `_cityRole` | `CityRole` | enum: `Shop`, `Clinic`, `Farm`, `Gym`, `Info`, `Guard`, `Chef`, `Other` |
| `_cityZone` | `ZoneData` | Where they appear in the city after recruitment |
| `_cityPosition` | `Vector3` | Where in the zone |

### RecruitmentManager (plain MonoBehaviour on _Gameplay)

**Fields:**
- `_recruitedIds: HashSet<string>`

**Methods:**
- `Recruit(string recruitId)` — adds to set, fires `OnRecruitment` event
- `IsRecruited(string recruitId) → bool`
- `RecruitedCount → int`

**Events:**
- `OnRecruitment(string recruitId)`

### Trigger

`QuestManager.CompleteQuest` checks if `QuestReward._recruitId` is non-empty → calls `RecruitmentManager.Recruit(id)`.

Post-battle recruitment (lose-to-recruit pattern): `WildDigimon` can have an optional `_recruitOnDefeat: string` field. On battle Win, if set, call `RecruitmentManager.Recruit(id)`.

### NPC spawning in city

City zone scenes check `RecruitmentManager.IsRecruited(id)` to activate/deactivate NPC GameObjects. The NPCs exist in the scene but start disabled — recruitment enables them.

---

## 5-D. City-Building

### What the original game does

File City starts nearly empty. As you recruit Digimon, they move in and open facilities (shop, clinic, item farm, training gym, restaurant). The city visually changes — new buildings appear, the population counter goes up. Prosperity rating rises with recruits.

### Design

City growth is driven entirely by recruitment — no separate resource/construction system.

**CityData** (`ScriptableObject`):

| Field | Type |
|---|---|
| `_cityName` | `string` |
| `_facilities` | `CityFacility[]` |

**CityFacility** (serializable struct):

| Field | Type | Notes |
|---|---|---|
| `_facilityName` | `string` | "Item Shop", "Clinic", etc. |
| `_requiredRecruitId` | `string` | Which recruit unlocks this |
| `_facilityType` | `CityRole` | Maps to gameplay function |
| `_buildingPrefab` | `GameObject` | Optional — visual to activate in the zone |

### CityManager (plain MonoBehaviour on _Gameplay)

**Fields:**
- `_cityData: CityData`
- Serialized ref to `RecruitmentManager`

**Methods:**
- `GetUnlockedFacilities() → List<CityFacility>` — filters by recruited status
- `Prosperity → int` — `RecruitmentManager.RecruitedCount`
- `IsFacilityUnlocked(CityRole role) → bool`

**On zone load:** When the city zone loads, `CityManager` activates building GameObjects and NPC objects for all unlocked facilities. This reuses the same enable/disable pattern from recruitment.

### Integration with gameplay

- **Shop** unlocks when a Shop-role Digimon is recruited. The shop NPC's `Interact()` opens the ShopScreen.
- **Clinic** unlocks a healing NPC: interact to fully restore HP/MP (for a Bits cost).
- **Gym** may add new training facilities to the city zone.
- Other roles provide lore NPCs, item gifts, etc.

The HUD could optionally show city prosperity. Defer this to content pass or Phase 6.

---

## 5-E. Shop UI

### What the original game does

Simple buy/sell screen. Browse items, see price, buy with Bits. Separate sell tab lets you sell inventory items for their sell price.

### Data

**ShopInventoryData** (`ScriptableObject`, `[CreateAssetMenu(menuName = "DigimonWorld/ShopInventoryData")]`):

| Field | Type |
|---|---|
| `_shopName` | `string` |
| `_itemsForSale` | `ShopEntry[]` |

**ShopEntry** (serializable struct):

| Field | Type |
|---|---|
| `_item` | `ItemData` |
| `_stock` | `int` | -1 = infinite |

### ShopScreen (MonoBehaviour, UI screen)

Canvas sorting order: 80 (same as Inventory/Status).

**Layout:**
- Top: Shop name + player's Bits
- Two tabs: Buy / Sell (switch with Q/E or A/D)
- Buy tab: list of shop items with price, W/S to navigate, E to buy, quantity selector (optional: just buy 1 at a time for simplicity)
- Sell tab: list of player inventory items with sell price, E to sell
- Bottom: selected item description

**Flow:**
1. NPC with `CityRole.Shop` + `ShopInventoryData` reference.
2. On interact → `GameplayManager` opens ShopScreen, passes the `ShopInventoryData`.
3. Buy: check `Inventory.Bits >= price`, call `SpendBits` + `AddItem`.
4. Sell: call `RemoveItem` + `AddBits(sellPrice)`.
5. Close with Escape or Q from root menu.

Player input disabled while shop is open (same pattern as other screens).

### ShopNPCInteractable

New interactable type (or extend `NPCInteractable` with a shop mode). Has:
- `_shopInventory: ShopInventoryData`
- `_greeting: DialogueData` (optional, shown before shop opens)

On interact: show greeting dialogue (if any) → open shop screen.

---

## Implementation Order

Each step below is a self-contained commit that compiles and runs.

### Step 1: Evolution data + logic
- `EvolutionRequirement` struct
- `EvolutionTable` SO
- Add `_evolutionTable` field to `DigimonSpeciesData`
- Add `CheckEvolution`, `Evolve` methods to `DigimonInstance`
- Add `_totalLives`, `_inheritedStatBonuses` fields to `DigimonInstance`
- Generator: create sample EvolutionTable SOs (Botamon→Koromon→Agumon→Greymon→MetalGreymon chain)

### Step 2: Evolution trigger + death/reincarnation
- CareSystem: hourly evolution check, `OnEvolutionReady` event
- CareSystem: lifespan check, `OnPartnerDied` event  
- `DigimonInstance.Die()`, `Reincarnate()` methods
- `DigimonInheritance` data struct
- GameplayManager: subscribe to events, run fade + text overlay sequences

### Step 3: Quest data + QuestManager
- `QuestObjectiveType` enum
- `QuestObjective`, `QuestReward`, `ItemReward` structs
- `QuestData` SO
- `QuestInstance` runtime class
- `QuestManager` MonoBehaviour
- Wire into GameplayManager

### Step 4: Quest integration hooks
- BattleSystem → `ReportProgress(DefeatDigimon, ...)`
- Inventory → `ReportProgress(CollectItem, ...)`
- GameplayManager.LoadZone → `ReportProgress(VisitZone, ...)`
- NPCInteractable quest state machine (intro/progress/complete/default)
- Generator: create sample quest data

### Step 5: Recruitment
- `CityRole` enum
- `RecruitData` struct
- `RecruitmentManager` MonoBehaviour
- QuestManager → RecruitmentManager hook on quest complete
- WildDigimon `_recruitOnDefeat` field
- Wire into GameplayManager

### Step 6: City-building
- `CityFacility` struct, `CityData` SO
- `CityManager` MonoBehaviour
- Zone load → activate/deactivate city objects
- Wire into GameplayManager
- Generator: sample CityData, city zone with gated objects

### Step 7: Shop UI
- `ShopEntry` struct, `ShopInventoryData` SO
- `ShopScreen` MonoBehaviour + prefab
- `ShopNPCInteractable` (or extend NPCInteractable)
- Wire into GameplayManager
- Generator: sample ShopInventoryData, shop NPC in city zone

### Step 8: Generator updates + content pass
- Update GeneratorWindow with all new generators
- Update GeneratePrefabs with new prefabs (ShopScreen)
- Update GenerateScenes to wire new systems in _Gameplay scene
- Create sample content: evolution chains, 3-4 quests, 2-3 recruitable NPCs, shop inventory
- Verify full loop: raise → evolve → quest → recruit → city grows → shop opens → death → reincarnate

---

## New Files Summary

| File | Type | Location |
|---|---|---|
| `EvolutionRequirement.cs` | Struct | Scripts/ |
| `EvolutionTable.cs` | ScriptableObject | Scripts/ |
| `DigimonInheritance.cs` | Struct | Scripts/ |
| `QuestObjectiveType.cs` | Enum | Scripts/ |
| `QuestObjective.cs` | Struct | Scripts/ |
| `QuestReward.cs` | Struct | Scripts/ |
| `QuestData.cs` | ScriptableObject | Scripts/ |
| `QuestInstance.cs` | Class | Scripts/ |
| `QuestManager.cs` | MonoBehaviour | Scripts/ |
| `CityRole.cs` | Enum | Scripts/ |
| `RecruitData.cs` | Struct | Scripts/ |
| `RecruitmentManager.cs` | MonoBehaviour | Scripts/ |
| `CityFacility.cs` | Struct | Scripts/ |
| `CityData.cs` | ScriptableObject | Scripts/ |
| `CityManager.cs` | MonoBehaviour | Scripts/ |
| `ShopEntry.cs` | Struct | Scripts/ |
| `ShopInventoryData.cs` | ScriptableObject | Scripts/ |
| `ShopScreen.cs` | MonoBehaviour | Scripts/ |
| `ShopNPCInteractable.cs` | MonoBehaviour | Scripts/ |
| `GenerateShopScreenPrefab.cs` | Editor | Scripts/Editor/Generators/ |

## Modified Files Summary

| File | Change |
|---|---|
| `DigimonSpeciesData.cs` | Add `_evolutionTable` field |
| `DigimonInstance.cs` | Add evolution/death/reincarnation fields + methods |
| `CareSystem.cs` | Hourly evolution check, lifespan check, new events |
| `GameplayManager.cs` | Add refs to QuestManager, RecruitmentManager, CityManager, ShopScreen. Subscribe to evolution/death events. Shop screen open/close. |
| `BattleSystem.cs` | Report quest progress on battle win |
| `Inventory.cs` | Report quest progress on item add |
| `NPCInteractable.cs` | Quest state machine, optional quest field |
| `WildDigimon.cs` | Optional `_recruitOnDefeat` field |
| `EncounterData.cs` | Optional `_recruitId` field |
| `GeneratorWindow.cs` | New generator buttons |
| `GeneratePrefabs.cs` | New data asset generators |
| `GenerateScenes.cs` | Wire new systems, city zone objects |
| `DigimonEnums.cs` | No changes expected (CityRole gets its own file) |

---

## Resolved Decisions

| # | Question | Decision |
|---|---|---|
| 1 | Technique handling on evolution | Wipe and re-seed from new species' learnable list (matches original) |
| 2 | Devolution | No devolution. Death/reincarnation is the only reset path |
| 3 | Multiple evolution targets qualifying | Array-order priority — designer controls via SO ordering |
| 4 | Quest log UI | No quest log — faithful to the original. Player remembers |
| 5 | Shop quantity selector | Buy/sell one at a time |
| 6 | City zone | Dedicated File City zone (new scene, not reusing Zone1/Zone2) |
| 7 | Clinic pricing | Flat fee per stage (Fresh/InTraining=50, Rookie=100, Champion=300, Ultimate=500) |
| 8 | NPC interactable refactor | Separate classes (`QuestNPCInteractable`, `ShopNPCInteractable`) implementing `IInteractable` |
