using System.IO;
using UnityEngine;

public sealed class SaveManager : MonoBehaviour
{
    private const string IndexFileName = "SaveIndex.json";

    public int CurrentSlot { get; private set; } = 1;
    public SaveData Current { get; private set; }

    private string SlotPath(int slot) => Path.Combine(Application.persistentDataPath, $"save_{slot}.json");
    private string IndexPath => Path.Combine(Application.persistentDataPath, IndexFileName);

    public SaveData LoadOrCreate(int slot)
    {
        CurrentSlot = slot;
        if (!File.Exists(SlotPath(slot)))
        {
            Current = new SaveData();
            return Current;
        }

        var json = File.ReadAllText(SlotPath(slot));
        Current = SaveSerializer.Deserialize(json) ?? new SaveData();
        return Current;
    }

    public void Save()
    {
        if (Current == null) return;
        var json = SaveSerializer.Serialize(Current);
        File.WriteAllText(SlotPath(CurrentSlot), json);
    }

    public void SetCurrent(SaveData data, int slot)
    {
        Current = data;
        CurrentSlot = slot;
    }
}
