using SimpleEventSystem;
using UnityEngine;
using TMPro;
using PrimeTween;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [SerializeField] TMP_Text _scoreText;
    int _score = 100;
    [SerializeField] GameObject _preGameText;
    [SerializeField] Image _gameOverImage;
    [SerializeField] GameObject _gameOverButtons;
    [SerializeField] TMP_Text _gameOverScoreText;
    
    [SerializeField] Image _medalImage;
    [SerializeField] GameObject _menuUi;
    [SerializeField] GameObject _gameplayUi;

    void Start()
    {
        Init();        
    }

    public void Init()
    {
        ToggleMenuUi();
        _medalImage.enabled = false;
        _gameOverButtons.SetActive(false);
        _preGameText.SetActive(true);
        var color = _gameOverImage.color;
        color.a = 0f;
        _gameOverImage.color = color;
    }

    public void DisablePreGameText()
    {
        _preGameText.SetActive(false);
    }

    public void StartGame()
    {
        Events.GameStart.Publish();
    }

    public void RestartGame()
    {
        Events.GameRestart.Publish();
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
        float duration = 1f;
        int endValue = 100;
        Tween.Custom(0, endValue, duration, onValueChange: value =>
            _gameOverScoreText.text = Mathf.RoundToInt(value).ToString()
        ).OnComplete(() => _gameOverButtons.SetActive(true));
    }
}
