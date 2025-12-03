using UnityEngine;
using PrimeTween;

public class PiranhaPlant : EnemyBase
{
    float _initialPosY;
    bool _isAttacking = false;

    private void Awake()
    {
        _initialPosY = gameObject.transform.position.y;
    }

    public void Attack()
    {
        if (!_player) return;
        if (_isAttacking) return;


        _isAttacking = true;
        Sequence.Create(cycles: 1)
            .Chain(Tween.PositionY(transform, endValue: _initialPosY + 5, duration: 0.5f))
            .ChainDelay(1)
            .Chain(Tween.PositionY(transform, endValue: _initialPosY, duration: 0.5f))
            .ChainDelay(1)
            .ChainCallback(() => _isAttacking = false)
            .ChainCallback(() => Attack());
    }
}
