using UnityEngine;

public class StraightMovement : MonoBehaviour
{
    [SerializeField] float speed = 18f;
    [SerializeField] float angleDeg;

    Vector2 direction;

    void Start()
    {
        int facing = (int)Mathf.Sign(transform.lossyScale.x);
        float rad = angleDeg * Mathf.Deg2Rad;
        direction = new Vector2(Mathf.Cos(rad) * facing, Mathf.Sin(rad));
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }
}
