using UnityEngine;

[CreateAssetMenu(fileName = "PowerupEffect", menuName = "Scriptable Objects/PowerupEffect")]
public abstract class PowerupEffect : ScriptableObject
{
    public abstract void Apply(GameObject target);
}
