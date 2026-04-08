using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TitleScreenUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] Button newGameButton;
    [SerializeField] Button continueButton;
    [SerializeField] TextMeshProUGUI newGameLabel;
    [SerializeField] TextMeshProUGUI continueLabel;

    bool initialized;

    void Start()
    {
        SetupUI();
    }

    void SetupUI()
    {
        // If references aren't set up by SceneSetup, build them programmatically
        if (titleText == null)
            BuildTitleUI();

        // Wire buttons
        newGameButton?.onClick.AddListener(OnNewGame);
        continueButton?.onClick.AddListener(OnContinue);

        // Check if saves exist
        bool hasSaves = GameManager.Instance?.SaveManager?.FindMostRecentSlot() != null;
        if (continueButton != null)
            continueButton.interactable = hasSaves;

        // Set initial selection
        if (newGameButton != null)
            EventSystem.current?.SetSelectedGameObject(newGameButton.gameObject);

        // Play title music
        GameManager.Instance?.Audio?.PlayBGM(MusicTrack.Title);

        initialized = true;
    }

    void BuildTitleUI()
    {
        // Create Canvas if needed
        var canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            gameObject.AddComponent<GraphicRaycaster>();
        }

        // Title text
        var titleGO = CreateTextObject("TitleText", "FINAL FANTASY", 48, TextAlignmentOptions.Center);
        var titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.SetParent(transform, false);
        titleRect.anchorMin = new Vector2(0.5f, 0.7f);
        titleRect.anchorMax = new Vector2(0.5f, 0.7f);
        titleRect.sizeDelta = new Vector2(600, 80);
        titleText = titleGO.GetComponent<TextMeshProUGUI>();

        // New Game button
        newGameButton = CreateMenuButton("NewGameBtn", "New Game", new Vector2(0.5f, 0.4f));
        newGameButton.transform.SetParent(transform, false);

        // Continue button
        continueButton = CreateMenuButton("ContinueBtn", "Continue", new Vector2(0.5f, 0.32f));
        continueButton.transform.SetParent(transform, false);

        // Wire navigation
        var newNav = new Navigation { mode = Navigation.Mode.Explicit };
        newNav.selectOnDown = continueButton;
        newGameButton.navigation = newNav;

        var contNav = new Navigation { mode = Navigation.Mode.Explicit };
        contNav.selectOnUp = newGameButton;
        continueButton.navigation = contNav;

        // Get labels
        newGameLabel = newGameButton.GetComponentInChildren<TextMeshProUGUI>();
        continueLabel = continueButton.GetComponentInChildren<TextMeshProUGUI>();
    }

    Button CreateMenuButton(string name, string label, Vector2 anchorPos)
    {
        var go = new GameObject(name);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorPos;
        rect.anchorMax = anchorPos;
        rect.sizeDelta = new Vector2(250, 50);

        var image = go.AddComponent<Image>();
        image.color = new Color(0, 0, 0.267f, 0.95f);

        var button = go.AddComponent<Button>();
        var colors = button.colors;
        colors.normalColor = new Color(0, 0, 0.267f, 0.95f);
        colors.highlightedColor = new Color(0.1f, 0.1f, 0.4f, 0.95f);
        colors.selectedColor = new Color(0.15f, 0.15f, 0.5f, 0.95f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.6f, 0.95f);
        colors.disabledColor = new Color(0.15f, 0.15f, 0.15f, 0.6f);
        button.colors = colors;

        var textGO = CreateTextObject("Label", label, 24, TextAlignmentOptions.Center);
        textGO.transform.SetParent(go.transform, false);
        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        return button;
    }

    GameObject CreateTextObject(string name, string text, float fontSize, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = alignment;
        tmp.fontStyle = FontStyles.Bold;
        return go;
    }

    async void OnNewGame()
    {
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Confirm);
        GameManager.Instance?.StateManager?.ChangeState(GameState.Transition);
        await GameManager.Instance.SceneLoader.LoadScene("PartyCreation");
    }

    async void OnContinue()
    {
        var saveManager = GameManager.Instance?.SaveManager;
        if (saveManager == null) return;

        int? slot = saveManager.FindMostRecentSlot();
        if (slot == null) return;

        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Confirm);

        // Load the save data
        var data = saveManager.Load(slot.Value);
        if (data == null) return;

        // Set pending position and load scene
        ExplorationInitializer.PendingPlayerPosition = new Vector2Int(data.PlayerGridX, data.PlayerGridY);

        GameManager.Instance?.StateManager?.ChangeState(GameState.Exploration);
        await GameManager.Instance.SceneLoader.LoadScene(data.CurrentScene);

        // Restore party/inventory/flags after scene loads
        SaveHelper.ApplySaveData(data);
    }
}
