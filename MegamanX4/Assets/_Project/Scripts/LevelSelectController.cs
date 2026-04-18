using UnityEngine;

public class LevelSelectController : MonoBehaviour
{
    [SerializeField] MenuNavigator _menu;
    [SerializeField] string[] _sceneNames;

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
        if (index < 0 || index >= _sceneNames.Length)
            return;
        var sceneName = _sceneNames[index];
        if (string.IsNullOrEmpty(sceneName))
            return;

        GameStateController.Instance.LoadStage(sceneName);
    }
}
