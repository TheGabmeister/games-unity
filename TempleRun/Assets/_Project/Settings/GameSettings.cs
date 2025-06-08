using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Scriptable Objects/GameSettings")]
public class GameSettings : ScriptableObject
{
    public float gameSpeed = 10.0f;
    public float obstacleSpawnRate = 1.5f;
}
