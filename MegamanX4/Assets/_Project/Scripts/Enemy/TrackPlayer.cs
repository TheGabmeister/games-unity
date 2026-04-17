using UnityEngine;

public class TrackPlayer : MonoBehaviour
{
    public enum Axis { X, Y }

    [SerializeField] Axis _axis = Axis.X;
    [SerializeField] float _maxSpeed = 3f;
    [SerializeField] float _detectionRadius = 20f;

    void FixedUpdate()
    {
        var collider = Physics2D.OverlapCircle(transform.position, _detectionRadius, 1 << Layers.Player);
        if (!collider) return;

        Transform playerTransform = collider.attachedRigidbody ? collider.attachedRigidbody.transform : collider.transform;
        Vector3 playerPos = playerTransform.position;
        Vector3 pos = transform.position;

        float step = _maxSpeed * Time.fixedDeltaTime;

        if (_axis == Axis.X)
            pos.x = Mathf.MoveTowards(pos.x, playerPos.x, step);
        else
            pos.y = Mathf.MoveTowards(pos.y, playerPos.y, step);

        transform.position = pos;
    }
}
