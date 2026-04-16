using UnityEngine;

public class MoveForward : MonoBehaviour
{
    [SerializeField] float _speed = 18f;

    void Update()
    {
        transform.position += transform.right * (_speed * Time.deltaTime);
    }
}
