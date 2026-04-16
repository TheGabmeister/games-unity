# Mega Man X4 — Unity 6 Recreation

A mechanical-fidelity recreation of Capcom's *Mega Man X4* (1997, PlayStation / Sega Saturn) in Unity 6 with procedural SVG visuals and stubbed audio. Part of the `games-unity/` monorepo of retro recreations (SMW, FF1, ...).

## About the game

*Mega Man X4* is the fourth entry in Capcom's "X" sub-series of the *Mega Man* franchise, released in 1997 for the PlayStation and Sega Saturn and later bundled in the *Mega Man X Legacy Collection*. It is the first X-series title designed natively for 32-bit hardware and the first to let the player choose between two fully separate campaigns:

- **X** — ranged buster play, charge shots, armor part upgrades (Fourth Armor).
- **Zero** — melee play, Z-Saber combos, learned special techniques from bosses.

The narrative follows the conflict between the Maverick Hunters and the Repliforce after the "Sky Lagoon" incident. The eight Mavericks are:

| | | | |
|---|---|---|---|
| Web Spider | Cyber Peacock | Storm Owl | Magma Dragoon |
| Jet Stingray | Slash Beast | Frost Walrus | Split Mushroom |

Each Maverick drops a new weapon (X) or learned technique (Zero). The game culminates in Final Weapon stages and a climactic Sigma fight.

## Project goals

- Mechanics-first: feel, physics, and combat behavior before aesthetics.
- Procedural / primitive visuals (SVG via `com.unity.vectorgraphics`).
- Audio stubbed via enum-keyed `SfxId` / `MusicId` until real assets land.
- ScriptableObject-driven data for enemies, weapons, stages, palettes.
- Hand-authored scenes; no scripted scene composition saved to disk.
- Composition over inheritance.

See [CLAUDE.md](CLAUDE.md) for contributor conventions.

---

# Gameplay feature checklist

Use this list to track implementation progress. Tick a box when a feature is playable end-to-end (not just scaffolded). Each section is rough scope order, not strict phase order.

## 1. Core player physics *(shared by X and Zero)*

- [x] Horizontal run / walk
- [x] Variable-height jump (release cuts upward velocity)
- [x] Gravity + terminal fall speed
- [x] Swept-cast collision resolution (kinematic rb + `rb.Cast`)
- [x] Ground probe / wall probe state
- [x] Facing direction (int, independent of transform scale)
- [x] Sprite flip on child `Visual` (root never flips)
- [ ] Coyote time / jump buffer (if desired for feel)
- [x] Dash (ground, fixed distance, cancelable)
- [ ] Dash-jump (extended air speed after ground dash into jump)
- [x] Air dash *(X only, via Fourth Armor Foot Parts in the real game)*
- [x] Wall slide (friction-slowed descent while pressing into wall)
- [x] Wall jump (diagonal kick off wall, consumes wall contact)
- [ ] Ladder climb / top-of-ladder dismount
- [ ] Damage knockback / invincibility frames *(specced in [SPEC2.md](SPEC2.md); prerequisite for SPEC.md ladder damage)*
- [ ] Death + respawn at checkpoint

## 2. Mega Man X — combat

- [x] X-Buster: small lemon shot
- [x] X-Buster: semi-charged shot
- [x] X-Buster: full-charged shot
- [x] 3-shot on-screen cap for lemons
- [ ] Charge hold visual (arm flash / particles)
- [ ] Plasma / charged-shot muzzle FX
- [ ] Weapon-energy meter per special weapon
- [ ] Charged versions of each special weapon
- [ ] Weapon swap input (shoulder buttons / cycle)

### Special weapons (one per Maverick)

- [ ] Lightning Web *(Web Spider)*
- [ ] Aiming Laser *(Cyber Peacock)*
- [ ] Double Cyclone *(Storm Owl)*
- [ ] Rising Fire *(Magma Dragoon)*
- [ ] Ground Hunter *(Jet Stingray)*
- [ ] Twin Slasher *(Slash Beast)*
- [ ] Frost Tower *(Frost Walrus)*
- [ ] Soul Body *(Split Mushroom)*

## 3. Mega Man X — Fourth Armor parts

- [ ] **Helmet** — item detection / stage map reveal
- [ ] **Body** — damage reduction + defensive aura on hit
- [ ] **Arms** — Plasma Shot / Stock Charge Shot
- [ ] **Legs** — Air Dash
- [ ] Ultimate Armor cheat variant (Nova Strike)
- [ ] Armor capsule pickup flow (Dr. Light cutscene stub)

## 4. Zero — combat

- [ ] Base Z-Saber 3-hit ground combo
- [ ] Air Z-Saber slash
- [ ] Dash-saber attack
- [ ] Zero's buster (early-game only, pre-saber)
- [ ] Charge attacks for learned techniques

### Learned techniques (one per Maverick)

- [ ] Raijingeki *(Web Spider)*
- [ ] Hienkyaku — air dash *(Cyber Peacock)*
- [ ] Tenkuuha *(Storm Owl)*
- [ ] Ryuenjin *(Magma Dragoon)*
- [ ] Hyouretsuzan *(Frost Walrus)*
- [ ] Shippuuga *(Slash Beast)*
- [ ] Kuuenzan *(Jet Stingray)*
- [ ] Soul Launcher *(Split Mushroom)*

## 5. Health, damage, pickups

- [x] Health component (player) — [Health.cs](Assets/_Project/Scripts/Health.cs)
- [x] Contact damage from enemies
- [ ] Life bar HUD
- [ ] Weapon-energy HUD (X)
- [ ] Boss health bar (vertical, fill-up intro)
- [ ] Small / large life capsule pickups
- [ ] Small / large weapon-energy capsule pickups
- [ ] Extra life (1-Up)
- [ ] Sub-tanks (collect, fill, manually use)
- [ ] Heart tanks (max-HP upgrades, one per stage)
- [ ] E-Tank-equivalent / W-Tank / EX-Tank if scope permits

## 6. Enemies & hazards

- [x] Test enemy stub
- [ ] Enemy ScriptableObject stat definitions
- [ ] Enemy shared damage/health composition (components, not inheritance)
- [ ] Per-stage enemy roster (Mets, Ride Armors, gunners, drones, ...)
- [ ] Enemy projectile system (re-use `BusterShot` pattern)
- [ ] Weapon weakness table (each boss weak to a specific weapon)
- [ ] Environmental hazards (spikes = instant death, lava, crusher, pits)
- [ ] Destructible blocks

## 7. Bosses

Eight Mavericks plus bosses of the Final Weapon and Sigma stages.

- [ ] Boss intro sequence (walk-in + health bar fill)
- [ ] Web Spider
- [ ] Cyber Peacock
- [ ] Storm Owl
- [ ] Magma Dragoon
- [ ] Jet Stingray
- [ ] Slash Beast
- [ ] Frost Walrus
- [ ] Split Mushroom
- [ ] Mid-bosses (e.g. Magma Dragoon stage mid-boss, Jet Stingray chase)
- [ ] Colonel *(story)*
- [ ] Double *(X campaign twist fight)*
- [ ] Iris *(Zero campaign)*
- [ ] General
- [ ] Sigma phase 1
- [ ] Sigma phase 2 (final form)

## 8. Stages

- [ ] Opening / intro stage (Sky Lagoon)
- [ ] Web Spider — forest / cyber-forest
- [ ] Cyber Peacock — cyberspace data stage (rank-based time-trial rooms)
- [ ] Storm Owl — airship interior + exterior
- [ ] Magma Dragoon — volcano / magma
- [ ] Jet Stingray — high-speed water-ski chase
- [ ] Slash Beast — speeding train
- [ ] Frost Walrus — ice base
- [ ] Split Mushroom — neon nightclub / illusion tower
- [ ] Final Weapon 1
- [ ] Final Weapon 2
- [ ] Final Weapon 3 / Sigma
- [ ] Ride Armor / Ride Chaser vehicle sections
- [ ] Stage-select screen

## 9. Progression / meta

- [ ] Character select (X vs Zero) at new game
- [ ] Stage-select grid with portrait + status
- [ ] Stage-clear summary screen
- [ ] Acquire weapon / technique on boss defeat
- [ ] Save / load (slot-based)
- [ ] Difficulty options (*X4* shipped with difficulty selection)
- [ ] Cutscenes (stubbed placeholder panels; real anime FMVs out of scope)

## 10. Audio *(stubbed)*

- [ ] `SfxId` enum + ScriptableObject catalog
- [ ] `MusicId` enum + stage/BGM routing
- [ ] `AudioService` wrapper (no direct `AudioClip` refs from gameplay code)
- [ ] Per-stage BGM
- [ ] Boss BGM
- [ ] SFX: shot fire, charge loop, jump, dash, land, damage, death, pickup
- [ ] Voice stubs (X / Zero shouts, saber vocal)

## 11. UI / HUD / front-end

- [ ] Pause menu
- [ ] Title screen
- [ ] Boot flow (`Init` → `Gameplay` additive load)
- [ ] In-game HUD: life bar, weapon bar, sub-tank, boss bar
- [ ] Pause + weapon-select menu
- [ ] Game-over screen
- [ ] Credits screen (stub)

## 12. Tech debt / infrastructure

- [ ] `SPEC.md` with phased roadmap
- [ ] Split `Assembly-CSharp` into `.asmdef` modules when it grows
- [ ] EditMode tests for `PlayerController` movement math
- [ ] PlayMode smoke test for scene boot
- [ ] Input rebinding UI
