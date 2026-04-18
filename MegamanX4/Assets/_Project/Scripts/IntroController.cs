using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

[RequireComponent(typeof(PlayerInput))]
public class IntroController : MonoBehaviour
{
    [SerializeField] VideoPlayer _videoPlayer;

    PlayerInput _playerInput;
    InputAction _submitAction;
    bool _transitioned;

    void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _submitAction = _playerInput.actions["Submit"];
    }

    void OnEnable()
    {
        _submitAction.started += OnSubmit;
        if (_videoPlayer)
            _videoPlayer.loopPointReached += OnVideoEnd;
    }

    void OnDisable()
    {
        _submitAction.started -= OnSubmit;
        if (_videoPlayer)
            _videoPlayer.loopPointReached -= OnVideoEnd;
    }

    void OnSubmit(InputAction.CallbackContext ctx)
    {
        GoToTitle();
    }

    void OnVideoEnd(VideoPlayer source)
    {
        GoToTitle();
    }

    void GoToTitle()
    {
        if (_transitioned)
            return;
        _transitioned = true;

        GameStateEvents.SetState.Raise(GameState.Title);
    }
}
