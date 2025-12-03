using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerProjectile : MonoBehaviour
{
    [SerializeField] float _groundCheckDistance = 0.5f;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<EnemyBase>()?.TakeDamage();
            Destroy(gameObject);
            //gameObject.GetComponent<Rigidbody2D>().AddForce(transform.up * 10.0f);
        }
    }

    void Update()
    {
        

        // Check if the object is about to hit the ground
        RaycastHit2D hit = Physics2D.Raycast(transform.position, -transform.up, _groundCheckDistance);
        if (hit.collider != null && hit.collider.gameObject.CompareTag("Obstacle"))
        {
            gameObject.GetComponent<Rigidbody2D>().AddForce(transform.up * 100.0f);
        }

        Debug.DrawRay(transform.position, -transform.up * _groundCheckDistance, Color.red);
    }

    
}
