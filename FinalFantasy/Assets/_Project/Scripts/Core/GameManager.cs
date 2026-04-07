using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameStateManager StateManager { get; private set; }
    public AudioManager Audio { get; private set; }
    public SaveManager SaveManager { get; private set; }
    public SceneLoader SceneLoader { get; private set; }
    public InputManager InputManager { get; private set; }
    public DataRepository DataRepository { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        StateManager = GetComponentInChildren<GameStateManager>();
        Audio = GetComponentInChildren<AudioManager>();
        SaveManager = GetComponentInChildren<SaveManager>();
        SceneLoader = GetComponentInChildren<SceneLoader>();
        InputManager = GetComponentInChildren<InputManager>();
        DataRepository = GetComponentInChildren<DataRepository>();
    }

    async Awaitable Start()
    {
        await Awaitable.NextFrameAsync();
        StateManager.ChangeState(GameState.Title);
        await SceneLoader.LoadScene("Title");
    }
}
