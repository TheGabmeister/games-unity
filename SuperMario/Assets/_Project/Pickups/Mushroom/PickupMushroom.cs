using UnityEngine;

public class PickupMushroom : Pickup
{
    protected override void TriggerEffect(GameObject other)
    {
        if (TryGetComponent<PlayerController>(out PlayerController player))
        {
            player.Grow();
        }
    }
}
