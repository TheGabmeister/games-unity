using UnityEngine;

public class MoveVertical : MonoBehaviour
{
    public enum Direction
    {
        Up,
        Down
    }

    [SerializeField] float _speed = 10f;
    [SerializeField] Direction _direction = Direction.Up;

    void Update()
    {
        switch (_direction)
        {
            case Direction.Up:
                transform.position += transform.up * _speed * Time.deltaTime;
                break;
            case Direction.Down:
                transform.position += -transform.up * _speed * Time.deltaTime;
                break;
        }
    }
}
