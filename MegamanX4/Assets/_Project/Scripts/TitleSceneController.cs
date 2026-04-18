using PrimeTween;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class TitleSceneController : MonoBehaviour
{
    enum Phase { PressStart, Menu }

    [SerializeField] GameObject _pressStartRoot;
    [SerializeField] GameObject _menuRoot;
    [SerializeField] MenuNavigator _menuNav;
    [SerializeField] float _fadeDuration = 0.3f;

    PlayerInput _playerInput;
    InputAction _submitAction;
    ScreenFader _fader;

    Phase _phase;
    bool _transitioning;

    void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _submitAction = _playerInput.actions["Submit"];

        if (Services.TryGet<GameStateController>(out var gs))
            _fader = gs.Fader;

        ShowPressStart();
    }

    void OnEnable()
    {
        _submitAction.started += OnSubmit;
        if (_menuNav)
            _menuNav.Confirmed += OnMenuConfirm;
    }

    void OnDisable()
    {
        _submitAction.started -= OnSubmit;
        if (_menuNav)
            _menuNav.Confirmed -= OnMenuConfirm;
    }

    void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (_transitioning)
            return;
        if (_phase == Phase.PressStart)
            ShowMenu();
    }

    void OnMenuConfirm(int index)
    {
        if (_transitioning || _phase != Phase.Menu)
            return;

        if (index == 0)
            StartNewGame();
        else if (index == 1)
            Continue();
        else if (index == 2)
            OpenOptions();
    }

    void ShowPressStart()
    {
        _phase = Phase.PressStart;
        _pressStartRoot.SetActive(true);
        _menuRoot.SetActive(false);
    }

    async void ShowMenu()
    {
        _transitioning = true;
        await _fader.FadeToColor(Color.black, _fadeDuration);

        _phase = Phase.Menu;
        _pressStartRoot.SetActive(false);
        _menuRoot.SetActive(true);
        _menuNav.ResetSelection();

        await _fader.FadeToColor(Color.clear, _fadeDuration);
        _transitioning = false;
    }

    void StartNewGame()
    {
        if (Services.TryGet<GameStateController>(out var gameState))
            gameState.SetState(GameState.LevelSelect);
    }

    void Continue()
    {
    }

    void OpenOptions()
    {
    }
}
