using UnityEngine;

public class SplashscreenController : MonoBehaviour
{
    [SerializeField] private float _duration = 1f;

    private float _timer;

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _duration)
            GameManager.Instance.LoadIntroScene();
    }
}
