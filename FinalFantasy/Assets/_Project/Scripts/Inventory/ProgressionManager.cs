using System;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    HashSet<string> flags = new();
    List<KeyItemData> keyItems = new();

    public IReadOnlyCollection<string> Flags => flags;
    public IReadOnlyList<KeyItemData> KeyItems => keyItems;

    public event Action<string> OnFlagSet;
    public event Action<string> OnFlagCleared;

    public void SetFlag(string flag)
    {
        if (string.IsNullOrEmpty(flag)) return;
        if (flags.Add(flag))
        {
            Debug.Log($"[Progression] Flag set: {flag}");
            OnFlagSet?.Invoke(flag);
        }
    }

    public void ClearFlag(string flag)
    {
        if (flags.Remove(flag))
        {
            Debug.Log($"[Progression] Flag cleared: {flag}");
            OnFlagCleared?.Invoke(flag);
        }
    }

    public bool HasFlag(string flag) => flags.Contains(flag);

    public void AddKeyItem(KeyItemData item)
    {
        if (item == null || HasKeyItem(item)) return;
        keyItems.Add(item);
        if (!string.IsNullOrEmpty(item.GrantsFlag))
            SetFlag(item.GrantsFlag);
        Debug.Log($"[Progression] Key item obtained: {item.ItemName}");
    }

    public bool HasKeyItem(KeyItemData item)
    {
        return keyItems.Contains(item);
    }

    public void RemoveKeyItem(KeyItemData item)
    {
        keyItems.Remove(item);
        // Flag remains set even after key item is consumed
    }

    // For save/load
    public HashSet<string> GetAllFlags() => new(flags);
    public void RestoreFlags(IEnumerable<string> savedFlags)
    {
        flags.Clear();
        foreach (var f in savedFlags) flags.Add(f);
    }
}
