using UnityEngine;

public class Lifetime : MonoBehaviour
{
    [SerializeField] float _duration = 1f;

    float _timer;

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _duration) Destroy(gameObject);
    }
}
