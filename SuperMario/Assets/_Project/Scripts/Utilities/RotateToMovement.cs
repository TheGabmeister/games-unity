using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RotateToMovement : MonoBehaviour
{
    Rigidbody2D _rb;
    [SerializeField] bool _reversed = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_reversed)
        {
            if (_rb.linearVelocityX > 0)
            {
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
            else if (_rb.linearVelocityX < 0)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }
        else
        {
            if (_rb.linearVelocityX > 0)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else if (_rb.linearVelocityX < 0)
            {
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }
    }
}
