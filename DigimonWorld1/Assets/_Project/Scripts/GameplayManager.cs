using UnityEngine;

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
    [SerializeField] private ScreenManager _screenManager;

    public InputManager InputManager => _inputManager;
    public TimeSystem TimeSystem => _timeSystem;
    public CareSystem CareSystem => _careSystem;
    public Inventory Inventory => _inventory;
    public DialogueManager DialogueManager => _dialogueManager;
    public HUD HUD => _hud;
    public BattleSystem BattleSystem => _battleSystem;
    public BattleUI BattleUI => _battleUI;
    public ScreenManager ScreenManager => _screenManager;
}
