using UnityEngine;
using UnityEngine.InputSystem;

namespace SMW
{
    public sealed class GameServices : MonoBehaviour
    {
        public static GameServices Instance { get; private set; }

        [SerializeField] private SaveManager saveManager;
        [SerializeField] private SceneLoader sceneLoader;
        [SerializeField] private ScreenFader screenFader;
        [SerializeField] private GameStateMachine gameState;
        [SerializeField] private ScoreService scoreService;
        [SerializeField] private FeedbackService feedbackService;
        [SerializeField] private GameSession gameSession;
        [SerializeField] private AudioBus audioBus;
        [SerializeField] private PlayerInputManager playerInputManager;

        public static SaveManager Save => Instance != null ? Instance.saveManager : null;
        public static SceneLoader SceneLoader => Instance != null ? Instance.sceneLoader : null;
        public static ScreenFader Fader => Instance != null ? Instance.screenFader : null;
        public static GameStateMachine GameState => Instance != null ? Instance.gameState : null;
        public static ScoreService Score => Instance != null ? Instance.scoreService : null;
        public static FeedbackService Feedback => Instance != null ? Instance.feedbackService : null;
        public static GameSession Session => Instance != null ? Instance.gameSession : null;
        public static AudioBus Audio => Instance != null ? Instance.audioBus : null;
        public static PlayerInputManager InputManager => Instance != null ? Instance.playerInputManager : null;

        // Map switching helper. Iterates every joined PlayerInput and switches each to the named map.
        // No-op when no players are joined (common during Boot/Title/Phase 0 tests).
        // This pattern generalizes to local co-op: P1 and P2 both switch to UI on pause, both switch
        // back to Player on unpause — same call, N instances.
        public static void SwitchMapOnAllPlayers(string mapName)
        {
            foreach (var p in PlayerInput.all)
                if (p != null) p.SwitchCurrentActionMap(mapName);
        }

        public static bool IsRegistered => Instance != null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            sceneLoader?.Bind(screenFader);
            gameState?.Bind(this);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
