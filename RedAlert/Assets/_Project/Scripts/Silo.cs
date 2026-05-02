using UnityEngine;

public class Silo : MonoBehaviour
{
    private Entity _entity;

    void Awake()
    {
        _entity = GetComponent<Entity>();
    }

    void Start()
    {
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.RecalculateStorage(_entity.OwnerPlayerIndex);
    }

    void OnDestroy()
    {
        if (EconomyManager.Instance != null && _entity != null)
            EconomyManager.Instance.RecalculateStorage(_entity.OwnerPlayerIndex);
    }
}
