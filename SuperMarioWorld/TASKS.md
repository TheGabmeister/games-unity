# TASKS.md — Implementation Phases

Build-order checklist. Each phase lists **tasks** (one line each, pointing at the SPEC.md section that describes what to build), **automated tests**, and **manual verification** (including bugs to watch for). Architectural detail lives in [SPEC.md](SPEC.md); this file is the working scorecard.

Do not skip phases — each validates the architecture against real gameplay.

**Test legend:** `EM` = EditMode test (runs without a loaded scene — fast, for pure-logic and asset validation). `PM` = PlayMode test (runs in a Play session with full Unity lifecycle — slower, for gameplay assertions). Tests with no suffix default to EditMode.

---

## Phase 0 — Project foundation

### Tasks
- Assembly definitions `SMW.Runtime` / `SMW.Editor` / `SMW.Tests` ([SPEC.md §2](SPEC.md), CLAUDE.md).
- Physics 2D collision matrix ([SPEC.md §4.19](SPEC.md)). Commit `ProjectSettings/Physics2DSettings.asset` and `ProjectSettings/TagManager.asset`.
- Input action maps + `PlayerInputManager` on an `Input` GameObject in Systems.unity (no `PlayerInput` yet — that lives on the Player prefab in Phase 1). Map switching driven by `GameStateMachine` states iterating `PlayerInput.all` ([SPEC.md §4.1](SPEC.md)).
- `Boot.unity`, `Systems.unity`, `Title.unity`, `Overworld.unity` per the step-by-step in [SPEC.md §4.14](SPEC.md) (Creating Boot and Systems scenes + direct-entry support).
- `GameServices` locator with skeletons for every service: `SaveManager` ([§4.15](SPEC.md)), `SceneLoader` + `ScreenFader` ([§4.14](SPEC.md)), `GameStateMachine` ([§4.13](SPEC.md)), `FeedbackService`, `GameSession` ([§4.24](SPEC.md)), `AudioBus` stub ([§4.16](SPEC.md)).
- `HUDRoot` + `TransitionCanvas` canvases in `Systems.unity` ([§4.17](SPEC.md)).
- `EditorTestSettings` SO for direct-entry save mode ([§4.14](SPEC.md)).
- Build Settings: `Boot` at index 0, all non-level scenes registered ([§4.14](SPEC.md)).
- Target resolution 1280×720, Game view 16:9 ([§2](SPEC.md), [§4.17](SPEC.md)).
- Prefab + debug scene generator scaffolds ([§4.25](SPEC.md), [§4.26](SPEC.md)) — skeletons only; entity generators land in the phases that need them.

### Automated tests
- `AsmdefIsolationTest` — `SMW.Runtime` has no `UnityEditor` reference; `SMW.Tests` has the Test Assemblies flag.
- `PhysicsLayerMatrixTest` — key layer pairs on/off per §4.19.
- `InputActionMapTest` — every §4.1 action exists with both keyboard and gamepad bindings.
- `BuildSettingsIndexTest` — Boot at index 0; Systems/Title/Overworld present.
- `LevelDataValidatorTest` — unregistered `sceneRef` triggers the `OnValidate` warning.
- `BootstrapperLoadsSystemsTest` (PM) — any non-Boot scene auto-loads Systems additively.
- `GameServicesRegistrationTest` (PM) — all services present after Systems loads.
- `DirectEntryLevelTest` (PM) — opening a Level directly reaches `LevelState` within one frame.
- `SaveRoundTripTest` (PM) — empty `SaveData` write/read round-trips.
- `SceneLoaderFadeSequenceTest` (PM) — fade-out → `OnTransitionPeak` → fade-in, in that order.
- `PauseTimeScaleTest` (PM) — pause sets `Time.timeScale = 0` and `AudioListener.pause = true`; unpause reverses both.

### Manual verification
- Play-from-any-scene works per the [§4.14 matrix](SPEC.md).
- Playing from `Systems.unity` alone logs a helpful error.
- HUD letterboxes/pillarboxes at non-16:9 aspect ratios (no stretch, no clip).
- Pause toggle visibly freezes movement and audio; unpause resumes.

**Bugs to look for:**
- Systems loaded twice (duplicate `GameServices` in Hierarchy).
- Multiple `AudioListener`s active (Unity warning).
- `TransitionCanvas` sort-order below `HUDRoot` (fade invisible).
- `EnterDirectLevel` firing twice (Bootstrapper + `LevelRoot` both triggering).
- `FindAnyObjectByType` in Bootstrapper (order-dependent, flaky).

---

## Phase 1 — Player & physics

### Tasks
- `PlayerController` with dynamic zero-gravity Rigidbody2D, `GroundProbe`, manual gravity ([§4.2](SPEC.md)).
- Full required mechanics: variable jump + coyote + buffer, run gate, spin jump, skid, crouch, ceiling cancel, slope handling ([§4.2](SPEC.md)).
- `PlayerCarry` placeholder ([§4.2](SPEC.md)).
- `LevelCamera` with forward bias, vertical lock, `LevelBounds` clamp ([§4.4](SPEC.md)).
- Core Environment prefabs needed by the Movement Test: `Ground_Platform`, `Slope_Steep_L/R`, `Slope_Shallow_L/R` — with variable-`length` fields and the collider/visual regen editor hook ([§4.5](SPEC.md)). Remaining Environment prefabs (`OneWay_Platform`, `Pipe`, hazards) come in Phase 2.
- Phase 1 Movement Test debug scene — hand-authored in Unity per [§4.26](SPEC.md). Lives at `Assets/_Project/Scenes/Debug/MovementTest.unity`.

### Automated tests
- `JumpArcReproducibilityTest` (PM) — scripted input at 60Hz FixedUpdate lands at recorded position within ±0.01u. Canary for physics regressions.
- `CoyoteTimeEdgeTest` (PM) — jump at 6 frames post-ledge succeeds; 7 frames fails.
- `JumpBufferTest` (PM) — jump pressed 6 frames pre-landing fires on landing frame; 7+ doesn't.
- `VariableJumpHeightTest` (PM) — short tap vs. full hold produce jumps within bounds locked at end of phase.
- `CeilingCancelTest` (PM) — vertical velocity zeroed on ceiling contact, no rebound.
- `SlopeGroundedContinuityTest` (PM) — walking a 45° slope keeps `IsGrounded == true` every frame.
- `CameraForwardBiasTest` (PM) — sprinting right leads the player by the configured bias.
- `CameraVerticalLockTest` (PM) — isolated jump → camera Y unchanged. Grounded stair-climb → camera Y updates.
- `LevelBoundsClampTest` (PM) — camera clamps to bounds on out-of-bounds target.

### Manual verification
- Run + jump *feels* responsive — this is the subjective tuning gate; do not advance until dialed in.
- Short hop vs. full jump both feel good.
- Coyote (1–3 frames post-ledge) and buffer (just-before-landing) both trigger jumps.
- Skidding visual on high-speed reversals.
- Spin jump has lower arc + visible spin.
- Slopes: up slower, down faster, grounded throughout.
- Camera: leads on run, still on jump, clamps at bounds.

**Bugs to look for:**
- Coyote or buffer stacking into double-jumps.
- `IsGrounded` flicker on slope seams.
- Getting stuck in slope/flat platform junction corners.
- Jump feels "floaty" or "heavy" — gravity or hold-time tuning off.
- Subpixel camera stutter at 1280×720.
- Ceiling cancel firing on wall contact.

---

## Phase 2 — Environment prefabs, blocks, pickups, timer

### Tasks
- Remaining Environment prefabs (Phase 1 covered ground + slopes): `OneWay_Platform`, `Pipe`, `Hazard_Spikes`, `Hazard_Lava`, `Hazard_Pit` — same variable-length pattern as §4.5.
- Full block roster as standalone prefabs (`Block_Question`, `Block_Brick`, `Block_MultiCoin`, `Block_Note`, `Block_Rotating`, `Block_PSwitch`, `Block_SwitchPalace_Y/G/R/B`, `Block_Used`) authored directly into levels — no spawner ([§4.6](SPEC.md)).
- Coin, dragon coin, 1-up pickups + HUD counters via `HudViewModel` ([§4.9](SPEC.md), [§4.17](SPEC.md)).
- `LevelTimer` with low-time warning ([§4.21](SPEC.md)).
- `MidwayGate` checkpoints ([§4.5](SPEC.md), [§4.24](SPEC.md)).
- All Blocks debug scene ([§4.26](SPEC.md)).

### Automated tests
- `SlopePolygonColliderTest` (EM) — every slope prefab's `PolygonCollider2D` regenerates correctly for multiple `length` values (vertex positions match the angle formula).
- `EnvironmentPrefabLengthSyncTest` (EM) — changing the `length` field on a `Ground_Platform` / `Slope_*` instance resizes collider and visual in lockstep.
- `BrickBumpVsBreakTest` (PM) — Small bumps, Super breaks.
- `SpinJumpBreakRotatingBlockTest` (PM) — only spin-jump from above destroys.
- `QuestionBlockContentsTest` (PM) — `BlockContents` SO drives output; Small → mushroom even when SO says flower.
- `MultiCoinBrickTimerTest` (PM) — coin cap enforced; becomes used-block after window.
- `PSwitchGlobalSwapTest` (PM) — stomp swaps coins↔bricks globally; reverts on timer.
- `SwitchPalaceGateTest` (PM) — solid iff `SaveData.switchPalaces[color] == true`.
- `MidwayCheckpointPersistsTest` (PM) — cross → die → respawn at midway.
- `MidwayGrantsMushroomTest` (PM) — Small crossing auto-becomes Super.
- `CoinTo1UpTest` (PM) — 100 coins → lives +1.
- `DragonCoinFullSetTest` (PM) — all 5 → lives +1 + bitmask set.
- `LevelTimerCountdownTest` (PM) — timer → 0 fires death.
- `LowTimeWarningTest` (PM) — threshold crossing triggers SFX + music pitch-up.
- `HudViewModelBindingTest` (PM) — score change → subscribed HUD updates.

### Manual verification
- Open All Blocks debug scene; exercise every block from every angle (bump, stomp, spin-jump).
- P-switch: 10s global swap with music pitch-up; reverts cleanly.
- Switch palace blocks respect save flag.
- Coin, dragon coin, midway behaviors match §4.5 / §4.9.

**Bugs to look for:**
- Double-spawn on `?` block (bump + break both fire).
- P-switch timer completing while standing on a reverting brick (stuck inside geometry).
- P-switch misses sub-area objects.
- Multi-coin brick coin cap not enforced.
- Midway re-triggers on every cross.
- `HudViewModel` doesn't unsubscribe on scene unload (leak on reload).
- HUD display overflow past 999.

---

## Phase 3 — Power-ups & combat

### Tasks
- `PlayerStateMachine` Small/Super/Fire/Cape with full damage flow ([§4.3](SPEC.md)).
- Mushroom, Fire Flower, Cape Feather, Star pickups ([§4.9](SPEC.md), [§4.2](SPEC.md) star overlay).
- Fireball projectile ([§4.2](SPEC.md)).
- Cape sweep + slow-fall ([§4.2](SPEC.md)) — no flight, no dive, no P-meter.
- Death → `SceneLoader.ReloadLevelAsync` + respawn at checkpoint ([§4.14](SPEC.md), [§4.24](SPEC.md)).
- Lives counter, game over → Title.
- `PlayerCarry` wired for shells/P-switch/springs ([§4.1](SPEC.md) Action button).
- Power-up Transitions debug scene ([§4.26](SPEC.md)).

### Automated tests
- `PowerStateTransitionMatrixTest` (EM) — every diagram transition in §4.3 works.
- `DamageFlowNeverSkipsTest` (EM) — no Fire/Cape → Small path exists.
- `FireballMaxOnScreenTest` (PM) — spawn cap enforced.
- `FireballGroundBounceTest` (PM) — bounces on floor, dies on wall.
- `CapeSweepArcColliderTest` (PM) — arc ~0.25s + cooldown respected.
- `CapeSlowFallGravityTest` (PM) — 25% gravity only while descending + jump held.
- `StarInvincibilityOverlayTest` (PM) — overlay timer, power state unchanged, music stack push/pop.
- `DeathReloadCheckpointTest` (PM) — die after midway → respawn at midway.
- `DeathWithoutCheckpointTest` (PM) — die before midway → default spawn.
- `LevelRunStateWipedOnExitTest` (PM) — exit to overworld clears midway state.
- `LivesDecrementAndGameOverTest` (PM) — lives → 0 → Title.
- `PlayerCarryPickupDropTest` (PM) — hold/drop/throw behaviors.

### Manual verification
- Walk power-ups in sequence; visual + collider updates.
- Feather while Fire → direct Cape (no Super intermediate).
- Damage always routes through Super.
- Star: rainbow flash, music push, timer expires cleanly.
- Fire/Cape abilities work per spec.
- Death reload → correct respawn.

**Bugs to look for:**
- Fire → Small directly (damage flow broken).
- Fireball spam past cap.
- Fireball falls through slope prefabs.
- Cape sweep cooldown bypass via alternating inputs.
- Slow-fall active while rising, or as non-Cape Mario.
- Death reload loses session state (coin counter resets).
- Midway re-triggers on respawn.
- Level reload duplicates `GameServices`.

---

## Phase 4 — Enemies & combat

### Tasks
- Capability interfaces + shared helpers (`KinematicBody2D`, `EnemyDespawn`, `PeriodicEmitter`) ([§4.7](SPEC.md)).
- Six V1 enemy classes with their interface sets ([§4.7 V1 roster](SPEC.md)).
- `PlayerCollisionRouter` dispatching by contact direction ([§4.7 Combat via capability interfaces](SPEC.md)).
- `Fireball`, `CapeSweep`, `Shell` attacker-side dispatch via `TryGetComponent<I…>` ([§4.7](SPEC.md)).
- `StompCombo` chain scoring ([§4.7](SPEC.md)).
- Off-screen culling + re-spawn hysteresis ([§4.7](SPEC.md)).
- All Enemies debug scene ([§4.26](SPEC.md)).

### Automated tests
- `EnemyCapabilityDeclarationTest` (EM) — each V1 enemy implements the exact interface set per §4.7 and carries the `ContactDamage` / `SpinJumpSafe` components it's supposed to have.
- `GetActiveComponentFilterTest` (EM) — filters disabled sibling components correctly (Koopa walk↔shell).
- `StompFromAboveTest` (PM) — valid stomp kills + rebounds.
- `SideContactDamagesTest` (PM) — side contact reads the enemy's `ContactDamage` component and calls `player.TakeDamage`.
- `SpinJumpSafeBounceTest` (PM) — enemies with a `SpinJumpSafe` component bounce safely on spin-jump.
- `FireballReactionPathsTest` (PM) — `Absorbed` / `Passes` / wall-extinguish branches.
- `KoopaWalkToShellSwapTest` (PM) — sibling-component enable toggle dispatches correct interface.
- `ShellKickOnSideContactTest` (PM) — walk into stunned shell kicks it in facing direction.
- `ShellPickupTest` (PM) — Action press adjacent to shell parents to carry transform.
- `ShellThrownKillsEnemiesTest` (PM) — `IShellImpact` kills on contact.
- `StompComboChainTest` (PM) — 8 stomps without landing → 1-up + escalating points.
- `StompComboResetOnLandTest` (PM) — landing resets chain.
- `EnemyOffScreenDespawnTest` (PM) — past frustum + hysteresis → destroyed.
- `BulletBillLauncherHysteresisTest` (PM) — re-emit only past hysteresis margin.
- `BooConditionallyTangibleTest` (PM) — `IsTangibleTo` gates collision.
- `ChuckMultiStompTest` (PM) — 3 stomps to kill.

### Manual verification
- All Enemies debug scene: exercise each enemy from every relevant attack angle.
- Koopa walk↔shell visual transition is clean (no double-sprite frame).
- Stomp combo: 8-chain → 1-up popup.
- Boo face-toward freezes; face-away chases.
- Chuck visible state change between stomps.
- Off-screen despawn + launcher re-emit both work without visual artifacts.

**Bugs to look for:**
- Disabled Koopa walker still returned by `GetComponent<IStompable>` (didn't filter `.enabled`).
- Double-dispatch same frame (stomp + fireball).
- Fireball absorbed vs. extinguished confusion on Boo.
- Stomp combo reset on coyote-time grounded-flicker frame.
- Shell through wall with no reflection.
- Shell hits its own kicker before grace window.
- Despawn inside camera view (hysteresis too tight).
- Repeated spawn on camera pan over launcher (hysteresis not applied to re-spawn).
- Boo tangibility flicker at perpendicular angles.
- `PlayerCollisionRouter` misses multi-collider enemies.
- `StompCombo.Notch` fires from non-stomp kills.

---

## Phase 5 — Yoshi

### Tasks
- `RideableMount` + `YoshiController` + saddle parenting ([§4.8](SPEC.md)).
- Tongue, swallow, dismount-on-damage, panic + recovery ([§4.8](SPEC.md)).
- Green Yoshi only; `YoshiData` plumbing for future colors ([§4.8](SPEC.md)).
- Yoshi Test debug scene ([§4.26](SPEC.md)).

### Automated tests
- `YoshiHatchAndMountTest` (PM) — proximity hatch + mount parents to saddle.
- `MountSuspendsPlayerPhysicsTest` (PM) — `PlayerController.FixedUpdate` doesn't write velocity while mounted.
- `YoshiTongueBoxCastTest` (PM) — Action press spawns tongue; grabs first eligible target.
- `YoshiSwallowEnemyTest` (PM) — second Action destroys target + awards points.
- `YoshiBerryEatTest` (PM) — berry destroyed + points + egg-drop counter.
- `DismountOnDamageTest` (PM) — damage while mounted → forced dismount, no power-state loss, panic starts.
- `PanicRecoveryRemountTest` (PM) — touching during panic → remount.
- `PanicTimerExpiresTest` (PM) — panic ~3s → Yoshi lost.
- `PanicCrossBoundsLostTest` (PM) — Yoshi crosses `LevelBounds` → lost.
- `ExtendedJumpTest` (PM) — jump at apex → second jump with higher peak.
- `GreenYoshiFireballSwallowTest` (PM) — Green + colored shell → spit-fireball.

### Manual verification
- Hatch, mount, ride, tongue, swallow all feel clean.
- Extended jump timing works (second jump at apex).
- Damage dismount preserves power state.
- Recovery window remounts cleanly; expiry loses Yoshi.

**Bugs to look for:**
- Power-state lost on mount.
- Tongue grabs blocks / tiles.
- Swallow fires with empty mouth.
- Dismount launches Mario through floor.
- Panic timer persists across reloads.
- Extended jump fires after landing.
- Both Yoshi and Mario physics steps running.
- `GroundProbe` active on Mario while mounted.

---

## Phase 6 — Goal, sub-areas, overworld

### Tasks
- `GoalGate` + `KeyHole` + carryable `Key` prefab (standalone `IThrowable`, under `Prefabs/Environment/`) ([§4.11](SPEC.md)).
- Pipe entry → offset-region sub-area ([§4.5](SPEC.md)).
- `Overworld.unity` with `MapNode` graph, BFS, file select ([§4.12](SPEC.md), [§4.15](SPEC.md)).
- Save at milestones only ([§4.15](SPEC.md), [§4.24](SPEC.md)).

### Automated tests
- `GoalGateScoringTest` (PM) — high/mid/low → 8000/4000/2000 + timer bonus.
- `GoalGatePayloadTest` (PM) — `LevelClearPayload` fields correct.
- `KeyHoleSecretExitTest` (PM) — carrying Key into KeyHole sets `secretExit: true`.
- `PipeEntryAndReturnTest` (PM) — smooth tween + sub-area snap + return.
- `SubAreaLevelBoundsTest` (PM) — camera obeys sub-area bounds.
- `OverworldMapNodeBfsTest` (PM) — BFS respects allowed-direction graph.
- `OverworldBranchUnlockTest` (PM) — secret exit unlocks `unlocksOnSecretExit` nodes.
- `SaveAtMilestonesTest` (PM) — `SaveManager.Save` fires only at level clear / overworld move / switch palace / file-select. Never mid-level.
- `SaveSlotIsolationTest` (PM) — 3 slots fully independent.
- `DragonCoinBitmaskPersistsTest` (PM) — partial-collection bitmask survives level completion.

### Manual verification
- Goal bar scoring feels right.
- Secret exit via Key → alt branch unlocks.
- Pipe entry → sub-area → return is visually smooth.
- Overworld pathing + branching works.
- File select: 3 slots independent, last-played metadata correct.

**Bugs to look for:**
- Sub-area return spawns in wrong region.
- Pipe tween twitches mid-animation.
- Secret exit flags both branches.
- Dragon-coin bitmask overwritten on later completions with fewer coins.
- Save fires mid-level.
- BFS finds paths through non-connected nodes.
- `SaveIndex.json` corrupted on concurrent slot writes.
- Goal bar rotation speed drifts per level.

---

## Phase 7 — Content & polish

### Tasks
- 5 hand-authored content levels + 1 boss room ([§5](SPEC.md), [§4.26](SPEC.md) content-level side).
- Title screen, file select, pause menu UI passes ([§4.17](SPEC.md)).
- Playtest tuning: SVG colors, camera curves, jump constants, audio mixer ([§4.2](SPEC.md), [§4.4](SPEC.md), [§4.16](SPEC.md), [§4.18](SPEC.md)).

### Automated tests
- `ContentLevelsInBuildSettingsTest` (EM) — every `LevelData.sceneRef` is registered.
- `LevelDataCompletenessTest` (EM) — time limit, music, default entry, unlocks all populated.
- `PrefabReferenceIntegrityTest` (EM) — no broken prefab/script references in level scenes.
- `DragonCoinCountTest` (EM) — each non-boss level has exactly 5 dragon coins placed.
- `BossRoomStompKillTest` (PM) — placeholder Bowser takes 3 stomps to defeat.

### Manual verification
- Playtest each level end-to-end — subjective "feels like SMW" gate.
- All 5 dragon coins per level reachable without exploits.
- Level 5 secret exit unlocks boss room branch.
- Title / file select / pause render correctly at 16:9 and letterboxed aspect ratios.
- Audio mixer balanced; pause SFX plays while paused (`UiSfxChannel`).

**Bugs to look for:**
- Unreachable dragon coin.
- Level 5 secret path blocked by physics seam.
- Title/file-select unresponsive on gamepad (EventSystem selection).
- Pause menu background bleed-through.
- Music crossfades overlap between levels (missed pop).
- `LevelData.unlocksOnNormalExit` targets nonexistent level.
- Boss stomp count off-by-one.

---

## Phase 8 — Hardening

Phase 8 consolidates per-phase tests into a regression suite and adds cross-cutting validators. Most of its value is the earlier phases' tests running green together.

### Tasks
- Consolidated test suite — all earlier-phase tests pass together.
- Asset validation postprocessor ([§4.26](SPEC.md) test fixtures note; also see CLAUDE.md coding conventions for SO discipline).
- Unity Profiler pass — verify no per-frame allocations in `FixedUpdate`.

### Automated tests
- `AssetValidationPostprocessorTest` (EM) — missing references on any SO (`EnemyData.sprite`, `PowerStateData.collider`, `LevelData.sceneRef`) fail asset import with a clear error.
- `FixedUpdateAllocationFreeTest` (PM) — 60 FixedUpdate ticks on Phase 1 scene allocate 0 managed bytes (or below tight tolerance).
- `LongSoakTest` (PM) — 10-minute fast-forward on a content level: no frame budget overruns, no leaking audio sources or enemy instances.
- `SceneUnloadEventCleanupTest` (PM) — level load → unload → load; subscriber counts on `HudViewModel` don't grow.

### Manual verification
- Profiler: 60s per debug scene + content level 1 — no GC spikes, no per-frame allocations.
- Stress: many-enemy combo scene holds 60 FPS.
- Long soak: 10+ min idle with pause/unpause cycles + overworld round-trips — no state corruption.
- Rapid save/load: immediate replay + save at next milestone preserves integrity.
- Window resize during play — HUD re-layouts without crashing.
- Alt-tab and return — no stuck state.
- Standalone Windows build — same manual checks pass; no debug scenes in shipped build list.

**Bugs to look for:**
- PrimeTween callbacks not torn down on scene unload (memory leak).
- `AudioSource` count growing across transitions.
- Event subscribers surviving scene unload.
- Save file corrupted by interrupted write (kill process mid-save, verify recovery).
- `StripDebugScenesOnBuild` mis-filters a content scene.
- CI-flaky tests — usually `FindAnyObjectByType` in a hot path or missing `[DefaultExecutionOrder]`.
- Hidden allocations (enumerator boxing on dictionary iteration, closures in Update).
