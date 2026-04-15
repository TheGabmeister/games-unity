using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SMW.Save
{
    public sealed class SaveManager : MonoBehaviour
    {
        private const string IndexFileName = "SaveIndex.json";
        private readonly List<ISaveMigration> _migrations = new();

        public int CurrentSlot { get; private set; } = 1;
        public SaveData Current { get; private set; }

        private string SlotPath(int slot) => Path.Combine(Application.persistentDataPath, $"save_{slot}.json");
        private string IndexPath => Path.Combine(Application.persistentDataPath, IndexFileName);

        public void RegisterMigration(ISaveMigration migration) => _migrations.Add(migration);

        public SaveData LoadOrCreate(int slot)
        {
            CurrentSlot = slot;
            if (!File.Exists(SlotPath(slot)))
            {
                Current = new SaveData();
                return Current;
            }

            var json = File.ReadAllText(SlotPath(slot));
            json = ApplyMigrations(json);
            Current = SaveSerializer.Deserialize(json) ?? new SaveData();
            return Current;
        }

        public void Save()
        {
            if (Current == null) return;
            Current.saveVersion = SaveData.CurrentSaveVersion;
            var json = SaveSerializer.Serialize(Current);
            File.WriteAllText(SlotPath(CurrentSlot), json);
        }

        public void SetCurrent(SaveData data, int slot)
        {
            Current = data;
            CurrentSlot = slot;
        }

        private string ApplyMigrations(string json)
        {
            var obj = JObject.Parse(json);
            var version = obj.Value<int?>("saveVersion") ?? 0;
            while (version < SaveData.CurrentSaveVersion)
            {
                ISaveMigration step = null;
                foreach (var m in _migrations)
                {
                    if (m.FromVersion == version) { step = m; break; }
                }
                if (step == null) break;
                json = step.Migrate(json);
                obj = JObject.Parse(json);
                version = obj.Value<int?>("saveVersion") ?? step.ToVersion;
            }
            return json;
        }
    }
}
