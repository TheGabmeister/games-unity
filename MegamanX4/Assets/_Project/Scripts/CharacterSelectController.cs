using UnityEngine;

public class CharacterSelectController : MonoBehaviour
{
    [SerializeField] MenuNavigator _menu;
    [SerializeField] string _xStageSceneName = "SkyLagoon";

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
        if (Services.TryGet<GameStateController>(out var gameState))
            gameState.LoadStage(_xStageSceneName);
    }

    void SelectZero()
    {
    }
}
