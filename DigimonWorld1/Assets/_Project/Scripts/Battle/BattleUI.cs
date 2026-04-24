using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class BattleUI : MonoBehaviour
{
    [SerializeField] private GameObject _battlePanel;
    [SerializeField] private BattleSystem _battleSystem;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private TMP_Text _partnerNameText;
    [SerializeField] private TMP_Text _partnerHPText;
    [SerializeField] private TMP_Text _partnerMPText;
    [SerializeField] private TMP_Text _partnerStatusText;
    [SerializeField] private TMP_Text _enemyNameText;
    [SerializeField] private TMP_Text _enemyHPText;
    [SerializeField] private TMP_Text _enemyMPText;
    [SerializeField] private TMP_Text _enemyStatusText;
    [SerializeField] private GameObject _commandPanel;
    [SerializeField] private TMP_Text _commandListText;
    [SerializeField] private GameObject _techniquePanel;
    [SerializeField] private TMP_Text _techniqueListText;
    [SerializeField] private GameObject _itemPanel;
    [SerializeField] private TMP_Text _itemListText;
    [SerializeField] private TMP_Text _battleLogText;
    [SerializeField] private TMP_Text _instructionsText;

    private enum SubMenu { Commands, Techniques, Items }

    private static readonly string[] CommandNames = { "Attack", "Technique", "Item", "Flee", "Auto" };

    private SubMenu _currentSubMenu;
    private int _selectedIndex;
    private bool _waitingForInput;
    private DigimonInstance _partner;
    private WildDigimonInstance _enemy;
    private List<string> _logLines = new List<string>();
    private List<ItemData> _battleItems = new List<ItemData>();

    private void Awake()
    {
        if (_battlePanel != null)
            _battlePanel.SetActive(false);
    }

    public void Show(DigimonInstance partner, WildDigimonInstance enemy)
    {
        _partner = partner;
        _enemy = enemy;
        _logLines.Clear();
        _battlePanel.SetActive(true);
        RefreshStats(partner, enemy);
        UpdateLogDisplay();
    }

    public void Hide()
    {
        _waitingForInput = false;
        _battlePanel.SetActive(false);
        _partner = null;
        _enemy = null;
    }

    public void RefreshStats(DigimonInstance partner, WildDigimonInstance enemy)
    {
        if (partner != null && partner.Species != null)
        {
            _partnerNameText.text = partner.Species.SpeciesName;
            _partnerHPText.text = $"HP {partner.CurrentHP} / {partner.MaxHP}";
            _partnerMPText.text = $"MP {partner.CurrentMP} / {partner.MaxMP}";
            _partnerStatusText.text = _battleSystem.GetPartnerStatusText();
        }

        if (enemy != null && enemy.Species != null)
        {
            _enemyNameText.text = enemy.Species.SpeciesName;
            _enemyHPText.text = $"HP {enemy.CurrentHP} / {enemy.MaxHP}";
            _enemyMPText.text = $"MP {enemy.CurrentMP} / {enemy.MaxMP}";
            _enemyStatusText.text = _battleSystem.GetEnemyStatusText();
        }
    }

    public void ShowCommandMenu()
    {
        _currentSubMenu = SubMenu.Commands;
        _selectedIndex = 0;
        _waitingForInput = true;
        _commandPanel.SetActive(true);
        _techniquePanel.SetActive(false);
        _itemPanel.SetActive(false);
        RefreshCommandList();
        _instructionsText.text = "W/S: Navigate  E: Confirm";
    }

    public void HideCommandMenu()
    {
        _waitingForInput = false;
        _commandPanel.SetActive(false);
        _techniquePanel.SetActive(false);
        _itemPanel.SetActive(false);
    }

    public void AddLogLine(string message)
    {
        _logLines.Add(message);
        if (_logLines.Count > 8)
            _logLines.RemoveAt(0);
        UpdateLogDisplay();
    }

    private void Update()
    {
        if (!_waitingForInput) return;

        if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
            Navigate(-1);
        if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
            Navigate(1);
        if (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
            Confirm();
        if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.qKey.wasPressedThisFrame)
            Back();
    }

    private void Navigate(int direction)
    {
        int max = GetMaxIndex();
        if (max <= 0) return;
        _selectedIndex = Mathf.Clamp(_selectedIndex + direction, 0, max - 1);
        RefreshCurrentMenu();
    }

    private void Confirm()
    {
        switch (_currentSubMenu)
        {
            case SubMenu.Commands:
                ConfirmCommand();
                break;
            case SubMenu.Techniques:
                ConfirmTechnique();
                break;
            case SubMenu.Items:
                ConfirmItem();
                break;
        }
    }

    private void Back()
    {
        if (_currentSubMenu == SubMenu.Commands) return;
        _currentSubMenu = SubMenu.Commands;
        _selectedIndex = 0;
        _commandPanel.SetActive(true);
        _techniquePanel.SetActive(false);
        _itemPanel.SetActive(false);
        RefreshCommandList();
        _instructionsText.text = "W/S: Navigate  E: Confirm";
    }

    private void ConfirmCommand()
    {
        switch (_selectedIndex)
        {
            case 0: // Attack
                _battleSystem.SelectAttack();
                break;
            case 1: // Technique
                OpenTechniqueMenu();
                break;
            case 2: // Item
                OpenItemMenu();
                break;
            case 3: // Flee
                _battleSystem.SelectFlee();
                break;
            case 4: // Auto
                _battleSystem.SelectAuto();
                break;
        }
    }

    private void OpenTechniqueMenu()
    {
        if (_partner == null || _partner.KnownTechniques.Count == 0)
        {
            AddLogLine("No techniques known!");
            return;
        }

        _currentSubMenu = SubMenu.Techniques;
        _selectedIndex = 0;
        _commandPanel.SetActive(false);
        _techniquePanel.SetActive(true);
        RefreshTechniqueList();
        _instructionsText.text = "W/S: Navigate  E: Use  Q/ESC: Back";
    }

    private void OpenItemMenu()
    {
        BuildBattleItemList();
        if (_battleItems.Count == 0)
        {
            AddLogLine("No usable items!");
            return;
        }

        _currentSubMenu = SubMenu.Items;
        _selectedIndex = 0;
        _commandPanel.SetActive(false);
        _itemPanel.SetActive(true);
        RefreshItemList();
        _instructionsText.text = "W/S: Navigate  E: Use  Q/ESC: Back";
    }

    private void ConfirmTechnique()
    {
        if (_partner == null || _selectedIndex >= _partner.KnownTechniques.Count) return;
        TechniqueData tech = _partner.KnownTechniques[_selectedIndex];
        _battleSystem.SelectTechnique(tech);
    }

    private void ConfirmItem()
    {
        if (_selectedIndex >= _battleItems.Count) return;
        _battleSystem.SelectItem(_battleItems[_selectedIndex]);
    }

    private int GetMaxIndex()
    {
        return _currentSubMenu switch
        {
            SubMenu.Commands => CommandNames.Length,
            SubMenu.Techniques => _partner != null ? _partner.KnownTechniques.Count : 0,
            SubMenu.Items => _battleItems.Count,
            _ => 0
        };
    }

    private void RefreshCurrentMenu()
    {
        switch (_currentSubMenu)
        {
            case SubMenu.Commands:
                RefreshCommandList();
                break;
            case SubMenu.Techniques:
                RefreshTechniqueList();
                break;
            case SubMenu.Items:
                RefreshItemList();
                break;
        }
    }

    private void RefreshCommandList()
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < CommandNames.Length; i++)
        {
            string marker = i == _selectedIndex ? "> " : "  ";
            sb.AppendLine($"{marker}{CommandNames[i]}");
        }
        _commandListText.text = sb.ToString();
    }

    private void RefreshTechniqueList()
    {
        if (_partner == null) return;
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < _partner.KnownTechniques.Count; i++)
        {
            TechniqueData tech = _partner.KnownTechniques[i];
            string marker = i == _selectedIndex ? "> " : "  ";
            string affordable = tech.MpCost <= _partner.CurrentMP ? "" : " (!)";
            sb.AppendLine($"{marker}{tech.TechniqueName}  [{tech.Category}]  MP {tech.MpCost}{affordable}");
        }
        _techniqueListText.text = sb.ToString();
    }

    private void RefreshItemList()
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < _battleItems.Count; i++)
        {
            string marker = i == _selectedIndex ? "> " : "  ";
            int count = _inventory.GetItemCount(_battleItems[i]);
            sb.AppendLine($"{marker}{_battleItems[i].ItemName} x{count}");
        }
        _itemListText.text = sb.ToString();
    }

    private void BuildBattleItemList()
    {
        _battleItems.Clear();
        HashSet<ItemData> seen = new HashSet<ItemData>();

        for (int i = 0; i < _inventory.SlotCount; i++)
        {
            InventorySlot slot = _inventory.GetSlot(i);
            if (slot.Item == null) continue;
            if (seen.Contains(slot.Item)) continue;
            if (slot.Item.Category == ItemCategory.Recovery || slot.Item.Category == ItemCategory.Status)
            {
                _battleItems.Add(slot.Item);
                seen.Add(slot.Item);
            }
        }
    }

    private void UpdateLogDisplay()
    {
        if (_battleLogText == null) return;
        _battleLogText.text = string.Join("\n", _logLines);
    }
}
