using UnityEngine;
using EventBus;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float _jumpForce = 1250f;
    [SerializeField] GameObject _camera;
    [SerializeField] AudioClip _jumpSound;
    [SerializeField] AudioClip _dieSound;
    [SerializeField] Rigidbody _rigidBody;
    bool _isControllable = false;
    bool isDead = false;

    void OnEnable()
    {
        Bus<EV_PlayerEnableCam>.Add(EnableCam);
    }

    void OnDisable()
    {
        Bus<EV_PlayerEnableCam>.Remove(EnableCam);
    }

    void OnJump()
    {
        Debug.Log("Hello");
        Bus<EV_SfxPlay>.Raise(new EV_SfxPlay { clip = _jumpSound});
    }

    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.Space))
    //     {
    //         if (!isDead)
    //         {
    //             _rb.linearVelocity = Vector2.zero;
    //             _rb.AddForce(new Vector2(0, _jumpForce));

    //         }
    //     }
    // }

    public void EnableCam()
    {
        _camera.SetActive(true);
        
    }
}
