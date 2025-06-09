using EventBus;
using UnityEngine;
using TMPro;
using PrimeTween;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [SerializeField] GameObject _startUi;
    [SerializeField] GameObject _gameplayUi;
    [SerializeField] TMP_Text _scoreText;
    int _score = 100;
    Animation _anim;
    
    [SerializeField] TMP_Text _gameOverScoreText;

    [SerializeField] Image _medalImage;
    [SerializeField] GameObject _menuUi;
    

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
        ToggleMenuUi();
        ResetGameOverAnimation();
        
    }


    public void StartGame()
    {
        _startUi.SetActive(false);
        _gameplayUi.SetActive(true);
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
