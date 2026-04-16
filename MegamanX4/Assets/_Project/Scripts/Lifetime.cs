using UnityEngine;

public class Lifetime : MonoBehaviour
{
    [SerializeField] float duration = 1f;

    float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= duration) Destroy(gameObject);
    }
}
