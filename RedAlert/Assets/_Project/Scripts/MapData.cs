using UnityEngine;

[CreateAssetMenu(fileName = "NewMap", menuName = "Red Alert/Map Data")]
public class MapData : ScriptableObject
{
    public int Width = 40;
    public int Height = 40;
    public TerrainType[] Cells;

    public TerrainType GetCell(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return TerrainType.Water;
        return Cells[y * Width + x];
    }

    public void SetCell(int x, int y, TerrainType type)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return;
        Cells[y * Width + x] = type;
    }
}
