using UnityEngine;
using EventBus;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float _jumpForce = 1250f;
    [SerializeField] AudioClip _jumpSound;
    [SerializeField] AudioClip _dieSound;
    Rigidbody _rb;

    bool isDead = false;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isDead)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.AddForce(new Vector2(0, _jumpForce));
                //Events.SfxPlay.Raise(_jumpSound);
            }
        }
    }

    public void ToggleControls(bool value)
    {
        //_rb.simulated = value;
    }
}
