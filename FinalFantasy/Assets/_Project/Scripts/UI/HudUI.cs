using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HudUI : MonoBehaviour
{
    Canvas canvas;
    GameObject hudPanel;
    TextMeshProUGUI gilLabel;
    TextMeshProUGUI gilValueText;

    void Awake()
    {
        BuildUI();
        hudPanel.SetActive(false);
    }

    void OnEnable()
    {
        var stateManager = GameManager.Instance?.StateManager;
        if (stateManager != null)
            stateManager.OnStateChanged += OnStateChanged;

        var inv = GameManager.Instance?.InventoryManager;
        if (inv != null)
            inv.OnInventoryChanged += RefreshGil;
    }

    void OnDisable()
    {
        var stateManager = GameManager.Instance?.StateManager;
        if (stateManager != null)
            stateManager.OnStateChanged -= OnStateChanged;

        var inv = GameManager.Instance?.InventoryManager;
        if (inv != null)
            inv.OnInventoryChanged -= RefreshGil;
    }

    void Start()
    {
        // Re-subscribe in case Instance wasn't ready during OnEnable
        var stateManager = GameManager.Instance?.StateManager;
        if (stateManager != null)
        {
            stateManager.OnStateChanged -= OnStateChanged;
            stateManager.OnStateChanged += OnStateChanged;
        }

        var inv = GameManager.Instance?.InventoryManager;
        if (inv != null)
        {
            inv.OnInventoryChanged -= RefreshGil;
            inv.OnInventoryChanged += RefreshGil;
        }

        // Initial state check
        if (stateManager != null && stateManager.CurrentState == GameState.Exploration)
        {
            hudPanel.SetActive(true);
            RefreshGil();
        }
    }

    void OnStateChanged(GameState oldState, GameState newState)
    {
        hudPanel.SetActive(newState == GameState.Exploration);
        if (newState == GameState.Exploration)
            RefreshGil();
    }

    void RefreshGil()
    {
        var inv = GameManager.Instance?.InventoryManager;
        int gil = inv != null ? inv.Gil : 0;
        gilValueText.text = gil.ToString("N0");
    }

    void BuildUI()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5;
        }
        if (GetComponent<CanvasScaler>() == null)
        {
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
        }
        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();

        // HUD Panel (small window in top-left)
        hudPanel = new GameObject("HudPanel");
        var panelRect = hudPanel.AddComponent<RectTransform>();
        panelRect.SetParent(canvas.transform, false);
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(10, -10);
        panelRect.sizeDelta = new Vector2(120, 55);

        hudPanel.AddComponent<UIWindow>();

        // Vertical layout
        var layout = hudPanel.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 2;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.padding = new RectOffset(8, 8, 6, 6);

        // Gil label
        var labelGO = new GameObject("GilLabel");
        var labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.SetParent(panelRect, false);
        var labelLE = labelGO.AddComponent<LayoutElement>();
        labelLE.preferredHeight = 20;

        gilLabel = labelGO.AddComponent<TextMeshProUGUI>();
        gilLabel.text = "Gil";
        gilLabel.fontSize = 14;
        gilLabel.color = new Color(0.8f, 0.8f, 0.8f);
        gilLabel.alignment = TextAlignmentOptions.Center;

        // Gil value
        var valueGO = new GameObject("GilValue");
        var valueRect = valueGO.AddComponent<RectTransform>();
        valueRect.SetParent(panelRect, false);
        var valueLE = valueGO.AddComponent<LayoutElement>();
        valueLE.preferredHeight = 24;

        gilValueText = valueGO.AddComponent<TextMeshProUGUI>();
        gilValueText.text = "0";
        gilValueText.fontSize = 18;
        gilValueText.color = Color.white;
        gilValueText.alignment = TextAlignmentOptions.Center;
        gilValueText.fontStyle = FontStyles.Bold;
    }
}
