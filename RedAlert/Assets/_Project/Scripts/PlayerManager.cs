using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private PlayerState[] _players = new PlayerState[]
    {
        new() { PlayerIndex = 0, Faction = Faction.Allied, Color = new Color(0.2f, 0.6f, 1f) },
        new() { PlayerIndex = 1, Faction = Faction.Soviet, Color = new Color(1f, 0.3f, 0.3f) }
    };

    [SerializeField] private int _localPlayerIndex;

    public static PlayerManager Instance { get; private set; }

    public PlayerState LocalPlayer => _players[_localPlayerIndex];
    public int PlayerCount => _players.Length;

    void Awake()
    {
        Instance = this;
    }

    public PlayerState GetPlayer(int index) => _players[index];

    public bool AreEnemies(int playerA, int playerB) => playerA != playerB;
}
