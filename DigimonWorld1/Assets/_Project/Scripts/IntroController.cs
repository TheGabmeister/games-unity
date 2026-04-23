using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class IntroController : MonoBehaviour
{
    [SerializeField] private VideoPlayer _videoPlayer;

    private bool _transitioning;

    private void OnEnable()
    {
        _videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnDisable()
    {
        _videoPlayer.loopPointReached -= OnVideoFinished;
    }

    private void Update()
    {
        if (_transitioning)
            return;

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            Skip();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        Skip();
    }

    private void Skip()
    {
        if (_transitioning)
            return;

        _transitioning = true;
        _videoPlayer.Stop();
        GameManager.Instance.LoadMainMenuScene();
    }
}
