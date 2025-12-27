using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AutoMove))]
public class Koopa : EnemyBase
{
    
    [SerializeField] float _stompedTime = 3;
    [SerializeField] Animator _animator;
    bool _isStomped = false;
    bool _isRolling = false;
    AutoMove _autoMove;

    protected override void Start()
    {
        base.Start();
        _autoMove = GetComponent<AutoMove>();
    }

    public override void GetStomped()
    {
        base.GetStomped();
        if (_isRolling)
        {
            StartCoroutine(nameof(HideUnderShell));
            
            return;
        }
        else if (_isStomped)
        {
            
            SpawnScorePopup();
            StopAllCoroutines();
            StartRolling();
        }
        else
        {
            StartCoroutine(nameof(HideUnderShell));
        }
        
    }

    IEnumerator HideUnderShell()
    {
        _isStomped = true;
        _isRolling = false;
        _autoMove.enabled = false;
        _animator.SetTrigger("GetStomped");
        yield return new WaitForSeconds(_stompedTime);
        yield return new WaitForSeconds(2);
        ReturnToNormal();
    }

    void ReturnToNormal()
    {
        _isRolling = false;
        _isStomped = false;
        _autoMove.enabled = true;
        _autoMove.MoveSpeed = 2;
        _animator.SetTrigger("ReturnToNormal");
    }

    void StartRolling()
    {
        _isRolling = true;
        _isStomped = false;
        _autoMove.enabled = true;
        _autoMove.MoveSpeed = 10;
        _animator.SetTrigger("StartRolling");
    }
}
