using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Scriptable Objects/GameSettings")]
public class GameSettings : ScriptableObject
{
    public float obstacleSpeed = 10.0f;
    public float obstacleSpawnRate = 1.5f;
}
