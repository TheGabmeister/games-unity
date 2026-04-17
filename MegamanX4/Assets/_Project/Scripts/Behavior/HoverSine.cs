using UnityEngine;

public class HoverSine : MonoBehaviour
{
    [SerializeField] float _amplitude = 1f;
    [SerializeField] float _frequency = 0.5f;

    float _centerY;
    float _phase;
    bool _paused;

    void Awake()
    {
        _centerY = transform.position.y;
    }

    void Update()
    {
        if (_paused) return;

        _phase += Time.deltaTime * _frequency * Mathf.PI * 2f;
        var pos = transform.position;
        pos.y = _centerY + Mathf.Sin(_phase) * _amplitude;
        transform.position = pos;
    }

    public void Pause() => _paused = true;

    public void Resume()
    {
        _paused = false;
        _centerY = transform.position.y;
        _phase = 0f;
    }

    public void SetCenter(float y) => _centerY = y;

    public float CenterY => _centerY;
}
