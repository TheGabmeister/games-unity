using UnityEngine;

[CreateAssetMenu(fileName = "TilePalette", menuName = "FF1/Tile Palette")]
public class TilePalette : ScriptableObject
{
    [Header("Terrain Colors")]
    public Color Grass = new Color(0.2f, 0.65f, 0.2f);
    public Color GrassAlt = new Color(0.18f, 0.6f, 0.18f);
    public Color Forest = new Color(0.1f, 0.4f, 0.1f);
    public Color Mountain = new Color(0.55f, 0.35f, 0.2f);
    public Color Water = new Color(0.1f, 0.3f, 0.8f);
    public Color WaterAlt = new Color(0.15f, 0.35f, 0.85f);
    public Color River = new Color(0.3f, 0.55f, 0.9f);
    public Color Desert = new Color(0.85f, 0.75f, 0.4f);

    [Header("Structure Colors")]
    public Color TownEntrance = new Color(0.6f, 0.6f, 0.6f);
    public Color DungeonEntrance = new Color(0.3f, 0.3f, 0.35f);
    public Color Floor = new Color(0.7f, 0.65f, 0.55f);
    public Color Wall = new Color(0.25f, 0.25f, 0.3f);
    public Color Lava = new Color(0.9f, 0.3f, 0.1f);
    public Color Chest = new Color(0.6f, 0.4f, 0.15f);
    public Color ChestFloor = new Color(0.7f, 0.65f, 0.55f);
}
