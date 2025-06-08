using SimpleEventSystem;
using UnityEngine;
using TMPro;
using PrimeTween;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [SerializeField] TMP_Text _scoreText;
    [SerializeField] Image _gameOverImage;
    [SerializeField] GameObject _menuUi;
    [SerializeField] GameObject _gameplayUi;

    private void OnEnable()
    {
        Events.UiUpdateScore.Sub(UpdateScore);
        Events.PlayerDied.Sub(StartGameOverUiSequence);
    }

    private void OnDisable()
    {
        Events.UiUpdateScore.Unsub(UpdateScore);
        Events.PlayerDied.Unsub(StartGameOverUiSequence);
    }

    void Init()
    {
        ToggleMenuUi();

        var color = _gameOverImage.color;
        color.a = 0f;
        _gameOverImage.color = color;
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

    public void StartGameOverUiSequence()
    {

          //_gameOverText.DOFade(1f, 1f) 
          //  .OnComplete(() =>
          //  {
          //      restartButton.SetActive(true);
          //  });
    }
}
