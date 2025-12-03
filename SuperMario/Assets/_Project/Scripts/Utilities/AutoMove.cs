using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoMove : MonoBehaviour
{
    public float MoveSpeed = 4.0f;
    [SerializeField] float _wallCheckDistance = 0.5f;

    [SerializeField] Vector2 _moveDirection = Vector2.left;


    void Update()
    {
        transform.Translate(_moveDirection * MoveSpeed * Time.deltaTime);

        // Check if the object is about to hit a wall
        RaycastHit2D hit = Physics2D.Raycast(transform.position, _moveDirection, _wallCheckDistance);
        if (hit.collider != null && hit.collider.gameObject.CompareTag("Obstacle"))
        {
            // Change direction
            _moveDirection = -_moveDirection;
        }

        Debug.DrawRay(transform.position, _moveDirection * _wallCheckDistance, Color.red);
    }
}
