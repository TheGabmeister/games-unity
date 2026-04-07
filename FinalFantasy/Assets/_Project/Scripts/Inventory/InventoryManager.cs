using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventorySlot
{
    public ItemData Item;
    public int Count;
}

public class InventoryManager : MonoBehaviour
{
    [SerializeField] int gil = 0;
    public const int GilCap = 999999;
    public const int StackLimit = 99;

    List<InventorySlot> items = new();
    List<EquipmentData> equipment = new();

    public int Gil => gil;

    public event Action OnInventoryChanged;

    // --- Gil ---
    public void AddGil(int amount)
    {
        gil = Mathf.Min(gil + amount, GilCap);
        OnInventoryChanged?.Invoke();
    }

    public bool SpendGil(int amount)
    {
        if (amount > gil) return false;
        gil -= amount;
        OnInventoryChanged?.Invoke();
        return true;
    }

    public void SetGil(int amount)
    {
        gil = Mathf.Clamp(amount, 0, GilCap);
        OnInventoryChanged?.Invoke();
    }

    // --- Consumable Items ---
    public void AddItem(ItemData item, int count = 1)
    {
        if (item == null || count <= 0) return;

        var slot = items.Find(s => s.Item == item);
        if (slot != null)
        {
            slot.Count = Mathf.Min(slot.Count + count, StackLimit);
        }
        else
        {
            items.Add(new InventorySlot { Item = item, Count = Mathf.Min(count, StackLimit) });
        }
        OnInventoryChanged?.Invoke();
    }

    public bool RemoveItem(ItemData item, int count = 1)
    {
        var slot = items.Find(s => s.Item == item);
        if (slot == null || slot.Count < count) return false;

        slot.Count -= count;
        if (slot.Count <= 0) items.Remove(slot);
        OnInventoryChanged?.Invoke();
        return true;
    }

    public int GetItemCount(ItemData item)
    {
        var slot = items.Find(s => s.Item == item);
        return slot?.Count ?? 0;
    }

    public List<InventorySlot> GetAllItems() => new(items);

    // --- Equipment ---
    public void AddEquipment(EquipmentData equip)
    {
        if (equip == null) return;
        equipment.Add(equip);
        OnInventoryChanged?.Invoke();
    }

    public bool RemoveEquipment(EquipmentData equip)
    {
        bool removed = equipment.Remove(equip);
        if (removed) OnInventoryChanged?.Invoke();
        return removed;
    }

    public List<EquipmentData> GetAllEquipment() => new(equipment);

    public List<EquipmentData> GetEquipmentForSlot(EquipmentSlot slot)
    {
        return equipment.FindAll(e => e.Slot == slot);
    }

    /// Use an item on a party member (field usage). Returns true if item was consumed.
    public bool UseItemInField(ItemData item, PartyMember target)
    {
        if (item == null || target == null || !item.UsableInField) return false;
        if (GetItemCount(item) <= 0) return false;

        bool used = ApplyItemEffect(item, target);
        if (used) RemoveItem(item);
        return used;
    }

    bool ApplyItemEffect(ItemData item, PartyMember target)
    {
        switch (item.EffectType)
        {
            case ItemEffectType.HealHP:
                // FF1: potions heal even at full HP (consumed anyway)
                target.CurrentHP = Mathf.Min(target.CurrentHP + item.Power, target.MaxHP);
                return true;

            case ItemEffectType.HealMP:
                target.CurrentMP = Mathf.Min(target.CurrentMP + item.Power, target.MaxMP);
                return true;

            case ItemEffectType.CureStatus:
                if ((target.StatusEffects & item.CuresStatus) != 0)
                {
                    target.StatusEffects &= ~item.CuresStatus;
                    return true;
                }
                // Still consumed even if target doesn't have the status (FF1 behavior)
                return true;

            case ItemEffectType.Revive:
                if ((target.StatusEffects & StatusEffectFlags.KO) != 0)
                {
                    target.StatusEffects &= ~StatusEffectFlags.KO;
                    target.CurrentHP = Mathf.Max(1, target.MaxHP * item.Power / 100);
                    return true;
                }
                return false; // Can't use revive on alive target

            default:
                return false;
        }
    }
}
