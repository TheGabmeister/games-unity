using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using PrimeTween;

public class UiController : MonoBehaviour
{
    [Header("MainMenuUI")]
    [SerializeField] UIFruit[] _mainMenuFruits;
    [Header("GameModeUI")]
    [SerializeField] UIFruit[] _gameModeFruits;
    
    [Header("GameplayUI")]
    [SerializeField] GameObject[] _uiElements;
    [SerializeField] GameObject _gameOverUI;
    [SerializeField] GameObject _TimeUpUI;
    [SerializeField] TMP_Text _scoreText;
    [SerializeField] TMP_Text _livesText;
    [SerializeField] TMP_Text _timerText;
    Animation _anim;

    private void OnEnable()
    {
        Events.GameScoreUpdated += OnGameScoreUpdated;
        Events.GameLivesUpdated += OnGameLivesUpdated;
        Events.GameTimerUpdated += OnGameTimerUpdated;
        Events.GameClassicModeStarted += OnClassicModeStarted;
        Events.GameZenModeStarted += OnZenModeStarted;
        Events.GameClassicModeEnded += OnClassicModeEnded;
        Events.GameZenModeEnded += OnZenModeEnded;
    }
    
    private void OnDisable()
    {
        Events.GameScoreUpdated -= OnGameScoreUpdated;
        Events.GameLivesUpdated -= OnGameLivesUpdated;
        Events.GameTimerUpdated -= OnGameTimerUpdated;
        Events.GameClassicModeStarted -= OnClassicModeStarted;
        Events.GameZenModeStarted -= OnZenModeStarted;
        Events.GameClassicModeEnded -= OnClassicModeEnded;
        Events.GameZenModeEnded -= OnZenModeEnded;
    }

    void Awake()
    {
        _anim = GetComponent<Animation>();
    }

    void Start()
    {
        // play animation
    }

    public void OnNewGameClicked()
    {
        Sequence.Create()
            .ChainCallback(() =>
            {
                foreach (UIFruit fruit in _mainMenuFruits)
                    fruit.Exit();
            })
            .ChainDelay(1f)
            .ChainCallback(() =>
            {
                foreach (UIFruit fruit in _gameModeFruits)
                    fruit.Init();
            });
    }

    public void OnDojoClicked()
    {
        Debug.Log("OnDojoClicked");
    }
    
    public void OnMoreGamesClicked()
    {
        Debug.Log("OnMoreGamesClicked");
    }
    
    void OnGameScoreUpdated(int score)
    {
        _scoreText.text = score.ToString();
    }
    
    void OnGameLivesUpdated(int lives)
    {
        _livesText.text = lives.ToString();
    }

    void OnGameTimerUpdated(int time)
    {
        _timerText.text = time.ToString();
    }
    
    void OnClassicModeStarted()
    {
        _livesText.gameObject.SetActive(true);
        _timerText.gameObject.SetActive(false);
    }
    
    void OnZenModeStarted()
    {
        _livesText.gameObject.SetActive(false);
        _timerText.gameObject.SetActive(true);
    }
    
    void OnClassicModeEnded()
    {
        _gameOverUI.SetActive(true);
    }
    
    void OnZenModeEnded()
    {
        _gameOverUI.SetActive(true);
    }

    public void StartGame()
    {
        
    }

    public void RestartGame()
    {
        
    }
    

    void ToggleUiElement(int index)
    {
        for (int i = 0; i < _uiElements.Length; i++)
        {
            _uiElements[i].SetActive(i == index);
        }
    }
    

	void ChangeState(UiState state)
	{
        switch (state)
        {
            case UiState.Start:
                ToggleUiElement(0);
                break;
            case UiState.Gameplay:
                ToggleUiElement(1);
                break;
            case UiState.GameOver:
                ToggleUiElement(2);
                break;
            case UiState.Score:
                ToggleUiElement(3);
                break;
        }
	}

    // Referenced in animation clip
    void AnimateScore()
    {
        // float duration = 1f;
        // int endValue = 100;
        // Tween.Custom(0, endValue, duration, onValueChange: value =>
        //     _gameOverScoreText.text = Mathf.RoundToInt(value).ToString()
        // ).OnComplete(() => _gameOverButtons.SetActive(true));
    }
}

public enum UiState
{
    Start,
    Gameplay,
    GameOver,
    Score
}