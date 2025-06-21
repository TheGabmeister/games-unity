using UnityEngine;

namespace EventBus
{
    public class EVC_SceneLoad : MonoBehaviour
    {
        [SerializeField] string _sceneName;
        
        public void TriggerEvent()
        {
            Bus<EV_SceneLoad>.Raise(new EV_SceneLoad { value = _sceneName});
        }
    }
}