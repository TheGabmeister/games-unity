using EventBus;
using UnityEngine;
using TMPro;

public class UiManager : MonoBehaviour
{
    [SerializeField] TMP_Text _scoreText;
    [SerializeField] GameObject _menuUi;
    [SerializeField] GameObject _gameplayUi;

    private void OnEnable()
    {
        Bus.UiUpdateScore.Sub(UpdateScore);
        Bus.UiToggleMenu.Sub(ToggleMenuUi);
        Bus.UiToggleGameplay.Sub(ToggleGameplayUi);
    }

    private void OnDisable()
    {
        Bus.UiUpdateScore.Unsub(UpdateScore);
        Bus.UiToggleMenu.Unsub(ToggleMenuUi);
        Bus.UiToggleGameplay.Unsub(ToggleGameplayUi);
    }

    public void StartGame()
    {
        Bus.GameStart.Publish();
    }

    public void RestartGame()
    {
        Bus.GameRestart.Publish();
    }

    void UpdateScore(int value)
    {
        _scoreText.text = value.ToString();
    }

    void ToggleMenuUi()
    {
        _menuUi.SetActive(true);
        _gameplayUi.SetActive(false);
    }

    void ToggleGameplayUi()
    {
        _menuUi.SetActive(false);
        _gameplayUi.SetActive(true);
    }
}
