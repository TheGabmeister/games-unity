using UnityEngine;
using SMW.Audio;
using SMW.Feedback;
using SMW.Save;
using SMW.Scene;
using SMW.Score;
using SMW.Session;
using SMW.State;

namespace SMW.Core
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

        public static SaveManager Save => Instance != null ? Instance.saveManager : null;
        public static SceneLoader SceneLoader => Instance != null ? Instance.sceneLoader : null;
        public static ScreenFader Fader => Instance != null ? Instance.screenFader : null;
        public static GameStateMachine GameState => Instance != null ? Instance.gameState : null;
        public static ScoreService Score => Instance != null ? Instance.scoreService : null;
        public static FeedbackService Feedback => Instance != null ? Instance.feedbackService : null;
        public static GameSession Session => Instance != null ? Instance.gameSession : null;
        public static AudioBus Audio => Instance != null ? Instance.audioBus : null;

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
