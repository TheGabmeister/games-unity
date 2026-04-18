using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuNavigator : MonoBehaviour
{
    [SerializeField] TMP_Text[] _labels;
    [SerializeField] Color _selectedColor = Color.yellow;
    [SerializeField] Color _normalColor = Color.white;

    public event Action<int> Confirmed;

    PlayerInput _playerInput;
    InputAction _submitAction;
    InputAction _navigateAction;
    int _selectedIndex;

    public int SelectedIndex => _selectedIndex;
    public int ItemCount => _labels.Length;

    void Awake()
    {
        _playerInput = GetComponentInParent<PlayerInput>(true);
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

    public void ResetSelection()
    {
        _selectedIndex = 0;
        Repaint();
    }

    void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (_labels.Length == 0)
            return;
        Confirmed?.Invoke(_selectedIndex);
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
        int count = _labels.Length;
        if (count == 0)
            return;
        _selectedIndex = (_selectedIndex + delta + count) % count;
        Repaint();
    }

    void Repaint()
    {
        for (int i = 0; i < _labels.Length; i++)
        {
            var label = _labels[i];
            if (!label)
                continue;

            if (i == _selectedIndex)
                label.color = _selectedColor;
            else
                label.color = _normalColor;
        }
    }
}
