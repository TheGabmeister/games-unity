using EventBus;
using UnityEngine;

namespace EventBus
{
    public class EVC_GameStateChange : MonoBehaviour
    {
        [SerializeField] GameState _gameState;
        
        public void TriggerEvent()
        {
            Bus<EV_GameStateChange>.Raise(new EV_GameStateChange { state = _gameState});
        }
    }
}