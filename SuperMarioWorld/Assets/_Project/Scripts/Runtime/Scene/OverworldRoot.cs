using UnityEngine;

public sealed class OverworldRoot : MonoBehaviour
{
    private void Awake()
    {
        if (!GameServices.IsRegistered) return;
        if (GameServices.GameState.Current is OverworldState) return;
#if UNITY_EDITOR
        GameServices.GameState.EnterDirectOverworld();
#endif
    }
}
