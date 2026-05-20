using UnityEngine;

public sealed class TitleRoot : MonoBehaviour
{
    private void Awake()
    {
        if (!GameServices.IsRegistered) return;
        if (GameServices.GameState.Current is TitleState) return;
#if UNITY_EDITOR
        GameServices.GameState.EnterDirectTitle();
#endif
    }
}
