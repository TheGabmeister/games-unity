using UnityEngine;
using EventBus;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float _jumpForce = 200f;
    [SerializeField] AudioClip _jumpSound;
    Rigidbody2D _rb;

    bool isDead = false;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isDead)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.AddForce(new Vector2(0, _jumpForce));
                Bus.SfxPlay.Publish(_jumpSound);
            }
        }
    }

    public void StartDeathSequence()
    {
        isDead = true;
        Destroy(gameObject, 5.0f);
    }
}
