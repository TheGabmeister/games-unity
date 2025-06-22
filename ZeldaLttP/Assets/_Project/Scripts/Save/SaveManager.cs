using System;
using System.Linq;
using UnityEngine;

public static class SaveManager
{
    public static PlayerData ActivePlayerData { get; private set; }
    public const string SaveFileName = "SaveFile";
    public static int ActiveSaveSlot { get; private set; } = 0;

    public static void Init()
    {
        
    }
    
    private static string GetSaveFilename(int saveSlot)
    {
        return $"save{saveSlot}.es3";
    }

    public static void CreateSave(int saveSlot, string username)
    {
        var playerData = new PlayerData();
        playerData.username = username;
        ES3.Save("playerData", playerData, SaveFileName + saveSlot);
    }

    public static PlayerData LoadData(int saveSlot)
    {
        return ES3.Load<PlayerData>("playerData", SaveFileName + (saveSlot));
    }
    
    public static bool DoesSaveExist(int saveSlot)
    {
        string filename = SaveFileName + (saveSlot);
        return ES3.FileExists(filename) && ES3.KeyExists("playerData", filename);
    }
    
    public static void SaveGame()
    {
        //ES3.Save("playerData", _playerData, SaveFileName + ActiveSaveSlot);
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
