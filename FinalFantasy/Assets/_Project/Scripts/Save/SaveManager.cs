using UnityEngine;
using Newtonsoft.Json;
using System.IO;

public class SaveManager : MonoBehaviour
{
    const int ManualSlotCount = 4;
    const int AutoSlotIndex = 4;
    const int QuickSlotIndex = 5;
    const int TotalSlots = 6;

    string SaveDirectory => Path.Combine(Application.persistentDataPath, "saves");

    JsonSerializerSettings jsonSettings;

    void Awake()
    {
        jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto
        };

        if (!Directory.Exists(SaveDirectory))
            Directory.CreateDirectory(SaveDirectory);
    }

    string GetSavePath(int slot) => Path.Combine(SaveDirectory, $"save_{slot}.json");
    string GetBackupPath(int slot) => Path.Combine(SaveDirectory, $"save_{slot}.backup.json");

    public void Save(int slot, SaveData data)
    {
        data.Timestamp = System.DateTime.UtcNow.ToString("o");
        string json = JsonConvert.SerializeObject(data, jsonSettings);
        string path = GetSavePath(slot);
        string tmpPath = path + ".tmp";

        // Atomic write: write to temp, backup old, rename
        File.WriteAllText(tmpPath, json);

        if (File.Exists(path))
            File.Copy(path, GetBackupPath(slot), overwrite: true);

        if (File.Exists(path))
            File.Delete(path);
        File.Move(tmpPath, path);

        Debug.Log($"[Save] Saved slot {slot} to {path}");
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.SaveGame);
    }

    public SaveData Load(int slot)
    {
        string path = GetSavePath(slot);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[Save] No save file at slot {slot}");
            return null;
        }

        string json = File.ReadAllText(path);
        var data = JsonConvert.DeserializeObject<SaveData>(json, jsonSettings);
        Debug.Log($"[Save] Loaded slot {slot}");
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.LoadGame);

        // If quick save, consume it
        if (data.Type == SaveType.Quick)
        {
            File.Delete(path);
            Debug.Log("[Save] Quick save consumed");
        }

        return data;
    }

    public bool SlotExists(int slot) => File.Exists(GetSavePath(slot));

    public SaveSlotInfo GetSlotInfo(int slot)
    {
        string path = GetSavePath(slot);
        if (!File.Exists(path))
            return new SaveSlotInfo { Exists = false };

        string json = File.ReadAllText(path);
        var data = JsonConvert.DeserializeObject<SaveData>(json, jsonSettings);

        return new SaveSlotInfo
        {
            Exists = true,
            Type = data.Type,
            Timestamp = data.Timestamp,
            LocationName = data.CurrentScene,
            PlayTimeSeconds = data.PlayTimeSeconds,
            Gil = data.Gil
        };
    }

    public void DeleteSlot(int slot)
    {
        string path = GetSavePath(slot);
        if (File.Exists(path))
            File.Delete(path);
    }

    public int? FindMostRecentSlot()
    {
        int? bestSlot = null;
        System.DateTime bestTime = System.DateTime.MinValue;

        for (int i = 0; i < TotalSlots; i++)
        {
            if (!SlotExists(i)) continue;

            var info = GetSlotInfo(i);
            if (info != null && System.DateTime.TryParse(info.Timestamp, out var time) && time > bestTime)
            {
                bestTime = time;
                bestSlot = i;
            }
        }

        return bestSlot;
    }
}
