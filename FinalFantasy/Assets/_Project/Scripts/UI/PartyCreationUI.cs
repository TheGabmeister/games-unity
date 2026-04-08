using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class PartyCreationUI : MonoBehaviour
{
    [SerializeField] ClassDefinition[] availableClasses;

    // Runtime UI references
    Canvas canvas;
    GameObject rootPanel;
    GameObject previewPanel;

    // Per-slot data
    int[] classIndices = new int[4];
    TMP_InputField[] nameInputs = new TMP_InputField[4];
    TextMeshProUGUI[] classLabels = new TextMeshProUGUI[4];
    GameObject[] slotRows = new GameObject[4];
    Button confirmButton;

    // Preview stat texts
    TextMeshProUGUI previewHP, previewMP, previewSTR, previewAGI, previewVIT, previewINT, previewLCK;

    int activeSlot = 0;

    static readonly string[] DefaultNames = { "Warr", "Thie", "Monk", "RedM", "Whit", "Blac" };

    void Start()
    {
        if (availableClasses == null || availableClasses.Length == 0)
            availableClasses = GetDefaultClasses();

        BuildUI();
        UpdateAllSlotDisplays();
        UpdatePreview();
        SetActiveSlot(0);

        GameManager.Instance?.Audio?.PlayBGM(MusicTrack.PartyCreation);
        GameManager.Instance?.InputManager?.EnableUI();
    }

    void Update()
    {
        var input = GameManager.Instance?.InputManager;
        if (input == null) return;

        // Don't process navigation if an input field is focused
        if (EventSystem.current?.currentSelectedGameObject != null)
        {
            var selectedInput = EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>();
            if (selectedInput != null && selectedInput.isFocused)
                return;
        }

        var move = input.MoveAction;
        if (move != null && move.WasPressedThisFrame())
        {
            Vector2 dir = move.ReadValue<Vector2>();
            if (dir.x < -0.5f)
                CycleClass(-1);
            else if (dir.x > 0.5f)
                CycleClass(1);
            if (dir.y > 0.5f)
                NavigateSlot(-1);
            else if (dir.y < -0.5f)
                NavigateSlot(1);
        }

        if (input.ConfirmAction != null && input.ConfirmAction.WasPressedThisFrame())
        {
            if (activeSlot >= 4)
                OnConfirm();
        }

        if (input.CancelAction != null && input.CancelAction.WasPressedThisFrame())
        {
            if (activeSlot > 0)
            {
                GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cancel);
                SetActiveSlot(activeSlot - 1);
            }
        }
    }

    void CycleClass(int direction)
    {
        if (activeSlot >= 4) return;

        classIndices[activeSlot] = (classIndices[activeSlot] + direction + availableClasses.Length) % availableClasses.Length;
        UpdateSlotDisplay(activeSlot);
        UpdatePreview();

        // Update default name if name matches a default
        string currentName = nameInputs[activeSlot].text;
        bool isDefault = false;
        for (int i = 0; i < DefaultNames.Length; i++)
        {
            if (currentName == DefaultNames[i])
            {
                isDefault = true;
                break;
            }
        }
        if (isDefault || string.IsNullOrEmpty(currentName))
        {
            nameInputs[activeSlot].text = DefaultNames[classIndices[activeSlot]];
        }

        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cursor);
    }

    void NavigateSlot(int direction)
    {
        int newSlot = activeSlot + direction;
        newSlot = Mathf.Clamp(newSlot, 0, 4); // 4 = confirm button
        if (newSlot != activeSlot)
        {
            GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cursor);
            SetActiveSlot(newSlot);
        }
    }

    void SetActiveSlot(int slot)
    {
        activeSlot = slot;

        // Update highlight
        for (int i = 0; i < 4; i++)
        {
            var bg = slotRows[i].GetComponent<Image>();
            if (bg != null)
                bg.color = (i == activeSlot) ? new Color(0.15f, 0.15f, 0.5f, 0.95f) : new Color(0, 0, 0, 0);
        }

        if (activeSlot < 4)
        {
            UpdatePreview();
            var colors = confirmButton.colors;
            colors.normalColor = new Color(0, 0, 0.267f, 0.95f);
            confirmButton.colors = colors;
        }
        else
        {
            EventSystem.current?.SetSelectedGameObject(confirmButton.gameObject);
            var colors = confirmButton.colors;
            colors.normalColor = new Color(0.15f, 0.15f, 0.5f, 0.95f);
            confirmButton.colors = colors;
        }
    }

    void UpdateSlotDisplay(int slot)
    {
        var classDef = availableClasses[classIndices[slot]];
        classLabels[slot].text = classDef.ClassName;
        classLabels[slot].color = classDef.ClassColor;
    }

    void UpdateAllSlotDisplays()
    {
        for (int i = 0; i < 4; i++)
            UpdateSlotDisplay(i);
    }

    void UpdatePreview()
    {
        if (activeSlot >= 4) return;
        var classDef = availableClasses[classIndices[activeSlot]];
        previewHP.text = $"HP: {classDef.BaseHP}";
        previewMP.text = $"MP: {classDef.BaseMP}";
        previewSTR.text = $"STR: {classDef.BaseStrength}";
        previewAGI.text = $"AGI: {classDef.BaseAgility}";
        previewVIT.text = $"VIT: {classDef.BaseVitality}";
        previewINT.text = $"INT: {classDef.BaseIntellect}";
        previewLCK.text = $"LCK: {classDef.BaseLuck}";
    }

    async void OnConfirm()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        var classes = new ClassDefinition[4];
        var names = new string[4];
        for (int i = 0; i < 4; i++)
        {
            classes[i] = availableClasses[classIndices[i]];
            names[i] = string.IsNullOrWhiteSpace(nameInputs[i].text)
                ? DefaultNames[classIndices[i]]
                : nameInputs[i].text;
        }

        gm.PartyManager.CreateParty(classes, names);
        gm.Audio?.PlaySFX(SoundEffect.Confirm);
        gm.StateManager.ChangeState(GameState.Exploration);
        await gm.SceneLoader.LoadScene("Exploration");
    }

    // --- UI Building ---

    void BuildUI()
    {
        // Canvas
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
        }
        if (GetComponent<CanvasScaler>() == null)
        {
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
        }
        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();

        // Main window
        rootPanel = CreateWindow("PartyCreationWindow", transform);
        var rootRect = rootPanel.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.15f, 0.1f);
        rootRect.anchorMax = new Vector2(0.85f, 0.9f);
        rootRect.sizeDelta = Vector2.zero;
        rootRect.anchoredPosition = Vector2.zero;

        // Title
        var titleGO = CreateText("Title", "Choose your Warriors of Light", 22, TextAlignmentOptions.Center, rootRect);
        var titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.9f);
        titleRect.anchorMax = new Vector2(1, 1f);
        titleRect.sizeDelta = Vector2.zero;
        titleRect.anchoredPosition = Vector2.zero;

        // Slots area
        var slotsArea = new GameObject("SlotsArea");
        var slotsRect = slotsArea.AddComponent<RectTransform>();
        slotsRect.SetParent(rootRect, false);
        slotsRect.anchorMin = new Vector2(0.05f, 0.45f);
        slotsRect.anchorMax = new Vector2(0.95f, 0.88f);
        slotsRect.sizeDelta = Vector2.zero;

        var slotsLayout = slotsArea.AddComponent<VerticalLayoutGroup>();
        slotsLayout.spacing = 8;
        slotsLayout.childForceExpandWidth = true;
        slotsLayout.childForceExpandHeight = true;
        slotsLayout.childControlWidth = true;
        slotsLayout.childControlHeight = true;
        slotsLayout.padding = new RectOffset(10, 10, 5, 5);

        // Create 4 slot rows
        for (int i = 0; i < 4; i++)
        {
            CreateSlotRow(i, slotsRect);
        }

        // Set initial class indices and default names
        classIndices[0] = 0; // Warrior
        classIndices[1] = 1; // Thief
        classIndices[2] = 2; // Monk
        classIndices[3] = 4; // White Mage

        for (int i = 0; i < 4; i++)
        {
            nameInputs[i].text = DefaultNames[classIndices[i]];
        }

        // Preview panel
        BuildPreviewPanel(rootRect);

        // Confirm button
        BuildConfirmButton(rootRect);
    }

    void CreateSlotRow(int index, RectTransform parent)
    {
        var row = new GameObject($"Slot_{index}");
        var rowRect = row.AddComponent<RectTransform>();
        rowRect.SetParent(parent, false);

        var rowImage = row.AddComponent<Image>();
        rowImage.color = new Color(0, 0, 0, 0); // transparent by default

        var rowLayout = row.AddComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 10;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = true;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childAlignment = TextAnchor.MiddleLeft;
        rowLayout.padding = new RectOffset(10, 10, 2, 2);

        // Slot label
        var labelGO = CreateText($"SlotLabel_{index}", $"Slot {index + 1}:", 18, TextAlignmentOptions.Left, rowRect);
        var labelLayout = labelGO.AddComponent<LayoutElement>();
        labelLayout.preferredWidth = 70;

        // Left arrow
        var leftArrow = CreateText($"LeftArrow_{index}", "<", 16, TextAlignmentOptions.Center, rowRect);
        var leftLayout = leftArrow.AddComponent<LayoutElement>();
        leftLayout.preferredWidth = 25;

        // Class name
        var classGO = CreateText($"ClassName_{index}", "Warrior", 18, TextAlignmentOptions.Center, rowRect);
        var classLayout = classGO.AddComponent<LayoutElement>();
        classLayout.preferredWidth = 120;
        classLabels[index] = classGO.GetComponent<TextMeshProUGUI>();

        // Right arrow
        var rightArrow = CreateText($"RightArrow_{index}", ">", 16, TextAlignmentOptions.Center, rowRect);
        var rightLayout = rightArrow.AddComponent<LayoutElement>();
        rightLayout.preferredWidth = 25;

        // Name label
        var nameLabelGO = CreateText($"NameLabel_{index}", "Name:", 18, TextAlignmentOptions.Left, rowRect);
        var nameLabelLayout = nameLabelGO.AddComponent<LayoutElement>();
        nameLabelLayout.preferredWidth = 55;

        // Name input field
        var inputGO = new GameObject($"NameInput_{index}");
        var inputRect = inputGO.AddComponent<RectTransform>();
        inputRect.SetParent(rowRect, false);

        var inputBG = inputGO.AddComponent<Image>();
        inputBG.color = new Color(0.1f, 0.1f, 0.3f, 0.9f);

        var inputLayout = inputGO.AddComponent<LayoutElement>();
        inputLayout.preferredWidth = 100;
        inputLayout.flexibleWidth = 1;

        // Text area child
        var textAreaGO = new GameObject("Text Area");
        var textAreaRect = textAreaGO.AddComponent<RectTransform>();
        textAreaRect.SetParent(inputRect, false);
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.sizeDelta = new Vector2(-10, -6);
        textAreaRect.anchoredPosition = Vector2.zero;

        // Input text child
        var inputTextGO = new GameObject("Text");
        var inputTextRect = inputTextGO.AddComponent<RectTransform>();
        inputTextRect.SetParent(textAreaRect, false);
        inputTextRect.anchorMin = Vector2.zero;
        inputTextRect.anchorMax = Vector2.one;
        inputTextRect.sizeDelta = Vector2.zero;

        var inputTMP = inputTextGO.AddComponent<TextMeshProUGUI>();
        inputTMP.fontSize = 18;
        inputTMP.color = Color.white;
        inputTMP.alignment = TextAlignmentOptions.Left;

        var inputField = inputGO.AddComponent<TMP_InputField>();
        inputField.textComponent = inputTMP;
        inputField.textViewport = textAreaRect;
        inputField.characterLimit = 6;
        inputField.contentType = TMP_InputField.ContentType.Alphanumeric;

        nameInputs[index] = inputField;
        slotRows[index] = row;
    }

    void BuildPreviewPanel(RectTransform parent)
    {
        previewPanel = CreateWindow("PreviewPanel", parent);
        var previewRect = previewPanel.GetComponent<RectTransform>();
        previewRect.anchorMin = new Vector2(0.05f, 0.12f);
        previewRect.anchorMax = new Vector2(0.55f, 0.42f);
        previewRect.sizeDelta = Vector2.zero;

        // Title
        var titleGO = CreateText("PreviewTitle", "Preview", 16, TextAlignmentOptions.Center, previewRect);
        var titleR = titleGO.GetComponent<RectTransform>();
        titleR.anchorMin = new Vector2(0, 0.82f);
        titleR.anchorMax = new Vector2(1, 1f);
        titleR.sizeDelta = Vector2.zero;

        // Stats - Left column
        previewHP = CreateStatText("HP", "HP: 35", new Vector2(0.05f, 0.55f), new Vector2(0.5f, 0.8f), previewRect);
        previewMP = CreateStatText("MP", "MP: 0", new Vector2(0.05f, 0.3f), new Vector2(0.5f, 0.55f), previewRect);
        previewVIT = CreateStatText("VIT", "VIT: 10", new Vector2(0.05f, 0.05f), new Vector2(0.5f, 0.3f), previewRect);

        // Stats - Right column
        previewSTR = CreateStatText("STR", "STR: 20", new Vector2(0.5f, 0.55f), new Vector2(0.95f, 0.8f), previewRect);
        previewAGI = CreateStatText("AGI", "AGI: 5", new Vector2(0.5f, 0.3f), new Vector2(0.95f, 0.55f), previewRect);
        previewINT = CreateStatText("INT", "INT: 1", new Vector2(0.5f, 0.05f), new Vector2(0.95f, 0.3f), previewRect);

        // LCK below
        previewLCK = CreateStatText("LCK", "LCK: 5", new Vector2(0.05f, -0.2f), new Vector2(0.5f, 0.05f), previewRect);
    }

    TextMeshProUGUI CreateStatText(string name, string defaultText, Vector2 anchorMin, Vector2 anchorMax, RectTransform parent)
    {
        var go = CreateText(name, defaultText, 16, TextAlignmentOptions.Left, parent);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
        return go.GetComponent<TextMeshProUGUI>();
    }

    void BuildConfirmButton(RectTransform parent)
    {
        var btnGO = new GameObject("ConfirmButton");
        var btnRect = btnGO.AddComponent<RectTransform>();
        btnRect.SetParent(parent, false);
        btnRect.anchorMin = new Vector2(0.35f, 0.02f);
        btnRect.anchorMax = new Vector2(0.65f, 0.1f);
        btnRect.sizeDelta = Vector2.zero;

        var btnImage = btnGO.AddComponent<Image>();
        btnImage.color = new Color(0, 0, 0.267f, 0.95f);

        var outline = btnGO.AddComponent<Outline>();
        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(2, -2);
        var outline2 = btnGO.AddComponent<Outline>();
        outline2.effectColor = Color.white;
        outline2.effectDistance = new Vector2(-2, 2);

        confirmButton = btnGO.AddComponent<Button>();
        var colors = confirmButton.colors;
        colors.normalColor = new Color(0, 0, 0.267f, 0.95f);
        colors.highlightedColor = new Color(0.1f, 0.1f, 0.4f, 0.95f);
        colors.selectedColor = new Color(0.15f, 0.15f, 0.5f, 0.95f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.6f, 0.95f);
        confirmButton.colors = colors;

        confirmButton.onClick.AddListener(OnConfirm);

        var labelGO = CreateText("ConfirmLabel", "Confirm", 20, TextAlignmentOptions.Center, btnRect);
        var labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;

        // Set navigation: None mode, we handle it manually
        var nav = new Navigation { mode = Navigation.Mode.None };
        confirmButton.navigation = nav;
    }

    // --- UI Helpers ---

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

    // --- Default Class Definitions ---

    static EquipmentData CreateStarterWeapon(string itemName, WeaponType weaponType,
        int attack, int accuracy, int critRate, int buyPrice)
    {
        var weapon = ScriptableObject.CreateInstance<EquipmentData>();
        weapon.name = itemName;
        weapon.ItemName = itemName;
        weapon.Slot = EquipmentSlot.Weapon;
        weapon.WeaponType = weaponType;
        weapon.Attack = attack;
        weapon.Accuracy = accuracy;
        weapon.CritRate = critRate;
        weapon.BuyPrice = buyPrice;
        weapon.SellPrice = buyPrice / 2;
        return weapon;
    }

    public static ClassDefinition[] GetDefaultClasses()
    {
        var classes = new ClassDefinition[6];

        // Warrior
        classes[0] = ScriptableObject.CreateInstance<ClassDefinition>();
        classes[0].ClassName = "Warrior";
        classes[0].Abbreviation = "W";
        classes[0].ClassColor = new Color(0.9f, 0.2f, 0.2f);
        classes[0].BaseHP = 35; classes[0].BaseMP = 0;
        classes[0].BaseStrength = 20; classes[0].BaseAgility = 5;
        classes[0].BaseVitality = 10; classes[0].BaseIntellect = 1; classes[0].BaseLuck = 5;
        classes[0].HPGrowth = 22; classes[0].MPGrowth = 0;
        classes[0].StrGrowth = 2; classes[0].AgiGrowth = 1;
        classes[0].VitGrowth = 1; classes[0].IntGrowth = 1; classes[0].LuckGrowth = 1;
        classes[0].BaseAccuracy = 10; classes[0].BaseEvasion = 5; classes[0].BaseMagicDefense = 15;
        classes[0].AllowedWeapons = new[] { WeaponType.Sword, WeaponType.Axe, WeaponType.Hammer };
        classes[0].AllowedArmor = new[] { ArmorType.Shield, ArmorType.HeavyArmor, ArmorType.HeavyHelmet, ArmorType.Gloves };
        classes[0].StarterWeapon = CreateStarterWeapon("Broadsword", WeaponType.Sword, 15, 10, 5, 1500);

        // Thief
        classes[1] = ScriptableObject.CreateInstance<ClassDefinition>();
        classes[1].ClassName = "Thief";
        classes[1].Abbreviation = "Th";
        classes[1].ClassColor = new Color(0.2f, 0.8f, 0.2f);
        classes[1].BaseHP = 30; classes[1].BaseMP = 0;
        classes[1].BaseStrength = 5; classes[1].BaseAgility = 15;
        classes[1].BaseVitality = 5; classes[1].BaseIntellect = 1; classes[1].BaseLuck = 15;
        classes[1].HPGrowth = 18; classes[1].MPGrowth = 0;
        classes[1].StrGrowth = 1; classes[1].AgiGrowth = 2;
        classes[1].VitGrowth = 1; classes[1].IntGrowth = 1; classes[1].LuckGrowth = 1;
        classes[1].BaseAccuracy = 10; classes[1].BaseEvasion = 5; classes[1].BaseMagicDefense = 15;
        classes[1].AllowedWeapons = new[] { WeaponType.Dagger, WeaponType.Sword };
        classes[1].AllowedArmor = new[] { ArmorType.LightArmor, ArmorType.Shield, ArmorType.Hat, ArmorType.Armlet };
        classes[1].StarterWeapon = CreateStarterWeapon("Dagger", WeaponType.Dagger, 5, 10, 3, 200);

        // Monk
        classes[2] = ScriptableObject.CreateInstance<ClassDefinition>();
        classes[2].ClassName = "Monk";
        classes[2].Abbreviation = "Mk";
        classes[2].ClassColor = new Color(0.9f, 0.6f, 0.1f);
        classes[2].BaseHP = 33; classes[2].BaseMP = 0;
        classes[2].BaseStrength = 5; classes[2].BaseAgility = 3;
        classes[2].BaseVitality = 10; classes[2].BaseIntellect = 1; classes[2].BaseLuck = 7;
        classes[2].HPGrowth = 24; classes[2].MPGrowth = 0;
        classes[2].StrGrowth = 2; classes[2].AgiGrowth = 1;
        classes[2].VitGrowth = 1; classes[2].IntGrowth = 1; classes[2].LuckGrowth = 1;
        classes[2].BaseAccuracy = 10; classes[2].BaseEvasion = 5; classes[2].BaseMagicDefense = 15;
        classes[2].AllowedWeapons = new[] { WeaponType.Nunchaku };
        classes[2].AllowedArmor = new[] { ArmorType.Armlet };

        // Red Mage
        classes[3] = ScriptableObject.CreateInstance<ClassDefinition>();
        classes[3].ClassName = "Red Mage";
        classes[3].Abbreviation = "RM";
        classes[3].ClassColor = new Color(0.7f, 0.2f, 0.7f);
        classes[3].BaseHP = 30; classes[3].BaseMP = 10;
        classes[3].BaseStrength = 10; classes[3].BaseAgility = 10;
        classes[3].BaseVitality = 5; classes[3].BaseIntellect = 10; classes[3].BaseLuck = 10;
        classes[3].HPGrowth = 17; classes[3].MPGrowth = 5;
        classes[3].StrGrowth = 1; classes[3].AgiGrowth = 1;
        classes[3].VitGrowth = 1; classes[3].IntGrowth = 1; classes[3].LuckGrowth = 1;
        classes[3].BaseAccuracy = 10; classes[3].BaseEvasion = 5; classes[3].BaseMagicDefense = 15;
        classes[3].AllowedWeapons = new[] { WeaponType.Sword, WeaponType.Dagger };
        classes[3].AllowedArmor = new[] { ArmorType.LightArmor, ArmorType.Shield, ArmorType.Hat, ArmorType.Armlet };
        classes[3].StarterWeapon = CreateStarterWeapon("Rapier", WeaponType.Sword, 9, 10, 5, 600);

        // White Mage
        classes[4] = ScriptableObject.CreateInstance<ClassDefinition>();
        classes[4].ClassName = "White Mage";
        classes[4].Abbreviation = "WM";
        classes[4].ClassColor = new Color(1f, 1f, 1f);
        classes[4].BaseHP = 28; classes[4].BaseMP = 15;
        classes[4].BaseStrength = 5; classes[4].BaseAgility = 5;
        classes[4].BaseVitality = 3; classes[4].BaseIntellect = 15; classes[4].BaseLuck = 7;
        classes[4].HPGrowth = 15; classes[4].MPGrowth = 8;
        classes[4].StrGrowth = 1; classes[4].AgiGrowth = 1;
        classes[4].VitGrowth = 1; classes[4].IntGrowth = 1; classes[4].LuckGrowth = 1;
        classes[4].BaseAccuracy = 10; classes[4].BaseEvasion = 5; classes[4].BaseMagicDefense = 15;
        classes[4].AllowedWeapons = new[] { WeaponType.Staff, WeaponType.Hammer };
        classes[4].AllowedArmor = new[] { ArmorType.Robe, ArmorType.Hat, ArmorType.Armlet };
        classes[4].StarterWeapon = CreateStarterWeapon("Staff", WeaponType.Staff, 6, 10, 0, 200);

        // Black Mage
        classes[5] = ScriptableObject.CreateInstance<ClassDefinition>();
        classes[5].ClassName = "Black Mage";
        classes[5].Abbreviation = "BM";
        classes[5].ClassColor = new Color(0.2f, 0.6f, 1f);
        classes[5].BaseHP = 25; classes[5].BaseMP = 15;
        classes[5].BaseStrength = 1; classes[5].BaseAgility = 5;
        classes[5].BaseVitality = 1; classes[5].BaseIntellect = 20; classes[5].BaseLuck = 7;
        classes[5].HPGrowth = 14; classes[5].MPGrowth = 8;
        classes[5].StrGrowth = 0; classes[5].AgiGrowth = 1;
        classes[5].VitGrowth = 1; classes[5].IntGrowth = 1; classes[5].LuckGrowth = 1;
        classes[5].BaseAccuracy = 10; classes[5].BaseEvasion = 5; classes[5].BaseMagicDefense = 15;
        classes[5].AllowedWeapons = new[] { WeaponType.Dagger, WeaponType.Staff };
        classes[5].AllowedArmor = new[] { ArmorType.Robe, ArmorType.Hat, ArmorType.Armlet };
        classes[5].StarterWeapon = CreateStarterWeapon("Staff", WeaponType.Staff, 6, 10, 0, 200);

        return classes;
    }
}
