using UnityEditor;
using UnityEngine;

public static class GenerateTestMap
{
    public static void Generate()
    {
        string dir = "Assets/_Project/Data";
        PrefabGeneratorUtils.EnsureFolder(dir);

        var mapData = ScriptableObject.CreateInstance<MapData>();
        mapData.Width = 40;
        mapData.Height = 40;
        mapData.Cells = new TerrainType[40 * 40];

        for (int x = 0; x < 40; x++)
            for (int y = 0; y < 40; y++)
                mapData.SetCell(x, y, TerrainType.Clear);

        for (int y = 0; y < 40; y++)
        {
            mapData.SetCell(10, y, TerrainType.Road);
            mapData.SetCell(11, y, TerrainType.Road);
        }
        for (int x = 0; x < 40; x++)
        {
            mapData.SetCell(x, 20, TerrainType.Road);
            mapData.SetCell(x, 21, TerrainType.Road);
        }

        for (int x = 28; x < 40; x++)
            for (int y = 0; y < 8; y++)
                mapData.SetCell(x, y, TerrainType.Water);
        for (int x = 30; x < 40; x++)
            for (int y = 8; y < 12; y++)
                mapData.SetCell(x, y, TerrainType.Water);

        for (int x = 28; x < 31; x++)
            for (int y = 7; y < 9; y++)
                mapData.SetCell(x, y, TerrainType.Sand);
        for (int x = 29; x < 32; x++)
            mapData.SetCell(x, 11, TerrainType.Sand);

        for (int x = 15; x < 22; x++)
            for (int y = 28; y < 35; y++)
                mapData.SetCell(x, y, TerrainType.Ore);

        for (int x = 32; x < 36; x++)
            for (int y = 32; y < 36; y++)
                mapData.SetCell(x, y, TerrainType.Gems);

        for (int x = 0; x < 6; x++)
            for (int y = 32; y < 38; y++)
                mapData.SetCell(x, y, TerrainType.Rough);

        for (int x = 22; x < 28; x++)
            for (int y = 12; y < 18; y++)
                mapData.SetCell(x, y, TerrainType.Rough);

        AssetDatabase.CreateAsset(mapData, $"{dir}/TestMap.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated TestMap (40x40)");
    }
}
