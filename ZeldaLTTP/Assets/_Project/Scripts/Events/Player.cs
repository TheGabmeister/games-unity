using UnityEngine;
using EventBus; 

public class Player : MonoBehaviour
{
    [SerializeField] AudioClip _dieSound;

    private void Start()
    {
        Bus<EV_PlayerSpawned>.Raise();
    }

    void Died()
    {
        Bus<EV_PlayerDied>.Raise();
    }

    void TakeDamage()
    {
        Bus<EV_PlayerDamaged>.Raise(new EV_PlayerDamaged { value = 5 });
        Bus<EV_SfxPlay>.Raise(new EV_SfxPlay { value = _dieSound });
    }   
}