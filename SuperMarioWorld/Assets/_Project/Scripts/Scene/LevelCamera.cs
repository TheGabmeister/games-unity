using UnityEngine;

[RequireComponent(typeof(Camera))]
public sealed class LevelCamera : MonoBehaviour
{
    public Transform target;
    [SerializeField] float _offset = 10.0f;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void Update()
    {
        if (target == null) return;

        var targetPosition = target.position;
        transform.position = new Vector3(targetPosition.x, targetPosition.y - _offset, transform.position.z);
    }
}
