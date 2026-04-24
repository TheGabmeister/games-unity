using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameplayManager : Singleton<GameplayManager>
{
    [SerializeField] private InputManager _inputManager;
    [SerializeField] private TimeSystem _timeSystem;
    [SerializeField] private CareSystem _careSystem;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private DialogueManager _dialogueManager;
    [SerializeField] private HUD _hud;
    [SerializeField] private BattleSystem _battleSystem;
    [SerializeField] private BattleUI _battleUI;
    [SerializeField] private InventoryScreen _inventoryScreen;
    [SerializeField] private PauseScreen _pauseScreen;
    [SerializeField] private StatusScreen _statusScreen;
    [SerializeField] private GameplayCamera _gameplayCamera;
    [SerializeField] private ZoneData _startingZone;
    [SerializeField] private ZoneData[] _allZones;
    [SerializeField] private DigimonSpeciesData _reincarnationSpecies;

    private ZoneData _currentZone;
    private bool _isTransitioning;
    private DigimonInstance _partner;

    public InputManager InputManager => _inputManager;
    public TimeSystem TimeSystem => _timeSystem;
    public CareSystem CareSystem => _careSystem;
    public Inventory Inventory => _inventory;
    public DialogueManager DialogueManager => _dialogueManager;
    public HUD HUD => _hud;
    public BattleSystem BattleSystem => _battleSystem;
    public BattleUI BattleUI => _battleUI;
    public InventoryScreen InventoryScreen => _inventoryScreen;
    public PauseScreen PauseScreen => _pauseScreen;
    public StatusScreen StatusScreen => _statusScreen;
    public ZoneData StartingZone => _startingZone;

    public bool IsAnyScreenOpen => _inventoryScreen.IsOpen
                                 || _pauseScreen.IsOpen
                                 || _statusScreen.IsOpen;

    private void Start()
    {
        _partner = FindFirstObjectByType<DigimonInstance>();
        _careSystem.OnEvolutionReady += OnEvolutionReady;
        _careSystem.OnPartnerDied += OnPartnerDied;

        foreach (var zone in _allZones)
        {
            if (SceneManager.GetSceneByPath(zone.Scene.Path).isLoaded)
            {
                _currentZone = zone;
                _gameplayCamera.transform.position = zone.CameraPosition;
                return;
            }
        }
    }

    protected override void OnDestroy()
    {
        if (_careSystem != null)
        {
            _careSystem.OnEvolutionReady -= OnEvolutionReady;
            _careSystem.OnPartnerDied -= OnPartnerDied;
        }
        base.OnDestroy();
    }

    public async void LoadZone(ZoneData zone)
    {
        if (_currentZone == zone || _isTransitioning) return;
        _isTransitioning = true;

        await ScreenFader.Instance.FadeOut();
        if (_currentZone != null)
            await SceneLoader.Instance.UnloadScene(_currentZone.Scene);
        await SceneLoader.Instance.LoadScene(zone.Scene);
        _currentZone = zone;
        _gameplayCamera.transform.position = zone.CameraPosition;
        await ScreenFader.Instance.FadeIn();

        _isTransitioning = false;
    }

    public void SetInitialZone(ZoneData zone)
    {
        _currentZone = zone;
        _gameplayCamera.transform.position = zone.CameraPosition;
    }

    private void OnEvolutionReady(DigimonSpeciesData newSpecies)
    {
        if (_partner == null || _isTransitioning) return;
        HandleEvolution(newSpecies);
    }

    private async void HandleEvolution(DigimonSpeciesData newSpecies)
    {
        _isTransitioning = true;
        _inputManager.SetPlayerInputEnabled(false);

        string oldName = _partner.Species.SpeciesName;
        await ScreenFader.Instance.FadeOut();
        _partner.Evolve(newSpecies);
        _careSystem.ClearEvolutionPending();
        Debug.Log($"[Evolution] {oldName} evolved into {newSpecies.SpeciesName}!");
        await ScreenFader.Instance.FadeIn();

        _inputManager.SetPlayerInputEnabled(true);
        _isTransitioning = false;
    }

    private void OnPartnerDied()
    {
        if (_partner == null || _isTransitioning) return;
        HandleDeath();
    }

    private async void HandleDeath()
    {
        _isTransitioning = true;
        _inputManager.SetPlayerInputEnabled(false);

        string oldName = _partner.Species.SpeciesName;
        await ScreenFader.Instance.FadeOut();

        DigimonInheritance inheritance = _partner.Die();
        if (_reincarnationSpecies != null)
        {
            _partner.Reincarnate(_reincarnationSpecies, inheritance);
            _careSystem.ClearDeathPending();
            Debug.Log($"[Death] {oldName} has passed away. Reborn as {_reincarnationSpecies.SpeciesName} (Life #{inheritance.TotalLives}).");
        }

        await ScreenFader.Instance.FadeIn();

        _inputManager.SetPlayerInputEnabled(true);
        _isTransitioning = false;
    }

    private void Update()
    {
        if (_isTransitioning) return;
        if (_battleSystem.InBattle) return;
        if (_dialogueManager.IsActive) return;

        if (Keyboard.current.tabKey.wasPressedThisFrame
            || Keyboard.current.iKey.wasPressedThisFrame)
        {
            if (_inventoryScreen.IsOpen)
                CloseScreen(_inventoryScreen);
            else if (!IsAnyScreenOpen)
                OpenScreen(_inventoryScreen);
            return;
        }

        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            if (_statusScreen.IsOpen)
                CloseScreen(_statusScreen);
            else if (!IsAnyScreenOpen)
                OpenScreen(_statusScreen);
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (_inventoryScreen.IsOpen)
                CloseScreen(_inventoryScreen);
            else if (_statusScreen.IsOpen)
                CloseScreen(_statusScreen);
            else if (_pauseScreen.IsOpen)
                CloseScreen(_pauseScreen);
            else
                OpenScreen(_pauseScreen);
        }
    }

    private void OpenScreen(MonoBehaviour screen)
    {
        _inputManager.SetPlayerInputEnabled(false);

        if (screen == _inventoryScreen) _inventoryScreen.Open();
        else if (screen == _pauseScreen) _pauseScreen.Open();
        else if (screen == _statusScreen) _statusScreen.Open();
    }

    private void CloseScreen(MonoBehaviour screen)
    {
        if (screen == _inventoryScreen) _inventoryScreen.Close();
        else if (screen == _pauseScreen) _pauseScreen.Close();
        else if (screen == _statusScreen) _statusScreen.Close();

        _inputManager.SetPlayerInputEnabled(true);
    }
}
