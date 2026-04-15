using UnityEngine;

namespace SMW
{
    public sealed class LevelRoot : MonoBehaviour
    {
        [SerializeField] private LevelData levelData;
        [SerializeField] private string defaultEntryPoint = "default";

        public LevelData LevelData => levelData;

        private void Awake()
        {
            if (!GameServices.IsRegistered) return;

            if (GameServices.GameState.Current is LevelState) return;

#if UNITY_EDITOR
            if (levelData == null)
            {
                Debug.LogError($"[LevelRoot] No LevelData assigned on {gameObject.name}. Direct-entry aborted.");
                return;
            }
            GameServices.GameState.EnterDirectLevel(levelData, defaultEntryPoint);
#else
            var levelId = levelData != null ? levelData.LevelId : "<unassigned>";
            Debug.LogError($"Level {levelId} loaded without GameStateMachine entry. Build config bug.");
#endif
        }
    }
}
