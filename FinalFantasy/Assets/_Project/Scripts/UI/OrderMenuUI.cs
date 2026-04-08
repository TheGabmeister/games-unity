using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class OrderMenuUI : MonoBehaviour, IMenuSubScreen
{
    GameObject rootPanel;

    Button[] memberButtons = new Button[4];
    TextMeshProUGUI[] memberLabels = new TextMeshProUGUI[4];
    TextMeshProUGUI instructionText;

    int selectedIndex;
    int firstSwapIndex = -1;
    Action onCloseCallback;

    public void Initialize(Transform canvasRoot)
    {
        BuildUI(canvasRoot);
        rootPanel.SetActive(false);
    }

    public void Show(Action onClose)
    {
        onCloseCallback = onClose;
        rootPanel.SetActive(true);
        selectedIndex = 0;
        firstSwapIndex = -1;
        instructionText.text = "Select a member, then select swap target";
        RefreshDisplay();
        UpdateCursor();
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

        if (input.CancelAction != null && input.CancelAction.WasPressedThisFrame())
        {
            if (firstSwapIndex >= 0)
            {
                // Deselect first member
                firstSwapIndex = -1;
                instructionText.text = "Select a member, then select swap target";
                UpdateCursor();
                GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cancel);
            }
            else
            {
                GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cancel);
                Hide();
            }
            return;
        }

        if (input.MoveAction != null && input.MoveAction.WasPressedThisFrame())
        {
            Vector2 dir = input.MoveAction.ReadValue<Vector2>();
            if (dir.y > 0.5f) Navigate(-1);
            else if (dir.y < -0.5f) Navigate(1);
        }

        if (input.ConfirmAction != null && input.ConfirmAction.WasPressedThisFrame())
        {
            OnConfirm();
        }
    }

    void Navigate(int direction)
    {
        selectedIndex = (selectedIndex + direction + 4) % 4;
        UpdateCursor();
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cursor);
    }

    void OnConfirm()
    {
        var pm = GameManager.Instance?.PartyManager;
        if (pm == null) return;

        if (pm.GetMember(selectedIndex) == null)
        {
            GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Error);
            return;
        }

        if (firstSwapIndex < 0)
        {
            // Select first member
            firstSwapIndex = selectedIndex;
            instructionText.text = $"Swap {pm.GetMember(firstSwapIndex)?.Name ?? "???"} with...";
            UpdateCursor();
            GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Confirm);
        }
        else
        {
            // Perform swap
            if (selectedIndex != firstSwapIndex)
            {
                pm.SwapMembers(firstSwapIndex, selectedIndex);
                GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Confirm);
            }
            else
            {
                GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cancel);
            }

            firstSwapIndex = -1;
            instructionText.text = "Select a member, then select swap target";
            RefreshDisplay();
            UpdateCursor();
        }
    }

    void RefreshDisplay()
    {
        var pm = GameManager.Instance?.PartyManager;
        for (int i = 0; i < 4; i++)
        {
            var member = pm?.GetMember(i);
            if (member != null)
                memberLabels[i].text = $"   {i + 1}. {member.Name}  Lv {member.Level}";
            else
                memberLabels[i].text = $"   {i + 1}. (empty)";
        }
    }

    void UpdateCursor()
    {
        var pm = GameManager.Instance?.PartyManager;
        for (int i = 0; i < 4; i++)
        {
            var member = pm?.GetMember(i);
            string prefix;

            if (i == firstSwapIndex && i == selectedIndex)
                prefix = ">*";
            else if (i == firstSwapIndex)
                prefix = "  *";
            else if (i == selectedIndex)
                prefix = "> ";
            else
                prefix = "   ";

            if (member != null)
            {
                memberLabels[i].text = $"{prefix}{i + 1}. {member.Name}  Lv {member.Level}";
                memberLabels[i].color = (i == firstSwapIndex) ? new Color(1f, 1f, 0.4f) : Color.white;
            }
            else
            {
                memberLabels[i].text = $"{prefix}{i + 1}. (empty)";
                memberLabels[i].color = new Color(0.5f, 0.5f, 0.5f);
            }
        }

        if (selectedIndex < memberButtons.Length)
            EventSystem.current?.SetSelectedGameObject(memberButtons[selectedIndex].gameObject);
    }

    // --- UI Building ---

    void BuildUI(Transform canvasRoot)
    {
        rootPanel = new GameObject("OrderMenuRoot");
        var rootRect = rootPanel.AddComponent<RectTransform>();
        rootRect.SetParent(canvasRoot, false);
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.sizeDelta = Vector2.zero;

        var windowGO = CreateWindow("OrderWindow", rootRect);
        var windowRect = windowGO.GetComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.15f, 0.15f);
        windowRect.anchorMax = new Vector2(0.85f, 0.85f);
        windowRect.sizeDelta = Vector2.zero;

        // Title
        var titleGO = CreateText("Title", "Order", 22, TextAlignmentOptions.Left, windowRect);
        var titleR = titleGO.GetComponent<RectTransform>();
        titleR.anchorMin = new Vector2(0.03f, 0.88f);
        titleR.anchorMax = new Vector2(0.5f, 0.98f);
        titleR.sizeDelta = Vector2.zero;

        // Member list
        var listArea = new GameObject("MemberList");
        var listRect = listArea.AddComponent<RectTransform>();
        listRect.SetParent(windowRect, false);
        listRect.anchorMin = new Vector2(0.03f, 0.25f);
        listRect.anchorMax = new Vector2(0.97f, 0.86f);
        listRect.sizeDelta = Vector2.zero;

        var layout = listArea.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.padding = new RectOffset(10, 10, 10, 10);

        for (int i = 0; i < 4; i++)
        {
            var btnGO = new GameObject($"Member_{i}");
            var btnRect = btnGO.AddComponent<RectTransform>();
            btnRect.SetParent(listRect, false);

            var image = btnGO.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0);

            var btn = btnGO.AddComponent<Button>();
            btn.navigation = new Navigation { mode = Navigation.Mode.None };

            var labelGO = CreateText($"MemberLabel_{i}", $"   {i + 1}. (empty)", 18, TextAlignmentOptions.Left, btnRect);
            var labelR = labelGO.GetComponent<RectTransform>();
            labelR.anchorMin = Vector2.zero;
            labelR.anchorMax = Vector2.one;
            labelR.sizeDelta = Vector2.zero;

            memberButtons[i] = btn;
            memberLabels[i] = labelGO.GetComponent<TextMeshProUGUI>();
        }

        // Instruction text
        var instrGO = CreateText("Instruction", "Select a member, then select swap target", 16, TextAlignmentOptions.Center, windowRect);
        var instrRect = instrGO.GetComponent<RectTransform>();
        instrRect.anchorMin = new Vector2(0.03f, 0.05f);
        instrRect.anchorMax = new Vector2(0.97f, 0.2f);
        instrRect.sizeDelta = Vector2.zero;
        instructionText = instrGO.GetComponent<TextMeshProUGUI>();
        instructionText.color = new Color(0.7f, 0.7f, 0.7f);
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
