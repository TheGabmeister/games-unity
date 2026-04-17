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

See [CLAUDE.md](CLAUDE.md) for contributor conventions and architecture notes.

## Tooling

- **Unity 6000.3.12f1** (URP 17.3, URP 2D Renderer). See [ProjectSettings/ProjectVersion.txt](ProjectSettings/ProjectVersion.txt).
- **Packages of note** ([Packages/manifest.json](Packages/manifest.json)):
  - `com.unity.inputsystem` — new Input System; action asset at [Assets/_Project/Input/InputSystem_Actions.inputactions](Assets/_Project/Input/InputSystem_Actions.inputactions).
  - `com.unity.vectorgraphics` — SVG importer; all visual assets are `.svg` and tessellate to sprites at import.
  - `com.kyrylokuzyk.primetween` — tweening lib (via npm scoped registry); prefer it over DOTween or coroutines for tweens.
  - `com.unity.feature.2d`, `com.unity.2d.aseprite`, `com.unity.2d.spriteshape`, `com.unity.2d.tilemap.extras`.
  - `com.unity.test-framework` — EditMode/PlayMode tests.
- **Solution:** `MegamanX4.slnx`. Two assemblies: `MegamanX4.Runtime` (refs `Unity.InputSystem`, `PrimeTween.Runtime`) and `MegamanX4.Editor`.

## Build / test

Unity project — no CLI build script is checked in. Open the project in the Unity Editor (`Unity 6000.3.12f1`) to play/build. Tests are run from **Window → General → Test Runner** (EditMode and PlayMode). For a CI-style run: `Unity.exe -batchmode -projectPath <path> -runTests -testPlatform {EditMode|PlayMode}`.

Generator utilities are exposed as editor menu items under **Tools/MegamanX4/** (e.g. `Generate Sky Lagoon Enemies`). Run them from the menu bar; they are idempotent and overwrite existing output. Canonical example: [SkyLagoonEnemyGenerator.cs](Assets/_Project/Scripts/Editor/SkyLagoonEnemyGenerator.cs).

## Layout

```
Assets/_Project/
├── Defaults/            TargetDummy.prefab, ProjectileDefault.prefab (enemy-shot reference)
├── Enemies/
│   ├── Recurring/       KnotBeretB, KnotBeretG, Kyunnbyunn, SpikeMarl (SVG + prefab per enemy)
│   ├── SkyLagoon/       MadBull97, TonboroidS, TrapBlast (SVG + prefab per enemy)
│   └── AirForce/, BioLaboratory/, CyberSpace/, Jungle/, MarineBase/, MilitaryTrain/, SnowBase/, Volcano/
│                        (one folder per stage; populated per SPEC_ENEMIES.md phased roadmap)
├── Input/               InputSystem_Actions.inputactions
├── Interactables/       Ladder.prefab
├── Player/
│   ├── Character/       MegamanX_{Idle,Jump,Fall,Dash}.svg
│   └── MegamanX.prefab  (Rigidbody2D + Collider2D + PlayerInput + PlayerController
│                         + Health + InvulnerabilityBlinker + child Visual)
├── Scenes/              Gameplay.unity
├── Scripts/
│   ├── Behavior/                  reusable movement/lifecycle components
│   │   ├── Gravity.cs             kinematic gravity with ground raycast + max fall speed
│   │   ├── HoverSine.cs           sine Y oscillation around a recorded center; pausable
│   │   ├── Lifetime.cs            auto-destroy timer
│   │   ├── MoveForward.cs         translate along transform.right
│   │   └── MoveVertical.cs        translate along ±transform.up
│   ├── Damage/
│   │   ├── Health.cs              HP, damage with source position, i-frames, event bus
│   │   ├── HitBox.cs              deals damage on contact (trigger/collision → HurtBox)
│   │   ├── HurtBox.cs             receives hits, routes to Health.ApplyDamage
│   │   ├── InvulnerabilityBlinker.cs  SpriteRenderer blink during i-frames (player-style)
│   │   └── DamageFlash.cs         SpriteRenderer white-flash on hit (enemies / destructibles)
│   ├── Editor/
│   │   ├── FileExtensions.cs      Project-panel extension labels
│   │   ├── SkyLagoonEnemyGenerator.cs  prefab generator menu; canonical composition pattern
│   │   └── MegamanX4.Editor.asmdef
│   ├── Enemy/                     enemy behaviors (composition-ready components)
│   │   ├── DestroyOnDepleted.cs   lifecycle: Health.Depleted → Destroy (reusable: enemies, destructibles)
│   │   ├── PlayerDetector.cs      radial OverlapCircle + optional LoS raycast
│   │   ├── PatrolWalk.cs          walk + wall/ledge raycast flip, pausable
│   │   ├── EnemyShoot.cs          polls CanSeePlayer, aims muzzle, fires burst with cooldown
│   │   ├── AutoShoot.cs           fire-and-forget projectile spawn at interval
│   │   ├── SwoopAttack.cs         Idle/Diving/Returning/Cooldown state machine
│   │   ├── DropTrigger.cs         overlap-below detection → enable Dynamic RB + gravity
│   │   └── DestroyOnWallContact.cs  self-destruct on Environment collision
│   ├── Player/
│   │   ├── PlayerController.cs    movement, input, sprite swap, knockback, ladder
│   │   ├── WeaponInventory.cs     weapon list + charge state machine + Q/E switch + tint
│   │   ├── WeaponData.cs          ScriptableObject: displayName, tint, small/semi/full prefab, energy block
│   │   └── DashSilhouetteTrail.cs LateUpdate-driven sprite afterimage trail
│   ├── Services/                  persistent systems (Bootstrapper, SfxManager,
│   │                              MusicManager, ScreenFader, SceneLoader/)
│   ├── Description.cs             editor-only TextArea annotation on any GO
│   ├── HUD.cs                     event-driven gameplay HUD (HP + energy), wired via Bind()
│   ├── Layers.cs                  compile-time layer index constants (mirror of TagManager.asset)
│   ├── Projectile.cs              lifecycle only: wall collision, piercing, Destroyed event
│   ├── StageSession.cs            spawns player + HUD, calls HUD.Bind, owns PlayerStart fallback
│   └── MegamanX4.Runtime.asmdef
├── Settings/              URP / Renderer2D / Volume profile assets
├── UI/                    GameplayHUD.prefab
└── Weapons/
    ├── Buster/        BusterShot_{Small,Semi,Full}.{svg,prefab}
    ├── DoubleCyclone/ (placeholder)
    ├── FrostTower/    FrostTower.svg
    ├── GroundHunter/  (placeholder)
    ├── RisingFire/    (placeholder)
    ├── SoulBody/      (placeholder)
    └── TwinSlasher/   TwinSlasher.svg
```

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
- [x] Coyote time / jump buffer (if desired for feel)
- [x] Dash (ground, fixed distance, cancelable)
- [x] Dash-jump (extended air speed after ground dash into jump)
- [x] Air dash *(X only, via Fourth Armor Foot Parts in the real game)*
- [x] Wall slide (friction-slowed descent while pressing into wall)
- [x] Wall jump (diagonal kick off wall, consumes wall contact)
- [x] Ladder climb / top-of-ladder dismount
- [x] Damage knockback / invincibility frames
- [x] Death + respawn *(simple scene reload; checkpoints later)*

## 2. Mega Man X — combat

- [x] X-Buster: small lemon shot
- [x] X-Buster: semi-charged shot
- [x] X-Buster: full-charged shot
- [x] 3-shot on-screen cap for lemons
- [ ] Charge hold visual (arm flash / particles)
- [ ] Plasma / charged-shot muzzle FX
- [x] Weapon-energy meter per special weapon
- [ ] Charged versions of each special weapon
- [x] Weapon swap input (shoulder buttons / cycle)
- [ ] Animation locks during firing.
- [ ] On-screen shot caps for special weapons (only the buster's 3-lemon cap carries over).
- [x] Per-weapon energy pools and depletion-driven auto-switch.

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

- [ ] Opening / intro stage — Sky Lagoon
- [ ] Web Spider — Jungle
- [ ] Cyber Peacock — Cyber Space
- [ ] Storm Owl — Air Force
- [ ] Magma Dragoon — Volcano
- [ ] Jet Stingray — Marine Base
- [ ] Slash Beast — Military Train
- [ ] Frost Walrus — Snow Base
- [ ] Split Mushroom — Bio Laboratory
- [ ] Colonel — Memorial Hall
- [ ] Space Port
- [ ] Final Weapon 1
- [ ] Final Weapon 2
- [ ] Final Weapon 3 / Sigma
- [ ] Ride Armor / Ride Chaser vehicle sections
- [ ] Stage-select screen

### Recurring Enemies

- [x] Knot Beret B *(Sky Lagoon, Military Train)*
- [x] Knot Beret G *(Sky Lagoon, Military Train)*
- [x] Spike Marl *(Sky Lagoon, Cyber Space)*
- [x] Kyunnbyunn *(Sky Lagoon, Jungle)*
- [x] Blast Raster *(Jungle, Bio Laboratory)*
- [x] Hover Gunner *(Cyber Space, Marine Base, Bio Laboratory)*
- [x] Giga Death *(Air Force, Volcano)*
- [x] Plasma Cannon *(Air Force, Military Train)*
- [x] Batton Bone B81 *(Volcano, Military Train, Bio Laboratory)*
- [x] Mettaur D2 *(Volcano, Military Train, Snow Base)*
- [x] Spiky Mk-II *(Volcano, Bio Laboratory)*
- [ ] Raiden *(Volcano, Military Train — AI-piloted Ride Armor; deferred pending Ride Armor subsystem)*

### Sky Lagoon enemy roster

- [x] Tonboroid S
- [x] Mad Bull 97
- [x] Trap Blast
- [ ] Eregion *(boss)*

### Jungle enemy roster

- [x] Kill Fisher
- [x] Metal Gabyoall *(floor-only; surface-crawl deferred)*
- [ ] King Poseidon *(deferred pending water system)*
- [x] Obiiru
- [x] Mega Nest
- [x] Spider Core
- [ ] Web Spider *(boss)*

### Cyber Space enemy roster

- [x] Miru Toraeru
- [x] TriScan
- [x] Protecton
- [ ] Cyber Peacock *(boss)*

### Air Force enemy roster

- [ ] Beam Cannon
- [ ] Metal Hawk
- [ ] Walk Shooter
- [ ] Generaid Core *(sub-boss)*
- [ ] Storm Owl *(boss)*

### Volcano enemy roster

- [ ] Prominence
- [ ] Magma Dragoon *(boss)*

### Marine Base enemy roster

- [ ] Hornet
- [ ] Jet Stingray *(boss / stage attacker)*

### Military Train enemy roster

- [ ] DG-42L *(sub-boss)*
- [ ] Slash Beast *(boss)*

### Snow Base enemy roster

- [ ] E-AT
- [ ] Yukidarubon
- [ ] Knot Beret S
- [ ] Fly Gunner
- [ ] Ice Wing
- [ ] Eyezard *(sub-boss)*
- [ ] Frost Walrus *(boss)*

### Bio Laboratory enemy roster

- [ ] Tentoroid RS
- [ ] Tentoroid BS
- [ ] Togerics
- [ ] Dejira
- [ ] Guardian
- [ ] Death Guardian
- [ ] Tentoroid *(sub-boss)*
- [ ] Split Mushroom *(boss)*

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

- [x] Pause menu
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
