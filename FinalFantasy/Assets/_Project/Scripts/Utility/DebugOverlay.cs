using UnityEngine;
using TMPro;

public class DebugOverlay : MonoBehaviour
{
    TextMeshProUGUI displayText;
    bool isVisible;
    float fpsTimer;
    float currentFPS;

    Canvas canvas;

    void Awake()
    {
        SetupUI();
        gameObject.SetActive(true);
        SetVisible(false);
    }

    void SetupUI()
    {
        // Find or create canvas - this should be on a DontDestroyOnLoad canvas
        canvas = GetComponentInParent<Canvas>();

        // Create text display
        var textGO = new GameObject("DebugText");
        textGO.transform.SetParent(transform, false);

        displayText = textGO.AddComponent<TextMeshProUGUI>();
        displayText.fontSize = 14;
        displayText.color = Color.green;
        displayText.alignment = TextAlignmentOptions.TopLeft;
        displayText.fontStyle = FontStyles.Normal;

        // Add a dark semi-transparent background
        var bg = gameObject.AddComponent<UnityEngine.UI.Image>();
        bg.color = new Color(0, 0, 0, 0.7f);
        bg.raycastTarget = false;

        var rect = GetComponent<RectTransform>();
        if (rect == null) rect = gameObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(10, -10);
        rect.sizeDelta = new Vector2(300, 150);

        var textRect = displayText.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(5, 5);
        textRect.offsetMax = new Vector2(-5, -5);
    }

    void Update()
    {
        // Toggle with F1 (also check InputManager if available)
        bool togglePressed = UnityEngine.Input.GetKeyDown(KeyCode.F1);
        if (!togglePressed)
        {
            var input = GameManager.Instance?.InputManager;
            if (input?.DebugOverlayAction != null && input.DebugOverlayAction.WasPressedThisFrame())
                togglePressed = true;
        }

        if (togglePressed)
            SetVisible(!isVisible);

        if (!isVisible) return;

        // FPS calculation
        fpsTimer += (Time.unscaledDeltaTime - fpsTimer) * 0.1f;
        currentFPS = 1f / fpsTimer;

        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        var gm = GameManager.Instance;
        string state = gm?.StateManager?.CurrentState.ToString() ?? "N/A";
        string scene = gm?.SceneLoader?.CurrentSceneName ?? "N/A";
        string explorationMode = gm?.StateManager?.CurrentExplorationMode.ToString() ?? "N/A";

        var player = Object.FindFirstObjectByType<PlayerController>();
        string playerPos = player != null ? player.GridPosition.ToString() : "N/A";
        string facing = player != null ? GetDirectionName(player.FacingDirection) : "N/A";

        displayText.text =
            $"FPS: {currentFPS:F1}\n" +
            $"State: {state}\n" +
            $"Scene: {scene}\n" +
            $"Mode: {explorationMode}\n" +
            $"Pos: {playerPos}\n" +
            $"Facing: {facing}";
    }

    string GetDirectionName(int dir) => dir switch
    {
        0 => "Up", 1 => "Right", 2 => "Down", 3 => "Left", _ => "?"
    };

    void SetVisible(bool visible)
    {
        isVisible = visible;
        if (displayText != null) displayText.gameObject.SetActive(visible);
        var bg = GetComponent<UnityEngine.UI.Image>();
        if (bg != null) bg.enabled = visible;
    }
}
