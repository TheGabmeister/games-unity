using UnityEngine;

public class Headbutter : MonoBehaviour
{
    Rigidbody2D _rb;
    [SerializeField] float _bounceForce = 25;

    void Start()
    {
        _rb = transform.parent.GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        IHeadbuttable testInterface = col.gameObject.GetComponent<IHeadbuttable>();
        if(testInterface!=null)
        {
            testInterface.OnHeadbutt();
            _rb.AddForce(-transform.up * _bounceForce, ForceMode2D.Impulse);
        }
    }
}
