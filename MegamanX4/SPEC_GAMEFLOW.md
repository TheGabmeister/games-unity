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

## New persistent overlay + services

All live on the `GameServices` prefab so they survive scene loads.

### `LoadingScreen` — persistent overlay (not a service)

Full-screen overlay canvas with a plain `LoadingScreen : MonoBehaviour` exposing `Show()` / `Hide()`. No progress bar in this phase — minimum visible duration is enforced by the caller.

Not registered in `Services`. The only caller is `GameStateController`, which holds a direct reference (either `[SerializeField] LoadingScreen _loading` wired in the prefab, or `GetComponentInChildren<LoadingScreen>(true)` in Awake to match the existing `RegisterKnownServices` pattern for ScreenFader et al.). Promote to a registered service only if a second caller ever appears.

Layering (in `GameServices` prefab):
- `ScreenFader` canvas renders **above** `LoadingScreen` canvas (higher `sortingOrder`), so fade-to-black visually covers the moment the loading screen appears/disappears.

### `ScreenFader` — return `Tween`, demote from service

Replace the hand-rolled `Update`-driven lerp with `PrimeTween.Tween.Color(...)` and have `FadeToColor` return the `Tween` so the conductor can `await` it:

```csharp
public class ScreenFader : MonoBehaviour
{
    [SerializeField] Image _image;

    public Tween FadeToColor(Color color, float duration) =>
        Tween.Color(_image, color, duration);
}
```

Drop `IScreenFaderService` and its `RegisterService<ScreenFader, IScreenFaderService>` line in `Services.RegisterKnownServices`. No external code calls `FadeToColor` today — the interface was abstraction-in-waiting for a caller that never appeared. `GameStateController` holds a direct reference (same pattern as `LoadingScreen`):

```csharp
// GameStateController
[SerializeField] ScreenFader _fader;  // wired in the GameServices prefab
// or: _fader = GetComponentInChildren<ScreenFader>(true);
```

Promote back to a registered service the day a second caller appears (likely when death/cutscene fades land).

### `SceneLoader` — return `Task`, demote from service, drop unused API

Three changes in one pass:

**1. Return `Task`.** `SceneManager.LoadSceneAsync` already exposes an `AsyncOperation.completed` event — bridge that directly to a `TaskCompletionSource`. No coroutine, no `while (!op.isDone)` polling loop.

**2. Drop `ISceneLoader`.** Same reasoning as `IScreenFaderService`: only `GameStateController` uses it. Drop the interface and the `RegisterService<SceneLoader, ISceneLoader>` line. `GameStateController` holds a direct reference.

**3. Delete `SwitchScene`.** It's an additive-load-then-unload helper with zero callers anywhere in the codebase. Dead code.

Final shape:

```csharp
public class SceneLoader : MonoBehaviour
{
    public Task LoadSceneByName(string sceneName)
    {
        var tcs = new TaskCompletionSource<bool>();
        var op = SceneManager.LoadSceneAsync(sceneName);
        op.completed += _ => tcs.SetResult(true);
        return tcs.Task;
    }

    public Task LoadSceneByIndex(int index)
    {
        var tcs = new TaskCompletionSource<bool>();
        var op = SceneManager.LoadSceneAsync(index);
        op.completed += _ => tcs.SetResult(true);
        return tcs.Task;
    }
}
```

The whole file shrinks by ~80 lines. `GameStateController` holds `[SerializeField] SceneLoader _sceneLoader;` (wired in the prefab) and awaits directly. Promote back to a registered service the day a second caller appears.

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

Existing controller. `ShowMenu()` becomes `async` and awaits the fader (see "Transition sequences" below). No scene load — this is pure intra-scene state change wrapped in a fade.

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

Transitions are sequenced in `GameStateController` using `async`/`await` over PrimeTween `Tween`s. No coroutines, no callback pyramids. Scene-load completion is a `TaskCompletionSource<bool>` adapter.

Shared timing lives on the conductor as serialized fields — no magic-number literals in sequence code:

```csharp
[SerializeField] float _fadeDuration = 0.3f;
[SerializeField] float _minLoadingSeconds = 1f;

Tween Fade(Color color) => _fader.FadeToColor(color, _fadeDuration);
```

### Title Press-Start → Menu (in-scene)

Lives on `TitleSceneController`, which holds its own fade reference (or reads `GameStateController._fadeDuration` if we decide to centralize). Spec default: scene-local fade duration.

```csharp
async void ShowMenu()
{
    await _fader.FadeToColor(Color.black, _fadeDuration);
    _phase = Phase.Menu;
    _pressStartRoot.SetActive(false);
    _menuRoot.SetActive(true);
    RepaintMenu();
    await _fader.FadeToColor(Color.clear, _fadeDuration);
}
```

### New Game → CharacterSelect / Character X → SkyLagoon

Both flows share the same sequence; only the destination scene differs. The conductor exposes two named intents and a private helper:

```csharp
public async void GoToCharacterSelect() =>
    await FadeToLoadingThenLoad("CharacterSelect");

public async void LoadStage(string stageSceneName) =>
    await FadeToLoadingThenLoad(stageSceneName);

async Task FadeToLoadingThenLoad(string sceneName)
{
    await Fade(Color.black);
    _loading.Show();
    await Fade(Color.clear);
    await Tween.Delay(_minLoadingSeconds);
    await Fade(Color.black);
    await _sceneLoader.LoadSceneByName(sceneName);
    _loading.Hide();
    await Fade(Color.clear);
}
```

The sequence reads top-to-bottom exactly like the wall-clock timeline. No TCS adapter — `SceneLoader` returns `Task` directly.

**`async void` caveat**: the public intents are `async void` because they're fire-and-forget UI calls. Safe here because `GameStateController` lives on `GameServices` (DontDestroyOnLoad) and never gets destroyed mid-transition. If that ever changes, swap the publics to `async Task` and let the caller await, or wrap in `Sequence.Create().Chain(...)` to stay synchronous at the callsite.

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
- Fade durations default to 0.3s via the serialized `_fadeDuration` on `GameStateController`.
- Minimum loading-screen visibility defaults to 1s via `_minLoadingSeconds`.
- Video rendering path: RawImage + RenderTexture vs Camera target. Default: RawImage + RenderTexture.

---

## Implementation phases

Each phase is independently shippable:

1. **Enum expansion + routing**. Add `Intro`, `CharacterSelect` to `GameState`. Route `SetState(Intro)` → `Init.unity`, `SetState(CharacterSelect)` → `CharacterSelect.unity`.
2. **Init scene + IntroController**. Hand-author scene with VideoPlayer + RawImage + PlayerInput. Implement skip/end → `SetState(Title)`.
3. **Async API pass + service demotions**: `ScreenFader` → PrimeTween with `Tween` return type, drop `IScreenFaderService`. `SceneLoader.LoadSceneByName`/`LoadSceneByIndex` → return `Task` via `AsyncOperation.completed`, drop the coroutine internals, drop `ISceneLoader`, delete the unused `SwitchScene`. `GameStateController` holds both as direct references. API changes only; no UX changes yet.
4. **LoadingScreen overlay**. Add `LoadingScreen` component + canvas to the GameServices prefab. Wire a reference into `GameStateController`. No service registration.
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
