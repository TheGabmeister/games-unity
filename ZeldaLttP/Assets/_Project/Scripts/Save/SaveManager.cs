using System;
using UnityEngine;

public static class SaveManager
{
    static PlayerData _playerData;


    public static void CreateSave()
    {
        _playerData = new PlayerData();
        _playerData.username = "ASDF";
        ES3.Save("playerData", _playerData);
    }

    public static void LoadData()
    {
        _playerData = ES3.Load<PlayerData>("playerData");
        Debug.Log(_playerData.username);
    }
}
