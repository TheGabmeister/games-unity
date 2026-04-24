using UnityEngine;
using UnityEngine.InputSystem;

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

    public bool IsAnyScreenOpen => _inventoryScreen.IsOpen
                                 || _pauseScreen.IsOpen
                                 || _statusScreen.IsOpen;

    private void Update()
    {
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
