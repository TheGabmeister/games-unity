using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ZoneTrigger : MonoBehaviour
{
    [SerializeField] private ZoneData _destinationZone;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
            GameplayManager.Instance.LoadZone(_destinationZone);
    }
}
