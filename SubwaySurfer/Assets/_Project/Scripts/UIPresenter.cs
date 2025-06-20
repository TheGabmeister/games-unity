using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Obvious.Soap;

public class UIPresenter : MonoBehaviour
{
    [SerializeField] GameObject[] _uiElements;
    [SerializeField] TMP_Text _scoreText;
    [SerializeField] TMP_Text _distanceText;
    [SerializeField] TMP_Text _coinsText;
    
    [Header("Listen to these events...")] 
    [SerializeField] ScriptableEventUIState _onUIStateChange;
    
    [Header("Call these events...")] 
    [SerializeField] ScriptableEventNoParam _onGameStateStart;
    
    int _score = 100;

    void OnEnable()
    {
        _onUIStateChange.OnRaised += ChangeState;
    }
    
    void OnDisable()
    {
        _onUIStateChange.OnRaised -= ChangeState;
    }
    
    void Start()
    {
        ChangeState(UIState.Menu);
    }
    
    public void StartGame()
    {
        _onGameStateStart.Raise();
    }

    void ToggleUiElement(int index)
    {
        for (int i = 0; i < _uiElements.Length; i++)
        {
            _uiElements[i].SetActive(i == index);
        }
    }
    

	void ChangeState(UIState state)
	{
        switch (state)
        {
            case UIState.Menu:
                ToggleUiElement(0);
                break;
            case UIState.Gameplay:
                ToggleUiElement(1);
                break;
            case UIState.GameOver:
                ToggleUiElement(2);
                break;
        }
	}
    
}

public enum UIState
{
    Menu,
    Gameplay,
    GameOver,
}