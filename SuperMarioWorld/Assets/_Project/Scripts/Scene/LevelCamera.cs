using UnityEngine;

[RequireComponent(typeof(Camera))]
public sealed class LevelCamera : MonoBehaviour
{
    public Transform Target;
    [SerializeField] float _offset = 10.0f;


    private void Update()
    {
        if (Target == null) return;

        var targetPosition = Target.position;
        transform.position = new Vector3(targetPosition.x, targetPosition.y, targetPosition.z - _offset);
    }
}
