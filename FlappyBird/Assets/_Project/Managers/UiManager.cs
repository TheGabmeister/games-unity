using SimpleEventSystem;
using UnityEngine;
using TMPro;

public class UiManager : MonoBehaviour
{
    [SerializeField] TMP_Text _scoreText;
    [SerializeField] GameObject _menuUi;
    [SerializeField] GameObject _gameplayUi;

    private void OnEnable()
    {
        Events.UiUpdateScore.Sub(UpdateScore);
    }

    private void OnDisable()
    {
        Events.UiUpdateScore.Unsub(UpdateScore);
    }

    public void StartGame()
    {
        Events.GameStart.Publish();
    }

    public void RestartGame()
    {
        Events.GameRestart.Publish();
    }

    void UpdateScore(int value)
    {
        _scoreText.text = value.ToString();
    }

    public void ToggleMenuUi()
    {
        _menuUi.SetActive(true);
        _gameplayUi.SetActive(false);
    }

    public void ToggleGameplayUi()
    {
        _menuUi.SetActive(false);
        _gameplayUi.SetActive(true);
    }

}
