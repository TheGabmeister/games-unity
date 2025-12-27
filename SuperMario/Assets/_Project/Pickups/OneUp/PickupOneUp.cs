using UnityEngine;

public class PickupOneUp : Pickup
{
    protected override void TriggerEffect(GameObject other)
    {
        GameInstance.Instance.UpdateLives(1);
    }
}
