using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenuController : MonoBehaviour
{
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

    private static readonly string[] MenuLabels = { "New Game", "Continue Game", "Delete Game", "Battle Mode" };

    private State _state = State.PressStart;
    private int _selectedOption;
    private float _blinkTimer;

    private void Update()
    {
        _blinkTimer += Time.deltaTime;

        if (Keyboard.current == null)
            return;

        switch (_state)
        {
            case State.PressStart:
                if (Keyboard.current.enterKey.wasPressedThisFrame)
                {
                    _state = State.Menu;
                    _selectedOption = 0;
                }
                break;

            case State.Menu:
                if (Keyboard.current.upArrowKey.wasPressedThisFrame)
                    _selectedOption = (_selectedOption - 1 + MenuLabels.Length) % MenuLabels.Length;
                else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
                    _selectedOption = (_selectedOption + 1) % MenuLabels.Length;
                else if (Keyboard.current.enterKey.wasPressedThisFrame)
                    SelectOption((MenuOption)_selectedOption);
                break;
        }
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

    private void OnGUI()
    {
        switch (_state)
        {
            case State.PressStart:
                DrawPressStart();
                break;
            case State.Menu:
                DrawMenu();
                break;
        }
    }

    private void DrawPressStart()
    {
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 36,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };

        GUIStyle blinkStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };

        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;

        GUI.Label(new Rect(cx - 200f, cy - 100f, 400f, 60f), "DIGIMON WORLD", titleStyle);

        bool visible = Mathf.Sin(_blinkTimer * 3f) > 0f;
        if (visible)
            GUI.Label(new Rect(cx - 150f, cy + 20f, 300f, 40f), "Press Start", blinkStyle);
    }

    private void DrawMenu()
    {
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 36,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };

        GUIStyle normalStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = Color.gray }
        };

        GUIStyle selectedStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = Color.white }
        };

        float cx = Screen.width / 2f;
        float menuX = cx - 100f;
        float menuY = Screen.height / 2f - 40f;

        GUI.Label(new Rect(cx - 200f, menuY - 100f, 400f, 60f), "DIGIMON WORLD", titleStyle);

        for (int i = 0; i < MenuLabels.Length; i++)
        {
            GUIStyle style = (i == _selectedOption) ? selectedStyle : normalStyle;
            string prefix = (i == _selectedOption) ? "> " : "  ";
            GUI.Label(new Rect(menuX, menuY + i * 35f, 300f, 35f), prefix + MenuLabels[i], style);
        }
    }
}
