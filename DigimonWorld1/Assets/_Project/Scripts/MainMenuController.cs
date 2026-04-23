using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenuController : MonoBehaviour
{
    private enum State
    {
        PressStart,
        Menu
    }

    private State _state = State.PressStart;

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        switch (_state)
        {
            case State.PressStart:
                if (Keyboard.current.enterKey.wasPressedThisFrame)
                    _state = State.Menu;
                break;

            case State.Menu:
                if (Keyboard.current.enterKey.wasPressedThisFrame)
                    GameManager.Instance.LoadNameScene();
                break;
        }
    }
}
