using UnityEngine;

public class DropTrigger : MonoBehaviour
{
    [SerializeField] float _detectionWidth = 2f;
    [SerializeField] float _detectionRange = 20f;
    [SerializeField] float _fallGravityScale = 3f;
    [SerializeField] HurtBox _hurtBoxToEnableOnDrop;

    Rigidbody2D _rb;
    bool _dropped;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (_hurtBoxToEnableOnDrop) _hurtBoxToEnableOnDrop.enabled = false;
    }

    void Update()
    {
        if (_dropped) return;

        Vector2 center = (Vector2)transform.position + Vector2.down * (_detectionRange * 0.5f);
        Vector2 size = new(_detectionWidth, _detectionRange);
        var hit = Physics2D.OverlapBox(center, size, 0f, 1 << Layers.Player);
        if (hit) Drop();
    }

    void Drop()
    {
        _dropped = true;
        if (_rb)
        {
            _rb.bodyType = RigidbodyType2D.Dynamic;
            _rb.gravityScale = _fallGravityScale;
        }
        if (_hurtBoxToEnableOnDrop) _hurtBoxToEnableOnDrop.enabled = true;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_dropped) return;
        if (collision.gameObject.layer == Layers.Environment)
            Destroy(gameObject);
    }
}
