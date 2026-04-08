using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class DebugConsole : MonoBehaviour
{
    TMP_InputField inputField;
    TextMeshProUGUI outputText;
    ScrollRect scrollRect;
    bool isVisible;

    Canvas canvas;
    string outputLog = "";

    void Awake()
    {
        SetupUI();
        SetVisible(false);
    }

    void SetupUI()
    {
        canvas = GetComponentInParent<Canvas>();

        var rect = GetComponent<RectTransform>();
        if (rect == null) rect = gameObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 0.35f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Background
        var bg = gameObject.GetComponent<Image>();
        if (bg == null) bg = gameObject.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.85f);
        bg.raycastTarget = true;

        // Output text area (scrollable)
        var outputGO = new GameObject("Output");
        outputGO.transform.SetParent(transform, false);
        var outputRect = outputGO.AddComponent<RectTransform>();
        outputRect.anchorMin = new Vector2(0, 0.12f);
        outputRect.anchorMax = Vector2.one;
        outputRect.offsetMin = new Vector2(10, 0);
        outputRect.offsetMax = new Vector2(-10, -5);

        outputText = outputGO.AddComponent<TextMeshProUGUI>();
        outputText.fontSize = 14;
        outputText.color = Color.green;
        outputText.alignment = TextAlignmentOptions.BottomLeft;
        outputText.textWrappingMode = TextWrappingModes.Normal;
        outputText.overflowMode = TextOverflowModes.Truncate;
        outputText.raycastTarget = false;

        // Input field
        var inputGO = new GameObject("Input");
        inputGO.transform.SetParent(transform, false);
        var inputRect = inputGO.AddComponent<RectTransform>();
        inputRect.anchorMin = Vector2.zero;
        inputRect.anchorMax = new Vector2(1, 0.12f);
        inputRect.offsetMin = new Vector2(10, 5);
        inputRect.offsetMax = new Vector2(-10, 0);

        // TMP Input field needs a background image on the inputGO
        var inputBG = inputGO.AddComponent<Image>();
        inputBG.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        // Create text area child for the input field
        var textAreaGO = new GameObject("Text Area");
        textAreaGO.transform.SetParent(inputGO.transform, false);
        var textAreaRect = textAreaGO.AddComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.offsetMin = new Vector2(5, 0);
        textAreaRect.offsetMax = new Vector2(-5, 0);

        // The actual text child
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(textAreaGO.transform, false);
        var textComponent = textGO.AddComponent<TextMeshProUGUI>();
        textComponent.fontSize = 14;
        textComponent.color = Color.white;
        var textR = textComponent.rectTransform;
        textR.anchorMin = Vector2.zero;
        textR.anchorMax = Vector2.one;
        textR.offsetMin = Vector2.zero;
        textR.offsetMax = Vector2.zero;

        // Placeholder
        var placeholderGO = new GameObject("Placeholder");
        placeholderGO.transform.SetParent(textAreaGO.transform, false);
        var placeholder = placeholderGO.AddComponent<TextMeshProUGUI>();
        placeholder.text = "Enter command...";
        placeholder.fontSize = 14;
        placeholder.color = new Color(1, 1, 1, 0.3f);
        placeholder.fontStyle = FontStyles.Italic;
        var phRect = placeholder.rectTransform;
        phRect.anchorMin = Vector2.zero;
        phRect.anchorMax = Vector2.one;
        phRect.offsetMin = Vector2.zero;
        phRect.offsetMax = Vector2.zero;

        inputField = inputGO.AddComponent<TMP_InputField>();
        inputField.textComponent = textComponent;
        inputField.textViewport = textAreaRect;
        inputField.placeholder = placeholder;
        inputField.onSubmit.AddListener(OnSubmitCommand);
    }

    void Update()
    {
        // Toggle with backtick key
        bool togglePressed = Keyboard.current != null && Keyboard.current.backquoteKey.wasPressedThisFrame;
        if (!togglePressed)
        {
            var input = GameManager.Instance?.InputManager;
            if (input?.DebugConsoleAction != null && input.DebugConsoleAction.WasPressedThisFrame())
                togglePressed = true;
        }

        if (togglePressed)
        {
            SetVisible(!isVisible);
        }
    }

    void SetVisible(bool visible)
    {
        isVisible = visible;

        // Show/hide all children
        foreach (Transform child in transform)
            child.gameObject.SetActive(visible);

        var bg = GetComponent<Image>();
        if (bg != null) bg.enabled = visible;

        // Switch input routing so gameplay doesn't consume keystrokes
        var inputManager = GameManager.Instance?.InputManager;
        if (visible)
        {
            inputManager?.EnableUI();
            if (inputField != null)
            {
                inputField.ActivateInputField();
                inputField.Select();
            }
        }
        else
        {
            if (GameManager.Instance?.StateManager?.CurrentState == GameState.Exploration)
                inputManager?.EnableGameplay();
        }
    }

    void OnSubmitCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command)) return;

        Log($"> {command}");
        ExecuteCommand(command.Trim());

        inputField.text = "";
        inputField.ActivateInputField();
    }

    void ExecuteCommand(string command)
    {
        var parts = command.Split(' ');
        string cmd = parts[0].ToLower();

        switch (cmd)
        {
            case "help":
                Log("Phase 1: help, state, pos, save, load, scene");
                Log("Phase 2: setlevel, addgil, levelup, learnspell");
                break;

            case "state":
                if (parts.Length < 2) { Log("Usage: state <stateName>"); break; }
                if (System.Enum.TryParse<GameState>(parts[1], true, out var newState))
                {
                    GameManager.Instance?.StateManager?.ChangeState(newState);
                    Log($"State changed to {newState}");
                }
                else Log($"Unknown state: {parts[1]}");
                break;

            case "pos":
                if (parts.Length < 3) { Log("Usage: pos <x> <y>"); break; }
                if (int.TryParse(parts[1], out int px) && int.TryParse(parts[2], out int py))
                {
                    var player = Object.FindFirstObjectByType<PlayerController>();
                    if (player != null)
                    {
                        player.SetPosition(new Vector2Int(px, py));
                        Log($"Teleported to ({px}, {py})");
                    }
                    else Log("No PlayerController found");
                }
                else Log("Invalid coordinates");
                break;

            case "save":
                if (parts.Length < 2) { Log("Usage: save <slot 0-3>"); break; }
                if (int.TryParse(parts[1], out int saveSlot))
                {
                    var data = SaveHelper.CreateSaveData(SaveType.Manual);
                    GameManager.Instance?.SaveManager?.Save(saveSlot, data);
                    Log($"Saved to slot {saveSlot}");
                }
                break;

            case "load":
                if (parts.Length < 2) { Log("Usage: load <slot>"); break; }
                if (int.TryParse(parts[1], out int loadSlot))
                {
                    var data = GameManager.Instance?.SaveManager?.Load(loadSlot);
                    if (data != null)
                    {
                        Log($"Loaded slot {loadSlot}: scene={data.CurrentScene} pos=({data.PlayerGridX},{data.PlayerGridY})");
                        SaveHelper.ApplySaveData(data);
                    }
                    else Log($"No save at slot {loadSlot}");
                }
                break;

            case "scene":
                if (parts.Length < 2) { Log("Usage: scene <sceneName>"); break; }
                Log($"Loading scene: {parts[1]}");
                _ = GameManager.Instance?.SceneLoader?.LoadScene(parts[1]);
                break;

            case "setlevel":
                if (parts.Length < 3) { Log("Usage: setlevel <slot 0-3> <level>"); break; }
                if (int.TryParse(parts[1], out int slSlot) && int.TryParse(parts[2], out int slLevel))
                {
                    GameManager.Instance?.PartyManager?.SetLevel(slSlot, slLevel);
                    var m = GameManager.Instance?.PartyManager?.GetMember(slSlot);
                    if (m != null) Log($"{m.Name} set to level {m.Level}");
                    else Log($"No member in slot {slSlot}");
                }
                break;

            case "addgil":
                if (parts.Length < 2) { Log("Usage: addgil <amount>"); break; }
                if (int.TryParse(parts[1], out int gilAmt))
                {
                    GameManager.Instance?.InventoryManager?.AddGil(gilAmt);
                    Log($"Gil: {GameManager.Instance?.InventoryManager?.Gil:N0}");
                }
                break;

            case "learnspell":
                if (parts.Length < 2) { Log("Usage: learnspell <slot 0-3> [spellName]"); break; }
                if (int.TryParse(parts[1], out int lsSlot))
                {
                    var member = GameManager.Instance?.PartyManager?.GetMember(lsSlot);
                    if (member == null) { Log($"No member in slot {lsSlot}"); break; }

                    if (parts.Length >= 3)
                    {
                        string spellName = parts[2];
                        var allSpells = Resources.FindObjectsOfTypeAll<SpellData>();
                        SpellData found = null;
                        foreach (var sp in allSpells)
                        {
                            if (sp.SpellName.Equals(spellName, System.StringComparison.OrdinalIgnoreCase))
                            { found = sp; break; }
                        }
                        if (found != null)
                        {
                            if (!member.KnownSpells.Contains(found))
                                member.KnownSpells.Add(found);
                            Log($"{member.Name} learned {found.SpellName}");
                        }
                        else
                        {
                            Log($"Spell '{spellName}' not found. Creating test Cure spell.");
                            var cure = CreateTestSpell("Cure", MagicSchool.White, 1, 3, SpellEffectType.Heal, 30, true);
                            member.KnownSpells.Add(cure);
                            Log($"{member.Name} learned Cure (test)");
                        }
                    }
                    else
                    {
                        // No spell name given — grant a set of test spells
                        var testSpells = new[]
                        {
                            CreateTestSpell("Cure", MagicSchool.White, 1, 3, SpellEffectType.Heal, 30, true),
                            CreateTestSpell("Fire", MagicSchool.Black, 1, 3, SpellEffectType.Damage, 20, false),
                            CreateTestSpell("Poisona", MagicSchool.White, 1, 3, SpellEffectType.StatusCure, 0, true),
                        };
                        foreach (var sp in testSpells)
                        {
                            if (!member.KnownSpells.Contains(sp))
                                member.KnownSpells.Add(sp);
                        }
                        Log($"{member.Name} learned Cure, Fire, Poisona (test spells)");
                    }
                }
                break;

            case "levelup":
                if (parts.Length < 2) { Log("Usage: levelup <slot 0-3>"); break; }
                if (int.TryParse(parts[1], out int luSlot))
                {
                    var result = GameManager.Instance?.PartyManager?.LevelUp(luSlot);
                    if (result != null)
                    {
                        Log($"Level {result.OldLevel} → {result.NewLevel}");
                        Log($"HP: {result.OldHP} → {result.NewHP} | STR: {result.OldStr} → {result.NewStr}");
                        Log($"AGI: {result.OldAgi} → {result.NewAgi} | VIT: {result.OldVit} → {result.NewVit}");
                        Log($"INT: {result.OldInt} → {result.NewInt} | LCK: {result.OldLuck} → {result.NewLuck}");
                    }
                    else Log($"Cannot level up slot {luSlot}");
                }
                break;

            default:
                Log($"Unknown command: {cmd}. Type 'help' for commands.");
                break;
        }
    }

    void Log(string message)
    {
        outputLog += message + "\n";
        // Keep last 20 lines
        var lines = outputLog.Split('\n');
        if (lines.Length > 21)
        {
            outputLog = string.Join("\n", lines, lines.Length - 21, 21);
        }
        if (outputText != null)
            outputText.text = outputLog;
    }

    SpellData CreateTestSpell(string spellName, MagicSchool school, int level, int mpCost,
        SpellEffectType effectType, int basePower, bool usableInField)
    {
        var spell = ScriptableObject.CreateInstance<SpellData>();
        spell.SpellName = spellName;
        spell.School = school;
        spell.Level = level;
        spell.MPCost = mpCost;
        spell.Effects = new[] { effectType };
        spell.BasePower = basePower;
        spell.UsableInField = usableInField;
        spell.UsableInBattle = true;
        spell.Targeting = effectType == SpellEffectType.Heal || effectType == SpellEffectType.StatusCure
            ? SpellTarget.SingleAlly
            : SpellTarget.SingleEnemy;
        if (effectType == SpellEffectType.StatusCure)
            spell.CuresStatus = StatusEffectFlags.Poison;
        spell.name = spellName;
        return spell;
    }
}
