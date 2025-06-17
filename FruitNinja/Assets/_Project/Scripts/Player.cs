using UnityEngine;
using UnityServiceLocator;

public class Player : MonoBehaviour
{
    ISfxService sfxService;

    [SerializeField] private AudioClip swingSound;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        ServiceLocator.Global.Get(out sfxService);
    }

    // Update is called once per frame
    void Start()
    {
        sfxService.PlaySound(swingSound);
    }
}
