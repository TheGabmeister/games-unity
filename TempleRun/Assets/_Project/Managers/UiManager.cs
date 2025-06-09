using EventBus;
using UnityEngine;
using TMPro;
using PrimeTween;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [SerializeField] GameObject _startUi;
    [SerializeField] GameObject _gameplayUi;
    [SerializeField] GameObject _gameOverUi;
    [SerializeField] TMP_Text _scoreText;
    [SerializeField] TMP_Text _coinsText;
    int _score = 100;
    Animation _anim;
    
    void OnEnable()
    {
        Bus<EV_UiShowStart>.Add(ShowStartUi);
        Bus<EV_UiShowGameplay>.Add(ShowGameplayUi);
        Bus<EV_UiShowGameOver>.Add(ShowGameOverUi);
    }

    void OnDisable()
    {
        Bus<EV_UiShowStart>.Remove(ShowStartUi);
        Bus<EV_UiShowGameplay>.Remove(ShowGameplayUi);
        Bus<EV_UiShowGameOver>.Remove(ShowGameOverUi);
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
        ShowStartUi();
        ResetGameOverAnimation();
    }


    public void StartGame()
    {
        ShowGameplayUi();
        Bus<EV_GameStart>.Raise();
    }

    public void RestartGame()
    {
        Bus<EV_GameRestart>.Raise();
    }

    public void UpdateScore(int value)
    {
        _scoreText.text = value.ToString();
    }

    void ShowStartUi()
    {
        _startUi.SetActive(true);
        _gameplayUi.SetActive(false);
        _gameOverUi.SetActive(false);
    }

    void ShowGameplayUi()
    {
        _startUi.SetActive(false);
        _gameplayUi.SetActive(true);
        _gameOverUi.SetActive(false);
    }

    void ShowGameOverUi()
    {
        _startUi.SetActive(false);
        _gameplayUi.SetActive(false);
        _gameOverUi.SetActive(true);
    }

    public void StartGameOverUiSequence()
    {
        GetComponent<Animation>().Play();
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
