using EventBus;
using UnityEngine;
using TMPro;
using PrimeTween;
using UnityEngine.UI;

public enum UiState
{
    Start,
    Gameplay,
    GameOver,
    Score
}

public class UiManager : MonoBehaviour
{
    [SerializeField] GameObject[] _uiElements;
    [SerializeField] TMP_Text _scoreText;
    [SerializeField] TMP_Text _distanceText;
    [SerializeField] TMP_Text _coinsText;
    int _score = 100;
    Animation _anim;
    
    void OnEnable()
    {
        Bus<EV_UiStatsUpdate>.Add(UpdateStats);
        Bus<EV_UiStateChange>.Add(ChangeStateHelper);
    }

    void OnDisable()
    {
        Bus<EV_UiStatsUpdate>.Remove(UpdateStats);
        Bus<EV_UiStateChange>.Remove(ChangeStateHelper);
    }
    

    void Awake()
    {
        _anim = GetComponent<Animation>();
    }

    void Start()
    {
        Init();
    }

    public void Init()
    {
        _scoreText.text = "0";
        ChangeState(UiState.Start);
        ResetGameOverAnimation();
    }


    public void StartGame()
    {
        Bus<EV_GameStart>.Raise();
    }

    public void RestartGame()
    {
        Bus<EV_GameRestart>.Raise();
    }

    public void UpdateStats(EV_UiStatsUpdate e)
    {
        _scoreText.text = e.score.ToString();
        _distanceText.text = e.distance.ToString();
        _coinsText.text = e.coins.ToString();
    }

    void ToggleUiElement(int index)
    {
        for (int i = 0; i < _uiElements.Length; i++)
        {
            _uiElements[i].SetActive(i == index);
        }
    }
    
    void ChangeStateHelper(EV_UiStateChange e)
    {
		ChangeState(e.state);
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

