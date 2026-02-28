using UnityEngine;
using TMPro;
using EventSystem;

public class GameUIManager : MonoBehaviour
{
    [SerializeField] GameMode _gameMode;
    //[SerializeField] IntReference _currentScore;
    //[SerializeField] IntReference _coins;
    //[SerializeField] IntReference _lives;
    [SerializeField] GameObject _loadingScreen;
    [SerializeField] GameObject _gameOver;
    [SerializeField] TMP_Text _scoreText;
    [SerializeField] TMP_Text _coinsText;
    [SerializeField] TMP_Text _livesText;
    [SerializeField] TMP_Text[] _worldText;
    [SerializeField] TMP_Text _timeText;
    [SerializeField] GameObject _pauseMenu;
    //[SerializeField] SceneDataReference _currentLevel;

    [Header("Listen to these events...")]
    //[SerializeField] BoolGameEvent _onToggleLoadingScreen;
    [SerializeField] GameEvent _onPauseGame;
    [SerializeField] GameEvent _onUnpauseGame;

    private void OnEnable()
    {
        Events.LevelDataInitialized.Sub(OnLevelDataInitialized);
        GameInstance.StatsCoinsUpdated += OnCoinsUpdated;
        GameInstance.StatsLivesUpdated += OnLivesUpdated;
        GameInstance.StatsScoreUpdated += OnScoreUpdated;
        Events.TimerUpdated.Sub(OnTimerUpdated);
    }

    private void OnDisable()
    {
        Events.LevelDataInitialized.Unsub(OnLevelDataInitialized);
        GameInstance.StatsCoinsUpdated -= OnCoinsUpdated;
        GameInstance.StatsLivesUpdated -= OnLivesUpdated;
        GameInstance.StatsScoreUpdated -= OnScoreUpdated;
        Events.TimerUpdated.Unsub(OnTimerUpdated);
    }

    public void RestartGame()
    {
        GameInstance.Instance.RestartGame();
    }

    void OnLevelDataInitialized(LevelData levelData)
    {
        UpdateWorldText(levelData.displayName);
        OnTimerUpdated(levelData.time);
    }

    public void OnScoreUpdated(int value)
    {
         _scoreText.SetText("Score: " + value);
    }

    public void OnCoinsUpdated(int value)
    {
        _coinsText.SetText("Coins: " + value);
    }

    public void OnLivesUpdated(int value)
    {
        _livesText.SetText("Lives: " + value);
    }
    private void OnTimerUpdated(int value)
    {
        _timeText.SetText("Time: " + value);
    }

    public void UpdateWorldText(string value)
    {
        foreach (TMP_Text text in _worldText)
        {
            _worldText[0].SetText(value);
            text.SetText(value);
        }
            
    }

    void ToggleLoadingScreen(bool value)
    {
        _loadingScreen.SetActive(value);
    }

    void HideLoadingScreen()
    {
        _loadingScreen.SetActive(false);
    }

    public void ShowGameOverUI()
    {
        _gameOver.SetActive(true);
    }

    void ShowPauseMenu()
    {
        _pauseMenu.SetActive(true);
    }
    void HidePauseMenu()
    {
        _pauseMenu.SetActive(false);
    }

    void ResetUI()
    {
        _scoreText.SetText("Score: 0");
        _coinsText.SetText("Coins: 0");
        _livesText.SetText("Lives: 3");
        _gameOver.SetActive(false);
    }

}
