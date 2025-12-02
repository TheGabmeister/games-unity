using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    float _moveInput = 0f;

    [SerializeField] private float moveSpeed = 5f;

    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>().y;
    }

    private void Update()
    {
        if (Mathf.Abs(_moveInput) > 0.001f)
        {
            Vector3 displacement = Vector3.up * _moveInput * moveSpeed * Time.deltaTime;

            if (NetworkManager.Singleton.IsClient)
            {
                MoveRpc(displacement);
                Debug.Log("Client Pressed");
            }    
        }
    }

    [Rpc(SendTo.Server)]
    void MoveRpc(Vector3 value, RpcParams rpcParams = default)
    {
        transform.Translate(value, Space.World);
        Debug.Log(value);
    }
}
