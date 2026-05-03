using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Minimap : MonoBehaviour, IPointerClickHandler, IDragHandler
{
    [SerializeField] private RawImage _mapImage;
    [SerializeField] private GameObject _offlineOverlay;

    private Texture2D _texture;
    private Color32[] _pixels;
    private int _width;
    private int _height;
    private int _frameCounter;
    private bool _hasRadar;
    private const int UpdateInterval = 6;

    private static readonly Color32 ColorClear = new(100, 160, 80, 255);
    private static readonly Color32 ColorRoad = new(140, 140, 130, 255);
    private static readonly Color32 ColorRough = new(120, 110, 70, 255);
    private static readonly Color32 ColorSand = new(190, 170, 110, 255);
    private static readonly Color32 ColorWater = new(40, 80, 160, 255);
    private static readonly Color32 ColorOre = new(200, 180, 60, 255);
    private static readonly Color32 ColorGems = new(160, 60, 200, 255);
    private static readonly Color32 ColorShroud = new(0, 0, 0, 255);

    public static Minimap Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        var map = MapManager.Instance;
        if (map == null || map.Width == 0) return;

        _width = map.Width;
        _height = map.Height;

        _texture = new Texture2D(_width, _height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        _pixels = new Color32[_width * _height];

        if (_mapImage != null)
            _mapImage.texture = _texture;

        RefreshMinimap();
    }

    void Update()
    {
        _frameCounter++;
        if (_frameCounter < UpdateInterval) return;
        _frameCounter = 0;

        CheckRadarStatus();
        if (_hasRadar)
            RefreshMinimap();
    }

    void CheckRadarStatus()
    {
        int localPlayer = PlayerManager.Instance.LocalPlayer.PlayerIndex;
        var player = PlayerManager.Instance.GetPlayer(localPlayer);

        bool hadRadar = _hasRadar;
        _hasRadar = false;

        foreach (var entity in player.OwnedEntities)
        {
            if (entity == null || entity.IsDead) continue;
            if (entity.UnitData == null) continue;
            if (entity.UnitData.DisplayName != "Radar Dome") continue;

            if (entity.UnitData.RequiresPower && PowerManager.Instance != null
                && PowerManager.Instance.IsLowPower(localPlayer))
                continue;

            _hasRadar = true;
            break;
        }

        if (_hasRadar != hadRadar)
        {
            if (_offlineOverlay != null)
                _offlineOverlay.SetActive(!_hasRadar);

            if (_hasRadar)
                RefreshMinimap();
            else
                ClearMinimap();
        }
    }

    void RefreshMinimap()
    {
        if (_texture == null) return;

        var map = MapManager.Instance;
        var fog = FogManager.Instance;

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                int i = y * _width + x;
                var cell = new Vector2Int(x, y);

                if (fog != null && !fog.IsExplored(cell))
                {
                    _pixels[i] = ColorShroud;
                    continue;
                }

                _pixels[i] = GetTerrainColor(map.GetTerrain(cell));
            }
        }

        int localPlayer = PlayerManager.Instance.LocalPlayer.PlayerIndex;
        for (int p = 0; p < PlayerManager.Instance.PlayerCount; p++)
        {
            var player = PlayerManager.Instance.GetPlayer(p);
            Color32 dotColor = p == localPlayer
                ? ToColor32(player.Color)
                : ToColor32(PlayerManager.Instance.GetPlayer(p).Color);

            for (int e = player.OwnedEntities.Count - 1; e >= 0; e--)
            {
                var entity = player.OwnedEntities[e];
                if (entity == null || entity.IsDead) continue;

                if (entity.OccupiedCells != null)
                {
                    foreach (var cell in entity.OccupiedCells)
                    {
                        if (fog != null && !fog.IsExplored(cell)) continue;
                        if (cell.x >= 0 && cell.x < _width && cell.y >= 0 && cell.y < _height)
                            _pixels[cell.y * _width + cell.x] = dotColor;
                    }
                }
            }
        }

        _texture.SetPixelData(_pixels, 0);
        _texture.Apply();
    }

    void ClearMinimap()
    {
        if (_texture == null) return;

        for (int i = 0; i < _pixels.Length; i++)
            _pixels[i] = ColorShroud;

        _texture.SetPixelData(_pixels, 0);
        _texture.Apply();
    }

    static Color32 GetTerrainColor(TerrainType terrain)
    {
        return terrain switch
        {
            TerrainType.Clear => ColorClear,
            TerrainType.Road => ColorRoad,
            TerrainType.Rough => ColorRough,
            TerrainType.Sand => ColorSand,
            TerrainType.Water => ColorWater,
            TerrainType.Ore => ColorOre,
            TerrainType.Gems => ColorGems,
            _ => ColorClear
        };
    }

    static Color32 ToColor32(Color c)
    {
        return new Color32(
            (byte)(c.r * 255), (byte)(c.g * 255),
            (byte)(c.b * 255), 255);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        JumpCameraToClick(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        JumpCameraToClick(eventData);
    }

    void JumpCameraToClick(PointerEventData eventData)
    {
        if (!_hasRadar) return;
        if (_mapImage == null) return;

        RectTransform rt = _mapImage.rectTransform;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rt, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            return;

        Rect rect = rt.rect;
        float normalizedX = (localPoint.x - rect.x) / rect.width;
        float normalizedY = (localPoint.y - rect.y) / rect.height;

        float worldX = normalizedX * _width;
        float worldY = normalizedY * _height;

        RTSCamera.Instance?.JumpTo(new Vector3(worldX, worldY, 0f));
    }
}
