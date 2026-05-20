using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PausedState : IGameState
{
    // Per-player previous-map snapshot so unpausing returns each player to the map they were on.
    // In V1 there's at most one player; the shape generalizes to N for co-op.
    private readonly Dictionary<int, string> _previousMaps = new();

    public void OnEnter()
    {
        Time.timeScale = 0f;
        AudioListener.pause = true;

        _previousMaps.Clear();
        foreach (var p in PlayerInput.all)
        {
            if (p == null) continue;
            _previousMaps[p.playerIndex] = p.currentActionMap?.name;
            p.SwitchCurrentActionMap(InputMapNames.UI);
        }

        AudioBus.Instance?.PlayUiSfx(UiSfxId.Pause);
    }

    public void OnExit()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        foreach (var p in PlayerInput.all)
        {
            if (p == null) continue;
            if (_previousMaps.TryGetValue(p.playerIndex, out var prev) && !string.IsNullOrEmpty(prev))
                p.SwitchCurrentActionMap(prev);
        }
        _previousMaps.Clear();

        AudioBus.Instance?.PlayUiSfx(UiSfxId.Unpause);
    }
}
