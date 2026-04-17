# SPEC_GAMESTATE

## Context

For now, the game state flow should stay extremely simple.

We only need enough top-level states to cover:

- title screen
- level selection
- being inside a level

Nothing else should be modeled yet.

Do not add a `Boot` game state.

`Bootstrapper` already initializes persistent services before the first scene loads. That is a technical startup step, not a player-facing game state. The first actual top-level state is `Title`.

---

## Top-level states

Use exactly these 3 top-level states:

1. `Title`
2. `LevelSelect`
3. `Gameplay`

---

## Intended flow

The game should move through states in this order:

`Title -> LevelSelect -> Gameplay`

That is the only required flow for now.

---

## Ownership

Top-level game state should be owned by a single persistent controller on `GameServices`.

Use:

- `GameStateController`

Responsibilities:

- store the current top-level `GameState`
- accept requests to change top-level state
- validate simple transitions
- call `SceneLoader` to perform the actual scene load
- update `CurrentGameState` when the transition is completed
- survive scene switches because it lives on `GameServices`

`SceneLoader` is not the owner of game state.

`SceneLoader` is only responsible for:

- loading scenes
- reporting when scene loading has finished

Scene-local UI should not set `CurrentGameState` directly.

Scene-local UI should only request transitions from `GameStateController`.

---

## Scene ownership

Each scene owns its own screen UI.

- `Title` scene owns the title screen UI
- `LevelSelect` scene owns the level selection UI
- `Gameplay` scene owns gameplay objects and gameplay HUD

`GameStateController` does not own screen UI.

It only owns:

- the current top-level state
- transition requests
- coordination with `SceneLoader`

Gameplay HUD is only for `Gameplay`.

`Title` and `LevelSelect` should not use the gameplay HUD.

In the current project, gameplay HUD ownership stays with `StageSession`.

---

## State definitions

### `Title`

The title screen.

Responsibilities:

- show the game's first visible screen
- wait for player input to continue
- own title-specific UI and title-specific scene logic

Exit:

- `Title -> LevelSelect`

### `LevelSelect`

The level selection screen.

Responsibilities:

- allow the player to choose a level
- own level-select-specific UI and scene logic

Exit:

- `LevelSelect -> Gameplay`

### `Gameplay`

The player is inside a level.

Responsibilities:

- run stage gameplay
- own level-specific logic
- own gameplay-only HUD and gameplay objects

---

## Input ownership

Input should be split by context.

### `Title` and `LevelSelect`

These states should use menu/UI input, not the player prefab.

Use:

- the persistent `EventSystem` on `GameServices`
- `InputSystemUIInputModule`
- the `UI` action map from the input actions asset

That means:

- no player spawn is required for `Title`
- no player spawn is required for `LevelSelect`
- menu controls should work without `PlayerInput` on a player prefab

### `Gameplay`

Gameplay input should continue to come from the player prefab.

Use:

- `PlayerInput` on the player prefab
- gameplay actions such as move, jump, attack, and other player controls

So the rule is:

- menu controls = `EventSystem` + `UI` action map
- gameplay controls = player prefab `PlayerInput`

---

## Transition examples

### Title screen -> level selection

If the player presses the Start Game button on the title screen:

- the Start Game button calls `GameStateController`
- `GameStateController` requests the transition to `LevelSelect`
- `GameStateController` calls `SceneLoader`
- when loading completes, `GameStateController` sets `CurrentGameState = LevelSelect`

So the button does **not** load the scene directly.

The button only sends intent.

`GameStateController` owns the transition.

### Level selection -> gameplay

If the player picks a stage:

- the level select UI calls `GameStateController`
- `GameStateController` requests the transition to `Gameplay`
- `GameStateController` calls `SceneLoader`
- when loading completes, `GameStateController` sets `CurrentGameState = Gameplay`

In both cases:

- scene UI sends intent
- `GameStateController` owns the transition
- `SceneLoader` performs the load

---

## Out of scope

Do not model any additional top-level states yet.

Not included for now:

- pause
- cutscenes
- death state
- ending state
- menus beyond title and level select
- stage clear / results

Those can be added later only when they are actually needed.

---

## Recommended enum

```csharp
public enum GameState
{
    Title,
    LevelSelect,
    Gameplay
}
```
