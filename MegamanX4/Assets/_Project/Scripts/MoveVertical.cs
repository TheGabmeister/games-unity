using UnityEngine;

public class MoveVertical : MonoBehaviour
{
    enum VerticalDirection
    {
        Up,
        Down
    }

    [SerializeField] float speed = 18f;
    [SerializeField] VerticalDirection direction = VerticalDirection.Up;

    void Update()
    {
        float signedSpeed = direction == VerticalDirection.Up ? speed : -speed;
        transform.position += transform.up * (signedSpeed * Time.deltaTime);
    }
}
