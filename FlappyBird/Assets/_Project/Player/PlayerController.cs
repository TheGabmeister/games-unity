using UnityEngine;
using EventBus;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float _jumpForce = 1250f;
    [SerializeField] AudioClip _jumpSound;
    Rigidbody2D _rb;

    private void OnEnable()
    {
        Bus.PlayerToggleControls.Sub(ToggleControls);
    }

    private void OnDisable()
    {
        Bus.PlayerToggleControls.Unsub(ToggleControls);
    }

    bool isDead = false;

    void Awake()
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

    public void ToggleControls(bool value)
    {
        _rb.simulated = value;
    }

    public void StartDeathSequence()
    {
        isDead = true;
        Destroy(gameObject, 5.0f);
    }
}
