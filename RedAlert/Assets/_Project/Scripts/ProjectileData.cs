using UnityEngine;

[CreateAssetMenu(fileName = "NewProjectile", menuName = "Red Alert/Projectile Data")]
public class ProjectileData : ScriptableObject
{
    public string DisplayName;
    public ProjectileType Type = ProjectileType.Hitscan;
    public float Speed = 10f;
    public float Scatter;
    public bool AntiAir = true;
    public bool AntiGround = true;
    public Sprite Sprite;
}
