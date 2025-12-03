using UnityEngine;
using System.Collections;

public class Goomba : EnemyBase
{
    [Header("Variables")]
    [SerializeField] GameObject _flattenedGoomba;

    public override void GetStomped()
    {
        base.GetStomped();
        SpawnScorePopup();
        Instantiate(_flattenedGoomba, gameObject.transform.position, Quaternion.identity);
        Die();
    }
}
