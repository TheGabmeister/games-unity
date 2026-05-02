using UnityEngine;

public class PrimaryBuilding : MonoBehaviour
{
    public bool IsPrimary { get; private set; }

    private SpriteRenderer _indicator;

    public void SetPrimary(bool primary)
    {
        IsPrimary = primary;
    }
}
