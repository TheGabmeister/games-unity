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
        _gameMode.LevelDataInitialized += OnLevelDataInitialized;
        //_currentScore.AddListener(UpdateScoreText);
        //_coins.AddListener(UpdateCoinsText);
        //_lives.AddListener(UpdateLivesText);
        //_currentLevel.AddListener(UpdateWorldText);

        //_onToggleLoadingScreen.AddListener(ToggleLoadingScreen);
        //_onPauseGame.AddListener(ShowPauseMenu);
        //_onUnpauseGame.AddListener(HidePauseMenu);
    }

    private void OnDisable()
    {
        _gameMode.LevelDataInitialized += OnLevelDataInitialized;
        //_currentScore.RemoveListener(UpdateScoreText);
        //_coins.RemoveListener(UpdateCoinsText);
        //_lives.RemoveListener(UpdateLivesText);
        //_currentLevel.RemoveListener(UpdateWorldText);

        //_onToggleLoadingScreen.RemoveListener(ToggleLoadingScreen);
        //_onPauseGame.RemoveListener(ShowPauseMenu);
        //_onUnpauseGame.RemoveListener(HidePauseMenu);
    }

    void Start()
    {
        ResetUI();
    }

    public void RestartGame()
    {
        GameInstance.Instance.RestartGame();
    }

    void OnLevelDataInitialized(LevelData levelData)
    {
        //OnScoreUpdated()
    }

    public void OnScoreUpdated(int value)
    {
         //_scoreText.SetText("Score: " + _currentScore.Value);
    }

    public void OnCoinsUpdated(int value)
    {
        //_coinsText.SetText("Coins: " + _coins.Value);
    }

    public void OnLivesUpdated(int value)
    {
        //_livesText.SetText("Lives: " + _lives.Value);
    }
    private void OnTimerUpdated(int value)
    {
        //_timeText.SetText("Time: " + _remainingTime.Value);
    }

    public void UpdateWorldText()
    {
        foreach (TMP_Text text in _worldText)
        {
            //_worldText[0].SetText(_currentLevel.Value.displayName);
            //text.SetText(_currentLevel.Value.displayName);
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
