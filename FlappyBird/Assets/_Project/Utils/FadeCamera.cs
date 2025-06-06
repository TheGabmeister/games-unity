using UnityEngine;
using EventBus;

// A camera fade script with customizable animation curve in editor. Just attach this to 
// the main camera and the viewport will always fade from black to clear for every new level.

public class FadeCamera : MonoBehaviour
{
    float _currentAlpha = 1;
    float _targetAlpha = 0;

    Texture2D _texture;
    bool _isClear = false;

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
        _isClear = false;
        _targetAlpha = 0;
        _currentAlpha = 1;
    }

    void SetTargetAlpha(float value)
    {
        _targetAlpha = Mathf.Clamp01(value);
        _isClear = false;
    }

    [RuntimeInitializeOnLoadMethod]
    public void RedoFade()
    {
        Reset();
    }

    public void OnGUI()
    {
        if (_isClear) return;
        if (_texture == null)
        {
            _texture = new Texture2D(1, 1);

        }
        _currentAlpha += Mathf.Sign(_targetAlpha - _currentAlpha) * Time.deltaTime;

        _texture.SetPixel(0, 0, new Color(0, 0, 0, _currentAlpha));
        _texture.Apply();
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _texture);

        if (_currentAlpha <= 0)
        {
            _isClear = true;
            return;
        }

        if (Mathf.Abs(_targetAlpha - _currentAlpha) < 0.01)
        {
            _currentAlpha = _targetAlpha;
            return;
        }
    }
}