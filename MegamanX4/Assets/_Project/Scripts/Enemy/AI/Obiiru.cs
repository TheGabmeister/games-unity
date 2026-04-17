using UnityEngine;

public class Obiiru : MonoBehaviour
{
    [SerializeField] Transform _anchor;
    [SerializeField] float _ropeLength = 2f;
    [SerializeField] float _swingAngle = 50f;
    [SerializeField] float _swingPeriod = 2f;

    Vector3 _anchorPosition;
    float _time;

    void Awake()
    {
        _anchorPosition = _anchor ? _anchor.position : transform.position + Vector3.up * _ropeLength;
    }

    void Update()
    {
        _time += Time.deltaTime;

        float phase = Mathf.Sin(_time * Mathf.PI * 2f / _swingPeriod);
        float angleDeg = _swingAngle * phase;
        float angleRad = angleDeg * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(Mathf.Sin(angleRad), -Mathf.Cos(angleRad), 0f) * _ropeLength;
        transform.position = _anchorPosition + offset;
        transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);
    }
}
