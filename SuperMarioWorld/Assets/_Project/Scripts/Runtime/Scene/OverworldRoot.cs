using UnityEngine;
using SMW.Core;
using SMW.State;

namespace SMW.Scene
{
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
}
