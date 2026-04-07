using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class MainMenuUI : MonoBehaviour
{
    // Sub-screen components (assigned at build time)
    ItemMenuUI itemMenu;
    EquipmentMenuUI equipmentMenu;
    MagicMenuUI magicMenu;
    StatusMenuUI statusMenu;
    OrderMenuUI orderMenu;

    // Runtime UI
    Canvas canvas;
    GameObject rootPanel;
    GameObject mainPanel;
    GameObject menuPanel;
    GameObject partyInfoPanel;

    // Menu buttons
    Button[] menuButtons;
    TextMeshProUGUI[] menuLabels;
    TextMeshProUGUI cursorText;
    int selectedMenuIndex;

    // Party info texts
    TextMeshProUGUI[] memberInfoTexts = new TextMeshProUGUI[4];
    TextMeshProUGUI gilText;
    TextMeshProUGUI timeText;

    bool isOpen;
    bool subScreenOpen;

    static readonly string[] MenuItems = { "Items", "Magic", "Equipment", "Status", "Order", "Config", "Save" };

    void Awake()
    {
        BuildUI();
        rootPanel.SetActive(false);

        // Create sub-screen components
        itemMenu = gameObject.AddComponent<ItemMenuUI>();
        equipmentMenu = gameObject.AddComponent<EquipmentMenuUI>();
        magicMenu = gameObject.AddComponent<MagicMenuUI>();
        statusMenu = gameObject.AddComponent<StatusMenuUI>();
        orderMenu = gameObject.AddComponent<OrderMenuUI>();
    }

    void Start()
    {
        // Defer sub-screen initialization so their Awake runs first
        itemMenu.Initialize(canvas.transform);
        equipmentMenu.Initialize(canvas.transform);
        magicMenu.Initialize(canvas.transform);
        statusMenu.Initialize(canvas.transform);
        orderMenu.Initialize(canvas.transform);
    }

    void Update()
    {
        var input = GameManager.Instance?.InputManager;
        var state = GameManager.Instance?.StateManager;
        if (input == null || state == null) return;

        // Open menu from exploration
        if (!isOpen && state.CurrentState == GameState.Exploration)
        {
            if (input.MenuAction != null && input.MenuAction.WasPressedThisFrame())
            {
                Open();
                return;
            }
        }

        if (!isOpen) return;

        // If a sub-screen is open, let it handle input
        if (subScreenOpen) return;

        // Close menu
        if (input.CancelAction != null && input.CancelAction.WasPressedThisFrame())
        {
            Close();
            return;
        }

        // Navigation
        if (input.MoveAction != null && input.MoveAction.WasPressedThisFrame())
        {
            Vector2 dir = input.MoveAction.ReadValue<Vector2>();
            if (dir.y > 0.5f)
                NavigateMenu(-1);
            else if (dir.y < -0.5f)
                NavigateMenu(1);
        }

        // Confirm
        if (input.ConfirmAction != null && input.ConfirmAction.WasPressedThisFrame())
        {
            OnMenuConfirm(selectedMenuIndex);
        }
    }

    public void Open()
    {
        isOpen = true;
        subScreenOpen = false;
        GameManager.Instance?.StateManager?.ChangeState(GameState.Menu);
        GameManager.Instance?.InputManager?.EnableUI();
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Confirm);

        RefreshPartyInfo();
        rootPanel.SetActive(true);
        mainPanel.SetActive(true);
        selectedMenuIndex = 0;
        UpdateMenuCursor();
    }

    public void Close()
    {
        isOpen = false;
        rootPanel.SetActive(false);
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cancel);
        GameManager.Instance?.StateManager?.ChangeState(GameState.Exploration);
        GameManager.Instance?.InputManager?.EnableGameplay();
    }

    void NavigateMenu(int direction)
    {
        selectedMenuIndex = (selectedMenuIndex + direction + MenuItems.Length) % MenuItems.Length;
        UpdateMenuCursor();
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cursor);
    }

    void UpdateMenuCursor()
    {
        for (int i = 0; i < menuLabels.Length; i++)
        {
            string prefix = (i == selectedMenuIndex) ? "\u25B6 " : "   ";
            menuLabels[i].text = prefix + MenuItems[i];
        }

        if (menuButtons.Length > selectedMenuIndex)
            EventSystem.current?.SetSelectedGameObject(menuButtons[selectedMenuIndex].gameObject);
    }

    void OnMenuConfirm(int index)
    {
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Confirm);

        switch (index)
        {
            case 0: // Items
                OpenSubScreen(itemMenu);
                break;
            case 1: // Magic
                OpenSubScreen(magicMenu);
                break;
            case 2: // Equipment
                OpenSubScreen(equipmentMenu);
                break;
            case 3: // Status
                OpenSubScreen(statusMenu);
                break;
            case 4: // Order
                OpenSubScreen(orderMenu);
                break;
            case 5: // Config - stub
                Debug.Log("[Menu] Config not yet implemented");
                break;
            case 6: // Save - stub
                Debug.Log("[Menu] Save not yet implemented");
                break;
        }
    }

    void OpenSubScreen(IMenuSubScreen subScreen)
    {
        subScreenOpen = true;
        mainPanel.SetActive(false);
        subScreen.Show(() =>
        {
            subScreenOpen = false;
            mainPanel.SetActive(true);
            RefreshPartyInfo();
            UpdateMenuCursor();
        });
    }

    void RefreshPartyInfo()
    {
        var pm = GameManager.Instance?.PartyManager;
        if (pm == null) return;

        for (int i = 0; i < 4; i++)
        {
            var member = pm.GetMember(i);
            if (member != null)
            {
                string className = member.ClassDef != null ? member.ClassDef.Abbreviation : "??";
                memberInfoTexts[i].text = $"{member.Name}  {className}  Lv {member.Level}  HP {member.CurrentHP}/{member.MaxHP}";
                memberInfoTexts[i].color = member.IsAlive ? Color.white : new Color(0.6f, 0.2f, 0.2f);
            }
            else
            {
                memberInfoTexts[i].text = "(empty)";
                memberInfoTexts[i].color = new Color(0.5f, 0.5f, 0.5f);
            }
        }

        var inv = GameManager.Instance?.InventoryManager;
        int gil = inv != null ? inv.Gil : 0;
        gilText.text = $"Gil: {gil:N0}";

        float playTime = Time.time; // placeholder — would come from save/timer system
        int hours = (int)(playTime / 3600);
        int minutes = (int)((playTime % 3600) / 60);
        int seconds = (int)(playTime % 60);
        timeText.text = $"Time: {hours}:{minutes:D2}:{seconds:D2}";
    }

    // --- UI Building ---

    void BuildUI()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
        }
        if (GetComponent<CanvasScaler>() == null)
        {
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
        }
        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();

        // Root panel (full screen dim overlay)
        rootPanel = new GameObject("MenuRoot");
        var rootRect = rootPanel.AddComponent<RectTransform>();
        rootRect.SetParent(canvas.transform, false);
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.sizeDelta = Vector2.zero;

        var dimImage = rootPanel.AddComponent<Image>();
        dimImage.color = new Color(0, 0, 0, 0.5f);

        // Main panel (the menu window)
        mainPanel = CreateWindow("MainPanel", rootRect);
        var mainRect = mainPanel.GetComponent<RectTransform>();
        mainRect.anchorMin = new Vector2(0.1f, 0.1f);
        mainRect.anchorMax = new Vector2(0.9f, 0.9f);
        mainRect.sizeDelta = Vector2.zero;

        // Left: Menu items
        menuPanel = new GameObject("MenuItems");
        var menuRect = menuPanel.AddComponent<RectTransform>();
        menuRect.SetParent(mainRect, false);
        menuRect.anchorMin = new Vector2(0.02f, 0.05f);
        menuRect.anchorMax = new Vector2(0.35f, 0.95f);
        menuRect.sizeDelta = Vector2.zero;

        var menuLayout = menuPanel.AddComponent<VerticalLayoutGroup>();
        menuLayout.spacing = 6;
        menuLayout.childForceExpandWidth = true;
        menuLayout.childForceExpandHeight = false;
        menuLayout.childControlWidth = true;
        menuLayout.childControlHeight = false;
        menuLayout.padding = new RectOffset(10, 10, 10, 10);

        menuButtons = new Button[MenuItems.Length];
        menuLabels = new TextMeshProUGUI[MenuItems.Length];

        for (int i = 0; i < MenuItems.Length; i++)
        {
            var btnGO = new GameObject($"MenuItem_{MenuItems[i]}");
            var btnRect = btnGO.AddComponent<RectTransform>();
            btnRect.SetParent(menuRect, false);

            var layoutElem = btnGO.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 36;

            var btnImage = btnGO.AddComponent<Image>();
            btnImage.color = new Color(0, 0, 0, 0);

            var btn = btnGO.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = new Color(0, 0, 0, 0);
            colors.highlightedColor = new Color(0.1f, 0.1f, 0.4f, 0.5f);
            colors.selectedColor = new Color(0.15f, 0.15f, 0.5f, 0.5f);
            btn.colors = colors;

            int idx = i;
            btn.onClick.AddListener(() => OnMenuConfirm(idx));
            btn.navigation = new Navigation { mode = Navigation.Mode.None };

            var labelGO = CreateText($"Label_{MenuItems[i]}", "   " + MenuItems[i], 20, TextAlignmentOptions.Left, btnRect);
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;

            menuButtons[i] = btn;
            menuLabels[i] = labelGO.GetComponent<TextMeshProUGUI>();
        }

        // Right: Party info panel
        partyInfoPanel = new GameObject("PartyInfo");
        var partyRect = partyInfoPanel.AddComponent<RectTransform>();
        partyRect.SetParent(mainRect, false);
        partyRect.anchorMin = new Vector2(0.37f, 0.05f);
        partyRect.anchorMax = new Vector2(0.98f, 0.95f);
        partyRect.sizeDelta = Vector2.zero;

        var partyLayout = partyInfoPanel.AddComponent<VerticalLayoutGroup>();
        partyLayout.spacing = 8;
        partyLayout.childForceExpandWidth = true;
        partyLayout.childForceExpandHeight = false;
        partyLayout.childControlWidth = true;
        partyLayout.childControlHeight = false;
        partyLayout.padding = new RectOffset(10, 10, 10, 10);

        for (int i = 0; i < 4; i++)
        {
            var memberGO = CreateText($"MemberInfo_{i}", "(empty)", 18, TextAlignmentOptions.Left, partyRect);
            var memberLayout = memberGO.AddComponent<LayoutElement>();
            memberLayout.preferredHeight = 32;
            memberInfoTexts[i] = memberGO.GetComponent<TextMeshProUGUI>();
        }

        // Spacer
        var spacer = new GameObject("Spacer");
        var spacerRect = spacer.AddComponent<RectTransform>();
        spacerRect.SetParent(partyRect, false);
        var spacerLayout = spacer.AddComponent<LayoutElement>();
        spacerLayout.preferredHeight = 40;
        spacerLayout.flexibleHeight = 1;

        // Gil
        var gilGO = CreateText("GilText", "Gil: 0", 18, TextAlignmentOptions.Left, partyRect);
        var gilLayout = gilGO.AddComponent<LayoutElement>();
        gilLayout.preferredHeight = 28;
        gilText = gilGO.GetComponent<TextMeshProUGUI>();

        // Time
        var timeGO = CreateText("TimeText", "Time: 0:00:00", 18, TextAlignmentOptions.Left, partyRect);
        var timeLayout = timeGO.AddComponent<LayoutElement>();
        timeLayout.preferredHeight = 28;
        timeText = timeGO.GetComponent<TextMeshProUGUI>();
    }

    // --- Helpers ---

    GameObject CreateWindow(string name, Transform parent)
    {
        var go = new GameObject(name);
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        go.AddComponent<UIWindow>();
        return go;
    }

    GameObject CreateText(string name, string text, float fontSize, TextAlignmentOptions alignment, RectTransform parent)
    {
        var go = new GameObject(name);
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = alignment;
        return go;
    }
}

public interface IMenuSubScreen
{
    void Initialize(Transform canvasRoot);
    void Show(Action onClose);
    void Hide();
}
