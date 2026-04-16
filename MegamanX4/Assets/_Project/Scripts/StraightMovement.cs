using UnityEngine;

public class StraightMovement : MonoBehaviour
{
    [SerializeField] float speed = 18f;

    void Update()
    {
        transform.position += transform.right * (speed * Time.deltaTime);
    }
}
