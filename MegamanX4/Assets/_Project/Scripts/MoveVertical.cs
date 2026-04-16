using System.Net;
using UnityEngine;

public class MoveVertical : MonoBehaviour
{
    enum Direction
    {
        Up,
        Down
    }

    [SerializeField] float speed = 10f;
    [SerializeField] Direction direction = Direction.Up;

    void Update()
    {
        switch (direction)
        {
            case Direction.Up:
                transform.position += transform.up * speed * Time.deltaTime;
                break;
            case Direction.Down:
                transform.position += -transform.up * speed * Time.deltaTime;
                break;
        }
    }
}
