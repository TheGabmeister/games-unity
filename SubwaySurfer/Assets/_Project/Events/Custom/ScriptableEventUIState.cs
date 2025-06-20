using UnityEngine;
using Obvious.Soap;

[CreateAssetMenu(fileName = "scriptable_event_" + nameof(UIState), menuName = "Soap/ScriptableEvents/"+ nameof(UIState))]
public class ScriptableEventUIState : ScriptableEvent<UIState>
{
    
}

