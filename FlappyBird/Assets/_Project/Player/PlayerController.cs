using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float jumpForce = 200f;
    [SerializeField] AudioClip jumpSound;
    Rigidbody2D rb;
    AudioSource audioSource;
    bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isDead)
            {
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(new Vector2(0, jumpForce));
                audioSource.PlayOneShot(jumpSound);
            }
        }
    }

    public void StartDeathSequence()
    {
        isDead = true;
        Destroy(gameObject, 5.0f);
    }
}
