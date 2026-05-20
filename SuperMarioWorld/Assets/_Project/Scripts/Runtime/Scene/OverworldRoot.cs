using UnityEngine;

public sealed class OverworldRoot : MonoBehaviour
{
    private void Awake()
    {
        if (GameStateMachine.Instance == null) return;
        if (GameStateMachine.Instance.Current is OverworldState) return;
#if UNITY_EDITOR
        GameStateMachine.Instance.EnterDirectOverworld();
#endif
    }
}
