using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class LevelSelectController : MonoBehaviour
{
    [Serializable]
    class StageEntry
    {
        public TMP_Text label;
        public string sceneName;
    }

    [SerializeField] StageEntry[] _stages;
    [SerializeField] Color _selectedColor = Color.yellow;
    [SerializeField] Color _normalColor = Color.white;

    PlayerInput _playerInput;
    InputAction _submitAction;
    InputAction _navigateAction;

    int _selectedIndex;

    void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _submitAction = _playerInput.actions["Submit"];
        _navigateAction = _playerInput.actions["Navigate"];
        Repaint();
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
        if (_stages.Length == 0)
            return;

        var entry = _stages[_selectedIndex];
        if (string.IsNullOrEmpty(entry.sceneName))
            return;

        if (Services.TryGet<GameStateController>(out var gameState))
            gameState.LoadStage(entry.sceneName);
    }

    void OnNavigate(InputAction.CallbackContext ctx)
    {
        float y = ctx.ReadValue<Vector2>().y;
        if (y > 0.5f)
            MoveSelection(-1);
        else if (y < -0.5f)
            MoveSelection(1);
    }

    void MoveSelection(int delta)
    {
        int count = _stages.Length;
        if (count == 0)
            return;
        _selectedIndex = (_selectedIndex + delta + count) % count;
        Repaint();
    }

    void Repaint()
    {
        for (int i = 0; i < _stages.Length; i++)
        {
            var label = _stages[i].label;
            if (!label)
                continue;

            if (i == _selectedIndex)
                label.color = _selectedColor;
            else
                label.color = _normalColor;
        }
    }
}
