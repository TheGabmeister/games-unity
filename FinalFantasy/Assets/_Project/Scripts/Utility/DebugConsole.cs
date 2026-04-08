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
                Log("Phase 2: setlevel, addgil, levelup, learnspell, addequip, additem");
                Log("Phase 3: encounter, kill, godmode, setstat, inflict, cure, nobattles");
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

            case "addequip":
                if (parts.Length < 2) { Log("Usage: addequip <name>"); break; }
                {
                    string equipName = string.Join(" ", parts, 1, parts.Length - 1);
                    var allEquip = Resources.FindObjectsOfTypeAll<EquipmentData>();
                    EquipmentData foundEquip = null;
                    foreach (var eq in allEquip)
                    {
                        if (eq.ItemName != null && eq.ItemName.Equals(equipName, System.StringComparison.OrdinalIgnoreCase))
                        { foundEquip = eq; break; }
                    }
                    if (foundEquip == null)
                        foundEquip = CreateTestEquipment(equipName);
                    GameManager.Instance?.InventoryManager?.AddEquipment(foundEquip);
                    Log($"Added {foundEquip.ItemName} ({foundEquip.Slot}, ATK:{foundEquip.Attack} DEF:{foundEquip.Defense})");
                }
                break;

            case "additem":
                if (parts.Length < 2) { Log("Usage: additem <name> [count]"); break; }
                {
                    int itemCount = 1;
                    int nameEnd = parts.Length;
                    if (parts.Length >= 3 && int.TryParse(parts[parts.Length - 1], out int pc))
                    {
                        itemCount = Mathf.Max(1, pc);
                        nameEnd = parts.Length - 1;
                    }
                    string iName = string.Join(" ", parts, 1, nameEnd - 1);
                    var allItems = Resources.FindObjectsOfTypeAll<ItemData>();
                    ItemData foundItem = null;
                    foreach (var it in allItems)
                    {
                        if (it.ItemName != null && it.ItemName.Equals(iName, System.StringComparison.OrdinalIgnoreCase))
                        { foundItem = it; break; }
                    }
                    if (foundItem == null)
                        foundItem = CreateTestItem(iName);
                    GameManager.Instance?.InventoryManager?.AddItem(foundItem, itemCount);
                    Log($"Added {foundItem.ItemName} x{itemCount}");
                }
                break;

            case "encounter":
                {
                    // Force a battle encounter. Creates a test formation if no ID given.
                    int enemyCount = 3;
                    string enemyName = "Goblin";
                    if (parts.Length >= 2) enemyName = parts[1];
                    if (parts.Length >= 3 && int.TryParse(parts[2], out int ec)) enemyCount = Mathf.Clamp(ec, 1, 9);

                    var enemyData = FindOrCreateTestEnemy(enemyName);
                    var formation = new EncounterFormation
                    {
                        FormationName = $"Debug: {enemyCount}x {enemyName}",
                        Groups = new[] { new EnemyGroup { Enemy = enemyData, Count = enemyCount } },
                        IsBoss = enemyName.ToLower().Contains("boss"),
                    };

                    Log($"Starting encounter: {enemyCount}x {enemyName}");
                    var encounterSys = Object.FindFirstObjectByType<EncounterSystem>();
                    if (encounterSys != null)
                        encounterSys.ForceEncounter(formation);
                    else if (BattleManager.Instance != null)
                        _ = BattleManager.Instance.StartBattle(formation);
                    else
                        Log("No BattleManager found!");
                }
                break;

            case "kill":
                {
                    var bm = BattleManager.Instance;
                    if (bm == null || bm.CurrentPhase == BattleManager.BattlePhase.Inactive)
                    { Log("Not in battle"); break; }
                    foreach (var enemy in bm.EnemyActors)
                    {
                        enemy.TakeDamage(enemy.CurrentHP);
                    }
                    Log("All enemies killed");
                }
                break;

            case "godmode":
                {
                    var bm = BattleManager.Instance;
                    if (bm == null) { Log("No BattleManager"); break; }
                    bm.SetGodMode(!bm.GodMode);
                    Log($"God mode: {(bm.GodMode ? "ON" : "OFF")}");
                }
                break;

            case "setstat":
                {
                    if (parts.Length < 4) { Log("Usage: setstat <slot 0-3> <stat> <value>"); break; }
                    if (!int.TryParse(parts[1], out int ssSlot) || !int.TryParse(parts[3], out int ssVal))
                    { Log("Invalid arguments"); break; }
                    var ssMember = GameManager.Instance?.PartyManager?.GetMember(ssSlot);
                    if (ssMember == null) { Log($"No member in slot {ssSlot}"); break; }

                    string stat = parts[2].ToLower();
                    switch (stat)
                    {
                        case "str": case "strength": ssMember.BaseStr = ssVal; break;
                        case "agi": case "agility": ssMember.BaseAgi = ssVal; break;
                        case "vit": case "vitality": ssMember.BaseVit = ssVal; break;
                        case "int": case "intellect": ssMember.BaseInt = ssVal; break;
                        case "luck": ssMember.BaseLuck = ssVal; break;
                        case "hp": ssMember.BaseMaxHP = ssVal; break;
                        case "mp": ssMember.BaseMaxMP = ssVal; break;
                        case "acc": case "accuracy": ssMember.BaseAccuracy = ssVal; break;
                        case "eva": case "evasion": ssMember.BaseEvasion = ssVal; break;
                        case "mdef": case "magicdef": ssMember.BaseMagicDef = ssVal; break;
                        default: Log($"Unknown stat: {stat}"); break;
                    }
                    ssMember.RecalculateStats();
                    Log($"{ssMember.Name}: {stat} set to {ssVal}");
                }
                break;

            case "inflict":
                {
                    if (parts.Length < 3) { Log("Usage: inflict <slot 0-3> <status>"); break; }
                    if (!int.TryParse(parts[1], out int infSlot))
                    { Log("Invalid slot"); break; }
                    var infMember = GameManager.Instance?.PartyManager?.GetMember(infSlot);
                    if (infMember == null) { Log($"No member in slot {infSlot}"); break; }

                    if (System.Enum.TryParse<StatusEffectFlags>(parts[2], true, out var infStatus))
                    {
                        infMember.StatusEffects |= infStatus;
                        if (infStatus == StatusEffectFlags.KO) infMember.CurrentHP = 0;
                        Log($"{infMember.Name}: inflicted {infStatus}");
                    }
                    else Log($"Unknown status: {parts[2]}");
                }
                break;

            case "cure":
                {
                    if (parts.Length < 2) { Log("Usage: cure <slot 0-3>"); break; }
                    if (!int.TryParse(parts[1], out int cureSlot))
                    { Log("Invalid slot"); break; }
                    var cureMember = GameManager.Instance?.PartyManager?.GetMember(cureSlot);
                    if (cureMember == null) { Log($"No member in slot {cureSlot}"); break; }

                    cureMember.StatusEffects = StatusEffectFlags.None;
                    if (cureMember.CurrentHP <= 0) cureMember.CurrentHP = cureMember.MaxHP;
                    Log($"{cureMember.Name}: all statuses cured, HP restored");
                }
                break;

            case "nobattles":
                {
                    var enc = Object.FindFirstObjectByType<EncounterSystem>();
                    if (enc != null)
                    {
                        enc.EncountersDisabled = !enc.EncountersDisabled;
                        Log($"Random encounters: {(enc.EncountersDisabled ? "OFF" : "ON")}");
                    }
                    else Log("No EncounterSystem found");
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

    EnemyData FindOrCreateTestEnemy(string name)
    {
        var allEnemies = Resources.FindObjectsOfTypeAll<EnemyData>();
        foreach (var e in allEnemies)
        {
            if (e.EnemyName != null && e.EnemyName.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                return e;
        }

        // Create a test enemy
        var enemy = ScriptableObject.CreateInstance<EnemyData>();
        enemy.name = name;
        enemy.EnemyName = name;
        enemy.Category = EnemyCategory.Normal;

        string lower = name.ToLower();
        if (lower.Contains("boss"))
        {
            enemy.Category = EnemyCategory.Boss;
            enemy.MaxHP = 200; enemy.Attack = 25; enemy.Defense = 10;
            enemy.Accuracy = 20; enemy.Evasion = 5; enemy.Agility = 10;
            enemy.EXPReward = 500; enemy.GilReward = 1000;
            enemy.PrimaryColor = new Color(0.6f, 0, 0); enemy.Shape = EnemyShape.Hexagon; enemy.SizeScale = 1.5f;
        }
        else if (lower.Contains("undead") || lower.Contains("zombie") || lower.Contains("skeleton"))
        {
            enemy.Category = EnemyCategory.Undead;
            enemy.MaxHP = 30; enemy.Attack = 12; enemy.Defense = 2;
            enemy.Accuracy = 10; enemy.Agility = 3;
            enemy.EXPReward = 30; enemy.GilReward = 20;
            enemy.PrimaryColor = new Color(0.4f, 0.5f, 0.3f); enemy.Shape = EnemyShape.Triangle;
            enemy.Affinities = new[] {
                new ElementalAffinityEntry { Element = Element.Fire, Affinity = ElementalAffinity.Weak },
                new ElementalAffinityEntry { Element = Element.Holy, Affinity = ElementalAffinity.Weak },
            };
        }
        else if (lower.Contains("dragon") || lower.Contains("wyrm"))
        {
            enemy.Category = EnemyCategory.Dragon;
            enemy.MaxHP = 100; enemy.Attack = 20; enemy.Defense = 8;
            enemy.Accuracy = 15; enemy.Evasion = 3; enemy.Agility = 8;
            enemy.EXPReward = 200; enemy.GilReward = 300;
            enemy.PrimaryColor = new Color(0.1f, 0.5f, 0.1f); enemy.Shape = EnemyShape.Diamond; enemy.SizeScale = 1.3f;
        }
        else
        {
            // Generic enemy
            enemy.MaxHP = 20; enemy.Attack = 8; enemy.Defense = 2;
            enemy.Accuracy = 10; enemy.Evasion = 2; enemy.Agility = 5;
            enemy.EXPReward = 15; enemy.GilReward = 10;
            enemy.PrimaryColor = new Color(0.7f, 0.2f, 0.2f); enemy.Shape = EnemyShape.Circle;
        }

        enemy.HitCount = 1;
        enemy.MagicDefense = 5;
        enemy.MagicEvasion = 5;
        enemy.Actions = new[] { new EnemyAction { Type = EnemyActionType.Attack, Weight = 10 } };

        return enemy;
    }

    EquipmentData CreateTestEquipment(string name)
    {
        var equip = ScriptableObject.CreateInstance<EquipmentData>();
        equip.name = name;
        equip.ItemName = name;
        equip.BuyPrice = 100;
        equip.SellPrice = 50;

        string lower = name.ToLower();
        if (lower.Contains("shield") || lower.Contains("buckler"))
        {
            equip.Slot = EquipmentSlot.Shield;
            equip.ArmorType = ArmorType.Shield;
            equip.Defense = 4; equip.Evasion = 2;
        }
        else if (lower.Contains("helm") || lower.Contains("cap"))
        {
            equip.Slot = EquipmentSlot.Helmet;
            equip.ArmorType = lower.Contains("heavy") ? ArmorType.HeavyHelmet : ArmorType.Hat;
            equip.Defense = 3;
        }
        else if (lower.Contains("armor") || lower.Contains("mail") || lower.Contains("robe"))
        {
            equip.Slot = EquipmentSlot.Armor;
            equip.ArmorType = lower.Contains("robe") ? ArmorType.Robe : ArmorType.HeavyArmor;
            equip.Defense = 8;
        }
        else // weapon
        {
            equip.Slot = EquipmentSlot.Weapon;
            if (lower.Contains("staff"))
            { equip.WeaponType = WeaponType.Staff; equip.Attack = 6; equip.Accuracy = 10; }
            else if (lower.Contains("dagger") || lower.Contains("knife"))
            { equip.WeaponType = WeaponType.Dagger; equip.Attack = 7; equip.Accuracy = 10; equip.CritRate = 3; }
            else if (lower.Contains("axe"))
            { equip.WeaponType = WeaponType.Axe; equip.Attack = 16; equip.Accuracy = 5; equip.TwoHanded = true; }
            else if (lower.Contains("hammer") || lower.Contains("mace"))
            { equip.WeaponType = WeaponType.Hammer; equip.Attack = 9; equip.Accuracy = 10; }
            else if (lower.Contains("nunchaku"))
            { equip.WeaponType = WeaponType.Nunchaku; equip.Attack = 12; equip.Accuracy = 10; }
            else if (lower.Contains("katana"))
            { equip.WeaponType = WeaponType.Katana; equip.Attack = 18; equip.Accuracy = 15; equip.CritRate = 10; }
            else // default to sword
            { equip.WeaponType = WeaponType.Sword; equip.Attack = 10; equip.Accuracy = 10; equip.CritRate = 5; }
        }
        return equip;
    }

    ItemData CreateTestItem(string name)
    {
        var item = ScriptableObject.CreateInstance<ItemData>();
        item.name = name;
        item.ItemName = name;
        item.UsableInField = true;
        item.UsableInBattle = true;
        item.Targeting = SpellTarget.SingleAlly;

        string lower = name.ToLower();
        if (lower.Contains("ether"))
        { item.EffectType = ItemEffectType.HealMP; item.Power = 30; item.BuyPrice = 1000; }
        else if (lower.Contains("phoenix") || lower.Contains("fenix"))
        { item.EffectType = ItemEffectType.Revive; item.Power = 25; item.BuyPrice = 5000; }
        else if (lower.Contains("antidote"))
        { item.EffectType = ItemEffectType.CureStatus; item.CuresStatus = StatusEffectFlags.Poison; item.BuyPrice = 75; }
        else if (lower.Contains("gold needle") || lower.Contains("soft"))
        { item.EffectType = ItemEffectType.CureStatus; item.CuresStatus = StatusEffectFlags.Stone; item.BuyPrice = 800; }
        else if (lower.Contains("eye drop"))
        { item.EffectType = ItemEffectType.CureStatus; item.CuresStatus = StatusEffectFlags.Blind; item.BuyPrice = 50; }
        else // Default: Potion
        { item.EffectType = ItemEffectType.HealHP; item.Power = 30; item.BuyPrice = 60; }

        item.SellPrice = item.BuyPrice / 2;
        return item;
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
