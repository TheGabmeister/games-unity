# Digimon World 1 — Unity Recreation

A gameplay-focused recreation of Digimon World 1 in Unity. Not a pixel-perfect remake — the goal is faithful mechanics with placeholder 3D models (no animations), placeholder textures, and placeholder audio.

## Scope & Constraints

- Gameplay mechanics over visual fidelity
- Placeholder 3D models, no animations
- Placeholder textures
- Placeholder music and SFX
- Single-player

## Implementation Order

Each system enters the plan at the phase where gameplay first actually needs it — not upfront. If infrastructure isn't required until Phase 3, it lands in Phase 3. KISS/YAGNI: build what this phase needs, refactor when a second use case shows up.

---

### Phase 0 — Foundation

The bare minimum plumbing to let Phase 1 start. Nothing else — every other "foundation" system (audio, UI framework, debug console, scene loader) waits until the phase that first needs it.

1. **Bootstrap + Service Locator** — `Bootstrapper` instantiates a persistent `Systems` prefab with `DontDestroyOnLoad`; `ServiceLocator` lets other systems find services without hardcoded refs. Runs before any scene `Awake` (Script Execution Order −1000) plus an `EditorBootstrapLoader` so Play-from-any-scene works.
2. **Input System** — keyboard, mouse, gamepad; action maps (`Gameplay`, `UI`); core actions (`Move`, `Look`, `Interact`, `Pause`); `IInputService`. Needed immediately by Phase 1's player movement. Rebind UI deferred to Phase 6.

### Phase 1 — The Player in a World

Goal: walk a capsule around a test zone with a camera.

3. **Player Movement** — third-person controller, walk/run, gravity, ground check.
4. **Camera** — follow camera, look control, collision avoidance.
5. **World & Map (one test zone)** — a single zone with walkable terrain and colliders.
6. **Scene / Zone Loader** — async load/unload, scene references, progress hook. Introduced here because zone transitions need it. No loading screen yet (the UI framework doesn't exist — either hold the black frame or fade via a bare full-screen quad).
7. **Zone Transitions** — trigger volumes swap zones via the scene loader.
8. **Interaction System** — raycast prompts, `IInteractable` interface, world-space label for the prompt. No UI framework required for a single floating "Press E" label.

### Phase 2 — Companions, Conversations, Clocks

Goal: partner follows you; NPCs talk; the world has time; the first real screens exist.

9. **UI Framework** — `ScreenManager` stack (`Push`/`Pop`/`Replace`), `BaseScreen`, fullscreen fader, pause screen. Introduced here because dialogue, HUD, and pause all need consistent screens. The Phase 1 zone loader gets a proper `LoadingScreen` now that one can exist.
10. **Partner Digimon Follow** — AI-controlled companion, pathfinding/follow, idle behaviours.
11. **Dialogue System** — speaker data, branching trees, choice UI, conditional lines.
12. **NPC Entities** — dialogue-ready NPCs with patrol/idle behaviour.
13. **Time System** — in-game clock, day/night cycle, time-gated hooks.
14. **HUD** — partner status bar, clock, zone name, currency readout.

### Phase 3 — Digimon as Data

Goal: the partner is a real Digimon with stats, needs, and care.

15. **Debug Tools** — toggleable overlay, FPS counter, in-game console, command registry, starter commands (`help`, `clear`, `time.scale`, `scene.load`, plus care/stat set-commands). Introduced here because stats, hidden stats, and care timing are tedious to test by hand — this is the first phase where a console pays for itself. Compile-guarded out of release builds.
16. **Digimon Data Model** — species, stages, families, types, elements (ScriptableObjects).
17. **Digimon Instance** — runtime stats, age, hidden stats (hunger, weight, tiredness, discipline, happiness, care mistakes).
18. **Care System** — feeding, sleeping, bathroom, praise/scold, training stubs.
19. **Item & Inventory System** — item catalog, stackable inventory, use-on-partner, Bits currency.
20. **Status / Digimon Info UI** — stat screen, inventory screen.
21. **Training Facilities** — mini-game stubs that feed stats, tiredness/happiness gating.

### Phase 4 — Combat

Goal: fight wild Digimon, win, lose, flee.

22. **Audio System (bare)** — `AudioMixer` with `Master`/`Music`/`SFX`/`UI` buses, `IAudioService` with `PlaySFX`/`PlayMusic`/`StopMusic`/`SetBusVolume`, SFX one-shot pool, simple music player. Introduced here because battle hits, technique cues, and status changes are the first place silence genuinely hurts. If an earlier phase ends up needing a single SFX, pull this item forward — don't add ad-hoc `AudioSource.Play` calls.
23. **Techniques / Moves Data** — move list, MP cost, element, power.
24. **Battle System Core** — encounter start, turn/command flow, damage calc, type/element advantage.
25. **Enemy AI (Battle)** — move selection, targeting, retreat thresholds.
26. **Status Effects** — poison, paralysis, sleep, confusion, etc.
27. **Battle UI** — HP/MP, commands, tech list, log.
28. **Brains-Driven Control** — obedience scaling with Brains/Discipline.
29. **Overworld Encounters** — patrol/chase/flee enemies, encounter trigger.

### Phase 5 — Progression & Story

Goal: evolution, quests, recruitment, city growth.

30. **Evolution System** — requirements, transition event, devolution, Digitama inheritance.
31. **Quest System** — states, objectives, rewards, quest log UI.
32. **Recruitment / Befriending** — post-battle recruit logic, conditions.
33. **City-Building** — File City expansion tied to recruits, service unlocks (shop/clinic/farm/gym).
34. **Shop UI** — buy/sell against inventory and Bits.
35. **Main Story Quests + Side Quests (content pass).**

### Phase 6 — Presentation & Persistence

Goal: scripted moments, permanence, polish.

36. **Cutscene System** — timeline-style authoring, skippable, triggered cutscenes.
37. **Save / Load System** — multi-slot, autosave, full serialization of all the above, versioning.
38. **Main Menu & Save Slot UI** — new game, continue, load, options.
39. **Settings Menu** — audio mix, controls rebind, graphics, text speed, subtitles.
40. **Full Audio Pass** — zone music selection, battle music, SFX coverage, crossfades.
41. **Debug / Cheat Menu (full)** — spawn, evolve, teleport, fast-forward time, save inspector.

---

## Systems Reference

Full list grouped by domain — same systems as above, for quick scanning.

### Digimon & Progression
- Digimon data & instances
- Stats & hidden stats
- Life stages & lifespan
- Evolution & devolution
- Digitama inheritance

### Gameplay Loops
- Care (feed/sleep/bathroom/praise/scold)
- Training
- Battle + techniques + status effects
- Recruitment / befriending
- Quests
- City-building

### World
- Player movement & camera
- Partner follow
- Zones & transitions
- Overworld encounters
- Time / day-night

### Characters
- NPCs
- Enemy AI (overworld + battle)
- Partner AI

### Narrative
- Dialogue system
- Cutscenes

### Meta / Framework
- Input
- UI & menus (HUD, status, inventory, quest log, battle, shop, save/load, settings)
- Audio
- Save/load
- Settings & config
- Debug tools

## Content Data (placeholder-backed)

- Digimon roster and evolution tree
- Techniques / move list
- Item catalog
- NPC roster
- Quest definitions
- Zone / map definitions
- Dialogue scripts
- Cutscene scripts

## Out of Scope (for now)

- Animations
- Final art and models
- Final music and SFX
- Multiplayer
- Localization content (hook only)
