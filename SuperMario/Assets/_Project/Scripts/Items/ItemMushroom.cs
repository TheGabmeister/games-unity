using UnityEngine;

public class ItemMushroom : ItemBase
{
    protected override void AffectPlayer(GameObject target)
    {
        base.AffectPlayer(target);
        target.GetComponent<PlayerController>()?.Grow();
    }
}
