using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryScreen : Singleton<InventoryScreen>
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_Text _itemListText;
    [SerializeField] private TMP_Text _bitsText;
    [SerializeField] private TMP_Text _instructionsText;

    private bool _isOpen;
    private int _selectedIndex;

    public bool IsOpen => _isOpen;

    protected override void Awake()
    {
        base.Awake();
        if (_panel != null)
            _panel.SetActive(false);
    }

    private void Update()
    {
        if (BattleSystem.Instance != null && BattleSystem.Instance.InBattle) return;

        if (Keyboard.current.tabKey.wasPressedThisFrame || Keyboard.current.iKey.wasPressedThisFrame)
        {
            if (PauseScreen.Instance != null && PauseScreen.Instance.IsOpen) return;
            if (StatusScreen.Instance != null && StatusScreen.Instance.IsOpen) return;
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsActive) return;

            if (_isOpen)
                Close();
            else
                Open();
            return;
        }

        if (!_isOpen) return;

        if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
            Navigate(-1);
        if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
            Navigate(1);
        if (Keyboard.current.eKey.wasPressedThisFrame)
            UseSelected();
        if (Keyboard.current.qKey.wasPressedThisFrame)
            DiscardSelected();
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            Close();
    }

    private void Open()
    {
        _isOpen = true;
        _panel.SetActive(true);
        _selectedIndex = 0;
        InputManager.Instance.SetPlayerInputEnabled(false);
        RefreshDisplay();
    }

    private void Close()
    {
        _isOpen = false;
        _panel.SetActive(false);
        InputManager.Instance.SetPlayerInputEnabled(true);
    }

    private void Navigate(int direction)
    {
        int slotCount = Inventory.Instance.SlotCount;
        if (slotCount == 0) return;
        _selectedIndex = Mathf.Clamp(_selectedIndex + direction, 0, slotCount - 1);
        RefreshDisplay();
    }

    private void UseSelected()
    {
        if (Inventory.Instance.SlotCount == 0) return;

        InventorySlot slot = Inventory.Instance.GetSlot(_selectedIndex);
        Inventory.Instance.UseItem(slot.Item);
        ClampSelection();
        RefreshDisplay();
    }

    private void DiscardSelected()
    {
        if (Inventory.Instance.SlotCount == 0) return;

        InventorySlot slot = Inventory.Instance.GetSlot(_selectedIndex);
        Inventory.Instance.RemoveItem(slot.Item);
        ClampSelection();
        RefreshDisplay();
    }

    private void ClampSelection()
    {
        int slotCount = Inventory.Instance.SlotCount;
        if (slotCount == 0)
            _selectedIndex = 0;
        else if (_selectedIndex >= slotCount)
            _selectedIndex = slotCount - 1;
    }

    private void RefreshDisplay()
    {
        _bitsText.text = $"Bits: {Inventory.Instance.Bits}";

        int slotCount = Inventory.Instance.SlotCount;
        if (slotCount == 0)
        {
            _itemListText.text = "(empty)";
            _instructionsText.text = "Tab/I: Close";
            return;
        }

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < slotCount; i++)
        {
            InventorySlot slot = Inventory.Instance.GetSlot(i);
            if (slot.Item == null) continue;
            string marker = i == _selectedIndex ? "> " : "  ";
            sb.AppendLine($"{marker}{slot.Item.ItemName} x{slot.Count}");
        }
        _itemListText.text = sb.ToString();

        InventorySlot selected = Inventory.Instance.GetSlot(_selectedIndex);
        if (selected.Item != null)
            _instructionsText.text = $"{selected.Item.Description}\nW/S: Navigate  E: Use  Q: Discard  Tab/I: Close";
        else
            _instructionsText.text = "Tab/I: Close";
    }
}
