# TASKS.md — Implementation Phases

This file tracks the build order for the project. Each phase ends with a runnable build that demos the new system. **Do not skip phases** — each one validates the architecture against real gameplay. See [SPEC.md](SPEC.md) for architectural context referenced by the bullets below (§ numbers point into SPEC.md).

Each phase has three parts:
- **Tasks** — the concrete work to do, in rough order.
- **Automated tests** — EditMode (no scene load) or PlayMode (scene-driven) tests added to the `SMW.Tests` asmdef. Each test listed here is a specific assertion, not a "test everything" goal.
- **Manual verification** — editor checklist plus a "bugs to look for" list. The manual pass is the *acceptance* gate; tests are regression insurance against future changes.

Phase 0 is the one exception where the automated-test surface is small — foundation scaffolding is mostly structural. Tests grow substantially from Phase 1 onward.

---

## Phase 0 — Project foundation

### Tasks
- Create assembly definitions: `SMW.Runtime`, `SMW.Editor`, `SMW.Tests` (the Tests asmdef references `UnityEngine.TestRunner` + `UnityEditor.TestRunner`).
- Define the **physics layers + 2D collision matrix** (§4.19). Commit `ProjectSettings/DynamicsManager.asset`.
- Define the **input action maps** in the existing `InputSystem_Actions.inputactions` (§4.1) and write `InputRouter`.
- Bootstrap scene + persistent `Systems` scene + `GameServices` locator.
- `Bootstrapper.EnsureSystemsLoaded` static method with `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` per §4.14 — additively loads `Systems.unity` before any other scene's `Awake` runs, in both builds and editor.
- `GameStateMachine` skeleton with empty Title/Overworld/Level/Paused states, including the editor-only `EnterDirectLevel(LevelData, entryPoint)` helper (§4.14) so pressing Play on any Level scene works end-to-end.
- `TitleRoot`, `OverworldRoot`, and `LevelRoot` MonoBehaviours with direct-entry detection in `Awake` (§4.14). Hit-Play-from-any-scene is a Phase 0 requirement, not a later polish pass — every subsequent phase depends on being able to iterate on a single scene without navigating the full flow.
- `EditorTestSettings` ScriptableObject at `Assets/_Project/Settings/EditorTestSettings.asset` with a `DirectEntrySaveMode` enum field (`FreshDefaults` | `Slot1`) defaulting to `FreshDefaults`.
- Add `Boot`, `Systems`, `Title`, `Overworld` scenes to Build Settings with `Boot` at index 0. Add a `LevelData.OnValidate()` check that warns when a referenced scene isn't in Build Settings.
- `SceneLoader` service + `ScreenFader` MonoBehaviour (on `TransitionCanvas` in `Systems.unity`) per the split in §4.14. Uses `SceneReference` for type-safe scene fields; `ScreenFader` uses PrimeTween with unscaled time.
- `AudioBus` stub (logs only) + empty `AudioCatalog` SO + `AudioMixer` asset wired up.
- `Palette` SO with placeholder colors for every `PaletteRole` we know we'll need.
- `PaletteBinding` MonoBehaviour wired up. SVG asset folder at `Assets/_Project/Art/Procedural/` (one smoke-test SVG to validate the import pipeline).
- Set the target resolution to **1280×720** in Player Settings (default resolution + fullscreen mode) and the Game view aspect to *16:9*. Commit `ProjectSettings/ProjectSettings.asset`.
- uGUI `HUDRoot` canvas in the `Systems` scene with `CanvasScalerPresetApplier` (reference 1280×720, `MatchWidthOrHeight`, `Match = 0.5`) and empty `HudPanel` + `PauseMenuPanel` placeholders. Pause flow with `Time.timeScale` toggle. Uses `InputSystemUIInputModule` for gamepad navigation.
- `LevelCamera` prefab with `orthographicSize = 7` so the camera shows ~14 tiles vertically at 16:9 (§4.4).
- `SaveManager` round-trips an empty `SaveData` to `save_1.json`.
- `ScoreService`, `FeedbackService`, and `GameSession` skeletons registered on `GameServices` (§4.24).
- **Content authoring pipeline scaffolding** (§4.25): folder structure, empty SO class shells, `PrefabGeneratorBase` with two-menu-item plumbing, one trivial concrete generator as smoke test.
- **Debug scene infrastructure** (§4.26): `DebugSceneGenerator` skeleton, `SceneBuildHelpers` static, shared `LevelData_Debug.asset`, `StripDebugScenesOnBuild` build preprocessor.

### Automated tests
- **EditMode `AsmdefIsolationTest`** — assert `SMW.Runtime.asmdef` does not reference `UnityEditor` or any editor-only assembly; assert `SMW.Tests.asmdef` has `Test Assemblies` flag and references both test runner modules.
- **EditMode `PhysicsLayerMatrixTest`** — assert each expected layer pair is on/off per §4.19 (e.g., `Player ↔ EnemyDamage` on, `Enemy ↔ Enemy` off, `Player ↔ PlayerInvulnerable` off).
- **EditMode `InputActionMapTest`** — parse the `.inputactions` asset; assert every action named in §4.1 exists (`Move`, `Jump`, `Action`, `SpinJump`, `Pause`) with at least one keyboard and one gamepad binding.
- **EditMode `BuildSettingsIndexTest`** — `Boot.unity` is at index 0; `Systems.unity`, `Title.unity`, `Overworld.unity` are all present.
- **EditMode `LevelDataValidatorTest`** — a `LevelData` with an unregistered `sceneRef` produces a warning from `OnValidate`.
- **PlayMode `BootstrapperLoadsSystemsTest`** — load any non-Boot scene directly; after one frame, `SceneManager.GetSceneByName("Systems").isLoaded` is true and `GameServices.Instance` is non-null.
- **PlayMode `GameServicesRegistrationTest`** — from a fresh Systems load, every expected service is registered: `SaveManager`, `SceneLoader`, `ScreenFader`, `GameStateMachine`, `ScoreService`, `FeedbackService`, `AudioBus`.
- **PlayMode `DirectEntryLevelTest`** — open a minimal test Level scene directly; assert `GameStateMachine.Current` is `LevelState` within one frame (via `EnterDirectLevel`).
- **PlayMode `SaveRoundTripTest`** — write an empty `SaveData`, read it back, assert field-by-field equality. Delete the written file in tear-down.
- **PlayMode `SceneLoaderFadeSequenceTest`** — subscribe to `OnTransitionPeak`; call `LoadAsync(targetScene)`; assert fader alpha went 0→1, then peak event fired, then alpha 1→0.
- **PlayMode `PauseTimeScaleTest`** — push `PausedState`; assert `Time.timeScale == 0`, `AudioListener.pause == true`. Pop; assert both reverse.

### Manual verification
- Pressing Play from `Boot.unity` → Title placeholder appears (Phase 0 Title is just a Canvas with one TMP label; real Title is Phase 7).
- Pressing Play from `Title.unity` alone → Systems loads additively (check Hierarchy for both scenes), Title placeholder renders.
- Pressing Play from `Overworld.unity` alone → Systems loads additively, empty Overworld placeholder.
- Pressing Play from a dummy Level scene → `LevelRoot.Awake` logs "direct entry detected"; HUD canvas renders; pause toggle works.
- Pressing Play from `Systems.unity` alone → console error "don't Play from Systems" (not a crash, a helpful message).
- Resize Game view to 4:3 or ultra-wide → HUD letterboxes/pillarboxes (does NOT clip or stretch).
- Pause during play → `Time.timeScale = 0` freezes movement; pause menu panel shows; UI nav works with gamepad.
- Unpause → time resumes.
- **Bugs to look for:**
  - Systems scene loaded twice (check Hierarchy for duplicated `GameServices` — symptom: second-load `Awake` overwrites first).
  - Multiple `AudioListener`s active (Unity warning in console).
  - `TransitionCanvas` sorting order below `HUDRoot` (fade appears *under* the HUD — can't see it work).
  - `PaletteBinding` runs before palette SO is populated (enemies tint to default gray).
  - `EnterDirectLevel` called twice (once from `RuntimeInitializeOnLoad`, once from `LevelRoot.Awake`) — level transitions fire twice.
  - `FindAnyObjectByType` accidentally used somewhere in Bootstrapper (order-dependent, flaky).

---

## Phase 1 — Player & physics

### Tasks
- `PlayerController` with dynamic Rigidbody2D (`gravityScale = 0`), `GroundProbe`, manual gravity.
- Walk, run, jump (variable height + coyote + buffer), spin jump, skid, crouch, ceiling cancel.
- `PlayerCarry` placeholder (no actual carryables yet).
- `LevelCamera` with forward bias, vertical lock, and `LevelBounds` clamping.
- **Phase 1 Movement Test** debug scene, produced by `DebugSceneGenerator` (§4.26): flat tilemap with a few stepped platforms and one of each slope variant. No enemies. **Tune until it feels right** before moving on. This is the most important tuning gate in the project.

### Automated tests
- **PlayMode `JumpArcReproducibilityTest`** — scripted input (hold Jump for N FixedUpdate ticks at fixed start position); landing position matches a recorded reference within ±0.01 world units. Run at 60Hz FixedUpdate. This is the canary for physics regressions.
- **PlayMode `CoyoteTimeEdgeTest`** — walk off a ledge; pressing Jump within 6 FixedUpdate frames after leaving ground produces a jump; at 7 frames, does not.
- **PlayMode `JumpBufferTest`** — press Jump 6 frames before landing → jump fires on landing frame; pressing 7+ frames before → no buffered jump.
- **PlayMode `VariableJumpHeightTest`** — short tap vs. full 18-frame hold produces jumps within expected low/high bounds (values TBD after Phase 1 tuning; lock in the bounds at end of phase).
- **PlayMode `CeilingCancelTest`** — jump into overhead block; `rb.linearVelocity.y` zeroed on first ceiling-contact frame, no rebound.
- **PlayMode `SlopeGroundedContinuityTest`** — walk down a 45° slope for 30 FixedUpdate frames; `GroundProbe.IsGrounded` is true every frame (no single-frame flicker to airborne).
- **PlayMode `SpinJumpInputTest`** — SpinJump input triggers a spin-jump state (lower arc, spin flag set on next stomp dispatch — tested here via state assertion, combat tested in Phase 4).
- **PlayMode `CameraForwardBiasTest`** — run right at full sprint for 2 seconds; camera X leads player X by the configured bias amount (not centered on player).
- **PlayMode `CameraVerticalLockTest`** — jump while standing; camera Y unchanged. Walk up stairs (grounded the whole time); camera Y updates.
- **PlayMode `LevelBoundsClampTest`** — force camera target past `LevelBounds`; camera X clamps to bound edge.

### Manual verification
- Open the Phase 1 Movement Test debug scene.
- Run + jump feels responsive — hold this to a high bar, this is the subjective tuning gate.
- Short hop (single tap) and full jump (hold) both feel good; not one "correct" height.
- Walking off a ledge and pressing Jump 1-3 frames later still jumps (coyote).
- Pressing Jump just before landing still queues the jump (buffer).
- Skidding visual plays when reversing direction at high speed.
- Spin-jump has a noticeably lower arc and a visible spin animation (even if placeholder).
- Crouching works — try as Small (shouldn't work — Small Mario can't crouch, but in Phase 1 we only have Small so test after Phase 3).
- Slope walking: up slope slower than flat; down slope faster; grounded the whole time.
- Ceiling hit: velocity zeroed, no "boing" rebound.
- Camera: leads Mario when running; doesn't move vertically during isolated jumps; respects level bounds.
- **Bugs to look for:**
  - "Sticky" jumps — coyote or buffer accidentally stacks, producing unexpected double-jumps.
  - Grounded flicker on slope transitions (`IsGrounded` toggles for a single frame, breaks jump/coyote state).
  - Getting stuck in the seam between a slope tile and an adjacent flat tile (slope physics shape + composite collider corner).
  - Jump feels "floaty" or "heavy" — may indicate gravity or hold-time is wrong.
  - Camera stutters on subpixel Mario positions at 1280×720 (SVG meshes shouldn't cause this; check PPU and pixel-perfect settings).
  - `LevelBounds` not enforced — camera shows gray outside level.
  - Ceiling cancel applied during side wall contact (mario hit wall, not ceiling).

---

## Phase 2 — Tiles, blocks, pickups, timer

### Tasks
- Custom `TileBase` types (`SolidTile`, `OneWayTile`, `SlopeTile`, `PipeTile`) with proper sprite-collider physics shapes for slopes.
- `InteractiveBlockSpawner` and the full block roster (`?`, brick, multi-coin brick, note, used, P-switch, rotating, switch palace).
- Coin, dragon coin, 1-up mushroom pickups + HUD counters via `HudViewModel`.
- `LevelTimer` countdown + low-time warning.
- `MidwayGate` checkpoints.

### Automated tests
- **EditMode `SlopeTilePhysicsShapeTest`** — for every `SlopeTile` sprite asset, assert `Sprite.GetPhysicsShapeCount() > 0` and all returned points form a triangle (3 vertices).
- **EditMode `InteractiveTileMarkerTest`** — paint interactive markers on a test tilemap; run `InteractiveBlockSpawner`; assert marker tiles are cleared and the correct prefabs are spawned at matching world positions.
- **PlayMode `BrickBumpVsBreakTest`** — Small Mario bumps brick from below → brick plays bump tween, stays intact. Super Mario bumps → brick shatters and is destroyed.
- **PlayMode `SpinJumpBreakRotatingBlockTest`** — spin-jump onto a rotating block from above → block destroyed. Normal stomp → block spins but survives.
- **PlayMode `QuestionBlockContentsTest`** — `?` block with `BlockContents = Coin` → releases coin, becomes used-block. `BlockContents = PowerUp` while Small → mushroom. Same while Super → flower/feather per SO.
- **PlayMode `MultiCoinBrickTimerTest`** — bump brick repeatedly within configured window → releases multiple coins up to cap (10). After window expires → becomes used-block.
- **PlayMode `PSwitchGlobalSwapTest`** — stomp a P-switch; all `Coin` tiles/objects become `Brick`, all `Brick` become `Coin`. After timer, reverts. Assert a known coin's position is now brick mid-timer, and coin again after.
- **PlayMode `SwitchPalaceGateTest`** — a colored switch-palace block is pass-through when `SaveData.switchPalaces[color] == false` and solid when true.
- **PlayMode `MidwayCheckpointPersistsTest`** — cross midway → die → level reloads → Mario spawns at the midway SpawnMarker, not default. Assert `LevelRunState.checkpointId` survives the reload via `GameSession`.
- **PlayMode `MidwayGrantsMushroomTest`** — cross midway while Small → power-up state is now Super.
- **PlayMode `CoinTo1UpTest`** — add 100 coins via `ScoreService` → lives counter +1, 1-up jingle fired.
- **PlayMode `DragonCoinFullSetTest`** — collect 5 dragon coins in a level → lives counter +1; `LevelRunState.dragonCoins` bitmask set.
- **PlayMode `LevelTimerCountdownTest`** — timer reaches 0 → death event fires.
- **PlayMode `LowTimeWarningTest`** — timer crosses the low-time threshold → `AudioBus` plays low-time SFX; music pitch increases.
- **PlayMode `HudViewModelBindingTest`** — `ScoreService.Add(100)` → `HudViewModel.OnCoinsChanged` fires; subscribed `HudPanel` text updates.

### Manual verification
- Open the All Blocks debug scene.
- Bump each block from below, stomp from above where applicable, spin-jump where applicable.
- `?` block: Small → mushroom; Super → flower or feather per `BlockContents` SO.
- Brick: Small bumps safely; Super breaks into shards; spin-jump from above also breaks.
- Multi-coin brick: bump rapidly → multiple coins spawn; eventually becomes used.
- Note block: bounces Mario with variable height based on jump-held.
- Rotating block: spins on hit; spin-jump from above destroys.
- P-switch: stomp → ~10s global timer → music pitch-up → every coin becomes brick, every brick becomes coin. Reverts when timer expires.
- Switch palace blocks: dotted outline until save flag flips, then become solid.
- Coins: increment HUD counter; 100 coins → 1-up.
- Dragon coins: separate HUD row; all 5 → 1-up + persistent bitmask.
- Midway gate: cross it, die into a pit, respawn at the midway. Cross while Small → grow to Super.
- Timer: HUD counts down; low-time warning fires.
- **Bugs to look for:**
  - Double-spawn of pickups when `?` block is hit by a spin-jump (both the bump and the break paths fire).
  - P-switch timer completes mid-swap — coins revert but Mario is standing on a reverting brick (stuck inside geometry).
  - P-switch swap misses objects in a sub-area (only swaps the active region).
  - Multi-coin brick coin cap not enforced (infinite coins).
  - Midway gate re-triggers on every pass-through (should fire once per attempt).
  - Switch palace blocks solid even before activation (save state read incorrectly on `Awake`).
  - `HudViewModel` subscribes but doesn't unsubscribe on scene unload (memory leak on level reload).
  - Coin counter increments past 999 wrong (HUD display overflow).

---

## Phase 3 — Power-ups & combat

### Tasks
- `PlayerStateMachine` (Small/Super/Fire/Cape) with full damage flow per §4.3.
- Mushroom, Fire Flower, Cape Feather, Star pickups (Star as `PlayerController` overlay timer, not a state).
- Fireball projectile (bounces along ground, dies on enemy/wall).
- Cape abilities per the simplified spec in §4.2: ground sweep (arc collider with cooldown) and airborne slow-fall (jump-held = 25% gravity while descending). No flight take-off, no dive, no P-meter.
- Death (fall pit / hit while small / time over) → level scene reload via `SceneLoader.ReloadLevel()` → respawn at last checkpoint per §4.24, with `LevelRunState.checkpointId` carried across via `GameSession`. Lives counter, game over → return to title.
- `PlayerCarry` wired up so future shells/P-switches/springs can be carried (uses the `Action` button per §4.1).

### Automated tests
- **EditMode `PowerStateTransitionMatrixTest`** — systematically exercise every transition in the §4.3 diagram. Small + Mushroom → Super. Super + Flower → Fire. Super + Feather → Cape. Fire + Feather → Cape (ability swap). Cape + Flower → Fire. Cape + damage → Super. Super + damage → Small. Small + damage → Death.
- **EditMode `DamageFlowNeverSkipsTest`** — assert there is NO path from Fire/Cape directly to Small. Apply damage repeatedly; enumerate state sequence; assert it's always Fire/Cape → Super → Small.
- **PlayMode `FireballMaxOnScreenTest`** — spawn fireballs via Action presses beyond `maxFireballsOnScreen` (e.g., 2); count active fireballs; assert cap enforced.
- **PlayMode `FireballGroundBounceTest`** — spawn fireball, let it fall on a flat tile; it bounces upward with reduced height; dies on wall contact.
- **PlayMode `CapeSweepArcColliderTest`** — press Action as grounded Cape Mario → arc collider spawns for ~0.25s then despawns. Second press within cooldown → no new arc.
- **PlayMode `CapeSlowFallGravityTest`** — as Cape Mario, jump, start descending, hold Jump → `PlayerController.currentGravityScale` is 25% of normal. Release Jump → restored. Rising phase ignores the check.
- **PlayMode `StarInvincibilityOverlayTest`** — pick up Star → `StarTimer` active, player state unchanged (Fire Mario stays Fire). Touching an enemy kills it (tested in Phase 4 integration). Timer ticks down; expires → overlay clears.
- **PlayMode `DeathReloadCheckpointTest`** — cross midway, die in pit → level scene reloads via `SceneLoader.ReloadLevel()`, Mario at midway spawn marker.
- **PlayMode `DeathWithoutCheckpointTest`** — die without crossing midway → respawn at default spawn marker.
- **PlayMode `LevelRunStateWipedOnExitTest`** — cross midway, exit to overworld → reload the level → spawn at default (not midway) because `LevelRunState` is per-attempt and wiped on exit.
- **PlayMode `LivesDecrementAndGameOverTest`** — die with `lives > 1` → lives decrements, level reloads. Die with `lives == 1` → game over → transition to Title.
- **PlayMode `PlayerCarryPickupDropTest`** — spawn a dummy `IThrowable` near Mario, press Action → object parented to carry transform, collider disabled on object. Release Action → drop. Tap Action while moving → throw with horizontal velocity.

### Manual verification
- Open the Power-up Transitions debug scene.
- Walk Small → Mushroom → Super. Collider/sprite updates.
- Super → Flower → Fire (red palette). Super → Feather → Cape.
- Fire → Feather → Cape (ability swap, not a separate Super intermediate).
- Cape → damage → Super. Super → damage → Small. Small → damage → Death.
- Pick up Star → invincibility music pushes on stack, Mario flashes rainbow; timer expires → music stack pops.
- Fire Mario: Action throws fireball; max 2 on screen; bounces; dies on wall and enemy.
- Cape Mario grounded: Action triggers arc sweep — enemies in arc get knocked back, nearby bricks break.
- Cape Mario airborne + descending + hold Jump: slow descent (~25% gravity). Release Jump → normal fall.
- Fall into pit → fade → reload → respawn at midway (if crossed) or default.
- Die while Small → same death flow.
- Reach 0 lives → game over screen → back to title.
- `PlayerCarry` placeholder: hold Action near a dummy carryable → hold animation; release → drop; tap → throw.
- **Bugs to look for:**
  - Fire → Small directly (damage flow broken; should always route through Super).
  - Fireball count cap not enforced (Action spam floods screen).
  - Fireball falls through ground on slopes.
  - Cape sweep cooldown bypassed by alternating Action with Jump.
  - Slow-fall active during upward part of jump (feels like a cape-float cheat).
  - Slow-fall active as non-Cape Mario (check `PowerStateData.can-slow-fall` flag).
  - Death reload loses some state (e.g., coin counter resets to save-time, not session).
  - Midway gate re-fires on respawn (because respawning Mario crosses it again).
  - Power-up pickup grants Fire/Cape while Small (should be Mushroom → Super first).
  - Level reload duplicates `GameServices` (Bootstrapper runs twice).

---

## Phase 4 — Enemies & combat

### Tasks
- Capability interfaces (`IStompable`, `IBumpable`, `IFireballHit`, `ICapeSweepHit`, `IShellImpact`, `IThrowable`, `IContactDamage`, `ISpinJumpSafe`, `IRideable`, `IConditionallyTangible`) + `KinematicBody2D` / `EnemyDespawn` / `PeriodicEmitter` shared helpers.
- Six V1 enemy classes (§4.7), each implementing the relevant interfaces.
- `PlayerCollisionRouter` on the player prefab — classifies contact direction (stomp vs. side), dispatches to interface methods, applies rebound + notches `StompCombo`.
- `Fireball`, `CapeSweep`, `Shell` all dispatch via `TryGetComponent<I…>` — no central resolver.
- `StompCombo` chain scoring.
- Off-screen culling and re-spawn hysteresis.

### Automated tests
- **EditMode `EnemyCapabilityDeclarationTest`** — for each V1 enemy prefab, assert it implements the exact interface set declared in §4.7 roster. Catches regressions where a refactor drops an interface.
- **EditMode `GetActiveComponentFilterTest`** — a GameObject with two `IStompable` sibling components (Walker enabled, Shell disabled) — `GetActiveComponent<IStompable>` returns the walker. Enable shell instead → returns shell.
- **PlayMode `StompFromAboveTest`** — drop player onto Goomba from above with downward velocity → `Goomba.OnStomped` called, Goomba destroyed, player rebounds upward.
- **PlayMode `SideContactDamagesTest`** — player walks into Goomba horizontally → `IContactDamage` path fires, player state degrades one step.
- **PlayMode `SpinJumpSafeBounceTest`** — spin-jump onto Spiny (when added; for V1 substitute Koopa's spiked variant if any) → player rebounds safely, Spiny unchanged.
- **PlayMode `FireballReactionPathsTest`** — fireball vs. Goomba → `FireballReaction.Absorbed`, fireball extinguishes, Goomba dies. Fireball vs. Boo → `FireballReaction.Passes`, fireball keeps flying (if Boo implemented as Passes) OR fireball extinguishes on Boo collider (if Boo doesn't implement `IFireballHit`). Per §4.7, Boo doesn't implement `IFireballHit` — so the fireball extinguishes on side collision like hitting a wall; test this.
- **PlayMode `KoopaWalkToShellSwapTest`** — stomp KoopaWalker → `KoopaWalker.enabled = false`, `KoopaShell.enabled = true`; `GetActiveComponent<IStompable>` now returns null, `GetComponent<IBumpable>` returns the shell.
- **PlayMode `ShellKickOnSideContactTest`** — walk into a stunned KoopaShell → shell's `OnTouchedSideways` equivalent (via `IShellImpact` / velocity apply) gives shell a horizontal velocity in the player's facing direction.
- **PlayMode `ShellPickupTest`** — with `PlayerCarry` from Phase 3, Action press adjacent to stunned shell → shell parented to carry transform.
- **PlayMode `ShellThrownKillsEnemiesTest`** — throw shell; shell collides with Goomba → Goomba dies via `IShellImpact.OnHitByShell`.
- **PlayMode `StompComboChainTest`** — 8 consecutive stomps without landing (bounce from one enemy to another) → lives +1; `StompCombo.Notch()` awards escalating points.
- **PlayMode `StompComboResetOnLandTest`** — land on ground mid-chain → next stomp starts combo back at 200.
- **PlayMode `EnemyOffScreenDespawnTest`** — move enemy past camera frustum + hysteresis → GameObject destroyed.
- **PlayMode `BulletBillLauncherHysteresisTest`** — launcher position moves off-camera → culled; moves back into camera + hysteresis margin → re-emits Bullet Bill.
- **PlayMode `BooConditionallyTangibleTest`** — Mario faces away from Boo → collision fires and damages. Mario faces toward Boo → `IsTangibleTo` returns false, collision aborted.
- **PlayMode `ChuckMultiStompTest`** — stomp Chuck 3 times → dies. First 2 stomps leave it alive (and possibly angry/faster); 3rd kills.

### Manual verification
- Open the All Enemies debug scene.
- Walk into each enemy from the side → damage per spec.
- Stomp each stompable enemy → kill + rebound.
- Spin-jump onto enemies → where `ISpinJumpSafe` is set, bounces safely; elsewhere, same as normal stomp.
- Fireball each enemy → extinguish/pass/kill as per their interface set.
- Cape sweep each enemy → knockback / kill / pass as applicable.
- Koopa: stomp → walker disappears, shell appears on the same GameObject (no respawn glitch). Kick shell; it slides. Pick up shell; throw it; it kills enemies.
- Chuck: 3 stomps to kill; behavior visibly changes between stomps (SMW has Chuck get angry).
- Boo: facing away, it approaches; facing toward, it freezes and becomes intangible (fireballs extinguish on collision, cape still kills).
- Stomp combo: bounce between 8 enemies without landing → 1-up.
- Walk far past an enemy → enemy despawns (watch Hierarchy count).
- Stand near a Bullet Bill launcher → Bills emit periodically while launcher is on-camera.
- **Bugs to look for:**
  - `GetComponent<IStompable>` returns disabled Koopa walker after shell swap (didn't filter `.enabled`).
  - Double-dispatch: fireball and stomp both resolve on same frame (player lands on enemy same tick fireball hits).
  - Fireball absorbed vs. extinguished confusion (Boo passing through eats the fireball anyway).
  - Stomp combo resets on coyote-time frames (grounded flickers → resets combo).
  - Shell kicked through wall (no wall reflection).
  - Shell hits Mario who kicked it (shell should ignore its kicker for a brief window).
  - Despawn triggers while enemy is on-screen (hysteresis too tight, or camera bounds wrong).
  - Enemy re-spawned repeatedly as camera pans back and forth over launcher (hysteresis not applied to re-spawn).
  - Boo's intangibility window flickers when Mario is exactly perpendicular (threshold needs hysteresis).
  - `PlayerCollisionRouter` misses multi-collider enemies (Chuck with split body).
  - `StompCombo.Notch()` fires from `ICapeSweepHit` kills (combo should only count stomps per §4.7).

---

## Phase 5 — Yoshi

### Tasks
- `RideableMount` + `YoshiController` + saddle parenting.
- Tongue, swallow, dismount-on-damage, recovery window.
- Green Yoshi only; `YoshiData` plumbing in place for future colors.

### Automated tests
- **PlayMode `YoshiHatchAndMountTest`** — proximity to Yoshi egg triggers hatch (configurable radius); pressing mount input with Mario touching Yoshi parents the player rigidbody to `YoshiController.saddle`.
- **PlayMode `MountSuspendsPlayerPhysicsTest`** — while mounted, `PlayerController.FixedUpdate` does not write `rb.linearVelocity`; Yoshi's body drives movement.
- **PlayMode `YoshiTongueBoxCastTest`** — Action press while mounted spawns a tongue box-cast; first eligible target (enemy, berry, pickup) gets parented to `Mouth` transform.
- **PlayMode `YoshiSwallowEnemyTest`** — target in mouth, Action press → target destroyed, points awarded, Yoshi chews.
- **PlayMode `YoshiBerryEatTest`** — target is a berry → berry destroyed; points awarded; egg drop counter increments (if implemented).
- **PlayMode `DismountOnDamageTest`** — external damage event fires while mounted → Mario dismounts without power-state loss, `YoshiController` enters `Panic` state, panic timer starts.
- **PlayMode `PanicRecoveryRemountTest`** — during panic window, Mario touches Yoshi → remounts, panic cleared.
- **PlayMode `PanicTimerExpiresTest`** — panic timer expires (~3s) → `YoshiController` destroyed or marked lost for rest of attempt.
- **PlayMode `PanicCrossBoundsLostTest`** — Yoshi crosses `LevelBounds` during panic → Yoshi lost.
- **PlayMode `ExtendedJumpTest`** — press Jump again at apex of Yoshi's jump → second jump fires (SMW extended-jump behavior); higher peak than a single jump.
- **PlayMode `GreenYoshiFireballSwallowTest`** — Green Yoshi + colored shell in mouth → Action becomes spit-fireball (per §4.8 Green-with-colored-shell borrow).

### Manual verification
- Open the Yoshi Test debug scene.
- Approach egg → hatches.
- Mount Yoshi → player parented; HUD unchanged; power-up state preserved.
- Walk/run/jump with Yoshi; time a second Jump at apex → extended jump (noticeably higher).
- Action press → tongue extends; retract grabs whatever's in reach.
- Action again → swallow; confirm appropriate effect per target.
- Take damage while mounted → forcibly dismounted; Yoshi panics; Mario keeps power state.
- During panic, touch Yoshi within ~3s → remount.
- Let panic expire or let Yoshi wander off the level edge → lost.
- **Bugs to look for:**
  - `PlayerStateMachine` breaks while mounted (Fire Mario loses Fire on mount).
  - Tongue grabs blocks or tiles instead of just `IEdibleByYoshi` targets.
  - Swallow fires even when mouth is empty.
  - Dismount transfers velocity wrong (Mario launches up or through the floor).
  - Panic timer persists across scene reloads (should reset on respawn).
  - Remount during panic doesn't reset the panic timer cleanly.
  - Extended jump works after landing (should only fire at apex of the currently-active jump).
  - Yoshi physics step + Mario physics step both run (mount should suspend Mario's step).
  - `GroundProbe` on Mario fires while mounted (should be suspended with controller step).

---

## Phase 6 — Goal, sub-areas, overworld

### Tasks
- `GoalGate` (with timing-based bonus points) + secret `KeyHole` + carryable `Key` pickup.
- Pipe entry → offset-region sub-area transition → return.
- `Overworld.unity` scene with `MapNode` graph, BFS path traversal, level entry/exit, save persistence (slot system + file select UI).

### Automated tests
- **PlayMode `GoalGateScoringTest`** — touch goal bar at high/mid/low heights → award 8000 / 4000 / 2000 respectively, plus timer-bonus × 50.
- **PlayMode `GoalGatePayloadTest`** — normal goal emits `LevelClearPayload { levelId, secretExit: false, dragonCoinsCollected, scoreEarned }` with correct fields.
- **PlayMode `KeyHoleSecretExitTest`** — carry Key into KeyHole → emits `LevelClearPayload { secretExit: true, ... }`. Normal goal bypassed.
- **PlayMode `PipeEntryAndReturnTest`** — hold Down over a flagged pipe segment → Mario tweens down; camera + Mario snap to sub-area `SpawnMarker`; exit pipe tweens Mario back to the origin region.
- **PlayMode `SubAreaLevelBoundsTest`** — sub-area has its own `LevelBounds`; camera obeys those while Mario is in sub-area.
- **PlayMode `OverworldMapNodeBfsTest`** — place Mario at node A connected to B (right) and not C (left); pressing Right → Mario moves to B; pressing Left → no move.
- **PlayMode `OverworldBranchUnlockTest`** — complete level 5 via secret exit → `LevelData.unlocksOnSecretExit` nodes become walkable.
- **PlayMode `SaveAtMilestonesTest`** — `SaveManager.Save` is called at level clear, overworld node move, switch palace activation, and file select. NOT called during level gameplay. Hook `SaveManager` to count calls.
- **PlayMode `SaveSlotIsolationTest`** — three slots: writing to slot 1 does not affect slot 2 or 3. `SaveIndex` records last-played correctly.
- **PlayMode `DragonCoinBitmaskPersistsTest`** — collect 3 dragon coins in level X, complete with normal exit → bitmask written to save. Re-enter level X → those 3 slots pre-filled.

### Manual verification
- Hit the goal bar at various rotation positions → correct point awards.
- Carry a Key to a KeyHole → secret exit fires; returns to overworld with secret branch flagged.
- Pipe entry: press Down over flagged pipe → smooth tween into pipe; camera + Mario emerge in sub-area.
- Sub-area: explore, return via another pipe.
- Overworld map: walk between nodes. Completed nodes visibly mark. Secret exit unlocks a previously-hidden branch.
- File select: three slots. Create save in slot 1. Quit to title. Continue slot 1 → state restored. Start fresh in slot 2 → slot 1 untouched.
- Save timing: play a level, die mid-level → re-enter title → last-played shows the state at level entry, not mid-level.
- Switch palace activation persists across launches.
- **Bugs to look for:**
  - Sub-area return spawns in the wrong region (offset math off).
  - Pipe tween twitches or teleports mid-animation.
  - Secret exit unlocks both branches (should be one or the other, unless the spec intends otherwise).
  - Dragon-coin bitmask overwritten on subsequent completions that collected fewer.
  - Save fires mid-level (violates §4.24 — only at milestones).
  - Overworld pathing ignores blocked directions (BFS finds a path through a node not actually connected).
  - File select shows stale slot metadata after deletion.
  - `SaveIndex.json` corrupted on simultaneous slot writes.
  - Goal bar rotation speed inconsistent across levels (should be fixed per spec).
  - Level `LevelClearPayload` arrives at overworld before dragon-coin counter is final.

---

## Phase 7 — Content & polish

### Tasks
- Author the 5 content levels + 1 boss room by hand in the Unity editor (§4.26 content-level side — generators are not used here). Each level starts from an empty scene with the standard level hierarchy (`LevelRoot`, tilemaps, `LevelBounds`, `LevelCamera`, `SpawnMarker_Default`, `GoalGate`) manually assembled or duplicated from an existing content level, with a fresh `LevelData_XX.asset` created via `Create → SMW → LevelData` and wired into the scene's `LevelRoot`.
- Title screen, file select, pause menu UI passes.
- Tune palettes, camera curves, jump constants, audio mixer routing with playtesting.

### Automated tests
- **EditMode `ContentLevelsInBuildSettingsTest`** — every `LevelData.sceneRef` references a scene present in Build Settings. Fails early on wiring mistakes.
- **EditMode `LevelDataCompletenessTest`** — every `LevelData` has `timeLimitSeconds > 0`, `musicId` set, `themePalette` assigned, `entryPoints["default"]` present, `unlocksOnNormalExit` configured (possibly empty, but assigned explicitly).
- **EditMode `PaletteRoleCoverageTest`** — every `PaletteRole` used in level palettes exists in the master `Palette` SO. A typoed role fails here.
- **EditMode `PrefabReferenceIntegrityTest`** — no level scene has a broken prefab reference (missing script, missing GUID).
- **EditMode `DragonCoinCountTest`** — each content level has exactly 5 dragon coins placed (or 0 if the level is a non-dragon-coin level like the boss room).
- **PlayMode `BossRoomStompKillTest`** — placeholder Bowser takes 3 stomps → defeated.

### Manual verification
- Playtest each of the 5 levels end-to-end. Subjective "does it feel like SMW" gate — tune jump physics, camera forward bias, and run speed until it does.
- Find 5 dragon coins in each level; they're reachable without tools or glitches.
- Level 5's secret exit unlocks the boss room branch on the overworld map.
- Title screen renders at 1280×720 and at 4:3 / ultrawide (letterbox/pillarbox).
- File select: create/select/delete slots cleanly.
- Pause menu: appears during gameplay; UI nav works with gamepad and keyboard; resume/quit-to-overworld options work.
- Audio mixer: music, SFX, jingles at balanced levels. Pause toggle SFX plays while paused (via `UiSfxChannel` `ignoreListenerPause`).
- Palettes feel consistent across enemies, tiles, Mario, pickups within a single level.
- **Bugs to look for:**
  - Dragon coin placed in an unreachable spot (impossible jump, blocked by enemy respawn, etc.).
  - Level 5 secret-exit path has a physics issue (slope seam, midway ordering) that blocks completion.
  - Title menu unresponsive on gamepad (EventSystem not targeting the panel's selection).
  - Pause menu background bleeds through to gameplay (HUDRoot `raycastTarget` or sort-order issue).
  - Music cross-fades overlap strangely between levels (stack push not paired with pop).
  - A level's `unlocksOnNormalExit` targets a level that doesn't exist (wire error).
  - Boss room stomp count is wrong (off-by-one, or survives the 3rd stomp because HP reset on stomp rebound).
  - Palette tint desyncs between `SpriteRenderer` and uGUI `Image` on the same logical color.

---

## Phase 8 — Hardening

### Tasks
- PlayMode tests for: jump arc reproducibility, power-up transition matrix, damage flow, stomp resolution, save/load round-trip, level state restoration after midway respawn, P-switch swap correctness.
- EditMode tests for SO data validation (every `EnemyData`, `PowerStateData`, `LevelData`, `PaletteRole` reference is checked at editor time via an asset postprocessor or one-shot validator).
- Profile with the Unity Profiler; verify no per-frame allocations in `FixedUpdate`. If a specific `Instantiate`/`Destroy` call site actually shows up as a hot spot, address it locally — don't introduce a general pooling layer.

### Automated tests
Phase 8 mostly *consolidates* tests added throughout earlier phases into a passing regression suite. New tests specific to this phase:
- **EditMode `AssetValidationPostprocessorTest`** — an `AssetPostprocessor` scans all SOs after import; every `EnemyData.sprite`, `PowerStateData.collider`, `LevelData.sceneRef`, and palette reference is non-null. Missing references fail the asset import with a clear error message.
- **EditMode `SaveVersionMigrationTest`** — hand-craft a `save_1.json` at `saveVersion = 0`; load via `SaveManager`; assert migration ran and loaded data matches expected shape.
- **PlayMode `FixedUpdateAllocationFreeTest`** — use a custom `GC.GetTotalAllocatedBytes(true)` scope around 60 FixedUpdate ticks in the Phase 1 movement test scene; assert delta is 0 (or below a small tolerance for Unity internals we can't control).
- **PlayMode `LongSoakTest`** — run a content level for 10 simulated minutes via fast-forward (`Time.timeScale = 10`); assert no frame exceeds `deltaTime` budget, no enemy pool leaks, audio source count doesn't grow.
- **PlayMode `SceneUnloadEventCleanupTest`** — load a level, subscribe-count all `HudViewModel` events; unload the level; re-load; subscribe-count should not have grown.

### Manual verification
- Unity Profiler: play each debug scene + content level 1 for 60 seconds each; watch for per-frame allocations or GC spikes.
- Stress test: spawn many enemies and trigger stomp combos in a custom scene; verify no frame drops at 60 FPS.
- Long soak: leave a level idle 10+ minutes, pause/unpause repeatedly, exit to overworld, re-enter — no state corruption.
- Rapid-save / rapid-load: complete a level, immediately replay it, save at next milestone — save file integrity preserved.
- Resize game window continuously during play — HUD re-layouts without crashing.
- Alt-tab mid-level, return, continue playing — no stuck state.
- Build a standalone player on Windows; exercise the same manual checks. No debug scenes in Build Settings of the shipped build (verify via the scene list in the built player's config).
- **Bugs to look for:**
  - Memory leaks from PrimeTween callbacks not torn down on scene unload.
  - `AudioSource` count growing across level transitions (sources instantiated but never destroyed).
  - Event subscribers surviving scene unload (`HudViewModel`, `ScoreService` subscribers from destroyed GameObjects).
  - Save file corrupted by interrupted write (kill the process during `SaveManager.Save`; relaunch; does it recover gracefully?).
  - `DebugSceneStripper` build preprocessor mis-filters a content scene (check build report).
  - Non-deterministic test failures on CI — often indicates `FindAnyObjectByType` in a hot path or reliance on Unity's default execution order without `[DefaultExecutionOrder]`.
  - Allocation regressions: a seemingly-innocent `foreach` over a `Dictionary.Values` allocating an enumerator every frame.
