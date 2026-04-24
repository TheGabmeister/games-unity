using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryScreen : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_Text _itemListText;
    [SerializeField] private TMP_Text _bitsText;
    [SerializeField] private TMP_Text _instructionsText;
    [SerializeField] private Inventory _inventory;

    private bool _isOpen;
    private int _selectedIndex;

    public bool IsOpen => _isOpen;

    private void Awake()
    {
        if (_panel != null)
            _panel.SetActive(false);
    }

    private void Update()
    {
        if (!_isOpen) return;

        if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
            Navigate(-1);
        if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
            Navigate(1);
        if (Keyboard.current.eKey.wasPressedThisFrame)
            UseSelected();
        if (Keyboard.current.qKey.wasPressedThisFrame)
            DiscardSelected();
    }

    public void Open()
    {
        _isOpen = true;
        _panel.SetActive(true);
        _selectedIndex = 0;
        RefreshDisplay();
    }

    public void Close()
    {
        _isOpen = false;
        _panel.SetActive(false);
    }

    private void Navigate(int direction)
    {
        int slotCount = _inventory.SlotCount;
        if (slotCount == 0) return;
        _selectedIndex = Mathf.Clamp(_selectedIndex + direction, 0, slotCount - 1);
        RefreshDisplay();
    }

    private void UseSelected()
    {
        if (_inventory.SlotCount == 0) return;

        InventorySlot slot = _inventory.GetSlot(_selectedIndex);
        _inventory.UseItem(slot.Item);
        ClampSelection();
        RefreshDisplay();
    }

    private void DiscardSelected()
    {
        if (_inventory.SlotCount == 0) return;

        InventorySlot slot = _inventory.GetSlot(_selectedIndex);
        _inventory.RemoveItem(slot.Item);
        ClampSelection();
        RefreshDisplay();
    }

    private void ClampSelection()
    {
        int slotCount = _inventory.SlotCount;
        if (slotCount == 0)
            _selectedIndex = 0;
        else if (_selectedIndex >= slotCount)
            _selectedIndex = slotCount - 1;
    }

    private void RefreshDisplay()
    {
        _bitsText.text = $"Bits: {_inventory.Bits}";

        int slotCount = _inventory.SlotCount;
        if (slotCount == 0)
        {
            _itemListText.text = "(empty)";
            _instructionsText.text = "Tab/I: Close";
            return;
        }

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < slotCount; i++)
        {
            InventorySlot slot = _inventory.GetSlot(i);
            if (slot.Item == null) continue;
            string marker = i == _selectedIndex ? "> " : "  ";
            sb.AppendLine($"{marker}{slot.Item.ItemName} x{slot.Count}");
        }
        _itemListText.text = sb.ToString();

        InventorySlot selected = _inventory.GetSlot(_selectedIndex);
        if (selected.Item != null)
            _instructionsText.text = $"{selected.Item.Description}\nW/S: Navigate  E: Use  Q: Discard  Tab/I: Close";
        else
            _instructionsText.text = "Tab/I: Close";
    }
}
