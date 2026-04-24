using UnityEngine;

public class GameplayCamera : MonoBehaviour
{
    [SerializeField] private Transform _target;

    private void LateUpdate()
    {
        if (_target != null)
            transform.LookAt(_target);
    }
}
