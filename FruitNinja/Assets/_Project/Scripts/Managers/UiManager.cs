using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [SerializeField] GameObject[] _uiElements;
    [SerializeField] GameObject _gameOverUI;
    [SerializeField] TMP_Text _scoreText;
    [SerializeField] TMP_Text _livesText;
    Animation _anim;

    private void OnEnable()
    {
        Events.GameScoreUpdated += OnGameScoreUpdated;
        Events.GameLivesUpdated += OnGameLivesUpdated;
        Events.GameEnded += OnGameEnded;
    }
    
    private void OnDisable()
    {
        Events.GameScoreUpdated -= OnGameScoreUpdated;
        Events.GameLivesUpdated -= OnGameLivesUpdated;
        Events.GameEnded -= OnGameEnded;
    }

    void Awake()
    {
        _anim = GetComponent<Animation>();
    }
    
    void OnGameScoreUpdated(int score)
    {
        _scoreText.text = score.ToString();
    }
    
    void OnGameLivesUpdated(int lives)
    {
        _livesText.text = lives.ToString();
    }

    void OnGameEnded()
    {
        _gameOverUI.SetActive(true);
    }

    void Start() => Init();

    public void Init()
    {
        _scoreText.text = "0";
        ChangeState(UiState.Start);
        ResetGameOverAnimation();
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
    
    public void ResetGameOverAnimation()
    {
        // _medalImage.enabled = false;
        // _gameOverButtons.SetActive(false);

        // if (_anim != null && _anim.clip != null)
        // {
        //     _anim.Play(_anim.clip.name);
        //     _anim[_anim.clip.name].time = 0f;
        //     _anim.Sample();
        //     _anim.Stop();
        // }
    }
}

public enum UiState
{
    Start,
    Gameplay,
    GameOver,
    Score
}