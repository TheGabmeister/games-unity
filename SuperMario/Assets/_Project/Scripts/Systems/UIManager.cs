using UnityEngine;
using TMPro;
using EventSystem;

public class UIManager : MonoBehaviour
{
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
    

    void Start()
    {
        ResetUI();
    }

    private void OnEnable()
    {
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
        //_currentScore.RemoveListener(UpdateScoreText);
        //_coins.RemoveListener(UpdateCoinsText);
        //_lives.RemoveListener(UpdateLivesText);
        //_currentLevel.RemoveListener(UpdateWorldText);

        //_onToggleLoadingScreen.RemoveListener(ToggleLoadingScreen);
        //_onPauseGame.RemoveListener(ShowPauseMenu);
        //_onUnpauseGame.RemoveListener(HidePauseMenu);
    }

    public void UpdateScoreText()
    {
         //_scoreText.SetText("Score: " + _currentScore.Value);
    }

    public void UpdateCoinsText()
    {
        //_coinsText.SetText("Coins: " + _coins.Value);
    }

    public void UpdateLivesText()
    {
        //_livesText.SetText("Lives: " + _lives.Value);
    }
    private void OnTimerUpdated()
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
