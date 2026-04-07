using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Collections.Generic;

public class EquipmentMenuUI : MonoBehaviour, IMenuSubScreen
{
    GameObject rootPanel;
    GameObject memberSelectPanel;
    GameObject slotPanel;
    GameObject equipListPanel;
    GameObject statPreviewPanel;

    // Member selection
    Button[] memberButtons = new Button[4];
    TextMeshProUGUI[] memberLabels = new TextMeshProUGUI[4];
    int selectedMemberIndex;

    // Slot selection
    Button[] slotButtons = new Button[4];
    TextMeshProUGUI[] slotLabels = new TextMeshProUGUI[4];
    int selectedSlotIndex;

    // Equipment list
    List<Button> equipButtons = new();
    List<TextMeshProUGUI> equipLabels = new();
    List<EquipmentData> displayedEquipment = new();
    int selectedEquipIndex;
    GameObject equipListContent;

    // Stat preview texts
    TextMeshProUGUI previewATK, previewDEF, previewEVA, previewMDEF;

    // Title
    TextMeshProUGUI titleText;

    Action onCloseCallback;

    enum EquipState { MemberSelect, SlotSelect, EquipSelect }
    EquipState state;

    static readonly string[] SlotNames = { "Weapon", "Shield", "Helmet", "Armor" };

    public void Initialize(Transform canvasRoot)
    {
        BuildUI(canvasRoot);
        rootPanel.SetActive(false);
    }

    public void Show(Action onClose)
    {
        onCloseCallback = onClose;
        state = EquipState.MemberSelect;
        rootPanel.SetActive(true);
        memberSelectPanel.SetActive(true);
        slotPanel.SetActive(false);
        equipListPanel.SetActive(false);
        statPreviewPanel.SetActive(false);
        titleText.text = "Equipment";

        selectedMemberIndex = 0;
        RefreshMemberList();
        UpdateMemberCursor();
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

        switch (state)
        {
            case EquipState.MemberSelect:
                UpdateMemberSelect(input);
                break;
            case EquipState.SlotSelect:
                UpdateSlotSelect(input);
                break;
            case EquipState.EquipSelect:
                UpdateEquipSelect(input);
                break;
        }
    }

    // --- Member Select ---

    void UpdateMemberSelect(InputManager input)
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
            if (dir.y > 0.5f) NavigateMember(-1);
            else if (dir.y < -0.5f) NavigateMember(1);
        }

        if (input.ConfirmAction != null && input.ConfirmAction.WasPressedThisFrame())
        {
            var pm = GameManager.Instance?.PartyManager;
            if (pm?.GetMember(selectedMemberIndex) != null)
            {
                GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Confirm);
                EnterSlotSelect();
            }
        }
    }

    void NavigateMember(int dir)
    {
        selectedMemberIndex = (selectedMemberIndex + dir + 4) % 4;
        UpdateMemberCursor();
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cursor);
    }

    void UpdateMemberCursor()
    {
        var pm = GameManager.Instance?.PartyManager;
        for (int i = 0; i < 4; i++)
        {
            string prefix = (i == selectedMemberIndex) ? "\u25B6 " : "   ";
            var member = pm?.GetMember(i);
            if (member != null)
                memberLabels[i].text = $"{prefix}{member.Name}  {member.ClassDef?.Abbreviation ?? "??"}  Lv {member.Level}";
            else
                memberLabels[i].text = $"{prefix}(empty)";
        }
        if (selectedMemberIndex < memberButtons.Length)
            EventSystem.current?.SetSelectedGameObject(memberButtons[selectedMemberIndex].gameObject);
    }

    void RefreshMemberList()
    {
        UpdateMemberCursor();
    }

    // --- Slot Select ---

    void EnterSlotSelect()
    {
        state = EquipState.SlotSelect;
        memberSelectPanel.SetActive(false);
        slotPanel.SetActive(true);
        statPreviewPanel.SetActive(true);
        equipListPanel.SetActive(false);
        selectedSlotIndex = 0;

        var member = GameManager.Instance?.PartyManager?.GetMember(selectedMemberIndex);
        titleText.text = $"{member?.Name ?? "???"} \u2014 Equipment";

        RefreshSlotDisplay();
        UpdateSlotCursor();
        UpdateStatPreviewForCurrentEquipment();
    }

    void UpdateSlotSelect(InputManager input)
    {
        if (input.CancelAction != null && input.CancelAction.WasPressedThisFrame())
        {
            GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cancel);
            state = EquipState.MemberSelect;
            slotPanel.SetActive(false);
            statPreviewPanel.SetActive(false);
            memberSelectPanel.SetActive(true);
            titleText.text = "Equipment";
            RefreshMemberList();
            UpdateMemberCursor();
            return;
        }

        if (input.MoveAction != null && input.MoveAction.WasPressedThisFrame())
        {
            Vector2 dir = input.MoveAction.ReadValue<Vector2>();
            if (dir.y > 0.5f) NavigateSlot(-1);
            else if (dir.y < -0.5f) NavigateSlot(1);
        }

        if (input.ConfirmAction != null && input.ConfirmAction.WasPressedThisFrame())
        {
            var member = GameManager.Instance?.PartyManager?.GetMember(selectedMemberIndex);
            if (member == null) return;

            // Check if shield slot is blocked by two-handed weapon
            if (selectedSlotIndex == 1 && member.Weapon != null && member.Weapon.TwoHanded)
            {
                GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Error);
                return;
            }

            GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Confirm);
            EnterEquipSelect();
        }
    }

    void NavigateSlot(int dir)
    {
        selectedSlotIndex = (selectedSlotIndex + dir + 4) % 4;
        UpdateSlotCursor();
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cursor);
    }

    void RefreshSlotDisplay()
    {
        var member = GameManager.Instance?.PartyManager?.GetMember(selectedMemberIndex);
        if (member == null) return;

        for (int i = 0; i < 4; i++)
        {
            var slot = (EquipmentSlot)i;
            var equipped = member.GetEquipment(slot);
            string equipName;

            if (i == 1 && member.Weapon != null && member.Weapon.TwoHanded)
                equipName = "\u2014Blocked\u2014";
            else if (equipped != null)
                equipName = equipped.ItemName;
            else
                equipName = "(empty)";

            slotLabels[i].text = $"   {SlotNames[i]}: {equipName}";
        }
    }

    void UpdateSlotCursor()
    {
        var member = GameManager.Instance?.PartyManager?.GetMember(selectedMemberIndex);
        if (member == null) return;

        for (int i = 0; i < 4; i++)
        {
            var slot = (EquipmentSlot)i;
            var equipped = member.GetEquipment(slot);
            string prefix = (i == selectedSlotIndex) ? "\u25B6 " : "   ";
            string equipName;

            if (i == 1 && member.Weapon != null && member.Weapon.TwoHanded)
                equipName = "\u2014Blocked\u2014";
            else if (equipped != null)
                equipName = equipped.ItemName;
            else
                equipName = "(empty)";

            slotLabels[i].text = $"{prefix}{SlotNames[i]}: {equipName}";
        }

        if (selectedSlotIndex < slotButtons.Length)
            EventSystem.current?.SetSelectedGameObject(slotButtons[selectedSlotIndex].gameObject);
    }

    void UpdateStatPreviewForCurrentEquipment()
    {
        var member = GameManager.Instance?.PartyManager?.GetMember(selectedMemberIndex);
        if (member == null) return;

        previewATK.text = $"ATK: {member.Attack}";
        previewDEF.text = $"DEF: {member.Defense}";
        previewEVA.text = $"EVA: {member.Evasion}";
        previewMDEF.text = $"M.DEF: {member.MagicDefense}";
    }

    // --- Equip Select ---

    void EnterEquipSelect()
    {
        state = EquipState.EquipSelect;
        equipListPanel.SetActive(true);
        selectedEquipIndex = 0;

        RefreshEquipList();
        if (displayedEquipment.Count > 0)
            UpdateEquipCursor();
    }

    void UpdateEquipSelect(InputManager input)
    {
        if (input.CancelAction != null && input.CancelAction.WasPressedThisFrame())
        {
            GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cancel);
            state = EquipState.SlotSelect;
            equipListPanel.SetActive(false);
            UpdateStatPreviewForCurrentEquipment();
            UpdateSlotCursor();
            return;
        }

        if (input.MoveAction != null && input.MoveAction.WasPressedThisFrame())
        {
            Vector2 dir = input.MoveAction.ReadValue<Vector2>();
            if (dir.y > 0.5f) NavigateEquip(-1);
            else if (dir.y < -0.5f) NavigateEquip(1);
        }

        if (input.ConfirmAction != null && input.ConfirmAction.WasPressedThisFrame())
        {
            ConfirmEquip();
        }
    }

    void NavigateEquip(int dir)
    {
        if (displayedEquipment.Count == 0) return;
        // +1 for the (Remove) option
        int totalCount = displayedEquipment.Count + 1;
        selectedEquipIndex = (selectedEquipIndex + dir + totalCount) % totalCount;
        UpdateEquipCursor();
        UpdateStatPreviewForSelection();
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cursor);
    }

    void RefreshEquipList()
    {
        // Clear old buttons
        foreach (var btn in equipButtons)
        {
            if (btn != null) Destroy(btn.gameObject);
        }
        equipButtons.Clear();
        equipLabels.Clear();
        displayedEquipment.Clear();

        var inv = GameManager.Instance?.InventoryManager;
        var member = GameManager.Instance?.PartyManager?.GetMember(selectedMemberIndex);
        if (inv == null || member == null) return;

        var slot = (EquipmentSlot)selectedSlotIndex;
        var available = inv.GetEquipmentForSlot(slot);

        foreach (var equip in available)
        {
            bool canEquip = member.CanEquip(equip);
            displayedEquipment.Add(equip);
            CreateEquipButton(equip, canEquip, equipListContent.transform);
        }

        // (Remove) option
        CreateRemoveButton(equipListContent.transform);

        if (displayedEquipment.Count > 0 || true) // always have at least (Remove)
        {
            selectedEquipIndex = 0;
            UpdateEquipCursor();
            UpdateStatPreviewForSelection();
        }
    }

    void CreateEquipButton(EquipmentData equip, bool canEquip, Transform parent)
    {
        var go = new GameObject($"Equip_{equip.ItemName}");
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);

        var layoutElem = go.AddComponent<LayoutElement>();
        layoutElem.preferredHeight = 28;

        var image = go.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0);

        var btn = go.AddComponent<Button>();
        btn.navigation = new Navigation { mode = Navigation.Mode.None };
        btn.interactable = canEquip;

        var labelGO = new GameObject("Label");
        var labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.SetParent(rect, false);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;

        var tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 16;
        tmp.color = canEquip ? Color.white : new Color(0.5f, 0.5f, 0.5f);
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.text = $"   {equip.ItemName}";

        equipButtons.Add(btn);
        equipLabels.Add(tmp);
    }

    void CreateRemoveButton(Transform parent)
    {
        var go = new GameObject("Equip_Remove");
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);

        var layoutElem = go.AddComponent<LayoutElement>();
        layoutElem.preferredHeight = 28;

        var image = go.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0);

        var btn = go.AddComponent<Button>();
        btn.navigation = new Navigation { mode = Navigation.Mode.None };

        var labelGO = new GameObject("Label");
        var labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.SetParent(rect, false);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;

        var tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 16;
        tmp.color = new Color(0.8f, 0.8f, 0.2f);
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.text = "   (Remove)";

        equipButtons.Add(btn);
        equipLabels.Add(tmp);
    }

    void UpdateEquipCursor()
    {
        int totalCount = displayedEquipment.Count + 1; // +1 for remove
        for (int i = 0; i < equipLabels.Count; i++)
        {
            string prefix = (i == selectedEquipIndex) ? "\u25B6 " : "   ";
            if (i < displayedEquipment.Count)
            {
                var equip = displayedEquipment[i];
                var member = GameManager.Instance?.PartyManager?.GetMember(selectedMemberIndex);
                bool canEquip = member != null && member.CanEquip(equip);
                string colorTag = canEquip ? "" : "<color=#888888>";
                string endTag = canEquip ? "" : "</color>";
                equipLabels[i].text = $"{prefix}{colorTag}{equip.ItemName}{endTag}";
            }
            else
            {
                equipLabels[i].text = $"{prefix}<color=#CCCC33>(Remove)</color>";
            }
        }

        if (selectedEquipIndex < equipButtons.Count)
            EventSystem.current?.SetSelectedGameObject(equipButtons[selectedEquipIndex].gameObject);
    }

    void UpdateStatPreviewForSelection()
    {
        var member = GameManager.Instance?.PartyManager?.GetMember(selectedMemberIndex);
        if (member == null) return;

        if (selectedEquipIndex < displayedEquipment.Count)
        {
            var equip = displayedEquipment[selectedEquipIndex];
            if (member.CanEquip(equip))
            {
                var preview = member.PreviewEquip(equip);
                previewATK.text = FormatStatDiff("ATK", preview.OldAttack, preview.NewAttack);
                previewDEF.text = FormatStatDiff("DEF", preview.OldDefense, preview.NewDefense);
                previewEVA.text = FormatStatDiff("EVA", preview.OldEvasion, preview.NewEvasion);
                previewMDEF.text = FormatStatDiff("M.DEF", preview.OldMagicDef, preview.NewMagicDef);
            }
            else
            {
                UpdateStatPreviewForCurrentEquipment();
            }
        }
        else
        {
            // Preview removing current equipment
            var slot = (EquipmentSlot)selectedSlotIndex;
            var current = member.GetEquipment(slot);
            if (current != null)
            {
                var preview = new StatPreview
                {
                    OldAttack = member.Attack, OldDefense = member.Defense,
                    OldEvasion = member.Evasion, OldMagicDef = member.MagicDefense,
                };

                var removed = member.Unequip(slot);
                preview.NewAttack = member.Attack;
                preview.NewDefense = member.Defense;
                preview.NewEvasion = member.Evasion;
                preview.NewMagicDef = member.MagicDefense;

                // Re-equip
                if (removed != null) member.Equip(removed);

                previewATK.text = FormatStatDiff("ATK", preview.OldAttack, preview.NewAttack);
                previewDEF.text = FormatStatDiff("DEF", preview.OldDefense, preview.NewDefense);
                previewEVA.text = FormatStatDiff("EVA", preview.OldEvasion, preview.NewEvasion);
                previewMDEF.text = FormatStatDiff("M.DEF", preview.OldMagicDef, preview.NewMagicDef);
            }
            else
            {
                UpdateStatPreviewForCurrentEquipment();
            }
        }
    }

    string FormatStatDiff(string label, int oldVal, int newVal)
    {
        if (newVal > oldVal)
            return $"{label}: {oldVal} \u2192 <color=#44FF44>{newVal}</color> (+{newVal - oldVal})";
        else if (newVal < oldVal)
            return $"{label}: {oldVal} \u2192 <color=#FF4444>{newVal}</color> ({newVal - oldVal})";
        else
            return $"{label}: {oldVal}";
    }

    void ConfirmEquip()
    {
        var member = GameManager.Instance?.PartyManager?.GetMember(selectedMemberIndex);
        var inv = GameManager.Instance?.InventoryManager;
        if (member == null || inv == null) return;

        var slot = (EquipmentSlot)selectedSlotIndex;

        if (selectedEquipIndex < displayedEquipment.Count)
        {
            // Equip new item
            var newEquip = displayedEquipment[selectedEquipIndex];
            if (!member.CanEquip(newEquip))
            {
                GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Error);
                return;
            }

            // Remove from inventory
            inv.RemoveEquipment(newEquip);

            // Handle shield being removed by two-handed weapon
            if (newEquip.TwoHanded && member.Shield != null)
            {
                inv.AddEquipment(member.Shield);
            }

            // Equip and return old item to inventory
            var oldEquip = member.Equip(newEquip);
            if (oldEquip != null)
                inv.AddEquipment(oldEquip);

            GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Confirm);
        }
        else
        {
            // Remove current equipment
            var removed = member.Unequip(slot);
            if (removed != null)
            {
                inv.AddEquipment(removed);
                GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Confirm);
            }
            else
            {
                GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Error);
                return;
            }
        }

        // Go back to slot select
        state = EquipState.SlotSelect;
        equipListPanel.SetActive(false);
        RefreshSlotDisplay();
        UpdateSlotCursor();
        UpdateStatPreviewForCurrentEquipment();
    }

    // --- UI Building ---

    void BuildUI(Transform canvasRoot)
    {
        rootPanel = new GameObject("EquipMenuRoot");
        var rootRect = rootPanel.AddComponent<RectTransform>();
        rootRect.SetParent(canvasRoot, false);
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.sizeDelta = Vector2.zero;

        // Main window
        var windowGO = CreateWindow("EquipWindow", rootRect);
        var windowRect = windowGO.GetComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.1f, 0.1f);
        windowRect.anchorMax = new Vector2(0.9f, 0.9f);
        windowRect.sizeDelta = Vector2.zero;

        // Title
        var titleGO = CreateText("Title", "Equipment", 22, TextAlignmentOptions.Left, windowRect);
        var titleR = titleGO.GetComponent<RectTransform>();
        titleR.anchorMin = new Vector2(0.02f, 0.9f);
        titleR.anchorMax = new Vector2(0.6f, 0.98f);
        titleR.sizeDelta = Vector2.zero;
        titleText = titleGO.GetComponent<TextMeshProUGUI>();

        // Member select panel
        BuildMemberSelectPanel(windowRect);

        // Slot panel (left side)
        BuildSlotPanel(windowRect);

        // Stat preview panel (right side)
        BuildStatPreviewPanel(windowRect);

        // Equipment list panel (overlays slot area)
        BuildEquipListPanel(windowRect);
    }

    void BuildMemberSelectPanel(RectTransform parent)
    {
        memberSelectPanel = new GameObject("MemberSelect");
        var rect = memberSelectPanel.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.02f, 0.15f);
        rect.anchorMax = new Vector2(0.98f, 0.88f);
        rect.sizeDelta = Vector2.zero;

        var layout = memberSelectPanel.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.padding = new RectOffset(10, 10, 10, 10);

        for (int i = 0; i < 4; i++)
        {
            var btnGO = new GameObject($"Member_{i}");
            var btnRect = btnGO.AddComponent<RectTransform>();
            btnRect.SetParent(rect, false);

            var le = btnGO.AddComponent<LayoutElement>();
            le.preferredHeight = 36;

            var image = btnGO.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0);

            var btn = btnGO.AddComponent<Button>();
            btn.navigation = new Navigation { mode = Navigation.Mode.None };

            var labelGO = CreateText($"MemberLabel_{i}", "(empty)", 18, TextAlignmentOptions.Left, btnRect);
            var labelR = labelGO.GetComponent<RectTransform>();
            labelR.anchorMin = Vector2.zero;
            labelR.anchorMax = Vector2.one;
            labelR.sizeDelta = Vector2.zero;

            memberButtons[i] = btn;
            memberLabels[i] = labelGO.GetComponent<TextMeshProUGUI>();
        }
    }

    void BuildSlotPanel(RectTransform parent)
    {
        slotPanel = new GameObject("SlotPanel");
        var rect = slotPanel.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.02f, 0.3f);
        rect.anchorMax = new Vector2(0.5f, 0.88f);
        rect.sizeDelta = Vector2.zero;

        var layout = slotPanel.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 6;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.padding = new RectOffset(10, 10, 10, 10);

        for (int i = 0; i < 4; i++)
        {
            var btnGO = new GameObject($"Slot_{SlotNames[i]}");
            var btnRect = btnGO.AddComponent<RectTransform>();
            btnRect.SetParent(rect, false);

            var le = btnGO.AddComponent<LayoutElement>();
            le.preferredHeight = 32;

            var image = btnGO.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0);

            var btn = btnGO.AddComponent<Button>();
            btn.navigation = new Navigation { mode = Navigation.Mode.None };

            var labelGO = CreateText($"SlotLabel_{i}", $"   {SlotNames[i]}: (empty)", 18, TextAlignmentOptions.Left, btnRect);
            var labelR = labelGO.GetComponent<RectTransform>();
            labelR.anchorMin = Vector2.zero;
            labelR.anchorMax = Vector2.one;
            labelR.sizeDelta = Vector2.zero;

            slotButtons[i] = btn;
            slotLabels[i] = labelGO.GetComponent<TextMeshProUGUI>();
        }

        slotPanel.SetActive(false);
    }

    void BuildStatPreviewPanel(RectTransform parent)
    {
        statPreviewPanel = new GameObject("StatPreview");
        var rect = statPreviewPanel.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.52f, 0.3f);
        rect.anchorMax = new Vector2(0.98f, 0.88f);
        rect.sizeDelta = Vector2.zero;

        var layout = statPreviewPanel.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.padding = new RectOffset(10, 10, 10, 10);

        previewATK = CreateStatLabel("ATK", "ATK: 0", layout.transform);
        previewDEF = CreateStatLabel("DEF", "DEF: 0", layout.transform);
        previewEVA = CreateStatLabel("EVA", "EVA: 0", layout.transform);
        previewMDEF = CreateStatLabel("MDEF", "M.DEF: 0", layout.transform);

        statPreviewPanel.SetActive(false);
    }

    TextMeshProUGUI CreateStatLabel(string name, string defaultText, Transform parent)
    {
        var go = CreateText(name, defaultText, 18, TextAlignmentOptions.Left, (RectTransform)parent);
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 30;
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.richText = true;
        return tmp;
    }

    void BuildEquipListPanel(RectTransform parent)
    {
        equipListPanel = CreateWindow("EquipListPanel", parent);
        var rect = equipListPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.02f, 0.02f);
        rect.anchorMax = new Vector2(0.5f, 0.28f);
        rect.sizeDelta = Vector2.zero;

        // Scroll area
        var scrollGO = new GameObject("EquipScroll");
        var scrollRect = scrollGO.AddComponent<RectTransform>();
        scrollRect.SetParent(rect, false);
        scrollRect.anchorMin = new Vector2(0.02f, 0.02f);
        scrollRect.anchorMax = new Vector2(0.98f, 0.98f);
        scrollRect.sizeDelta = Vector2.zero;

        var scrollView = scrollGO.AddComponent<ScrollRect>();

        var contentGO = new GameObject("Content");
        var contentRect = contentGO.AddComponent<RectTransform>();
        contentRect.SetParent(scrollRect, false);
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0, 1);
        contentRect.sizeDelta = new Vector2(0, 0);

        var contentLayout = contentGO.AddComponent<VerticalLayoutGroup>();
        contentLayout.spacing = 2;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = false;
        contentLayout.padding = new RectOffset(5, 5, 5, 5);

        var fitter = contentGO.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollView.content = contentRect;
        scrollView.vertical = true;
        scrollView.horizontal = false;
        scrollView.movementType = ScrollRect.MovementType.Clamped;

        equipListContent = contentGO;
        equipListPanel.SetActive(false);
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
