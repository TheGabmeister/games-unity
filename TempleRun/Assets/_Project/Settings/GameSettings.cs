using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Scriptable Objects/GameSettings")]
public class GameSettings : ScriptableObject
{
    public float initialSpeed = 10.0f;
    public int distanceScore = 10;
    public int coinScore = 10;
}
