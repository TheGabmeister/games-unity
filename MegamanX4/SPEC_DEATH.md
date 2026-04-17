# SPEC_DEATH

## Context

This spec defines the first pass of player death, respawn, and checkpoints for the current Unity 6 `Mega Man X4` prototype. The goal is a reliable in-stage retry loop that works with the existing code-driven bootstrap and hand-authored gameplay scenes.

This pass intentionally excludes lives, game-over flow, save persistence, and boss-specific retry rules.

## Goals

- Death immediately locks the player out of movement and firing.
- After a short delay, the active gameplay scene reloads.
- Persistent services survive the reload.
- The player respawns at the last activated checkpoint in the same scene.
- If no checkpoint was activated, the player respawns at `PlayerStart`.
- If no `PlayerStart` exists, spawning falls back to the current `StageSession` behavior.
- Respawn restores full health and full weapon energy by creating a fresh player instance from the player prefab.

## Runtime ownership

### `StageSession`

`StageSession` owns the stage-local retry flow.

Responsibilities:

- Resolve the initial spawn point for the active scene.
- Spawn the player prefab and bind the HUD.
- Listen to the player's `Health.Depleted` event.
- Start a short respawn delay.
- Mark the current scene as a pending respawn in persistent state.
- Reload the current scene in `LoadSceneMode.Single`.

### `CheckpointStateService`

`CheckpointStateService` lives on the persistent `GameServices` root and carries checkpoint metadata across a retry reload.

State tracked:

- current scene name
- active checkpoint id
- default spawn position for the current scene
- pending-respawn scene name

Behavior:

- Entering a scene normally clears the active checkpoint.
- Entering the same scene during a pending respawn preserves the active checkpoint id.
- Reloading a scene without a matching checkpoint falls back to the default spawn and clears the stale checkpoint id.

## Components

### `Checkpoint`

Author checkpoints as hand-placed scene objects.

Serialized fields:

- `_checkpointId`
- optional `_respawnPoint`

Behavior:

- Requires a `Collider2D`.
- Forces the collider to be a trigger.
- On player trigger enter, reports the checkpoint id to `CheckpointStateService`.
- Uses `_respawnPoint.position` when assigned; otherwise uses its own transform position.

### `Health`

Add a kill-style API for future instant-death hazards.

New API:

- `Kill()`
- `Kill(Vector2 sourcePosition)`

Behavior:

- Ignores invulnerability.
- Sets current health to zero immediately.
- Fires normal damage/depleted events so existing listeners still work.

### `PlayerController`

Add a minimal death gate.

Behavior on `Health.Depleted`:

- mark player as dead
- clear movement state and timers
- exit ladders
- cancel charge state
- disable `PlayerInput`
- stop processing movement and firing for the remainder of the object's lifetime

No dedicated death animation pipeline is included in this pass.

## Spawn and respawn rules

Spawn priority:

1. active checkpoint id in the current scene
2. `PlayerStart` tag
3. `StageSession` fallback position

Retry flow:

1. player health depletes
2. `PlayerController` locks control immediately
3. `StageSession` marks the scene as pending respawn
4. wait for a short serialized delay
5. reload the current gameplay scene
6. `StageSession` resolves the saved checkpoint and spawns a fresh player
7. HUD rebinds to the new player

## Authoring notes

- Checkpoints should be added to gameplay scenes by hand in the Unity Editor.
- The stage start still works as the implicit default checkpoint even if no checkpoint objects exist yet.
- This implementation uses scene reload instead of trying to diff/reset runtime objects manually.

## Verification

- Player death from normal HP depletion reloads the gameplay scene after a short delay.
- A fresh player is spawned with full HP and full weapon energy.
- Activating a checkpoint changes the next respawn location.
- Missing or stale checkpoint ids safely fall back to `PlayerStart`, then to the `StageSession` position.
- `Health.Kill()` depletes even while the target is invulnerable.
