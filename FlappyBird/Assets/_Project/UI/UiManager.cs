using SimpleEventSystem;
using UnityEngine;
using TMPro;
using PrimeTween;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [SerializeField] TMP_Text _scoreText;
    int _score = 100;
    [SerializeField] Image _gameOverImage;
    [SerializeField] GameObject _gameOverButtons;
    [SerializeField] TMP_Text _gameOverScoreText;
    
    [SerializeField] Image _medalImage;
    [SerializeField] GameObject _menuUi;
    [SerializeField] GameObject _gameplayUi;

    private void OnEnable()
    {
        //Events.UiUpdateScore.Sub(UpdateScore);
        Events.PlayerDied.Sub(StartGameOverUiSequence);
    }

    private void OnDisable()
    {
        //Events.UiUpdateScore.Unsub(UpdateScore);
        Events.PlayerDied.Unsub(StartGameOverUiSequence);
    }

    void Start()
    {
        Init();        
    }

    void Init()
    {
        ToggleMenuUi();
        _medalImage.enabled = false;
        _gameOverButtons.SetActive(false);
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
        GetComponent<Animation>().Play();
    }

    // Referenced in animation clip
    void AnimateScore()
    {
        float duration = 1f;
        int endValue = 100;
        Tween.Custom(0, endValue, duration, onValueChange: value =>
            _gameOverScoreText.text = Mathf.RoundToInt(value).ToString()
        ).OnComplete(() => _gameOverButtons.SetActive(true));
    }
}
