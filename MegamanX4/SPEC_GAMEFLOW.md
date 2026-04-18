# SPEC_GAMEFLOW

## Context

Extends [SPEC_GAMESTATE.md](SPEC_GAMESTATE.md). Defines the full boot → gameplay path, including the intro video, fade/loading transitions, and character selection.

SPEC_GAMESTATE originally locked the top-level state enum to exactly three values (`Title`, `LevelSelect`, `Gameplay`). This spec expands that list. Treat this file as authoritative where the two disagree.

---

## Target flow

The New Game path, exactly:

1. Game opens in `Init.unity`. Intro video plays. `Submit`/Confirm skips. Video end **or** skip → transition to `Title`.
2. In `Title`, pressing Start fades to black, fades back in, and shows the `New Game` / `Continue` / `Options` menu (same scene — not a scene load).
3. Selecting **New Game** fades to black, fades back in over a loading screen.
4. Loading screen persists ≥ 1 second, then fades to black, swaps scene to `CharacterSelect`, fades back in.
5. In `CharacterSelect`, selecting **X** fades to black, shows the loading screen, loads `SkyLagoon`, fades back in on gameplay.

Stops here for this spec.

---

## Top-level states

Expanded `GameState` enum:

```csharp
public enum GameState
{
    Intro,
    Title,
    CharacterSelect,
    LevelSelect,
    Gameplay
}
```

Notes:
- `LevelSelect` is kept for the post-SkyLagoon 8-stage screen, but **is not reachable from New Game**. That transition is deferred.
- `Loading` is **not** a top-level state. The loading screen is a persistent overlay, not a scene or state.

---

## New scenes

- `Init.unity` — first scene in Build Settings. Hosts video player + skip input.
- `CharacterSelect.unity` — X / Zero portraits + selection cursor.

Existing scenes used: `Title.unity`, `SkyLagoon.unity`, and (later) the other 7 stages + `LevelSelect.unity`.

Build Settings scene order:

```
0. Init
1. Title
2. CharacterSelect
3. LevelSelect
4..N. Stages/*.unity
```

---

## New persistent services

Both live on the `GameServices` prefab. Registered by `Services.RegisterKnownServices`.

### `LoadingScreen` (new)

Full-screen overlay canvas. Simple `Show()` / `Hide()`. No progress bar in this phase — minimum visible duration is enforced by the caller.

Contract:

```csharp
public interface ILoadingScreenService
{
    void Show();
    void Hide();
}
```

Layering (in `GameServices` prefab):
- `ScreenFader` canvas renders **above** `LoadingScreen` canvas (higher `sortingOrder`), so fade-to-black visually covers the moment the loading screen appears/disappears.

### `ScreenFader` — add completion callback

Existing `FadeToColor(Color, float)` is fire-and-forget. Add an overload:

```csharp
void FadeToColor(Color color, float duration, System.Action onComplete);
```

Implementation: swap the hand-rolled lerp for `PrimeTween.Tween.Color(...)` (already in the runtime asmdef) with `OnComplete`. Or keep the lerp and invoke the callback from `Update` when `_lerpTime >= _duration`. Either works; PrimeTween is cleaner.

### `SceneLoader` — completion for `LoadSceneByName`

`LoadSceneByIndex` already has an `onComplete` parameter. Add the same to `LoadSceneByName`:

```csharp
void LoadSceneByName(string sceneName, System.Action onComplete = null);
```

---

## `GameStateController` responsibilities

Becomes the orchestrator for composite transitions (fade + load + loading-screen min-duration + fade-in). The caller (a scene's UI controller) sends intent; the controller runs the sequence.

New/updated API:

```csharp
// Simple scene swaps (no fade / loading screen).
void SetState(GameState state);            // Intro, Title — direct loads
// New Game path:
void GoToCharacterSelect();                 // fade → show loading → load CharacterSelect → min 1s → hide loading → fade
// Character confirmation:
void LoadStage(string stageSceneName);      // fade → show loading → load stage → hide loading → fade
```

Rationale: composite sequences belong behind named methods, not as flags on `SetState`. Scene-local UI stays dumb.

---

## Scene-local controllers

### `IntroController` (on `Init.unity`)

Canvas + RawImage + VideoPlayer + PlayerInput (UI map, C# events).

- Video plays on scene start (Render to `RenderTexture` → RawImage).
- Subscribes to `Submit.started` → skip.
- Subscribes to `VideoPlayer.loopPointReached` → end.
- Either event → `gameState.SetState(GameState.Title)`. Fade handled by `GameStateController` if we decide to fade here; spec default is **snap** for Init→Title (open question, see below).

### `TitleSceneController` — in-scene fade between Press-Start and Menu

Existing controller. Extend so `ShowMenu()` does:
```
fader.FadeToColor(Black, 0.3s, () => {
    SwapToMenuPhase();
    fader.FadeToColor(Clear, 0.3s, null);
});
```

No scene load — this is pure intra-scene state change wrapped in a fade.

### `CharacterSelectController` (on `CharacterSelect.unity`)

Mirrors `LevelSelectController` pattern. Two entries (X, Zero). Navigate Left/Right. Submit on X → `gameState.LoadStage("SkyLagoon")`. Zero selection is scaffolded but not reachable from gameplay yet.

---

## Transition sequences

### Init → Title (video end or skip)

Snap — no fade. Video ends on its final frame, and the Title scene's Press-Start screen takes over immediately.

```
IntroController.OnSkipOrVideoEnd()
  → gameState.SetState(GameState.Title)
      → sceneLoader.LoadSceneByName("Title")
```

### Title Press-Start → Menu (in-scene)

```
TitleSceneController.OnSubmit() on PressStart phase
  → fader.FadeToColor(Black, 0.3s, () => {
        _phase = Menu; _pressStartRoot.SetActive(false); _menuRoot.SetActive(true);
        fader.FadeToColor(Clear, 0.3s, null);
    })
```

### New Game → CharacterSelect

```
TitleSceneController.StartNewGame()
  → gameState.GoToCharacterSelect()
      → fader.FadeToColor(Black, 0.3s, () => {
            loading.Show();
            fader.FadeToColor(Clear, 0.3s, () => {
                StartCoroutine(WaitThenLoad(1.0f, "CharacterSelect"));
            });
        })

WaitThenLoad(minSeconds, sceneName):
  yield return new WaitForSeconds(minSeconds);
  fader.FadeToColor(Black, 0.3s, () => {
      sceneLoader.LoadSceneByName(sceneName, () => {
          loading.Hide();
          fader.FadeToColor(Clear, 0.3s, null);
      });
  });
```

### Character X → SkyLagoon

Same pattern as `GoToCharacterSelect` — fade to black, reveal loading screen, wait ≥ 1s, fade to black, load scene, fade in. The 1s minimum applies everywhere a loading screen is shown.

```
CharacterSelectController.OnSubmitX()
  → gameState.LoadStage("SkyLagoon")
      → fader.FadeToColor(Black, 0.3s, () => {
            loading.Show();
            fader.FadeToColor(Clear, 0.3s, () => {
                StartCoroutine(WaitThenLoad(1.0f, "SkyLagoon"));
            });
        })

WaitThenLoad(minSeconds, sceneName):
  yield return new WaitForSeconds(minSeconds);
  fader.FadeToColor(Black, 0.3s, () => {
      sceneLoader.LoadSceneByName(sceneName, () => {
          loading.Hide();
          fader.FadeToColor(Clear, 0.3s, null);
      });
  });
```

`GoToCharacterSelect` and `LoadStage` share this exact sequence — only the destination scene name differs. Expect the implementation to extract a shared helper on `GameStateController` (e.g. `FadeToLoadingThenLoad(string sceneName, float minSeconds)`).

---

## Input ownership

Unchanged from SPEC_GAMESTATE: menus use the shared `EventSystem` on `GameServices` + the `UI` action map. `Intro` and `CharacterSelect` are menu screens, so they follow that rule. Only `Gameplay` uses the player prefab's `PlayerInput`.

---

## Asset dependencies

- **`IntroVideo.mp4`**: already at [Assets/_Project/Videos/IntroVideo.mp4](Assets/_Project/Videos/IntroVideo.mp4) (per the user prompt).
- **Character portraits (X, Zero)**: defer to procedural SVG or TextMeshPro labels if no art yet. CLAUDE.md: "No asset dependencies until explicitly added."
- **Loading screen visuals**: plain "NOW LOADING" TMP label centered on a black canvas is fine for phase 1.

---

## Resolved decisions

- **Init → Title**: snap, no fade.
- **Loading screen minimum duration**: 1 second, applied to every transition that shows it (New Game → CharacterSelect, CharacterSelect → Stage). The 1s is a deliberate floor — exercises the overlay so we can verify it renders.
- **Layering**: `ScreenFader` canvas renders above `LoadingScreen` canvas (higher `sortingOrder`), so fades visually cover the moments the loading screen appears and disappears.

Still open, low priority — tune once running:
- Fade durations (spec uses 0.3s everywhere).
- Video rendering path: RawImage + RenderTexture vs Camera target. Default: RawImage + RenderTexture.

---

## Implementation phases

Each phase is independently shippable:

1. **Enum expansion + routing**. Add `Intro`, `CharacterSelect` to `GameState`. Route `SetState(Intro)` → `Init.unity`, `SetState(CharacterSelect)` → `CharacterSelect.unity`.
2. **Init scene + IntroController**. Hand-author scene with VideoPlayer + RawImage + PlayerInput. Implement skip/end → `SetState(Title)`.
3. **ScreenFader callback + SceneLoader callback**. API additions only; no UX changes yet.
4. **LoadingScreen service**. Add to GameServices prefab, register in Services.
5. **Title in-scene fade**. Wrap `ShowMenu()` in fade-out/fade-in.
6. **Composite `GoToCharacterSelect()` + `LoadStage()` on GameStateController**.
7. **CharacterSelect scene + controller**.
8. **Wire Title → GoToCharacterSelect and CharacterSelect → LoadStage("SkyLagoon")**.

---

## Out of scope

- Continue flow (depends on save system)
- Options flow (depends on settings system)
- Post-SkyLagoon transition to `LevelSelect`
- Zero campaign playable path (character select scaffolds both, only X is reachable)
- Real async load progress UI (fixed minimum duration substitutes for it)
- Skipping the fade after a quick-restart from death
