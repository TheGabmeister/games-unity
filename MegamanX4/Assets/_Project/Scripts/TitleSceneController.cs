using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class TitleSceneController : MonoBehaviour
{
    enum Phase { PressStart, Menu }

    [SerializeField] GameObject _pressStartRoot;
    [SerializeField] GameObject _menuRoot;
    [SerializeField] TMP_Text[] _menuLabels;
    [SerializeField] Color _selectedColor = Color.yellow;
    [SerializeField] Color _normalColor = Color.white;

    PlayerInput _playerInput;
    InputAction _submitAction;
    InputAction _navigateAction;

    Phase _phase;
    int _selectedIndex;

    void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _submitAction = _playerInput.actions["Submit"];
        _navigateAction = _playerInput.actions["Navigate"];
        ShowPressStart();
    }

    void OnEnable()
    {
        _submitAction.started += OnSubmit;
        _navigateAction.performed += OnNavigate;
    }

    void OnDisable()
    {
        _submitAction.started -= OnSubmit;
        _navigateAction.performed -= OnNavigate;
    }

    void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (_phase == Phase.PressStart)
            ShowMenu();
        else
            ConfirmSelection();
    }

    void OnNavigate(InputAction.CallbackContext ctx)
    {
        if (_phase != Phase.Menu)
            return;

        float y = ctx.ReadValue<Vector2>().y;
        if (y > 0.5f)
            MoveSelection(-1);
        else if (y < -0.5f)
            MoveSelection(1);
    }

    void MoveSelection(int delta)
    {
        int count = _menuLabels.Length;
        if (count == 0)
            return;
        _selectedIndex = (_selectedIndex + delta + count) % count;
        RepaintMenu();
    }

    void ShowPressStart()
    {
        _phase = Phase.PressStart;
        _pressStartRoot.SetActive(true);
        _menuRoot.SetActive(false);
    }

    void ShowMenu()
    {
        _phase = Phase.Menu;
        _selectedIndex = 0;
        _pressStartRoot.SetActive(false);
        _menuRoot.SetActive(true);
        RepaintMenu();
    }

    void RepaintMenu()
    {
        for (int i = 0; i < _menuLabels.Length; i++)
        {
            if (i == _selectedIndex)
                _menuLabels[i].color = _selectedColor;
            else
                _menuLabels[i].color = _normalColor;
        }
    }

    void ConfirmSelection()
    {
        if (_selectedIndex == 0)
            StartNewGame();
        else if (_selectedIndex == 1)
            Continue();
        else if (_selectedIndex == 2)
            OpenOptions();
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
