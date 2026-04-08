using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Game Over screen: displayed when all party members are KO'd/Stone'd.
/// Shows "Game Over" text, then returns to title screen on Confirm.
public class GameOverUI : MonoBehaviour
{
    Canvas canvas;
    GameObject rootPanel;
    TextMeshProUGUI gameOverText;
    TextMeshProUGUI promptText;
    bool waitingForInput;

    void Awake()
    {
        BuildUI();
        rootPanel.SetActive(false);
    }

    void OnEnable()
    {
        var bm = BattleManager.Instance;
        if (bm != null)
            bm.OnDefeat += OnDefeat;
    }

    void OnDisable()
    {
        var bm = BattleManager.Instance;
        if (bm != null)
            bm.OnDefeat -= OnDefeat;
    }

    void Update()
    {
        if (!waitingForInput) return;
        var input = GameManager.Instance?.InputManager;
        if (input?.ConfirmAction != null && input.ConfirmAction.WasPressedThisFrame())
        {
            waitingForInput = false;
            GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Confirm);
            ReturnToTitle();
        }
    }

    async void OnDefeat()
    {
        rootPanel.SetActive(true);
        GameManager.Instance?.Audio?.PlayBGM(MusicTrack.GameOver);

        // Fade in the game over screen
        gameOverText.text = "Game Over";
        promptText.text = "";

        await Awaitable.WaitForSecondsAsync(2f);

        promptText.text = "Press Confirm to continue";
        waitingForInput = true;
    }

    async void ReturnToTitle()
    {
        rootPanel.SetActive(false);

        // Unload battle scene
        try { await GameManager.Instance.SceneLoader.UnloadScene("Battle"); }
        catch (System.Exception) { }

        // Change state to title
        GameManager.Instance?.StateManager?.ChangeState(GameState.Title);
        await GameManager.Instance.SceneLoader.LoadScene("Title");
    }

    void BuildUI()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
        }
        if (GetComponent<CanvasScaler>() == null)
        {
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
        }

        rootPanel = new GameObject("GameOverRoot");
        var rootRect = rootPanel.AddComponent<RectTransform>();
        rootRect.SetParent(canvas.transform, false);
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.sizeDelta = Vector2.zero;

        // Black background
        var bg = rootPanel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.9f);

        // Game Over text
        var goGO = new GameObject("GameOverText");
        goGO.transform.SetParent(rootRect, false);
        gameOverText = goGO.AddComponent<TextMeshProUGUI>();
        gameOverText.fontSize = 48;
        gameOverText.color = Color.red;
        gameOverText.alignment = TextAlignmentOptions.Center;
        var goRect = gameOverText.rectTransform;
        goRect.anchorMin = new Vector2(0, 0.4f);
        goRect.anchorMax = new Vector2(1, 0.7f);
        goRect.sizeDelta = Vector2.zero;

        // Prompt text
        var ptGO = new GameObject("PromptText");
        ptGO.transform.SetParent(rootRect, false);
        promptText = ptGO.AddComponent<TextMeshProUGUI>();
        promptText.fontSize = 20;
        promptText.color = Color.white;
        promptText.alignment = TextAlignmentOptions.Center;
        var ptRect = promptText.rectTransform;
        ptRect.anchorMin = new Vector2(0, 0.2f);
        ptRect.anchorMax = new Vector2(1, 0.35f);
        ptRect.sizeDelta = Vector2.zero;
    }
}
