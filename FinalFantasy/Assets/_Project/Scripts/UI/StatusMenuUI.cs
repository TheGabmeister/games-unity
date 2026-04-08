using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class StatusMenuUI : MonoBehaviour, IMenuSubScreen
{
    GameObject rootPanel;

    // Stat display texts
    TextMeshProUGUI titleText;
    TextMeshProUGUI classLevelText;
    TextMeshProUGUI expText;
    TextMeshProUGUI hpmpText;
    TextMeshProUGUI atkDefText;
    TextMeshProUGUI accEvaText;
    TextMeshProUGUI mdefCritText;
    TextMeshProUGUI strAgiText;
    TextMeshProUGUI vitIntText;
    TextMeshProUGUI lckText;
    TextMeshProUGUI weaponText;
    TextMeshProUGUI shieldText;
    TextMeshProUGUI helmetText;
    TextMeshProUGUI armorText;
    TextMeshProUGUI navHintText;

    int currentMemberIndex;
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
        currentMemberIndex = 0;
        RefreshDisplay();
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
            GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cancel);
            Hide();
            return;
        }

        if (input.MoveAction != null && input.MoveAction.WasPressedThisFrame())
        {
            Vector2 dir = input.MoveAction.ReadValue<Vector2>();
            if (dir.x > 0.5f)
                CycleMember(1);
            else if (dir.x < -0.5f)
                CycleMember(-1);
        }
    }

    void CycleMember(int direction)
    {
        currentMemberIndex = (currentMemberIndex + direction + 4) % 4;
        RefreshDisplay();
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cursor);
    }

    void RefreshDisplay()
    {
        var pm = GameManager.Instance?.PartyManager;
        var member = pm?.GetMember(currentMemberIndex);

        if (member == null)
        {
            titleText.text = "Status - (empty)";
            classLevelText.text = "";
            expText.text = "";
            hpmpText.text = "";
            atkDefText.text = "";
            accEvaText.text = "";
            mdefCritText.text = "";
            strAgiText.text = "";
            vitIntText.text = "";
            lckText.text = "";
            weaponText.text = "";
            shieldText.text = "";
            helmetText.text = "";
            armorText.text = "";
            return;
        }

        string className = member.ClassDef != null ? member.ClassDef.ClassName : "???";
        titleText.text = $"Status - {member.Name}";

        if (titleText.color != Color.white && member.ClassDef != null)
            titleText.color = Color.white;

        classLevelText.text = $"Class: {className}     Level: {member.Level}";

        int expNext = member.EXPForNextLevel();
        expText.text = $"EXP: {member.CurrentEXP:N0}       Next: {expNext:N0}";

        hpmpText.text = $"HP: {member.CurrentHP}/{member.MaxHP}    MP: {member.CurrentMP}/{member.MaxMP}";

        atkDefText.text = $"ATK: {member.Attack}    DEF: {member.Defense}";
        accEvaText.text = $"ACC: {member.Accuracy}    EVA: {member.Evasion}";
        mdefCritText.text = $"M.DEF: {member.MagicDefense}   CRIT: {member.CritRate}";

        strAgiText.text = $"STR: {member.Strength}    AGI: {member.Agility}";
        vitIntText.text = $"VIT: {member.Vitality}    INT: {member.Intellect}";
        lckText.text = $"LCK: {member.Luck}";

        weaponText.text = $"Weapon: {(member.Weapon != null ? member.Weapon.ItemName : "(none)")}";
        shieldText.text = $"Shield: {(member.Shield != null ? member.Shield.ItemName : "(none)")}";
        helmetText.text = $"Helmet: {(member.Helmet != null ? member.Helmet.ItemName : "(none)")}";
        armorText.text = $"Armor:  {(member.Armor != null ? member.Armor.ItemName : "(none)")}";

        navHintText.text = $"< {currentMemberIndex + 1}/4 >";
    }

    // --- UI Building ---

    void BuildUI(Transform canvasRoot)
    {
        rootPanel = new GameObject("StatusMenuRoot");
        var rootRect = rootPanel.AddComponent<RectTransform>();
        rootRect.SetParent(canvasRoot, false);
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.sizeDelta = Vector2.zero;

        var windowGO = CreateWindow("StatusWindow", rootRect);
        var windowRect = windowGO.GetComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.1f, 0.08f);
        windowRect.anchorMax = new Vector2(0.9f, 0.92f);
        windowRect.sizeDelta = Vector2.zero;

        // Title
        titleText = CreateStatLine("Title", "Status - ???", 22, new Vector2(0.03f, 0.9f), new Vector2(0.97f, 0.98f), windowRect);

        // Class and level
        classLevelText = CreateStatLine("ClassLevel", "Class: ???     Level: 1", 18, new Vector2(0.03f, 0.82f), new Vector2(0.97f, 0.9f), windowRect);

        // EXP
        expText = CreateStatLine("EXP", "EXP: 0       Next: 0", 18, new Vector2(0.03f, 0.74f), new Vector2(0.97f, 0.82f), windowRect);

        // Separator line (just space)

        // HP/MP
        hpmpText = CreateStatLine("HPMP", "HP: 0/0    MP: 0/0", 18, new Vector2(0.03f, 0.62f), new Vector2(0.97f, 0.7f), windowRect);

        // ATK/DEF
        atkDefText = CreateStatLine("ATKDEF", "ATK: 0    DEF: 0", 18, new Vector2(0.03f, 0.54f), new Vector2(0.48f, 0.62f), windowRect);

        // ACC/EVA
        accEvaText = CreateStatLine("ACCEVA", "ACC: 0    EVA: 0", 18, new Vector2(0.5f, 0.54f), new Vector2(0.97f, 0.62f), windowRect);

        // MDEF/CRIT
        mdefCritText = CreateStatLine("MDEFCRIT", "M.DEF: 0   CRIT: 0", 18, new Vector2(0.5f, 0.46f), new Vector2(0.97f, 0.54f), windowRect);

        // STR/AGI
        strAgiText = CreateStatLine("STRAGI", "STR: 0    AGI: 0", 18, new Vector2(0.03f, 0.46f), new Vector2(0.48f, 0.54f), windowRect);

        // VIT/INT
        vitIntText = CreateStatLine("VITINT", "VIT: 0    INT: 0", 18, new Vector2(0.03f, 0.38f), new Vector2(0.48f, 0.46f), windowRect);

        // LCK
        lckText = CreateStatLine("LCK", "LCK: 0", 18, new Vector2(0.03f, 0.3f), new Vector2(0.48f, 0.38f), windowRect);

        // Equipment
        weaponText = CreateStatLine("Weapon", "Weapon: (none)", 18, new Vector2(0.03f, 0.18f), new Vector2(0.97f, 0.26f), windowRect);
        shieldText = CreateStatLine("Shield", "Shield: (none)", 18, new Vector2(0.03f, 0.11f), new Vector2(0.97f, 0.19f), windowRect);
        helmetText = CreateStatLine("Helmet", "Helmet: (none)", 18, new Vector2(0.03f, 0.04f), new Vector2(0.48f, 0.12f), windowRect);
        armorText = CreateStatLine("Armor", "Armor:  (none)", 18, new Vector2(0.5f, 0.04f), new Vector2(0.97f, 0.12f), windowRect);

        // Nav hint
        navHintText = CreateStatLine("NavHint", "< 1/4 >", 16, new Vector2(0.35f, 0.0f), new Vector2(0.65f, 0.06f), windowRect);
        navHintText.alignment = TextAlignmentOptions.Center;
        navHintText.color = new Color(0.7f, 0.7f, 0.7f);
    }

    TextMeshProUGUI CreateStatLine(string name, string defaultText, float fontSize, Vector2 anchorMin, Vector2 anchorMax, RectTransform parent)
    {
        var go = CreateText(name, defaultText, fontSize, TextAlignmentOptions.Left, parent);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
        return go.GetComponent<TextMeshProUGUI>();
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
