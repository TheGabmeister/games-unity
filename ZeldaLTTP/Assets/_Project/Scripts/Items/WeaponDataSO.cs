using UnityEngine;
using UnityEditor.Animations;

[CreateAssetMenu(fileName = "WeaponDataSO", menuName = "Scriptable Objects/WeaponDataSO")]
public class WeaponDataSO : ScriptableObject
{
    public AnimatorController animator;
    public AnimationClip attackAnimation;
    public float damage;
}
