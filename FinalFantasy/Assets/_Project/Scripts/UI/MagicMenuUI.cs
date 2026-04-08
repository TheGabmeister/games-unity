using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Collections.Generic;

public class MagicMenuUI : MonoBehaviour, IMenuSubScreen
{
    GameObject rootPanel;
    GameObject memberSelectPanel;
    GameObject spellListPanel;
    GameObject targetPanel;

    // Member selection
    Button[] memberButtons = new Button[4];
    TextMeshProUGUI[] memberLabels = new TextMeshProUGUI[4];
    int selectedMemberIndex;

    // Spell list
    List<Button> spellButtons = new();
    List<TextMeshProUGUI> spellLabels = new();
    List<SpellData> displayedSpells = new();
    int selectedSpellIndex;
    GameObject spellListContent;

    // Target selection
    Button[] targetButtons = new Button[4];
    TextMeshProUGUI[] targetLabels = new TextMeshProUGUI[4];
    int selectedTargetIndex;

    // Info texts
    TextMeshProUGUI titleText;
    TextMeshProUGUI mpText;
    TextMeshProUGUI noSpellsText;

    Action onCloseCallback;

    enum MagicState { MemberSelect, SpellList, TargetSelect }
    MagicState state;

    public void Initialize(Transform canvasRoot)
    {
        BuildUI(canvasRoot);
        rootPanel.SetActive(false);
    }

    public void Show(Action onClose)
    {
        onCloseCallback = onClose;
        state = MagicState.MemberSelect;
        rootPanel.SetActive(true);
        memberSelectPanel.SetActive(true);
        spellListPanel.SetActive(false);
        targetPanel.SetActive(false);
        titleText.text = "Magic";

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
            case MagicState.MemberSelect:
                UpdateMemberSelect(input);
                break;
            case MagicState.SpellList:
                UpdateSpellList(input);
                break;
            case MagicState.TargetSelect:
                UpdateTargetSelect(input);
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
            var member = pm?.GetMember(selectedMemberIndex);
            if (member != null)
            {
                GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Confirm);
                EnterSpellList();
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
            string prefix = (i == selectedMemberIndex) ? "> " : "   ";
            var member = pm?.GetMember(i);
            if (member != null)
                memberLabels[i].text = $"{prefix}{member.Name}  MP {member.CurrentMP}/{member.MaxMP}";
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

    // --- Spell List ---

    void EnterSpellList()
    {
        state = MagicState.SpellList;
        memberSelectPanel.SetActive(false);
        spellListPanel.SetActive(true);
        targetPanel.SetActive(false);

        var member = GameManager.Instance?.PartyManager?.GetMember(selectedMemberIndex);
        titleText.text = $"Magic - {member?.Name ?? "???"}";

        RefreshSpellList();
        selectedSpellIndex = 0;

        if (displayedSpells.Count > 0)
        {
            noSpellsText.gameObject.SetActive(false);
            UpdateSpellCursor();
        }
        else
        {
            noSpellsText.gameObject.SetActive(true);
            noSpellsText.text = "No spells known";
        }

        UpdateMPDisplay();
    }

    void UpdateSpellList(InputManager input)
    {
        if (input.CancelAction != null && input.CancelAction.WasPressedThisFrame())
        {
            GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cancel);
            state = MagicState.MemberSelect;
            spellListPanel.SetActive(false);
            memberSelectPanel.SetActive(true);
            titleText.text = "Magic";
            RefreshMemberList();
            UpdateMemberCursor();
            return;
        }

        if (displayedSpells.Count == 0) return;

        if (input.MoveAction != null && input.MoveAction.WasPressedThisFrame())
        {
            Vector2 dir = input.MoveAction.ReadValue<Vector2>();
            if (dir.y > 0.5f) NavigateSpell(-1);
            else if (dir.y < -0.5f) NavigateSpell(1);
        }

        if (input.ConfirmAction != null && input.ConfirmAction.WasPressedThisFrame())
        {
            if (selectedSpellIndex < displayedSpells.Count)
            {
                var spell = displayedSpells[selectedSpellIndex];
                if (spell.UsableInField)
                {
                    var member = GameManager.Instance?.PartyManager?.GetMember(selectedMemberIndex);
                    if (member != null && member.CurrentMP >= spell.MPCost)
                    {
                        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Confirm);
                        EnterTargetSelect();
                    }
                    else
                    {
                        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Error);
                    }
                }
                else
                {
                    GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Error);
                }
            }
        }
    }

    void NavigateSpell(int dir)
    {
        if (displayedSpells.Count == 0) return;
        selectedSpellIndex = (selectedSpellIndex + dir + displayedSpells.Count) % displayedSpells.Count;
        UpdateSpellCursor();
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cursor);
    }

    void RefreshSpellList()
    {
        foreach (var btn in spellButtons)
        {
            if (btn != null) Destroy(btn.gameObject);
        }
        spellButtons.Clear();
        spellLabels.Clear();
        displayedSpells.Clear();

        var member = GameManager.Instance?.PartyManager?.GetMember(selectedMemberIndex);
        if (member == null || member.KnownSpells == null) return;

        foreach (var spell in member.KnownSpells)
        {
            displayedSpells.Add(spell);
            CreateSpellButton(spell, member, spellListContent.transform);
        }
    }

    void CreateSpellButton(SpellData spell, PartyMember caster, Transform parent)
    {
        var go = new GameObject($"Spell_{spell.SpellName}");
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 30;

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
        tmp.fontSize = 17;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.richText = true;

        string fieldTag = spell.UsableInField ? " (field)" : "";
        bool canCast = caster.CurrentMP >= spell.MPCost;
        tmp.color = canCast ? Color.white : new Color(0.5f, 0.5f, 0.5f);
        tmp.text = $"   {spell.SpellName}  Lv{spell.Level}  {spell.MPCost} MP{fieldTag}";

        spellButtons.Add(btn);
        spellLabels.Add(tmp);
    }

    void UpdateSpellCursor()
    {
        var member = GameManager.Instance?.PartyManager?.GetMember(selectedMemberIndex);

        for (int i = 0; i < spellLabels.Count; i++)
        {
            var spell = displayedSpells[i];
            string prefix = (i == selectedSpellIndex) ? "> " : "   ";
            string fieldTag = spell.UsableInField ? " (field)" : "";
            bool canCast = member != null && member.CurrentMP >= spell.MPCost;
            spellLabels[i].color = canCast ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            spellLabels[i].text = $"{prefix}{spell.SpellName}  Lv{spell.Level}  {spell.MPCost} MP{fieldTag}";
        }

        if (selectedSpellIndex < spellButtons.Count)
            EventSystem.current?.SetSelectedGameObject(spellButtons[selectedSpellIndex].gameObject);
    }

    void UpdateMPDisplay()
    {
        var member = GameManager.Instance?.PartyManager?.GetMember(selectedMemberIndex);
        if (member != null)
            mpText.text = $"MP: {member.CurrentMP}/{member.MaxMP}";
        else
            mpText.text = "MP: 0/0";
    }

    // --- Target Select ---

    void EnterTargetSelect()
    {
        state = MagicState.TargetSelect;
        targetPanel.SetActive(true);
        selectedTargetIndex = 0;
        RefreshTargetList();
        UpdateTargetCursor();
    }

    void UpdateTargetSelect(InputManager input)
    {
        if (input.CancelAction != null && input.CancelAction.WasPressedThisFrame())
        {
            GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cancel);
            state = MagicState.SpellList;
            targetPanel.SetActive(false);
            UpdateSpellCursor();
            return;
        }

        if (input.MoveAction != null && input.MoveAction.WasPressedThisFrame())
        {
            Vector2 dir = input.MoveAction.ReadValue<Vector2>();
            if (dir.y > 0.5f) NavigateTarget(-1);
            else if (dir.y < -0.5f) NavigateTarget(1);
        }

        if (input.ConfirmAction != null && input.ConfirmAction.WasPressedThisFrame())
        {
            CastSpellOnTarget(selectedTargetIndex);
        }
    }

    void NavigateTarget(int dir)
    {
        selectedTargetIndex = (selectedTargetIndex + dir + 4) % 4;
        UpdateTargetCursor();
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cursor);
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

    void UpdateTargetCursor()
    {
        var pm = GameManager.Instance?.PartyManager;
        for (int i = 0; i < 4; i++)
        {
            string prefix = (i == selectedTargetIndex) ? "> " : "   ";
            var member = pm?.GetMember(i);
            if (member != null)
                targetLabels[i].text = $"{prefix}{member.Name}  HP {member.CurrentHP}/{member.MaxHP}";
            else
                targetLabels[i].text = $"{prefix}(empty)";
        }
        if (selectedTargetIndex < targetButtons.Length)
            EventSystem.current?.SetSelectedGameObject(targetButtons[selectedTargetIndex].gameObject);
    }

    void CastSpellOnTarget(int targetIndex)
    {
        var pm = GameManager.Instance?.PartyManager;
        var caster = pm?.GetMember(selectedMemberIndex);
        var target = pm?.GetMember(targetIndex);
        if (caster == null || target == null) return;

        if (selectedSpellIndex >= displayedSpells.Count) return;
        var spell = displayedSpells[selectedSpellIndex];

        if (caster.CurrentMP < spell.MPCost)
        {
            GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Error);
            return;
        }

        // Deduct MP
        caster.CurrentMP -= spell.MPCost;

        // Apply effect based on spell type
        if (spell.Effects != null)
        {
            foreach (var effect in spell.Effects)
            {
                switch (effect)
                {
                    case SpellEffectType.Heal:
                        int healAmount = spell.BasePower + caster.Intellect;
                        target.CurrentHP = Mathf.Min(target.CurrentHP + healAmount, target.MaxHP);
                        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Heal);
                        break;
                    case SpellEffectType.StatusCure:
                        target.StatusEffects &= ~spell.CuresStatus;
                        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Heal);
                        break;
                    case SpellEffectType.Buff:
                        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Buff);
                        break;
                    default:
                        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.MagicCast);
                        break;
                }
            }
        }
        else
        {
            // Fallback: simple heal if it's a healing spell
            if (spell.BasePower > 0 && spell.UsableInField)
            {
                int healAmount = spell.BasePower + caster.Intellect;
                target.CurrentHP = Mathf.Min(target.CurrentHP + healAmount, target.MaxHP);
                GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Heal);
            }
        }

        // Refresh UI
        RefreshTargetList();
        UpdateTargetCursor();
        UpdateMPDisplay();

        // Update spell list colors (may no longer have MP)
        RefreshSpellList();
        selectedSpellIndex = Mathf.Clamp(selectedSpellIndex, 0, Mathf.Max(0, displayedSpells.Count - 1));
    }

    // --- UI Building ---

    void BuildUI(Transform canvasRoot)
    {
        rootPanel = new GameObject("MagicMenuRoot");
        var rootRect = rootPanel.AddComponent<RectTransform>();
        rootRect.SetParent(canvasRoot, false);
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.sizeDelta = Vector2.zero;

        var windowGO = CreateWindow("MagicWindow", rootRect);
        var windowRect = windowGO.GetComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.1f, 0.1f);
        windowRect.anchorMax = new Vector2(0.9f, 0.9f);
        windowRect.sizeDelta = Vector2.zero;

        // Title
        var titleGO = CreateText("Title", "Magic", 22, TextAlignmentOptions.Left, windowRect);
        var titleR = titleGO.GetComponent<RectTransform>();
        titleR.anchorMin = new Vector2(0.02f, 0.9f);
        titleR.anchorMax = new Vector2(0.6f, 0.98f);
        titleR.sizeDelta = Vector2.zero;
        titleText = titleGO.GetComponent<TextMeshProUGUI>();

        // MP display
        var mpGO = CreateText("MPText", "MP: 0/0", 18, TextAlignmentOptions.Right, windowRect);
        var mpR = mpGO.GetComponent<RectTransform>();
        mpR.anchorMin = new Vector2(0.6f, 0.9f);
        mpR.anchorMax = new Vector2(0.98f, 0.98f);
        mpR.sizeDelta = Vector2.zero;
        mpText = mpGO.GetComponent<TextMeshProUGUI>();

        // Member select
        BuildMemberSelectPanel(windowRect);

        // Spell list
        BuildSpellListPanel(windowRect);

        // No spells message
        var noSpellGO = CreateText("NoSpells", "No spells known", 18, TextAlignmentOptions.Center, windowRect);
        var noSpellR = noSpellGO.GetComponent<RectTransform>();
        noSpellR.anchorMin = new Vector2(0.2f, 0.4f);
        noSpellR.anchorMax = new Vector2(0.8f, 0.6f);
        noSpellR.sizeDelta = Vector2.zero;
        noSpellsText = noSpellGO.GetComponent<TextMeshProUGUI>();
        noSpellsText.color = new Color(0.6f, 0.6f, 0.6f);
        noSpellsText.gameObject.SetActive(false);

        // Target panel
        BuildTargetPanel(windowRect);
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

    void BuildSpellListPanel(RectTransform parent)
    {
        spellListPanel = new GameObject("SpellListPanel");
        var rect = spellListPanel.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.02f, 0.05f);
        rect.anchorMax = new Vector2(0.98f, 0.88f);
        rect.sizeDelta = Vector2.zero;

        var scrollGO = new GameObject("SpellScroll");
        var scrollRect = scrollGO.AddComponent<RectTransform>();
        scrollRect.SetParent(rect, false);
        scrollRect.anchorMin = Vector2.zero;
        scrollRect.anchorMax = Vector2.one;
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
        contentLayout.spacing = 4;
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

        spellListContent = contentGO;
        spellListPanel.SetActive(false);
    }

    void BuildTargetPanel(RectTransform parent)
    {
        targetPanel = CreateWindow("TargetPanel", parent);
        var targetRect = targetPanel.GetComponent<RectTransform>();
        targetRect.anchorMin = new Vector2(0.3f, 0.3f);
        targetRect.anchorMax = new Vector2(0.7f, 0.7f);
        targetRect.sizeDelta = Vector2.zero;

        var titleGO = CreateText("TargetTitle", "Cast on", 18, TextAlignmentOptions.Center, targetRect);
        var titleR = titleGO.GetComponent<RectTransform>();
        titleR.anchorMin = new Vector2(0, 0.85f);
        titleR.anchorMax = new Vector2(1, 1f);
        titleR.sizeDelta = Vector2.zero;

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
