using System;
using TMPro;
using UnityEngine;

public class MenuNavigator : MonoBehaviour
{
    public enum NavMode { Vertical, Horizontal }

    [SerializeField] TMP_Text[] _labels;
    [SerializeField] NavMode _mode = NavMode.Vertical;
    [SerializeField] Color _selectedColor = Color.yellow;
    [SerializeField] Color _normalColor = Color.white;

    public event Action<int> Confirmed;

    int _selectedIndex;

    public int SelectedIndex => _selectedIndex;
    public int ItemCount => _labels.Length;

    void OnEnable()
    {
        Repaint();
    }

    public void ResetSelection()
    {
        _selectedIndex = 0;
        Repaint();
    }

    public void Submit()
    {
        if (_labels.Length == 0)
            return;
        Confirmed?.Invoke(_selectedIndex);
    }

    public void Navigate(Vector2 direction)
    {
        if (_mode == NavMode.Vertical)
        {
            if (direction.y > 0.5f)
                MoveSelection(-1);
            else if (direction.y < -0.5f)
                MoveSelection(1);
        }
        else
        {
            if (direction.x < -0.5f)
                MoveSelection(-1);
            else if (direction.x > 0.5f)
                MoveSelection(1);
        }
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
