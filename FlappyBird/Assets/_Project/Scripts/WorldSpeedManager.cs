using ScriptableObjectArchitecture;
using UnityEngine;

public class WorldSpeedManager : MonoBehaviour
{
    [SerializeField] FloatVariable worldSpeed;

    void Start()
    {
        worldSpeed.Value = -5.0f;
    }
}
