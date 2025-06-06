using UnityEngine;
using EventBus;

// A camera fade script with customizable animation curve in editor. Just attach this to 
// the main camera and the viewport will always fade from black to clear for every new level.

public class FadeCamera : MonoBehaviour
{
    public AnimationCurve FadeCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.6f, 0.7f, -1.8f, -1.2f), new Keyframe(1, 0));

    float _currentAlpha = 1;
    float _targetAlpha = 0;

    Texture2D _texture;
    bool _done;
    float _time;

    void OnEnable()
    {
        Bus.CameraSetAlpha.Sub(SetTargetAlpha);
    }

    void OnDisable()
    {
        Bus.CameraSetAlpha.Unsub(SetTargetAlpha);
    }

    public void Reset()
    {
        _done = false;
        _targetAlpha = 0;
        _currentAlpha = 1;
        _time = 0;
    }

    void SetTargetAlpha(float value)
    {
        _targetAlpha = Mathf.Clamp01(value);
        _done = false;
        _time = 0;
    }

    [RuntimeInitializeOnLoadMethod]
    public void RedoFade()
    {
        Reset();
    }

    public void OnGUI()
    {
        if (_done) return;
        if (_texture == null) _texture = new Texture2D(1, 1);

        _time += Time.deltaTime;
        _currentAlpha = FadeCurve.Evaluate(_time);

        _texture.SetPixel(0, 0, new Color(0, 0, 0, _currentAlpha));
        _texture.Apply();
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _texture);

        if (Mathf.Abs(_targetAlpha - _currentAlpha) < 0.01)
        {
            _currentAlpha = _targetAlpha;
            _done = true;
            return;
        }
    }
}