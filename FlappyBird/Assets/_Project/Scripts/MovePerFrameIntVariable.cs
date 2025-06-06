using ScriptableObjectArchitecture;
using UnityEngine;

public class MovePerFrameIntVariable : MonoBehaviour
{
    [SerializeField] FloatVariable worldMovementSpeed;

    void Update()
    {
        transform.Translate(Vector3.right * worldMovementSpeed * Time.deltaTime);
    }
}
