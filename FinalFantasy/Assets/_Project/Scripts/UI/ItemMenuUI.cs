using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Collections.Generic;

public class ItemMenuUI : MonoBehaviour, IMenuSubScreen
{
    GameObject rootPanel;
    GameObject itemListPanel;
    GameObject targetPanel;

    // Item list
    List<Button> itemButtons = new();
    List<TextMeshProUGUI> itemLabels = new();
    List<InventorySlot> displayedItems = new();
    int selectedItemIndex;

    // Target selection
    Button[] targetButtons = new Button[4];
    TextMeshProUGUI[] targetLabels = new TextMeshProUGUI[4];
    int selectedTargetIndex;

    // Description
    TextMeshProUGUI descriptionText;

    // State
    Action onCloseCallback;
    bool isShowingTargets;
    InventorySlot selectedSlot;

    enum ItemMenuState { ItemList, TargetSelect }
    ItemMenuState state;

    public void Initialize(Transform canvasRoot)
    {
        BuildUI(canvasRoot);
        rootPanel.SetActive(false);
    }

    public void Show(Action onClose)
    {
        onCloseCallback = onClose;
        state = ItemMenuState.ItemList;
        isShowingTargets = false;
        rootPanel.SetActive(true);
        targetPanel.SetActive(false);
        RefreshItemList();
    }

    public void Hide()
    {
        rootPanel.SetActive(false);
        onCloseCallback?.Invoke();
    }

    void Update()
    {
        if (rootPanel == null || !rootPanel.activeSelf) return;

        var input = GameManager.Instance?.InputManager;
        if (input == null) return;

        if (state == ItemMenuState.ItemList)
            UpdateItemList(input);
        else if (state == ItemMenuState.TargetSelect)
            UpdateTargetSelect(input);
    }

    void UpdateItemList(InputManager input)
    {
        if (input.CancelAction != null && input.CancelAction.WasPressedThisFrame())
        {
            GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cancel);
            Hide();
            return;
        }

        if (input.MoveAction != null && input.MoveAction.WasPressedThisFrame())
        {
            Vector2 dir = input.MoveAction.ReadValue<Vector2>();
            if (dir.y > 0.5f)
                NavigateItems(-1);
            else if (dir.y < -0.5f)
                NavigateItems(1);
        }

        if (input.ConfirmAction != null && input.ConfirmAction.WasPressedThisFrame())
        {
            if (displayedItems.Count > 0 && selectedItemIndex < displayedItems.Count)
            {
                var slot = displayedItems[selectedItemIndex];
                if (slot.Item.UsableInField)
                {
                    selectedSlot = slot;
                    ShowTargetSelection();
                }
                else
                {
                    GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Error);
                }
            }
        }
    }

    void UpdateTargetSelect(InputManager input)
    {
        if (input.CancelAction != null && input.CancelAction.WasPressedThisFrame())
        {
            GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cancel);
            HideTargetSelection();
            return;
        }

        if (input.MoveAction != null && input.MoveAction.WasPressedThisFrame())
        {
            Vector2 dir = input.MoveAction.ReadValue<Vector2>();
            if (dir.y > 0.5f)
                NavigateTargets(-1);
            else if (dir.y < -0.5f)
                NavigateTargets(1);
        }

        if (input.ConfirmAction != null && input.ConfirmAction.WasPressedThisFrame())
        {
            UseItemOnTarget(selectedTargetIndex);
        }
    }

    void NavigateItems(int direction)
    {
        if (displayedItems.Count == 0) return;
        selectedItemIndex = (selectedItemIndex + direction + displayedItems.Count) % displayedItems.Count;
        UpdateItemCursor();
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cursor);
    }

    void NavigateTargets(int direction)
    {
        int count = 4;
        selectedTargetIndex = (selectedTargetIndex + direction + count) % count;
        UpdateTargetCursor();
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cursor);
    }

    void UpdateItemCursor()
    {
        for (int i = 0; i < itemLabels.Count; i++)
        {
            var slot = displayedItems[i];
            string prefix = (i == selectedItemIndex) ? "> " : "   ";
            string usable = slot.Item.UsableInField ? "" : " <color=#888888>(battle only)</color>";
            itemLabels[i].text = $"{prefix}{slot.Item.ItemName}  x{slot.Count}{usable}";
        }

        // Update description
        if (selectedItemIndex < displayedItems.Count)
            descriptionText.text = displayedItems[selectedItemIndex].Item.Description ?? "";
        else
            descriptionText.text = "";

        if (selectedItemIndex < itemButtons.Count)
            EventSystem.current?.SetSelectedGameObject(itemButtons[selectedItemIndex].gameObject);
    }

    void UpdateTargetCursor()
    {
        var pm = GameManager.Instance?.PartyManager;
        for (int i = 0; i < 4; i++)
        {
            var member = pm?.GetMember(i);
            string prefix = (i == selectedTargetIndex) ? "> " : "   ";
            if (member != null)
                targetLabels[i].text = $"{prefix}{member.Name}  HP {member.CurrentHP}/{member.MaxHP}";
            else
                targetLabels[i].text = $"{prefix}(empty)";
        }

        if (selectedTargetIndex < targetButtons.Length)
            EventSystem.current?.SetSelectedGameObject(targetButtons[selectedTargetIndex].gameObject);
    }

    void ShowTargetSelection()
    {
        state = ItemMenuState.TargetSelect;
        isShowingTargets = true;
        targetPanel.SetActive(true);
        selectedTargetIndex = 0;

        RefreshTargetList();
        UpdateTargetCursor();
    }

    void HideTargetSelection()
    {
        state = ItemMenuState.ItemList;
        isShowingTargets = false;
        targetPanel.SetActive(false);
        UpdateItemCursor();
    }

    void RefreshTargetList()
    {
        var pm = GameManager.Instance?.PartyManager;
        for (int i = 0; i < 4; i++)
        {
            var member = pm?.GetMember(i);
            if (member != null)
                targetLabels[i].text = $"   {member.Name}  HP {member.CurrentHP}/{member.MaxHP}";
            else
                targetLabels[i].text = "   (empty)";
        }
    }

    void UseItemOnTarget(int targetIndex)
    {
        var pm = GameManager.Instance?.PartyManager;
        var inv = GameManager.Instance?.InventoryManager;
        if (pm == null || inv == null || selectedSlot == null) return;

        var member = pm.GetMember(targetIndex);
        if (member == null)
        {
            GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Error);
            return;
        }

        // Use the item
        inv.UseItemInField(selectedSlot.Item, member);
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Heal);

        // Refresh UI
        RefreshTargetList();
        UpdateTargetCursor();

        // Check if item ran out
        int remaining = inv.GetItemCount(selectedSlot.Item);
        if (remaining <= 0)
        {
            HideTargetSelection();
            RefreshItemList();
        }
    }

    void RefreshItemList()
    {
        var inv = GameManager.Instance?.InventoryManager;

        // Clear old items
        foreach (var btn in itemButtons)
        {
            if (btn != null) Destroy(btn.gameObject);
        }
        itemButtons.Clear();
        itemLabels.Clear();
        displayedItems.Clear();

        if (inv == null)
        {
            descriptionText.text = "No inventory available";
            return;
        }

        var allItems = inv.GetAllItems();
        foreach (var slot in allItems)
        {
            displayedItems.Add(slot);
            CreateItemButton(slot, itemListPanel.transform);
        }

        if (displayedItems.Count > 0)
        {
            selectedItemIndex = Mathf.Clamp(selectedItemIndex, 0, displayedItems.Count - 1);
            UpdateItemCursor();
        }
        else
        {
            descriptionText.text = "No items";
        }
    }

    void CreateItemButton(InventorySlot slot, Transform parent)
    {
        var go = new GameObject($"Item_{slot.Item.ItemName}");
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);

        var layoutElem = go.AddComponent<LayoutElement>();
        layoutElem.preferredHeight = 32;

        var image = go.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0);

        var btn = go.AddComponent<Button>();
        btn.navigation = new Navigation { mode = Navigation.Mode.None };
        var colors = btn.colors;
        colors.normalColor = new Color(0, 0, 0, 0);
        colors.highlightedColor = new Color(0.1f, 0.1f, 0.4f, 0.5f);
        colors.selectedColor = new Color(0.15f, 0.15f, 0.5f, 0.5f);
        btn.colors = colors;

        var labelGO = new GameObject("Label");
        var labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.SetParent(rect, false);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;

        var tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 18;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.richText = true;

        string usable = slot.Item.UsableInField ? "" : " <color=#888888>(battle only)</color>";
        tmp.text = $"   {slot.Item.ItemName}  x{slot.Count}{usable}";

        itemButtons.Add(btn);
        itemLabels.Add(tmp);
    }

    // --- UI Building ---

    void BuildUI(Transform canvasRoot)
    {
        rootPanel = new GameObject("ItemMenuRoot");
        var rootRect = rootPanel.AddComponent<RectTransform>();
        rootRect.SetParent(canvasRoot, false);
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.sizeDelta = Vector2.zero;

        // Main window
        var windowGO = CreateWindow("ItemWindow", rootRect);
        var windowRect = windowGO.GetComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.1f, 0.1f);
        windowRect.anchorMax = new Vector2(0.9f, 0.9f);
        windowRect.sizeDelta = Vector2.zero;

        // Title
        var titleGO = CreateText("Title", "Items", 22, TextAlignmentOptions.Left, windowRect);
        var titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.02f, 0.9f);
        titleRect.anchorMax = new Vector2(0.5f, 0.98f);
        titleRect.sizeDelta = Vector2.zero;

        // Item list area with scroll
        var scrollGO = new GameObject("ItemScroll");
        var scrollRect = scrollGO.AddComponent<RectTransform>();
        scrollRect.SetParent(windowRect, false);
        scrollRect.anchorMin = new Vector2(0.02f, 0.12f);
        scrollRect.anchorMax = new Vector2(0.98f, 0.88f);
        scrollRect.sizeDelta = Vector2.zero;

        var scrollView = scrollGO.AddComponent<ScrollRect>();

        // Content
        var contentGO = new GameObject("Content");
        var contentRect = contentGO.AddComponent<RectTransform>();
        contentRect.SetParent(scrollRect, false);
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0, 1);
        contentRect.sizeDelta = new Vector2(0, 0);

        var contentLayout = contentGO.AddComponent<VerticalLayoutGroup>();
        contentLayout.spacing = 4;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = false;
        contentLayout.padding = new RectOffset(5, 5, 5, 5);

        var contentFitter = contentGO.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollView.content = contentRect;
        scrollView.vertical = true;
        scrollView.horizontal = false;
        scrollView.movementType = ScrollRect.MovementType.Clamped;
        scrollView.scrollSensitivity = 30;

        itemListPanel = contentGO;

        // Description bar at bottom
        var descGO = CreateText("Description", "", 16, TextAlignmentOptions.Left, windowRect);
        var descRect = descGO.GetComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0.02f, 0.02f);
        descRect.anchorMax = new Vector2(0.98f, 0.1f);
        descRect.sizeDelta = Vector2.zero;
        descriptionText = descGO.GetComponent<TextMeshProUGUI>();
        descriptionText.fontStyle = FontStyles.Italic;

        // Target selection panel (hidden by default)
        BuildTargetPanel(windowRect);
    }

    void BuildTargetPanel(RectTransform parent)
    {
        targetPanel = CreateWindow("TargetPanel", parent);
        var targetRect = targetPanel.GetComponent<RectTransform>();
        targetRect.anchorMin = new Vector2(0.3f, 0.3f);
        targetRect.anchorMax = new Vector2(0.7f, 0.7f);
        targetRect.sizeDelta = Vector2.zero;

        // Title
        var titleGO = CreateText("TargetTitle", "Use on", 18, TextAlignmentOptions.Center, targetRect);
        var titleR = titleGO.GetComponent<RectTransform>();
        titleR.anchorMin = new Vector2(0, 0.85f);
        titleR.anchorMax = new Vector2(1, 1f);
        titleR.sizeDelta = Vector2.zero;

        // Targets
        var listArea = new GameObject("TargetList");
        var listRect = listArea.AddComponent<RectTransform>();
        listRect.SetParent(targetRect, false);
        listRect.anchorMin = new Vector2(0.05f, 0.05f);
        listRect.anchorMax = new Vector2(0.95f, 0.85f);
        listRect.sizeDelta = Vector2.zero;

        var listLayout = listArea.AddComponent<VerticalLayoutGroup>();
        listLayout.spacing = 4;
        listLayout.childForceExpandWidth = true;
        listLayout.childForceExpandHeight = true;
        listLayout.childControlWidth = true;
        listLayout.childControlHeight = true;
        listLayout.padding = new RectOffset(5, 5, 5, 5);

        for (int i = 0; i < 4; i++)
        {
            var btnGO = new GameObject($"Target_{i}");
            var btnRect = btnGO.AddComponent<RectTransform>();
            btnRect.SetParent(listRect, false);

            var btnImage = btnGO.AddComponent<Image>();
            btnImage.color = new Color(0, 0, 0, 0);

            var btn = btnGO.AddComponent<Button>();
            btn.navigation = new Navigation { mode = Navigation.Mode.None };

            var labelGO = CreateText($"TargetLabel_{i}", "(empty)", 16, TextAlignmentOptions.Left, btnRect);
            var labelR = labelGO.GetComponent<RectTransform>();
            labelR.anchorMin = Vector2.zero;
            labelR.anchorMax = Vector2.one;
            labelR.sizeDelta = Vector2.zero;

            targetButtons[i] = btn;
            targetLabels[i] = labelGO.GetComponent<TextMeshProUGUI>();
        }

        targetPanel.SetActive(false);
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
