using UnityEngine;

// A simple black screen fader 

[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    float _currentAlpha = 0;
    float _targetAlpha = 0;
    float _duration = 1;
    bool _isDone = false;
    CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Reset()
    {
        _targetAlpha = 0;
        _currentAlpha = 1;
    }

    public void FadeToBlack(float duration)
    {
        _duration = duration;
        _targetAlpha = 1.0f;
        _isDone = false;
    }

    public void FadeToClear(float duration)
    {
        _duration = duration;
        _targetAlpha = 0.0f;
        _isDone = false;
    }

    // FIXME: Fire everytime a new level is loaded
    // [RuntimeInitializeOnLoadMethod]
    // public void RedoFade()
    // {
    //     Reset();
    // }

    public void Update()
    {
        if (_isDone) return;

        if (Mathf.Abs(_targetAlpha - _currentAlpha) < 0.01)
        {
            _currentAlpha = _targetAlpha;
            canvasGroup.alpha = _currentAlpha;
            _isDone = true;
            return;
        }

        _currentAlpha = Mathf.MoveTowards(_currentAlpha, _targetAlpha, Time.deltaTime / _duration);
        canvasGroup.alpha = _currentAlpha;
    }
}