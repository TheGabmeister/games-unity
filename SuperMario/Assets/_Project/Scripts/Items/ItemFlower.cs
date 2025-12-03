using UnityEngine;

public class ItemFlower : ItemBase
{
    protected override void AffectPlayer(GameObject target)
    {
        base.AffectPlayer(target);
        target.GetComponent<PlayerController>()?.ActivateStarMode();
    }
}
