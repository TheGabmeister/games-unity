using UnityEngine;

public class PowerManager : MonoBehaviour
{
    private int[] _produced;
    private int[] _consumed;
    private bool[] _lowPower;

    public static PowerManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        int count = PlayerManager.Instance.PlayerCount;
        _produced = new int[count];
        _consumed = new int[count];
        _lowPower = new bool[count];

        for (int i = 0; i < count; i++)
            Recalculate(i);
    }

    public int GetProduced(int playerIndex) => _produced[playerIndex];
    public int GetConsumed(int playerIndex) => _consumed[playerIndex];
    public bool IsLowPower(int playerIndex) => _lowPower[playerIndex];

    public float GetPowerRatio(int playerIndex)
    {
        if (_produced[playerIndex] <= 0) return 0f;
        return (float)(_produced[playerIndex] - _consumed[playerIndex]) / _produced[playerIndex];
    }

    public void Recalculate(int playerIndex)
    {
        var player = PlayerManager.Instance.GetPlayer(playerIndex);
        int produced = 0;
        int consumed = 0;

        foreach (var entity in player.OwnedEntities)
        {
            if (entity == null || entity.IsDead) continue;
            if (entity.UnitData == null) continue;
            produced += entity.UnitData.PowerProduced;
            consumed += entity.UnitData.PowerConsumed;
        }

        _produced[playerIndex] = produced;
        _consumed[playerIndex] = consumed;
        _lowPower[playerIndex] = consumed > produced;
    }
}
