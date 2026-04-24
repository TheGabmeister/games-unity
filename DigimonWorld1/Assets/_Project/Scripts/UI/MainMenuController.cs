using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject _pressStartPanel;
    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private TMP_Text _pressStartText;
    [SerializeField] private TMP_Text[] _menuOptionTexts;

    private enum State
    {
        PressStart,
        Menu
    }

    private enum MenuOption
    {
        NewGame,
        ContinueGame,
        DeleteGame,
        BattleMode
    }

    private State _state = State.PressStart;
    private int _selectedOption;
    private float _blinkTimer;

    private void Update()
    {
        _blinkTimer += Time.deltaTime;

        if (_state == State.PressStart)
        {
            bool visible = Mathf.Sin(_blinkTimer * 3f) > 0f;
            _pressStartText.enabled = visible;
        }

        if (Keyboard.current == null)
            return;

        switch (_state)
        {
            case State.PressStart:
                if (Keyboard.current.enterKey.wasPressedThisFrame)
                {
                    _state = State.Menu;
                    _selectedOption = 0;
                    _pressStartPanel.SetActive(false);
                    _menuPanel.SetActive(true);
                    UpdateMenuHighlight();
                }
                break;

            case State.Menu:
                if (Keyboard.current.upArrowKey.wasPressedThisFrame)
                {
                    _selectedOption = (_selectedOption - 1 + _menuOptionTexts.Length) % _menuOptionTexts.Length;
                    UpdateMenuHighlight();
                }
                else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
                {
                    _selectedOption = (_selectedOption + 1) % _menuOptionTexts.Length;
                    UpdateMenuHighlight();
                }
                else if (Keyboard.current.enterKey.wasPressedThisFrame)
                {
                    SelectOption((MenuOption)_selectedOption);
                }
                break;
        }
    }

    private void UpdateMenuHighlight()
    {
        for (int i = 0; i < _menuOptionTexts.Length; i++)
        {
            if (i == _selectedOption)
            {
                _menuOptionTexts[i].color = Color.white;
                _menuOptionTexts[i].text = "> " + GetOptionLabel(i);
            }
            else
            {
                _menuOptionTexts[i].color = Color.gray;
                _menuOptionTexts[i].text = "  " + GetOptionLabel(i);
            }
        }
    }

    private static string GetOptionLabel(int index)
    {
        return index switch
        {
            0 => "New Game",
            1 => "Continue Game",
            2 => "Delete Game",
            3 => "Battle Mode",
            _ => ""
        };
    }

    private void SelectOption(MenuOption option)
    {
        switch (option)
        {
            case MenuOption.NewGame:
                GameManager.Instance.LoadNameScene();
                break;
            case MenuOption.ContinueGame:
                break;
            case MenuOption.DeleteGame:
                break;
            case MenuOption.BattleMode:
                break;
        }
    }
}
