using UnityEngine;
using EventBus;

[RequireComponent(typeof(BoxCollider2D))]
public class SceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] string _targetScene;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            collision.gameObject.transform.position += gameObject.transform.right * 3;
            Bus<EV_SceneSwitch>.Raise(new EV_SceneSwitch { value = _targetScene });
        }
    }
}
