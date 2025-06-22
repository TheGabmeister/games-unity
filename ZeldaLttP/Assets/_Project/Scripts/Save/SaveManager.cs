using System;
using System.Linq;
using UnityEngine;

public static class SaveManager
{
    static PlayerData _playerData;
    public const string SaveFileName = "SaveFile";
    public static int CurrentSaveSlot { get; private set; } = 1;

    public static void Init()
    {
        
    }
    
    private static string GetSaveFilename(int saveSlot)
    {
        return $"save{saveSlot}.es3";
    }

    public static void CreateNewSave(int saveSlot = 1)
    {
        CurrentSaveSlot = saveSlot;
        _playerData = new PlayerData();
        _playerData.username = "ASDF";
        ES3.Save("playerData", _playerData, SaveFileName + saveSlot.ToString());
    }

    public static void LoadData(int saveSlot = 1)
    {
        CurrentSaveSlot = saveSlot;
        string filename = GetSaveFilename(saveSlot);
        
        if (ES3.FileExists(filename) && ES3.KeyExists("playerData", filename))
        {
            _playerData = ES3.Load<PlayerData>("playerData", filename);
            Debug.Log($"Loaded save slot {saveSlot}: {_playerData.username}");
        }
        else
        {
            Debug.LogWarning($"Save slot {saveSlot} does not exist");
        }
    }
    
    public static bool DoesSaveExist(int saveSlot)
    {
        string filename = SaveFileName + saveSlot.ToString();
        return ES3.FileExists(filename) && ES3.KeyExists("playerData", filename);
    }
    
    public static void SaveGame()
    {
        // Save to current slot
        ES3.Save("playerData", _playerData, GetSaveFilename(CurrentSaveSlot));
    }
    
    public static void DeleteSave(int saveSlot)
    {
        string filename = GetSaveFilename(saveSlot);
        if (ES3.FileExists(filename))
        {
            ES3.DeleteFile(filename);
        }
    }

}
