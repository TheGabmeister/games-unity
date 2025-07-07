using UnityEngine;
using UnityEngine.Events;
using PrimeTween;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Collider))]
public class UIFruit : MonoBehaviour
{
    MeshRenderer _meshRenderer;
    Collider _collider;
    [SerializeField] GameObject _fruitSlicedPrefab;
    [SerializeField] UnityEvent _events;
    void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _collider = GetComponent<Collider>();
    }

    public void Init()
    {
        _meshRenderer.enabled = true;
        _collider.enabled = true;
        Tween.Scale(transform, startValue: 0, endValue: 1f, duration: 1f);
    }
    
    void OnMouseDown()
    {
        Instantiate(_fruitSlicedPrefab, transform.position, Quaternion.identity);
        _events?.Invoke();
        Exit();
    }

    public void Exit()
    {
        _meshRenderer.enabled = false;
        _collider.enabled = false;
        Tween.Scale(transform, startValue: 1f, endValue: 0f, duration: 1f);
    }
}
