using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct InventorySlot
{
    public ItemData Item;
    public int Count;

    public InventorySlot(ItemData item, int count)
    {
        Item = item;
        Count = count;
    }
}

public class Inventory : Singleton<Inventory>
{
    [SerializeField] private int _maxSlots = 20;
    [SerializeField] private int _startingBits = 0;

    private List<InventorySlot> _slots = new List<InventorySlot>();
    private int _bits;

    public int Bits => _bits;
    public int MaxSlots => _maxSlots;
    public int SlotCount => _slots.Count;

    public event Action OnInventoryChanged;

    protected override void Awake()
    {
        base.Awake();
        _bits = _startingBits;
    }

    public bool AddItem(ItemData item, int amount = 1)
    {
        int remaining = amount;

        for (int i = 0; i < _slots.Count && remaining > 0; i++)
        {
            if (_slots[i].Item == item && _slots[i].Count < item.MaxStack)
            {
                int canAdd = Mathf.Min(remaining, item.MaxStack - _slots[i].Count);
                _slots[i] = new InventorySlot(item, _slots[i].Count + canAdd);
                remaining -= canAdd;
            }
        }

        while (remaining > 0 && _slots.Count < _maxSlots)
        {
            int toAdd = Mathf.Min(remaining, item.MaxStack);
            _slots.Add(new InventorySlot(item, toAdd));
            remaining -= toAdd;
        }

        OnInventoryChanged?.Invoke();
        return remaining <= 0;
    }

    public bool RemoveItem(ItemData item, int amount = 1)
    {
        if (GetItemCount(item) < amount) return false;

        int remaining = amount;
        for (int i = _slots.Count - 1; i >= 0 && remaining > 0; i--)
        {
            if (_slots[i].Item != item) continue;

            int canRemove = Mathf.Min(remaining, _slots[i].Count);
            int left = _slots[i].Count - canRemove;
            if (left <= 0)
                _slots.RemoveAt(i);
            else
                _slots[i] = new InventorySlot(item, left);
            remaining -= canRemove;
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool UseItem(ItemData item)
    {
        if (!HasItem(item)) return false;

        DigimonInstance partner = FindFirstObjectByType<DigimonInstance>();
        if (partner == null || partner.IsSleeping) return false;

        if (item.Category == ItemCategory.Food)
        {
            CareSystem.Instance.Feed(item.HungerReduction, item.WeightGain);
        }
        else
        {
            if (item.HpRestore != 0) partner.Heal(item.HpRestore);
            if (item.MpRestore != 0) partner.RestoreMP(item.MpRestore);
        }

        if (item.HappinessChange != 0) partner.ModifyHappiness(item.HappinessChange);
        if (item.DisciplineChange != 0) partner.ModifyDiscipline(item.DisciplineChange);
        if (item.TirednessReduction != 0) partner.ModifyTiredness(-item.TirednessReduction);

        RemoveItem(item);
        return true;
    }

    public bool HasItem(ItemData item)
    {
        return GetItemCount(item) > 0;
    }

    public int GetItemCount(ItemData item)
    {
        int total = 0;
        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i].Item == item)
                total += _slots[i].Count;
        }
        return total;
    }

    public InventorySlot GetSlot(int index)
    {
        if (index < 0 || index >= _slots.Count) return default;
        return _slots[index];
    }

    public void AddBits(int amount)
    {
        _bits = Mathf.Max(0, _bits + amount);
    }

    public bool SpendBits(int amount)
    {
        if (_bits < amount) return false;
        _bits -= amount;
        return true;
    }
}
