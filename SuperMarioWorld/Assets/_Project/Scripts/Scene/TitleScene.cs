using UnityEngine;
using UnityEngine.InputSystem;

public sealed class TitleScene : MonoBehaviour
{
    private bool _transitioning;

    private void Update()
    {
        if (_transitioning) return;

        var keyboard = Keyboard.current;
        var gamepad = Gamepad.current;

        bool pressed = (keyboard != null && keyboard.enterKey.wasPressedThisFrame)
                    || (gamepad != null && gamepad.startButton.wasPressedThisFrame);

        if (pressed)
        {
            _transitioning = true;
            GameStateMachine.Instance.TransitionToOverworld();
        }
    }
}
