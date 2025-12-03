using UnityEngine;

public class ItemStar : ItemBase
{
    protected override void AffectPlayer(GameObject target)
    {
        base.AffectPlayer(target);
        target.GetComponent<PlayerController>()?.ActivateStarMode();
    }
}
