using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    [SerializeField] private int _startingCredits = 10000;

    private bool _initialized;

    public static EconomyManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void LateUpdate()
    {
        if (_initialized) return;
        _initialized = true;

        for (int i = 0; i < PlayerManager.Instance.PlayerCount; i++)
        {
            RecalculateStorage(i);
            PlayerManager.Instance.GetPlayer(i).Credits = _startingCredits;
        }
    }

    public int GetCredits(int playerIndex)
    {
        return PlayerManager.Instance.GetPlayer(playerIndex).Credits;
    }

    public int GetStorageCapacity(int playerIndex)
    {
        return PlayerManager.Instance.GetPlayer(playerIndex).StorageCapacity;
    }

    public int AddCredits(int playerIndex, int amount)
    {
        var player = PlayerManager.Instance.GetPlayer(playerIndex);
        int space = player.StorageCapacity - player.Credits;
        if (space <= 0) return 0;
        int deposited = Mathf.Min(amount, space);
        player.Credits += deposited;
        return deposited;
    }

    public bool SpendCredits(int playerIndex, int amount)
    {
        var player = PlayerManager.Instance.GetPlayer(playerIndex);
        if (player.Credits < amount) return false;
        player.Credits -= amount;
        return true;
    }

    public void RecalculateStorage(int playerIndex)
    {
        var player = PlayerManager.Instance.GetPlayer(playerIndex);
        int total = 0;

        foreach (var entity in player.OwnedEntities)
        {
            if (entity == null || entity.IsDead) continue;
            if (entity.UnitData == null) continue;
            if (entity.UnitData.StorageCapacity <= 0) continue;
            total += entity.UnitData.StorageCapacity;
        }

        int oldCapacity = player.StorageCapacity;
        player.StorageCapacity = total;

        if (player.Credits > player.StorageCapacity && total < oldCapacity)
            player.Credits = player.StorageCapacity;
    }
}
