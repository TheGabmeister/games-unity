using UnityEngine;

public class Weapon : MonoBehaviour
{
    Animator _animator;
    [SerializeField] WeaponDataSO _weaponData;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        _animator.runtimeAnimatorController = _weaponData.animator;
    }

    public void Attack()
    {
        //_animator.Play("SwordAttack");
    }
}
