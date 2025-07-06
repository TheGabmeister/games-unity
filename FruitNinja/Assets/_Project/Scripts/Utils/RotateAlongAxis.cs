using UnityEngine;

public class RotateAroundAxis : MonoBehaviour
{
    [SerializeField] Vector3 _rotationAxis = Vector3.up; 
    [SerializeField] float _rotationSpeed = 90f; 

    void Update()
    {
        // Normalize the rotation axis to ensure consistent speed
        Vector3 normalizedAxis = _rotationAxis.normalized;
        transform.Rotate(normalizedAxis, _rotationSpeed * Time.deltaTime, Space.World);
    }
}