using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Master battle UI: party status, command menu, spell/item submenus,
/// enemy/ally targeting, damage popups, and battle log.
public class BattleUI : MonoBehaviour
{
    // UI containers built at runtime
    Canvas canvas;
    GameObject rootPanel;

    // Party status bar (bottom of screen)
    GameObject partyStatusPanel;
    TextMeshProUGUI[] partyNameTexts = new TextMeshProUGUI[4];
    TextMeshProUGUI[] partyHPTexts = new TextMeshProUGUI[4];
    TextMeshProUGUI[] partyMPTexts = new TextMeshProUGUI[4];

    // Command menu
    GameObject commandPanel;
    TextMeshProUGUI[] commandLabels;
    int commandIndex;
    string[] currentCommands;
    bool commandMenuActive;

    // Spell submenu
    GameObject spellPanel;
    TextMeshProUGUI[] spellLabels;
    int spellIndex;
    List<SpellData> currentSpells;
    bool spellMenuActive;

    // Item submenu
    GameObject itemPanel;
    TextMeshProUGUI[] itemLabels;
    int itemIndex;
    List<(ItemData item, int count)> currentItems;
    bool itemMenuActive;

    // Use (castable equipment) submenu
    GameObject usePanel;
    TextMeshProUGUI[] useLabels;
    int useIndex;
    List<(EquipmentData equip, SpellData spell)> currentUseItems;
    bool useMenuActive;

    // Target selection
    GameObject targetPanel;
    TextMeshProUGUI targetPromptText;
    int targetIndex;
    List<BattleActor> targetCandidates;
    bool targetSelectionActive;
    Action<BattleActor> onTargetSelected;

    // Damage popups
    List<DamagePopup> activePopups = new();

    // Battle log
    GameObject logPanel;
    TextMeshProUGUI logText;
    string logContent = "";

    // Auto-battle indicator
    TextMeshProUGUI autoBattleText;

    // Current input state
    BattleActor currentActor;
    BattleCommandType pendingCommandType;
    SpellData pendingSpell;
    ItemData pendingItem;

    // Scroll state for long lists
    int spellScrollOffset;
    int itemScrollOffset;
    const int MaxVisibleItems = 8;

    void Awake()
    {
        BuildUI();
        HideAll();
    }

    void OnEnable()
    {
        var bm = BattleManager.Instance;
        if (bm == null) return;
        bm.OnPhaseChanged += OnPhaseChanged;
        bm.OnCommandInputStart += OnCommandInputStart;
        bm.OnDamageDealt += OnDamageDealt;
        bm.OnHealing += OnHealingReceived;
        bm.OnBattleMessage += OnBattleMessage;
        bm.OnActorDefeated += OnActorDefeated;
        bm.OnActionExecuted += OnActionExecuted;

        // If battle already started before we subscribed, activate now
        if (bm.CurrentPhase != BattleManager.BattlePhase.Inactive)
        {
            rootPanel.SetActive(true);
            partyStatusPanel.SetActive(true);
        }
    }

    void OnDisable()
    {
        var bm = BattleManager.Instance;
        if (bm == null) return;
        bm.OnPhaseChanged -= OnPhaseChanged;
        bm.OnCommandInputStart -= OnCommandInputStart;
        bm.OnDamageDealt -= OnDamageDealt;
        bm.OnHealing -= OnHealingReceived;
        bm.OnBattleMessage -= OnBattleMessage;
        bm.OnActorDefeated -= OnActorDefeated;
        bm.OnActionExecuted -= OnActionExecuted;
    }

    void Update()
    {
        UpdatePartyStatus();
        UpdatePopups();
        HandleInput();

        // Auto-battle indicator
        if (autoBattleText != null)
        {
            bool show = BattleManager.Instance != null && BattleManager.Instance.IsAutoBattleActive;
            autoBattleText.gameObject.SetActive(show);
        }
    }

    // --- Event Handlers ---

    void OnPhaseChanged(BattleManager.BattlePhase phase)
    {
        switch (phase)
        {
            case BattleManager.BattlePhase.Starting:
                rootPanel.SetActive(true);
                partyStatusPanel.SetActive(true);
                break;
            case BattleManager.BattlePhase.Executing:
                HideMenus();
                break;
            case BattleManager.BattlePhase.Inactive:
                HideAll();
                break;
        }
    }

    void OnCommandInputStart(int slotIndex)
    {
        var bm = BattleManager.Instance;
        if (bm == null || slotIndex >= bm.PartyActors.Count) return;
        currentActor = bm.PartyActors[slotIndex];
        ShowCommandMenu();
    }

    void OnDamageDealt(BattleActor target, int amount)
    {
        SpawnDamagePopup(target, amount.ToString(), Color.white);
    }

    void OnHealingReceived(BattleActor target, int amount)
    {
        SpawnDamagePopup(target, amount.ToString(), Color.green);
    }

    void OnBattleMessage(BattleActor actor, string message)
    {
        if (actor != null)
            SpawnDamagePopup(actor, message, Color.yellow);
        AppendLog(message);
    }

    void OnActorDefeated(BattleActor actor)
    {
        if (actor.Visual != null)
            actor.Visual.SetActive(false);
    }

    void OnActionExecuted(BattleCommand command, List<ActionResult> results)
    {
        string actorName = command.Actor?.Name ?? "???";
        foreach (var r in results)
        {
            if (r.Hit && r.TotalDamage > 0 && !r.IsHealing)
            {
                string crit = r.IsCritical ? " Critical!" : "";
                AppendLog($"{actorName} hits {r.Target.Name} for {r.TotalDamage}{crit}");
            }
            else if (r.IsHealing && r.TotalDamage > 0)
            {
                AppendLog($"{r.Target.Name} healed {r.TotalDamage} HP");
            }
        }
    }

    // --- Input Handling ---

    void HandleInput()
    {
        var input = GameManager.Instance?.InputManager;
        if (input == null) return;

        // Auto-battle toggle (Tab key / check keyboard directly)
        if (UnityEngine.InputSystem.Keyboard.current != null
            && UnityEngine.InputSystem.Keyboard.current.tabKey.wasPressedThisFrame)
        {
            BattleManager.Instance?.ToggleAutoBattle();
        }

        if (commandMenuActive) HandleCommandInput(input);
        else if (spellMenuActive) HandleSpellInput(input);
        else if (itemMenuActive) HandleItemInput(input);
        else if (useMenuActive) HandleUseInput(input);
        else if (targetSelectionActive) HandleTargetInput(input);
    }

    void HandleCommandInput(InputManager input)
    {
        if (input.MoveAction != null && input.MoveAction.WasPressedThisFrame())
        {
            Vector2 dir = input.MoveAction.ReadValue<Vector2>();
            if (dir.y > 0.5f) NavigateCommand(-1);
            else if (dir.y < -0.5f) NavigateCommand(1);
        }

        if (input.ConfirmAction != null && input.ConfirmAction.WasPressedThisFrame())
        {
            ConfirmCommand();
        }

        if (input.CancelAction != null && input.CancelAction.WasPressedThisFrame())
        {
            // Cancel backs up to previous party member
            // For now just play cancel sound
            GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cancel);
        }
    }

    void HandleSpellInput(InputManager input)
    {
        if (input.MoveAction != null && input.MoveAction.WasPressedThisFrame())
        {
            Vector2 dir = input.MoveAction.ReadValue<Vector2>();
            if (dir.y > 0.5f) NavigateSpell(-1);
            else if (dir.y < -0.5f) NavigateSpell(1);
        }

        if (input.ConfirmAction != null && input.ConfirmAction.WasPressedThisFrame())
            ConfirmSpell();

        if (input.CancelAction != null && input.CancelAction.WasPressedThisFrame())
        {
            spellMenuActive = false;
            spellPanel.SetActive(false);
            ShowCommandMenu();
        }
    }

    void HandleItemInput(InputManager input)
    {
        if (input.MoveAction != null && input.MoveAction.WasPressedThisFrame())
        {
            Vector2 dir = input.MoveAction.ReadValue<Vector2>();
            if (dir.y > 0.5f) NavigateItem(-1);
            else if (dir.y < -0.5f) NavigateItem(1);
        }

        if (input.ConfirmAction != null && input.ConfirmAction.WasPressedThisFrame())
            ConfirmItem();

        if (input.CancelAction != null && input.CancelAction.WasPressedThisFrame())
        {
            itemMenuActive = false;
            itemPanel.SetActive(false);
            ShowCommandMenu();
        }
    }

    void HandleUseInput(InputManager input)
    {
        if (input.MoveAction != null && input.MoveAction.WasPressedThisFrame())
        {
            Vector2 dir = input.MoveAction.ReadValue<Vector2>();
            if (dir.y > 0.5f) NavigateUse(-1);
            else if (dir.y < -0.5f) NavigateUse(1);
        }

        if (input.ConfirmAction != null && input.ConfirmAction.WasPressedThisFrame())
            ConfirmUse();

        if (input.CancelAction != null && input.CancelAction.WasPressedThisFrame())
        {
            useMenuActive = false;
            usePanel.SetActive(false);
            ShowCommandMenu();
        }
    }

    void HandleTargetInput(InputManager input)
    {
        if (input.MoveAction != null && input.MoveAction.WasPressedThisFrame())
        {
            Vector2 dir = input.MoveAction.ReadValue<Vector2>();
            if (dir.y > 0.5f || dir.x < -0.5f) NavigateTarget(-1);
            else if (dir.y < -0.5f || dir.x > 0.5f) NavigateTarget(1);
        }

        if (input.ConfirmAction != null && input.ConfirmAction.WasPressedThisFrame())
        {
            if (targetCandidates != null && targetIndex >= 0 && targetIndex < targetCandidates.Count)
            {
                targetSelectionActive = false;
                targetPanel.SetActive(false);
                GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Confirm);
                onTargetSelected?.Invoke(targetCandidates[targetIndex]);
            }
        }

        if (input.CancelAction != null && input.CancelAction.WasPressedThisFrame())
        {
            targetSelectionActive = false;
            targetPanel.SetActive(false);

            // Return to the previous menu
            if (pendingCommandType == BattleCommandType.Magic) ShowSpellMenu();
            else if (pendingCommandType == BattleCommandType.Item) ShowItemMenu();
            else if (pendingCommandType == BattleCommandType.Use) ShowUseMenu();
            else ShowCommandMenu();
        }
    }

    // --- Command Menu ---

    void ShowCommandMenu()
    {
        HideMenus();
        commandMenuActive = true;

        var commands = new List<string> { "Attack", "Magic", "Item", "Defend" };

        // Use command: only if character has equipment with CastableSpell
        if (currentActor != null && currentActor.GetCastableSpells().Count > 0)
            commands.Insert(2, "Use");

        // Flee: hidden for boss encounters
        var bm = BattleManager.Instance;
        if (bm != null && !bm.IsBossEncounter)
            commands.Add("Flee");

        currentCommands = commands.ToArray();
        commandIndex = 0;

        // Update labels
        for (int i = 0; i < commandLabels.Length; i++)
        {
            if (i < currentCommands.Length)
            {
                commandLabels[i].gameObject.SetActive(true);
                commandLabels[i].text = (i == commandIndex ? "> " : "  ") + currentCommands[i];
            }
            else
            {
                commandLabels[i].gameObject.SetActive(false);
            }
        }

        commandPanel.SetActive(true);
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cursor);
    }

    void NavigateCommand(int dir)
    {
        commandIndex = (commandIndex + dir + currentCommands.Length) % currentCommands.Length;
        for (int i = 0; i < commandLabels.Length && i < currentCommands.Length; i++)
            commandLabels[i].text = (i == commandIndex ? "> " : "  ") + currentCommands[i];
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cursor);
    }

    void ConfirmCommand()
    {
        if (currentCommands == null || commandIndex >= currentCommands.Length) return;
        string cmd = currentCommands[commandIndex];
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Confirm);

        switch (cmd)
        {
            case "Attack":
                pendingCommandType = BattleCommandType.Attack;
                commandMenuActive = false;
                commandPanel.SetActive(false);
                StartTargetSelection(BattleManager.Instance.GetLivingEnemies(), target =>
                {
                    BattleManager.Instance.SubmitCommand(new BattleCommand
                    {
                        Type = BattleCommandType.Attack,
                        Actor = currentActor,
                        Target = target,
                        Targets = new List<BattleActor> { target },
                    });
                });
                break;

            case "Magic":
                ShowSpellMenu();
                break;

            case "Use":
                ShowUseMenu();
                break;

            case "Item":
                ShowItemMenu();
                break;

            case "Defend":
                commandMenuActive = false;
                commandPanel.SetActive(false);
                BattleManager.Instance.SubmitCommand(new BattleCommand
                {
                    Type = BattleCommandType.Defend,
                    Actor = currentActor,
                    Targets = new List<BattleActor>(),
                });
                break;

            case "Flee":
                commandMenuActive = false;
                commandPanel.SetActive(false);
                BattleManager.Instance.SubmitCommand(new BattleCommand
                {
                    Type = BattleCommandType.Flee,
                    Actor = currentActor,
                    Targets = new List<BattleActor>(),
                });
                break;
        }
    }

    // --- Spell Menu ---

    void ShowSpellMenu()
    {
        HideMenus();
        spellMenuActive = true;

        currentSpells = new List<SpellData>();
        if (currentActor?.IsPartyMember == true)
        {
            foreach (var spell in currentActor.PartyMember.KnownSpells)
            {
                if (spell != null && spell.UsableInBattle)
                    currentSpells.Add(spell);
            }
        }

        spellIndex = 0;
        spellScrollOffset = 0;
        RefreshSpellLabels();
        spellPanel.SetActive(true);
    }

    void RefreshSpellLabels()
    {
        for (int i = 0; i < spellLabels.Length; i++)
        {
            int dataIndex = spellScrollOffset + i;
            if (dataIndex < currentSpells.Count)
            {
                var sp = currentSpells[dataIndex];
                bool canAfford = currentActor.IsPartyMember && currentActor.PartyMember.CurrentMP >= sp.MPCost;
                string prefix = (dataIndex == spellIndex) ? "> " : "  ";
                string mpStr = $"  {sp.MPCost}MP";
                spellLabels[i].text = prefix + sp.SpellName + mpStr;
                spellLabels[i].color = canAfford ? Color.white : new Color(0.5f, 0.5f, 0.5f);
                spellLabels[i].gameObject.SetActive(true);
            }
            else
            {
                spellLabels[i].gameObject.SetActive(false);
            }
        }
    }

    void NavigateSpell(int dir)
    {
        if (currentSpells.Count == 0) return;
        spellIndex = (spellIndex + dir + currentSpells.Count) % currentSpells.Count;

        // Scroll
        if (spellIndex < spellScrollOffset) spellScrollOffset = spellIndex;
        if (spellIndex >= spellScrollOffset + MaxVisibleItems) spellScrollOffset = spellIndex - MaxVisibleItems + 1;

        RefreshSpellLabels();
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cursor);
    }

    void ConfirmSpell()
    {
        if (currentSpells.Count == 0 || spellIndex >= currentSpells.Count) return;
        var spell = currentSpells[spellIndex];

        // MP check
        if (currentActor.IsPartyMember && currentActor.PartyMember.CurrentMP < spell.MPCost)
        {
            GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Error);
            return;
        }

        pendingSpell = spell;
        pendingCommandType = BattleCommandType.Magic;
        spellMenuActive = false;
        spellPanel.SetActive(false);
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Confirm);

        SelectTargetForSpell(spell, targets =>
        {
            BattleManager.Instance.SubmitCommand(new BattleCommand
            {
                Type = BattleCommandType.Magic,
                Actor = currentActor,
                Spell = spell,
                Target = targets.Count == 1 ? targets[0] : null,
                Targets = targets,
            });
        });
    }

    // --- Use Menu (Castable Equipment Spells) ---

    void ShowUseMenu()
    {
        HideMenus();
        useMenuActive = true;

        currentUseItems = currentActor?.GetCastableSpells() ?? new();
        useIndex = 0;

        for (int i = 0; i < useLabels.Length; i++)
        {
            if (i < currentUseItems.Count)
            {
                string prefix = (i == useIndex) ? "> " : "  ";
                useLabels[i].text = $"{prefix}{currentUseItems[i].equip.ItemName} -> {currentUseItems[i].spell.SpellName}";
                useLabels[i].gameObject.SetActive(true);
            }
            else
            {
                useLabels[i].gameObject.SetActive(false);
            }
        }

        usePanel.SetActive(true);
    }

    void NavigateUse(int dir)
    {
        if (currentUseItems.Count == 0) return;
        useIndex = (useIndex + dir + currentUseItems.Count) % currentUseItems.Count;
        for (int i = 0; i < useLabels.Length && i < currentUseItems.Count; i++)
        {
            string prefix = (i == useIndex) ? "> " : "  ";
            useLabels[i].text = $"{prefix}{currentUseItems[i].equip.ItemName} -> {currentUseItems[i].spell.SpellName}";
        }
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cursor);
    }

    void ConfirmUse()
    {
        if (currentUseItems.Count == 0 || useIndex >= currentUseItems.Count) return;
        var (equip, spell) = currentUseItems[useIndex];

        pendingSpell = spell;
        pendingCommandType = BattleCommandType.Use;
        useMenuActive = false;
        usePanel.SetActive(false);
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Confirm);

        SelectTargetForSpell(spell, targets =>
        {
            BattleManager.Instance.SubmitCommand(new BattleCommand
            {
                Type = BattleCommandType.Use,
                Actor = currentActor,
                Spell = spell,
                Target = targets.Count == 1 ? targets[0] : null,
                Targets = targets,
            });
        });
    }

    // --- Item Menu ---

    void ShowItemMenu()
    {
        HideMenus();
        itemMenuActive = true;

        currentItems = new List<(ItemData, int)>();
        var inv = GameManager.Instance?.InventoryManager;
        if (inv != null)
        {
            foreach (var slot in inv.GetAllItems())
            {
                if (slot.Item != null && slot.Item.UsableInBattle)
                    currentItems.Add((slot.Item, slot.Count));
            }
        }

        itemIndex = 0;
        itemScrollOffset = 0;
        RefreshItemLabels();
        itemPanel.SetActive(true);
    }

    void RefreshItemLabels()
    {
        for (int i = 0; i < itemLabels.Length; i++)
        {
            int dataIndex = itemScrollOffset + i;
            if (dataIndex < currentItems.Count)
            {
                var (item, count) = currentItems[dataIndex];
                string prefix = (dataIndex == itemIndex) ? "> " : "  ";
                itemLabels[i].text = $"{prefix}{item.ItemName}  x{count}";
                itemLabels[i].color = Color.white;
                itemLabels[i].gameObject.SetActive(true);
            }
            else
            {
                itemLabels[i].gameObject.SetActive(false);
            }
        }
    }

    void NavigateItem(int dir)
    {
        if (currentItems.Count == 0) return;
        itemIndex = (itemIndex + dir + currentItems.Count) % currentItems.Count;

        if (itemIndex < itemScrollOffset) itemScrollOffset = itemIndex;
        if (itemIndex >= itemScrollOffset + MaxVisibleItems) itemScrollOffset = itemIndex - MaxVisibleItems + 1;

        RefreshItemLabels();
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cursor);
    }

    void ConfirmItem()
    {
        if (currentItems.Count == 0 || itemIndex >= currentItems.Count) return;
        var (item, _) = currentItems[itemIndex];

        pendingItem = item;
        pendingCommandType = BattleCommandType.Item;
        itemMenuActive = false;
        itemPanel.SetActive(false);
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Confirm);

        // Determine targets based on item targeting
        var bm = BattleManager.Instance;
        List<BattleActor> candidates;
        bool isAoE = false;

        switch (item.Targeting)
        {
            case SpellTarget.SingleEnemy:
                candidates = bm.GetLivingEnemies();
                break;
            case SpellTarget.AllEnemies:
                candidates = bm.GetLivingEnemies();
                isAoE = true;
                break;
            case SpellTarget.AllAllies:
                candidates = bm.GetLivingParty();
                isAoE = true;
                break;
            case SpellTarget.Self:
                candidates = new List<BattleActor> { currentActor };
                isAoE = true;
                break;
            default: // SingleAlly
                candidates = new List<BattleActor>(bm.PartyActors); // include KO'd for revive
                break;
        }

        if (isAoE)
        {
            // AoE: no target selection needed
            BattleManager.Instance.SubmitCommand(new BattleCommand
            {
                Type = BattleCommandType.Item,
                Actor = currentActor,
                Item = item,
                Targets = candidates,
            });
        }
        else
        {
            StartTargetSelection(candidates, target =>
            {
                BattleManager.Instance.SubmitCommand(new BattleCommand
                {
                    Type = BattleCommandType.Item,
                    Actor = currentActor,
                    Item = item,
                    Target = target,
                    Targets = new List<BattleActor> { target },
                });
            });
        }
    }

    // --- Target Selection ---

    void SelectTargetForSpell(SpellData spell, Action<List<BattleActor>> onSelected)
    {
        var bm = BattleManager.Instance;
        switch (spell.Targeting)
        {
            case SpellTarget.SingleEnemy:
                StartTargetSelection(bm.GetLivingEnemies(), t =>
                    onSelected(new List<BattleActor> { t }));
                break;
            case SpellTarget.AllEnemies:
                onSelected(bm.GetLivingEnemies());
                break;
            case SpellTarget.SingleAlly:
                StartTargetSelection(new List<BattleActor>(bm.PartyActors), t =>
                    onSelected(new List<BattleActor> { t }));
                break;
            case SpellTarget.AllAllies:
                onSelected(bm.GetLivingParty());
                break;
            case SpellTarget.Self:
                onSelected(new List<BattleActor> { currentActor });
                break;
        }
    }

    void StartTargetSelection(List<BattleActor> candidates, Action<BattleActor> onSelected)
    {
        targetCandidates = candidates;
        targetIndex = 0;
        this.onTargetSelected = onSelected;
        targetSelectionActive = true;

        // Update target display
        string names = "";
        for (int i = 0; i < candidates.Count; i++)
        {
            string prefix = (i == targetIndex) ? "> " : "  ";
            names += prefix + candidates[i].Name + "\n";
        }
        targetPromptText.text = names;
        targetPanel.SetActive(true);
    }

    void NavigateTarget(int dir)
    {
        if (targetCandidates == null || targetCandidates.Count == 0) return;
        targetIndex = (targetIndex + dir + targetCandidates.Count) % targetCandidates.Count;

        string names = "";
        for (int i = 0; i < targetCandidates.Count; i++)
        {
            string prefix = (i == targetIndex) ? "> " : "  ";
            string extra = "";
            if (targetCandidates[i].IsEnemy)
                extra = $"  HP:{targetCandidates[i].CurrentHP}/{targetCandidates[i].MaxHP}";
            names += prefix + targetCandidates[i].Name + extra + "\n";
        }
        targetPromptText.text = names;
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Cursor);
    }

    // --- Damage Popups ---

    void SpawnDamagePopup(BattleActor target, string text, Color color)
    {
        if (target?.Visual == null && !target.IsPartyMember) return;

        Vector3 worldPos;
        if (target.Visual != null)
            worldPos = target.Visual.transform.position + Vector3.up * 0.8f;
        else
            worldPos = target.HomePosition + Vector3.up * 0.8f;

        var go = new GameObject("DamagePopup");
        go.transform.SetParent(canvas.transform, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 28;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        var rect = tmp.rectTransform;
        rect.sizeDelta = new Vector2(200, 50);

        // Convert world position to screen position
        var cam = Camera.main;
        if (cam != null)
        {
            var screenPos = cam.WorldToScreenPoint(worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(), screenPos, null, out var localPos);
            rect.anchoredPosition = localPos;
        }
        else
        {
            // Fallback: position based on slot
            float x = target.IsPartyMember ? 300 + target.SlotIndex * 100 : -200 + target.SlotIndex * 80;
            rect.anchoredPosition = new Vector2(x, 100 + UnityEngine.Random.Range(-20f, 20f));
        }

        activePopups.Add(new DamagePopup { Go = go, Timer = BattleConfig.Instance.DamagePopupDuration });
    }

    void UpdatePopups()
    {
        for (int i = activePopups.Count - 1; i >= 0; i--)
        {
            var popup = activePopups[i];
            popup.Timer -= Time.deltaTime;

            if (popup.Go != null)
            {
                var rect = popup.Go.GetComponent<RectTransform>();
                rect.anchoredPosition += Vector2.up * 40f * Time.deltaTime;
                var tmp = popup.Go.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    var c = tmp.color;
                    c.a = Mathf.Clamp01(popup.Timer / 0.3f);
                    tmp.color = c;
                }
            }

            if (popup.Timer <= 0)
            {
                if (popup.Go != null) Destroy(popup.Go);
                activePopups.RemoveAt(i);
            }
        }
    }

    // --- Party Status ---

    void UpdatePartyStatus()
    {
        var bm = BattleManager.Instance;
        if (bm == null) return;

        for (int i = 0; i < 4; i++)
        {
            if (i < bm.PartyActors.Count)
            {
                var actor = bm.PartyActors[i];
                partyNameTexts[i].text = actor.Name;
                partyHPTexts[i].text = $"HP {actor.CurrentHP}/{actor.MaxHP}";
                partyMPTexts[i].text = actor.IsPartyMember ? $"MP {actor.PartyMember.CurrentMP}/{actor.PartyMember.MaxMP}" : "";

                partyNameTexts[i].color = actor.IsAlive ? Color.white : new Color(0.5f, 0.2f, 0.2f);
            }
            else
            {
                partyNameTexts[i].text = "";
                partyHPTexts[i].text = "";
                partyMPTexts[i].text = "";
            }
        }
    }

    // --- Log ---

    void AppendLog(string message)
    {
        logContent += message + "\n";
        var lines = logContent.Split('\n');
        if (lines.Length > 6)
            logContent = string.Join("\n", lines, lines.Length - 6, 6);
        if (logText != null) logText.text = logContent;
    }

    // --- Helpers ---

    void HideAll()
    {
        if (rootPanel != null) rootPanel.SetActive(false);
    }

    void HideMenus()
    {
        commandMenuActive = false;
        spellMenuActive = false;
        itemMenuActive = false;
        useMenuActive = false;
        targetSelectionActive = false;
        if (commandPanel != null) commandPanel.SetActive(false);
        if (spellPanel != null) spellPanel.SetActive(false);
        if (itemPanel != null) itemPanel.SetActive(false);
        if (usePanel != null) usePanel.SetActive(false);
        if (targetPanel != null) targetPanel.SetActive(false);
    }

    // --- UI Building ---

    void BuildUI()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
        }
        if (GetComponent<CanvasScaler>() == null)
        {
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
        }
        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();

        rootPanel = new GameObject("BattleUIRoot");
        var rootRect = rootPanel.AddComponent<RectTransform>();
        rootRect.SetParent(canvas.transform, false);
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.sizeDelta = Vector2.zero;

        // Party status panel (bottom bar)
        BuildPartyStatusPanel(rootRect);

        // Command menu (bottom-left)
        BuildCommandPanel(rootRect);

        // Spell menu
        BuildSpellPanel(rootRect);

        // Item menu
        BuildItemPanel(rootRect);

        // Use menu
        BuildUsePanel(rootRect);

        // Target selection
        BuildTargetPanel(rootRect);

        // Battle log (top-left)
        BuildLogPanel(rootRect);

        // Auto-battle indicator
        BuildAutoBattleIndicator(rootRect);
    }

    void BuildPartyStatusPanel(RectTransform parent)
    {
        partyStatusPanel = CreateWindow("PartyStatus", parent);
        var rect = partyStatusPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(1f, 0.25f);
        rect.offsetMin = new Vector2(5, 5);
        rect.offsetMax = new Vector2(-5, 0);

        var layout = partyStatusPanel.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 2;
        layout.padding = new RectOffset(10, 10, 8, 8);
        layout.childForceExpandWidth = true;
        layout.childControlHeight = false;

        for (int i = 0; i < 4; i++)
        {
            var row = new GameObject($"Member_{i}");
            row.transform.SetParent(partyStatusPanel.transform, false);
            var rowRect = row.AddComponent<RectTransform>();
            var rowLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 10;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childControlWidth = false;
            var rowLE = row.AddComponent<LayoutElement>();
            rowLE.preferredHeight = 30;

            partyNameTexts[i] = CreateTextInParent("Name", row.transform, 120, 16);
            partyHPTexts[i] = CreateTextInParent("HP", row.transform, 120, 16);
            partyMPTexts[i] = CreateTextInParent("MP", row.transform, 100, 16);
        }
    }

    void BuildCommandPanel(RectTransform parent)
    {
        commandPanel = CreateWindow("CommandMenu", parent);
        var rect = commandPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0.25f, 0.35f);
        rect.offsetMin = new Vector2(5, 5);
        rect.offsetMax = new Vector2(0, 0);

        var layout = commandPanel.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 4;
        layout.padding = new RectOffset(10, 10, 8, 8);
        layout.childForceExpandWidth = true;
        layout.childControlHeight = false;

        commandLabels = new TextMeshProUGUI[7]; // max: Attack, Magic, Use, Item, Defend, Flee + spare
        for (int i = 0; i < commandLabels.Length; i++)
        {
            var go = CreateTextGO($"Cmd_{i}", "  ---", 18, TextAlignmentOptions.Left);
            go.transform.SetParent(commandPanel.transform, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 28;
            commandLabels[i] = go.GetComponent<TextMeshProUGUI>();
        }
    }

    void BuildSpellPanel(RectTransform parent)
    {
        spellPanel = CreateWindow("SpellMenu", parent);
        var rect = spellPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.25f, 0f);
        rect.anchorMax = new Vector2(0.55f, 0.45f);
        rect.offsetMin = new Vector2(3, 5);
        rect.offsetMax = new Vector2(0, 0);

        var layout = spellPanel.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 2;
        layout.padding = new RectOffset(10, 10, 8, 8);
        layout.childForceExpandWidth = true;
        layout.childControlHeight = false;

        spellLabels = new TextMeshProUGUI[MaxVisibleItems];
        for (int i = 0; i < MaxVisibleItems; i++)
        {
            var go = CreateTextGO($"Spell_{i}", "", 16, TextAlignmentOptions.Left);
            go.transform.SetParent(spellPanel.transform, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 26;
            spellLabels[i] = go.GetComponent<TextMeshProUGUI>();
        }

        spellPanel.SetActive(false);
    }

    void BuildItemPanel(RectTransform parent)
    {
        itemPanel = CreateWindow("ItemMenu", parent);
        var rect = itemPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.25f, 0f);
        rect.anchorMax = new Vector2(0.55f, 0.45f);
        rect.offsetMin = new Vector2(3, 5);
        rect.offsetMax = new Vector2(0, 0);

        var layout = itemPanel.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 2;
        layout.padding = new RectOffset(10, 10, 8, 8);
        layout.childForceExpandWidth = true;
        layout.childControlHeight = false;

        itemLabels = new TextMeshProUGUI[MaxVisibleItems];
        for (int i = 0; i < MaxVisibleItems; i++)
        {
            var go = CreateTextGO($"Item_{i}", "", 16, TextAlignmentOptions.Left);
            go.transform.SetParent(itemPanel.transform, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 26;
            itemLabels[i] = go.GetComponent<TextMeshProUGUI>();
        }

        itemPanel.SetActive(false);
    }

    void BuildUsePanel(RectTransform parent)
    {
        usePanel = CreateWindow("UseMenu", parent);
        var rect = usePanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.25f, 0f);
        rect.anchorMax = new Vector2(0.55f, 0.35f);
        rect.offsetMin = new Vector2(3, 5);
        rect.offsetMax = new Vector2(0, 0);

        var layout = usePanel.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 2;
        layout.padding = new RectOffset(10, 10, 8, 8);
        layout.childForceExpandWidth = true;
        layout.childControlHeight = false;

        useLabels = new TextMeshProUGUI[4]; // max 4 equipment slots
        for (int i = 0; i < useLabels.Length; i++)
        {
            var go = CreateTextGO($"Use_{i}", "", 16, TextAlignmentOptions.Left);
            go.transform.SetParent(usePanel.transform, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 26;
            useLabels[i] = go.GetComponent<TextMeshProUGUI>();
        }

        usePanel.SetActive(false);
    }

    void BuildTargetPanel(RectTransform parent)
    {
        targetPanel = CreateWindow("TargetSelect", parent);
        var rect = targetPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.35f);
        rect.anchorMax = new Vector2(0.3f, 0.7f);
        rect.offsetMin = new Vector2(5, 3);
        rect.offsetMax = new Vector2(0, 0);

        targetPromptText = CreateTextGO("TargetPrompt", "", 16, TextAlignmentOptions.Left)
            .GetComponent<TextMeshProUGUI>();
        targetPromptText.transform.SetParent(targetPanel.transform, false);
        var tRect = targetPromptText.rectTransform;
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.offsetMin = new Vector2(10, 5);
        tRect.offsetMax = new Vector2(-10, -5);

        targetPanel.SetActive(false);
    }

    void BuildLogPanel(RectTransform parent)
    {
        logPanel = CreateWindow("BattleLog", parent);
        var rect = logPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.75f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.offsetMin = new Vector2(5, 0);
        rect.offsetMax = new Vector2(0, -5);

        logText = CreateTextGO("LogText", "", 14, TextAlignmentOptions.TopLeft)
            .GetComponent<TextMeshProUGUI>();
        logText.transform.SetParent(logPanel.transform, false);
        var lRect = logText.rectTransform;
        lRect.anchorMin = Vector2.zero;
        lRect.anchorMax = Vector2.one;
        lRect.offsetMin = new Vector2(8, 5);
        lRect.offsetMax = new Vector2(-8, -5);
    }

    void BuildAutoBattleIndicator(RectTransform parent)
    {
        var go = CreateTextGO("AutoBattle", "AUTO", 20, TextAlignmentOptions.Center);
        go.transform.SetParent(parent, false);
        autoBattleText = go.GetComponent<TextMeshProUGUI>();
        autoBattleText.color = Color.yellow;
        var rect = autoBattleText.rectTransform;
        rect.anchorMin = new Vector2(0.85f, 0.9f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = new Vector2(-5, -5);
        go.SetActive(false);
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

    TextMeshProUGUI CreateTextInParent(string name, Transform parent, float width, float fontSize)
    {
        var go = CreateTextGO(name, "", fontSize, TextAlignmentOptions.Left);
        go.transform.SetParent(parent, false);
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = width;
        return go.GetComponent<TextMeshProUGUI>();
    }

    GameObject CreateTextGO(string name, string text, float fontSize, TextAlignmentOptions align)
    {
        var go = new GameObject(name);
        go.AddComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = align;
        tmp.raycastTarget = false;
        return go;
    }

    class DamagePopup
    {
        public GameObject Go;
        public float Timer;
    }
}
