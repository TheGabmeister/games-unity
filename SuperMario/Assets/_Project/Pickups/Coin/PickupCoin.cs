using UnityEngine;

public class PickupCoin : Pickup
{
    protected override void TriggerEffect(GameObject other)
    {
        GameInstance.Instance.UpdateCoins(1);
    }
}
