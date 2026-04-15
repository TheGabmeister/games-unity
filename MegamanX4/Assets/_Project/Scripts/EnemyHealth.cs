using UnityEngine;

public class EnemyHealth : Health
{
    protected override void HandleDepleted()
    {
        base.HandleDepleted();
        Destroy(gameObject);
    }
}
