using UnityEngine;

public class Selectable : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _selectionCircle;
    [SerializeField] private HealthBar _healthBar;

    private bool _selected;

    public bool IsSelected => _selected;

    void Awake()
    {
        if (_selectionCircle != null) _selectionCircle.enabled = false;
        if (_healthBar != null) _healthBar.SetVisible(false);
    }

    public void Select()
    {
        _selected = true;
        if (_selectionCircle != null) _selectionCircle.enabled = true;
        if (_healthBar != null) _healthBar.SetVisible(true);
    }

    public void Deselect()
    {
        _selected = false;
        if (_selectionCircle != null) _selectionCircle.enabled = false;
        if (_healthBar != null) _healthBar.SetVisible(false);
    }
}
