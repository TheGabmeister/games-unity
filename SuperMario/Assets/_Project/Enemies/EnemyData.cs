using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy Data", order = 0)]
public class EnemyData : ScriptableObject
{
    public string displayName;
    public int health;
    public int score;
}