using UnityEngine;
using SimpleEventSystem;
using Unity.Mathematics;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float _jumpForce = 1250f;
    [SerializeField] AudioClip _jumpSound;
    [SerializeField] AudioClip _dieSound;
    [SerializeField] GameObject _deadPlayerPrefab;
    Rigidbody2D _rb;

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
                Events.SfxPlay.Publish(_jumpSound);
            }
        }
    }

    public void ToggleControls(bool value)
    {
        _rb.simulated = value;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isDead)
        {
            StartDeathSequence();
        }
    }
  
    public void StartDeathSequence()
    {
        isDead = true;
        Events.PlayerDied.Publish();
        Events.SfxPlay.Publish(_dieSound);
        Instantiate(_deadPlayerPrefab, gameObject.transform.position, quaternion.identity);
        Destroy(gameObject);
    }


}
