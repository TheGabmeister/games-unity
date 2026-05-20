using UnityEngine;

public sealed class TitleRoot : MonoBehaviour
{
    private void Awake()
    {
        if (GameStateMachine.Instance == null) return;
        if (GameStateMachine.Instance.Current is TitleState) return;
#if UNITY_EDITOR
        GameStateMachine.Instance.EnterDirectTitle();
#endif
    }
}
