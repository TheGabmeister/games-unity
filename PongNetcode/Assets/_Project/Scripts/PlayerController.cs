using UnityEngine;
using UnityEngine.InputSystem;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class PlayerController : MonoBehaviour
{
    float _moveInput = 0f;

    [SerializeField] private float moveSpeed = 5f;

    public void OnJump()
    {
        Debug.Log("Jump");
    }

    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>().y;
    }

    private void Update()
    {
        if (Mathf.Abs(_moveInput) > 0.001f)
        {
            Vector3 displacement = Vector3.up * _moveInput * moveSpeed * Time.deltaTime;
            transform.Translate(displacement, Space.World);
        }
    }
}
