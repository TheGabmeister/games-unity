using UnityEngine;

public class CharacterSelectController : MonoBehaviour
{
    [SerializeField] MenuNavigator _menu;

    void OnEnable()
    {
        if (_menu)
            _menu.Confirmed += OnConfirm;
    }

    void OnDisable()
    {
        if (_menu)
            _menu.Confirmed -= OnConfirm;
    }

    void OnConfirm(int index)
    {
        if (index == 0)
            SelectX();
        else if (index == 1)
            SelectZero();
    }

    void SelectX()
    {
        GameStateEvents.SetState.Raise(GameState.SkyLagoon);
    }

    void SelectZero()
    {
    }
}
